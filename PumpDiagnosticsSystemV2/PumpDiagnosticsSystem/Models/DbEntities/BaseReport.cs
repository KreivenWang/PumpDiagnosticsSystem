using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PumpDiagnosticsSystem.Models.DbEntities
{
    public class BaseReport
    {
        [Key]
        public int Id { get; set; }

        public string Remark1 { get; set; }

        public string Remark2 { get; set; }

        public string Remark3 { get; set; }

    }
}
