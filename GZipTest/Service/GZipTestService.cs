using GZipTest.DataModel;
using GZipTest.DataModel.Enums;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace GZipTest.Service
{
    /// <summary>
    /// Application service. 
    /// </summary>
    abstract partial class GZipTestService
    {
        /// <summary>
        /// <see cref="GZipTestParameters"/>
        /// </summary>
        protected GZipTestParameters _parameters;

        /// <summary>
        /// Thread flags.
        /// </summary>
        protected ManualResetEvent[] _flags;

        /// <summary>
        /// <see cref="DataBlockQueuesService"/>
        /// </summary>
        protected DataBlockQueuesService _dataService;

        /// <summary>
        /// Flag to handle interruption.
        /// </summary>
        protected volatile bool isInterrupted = false;

        /// <summary>
        /// Exception that may be raised.
        /// </summary>
        protected Exception exception;

        /// <summary>
        /// Offset for gzip header.
        /// </summary>
        protected int GZipHeaderOffset = 4;

        /// <summary>
        /// Common constructor for any type of operation.
        /// </summary>
        /// <param name="parameters"><see cref="GZipTestParameters"/>.</param>
        protected GZipTestService(GZipTestParameters parameters)
        {
            _parameters = parameters;

            _flags = new ManualResetEvent[parameters.ProcessingThreadCount];

            _dataService = new DataBlockQueuesService(parameters.QueueItemsLimit);
        }

        /// <summary>
        /// Creating service with valid paramters object.
        /// </summary>
        /// <param name="parameters"><see cref="GZipTestParameters"/>.</param>
        public static GZipTestService Factory(GZipTestParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException("Parameters is null.");

            switch (parameters.Operation)
            {
                case GZipTestOperation.Compress:
                    return new CompressionService(parameters);
                case GZipTestOperation.Decompress:
                    return new DecompressionService(parameters);
                default:
                    throw new InvalidOperationException($"Operation {parameters.Operation.ToString()} is not supported");
            }
        }

        /// <summary>
        /// Start processing.
        /// </summary>
        /// <returns>Success code 0</returns>
        /// <exception cref="GZipTestService.exception"></exception>
        public int Run()
        {         
            new Thread(Read).Start();

            for (int i = 0; i < _parameters.ProcessingThreadCount; i++)
            {
                _flags[i] = new ManualResetEvent(false);
                new Thread(Process).Start(_flags[i]);
            }

            new Thread(Wait).Start();

            var writeThread = new Thread(Write);

            writeThread.Start();

            writeThread.Join();

            if (exception != null)
                throw new Exception("There was an error during operation", exception);

            return 0;
        }

        /// <summary>
        /// Set IsInterrupted flag and stop processing.
        /// </summary>
        public void Interrupt()
        {
            if (!isInterrupted)
            {
                isInterrupted = true;

                _dataService.SetIsTerminated();

                _flags.ToList().ForEach(flag => flag.Set());
            }           
        }

        /// <summary>
        /// Read data blocks and fill read queue. Processing would be started automatically.
        /// </summary>
        abstract protected void Read();

        /// <summary>
        /// Process data blocks in read queue.
        /// </summary>
        /// <param name="flag">Reset flag <seealso cref="_flags"/></param>
        abstract protected void Process(object flag);

        /// <summary>
        /// Write data blocks from write queue into result file.
        /// </summary>
        void Write()
        {
            try
            {
                using (var stream = new FileStream(_parameters.ResultFile.FullName, FileMode.Append))
                {
                    while (!isInterrupted)
                    {
                        var block = _dataService.DequeueWriting();

                        if (block == null)
                            return;

                        stream.Write(block.DataBytes, 0, block.DataBytes.Length);
                    }
                }
                _dataService.SetIsTerminated();
            }
            catch (Exception ex)
            {
                exception = ex;
                Interrupt();
            }         
        }

        /// <summary>
        /// Waiting for all processing threads to finish and set Processing complete flag 
        /// <see cref="DataBlockQueuesService.SetProcessingCompleted"/>
        /// </summary>
        void Wait()
        {
            WaitHandle.WaitAll(_flags);

            _dataService.SetProcessingCompleted();
        }
    }
}
