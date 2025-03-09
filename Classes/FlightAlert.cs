using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalRHub
{

    public class RuleEvent
    {
        public string type { get; set; }
    }

    public class RuleEvents
    {
        public List<RuleEvent> ruleEvent { get; set; }
    }

    public class Delivery
    {
        public string format { get; set; }
        public string destination { get; set; }
    }

    public class Rule
    {
        public string id { get; set; }
        public string carrierFsCode { get; set; }
        public string flightNumber { get; set; }
        public string departureAirportFsCode { get; set; }
        public string arrivalAirportFsCode { get; set; }
        public DateTime departure { get; set; }
        public DateTime arrival { get; set; }
        public string name { get; set; }
        public RuleEvents ruleEvents { get; set; }
        public Delivery delivery { get; set; }
    }

    public class DepartureDate
    {
        public DateTime dateUtc { get; set; }
        public DateTime dateLocal { get; set; }
    }

    public class ArrivalDate
    {
        public DateTime dateUtc { get; set; }
        public DateTime dateLocal { get; set; }
    }

    public class Schedule
    {
        public string flightType { get; set; }
        public string serviceClasses { get; set; }
        public string restrictions { get; set; }
    }

    public class PublishedDeparture
    {
        public DateTime dateUtc { get; set; }
        public DateTime dateLocal { get; set; }
    }

    public class ScheduledGateDeparture
    {
        public DateTime dateUtc { get; set; }
        public DateTime dateLocal { get; set; }
    }

    public class EstimatedGateDeparture
    {
        public DateTime dateUtc { get; set; }
        public DateTime dateLocal { get; set; }
    }

    public class ActualGateDeparture
    {
        public DateTime dateUtc { get; set; }
        public DateTime dateLocal { get; set; }
    }

    public class FlightPlanPlannedDeparture
    {
        public DateTime dateUtc { get; set; }
        public DateTime dateLocal { get; set; }
    }

    public class EstimatedRunwayDeparture
    {
        public DateTime dateUtc { get; set; }
        public DateTime dateLocal { get; set; }
    }

    public class ActualRunwayDeparture
    {
        public DateTime dateUtc { get; set; }
        public DateTime dateLocal { get; set; }
    }

    public class PublishedArrival
    {
        public DateTime dateUtc { get; set; }
        public DateTime dateLocal { get; set; }
    }

    public class FlightPlanPlannedArrival
    {
        public DateTime dateUtc { get; set; }
        public DateTime dateLocal { get; set; }
    }

    public class ScheduledGateArrival
    {
        public DateTime dateUtc { get; set; }
        public DateTime dateLocal { get; set; }
    }

    public class EstimatedGateArrival
    {
        public DateTime dateUtc { get; set; }
        public DateTime dateLocal { get; set; }
    }

    public class ActualGateArrival
    {
        public DateTime dateUtc { get; set; }
        public DateTime dateLocal { get; set; }
    }

    public class EstimatedRunwayArrival
    {
        public DateTime dateUtc { get; set; }
        public DateTime dateLocal { get; set; }
    }

    public class ActualRunwayArrival
    {
        public DateTime dateUtc { get; set; }
        public DateTime dateLocal { get; set; }
    }

    public class OperationalTimes
    {
        public PublishedDeparture publishedDeparture { get; set; }
        public ScheduledGateDeparture scheduledGateDeparture { get; set; }
        public EstimatedGateDeparture estimatedGateDeparture { get; set; }
        public ActualGateDeparture actualGateDeparture { get; set; }
        public FlightPlanPlannedDeparture flightPlanPlannedDeparture { get; set; }
        public EstimatedRunwayDeparture estimatedRunwayDeparture { get; set; }
        public ActualRunwayDeparture actualRunwayDeparture { get; set; }
        public PublishedArrival publishedArrival { get; set; }
        public FlightPlanPlannedArrival flightPlanPlannedArrival { get; set; }
        public ScheduledGateArrival scheduledGateArrival { get; set; }
        public EstimatedGateArrival estimatedGateArrival { get; set; }
        public ActualGateArrival actualGateArrival { get; set; }
        public EstimatedRunwayArrival estimatedRunwayArrival { get; set; }
        public ActualRunwayArrival actualRunwayArrival { get; set; }
    }

    public class Codeshare
    {
        public string fsCode { get; set; }
        public string flightNumber { get; set; }
        public string relationship { get; set; }
    }

    public class Codeshares
    {
        public List<Codeshare> codeshare { get; set; }
    }

    public class Delays
    {
        public string departureGateDelayMinutes { get; set; }
    }

    public class FlightDurations
    {
        public string scheduledBlockMinutes { get; set; }
        public string blockMinutes { get; set; }
        public string scheduledAirMinutes { get; set; }
        public string airMinutes { get; set; }
        public string scheduledTaxiOutMinutes { get; set; }
        public string taxiOutMinutes { get; set; }
        public string scheduledTaxiInMinutes { get; set; }
        public string taxiInMinutes { get; set; }
    }

    public class AirportResources
    {
        public string departureTerminal { get; set; }
        public string departureGate { get; set; }
        public string arrivalTerminal { get; set; }
        public string baggage { get; set; }
    }

    public class FlightEquipment
    {
        public string scheduledEquipmentIataCode { get; set; }
        public string actualEquipmentIataCode { get; set; }
        public string tailNumber { get; set; }
        public string fleetAircraftId { get; set; }
    }

    public class UpdatedAt
    {
        public DateTime dateUtc { get; set; }
    }

    public class UpdatedTextField
    {
        public string field { get; set; }
        public string newText { get; set; }
        public string originalText { get; set; }
    }

    public class UpdatedTextFields
    {
        public List<UpdatedTextField> updatedTextField { get; set; }
    }

    public class UpdatedDateField
    {
        public string field { get; set; }
        public DateTime newDateLocal { get; set; }
        public DateTime newDateUtc { get; set; }
        public DateTime? originalDateLocal { get; set; }
        public DateTime? originalDateUtc { get; set; }
    }

    public class UpdatedDateFields
    {
        public List<UpdatedDateField> updatedDateField { get; set; }
    }

    public class FlightStatusUpdate
    {
        public UpdatedAt updatedAt { get; set; }
        public string source { get; set; }
        public UpdatedTextFields updatedTextFields { get; set; }
        public UpdatedDateFields updatedDateFields { get; set; }
    }

    public class FlightStatusUpdates
    {
        public List<FlightStatusUpdate> flightStatusUpdate { get; set; }
    }

    public class FlightStatus
    {
        public string flightId { get; set; }
        public string carrierFsCode { get; set; }
        public string operatingCarrierFsCode { get; set; }
        public string primaryCarrierFsCode { get; set; }
        public string flightNumber { get; set; }
        public string departureAirportFsCode { get; set; }
        public string arrivalAirportFsCode { get; set; }
        public DepartureDate departureDate { get; set; }
        public ArrivalDate arrivalDate { get; set; }
        public string status { get; set; }
        public Schedule schedule { get; set; }
        public OperationalTimes operationalTimes { get; set; }
        public Codeshares codeshares { get; set; }
        public Delays delays { get; set; }
        public FlightDurations flightDurations { get; set; }
        public AirportResources airportResources { get; set; }
        public FlightEquipment flightEquipment { get; set; }
        public FlightStatusUpdates flightStatusUpdates { get; set; }
    }

    public class Event
    {
        public string type { get; set; }
    }

    public class Alert
    {
        public Rule rule { get; set; }
        public FlightStatus flightStatus { get; set; }
        public Event @event { get; set; }
        public string dataSource { get; set; }
        public DateTime dateTimeRecorded { get; set; }
    }

    public class Airline
    {
        public string fs { get; set; }
        public string iata { get; set; }
        public string icao { get; set; }
        public string name { get; set; }
        public string active { get; set; }
    }

    public class Airlines
    {
        public List<Airline> airline { get; set; }
    }

    public class Airport
    {
        public string fs { get; set; }
        public string iata { get; set; }
        public string icao { get; set; }
        public string faa { get; set; }
        public string name { get; set; }
        public string city { get; set; }
        public string cityCode { get; set; }
        public string stateCode { get; set; }
        public string countryCode { get; set; }
        public string countryName { get; set; }
        public string regionName { get; set; }
        public string timeZoneRegionName { get; set; }
        public string weatherZone { get; set; }
        public DateTime localTime { get; set; }
        public string utcOffsetHours { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string elevationFeet { get; set; }
        public string classification { get; set; }
        public string active { get; set; }
        public string street1 { get; set; }
        public string street2 { get; set; }
        public string postalCode { get; set; }
    }

    public class Airports
    {
        public List<Airport> airport { get; set; }
    }

    public class Equipment
    {
        public string iata { get; set; }
        public string name { get; set; }
        public string turboProp { get; set; }
        public string jet { get; set; }
        public string widebody { get; set; }
        public string regional { get; set; }
    }

    public class Equipments
    {
        public List<Equipment> equipment { get; set; }
    }

    public class Appendix
    {
        public Airlines airlines { get; set; }
        public Airports airports { get; set; }
        public Equipments equipments { get; set; }
    }

    public class ArrivalAirportRoot
    {
        public Alert alert { get; set; }
        public Appendix appendix { get; set; }
    }



    public class ClsBookingsInfo
    {

        public List<stp_GetBookingsDataResult> listofBookings = null;       
        public string flightnumber;
        public long JobId;
        public string BookingNo;
        public string NewPickupDate;
        public string soundfilename;
        public bool shownotification;
        public int notificationappearon = 0;
        public string notificationtitle;
        public string notificationcontent;
        public string notificationcolor;
        public int notificationautoclosedelay;
        public bool shownotificationimage;
    }

}