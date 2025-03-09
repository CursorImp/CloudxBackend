using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using Taxi_Model;
using System.IO;

using Utils;
using Taxi_BLL;

using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Xml;
using System.Net;

using System.Net.Sockets;
using System.Reflection;

using System.Collections;
using System.Data.Linq;
using DotNetCoords;
//

namespace SignalRHub
{
	public class BroadcasterData
	{
		
		#region Delegates
		public delegate void MessageSuccess();
		public delegate void MessageFailure();
		#endregion
		
		#region Private Fields
		private string _NetIPAddress;
		private short _Port;
		private string _BroadcastMessage;
		
		private byte[] _Info;
		//Points to MessageSuccess()
		private MessageSuccess MessageSentEvent;
		public event MessageSuccess MessageSent
		{
			add
			{
				MessageSentEvent = (MessageSuccess) System.Delegate.Combine(MessageSentEvent, value);
			}
			remove
			{
				MessageSentEvent = (MessageSuccess) System.Delegate.Remove(MessageSentEvent, value);
			}
		}
		
		//Points to MessageFailure
		private MessageFailure MessageFailedEvent;
		public event MessageFailure MessageFailed
		{
			add
			{
				MessageFailedEvent = (MessageFailure) System.Delegate.Combine(MessageFailedEvent, value);
			}
			remove
			{
				MessageFailedEvent = (MessageFailure) System.Delegate.Remove(MessageFailedEvent, value);
			}
		}
		
		#endregion
		
		#region Properties
		public string NetIPAddress
		{
			get
			{
				return _NetIPAddress;
			}
			set
			{
				_NetIPAddress = value;
			}
		}
		
		public short Port
		{
			get
			{
				return _Port;
			}
			set
			{
				_Port = value;
			}
		}
		public string BroadcastMessage
		{
			get
			{
				return _BroadcastMessage;
			}
			set
			{
				_BroadcastMessage = value;
			}
		}
		#endregion
		
		#region Methods
	
		public BroadcasterData(string IP_Address, short PortNumber, string Msg)
		{
			this.NetIPAddress = IP_Address;
			this.Port = PortNumber;
			this.BroadcastMessage = Msg;
		}
	
        public BroadcasterData(string IP_Address, short PortNumber)
		{
			this.NetIPAddress = IP_Address;
			this.Port = PortNumber;
		}

      

        public BroadcasterData()
        {
            //this.BroadcastMessage = "refresh dashboard";
           // this.NetIPAddress = IP_Address;
            this.Port = 3530;
        }
		
		public void BroadCastToAll(string message)
		{

            using (Socket myClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {


                myClient.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                myClient.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
                IPEndPoint mc2EndPoint = new IPEndPoint(IPAddress.Any, 0);
                myClient.SendTimeout = 5000;

                myClient.Bind(mc2EndPoint);

                _Info = System.Text.Encoding.UTF8.GetBytes(message);

                IPEndPoint EndPoint = new IPEndPoint(IPAddress.Broadcast, this.Port);
                //	IPEndPoint EndPoint = new IPEndPoint(IPAddress.Parse(this.NetIPAddress), this.Port);
                //IPEndPoint EndPoint2 = new IPEndPoint(IPAddress.Parse("192.168.0.19"), this.Port);


                try
                {
                    myClient.SendTo(this._Info, this._Info.Length, System.Net.Sockets.SocketFlags.None, EndPoint);




                    if (MessageFailedEvent != null)
                        MessageFailedEvent();
                }
                catch (System.Net.Sockets.SocketException)
                {

                    if (MessageSentEvent != null)
                        MessageSentEvent();
                }

                myClient.Close();
            }
		
		}

        public void BroadCastToLocal(string message)
        {


            IPAddress ip = GetLocalIPAddress();

            using (Socket sockClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                sockClient.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                byte[] _Info = System.Text.Encoding.UTF8.GetBytes(message);

                IPEndPoint EndPoint = new IPEndPoint(ip, 3530);
                sockClient.SendTo(_Info, _Info.Length, System.Net.Sockets.SocketFlags.None, EndPoint);

                sockClient.Close();
            }
        }

        public void BroadCastToLocal(string message,int Bport)
        {


            IPAddress ip = GetLocalIPAddress();

            using (Socket sockClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                sockClient.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                byte[] _Info = System.Text.Encoding.UTF8.GetBytes(message);

                IPEndPoint EndPoint = new IPEndPoint(ip, Bport);
                sockClient.SendTo(_Info, _Info.Length, System.Net.Sockets.SocketFlags.None, EndPoint);

                sockClient.Close();
            }
        }


        public void SendMessage(IPAddress ipAddress, string message)
        {



            //myClient.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            //  _Info = System.Text.Encoding.UTF8.GetBytes(message);


            //IPEndPoint EndPoint = new IPEndPoint(ipAddress, this.Port);
           

            //try
            //{
            //    myClient.SendTo(this._Info, this._Info.Length, System.Net.Sockets.SocketFlags.None, EndPoint);



            //    if (MessageFailedEvent != null)
            //        MessageFailedEvent();
            //}
            //catch (System.Net.Sockets.SocketException)
            //{

            //    if (MessageSentEvent != null)
            //        MessageSentEvent();
            //}

            //myClient.Close();

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

		#endregion
	}
	
	
	
	
	
}
