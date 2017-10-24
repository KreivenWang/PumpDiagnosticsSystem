using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PumpDiagnosticsSystem.Util;

namespace PumpDiagnosticsSystem.Core.Parser.Base
{
    public abstract class BaseParser
    {
        protected readonly ExpressionParser _exParser = new ExpressionParser();
        protected readonly Variable _evalExpResult = new Variable();

        /// <summary>
        /// 表达式中函数的计算结果列表
        /// </summary>
        protected readonly List<FuncParseResult> _funcParsedResults = new List<FuncParseResult>();

        #region static funcs

        protected static List<string> RegexMatch(string expression, string regStr)
        {
            var rgx = new Regex(regStr, RegexOptions.Multiline);
            var matches = rgx.Matches(expression);
            return (from object match in matches select match.ToString()).ToList();
        }

        #endregion

        protected BaseParser()
        {
            _exParser.SetArgumentsSeparator(',');
            _exParser.SetDecimalSeparator('.');
            _exParser.FuncParsed += result => { _funcParsedResults.AddSingle(result); };
        }

        protected void ResetParser()
        {
            _funcParsedResults.Clear();
        }

        protected bool EvaluateExpression(string expression)
        {
            if (string.IsNullOrEmpty(expression))
                return false;
            try {
                _exParser.SetExpression(expression);
                _evalExpResult.Value = _exParser.Eval();
                return Convert.ToBoolean(_evalExpResult.Value);
            } catch (Exception ex) {
                Log.Error($"表达式( {expression} )在解析时发生错误: " + ex.Message);
                return false;
            }
        }

        protected double Calculate(string expression)
        {
            if (string.IsNullOrEmpty(expression))
                return 0D;
            try {
                _exParser.SetExpression(expression);
                _evalExpResult.Value = _exParser.Eval();
                return _evalExpResult.Value;
            } catch (Exception ex) {
                Log.Error($"表达式( {expression} )在计算时发生错误: " + ex.Message);
                return 0D;
            }
        }
    }
}
