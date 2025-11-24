using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalRHub
{
    public class BookingInformation
    {



        public int? SubCompanyId
        {
            get;
            set;
        }

        public string fromLatLng;
        public string toLatLng;


        public string FromAddress
        {
            get;
            set;
        }

        public string MapKey = "";
        public int MapType;


        public int VehicleTypeId;


        public string ToAddress
        {
            get;
            set;
        }


        public string FromType
        {
            get;
            set;
        }
        public string RouteCoordinates
        {
            get;
            set;

        }

        public string ToType
        {
            get;
            set;
        }



        public decimal Fares
        {
            get;
            set;
        }


        public int CompanyId
        {
            get;
            set;
        }


        public string Vehicle
        {
            get;
            set;
        }
         public string ReturnVehicle
        {
            get;
            set;
        }



        public string PickupDateTime
        {
            get;
            set;
        }


        public string returnPickupDateTime { get; set; }

        public decimal Mileage
        {
            get;
            set;

        }

        public string Miles
        {
            get;
            set;

        }

        public int Duration
        {
            get; set;
        }


        public int Noofhours { get; set; }
        public string PaymentType { get; set; }

        public ViaAddresses[] Via
        {
            get;
            set;

        }
        public int? ReturnVehicleTypeId { get;  set; }
    }

    public class ViaAddresses
    {


        public string Viaaddress
        {
            get;
            set;
        }


        public string Viatype
        {
            get;
            set;
        }


        public string ViaCoordinates
        {

            get;
            set;
        }

    }


    public class clsSTCReminder
    {
        public long JobId;
        public int DriverId;
        public int FixedZoneId;
        public int NewZoneId;


    }

    public class FaresInformation
    {

        public long ID { get; set; }
        public string Name { get; set; }
        public int StartRate { get; set; }
        public short? NoOfLuggages { get; set; }
        public short? HandLuggages { get; set; }
        public short? NoOfPassengers { get; set; }
        public int SortOrder { get; set; }
        public decimal? StartRateValidMiles { get; set; }
        public decimal? Fare { get; set; }
        public decimal? ReturnFare { get; set; }

    }

}
