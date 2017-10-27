using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PumpDiagnosticsSystem.Business;
using PumpDiagnosticsSystem.Models.DbEntities;
using PumpDiagnosticsSystem.Web.Models;

namespace PumpDiagnosticsSystem.Web.Controllers
{
    public class HomeController : Controller
    {
        private bool _started;

        public ActionResult Index()
        {
            var result = ConsumeGraphMsg();

            return View(new HomeModel() {Message = result });
        }


        public ActionResult GetSpec()
        {
            if (!_started) {
                SpectrumMessenger.StartReceive(r =>
//                MsgRepo.GraphMsgs.Add(r)
MsgRepo.GraphMsg = r
                );
                _started = true;
            }
            var graphMsg = ConsumeGraphMsg();
            if (string.IsNullOrEmpty(graphMsg)) return Json(new {msg = "NoData"}, JsonRequestBehavior.AllowGet);
            var msgs = graphMsg.Replace("|||", "~").Split('~').ToList();

            if (msgs[4] != "Spectrum") return Json(new { msg = "Not Spectrum" }, JsonRequestBehavior.AllowGet);

            var index = 0;
            var result = new {
                guid = msgs[0],
                rpm = msgs[1],
                time = msgs[2],
                pos = msgs[3],
                type = msgs[4],
                dataArr = msgs[5].Split(',').Select(d => new {i = index++, v = d})
            };
//            msgs.Add(ppSys.Guid.ToFormatedString());
//            msgs.Add(GetSpeed(ppSys)?.ToString() ?? "-1");
//            msgs.Add(graph.Time.ToString("yyyy-MM-dd HH:mm:ss"));
//            msgs.Add(graph.Pos.ToString());
//            msgs.Add(graph.Type.ToString());
//            msgs.Add(GraphArchive.FromGraph(graph).DataStr);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private string ConsumeGraphMsg()
        {
            return MsgRepo.GraphMsg;
            var result = string.Empty;
            if (MsgRepo.GraphMsgs.Any()) {
                result = MsgRepo.GraphMsgs[0];
                if (MsgRepo.GraphMsgs.Count > 10)
                    MsgRepo.GraphMsgs.RemoveAt(0);
            }
            return result;
        }
    }
}