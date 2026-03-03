namespace vn.edu.fpt.Utilities
{
    public static class DateTimeHelper
    {
        private static readonly TimeZoneInfo VietnamTimeZone = ResolveVietnamTimeZone();

        public static DateTime NowVietnam =>
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamTimeZone);

        public static DateTime ToVietnamTime(DateTime dateTime)
        {
            var utcTime = dateTime.Kind switch
            {
                DateTimeKind.Utc => dateTime,
                DateTimeKind.Local => dateTime.ToUniversalTime(),
                _ => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
            };

            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, VietnamTimeZone);
        }

        private static TimeZoneInfo ResolveVietnamTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            }
        }
    }
}

