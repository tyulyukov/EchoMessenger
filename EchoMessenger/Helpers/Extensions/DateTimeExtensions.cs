using System;

namespace EchoMessenger.Helpers.Extensions
{
    public static class DateTimeExtensions
    {
        public static bool IsToday(this DateTime dateTime) => dateTime.Date == DateTime.Today;

        public static bool IsThisYear(this DateTime dateTime) => DateTime.Now.Year == dateTime.Year;

        public static bool IsOlderOnTime(this DateTime dateTime, DateTime comparingDateTime, TimeSpan time) => comparingDateTime.AddTicks(time.Ticks) < dateTime;
    }
}
