using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;


namespace HelloAuth_Data
{
    public class DataRepository<T> where T : class
    {
        public IDbConnection Connection { get; private set; }
        public static Logger logger = NLog.LogManager.GetCurrentClassLogger();

        protected DataRepository(IDbConnection conn) //: base(conn)
        {
            if (conn == null)
                throw new ArgumentNullException("no database connection");

            this.Connection = conn;
        }

        protected IEnumerable<T> GetList(string query, object arguments, CommandType type)
        {
            IList<T> entities;

            using (Connection)
            {

                entities = Connection.Query<T>(query, arguments, commandType: type).ToList();
            }

            return entities;
        }

        protected T GetSingleOrDefault(string query, object arguments, CommandType type)
        {
            T entity;

            using (Connection)
            {
                entity =
                    Connection.Query<T>(query, arguments, commandType: type).SingleOrDefault();
            }

            return entity;
        }

        protected void Update(string query, object arguments, CommandType type)
        {
            using (Connection)
            {
                Connection.Execute(query, arguments, commandType: type);
            }
        }

        protected int ExecuteScalar(string query, object arguments, CommandType type)
        {
            var id = 0;
            using (Connection)
            {
                id = Connection.ExecuteScalar<int>(query, arguments, commandType: type);
            }
            return id;
        }

        protected long GetSeqNextVal(string SeqName)
        {

            var data = new long();

            string sql = " select " + SeqName + ".nextval from dual  ";
            var result = this.Connection.Query<int>(sql, null).FirstOrDefault();

            data = result;


            return data;
        }
    }
}

