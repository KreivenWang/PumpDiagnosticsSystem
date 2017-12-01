using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PumpDiagnosticsSystem.Models.DbEntities
{
    /// <summary>
    /// 软件诊断（统计）记录
    /// </summary>
    [Table("DiagRecord")]
    public class DiagRecord
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 运行记录的Id
        /// </summary>
        public int RRId { get; set; }

        /// <summary>
        /// 组合推断的判据库Id
        /// </summary>
        public int IcId { get; set; }

        public string PSGuid { get; set; }
        public string CompCode { get; set; }
        public int DiagCount { get; set; }

        public bool IsSameDiagRecord(DiagRecord dr)
        {
            return RRId == dr.RRId &&
                   IcId == dr.IcId &&
                   PSGuid == dr.PSGuid &&
                   CompCode == dr.CompCode;
        }
    }
}
