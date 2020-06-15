using GZipTest.DataModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;

namespace GZipTest.Service
{
    /// <summary>
    /// <see cref="GZipTestService"/>
    /// </summary>
    abstract partial class GZipTestService
    {
        /// <summary>
        /// Service to compress data blocks.
        /// </summary>
        class CompressionService : GZipTestService
        {
            /// <summary>
            /// <seealso  cref="GZipTestService.GZipTestService(GZipTestParameters)"/>
            /// </summary>
            /// <param name="parameters"><see cref="GZipTestParameters"/></param>
            public CompressionService(GZipTestParameters parameters) : base(parameters)
            {

            }

            /// <summary>
            /// <see cref="GZipTestService.Read"/>
            /// </summary>
            protected override void Read()
            {
                try
                {
                    using (var fs = new FileStream(_parameters.OriginalFile.FullName, FileMode.Open))
                    {
                        int buffer_size = 0;
                        byte[] buffer;

                        while (!isInterrupted && fs.Position < fs.Length)
                        {
                            buffer_size = (fs.Length - fs.Position <= _parameters.DataBlockSize) ? (int)(fs.Length - fs.Position) : _parameters.DataBlockSize;
                            buffer = new byte[buffer_size];
                            fs.Read(buffer, 0, buffer_size);
                            _dataService.EnqueueRead(buffer);
                        }
                    }
                    _dataService.SetReadCompleted();
                }
                catch (Exception ex)
                {
                    exception = ex;
                    Interrupt();
                }           
            }

            /// <summary>
            /// <see cref="GZipTestService.Process(object)"/>
            /// </summary>
            /// <param name="flag">Reset flag.</param>
            protected override void Process(object flag)
            {
                try
                {
                    while (!isInterrupted)
                    {
                        var block = _dataService.DequeueProcessing();

                        if (block == null)
                        {
                            (flag as ManualResetEvent).Set();
                            return;
                        }

                        using (var block_stream = new MemoryStream())
                        using (var gz_stream = new GZipStream(block_stream, CompressionLevel.Optimal))
                        {
                            gz_stream.Write(block.DataBytes, 0, block.DataBytes.Length);
                            gz_stream.Close();
                               
                            var new_block = new DataBlock(block.ID, block_stream.ToArray());

                            BitConverter.GetBytes(new_block.DataBytes.Length).CopyTo(new_block.DataBytes, GZipHeaderOffset);

                            _dataService.EnqueueWrite(new_block);
                        }
                    }
                }
                catch (Exception ex)
                {
                    exception = ex;
                    Interrupt();
                }
            }
        }
    }
}
