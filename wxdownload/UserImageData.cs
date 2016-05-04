using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wxdownload
{
    public class UserImageData
    {
        public string success { set; get; }

        public string errType { set; get; }

        public string errMsg { set; get; }

        public PhotoMpLoadOpenIdDto_OpenId data { set; get; }

    }
  
    public class PhotoMpLoadOpenIdDto_OpenId
    {
        public string OpenIds { set; get; }

        public string NickName { get; set; }

        public string HeadImgUrl { get; set; }

        public List<PhotoDate> Images { set; get; }

        public List<TextDate> Texts { set; get; }

    }
    public class PhotoDate
    {
        public string MsgId { set; get; }

        public string Time { set; get; }

        public string ImageUrl { set; get; }

    }
    public class TextDate
    {
        public string MsgId { set; get; }

        public string Time { set; get; }

        public string Content { set; get; }

    }

}
