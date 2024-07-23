using System;
using System.Globalization;

namespace Web_App.Extensions
{
    public static class DateTimeExtensions
    {
        public static bool IsInSameWeek(this DateTime date1, DateTime date2)
        {
            return date1.Date.AddDays(-1 * (int)DateTimeFormatInfo.CurrentInfo.Calendar.GetDayOfWeek(date1)) == 
                date2.Date.AddDays(-1 * (int)DateTimeFormatInfo.CurrentInfo.Calendar.GetDayOfWeek(date2));
        }

        public static bool IsBetween(this DateTime date1, DateTime date2, DateTime date3)
        {
            return date1 > date2 && date1 < date3;
        }
    }
}