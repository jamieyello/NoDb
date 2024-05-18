namespace SlothSerializer.Internal;

public class BinaryDiff
{
    public enum MethodType
    {
        replace
    }

    MethodType Method { get; set; }
    ulong[] PatchData { get; set; }
    ulong? Hash { get; set; }

    public BinaryDiff() { }
    public BinaryDiff(BitBuilderReader old, BitBuilderReader new_, MethodType method)
    {
        Method = method;
        if (Method == MethodType.replace) PatchData = new_.ToArray();
        else throw new NotImplementedException();
    }

    public void Apply(ulong[] binary)
    {
        if (Method == MethodType.replace) ApplyReplace(PatchData, binary);
        else throw new NotImplementedException();
    }

    static void ApplyReplace(ulong[] patch, ulong[] binary)
    {
        Array.Resize(ref binary, patch.Length);
        patch.CopyTo(binary, 0);
    }

    // https://stackoverflow.com/a/9545731
    static ulong CalculateHash(ulong[] read)
    {
        ulong hashedValue = 3074457345618258791ul;
        for (int i = 0; i < read.Length; i++)
        {
            hashedValue += read[i];
            hashedValue *= 3074457345618258799ul;
        }
        return hashedValue;
    }
}
