using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;
using System.Security.Cryptography;
using System.IO;

namespace ThreeByte.Security
{
    public class Encryptor
    {

        private TripleDESCryptoServiceProvider TDES;

        public Encryptor(byte[] key, byte[] iv) {
            TDES = new TripleDESCryptoServiceProvider() { Key = key, IV = iv };
        }

        public string Encrypt(string input) {
            using(MemoryStream outStream = new MemoryStream()) {
                using(CryptoStream encStream = new CryptoStream(outStream, TDES.CreateEncryptor(), CryptoStreamMode.Write)) {
                    using(StreamWriter sWriter = new StreamWriter(encStream)) {
                        sWriter.Write(input);
                    }

                    string encryptedString = Convert.ToBase64String(outStream.ToArray());
                    return encryptedString;
                }
            }
        }

        public string Decrypt(string input) {
            using(MemoryStream outStream = new MemoryStream()) {
                using(CryptoStream encStream = new CryptoStream(outStream, TDES.CreateDecryptor(), CryptoStreamMode.Write)) {
                    byte[] encryptedBytes = Convert.FromBase64String(input);
                    encStream.Write(encryptedBytes, 0, encryptedBytes.Length);
                }
                using(StreamReader sWriter = new StreamReader(new MemoryStream(outStream.ToArray()))) {
                    string decrypted = sWriter.ReadToEnd();
                    return decrypted;
                }
            }

        }
        
        public static readonly Encryptor Default;
        private static readonly string SEED_KEY = "This is the best way to encrypt a string";
        private static readonly byte[] SEED_IV = new byte[] { 0xb9, 0x21, 0xe1, 0xa8, 0xf3, 0x0b, 0x9f, 0xd7 };

        static Encryptor() {
            MD5 md5 = new MD5CryptoServiceProvider();
            Default = new Encryptor(md5.ComputeHash(Encoding.ASCII.GetBytes(SEED_KEY)), SEED_IV);
        }

    }
}
