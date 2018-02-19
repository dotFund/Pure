using Pure.IO;
using Pure.VM;
using System.IO;

namespace Pure.Core
{
    public interface IVerifiable : ISerializable, IScriptContainer
    {
        Witness[] Scripts { get; set; }
        
        void DeserializeUnsigned(BinaryReader reader);

        UInt160[] GetScriptHashesForVerifying();
        
        void SerializeUnsigned(BinaryWriter writer);
    }
}
