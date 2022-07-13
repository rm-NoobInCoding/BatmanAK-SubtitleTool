using Gibbed.IO;
using System.IO;
using System.Text;

namespace BatAkTool
{
    internal class TString
    {
        private bool toUnicode;
        private int length;
        private string str;

        public TString() => this.toUnicode = true;

        public TString(bool toUnicode) => this.toUnicode = toUnicode;

        public void Read(Stream reader)
        {
            this.length = reader.ReadValueS32(Tool.endian);
            if (this.length > 0)
                this.str = reader.ReadString(this.length, true, Encoding.Default);
            else if (this.length < 0)
            {
                this.length *= -2;
                this.str = reader.ReadString(this.length, true, Encoding.Unicode);
            }
            else
                this.str = "";
        }

        public void WriteIf(Stream writer)
        {
            this.toUnicode = this.Check(Encoding.Unicode.GetBytes(this.str + "\0"));
            this.Write(writer);
        }

        private bool Check(byte[] unicode)
        {
            for (int index = 1; index < unicode.Length; index += 2)
            {
                if (unicode[index] != (byte)0)
                    return true;
            }
            return false;
        }

        public void Write(Stream writer)
        {
            if (this.length == 0)
                writer.WriteValueS32(0, Tool.endian);
            else if (this.toUnicode)
            {
                byte[] bytes = Encoding.Unicode.GetBytes(this.str + "\0");
                writer.WriteValueS32(bytes.Length / -2, Tool.endian);
                writer.WriteBytes(bytes);
            }
            else
            {
                byte[] bytes = Encoding.Default.GetBytes(this.str + "\0");
                writer.WriteValueS32(bytes.Length, Tool.endian);
                writer.WriteBytes(bytes);
            }
        }

        public void WriteField(Stream writer)
        {
            if (this.str.Length == 0)
            {
                writer.WriteValueS32(4, Tool.endian);
                writer.WriteValueS32(0, Tool.endian);
                writer.WriteValueS32(0, Tool.endian);
            }
            else if (this.toUnicode)
            {
                byte[] bytes = Encoding.Unicode.GetBytes(this.str + "\0");
                writer.WriteValueS32(bytes.Length + 4, Tool.endian);
                writer.WriteValueS32(0, Tool.endian);
                writer.WriteValueS32(bytes.Length / -2, Tool.endian);
                writer.WriteBytes(bytes);
            }
            else
            {
                byte[] bytes = Encoding.Default.GetBytes(this.str + "\0");
                writer.WriteValueS32(bytes.Length + 4, Tool.endian);
                writer.WriteValueS32(0, Tool.endian);
                writer.WriteValueS32(bytes.Length, Tool.endian);
                writer.WriteBytes(bytes);
            }
        }

        public string Str
        {
            get => this.str;
            set => this.str = value;
        }

        public bool ToUnicode
        {
            set => this.toUnicode = value;
        }
    }
}
