using Jjamcat;

namespace Rpg.Sys.Secure
{
    public class CipherString
    {
        private byte[] cipheredStream;

        public CipherString(string plainText)
        {
            cipheredStream = plainText.ToCipherCode();
        }

        public override string ToString()
        {
            return cipheredStream.ToPlainCode();
        }

        public static CipherString operator +(CipherString cipherString, string plainText)
        {
            return new CipherString($"{cipherString}{plainText}");
        }
        
        public static CipherString operator +(CipherString cipherString, int plainInt)
        {
            return new CipherString($"{cipherString}{plainInt}");
        }
    }
}