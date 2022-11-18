using Dapper;
using HelloAuth_Model.Table;
using System;
using System.Data;
using System.Data.SqlClient;

namespace HelloAuth_Data
{
    public class Da_ACCESSTOKEN : DataRepository<ACCESSTOKEN>
    {
        SqlConnection cn;
        public Da_ACCESSTOKEN(SqlConnection conn) : base(conn)
        {
            this.cn = conn;
        }

        public int SaveLog(ACCESSTOKEN application)
        {
            int rtnValue = 0;

            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Used_System", application.Used_System, DbType.String, ParameterDirection.Input);
                parameters.Add("@IP_Address",application.IP_Address,DbType.String,ParameterDirection.Input);
                parameters.Add("@Access_DateTime", application.Access_DateTime, DbType.DateTime, ParameterDirection.Input);
                parameters.Add("@Access_Status", application.Access_Status, DbType.String, ParameterDirection.Input);
                parameters.Add("@Exception", application.Exception, DbType.String, ParameterDirection.Input);

                var strSQL = @"INSERT INTO HelloAuth.dbo.token(Used_System,IP_Address,Access_DateTime,Access_Status,Exception)
                                    VALUES (@Used_System,@IP_Address,@Access_DateTime,@Access_Status,@Exception)";

                Connection.Execute
                            (
                                sql: strSQL,
                                param: parameters,
                                commandType: CommandType.Text
                            );
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return -1;
            }
            return rtnValue;
        }
    }
}
