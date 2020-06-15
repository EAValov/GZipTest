using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace GZipTest.DataModel
{
    /// <summary>
    /// Concurrent data provider.
    /// </summary>
    public sealed class DataBlockQueuesService
    {
        #region "Locking objects"

        /// <summary>
        /// Read locking object.
        /// </summary>
        readonly object _readLock;

        /// <summary>
        /// Write locking object.
        /// </summary>
        readonly object _writeLock;

        #endregion

        #region "State Flags"

        /// <summary>
        /// Flag, set true when exception was raised, service was interrupted or job was done.
        /// </summary>
        volatile bool _isTerminated = false;

        /// <summary>
        /// Flag, set true when reading data blocks is finished.
        /// </summary>
        volatile bool _isReadCompleted = false;

        /// <summary>
        /// Flag, set true when all working threads is finished.
        /// </summary>
        volatile bool _isProcessingCompleted = false;

        #endregion

        #region "Queues and config"

        /// <summary>
        /// Queue for incoming data chunks.
        /// </summary>
        Queue<DataBlock> _readQueue;

        /// <summary>
        /// Queue for proceseed data chunks.
        /// </summary>
        Queue<DataBlock> _writeQueue;

        /// <summary>
        /// Max items in a queue, to preserve OutOfMemoryException.
        /// </summary>
        readonly int _queuesSize;

        #endregion

        #region "Counters"

        /// <summary>
        /// Incoming queue counter, to assign id to new chunk.
        /// </summary>
        volatile int _readBlockCount;

        /// <summary>
        /// Processed queue counter, to assign id to new chunk.
        /// </summary>
        volatile int _writeBlockCount;

        #endregion

        /// <summary>
        /// <see cref="DataBlockQueuesService"/>
        /// </summary>
        /// <param name="queue_limit"><see cref="_queuesSize"/></param>
        public DataBlockQueuesService(int queue_limit)
        {
            this._readLock = new object();
            this._writeLock = new object();
            this._queuesSize = queue_limit;
            this._readBlockCount = 0;
            this._writeBlockCount = 0;
            this._readQueue = new Queue<DataBlock>();
            this._writeQueue = new Queue<DataBlock>();
        }

        /// <summary>
        /// Add new data piece to reading queue.
        /// </summary>
        /// <param name="data"><see cref="DataBlock.DataBytes"/></param>
        public void EnqueueRead(byte[] data)
        {
            lock (_readLock)
            {     
                while (!_isTerminated && _readQueue.Count >= _queuesSize)
                    Monitor.Wait(_readLock);

                if (_isTerminated)
                    throw new InvalidOperationException("Task was terminated");

                _readQueue.Enqueue(new DataBlock(_readBlockCount++, data));

                Monitor.PulseAll(_readLock);
            }
        }

        /// <summary>
        /// Add processed data block to writing queue.
        /// </summary>
        /// <param name="block"><see cref="DataBlock"/></param>
        public void EnqueueWrite(DataBlock block)
        {
            lock (_writeLock)
            {
                while (!_isTerminated && block.ID != _writeBlockCount)
                    Monitor.Wait(_writeLock);

                while (!_isTerminated && _writeQueue.Count >= _queuesSize)
                    Monitor.Wait(_writeLock);

                _writeQueue.Enqueue(block);

                _writeBlockCount++;

                Monitor.PulseAll(_writeLock);
            }
        }

        /// <summary>
        /// Waits for reading queue to populate and gets the next block to process.
        /// </summary>
        /// <returns>Next block to process.</returns>
        public DataBlock DequeueProcessing()
        {
            lock (_readLock)
            {
                while (!_isTerminated && !_isReadCompleted && _readQueue.Count < 1)
                    Monitor.Wait(_readLock);

                var block = (_readQueue.Count == 0) ? null : _readQueue.Dequeue();

                Monitor.PulseAll(_readLock);

                return block;
            }
        }

        /// <summary>
        /// Waits for writing queue to populate and gets next block to process.
        /// </summary>
        /// <returns>Next block to process.</returns>
        public DataBlock DequeueWriting()
        {
            lock (_writeLock)
            {
                while (!_isTerminated && !_isProcessingCompleted && _writeQueue.Count < 1)
                    Monitor.Wait(_writeLock);

                var block = (_writeQueue.Count == 0) ? null : _writeQueue.Dequeue();

                Monitor.PulseAll(_writeLock);

                return block;
            }
        }

        /// <summary>
        /// Set Teminated flag <see cref="_isTerminated"/>.
        /// </summary>
        public void SetIsTerminated()
        {
            lock (_readLock)
            {
                lock (_writeLock)
                {
                    _isTerminated = true;
                    Monitor.PulseAll(_writeLock);
                }
                Monitor.PulseAll(_readLock);
            }
        }

        /// <summary>
        /// Set ReadCompleted flag <see cref="_isReadCompleted"/>.
        /// </summary>
        public void SetReadCompleted()
        {
            lock (_readLock)
            {
                _isReadCompleted = true;
                Monitor.PulseAll(_readLock);
            }
        }

        /// <summary>
        /// Set Processed flag <see cref="_isProcessingCompleted"/>
        /// </summary>
        public void SetProcessingCompleted()
        {
            lock (_writeLock)
            {
                _isProcessingCompleted = true;
                Monitor.PulseAll(_writeLock);
            }
        }
    }
}
