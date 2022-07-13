using Gibbed.IO;
using System.IO;

namespace BatAkTool
{
    internal class TImport
    {
        private int index;
        private int packageName;
        private int packageNameOrd;
        private int className;
        private int classNameOrd;
        private int outerObj;
        public int objName;
        public int objNameOrd;

        public TImport(int index) => this.index = index;

        public void Read(Stream reader)
        {
            this.packageName = reader.ReadValueS32(Tool.endian);
            this.packageNameOrd = reader.ReadValueS32(Tool.endian);
            this.className = reader.ReadValueS32(Tool.endian);
            this.classNameOrd = reader.ReadValueS32(Tool.endian);
            this.outerObj = reader.ReadValueS32(Tool.endian);
            this.objName = reader.ReadValueS32(Tool.endian);
            this.objNameOrd = reader.ReadValueS32(Tool.endian);
        }

        public void Write(Stream writer)
        {
            writer.WriteValueS32(this.packageName, Tool.endian);
            writer.WriteValueS32(this.packageNameOrd, Tool.endian);
            writer.WriteValueS32(this.className, Tool.endian);
            writer.WriteValueS32(this.classNameOrd, Tool.endian);
            writer.WriteValueS32(this.outerObj, Tool.endian);
            writer.WriteValueS32(this.objName, Tool.endian);
            writer.WriteValueS32(this.objNameOrd, Tool.endian);
        }
    }
}
