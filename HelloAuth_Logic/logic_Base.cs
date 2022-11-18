using HelloAuth_Data;
using NLog;

namespace HelloAuth_Logic
{
    public class logic_Base : ConnectionDataBase
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        public logic_Base()
        {
            base.ConnectionDataBaseInit(GlobalVars.G_IS_TEST_MODE);
        }
    }
}
