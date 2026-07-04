namespace ShalazamPlugin;

internal static class HashHelper
{
    internal static uint StableHash(string s)
    {
        const uint prime = 16777619;
        var hash = 2166136261u;
        foreach (var c in s)
        {
            hash ^= c;
            hash *= prime;
        }
        return hash;
    }
}
