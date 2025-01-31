using System;
using System.Collections.Generic;
using System.Linq;

namespace RealtimeFramework
{
    public class RealtimeAPI : IRealtimeAPI
    {
        public IEnumerable<string> GetAllHolidays()
        {
            foreach (string holiday in ModEntry.Assets.Holidays.Keys)
            {
                yield return holiday;
            }
        }

        public IEnumerable<string> GetComingHolidays()
        {
            foreach (string holiday in ModEntry.Assets.Holidays.Keys)
            {
                HolidayModel m = ModEntry.Assets.Holidays[holiday];
                DateTime start = GetStart(m, DateTime.Today.Year);

                if (DateTime.Now < start && DateTime.Now >= start.AddDays(-m.ComingDays))
                {
                    yield return holiday;
                }
                else
                {
                    // Check if we are coming up on this holiday next year
                    start = GetStart(m, DateTime.Today.Year + 1);
                    if (DateTime.Now < start && DateTime.Now >= start.AddDays(-m.ComingDays))
                    {
                        yield return holiday;
                    }
                }
            }
        }

        public IEnumerable<string> GetCurrentHolidays()
        {
            foreach (string holiday in ModEntry.Assets.Holidays.Keys)
            {
                HolidayModel m = ModEntry.Assets.Holidays[holiday];
                DateTime start = GetStart(m, DateTime.Today.Year);
                DateTime end = GetEnd(m, DateTime.Today.Year);

                if (DateTime.Now >= start && DateTime.Now <= end)
                {
                    yield return holiday;
                }                
            }
        }

        public IEnumerable<string> GetPassingHolidays()
        {
            foreach (string holiday in ModEntry.Assets.Holidays.Keys)
            {
                HolidayModel m = ModEntry.Assets.Holidays[holiday];
                DateTime end = GetEnd(m, DateTime.Today.Year);

                if (DateTime.Now > end && DateTime.Now <= end.AddDays(m.PassingDays))
                {
                    yield return holiday;
                }
                else
                {
                    // Check if we have passed this holiday in the previous year
                    end = GetEnd(m, DateTime.Today.Year - 1);
                    if (DateTime.Now > end && DateTime.Now <= end.AddDays(m.PassingDays))
                    {
                        yield return holiday;
                    }
                }
            }
        }

        private DateTime GetStart(HolidayModel m, int year)
        {
            return this.GetDate(m, year).AddHours(m.StartDelayHours);
        }

        private DateTime GetEnd(HolidayModel m, int year)
        {
            return this.GetDate(m, year).AddHours(24 + m.EndDelayHours);
        }

        private DateTime GetDate(HolidayModel m, int year)
        {
            int[] monthDay = m.Date;
            if (monthDay is null || monthDay.Length == 0)
            {
                monthDay = m.VaryingDates["" + year];
            }

            if (monthDay.Length != 2)
            {
                throw new ArgumentOutOfRangeException("m", "Included month/day value array without exactly 2 elements.");
            }

            return new DateTime(year, monthDay[0], monthDay[1], 0, 0, 0);
        }

        public string GetLocalName(string holiday)
        {
            return ModEntry.Instance.I18n.Get("holiday." + holiday);
        }

        public bool IsHoliday(string holiday)
        {
            return GetAllHolidays().Contains(holiday);
        }

        public bool IsComingHoliday(string holiday)
        {
            return GetComingHolidays().Contains(holiday);
        }

        public bool IsCurrentHoliday(string holiday)
        {
            return GetCurrentHolidays().Contains(holiday);
        }

        public bool IsPassingHoliday(string holiday)
        {
            return GetPassingHolidays().Contains(holiday);
        }
    }
}
