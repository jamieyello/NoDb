namespace SlothSerializer;

/// <summary> Serialize either properties or fields. Recursive through all child objects unless otherwise specified. Can be combined with |. </summary>
/// <remarks> If this is not adequate please submit an issue. </remarks>
public enum SerializeMode {
    Fields = 0b_01,
    Properties = 0b_10,
    FieldsAndProperties = 0b_11,
}
