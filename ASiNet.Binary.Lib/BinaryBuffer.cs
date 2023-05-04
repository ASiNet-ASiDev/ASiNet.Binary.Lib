using System;
using System.Text;

namespace ASiNet.Binary.Lib;
/// <summary>
/// Представляет обёртку над <see cref="Span{T}"/>
/// </summary>
public ref struct BinaryBuffer
{
    #region constructors
    /// <summary>
    /// Создаёт буфер из заданого <see cref="Span{T}"/> 
    /// </summary>
    /// <param name="buffer">Буфер в который будут записанны данные.</param>
    public BinaryBuffer(Span<byte> buffer)
    {
        _buffer = buffer;
    }
    /// <summary>
    /// Создаёт буфер из заданного <see cref="Span{T}"/> и начальной позиции с которой будут записываться данные.
    /// </summary>
    /// <param name="buffer">Буфер в который будут записанны данные.</param>
    /// <param name="writeOffset">Позиция в которую будет установлен <see cref="WritePosition"/> по умолчанию.</param>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public BinaryBuffer(Span<byte> buffer, int writeOffset)
    {
        if(writeOffset < 0 || writeOffset >= buffer.Length)
            throw new ArgumentOutOfRangeException(nameof(writeOffset));
        _defaultWriteStartPos = writeOffset;
        _buffer = buffer;
        _writePosition = writeOffset;
    }
    /// <summary>
    /// Создаёт буфер из заданного <see cref="Span{T}"/> и начальной позиции с которой будут записываться данные, и начальной позицией с которой будут читаться данные.
    /// </summary>
    /// <param name="buffer">Буфер в который будут записанны данные.</param>
    /// <param name="writeOffset">Позиция в которую будет установлен <see cref="WritePosition"/> по умолчанию.</param>
    /// <param name="readOffset">Позиция в которую будет установлен <see cref="ReadPosition"/> по умолчанию.</param>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public BinaryBuffer(Span<byte> buffer, int writeOffset, int readOffset)
    {
        if (writeOffset < 0 || writeOffset >= buffer.Length)
            throw new ArgumentOutOfRangeException(nameof(writeOffset));
        if (readOffset < 0 || readOffset >= buffer.Length)
            throw new ArgumentOutOfRangeException(nameof(readOffset));
        _defaultWriteStartPos = writeOffset;
        _defaultReadStartPos = readOffset;
        _buffer = buffer;
        _writePosition = writeOffset;
        _readPosition = readOffset;
    }
    #endregion

    /// <summary>
    /// Общий размер буфера.
    /// </summary>
    public int Length => _buffer.Length;
    /// <summary>
    /// Размер буфера занемаемый данными.
    /// </summary>
    public int Size =>  _writePosition - _defaultWriteStartPos;
    /// <summary>
    /// Позиция с которой будут прочитаны последующие данные.
    /// </summary>
    public int ReadPosition => _readPosition;
    /// <summary>
    /// Позиция в которую будут записанные последующие данные.
    /// </summary>
    public int WritePosition => _writePosition;

    private int _defaultWriteStartPos = 0;
    private int _defaultReadStartPos = 0;
    private int _writePosition = 0;
    private int _readPosition = 0;
    private Span<byte> _buffer;
    

    public static BinaryBuffer FromStream(Stream stream, in Span<byte> buffer)
    {
        var binaryBuffer = new BinaryBuffer(buffer);
        stream.Read(buffer);

        return binaryBuffer;
    }

    #region write
    public bool Write(in Span<byte> data)
    {
        var offset = _writePosition + data.Length;
        if (offset > _buffer.Length)
            return false;

        for (int i = 0; i < data.Length; i++)
            _buffer[_writePosition + i] = data[i];
        _writePosition = offset;
        return true;
    }

    public bool Write(in ReadOnlySpan<byte> data)
    {
        var offset = _writePosition + data.Length;
        if (offset > _buffer.Length)
            return false;

        for (int i = 0; i < data.Length; i++)
            _buffer[_writePosition + i] = data[i];
        _writePosition = offset;
        return true;
    }

    public bool Write(in ReadOnlySpan<char> data, Encoding encoding)
    {
        var size = encoding.GetByteCount(data);
        var offset = _writePosition + size;
        if (offset > _buffer.Length)
            return false;

        Span<byte> buffer = stackalloc byte[size];
        encoding.GetBytes(data, buffer);

        for (int i = 0; i < buffer.Length; i++)
            _buffer[_writePosition + i] = buffer[i];
        _writePosition = offset;
        return true;
    }

    public bool Write(in byte data)
    {
        if (_writePosition >= _buffer.Length)
            return false;
        _buffer[_writePosition] = data;
        _writePosition++;
        return true;
    }

    public bool Write(in sbyte data)
    {
        if (_readPosition >= _buffer.Length)
            return false;
        _buffer[_writePosition] = (byte)data;
        _writePosition++;
        return true;
    }

    public bool Write(in bool data)
    {
        Span<byte> buffer = stackalloc byte[sizeof(bool)];
        BitConverter.TryWriteBytes(buffer, data);
        
        var offset = _writePosition + buffer.Length;
        if (offset > _buffer.Length)
            return false;

        for (int i = 0; i < buffer.Length; i++)
            _buffer[_writePosition + i] = buffer[i];
        _writePosition = offset;
        return true;
    }

    public bool Write(in short data)
    {
        Span<byte> buffer = stackalloc byte[sizeof(short)];
        BitConverter.TryWriteBytes(buffer, data);

        var offset = _writePosition + buffer.Length;
        if (offset > _buffer.Length)
            return false;

        for (int i = 0; i < buffer.Length; i++)
            _buffer[_writePosition + i] = buffer[i];
        _writePosition = offset;
        return true;
    }

    public bool Write(in ushort data)
    {
        Span<byte> buffer = stackalloc byte[sizeof(ushort)];
        BitConverter.TryWriteBytes(buffer, data);

        var offset = _writePosition + buffer.Length;
        if (offset > _buffer.Length)
            return false;

        for (int i = 0; i < buffer.Length; i++)
            _buffer[_writePosition + i] = buffer[i];
        _writePosition = offset;
        return true;
    }

    public bool Write(in int data)
    {
        Span<byte> buffer = stackalloc byte[sizeof(int)];
        BitConverter.TryWriteBytes(buffer, data);

        var offset = _writePosition + buffer.Length;
        if (offset > _buffer.Length)
            return false;

        for (int i = 0; i < buffer.Length; i++)
            _buffer[_writePosition + i] = buffer[i];
        _writePosition = offset;
        return true;
    }

    public bool Write(in uint data)
    {
        Span<byte> buffer = stackalloc byte[sizeof(uint)];
        BitConverter.TryWriteBytes(buffer, data);

        var offset = _writePosition + buffer.Length;
        if (offset > _buffer.Length)
            return false;

        for (int i = 0; i < buffer.Length; i++)
            _buffer[_writePosition + i] = buffer[i];
        _writePosition = offset;
        return true;
    }

    public bool Write(in long data)
    {
        Span<byte> buffer = stackalloc byte[sizeof(long)];
        BitConverter.TryWriteBytes(buffer, data);

        var offset = _writePosition + buffer.Length;
        if (offset > _buffer.Length)
            return false;

        for (int i = 0; i < buffer.Length; i++)
            _buffer[_writePosition + i] = buffer[i];
        _writePosition = offset;
        return true;
    }

    public bool Write(in ulong data)
    {
        Span<byte> buffer = stackalloc byte[sizeof(ulong)];
        BitConverter.TryWriteBytes(buffer, data);

        var offset = _writePosition + buffer.Length;
        if (offset > _buffer.Length)
            return false;

        for (int i = 0; i < buffer.Length; i++)
            _buffer[_writePosition + i] = buffer[i];
        _writePosition = offset;
        return true;
    }

    public bool Write(in float data)
    {
        Span<byte> buffer = stackalloc byte[sizeof(float)];
        BitConverter.TryWriteBytes(buffer, data);

        var offset = _writePosition + buffer.Length;
        if (offset > _buffer.Length)
            return false;

        for (int i = 0; i < buffer.Length; i++)
            _buffer[_writePosition + i] = buffer[i];
        _writePosition = offset;
        return true;
    }

    public bool Write(in double data)
    {
        Span<byte> buffer = stackalloc byte[sizeof(double)];
        BitConverter.TryWriteBytes(buffer, data);

        var offset = _writePosition + buffer.Length;
        if (offset > _buffer.Length)
            return false;

        for (int i = 0; i < buffer.Length; i++)
            _buffer[_writePosition + i] = buffer[i];
        _writePosition = offset;
        return true;
    }

    public bool Write(in char data)
    {
        Span<byte> buffer = stackalloc byte[sizeof(char)];
        BitConverter.TryWriteBytes(buffer, data);

        var offset = _writePosition + buffer.Length;
        if (offset > _buffer.Length)
            return false;

        for (int i = 0; i < buffer.Length; i++)
            _buffer[_writePosition + i] = buffer[i];
        _writePosition = offset;
        return true;
    }

    public bool Write(in DateTime data)
    {
        var binaryDate = data.ToBinary();
        Span<byte> buffer = stackalloc byte[sizeof(long)];
        BitConverter.TryWriteBytes(buffer, binaryDate);

        var offset = _writePosition + buffer.Length;
        if (offset > _buffer.Length)
            return false;

        for (int i = 0; i < buffer.Length; i++)
            _buffer[_writePosition + i] = buffer[i];
        _writePosition = offset;
        return true;
    }

    public bool Write(in Guid data)
    {
        
        Span<byte> buffer = stackalloc byte[sizeof(decimal)];
        if(data.TryWriteBytes(buffer))
        {
            var offset = _writePosition + buffer.Length;
            if (offset > _buffer.Length)
                return false;

            for (int i = 0; i < buffer.Length; i++)
                _buffer[_writePosition + i] = buffer[i];
            _writePosition = offset;
            return true;
        }
        return false;
    }

    public bool Write(string data, Encoding encoding)
    {
        var size = encoding.GetByteCount(data);
        var offset = _writePosition + size + sizeof(int);
        if (offset > _buffer.Length)
            return false;

        var localBuffer = new BinaryBuffer(stackalloc byte[size + sizeof(int)]);

        localBuffer.Write(size);
        localBuffer.Write(data.AsSpan(), encoding);

        var buffer = localBuffer.ToSpan();

        for (int i = 0; i < buffer.Length; i++)
            _buffer[_writePosition + i] = buffer[i];
        _writePosition = offset;

        return true;
    }

    #endregion
    
    #region read
    public byte ReadByte()
    {
        var data = _buffer[_readPosition];
        _readPosition++;
        return data;
    }

    public sbyte ReadSByte()
    {
        var data = _buffer[_readPosition];
        _readPosition++;
        return (sbyte)data;
    }

    public bool ReadBoolean()
    {
        var data = _buffer.Slice(_readPosition, sizeof(bool));
        _readPosition += sizeof(bool);
        return BitConverter.ToBoolean(data);
    }

    public short ReadInt16()
    {
        var data = _buffer.Slice(_readPosition, sizeof(short));
        _readPosition += sizeof(short);
        return BitConverter.ToInt16(data);
    }

    public ushort ReadUInt16()
    {
        var data = _buffer.Slice(_readPosition, sizeof(ushort));
        _readPosition += sizeof(ushort);
        return BitConverter.ToUInt16(data);
    }

    public int ReadInt32()
    {
        var data = _buffer.Slice(_readPosition, sizeof(int));
        _readPosition += sizeof(int);
        return BitConverter.ToInt32(data);
    }

    public uint ReadUInt32()
    {
        var data = _buffer.Slice(_readPosition, sizeof(uint));
        _readPosition += sizeof(uint);
        return BitConverter.ToUInt32(data);
    }

    public long ReadInt64()
    {
        var data = _buffer.Slice(_readPosition, sizeof(long));
        _readPosition += sizeof(long);
        return BitConverter.ToInt64(data);
    }

    public ulong ReadUInt64()
    {
        var data = _buffer.Slice(_readPosition, sizeof(ulong));
        _readPosition += sizeof(ulong);
        return BitConverter.ToUInt64(data);
    }

    public float ReadSingle()
    {
        var data = _buffer.Slice(_readPosition, sizeof(float));
        _readPosition += sizeof(float);
        return BitConverter.ToSingle(data);
    }

    public double ReadDouble()
    {
        var data = _buffer.Slice(_readPosition, sizeof(double));
        _readPosition += sizeof(double);
        return BitConverter.ToDouble(data);
    }

    public char ReadChar()
    {
        var data = _buffer.Slice(_readPosition, sizeof(char));
        _readPosition += sizeof(char);
        return BitConverter.ToChar(data);
    }

    public DateTime ReadDateTime()
    {
        var data = _buffer.Slice(_readPosition, sizeof(long));
        _readPosition += sizeof(long);
        var result = BitConverter.ToInt64(data);
        return DateTime.FromBinary(result);
    }

    public Guid ReadGuid()
    {
        var data = _buffer.Slice(_readPosition, sizeof(decimal));
        _readPosition += sizeof(decimal);
        return new(data);
    }

    public string ReadString(Encoding encoding)
    {
        var dataSize = _buffer.Slice(_readPosition, sizeof(int));
        _readPosition += sizeof(int);
        var size = BitConverter.ToInt32(dataSize);
        var data = _buffer.Slice(_readPosition, size);
        _readPosition += size;
        var str = encoding.GetString(data);
        return str;
    }

    #endregion

    #region to
    /// <summary>
    /// Преобразует <see cref="BinaryBuffer"/> в <see cref="Span{T}"/> обрезая незадействоное пространство используя <see cref="Span{T}.Slice(int, int)"/>.
    /// </summary>
    /// <returns> <see cref="Span{T}"/> с размером <see cref="Size"/></returns>
    public Span<byte> ToSpan() => _buffer.Slice(_defaultReadStartPos, _writePosition);
    /// <summary>
    /// Преобразует <see cref="BinaryBuffer"/> в массив <see cref="byte"/> обрезая незадействоное пространство используя <see cref="Span{T}.Slice(int, int)"/>.
    /// </summary>
    /// <returns> массив <see cref="byte"/> с размером <see cref="Size"/></returns>
    public byte[] ToArray() => _buffer.Slice(_defaultReadStartPos, _writePosition).ToArray();
    #endregion

    #region stream 
    /// <summary>
    /// Записывает текущий объект <see cref="BinaryBuffer"/> в указанный <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream"><see cref="Stream"/> в который будут записанны данные находящиеся в буфере.</param>
    /// <returns><see cref="true"/> если данные были записанны, иначе <see cref="false"/></returns>
    public bool WriteToStream(Stream stream)
    {
        if (stream.CanWrite)
            return false;
        stream.Write(_buffer.Slice(_defaultWriteStartPos, _writePosition));
        return true;
    }
    #endregion

    public void ResetReadPosition() => _readPosition = 0;
    public void ResetWritePosition() => _writePosition = 0;

    public void ResetDefaultReadPosition() => _defaultReadStartPos = 0;
    public void ResetDefaultWritePosition() => _defaultWriteStartPos = 0;

    public void Clear()
    {
        _readPosition = 0;
        _writePosition = 0;
        _buffer.Clear();
    }
}
