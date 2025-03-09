using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalRHub
{
     public class ClsLic
    {
        private string _DefaultClientID;

        public string DefaultClientID
        {
            get { return _DefaultClientID; }
            set { _DefaultClientID = value; }
        }



        private string _CabTrackUrl;
        private string _AppServiceUrl;

        public string CabTrackUrl
        {
            get { return _CabTrackUrl; }
            set { _CabTrackUrl = value; }
        }


        public string AppServiceUrl
        {
            get { return _AppServiceUrl; }
            set { _AppServiceUrl = value; }
        }


        private bool _IsValid;
        //
        public bool IsValid
        {
            get { return _IsValid; }
            set { _IsValid = value; }
        }

        private string _OnlineDataString;

        public string OnlineDataString
        {
            get { return _OnlineDataString; }
            set { _OnlineDataString = value; }
        }
        private string _ExpiryDateTime;

        public string ExpiryDateTime
        {
            get { return _ExpiryDateTime; }
            set { _ExpiryDateTime = value; }
        }
        private string _OtherInformation1;

        public string OtherInformation1
        {
            get { return _OtherInformation1; }
            set { _OtherInformation1 = value; }
        }
        private string _OtherInformation2;

        public string OtherInformation2
        {
            get { return _OtherInformation2; }
            set { _OtherInformation2 = value; }
        }
        private string _OtherInformation3;

        public string OtherInformation3
        {
            get { return _OtherInformation3; }
            set { _OtherInformation3 = value; }
        }
        private string _Reason;

        public string Reason
        {
            get { return _Reason; }
            set { _Reason = value; }
        }

    }
}
