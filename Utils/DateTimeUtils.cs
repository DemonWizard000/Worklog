namespace WorkLog.Utils
{
    public class DateTimeUtils
    {
        public DateTime startTimeOfDate(DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, 0);
        }
        public DateTime endTimeOfDate(DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 23, 59, 59, 59);
        }
    }
}
