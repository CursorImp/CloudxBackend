using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalRHub
{

    public class Clsstp_CalculateGeneralFaresBySubCompany
    {
        public Clsstp_CalculateGeneralFaresBySubCompany()
        {

        }

      
        public decimal? totalFares { get; set; }
        public decimal? totalCost { get; set; }

        public string Result { get; set; }
   
        public bool? CompanyFareExist { get; set; }
    }

}