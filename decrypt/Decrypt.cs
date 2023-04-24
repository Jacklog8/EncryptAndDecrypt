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
                        DecryptFile(path, key);
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
                Console.WriteLine("Please enter in the file to decrypt:\n    decrypt <file path>");
                return path;
            }
            if (File.Exists(args[0].Replace("_", " ")))
                path = args[0].Replace("_", " ");
            else path = Directory.GetCurrentDirectory() + "\\" + args[0].Replace("_", " ");
            if (!File.Exists(path))
            {
                Console.WriteLine($"Could not find either file:\n    {Directory.GetCurrentDirectory() + "\\" + args[0].Replace("_", " ")}\n    {args[0].Replace("_", " ")}");
                path = "";
            }
            return path;
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
