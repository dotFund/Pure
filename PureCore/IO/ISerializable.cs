using System.IO;

namespace Pure.IO
{
    internal interface ISerializable
    {
        void Serialize(BinaryWriter writer);
        void Deserialize(BinaryReader reader);
    }
}
