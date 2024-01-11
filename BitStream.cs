using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BitStreams;

/// <summary>
///     Stream wrapper to use Bit-level operations
/// </summary>
public class BitStream
{
    private Encoding encoding;
    private Stream stream;
    public long Offset { get; set; }
    public int Bit { get; set; }
    private bool MSB { get; }

    /// <summary>
    ///     Allows the <see cref="BitStream" /> auto increase in size when needed
    /// </summary>
    public bool AutoIncreaseStream { get; set; }

    /// <summary>
    ///     Get the stream length
    /// </summary>
    public long Length => stream.Length;

    /// <summary>
    ///     Get the current Bit position in the stream
    /// </summary>
    public long BitPosition => Bit;

    /// <summary>
    ///     Check if <see cref="BitStream" /> Offset is inside the stream length
    /// </summary>
    public bool ValidPosition => Offset < Length;

    #region Constructors

    /// <summary>
    ///     Creates a <see cref="BitStream" /> using a Stream
    /// </summary>
    /// <param name="stream">Stream to use</param>
    /// <param name="MSB">true if Most Significant Bit will be used, if false LSB will be used</param>
    public BitStream (Stream stream, bool MSB = false)
    {
        this.stream = new MemoryStream();
        stream.CopyTo(this.stream);
        this.MSB = MSB;
        Offset = 0;
        Bit = 0;
        encoding = Encoding.UTF8;
        AutoIncreaseStream = false;
    }

    /// <summary>
    ///     Creates a <see cref="BitStream" /> using a Stream
    /// </summary>
    /// <param name="stream">Stream to use</param>
    /// <param name="encoding">Encoding to use with chars</param>
    /// <param name="MSB">true if Most Significant Bit will be used, if false LSB will be used</param>
    public BitStream (Stream stream, Encoding encoding, bool MSB = false)
    {
        this.stream = new MemoryStream();
        stream.CopyTo(this.stream);
        this.MSB = MSB;
        Offset = 0;
        Bit = 0;
        this.encoding = encoding;
        AutoIncreaseStream = false;
    }

    /// <summary>
    ///     Creates a <see cref="BitStream" /> using a byte[]
    /// </summary>
    /// <param name="buffer">byte[] to use</param>
    /// <param name="MSB">true if Most Significant Bit will be used, if false LSB will be used</param>
    public BitStream (byte[] buffer, bool MSB = false)
    {
        stream = new MemoryStream();
        var m = new MemoryStream(buffer);
        m.CopyTo(stream);
        this.MSB = MSB;
        Offset = 0;
        Bit = 0;
        encoding = Encoding.UTF8;
        AutoIncreaseStream = false;
    }

    /// <summary>
    ///     Creates a <see cref="BitStream" /> using a byte[]
    /// </summary>
    /// <param name="buffer">byte[] to use</param>
    /// <param name="encoding">Encoding to use with chars</param>
    /// <param name="MSB">true if Most Significant Bit will be used, if false LSB will be used</param>
    public BitStream (byte[] buffer, Encoding encoding, bool MSB = false)
    {
        stream = new MemoryStream();
        var m = new MemoryStream(buffer);
        m.CopyTo(stream);
        this.MSB = MSB;
        Offset = 0;
        Bit = 0;
        this.encoding = encoding;
        AutoIncreaseStream = false;
    }

    /// <summary>
    ///     Creates a <see cref="BitStream" /> using a byte[]
    /// </summary>
    /// <param name="buffer">byte[] to use</param>
    /// <param name="MSB">true if Most Significant Bit will be used, if false LSB will be used</param>
    public static BitStream Create (byte[] buffer, bool MSB = false)
    {
        return new BitStream(buffer, MSB);
    }

    /// <summary>
    ///     Creates a <see cref="BitStream" /> using a byte[]
    /// </summary>
    /// <param name="buffer">byte[] to use</param>
    /// <param name="encoding">
    ///     Encoding to use with chars/param>
    ///     <param name="MSB">true if Most Significant Bit will be used, if false LSB will be used</param>
    public static BitStream Create (byte[] buffer, Encoding encoding, bool MSB = false)
    {
        return new BitStream(buffer, encoding, MSB);
    }

