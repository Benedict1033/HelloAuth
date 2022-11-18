using HelloAuth_Model.Data;
using HelloAuth_Model.Table;
using HelloAuth_Utility;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace HelloAuth_API.Controllers
{
    [RoutePrefix("api")]
    [EnableCors(origins: "*", headers: "*", methods: "*")]

    public class AccessTokenController : BaseController
    {
        [HttpPost]
        [Route("GetToken")]
        public dataResults<dataJWTTOKEN> GetAccessToken(tokenClass model)
        {
            try
            {
                if (LogicSystemInfo.CheckSystemExits(model) == true) //若系統存在且密碼正確
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
                    var encrypt = ExtendMethods.AesEncrypt(base64Payload, key.Substring(0, 32), iv); //使用 AES 加密 Payload
                    var signature = ExtendMethods.ComputeHMACSHA256(iv + "." + encrypt, key.Substring(0, 64)); //取得簽章

                    // save log
                    var Log = new ACCESSTOKEN
                    {
                        Used_System = model.system,
                        IP_Address = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"],
                        Access_DateTime = DateTime.Now,
                        Access_Status = "Success",
                        Exception = "",
                    };

                    LogicAccessToken.SaveTokenLog(Log);

                    // return response
                    var application = new dataJWTTOKEN
                    {
                        accessToken = iv + "." + encrypt + "." + signature,
                        refreshToken = Guid.NewGuid().ToString().Replace("-", ""),
                        expiresIn = exp,
                    };

                    return new dataResults<dataJWTTOKEN>
                    {
                        Message = "ok",
                        Code = "200",
                        Data = application,
                        Success = true
                    };
                }
                else
                {
                    // save Log
                    var Log = new ACCESSTOKEN
                    {
                        Used_System = model.system,
                        IP_Address = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"],
                        Access_DateTime = DateTime.Now,
                        Access_Status = "Unsuccess",
                        Exception = "System not registered or Password incorrect",
                    };

                    LogicAccessToken.SaveTokenLog(Log);

                    //return response
                    return new dataResults<dataJWTTOKEN>
                    {
                        Message = "System not registered or Password incorrect",
                        Code = "200",
                        Data = null,
                        Success = true
                    };
                }
            }
            catch (Exception ex)
            {
                // save log
                var Log = new ACCESSTOKEN
                {
                    Used_System = "",
                    IP_Address = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"],
                    Access_DateTime = DateTime.Now,
                    Access_Status = "Unsuccess",
                    Exception = ex.ToString(),
                };

                LogicAccessToken.SaveTokenLog(Log);

                // return response
                return new dataResults<dataJWTTOKEN>
                {
                    Message = ex.ToString(),
                    Code = "200",
                    Data = null,
                    Success = true
                };
            }
        }

        [HttpGet]
        [Route("CheckToken")]
        public dataResults<bool> Check(string token)
        {
            try
            {
                if (CheckAccessToken.CheckToken(token))
                {
                    return new dataResults<bool>
                    {
                        Message = "Valid Token",
                        Code = "200",
                        Data = true,
                        Success = true
                    };
                }
                else
                {
                    return new dataResults<bool>
                    {
                        Message = "Invalid Token",
                        Code = "200",
                        Data = false,
                        Success = true
                    };
                }
            }
            catch (Exception ex)
            {
                return new dataResults<bool>
                {
                    Message = ex.ToString(),
                    Code = "200",
                    Data = false,
                    Success = true
                };
            }
        }
    }
}