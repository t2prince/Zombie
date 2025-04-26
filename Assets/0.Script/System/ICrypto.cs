namespace Rpg.Sys.Secure
{
    public interface ICryptor
    {
        byte[] Encrypt(string plainText);
        string Decrypt(byte[] cipherText);
    }
}