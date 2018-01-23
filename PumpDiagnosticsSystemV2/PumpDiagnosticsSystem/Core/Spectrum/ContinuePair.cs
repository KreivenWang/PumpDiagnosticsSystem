using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PumpDiagnosticsSystem.Core
{
    public class ContinuePair
    {
        public int Head { get; set; }
        public int Tail { get; set; }

        public int NotContinueCount { get; set; }

        public int Width => Tail - Head + 1;

        public bool HasIn { get; set; }

        public ContinuePair(int head, int tail)
        {
            Debug.Assert(tail > head);
            Head = head;
            Tail = tail;
            NotContinueCount = 0;
        }

        public bool Contains(int index)
        {
            return Head <= index && Tail >= index;
        }

        public void InsertPair(ContinuePair frontPair)
        {
            Debug.Assert(frontPair.Tail < Head);

        }

        public void AppendPair(ContinuePair behindPair)
        {
            Debug.Assert(behindPair.Head > Tail);
            behindPair.HasIn = true;
            NotContinueCount = behindPair.Head - Tail;
            Tail = behindPair.Tail;
        }
    }
}
