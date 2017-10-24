namespace PumpDiagnosticsSystem.Core.Parser.Base
{
    public class ExpressionParserException : System.Exception
    {
        private string m_sMsg;
        private string m_sExpr;
        private string m_sTok;
        private int m_nPos;

        public ExpressionParserException(string sExpr, string sMsg, int nPos, string sTok)
        {
            m_sExpr = sExpr;
            m_sTok = sTok;
            m_nPos = nPos;
            m_sMsg = sMsg;
        }

        public string Expression
        {
            get
            {
                return m_sExpr;
            }
        }

        override public string Message
        {
            get
            {
                return m_sMsg;
            }
        }

        public string Token
        {
            get
            {
                return m_sTok;
            }
        }

        public int Position
        {
            get
            {
                return m_nPos;
            }
        }


    }
}