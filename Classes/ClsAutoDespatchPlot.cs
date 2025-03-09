using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalRHub
{
    public class ClsAutoDespatchPlot
    {
        private int? _DriverId;

        public int? DriverId
        {
            get { return _DriverId; }
            set { _DriverId = value; }
        }



        private long _JobId;

        public long JobId
        {
            get { return _JobId; }
            set { _JobId = value; }
        }


        private string _PlotName;

        public string PlotName
        {
            get { return _PlotName; }
            set { _PlotName = value; }
        }
        private string _BackupPlot1;

        public string BackupPlot1
        {
            get { return _BackupPlot1; }
            set { _BackupPlot1 = value; }
        }
        private string _BackupPlot2;

        public string BackupPlot2
        {
            get { return _BackupPlot2; }
            set { _BackupPlot2 = value; }
        }


        private bool _IsDespatched;

        public bool IsDespatched
        {
            get { return _IsDespatched; }
            set { _IsDespatched = value; }
        }



    }
}