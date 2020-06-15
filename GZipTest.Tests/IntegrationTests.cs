using GZipTest.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GZipTest.DataModel.Enums;
using System.IO;
using System.Security.Cryptography;
using System;
using System.Linq;

namespace UnitTestProject1
{
    [TestClass]
    public class IntegrationTests
    {
        [TestMethod]
        public void TestFile_PerformCompressAndDecompress_HashMatches()
        {
            using (var test_file = TestFile.Factory(50 * 1024 * 1024))
            {
                var compression_args = new string[3] { GZipTestOperation.Compress.ToString(), test_file.FilePath, test_file.CompressedFileName };
                var compression_result = GZipTest.Program.Main(compression_args);

                Assert.AreEqual(0, compression_result);

                var decompression_args = new string[3] { GZipTestOperation.Decompress.ToString(), test_file.CompressedFileName, test_file.DecompressedFileName };
                var decompression_result = GZipTest.Program.Main(decompression_args);

                Assert.AreEqual(0, compression_result);

                using (var unzippedFile = new FileStream(test_file.DecompressedFileName, FileMode.Open))
                using (var md5 = MD5.Create())
                {
                    var actualHash = md5.ComputeHash(unzippedFile);
                    Assert.IsTrue(test_file.FileHash.SequenceEqual(actualHash));
                }
            }
        }     
    }
}
