using System;
using System.IO;

namespace Syndll2
{
    internal interface IConnection : IDisposable
    {
        Stream Stream { get; }
        bool Connected { get; }
    }
}