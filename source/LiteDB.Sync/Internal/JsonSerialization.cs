using Newtonsoft.Json;

namespace LiteDB.Sync.Internal
{
    internal static class JsonSerialization
    {
        internal static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            // TypeNameHandling = TypeNameHandling.All
        };
    }
}