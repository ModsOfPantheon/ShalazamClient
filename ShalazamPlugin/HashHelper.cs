namespace ShalazamPlugin;

internal static class HashHelper
{
    internal static uint StableHash(string s)
    {
        const uint prime = 16777619;
        uint hash = 2166136261;
        foreach (char c in s)
        {
            hash ^= c;
            hash *= prime;
        }
        return hash;
    }
}
