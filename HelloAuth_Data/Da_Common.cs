using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloAuth_Data
{
    public class Da_Common
    {
        SqlConnection cn;
        public Da_Common(SqlConnection conn)
        {
            this.cn = conn;
        }
    }
}
