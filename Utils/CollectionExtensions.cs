using System.Collections.Generic;

namespace MCMicroLauncher.Utils
{
    internal static class CollectionExtensions
    {
        internal static string JoinUsing(
            this string[] source,
            string separator)
        => string.Join(separator, source);

        internal static string JoinUsing(
            this IEnumerable<string> source,
            string separator)
        => string.Join(separator, source);
    }
}
