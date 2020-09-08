using System;
using System.Threading.Tasks;

namespace fs2ff
{
    public static class Extensions
    {
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
    }
}
