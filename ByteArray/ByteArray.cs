using Ionic.Zlib;
using System;
using System.Linq;
using System.Text;

namespace Flash.Utils
{
    public class ByteArray
    {
        private Endian endian;

        private byte[] buffer;

        private bool compressed;

        private int position;

        public Endian Endian
        {
            get { return endian; }
            set { endian = value; }
        }

        public byte[] Bytes
        {
            get { return buffer; }
            set
            {
                buffer = value;
                compressed = false;
            }
        }

        public int BytesAvailable
        {
            get
            {
                var available = buffer.Length - position;
                if (available > buffer.Length || available < 0)
                    available = 0;
                return available;
            }
        }

        public bool Compressed
        {
            get { return compressed; }
            set { compressed = value; }
        }

        public int Length
        {
            get { return buffer.Length; }
        }

        public int Position
        {
            get { return position; }
            set { position = value; }
        }

        public ByteArray() : this(new byte[0])
        {
        }

        public ByteArray(byte[] buffer)
        {
            this.buffer = buffer;
            endian = Endian.BIG_ENDIAN;
        }

        public void Clear()
        {
            buffer = new byte[0];
            endian = Endian.BIG_ENDIAN;
            compressed = false;
            position = 0;
        }

        protected byte[] EndianReverse(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian ^ endian == Endian.LITTLE_ENDIAN)
                return bytes.Reverse().ToArray();
            else return bytes;
        }

        private void IsCompressedRead()
        {
            if (compressed)
                throw new Exception("圧縮されたデータを読み込もうとしました。");
        }

        private void IsCompressedWrite()
        {
            if (compressed)
                throw new Exception("圧縮されたデータへ書き込もうとしました。");
        }

        public void Compress()
        {
            if (compressed)
                throw new Exception("既に圧縮されたデータを圧縮しようとしました。");

            buffer = ZlibStream.CompressBuffer(buffer);
            position = 0;
            compressed = true;
        }

        public bool ReadBoolean()
        {
            IsCompressedRead();
            return buffer[position++] == 1;
        }

        public sbyte ReadByte()
        {
            IsCompressedRead();
            var cast = unchecked((sbyte)buffer[position++]);
            return cast;
        }

        public byte[] ReadBytes(int length)
        {
            var array = new byte[length];
            Buffer.BlockCopy(buffer, position, array, 0, length);
            position += length;
            return array;
        }

        public double ReadDouble()
        {
            IsCompressedRead();
            var value = EndianReverse(ReadBytes(8));
            return BitConverter.ToDouble(value, 0);
        }

        public DateTime ReadTime()
        {
            var time = DateTime.Parse("1970/1/1 09:00");
            var value = ReadDouble();
            return time.AddMilliseconds(value);
        }

        public float ReadFloat()
        {
            IsCompressedRead();
            var value = EndianReverse(ReadBytes(4));
            return BitConverter.ToSingle(value, 0);
        }

        public int ReadInt()
        {
            IsCompressedRead();
            var value = EndianReverse(ReadBytes(4));
            return BitConverter.ToInt32(value, 0);
        }

        public uint ReadUnsignedInt()
        {
            IsCompressedRead();
            var value = EndianReverse(ReadBytes(4));
            return BitConverter.ToUInt32(value, 0);
        }

        public long ReadLong()
        {
            IsCompressedRead();
            var value = EndianReverse(ReadBytes(8));
            return BitConverter.ToInt64(value, 0);
        }

        public short ReadShort()
        {
            IsCompressedRead();
            var value = EndianReverse(ReadBytes(2));
            return BitConverter.ToInt16(value, 0);
        }

        public ushort ReadUnsignedShort()
        {
            IsCompressedRead();
            var value = EndianReverse(ReadBytes(2));
            return BitConverter.ToUInt16(value, 0);
        }

        public string ReadUTF()
        {
            return ReadUTFBytes(ReadUnsignedShort());
        }

        public string ReadUTFBytes(int count)
        {
            IsCompressedRead();
            var @string = Encoding.UTF8.GetString(buffer, position, count);
            position += count;
            return @string;
        }

        public void Uncompress()
        {
            buffer = ZlibStream.UncompressBuffer(buffer);
            position = 0;
            compressed = false;
        }

        public void WriteBoolean(bool value)
        {
            IsCompressedWrite();
            var bytes = BitConverter.GetBytes(value);
            WriteBytes(new byte[] { bytes[0] });
        }

        public void WriteByte(sbyte value)
        {
            IsCompressedWrite();
            var bytes = BitConverter.GetBytes(value);
            WriteBytes(new byte[] { bytes[0] });
        }

        public void WriteBytes(byte[] value)
        {
            WriteBytes(value, 0, value.Length);
        }

        public void WriteBytes(ByteArray Base, int start, int length)
        {
            var buffer = Base.ReadBytes(length);
            WriteBytes(buffer);
        }

        public void WriteBytes(byte[] value, int offset, int length)
        {
            var next = length + position;
            var size = next;

            if (size <= buffer.Length)
                size = buffer.Length;

            var destination = new byte[size];
            Buffer.BlockCopy(buffer, 0, destination, 0, buffer.Length);
            Buffer.BlockCopy(value, offset, destination, position, length);
            position = next;
            buffer = destination;
        }

        public void WriteDouble(double value)
        {
            IsCompressedWrite();
            var bytes = BitConverter.GetBytes(value);
            WriteBytes(EndianReverse(bytes));
        }

        public void WriteTime(DateTime date)
        {
            WriteDouble(date.Subtract(DateTime.Parse("1970/1/1 09:00")).TotalMilliseconds);
        }

        public void WriteFloat(float value)
        {
            IsCompressedWrite();
            var bytes = BitConverter.GetBytes(value);
            WriteBytes(EndianReverse(bytes));
        }

        public void WriteInt(int value)
        {
            IsCompressedWrite();
            var bytes = BitConverter.GetBytes(value);
            WriteBytes(EndianReverse(bytes));
        }

        public void WriteUnsignedInt(uint value)
        {
            IsCompressedWrite();
            var bytes = BitConverter.GetBytes(value);
            WriteBytes(EndianReverse(bytes));
        }

        public void WriteLong(long value)
        {
            IsCompressedWrite();
            var bytes = BitConverter.GetBytes(value);
            WriteBytes(EndianReverse(bytes));
        }

        public void WriteShort(short value)
        {
            IsCompressedWrite();
            var bytes = BitConverter.GetBytes(value);
            WriteBytes(EndianReverse(bytes));
        }

        public void ReadUnsignedShort(ushort value)
        {
            IsCompressedWrite();
            var bytes = BitConverter.GetBytes(value);
            WriteBytes(EndianReverse(bytes));
        }

        public void WriteUTF(string value)
        {
            IsCompressedWrite();
            var bytes = Encoding.UTF8.GetBytes(value ?? "");
            var length = bytes.Length;

            if (length > short.MaxValue)
                throw new FormatException("文字数が多すぎます。");

            WriteShort((short)length);
            WriteBytes(bytes);
        }

        public void WriteUTFBytes(string Message)
        {
            IsCompressedWrite();
            var bytes = Encoding.UTF8.GetBytes(Message ?? "");
            var length = bytes.Length;

            if (length > short.MaxValue)
                throw new FormatException("文字数が多すぎます。");

            WriteBytes(bytes);
        }
    }
}
