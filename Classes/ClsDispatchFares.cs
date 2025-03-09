using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Linq.Mapping;

namespace CabTreasureAppAPI
{
    public partial class ClsDispatchFares
    {

      

        private int? _ID;

        private string _Name;

        private string _Image;

        private System.Nullable<int> _NoOfPassengers;

        private System.Nullable<int> _NoOfLuggages;

        private System.Nullable<int> _HandLuggages;

        private System.Nullable<decimal> _StartRate;

        private System.Nullable<decimal> _IncrementRate;

        private System.Nullable<decimal> _StartRateValidMiles;

        private string _Logo;

        private System.Nullable<int> _SortOrder;

        private System.Nullable<decimal> _Fare;

        private System.Nullable<decimal> _ReturnFare;
        private System.Nullable<decimal> _WaitAndReturnFare;

        private string _IsQuoted;

        private System.Nullable<decimal> _ExtraCharges;
        private System.Nullable<decimal> _AgentFee;
        private System.Nullable<decimal> _AgentCharge;


        private System.Nullable<decimal> _Congestion;
        private System.Nullable<decimal> _Parking;
        private System.Nullable<decimal> _Waiting;

        private System.Nullable<decimal> _JourneyMiles;

     
        public decimal? JourneyMiles
        {
            get { return _JourneyMiles; }
            set { _JourneyMiles = value; }
        }

        private System.Nullable<decimal> _CompanyPrice;

        public System.Nullable<decimal> CompanyPrice
        {
            get { return _CompanyPrice; }
            set { _CompanyPrice = value; }
        }

        private System.Nullable<decimal> _ReturnCompanyPrice;

        public System.Nullable<decimal> ReturnCompanyPrice
        {
            get { return _ReturnCompanyPrice; }
            set { _ReturnCompanyPrice = value; }
        }



        public string IsQuoted
        {
            get { return _IsQuoted; }
            set { _IsQuoted = value; }
        }
        private string _DisplayMessage;

        public string DisplayMessage
        {
            get { return _DisplayMessage; }
            set { _DisplayMessage = value; }
        }



        private string _PromotionDetails;

        public string PromotionDetails
        {
            get { return _PromotionDetails; }
            set { _PromotionDetails = value; }
        }

        public ClsDispatchFares()
        {
        }

       

       
        public int? ID
        {
            get
            {
                return this._ID;
            }
            set
            {
                if ((this._ID != value))
                {
                    this._ID = value;
                }
            }
        }

        
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


        public System.Nullable<int> NoOfPassengers
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


        public System.Nullable<int> NoOfLuggages
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

      
        public System.Nullable<int> HandLuggages
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

      
        public System.Nullable<decimal> StartRate
        {
            get
            {
                return this._StartRate;
            }
            set
            {
                if ((this._StartRate != value))
                {
                    this._StartRate = value;
                }
            }
        }

   
        public System.Nullable<decimal> IncrementRate
        {
            get
            {
                return this._IncrementRate;
            }
            set
            {
                if ((this._IncrementRate != value))
                {
                    this._IncrementRate = value;
                }
            }
        }


        public System.Nullable<decimal> StartRateValidMiles
        {
            get
            {
                return this._StartRateValidMiles;
            }
            set
            {
                if ((this._StartRateValidMiles != value))
                {
                    this._StartRateValidMiles = value;
                }
            }
        }


        public string Logo
        {
            get
            {
                return this._Logo;
            }
            set
            {
                if ((this._Logo != value))
                {
                    this._Logo = value;
                }
            }
        }

      
        public System.Nullable<int> SortOrder
        {
            get
            {
                return this._SortOrder;
            }
            set
            {
                if ((this._SortOrder != value))
                {
                    this._SortOrder = value;
                }
            }
        }

       
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



      
        public System.Nullable<decimal> WaitAndReturnFare
        {
            get
            {
                return this._WaitAndReturnFare;
            }
            set
            {
                if ((this.WaitAndReturnFare != value))
                {
                    this._WaitAndReturnFare = value;
                }
            }
        }




        public System.Nullable<decimal> ExtraCharges
        {
            get
            {
                return this._ExtraCharges;
            }
            set
            {
                if ((this._ExtraCharges != value))
                {
                    this._ExtraCharges = value;
                }
            }
        }


        public System.Nullable<decimal> AgentFees
        {
            get
            {
                return this._AgentFee;
            }
            set
            {
                if ((this._AgentFee != value))
                {
                    this._AgentFee = value;
                }
            }
        }



        public System.Nullable<decimal> AgentCharge
        {
            get
            {
                return this._AgentCharge;
            }
            set
            {
                if ((this._AgentCharge != value))
                {
                    this._AgentCharge = value;
                }
            }
        }


        public System.Nullable<decimal> Congestion
        {
            get
            {
                return this._Congestion;
            }
            set
            {
                if ((this._Congestion != value))
                {
                    this._Congestion = value;
                }
            }
        }


        public System.Nullable<decimal> Parking
        {
            get
            {
                return this._Parking;
            }
            set
            {
                if ((this._Parking != value))
                {
                    this._Parking = value;
                }
            }
        }


        public System.Nullable<decimal> Waiting
        {
            get
            {
                return this._Waiting;
            }
            set
            {
                if ((this._Waiting != value))
                {
                    this._Waiting = value;
                }
            }
        }




    }

}