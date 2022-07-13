using Gibbed.IO;
using System.IO;

namespace BatAkTool
{
    internal class TExport
    {
        private int index;
        public int classObj;
        public int superObj;
        public int outerObj;
        public int objName;
        public int objNameOrder;
        private int objArchetype;
        private ulong objFlags;
        private int objFlagsExt;
        public int size;
        public int offset;
        private uint exportFlags;
        private byte[] GUID;
        private int packageFlags;
        private int packageFlagsExt;

        public TExport(int index) => this.index = index;

        public void Read(Stream reader)
        {
            this.classObj = reader.ReadValueS32(Tool.endian);
            this.superObj = reader.ReadValueS32(Tool.endian);
            this.outerObj = reader.ReadValueS32(Tool.endian);
            this.objName = reader.ReadValueS32(Tool.endian);
            this.objNameOrder = reader.ReadValueS32(Tool.endian);
            this.objArchetype = reader.ReadValueS32(Tool.endian);
            this.objFlags = reader.ReadValueU64(Tool.endian);
            this.objFlagsExt = reader.ReadValueS32(Tool.endian);
            this.size = reader.ReadValueS32(Tool.endian);
            if (this.size > 0)
                this.offset = reader.ReadValueS32(Tool.endian);
            this.exportFlags = reader.ReadValueU32(Tool.endian);
            this.GUID = reader.ReadBytes(16);
            this.packageFlags = reader.ReadValueS32(Tool.endian);
            this.packageFlagsExt = reader.ReadValueS32(Tool.endian);
        }

        public void Write(Stream writer)
        {
            writer.WriteValueS32(this.classObj, Tool.endian);
            writer.WriteValueS32(this.superObj, Tool.endian);
            writer.WriteValueS32(this.outerObj, Tool.endian);
            writer.WriteValueS32(this.objName, Tool.endian);
            writer.WriteValueS32(this.objNameOrder, Tool.endian);
            writer.WriteValueS32(this.objArchetype, Tool.endian);
            writer.WriteValueU64(this.objFlags, Tool.endian);
            writer.WriteValueS32(this.objFlagsExt, Tool.endian);
            writer.WriteValueS32(this.size, Tool.endian);
            if (this.size > 0)
                writer.WriteValueS32(this.offset, Tool.endian);
            writer.WriteValueU32(this.exportFlags, Tool.endian);
            writer.WriteBytes(this.GUID);
            writer.WriteValueS32(this.packageFlags, Tool.endian);
            writer.WriteValueS32(this.packageFlagsExt, Tool.endian);
        }
    }
}
