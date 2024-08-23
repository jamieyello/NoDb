namespace SlothSerializer.DiffTracking;

public class BinaryDiffSegment {
    public enum SegmentType {
        remove,
        replace,
        insert
    }

    public SegmentType Type { get; private set; }
    public int StartIndex { get; private set; }
    public byte StartXpos { get; private set; }
    public ulong LengthBits { get; private set; }

    public ulong[]? Data { get; private set; }
    public byte DataStartXpos { get; private set; }
    public byte DataEndXpos { get; private set; }

    public static BinaryDiffSegment Remove(
            int index, 
            byte xpos, 
            ulong length_bits) => new() {
            Type = SegmentType.remove,
            StartIndex = index,
            StartXpos = xpos,
            LengthBits = length_bits
        };
    
    public static BinaryDiffSegment Replace(
            int index, 
            byte xpos, 
            ulong length_bits,
            ulong[] data,
            byte data_start_xpos,
            byte data_end_xpos) => new() {
            Type = SegmentType.replace,
            StartIndex = index,
            StartXpos = xpos,
            LengthBits = length_bits,
            Data = data,
            DataStartXpos = data_start_xpos,
            DataEndXpos = data_end_xpos
        };
    
    public static BinaryDiffSegment Insert(
            int index, 
            byte xpos, 
            ulong[] data,
            byte data_start_xpos,
            byte data_end_xpos) => new() {
            Type = SegmentType.insert,
            StartIndex = index,
            StartXpos = xpos,
            Data = data,
            DataStartXpos = data_start_xpos,
            DataEndXpos = data_end_xpos
        };
}