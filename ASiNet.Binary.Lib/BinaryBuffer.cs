using System;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
    public BinaryBuffer(Span<byte> area, Span<byte> buffer)
    {
        _area = area;
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
        _area = buffer;
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
        _area = buffer;
        _writePosition = writeOffset;
        _readPosition = readOffset;
    }
    #endregion

    /// <summary>
    /// Общий размер буфера.
    /// </summary>
    public int Length => _area.Length;
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

    private Span<byte> _area;
    private Span<byte> _buffer;

    public void ClearBuffer() => _buffer.Clear();
    public Span<byte> GetBuffer() => _buffer;

    public bool WriteBuffer(ushort count)
    {
        var offset = _writePosition + count;
        if (offset > _area.Length)
            return false;

        for (int i = 0; i < count; i++)
            _area[_writePosition + i] = _buffer[i];
        _writePosition = offset;

        _buffer.Clear();
        return true;
    }

    public bool ReadToBuffer(ushort count)
    {
        try
        {
            if(count > _buffer.Length)
                return false;
            _buffer.Clear();
            var data = _area.Slice(_readPosition, count);
            data.CopyTo(_buffer);
            _readPosition += count;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool WriteSpan(in Span<byte> data)
    {
        var offset = _writePosition + data.Length;
        if (offset > _area.Length)
            return false;

        for (int i = 0; i < data.Length; i++)
            _area[_writePosition + i] = data[i];
        _writePosition = offset;
        return true;
    }

    public bool WriteMemory(in Memory<byte> value)
    {
        var offset = _writePosition + value.Length;
        if (offset > _area.Length)
            return false;
        var data = value.Span;

        for (int i = 0; i < data.Length; i++)
            _area[_writePosition + i] = data[i];
        _writePosition = offset;
        return true;
    }

    public bool WriteSpan(in ReadOnlySpan<byte> data)
    {
        var offset = _writePosition + data.Length;
        if (offset > _area.Length)
            return false;

        for (int i = 0; i < data.Length; i++)
            _area[_writePosition + i] = data[i];
        _writePosition = offset;
        return true;
    }

    public bool WriteSpan(in ReadOnlySpan<char> data, Encoding encoding)
    {
        var size = encoding.GetByteCount(data);
        var offset = _writePosition + size;
        if (offset > _area.Length)
            return false;

        Span<byte> buffer = stackalloc byte[size];
        encoding.GetBytes(data, buffer);

        for (int i = 0; i < buffer.Length; i++)
            _area[_writePosition + i] = buffer[i];
        _writePosition = offset;
        return true;
    }

    #region to
    /// <summary>
    /// Преобразует <see cref="BinaryBuffer"/> в <see cref="Span{T}"/> обрезая незадействоное пространство используя <see cref="Span{T}.Slice(int, int)"/>.
    /// </summary>
    /// <returns> <see cref="Span{T}"/> с размером <see cref="Size"/></returns>
    public Span<byte> ToSpan(bool trimStart = false) => trimStart ? _area.Slice(ReadPosition, _writePosition) : _area.Slice(_defaultReadStartPos, _writePosition);
    /// <summary>
    /// Преобразует <see cref="BinaryBuffer"/> в массив <see cref="byte"/> обрезая незадействоное пространство используя <see cref="Span{T}.Slice(int, int)"/>.
    /// </summary>
    /// <returns> массив <see cref="byte"/> с размером <see cref="Size"/></returns>
    public byte[] ToArray(bool trimStart = false) => trimStart ? _area.Slice(ReadPosition, _writePosition).ToArray() : _area.Slice(_defaultReadStartPos, _writePosition).ToArray();
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
        stream.Write(_area.Slice(_defaultWriteStartPos, _writePosition));
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
        _area.Clear();
    }
}
