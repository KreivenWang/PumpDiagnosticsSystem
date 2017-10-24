using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PumpDiagnosticsSystem.Datas
{
    public class DataDictionary : List<DataPair>
    {
        public double this[string key]
        {
            get { return this.First(d=>d.Key == key).Value; }
        }

        public void Add(string key, double value)
        {
            Add(new DataPair {Key = key, Value = value});
        }

        public bool ContainsKey(string key)
        {
            return Find(d => d.Key == key) != null;
        }

        public bool TryGetMaxValue(string key, out double maxValue)
        {
            var matches = FindAll(d => d.Key == key);
            if (matches.Any()) {
                maxValue =  matches.Select(d => d.Value).Max();
                maxValue = Math.Round(maxValue, 6);
                return true;
            }
            maxValue = -1D;
            return false;
        }
    }

    public class DataPair
    {
        public string Key { get; set; }
        public double Value { get; set; }
    }
}
