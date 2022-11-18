using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using HelloAuth_Logic;
using HelloAuth_Model.Table;
using NLog;

namespace HelloAuth_API.Controllers
{
    public class BaseController : ApiController
    {

        public static Logger logger = LogManager.GetCurrentClassLogger();

        #region Logic Objecrt
        private logic_AccessToken _LogicAccessToken;
        public logic_AccessToken LogicAccessToken
        {
            get { return this._LogicAccessToken ?? (this._LogicAccessToken = new logic_AccessToken()); }
        }

        private static logic_SystemInfo _LogicSystemInfo;
        public static logic_SystemInfo LogicSystemInfo
        {
            get { return _LogicSystemInfo ?? (_LogicSystemInfo = new logic_SystemInfo()); }
        }

        //private logic_User _LogicUser;
        //public logic_User LogicUser
        //{
        //    get { return this._LogicUser ?? (this._LogicUser = new logic_User()); }
        //}

        //private logic_Asset _LogicAsset;
        //public logic_Asset LogicAsset
        //{
        //    get { return this._LogicAsset ?? (this._LogicAsset = new logic_Asset()); }
        //}

        #endregion

    }
}