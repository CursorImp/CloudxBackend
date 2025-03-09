using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalRHub
{
    public class JobPromotion
    {
        public string PromotionCode;
        public string PromotionTitle;
        public string PromotionMessage;
        public string PromotionStartDateTime;
        public string PromotionEndDateTime;
        public int? PromotionTypeID; // 1 percent // 2 Amount
        public decimal? Charges;

    }
}
