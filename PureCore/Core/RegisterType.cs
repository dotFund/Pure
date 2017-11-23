using System;

namespace Pure.Core
{
    public enum RegisterType : byte
    {
        System = 0,
        Share = 0x10,
        Currency = 0x20,
        Token = 0x40,
    }
}
