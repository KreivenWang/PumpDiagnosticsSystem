using System;

namespace PumpDiagnosticsSystem.Core.Parser.Base
{
    public struct FuncParseResult
    {
        public string FuncName;
        public double Value;

        public FuncParseResult(string funcName, double value)
        {
            FuncName = funcName;
            Value = value;
        }
    }
}