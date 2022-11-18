using HelloAuth_Model.Data;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace HelloAuth_Utility
{
    public  static class CheckAccessToken
    {
        public  static bool CheckToken(string token)
        {
            try
            {
                var split = token.Split('.');
                var iv = split[0];
                var encrypt = split[1];
                var signature = split[2];
                string key = ConfigurationManager.AppSettings["TokenKey"]; //從 web.config 取得TokenKey

                if (signature != ExtendMethods.ComputeHMACSHA256(iv + "." + encrypt, key.Substring(0, 64)))//檢查簽章是否正確
                    return false;

                //使用 AES 解密 Payload
                var base64 = AesDecrypt(encrypt, key.Substring(0, 32), iv);
                var json = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
                var payload = JsonConvert.DeserializeObject<dataPayload>(json);

                if (payload.exp < Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds))//檢查是否過期
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }
        public static string AesDecrypt(string base64String, string key, string iv)
        {
            ExtendMethods.ValidateKeyIVLength(key, iv);
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
    }
}
