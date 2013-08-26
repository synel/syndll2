using System;
using System.Globalization;

namespace Syndll2.Data
{
    public abstract class ClockTransition
    {
        public int HoursToChange { get; protected set; }

        public static ClockTransition Parse(string value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            if (value.Length != 13)
                throw new ArgumentException("Clock transitions should be 13 characters in length.");

            switch (value[0])
            {
                case '1':
                    return FixedClockTransition.Parse(value);
                case '2':
                    return DynamicClockTransition.Parse(value);
                default:
                    throw new ArgumentException("Invalid clock transition type.");
            }
        }
    }

    public sealed class FixedClockTransition : ClockTransition
    {
        public DateTime TransitionDateTime { get; private set; }

        public FixedClockTransition(int hoursToChange, DateTime transitionDateTime)
        {
            if (transitionDateTime.Kind != DateTimeKind.Unspecified)
                throw new ArgumentException("The date should have unspecified kind.  It is the local date/time of the transition.");

            if (hoursToChange != -1 && hoursToChange != 1)
                throw new ArgumentOutOfRangeException("hoursToChange", "The change should be -1 or +1 hours.");

            TransitionDateTime = transitionDateTime;
            HoursToChange = hoursToChange;
        }
        
        public override string ToString()
        {
            return string.Format("1{0:ddMMyyHHmm}{1:+0;-0}", TransitionDateTime, HoursToChange);
        }

        public static new FixedClockTransition Parse(string value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            if (value.Length != 13)
                throw new ArgumentException("Clock transitions should be 13 characters in length.");

            if (value[0] != '1')
                throw new ArgumentException("Fixed clock transitions should start with a 1.");

            DateTime transitionDatetime;
            if (!DateTime.TryParseExact(value.Substring(1, 10), "ddMMyyHHmm", CultureInfo.InvariantCulture, DateTimeStyles.None, out transitionDatetime))
                throw new ArgumentException("Error parsing date/time for fixed clock transition.");

            int hourstoChange;
            if (!int.TryParse(value.Substring(11, 2), out hourstoChange))
                throw new ArgumentException("Error parsing hours to change for fixed clock transition.");

            return new FixedClockTransition(hourstoChange, transitionDatetime);
        }
    }

    public sealed class DynamicClockTransition : ClockTransition
    {
        public int Month { get; private set; }
        public DayOfWeek DayOfWeek { get; private set; }
        public WeekOfMonth WeekOfMonth { get; private set; }
        public TimeSpan TransitionTimeOfDay { get; private set; }

        public DynamicClockTransition(int hoursToChange, WeekOfMonth weekOfMonth, DayOfWeek dayOfWeek, int month, TimeSpan transitionTimeOfDay)
        {
            if (month < 1 || month > 12)
                throw new ArgumentOutOfRangeException("month", "Invalid month.");

            if (!Enum.IsDefined(typeof(DayOfWeek), dayOfWeek))
                throw new ArgumentException("Invalid day of week.", "dayOfWeek");

            if (!Enum.IsDefined(typeof(WeekOfMonth), weekOfMonth))
                throw new ArgumentException("Invalid week of month.", "weekOfMonth");

            if (transitionTimeOfDay < TimeSpan.Zero || transitionTimeOfDay.Days > 0)
                throw new ArgumentOutOfRangeException("transitionTimeOfDay", "The transition timespan must be a valid time of day.");

            if (hoursToChange != -1 && hoursToChange != 1)
                throw new ArgumentOutOfRangeException("hoursToChange", "The change should be -1 or +1 hours.");

            Month = month;
            DayOfWeek = dayOfWeek;
            WeekOfMonth = weekOfMonth;
            TransitionTimeOfDay = transitionTimeOfDay;
            HoursToChange = hoursToChange;
        }

        public override string ToString()
        {
            return string.Format("2{0:D2}0{1}0{2}{3:hhmm}{4:+0;-0}",
                Month,
                (int) DayOfWeek,
                WeekOfMonth == WeekOfMonth.Last ? "L" : ((int) WeekOfMonth).ToString(CultureInfo.InvariantCulture),
                TransitionTimeOfDay,
                HoursToChange);
        }

        public static new DynamicClockTransition Parse(string value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            if (value.Length != 13)
                throw new ArgumentException("Clock transitions should be 13 characters in length.");

            if (value[0] != '2')
                throw new ArgumentException("Dynamic clock transitions should start with a 2.");

            int month;
            if (!int.TryParse(value.Substring(1,2), out month) || month < 1 || month > 12)
                throw new ArgumentException("Error parsing month for dynamic clock transition.");

            DayOfWeek dayOfWeek;
            if (!Enum.TryParse(value.Substring(4,1), out dayOfWeek))
                throw new ArgumentException("Error parsing day of week for dynamic clock transition.");

            WeekOfMonth weekOfMonth;
            if (value.Substring(6,1) == "L")
                weekOfMonth = WeekOfMonth.Last;
            else if (!Enum.TryParse(value.Substring(6, 1), out weekOfMonth))
                throw new ArgumentException("Error parsing week of month for dynamic clock transition.");

            TimeSpan transitionTimeOfDay;
            if (!TimeSpan.TryParseExact(value.Substring(7,4), "hhmm", CultureInfo.InvariantCulture, out transitionTimeOfDay))
                throw new ArgumentException("Error parsing transition time of day for dynamic clock transition.");

            int hourstoChange;
            if (!int.TryParse(value.Substring(11, 2), out hourstoChange))
                throw new ArgumentException("Error parsing hours to change for dynamic clock transition.");

            return new DynamicClockTransition(hourstoChange, weekOfMonth, dayOfWeek, month, transitionTimeOfDay);
        }
    }

    public enum WeekOfMonth
    {
        First = 1,
        Second = 2,
        Third = 3,
        Fourth = 4,
        Last = 5
    }
}