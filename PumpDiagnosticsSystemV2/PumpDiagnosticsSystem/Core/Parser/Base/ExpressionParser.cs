using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using PumpDiagnosticsSystem.Dbs;
using PumpDiagnosticsSystem.Util;

namespace PumpDiagnosticsSystem.Core.Parser.Base
{
    /// <summary>
    /// 表达式解析器
    /// </summary>
    public partial class ExpressionParser
    {
        private const string MuParserPath = @"Assets\muParser.dll";

        private IntPtr m_parser = IntPtr.Zero;

        // Keep the delegate in order to prevent deletion
        private List<Delegate> m_binOprtDelegates = new List<Delegate>();

        // Keep the delegate in order to prevent deletion
        private List<Delegate> m_funDelegates = new List<Delegate>();

        // Buffer with all parser variables
        private Dictionary<string, Variable> m_varBuf = new Dictionary<string, Variable>();

        // Keep reference to the delegate of the error function
        private ErrorDelegate m_errCallback;

        //------------------------------------------------------------------------------
        public enum EPrec : int
        {
            // binary operators
            prLOGIC = 1,
            prCMP = 2,
            prADD_SUB = 3,
            prMUL_DIV = 4,
            prPOW = 5,

            // infix operators
            prINFIX = 4,
            prPOSTFIX = 4
        }

        public enum ParseDataType
        {
            DOUBLE = 0,
            INT = 1
        }

        //---------------------------------------------------------------------------
        // Delegates
        //---------------------------------------------------------------------------
        #region Delegate definitions

        //[UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        //protected delegate IntPtr FactoryDelegate(String name, IntPtr parser);

        // Value identification callback
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int IdentFunDelegate(String name, ref int pos, ref double val);

        // Callback for errors 
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        protected delegate void ErrorDelegate();

        // Functions taking double arguments
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate double Fun1Delegate(double val1);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate double Fun2Delegate(double val1, double val2);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate double Fun3Delegate(double val1, double val2, double val3);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate double Fun4Delegate(double val1, double val2, double val3, double val4);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate double Fun5Delegate(double val1, double val2, double val3, double val4, double val5);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate double Fun6Delegate(double val1, double val2, double val3, double val4, double val5, double val6);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate double Fun7Delegate(double val1, double val2, double val3, double val4, double val5, double val6, double val7);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate double Fun8Delegate(double val1, double val2, double val3, double val4, double val5, double val6, double val7, double val8);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate double Fun9Delegate(double val1, double val2, double val3, double val4, double val5, double val6, double val7, double val8, double val9);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate double Fun10Delegate(double val1, double val2, double val3, double val4, double val5, double val6, double val7, double val8, double val9, double val10);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate double BulkFun1Delegate(int nBulkIdx, int nThreadIdx, double val1);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate double BulkFun2Delegate(int nBulkIdx, int nThreadIdx, double val1, double val2);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate double BulkFun3Delegate(int nBulkIdx, int nThreadIdx, double val1, double val2, double val3);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate double BulkFun4Delegate(int nBulkIdx, int nThreadIdx, double val1, double val2, double val3, double val4);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate double BulkFun5Delegate(int nBulkIdx, int nThreadIdx, double val1, double val2, double val3, double val4, double val5);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate double BulkFun6Delegate(int nBulkIdx, int nThreadIdx, double val1, double val2, double val3, double val4, double val5, double val6);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate double BulkFun7Delegate(int nBulkIdx, int nThreadIdx, double val1, double val2, double val3, double val4, double val5, double val6, double val7);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate double BulkFun8Delegate(int nBulkIdx, int nThreadIdx, double val1, double val2, double val3, double val4, double val5, double val6, double val7, double val8);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate double BulkFun9Delegate(int nBulkIdx, int nThreadIdx, double val1, double val2, double val3, double val4, double val5, double val6, double val7, double val8, double val9);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate double BulkFun10Delegate(int nBulkIdx, int nThreadIdx, double val1, double val2, double val3, double val4, double val5, double val6, double val7, double val8, double val9, double val10);

        // Functions taking an additional string parameter
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate double StrFun1Delegate(String name);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate double StrFun2Delegate(String name, double val1);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate double StrFun3Delegate(String name, double val1, double val2);

        // Functions taking an additional string parameter
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate double MultFunDelegate(
                    [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] double[] array,
                    int size);

        #endregion

        //---------------------------------------------------------------------------
        // Parser methode wrappers
        //---------------------------------------------------------------------------
        #region Parser methode wrappers

