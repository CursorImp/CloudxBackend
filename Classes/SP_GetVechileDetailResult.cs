using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;

namespace SignalRHub
{
    public partial class SP_GetVechileDetailResult
    {

        private System.Nullable<long> _ClientID;

        private long _vehicleID;

        private string _Name;

        private string _Image;

        private System.Nullable<short> _HandLuggages;

        private System.Nullable<short> _NoOfLuggages;

        private System.Nullable<short> _NoOfPassengers;

        private System.Nullable<decimal> _FixedFare;

        private System.Nullable<bool> _IsAmountWise;

        private System.Nullable<short> _PercentageFare;

        private System.Nullable<decimal> _AmountFare;

        private System.Nullable<decimal> _OneWayExtraFare;

        private System.Nullable<decimal> _TwoWayExtraFare;

        private System.Nullable<decimal> _Discount;

        private System.Nullable<decimal> _oneWayFare;

        private System.Nullable<decimal> _ReturnFare;

        private System.Nullable<bool> _IsDefault;

        private System.Nullable<bool> _IsReturn;

        private System.Nullable<bool> _IsDisablePrice;

        private System.Nullable<bool> _ShowPriceMessage;

        private string _DisablePriceMessage;

        public SP_GetVechileDetailResult()
        {
        }

        [Column(Storage = "_ClientID", DbType = "BigInt")]
        public System.Nullable<long> ClientID
        {
            get
            {
                return this._ClientID;
            }
            set
            {
                if ((this._ClientID != value))
                {
                    this._ClientID = value;
                }
            }
        }

        [Column(Storage = "_vehicleID", DbType = "BigInt NOT NULL")]
        public long vehicleID
        {
            get
            {
                return this._vehicleID;
            }
            set
            {
                if ((this._vehicleID != value))
                {
                    this._vehicleID = value;
                }
            }
        }

        [Column(Storage = "_Name", DbType = "VarChar(50)")]
        public string Name
        {
            get
            {
                return this._Name;
            }
            set
            {
                if ((this._Name != value))
                {
                    this._Name = value;
                }
            }
        }

        [Column(Storage = "_Image", DbType = "VarChar(50)")]
        public string Image
        {
            get
            {
                return this._Image;
            }
            set
            {
                if ((this._Image != value))
                {
                    this._Image = value;
                }
            }
        }

        [Column(Storage = "_HandLuggages", DbType = "SmallInt")]
        public System.Nullable<short> HandLuggages
        {
            get
            {
                return this._HandLuggages;
            }
            set
            {
                if ((this._HandLuggages != value))
                {
                    this._HandLuggages = value;
                }
            }
        }

        [Column(Storage = "_NoOfLuggages", DbType = "SmallInt")]
        public System.Nullable<short> NoOfLuggages
        {
            get
            {
                return this._NoOfLuggages;
            }
            set
            {
                if ((this._NoOfLuggages != value))
                {
                    this._NoOfLuggages = value;
                }
            }
        }

        [Column(Storage = "_NoOfPassengers", DbType = "SmallInt")]
        public System.Nullable<short> NoOfPassengers
        {
            get
            {
                return this._NoOfPassengers;
            }
            set
            {
                if ((this._NoOfPassengers != value))
                {
                    this._NoOfPassengers = value;
                }
            }
        }

        [Column(Storage = "_FixedFare", DbType = "Decimal(0,0)")]
        public System.Nullable<decimal> FixedFare
        {
            get
            {
                return this._FixedFare;
            }
            set
            {
                if ((this._FixedFare != value))
                {
                    this._FixedFare = value;
                }
            }
        }

        [Column(Storage = "_IsAmountWise", DbType = "Bit")]
        public System.Nullable<bool> IsAmountWise
        {
            get
            {
                return this._IsAmountWise;
            }
            set
            {
                if ((this._IsAmountWise != value))
                {
                    this._IsAmountWise = value;
                }
            }
        }

