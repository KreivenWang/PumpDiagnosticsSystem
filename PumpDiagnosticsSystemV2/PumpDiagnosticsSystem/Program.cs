using System;
using System.Timers;
using PumpDiagnosticsSystem.Business;
using PumpDiagnosticsSystem.Datas;
using PumpDiagnosticsSystem.Util;

namespace PumpDiagnosticsSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            MainController.Initialze();
            MainController.RunProgramLoop();
            Console.ReadLine();
        }
    }
}