        /// <summary>
        /// 缺省情况下使用双精度型数据类型解析数据
        /// </summary>
        public ExpressionParser()
        {
            m_parser = mupCreate((int)ParseDataType.DOUBLE);
            Debug.Assert(m_parser != null, "Parser object is null");
            m_errCallback = RaiseException;
            mupSetErrorHandler(m_parser, m_errCallback);
            DefineInfixOprt("!", Not, EPrec.prLOGIC);
            DefineFun("ABS", ABS, true);
            DefineFun("rint", rint, true);
            RegisterFuncs();
        }

        private double rint(double val1)
        {
            return (int)val1;
        }

        private double ABS(double val1)
        {
            return Math.Abs(val1);
        }

        private double Not(double val1)
        {
            return val1 <= 0 ? 1 : 0;
        }

        ~ExpressionParser()
        {
            mupRelease(m_parser);
        }

        protected void RaiseException()
        {
            string s = GetExpr();

            ExpressionParserException exc = new ExpressionParserException(GetExpr(),
                                                      GetErrorMsg(),
                                                      mupGetErrorPos(m_parser),
                                                      GetErrorToken());
            //throw exc;
            
            //Log.Inform(exc.Token);
        }

        public void AddValIdent(IdentFunDelegate fun)
        {
            mupAddValIdent(m_parser, fun);
        }

        public void SetExpression(string expr)
        {
            mupSetExpr(m_parser, expr);
        }

        public string GetVersion()
        {
            return Marshal.PtrToStringAnsi(mupGetVersion(m_parser));
        }

        private string GetErrorMsg()
        {
            return Marshal.PtrToStringAnsi(mupGetErrorMsg(m_parser));
        }

        private string GetErrorToken()
        {
            return Marshal.PtrToStringAnsi(mupGetErrorToken(m_parser));
        }

        public string GetExpr()
        {
            return Marshal.PtrToStringAnsi(mupGetExpr(m_parser));
        }
        object thislock = new object();
        public double Eval()
        {
            double result;
            lock (thislock)
            {
                result = mupEval(m_parser);
            }
            return result;
        }

        public double[] EvalMultiExpr()
        {
            int nNum;
            IntPtr p = mupEvalMulti(m_parser, out nNum);
            double[] array = new double[nNum];
            Marshal.Copy(p, array, 0, nNum);
            return array;
        }

        public void Eval(double[] results, int nSize)
        {
            mupEvalBulk(m_parser, results, nSize);
        }

        public void DefineConst(string name, double val)
        {
            mupDefineConst(m_parser, name, val);
        }

        public void DefineStrConst(string name, String str)
        {
            mupDefineStrConst(m_parser, name, str);
        }

        public void DefineVar(string name, double[] var)
        {
            mupDefineBulkVar(m_parser, name, var);
        }

        public void DefineVar(string name, Variable var)
        {
            mupDefineVar(m_parser, name, var.Pointer);
            m_varBuf[name] = var;
        }

        public void RemoveVar(string name)
        {
            mupRemoveVar(m_parser, name);
            m_varBuf.Remove(name);
        }

        public void ClearVar()
        {
            mupClearVar(m_parser);
        }

        public void ClearConst()
        {
            mupClearConst(m_parser);
        }

        public void ClearOprt()
        {
            m_binOprtDelegates.Clear();
            mupClearOprt(m_parser);
        }

        public void ClearFun()
        {
            m_funDelegates.Clear();
            mupClearFun(m_parser);
        }

        #region define numeric functions with fixed number of arguments

        public void DefineFun(string name, Fun1Delegate function, bool bAllowOptimization)
        {
            m_funDelegates.Add(function);
            mupDefineFun1(m_parser, name, function, (bAllowOptimization) ? 1 : 0);
        }

        public void DefineFun(string name, Fun2Delegate function, bool bAllowOptimization)
        {
            m_funDelegates.Add(function);
            mupDefineFun2(m_parser, name, function, (bAllowOptimization) ? 1 : 0);
        }

        public void DefineFun(string name, Fun3Delegate function, bool bAllowOptimization)
        {
            m_funDelegates.Add(function);
            mupDefineFun3(m_parser, name, function, (bAllowOptimization) ? 1 : 0);
        }

        public void DefineFun(string name, Fun4Delegate function, bool bAllowOptimization)
        {
            m_funDelegates.Add(function);
            mupDefineFun4(m_parser, name, function, (bAllowOptimization) ? 1 : 0);
        }

