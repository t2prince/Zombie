using System;
using System.Security.Cryptography;
using System.Text;

namespace Rpg.Sys.Secure
{
    public class RijndaelManagedCryptor : ICryptor
    {
        private readonly RijndaelManaged _rijndael; 

        public RijndaelManagedCryptor(string key, string iv)
        {
            _rijndael = new RijndaelManaged
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                KeySize = 128,
                BlockSize = 128,
                
                Key = key.ToAesKey(),
                IV = iv.ToAesKey()
            }; 
        }
        
        public byte[] Encrypt(string plainText)
        {
            var plainTextBytes = UTF8Encoding.UTF8.GetBytes(plainText);
            
            var cTransform = _rijndael.CreateEncryptor();
            var resultArray = cTransform.TransformFinalBlock(plainTextBytes, 0, plainTextBytes.Length);
            return resultArray;
        }
        
        public string Decrypt(byte[] cipheredText)
        {
            var cTransform = _rijndael.CreateDecryptor();
            var resultArray = cTransform.TransformFinalBlock(cipheredText, 0, cipheredText.Length);
            return Encoding.UTF8.GetString(resultArray);
        }
        
        public string EncryptString(string plainText)
        {
            var resultArray = Encrypt(plainText);            
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        public string DecryptString(string cipherText)
        {
            return Decrypt(Convert.FromBase64String(cipherText));
        }
    }
}