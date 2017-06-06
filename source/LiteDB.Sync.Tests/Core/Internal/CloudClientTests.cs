using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LiteDB.Sync.Exceptions;
using LiteDB.Sync.Internal;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace LiteDB.Sync.Tests.Core.Internal
{
    [TestFixture]
    public class CloudClientTests
    {
        /*
             * No local cloud state
             * - create -> conflict - other client created it first
             
             * One patch
             * Multiple patches
             */

        public abstract class WhenPulling<TProvider> : CloudClientTests 
            where TProvider : class, ILiteSyncCloudProvider
        {
            private CloudClient cloudClient;
            private Mock<TProvider> providerMock;

            [SetUp]
            public void Setup()
            {
                this.providerMock = new Mock<TProvider>();
                this.cloudClient = new CloudClient(this.providerMock.Object);
            }

            [Test]
            public async Task WithNoStateShouldReturnExistingRemoteState()
            {
                var remoteState = new CloudState("NextPatchId");

                this.providerMock.Setup(x => x.DownloadPatchFile(It.IsIn("NextPatchId"), It.IsAny<CancellationToken>())).ReturnsAsync((Stream)null);
                this.providerMock.Setup(x => x.DownloadInitFile(It.IsAny<CancellationToken>())).ReturnsAsync(this.CreateJsonStream(remoteState));

                var pullResult = await this.cloudClient.Pull(null, CancellationToken.None);

                this.AssertPullResultHasState(pullResult);
                Assert.AreEqual("NextPatchId", pullResult.CloudState.NextPatchId);

                this.providerMock.VerifyAll();
            }

            [Test]
            public async Task WithNoStateShouldCreateCloudState()
            {
                this.providerMock.Setup(x => x.DownloadPatchFile(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((Stream)null);
                this.providerMock.Setup(x => x.DownloadInitFile(It.IsAny<CancellationToken>())).ReturnsAsync((Stream)null);

                this.providerMock
                    .Setup(x => x.UploadInitFile(It.IsAny<Stream>()))
                    .Callback<Stream>(x =>
                    {
                        var cloudState = this.DeserializeFromStream<CloudState>(x);
                        this.IsPatchIdValid(cloudState.NextPatchId);
                    })
                    .Returns(Task.FromResult(0));

                var pullResult = await this.cloudClient.Pull(null, CancellationToken.None);

                this.AssertPullResultHasState(pullResult);

                this.providerMock.VerifyAll();
            }

            [Test]
            public async Task WithNoStateShouldReturnExistingRemoteStateWhenCreateAttemptConflicts()
            {
                var remoteState = new CloudState("NextPatchId");

                this.providerMock.Setup(x => x.DownloadPatchFile(It.IsIn("NextPatchId"), It.IsAny<CancellationToken>())).ReturnsAsync((Stream)null);
                this.providerMock.SetupSequence(x => x.DownloadInitFile(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(null)
                    .ReturnsAsync(this.CreateJsonStream(remoteState));

                this.providerMock.Setup(x => x.UploadInitFile(It.IsAny<Stream>())).Throws(new LiteSyncConflictOccuredException());

                var pullResult = await this.cloudClient.Pull(null, CancellationToken.None);

                this.AssertPullResultHasState(pullResult);
                Assert.AreEqual("NextPatchId", pullResult.CloudState.NextPatchId);

                this.providerMock.VerifyAll();
            }

            [Test]
            public async Task WithStateShouldReturnNoChangeIfNoNewPatchIsAvailable()
            {
                var localState = new CloudState("NextPatchId");

                this.providerMock.Setup(x => x.DownloadPatchFile(It.IsIn("NextPatchId"), It.IsAny<CancellationToken>())).ReturnsAsync((Stream)null);

                var pullResult = await this.cloudClient.Pull(localState, CancellationToken.None);

                Assert.IsNotNull(pullResult);
                Assert.IsFalse(pullResult.CloudStateChanged);
                Assert.IsFalse(pullResult.HasChanges);

                this.providerMock.VerifyAll();
            }

            [Test]
            public async Task WithStateShouldDownloadNewPatchAndUpdateState()
            {
                var localState = new CloudState("Patch1");
                var remotePatch = this.CreatePatch("Patch2");

                this.providerMock.Setup(x => x.DownloadPatchFile(It.IsIn("Patch1"), It.IsAny<CancellationToken>())).ReturnsAsync(this.CreateJsonStream(remotePatch));
                this.providerMock.Setup(x => x.DownloadPatchFile(It.IsIn("Patch2"), It.IsAny<CancellationToken>())).ReturnsAsync((Stream)null);

                var pullResult = await this.cloudClient.Pull(localState, CancellationToken.None);

                Assert.IsNotNull(pullResult);

                Assert.IsTrue(pullResult.CloudStateChanged);
                Assert.AreEqual("Patch2", pullResult.CloudState.NextPatchId);

                Assert.IsTrue(pullResult.HasChanges);

                this.providerMock.VerifyAll();
            }

            protected virtual void SetupGeneratePatchId(Mock<TProvider> mock) { }

            protected abstract bool IsPatchIdValid(string patchId);
        }

        public class WhenPullingWithBuiltinIdGenerator : WhenPulling<ILiteSyncCloudProvider>
        {
            protected override bool IsPatchIdValid(string patchId)
            {
                return Guid.TryParse(patchId, out Guid _);
            }
        }

        public class WhenPullingWithCustomIdGenerator : WhenPulling<ICombinedProvider>
        {
            private const string GeneratedId = "MyCustomId";

            protected override void SetupGeneratePatchId(Mock<ICombinedProvider> providerMock)
            {
                providerMock.Setup(x => x.GeneratePatchId(It.IsAny<CancellationToken>())).ReturnsAsync(GeneratedId);
            }

            protected override bool IsPatchIdValid(string patchId)
            {
                return patchId == GeneratedId;
            }
        }

        public abstract class WhenPushing<TProvider> : CloudClientTests
            where TProvider : class, ILiteSyncCloudProvider
        {
            private CloudClient cloudClient;
            private Mock<TProvider> providerMock;

            [SetUp]
            public void Setup()
            {
                this.providerMock = new Mock<TProvider>();
                this.cloudClient = new CloudClient(this.providerMock.Object);
            }
        }

        public class WhenPushingWithBuiltinIdGenerator : WhenPushing<ILiteSyncCloudProvider>
        {
            
        }

        public class WhenPushingWithCustomIdGenerator : WhenPushing<ICombinedProvider>
        {

        }

        internal Patch CreatePatch(string nextPatchId)
        {
            var id = new EntityId("MyCollection", 123);
            var change = new EntityChange(id);

            var result = new Patch(new []{ change });
            result.NextPatchId = nextPatchId;

            return result;
        }

        protected T DeserializeFromStream<T>(Stream stream)
        {
            var serializer = new Newtonsoft.Json.JsonSerializer();

            // Do not dispose the reader, this would dispose the stream 
            // which is a responsibility of the CloudClient
            var reader = new StreamReader(stream);
            var jsonReader = new JsonTextReader(reader);

            return serializer.Deserialize<T>(jsonReader);
        }

        protected Stream CreateJsonStream<T>(T item)
        {
            var serializer = new Newtonsoft.Json.JsonSerializer();
            var stream = new MemoryStream();

            var writer = new StreamWriter(stream);
            serializer.Serialize(writer, item);

            writer.Flush();
            stream.Position = 0;

            Console.WriteLine(JsonConvert.SerializeObject(item));

            return stream;
        }

        internal void AssertPullResultHasState(PullResult pullResult)
        {
            Assert.IsNotNull(pullResult);
            Assert.IsNotNull(pullResult.CloudState);

            Assert.IsTrue(pullResult.CloudStateChanged);
        }

        public interface ICombinedProvider : ILiteSyncCloudProvider, ILiteSyncPatchIdGenerator
        {
            
        }
    }
}