using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using PumpDiagnosticsSystem.Business;
using PumpDiagnosticsSystem.Web.Models;

namespace PumpDiagnosticsSystem.Web.Controllers
{
    public class HomeController : Controller
    {
        private bool _started;
        private Thread _receiveThread;
        public ActionResult Index()
        {
            var result = ConsumeGraphMsg();

            return View(new HomeModel() {Message = result });
        }


        public ActionResult GetSpec()
        {
            if (!_started) {
                _receiveThread = new Thread(() =>
                {
                    Debug.WriteLine($"Start to Receive");
                    SpectrumMessenger.Receive(s => MsgRepo.GraphMsg = s);
//                    Thread.Sleep(Timeout.Infinite);
                    Debug.WriteLine($"Receive End");
                });
                _receiveThread.Name = "Receive Thread";
                _receiveThread.Start();
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
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Update()
        {
            //SpectrumMessenger.ReceiveAction(s => MsgRepo.GraphMsg = s);
            return Json(new {result = "done"}, JsonRequestBehavior.AllowGet);
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


//        [AsyncTimeout(1000)]
//        public async Task<ActionResult> GetSpecAsync()
//        {
//            var data = await GetPageTaskAsync("http://163.com");
//            return data;
//        }
//
//        public async Task<ActionResult> GetPageTaskAsync(string url)
//        {
//            try {
//                using (var client = new HttpClient()) {
//                    await Task.Delay(3000);
//                    var fetchTextTask = client.GetStringAsync(url);
//                    return Json(new { fetchText = await fetchTextTask, error = "NO" }, JsonRequestBehavior.AllowGet);
//                }
//            } catch (WebException ex) {
//
//                throw ex;
//            }
//        }
    }
}