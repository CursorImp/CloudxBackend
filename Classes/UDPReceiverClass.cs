using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;

namespace SignalRHub
{
    public class UdpReceiverClass
    {

        public string sReceivedMessage;
        public bool bPause = false;
        public bool bTestMode = false;
        private string pLastPacket;
        public delegate void DataReceivedEventHandler();
        private DataReceivedEventHandler DataReceivedEvent;

        public event DataReceivedEventHandler DataReceived
        {
            add
            {
                DataReceivedEvent = (DataReceivedEventHandler)System.Delegate.Combine(DataReceivedEvent, value);
            }
            remove
            {
                DataReceivedEvent = (DataReceivedEventHandler)System.Delegate.Remove(DataReceivedEvent, value);
            }
        }

        public delegate void LogEventHandler(string LogMessage, int iLogLevel);
        private LogEventHandler LogEvent;

        public event LogEventHandler Log
        {
            add
            {
                LogEvent = (LogEventHandler)System.Delegate.Combine(LogEvent, value);
            }
            remove
            {
                LogEvent = (LogEventHandler)System.Delegate.Remove(LogEvent, value);
            }
        }


        public void UdpIdleReceive()
        {

            bool done = false;
            Socket udpClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint intEndPoint = new IPEndPoint(IPAddress.Any, 3529);


            //       Try
            udpClient.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            //udpClient.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, True)
            //Catch ex As Exception
            //MsgBox("Unable to bind to port 3520. Try turning off IPTest if it is on.", MsgBoxStyle.Information, Left(ex.ToString, 60) + "...")
            //End Try

            try
            {
                udpClient.Bind(intEndPoint);
            }
            catch (Exception)
            {
                //	MessageBox.Show("Could not connect.");
                return;
            }
            sReceivedMessage = "";
            while (!done)
            {

                byte[] receiveBytes = new byte[4000];
                int nByteCount;
              
                try
                {
                    nByteCount = udpClient.Receive(receiveBytes);

                }
                catch (Exception ex)
                {
                    //If Not TypeOf (ex) Is System.Threading.ThreadAbortException Then MsgBox("Could not receive incoming packet" + vbCrLf + ex.ToString)
                    if ((ex) is System.Threading.ThreadAbortException)
                    {
                        udpClient.Close();
                    }
                    continue;
                }
                if (bTestMode == false)
                {

                    try
                    {
                        sReceivedMessage = Encoding.Default.GetString(receiveBytes, 0, nByteCount);
                   
                        pLastPacket = sReceivedMessage;
                     
                    }
                    catch (Exception)
                    {

                    }
                }
         


                if (sReceivedMessage.StartsWith("**"))
                {
                    if (DataReceivedEvent != null)
                        DataReceivedEvent();

                }
               
              


            }
        }

        public IPAddress GetLocalIPAddress()
        {
            IPAddress ipAddress = null;
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    ipAddress = ip;
                    break;
                }
            }

            return ipAddress;
            // throw new Exception("Local IP Address Not Found!");
        }

    }

}
