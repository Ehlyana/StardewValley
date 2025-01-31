using System.Collections.Generic;
using BirbShared.Asset;

namespace RealtimeFramework
{
    [AssetClass]
    class Assets
    {
        [AssetProperty("assets/holidays.json")]
        public Dictionary<string, HolidayModel> Holidays { get; set; }
    }

    class HolidayModel
    {
        public int ComingDays { get; set; } = 7;
        public int PassingDays { get; set; } = 1;

        public int StartDelayHours { get; set; } = 0;

        public int EndDelayHours { get; set; } = 0;

        public int[] Date { get; set; } = { 1, 1 };
        public Dictionary<string, int[]> VaryingDates { get; set; } = null;
    }
}
