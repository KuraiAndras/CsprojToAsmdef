using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CsprojToAsmdef.Cli.Extensions
{
    public static class EnumerableExtensions
    {
        public static async Task ForEachAsync<T>(this IEnumerable<T> enumerable, Func<T, Task> action)
        {
            foreach (var item in enumerable)
            {
                await action.Invoke(item).ConfigureAwait(false);
            }
        }
    }
}
