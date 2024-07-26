using System.Collections;
using System.Reflection;

namespace SlothSerializer.Internal;

internal static class BitBuilderSerializer {
    // Reflection is slow unless you cache it.
    static readonly Dictionary<Type, SlothSerializeAttribute?> cache_GetSerializeAttribute = new();
    internal static SlothSerializeAttribute? GetSerializeAttribute(Type type) {
        if (cache_GetSerializeAttribute.TryGetValue(type, out var attribute)) return attribute;
        var result = type.GetCustomAttribute<SlothSerializeAttribute>();
        cache_GetSerializeAttribute.Add(type, result);
        return result;
    }

    /// <summary> A primitive type is a type with no underlying fields, like a bool int or string. </summary>
    internal static bool IsPrimitiveType(Type type) => 
        type.IsPrimitive || (type == typeof(string));

    static readonly Dictionary<Type, FieldInfo[]> cache_GetTargetFields = new();
    internal static FieldInfo[] GetTargetFields(Type type) {
        if (cache_GetTargetFields.TryGetValue(type, out var result)) return result;

        var binding_flags = BindingFlags.Instance | BindingFlags.Public;

        // Some exceptions need to be made for commonly used structs with private readonly fields.
        // This is consistent with how JsonConvert works.
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>)) {
            binding_flags = BindingFlags.Instance | BindingFlags.NonPublic;
        }

        result = type.GetFields(binding_flags).OrderBy(field => field.MetadataToken).ToArray();
        cache_GetTargetFields.Add(type, result);
        return result;
    }

    static readonly Dictionary<Type, PropertyInfo[]> cache_GetTargetProperties = new();
    internal static PropertyInfo[] GetTargetProperties(Type type) {
        if (cache_GetTargetProperties.TryGetValue(type, out var result)) return result;

        var binding_flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        result = type
            .GetProperties(binding_flags)
            .OrderBy(property => property.MetadataToken)
            .Where(property => property.SetMethod != null && property.GetMethod != null)
            .ToArray();
        cache_GetTargetProperties.Add(type, result);
        return result;
    }

    static readonly Dictionary<Type, object?> cache_GetDefault = new();
    public static object? GetDefault(Type type) {
        if (cache_GetDefault.TryGetValue(type, out var @default)) return @default;
        var result = type.IsValueType ? Activator.CreateInstance(type) : null;
        cache_GetDefault.Add(type, result);
        return result;
    }

    static readonly Dictionary<Type, (FastMethodInfo Method, Type ParamType)> cache_GetAddMethod = new();
    /// <summary> Get the <see cref="ICollection{T}.Add(T)"/> method. </summary>
    public static (FastMethodInfo Method, Type ParamType) GetAddMethod(Type type) {
        if (cache_GetAddMethod.TryGetValue(type, out var add_method)) return add_method;
        var method = type.GetInterface("ICollection`1")?.GetMethod("Add") ?? throw new Exception($"Failed to get add method from type {type}");
        var result_method = new FastMethodInfo(method);
        var param_type = method.GetParameters().Single().ParameterType;
        var result = (result_method, param_type);
        cache_GetAddMethod.Add(type, result);
        return result;
    }

    /// <summary> Serialize object to <see cref="BitBuilderBuffer"/>. </summary>
    /// <exception cref="NotImplementedException"> Thrown if a type in the object is not supported. </exception>
    internal static void Serialize(object? obj, BitBuilderWriter builder, SerializeMode default_mode) {
        if (obj == null) {
            builder.Append(new ObjectSerializationFlags() { IsNull = true });
            return;
        }

        var type = obj.GetType();
        var attribute = GetSerializeAttribute(type);
        var mode = attribute?.Mode ?? default_mode;

        if (IsPrimitiveType(type)) {
            if (!BitBuilderWriter.IsBaseSupportedType(type)) throw new NotImplementedException($"Type must by implemented in {nameof(BitBuilderWriter)}.");
            if (type == typeof(string)) builder.Append(new ObjectSerializationFlags()); // shouldn't this check for null???
            builder.AppendBaseTypeObject(obj);
        }
        else {
            if (obj is ICollection obj_e) {
                if (type.IsArray) {
                    var a_obj = (Array)obj;
                    var array_rank = (ushort)type.GetArrayRank();
                    var dimensions = Enumerable.Range(0, array_rank).Select(a_obj.GetLongLength).ToArray();
                    var element_type = type.GetElementType() ?? throw new Exception("Failed to get array type.");
                    var indices = new long[array_rank];
                    builder.Append(new ObjectSerializationFlags() { IsNull = false, IsICollection = true, Length = a_obj.LongLength, IsArray = true, ArrayDimensionCount = array_rank, ArrayLengths = dimensions });

                    do {
                        Serialize(a_obj.GetValue(indices), builder, default_mode);
                    } while (IncrementArray(indices, dimensions));
                }
                else {
                    builder.Append(new ObjectSerializationFlags() { IsNull = false, IsICollection = true, Length = obj_e.Count });
                    foreach (var v in obj_e) Serialize(v, builder, default_mode);
                }
            } 
            else {
                builder.Append(new ObjectSerializationFlags() { IsNull = false, IsICollection = false });
                if ((mode & SerializeMode.Fields) > 0) {
                    foreach (var field in GetTargetFields(type)) {
                        // reflection slow?
                        Serialize(field.GetValue(obj), builder, default_mode);
                    }
                }
                if ((mode & SerializeMode.Properties) > 0) {
                    foreach (var property in GetTargetProperties(type)) {
                        Serialize(property.GetValue(obj), builder, default_mode);
                    }
                }
            }
        }
    }

    internal static object? DeSerialize(Type type, BitBuilderReader reader, SerializeMode default_mode) {
        var attribute = GetSerializeAttribute(type);
        var mode = attribute?.Mode ?? default_mode;

        // strings were a mistake
        if (type == typeof(string)) {
            var flags = reader.ReadObjectSerializationFlags();
            if (flags.IsNull) return default;
            return reader.Read(type);
        }
        else if (IsPrimitiveType(type)) {
            if (!reader.IsSupportedType(type)) throw new NotImplementedException($"Type must by implemented in {nameof(BitBuilderWriter)}.");
            return reader.Read(type);
        }
        else {
            var flags = reader.ReadObjectSerializationFlags();
            if (flags.IsNull) return default;

            if (flags.IsICollection) {
                if (type.IsArray) {
                    var element_type = type.GetElementType() ?? throw new Exception("Failed to get array type.");

                    var array_rank = type.GetArrayRank();
                    var array_obj = Array.CreateInstance(element_type, flags.ArrayLengths);
                    var indices = new long[array_rank];

                    do {
                        array_obj.SetValue(DeSerialize(element_type, reader, default_mode), indices);
                    } while (IncrementArray(indices, flags.ArrayLengths));

                    return array_obj;
                }
                else {
                    var obj = Activator.CreateInstance(type)
                        ?? throw new Exception($"Failed to create instance of {type.FullName}");

                    var (method, param_type) = GetAddMethod(type);
                    for (long i = 0; i < flags.Length; i++) method.Invoke(obj, DeSerialize(param_type, reader, default_mode));
                    return obj;
                }
            }
            else {
                var obj = Activator.CreateInstance(type)
                    ?? throw new Exception($"Failed to create instance of {type.FullName}");
                if ((mode & SerializeMode.Fields) > 0) {
                    foreach (var field in GetTargetFields(type)) {
                        // reflection slow
                        field.SetValue(obj, DeSerialize(field.FieldType, reader, default_mode));
                    }
                }
                if ((mode & SerializeMode.Properties) > 0) {
                    foreach (var property in GetTargetProperties(type)) {
                        // reflection slow
                        property.SetValue(obj, DeSerialize(property.PropertyType, reader, default_mode));
                    }
                }
                return obj;
            }
        }
    }

    static bool IncrementArray(long[] indexes, long[] dimensions, long i = 0) {
        if (i == indexes.Length) return false;
        if (++indexes[i] == dimensions[i]) {
            indexes[i] = 0;
            return IncrementArray(indexes, dimensions, i + 1);
        }
        return true;
    }
}
