using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace EDFab_Telemetry_Server
{
    class SecureSockets : Encryption
    {
        private Socket c;
        private byte[] bytes = new byte[1024];
        public SecureSockets(Socket soc)
        {
            c = soc;
        }
        public void Send(PacketData packet)
        {
            c.Send(Encrypt(JsonSerializer.Serialize(packet)));
        }
        public PacketData Receive()
        {
            byte[] trimBytes = new byte[c.Receive(bytes)];
            Array.Copy(bytes, trimBytes, trimBytes.Length);
            return JsonSerializer.Deserialize<PacketData>(Decrypt(trimBytes));
        }
        public void Close()
        {
            c.Close();
        }
    }
    class Email : Secrets
    {
        public static void Send(string Recipient, string Subject, string Message)
        {
            MailMessage Mail = new MailMessage();
            SmtpClient Email = new SmtpClient(emailServer);
            Mail.From = new MailAddress(email);
            Mail.To.Add(Recipient);
            Mail.Subject = Subject;
            Mail.Body = Message;
            Email.Port = 25;
            Email.Credentials = new System.Net.NetworkCredential(email, pass);
            Email.EnableSsl = true;
            Email.Send(Mail);
        }
    }
    class Encryption : Secrets
    {
        private const int Keysize = 128;
        private const int DerivationIterations = 1000;
        protected static byte[] Encrypt(string plainText)
        {
            byte[] saltStringBytes = GenerateEntropy(16);
            byte[] ivStringBytes = GenerateEntropy(16);
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            Rfc2898DeriveBytes password = new Rfc2898DeriveBytes(Key, saltStringBytes, DerivationIterations);
            byte[] keyBytes = password.GetBytes(Keysize / 8);
            RijndaelManaged symmetricKey = new RijndaelManaged { BlockSize = 128, Mode = CipherMode.CBC, Padding = PaddingMode.PKCS7 };
            ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes);
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.FlushFinalBlock();
            byte[] cipherTextBytes = saltStringBytes;
            cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
            cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();

            memoryStream.Close();
            cryptoStream.Close();
            return cipherTextBytes;
        }
        protected static string Decrypt(byte[] cipherTextBytesWithSaltAndIv)
        {
            byte[] saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
            byte[] ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
            byte[] cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();

            Rfc2898DeriveBytes password = new Rfc2898DeriveBytes(Key, saltStringBytes, DerivationIterations);
            byte[] keyBytes = password.GetBytes(Keysize / 8);
            RijndaelManaged symmetricKey = new RijndaelManaged { BlockSize = 128, Mode = CipherMode.CBC, Padding = PaddingMode.PKCS7 };
            ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes);
            MemoryStream memoryStream = new MemoryStream(cipherTextBytes);
            CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            byte[] plainTextBytes = new byte[cipherTextBytes.Length];
            int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);

            memoryStream.Close();
            cryptoStream.Close();
            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
        }
        public static byte[] GenerateEntropy(int count)
        {
            byte[] randomBytes = new byte[count];
            RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
            rngCsp.GetBytes(randomBytes);
            return randomBytes;
        }
        public static string Hash(string password)
        {
            byte[] salt = GenerateEntropy(16);
            Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);
            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);
            return Convert.ToBase64String(hashBytes);
        }
        public static bool Compare_Hash(string current, string inputted)
        {
            byte[] hashBytes = Convert.FromBase64String(current);
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);
            Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(inputted, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);
            for (int i = 0; i < 20; i++)
            {
                if (hashBytes[i + 16] != hash[i])
                {
                    return false;
                }
            }
            return true;
        }
        public static string Generate_Token()
        {
            byte[] tokenbytes = new byte[36];
            RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
            rngCsp.GetBytes(tokenbytes);
            return Convert.ToBase64String(tokenbytes);
        }
    }
    class Secrets
    {
        protected const string Key = "";
        protected const string emailServer = "";
        protected const string email = "";
        protected const string pass = "";
    }
}
