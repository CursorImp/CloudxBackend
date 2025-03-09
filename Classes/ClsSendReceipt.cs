using System;

using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

using Taxi_BLL;
using Taxi_Model;
using Utils;

namespace SignalRHub
{
    public class ClsSendReceipt
    {

        string body = string.Empty;

        Gen_Coordinate objFromJobCoord = null;

        Gen_Coordinate objToJobCoord = null;



        public void SendReceipt(long Id)
        {

            try
            {

                string str = string.Empty;

                if (Id != 0)
                {
                    using (TaxiDataContext taxiDataContext = new TaxiDataContext())
                    {
                        try
                        {
                            taxiDataContext.DeferredLoadingEnabled = false;
                            taxiDataContext.CommandTimeout = 3;
                            var booking = taxiDataContext.Bookings.Where(c => c.Id == Id && (c.CustomerEmail != null && c.CustomerEmail != ""))
                                .Select(args => new
                                {
                                    args.Id,
                                    args.BookingNo,
                                    args.PickupDateTime,
                                    args.BookingDate,
                                    args.FromAddress,
                                    args.ToAddress,
                                    args.FromDoorNo,
                                    args.ToDoorNo,
                                    args.FromStreet,
                                    args.ToStreet,
                                    args.FromLocTypeId,
                                    args.ToLocTypeId
                                ,
                                    args.JourneyTypeId,
                                    args.TotalTravelledMiles
                                ,
                                    args.PaymentTypeId,
                                    args.PaymentComments,
                                    args.SpecialRequirements
                                ,
                                    args.CustomerName,
                                    args.CustomerEmail,
                                    args.CustomerMobileNo,
                                    args.CustomerPhoneNo
                                ,
                                    args.FareRate,
                                    args.CompanyPrice,
                                    args.ParkingCharges,
                                    args.CongtionCharges
                                ,
                                    args.WaitingCharges,
                                    args.MeetAndGreetCharges,
                                    args.ExtraDropCharges,
                                    args.ServiceCharges
                                ,
                                    args.CompanyId,
                                    args.SubcompanyId,
                                    args.VehicleTypeId
                                }).FirstOrDefault();
                            if (booking != null && booking.CustomerEmail.ToStr().Trim().Length > 0 && booking.PaymentTypeId.ToInt() != 2)
                            {
                                var genSubCompany = taxiDataContext.Gen_SubCompanies.Where(c => c.Id == booking.SubcompanyId)
                                    .Select(args => new
                                    {
                                        args.Id,
                                        args.CompanyName,
                                        args.EmailAddress,
                                        args.Address,
                                        args.TelephoneNo,
                                        args.WebsiteUrl,
                                        args.SmtpHost,
                                        args.SmtpPassword,
                                        args.SmtpUserName,
                                        args.SmtpPort,
                                        args.SmtpHasSSL,
                                        args.EmailCC
                                    }
                                    ).FirstOrDefault();



                                string postCodeMatch = GetPostCodeMatch(booking.FromAddress.ToStr().ToUpper().Trim());
                                string postCodeMatch1 = GetPostCodeMatch(booking.ToAddress.ToStr().ToUpper().Trim());
                                stp_getCoordinatesByAddressResult stpGetCoordinatesByAddressResult = taxiDataContext.stp_getCoordinatesByAddress(booking.FromAddress.ToStr().ToUpper().Trim(), postCodeMatch).ToList<stp_getCoordinatesByAddressResult>().FirstOrDefault<stp_getCoordinatesByAddressResult>();
                                if (str.ToStr().Trim().Length == 0)
                                {
                                    str = taxiDataContext.ExecuteQuery<string>("select APIKey from MapKeys where MapType='google'", new object[0]).FirstOrDefault<string>().ToStr().Trim();
                                }
                                if (stpGetCoordinatesByAddressResult != null)
                                {
                                    this.objFromJobCoord = new Gen_Coordinate()
                                    {
                                        Latitude = stpGetCoordinatesByAddressResult.Latitude,
                                        Longitude = stpGetCoordinatesByAddressResult.Longtiude
                                    };
                                }
                                stp_getCoordinatesByAddressResult stpGetCoordinatesByAddressResult1 = taxiDataContext.stp_getCoordinatesByAddress(booking.ToAddress.ToStr().ToUpper().Trim(), postCodeMatch1).ToList<stp_getCoordinatesByAddressResult>().FirstOrDefault<stp_getCoordinatesByAddressResult>();
                                if (stpGetCoordinatesByAddressResult1 != null)
                                {
                                    this.objToJobCoord = new Gen_Coordinate()
                                    {
                                        Latitude = stpGetCoordinatesByAddressResult1.Latitude,
                                        Longitude = stpGetCoordinatesByAddressResult1.Longtiude
                                    };
                                }


                                long id = booking.Id;

                                string vehicle = taxiDataContext.Fleet_VehicleTypes.Where(c => c.Id == booking.VehicleTypeId).Select(c => c.VehicleType).FirstOrDefault();
                                string fromAddress = booking.FromAddress;
                                string toAddress = booking.ToAddress;
                                int? fromLocTypeId = booking.FromLocTypeId;
                                int? toLocTypeId = booking.ToLocTypeId;
                                string empty1 = string.Empty;
                                string str1 = string.Empty;
                                string empty2 = string.Empty;
                                string str2 = string.Empty;
                                string empty3 = string.Empty;
                                string toStreet = string.Empty;
                                string str3 = string.Empty;
                                string empty4 = string.Empty;
                                int nullable = (fromLocTypeId.HasValue ? fromLocTypeId.Value : 0);
                                int aIRPORT = Enums.LOCATION_TYPES.AIRPORT;
                                if ((nullable != aIRPORT ? 0 : nullable) == 0)
                                {
                                    nullable = (fromLocTypeId.HasValue ? fromLocTypeId.Value : 0);
                                    aIRPORT = Enums.LOCATION_TYPES.ADDRESS;
                                    if ((nullable != aIRPORT ? 0 : nullable) == 0)
                                    {
                                        nullable = (fromLocTypeId.HasValue ? fromLocTypeId.Value : 0);
                                        aIRPORT = Enums.LOCATION_TYPES.POSTCODE;
                                        if ((nullable != aIRPORT ? 0 : nullable) == 0)
                                        {
                                            str3 = "";
                                            empty3 = "";
                                        }
                                        else
                                        {
                                            if (booking.FromDoorNo.ToStr().ToStr().Trim() != string.Empty)
                                            {
                                                str3 = "Door No :";
                                            }
                                            if (booking.FromStreet.ToStr().ToStr().Trim() != string.Empty)
                                            {
                                                empty2 = "Street :";
                                            }
                                        }
                                    }
                                    else if (booking.FromDoorNo.ToStr().ToStr().Trim() != string.Empty)
                                    {
                                        str3 = "Door No :";
                                    }
                                }
                                else
                                {
                                    if (booking.FromDoorNo.ToStr().ToStr().Trim() != string.Empty)
                                    {
                                        str3 = "Flight No :";
                                    }
                                    if (booking.FromStreet.ToStr().Trim() != string.Empty)
                                    {
                                        empty2 = "Coming From :";
                                    }
                                }
                                nullable = (fromLocTypeId.HasValue ? fromLocTypeId.Value : 0);
                                aIRPORT = Enums.LOCATION_TYPES.AIRPORT;
                                if ((nullable != aIRPORT ? 0 : nullable) == 0)
                                {
                                    nullable = (fromLocTypeId.HasValue ? fromLocTypeId.Value : 0); ;
                                    aIRPORT = Enums.LOCATION_TYPES.ADDRESS;
                                    if ((nullable != aIRPORT ? 0 : nullable) == 0)
                                    {
                                        nullable = (fromLocTypeId.HasValue ? fromLocTypeId.Value : 0);
                                        aIRPORT = Enums.LOCATION_TYPES.POSTCODE;
                                        if ((nullable != aIRPORT ? 0 : nullable) == 0)
                                        {
                                            empty4 = "";
                                            toStreet = "";
                                        }
                                        else
                                        {
                                            if (booking.ToDoorNo.ToStr().ToStr().Trim() != string.Empty)
                                            {
                                                empty4 = "Door No :";
                                            }
                                            if (booking.ToStreet.ToStr().ToStr().Trim() != string.Empty)
                                            {
                                                str2 = "Street :";
                                            }
                                        }
                                    }
                                    else if (booking.ToDoorNo.ToStr().ToStr() != string.Empty)
                                    {
                                        empty4 = "Door No :";
                                    }
                                }
                                else
                                {
                                    if (booking.ToDoorNo.ToStr().ToStr() != string.Empty)
                                    {
                                        empty4 = "Flight No :";
                                    }
                                    toStreet = booking.ToStreet;
                                }

                                string str4 = genSubCompany.CompanyName.ToStr();
                                if (string.IsNullOrEmpty(string.Empty))
                                {
                                    string.Concat("<td colspan='2' align='center' style='font-family: Arial, Helvetica, sans-serif; background-repeat: no-repeat;background-position: center top; background-color: #4d8fef; font-weight: bold;font-size: 18px; font-style: normal; line-height: normal; font-variant: normal;text-transform: none; color: White; text-decoration: none;'>", str4, "</td>");
                                }
                                decimal num1 = 0.00m;

                                if (booking.CompanyId == null)
                                {
                                    num1 = (booking.FareRate.ToDecimal() + booking.ExtraDropCharges.ToDecimal())
                                       + booking.MeetAndGreetCharges.ToDecimal() + booking.CongtionCharges.ToDecimal()+ booking.ServiceCharges.ToDecimal();



                                }
                                else
                                {
                                    num1 = (((booking.FareRate.ToDecimal() + booking.ParkingCharges.ToDecimal())
                                    + booking.WaitingCharges.ToDecimal()) + booking.ExtraDropCharges.ToDecimal() + booking.ServiceCharges.ToDecimal());

                                }

                                string empty5 = string.Empty;
                                if (booking.TotalTravelledMiles.HasValue)
                                {
                                    empty5 = string.Concat(booking.TotalTravelledMiles, " (miles)");
                                }
                                string str5 = "morning";
                                int hour = booking.PickupDateTime.Value.Hour;
                                if (!(hour < 12 ? true : hour >= 17))
                                {
                                    str5 = "afternoon";
                                }
                                else if (!(hour <= 0 ? true : hour >= 12))
                                {
                                    str5 = "morning";
                                }
                                else if (hour >= 17)
                                {
                                    str5 = "evening";
                                }
                                this.body = string.Empty;
                                //object[] objArray = new object[] { "<!DOCTYPE html><html xmlns=\"http://www.w3.org/1999/xhtml\"><head><title></title></head><body>", this.body
                                //    , "<table border=\"0\" style=\" border-collapse: collapse; background-color: #FFF; width: 100%; float: left; text-align: center; font-family: Verdana, arial;\" cellpadding=\"1\" cellspacing=\"0\" align=\"center\">", this.body
                                //    , "<tr>", this.body, "<td colspan=\"3\" style=\"padding: 1%;color: #6b97c2;\">", this.body
                                //    , "<div style=\"font-size:28px;font-weight:bold;margin:0px 0 10px 0;padding:0px;\">", str4, "</div>",
                                //    this.body, "<div style=\"font-size: 17px; font-weight: bold; margin: 0px 0 10px 0; padding: 0px;\">", genSubCompany.TelephoneNo.ToString(), "</div>",
                                //    this.body, "<p style=\"font-size: 15px; font-weight: bold; margin: 0px 0 10px 0; padding: 0px;\">", genSubCompany.Address.ToString(), "</p>", this.body, "</td>",
                                //    this.body, "</tr>", this.body, "<tr style=\"background: #eff3f9; text-align: left; font-family: 'Times New Roman', arial;\">",
                                //    this.body, "<td style=\"width: 35%; font-size: 13px; font-weight: bold; margin: 0px 0 10px 0; padding: 8px 7px;\">Dear: &nbsp; ", booking.CustomerName, "</td>",
                                //    this.body, "<td style=\"width: 64%; font-size: 13px; font-weight: bold; text-align: right; margin: 0px 0 10px 0; padding: 8px 7px; \">REF NO:&nbsp; ", booking.BookingNo, "</td>", this.body, "</tr>", this.body, "</table>",
                                //    this.body, "<table border=\"1\" style=\"border: solid 1px #d4e0ee; border-top: none; border-collapse: collapse; background-color: #FFF; width: 100%; float: left; text-align: left; font-family: Verdana, arial;\" cellpadding=\"0\" cellspacing=\"0\" align=\"center\">", this.body, "<tr>",
                                //    this.body, "<td style=\"vertical-align:top\">", this.body, "<table border=\"0\" style=\"border: none; border-collapse: collapse; background-color: #FFF; width: 100%; float: left; text-align: left; font-family: Verdana, arial; \" cellpadding=\"0\" cellspacing=\"0\" align=\"center\">",
                                //    this.body, "<tr>", this.body, "<td style=\"width: 30%; font-size: 12px; font-weight: normal; margin: 0px ; padding: 7px; border-right: solid 1px #d4e0ee; border-bottom: solid 1px #d4e0ee;\">Passenger:</td>",
                                //    this.body, "<td style=\"width: 70%; font-size: 12px; font-weight: normal; margin: 0px ; padding: 7px; border-bottom: solid 1px #d4e0ee;\">", booking.CustomerName.ToString(), "</td>", this.body, "</tr>", this.body, "<tr>",
                                //    this.body, "<td style=\"width: 30%; font-size: 12px; font-weight: normal; margin: 0px ; padding: 7px; border-right: solid 1px #d4e0ee; border-bottom: solid 1px #d4e0ee;\">Pickup Date/Time:</td>",
                                //    this.body, "<td style=\"width: 70%; font-size: 12px; font-weight: normal; margin: 0px ; padding: 7px; border-bottom: solid 1px #d4e0ee;\">", booking.PickupDateTime, "</td>", this.body, "</tr>", this.body, "<tr>",
                                //    this.body, "<td style=\"width: 30%; font-size: 12px; font-weight: normal; margin: 0px ; padding: 7px; border-right: solid 1px #d4e0ee; border-bottom: solid 1px #d4e0ee;\">Vehicle:</td>", this.body, "<td style=\"width: 70%; font-size: 12px; font-weight: normal; margin: 0px ; padding: 7px; border-bottom: solid 1px #d4e0ee;\">", booking.Fleet_VehicleType.VehicleType, "</td>", this.body, "</tr>", this.body, "<tr>", this.body, "<td style=\"width: 30%; font-size: 12px; font-weight: normal; margin: 0px ; padding: 7px; border-right: solid 1px #d4e0ee; border-bottom: solid 1px #d4e0ee;\">Special Ins:</td>", this.body, "<td style=\"width: 70%; font-size: 12px; font-weight: normal; margin: 0px ; padding: 7px; border-bottom: solid 1px #d4e0ee;\">", booking.NotesString, "</td>", this.body, "</tr>", this.body, "<tr>", this.body, "<td colspan=\"2\" style=\"width: 100%; background-color: #eff3f9; font-size: 12px; font-weight: bold; margin: 0px ; padding: 7px; border-right: solid 1px #d4e0ee; border-bottom: solid 1px #d4e0ee;\">Pick-up Information</td>", this.body, "</tr>", this.body, "<tr>", this.body, "<td style=\"width: 30%; font-size: 12px; font-weight: normal; margin: 0px ; padding: 7px; border-right: solid 1px #d4e0ee; border-bottom: solid 1px #d4e0ee;\">From:</td>", this.body, "<td style=\"width: 70%; font-size: 12px; font-weight: normal; margin: 0px ; padding: 7px; border-bottom: solid 1px #d4e0ee;\">", booking.FromAddress, "</td>", this.body, "</tr>", this.body, "<tr>", this.body, "<td colspan=\"2\" style=\"width: 100%; background-color: #eff3f9; font-size: 12px; font-weight: bold; margin: 0px ; padding: 7px; border-right: solid 1px #d4e0ee; border-bottom: solid 1px #d4e0ee; \">Drop-off Information</td>", this.body, "</tr>", this.body, "<tr>", this.body, "<td style=\"width: 30%; font-size: 12px; font-weight: normal; margin: 0px ; padding: 7px; border-right: solid 1px #d4e0ee; border-bottom: solid 1px #d4e0ee;\">To:</td>", this.body, "<td style=\"width: 70%; font-size: 12px; font-weight: normal; margin: 0px ; padding: 7px; border-bottom: solid 1px #d4e0ee;\">", booking.ToAddress, "</td>", this.body, "</tr>", this.body, "<tr>", this.body, "<td colspan=\"2\" style=\"width: 100%; background-color: #eff3f9; font-size: 12px; font-weight: bold; margin: 0px ; padding: 7px; border-right: solid 1px #d4e0ee; border-bottom: solid 1px #d4e0ee;\">Trip Summary</td>", this.body, "</tr>", this.body, "<tr>", this.body, "</tr>", this.body, "<tr>", this.body, "<td style=\"width: 30%; font-size: 12px; font-weight: normal; margin: 0px ; padding: 7px; border-right: solid 1px #d4e0ee; border-bottom: solid 1px #d4e0ee;\">Payment:</td>", this.body, "<td style=\"width: 70%; font-size: 12px; font-weight: normal; margin: 0px ; padding: 7px; border-bottom: solid 1px #d4e0ee;\">", booking.Gen_PaymentType.PaymentType, "</td>", this.body, "</tr>", this.body, "<tr>", this.body, "<td style=\"width: 30%; font-size: 12px; font-weight: normal; margin: 0px ; padding: 7px; border-right: solid 1px #d4e0ee; border-bottom: solid 1px #d4e0ee;\">Fare:</td>", this.body, "<td style=\"width: 70%; font-size: 12px; font-weight: normal; margin: 0px ; padding: 7px; border-bottom: solid 1px #d4e0ee;\">£", string.Format("{0:f2}", booking.FareRate), "</td>", this.body, "</tr>", this.body, "<tr>", this.body, "<td style=\"width: 30%; font-size: 12px; font-weight: normal; margin: 0px; padding: 7px; border-right: solid 1px #d4e0ee; border-bottom: solid 1px #d4e0ee;\">Parking Charges:</td>", this.body, "<td style=\"width: 70%; font-size: 12px; font-weight: normal; margin: 0px; padding: 7px; border-bottom: solid 1px #d4e0ee;\">£", string.Format("{0:f2}", booking.CongtionCharges), "</td>", this.body, "</tr>", this.body, "<tr>", this.body, "</tr>", this.body, "<tr>", this.body, "<td style=\"width: 30%; font-size: 12px; font-weight: normal; margin: 0px; padding: 7px; border-right: solid 1px #d4e0ee; border-bottom: solid 1px #d4e0ee;\">Waiting Charges:</td>", this.body, "<td style=\"width: 70%; font-size: 12px; font-weight: normal; margin: 0px; padding: 7px; border-bottom: solid 1px #d4e0ee;\">£", string.Format("{0:f2}", booking.MeetAndGreetCharges), "</td>", this.body, "</tr>", this.body, "<tr>", this.body, "</tr>", this.body, "<tr>", this.body, "<td style=\"width: 30%; font-size: 12px; font-weight: normal; margin: 0px; padding: 7px; border-right: solid 1px #d4e0ee; border-bottom: none;\">Distance:</td>", this.body, "<td style=\"width: 70%; font-size: 12px; font-weight: normal; margin: 0px; padding: 7px; border-bottom: none; \">", empty5, "</td>", this.body, "</tr>", this.body, "</table>", this.body, "</td>", this.body, "<td>", this.body, "<table border=\"0\" style=\"border: none; border-collapse: collapse; background-color: #FFF; width: 100%; float: left; text-align: left; font-family: Verdana, arial; \" cellpadding=\"0\" cellspacing=\"0\" align=\"center\">", this.body, "<td colspan=\"2\" style=\"width: 100%; font-size: 12px; font-weight: normal; margin:0px; padding: 7px; border-bottom: solid 1px #d4e0ee;\">",
                                //    this.body, "<img style='width:100%;height=300px;' src=\"https://maps.googleapis.com/maps/api/staticmap?center=", postCodeMatch, "&zoom=11&size=330x400&maptype=roadmap&markers=color:red%7Clabel:A%7C", this.objFromJobCoord.Latitude, ",", this.objFromJobCoord.Longitude, "&markers=color:blue%7Clabel:B%7C", this.objToJobCoord.Latitude, ",", this.objToJobCoord.Longitude, "&key=", str, "\">"

                                //    , this.body, "</td>", this.body, "</table>", this.body, "</td>", this.body, "</tr>", this.body, "</table>", this.body, "<table border=\"1\" style=\"border: solid 1px #d4e0ee; border-top: none; border-collapse: collapse; background-color: #FFF; width: 100%; float: left; text-align: center; font-family: Verdana, arial; \" cellpadding=\"1\" cellspacing=\"0\" align=\"center\">", this.body, "<tr>", this.body, "<td style=\"width: 100%; font-size: 16px; font-weight: bold; margin: 0; padding: 0px; border-top: none; border-bottom: none; \">&nbsp;</td>", this.body, "</tr>", this.body, "<tr style=\"background: #eff3f9; text-align: left; font-family: Verdana, arial;\">",
                                //    this.body, "<td style=\"width: 100%; font-size: 14px; font-weight: normal; margin: 0px 0 10px 0; padding: 8px 7px;\">Total Charges: <span style=\"color: #008000;\">£ ", string.Format("{0:f2}", num1), " Cash</span></td>",
                                //    this.body, "</tr>", this.body, "<tr>", this.body, "<td style=\"width: 100%; f ont-size: 20px;color: #6b97c2; font-weight: bold; margin: 0px 0 10px 0; padding: 8px 7px;\">Thanks for riding, ", booking.CustomerName, "<br/> We hope you enjoyed your ride this ", str5, ".</td>", this.body, "</tr>", this.body, "</table>", this.body, "</body>", this.body, "</html>" };
                                //this.body = string.Concat(objArray);


                                string paymentType = "CASH";
                                object[] objPayment = null;

                                if (booking.PaymentTypeId.ToInt() != 1)
                                {
                                    paymentType = taxiDataContext.Gen_PaymentTypes
                                                    .Where(c => c.Id == booking.PaymentTypeId)
                                                    .Select(c => c.PaymentType).FirstOrDefault();

                                    if (booking.PaymentTypeId > 2)
                                    {
                                        string authCode = taxiDataContext.Booking_Payments.Where(c => c.BookingId == booking.Id).Select(c => c.AuthCode).FirstOrDefault().ToStr().Trim();
                                        if (authCode.ToStr().Trim().Length > 0)
                                            objPayment = new object[] {  this.body, "</tr>", this.body, "<tr>", this.body, "</tr>", this.body, "<tr>", this.body, "<td style=\"width: 30%; font-size: 12px; font-weight: normal; margin: 0px ; padding: 7px; border-right: solid 1px #d4e0ee; border-bottom: solid 1px #d4e0ee;\">Transaction ID:</td>"
                                        , this.body, "<td style=\"width: 70%; font-size: 12px; font-weight: normal; margin: 0px ; padding: 7px; border-bottom: solid 1px #d4e0ee;\">" , authCode, "</td>", this.body, "</tr>" };

                                    }

                                }

                                if (objPayment == null)
                                    objPayment = new object[] { this.body };


                                object[] objArray = new object[] { "<!DOCTYPE html><html xmlns=\"http://www.w3.org/1999/xhtml\"><head><title></title></head><body>", this.body
                                , "<table border=\"0\" style=\" border-collapse: collapse; background-color: #FFF; width: 100%; float: left; text-align: center; font-family: Verdana, arial;\" cellpadding=\"1\" cellspacing=\"0\" align=\"center\">", this.body
                                , "<tr>", this.body, "<td colspan=\"3\" style=\"padding: 1%;color: #6b97c2;\">", this.body
                                , "<div style=\"font-size:28px;font-weight:bold;margin:0px 0 10px 0;padding:0px;\">", str4, "</div>",
                                this.body, "<div style=\"font-size: 17px; font-weight: bold; margin: 0px 0 10px 0; padding: 0px;\">", genSubCompany.TelephoneNo.ToString(), "</div>",
                                this.body, "<p style=\"font-size: 15px; font-weight: bold; margin: 0px 0 10px 0; padding: 0px;\">", genSubCompany.Address.ToString(), "</p>", this.body, "</td>",
                                this.body, "</tr>", this.body, "<tr style=\"background: #eff3f9; text-align: left; font-family: 'Times New Roman', arial;\">",
                                this.body, "<td style=\"width: 35%; font-size: 13px; font-weight: bold; margin: 0px 0 10px 0; padding: 8px 7px;\">Dear: &nbsp; ", booking.CustomerName, "</td>",
                                this.body, "<td style=\"width: 64%; font-size: 13px; font-weight: bold; text-align: right; margin: 0px 0 10px 0; padding: 8px 7px; \">REF NO:&nbsp; ", booking.BookingNo, "</td>", this.body, "</tr>", this.body, "</table>",
                                this.body, "<table border=\"1\" style=\"border: solid 1px #d4e0ee; border-top: none; border-collapse: collapse; background-color: #FFF; width: 100%; float: left; text-align: left; font-family: Verdana, arial;\" cellpadding=\"0\" cellspacing=\"0\" align=\"center\">", this.body, "<tr>",
                                this.body, "<td style=\"vertical-align:top\">", this.body, "<table border=\"0\" style=\"border: none; border-collapse: collapse; background-color: #FFF; width: 100%; float: left; text-align: left; font-family: Verdana, arial; \" cellpadding=\"0\" cellspacing=\"0\" align=\"center\">",
                                this.body, "<tr>", this.body, "<td style=\"width: 30%; font-size: 12px; font-weight: normal; margin: 0px ; padding: 7px; border-right: solid 1px #d4e0ee; border-bottom: solid 1px #d4e0ee;\">Passenger:</td>",
                                this.body, "<td style=\"width: 70%; font-size: 12px; font-weight: normal; margin: 0px ; padding: 7px; border-bottom: solid 1px #d4e0ee;\">", booking.CustomerName.ToString(), "</td>", this.body, "</tr>", this.body, "<tr>",
                                this.body, "<td style=\"width: 30%; font-size: 12px; font-weight: normal; margin: 0px ; padding: 7px; border-right: solid 1px #d4e0ee; border-bottom: solid 1px #d4e0ee;\">Pickup Date/Time:</td>",
                                this.body, "<td style=\"width: 70%; font-size: 12px; font-weight: normal; margin: 0px ; padding: 7px; border-bottom: solid 1px #d4e0ee;\">", booking.PickupDateTime, "</td>", this.body, "</tr>", this.body, "<tr>",
                                this.body, "<td style=\"width: 30%; font-size: 12px; font-weight: normal; margin: 0px ; padding: 7px; border-right: solid 1px #d4e0ee; border-bottom: solid 1px #d4e0ee;\">Vehicle:</td>", this.body, "<td style=\"width: 70%; font-size: 12px; font-weight: normal; margin: 0px ; padding: 7px; border-bottom: solid 1px #d4e0ee;\">", vehicle, "</td>", this.body, "</tr>", this.body, "<tr>", this.body, "<td style=\"width: 30%; font-size: 12px; font-weight: normal; margin: 0px ; padding: 7px; border-right: solid 1px #d4e0ee; border-bottom: solid 1px #d4e0ee;\">Special Ins:</td>", this.body, "<td style=\"width: 70%; font-size: 12px; font-weight: normal; margin: 0px ; padding: 7px; border-bottom: solid 1px #d4e0ee;\">",  "</td>", this.body, "</tr>", this.body, "<tr>", this.body, "<td colspan=\"2\" style=\"width: 100%; background-color: #eff3f9; font-size: 12px; font-weight: bold; margin: 0px ; padding: 7px; border-right: solid 1px #d4e0ee; border-bottom: solid 1px #d4e0ee;\">Pick-up Information</td>", this.body, "</tr>", this.body, "<tr>", this.body, "<td style=\"width: 30%; font-size: 12px; font-weight: normal; margin: 0px ; padding: 7px; border-right: solid 1px #d4e0ee; border-bottom: solid 1px #d4e0ee;\">From:</td>", this.body, "<td style=\"width: 70%; font-size: 12px; font-weight: normal; margin: 0px ; padding: 7px; border-bottom: solid 1px #d4e0ee;\">", booking.FromAddress, "</td>", this.body, "</tr>", this.body, "<tr>", this.body, "<td colspan=\"2\" style=\"width: 100%; background-color: #eff3f9; font-size: 12px; font-weight: bold; margin: 0px ; padding: 7px; border-right: solid 1px #d4e0ee; border-bottom: solid 1px #d4e0ee; \">Drop-off Information</td>", this.body, "</tr>", this.body, "<tr>", this.body, "<td style=\"width: 30%; font-size: 12px; font-weight: normal; margin: 0px ; padding: 7px; border-right: solid 1px #d4e0ee; border-bottom: solid 1px #d4e0ee;\">To:</td>", this.body, "<td style=\"width: 70%; font-size: 12px; font-weight: normal; margin: 0px ; padding: 7px; border-bottom: solid 1px #d4e0ee;\">", booking.ToAddress, "</td>", this.body, "</tr>", this.body, "<tr>", this.body, "<td colspan=\"2\" style=\"width: 100%; background-color: #eff3f9; font-size: 12px; font-weight: bold; margin: 0px ; padding: 7px; border-right: solid 1px #d4e0ee; border-bottom: solid 1px #d4e0ee;\">Trip Summary</td>",
                                this.body, "</tr>", this.body, "<tr>", this.body, "</tr>", this.body, "<tr>", this.body, "<td style=\"width: 30%; font-size: 12px; font-weight: normal; margin: 0px ; padding: 7px; border-right: solid 1px #d4e0ee; border-bottom: solid 1px #d4e0ee;\">Payment:</td>", this.body, "<td style=\"width: 70%; font-size: 12px; font-weight: normal; margin: 0px ; padding: 7px; border-bottom: solid 1px #d4e0ee;\">" , paymentType, "</td>", this.body, "</tr>"

                               ,  string.Concat(objPayment)


                               , this.body, "<tr>", this.body, "<td style=\"width: 30%; font-size: 12px; font-weight: normal; margin: 0px ; padding: 7px; border-right: solid 1px #d4e0ee; border-bottom: solid 1px #d4e0ee;\">Fare:</td>", this.body, "<td style=\"width: 70%; font-size: 12px; font-weight: normal; margin: 0px ; padding: 7px; border-bottom: solid 1px #d4e0ee;\">£", string.Format("{0:f2}", booking.FareRate), "</td>", this.body, "</tr>", this.body, "<tr>", this.body, "<td style=\"width: 30%; font-size: 12px; font-weight: normal; margin: 0px; padding: 7px; border-right: solid 1px #d4e0ee; border-bottom: solid 1px #d4e0ee;\">Parking Charges:</td>", this.body, "<td style=\"width: 70%; font-size: 12px; font-weight: normal; margin: 0px; padding: 7px; border-bottom: solid 1px #d4e0ee;\">£", string.Format("{0:f2}", booking.CongtionCharges), "</td>"
                                , this.body, "</tr>", this.body, "<tr>", this.body, "</tr>", this.body,
                                "<tr>", this.body, "<td style=\"width: 30%; font-size: 12px; font-weight: normal; margin: 0px; padding: 7px; border-right: solid 1px #d4e0ee; border-bottom: solid 1px #d4e0ee;\">Waiting Charges:</td>", this.body, "<td style=\"width: 70%; font-size: 12px; font-weight: normal; margin: 0px; padding: 7px; border-bottom: solid 1px #d4e0ee;\">£", string.Format("{0:f2}", booking.MeetAndGreetCharges), "</td>", this.body, "</tr>"
                                , this.body,
                              "<tr>", this.body, "<td style=\"width: 30%; font-size: 12px; font-weight: normal; margin: 0px; padding: 7px; border-right: solid 1px #d4e0ee; border-bottom: solid 1px #d4e0ee;\">Extra Charges:</td>", this.body, "<td style=\"width: 70%; font-size: 12px; font-weight: normal; margin: 0px; padding: 7px; border-bottom: solid 1px #d4e0ee;\">£", string.Format("{0:f2}", booking.ExtraDropCharges), "</td>", this.body, "</tr>", this.body,


                                 "<tr>", this.body, "<td style=\"width: 30%; font-size: 12px; font-weight: normal; margin: 0px; padding: 7px; border-right: solid 1px #d4e0ee; border-bottom: solid 1px #d4e0ee;\">Booking Fee:</td>", this.body, "<td style=\"width: 70%; font-size: 12px; font-weight: normal; margin: 0px; padding: 7px; border-bottom: solid 1px #d4e0ee;\">£", string.Format("{0:f2}", booking.ServiceCharges), "</td>", this.body, "</tr>"

                                , this.body, "<tr>", this.body, "</tr>", this.body, "<tr>", this.body, "<td style=\"width: 30%; font-size: 12px; font-weight: normal; margin: 0px; padding: 7px; border-right: solid 1px #d4e0ee; border-bottom: none;\">Distance:</td>", this.body, "<td style=\"width: 70%; font-size: 12px; font-weight: normal; margin: 0px; padding: 7px; border-bottom: none; \">", empty5, "</td>", this.body, "</tr>", this.body, "</table>", this.body, "</td>", this.body, "<td>", this.body, "<table border=\"0\" style=\"border: none; border-collapse: collapse; background-color: #FFF; width: 100%; float: left; text-align: left; font-family: Verdana, arial; \" cellpadding=\"0\" cellspacing=\"0\" align=\"center\">", this.body, "<td colspan=\"2\" style=\"width: 100%; font-size: 12px; font-weight: normal; margin:0px; padding: 7px; border-bottom: solid 1px #d4e0ee;\">",

                                   this.body, "<img style='width:100%;height:100%;' src=\"https://maps.googleapis.com/maps/api/staticmap?center=", postCodeMatch, "&zoom=11&size=330x400&maptype=roadmap&markers=color:red%7Clabel:A%7C", this.objFromJobCoord.Latitude, ",", this.objFromJobCoord.Longitude, "&markers=color:blue%7Clabel:B%7C", this.objToJobCoord.Latitude, ",", this.objToJobCoord.Longitude, "&key=", str, "\">"

                                , this.body, "</td>", this.body, "</table>", this.body, "</td>", this.body, "</tr>", this.body, "</table>", this.body, "<table border=\"1\" style=\"border: solid 1px #d4e0ee; border-top: none; border-collapse: collapse; background-color: #FFF; width: 100%; float: left; text-align: center; font-family: Verdana, arial; \" cellpadding=\"1\" cellspacing=\"0\" align=\"center\">", this.body, "<tr>", this.body, "<td style=\"width: 100%; font-size: 16px; font-weight: bold; margin: 0; padding: 0px; border-top: none; border-bottom: none; \">&nbsp;</td>", this.body, "</tr>", this.body, "<tr style=\"background: #eff3f9; text-align: left; font-family: Verdana, arial;\">",
                                this.body, "<td style=\"width: 100%; font-size: 14px; font-weight: normal; margin: 0px 0 10px 0; padding: 8px 7px;\">Total Charges: <span style=\"color: #008000;\">£ ", string.Format("{0:f2}", num1), " </span></td>",
                                this.body, "</tr>", this.body, "<tr>", this.body, "<td style=\"width: 100%; f ont-size: 20px;color: #6b97c2; font-weight: bold; margin: 0px 0 10px 0; padding: 8px 7px;\">Thanks for riding, ", booking.CustomerName, "<br/> We hope you enjoyed your ride this ", str5, ".</td>", this.body, "</tr>", this.body, "</table>", this.body, "</body>", this.body, "</html>" };
                                this.body = string.Concat(objArray);

                                string subject = "BOOKING RECEIPT - " + String.Format("{0:dd MMMM yyyy}", booking.PickupDateTime)
                                    + ", TIME " + string.Format("{0:HH.mm}", booking.PickupDateTime) + " - BOOKING ID " + booking.BookingNo.ToStr();

                                this.SendEmail(genSubCompany.CompanyName, genSubCompany.SmtpHost, genSubCompany.SmtpUserName,
                                   genSubCompany.SmtpPassword, genSubCompany.SmtpPort, genSubCompany.SmtpHasSSL.ToBool(), genSubCompany.EmailCC.ToStr()
                                   , booking.CustomerEmail.ToStr().Trim(), subject, this.body, genSubCompany.EmailAddress);

                                //booking.CustomerEmail.ToStr().Trim(), subject, this.body);
                            }

                        }
                        catch (Exception ex)
                        {

                        }
                        finally
                        {
                            if (taxiDataContext != null)
                            {
                                ((IDisposable)taxiDataContext).Dispose();
                            }
                        }
                    }

                }
            }
            catch
            {

            }
        }



