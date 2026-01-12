using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace YDs_AwesomeDataGrid.Helpers
{
    internal static class MathHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Clamp(int value, int min, int max)
        {
            if (min > max)
                return min;

            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
