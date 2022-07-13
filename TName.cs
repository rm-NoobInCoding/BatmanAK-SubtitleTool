using Gibbed.IO;
using System.IO;

namespace BatAkTool
{
    internal class TName
    {
        private int index;
        private TString name;
        private ulong flags;

        public TName(int index)
        {
            this.index = index;
            this.name = new TString(false);
        }

        public void Read(Stream reader)
        {
            this.name.Read(reader);
            this.flags = reader.ReadValueU64(Tool.endian);
        }

        public void Write(Stream writer)
        {
            this.name.WriteIf(writer);
            writer.WriteValueU64(this.flags, Tool.endian);
        }

        public string Name => this.name.Str;
    }
}
