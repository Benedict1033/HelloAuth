using HelloAuth_Utility;
using System.Configuration;

namespace HelloAuth_Data
{
    public class ConnectionDataBase
    {
        private string _connectionString;
        public string ConnectionString
        {
            get { return this._connectionString.ToStringSafe(); }
            set { this._connectionString = value; }
        }

        public ConnectionDataBase()
        {
            this.ConnectionString_SQL_HelloAuth = ConfigurationManager.ConnectionStrings["HelloAuth"].ConnectionString;
        }

        public void ConnectionDataBaseInit(bool is_test)
        {
            this.ConnectionString = ConfigurationManager.ConnectionStrings["HelloAuth"].ConnectionString;
        }

        private string _connectionString_SQL_HelloAuth;
        public string ConnectionString_SQL_HelloAuth
        {
            get { return this._connectionString_SQL_HelloAuth.ToStringSafe(); }
            set { this._connectionString_SQL_HelloAuth = value; }
        }
    }
}
