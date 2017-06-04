using System;
using System.IO;
using Newtonsoft.Json;

namespace LiteDB.Sync.Tests.Providers
{
    internal class TestSecrets
    {
        private const string FileName = @"Tests.secrets";

        public static TestSecrets LoadFromFile()
        {
            var filePath = Path.Combine(Environment.CurrentDirectory, FileName);
            var json = File.ReadAllText(filePath);

            return JsonConvert.DeserializeObject<TestSecrets>(json);
        }

        public string OneDriveClientId { get; private set; }

        public string DropBoxAppKey { get; private set; }

        public string DropBoxAppSecret { get; private set; }
    }
}