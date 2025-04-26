using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;


namespace Rpg.Sys.Secure
{
    public class AesCrypto : ICryptor
    {
        private readonly Aes _aes;
        const int KEY_SIZE = 256;
        const int BLOCK_SIZE = 128;
        
        public AesCrypto()
        {
            _aes = Aes.Create();
            _aes.GenerateKey();
            _aes.GenerateIV();
        }
        
        public AesCrypto(byte[] key, byte[] iv)
        {
            _aes = Aes.Create();                                     
            _aes.Key = key;
            _aes.IV = iv;        
        }

        public byte[] Encrypt(string plainText)
        {
            return EncryptStringToBytes(plainText, _aes.Key, _aes.IV);
        }
        
        public string Decrypt(byte[] cipherText)
        {
            return DecryptStringFromBytes(cipherText, _aes.Key, _aes.IV);
        }

        private static byte[] EncryptStringToBytes(string plainText, byte[] Key, byte[] IV)
        {
            byte[] encrypted;
            // Create an Aes object
            // with the specified key and IV.
            using (var aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;                           

                // Create a decryptor to perform the stream transform.
                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {

                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        private static string DecryptStringFromBytes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Declare the string used to hold
            // the decrypted text.
            string plaintext;

            // Create an Aes object
            // with the specified key and IV.
            using (var aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                
                var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (var msDecrypt = new MemoryStream(cipherText))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }    
    }
}