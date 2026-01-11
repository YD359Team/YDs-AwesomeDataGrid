using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace YDs_AwesomeDataGrid.Managers
{
    internal static class FontManager
    {
        public static Font ModernCommon => new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        public static Font ModernTitle => new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
    }
}
