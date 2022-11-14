using CheckAccessToken;
using Dapper;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web.Http;
using System.Web.Http.Cors; //網頁Call API需啟用Cors

namespace HelloAuth.Controllers
{
    [EnableCors("*", "*", "*")] //啟用 Cors
    [RoutePrefix("api")]//路由前綴 base url/api

    public class SystemRegisterController : ApiController
    {
        Class1 checkDll = new Class1(); //初始化dll

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
                var con = new SqlConnection(ConfigurationManager.AppSettings["Connection1"]);
                con.Open();

                try
                {
                    var oeSql = @"SELECT System_Name FROM HelloAuth.dbo.SystemInfo WHERE System_Name = @System_Name";
                    var parameters = new DynamicParameters();
                    parameters.Add("@System_Name", model.system);
                    string sys = con.Query<string>(oeSql, parameters).FirstOrDefault();

                    if (sys == null || sys == "")
                    {
                        var qq = "INSERT INTO HelloAuth.dbo.SystemInfo(System_Name,Password) VALUES(@System_Name,@Password)";
                        var dp = new DynamicParameters();
                        dp.Add("@System_Name", model.system);
                        dp.Add("@Password", checkDll.strToMD5(model.passWord));
                        con.Execute(qq, dp);

                        return Ok("Register success");
                    }
                    else
                        return BadRequest("System name duplicated");
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message + ";" + ex.StackTrace);
                }
            }
            else
                return BadRequest("密碼已過期，請更換密碼");
        }

        [HttpPost]
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

            if (checkDll.strToMD5(model.passWord) == pass)
            {
                var qq = "DELETE FROM HelloAuth.dbo.SystemInfo WHERE System_Name ='" + model.system + "'";
                con.Execute(qq);
                return "刪除成功";
            }
            else
                return "請提供正確的密碼";
        }

        [HttpPost]
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

            if (checkDll.strToMD5(model.passWord) == pass)
            {
                var qq = "UPDATE  HelloAuth.dbo.SystemInfo SET System_Name =@System_Name ,PassWord=@PassWord WHERE id=@id ";
                var dp = new DynamicParameters();
                dp.Add("@System_Name", model.system);
                dp.Add("@PassWord", checkDll.strToMD5(model.newPassword));
                dp.Add("@id", ID);
                con.Execute(qq, dp);
                return "修改成功";
            }
            else
                return "請提供正確的密碼";
        }

        public class tokenClass //request 的參數
        {
            public string system { get; set; }
            public string passWord { get; set; }
            public string newPassword { get; set; }
        }
    }
}