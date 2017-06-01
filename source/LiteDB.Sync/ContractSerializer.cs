using System.IO;
using LiteDB.Sync.Contract;

namespace LiteDB.Sync
{
    public class ContractSerializer : IContractSerializer
    {
        public void WriteHead(Stream destination, Head head)
        {
            
        }

        public Head ReadHead(Stream source)
        {
            
        }

        public Patch ReadPatch(Stream source)
        {

        }

        public void WritePatch(Stream destination, Patch patch)
        {

        }
    }

    public interface IContractSerializer
    {
        Head ReadHead(Stream source);

        void WriteHead(Stream destination, Head head);

        Patch ReadPatch(Stream source);

        void WritePatch(Stream destination, Patch patch);
    }
}