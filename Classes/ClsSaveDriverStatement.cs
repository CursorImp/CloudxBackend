using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Taxi_BLL;
using Taxi_Model;
using Utils;

namespace SignalRHub.Classes
{
    public class ClsSaveDriverStatement
    {
        public string SaveDriverRent(AdminApi obj)
        {
            DriverRentBO objMaster = new DriverRentBO();          
            int? DriverId = obj.DriverId;
            string objTransNo = obj.TransNo;
            int? ObjDriverTypeId = obj.DriverTypeId;
            using (TaxiDataContext db = new TaxiDataContext())
            {

                //if (DriverId == 0)
                if (obj.driverRent.Id == 0)
                {
                    //objdriverRent = General.GetQueryable<DriverRent>(null).OrderByDescending(c => c.Id).FirstOrDefault();
                    objMaster.New();
                }
                else
                {
                    var query = General.GetQueryable<DriverRent>(c => c.DriverId == DriverId).OrderByDescending(c => c.Id).FirstOrDefault();
                    if (query != null)
                    {
                        string Transno = query.TransNo.ToStr();

                        if (Transno == objTransNo)
                        {
                            objMaster.GetByPrimaryKey(query.Id);
                            objMaster.Edit();
                        }
                        else
                        {
                            return "Can not Save Record. Another Record Exits..";
                        }
                    }
                    else
                    {
                        objMaster.GetByPrimaryKey(query.Id);
                        objMaster.Edit();
                    }

                }

                objMaster.Current.TransDate = obj.driverRent.TransDate.ToDateTime();
                objMaster.Current.DriverId = obj.driverRent.DriverId.ToIntorNull();
                objMaster.Current.DriverRent1 = obj.driverRent.DriverRent1.ToDecimal();
                objMaster.Current.Balance = obj.driverRent.Balance.ToDecimal();
                objMaster.Current.OldBalance = obj.driverRent.OldBalance.ToDecimal();
                objMaster.Current.PDARent = obj.driverRent.PDARent.ToDecimal();
                //objMaster.Current.VAT = obj.driverRent.VAT.ToDecimal();   // property missing or not found
                objMaster.Current.FromDate = obj.driverRent.FromDate.ToDate();
                objMaster.Current.ToDate = obj.driverRent.ToDate.ToDate();
                objMaster.Current.TransFor = obj.driverRent.TransFor.ToStr();
                // Driver Vehicle Rent
                objMaster.Current.CarRent = obj.driverRent.CarRent.ToDecimal();
                objMaster.Current.CarInsuranceRent = obj.driverRent.CarInsuranceRent.ToDecimal();
                objMaster.Current.PrimeCompanyRent = obj.driverRent.PrimeCompanyRent.ToDecimal();
                objMaster.Current.IsHoliday = obj.driverRent.IsHoliday.ToBool();
                objMaster.Current.AccountJobsTotal = obj.driverRent.AccountJobsTotal.ToDecimal();
                objMaster.Current.CashJobsTotal = obj.driverRent.CashJobsTotal.ToDecimal();
                objMaster.Current.JobsTotal = obj.driverRent.AccountJobsTotal.ToDecimal() + obj.driverRent.CashJobsTotal.ToDecimal();
                objMaster.Current.TotalJobs = obj.driverRent.TotalJobs.ToIntorNull();  //grdLister.Rows.Count;
                objMaster.Current.AccountExpenses = obj.driverRent.AccountExpenses.ToDecimal();
                objMaster.Current.AgentTotal = obj.driverRent.AgentTotal.ToDecimal();
                objMaster.Current.OldAgentCommission = obj.driverRent.OldAgentCommission.ToDecimal();
                objMaster.Current.Extra = obj.driverRent.Extra.ToDecimal();
                objMaster.Current.Fuel = obj.driverRent.Fuel.ToDecimal();



                string[] skipProperties = { "DriverRent", "Booking" };
                IList<DriverRent_Charge> savedList = objMaster.Current.DriverRent_Charges;
                List<DriverRent_Charge> listofDetail = (from r in obj.driverRent.DriverRent_Charges

                                                        select new DriverRent_Charge
                                                        {
                                                            Id = r.Id,
                                                            TransId = r.TransId.ToLong(),
                                                            BookingId = r.BookingId.ToLongorNull(),

                                                        }).ToList();


                Utils.General.SyncChildCollection(ref savedList, ref listofDetail, "Id", skipProperties);


                string[] skipExpensesProperties = { "DriverRent" };
                IList<Fleet_DriverRentExpense> savedlistExpenses = objMaster.Current.Fleet_DriverRentExpenses;
                List<Fleet_DriverRentExpense> listofDetailExpenses = (from a in obj.driverRent.Fleet_DriverRentExpenses
                                                                      select new Fleet_DriverRentExpense
                                                                      {
                                                                          Id = a.Id.ToLong(),
                                                                          RentId = a.RentId.ToLongorNull(),
                                                                          Credit = a.Credit.ToDecimal(),
                                                                          Debit = a.Debit.ToDecimal(),
                                                                          Date = a.Date.ToDateorNull(),
                                                                          Amount = a.Amount.ToDecimal(),
                                                                          Description = a.Description.ToStr(),
                                                                          AddBy = a.AddBy.ToStr() //a.User.ToStr()

                                                                      }).ToList();
                Utils.General.SyncChildCollection(ref savedlistExpenses, ref listofDetailExpenses, "Id", skipExpensesProperties);

                objMaster.CheckDataValidation = true;

                objMaster.Current.TransactionType = Enums.TRANSACTIONTYPE.DRIVER_RENT_EXPENSE3;


                objMaster.Save();

                return "success";
            }
            
        }

