using System.Collections.Generic;
using System.Linq;

namespace PumpDiagnosticsSystem.Core
{
    public class SpectrumAnalyser
    {
        public List<Spectrum> Specs { get; } = new List<Spectrum>();

        public Spectrum[][] BrPoses_Specs { get; private set; }

        public SpectrumAnalyser()
        {
        }

        public void UpdateSpecs(List<Spectrum> specs)
        {
            Specs.Clear();
            Specs.AddRange(specs);
            ClassifySpecsBs();
        }

        /// <summary>
        /// 从左到右:水泵非驱动端到电机非驱动端为0,1,2,3
        /// </summary>
        private void ClassifySpecsBs()
        {
            BrPoses_Specs = new Spectrum[4][];
            for (int i = 0; i < 4; i++) {
                BrPoses_Specs[i] = new Spectrum[3];
            }

            BrPoses_Specs[0][0] = Specs.FirstOrDefault(s => s.Pos.IsPumpOutX);
            BrPoses_Specs[0][1] = Specs.FirstOrDefault(s => s.Pos.IsPumpOutY);
            BrPoses_Specs[0][2] = Specs.FirstOrDefault(s => s.Pos.IsPumpOutZ);

            BrPoses_Specs[1][0] = Specs.FirstOrDefault(s => s.Pos.IsPumpInX);
            BrPoses_Specs[1][1] = Specs.FirstOrDefault(s => s.Pos.IsPumpInY);
            BrPoses_Specs[1][2] = Specs.FirstOrDefault(s => s.Pos.IsPumpInZ);

            BrPoses_Specs[2][0] = Specs.FirstOrDefault(s => s.Pos.IsMotorInX);
            BrPoses_Specs[2][1] = Specs.FirstOrDefault(s => s.Pos.IsMotorInY);
            BrPoses_Specs[2][2] = Specs.FirstOrDefault(s => s.Pos.IsMotorInZ);

            BrPoses_Specs[3][0] = Specs.FirstOrDefault(s => s.Pos.IsMotorOutX);
            BrPoses_Specs[3][1] = Specs.FirstOrDefault(s => s.Pos.IsMotorOutY);
            BrPoses_Specs[3][2] = Specs.FirstOrDefault(s => s.Pos.IsMotorOutZ);
        }

        private bool IsNearbySpec(Spectrum.Position pos1, Spectrum.Position pos2)
        {
            //电机2端
            if (pos1.CompPos == Spectrum.Position.Component.Motor &&
                pos2.CompPos == Spectrum.Position.Component.Motor &&
                pos1.DriverPos != pos2.DriverPos && //2端
                pos1.DirectionPos == pos2.DirectionPos ) { //xyz 相同
                return true;
            }

            //联轴器2端
            if (pos1.CompPos != pos2.CompPos && //水泵和电机 或者 电机和水泵
                pos1.DriverPos == Spectrum.Position.Driver.In && //都是驱动端
                pos2.DriverPos == Spectrum.Position.Driver.In &&
                pos1.DirectionPos == pos2.DirectionPos) { //xyz 相同
                return true;
            }

            //水泵2端
            if (pos1.CompPos == Spectrum.Position.Component.Pump &&
                pos2.CompPos == Spectrum.Position.Component.Pump &&
                pos1.DriverPos != pos2.DriverPos &&
                pos1.DirectionPos == pos2.DirectionPos) { //xyz 相同
                return true;
            }

            return false;
        }
    }
}
