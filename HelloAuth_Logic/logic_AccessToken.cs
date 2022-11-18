using HelloAuth_Data;
using HelloAuth_Model.Table;
using System;
using System.Data.SqlClient;

namespace HelloAuth_Logic
{
    public class logic_AccessToken : logic_Base
    {

        public bool SaveTokenLog(ACCESSTOKEN application)
        {
            using (SqlConnection conn = new SqlConnection(this.ConnectionString))
            {
                //insert
                new Da_ACCESSTOKEN(conn).SaveLog(application);
            }

            return true;
        }

    }
}
