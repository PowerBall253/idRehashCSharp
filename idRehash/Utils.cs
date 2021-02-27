using System;
using System.Collections.Generic;
using System.IO;
using Farmhash.Sharp;

namespace idRehash
{
    class Utils
    {
        public static bool IsLinux
        {
            get
            {
                int p = (int) Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }
        public static uint ByteArrayToUint(byte[] bytes)
        {
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            uint number = BitConverter.ToUInt32(bytes, 0);
            return number;
        }
        public static UInt64 ByteArrayToUint64(byte[] bytes)
        {
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            UInt64 number = BitConverter.ToUInt32(bytes, 0);
            return number;
        }
        public static bool HashResourceHeaders(string path, ref UInt64 hash)
        {
            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(path, FileMode.Open);
            }
            catch (IOException)
            {
                Console.WriteLine($"Failed to open {path} for reading!");
                return false;
            }

            fileStream.Seek(0x74, SeekOrigin.Begin);
            UInt64 headersStartAddress = 0x7C;
            UInt64 headersEndAddress = 0;

            byte[] buffer = new byte[8];
            fileStream.Read(buffer, 0, buffer.Length);
            headersEndAddress = ByteArrayToUint64(buffer);
            headersEndAddress += 4;

            UInt64 headersSize = headersEndAddress - headersStartAddress;

            fileStream.Seek((long)headersStartAddress, SeekOrigin.Begin);
            byte[] hashedData = new byte[headersSize];
            fileStream.Read(hashedData, 0, (int)headersSize);

            hash = Farmhash.Sharp.Farmhash.Hash64(hashedData, (int)headersSize);
            Console.WriteLine($"{path}: {hash.ToString("x")}");
            return true;
        }
        public static List<string> GetResourceFileList()
        {
            List<string> resourceFiles = new List<string>();
            string[] files = Directory.GetFiles(".", "*.resources", SearchOption.AllDirectories);

            for (int i = 0; i < files.Length; i++)
            {
                if (Path.GetExtension(files[i]) == ".resources" & Path.GetFileName(files[i]) != "meta.resources")
                    resourceFiles.Add(files[i]);
            }
            return resourceFiles;
        }
        public static int GetResourceFileHashOffset(string path, byte[] decompressedContainerMaskData, int decompressedDataSize)
        {
            UInt64 hash = 0;

            if (!HashResourceHeaders(path, ref hash))
            {
                Console.WriteLine($"Failed to get hash for resource: {path}");
                return 0;
            }

            byte[] hashByteArray = BitConverter.GetBytes(hash);

            int hashOffset = 0;
            int currentHashByte = 0;

            for (int i = decompressedDataSize - 1; i >= 0; i--)
            {
                if (decompressedContainerMaskData[i] == hashByteArray[7 - currentHashByte])
                {
                    currentHashByte++;

                    if (currentHashByte == 8)
                    {
                        hashOffset = i;
                        break;
                    }
                }
                else
                {
                    currentHashByte = 0;
                }
            }

            if (currentHashByte == 0)
            {
                Console.WriteLine($"GetResourceFileOffset: Failed to get offset for resource: {path}");
                return 0;
            }
            return hashOffset;
        }
    }
}