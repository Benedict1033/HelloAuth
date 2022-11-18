using HelloAuth_Data;
using HelloAuth_Model.Data;
using HelloAuth_Model.Table;
using HelloAuth_Utility;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloAuth_Logic
{
    public class logic_SystemInfo : logic_Base
    {

        public bool CheckSystemExits(tokenClass model)
        {
            using (SqlConnection conn = new SqlConnection(this.ConnectionString))
            {
                //select
                List<SYSTEMINFO> sys = new Da_SYSTEMINFO(conn).SelectSystem(model);

                try
                {
                    if (sys[0].Password == ExtendMethods.ConvertToMD5(model.passWord))
                        return true;
                    else
                        return false;
                }
                catch
                {
                    return false;
                }

            }
        }

        public void CreateSystem(tokenClass application)
        {
            using (SqlConnection conn = new SqlConnection(this.ConnectionString))
            {
                //insert
                new Da_SYSTEMINFO(conn).CreateSystem(application);
            }
        }

        public void UpdateSystem(tokenClass application)
        {
            using (SqlConnection conn = new SqlConnection(this.ConnectionString))
            {
                //update
                new Da_SYSTEMINFO(conn).UpdateSystem(application);
            }
        }
    }
}
