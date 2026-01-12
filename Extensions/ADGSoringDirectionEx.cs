using System;
using System.Collections.Generic;
using System.Text;
using YDs_AwesomeDataGrid.Enums;

namespace YDs_AwesomeDataGrid.Extensions
{
    internal static class ADGSoringDirectionEx
    {
        public static ADGSortingDirection GetNextClickState(this ADGSortingDirection currentSortingDir)
        {
            if (currentSortingDir == ADGSortingDirection.None)
                return currentSortingDir = ADGSortingDirection.Ascending;
            if (currentSortingDir == ADGSortingDirection.Ascending)
                return currentSortingDir = ADGSortingDirection.Descending;
            if (currentSortingDir == ADGSortingDirection.Descending)
                return currentSortingDir = ADGSortingDirection.Ascending;
            return currentSortingDir = ADGSortingDirection.None;
        }
    }
}
