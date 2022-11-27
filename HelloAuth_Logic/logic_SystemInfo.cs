using HelloAuth_Data;
using HelloAuth_Model.Data;
using HelloAuth_Model.Table;
using HelloAuth_Utility;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
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
                    if (sys[0].Enable == false)
                        return false;
                    else if (sys[0].Password == ExtendMethods.ConvertToMD5(model.passWord) || sys[0].Password != null)
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

        public bool UpdateSystem(tokenClass application)
        {
            using (SqlConnection conn = new SqlConnection(this.ConnectionString))
            {
                List<SYSTEMINFO> sys = new Da_SYSTEMINFO(conn).SelectSystem(application);


                if (sys[0].Password == ExtendMethods.ConvertToMD5(application.newPassword))
                    return false;
                else if (sys[0].Password == ExtendMethods.ConvertToMD5(application.passWord))
                {
                    //update
                    new Da_SYSTEMINFO(conn).UpdateSystem(application);
                    return true;
                }
                else
                    return false;
            }
        }

            public bool DeleteSystem(tokenClass application)
            {
                using (SqlConnection conn = new SqlConnection(this.ConnectionString))
                {
                    List<SYSTEMINFO> sys = new Da_SYSTEMINFO(conn).SelectSystem(application);

                     if (sys[0].Password == ExtendMethods.ConvertToMD5(application.passWord))
                    {
                        //update
                        new Da_SYSTEMINFO(conn).DeleteSystem(application);
                        return true;
                    }
                    else
                        return false;
                }
            }
    }
}
