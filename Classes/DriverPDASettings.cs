using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalRHub
{
   public  class DriverPDASettings
    {
       public string Ip;
       public string DrvId;
       public string DrvNo;
       public string DrvName;
       public string VehType;
       public string GPSInterval;

       public string EnableJobExtraCharges;
       public string ShowCompletedJobs;
       public string EnableBidding;
       public string ShowPlots;
       public string ShowNavigation;
       public string JobTimeout;
       public string ZoneInterval;
       public string SoundOnZoneChange;
       public string MessageStayOnScreen;
       public string EnableCompanyCars;
       public string EnableFareMeter;
       public string ShowCustomerNo;
       public string HidePickupAndDest;
       public string EnableLogoutOnRejectJob;
       public string NavigationType;
       public string EnableFlagDown;
       public string DisablePanic;
       public string DisableRank;
       public string DisableChangeJobPlot;
       public string EnableJ15Jobs;
       public string EnableLogoutAuth;
       public string EnableIgnoreArrive;
       public string BiddingType;
       public string FareMeterType;
       public string EnableOptMeter;
       public string DisableMeterForAccJob;
       public string ShowFaresOnExtraCharges;
       public string EnableCallCustomer;
       public string EnableRecoverJob;
       public string EnableMeterWaitingCharges;
       public string LogoutOnOverShift;
       public string DisableBase;
       public string DisableBreak;
       public string DisableRejectJob;
       public string DisableChangeDest;
       public string ShowJobasAlert;
       public string DisableNoPickup;
       public string DisableAlarm;
       public string ShowSpecialReqOnFront;
       public string DisableFareOnAccJob;
       public string DisableSTC;
       public string ShowAlertOnJobLate;
       public string EnableAutoRotate;
       public string ShowPlotOnOffer;
       public string OnBreakDur;
       public string ManualFares;
       public string EnablePriceBid;
       public string DrvWaitingMins;
       public string AccWaitingMins;
       public string FontSize;
       public string MeterVoice;
       public string Courier;
       public string DisableJobAuth;
       public string OptionalFareMeter;



        public string isDriverConnectEnabled;
       


        public string drvConHost = "";
        public string drvConusername = "";
        public string drvConPass = "";
        public string drvConPort = "";
        public string isRingback = "";


        public string showDestAfterPob;
        public string EnableDriverConnect;

        public string enableCallOffice = "0";
        public string DriverPay;
        public string EnableBidOnPlots = "0";
        public string SyncBookingHistory = "0";


        public string EnableWaitingAfterArrive="0";

        public string EnableExtrasOnFixedFare = "0";
        public string EnablePickLocation = "0";
        public string ExtrasOnSTC = "1";
        public string SyncMessageTemplates = "0";
        public string SyncFutureJobs = "1";
        public string EnableOnlineStatus = "1";

        public string EnableDriverPinLogin = "0";
        public DriverProfile objProfile;

        public string EnableSocketIO = "0";

        public string EnableLocationAck = "1";
        public string CheckJobStatus = "1";
        public string EnableDriverDocuments = "";
        public string DisableEarning = "";
        public string AcceptJobAdditional = "";
        public string EnableWaitingOnAddStop = "";
        public string EnableParkExtraStop = "";
        public string EnablePassengerChat = "";
        public string EnableUpcomingJob = "";
        public string ShowBidList = "";
        public string HideAccountName = "";

        public string EnablePriceBidding = "";
        public string EnableOptionalManualFares = "";
        public string EnableAutoRotateScreen = "";
        public string EnableCourierSignature = "";
        public string LogoutOnRejectJob = "";
        public string DisableSetAlarm = "";
        public string DisableChangeDestination = "";
        public string ShowJobAsAlert = "";

        public string ShowSpecReqOnFront = "";

        public string OldPdaVersion = "";

        public string NotifyOnJobLate = "";
        public string EnableCustomPayment = "";
        public string EnableSoundAdjustment = "";
        public string EnablekonnectPayReciept = "";
    }
}
