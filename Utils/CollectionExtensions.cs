namespace MCMicroLauncher.Utils
{
    public static class CollectionExtensions
    {
        public static string JoinUsing(
            this string[] source,
            string separator)
        => string.Join(separator, source);
    }
}
