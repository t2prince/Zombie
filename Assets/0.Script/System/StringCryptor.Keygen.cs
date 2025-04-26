using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Rpg.Sys.Secure
{
    public static partial class StringCryptor
    {
        public static byte[] ToAesKey(this string key)
        {
            var md5 = new MD5CryptoServiceProvider();
            return md5.ComputeHash(Encoding.UTF8.GetBytes(key));
        }
        
        public static void SetPacketKey(string key, string iv)
        {
            _packetCriptor = new RijndaelManagedCryptor(key, iv);
        }
    }
}