        public void DefineFun(string name, Fun5Delegate function, bool bAllowOptimization)
        {
            m_funDelegates.Add(function);
            mupDefineFun5(m_parser, name, function, (bAllowOptimization) ? 1 : 0);
        }

        public void DefineFun(string name, Fun6Delegate function, bool bAllowOptimization)
        {
            m_funDelegates.Add(function);
            mupDefineFun6(m_parser, name, function, (bAllowOptimization) ? 1 : 0);
        }

        public void DefineFun(string name, Fun7Delegate function, bool bAllowOptimization)
        {
            m_funDelegates.Add(function);
            mupDefineFun7(m_parser, name, function, (bAllowOptimization) ? 1 : 0);
        }

        public void DefineFun(string name, Fun8Delegate function, bool bAllowOptimization)
        {
            m_funDelegates.Add(function);
            mupDefineFun8(m_parser, name, function, (bAllowOptimization) ? 1 : 0);
        }

        public void DefineFun(string name, Fun9Delegate function, bool bAllowOptimization)
        {
            m_funDelegates.Add(function);
            mupDefineFun9(m_parser, name, function, (bAllowOptimization) ? 1 : 0);
        }

        public void DefineFun(string name, Fun10Delegate function, bool bAllowOptimization)
        {
            m_funDelegates.Add(function);
            mupDefineFun10(m_parser, name, function, (bAllowOptimization) ? 1 : 0);
        }

        #endregion

        #region Defining bulk mode functions

        public void DefineFun(string name, BulkFun1Delegate function)
        {
            m_funDelegates.Add(function);
            mupDefineBulkFun1(m_parser, name, function);
        }

        public void DefineFun(string name, BulkFun2Delegate function)
        {
            m_funDelegates.Add(function);
            mupDefineBulkFun2(m_parser, name, function);
        }

        public void DefineFun(string name, BulkFun3Delegate function)
        {
            m_funDelegates.Add(function);
            mupDefineBulkFun3(m_parser, name, function);
        }

        public void DefineFun(string name, BulkFun4Delegate function)
        {
            m_funDelegates.Add(function);
            mupDefineBulkFun4(m_parser, name, function);
        }

        public void DefineFun(string name, BulkFun5Delegate function)
        {
            m_funDelegates.Add(function);
            mupDefineBulkFun5(m_parser, name, function);
        }

        public void DefineFun(string name, BulkFun6Delegate function)
        {
            m_funDelegates.Add(function);
            mupDefineBulkFun6(m_parser, name, function);
        }

        public void DefineFun(string name, BulkFun7Delegate function)
        {
            m_funDelegates.Add(function);
            mupDefineBulkFun7(m_parser, name, function);
        }

        public void DefineFun(string name, BulkFun8Delegate function)
        {
            m_funDelegates.Add(function);
            mupDefineBulkFun8(m_parser, name, function);
        }

        public void DefineFun(string name, BulkFun9Delegate function)
        {
            m_funDelegates.Add(function);
            mupDefineBulkFun9(m_parser, name, function);
        }

        public void DefineFun(string name, BulkFun10Delegate function)
        {
            m_funDelegates.Add(function);
            mupDefineBulkFun10(m_parser, name, function);
        }

        #endregion

        #region define other functions

        public void DefineFun(string name, StrFun1Delegate function)
        {
            m_funDelegates.Add(function);
            mupDefineStrFun1(m_parser, name, function);
        }

        public void DefineFun(string name, StrFun2Delegate function)
        {
            m_funDelegates.Add(function);
            mupDefineStrFun2(m_parser, name, function);
        }

        public void DefineFun(string name, StrFun3Delegate function)
        {
            m_funDelegates.Add(function);
            mupDefineStrFun3(m_parser, name, function);
        }

        public void DefineFun(string name, MultFunDelegate function, bool bAllowOptimization)
        {
            m_funDelegates.Add(function);
            mupDefineMultFun(m_parser, name, function, (bAllowOptimization) ? 1 : 0);
        }

        #endregion

        #region define operators

        public void DefineOprt(string name, Fun2Delegate function, int precedence)
        {
            m_binOprtDelegates.Add(function);
            mupDefineOprt(m_parser, name, function, precedence, 0);
        }

        public void DefinePostfixOprt(string name, Fun1Delegate oprt)
        {
            m_binOprtDelegates.Add(oprt);
            mupDefinePostfixOprt(m_parser, name, oprt, 0);
        }

