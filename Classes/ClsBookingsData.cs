using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalRHub
{
    public partial class clsBookingsData
    {

        private long _Id;

        private System.Nullable<System.DateTime> _Lead;

        private string _Plot;

        private System.Nullable<System.DateTime> _PlotHour;

        private string _RefNumber;

        private System.Nullable<System.DateTime> _BookingDateTime;

        private System.Nullable<System.DateTime> _PickupDateTemp;

        private string _PickUpDate;

        private string _Time;

        private string _Passenger;

        private string _MobileNo;

        private string _TelephoneNo;

        private string _From;

        private string _Pickup;

        private string _FromPostCode;

        private string _To;

        private string _GoingTo;

        private string _ToPostCode;

        private System.Nullable<decimal> _Fare;

        private System.Nullable<int> _Pax;

        private string _PaymentMethod;

        private System.Nullable<int> _FromLocTypeId;

        private System.Nullable<int> _ToLocTypeId;

        private string _BackgroundColor1;

        private string _TextColor1;

        private string _TextColor;

        private string _Account;

        private string _PReference;

        private string _Vehicle;

        private string _UpdateBy;

        private string _SpecialReq;

        private System.Nullable<int> _StatusId;

        private string _Status;

        private string _StatusColor;

        private string _Driver;

        private System.Nullable<int> _DriverId;

        private System.Nullable<bool> _IsAutoDespatch;

        private System.Nullable<int> _BookingTypeId;

        private System.Nullable<int> _HasNotes;

        private string _HasNotesImg;

        private System.Nullable<int> _SubCompanyBgColor;

        private System.Nullable<int> _BookingBackgroundColor;

        private string _GroupId;

        private System.Nullable<int> _FromLocId;

        private System.Nullable<System.DateTime> _PrePickupDate;

        private string _BabySeats;

        private System.Nullable<bool> _IsConfirmedDriver;

        private System.Nullable<decimal> _MilesFromBase;

        private System.Nullable<bool> _IsBidding;

        private System.Nullable<decimal> _DeadMileage;

        private System.Nullable<System.DateTime> _DespatchDateTime;

        private System.Nullable<int> _JourneyTypeId;

        private System.Nullable<System.DateTime> _Due;

        private string _BackgroundColor;

        private System.Nullable<int> _NoofLuggages;

        private int _Vias;

        public clsBookingsData()
        {
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_Id", DbType = "BigInt NOT NULL")]
        public long Id
        {
            get
            {
                return this._Id;
            }
            set
            {
                if ((this._Id != value))
                {
                    this._Id = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_Lead", DbType = "DateTime")]
        public System.Nullable<System.DateTime> Lead
        {
            get
            {
                return this._Lead;
            }
            set
            {
                if ((this._Lead != value))
                {
                    this._Lead = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_Plot", DbType = "VarChar(20)")]
        public string Plot
        {
            get
            {
                return this._Plot;
            }
            set
            {
                if ((this._Plot != value))
                {
                    this._Plot = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_PlotHour", DbType = "DateTime")]
        public System.Nullable<System.DateTime> PlotHour
        {
            get
            {
                return this._PlotHour;
            }
            set
            {
                if ((this._PlotHour != value))
                {
                    this._PlotHour = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_RefNumber", DbType = "VarChar(50)")]
        public string RefNumber
        {
            get
            {
                return this._RefNumber;
            }
            set
            {
                if ((this._RefNumber != value))
                {
                    this._RefNumber = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_BookingDateTime", DbType = "DateTime")]
        public System.Nullable<System.DateTime> BookingDateTime
        {
            get
            {
                return this._BookingDateTime;
            }
            set
            {
                if ((this._BookingDateTime != value))
                {
                    this._BookingDateTime = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_PickupDateTemp", DbType = "DateTime")]
        public System.Nullable<System.DateTime> PickupDateTemp
        {
            get
            {
                return this._PickupDateTemp;
            }
            set
            {
                if ((this._PickupDateTemp != value))
                {
                    this._PickupDateTemp = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_PickUpDate", DbType = "VarChar(8000)")]
        public string PickUpDate
        {
            get
            {
                return this._PickUpDate;
            }
            set
            {
                if ((this._PickUpDate != value))
                {
                    this._PickUpDate = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_Time", DbType = "VarChar(5)")]
        public string Time
        {
            get
            {
                return this._Time;
            }
            set
            {
                if ((this._Time != value))
                {
                    this._Time = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_Passenger", DbType = "VarChar(100)")]
        public string Passenger
        {
            get
            {
                return this._Passenger;
            }
            set
            {
                if ((this._Passenger != value))
                {
                    this._Passenger = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_MobileNo", DbType = "VarChar(50)")]
        public string MobileNo
        {
            get
            {
                return this._MobileNo;
            }
            set
            {
                if ((this._MobileNo != value))
                {
                    this._MobileNo = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_TelephoneNo", DbType = "VarChar(50)")]
        public string TelephoneNo
        {
            get
            {
                return this._TelephoneNo;
            }
            set
            {
                if ((this._TelephoneNo != value))
                {
                    this._TelephoneNo = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Name = "[From]", Storage = "_From", DbType = "VarChar(502)")]
        public string From
        {
            get
            {
                return this._From;
            }
            set
            {
                if ((this._From != value))
                {
                    this._From = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_Pickup", DbType = "VarChar(200)")]
        public string Pickup
        {
            get
            {
                return this._Pickup;
            }
            set
            {
                if ((this._Pickup != value))
                {
                    this._Pickup = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_FromPostCode", DbType = "VarChar(50)")]
        public string FromPostCode
        {
            get
            {
                return this._FromPostCode;
            }
            set
            {
                if ((this._FromPostCode != value))
                {
                    this._FromPostCode = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Name = "[To]", Storage = "_To", DbType = "VarChar(200)")]
        public string To
        {
            get
            {
                return this._To;
            }
            set
            {
                if ((this._To != value))
                {
                    this._To = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_GoingTo", DbType = "VarChar(200)")]
        public string GoingTo
        {
            get
            {
                return this._GoingTo;
            }
            set
            {
                if ((this._GoingTo != value))
                {
                    this._GoingTo = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_ToPostCode", DbType = "VarChar(50)")]
        public string ToPostCode
        {
            get
            {
                return this._ToPostCode;
            }
            set
            {
                if ((this._ToPostCode != value))
                {
                    this._ToPostCode = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_Fare", DbType = "Decimal(18,2)")]
        public System.Nullable<decimal> Fare
        {
            get
            {
                return this._Fare;
            }
            set
            {
                if ((this._Fare != value))
                {
                    this._Fare = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_Pax", DbType = "Int")]
        public System.Nullable<int> Pax
        {
            get
            {
                return this._Pax;
            }
            set
            {
                if ((this._Pax != value))
                {
                    this._Pax = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_PaymentMethod", DbType = "VarChar(50)")]
        public string PaymentMethod
        {
            get
            {
                return this._PaymentMethod;
            }
            set
            {
                if ((this._PaymentMethod != value))
                {
                    this._PaymentMethod = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_FromLocTypeId", DbType = "Int")]
        public System.Nullable<int> FromLocTypeId
        {
            get
            {
                return this._FromLocTypeId;
            }
            set
            {
                if ((this._FromLocTypeId != value))
                {
                    this._FromLocTypeId = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_ToLocTypeId", DbType = "Int")]
        public System.Nullable<int> ToLocTypeId
        {
            get
            {
                return this._ToLocTypeId;
            }
            set
            {
                if ((this._ToLocTypeId != value))
                {
                    this._ToLocTypeId = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_BackgroundColor1", DbType = "VarChar(50)")]
        public string BackgroundColor1
        {
            get
            {
                return this._BackgroundColor1;
            }
            set
            {
                if ((this._BackgroundColor1 != value))
                {
                    this._BackgroundColor1 = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_TextColor1", DbType = "VarChar(20)")]
        public string TextColor1
        {
            get
            {
                return this._TextColor1;
            }
            set
            {
                if ((this._TextColor1 != value))
                {
                    this._TextColor1 = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_TextColor", DbType = "VarChar(30)")]
        public string TextColor
        {
            get
            {
                return this._TextColor;
            }
            set
            {
                if ((this._TextColor != value))
                {
                    this._TextColor = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_Account", DbType = "VarChar(100)")]
        public string Account
        {
            get
            {
                return this._Account;
            }
            set
            {
                if ((this._Account != value))
                {
                    this._Account = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_PReference", DbType = "VarChar(MAX)")]
        public string PReference
        {
            get
            {
                return this._PReference;
            }
            set
            {
                if ((this._PReference != value))
                {
                    this._PReference = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_Vehicle", DbType = "VarChar(100)")]
        public string Vehicle
        {
            get
            {
                return this._Vehicle;
            }
            set
            {
                if ((this._Vehicle != value))
                {
                    this._Vehicle = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_UpdateBy", DbType = "VarChar(50)")]
        public string UpdateBy
        {
            get
            {
                return this._UpdateBy;
            }
            set
            {
                if ((this._UpdateBy != value))
                {
                    this._UpdateBy = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_SpecialReq", DbType = "VarChar(MAX)")]
        public string SpecialReq
        {
            get
            {
                return this._SpecialReq;
            }
            set
            {
                if ((this._SpecialReq != value))
                {
                    this._SpecialReq = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_StatusId", DbType = "Int")]
        public System.Nullable<int> StatusId
        {
            get
            {
                return this._StatusId;
            }
            set
            {
                if ((this._StatusId != value))
                {
                    this._StatusId = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_Status", DbType = "VarChar(50) NOT NULL", CanBeNull = false)]
        public string Status
        {
            get
            {
                return this._Status;
            }
            set
            {
                if ((this._Status != value))
                {
                    this._Status = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_StatusColor", DbType = "VarChar(30)")]
        public string StatusColor
        {
            get
            {
                return this._StatusColor;
            }
            set
            {
                if ((this._StatusColor != value))
                {
                    this._StatusColor = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_Driver", DbType = "VarChar(83)")]
        public string Driver
        {
            get
            {
                return this._Driver;
            }
            set
            {
                if ((this._Driver != value))
                {
                    this._Driver = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_DriverId", DbType = "Int")]
        public System.Nullable<int> DriverId
        {
            get
            {
                return this._DriverId;
            }
            set
            {
                if ((this._DriverId != value))
                {
                    this._DriverId = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_IsAutoDespatch", DbType = "Bit")]
        public System.Nullable<bool> IsAutoDespatch
        {
            get
            {
                return this._IsAutoDespatch;
            }
            set
            {
                if ((this._IsAutoDespatch != value))
                {
                    this._IsAutoDespatch = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_BookingTypeId", DbType = "Int")]
        public System.Nullable<int> BookingTypeId
        {
            get
            {
                return this._BookingTypeId;
            }
            set
            {
                if ((this._BookingTypeId != value))
                {
                    this._BookingTypeId = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_HasNotes", DbType = "Int")]
        public System.Nullable<int> HasNotes
        {
            get
            {
                return this._HasNotes;
            }
            set
            {
                if ((this._HasNotes != value))
                {
                    this._HasNotes = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_HasNotesImg", DbType = "VarChar(1) NOT NULL", CanBeNull = false)]
        public string HasNotesImg
        {
            get
            {
                return this._HasNotesImg;
            }
            set
            {
                if ((this._HasNotesImg != value))
                {
                    this._HasNotesImg = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_SubCompanyBgColor", DbType = "Int")]
        public System.Nullable<int> SubCompanyBgColor
        {
            get
            {
                return this._SubCompanyBgColor;
            }
            set
            {
                if ((this._SubCompanyBgColor != value))
                {
                    this._SubCompanyBgColor = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_BookingBackgroundColor", DbType = "Int")]
        public System.Nullable<int> BookingBackgroundColor
        {
            get
            {
                return this._BookingBackgroundColor;
            }
            set
            {
                if ((this._BookingBackgroundColor != value))
                {
                    this._BookingBackgroundColor = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_GroupId", DbType = "VarChar(10)")]
        public string GroupId
        {
            get
            {
                return this._GroupId;
            }
            set
            {
                if ((this._GroupId != value))
                {
                    this._GroupId = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_FromLocId", DbType = "Int")]
        public System.Nullable<int> FromLocId
        {
            get
            {
                return this._FromLocId;
            }
            set
            {
                if ((this._FromLocId != value))
                {
                    this._FromLocId = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_PrePickupDate", DbType = "Date")]
        public System.Nullable<System.DateTime> PrePickupDate
        {
            get
            {
                return this._PrePickupDate;
            }
            set
            {
                if ((this._PrePickupDate != value))
                {
                    this._PrePickupDate = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_BabySeats", DbType = "VarChar(200)")]
        public string BabySeats
        {
            get
            {
                return this._BabySeats;
            }
            set
            {
                if ((this._BabySeats != value))
                {
                    this._BabySeats = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_IsConfirmedDriver", DbType = "Bit")]
        public System.Nullable<bool> IsConfirmedDriver
        {
            get
            {
                return this._IsConfirmedDriver;
            }
            set
            {
                if ((this._IsConfirmedDriver != value))
                {
                    this._IsConfirmedDriver = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_MilesFromBase", DbType = "Decimal(18,2)")]
        public System.Nullable<decimal> MilesFromBase
        {
            get
            {
                return this._MilesFromBase;
            }
            set
            {
                if ((this._MilesFromBase != value))
                {
                    this._MilesFromBase = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_IsBidding", DbType = "Bit")]
        public System.Nullable<bool> IsBidding
        {
            get
            {
                return this._IsBidding;
            }
            set
            {
                if ((this._IsBidding != value))
                {
                    this._IsBidding = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_DeadMileage", DbType = "Decimal(18,2)")]
        public System.Nullable<decimal> DeadMileage
        {
            get
            {
                return this._DeadMileage;
            }
            set
            {
                if ((this._DeadMileage != value))
                {
                    this._DeadMileage = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_DespatchDateTime", DbType = "DateTime")]
        public System.Nullable<System.DateTime> DespatchDateTime
        {
            get
            {
                return this._DespatchDateTime;
            }
            set
            {
                if ((this._DespatchDateTime != value))
                {
                    this._DespatchDateTime = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_JourneyTypeId", DbType = "Int")]
        public System.Nullable<int> JourneyTypeId
        {
            get
            {
                return this._JourneyTypeId;
            }
            set
            {
                if ((this._JourneyTypeId != value))
                {
                    this._JourneyTypeId = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_Due", DbType = "DateTime")]
        public System.Nullable<System.DateTime> Due
        {
            get
            {
                return this._Due;
            }
            set
            {
                if ((this._Due != value))
                {
                    this._Due = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_BackgroundColor", DbType = "VarChar(30)")]
        public string BackgroundColor
        {
            get
            {
                return this._BackgroundColor;
            }
            set
            {
                if ((this._BackgroundColor != value))
                {
                    this._BackgroundColor = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_NoofLuggages", DbType = "Int")]
        public System.Nullable<int> NoofLuggages
        {
            get
            {
                return this._NoofLuggages;
            }
            set
            {
                if ((this._NoofLuggages != value))
                {
                    this._NoofLuggages = value;
                }
            }
        }

        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_Vias", DbType = "Int NOT NULL")]
        public int Vias
        {
            get
            {
                return this._Vias;
            }
            set
            {
                if ((this._Vias != value))
                {
                    this._Vias = value;
                }
            }
        }
    }
}