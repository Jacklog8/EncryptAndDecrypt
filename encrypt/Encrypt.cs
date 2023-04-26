using System;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace encrypt
{
    internal class Encrypt
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

            byte[] key = GenerateKey();
            if(isFile)
                EncryptFile(path, key);
            else
            {
                List<string> files = GetFiles(path);
                foreach(string file in files)
                {
                    try
                    {
                        EncryptFile(file, key);
                    }
                    catch(Exception) { Console.WriteLine($"File {file} is protected and cannot be encrypted..."); }
                }
            }
            Console.WriteLine("Writing key file...");
            string keyPath;
            if (isFile)
                keyPath = Directory.GetParent(path) + "\\key.txt";
            else keyPath = path + "\\key.txt";
            File.WriteAllText(keyPath, Convert.ToBase64String(key));
            Console.WriteLine($"Key can be found at \"{keyPath}\"");
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

        static byte[] GenerateKey()
        {
            Console.WriteLine("Generating key...");
            using(RandomNumberGenerator rng = RNGCryptoServiceProvider.Create())
            {
                byte[] key = new byte[32];
                rng.GetBytes(key);
                Console.WriteLine("Key generated succesfully");
                return key;
            }
        }

        static byte[] GenerateIV()
        {
            using (RandomNumberGenerator rng = RNGCryptoServiceProvider.Create())
            {
                byte[] iv = new byte[16];
                rng.GetBytes(iv);
                return iv;
            }
        }

        static void EncryptFile(string path, byte[] key)
        {
            Console.WriteLine("Encrypting file:");
            Console.WriteLine("    Reading bytes...");
            byte[] bytes = File.ReadAllBytes(path);
            List<byte> encryptedBytes;

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                Console.WriteLine("    Generating IV...");
                aes.IV = GenerateIV();

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    Console.WriteLine("    Encrypting bytes...");
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(bytes, 0, bytes.Length);
                        cryptoStream.FlushFinalBlock();
                    }

                    encryptedBytes = new List<byte>(memoryStream.ToArray());
                    Console.WriteLine("    Adding IV to file...");
                    encryptedBytes.AddRange(aes.IV);
                }
            }

            Console.WriteLine("    Writing file");
            File.WriteAllBytes(path, encryptedBytes.ToArray());
            Console.WriteLine("File encryption success!\n");
        }
    }
}