    /// <summary>
    ///     Creates a <see cref="BitStream" /> using a file path, throws IOException if file doesn't exists or path is not a
    ///     file
    /// </summary>
    /// <param name="path">File path</param>
    /// <param name="encoding">Encoding of the file, if null default <see cref="Encoding" /> will be used</param>
    /// <returns></returns>
    public static BitStream CreateFromFile (string path, Encoding encoding = null)
    {
        if (!File.Exists(path))
        {
            throw new IOException("File doesn't exists!");
        }

        if (File.GetAttributes(path) == FileAttributes.Directory)
        {
            throw new IOException("Path is a directory!");
        }

        if (encoding == null)
        {
            encoding = Encoding.UTF8;
        }

        return new BitStream(File.ReadAllBytes(path), encoding);
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Seek to the specified Offset and check if it is a valid position for reading in the stream
    /// </summary>
    /// <param name="offset">Offset on the stream</param>
    /// <param name="bit">Bit position</param>
    /// <returns>true if Offset is valid to do reading, false otherwise</returns>
    public bool this [long offset, int bit]
    {
        get
        {
            Seek(offset, bit);
            return ValidPosition;
        }
        //set {
        //    Seek(Offset, Bit);
        //}
        private set { }
    }

    /// <summary>
    ///     Seek through the stream selecting the Offset and Bit using <see cref="SeekOrigin.Begin" />
    /// </summary>
    /// <param name="offset">Offset on the stream</param>
    /// <param name="bit">Bit position</param>
    public void Seek (long offset, int bit)
    {
        if (offset > Length)
        {
            Offset = Length;
        }
        else
        {
            if (offset >= 0)
            {
                Offset = offset;
            }
            else
            {
                offset = 0;
            }
        }

        if (bit >= 8)
        {
            var n = bit / 8;
            Offset += n;
            Bit = bit % 8;
        }
        else
        {
            Bit = bit;
        }

        stream.Seek(offset, SeekOrigin.Begin);
    }

    /// <summary>
    ///     Advances the stream by one Bit
    /// </summary>
    public void AdvanceBit ()
    {
        Bit = (Bit + 1) % 8;
        if (Bit == 0)
        {
            Offset++;
        }
    }

    /// <summary>
    ///     Returns the stream by one Bit
    /// </summary>
    public void ReturnBit ()
    {
        Bit = Bit - 1 == -1 ? 7 : Bit - 1;
        if (Bit == 7)
        {
            Offset--;
        }

        if (Offset < 0)
        {
            Offset = 0;
        }
    }

    /// <summary>
    ///     Get the edited stream
    /// </summary>
    /// <returns>Modified stream</returns>
    public Stream GetStream ()
    {
        return stream;
    }

    /// <summary>
    ///     Get the stream data as a byte[]
    /// </summary>
    /// <returns>Stream as byte[]</returns>
    public byte[] GetStreamData ()
    {
        stream.Seek(0, SeekOrigin.Begin);
        var s = new MemoryStream();
        stream.CopyTo(s);
        Seek(Offset, Bit);
        return s.ToArray();
    }

    /// <summary>
    ///     Get the stream data as a byte[] from current offset to the end
    /// </summary>
    /// <returns>Stream as byte[]</returns>
    public byte[] GetStreamDataFromCurrentOffsetAndBit ()
    {
        var offset = Offset;
        var bit = Bit;

        var bits = ReadBits(int.MaxValue);

        Seek(offset, bit);

        return BitArrayToBytes(bits);
    }

    public byte[] GetStreamDataBetween (long offsetStart, int bitStart, long offsetEnd, int bitEnd)
    {
        var length = (offsetEnd - offsetStart) * 8 + (bitStart == 0 ? 0 : 8 - bitStart) + bitEnd;

        var offset = Offset;
        var bit = Bit;

        Seek(offsetStart, bitStart);
        var bits = ReadBits(length);

        Seek(offset, bit);

        return BitArrayToBytes(bits);
    }

    public static byte[] BitArrayToBytes (Bit[] bits)
    {
        if (bits == null || bits.Length == 0)
        {
            return new byte[] { };
        }

        var result = new List<byte>();
        var curr = 0;

        while (curr < bits.Length - 8)
        {
            var val = 0;
            for (var i = curr + 7; i >= curr; i--)
            {
                val <<= 1;
                val += bits[i];
            }

            result.Add((byte) val);
            curr += 8;
        }

        var valEnd = 0;
        for (var i = bits.Length - 1; i >= curr; i--)
        {
            valEnd <<= 1;
            valEnd += bits[i];
        }

        result.Add((byte) valEnd);

        return result.ToArray();
    }


    /// <summary>
    ///     Get the <see cref="Encoding" /> used for chars and strings
    /// </summary>
    /// <returns><see cref="Encoding" /> used</returns>
    public Encoding GetEncoding ()
    {
        return encoding;
    }

    /// <summary>
    ///     Set the <see cref="Encoding" /> that will be used for chars and strings
    /// </summary>
    /// <param name="encoding"><see cref="Encoding" /> to use</param>
    public void SetEncoding (Encoding encoding)
    {
        this.encoding = encoding;
    }

    /// <summary>
    ///     Changes the length of the stream, if new length is less than current length stream data will be truncated
    /// </summary>
    /// <param name="length">New stream length</param>
    /// <returns>return true if stream changed length, false if it wasn't possible</returns>
    public bool ChangeLength (long length)
    {
        if (stream.CanSeek && stream.CanWrite)
        {
            stream.SetLength(length);
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Cuts the <see cref="BitStream" /> from the specified Offset and given length, will throw an exception when length +
    ///     Offset is higher than stream's length, Offset and Bit will be set to 0
    /// </summary>
    /// <param name="offset">Offset to start</param>
    /// <param name="length">Length of the new <see cref="BitStream" /></param>
    public void CutStream (long offset, long length)
    {
        var data = GetStreamData();
        var buffer = new byte[length];
        Array.Copy(data, offset, buffer, 0, length);
        stream = new MemoryStream();
        var m = new MemoryStream(buffer);
        stream = new MemoryStream();
        m.CopyTo(stream);
        Offset = 0;
        Bit = 0;
    }

    /// <summary>
    ///     Copies the current <see cref="BitStream" /> buffer to another <see cref="Stream" />
    /// </summary>
    /// <param name="stream"><see cref="Stream" /> to copy buffer</param>
    public void CopyStreamTo (Stream stream)
    {
        Seek(0, 0);
        stream.SetLength(this.stream.Length);
        this.stream.CopyTo(stream);
    }

    /// <summary>
    ///     Copies the current <see cref="BitStream" /> buffer to another <see cref="BitStream" />
    /// </summary>
    /// <param name="stream"><see cref="BitStream" /> to copy buffer</param>
    public void CopyStreamTo (BitStream stream)
    {
        Seek(0, 0);
        stream.ChangeLength(this.stream.Length);
        this.stream.CopyTo(stream.stream);
        stream.Seek(0, 0);
    }

    /// <summary>
    ///     Saves current <see cref="BitStream" /> buffer into a file
    /// </summary>
    /// <param name="filename">File to write data, if it exists it will be overwritten</param>
    public void SaveStreamAsFile (string filename)
    {
        File.WriteAllBytes(filename, GetStreamData());
    }

    /// <summary>
    ///     Returns the current content of the stream as a <see cref="MemoryStream" />
    /// </summary>
    /// <returns><see cref="MemoryStream" /> containing current <see cref="BitStream" /> data</returns>
    public MemoryStream CloneAsMemoryStream ()
    {
        return new MemoryStream(GetStreamData());
    }

    /// <summary>
    ///     Returns the current content of the stream as a <see cref="BufferedStream" />
    /// </summary>
    /// <returns><see cref="BufferedStream" /> containing current <see cref="BitStream" /> data</returns>
    public BufferedStream CloneAsBufferedStream ()
    {
        var bs = new BufferedStream(stream);
        var sw = new StreamWriter(bs);
        sw.Write(GetStreamData());
        bs.Seek(0, SeekOrigin.Begin);
        return bs;
    }


    /// <summary>
    ///     Checks if the <see cref="BitStream" /> will be in a valid position on its last Bit read/write
    /// </summary>
    /// <param name="bits">Number of bits it will advance</param>
    /// <returns>true if <see cref="BitStream" /> will be inside the stream length</returns>
    public bool ValidPositionWhen (int bits)
    {
        var o = Offset;
        var b = Bit;
        b = (b + 1) % 8;
        if (b == 0)
        {
            o++;
        }

        return o < Length;
    }

    public long BitOffsetFromStart => Offset * 8 + Bit;

    #endregion

    #region BitRead/Write

    /// <summary>
    ///     Read current position Bit and advances the position within the stream by one Bit
    /// </summary>
    /// <returns>Returns the current position Bit as 0 or 1</returns>
    public Bit ReadBit ()
    {
        if (!ValidPosition)
        {
            throw new IOException("Cannot read in an Offset bigger than the length of the stream");
        }

        stream.Seek(Offset, SeekOrigin.Begin);
        byte value;
        if (!MSB)
        {
            value = (byte) ((stream.ReadByte() >> Bit) & 1);
        }
        else
        {
            value = (byte) ((stream.ReadByte() >> (7 - Bit)) & 1);
        }

        AdvanceBit();
        stream.Seek(Offset, SeekOrigin.Begin);
        return value;
    }

    /// <summary>
    ///     Read from current position the specified number of bits
    /// </summary>
    /// <param name="length">Bits to read</param>
    /// <param name="readToEndIfTooLong">If true, will read every available Bit up to length</param>
    /// <returns><see cref="BitStreams.Bit" />[] containing read bits</returns>
    public Bit[] ReadBits (long length, bool readToEndIfTooLong = true)
    {
        var bits = new List<Bit>();
        for (long i = 0; i < length; i++)
        {
            if (readToEndIfTooLong && !ValidPosition)
            {
                return bits.ToArray();
            }

            bits.Add(ReadBit());
        }

        return bits.ToArray();
    }

    /// <summary>
    ///     Writes a Bit in the current position
    /// </summary>
    /// <param name="data">Bit to write, it data is not 0 or 1 data = data & 1</param>
    public void WriteBit (Bit data)
    {
        stream.Seek(Offset, SeekOrigin.Begin);
        var value = (byte) stream.ReadByte();
        stream.Seek(Offset, SeekOrigin.Begin);
        if (!MSB)
        {
            value &= (byte) ~(1 << Bit);
            value |= (byte) (data << Bit);
        }
        else
        {
            value &= (byte) ~(1 << (7 - Bit));
            value |= (byte) (data << (7 - Bit));
        }

        if (ValidPosition)
        {
            stream.WriteByte(value);
        }
        else
        {
            if (AutoIncreaseStream)
            {
                if (ChangeLength(Length + (Offset - Length) + 1))
                {
                    stream.WriteByte(value);
                }
                else
                {
                    throw new IOException("Cannot write in an Offset bigger than the length of the stream");
                }
            }
            else
            {
                throw new IOException("Cannot write in an Offset bigger than the length of the stream");
            }
        }

        AdvanceBit();
        stream.Seek(Offset, SeekOrigin.Begin);
    }

    /// <summary>
    ///     Write a sequence of bits into the stream
    /// </summary>
    /// <param name="bits"><see cref="BitStreams.Bit" />[] to write</param>
    public void WriteBits (ICollection<Bit> bits)
    {
        foreach (var b in bits)
        {
            WriteBit(b);
        }
    }

    /// <summary>
    ///     Write a sequence of bits into the stream
    /// </summary>
    /// <param name="bits"><see cref="BitStreams.Bit" />[] to write</param>
    /// <param name="length">Number of bits to write</param>
    public void WriteBits (ICollection<Bit> bits, int length)
    {
        var b = new Bit[bits.Count];
        bits.CopyTo(b, 0);
        for (var i = 0; i < length; i++)
        {
            WriteBit(b[i]);
        }
    }

    /// <summary>
    ///     Write a sequence of bits into the stream
    /// </summary>
    /// <param name="bits"><see cref="BitStreams.Bit" />[] to write</param>
    /// <param name="offset">Offset to begin Bit writing</param>
    /// <param name="length">Number of bits to write</param>
    public void WriteBits (Bit[] bits, int offset, int length)
    {
        for (var i = offset; i < length; i++)
        {
            WriteBit(bits[i]);
        }
    }

    #endregion

    #region Read

    /// <summary>
    ///     Read from the current position Bit the specified number of bits or bytes and creates a byte[]
    /// </summary>
    /// <param name="length">Number of bits or bytes</param>
    /// <param name="isBytes">if true will consider length as byte length, if false it will count the specified length of bits</param>
    /// <returns>byte[] containing bytes created from current position</returns>
    public byte[] ReadBytes (long length, bool isBytes = false)
    {
        if (isBytes)
        {
            length *= 8;
        }

        var data = new List<byte>();
        for (long i = 0; i < length;)
        {
            if (!ValidPosition)
            {
                for (var t = data.Count; t <= (length - 1) / 8; t++)
                {
                    data.Add(0);
                }

                return data.ToArray();
            }

            byte value = 0;
            for (var p = 0; p < 8 && i < length; i++, p++)
            {
                if (!ValidPosition)
                {
                    if (p != 0)
                    {
                        data.Add(value);
                    }

                    for (var t = data.Count; t <= (length - 1) / 8; t++)
                    {
                        data.Add(0);
                    }

                    return data.ToArray();
                }

                if (!MSB)
                {
                    value |= (byte) (ReadBit() << p);
                }
                else
                {
                    value |= (byte) (ReadBit() << (7 - p));
                }
            }

            data.Add(value);
        }

        return data.ToArray();
    }

    /// <summary>
    ///     Read a byte based on the current stream and Bit position
    /// </summary>
    public byte ReadByte ()
    {
        return ReadBytes(8)[0];
    }

    /// <summary>
    ///     Read a byte made of specified number of bits (1-8)
    /// </summary>
    public byte ReadByte (int bits)
    {
        if (bits < 0)
        {
            bits = 0;
        }

        if (bits > 8)
        {
            bits = 8;
        }

        return ReadBytes(bits)[0];
    }

    /// <summary>
    ///     Read a signed byte based on the current stream and Bit position
    /// </summary>
    public sbyte ReadSByte ()
    {
        return (sbyte) ReadBytes(8)[0];
    }

    /// <summary>
    ///     Read a sbyte made of specified number of bits (1-8)
    /// </summary>
    public sbyte ReadSByte (int bits)
    {
        if (bits < 0)
        {
            bits = 0;
        }

        if (bits > 8)
        {
            bits = 8;
        }

        return (sbyte) ReadBytes(bits)[0];
    }

    /// <summary>
    ///     Read a byte based on the current stream and Bit position and check if it is 0
    /// </summary>
    public bool ReadBool ()
    {
        return ReadBytes(8)[0] != 0;
    }

    /// <summary>
    ///     Read a char based on the current stream and Bit position and the <see cref="BitStream" /> encoding
    /// </summary>
    public char ReadChar ()
    {
        return encoding.GetChars(ReadBytes(encoding.GetMaxByteCount(1) * 8))[0];
    }

    /// <summary>
    ///     Read a string based on the current stream and Bit position and the <see cref="BitStream" /> encoding
    /// </summary>
    /// <param name="length">Length of the string to read</param>
    public string ReadString (int length)
    {
        var bitsPerChar = encoding.GetByteCount(" ") * 8;
        return encoding.GetString(ReadBytes(bitsPerChar * length));
    }

    /// <summary>
    ///     Read a short based on the current stream and Bit position
    /// </summary>
    public short ReadInt16 ()
    {
        var value = BitConverter.ToInt16(ReadBytes(16), 0);
        return value;
    }

    /// <summary>
    ///     Read a 24bit value based on the current stream and Bit position
    /// </summary>
    public Int24 ReadInt24 ()
    {
        var bytes = ReadBytes(24);
        Array.Resize(ref bytes, 4);
        Int24 value = BitConverter.ToInt32(bytes, 0);
        return value;
    }

    /// <summary>
    ///     Read an int based on the current stream and Bit position
    /// </summary>
    public int ReadInt32 ()
    {
        var value = BitConverter.ToInt32(ReadBytes(32), 0);
        return value;
    }

    /// <summary>
    ///     Read a 48bit value based on the current stream and Bit position
    /// </summary>
    public Int48 ReadInt48 ()
    {
        var bytes = ReadBytes(48);
        Array.Resize(ref bytes, 8);
        Int48 value = BitConverter.ToInt64(bytes, 0);
        return value;
    }

    /// <summary>
    ///     Read a long based on the current stream and Bit position
    /// </summary>
    public long ReadInt64 ()
    {
        var value = BitConverter.ToInt64(ReadBytes(64), 0);
        return value;
    }

    /// <summary>
    ///     Read a ushort based on the current stream and Bit position
    /// </summary>
    public ushort ReadUInt16 ()
    {
        var bytes = ReadBytes(16);

        if (bytes.Length == 1)
        {
            bytes = new byte[] { 0, bytes[0] };
        }

        var value = BitConverter.ToUInt16(bytes, 0);
        return value;
    }

    /// <summary>
    ///     Read an unsigned 24bit value based on the current stream and Bit position
    /// </summary>
    public UInt24 ReadUInt24 ()
    {
        var bytes = ReadBytes(24);
        Array.Resize(ref bytes, 4);
        UInt24 value = BitConverter.ToUInt32(bytes, 0);
        return value;
    }

    /// <summary>
    ///     Read an uint based on the current stream and Bit position
    /// </summary>
    public uint ReadUInt32 ()
    {
        var value = BitConverter.ToUInt32(ReadBytes(32), 0);
        return value;
    }

    /// <summary>
    ///     Read an unsigned 48bit value based on the current stream and Bit position
    /// </summary>
    public UInt48 ReadUInt48 ()
    {
        var bytes = ReadBytes(48);
        Array.Resize(ref bytes, 8);
        UInt48 value = BitConverter.ToUInt64(bytes, 0);
        return value;
    }

    /// <summary>
    ///     Read an ulong based on the current stream and Bit position
    /// </summary>
    public ulong ReadUInt64 ()
    {
        var value = BitConverter.ToUInt64(ReadBytes(64), 0);
        return value;
    }

    #endregion

    #region Write

    /// <summary>
    ///     Writes as bits a byte[] by a specified number of bits or bytes
    /// </summary>
    /// <param name="data">byte[] to write</param>
    /// <param name="length">Number of bits or bytes to use from the array</param>
    /// <param name="isBytes">if true will consider length as byte length, if false it will count the specified length of bits</param>
    public void WriteBytes (byte[] data, long length, bool isBytes = false)
    {
        if (isBytes)
        {
            length *= 8;
        }

        var position = 0;
        for (long i = 0; i < length;)
        {
            byte value = 0;
            for (var p = 0; p < 8 && i < length; i++, p++)
            {
                if (!MSB)
                {
                    value = (byte) ((data[position] >> p) & 1);
                }
                else
                {
                    value = (byte) ((data[position] >> (7 - p)) & 1);
                }

                WriteBit(value);
            }

            position++;
        }
    }

    public void WriteBytes (byte[] data)
    {
        WriteBytes(data, data.Length, true);
    }

    /// <summary>
    ///     Write a byte value based on the current stream and Bit position
    /// </summary>
    public void WriteByte (byte value)
    {
        WriteBytes(new[] { value }, 8);
    }

    /// <summary>
    ///     Write a byte value based on the current stream and Bit position
    /// </summary>
    public void WriteByte (byte value, int bits)
    {
        if (bits < 0)
        {
            bits = 0;
        }

        if (bits > 8)
        {
            bits = 8;
        }

        WriteBytes(new[] { value }, bits);
    }

    /// <summary>
    ///     Write a byte value based on the current stream and Bit position
    /// </summary>
    public void WriteSByte (sbyte value)
    {
        WriteBytes(new[] { (byte) value }, 8);
    }

    /// <summary>
    ///     Write a byte value based on the current stream and Bit position
    /// </summary>
    public void WriteSByte (sbyte value, int bits)
    {
        if (bits < 0)
        {
            bits = 0;
        }

        if (bits > 8)
        {
            bits = 8;
        }

        WriteBytes(new[] { (byte) value }, bits);
    }

    /// <summary>
    ///     Write a bool value as 0:false, 1:true as byte based on the current stream and Bit position
    /// </summary>
    public void WriteBool (bool value)
    {
        WriteBytes(new[] { value ? (byte) 1 : (byte) 0 }, 8);
    }

    /// <summary>
    ///     Write a char value based on the <see cref="BitStream" /> encoding
    /// </summary>
    public void WriteChar (char value)
    {
        var bytes = encoding.GetBytes(new[] { value }, 0, 1);
        WriteBytes(bytes, bytes.Length * 8);
    }

    /// <summary>
    ///     Write a string based on the <see cref="BitStream" /> encoding
    /// </summary>
    public void WriteString (string value)
    {
        var bytes = encoding.GetBytes(value);
        WriteBytes(bytes, bytes.Length * 8);
    }

    /// <summary>
    ///     Write a short value based on the current stream and Bit position
    /// </summary>
    public void WriteInt16 (short value)
    {
        WriteBytes(BitConverter.GetBytes(value), 16);
    }

    /// <summary>
    ///     Write a 24bit value based on the current stream and Bit position
    /// </summary>
    public void WriteInt24 (Int24 value)
    {
        WriteBytes(BitConverter.GetBytes(value), 24);
    }

    /// <summary>
    ///     Write an int value based on the current stream and Bit position
    /// </summary>
    public void WriteInt32 (int value)
    {
        WriteBytes(BitConverter.GetBytes(value), 32);
    }

    /// <summary>
    ///     Write a 48bit value based on the current stream and Bit position
    /// </summary>
    public void WriteInt48 (Int48 value)
    {
        WriteBytes(BitConverter.GetBytes(value), 48);
    }

    /// <summary>
    ///     Write a long value based on the current stream and Bit position
    /// </summary>
    public void WriteInt64 (long value)
    {
        WriteBytes(BitConverter.GetBytes(value), 64);
    }

    /// <summary>
    ///     Write an ushort value based on the current stream and Bit position
    /// </summary>
    public void WriteUInt16 (ushort value)
    {
        WriteBytes(BitConverter.GetBytes(value), 16);
    }

    /// <summary>
    ///     Write an unsigned 24bit value based on the current stream and Bit position
    /// </summary>
    public void WriteUInt24 (UInt24 value)
    {
        WriteBytes(BitConverter.GetBytes(value), 24);
    }

    /// <summary>
    ///     Write an uint value based on the current stream and Bit position
    /// </summary>
    public void WriteUInt32 (uint value)
    {
        WriteBytes(BitConverter.GetBytes(value), 32);
    }

    /// <summary>
    ///     Write an unsigned 48bit value based on the current stream and Bit position
    /// </summary>
    public void WriteUInt48 (UInt48 value)
    {
        WriteBytes(BitConverter.GetBytes(value), 48);
    }

    /// <summary>
    ///     Write an ulong value based on the current stream and Bit position
    /// </summary>
    public void WriteUInt64 (ulong value)
    {
        WriteBytes(BitConverter.GetBytes(value), 64);
    }

    #endregion

    #region Shifts

    /// <summary>
    ///     Do a bitwise shift on the current position of the stream on Bit 0
    /// </summary>
    /// <param name="bits">bits to shift</param>
    /// <param name="leftShift">true to left shift, false to right shift</param>
    public void bitwiseShift (int bits, bool leftShift)
    {
        if (!ValidPositionWhen(8))
        {
            throw new IOException("Cannot read in an Offset bigger than the length of the stream");
        }

        Seek(Offset, 0);
        if (bits != 0 && bits <= 7)
        {
            var value = (byte) stream.ReadByte();
            if (leftShift)
            {
                value = (byte) (value << bits);
            }
            else
            {
                value = (byte) (value >> bits);
            }

            Seek(Offset, 0);
            stream.WriteByte(value);
        }

        Bit = 0;
        Offset++;
    }

    /// <summary>
    ///     Do a bitwise shift on the current position of the stream on current Bit
    /// </summary>
    /// <param name="bits">bits to shift</param>
    /// <param name="leftShift">true to left shift, false to right shift</param>
    public void bitwiseShiftOnBit (int bits, bool leftShift)
    {
        if (!ValidPositionWhen(8))
        {
            throw new IOException("Cannot read in an Offset bigger than the length of the stream");
        }

        Seek(Offset, Bit);
        if (bits != 0 && bits <= 7)
        {
            var value = ReadByte();
            if (leftShift)
            {
                value = (byte) (value << bits);
            }
            else
            {
                value = (byte) (value >> bits);
            }

            Offset--;
            Seek(Offset, Bit);
            WriteByte(value);
        }

        Offset++;
    }

    /// <summary>
    ///     Do a circular shift on the current position of the stream on Bit 0
    /// </summary>
    /// <param name="bits">bits to shift</param>
    /// <param name="leftShift">true to left shift, false to right shift</param>
    public void circularShift (int bits, bool leftShift)
    {
        if (!ValidPositionWhen(8))
        {
            throw new IOException("Cannot read in an Offset bigger than the length of the stream");
        }

        Seek(Offset, 0);
        if (bits != 0 && bits <= 7)
        {
            var value = (byte) stream.ReadByte();
            if (leftShift)
            {
                value = (byte) ((value << bits) | (value >> (8 - bits)));
            }
            else
            {
                value = (byte) ((value >> bits) | (value << (8 - bits)));
            }

            Seek(Offset, 0);
            stream.WriteByte(value);
        }

        Bit = 0;
        Offset++;
    }

    /// <summary>
    ///     Do a circular shift on the current position of the stream on current Bit
    /// </summary>
    /// <param name="bits">bits to shift</param>
    /// <param name="leftShift">true to left shift, false to right shift</param>
    public void circularShiftOnBit (int bits, bool leftShift)
    {
        if (!ValidPositionWhen(8))
        {
            throw new IOException("Cannot read in an Offset bigger than the length of the stream");
        }

        Seek(Offset, Bit);
        if (bits != 0 && bits <= 7)
        {
            var value = ReadByte();
            if (leftShift)
            {
                value = (byte) ((value << bits) | (value >> (8 - bits)));
            }
            else
            {
                value = (byte) ((value >> bits) | (value << (8 - bits)));
            }

            Offset--;
            Seek(Offset, Bit);
            WriteByte(value);
        }

        Offset++;
    }

    #endregion

    #region Bitwise Operators

    /// <summary>
    ///     Apply an and operator on the current stream and Bit position byte and advances one byte position
    /// </summary>
    /// <param name="x">Byte value to apply and</param>
    public void And (byte x)
    {
        if (!ValidPositionWhen(8))
        {
            throw new IOException("Cannot read in an Offset bigger than the length of the stream");
        }

        Seek(Offset, Bit);
        var value = ReadByte();
        Offset--;
        Seek(Offset, Bit);
        WriteByte((byte) (value & x));
    }

    /// <summary>
    ///     Apply an or operator on the current stream and Bit position byte and advances one byte position
    /// </summary>
    /// <param name="x">Byte value to apply or</param>
    public void Or (byte x)
    {
        if (!ValidPositionWhen(8))
        {
            throw new IOException("Cannot read in an Offset bigger than the length of the stream");
        }

        Seek(Offset, Bit);
        var value = ReadByte();
        Offset--;
        Seek(Offset, Bit);
        WriteByte((byte) (value | x));
    }

    /// <summary>
    ///     Apply a xor operator on the current stream and Bit position byte and advances one byte position
    /// </summary>
    /// <param name="x">Byte value to apply xor</param>
    public void Xor (byte x)
    {
        if (!ValidPositionWhen(8))
        {
            throw new IOException("Cannot read in an Offset bigger than the length of the stream");
        }

        Seek(Offset, Bit);
        var value = ReadByte();
        Offset--;
        Seek(Offset, Bit);
        WriteByte((byte) (value ^ x));
    }

    /// <summary>
    ///     Apply a not operator on the current stream and Bit position byte and advances one byte position
    /// </summary>
    public void Not ()
    {
        if (!ValidPositionWhen(8))
        {
            throw new IOException("Cannot read in an Offset bigger than the length of the stream");
        }

        Seek(Offset, Bit);
        var value = ReadByte();
        Offset--;
        Seek(Offset, Bit);
        WriteByte((byte) ~value);
    }

    /// <summary>
    ///     Apply an and operator on the current stream and Bit position and advances one Bit position
    /// </summary>
    /// <param name="bit">Bit value to apply and</param>
    public void BitAnd (Bit x)
    {
        if (!ValidPosition)
        {
            throw new IOException("Cannot read in an Offset bigger than the length of the stream");
        }

        Seek(Offset, Bit);
        var value = ReadBit();
        ReturnBit();
        WriteBit(x & value);
    }

    /// <summary>
    ///     Apply an or operator on the current stream and Bit position and advances one Bit position
    /// </summary>
    /// <param name="bit">Bit value to apply or</param>
    public void BitOr (Bit x)
    {
        if (!ValidPosition)
        {
            throw new IOException("Cannot read in an Offset bigger than the length of the stream");
        }

        Seek(Offset, Bit);
        var value = ReadBit();
        ReturnBit();
        WriteBit(x | value);
    }

    /// <summary>
    ///     Apply a xor operator on the current stream and Bit position and advances one Bit position
    /// </summary>
    /// <param name="bit">Bit value to apply xor</param>
    public void BitXor (Bit x)
    {
        if (!ValidPosition)
        {
            throw new IOException("Cannot read in an Offset bigger than the length of the stream");
        }

        Seek(Offset, Bit);
        var value = ReadBit();
        ReturnBit();
        WriteBit(x ^ value);
    }

    /// <summary>
    ///     Apply a not operator on the current stream and Bit position and advances one Bit position
    /// </summary>
    public void BitNot ()
    {
        if (!ValidPosition)
        {
            throw new IOException("Cannot read in an Offset bigger than the length of the stream");
        }

        Seek(Offset, Bit);
        var value = ReadBit();
        ReturnBit();
        WriteBit(~value);
    }

    /// <summary>
    ///     Reverses the Bit order on the byte in the current position of the stream
    /// </summary>
    public void ReverseBits ()
    {
        if (!ValidPosition)
        {
            throw new IOException("Cannot read in an Offset bigger than the length of the stream");
        }

        Seek(Offset, 0);
        var value = ReadByte();
        Offset--;
        Seek(Offset, 0);
        WriteByte(value.ReverseBits());
    }

    #endregion
}