        public string SaveDriverCommission(AdminApi obj)
        {
            DriverCommisionBO objMasterCommission = new DriverCommisionBO();
            int? DriverId = obj.DriverId;
            string objTransNo = obj.TransNo;
            int? ObjDriverTypeId = obj.DriverTypeId;

            {               
                //---------------------------
                if (obj.driverCommission.Id == 0)
                //if (obj.Id == null)
                {

                    //objdriverRent = General.GetQueryable<DriverRent>(null).OrderByDescending(c => c.Id).FirstOrDefault();
                    objMasterCommission.New();
                }
                else
                {
                    objMasterCommission.GetByPrimaryKey(obj.driverCommission.Id);
                    DateTime? LastEditDate = objMasterCommission.Current.EditOn;

                    var query = General.GetQueryable<Fleet_DriverCommision>(c => c.DriverId == DriverId).OrderByDescending(c => c.Id).FirstOrDefault();
                    if (query != null)
                    {
                        string Transno = query.TransNo.ToStr();

                        if (Transno == obj.TransNo)
                        {

                            if (General.GetQueryable<DriverCommission_PaymentHistory>(c => c.CommissionId == objMasterCommission.Current.Id).Count() > 0)
                            {
                                return "Can not Save Record. Payment already Exist Against this Transaction..";                                
                            }

                            objMasterCommission.Edit();
                        }
                        else
                        {
                            return "Can not Save Record. Another Record Exist..";                            
                        }
                    }
                    else
                    {

                        if (General.GetQueryable<DriverCommission_PaymentHistory>(c => c.CommissionId == objMasterCommission.Current.Id).Count() > 0)
                        {
                            return "Can not Save Record. Payment already Exist Against this Transaction..";
                            

                        }


                        if (General.GetQueryable<DriverCommission_PaymentHistory>(c => c.CommissionId == objMasterCommission.Current.Id).Count() > 0)
                        {                            
                            return "Can not Save Record. Payment already Exist Against this Transaction..";

                        }

                        objMasterCommission.Edit();


                    }
                    DateTime? NewEditDate = objMasterCommission.Current.EditOn;

                    if (NewEditDate != null)
                    {
                        if (LastEditDate == null && NewEditDate != null)
                        {

                            //response.Message = "This record is already updated from other side" + Environment.NewLine + "you need to close and open this record again";
                            //return Json(response, JsonRequestBehavior.AllowGet);
                            return "This record is already updated from other side" + Environment.NewLine + "you need to close and open this record again";
                        }
                        else
                        {

                            if (LastEditDate < NewEditDate)
                            {                                
                                return "This record is already updated from other side" + Environment.NewLine + "you need to close and open this record again";

                            }
                        }

                    }
                }

                //   decimal Fuel = txtFuel.Text == "" ? 0 : txtFuel.Text.ToDecimal();


                decimal Extra = obj.driverCommission.Extra == null ? 0 : obj.driverCommission.Extra.ToDecimal();
                DateTime Datetime = obj.driverCommission.TransDate.ToDateTime(); //dtpTransactionDate.Value.ToDateTime();
                objMasterCommission.Current.TransDate = obj.driverCommission.TransDate.ToDateTime(); //dtpTransactionDate.Value.ToDateTime();
                objMasterCommission.Current.DriverId = obj.driverCommission.DriverId.ToIntorNull(); //ddlDriver.SelectedValue.ToIntorNull();
                                                                                                    //  objMaster.Current.DriverCommision = txtDriverOwed.Text == "" ? 0 : txtDriverOwed.Text.ToDecimal();

                objMasterCommission.Current.DriverCommision = obj.driverCommission.DriverCommision; //numCommissionPercent.Value;
                objMasterCommission.Current.Balance = obj.driverCommission.Balance.ToDecimal();
                objMasterCommission.Current.OldBalance = obj.driverCommission.OldBalance.ToDecimal();
                objMasterCommission.Current.FromDate = obj.driverCommission.FromDate.Value.ToDate();
                objMasterCommission.Current.ToDate = obj.driverCommission.ToDate.Value.ToDate();  //TillDate.Value.ToDate();
                objMasterCommission.Current.TransFor = obj.driverCommission.TransFor.ToStr();      //DayWise.SelectedText.ToStr();
                                                                                                   //objMasterCommission.Current.VAT = obj.driverCommission.VAT;  //numVAT.Value.ToDecimal();   //?? Missing Property 

                //   objMaster.Current.CollectionDeliveryCharges

                objMasterCommission.Current.AccountExpenses = obj.driverCommission.AccountExpenses.ToDecimal(); // spnAccountExpenses.Value.ToDecimal();

                //         objMaster.Current.JobsTotal = grdLister.Rows.Sum(c => c.Cells[COLS.Fares].Value.ToDecimal()
                //+ c.Cells[COLS.Waiting].Value.ToDecimal()
                //+ c.Cells[COLS.ExtraDrop].Value.ToDecimal());

                objMasterCommission.Current.JobsTotal = obj.driverCommission.JobsTotal.ToDecimal();
                objMasterCommission.Current.Remarks = obj.driverCommission.Remarks;   //numMinCommLimit.Value.ToStr();
                objMasterCommission.Current.IsCreditOrDebit = obj.driverCommission.IsCreditOrDebit;   //optCredit.ToggleState == Telerik.WinControls.Enumerations.ToggleState.On ? true : false;
                objMasterCommission.Current.PDARent = obj.driverCommission.PDARent.ToDecimal();   //numpdaRent.Value.ToDecimal();
                objMasterCommission.Current.DriverOwed = obj.driverCommission.DriverOwed.ToDecimal();   //txtDriverOwed.Text == "" ? 0 : txtDriverOwed.Text.ToDecimal();
                objMasterCommission.Current.AgentFeesTotal = obj.driverCommission.AgentFeesTotal;     //numAgentFeeTotal.Value;
                objMasterCommission.Current.OldAgentCommission = obj.driverCommission.OldAgentCommission; //NumAccountBookingFeeTotal.Value;
                objMasterCommission.Current.Extra = obj.driverCommission.Extra;    //numPDARentPerWeek.Value;
                                                                                   //   objMaster.Current.CommissionTotal = ((objMaster.Current.JobsTotal - objMaster.Current.AgentFeesTotal) * numCommissionPercent.Value) / 100;
                objMasterCommission.Current.CommissionTotal = obj.driverCommission.CommissionTotal; //txtCommsionTotal.Text.ToDecimal();
                //grdLister.Rows.Sum(c => c.Cells[COLS.Commission].Value.ToDecimal()).ToDecimal();
                //objMaster.Current.AccJobsTotal = grdLister.Rows.Where(c => c.Cells[COLS.CompanyId].Value != null).Sum(c => c.Cells[COLS.Fares].Value.ToDecimal()).ToDecimal();
                objMasterCommission.Current.AccJobsTotal = obj.driverCommission.AccJobsTotal;   //txtTotalAccountBooking.Text.Replace("£","").ToDecimal();
                //  objMaster.Current.CollectionDeliveryCharges = numTotalCollectionDelivery.Value.ToDecimal();
                objMasterCommission.Current.CollectionDeliveryCharges = obj.driverCommission.CollectionDeliveryCharges; //  numPromotion.Value;
                objMasterCommission.Current.AccountBookingDays = obj.driverCommission.AccountBookingDays; // ddlAccountBookingDays.Text.ToInt();
                objMasterCommission.Current.TotalWeeks = obj.driverCommission.TotalWeeks; //TotalWeeks;
                objMasterCommission.Current.Fuel = obj.driverCommission.Fuel.ToDecimal();  // numParkingTotal.Value.ToDecimal();
                objMasterCommission.Current.Adjustments = obj.driverCommission.Adjustments;  //numExtraTotal.Value.ToDecimal();
                // objMaster.Current.Extra = Extra.ToDecimal();
                //MaxCommission
                objMasterCommission.Current.MaxCommission = obj.driverCommission.MaxCommission;  //numMaxCommission.Value.ToDecimal();
                                                                                                 //
                objMasterCommission.Current.WeekOff = obj.driverCommission.WeekOff;  //chkHoliday.Checked;
                string[] skipProperties = { "Fleet_DriverCommision", "Booking" };
                IList<Fleet_DriverCommision_Charge> savedList = objMasterCommission.Current.Fleet_DriverCommision_Charges;
                List<Fleet_DriverCommision_Charge> listofDetail = (from r in obj.driverCommission.Fleet_DriverCommision_Charges            //grdLister.Rows
                                                                   select new Fleet_DriverCommision_Charge
                                                                   {
                                                                       Id = r.Id.ToLong(),
                                                                       TransId = r.TransId.ToLong(),
                                                                       BookingId = r.BookingId.ToLongorNull(),
                                                                       CommissionPerBooking = r.CommissionPerBooking.ToDecimal(), //  Commission.ToDecimal(),
                                                                                                                                  //BookingTransId = r.BookingTransId.ToLongorNull()  //BookingTransId    Property Missing
                                                                   }).ToList();


                Utils.General.SyncChildCollection(ref savedList, ref listofDetail, "Id", skipProperties);
                string[] skipExpProperties = { "Fleet_DriverCommision" };

                IList<Fleet_DriverCommissionExpense> savedlistExp = objMasterCommission.Current.Fleet_DriverCommissionExpenses;
                List<Fleet_DriverCommissionExpense> listExpDetail = (from a in obj.driverCommission.Fleet_DriverCommissionExpenses //grdDriverExpenses.Rows
                                                                     select new Fleet_DriverCommissionExpense
                                                                     {
                                                                         Id = a.Id.ToLong(),
                                                                         CommissionId = a.CommissionId.ToLong(), // Misssing Property
                                                                         Debit = a.Debit.ToDecimal(),
                                                                         Credit = a.Credit.ToDecimal(),
                                                                         Amount = a.Amount.ToDecimal(),
                                                                         Description = a.Description.ToStr(),
                                                                         Date = a.Date.ToDateTimeorNull(),
                                                                         AddBy = a.AddBy.ToStr()   //User.Value.ToStr()
                                                                     }).ToList();
                Utils.General.SyncChildCollection(ref savedlistExp, ref listExpDetail, "Id", skipExpProperties);


                objMasterCommission.Current.TransactionType = Enums.TRANSACTIONTYPE.DRIVER_COMMISSION_EXPENSE5;


                objMasterCommission.Save();

            }
            return objMasterCommission.Current.Id.ToStr();
        }
    }
}