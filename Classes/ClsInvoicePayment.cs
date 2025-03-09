using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalRHub.Classes
{
    public class ClsInvoicePayment
    {
        public long? Id { set; get; }

        public string InvoiceNo { set; get; }

        public DateTime? InvoiceDate { set; get; }

        public decimal? CreditNoteTotal { set; get; }

        public decimal? InvoiceTotal { set; get; }

        public decimal? InvoicePayment { set; get; }

        public decimal? Balance { set; get; }

        public decimal? TotalBalance { set; get; }

        public DateTime? CreditNoteDate { set; get; }

        public DateTime? PaymentDate { set; get; }

        public string CompanyName { set; get; }

        public string CreditNoteNo { set; get; }

        public string Address { set; get; }
    }
}