using System;

namespace SignalRHub
{
    public class SignalRClient
    {
        private SignalRUserType _UserType;
        private SignalRClientsType _ConnectionType;
        private string _ConnetionID;
        //private long _DomainId;
        private DateTime _ConnectedOn;        
        private SignalRClientsStatus _ConnectionStatus;

        public DateTime ConnectedOn
        {
            get { return _ConnectedOn; }
            set { _ConnectedOn = value; }
        }

        private string _IPAddress;

        public string IPAddress
        {
            get { return _IPAddress; }
            set { _IPAddress = value; }
        }

        public SignalRClientsStatus ConnectionStatus
        {
            get { return _ConnectionStatus; }
            set { _ConnectionStatus = value; }
        }

        public string ConnectionID
        {
            get { return _ConnetionID; }
            set { _ConnetionID = value; }
        }

        public long? DomainId { get; set; }

        public SignalRClientsType ConnectionType
        {
            get { return _ConnectionType; }
            set { _ConnectionType = value; }
        }

        public SignalRUserType UserType
        {
            get { return _UserType; }
            set { _UserType = value; }
        }
    }
}