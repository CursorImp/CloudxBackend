using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalRHub
{

   
    public partial class AutoDispatchSetting
    {

        

        private int _Id;

        private System.Nullable<int> _AutoDispatchModeType;

        private System.Nullable<bool> _TopStandingInQueue;

        private System.Nullable<bool> _TopStandingInQueueBackupPlot;

        private System.Nullable<bool> _NearestDriver;

        private System.Nullable<decimal> _NearestDriverRadius;

        private System.Nullable<bool> _EnableBid;

        private System.Nullable<bool> _BiddingRadius;

        private System.Nullable<bool> _AutoAllocateSTC;

        private System.Nullable<bool> _BiddingType;

        private string _OtherData;


        public AutoDispatchSetting()
        {
          
        }

        public int Id
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

        public System.Nullable<int> AutoDispatchModeType
        {
            get
            {
                return this._AutoDispatchModeType;
            }
            set
            {
                if ((this._AutoDispatchModeType != value))
                {
                  
                    this._AutoDispatchModeType = value;
             
                }
            }
        }

      
        public System.Nullable<bool> TopStandingInQueue
        {
            get
            {
                return this._TopStandingInQueue;
            }
            set
            {
                if ((this._TopStandingInQueue != value))
                {
                 
                    this._TopStandingInQueue = value;
                   
                }
            }
        }

      
        public System.Nullable<bool> TopStandingInQueueBackupPlot
        {
            get
            {
                return this._TopStandingInQueueBackupPlot;
            }
            set
            {
                if ((this._TopStandingInQueueBackupPlot != value))
                {
                   
                    this._TopStandingInQueueBackupPlot = value;
                  
                }
            }
        }
    
        public System.Nullable<bool> NearestDriver
        {
            get
            {
                return this._NearestDriver;
            }
            set
            {
                if ((this._NearestDriver != value))
                {
                    
                    this._NearestDriver = value;
                  
                }
            }
        }


        public System.Nullable<decimal> NearestDriverRadius
        {
            get
            {
                return this._NearestDriverRadius;
            }
            set
            {
                if ((this._NearestDriverRadius != value))
                {
                   
                    this._NearestDriverRadius = value;
                   
                }
            }
        }

        public System.Nullable<bool> EnableBid
        {
            get
            {
                return this._EnableBid;
            }
            set
            {
                if ((this._EnableBid != value))
                {
                   
                
                    this._EnableBid = value;
            
                   
                }
            }
        }

      
        public System.Nullable<bool> BiddingRadius
        {
            get
            {
                return this._BiddingRadius;
            }
            set
            {
                if ((this._BiddingRadius != value))
                {
                 
                  
                    this._BiddingRadius = value;
            
                }
            }
        }

    
        public System.Nullable<bool> AutoAllocateSTC
        {
            get
            {
                return this._AutoAllocateSTC;
            }
            set
            {
                if ((this._AutoAllocateSTC != value))
                {
               
                    this._AutoAllocateSTC = value;
                
                }
            }
        }

    
        public System.Nullable<bool> BiddingType
        {
            get
            {
                return this._BiddingType;
            }
            set
            {
                if ((this._BiddingType != value))
                {
                  
                    this._BiddingType = value;
           
                }
            }
        }

        public string OtherData
        {
            get
            {
                return this._OtherData;
            }
            set
            {
                if ((this._OtherData != value))
                {

                    this._OtherData = value;

                }
            }
        }


    }


    public class ClsAutoDespatchOtherData
    {
        public int BookingType;
        public decimal? Radius;
        public string OtherData;
    }
}