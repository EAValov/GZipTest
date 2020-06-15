using GZipTest.DataModel;
using GZipTest.Service;
using NLog;
using System;
using System.Diagnostics;
using System.IO;

namespace GZipTest
{
    /// <summary>
    /// Application for block compression and decompression of files using System.IO.Compression.GzipStream. 
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Logger.
        /// </summary>
        static ILogger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Application start point.
        /// </summary>
        /// <param name="args"></param>
        public static int Main(string[] args)
        {
            try
            {
                _logger.Info("Application started with parameters {args}", args);

                Stopwatch stop_watch = new Stopwatch();

                stop_watch.Start();

                var parameters = new GZipTestParameters(args);

                var service = GZipTestService.Factory(parameters);

                service.Run();

                stop_watch.Stop();

                _logger.Info($"Completed at {stop_watch.Elapsed.TotalSeconds} seconds.");

                return 0;
            }
            catch (OutOfMemoryException oom_ex)
            {
                _logger.Error(oom_ex, "Insufficient RAM, close some memory consuming applications and try again");
                return 1;
            }
            catch (IOException io_ex)
            {
                _logger.Error(io_ex, "Disc operation failed, check original file availability, disc space and file paths");
                return 1;
            }                    
            catch (Exception ex)
            {
                _logger.Error(ex);
                return 1;
            }
        }
    }
}
