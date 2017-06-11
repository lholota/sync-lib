using System;
using System.IO;
using Newtonsoft.Json;

namespace LiteDB.Sync.Tests.Providers
{
    public class TestSecrets
    {
        private const string FileName = @"Tests.secrets";

        public static TestSecrets LoadFromFile()
        {
            var secretsPath = Environment.GetEnvironmentVariable("LiteDbSyncSecretsPath");

            if (string.IsNullOrEmpty(secretsPath))
            {
                throw new Exception("The secrets env. variable is not set.");
            }

            if (!File.Exists(secretsPath))
            {
                throw new FileNotFoundException($"The secrets file {secretsPath} could not be found.", secretsPath);
            }

            var json = File.ReadAllText(secretsPath);

            return JsonConvert.DeserializeObject<TestSecrets>(json);
        }

        public string OneDriveClientId { get; private set; }

        public string DropBoxAppKey { get; private set; }

        public string DropBoxAppSecret { get; private set; }
    }
}