using System;

namespace Syndll2.Data
{
    /// <summary>
    /// Provides a generic base class for working with data retrieved from the terminal.
    /// Implementations will parse data differently, depending on application.
    /// </summary>
    public abstract class TerminalData<T>
        where T : TerminalData<T>
    {
        protected abstract void ParseData(string data);

        public static T Parse(string data)
        {
            var instance = (T)Activator.CreateInstance(typeof(T), true);
            instance.ParseData(data);
            return instance;
        }
    }
}