        public void DefineInfixOprt(string name, Fun1Delegate oprt, EPrec precedence)
        {
            m_binOprtDelegates.Add(oprt);
            mupDefineInfixOprt(m_parser, name, oprt, 0);
        }

        #endregion

        public Dictionary<string, double> GetConst()
        {
            int num = mupGetConstNum(m_parser);

            Dictionary<string, double> map = new Dictionary<string, double>();
            for (int i = 0; i < num; ++i)
            {
                string name = "";
                double value = 0;
                mupGetConst(m_parser, i, ref name, ref value);

                map[name] = value;
            }

            return map;
        }

        public Dictionary<string, Variable> GetVar()
        {
            return m_varBuf;
        }

        public Dictionary<string, IntPtr> GetExprVar()
        {
            int num = mupGetExprVarNum(m_parser);

            Dictionary<string, IntPtr> map = new Dictionary<string, IntPtr>();
            for (int i = 0; i < num; ++i)
            {
                string name = "";
                IntPtr ptr = IntPtr.Zero;
                mupGetExprVar(m_parser, i, ref name, ref ptr);

                map[name] = ptr;
            }

            return map;
        }

        public void SetArgumentsSeparator(char cArgSep)
        {
            mupSetArgSep(m_parser, Convert.ToByte(cArgSep));
        }

        public void SetDecimalSeparator(char cDecSep)
        {
            mupSetDecSep(m_parser, Convert.ToByte(cDecSep));
        }

        public void SetThousandsSep(char cThSep)
        {
            mupSetThousandsSep(m_parser, Convert.ToByte(cThSep));
        }

        public void ResetLocale()
        {
            mupResetLocale(m_parser);
        }
        #endregion


        #region DLL function bindings

        //----------------------------------------------------------
        // Basic operations / initialization  
        //----------------------------------------------------------

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern IntPtr mupCreate(int nType);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupRelease(IntPtr a_pParser);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupResetLocale(IntPtr a_pParser);

        // Achtung Marshalling von Classen, die strings zur點kgeben funktioniert 黚er IntPtr
        // weil C# den string sonst freigeben wird!
        //
        // siehe auch:
        // http://discuss.fogcreek.com/dotnetquestions/default.asp?cmd=show&ixPost=1108
        // http://groups.google.com/group/microsoft.public.dotnet.framework/msg/9807f3b190c31f6d
        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern IntPtr mupGetVersion(IntPtr a_pParser);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern IntPtr mupGetExpr(IntPtr a_pParser);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern IntPtr mupGetErrorMsg(IntPtr a_pParser);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern IntPtr mupGetErrorToken(IntPtr a_pParser);
        // ende

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupSetExpr(IntPtr a_pParser, string a_szExpr);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupSetErrorHandler(IntPtr a_pParser, ErrorDelegate errFun);

        //---------------------------------------------------------------------------
        // Non numeric callbacks
        //---------------------------------------------------------------------------

        //[DllImport(MuParserPath)]
        //protected static extern void mupSetVarFactory(HandleRef a_pParser, muFacFun_t a_pFactory, void* pUserData);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupAddValIdent(IntPtr a_parser, IdentFunDelegate fun);

        //----------------------------------------------------------
        // Defining variables and constants
        //----------------------------------------------------------

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupDefineConst(IntPtr a_pParser,
                                                     string a_szName,
                                                     double a_fVal);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupDefineStrConst(IntPtr parser,
                                                        string name,
                                                        string val);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupDefineVar(IntPtr parser,
                                                  string name,
                                                  IntPtr var);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupDefineBulkVar(IntPtr parser,
                                                      string name,
                                                      double[] var);

        //----------------------------------------------------------
        // Querying variables / expression variables / constants
        //----------------------------------------------------------

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern int mupGetExprVarNum(IntPtr a_parser);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupGetExprVar(IntPtr a_parser,
                                                   int idx,
                                                   ref string name,
                                                   ref IntPtr ptr);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern int mupGetVarNum(IntPtr a_parser);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupGetVar(IntPtr a_parser,
                                               int idx,
                                               ref string name,
                                               ref IntPtr ptr);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern int mupGetConstNum(IntPtr a_parser);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupGetConst(IntPtr a_parser,
                                                  int idx,
                                                  ref string str,
                                                  ref double value);

        //[DllImport(MuParserPath)]
        //protected static extern void mupGetExprVar(IntPtr a_parser, unsigned a_iVar, const muChar_t** a_pszName, muFloat_t** a_pVar);

