using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wxdownload
{
    public  class UserData
    {
        public string success { set; get; }

        public string errType { set; get; }

        public string errMsg { set; get; }

        public User_OpenId data { set; get; }

    }
    public class User_OpenId
    {
        public List<User_OpenIds> OpenIds { set; get; }
    }

    public class User_OpenIds
    {
        public string OpenId { set; get; }

        public string NickName { get; set; }

        public string HeadImgUrl { get; set; }

    }

}
