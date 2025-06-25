using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalRHub
{
    public class FareMeterSettings
    {
        public string RemoveExtraCharges;
        public string ExtraChargesPerQty;
        public string ShowExtraCharges;
        public string ShowBookingFees;
        public List<BookingFeeRange> BookingFeesRange;
        public string ShowParkingCharges;
        public string ChangePlotOnAsDirected;
        public List<MeterTarrif> meterTarrif;
        public string EnableViaAction;
        public string EnableDropOffAction;
        public string EnablePauseMeter;

        public FareMeterSettings()
        {


        }

            public FareMeterSettings(DateTime? PickupDateTime,int? vehicleTypeId,int? subCompanyId
            ,bool autoStartWaiting,decimal autoStartSpeed,int autoStartSecs,int autoStopSpeed,decimal charges)
        {
            RemoveExtraCharges = "1";
            ExtraChargesPerQty = "0.5";
            ShowExtraCharges = "1";
            ShowBookingFees = "1";
            BookingFeesRange = new List<BookingFeeRange>();

            BookingFeesRange.Add(new BookingFeeRange { From = 0, To = 2, Charges = 0.2F });
            BookingFeesRange.Add(new BookingFeeRange { From = 2, To = 5, Charges = 0.5F });
            BookingFeesRange.Add(new BookingFeeRange { From = 5, To = 1000, Charges = 1F });

            //2
            // 0>=FARE && FARE<=2
            // 2>=FARE && FARE<=5
            //using (Taxi_Model.TaxiDataContext db = new Taxi_Model.TaxiDataContext())
            //{

            //    var jobTariff = (from f in db.Fares
            //                     join c in db.Fare_OtherCharges on f.Id equals c.FareId
            //                     where (f.VehicleTypeId == vehicleTypeId && f.SubCompanyId == subCompanyId && f.IsCompanyWise == false)
            //                     select new
            //                     {
            //                         f.StartRate,
            //                         f.StartRateValidMiles,
            //                         f.FromDateTime,
            //                         f.TillDateTime,
            //                         f.FromSpecialDate,
            //                         f.TillSpecialDate,
            //                         f.DayValue,
            //                         f.IsDayWise


            //                     }).ToList();


            //    Global.listMeterTariff = new List<MeterTarrif>();

            //    foreach (var item in jobTariff)
            //    {


            //    }



            //}

            //meterTarrif = new List<MeterTarrif>();
            //meterTarrif.Add(new MeterTarrif { StartRate=3.8f, StartRateValidMiles=1.00f, FromMile=1f, TillMile=2f, Rate=3.8f
            //    , AutoStartWaiting=autoStartWaiting==true?1:0, AutoStartWaitingBelowSpeed=autoStartSpeed, AutoStartWaitingBelowSpeedSeconds=autoStartSecs
            //    , AutoStopWaitingOnSpeed=autoStopSpeed, DrvWaitingChargesPerMin=charges });

            //meterTarrif.Add(new MeterTarrif { StartRate = 3.8f, StartRateValidMiles = 1.00f, FromMile = 2f, TillMile = 100f, Rate = 4f
            //     ,
            //    AutoStartWaiting = autoStartWaiting == true ? 1 : 0,
            //    AutoStartWaitingBelowSpeed = autoStartSpeed,
            //    AutoStartWaitingBelowSpeedSeconds = autoStartSecs
            //    ,
            //    AutoStopWaitingOnSpeed = autoStopSpeed,
            //    DrvWaitingChargesPerMin = charges
            //});

          

            ShowParkingCharges = "1";
            ChangePlotOnAsDirected = "1"; ;
        }

        public FareMeterSettings(bool useDefault)
        {
            RemoveExtraCharges = "0";
            ExtraChargesPerQty = "0";
            ShowExtraCharges = "0";
            ShowBookingFees = "0";
            BookingFeesRange = new List<BookingFeeRange>();


            ShowParkingCharges = "0";
            ChangePlotOnAsDirected = "0";
        }
        public class BookingFeeRange
        {
            public float From;
            public float To;
            public float Charges;

        }


       
    }
}