using Gibbed.IO;
using System;
using System.IO;

namespace BatAkTool
{
    internal class UpkPackage
    {
        private static readonly uint UPK_PACKAGE_SIGN = 2653586369;
        private uint packageSign;
        private int gameVer;
        private int packageHeaderSize;
        private readonly TString none;
        private uint packageFlags;
        public int nameCount;
        public int nameOffset;
        public int exportCount;
        public int exportOffset;
        public int importCount;
        public int importOffset;
        private int dependOffset1;
        private int dependOffset2;
        private byte[] chunk;
        private byte[] GUID;
        private int generationCount;
        private byte[] generationData;
        private int engineVer;
        private int cookerVer;
        private int compressed;
        private int chunkSize;
        private byte[] unknown1;
        public TName[] nameTable;
        public TImport[] importTable;
        public TExport[] exportTable;
        private byte[] unknown2;
        public Stream reader;
        public Stream writer;

        public UpkPackage(Stream reader)
        {
            this.none = new TString(false);
            this.reader = reader;
        }

        public UpkPackage(Stream reader, Stream writer)
        {
            this.none = new TString(false);
            this.reader = reader;
            this.writer = writer;
        }

        public void Read()
        {
            this.reader.Seek(0L, SeekOrigin.Begin);
            this.packageSign = this.reader.ReadValueU32(Tool.endian);
            if ((int)this.packageSign != (int)UpkPackage.UPK_PACKAGE_SIGN)
                throw new Exception("Not valid UPK Package file!");
            this.gameVer = this.reader.ReadValueS32(Tool.endian);
            if (this.gameVer != Tool.GAME_VER)
                throw new NotSupportedException("This game is not supported!");
            this.packageHeaderSize = this.reader.ReadValueS32(Tool.endian);
            this.none.Read(this.reader);
            this.packageFlags = this.reader.ReadValueU32(Tool.endian);
            this.nameCount = this.reader.ReadValueS32(Tool.endian);
            this.nameOffset = this.reader.ReadValueS32(Tool.endian);
            this.exportCount = this.reader.ReadValueS32(Tool.endian);
            this.exportOffset = this.reader.ReadValueS32(Tool.endian);
            this.importCount = this.reader.ReadValueS32(Tool.endian);
            this.importOffset = this.reader.ReadValueS32(Tool.endian);
            this.dependOffset1 = this.reader.ReadValueS32(Tool.endian);
            this.dependOffset2 = this.reader.ReadValueS32(Tool.endian);
            this.chunk = this.reader.ReadBytes(12);
            this.GUID = this.reader.ReadBytes(16);
            this.generationCount = this.reader.ReadValueS32(Tool.endian);
            this.generationData = this.reader.ReadBytes(12 * this.generationCount);
            this.engineVer = this.reader.ReadValueS32(Tool.endian);
            this.cookerVer = this.reader.ReadValueS32(Tool.endian);
            this.compressed = this.reader.ReadValueS32(Tool.endian);
            this.chunkSize = this.reader.ReadValueS32(Tool.endian);
            if (this.compressed != 0)
                throw new Exception("Compressed UPK Files are not supported!");
            this.unknown1 = this.reader.ReadBytes(this.nameOffset - (int)this.reader.Position);
            this.nameTable = new TName[this.nameCount];
            for (int index = 0; index < this.nameCount; ++index)
            {
                TName tname = new TName(index);
                tname.Read(this.reader);
                this.nameTable[index] = tname;
            }
            this.importTable = new TImport[this.importCount];
            for (int index = 0; index < this.importCount; ++index)
            {
                TImport timport = new TImport(index);
                timport.Read(this.reader);
                this.importTable[index] = timport;
            }
            this.exportTable = new TExport[this.exportCount];
            for (int index = 0; index < this.exportCount; ++index)
            {
                TExport texport = new TExport(index);
                texport.Read(this.reader);
                this.exportTable[index] = texport;
            }
            this.unknown2 = this.reader.ReadBytes(this.packageHeaderSize - (int)this.reader.Position);
        }

        public void Write()
        {
            if (this.writer == null)
                return;
            this.writer.Seek(0L, SeekOrigin.Begin);
            this.writer.WriteValueU32(this.packageSign, Tool.endian);
            this.writer.WriteValueS32(this.gameVer, Tool.endian);
            this.writer.WriteValueS32(this.packageHeaderSize, Tool.endian);
            this.none.Write(this.writer);
            this.writer.WriteValueU32(this.packageFlags, Tool.endian);
            this.writer.WriteValueS32(this.nameCount, Tool.endian);
            this.writer.WriteValueS32(this.nameOffset, Tool.endian);
            this.writer.WriteValueS32(this.exportCount, Tool.endian);
            this.writer.WriteValueS32(this.exportOffset, Tool.endian);
            this.writer.WriteValueS32(this.importCount, Tool.endian);
            this.writer.WriteValueS32(this.importOffset, Tool.endian);
            this.writer.WriteValueS32(this.dependOffset1, Tool.endian);
            this.writer.WriteValueS32(this.dependOffset2, Tool.endian);
            this.writer.WriteBytes(this.chunk);
            this.writer.WriteBytes(this.GUID);
            this.writer.WriteValueS32(this.generationCount, Tool.endian);
            this.writer.WriteBytes(this.generationData);
            this.writer.WriteValueS32(this.engineVer, Tool.endian);
            this.writer.WriteValueS32(this.cookerVer, Tool.endian);
            this.writer.WriteValueS32(this.compressed, Tool.endian);
            this.writer.WriteValueS32(this.chunkSize, Tool.endian);
            this.writer.WriteBytes(this.unknown1);
            for (int index = 0; index < this.nameCount; ++index)
                this.nameTable[index].Write(this.writer);
            for (int index = 0; index < this.importCount; ++index)
                this.importTable[index].Write(this.writer);
            for (int index = 0; index < this.exportCount; ++index)
                this.exportTable[index].Write(this.writer);
            this.writer.WriteBytes(this.unknown2);
        }

        public string ReadObjectIndex(int index)
        {
            if (index < 0)
                return this.nameTable[this.importTable[-index - 1].objName].Name;
            return index > 0 ? this.nameTable[this.exportTable[index - 1].objName].Name : "null";
        }
    }
}
