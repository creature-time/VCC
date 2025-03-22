
namespace CreatureTime
{
    public static class CtEncodingUtils
    {
        public static int Encode(string key)
        {
            // Compute FNV1a Hash; Does have a low collision rate.
            int hash = -2128831035; // Note: int overflow number of 2166136261;
            foreach (char c in key)
            {
                hash = (hash ^ c) * 16777619;
            }
            return hash;
        }
    }
}