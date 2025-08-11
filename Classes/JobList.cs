using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalRHub
{
    public class JobModel
    {
        public long JobId;
        public int? DriverId;
        public string PickupDateTime;
        public string FromAddress;
        public string ToAddress;
        public int? PaymentTypeId;
        public int? JourneyTypeId;
     
        public decimal? Fares;
        public decimal? Parking;
        public decimal? Waiting;
        public decimal? BookingFee;
        public decimal? Congestion;
        public decimal? AgentFee;

        public decimal? Extra;

        public decimal? TotalCharges;
        public string CustomerName;
        public string PaymentRef;
        public decimal? Mileage;
        public int? BookingStatusId;


        public int DropOffZoneId;
        public string DriverNo;
        public string OldToAddress;

        public bool UpdateCharges;
        public string showFares;

        public string revertstatus = "";
        public List<ChargesSummary> Summary;
    }

    public class DefaultPolicies
    {
        public int Id;
        public int Value;
        public int PolicyName;

    }

     public class ClsAddStop
    {

        public string JobId;
        public string DrvId;
        public string DrvNo;
        public double Latitude;
        public double Longitude;
        public string version;
        public string JStatus;
        public string PickupDateTime;
        public string StopName;


        public string FromAddress;
        public string ToAddress;
        public string PaymentType;
        public string JourneyType;

        public decimal? Fares;
        public decimal? Parking;
        public decimal? Waiting;
        public decimal? BookingFee;
        public decimal? Congestion;
        public decimal? AgentFee;

        public decimal? Extra;

        public bool UpdateCharges;
        public string Message;
    }


    //public class JobSummaryModel
    //{
       
       
        

    //    public decimal? Fares;
    //    public decimal? Parking;
    //    public decimal? Waiting;
    //    public decimal? BookingFee;
    //    public decimal? Congestion;
    //    public decimal? AgentFee;
    //    public decimal? TotalCharges;

    //    public decimal? TotalCash;
    //    public decimal? TotalAccount;
    //    public decimal? TotalCard;

    //    public decimal TotalCashJobs;
    //    public decimal TotalAccountJobs;
    //    public decimal TotalCardJobs;

       
    //}


    public  class JobSummaryModel
    {

        public System.Nullable<decimal> Fares;

        public System.Nullable<decimal> AgentFee;

        public System.Nullable<decimal> BookingFee;

        public System.Nullable<decimal> Congestion;

        public System.Nullable<decimal> Parking;

        public System.Nullable<decimal> Extra;

        public System.Nullable<decimal> Waiting;

        public System.Nullable<decimal> TotalCash;

        public System.Nullable<decimal> TotalAccount;

        public System.Nullable<decimal> TotalCard;

        public System.Nullable<decimal> TotalCharges;

        public System.Nullable<int> TotalCashJobs;

        public System.Nullable<int> TotalAccountJobs;

        public System.Nullable<int> TotalCardJobs;

    }
        public class JobObject
    {
        public long JobId;
        public string DrvId;
        public string DriverNo;
        public string Ver;
        public List<JobModel> Joblist;
        public bool ShowSummary;
        public int FetchType;
        public int PaymentTypeId;
        public string FilterFrom;
        public string FilterTo;

        public JobSummaryModel JobSummary;


        public string Message;
        public string TotalOnlineMin;
    }


    public class BookingInformationData
    {
        public string FromAddress;
        public string ToAddress;
        public int FromLocTypeId;
        public int ToLocTypeId;
        public int VehicleTypeId;
        public int CompanyId;
        public DateTime PickupDateTime;
        public string RouteCoordinates;
        public string MapKey;
        public int MapType;
        public long JobId;
        public ViaAddresses[] Via
        {
            get;
            set;

        }
    }


   


    public class DriverModel
    {
        public int DriverId;
        public int Rank;


    }
}