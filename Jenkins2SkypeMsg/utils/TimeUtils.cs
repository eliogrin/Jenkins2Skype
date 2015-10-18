using System;

namespace Jenkins2SkypeMsg.utils
{
    class TimeUtils
    {
        public static String getCurrentDate()
        {
            DateTime dateTime = DateTime.Now.Date;
            return dateTime.ToString("yyyy-MM-dd");
        }

        public static String getCurrentTime()
        {
            DateTime dateTime = DateTime.Now;
            return dateTime.ToString("hh:mm:ss");
        }

        public static long getTimestamp(DateTime fromTime)
        {
            fromTime = TimeZoneInfo.ConvertTimeToUtc(fromTime);
            long result = (fromTime.Ticks - DateTime.Parse("01/01/1970 00:00:00").Ticks) / 10000;
            return result;
        }

        public static long getTimestamp()
        {
            return (DateTime.UtcNow.Ticks - DateTime.Parse("01/01/1970 00:00:00").Ticks) / 10000;
        }

        public static long getTimestamp(String time, int dataChange)
        {
            String[] timeModel = time.Split(':');
            int hour = Convert.ToInt32(timeModel[0]);
            int minute = Convert.ToInt32(timeModel[1]);

            DateTime timeForStamp = DateTime.Now;
            TimeSpan ts = new TimeSpan(hour, minute, 00);
            timeForStamp = timeForStamp.AddDays(dataChange);
            timeForStamp = timeForStamp.Date + ts;
            timeForStamp = TimeZoneInfo.ConvertTimeToUtc(timeForStamp);
            long result = (timeForStamp.Ticks - DateTime.Parse("01/01/1970 00:00:00").Ticks) / 10000;
            return result;
        }

        public static long getTimestamp(String timeMap)
        {
            String[] timeSplit = timeMap.Split('|');
            int days = 0;
            String time;
            if (timeSplit.Length > 1)
            {
                days = Convert.ToInt16(timeSplit[0]);
                time = timeSplit[1]; 
            }
            else
            {
                time = timeSplit[0];
            }
            return getTimestamp(time, days);
        }

        public static String convertTimestampToTime(long timestamp)
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            epoch = epoch.AddMilliseconds(timestamp);
            epoch = TimeZoneInfo.ConvertTimeFromUtc(epoch, TimeZoneInfo.Local);
            return epoch.ToString("HH':'mm");
        }

        public static int timestampToSeconds(long timestamp)
        {
            return Convert.ToInt32(timestamp / 1000);
        }

        public static long getSeconsToTime(String timeToGo)
        {
            long result;
            String[] timeModel = timeToGo.Split(':');
            int hour = Convert.ToInt32(timeModel[0]);
            int minute = Convert.ToInt32(timeModel[1]);

            DateTime current = DateTime.Now;
            TimeSpan miliseconds = new TimeSpan(hour, minute, 00) - current.TimeOfDay;
            if (miliseconds.TotalSeconds <= 0)
            {
                miliseconds = new TimeSpan(hour + 24, minute, 00) - current.TimeOfDay;
            }
            result = Convert.ToInt64(miliseconds.TotalSeconds);
            return result;
        }

        public static String convertMsToTime(long ms)
        {
            TimeSpan time = TimeSpan.FromMilliseconds(ms);
            if (time.Days > 0)
                return "all day";
            if (time.Hours > 0)
                return time.ToString("h' hours and 'mm' minutes'");
            if (time.Minutes > 0)
                return time.ToString("mm' minutes'");
            if (time.Seconds > 0)
                return time.ToString("ss' seconds'");
            return "zero time";
        }

        public static String getCurrentWeek()
        {
            return DateTime.Now.ToString("ddd");
        }

        public static String convertMoveDay(int day)
        {
            String moveDay = "";
            if (day < 0) day *= -1;
            switch (day)
            {
                case 0:
                    moveDay = "today";
                    break;
                case 1:
                    moveDay = "yesterday";
                    break;
                default:
                    moveDay = String.Format("{0} days ago", day);
                    break;
            }
            return moveDay;
        }
    }
}
