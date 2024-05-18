namespace SlothSerializer;

/// <summary> Serialize either properties or fields. Recursive through all child objects unless otherwise specified. </summary>
/// <remarks> If this is not adequate please submit an issue. </remarks>
public enum SerializeMode {
    Fields = 1,
    Properties = 2,
}
