using System.Data;
using System.Linq;
using PumpDiagnosticsSystem.Dbs;

namespace PumpDiagnosticsSystem.Security
{
    public static class RegisterOp
    {
        public static RegisterInfo GetRegistInfo(string localMac)
        {
            const string tblName = "ClientMac";
            var accessOp = new AccessOp();
            var macTable = accessOp.LoadTable(tblName);
            var registedMacs = (from DataRow row in macTable.Rows
                select new RegisterInfo {
                    Mac = row[1].ToString(),
                    Remark = row[2].ToString()
                }).ToList();

            return registedMacs.Find(m => m.Mac == localMac);
        }
    }

    public class RegisterInfo
    {
        public string Mac { get; set; }
        public string Remark { get; set; }
    }
}
