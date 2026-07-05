using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Script_Kiddie.Utils
{
    public static class SecureFileStorage
    {
        private const int KeySize = 32;  // AES-256
        private const int IvSize = 16;   // AES 块大小

        /// <summary>
        /// 保存字符串到文件（自动生成随机密钥）
        /// </summary>
        public static void Save(string filePath, string content)
        {
            var plainBytes = Encoding.UTF8.GetBytes(content);

            using var aes = Aes.Create();

            // 自动生成随机密钥和IV
            aes.GenerateKey();
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            // 格式: [密钥(32字节)] + [IV(16字节)] + [密文]
            var result = new byte[KeySize + IvSize + cipherBytes.Length];
            Buffer.BlockCopy(aes.Key, 0, result, 0, KeySize);
            Buffer.BlockCopy(aes.IV, 0, result, KeySize, IvSize);
            Buffer.BlockCopy(cipherBytes, 0, result, KeySize + IvSize, cipherBytes.Length);

            File.WriteAllBytes(filePath, result);
        }

        /// <summary>
        /// 从文件读取字符串（用文件里的密钥解密）
        /// </summary>
        public static string Load(string filePath)
        {
            if (!File.Exists(filePath))
                return string.Empty;

            var data = File.ReadAllBytes(filePath);

            // 提取密钥、IV、密文
            var key = new byte[KeySize];
            var iv = new byte[IvSize];
            var cipherBytes = new byte[data.Length - KeySize - IvSize];

            Buffer.BlockCopy(data, 0, key, 0, KeySize);
            Buffer.BlockCopy(data, KeySize, iv, 0, IvSize);
            Buffer.BlockCopy(data, KeySize + IvSize, cipherBytes, 0, cipherBytes.Length);

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

            return Encoding.UTF8.GetString(plainBytes);
        }

        public static bool Exists(string filePath) => File.Exists(filePath);

        public static void Delete(string filePath)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }
}