using System;
using System.Threading;
using LiteDB.Sync.Internal;

namespace LiteDB.Sync
{
    internal interface ILiteSyncDatabase : ILiteDatabase
    {
        CloudState GetLocalCloudState();

        void SaveLocalCloudState(CloudState cloudState);

        IDisposable LockExclusive();

        Patch GetLocalChanges(CancellationToken ct);

        void ApplyChanges(Patch patch, CancellationToken ct);
    }
}