namespace SlothSerializer;

/// <summary>
/// Reads/writes the data of the <see cref="BitBuilderBuffer"/>, header included.
/// </summary>
public class BitBuilderStream : Stream {
    byte[] _header;
    BitBuilderBuffer _buffer;

    internal BitBuilderStream(BitBuilderBuffer buffer) {
        _buffer = buffer;
        _header = buffer.EnumerateHeader().ToArray();
    }

    public override bool CanRead => throw new NotImplementedException();

    public override bool CanSeek => throw new NotImplementedException();

    public override bool CanWrite => throw new NotImplementedException();

    public override long Length => throw new NotImplementedException();

    public override long Position { get; set; }

    public override void Flush()
    {
        throw new NotImplementedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        throw new NotImplementedException();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotImplementedException();
    }

    public override void SetLength(long value)
    {
        throw new NotImplementedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotImplementedException();
    }
}