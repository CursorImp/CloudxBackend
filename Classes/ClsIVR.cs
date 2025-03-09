using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalRHub
{
    public class IVRBookingDetails
    {
        public long BookingId { get; set; }

        public string Name { get; set; }

        public string Phone { get; set; }

        public string PickUp { get; set; }

        public string Destination { get; set; }


    }

    public class IVRNotificationClient
    {
        public string NotificationType { get; set; }
        public string NotificationMessage { get; set; }
        public DateTime TimeStamp { get; set; }

        public string BookingJson { get; set; }
    }
    public class IVRNotification : IVRBookingDetails
    {
        public string BookingNumber { get; set; }

        public string PickUpDateTime { get; set; }

    }
}