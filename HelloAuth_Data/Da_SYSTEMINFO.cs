using Dapper;
using HelloAuth_Model.Data;
using HelloAuth_Model.Table;
using HelloAuth_Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;


namespace HelloAuth_Data
{
    public class Da_SYSTEMINFO : DataRepository<SYSTEMINFO>
    {
        SqlConnection cn;
        public Da_SYSTEMINFO(SqlConnection conn) : base(conn)
        {
            this.cn = conn;
        }

        public List<SYSTEMINFO> SelectSystem(tokenClass model)
        {
            var strSQL = @"SELECT System_Name,Password,id,Enable
                            FROM  HelloAuth.dbo.SystemInfo
                           WHERE System_Name =@System_Name";
            var parameters = new DynamicParameters();
            parameters.Add("@System_Name", model.system);

            List<SYSTEMINFO> sys = Connection.Query<SYSTEMINFO>(strSQL, parameters).ToList();

            return sys;
        }

        public void CreateSystem(tokenClass model)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@System_Name", model.system, DbType.String, ParameterDirection.Input);
            parameters.Add("@Password", ExtendMethods.ConvertToMD5(model.passWord), DbType.String, ParameterDirection.Input);
            parameters.Add("@Enable", true,DbType.Boolean, ParameterDirection.Input);

            var strSQL = @"INSERT INTO HelloAuth.dbo.SystemInfo(System_Name,Password,Enable)
                                    VALUES (@System_Name,@Password,@Enable)";

            Connection.Execute(
                                    sql: strSQL,
                                    param: parameters,
                                    commandType: CommandType.Text
                                );
        }

        //public List<SYSTEMINFO> DeleteSystem(tokenClass model)
        //{
        //    //    var qq = "DELETE FROM HelloAuth.dbo.SystemInfo WHERE System_Name ='" + model.system + "'";
        //    //var qq = "UPDATE  HelloAuth.dbo.SystemInfo SET System_Name =@System_Name ,PassWord=@PassWord WHERE id=@id ";
        //    var parameters = new DynamicParameters();

        //    parameters.Add("@System_Name", model.system, DbType.String, ParameterDirection.Input);
        //    parameters.Add("@PassWord", model.passWord, DbType.String, ParameterDirection.Input);
        //    //parameters.Add("@id", );

        //    var strSQL = @"UPDATE  HelloAuth.dbo.SystemInfo
        //                    SET 
        //                    System_Name =@System_Name ,
        //                    PassWord=@PassWord 
        //                    WHERE id=@id";

        //    List<SYSTEMINFO> sys = Connection.Query<SYSTEMINFO>(strSQL, parameters).ToList();

        //    return sys;
        //}

        public List<SYSTEMINFO> UpdateSystem(tokenClass model)
        {
            var parameters = new DynamicParameters();

            parameters.Add("@System_Name", model.system, DbType.String, ParameterDirection.Input);
            parameters.Add("@PassWord", ExtendMethods.ConvertToMD5(model.newPassword), DbType.String, ParameterDirection.Input);
            parameters.Add("@id", SelectSystem(model)[0].ID);

            var strSQL = @"UPDATE HelloAuth.dbo.SystemInfo
                            SET 
                            System_Name =@System_Name ,
                            PassWord=@PassWord 
                            WHERE id=@id";

            Connection.Execute(
                                    sql: strSQL,
                                    param: parameters,
                                    commandType: CommandType.Text
                                );

            return null;
        }

        public List<SYSTEMINFO> DeleteSystem(tokenClass model)
        {
            var parameters = new DynamicParameters();

            parameters.Add("@Enable", false, DbType.Boolean, ParameterDirection.Input);
            parameters.Add("@id", SelectSystem(model)[0].ID);

            var strSQL = @"UPDATE HelloAuth.dbo.SystemInfo
                            SET 
                            Enable=@Enable
                            WHERE id=@id";

            Connection.Execute(
                                    sql: strSQL,
                                    param: parameters,
                                    commandType: CommandType.Text
                                );

            return null;
        }
    }
}
