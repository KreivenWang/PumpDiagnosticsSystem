using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PumpDiagnosticsSystem.Core.Parser.Base;
using PumpDiagnosticsSystem.Models;

namespace PumpDiagnosticsSystem.Core.Parser
{
    public class InferComboParser : BaseParser
    {
        /// <summary>
        /// 推断组合表达式中的标识符
        /// </summary>
        public const string Symbol = "A";

        private InferComboItem _icItem;

        /// <summary>
        /// 【判据LibId，判据是否通过】的字典
        /// </summary>
        private Dictionary<int, int> _dict = new Dictionary<int, int>();

        #region static funcs

        public static List<string> MatchAVars(string expression)
        {
            const string ConstMatchRegex = @"A\d{1,4}";
            return RegexMatch(expression, ConstMatchRegex);
        }

        #endregion

        public InferComboParser()
        {

        }

        public void ParseInferComboItem(InferComboItem icItem, Dictionary<int, int> dict)
        {
            //设置字段
            _icItem = icItem;
            _dict = dict;

            //初始化
            ResetParser();

            string parsedExp;
            if (TryParseVariables(out parsedExp)) {
                _icItem.IsHappening = EvaluateExpression(parsedExp);
            }
        }

        public int[] GetCurrentItemCtLibIds()
        {
            var ctLibIds = new List<int>();
            ForEachCtLibId(ctLibId => {
                ctLibIds.Add(ctLibId);
            });
            return ctLibIds.ToArray();
        }

        private void ForEachCtLibId(Action<int> action)
        {
            var exp = _icItem.Expression;
            var vars = MatchAVars(exp).Distinct().ToArray();
            foreach (var v in vars) {
                //替换‘A变量’为 字典中的0或1
                if (v.StartsWith(Symbol)) {
                    var ctLibIdStr = v.Replace(Symbol, string.Empty);
                    int ctLibId;
                    if (int.TryParse(ctLibIdStr, out ctLibId)) {
                        if (_dict.ContainsKey(ctLibId)) {
                            action(ctLibId);
                        } else {
                            throw new Exception($"{nameof(InferComboParser)}: 当前字典中不包含判据Id:{ctLibId}");
                        }
                    } else {
                        throw new Exception($"{nameof(InferComboParser)}: 无法解析{Symbol}变量{v}");
                    }

                } else {
                    throw new Exception($"{nameof(InferComboParser)}: 未知形式的变量.");
                }
            }
        }

        private bool TryParseVariables(out string parsedExpression)
        {
            var exp = _icItem.Expression;
            ForEachCtLibId(ctLibId =>
            {
                var data = _dict[ctLibId];
                exp = exp.Replace($"{Symbol}{ctLibId}", data.ToString());
            });
            parsedExpression = exp;
            return true;
        }
    }
}
    
