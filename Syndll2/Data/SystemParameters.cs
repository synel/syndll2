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
            var values = TrimParameters(_parameters)
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

        private static IEnumerable<KeyValuePair<int, string>> TrimParameters(Dictionary<int, string> parameters)
        {
            foreach (var parameter in parameters)
            {
                var key = parameter.Key;
                var value = parameter.Value;

                // skip empty values
                if (string.IsNullOrEmpty(value))
                    continue;

                // skip default values, and clean up where possible
                switch (key)
                {
                    case 0:
                        if (value == "6") continue;
                        break;
                    case 1:
                        if (value == "B") continue;
                        break;
                    case 2:
                        if (value == "000000") continue;
                        var l = parameters.ContainsKey(0) ? int.Parse(parameters[0]) : 6;
                        if (value.Length > l) value = value.Substring(0, l);
                        break;
                    case 3:
                        if (value == "Y") continue;
                        break;
                    case 4:
                        if (value == "15") continue;
                        break;
                    case 6:
                        if (value == "50") continue;
                        if (value == "--") continue;
                        break;
                    case 7:
                        if (value == "075") continue;
                        break;
                    case 8:
                        if (value == "15") continue;
                        break;
                    case 9:
                        if (value == "Y") continue;
                        break;
                    case 10:
                        if (value == "N") continue;
                        break;
                    case 11:
                        if (value == "0") continue;
                        break;
                    case 13:
                        if (value == "--") continue;
                        break;
                }

                yield return new KeyValuePair<int, string>(key, value);
            }
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
