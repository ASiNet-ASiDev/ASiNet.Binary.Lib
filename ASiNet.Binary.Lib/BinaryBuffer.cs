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
    public BinaryBuffer(Span<byte> area, Span<byte> buffer, ref int writePosition, ref int readPosition)
    {
        _area = area;
        _buffer = buffer;
        _writePosition = ref writePosition;
        _readPosition = ref readPosition;
    }

    #endregion

    /// <summary>
    /// Общий размер буфера.
    /// </summary>
    public readonly int Length => _area.Length;

    public readonly int FreeSpace => _area.Length - _writePosition;

    public int ReadPosition
    {
        get => _readPosition;
        set
        {
            if (value < 0 || value >= _area.Length)
                throw new IndexOutOfRangeException();
            _readPosition = value;
        }
    }
    /// <summary>
    /// Позиция в которую будут записанные последующие данные.
    /// </summary>
    public int WritePosition
    {
        get => _writePosition;
        set
        {
            if (value < 0 || value >= _area.Length)
                throw new IndexOutOfRangeException();
            _writePosition = value;
        }
    }

    private ref int _writePosition;
    private ref int _readPosition;

    private Span<byte> _area;
    private Span<byte> _buffer;

    public void ClearBuffer() => _buffer.Clear();
    public Span<byte> GetBuffer() => _buffer;

    public bool WriteBuffer(int count)
    {
        var offset = _writePosition + count;
        if (offset > _area.Length)
            throw new IndexOutOfRangeException();

        for (int i = 0; i < count; i++)
            _area[_writePosition + i] = _buffer[i];
        _writePosition = offset;

        _buffer.Clear();
        return true;
    }

    public bool ReadToBuffer(int count)
    {
        try
        {
            if (count > _buffer.Length)
                throw new IndexOutOfRangeException();
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

    public bool WriteSpan(Span<byte> data)
    {
        var offset = _writePosition + data.Length;
        if (offset > _area.Length)
            throw new IndexOutOfRangeException();

        for (int i = 0; i < data.Length; i++)
            _area[_writePosition + i] = data[i];
        _writePosition = offset;
        return true;
    }

    public bool WriteMemory(Memory<byte> value)
    {
        var offset = _writePosition + value.Length;
        if (offset > _area.Length)
            throw new IndexOutOfRangeException();
        var data = value.Span;

        for (int i = 0; i < data.Length; i++)
            _area[_writePosition + i] = data[i];
        _writePosition = offset;
        return true;
    }

    public bool WriteSpan(ReadOnlySpan<byte> data)
    {
        var offset = _writePosition + data.Length;
        if (offset > _area.Length)
            throw new IndexOutOfRangeException();

        for (int i = 0; i < data.Length; i++)
            _area[_writePosition + i] = data[i];
        _writePosition = offset;
        return true;
    }

    public bool WriteSpan(ReadOnlySpan<char> data, Encoding encoding)
    {
        var size = encoding.GetByteCount(data);
        var offset = _writePosition + size;
        if (offset > _area.Length)
            throw new IndexOutOfRangeException();

        Span<byte> buffer = stackalloc byte[size];
        encoding.GetBytes(data, buffer);

        for (int i = 0; i < buffer.Length; i++)
            _area[_writePosition + i] = buffer[i];
        _writePosition = offset;
        return true;
    }

    public bool ReadToSpan(Span<byte> dist)
    {
        var data = _area.Slice(_readPosition, dist.Length);
        data.CopyTo(dist);
        _readPosition += dist.Length;
        return true;
    }


    #region to
    /// <summary>
    /// Преобразует <see cref="BinaryBuffer"/> в <see cref="Span{T}"/> обрезая незадействоное пространство используя <see cref="Span{T}.Slice(int, int)"/>.
    /// </summary>
    /// <param name="trimStart">Если <see cref="true"/>, то обрезает начиная с <see cref="ReadPosition"/></param>
    /// <returns> <see cref="Span{T}"/> с размером <see cref="Size"/></returns>
    public Span<byte> ToSpan(bool trimStart = false) => trimStart ? _area.Slice(_readPosition, _writePosition) : _area.Slice(0, _writePosition);
    /// <summary>
    /// Преобразует <see cref="BinaryBuffer"/> в массив <see cref="byte"/> обрезая незадействоное пространство используя <see cref="Span{T}.Slice(int, int)"/>.
    /// </summary>
    /// <param name="trimStart">Если <see cref="true"/>, то обрезает начиная с <see cref="ReadPosition"/></param>
    /// <returns> массив <see cref="byte"/> с размером <see cref="Size"/></returns>
    public byte[] ToArray(bool trimStart = false) => (trimStart ? _area.Slice(_readPosition, _writePosition) : _area.Slice(0, _writePosition)).ToArray();
    /// <summary>
    /// Преобразует <see cref="BinaryBuffer"/> в массив <see cref="byte"/> обрезая незадействоное пространство используя <see cref="Span{T}.Slice(int, int)"/>.
    /// </summary>
    /// <param name="trimStart">Если <see cref="true"/>, то обрезает начиная с <see cref="ReadPosition"/></param>
    /// <returns> массив <see cref="byte"/> с размером <see cref="Size"/></returns>
    public List<byte> ToList(bool trimStart = false) => new((trimStart ? _area.Slice(_readPosition, _writePosition) : _area.Slice(0, _writePosition)).ToArray());
    #endregion

    #region stream 
    /// <summary>
    /// Записывает текущий объект <see cref="BinaryBuffer"/> в указанный <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream"><see cref="Stream"/> в который будут записанны данные находящиеся в буфере.</param>
    /// <returns><see cref="true"/> если данные были записанны, иначе <see cref="false"/></returns>
    public bool WriteToStream(Stream stream)
    {
        if (!stream.CanWrite)
            return false;
        stream.Write(_area.Slice(0, _writePosition));
        return true;
    }
    #endregion


    public void Clear()
    {
        _readPosition = 0;
        _writePosition = 0;
        _buffer.Clear();
        _area.Clear();
    }
}
