using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloAuth_Model.Data
{
    public class dataJWTTOKEN
    {
        public string accessToken { get; set; } 
        public string refreshToken { get; set; } 
        public int expiresIn { get; set; } 
    }
}
