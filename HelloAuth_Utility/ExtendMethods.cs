using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace HelloAuth_Utility
{
    public static class ExtendMethods 
    {
        public static string ToStringSafe(this object obj)
        {
            return (obj ?? string.Empty).ToString();
        }

        public static string AesEncrypt(string plain_text, string key, string iv) // 加密後回傳base64String
        {
            ValidateKeyIVLength(key, iv);
            Aes aes = Aes.Create();
            aes.Mode = CipherMode.CBC;//非必須，但加了較安全
            aes.Padding = PaddingMode.PKCS7;//非必須，但加了較安全

            ICryptoTransform transform = aes.CreateEncryptor(Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(iv));

            byte[] bPlainText = Encoding.UTF8.GetBytes(plain_text);//明碼文字轉byte[]
            byte[] outputData = transform.TransformFinalBlock(bPlainText, 0, bPlainText.Length);//加密
            return Convert.ToBase64String(outputData);
        }

        public static void ValidateKeyIVLength(string key, string iv) //驗證key和iv都必須為128bits或192bits或256bits
        {
            List<int> LegalSizes = new List<int>() { 128, 192, 256 };
            int keyBitSize = Encoding.UTF8.GetBytes(key).Length * 8;
            int ivBitSize = Encoding.UTF8.GetBytes(iv).Length * 8;
            if (!LegalSizes.Contains(keyBitSize) || !LegalSizes.Contains(ivBitSize))
                throw new Exception($@"key或iv的長度不在128bits、192bits、256bits其中一個，輸入的key bits:{keyBitSize},iv bits:{ivBitSize}");
        }

        public static string ComputeHMACSHA256(string data, string key) //產生 HMACSHA256 雜湊
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            using (var hmacSHA = new HMACSHA256(keyBytes))
            {
                var dataBytes = Encoding.UTF8.GetBytes(data);
                var hash = hmacSHA.ComputeHash(dataBytes, 0, dataBytes.Length);
                return BitConverter.ToString(hash).Replace("-", "").ToUpper();
            }
        }

        public static string ConvertToMD5(string str) //MD5加密
        {
            using (var cryptoMD5 = MD5.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(str);
                var hash = cryptoMD5.ComputeHash(bytes);
                var md5 = BitConverter.ToString(hash).Replace("-", String.Empty).ToUpper();

                return md5;
            }
        }

       
    }
}
