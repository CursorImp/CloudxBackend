
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Newtonsoft.Json;
using SignalRHub.Classes;
using SignalRHub.WebApiClasses;
using SMSGateway;
using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Services.Description;
using Taxi_BLL;
using Taxi_Model;
using static SignalRHub.WebApiClasses.ClsBookingListData;

namespace SignalRHub
{
    public class GoogleCalendarEvent
    {

        private static string[] Scopes = { CalendarService.Scope.Calendar, CalendarService.Scope.CalendarEvents };
        private static string ApplicationName = "Google Calendar API .NET";

        public async Task CreateEvent(int? jobID)
        {
            try
            {
                string calendarId = ConfigurationManager.AppSettings["GoogleCalendarEmail"];
                CalendarService service;

                string jsonKeyFilePath = AppContext.BaseDirectory + "GoogleCalendarCredentials.json";

                var credential = GoogleCredential.FromFile(jsonKeyFilePath)
                    .CreateScoped(Scopes);

                // Create the service.
                service = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Google Calendar API Service Account",
                });


                WebApiClasses.RequestWebApi obj = new RequestWebApi();
                obj.JobId = jobID;
                var objBook = new stp_GetBookingDetailsResult();
                using (TaxiDataContext db = new TaxiDataContext())
                {
                    objBook = db.stp_GetBookingDetails((long)jobID).FirstOrDefault();
                }
                

                DateTime pdate = (DateTime)objBook.PickupDateTime;                
                // Create a new event
                Google.Apis.Calendar.v3.Data.Event newEvent = new Google.Apis.Calendar.v3.Data.Event()
                {
                    Summary = objBook.CustomerName,
                    Location = objBook.FromAddress,
                    Description = "Ref# " + objBook.BookingNo,
                    Start = new EventDateTime()
                    {
                        DateTimeRaw = pdate.ToString(),
                        DateTime = pdate.ToUniversalTime(),
                        TimeZone = "UTC"
                    },
                    End = new EventDateTime()
                    {
                        DateTimeRaw = pdate.AddMinutes(15).ToUniversalTime().ToString(),
                        DateTime = pdate.AddMinutes(15),
                        TimeZone = "UTC"
                    },
                };
                var eventid = "";
                using (TaxiDataContext db = new TaxiDataContext())
                {
                    eventid = db.ExecuteQuery<string>("exec [stp_GetBooking_EventsByID] {0}", (long)jobID).FirstOrDefault();
                }
                if (eventid != null)
                {

                    // Update the event
                    var updateRequest = service.Events.Update(newEvent, calendarId, eventid);
                    await updateRequest.ExecuteAsync();

                }
                else
                {
                    // Insert the event
                    EventsResource.InsertRequest irequest = service.Events.Insert(newEvent, calendarId);
                    Google.Apis.Calendar.v3.Data.Event createdEvent = await irequest.ExecuteAsync();

                    Booking_Events objevent = new Booking_Events();
                    objevent.JobId = (long)jobID;
                    objevent.EventId = createdEvent.Id;
                    SaveBookingEvents(objevent);
                }
            }

            catch (Exception ex)
            {
                //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "CreateEvent_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "exception:" + ex.Message + Environment.NewLine);
                General.WriteLog("CreateEvent_exception", " Exception: " + ex.Message);
            }
        }

        public void SaveBookingEvents(WebApiClasses.Booking_Events obj)
        {
            ResponseWebApi response = new ResponseWebApi();
            try
            {
                Booking_Events objevent = new Booking_Events();
                objevent.JobId = obj.JobId;
                objevent.EventId = obj.EventId;

                using (TaxiDataContext db1 = new TaxiDataContext())
                {

                    db1.ExecuteQuery<int?>("exec stp_SaveBooking_Events {0},{1}"
                                                   , objevent.JobId, objevent.EventId);
                }
            }
            catch (Exception ex)
            {
                response.HasError = true;
                response.Message = ex.Message;
            }
        }

        public async Task DeleteBookingEvents(long Jobid)
        {
            try
            {
                string calendarId = ConfigurationManager.AppSettings["GoogleCalendarEmail"];
                CalendarService service;

                string jsonKeyFilePath = AppContext.BaseDirectory + "credentials.json";

                var credential = GoogleCredential.FromFile(jsonKeyFilePath)
                    .CreateScoped(Scopes);

                // Create the service.
                service = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Google Calendar API Service Account",
                });
                string eventid = "";
                using (TaxiDataContext db = new TaxiDataContext())
                {
                    eventid = db.ExecuteQuery<string>("exec [stp_GetBooking_EventsByID] {0}", Jobid).FirstOrDefault();
                }

                EventsResource.DeleteRequest request = service.Events.Delete(calendarId, eventid);
                await request.ExecuteAsync();


                using (TaxiDataContext db = new TaxiDataContext())
                {
                    //var Query = "delete from Booking_Events where Bookingid=" + Jobid + "";
                    db.ExecuteQuery<int?>("exec stp_DeleteBooking_EventsByID {0}", Jobid);
                }
            }
            catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
                //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "DeleteBookingEvent_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "Jobid =" + Jobid + "Event not found,exception:" + ex.Message + Environment.NewLine);
                General.WriteLog("DeleteBookingEvent_exception", " Jobid =" + Jobid + "Event not found,exception:" + ex.Message);
            }
            catch (Exception ex)
            {
                //System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\" + "DeleteBookingEvent_exception.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ",exception:" + ex.Message + Environment.NewLine);
                General.WriteLog("DeleteBookingEvent_exception", " exception:" + ex.Message);
            }
        }
    }
}