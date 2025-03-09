using System;
using System.Linq;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Taxi_Model;

namespace SignalRHub
{
    public class HubProcessor //: HubProcessorBase<WebDispatchHub>
    {




        #region Data
        public Gen_SysPolicy_Configuration objPolicy = null;
        public List<Gen_Zone_PolyVertice> listofPolyVertices = null;
        public List<clsZones> listOfZone = null;
      

        public List<string> listofSMS = null;
        public List<clsPDA> listofJobs = new List<clsPDA>();
        public System.Timers.Timer smsTimer = null;
        public System.Timers.Timer autoDispatchTimer = null;

        //////////private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(HubProcessor));

        public ConcurrentDictionary<string, SignalRClient> Connections = new ConcurrentDictionary<string, SignalRClient>();

        //  private static object _lock = new object();

        #endregion

        #region Singleton and Constructor



        private static readonly Lazy<HubProcessor> _instance = new Lazy<HubProcessor>(() => new HubProcessor(GlobalHost.ConnectionManager.GetHubContext<DispatchHub>().Clients));

        public static HubProcessor Instance
        {
            get { return _instance.Value; }
        }

        private HubProcessor(IHubConnectionContext<dynamic> clients)
        {
            Clients = clients;
        }

        public IHubConnectionContext<dynamic> Clients
        {
            get;
            set;
        }

        #endregion

        #region Connection

        public void Connect(HubCallerContext Context)
        {

            if (Context != null)
            {
                long UserIdInConnection = Context.QueryString["SignalRClientDomainId"] != "" ? Convert.ToInt64(Context.QueryString["SignalRClientDomainId"]) : 0;
                var client = Connections.FirstOrDefault(x => x.Key == Context.ConnectionId);

                if (UserIdInConnection != 0 && Context.QueryString["SignalRUserType"] != null)
                {
                    try
                    {
                        var AllExists = Connections.Where(x => x.Value.DomainId == UserIdInConnection).Select(x => x.Key).ToList();
                        if (AllExists != null && AllExists.Count > 0)
                        {
                            for (int i = 0; i < AllExists.Count; i++)
                            {
                                SignalRClient oldClient = Connections.Where(x => x.Key == AllExists[i]).FirstOrDefault().Value;
                                Connections.TryRemove(AllExists[i], out oldClient);

                            }
                        }
                    }
                    catch (Exception exx)
                    {
                        try
                        {
                            File.AppendAllText(AppContext.BaseDirectory + "\\hublog_OnDisconnected.txt", DateTime.Now + "," + "onDisconnect() catch" + exx.Message + Environment.NewLine);
                        }
                        catch
                        {

                        }


                    }
                }

                if (!string.IsNullOrEmpty(client.Key))
                {
                    ////////////log.Info("Connect -- Client updated by ConnectionId = " + UserIdInConnection);
                    SignalRClient oldClient = Connections.Where(x => x.Key == Context.ConnectionId).FirstOrDefault().Value;
                    Connections.TryUpdate(Context.ConnectionId, oldClient, Connections[Context.ConnectionId]);
                }
                else
                {
                    AddSignalRClient(Context);
                }


            }
        }


        public void Reconnect(HubCallerContext Context)
        {
            if (Connections != null)
            {
                var client = Connections.FirstOrDefault(x => x.Key == Context.ConnectionId);
                if (!string.IsNullOrEmpty(client.Key))
                {

                    SignalRClient oldClient = Connections.Where(x => x.Key == Context.ConnectionId).FirstOrDefault().Value;
                    Connections.TryUpdate(Context.ConnectionId, oldClient, Connections[Context.ConnectionId]);
                }
                else
                {
                    Connect(Context);
                }
            }
        }

        public void Disconnect(HubCallerContext Context)
        {
            if (Context != null)
            {
                SignalRClient oldClient = Connections.Where(x => x.Key == Context.ConnectionId).FirstOrDefault().Value;
                Connections.TryRemove(Context.ConnectionId, out oldClient);
            }
        }

        //public string Disconnect(HubCallerContext Context)
        //{
        //    if (Context != null)
        //    {
        //        SignalRClient oldClient = Connections.Where(x => x.Key == Context.ConnectionId).FirstOrDefault().Value;
        //        var a = oldClient.DomainId.ToString();
        //        Connections.TryRemove(Context.ConnectionId, out oldClient);
        //        return a;
        //    }
        //    return "";
        //}
        #endregion

