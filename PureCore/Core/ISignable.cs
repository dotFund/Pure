namespace Pure.Core
{
    public interface ISignable
    {
        byte[] GetHashForSigning();

        UInt160[] GetScriptHashesForVerifying();

        byte[][] GetScriptsForVerifying();

        byte[] ToUnsignedArray();
    }
}
