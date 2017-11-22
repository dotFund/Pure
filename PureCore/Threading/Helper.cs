using System;
using System.Threading;

namespace Pure.Threading
{
    internal static class Helper
    {
        public static IDisposable Do(this SemaphoreSlim semaphore)
        {
            return new SemaphoreContext(semaphore);
        }
    }
}
