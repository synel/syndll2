using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Syndll2.Data
{
    public class SystemParameters
    {
        private readonly Dictionary<int, string> _parameters;

        public Dictionary<int, string> Parameters
        {
            get { return _parameters; }
        } 

        public SystemParameters()
        {
            _parameters = new Dictionary<int, string>();
        }

        public SystemParameters(Dictionary<int, string> parameters)
        {
            _parameters = parameters;
        }

        public SystemParameters(RdyFile rdyFile)
        {
            if (rdyFile.Header.TableType != 'p' || rdyFile.Header.TableId != 1)
                throw new ArgumentException("The file should be for table p001 only.");

            if (rdyFile.Header.RecordCount != 1)
                throw new ArgumentException("The file should have exactly one record.");

            if (rdyFile.Header.RecordSize != rdyFile.Records[0].Data.Length)
                throw new ArgumentException("The record length does not match the value specified in the header.");

            _parameters = Parse(rdyFile.Records[0].Data);
        }

        public RdyFile GetRdyFile()
        {
            var values = _parameters
                .OrderBy(x => x.Key)
                .Select(x => string.Format("^{0:D3}{1}", x.Key, x.Value));
            var data = string.Join("", values) + "^^";
            var rdy = RdyFile.Create('p', 1, data.Length);
            rdy.AddRecord(data);
            return rdy;
        }

        private static Dictionary<int, string> Parse(string data)
        {
            var items = data.Trim('^').Split('^');

            return items.ToDictionary(
                x => int.Parse(x.Substring(0, 3)),
                x => x.Length > 3 ? x.Substring(3) : "");
        }

        public IList<ClockTransition> ClockTransitions
        {
            get
            {
                // The DST rule is in key 5
                const int key = 5;

                // Return an empty collection if there is no setting or it's empty.
                if (!_parameters.ContainsKey(key) || string.IsNullOrEmpty(_parameters[key]))
                    return new ReadOnlyCollection<ClockTransition>(new ClockTransition[0]);

                // Parse the rules.
                var value = _parameters[key];
                var count = value.Length / 13;
                var transitions = new List<ClockTransition>(count);
                for (int i = 0; i < count; i++)
                {
                    var s = value.Substring(i * 13, 13);
                    var transition = ClockTransition.Parse(s);
                    transitions.Add(transition);
                }
                return new ReadOnlyCollection<ClockTransition>(transitions);
            }
            set
            {
                // The DST rule is in key 5
                const int key = 5;

                // Remove the existing rules if they exist.
                _parameters.Remove(key);

                // figure out the new value.
                var s = string.Join("", value.Select(x => x.ToString()));

                // Add it to the parameters
                _parameters.Add(key, s);
            }
        }
    }
}
