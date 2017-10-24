using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PumpDiagnosticsSystem.Core.Parser.Base;

namespace PumpDiagnosticsSystem.Core.Parser
{
    public class EasyParser :BaseParser
    {
        public static double Parse(string expression)
        {
            var calc = new EasyParser();
            return calc.Calculate(expression);
        }
    }
}
