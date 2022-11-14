using CheckAccessToken;
using Dapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors; //網頁Call API需啟用Cors

namespace HelloAuth.Controllers
{
    [EnableCors("*", "*", "*")] //啟用 Cors
    [RoutePrefix("api")]//路由前綴 base url/api

    public class AccessTokenController : ApiController
    {
        Class1 checkDll = new Class1(); //初始化checkdDll

        [HttpPost]
        [Route("getToken")] //API = https://localhost:44345/api/getToken
        public dataJWTTOKEN GetAccessToken(tokenClass model)
        {
            try
            {
                if (checkPassword(model) == true) //若系統存在且密碼正確
                {
                    var exp = 10 * 60;

                    var payload = new dataPayload
                    {
                        system = model.system,
                        exp = Convert.ToInt64((DateTime.UtcNow.AddSeconds(exp) - new DateTime(1970, 1, 1)).TotalSeconds)
                    };

                    var json = JsonConvert.SerializeObject(payload);
                    var base64Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
                    var iv = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16);

                    string key = ConfigurationManager.AppSettings["TokenKey"]; //從 web.config 取得TokenKey
                    var encrypt = checkDll.AesEncrypt(base64Payload, key.Substring(0, 32), iv); //使用 AES 加密 Payload
                    var signature = checkDll.ComputeHMACSHA256(iv + "." + encrypt, key.Substring(0, 64)); //取得簽章

                    Token_Log(model, "成功", "");  //存成功的Log

                    return new dataJWTTOKEN //回傳 Access Token
                    {
                        accessToken = iv + "." + encrypt + "." + signature,
                        refreshToken = Guid.NewGuid().ToString().Replace("-", ""),
                        expiresIn = exp,
                    };
                }
                else //若系統不存在
                {
                    Token_Log(model, "失敗", "帳密錯誤或系統未注冊"); //存失敗的Log + exception
                    return new dataJWTTOKEN { accessToken = "帳密錯誤或系統未注冊，無法取得 Access Token" };
                }
            }
            catch (Exception ex) //密碼不符合base64格式
            {
                Token_Log(model, "失敗", ex.ToString()); //存失敗的Log + exception
                return new dataJWTTOKEN { accessToken = ex.ToString(), };
            }
        }

        [HttpGet]
        [Route("checkToken")] //API = https://localhost:44345/api/checkToken
        public string check(string token)
        {
            if (CheckAccessToken(token))
                return "acess token 是有效的";
            else
                return "acess token 是無效的";
        }

        void Token_Log(tokenClass model, string status, string ex) //存client存取的Log
        {
            string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];

            var con = new SqlConnection(ConfigurationManager.AppSettings["Connection1"]);
            con.Open();

            var query = "INSERT INTO HelloAuth.dbo.token(Used_System,IP_Address,Access_DateTime,Access_Status,Exception) " +
                        "VALUES(@Used_System,@IP_Address,@Access_DateTime,@Access_Status,@Exception)";

            var dp = new DynamicParameters();
            dp.Add("@Used_System", model.system);
            dp.Add("@IP_Address", ip);
            dp.Add("@Access_DateTime", time);
            dp.Add("@Access_Status", status);
            dp.Add("@Exception", ex);
            con.Execute(query, dp);
        }

        bool checkPassword(tokenClass model) //檢查系統是否存在及密碼是否正確
        {
            var con = new SqlConnection(ConfigurationManager.AppSettings["Connection1"]);
            con.Open();
            var query = "Select System_Name,Password FROM  HelloAuth.dbo.SystemInfo WHERE System_Name ='" + model.system + "'";
            SqlCommand command = new SqlCommand(query, con);
            SqlDataReader reader = command.ExecuteReader();
            string password = ""; //存db抓出來的password

            while (reader.Read())
                password = reader["Password"].ToString();

            if (checkDll.strToMD5(model.passWord) != password)
                return false;
            else
                return true;
        }

        public class tokenClass //request 的參數
        {
            public string system { get; set; }
            public string passWord { get; set; }
            public string scope { get; set; } //暫時用不到
        }

        public class dataJWTTOKEN  //respone 的參數
        {
            public string accessToken { get; set; } //Token
            public string refreshToken { get; set; } //Refresh Token
            public int expiresIn { get; set; } //幾秒過期
        }

        public class dataPayload
        {
            public string system { get; set; }
            public long exp { get; set; } //過期時間
        }
























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

        public string ComputeHMACSHA256(string data, string key) //產生 HMACSHA256 雜湊
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
                string test = ComputeHMACSHA256(iv + "." + encrypt.Replace(" ", "+"), key.Substring(0, 64));

                if (signature !=test )//檢查簽章是否正確
                    return false;

                //使用 AES 解密 Payload
                var base64 = AesDecrypt(encrypt.Replace(" ", "+"), key.Substring(0, 32), iv);
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