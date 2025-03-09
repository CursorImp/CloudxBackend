using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using Microsoft.VisualBasic;
using System.Threading;
using SignalRHub;

namespace SignalRHub
{
    public class BroadCastMessage
    {
        public delegate void MessageReceiveDataHandler(string line,string name,string phoneNumber);
       


        public delegate void AutoRefreshDataHandler(string message);
        public event AutoRefreshDataHandler AutoRefreshMessage;
        //

        public BroadCastMessage()
        {
            //
            //Added to support default instance behavour in C#
            if (defaultInstance == null)
                defaultInstance = this;

            //
            UdpReceiver.DataReceived += new UdpReceiverClass.DataReceivedEventHandler(UdpReceiver_DataReceived);
            UdpReceiveThread = new System.Threading.Thread(new ThreadStart(UdpReceiver.UdpIdleReceive));

            Start();
        }

        void UdpReceiver_DataReceived()
        {
            string sReceivedText = UdpReceiver.sReceivedMessage;
            DataHandler(sReceivedText);
        }

        private void DataHandler(string sDataText)
        {
            RecordDisplay(sDataText);


        }

        #region Default Instance

        private static BroadCastMessage defaultInstance;

        /// <summary>
        /// Added by the VB.Net to C# Converter to support default instance behavour in C#
        /// </summary>



        #endregion

        #region Properties


        public bool bOutboundPopup = false;
        public bool bInboundPopup = true;
        public int nPopupTimer = 3;
        public string fLog;
        public int LogLevel;
        public string sSerial;
        public string sIpRelay = "255.255.255.255";
        public string sAreaCode;

        //public Growl.Connector.GrowlConnector growl = new Growl.Connector.GrowlConnector();

        public bool bDiagOpen = false;

 

      
     
        private UdpReceiverClass UdpReceiver = new UdpReceiverClass();
        private System.Threading.Thread UdpReceiveThread;

    

        public struct WC2DownloadStruct
        {
            public int maxDownloads;
            public int indxDownloads;
            public int dupeDownloads;
            public int newDownloads;
            public int allDownloads;
            public int errorDownloads;
            public bool errorFlag;
            public string errorMessage;
            public bool connected;
        }


    
       
        public WC2DownloadStruct WC2Download;

      
        #endregion
        delegate void Text_delegate(string text);
     
        [DllImport("shell32.dll", EntryPoint = "ShellExecuteA", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern long ShellExecute(long hwnd, string lpOperation, string lpFile, string lpParameters, string lpDirectory, long nShowCmd);




        private void Start()
        {



            UdpReceiveThread.IsBackground = true;
            UdpReceiveThread.Start();


            WC2Download.connected = false;

          
          

        }



        //EL Config and EL Popup use the same port and rarely work at the same time, so EL Config sends an event to EL Popup to shut down it's
        //network receiver and sents another event when it closes to turn it back on.
        public void NetworkStart()
        {
            if (!UdpReceiveThread.IsAlive)
            {
                //GeneralMessage("EL Popup is now running again.", "EL Popup Resuming");
                UdpReceiveThread = new System.Threading.Thread(new System.Threading.ThreadStart(UdpReceiver.UdpIdleReceive));
                UdpReceiveThread.IsBackground = true;
                UdpReceiveThread.Start();
            }
        }
        public void NetworkEnd()
        {
            //GeneralMessage("EL Popup will suspent operation while EL Config is running.", "EL Popup Suspended");
            if (UdpReceiveThread.IsAlive)
            {
                UdpReceiveThread.Abort();
            }
         //   BroadcasterData bcast = new BroadcasterData("255.255.255.255", 3520, "^^IdX"); //The data receiver blocks the thread abort event, so send one extra thing to get the ball rolling.
         //   bcast.SendMessage();
        }

 



        private void RecordDisplay(string message)
        {
            if (message.StartsWith("**") )
            {

                if (AutoRefreshMessage != null)
                {
                    message = message.Replace("**", "").Trim();

                    AutoRefreshMessage(message);
                }

            }
        
           



        }

    



    }
}
