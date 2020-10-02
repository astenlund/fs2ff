using System;
using System.Threading.Tasks;

namespace fs2ff
{
    public static class Extensions
    {
        public static T AdjustToBounds<T>(this T value, T min, T max) where T : IComparable =>
            value.CompareTo(min) < 0
                ? min
                : value.CompareTo(max) > 0
                    ? max
                    : value;

        public static async Task RaiseAsync<T>(this Func<T, Task>? handler, T value)
        {
            if (handler == null)
            {
                return;
            }

            Delegate[] delegates = handler.GetInvocationList();
            Task[] tasks = new Task[delegates.Length];

            for (var i = 0; i < delegates.Length; i++)
            {
                tasks[i] = ((Func<T, Task>) delegates[i])(value);
            }

            await Task.WhenAll(tasks);
        }

        public static async Task RaiseAsync<T1, T2>(this Func<T1, T2, Task>? handler, T1 value1, T2 value2)
        {
            if (handler == null)
            {
                return;
            }

            Delegate[] delegates = handler.GetInvocationList();
            Task[] tasks = new Task[delegates.Length];

            for (var i = 0; i < delegates.Length; i++)
            {
                tasks[i] = ((Func<T1, T2, Task>) delegates[i])(value1, value2);
            }

            await Task.WhenAll(tasks);
        }
    }
}
