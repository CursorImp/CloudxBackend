using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quobject.SocketIoClientDotNet.Client;
using Newtonsoft.Json.Linq;
using Utils;
using System.IO;
using System.Configuration;

namespace SignalRHub
{



    public class SocketData
    {
        public SendDataRequest Data { get; set; }
        public string Method { get; set; }
    }


    public class SocketRequest
    {
        public object Data { get; set; }
        public string Method { get; set; }
        public string DriverId { get; set; }
        public string fromname = "";
        public string Id { get; set; }
    }
    public class SocketAck
    {
        public object Data { get; set; }
        public string method { get; set; }
        public string driverId { get; set; }
        public string fromname = "";
        public string Id { get; set; }
    }




    public class SocketIO
    {

        public static DateTime? LastAcknowledgementReceivedOn;

        public static Socket socket = null;
        public void SendSocketMsg(SocketRequest request)
        {
            try
            {
                string URLSocketIO = ConfigurationManager.AppSettings["socketurl"].ToStr();
                if (URLSocketIO.ToStr().Trim().Length > 0)
                {






                    var loginjson = new JObject();
                    loginjson.Add("Data", request.Data.ToStr());
                    loginjson.Add("MethodName", request.Method);
                    loginjson.Add("DriverId", request.DriverId);
                    loginjson.Add("fromname", request.fromname);
                    loginjson.Add("Id", request.Id);



                    if (socket == null)
                    {

                        //  string URLSocketIO = ConfigurationManager.AppSettings["socketurl"].ToStr();


                        var options = new IO.Options() { IgnoreServerCertificateValidation = true, AutoConnect = true, ForceNew = true };

                        socket = IO.Socket(URLSocketIO, options);


                        socket.On(Socket.EVENT_CONNECT, (data) =>
                        {

                            try
                            {
                                //
                                File.AppendAllText(AppContext.BaseDirectory + "\\Socket_EVENT_CONNECT.txt", DateTime.Now.ToStr() + ",ID:" + request.Id.ToStr() + ",data:" + request.Data + Environment.NewLine);
                            }
                            catch
                            {

                            }




                        });

                        if (socket != null)
                        {
                            socket.Emit("ServerResponseSend", loginjson);


                            //

                            socket.On("Ack", (data2) =>
                            {
                                try
                                {
                                    //
                                    File.AppendAllText(AppContext.BaseDirectory + "\\AckReceived.txt", DateTime.Now.ToStr() + ",data:" + data2.ToStr() + Environment.NewLine);
                                    LastAcknowledgementReceivedOn = DateTime.Now;
                                }
                                catch
                                {

                                }
                            });
                        }


                        socket.On(Socket.EVENT_CONNECT_ERROR, (data) =>
                        {
                            try
                            {
                                //
                                File.AppendAllText(AppContext.BaseDirectory + "\\Socket_EVENT_CONNECT_ERROR.txt", DateTime.Now.ToStr() + ",ID:" + request.Id.ToStr() + ",data:" + request.Data + Environment.NewLine);
                            }
                            catch
                            {

                            }

                            socket.Off();
                            socket = null;

                        });

                        socket.On(Socket.EVENT_CONNECT_TIMEOUT, (data) =>
                        {

                            try
                            {
                                //
                                File.AppendAllText(AppContext.BaseDirectory + "\\Socket_EVENT_CONNECT_TIMEOUT.txt", DateTime.Now.ToStr() + ",ID:" + request.Id.ToStr() + ",data:" + request.Data + Environment.NewLine);
                            }
                            catch
                            {

                            }
                            socket.Off();
                            socket = null;
                        });

                        socket.On(Socket.EVENT_DISCONNECT, (data) =>
                        {
                            //

                            try
                            {
                                //
                                File.AppendAllText(AppContext.BaseDirectory + "\\Socket_EVENT_DISCONNECT.txt", DateTime.Now.ToStr() + ",ID:" + request.Id.ToStr() + ",data:" + request.Data + Environment.NewLine);
                            }
                            catch
                            {

                            }

                            socket.Off();
                            socket = null;
                        });
                    }
                    else
                    {
                        socket.Emit("ServerResponseSend", loginjson);
                    }





                    if (LastAcknowledgementReceivedOn == null)
                        LastAcknowledgementReceivedOn = DateTime.Now;


                    if (DateTime.Now.Subtract(LastAcknowledgementReceivedOn.Value).TotalMinutes > 4)
                    {
                        try
                        {
                            //
                            File.AppendAllText(AppContext.BaseDirectory + "\\AutoRecycle.txt", DateTime.Now.ToStr() + ",Last Ack recvd on:" + string.Format("dd/MM/yyyy HH:mm:ss", LastAcknowledgementReceivedOn) + ",data:" + request.Data + Environment.NewLine);
                            LastAcknowledgementReceivedOn = DateTime.Now;
                            General.RecyclePool();

                            //
                        }
                        catch
                        {

                        }

                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    //
                    File.AppendAllText(AppContext.BaseDirectory + "\\SendSocketMsgException.txt", DateTime.Now.ToStr() + " json" + request.Data + Environment.NewLine);
                    General.RecyclePool();
                }
                catch
                {

                }

            }
        }

        public static void SendToSocket(string driverId, string message, string Method, string type = "", string Id = "")
        {
            SocketRequest msgreq = new SocketRequest()
            {
                //
                Data = message,
                DriverId = driverId,
                Method = Method,
                fromname = type,
                Id = Id
            };


            //try
            //{
            //    //
            //    File.AppendAllText(AppContext.BaseDirectory + "\\SendToSocket.txt", DateTime.Now.ToStr() + " Id" + Id + Environment.NewLine);
            //}
            //catch
            //{

            //}

            SocketIO soc = new SocketIO();
            soc.SendSocketMsg(msgreq);
        }
    }
}