        [Column(Storage = "_PercentageFare", DbType = "SmallInt")]
        public System.Nullable<short> PercentageFare
        {
            get
            {
                return this._PercentageFare;
            }
            set
            {
                if ((this._PercentageFare != value))
                {
                    this._PercentageFare = value;
                }
            }
        }

        [Column(Storage = "_AmountFare", DbType = "Decimal(0,0)")]
        public System.Nullable<decimal> AmountFare
        {
            get
            {
                return this._AmountFare;
            }
            set
            {
                if ((this._AmountFare != value))
                {
                    this._AmountFare = value;
                }
            }
        }

        [Column(Storage = "_OneWayExtraFare", DbType = "Decimal(0,0)")]
        public System.Nullable<decimal> OneWayExtraFare
        {
            get
            {
                return this._OneWayExtraFare;
            }
            set
            {
                if ((this._OneWayExtraFare != value))
                {
                    this._OneWayExtraFare = value;
                }
            }
        }

        [Column(Storage = "_TwoWayExtraFare", DbType = "Decimal(0,0)")]
        public System.Nullable<decimal> TwoWayExtraFare
        {
            get
            {
                return this._TwoWayExtraFare;
            }
            set
            {
                if ((this._TwoWayExtraFare != value))
                {
                    this._TwoWayExtraFare = value;
                }
            }
        }

        [Column(Storage = "_Discount", DbType = "Decimal(0,0)")]
        public System.Nullable<decimal> Discount
        {
            get
            {
                return this._Discount;
            }
            set
            {
                if ((this._Discount != value))
                {
                    this._Discount = value;
                }
            }
        }

        [Column(Storage = "_oneWayFare", DbType = "Decimal(0,0)")]
        public System.Nullable<decimal> oneWayFare
        {
            get
            {
                return this._oneWayFare;
            }
            set
            {
                if ((this._oneWayFare != value))
                {
                    this._oneWayFare = value;
                }
            }
        }

        [Column(Storage = "_ReturnFare", DbType = "Decimal(0,0)")]
        public System.Nullable<decimal> ReturnFare
        {
            get
            {
                return this._ReturnFare;
            }
            set
            {
                if ((this._ReturnFare != value))
                {
                    this._ReturnFare = value;
                }
            }
        }

        [Column(Storage = "_IsDefault", DbType = "Bit")]
        public System.Nullable<bool> IsDefault
        {
            get
            {
                return this._IsDefault;
            }
            set
            {
                if ((this._IsDefault != value))
                {
                    this._IsDefault = value;
                }
            }
        }

        [Column(Storage = "_IsReturn", DbType = "Bit")]
        public System.Nullable<bool> IsReturn
        {
            get
            {
                return this._IsReturn;
            }
            set
            {
                if ((this._IsReturn != value))
                {
                    this._IsReturn = value;
                }
            }
        }

        [Column(Storage = "_IsDisablePrice", DbType = "Bit")]
        public System.Nullable<bool> IsDisablePrice
        {
            get
            {
                return this._IsDisablePrice;
            }
            set
            {
                if ((this._IsDisablePrice != value))
                {
                    this._IsDisablePrice = value;
                }
            }
        }

        [Column(Storage = "_ShowPriceMessage", DbType = "Bit")]
        public System.Nullable<bool> ShowPriceMessage
        {
            get
            {
                return this._ShowPriceMessage;
            }
            set
            {
                if ((this._ShowPriceMessage != value))
                {
                    this._ShowPriceMessage = value;
                }
            }
        }

        [Column(Storage = "_DisablePriceMessage", DbType = "VarChar(250)")]
        public string DisablePriceMessage
        {
            get
            {
                return this._DisablePriceMessage;
            }
            set
            {
                if ((this._DisablePriceMessage != value))
                {
                    this._DisablePriceMessage = value;
                }
            }
        }
    }
}
