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

        public TestSecrets(string oneDriveClientId, string oneDriveClientSecret, string dropBoxAppKey, string dropBoxAppSecret)
        {
            this.OneDriveClientId = oneDriveClientId;
            this.DropBoxAppKey = dropBoxAppKey;
            this.DropBoxAppSecret = dropBoxAppSecret;
        }

        public string OneDriveClientId { get; }

        public string DropBoxAppKey { get; }

        public string DropBoxAppSecret { get; }
    }
}