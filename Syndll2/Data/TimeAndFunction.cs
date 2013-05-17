using System;
using System.Globalization;

namespace Syndll2.Data
{
    internal class TimeAndFunction
    {
        private readonly DateTime _timestamp;
        private readonly char _activeFunction;

        public DateTime Timestamp
        {
            get { return _timestamp; }
        }

        public char ActiveFunction
        {
            get { return _activeFunction; }
        }

        internal TimeAndFunction(DateTime timestamp, char activeFunction)
        {
            _timestamp = timestamp;
            _activeFunction = activeFunction;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:yyMMddHHmm}{1}{0:ss}", _timestamp, _activeFunction);
        }
    }
}
