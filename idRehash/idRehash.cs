using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace idRehash
{
    class idRehash
    {
        static void Main(string[] args)
        {
            Console.WriteLine("idRehash C# by PowerBall253 :D");
            Console.WriteLine("Based on idRehash by infogram and proteh");
            Console.Write(Environment.NewLine);

            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream("meta.resources", FileMode.Open, FileAccess.Read);
            }
            catch (IOException)
            {
                Console.WriteLine("Failed to open meta.resources for reading!");
                Environment.Exit(1);
            }
            
            byte[] buffer = new byte[8];

            fileStream.Seek(0x50, SeekOrigin.Begin);
            fileStream.Read(buffer, 0, 8);
            uint infoOff = Utils.ByteArrayToUint(buffer);

            fileStream.Seek(0x38 + infoOff, SeekOrigin.Begin);
            fileStream.Read(buffer, 0, 8);
            uint fileOff = Utils.ByteArrayToUint(buffer);

            fileStream.Read(buffer, 0, 8);
            uint sizeZ = Utils.ByteArrayToUint(buffer);

            fileStream.Read(buffer, 0, 8);
            byte[] sizeBytes = new byte[buffer.Length];
            buffer.CopyTo(sizeBytes, 0);
            uint size = Utils.ByteArrayToUint(buffer);

            byte[] decompressedData = new byte[size];

            if (size == sizeZ)
            {
                fileStream.Seek(fileOff, SeekOrigin.Begin);
                if (fileStream.Read(decompressedData, 0, (int)size) != (int)size)
                {
                    Console.WriteLine("Bad meta.resources file.");
                    Environment.Exit(1);
                }
            }
            else
            {
                byte[] compressedData = new byte[sizeZ];
                fileStream.Seek(fileOff, SeekOrigin.Begin);
                if (fileStream.Read(compressedData, 0, (int)sizeZ) != (int)sizeZ)
                {
                    Console.WriteLine("Bad meta.resources file.");
                    Environment.Exit(1);
                }

                if (Utils.IsLinux)
                {
                    if (LinuxOodle.OodleLZ_Decompress(compressedData, (int)sizeZ, decompressedData, size, 0, 0, 0, 0, 0, (IntPtr)0, (IntPtr)0, (IntPtr)0, 0, 0) != size)
                    {
                        Console.WriteLine("Error while decompressing meta.resources - bad file?");
                        Environment.Exit(1);
                    }
                }
                else
                {
                    if (WindowsOodle.OodleLZ_Decompress(compressedData, (int)sizeZ, decompressedData, size, 0, 0, 0, 0, 0, (IntPtr)0, (IntPtr)0, (IntPtr)0, 0, 0) != size)
                    {
                        Console.WriteLine("Error while decompressing meta.resources - bad file?");
                        Environment.Exit(1);
                    }
                }
            }

            fileStream.Close();

            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "--getoffsets")
                    {
                        try
                        {
                            fileStream = new FileStream("idRehash.map", FileMode.OpenOrCreate);
                        }
                        catch (IOException)
                        {
                            Console.WriteLine("Failed to open idRehash.map for writing.");
                            Environment.Exit(1);
                        }
                        
                        string line;
                        byte[] linebytes;
                        var path = Utils.GetResourceFileList();
                        for (int a = 0; a < path.Count; a++)
                        {
                            string resourcePath = path[a];
                            int hashOffset = Utils.GetResourceFileHashOffset(resourcePath, decompressedData, (int)size);
                            if (hashOffset == 0)
                                Environment.Exit(1);
                            line = $"{path[a]};{hashOffset}" + Environment.NewLine;
                            linebytes = new UTF8Encoding(true).GetBytes(line);
                            fileStream.Write(linebytes, 0, linebytes.Length);
                        }
                        Console.WriteLine("idRehash.map has been succesfully generated.");
                        Environment.Exit(0);
                    }

                    Console.WriteLine($"USAGE: idRehash.exe [--getoffsets]");
                    Console.WriteLine("Options:");
                    Console.WriteLine($"\t--getoffsets\tGenerates the hash file offset map file required to run this tool");
                    Environment.Exit(0);
                }

                fileStream.Close();
            }

            try
            {
                fileStream = new FileStream("idRehash.map", FileMode.Open);
            }
            catch (IOException)
            {
                Console.WriteLine("Failed to open idRehash.map for reading.");
                Console.WriteLine("Make sure to generate the hash offset map using the --getoffsets option.");
                Environment.Exit(1);
            }

            var reader = new StreamReader(fileStream);
            var resourceOffsets = new Dictionary<string, int>();
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(';');
                resourceOffsets.Add(values[0], int.Parse(values[1]));
            }

            reader.Close();
            fileStream.Close();

            int fixedHashes = 0;
            foreach (KeyValuePair<string, int> kvp in resourceOffsets)
            {
                int offset = kvp.Value;
                string key = kvp.Key;
                UInt64 hash = 0;

                if (Utils.HashResourceHeaders(key, ref hash))
                {
                    if (offset > 0)
                    {
                        UInt64 oldHash = 0;
                        for (int i = 7; i >= 0; i--)
                        {
                            oldHash <<= 8;
                            oldHash |= decompressedData[offset + i];
                        }
                        if (oldHash != hash)
                        {
                            byte[] hashBytes = BitConverter.GetBytes(hash);
                            byte[] pHash = hashBytes;
                            for (int i = 7; i >= 0; i--)
                            {
                                decompressedData[offset + i] = pHash[i];
                            }

                            fixedHashes++;
                            Console.WriteLine($"  ^ Updated from {oldHash.ToString("x")}\n");
                        }
                    }
                }
            }         

            if (fixedHashes > 0)
            {
                try
                {
                    fileStream = new FileStream("meta.resources", FileMode.Open);
                }
                catch (IOException)
                {
                    Console.WriteLine("Failed to open meta.resources for writing.");
                    Environment.Exit(1);
                }
                
                fileStream.Seek(0x38 + infoOff + 0x8, SeekOrigin.Begin);
                fileStream.Write(sizeBytes, 0, 4);

                byte[] zero = new byte[1];
                zero[0] = 0;
                fileStream.Seek(0x38 + infoOff + 0x38, SeekOrigin.Begin);
                fileStream.Write(zero, 0, 1);

                fileStream.Seek(fileOff, SeekOrigin.Begin);
                fileStream.Write(decompressedData, 0, (int)size);

                Console.Write(Environment.NewLine);
                Console.WriteLine($"Done, {fixedHashes} hashes changed.");
                Environment.Exit(0);
            }
        }
    }
}