using GZipTest.DataModel;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace GZipTest.Service
{ 
    /// <summary>
    /// <see cref="GZipTestService"/>
    /// </summary>
    abstract partial class GZipTestService
    {
        /// <summary>
        /// Service to decompress data blocks.
        /// </summary>
        class DecompressionService : GZipTestService
        {
            /// <summary>
            /// <see cref="GZipTestService.GZipTestService(GZipTestParameters)"/>
            /// </summary>
            /// <param name="parameters"><see cref="GZipTestParameters"/></param>
            public DecompressionService(GZipTestParameters parameters) : base(parameters)
            {
            }

            /// <summary>
            /// <see cref="GZipTestService.Read"/>
            /// </summary>
            override protected void Read()
            {
                try
                {
                    using (var fs = new FileStream(_parameters.OriginalFile.FullName, FileMode.Open))
                    {
                        int selection = 0;
                        byte[] data;

                        while (!isInterrupted && fs.Position < fs.Length)
                        {
                            var header = new byte[8];

                            fs.Read(header, 0, header.Length);

                            selection = BitConverter.ToInt32(header, _gZipHeaderOffset);
                            data = new byte[selection];
                            header.CopyTo(data, 0);
                            fs.Read(data, header.Length, selection - header.Length);

                            _dataService.EnqueueRead(data);
                        }

                        _dataService.SetReadCompleted();
                    }
                }
                catch (Exception ex)
                {
                    _exception = ex;
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

                        using (var block_stream = new MemoryStream(block.DataBytes))
                        using (var gz_stream = new GZipStream(block_stream, CompressionMode.Decompress))
                        using (var write_stream = new MemoryStream())
                        {
                            gz_stream.CopyTo(write_stream);

                            _dataService.EnqueueWrite(new DataBlock(block.ID, write_stream.ToArray()));
                        }
                    }
                }
                catch (Exception ex)
                {
                    _exception = ex;
                    Interrupt();
                }
            }
        }
    }
}
