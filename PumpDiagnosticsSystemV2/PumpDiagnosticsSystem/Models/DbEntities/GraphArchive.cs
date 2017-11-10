using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PumpDiagnosticsSystem.Models.DbEntities
{
    public class GraphArchive : Graph
    {
        /// <summary>
        /// 数据库中的Id标识，保存故障数据时，由EF自动生成
        /// </summary>
        [Key]
        public int Id { get; set; }

        public string DataStr { get; set; }

        public static GraphArchive FromGraph(Graph g)
        {
            var ga = new GraphArchive();
            ga.Time = g.Time;
            ga.Signal = g.Signal;
            ga.Number = g.Number;
            ga.SSGuid = g.SSGuid;
            ga.PPGuid = g.PPGuid;
            ga.Pos = g.Pos;
            ga.Type = g.Type;
            ga.DataStr = string.Join(",", g.Data);
            return ga;
        }
    }
}
