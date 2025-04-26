namespace Rpg.Sys.Secure
{
    public static partial class StringCryptor
    {         
        private static ICryptor _codeCriptor = new AesCrypto();
        private static ICryptor _packetCriptor = new AesCrypto();

        private static ICryptor _tableCriptor = new AesCrypto(
            "Woals83Dmstjs83Tlgh15Tjgh19".ToAesKey(), 
            "Woals83Dmstjs83Tlgh15Tjgh19".ToAesKey());

        private static byte[] CodeEncrypt(string text)
        {
            return _codeCriptor.Encrypt(text);
        }

        private static string CodeDecrypt(byte[] cipherBytes)
        {
            return _codeCriptor.Decrypt(cipherBytes);
        }
        
        public static byte[] PacketEncrypt(string text)
        {
            return _packetCriptor.Encrypt(text);
        }
        
        public static string PacketDecrypt(byte[] cipherBytes)
        {
            return _packetCriptor.Decrypt(cipherBytes);
        }

        private static byte[] TableEncrypt(string text)
        {
            return _tableCriptor.Encrypt(text);
        }

        private static string TableDecrypt(byte[] cipherBytes)
        {
            return _tableCriptor.Decrypt(cipherBytes);
        }
        
        public static byte[] ToCipherCode(this string str)
        {
            return CodeEncrypt(str);
        }
        
        public static string ToPlainCode(this byte[] stream)
        {
            return CodeDecrypt(stream);
        }
        
        public static byte[] ToCipheredTable(this string str)
        {
            return TableEncrypt(str);
        }
        
        public static string ToPlainTable(this byte[] stream)
        {
            return TableDecrypt(stream);
        }
    }
}