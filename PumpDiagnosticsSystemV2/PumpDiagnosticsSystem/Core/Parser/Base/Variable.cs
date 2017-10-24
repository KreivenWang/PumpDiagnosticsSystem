using System;
using System.Runtime.InteropServices;

namespace PumpDiagnosticsSystem.Core.Parser.Base
{
    public class Variable
    {
        private const string MuParserPath = @"Assets\muParser.dll";

        public IntPtr Pointer
        {
            get { return m_pVar; }
        }

        private IntPtr m_pVar;

        public unsafe double Value
        {
            get { return *((double*) m_pVar.ToPointer()); }

            set { *((double*) m_pVar.ToPointer()) = value; }
        }

        public unsafe Variable()
        {
            m_pVar = mupCreateVar();
            *((double*) m_pVar.ToPointer()) = 0;
        }

        public unsafe Variable(double val)
        {
            m_pVar = mupCreateVar();
            *((double*) m_pVar.ToPointer()) = val;
        }

        ~Variable()
        {
            mupReleaseVar(m_pVar);
        }

        #region DLL imports

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern IntPtr mupCreateVar();

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern IntPtr mupReleaseVar(IntPtr var);

        #endregion
    }
}