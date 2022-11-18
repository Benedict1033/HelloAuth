using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloAuth_Model.Data
{
    public class dataResults<T>
    {
        /// <summary>
        /// 回傳訊息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 狀態碼
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 資料內容
        /// </summary>
        private T data;
        public virtual T Data
        {
            get
            {
                return data;
            }
            set
            {
                data = value;

            }
        }
    }
    }
