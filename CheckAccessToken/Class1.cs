using HelloAuth.Controllers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace CheckAccessToken
{
    public class Class1
    {
        #region 加密
        public string AesEncrypt(string plain_text, string key, string iv) // 加密後回傳base64String
        {
            Validate_KeyIV_Length(key, iv);
            Aes aes = Aes.Create();
            aes.Mode = CipherMode.CBC;//非必須，但加了較安全
            aes.Padding = PaddingMode.PKCS7;//非必須，但加了較安全

            ICryptoTransform transform = aes.CreateEncryptor(Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(iv));

            byte[] bPlainText = Encoding.UTF8.GetBytes(plain_text);//明碼文字轉byte[]
            byte[] outputData = transform.TransformFinalBlock(bPlainText, 0, bPlainText.Length);//加密
            return Convert.ToBase64String(outputData);
        }

        public static void Validate_KeyIV_Length(string key, string iv) //驗證key和iv都必須為128bits或192bits或256bits
        {
            List<int> LegalSizes = new List<int>() { 128, 192, 256 };
            int keyBitSize = Encoding.UTF8.GetBytes(key).Length * 8;
            int ivBitSize = Encoding.UTF8.GetBytes(iv).Length * 8;
            if (!LegalSizes.Contains(keyBitSize) || !LegalSizes.Contains(ivBitSize))
                throw new Exception($@"key或iv的長度不在128bits、192bits、256bits其中一個，輸入的key bits:{keyBitSize},iv bits:{ivBitSize}");
        }

        public  string ComputeHMACSHA256(string data, string key) //產生 HMACSHA256 雜湊
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            using (var hmacSHA = new HMACSHA256(keyBytes))
            {
                var dataBytes = Encoding.UTF8.GetBytes(data);
                var hash = hmacSHA.ComputeHash(dataBytes, 0, dataBytes.Length);
                return BitConverter.ToString(hash).Replace("-", "").ToUpper();
            }
        }

        public string strToMD5(string str) //MD5加密
        {
            using (var cryptoMD5 = MD5.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(str);
                var hash = cryptoMD5.ComputeHash(bytes);
                var md5 = BitConverter.ToString(hash).Replace("-", String.Empty).ToUpper();

                return md5;
            }
        }

        #endregion

        #region 解密
        public bool CheckAccessToken(string token)
        {
            try
            {
                Class1 c = new Class1();
                var split = token.Split('.');
                var iv = split[0];
                var encrypt = split[1];
                var signature = split[2];
                string key = ConfigurationManager.AppSettings["TokenKey"]; //從 web.config 取得TokenKey

                if (signature != c.ComputeHMACSHA256(iv + "." + encrypt, key.Substring(0, 64)))//檢查簽章是否正確
                    return false;

                //使用 AES 解密 Payload
                var base64 = AesDecrypt(encrypt, key.Substring(0, 32), iv);
                var json = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
                var payload = JsonConvert.DeserializeObject<AccessTokenController.dataPayload>(json);

                if (payload.exp < Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds))//檢查是否過期
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string AesDecrypt(string base64String, string key, string iv)
        {
            Validate_KeyIV_Length(key, iv);
            Aes aes = Aes.Create();
            aes.Mode = CipherMode.CBC;//非必須，但加了較安全
            aes.Padding = PaddingMode.PKCS7;//非必須，但加了較安全

            ICryptoTransform transform = aes.CreateDecryptor(Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(iv));
            byte[] bEnBase64String = null;
            byte[] outputData = null;
            try
            {
                bEnBase64String = Convert.FromBase64String(base64String);//有可能base64String格式錯誤
                outputData = transform.TransformFinalBlock(bEnBase64String, 0, bEnBase64String.Length);//有可能解密出錯
            }
            catch (Exception ex)
            {
                throw new Exception($@"解密出錯:{ex.Message}");
            }

            //解密成功
            return Encoding.UTF8.GetString(outputData);
        }
        #endregion
    }
}
