using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalRHub
{
 

    public struct RefreshTypes
    {
        public static string DESPATCH_TERMINAL = "despatch terminal";

        public static string REFRESH_DASHBOARD_DRIVER = "refresh dashboard drivers";

        public static string REFRESH_DASHBOARD = "refresh dashboard";
        public static string REFRESH_ONLY_DASHBOARD = "refresh only dashboard";
        public static string REFRESH_ACTIVE_DASHBOARD = "refresh active dashboard";
        public static string REFRESH_REQUIRED_DASHBOARD = "refresh required dashboard";
        public static string REFRESH_ACTIVEBOOKINGS_DASHBOARD = "refresh active booking dashboard";

        public static string REFRESH_BOOKING_DASHBOARD = "refresh booking dashboard";
        public static string REFRESH_WAITING_AND_DASBOARD = "refresh waiting and dasboard";

        public static string REFRESH_TODAY_AND_PREBOOKING_DASHBOARD = "refresh today and prebooking dashboard";


        public static string REFRESH_WEBBOOKINGS_DASHBOARD = "refresh webbookings dashboard";
        public static string REFRESH_DECLINEDWEBBOOKINGS_DASHBOARD = "refresh declined webbookings dashboard";


        public static string REFRESH_BOOKINGHISTORY_DASHBOARD = "refresh bookinghistory dashboard";


        public static string INCOMING_CALL = "incoming call";

        public static string JOB_LATE = "joblate=";
        public static string SMS = "sms=";
        public static string REFRESH_PLOTS = "refresh plots";
        public static string AUTHORIZE_WEBBOOKING = "authorize web";
        public static string REFRESH_DESPATCHJOB = "refresh despatchjob";

    }

}