        private void AddSignalRClient(HubCallerContext Context)
        {
            int ClientTypeID = 0;
            long ClientDomainID = 0;
            long ClientUserTypeID = 0;

            SignalRClient client = new SignalRClient();
            client.ConnectedOn = DateTime.Now;
            client.ConnectionStatus = SignalRClientsStatus.CONNECTED;
            client.ConnectionID = Context.ConnectionId;

            var qs = Context.QueryString.Get("SignalRClientsType");
            var qsUserType = Context.QueryString.Get("SignalRUserType");
            var qsID = Context.QueryString.Get("SignalRClientDomainId");

            if (qs == null)
            {
                client.ConnectionType = SignalRClientsType.ANONYMOUS;
            }
            else
            {
                if (int.TryParse(qs, out ClientTypeID))
                {

                    //try
                    //{
                    if ((int)SignalRClientsType.ANDROID == ClientTypeID)
                    {
                        client.ConnectionType = SignalRClientsType.ANDROID;
                    }
                    else if ((int)SignalRClientsType.WEBBROWSER == ClientTypeID)
                    {
                        client.ConnectionType = SignalRClientsType.WEBBROWSER;
                    }
                    else if ((int)SignalRClientsType.IOS == ClientTypeID)
                    {
                        client.ConnectionType = SignalRClientsType.IOS;
                    }
                    else if ((int)SignalRClientsType.DESKTOP == ClientTypeID)
                    {
                        client.ConnectionType = SignalRClientsType.DESKTOP;
                    }
                }
                //catch (Exception)
                //{
                //    client.ConnectionType = SignalRClientsType.ANONYMOUS;
                //}
                //}
                else
                {
                    client.ConnectionType = SignalRClientsType.ANONYMOUS;
                }
            }
            switch (Convert.ToInt16(qsUserType))
            {
                case 1:
                    client.UserType = SignalRUserType.WEB;
                    break;
                case 2:
                    client.UserType = SignalRUserType.DESKTOP;
                    break;
                case 3:
                    client.UserType = SignalRUserType.DRIVER;
                    break;
                case 4:
                    client.UserType = SignalRUserType.CLIENT;
                    break;
                default:
                    break;
            }
            if (qsID == null || qsID == "")
            {
                client.DomainId = null;
            }
            else
            {
                client.DomainId = Convert.ToInt64(qsID);
            }
            Connections[Context.ConnectionId] = client;
        }

        public void UpdateSignalRClient(HubCallerContext Context, long domainId)
        {
            if (Context != null)
            {
                //SignalRClient client;
                SignalRClient oldClient = Connections.Where(x => x.Key == Context.ConnectionId).FirstOrDefault().Value;
                SignalRClient newClient = oldClient;
                newClient.DomainId = domainId;
                Connections.TryUpdate(Context.ConnectionId, newClient, oldClient);
            }
        }

        public void RegisterClient(string conId, long domainId)
        {
            if (Connections != null)
            {
                try
                {
                    SignalRClient client = Connections.Where(x => x.Value.ConnectionID == conId).FirstOrDefault().Value;
                    client.DomainId = domainId;
                    Connections.TryUpdate(conId, client, Connections[client.ConnectionID]);
                }
                catch (Exception ex)
                {
                    ////////////log.Error(ex.Message, ex);
                }
            }
        }

        public List<string> ReturnDriverConnections(int? driverID = 0)
        {
            List<string> DriverConnectionIDs;
            if (driverID == 0)
            {
                DriverConnectionIDs = Connections.Where(x => x.Value.UserType == SignalRUserType.DRIVER).Select(x => x.Key).ToList();
            }
            else
            {
                DriverConnectionIDs = Connections.Where(x => x.Value.UserType == SignalRUserType.DRIVER && x.Value.DomainId == driverID).Select(x => x.Key).ToList();
            }
            return DriverConnectionIDs;
        }

        public List<string> ReturnConnections(string connectionID)
        {
            List<string> connections;
            connections = Connections.Where(x => x.Value.ConnectionID == connectionID).Select(x => x.Key).ToList();
            return connections;
        }

        public List<string> ReturnWebConnections(int? userID = 0)
        {
            List<string> UserConnectionIDs;
            if (userID == 0)
            {
                UserConnectionIDs = Connections.Where(x => x.Value.UserType == SignalRUserType.WEB).Select(x => x.Key).ToList();
            }
            else
            {
                UserConnectionIDs = Connections.Where(x => x.Value.UserType == SignalRUserType.DRIVER && x.Value.DomainId == userID).Select(x => x.Key).ToList();
            }
            return UserConnectionIDs;
        }

        public List<string> ReturnDesktopConnections(int? userID = 0)
        {
            List<string> UserConnectionIDs;
            if (userID == 0)
            {
                UserConnectionIDs = Connections.Where(x => x.Value.UserType == SignalRUserType.DESKTOP).Select(x => x.Key).ToList();
            }
            else
            {
                UserConnectionIDs = Connections.Where(x => x.Value.UserType == SignalRUserType.DRIVER && x.Value.DomainId == userID).Select(x => x.Key).ToList();
            }
            return UserConnectionIDs;
        }

    }
}