        //----------------------------------------------------------
        // Remove all / single variables
        //----------------------------------------------------------

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupRemoveVar(IntPtr a_parser, string name);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupClearVar(IntPtr a_parser);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupClearConst(IntPtr a_parser);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupClearOprt(IntPtr a_parser);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupClearFun(IntPtr a_parser);

        //----------------------------------------------------------
        // Define character sets for identifiers
        //----------------------------------------------------------

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupDefineNameChars(IntPtr a_parser, string charset);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupDefineOprtChars(IntPtr a_parser, string charset);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupDefineInfixOprtChars(IntPtr a_parser, string charset);

        //----------------------------------------------------------
        // Defining callbacks / variables / constants
        //----------------------------------------------------------

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupDefineFun1(IntPtr a_parser, string name, Fun1Delegate fun, int optimize);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupDefineFun2(IntPtr a_parser, string name, Fun2Delegate fun, int optimize);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupDefineFun3(IntPtr a_parser, string name, Fun3Delegate fun, int optimize);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupDefineFun4(IntPtr a_parser, string name, Fun4Delegate fun, int optimize);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupDefineFun5(IntPtr a_parser, string name, Fun5Delegate fun, int optimize);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupDefineFun6(IntPtr a_parser, string name, Fun6Delegate fun, int optimize);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupDefineFun7(IntPtr a_parser, string name, Fun7Delegate fun, int optimize);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupDefineFun8(IntPtr a_parser, string name, Fun8Delegate fun, int optimize);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupDefineFun9(IntPtr a_parser, string name, Fun9Delegate fun, int optimize);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupDefineFun10(IntPtr a_parser, string name, Fun10Delegate fun, int optimize);

        // Bulk mode functions
        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupDefineBulkFun1(IntPtr a_parser, string name, BulkFun1Delegate fun);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupDefineBulkFun2(IntPtr a_parser, string name, BulkFun2Delegate fun);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupDefineBulkFun3(IntPtr a_parser, string name, BulkFun3Delegate fun);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupDefineBulkFun4(IntPtr a_parser, string name, BulkFun4Delegate fun);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupDefineBulkFun5(IntPtr a_parser, string name, BulkFun5Delegate fun);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupDefineBulkFun6(IntPtr a_parser, string name, BulkFun6Delegate fun);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupDefineBulkFun7(IntPtr a_parser, string name, BulkFun7Delegate fun);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupDefineBulkFun8(IntPtr a_parser, string name, BulkFun8Delegate fun);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupDefineBulkFun9(IntPtr a_parser, string name, BulkFun9Delegate fun);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupDefineBulkFun10(IntPtr a_parser, string name, BulkFun10Delegate fun);

        // string functions
        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupDefineStrFun1(IntPtr a_parser, string name, StrFun1Delegate fun);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupDefineStrFun2(IntPtr a_parser, string name, StrFun2Delegate fun);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupDefineStrFun3(IntPtr a_parser, string name, StrFun3Delegate fun);

        // Multiple argument functions
        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupDefineMultFun(IntPtr a_parser, string name, MultFunDelegate fun, int optimize);

        //----------------------------------------------------------
        // Operator definitions
        //----------------------------------------------------------

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupDefineOprt(IntPtr a_pParser,
                                                   string name,
                                                   Fun2Delegate fun,
                                                   int precedence,
                                                   int optimize);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupDefinePostfixOprt(IntPtr a_pParser,
                                                          string id,
                                                          Fun1Delegate fun,
                                                          int optimize);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupDefineInfixOprt(IntPtr a_pParser,
                                                        string id,
                                                        Fun1Delegate fun,
                                                        int optimize);

        //----------------------------------------------------------
        // 
        //----------------------------------------------------------

        [DllImport(MuParserPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        protected static extern double mupEval(IntPtr a_pParser);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern double mupEvalBulk(IntPtr a_pParser, double[] results, int nBulkSize);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern IntPtr mupEvalMulti(IntPtr a_pParser, out int nSize);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern int mupError(IntPtr a_pParser);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupErrorReset(IntPtr a_pParser);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern int mupGetErrorCode(IntPtr a_pParser);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern int mupGetErrorPos(IntPtr a_pParser);

        //----------------------------------------------------------
        // Localization
        //----------------------------------------------------------

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupSetArgSep(IntPtr a_pParser, byte cArgSep);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupSetDecSep(IntPtr a_pParser, byte cArgSep);

        [DllImport(MuParserPath, CallingConvention = CallingConvention.Cdecl)]
        protected static extern void mupSetThousandsSep(IntPtr a_pParser, byte cArgSep);

        #endregion
    }
}
