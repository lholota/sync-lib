using System;
using System.IO;
using System.Linq;
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
        protected const string EntityPropKey = "Key";

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

                var pullResult = await this.cloudClient.PullAsync(null, CancellationToken.None);

                this.AssertPullResultHasState(pullResult);
                Assert.AreEqual("NextPatchId", pullResult.CloudState.NextPatchId);

                this.providerMock.VerifyAll();
            }

            [Test]
            public async Task WithNoStateShouldReturnExistingRemoteStateAndDownloadPatches()
            {
                var remoteState = new CloudState("Patch1");
                var remotePatch = this.CreatePatch("Patch2");

                this.providerMock.Setup(x => x.DownloadInitFile(It.IsAny<CancellationToken>())).ReturnsAsync(this.CreateJsonStream(remoteState));
                this.providerMock.Setup(x => x.DownloadPatchFile(It.IsIn("Patch1"), It.IsAny<CancellationToken>())).ReturnsAsync(this.CreateJsonStream(remotePatch));
                this.providerMock.Setup(x => x.DownloadPatchFile(It.IsIn("Patch2"), It.IsAny<CancellationToken>())).ReturnsAsync((Stream)null);

                var pullResult = await this.cloudClient.PullAsync(null, CancellationToken.None);

                this.AssertPullResultHasState(pullResult);
                Assert.AreEqual("Patch2", pullResult.CloudState.NextPatchId);

                Assert.IsNotNull(pullResult.RemotePatch);

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

                var pullResult = await this.cloudClient.PullAsync(null, CancellationToken.None);

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

                var pullResult = await this.cloudClient.PullAsync(null, CancellationToken.None);

                this.AssertPullResultHasState(pullResult);
                Assert.AreEqual("NextPatchId", pullResult.CloudState.NextPatchId);

                this.providerMock.VerifyAll();
            }

            [Test]
            public async Task WithStateShouldReturnNoChangeIfNoNewPatchIsAvailable()
            {
                var localState = new CloudState("NextPatchId");

                this.providerMock.Setup(x => x.DownloadPatchFile(It.IsIn("NextPatchId"), It.IsAny<CancellationToken>())).ReturnsAsync((Stream)null);

                var pullResult = await this.cloudClient.PullAsync(localState, CancellationToken.None);

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

                var pullResult = await this.cloudClient.PullAsync(localState, CancellationToken.None);

                Assert.IsNotNull(pullResult);

                Assert.IsTrue(pullResult.CloudStateChanged);
                Assert.AreEqual("Patch2", pullResult.CloudState.NextPatchId);

                Assert.IsTrue(pullResult.HasChanges);

                this.providerMock.VerifyAll();
            }

            [Test]
            public async Task WithStateShouldDownloadMultiplePatches()
            {
                var localState = new CloudState("Patch1");
                var remotePatch1 = this.CreatePatch("Patch2", "Value2");
                var remotePatch2 = this.CreatePatch("Patch3", "Value3");

                this.providerMock.Setup(x => x.DownloadPatchFile(It.IsIn("Patch1"), It.IsAny<CancellationToken>())).ReturnsAsync(this.CreateJsonStream(remotePatch1));
                this.providerMock.Setup(x => x.DownloadPatchFile(It.IsIn("Patch2"), It.IsAny<CancellationToken>())).ReturnsAsync(this.CreateJsonStream(remotePatch2));
                this.providerMock.Setup(x => x.DownloadPatchFile(It.IsIn("Patch3"), It.IsAny<CancellationToken>())).ReturnsAsync((Stream)null);

                var pullResult = await this.cloudClient.PullAsync(localState, CancellationToken.None);

                Assert.IsNotNull(pullResult);

                Assert.IsTrue(pullResult.CloudStateChanged);
                Assert.AreEqual("Patch3", pullResult.CloudState.NextPatchId);

                Assert.IsTrue(pullResult.HasChanges);
                Assert.AreEqual("Value3", pullResult.RemotePatch.Changes.OfType<UpsertEntityChange>().Single().Entity[EntityPropKey].RawValue);

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

            [Test]
            public async Task ShouldPushChangeSuccessfully()
            {
                this.providerMock
                    .Setup(x => x.UploadPatchFile("Patch1", It.IsAny<Stream>()))
                    .Callback<string, Stream>((fileNamePatchId, stream) =>
                    {
                        var deserializedPatch = this.DeserializeFromStream<Patch>(stream);
                        this.IsPatchIdValid(deserializedPatch.NextPatchId);
                        this.IsPatchIdValid(fileNamePatchId);
                    })
                    .Returns(Task.FromResult(0));

                var cloudState = new CloudState("Patch1");
                var patch = this.CreatePatch(null);

                await this.cloudClient.PushAsync(cloudState, patch, CancellationToken.None);

                this.IsPatchIdValid(cloudState.NextPatchId);
            }

            [Test]
            public void ShouldThrowIfConflictOccurs()
            {
                this.providerMock
                    .Setup(x => x.UploadPatchFile("Patch1", It.IsAny<Stream>()))
                    .Throws(new LiteSyncConflictOccuredException());

                var cloudState = new CloudState("Patch1");
                var patch = this.CreatePatch(null);

                var ex = Assert.Throws<AggregateException>(() => this.cloudClient.PushAsync(cloudState, patch, CancellationToken.None).Wait());

                ex = ex.Flatten();
                Assert.IsInstanceOf<LiteSyncConflictOccuredException>(ex.InnerExceptions.Single());
            }

            /*
             * Conflict - patch already exists
             * 
             * 
             * Problem - upload succeeds, but local transaction is not commited
             */

            protected virtual void SetupGeneratePatchId(Mock<TProvider> mock) { }

            protected abstract bool IsPatchIdValid(string patchId);
        }

        public class WhenPushingWithBuiltinIdGenerator : WhenPushing<ILiteSyncCloudProvider>
        {
            protected override bool IsPatchIdValid(string patchId)
            {
                return Guid.TryParse(patchId, out Guid _);
            }
        }

        public class WhenPushingWithCustomIdGenerator : WhenPushing<ICombinedProvider>
        {
            protected override void SetupGeneratePatchId(Mock<ICombinedProvider> mock)
            {
                mock.Setup(x => x.GeneratePatchId(It.IsAny<CancellationToken>()))
                    .ReturnsAsync("CustomId");
            }

            protected override bool IsPatchIdValid(string patchId)
            {
                return patchId == "CustomId";
            }
        }

        internal Patch CreatePatch(string nextPatchId, string entityPropValue = null)
        {
            var id = new EntityId("MyCollection", 123);
            var doc = new BsonDocument();
            doc[EntityPropKey] = entityPropValue;

            var change = new UpsertEntityChange(id, doc);

            var result = new Patch(new[] { change });
            result.NextPatchId = nextPatchId;

            return result;
        }

        protected T DeserializeFromStream<T>(Stream stream)
        {
            var serializer = JsonSerialization.CreateSerializer();

            // Do not dispose the reader, this would dispose the stream 
            // which is a responsibility of the CloudClient
            var reader = new StreamReader(stream);
            var jsonReader = new JsonTextReader(reader);

            return serializer.Deserialize<T>(jsonReader);
        }

        protected Stream CreateJsonStream<T>(T item)
        {
            var serializer = JsonSerialization.CreateSerializer();
            var stream = new MemoryStream();

            var writer = new StreamWriter(stream);
            serializer.Serialize(writer, item);

            writer.Flush();
            stream.Position = 0;

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