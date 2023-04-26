using System;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace decrypt
{
    internal class Decrypt
    {
        static void Main(string[] args)
        {
            string path = GetPath(args);
            if (path == "")
                return;

            bool isFile;
            if (File.Exists(path))
                isFile = true;
            else isFile = false;

            while (true)
            {
                byte[] key = new byte[32];
                bool flag = false;
                Console.WriteLine("Enter base64 key to decrypt:");
                try
                {
                    key = Convert.FromBase64String(Console.ReadLine());
                    flag = true;
                }
                catch (Exception)
                {
                    Console.WriteLine("Key must be base 64 encoded!\n");
                }

                if(flag)
                {
                    try
                    {
                        if(isFile)
                            DecryptFile(path, key);
                        else
                        {
                            List<string> files = GetFiles(path);
                            foreach (string file in files)
                            {
                                try
                                {
                                    DecryptFile(file, key);
                                }
                                catch (Exception) { Console.WriteLine($"File {file} is protected and cannot be encrypted..."); }
                            }
                        }
                        break;
                    }
                    catch(Exception)
                    {
                        Console.WriteLine("\nError decrypting. Possibly the wrong key, try again...\n");
                    }
                }
            }
        }

        static string GetPath(string[] args)
        {
            string path = "";
            if (args.Length != 1)
            {
                Console.WriteLine("Please enter in the file or directory to encrypt:\n    encrypt <file path>");
                return path;
            }
            if (File.Exists(args[0].Replace("_", " ")) || Directory.Exists(args[0].Replace("_", " ")))
                path = args[0].Replace("_", " ");
            else path = Directory.GetCurrentDirectory() + "\\" + args[0].Replace("_", " ");
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                Console.WriteLine($"Could not find either file or directory:\n    {Directory.GetCurrentDirectory() + "\\" + args[0].Replace("_", " ")}\n    {args[0].Replace("_", " ")}");
                path = "";
            }
            return path;
        }

        static List<string> GetFiles(string path)
        {
            var files = new List<string>();
            var directories = new string[] { };

            try
            {
                files.AddRange(Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly));
                directories = Directory.GetDirectories(path);
            }
            catch (UnauthorizedAccessException) { }

            foreach (var directory in directories)
                try
                {
                    files.AddRange(GetFiles(directory));
                }
                catch (UnauthorizedAccessException) { }

            return files;
        }

        static void DecryptFile(string path, byte[] key)
        {
            Console.WriteLine("Decrypting file:");
            Console.WriteLine("    Reading bytes...");
            byte[] bytes = File.ReadAllBytes(path);
            byte[] decryptedBytes;

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                Console.WriteLine("    Retrieving IV...");
                List<byte> iv = new List<byte>();
                int i = 0;
                foreach(byte arrayByte in bytes)
                {
                    if (i >= bytes.Length - 16)
                        iv.Add(arrayByte);
                    i++;
                }
                aes.IV = iv.ToArray();

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    Console.WriteLine("    Decrypting bytes...");
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(bytes, 0, bytes.Length - 16);
                        cryptoStream.FlushFinalBlock();
                    }

                    decryptedBytes = memoryStream.ToArray();
                }
            }

            Console.WriteLine("    Writing file");
            File.WriteAllBytes(path, decryptedBytes);
            Console.WriteLine("File decryption success!\n");
        }
    }
}
