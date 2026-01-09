using System;
using System.Collections.Generic;
using System.Text;

namespace YDs_AwesomeDataGrid.Helpers
{
    internal static class MathHelper
    {
        public static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
