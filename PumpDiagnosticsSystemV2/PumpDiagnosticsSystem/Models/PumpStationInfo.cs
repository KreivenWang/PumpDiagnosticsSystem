using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PumpDiagnosticsSystem.Models
{
    public class PumpStationInfo
    {
        public Guid PSGuid { get; set; }

        public string PSCode { get; set; }

        public string PSName { get; set; }
    }
}
