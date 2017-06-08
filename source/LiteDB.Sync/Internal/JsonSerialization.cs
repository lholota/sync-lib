using Newtonsoft.Json;

namespace LiteDB.Sync.Internal
{
    internal static class JsonSerialization
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
        };

        internal static Newtonsoft.Json.JsonSerializer CreateSerializer()
        {
            return Newtonsoft.Json.JsonSerializer.Create(Settings);
        }

        internal static string SerializeToString(object obj)
        {
            return JsonConvert.SerializeObject(obj, Settings);
        }

        internal static T DeserializeFromString<T>(string str)
        {
            return JsonConvert.DeserializeObject<T>(str, Settings);
        }
    }
}