        private string GetPostCodeMatch(string value)
        {
            string str = "";
            if (value.ToStr().Contains(","))
            {
                value = value.Replace(",", "").Trim();
            }
            if (value.ToStr().Contains("\u00a0"))
            {
                value = value.Replace("\u00a0", " ").Trim();
            }
            Match match = (new Regex("^(GIR 0AA)|((([A-PR-UWYZ][0-9][0-9]?)|(([A-PR-UWYZ][A-HK-Y][0-9][0-9]?)|(([A-PR-UWYZ][0-9][A-HJKSTUW])|([A-PR-UWYZ][A-HK-Y][0-9][ABEHMNPRVWXY])))) [0-9][A-BD-HJLNP-UW-Z]{2})$")).Match(value);
            if (match != null)
            {
                str = match.Value;
            }
            if (match.Value == "")
            {
                foreach (Match match1 in (new Regex("[A-Z]{1,2}[0-9R][0-9A-Z]?")).Matches(value))
                {
                    if (!match1.Value.ToStr().IsAlpha())
                    {
                        str = string.Concat(str, match1.Value.ToStr(), " ");
                    }
                }
            }
            return str.Trim();
        }


        private void SendEmail(string CompanyName, string SmtpHost, string SmtpUserName, string SmtpPassword, string SmtpPort
            , bool SmtpHasSSL, string EmailCC, string toEmail, string subject, string body, string EmailAddress)
        {
            try
            {



                MailMessage mailMessage = new MailMessage();
                try
                {

                    mailMessage.To.Add(toEmail);
                    mailMessage.From = new MailAddress(EmailAddress, CompanyName.ToStr().Trim());
                    if (EmailCC != string.Empty)
                    {
                        mailMessage.CC.Add(EmailCC);
                    }
                    mailMessage.Subject = subject;
                    mailMessage.Body = body;
                    mailMessage.BodyEncoding = Encoding.UTF8;
                    mailMessage.IsBodyHtml = true;
                    SmtpClient smtpClient = new SmtpClient()
                    {
                        Credentials = new NetworkCredential(SmtpUserName, SmtpPassword.ToStr().Trim()),
                        Port = Convert.ToInt32(SmtpPort.ToStr().Trim()),
                        Host = SmtpHost.ToStr().Trim(),
                        EnableSsl = SmtpHasSSL.ToBool(),
                        DeliveryMethod = SmtpDeliveryMethod.Network
                    };
                    FieldInfo field = smtpClient.GetType().GetField("transport", BindingFlags.Instance | BindingFlags.NonPublic);
                    FieldInfo fieldInfo = field.GetValue(smtpClient).GetType().GetField("authenticationModules", BindingFlags.Instance | BindingFlags.NonPublic);
                    Array value = fieldInfo.GetValue(field.GetValue(smtpClient)) as Array;
                    value.SetValue(value.GetValue(3), 1);
                    ServicePointManager.ServerCertificateValidationCallback = (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;
                    smtpClient.Send(mailMessage);
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    if (mailMessage != null)
                    {
                        ((IDisposable)mailMessage).Dispose();
                    }
                }
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                //try
                //{
                //    File.AppendAllText(string.Concat(Application, "\\log_exceptionemail.txt"), string.Concat(DateTime.Now.ToStr(), " : ", exception.Message, Environment.NewLine));
                //}
                //catch
                //{
                //}
            }
        }


    }
}
