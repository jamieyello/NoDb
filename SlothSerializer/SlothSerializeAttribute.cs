namespace SlothSerializer;

/// <summary> Specifies how a class should be serialized by SlothSockets. </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class SlothSerializeAttribute : System.Attribute {
    public readonly SerializeMode Mode;

    public SlothSerializeAttribute(SerializeMode mode = SerializeMode.Fields)
    {
        Mode = mode;
    }
}