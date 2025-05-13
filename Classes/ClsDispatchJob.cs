using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Taxi_Model;
using Utils;
using Taxi_BLL;
using System.Threading;

namespace SignalRHub
{
    public class ClsDispatchJob
    {

        public static void OnDespatching(Gen_SysPolicy_Configuration objPolicy, Booking objBooking, Fleet_Driver ObjDriver)
        {




            try
            {

                if (ObjDriver != null && objBooking != null)
                {



                    string customerMobileNo = objBooking.CustomerMobileNo.Trim();
                    // For testing Purpose
                    //  customerMobileNo = "03323755646"; 
                    //
                    string customerName = objBooking.CustomerName;

                    string via = string.Join(",", objBooking.Booking_ViaLocations.Select(c => c.ViaLocValue.ToStr()).ToArray<string>());

                    if (!string.IsNullOrEmpty(via.Trim()))
                        via = "Via: " + via;

                    //    string specialReq = objBooking.SpecialRequirements.ToStr().Trim();
                    //if (!string.IsNullOrEmpty(specialReq))
                    //    specialReq = "Special Req: " + specialReq;



                    bool enablePDA = objPolicy.EnablePDA.ToBool();

                    string custNo = !string.IsNullOrEmpty(objBooking.CustomerMobileNo) ? objBooking.CustomerMobileNo : objBooking.CustomerPhoneNo;



                    // Send To Driver









                    string paymentType = objBooking.Gen_PaymentType.PaymentCategoryId == null ? objBooking.Gen_PaymentType.DefaultIfEmpty().PaymentType.ToStr()
                            : objBooking.Gen_PaymentType.Gen_PaymentCategory.CategoryName.ToStr();

                    string strDeviceRegistrationId = ObjDriver.DeviceId.ToStr();
                    string journey = "O/W";

                    //if (objBooking.JourneyTypeId.ToInt() == 2)
                    //{
                    //    journey = "Return";

                    //}
                    if (objBooking.JourneyTypeId.ToInt() == 3)
                    {
                        journey = "W/R";
                    }


                    string IsExtra = (objBooking.CompanyId != null || objBooking.FromLocTypeId == Enums.LOCATION_TYPES.AIRPORT || objBooking.ToLocTypeId == Enums.LOCATION_TYPES.AIRPORT) ? "1" : "0";
                    int i = 1;
                    string viaP = "";



                    if (objBooking.Booking_ViaLocations.Count > 0)
                    {



                        viaP = string.Join(" * ", objBooking.Booking_ViaLocations.Select(c => "(" + i++.ToStr() + ")" + c.ViaLocValue.ToStr()).ToArray<string>());
                    }


                    string mobileNo = objBooking.CustomerMobileNo.ToStr();
                    string telNo = objBooking.CustomerPhoneNo.ToStr();

                    // decimal drvPdaVersion = ObjDriver.Fleet_Driver_PDASettings.Count > 0 ? ObjDriver.Fleet_Driver_PDASettings[0].CurrentPdaVersion.ToDecimal() : 9.40m;


                    if (string.IsNullOrEmpty(mobileNo) && !string.IsNullOrEmpty(telNo))
                    {
                        mobileNo = telNo;
                    }
                    else if (!string.IsNullOrEmpty(mobileNo) && !string.IsNullOrEmpty(telNo))
                    {
                        mobileNo += "/" + telNo;
                    }


                    string pickUpPlot = "";
                    string dropOffPlot = "";
                    string companyName = string.Empty;

                    //if (drvPdaVersion < 11 && objBooking.CompanyId != null && objBooking.Gen_Company.DefaultIfEmpty().AccountTypeId.ToInt() != Enums.ACCOUNT_TYPE.CASH)
                    //    companyName = objBooking.Gen_Company.DefaultIfEmpty().CompanyName;
                    //    else
                    companyName = objBooking.Gen_Company.DefaultIfEmpty().CompanyName.ToStr();


                    //error in 13.4 => if its a plot job, then pickup point is hiding in pda.
                    //if (drvPdaVersion >9 && drvPdaVersion!=13.4m)
                    //{
                    pickUpPlot = objBooking.ZoneId != null ? "<<<" + objBooking.Gen_Zone1.DefaultIfEmpty().ZoneName.ToStr() : "";
                    dropOffPlot = objBooking.DropOffZoneId != null ? "<<<" + objBooking.Gen_Zone.DefaultIfEmpty().ZoneName.ToStr() : "";
                    //  }


                    string FOJJob = string.Empty;








                    string startJobPrefix = "JobId:";
                    //if (objPolicy.PDAJobAlertOnly.ToBool() &&  ObjDriver.Fleet_Driver_PDASettings[0].CurrentPdaVersion.ToDecimal() >= 8.3m && ObjDriver.Fleet_Driver_PDASettings[0].ShowJobAsAlert.ToBool())
                    //{
                    //    startJobPrefix = "AlertJobId:";                                   

                    //}

                    string fromAddress = objBooking.FromAddress.ToStr().Trim();
                    string toAddress = objBooking.ToAddress.ToStr().Trim();

                    if (objBooking.FromLocTypeId.ToInt() == Enums.LOCATION_TYPES.POSTCODE || objBooking.FromLocTypeId.ToInt() == Enums.LOCATION_TYPES.AIRPORT)
                    {
                        fromAddress = objBooking.FromStreet.ToStr() + " " + objBooking.FromAddress.ToStr();

                    }






                    if (objBooking.ToLocTypeId.ToInt() == Enums.LOCATION_TYPES.POSTCODE || objBooking.ToLocTypeId.ToInt() == Enums.LOCATION_TYPES.AIRPORT)
                    {
                        toAddress = objBooking.ToStreet.ToStr() + " " + objBooking.ToAddress.ToStr();
                    }




                    //half card and cash
                    string specialRequirements = objBooking.SpecialRequirements.ToStr();
                    if (objBooking.SecondaryPaymentTypeId != null && objBooking.CashFares.ToDecimal() > 0)
                    {

                        specialRequirements += " , Additional Cash Payment : " + objBooking.CashFares.ToDecimal();
                    }

                    decimal pdafares = objBooking.GetType().GetProperty(objPolicy.PDAFaresPropertyName.ToStr().Trim()).GetValue(objBooking, null).ToDecimal();

                    //  pdafares = objBooking.TotalCharges.ToDecimal();

                    string msg = string.Empty;


                    string showFaresValue = objBooking.Gen_PaymentType.ShowFaresOnPDA.ToStr().Trim();




                    string showFares = ",\"ShowFares\":\"" + showFaresValue + "\"";
                    string showSummary = ",\"ShowSummary\":\"" + showFaresValue + "\"";
                    //   string showSummary = string.Empty;




                    string agentDetails = string.Empty;
                    string parkingandWaiting = string.Empty;
                    if (objBooking.CompanyId != null)
                    {


                        if (objPolicy.PickCommissionDeductionFromJobsTotal.ToBool())
                        {
                            agentDetails = ",\"AgentFees\":\"" + String.Format("{0:0.00}", objBooking.AgentCommission + objBooking.ServiceCharges.ToDecimal()) + "\"";
                        }
                        else
                        {
                            agentDetails = ",\"AgentFees\":\"" + String.Format("{0:0.00}", objBooking.AgentCommission) + "\"";

                        }

                        parkingandWaiting = ",\"Parking\":\"" + string.Format("{0:0.00}", objBooking.ParkingCharges) + "\",\"Waiting\":\"" + String.Format("{0:0.00}", objBooking.WaitingCharges) + "\"";

                    }
                    else
                    {

                        if (objPolicy.PickCommissionDeductionFromJobsTotal.ToBool())
                        {
                            agentDetails = ",\"AgentFees\":\"" + String.Format("{0:0.00}", objBooking.ServiceCharges.ToDecimal()) + "\"";

                        }


                        parkingandWaiting = ",\"Parking\":\"" + string.Format("{0:0.00}", objBooking.CongtionCharges) + "\",\"Waiting\":\"" + String.Format("{0:0.00}", objBooking.MeetAndGreetCharges) + "\"";
                        //


                    }





                    string fromdoorno = objBooking.FromDoorNo.ToStr().Trim();
                    if (fromdoorno.Length > 0 && fromdoorno.WordCount() > 2 && fromdoorno.Contains(" "))
                    {

                        try
                        {

                            fromdoorno = fromdoorno.Replace(" ", "-");
                        }
                        catch
                        {


                        }
                    }


                    //if (drvPdaVersion == 23.50m && fromAddress.ToStr().Trim().Contains("-"))
                    //{
                    //    fromAddress = fromAddress.Replace("-", "  ");

                    //}


                    if (specialRequirements.ToStr().Contains("\""))
                        specialRequirements = specialRequirements.ToStr().Replace("\"", "-").Trim();


                    string summary = string.Empty;

                    List<ChargesSummary> listofSummary = new List<ChargesSummary>();

                    listofSummary.Add(new ChargesSummary { label = "Fares", value = string.Format("{0:0.00}", objBooking.FareRate.ToDecimal()) });

                    listofSummary.Add(new ChargesSummary { label = "Parking", value = string.Format("{0:0.00}", objBooking.CongtionCharges.ToDecimal()) });
                    listofSummary.Add(new ChargesSummary { label = "Waiting", value = string.Format("{0:0.00}", objBooking.MeetAndGreetCharges.ToDecimal()) });
                    listofSummary.Add(new ChargesSummary { label = "Extras", value = string.Format("{0:0.00}", objBooking.ExtraDropCharges.ToDecimal()) });
                    listofSummary.Add(new ChargesSummary { label = "Fee", value = string.Format("{0:0.00}", objBooking.AgentCommission.ToDecimal() + objBooking.CashRate.ToDecimal() + objBooking.ServiceCharges.ToDecimal()) });

                    summary = ",\"Summary\":" + Newtonsoft.Json.JsonConvert.SerializeObject(listofSummary);

                    msg = FOJJob + startJobPrefix + "{ \"JobId\" :\"" + objBooking.Id.ToStr() +
                                       "\", \"Pickup\":\"" + (!string.IsNullOrEmpty(objBooking.FromDoorNo) ? fromdoorno + "-" + fromAddress + pickUpPlot : fromAddress + pickUpPlot) +
                                       "\", \"Destination\":\"" + (!string.IsNullOrEmpty(objBooking.ToDoorNo) ? objBooking.ToDoorNo + "-" + toAddress + dropOffPlot : toAddress + dropOffPlot) + "\"," +
                                       "\"PickupDateTime\":\"" + string.Format("{0:dd/MM/yyyy   HH:mm}", objBooking.PickupDateTime) + "\"" +
                                       ",\"Cust\":\"" + objBooking.CustomerName + "\",\"Mob\":\"" + mobileNo + " " + "\",\"Fare\":\"" + string.Format("{0:0.00}", pdafares) + "\",\"Vehicle\":\"" + objBooking.Fleet_VehicleType.VehicleType + "\",\"Account\":\"" + companyName + " " + "\"" +
                                         ",\"Lug\":\"" + objBooking.NoofLuggages.ToInt() + "\",\"Passengers\":\"" + objBooking.NoofPassengers.ToInt() + "\",\"Journey\":\"" + journey + "\",\"Payment\":\"" + paymentType + "\",\"Special\":\"" + specialRequirements + " " + "\",\"Extra\":\"" + IsExtra + "\",\"Via\":\"" + viaP + " " + "\"" +

                                         ",\"CompanyId\":\"" + objBooking.CompanyId.ToInt() + "\",\"SubCompanyId\":\"" + objBooking.SubcompanyId.ToInt() + "\",\"QuotedPrice\":\"" + (objBooking.IsQuotedPrice.ToBool() ? "1" : "0") + "\"" +


                                         parkingandWaiting + ",\"DriverFares\":\"" + String.Format("{0:0.00}", objBooking.FareRate) + "\"" +
                                      agentDetails +
                                         ",\"Did\":\"" + ObjDriver.Id + "\",\"BabySeats\":\"" + objBooking.BabySeats.ToStr() + "\"" + showFares + showSummary + summary + " }";





                    if (msg.Contains("\r\n"))
                    {
                        msg = msg.Replace("\r\n", " ").Trim();
                    }
                    else
                    {
                        if (msg.Contains("\n"))
                        {
                            msg = msg.Replace("\n", " ").Trim();

                        }

                    }

                    if (msg.Contains("&"))
                    {
                        msg = msg.Replace("&", "And");
                    }

                    if (msg.Contains(">"))
                        msg = msg.Replace(">", " ");


                    if (msg.Contains("="))
                        msg = msg.Replace("=", " ");




                    (new TaxiDataContext()).stp_DespatchedJobWithLogReason(objBooking.Id, ObjDriver.Id, ObjDriver.DriverNo.ToStr(), ObjDriver.HasPDA.ToBool(), true, false, true, "Admin", Enums.BOOKINGSTATUS.PENDING, false, "");



                    //needtouncomment
                    // General.SendMessageToPDA("request pda=" + objBooking.Id + "=" + ObjDriver.Id + "=" + msg + "=1=" + ObjDriver.DriverNo).Result.ToBool();



                }
            }
            catch (Exception ex)
            {

            }




        }










        public bool SuccessDespatched = false;

        private bool _IsFOJ;

        public bool IsFOJ
        {
            get { return _IsFOJ; }
            set { _IsFOJ = value; }
        }

        public bool IsPDADriver = false;
        public bool IsDespatched = false;

        public static void DespatchJob(Gen_SysPolicy_Configuration objPolicy, Booking objBooking, Fleet_Driver ObjDriver)
        {

            List<string> listofErrors = new List<string>();

              OnDespatching(objPolicy,objBooking, ObjDriver);

           

                try
                {

                   

                   
                     
                   
                   


                  

                   
                }
                catch (Exception ex)
                {

                   

                }
           
          

        }


    }
}