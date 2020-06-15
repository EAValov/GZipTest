using GZipTest.DataModel.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GZipTest.DataModel
{
    /// <summary>
    /// Parameters of GZipTest application.
    /// </summary>
    public class GZipTestParameters
    {
        /// <summary>
        /// Original file.
        /// </summary>
        public FileInfo OriginalFile { get; private set; }

        /// <summary>
        /// Result file.
        /// </summary>
        public FileInfo ResultFile { get; private set; }

        /// <summary>
        /// Operation to be performed with original file.
        /// <see cref="GZipTestOperation"/>.
        /// </summary>
        public GZipTestOperation Operation { get; private set; }

        /// <summary>
        /// Number of processing threads.
        /// </summary>
        public int ProcessingThreadCount { get; private set; } = Environment.ProcessorCount;

        /// <summary>
        /// Data block size in bytes.
        /// </summary>
        public int DataBlockSize { get; private set; } = 1024 * 1024 * 50;

        /// <summary>
        /// Max data blocks in a queue <seealso cref=" DataBlockQueuesService._queuesSize"/>
        /// </summary>
        public int QueueItemsLimit { get; private set; } = 5;

        /// <summary>
        /// Constructor for command line arguments.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        public GZipTestParameters(string[] args)
        {
            try
            {
                var operations_available = string.Join(",", Enum.GetValues(typeof(GZipTestOperation)));

                if (args == null || args.Length != 3)
                    throw new ArgumentException($"Expected parameters: operation type ({operations_available}), original file path, result file path.");

                if (!Enum.TryParse(args[0], out GZipTestOperation operation_enum))
                    throw new ArgumentException($"Operation {args[0]} not supported. Supported operations: {operations_available}");

                Operation = operation_enum;

                OriginalFile = ValidateFileArgumentAndGetFileInfo(args[1], GZipTestParametersFileType.Original, Operation);

                ResultFile = ValidateFileArgumentAndGetFileInfo(args[2], GZipTestParametersFileType.Result, Operation);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Error while parsing command line arguments", ex);
            }
        }

        /// <summary>
        /// Validating file arguments and returning them as FileInfo Objects. 
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="file_type">File type <see cref="GZipTestParametersFileType"/></param>
        /// <param name="operation">Operation to be performed with original file <see cref="GZipTestOperation"/>.</param>
        FileInfo ValidateFileArgumentAndGetFileInfo(string path, GZipTestParametersFileType file_type, GZipTestOperation operation)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException($"{file_type.ToString()} file path is empty");

            FileInfo file_info = new FileInfo(path);

            if(file_type == GZipTestParametersFileType.Original)
            {
                if (!file_info.Exists)
                    throw new ArgumentException($"{file_type.ToString()} file is not exsists or path is incorrect");

                if (operation == GZipTestOperation.Compress && file_info.Extension == ".gz")
                    throw new ArgumentException("File is already compressed.");

                if (operation == GZipTestOperation.Decompress && file_info.Extension != ".gz")
                    throw new ArgumentException("File to be decompressed shall have .gz extension.");
            }

            return file_info;
        }
    }
}
