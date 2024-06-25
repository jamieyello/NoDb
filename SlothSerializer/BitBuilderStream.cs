namespace SlothSerializer;

/// <summary>
/// A read/write stream of a <see cref="BitBuilderBuffer"/>'s internal binary data.
/// </summary>
public class BitBuilderStream : Stream {
    readonly BitBuilderBuffer _buffer;
    readonly BitBuilderReader _reader;

    public override bool CanRead => true;
    public override bool CanSeek => true;
    public override bool CanWrite => true;

    public override long Length => _buffer.TotalStreamLengthBytes;

    public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    internal BitBuilderStream(BitBuilderBuffer buffer) {
        _buffer = buffer;
        _reader = buffer.GetReader();
    }

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