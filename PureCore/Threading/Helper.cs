using System.Threading.Tasks;

namespace Pure.Threading
{
    internal static class Helper
    {
        public static async void Void(this Task task)
        {
            await task;
        }
    }
}
