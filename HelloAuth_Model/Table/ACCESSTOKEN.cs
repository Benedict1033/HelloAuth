using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloAuth_Model.Table
{
    public class ACCESSTOKEN
    {
        public Int32 ID { get; set; }

        public String Used_System { get; set; }

        public String IP_Address { get; set; }

        public DateTime Access_DateTime { get; set; }

        public String Access_Status { get; set; }

        public String Exception { get; set; }

    }
}
