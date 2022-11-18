using Dapper;
using HelloAuth_Model.Data;
using HelloAuth_Utility;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace HelloAuth_API.Controllers
{
    [RoutePrefix("api")]
    [EnableCors(origins: "*", headers: "*", methods: "*")]

    public class SystemRegisterController : BaseController
    {
        [HttpPost]
        [Route("create")]
        public IHttpActionResult Create(tokenClass model)
        {
            var exp = 10 * 30; //300秒=5分鐘
            string pass = Encoding.UTF8.GetString(Convert.FromBase64String(model.passWord)); //轉一般密碼
            string date_ticks = pass.Substring(pass.IndexOf("_"), pass.Length - pass.IndexOf("_"));//取密碼中的 dateTime ticks
            long ticks = Convert.ToInt64(date_ticks.Replace("_", ""));//移除底線
            DateTime dateFromTicks = new DateTime(ticks); //轉一般時間格式
            int exp_Time = Convert.ToInt32((dateFromTicks.AddSeconds(exp) - new DateTime(1970, 1, 1)).TotalSeconds); //注冊時間加5分鐘
            long now = Convert.ToInt32((DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds);//當前的時間

            if (exp_Time > now)//檢查是否過期
            {
                try
                {
                    if (LogicSystemInfo.CheckSystemExits(model))
                    {
                        CreateLog(model, "unsuccess", "System name duplicated");

                        return BadRequest("System name duplicated");
                    }
                    else {
                        LogicSystemInfo.CreateSystem(model);

                        CreateLog(model, "success", "");

                        return Ok("Register success");
                    }
                }
                catch (Exception ex)
                {
                    CreateLog(model, "unsuccess", ex.ToString());

                    return BadRequest(ex.Message + ";" + ex.StackTrace);
                }
            }
            else
                return BadRequest("password expire");
        }

        //[HttpPut]
        //[Route("update")]
        //public IHttpActionResult Update(tokenClass model)
        //{
        //    try
        //    {
        //        if (LogicSystemInfo.CheckSystemExits(model))
        //        {
        //            LogicSystemInfo.UpdateSystem(model);

        //            return Ok("update success");
        //        }
        //        else
        //            return BadRequest("system not exist");

        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message + ";" + ex.StackTrace);
        //    }
        //}

        ///以下程式碼還沒整理好

        [HttpDelete]
        [Route("delete")]
        public string Delete(tokenClass model)
        {
            var con = new SqlConnection(ConfigurationManager.AppSettings["Connection1"]);
            con.Open();
            var query = "Select System_Name,Password,ID FROM  HelloAuth.dbo.SystemInfo WHERE System_Name ='" + model.system + "'";
            SqlCommand command = new SqlCommand(query, con);
            SqlDataReader reader = command.ExecuteReader();
            string pass = "";
            while (reader.Read())
                pass = reader["PassWord"].ToString();

            if (ExtendMethods.ConvertToMD5(model.passWord) == pass)
            {
                var qq = "DELETE FROM HelloAuth.dbo.SystemInfo WHERE System_Name ='" + model.system + "'";
                con.Execute(qq);

                DeleteLog(model, "成功", "刪除成功"); //存失敗的Log + exception
                return "刪除成功";
            }
            else
            {
                DeleteLog(model, "失敗", "請提供正確的密碼"); //存失敗的Log + exception
                return "請提供正確的密碼";
            }
        }

        [HttpPut]
        [Route("update")]
        public string Update(tokenClass model)
        {
            var con = new SqlConnection(ConfigurationManager.AppSettings["Connection1"]);
            con.Open();
            var query = "Select System_Name,Password,ID FROM  HelloAuth.dbo.SystemInfo WHERE System_Name ='" + model.system + "'";
            SqlCommand command = new SqlCommand(query, con);
            SqlDataReader reader = command.ExecuteReader();
            string ID = "";
            string pass = "";

            while (reader.Read())
            {
                ID = reader["ID"].ToString();
                pass = reader["PassWord"].ToString();
            }

            if (model.newPassword == model.passWord)
            {
                UpdateLog(model, "失敗", "password cannot be same");
                return "password cannot be same";
            }
            else if (ExtendMethods.ConvertToMD5(model.passWord) == pass)
            {
                var qq = "UPDATE  HelloAuth.dbo.SystemInfo SET System_Name =@System_Name ,PassWord=@PassWord WHERE id=@id ";
                var dp = new DynamicParameters();
                dp.Add("@System_Name", model.system);
                dp.Add("@PassWord", ExtendMethods.ConvertToMD5(model.newPassword));
                dp.Add("@id", ID);
                con.Execute(qq, dp);
                UpdateLog(model, "成功", "");
                return "修改成功";
            }
            else
            {
                UpdateLog(model, "失敗", "請提供正確的密碼");
                return "請提供正確的密碼";
            }
        }

        void CreateLog(tokenClass model, string status, string ex)
        {
            string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            var con = new SqlConnection(ConfigurationManager.AppSettings["Connection1"]);
            con.Open();
            var query = "INSERT INTO HelloAuth.dbo.register(Used_System,IP_Address,Access_DateTime,Access_Status,Exception) " +
                        "VALUES(@Used_System,@IP_Address,@Access_DateTime,@Access_Status,@Exception)";

            var dp = new DynamicParameters();
            dp.Add("@Used_System", model.system);
            dp.Add("@IP_Address", ip);
            dp.Add("@Access_DateTime", time);
            dp.Add("@Access_Status", status);
            dp.Add("@Exception", ex);
            con.Execute(query, dp);
        }

        void UpdateLog(tokenClass model, string status, string ex)
        {
            string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            var con = new SqlConnection(ConfigurationManager.AppSettings["Connection1"]);
            con.Open();
            var query = "INSERT INTO HelloAuth.dbo.renew(Used_System,IP_Address,Access_DateTime,Access_Status,Exception) " +
                        "VALUES(@Used_System,@IP_Address,@Access_DateTime,@Access_Status,@Exception)";

            var dp = new DynamicParameters();
            dp.Add("@Used_System", model.system);
            dp.Add("@IP_Address", ip);
            dp.Add("@Access_DateTime", time);
            dp.Add("@Access_Status", status);
            dp.Add("@Exception", ex);
            con.Execute(query, dp);
        }

        void DeleteLog(tokenClass model, string status, string ex)
        {
            string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            var con = new SqlConnection(ConfigurationManager.AppSettings["Connection1"]);
            con.Open();
            var query = "INSERT INTO HelloAuth.dbo.remove(Used_System,IP_Address,Access_DateTime,Access_Status,Exception) " +
                        "VALUES(@Used_System,@IP_Address,@Access_DateTime,@Access_Status,@Exception)";

            var dp = new DynamicParameters();
            dp.Add("@Used_System", model.system);
            dp.Add("@IP_Address", ip);
            dp.Add("@Access_DateTime", time);
            dp.Add("@Access_Status", status);
            dp.Add("@Exception", ex);
            con.Execute(query, dp);
        }
    }
}