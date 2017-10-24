using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PumpDiagnosticsSystem.Models
{
    [Serializable]
    public class Component : BaseComponent
    {
        public Component(Guid guid, CompType type) : base(guid, type)
        {
//            BuildFMEATrees();
        }
    }
}
