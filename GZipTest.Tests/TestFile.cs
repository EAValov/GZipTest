using System;
using System.Buffers;
using System.IO;
using System.Security.Cryptography;

namespace GZipTest.Tests
{
    public class TestFile: IDisposable
    {
        public string FilePath { get; }

        public byte[] FileHash { get; }

        public string CompressedFileName => $"{FilePath}.gz";

        public string DecompressedFileName => $"{FilePath}.unzip";

        TestFile(string file_path, byte[] file_hash)
        {
            FilePath = file_path;
            FileHash = file_hash;
        }

        public static TestFile Factory(int file_size_in_bytes)
        {
            var temp_path = Path.GetTempFileName();

            var buffer = ArrayPool<byte>.Shared.Rent(file_size_in_bytes);

            var random = new Random();

            random.NextBytes(buffer);

            File.WriteAllBytes(temp_path, buffer);

            ArrayPool<byte>.Shared.Return(buffer);

            using (var md5 = MD5.Create())
            {
                return new TestFile(temp_path, md5.ComputeHash(buffer));
            }
        }

        public void Dispose()
        {
            File.Delete(FilePath);

            if (File.Exists(CompressedFileName))
                File.Delete(CompressedFileName);

            if (File.Exists(DecompressedFileName))
                File.Delete(DecompressedFileName);
        }
    }
}
