using Gibbed.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BatAkTool
{
    internal class RDialogueEvent
    {
        public static readonly string KEY = nameof(RDialogueEvent);
        private static readonly Dictionary<short, string> typeDict = new Dictionary<short, string>()
    {
      {
        (short) 1,
        "NameProperty"
      },
      {
        (short) 2,
        "IntProperty"
      },
      {
        (short) 3,
        "BoolProperty"
      },
      {
        (short) 4,
        "FloatProperty"
      },
      {
        (short) 9,
        "ArrayProperty"
      },
      {
        (short) 13,
        "StrProperty"
      },
      {
        (short) 16,
        "IntProperty"
      }
    };
        private readonly StringBuilder allTexts;
        private readonly string[] allTextLines;
        private readonly Stream reader;
        private readonly Stream writer;
        private readonly TName[] nameTable;

        public RDialogueEvent(Stream reader, TName[] nameTable, StringBuilder outputStream)
        {
            this.reader = reader;
            this.nameTable = nameTable;
            this.allTexts = outputStream;
        }

        public RDialogueEvent(Stream reader, Stream writer, TName[] nameTable, string[] texts)
        {
            this.reader = reader;
            this.writer = writer;
            this.nameTable = nameTable;
            this.allTextLines = texts;
        }

        public void Search(long endPos, bool inArray)
        {
            while (this.reader.Position < endPos)
            {
                short key = this.reader.ReadValueS16(Tool.endian);
                if (key != (short)0)
                {
                    short num1 = this.reader.ReadValueS16(Tool.endian);
                    if (RDialogueEvent.typeDict.TryGetValue(key, out string str))
                    {
                        if (!(str == "IntProperty") && !(str == "FloatProperty"))
                        {
                            if (!(str == "BoolProperty"))
                            {
                                if (!(str == "NameProperty"))
                                {
                                    if (!(str == "StrProperty"))
                                    {
                                        if (!(str == "ArrayProperty"))
                                            throw new Exception("Unexpected field in RDialogueEvent class!!");
                                        int index1 = this.reader.ReadValueS32(Tool.endian);
                                        this.reader.Seek(4L, SeekOrigin.Current);
                                        int num2 = this.reader.ReadValueS32(Tool.endian);
                                        this.reader.Seek(4L, SeekOrigin.Current);
                                        long num3 = (long)num2 + this.reader.Position;
                                        int num4 = this.reader.ReadValueS32(Tool.endian);
                                        if (num4 > 0)
                                        {
                                            string name = this.nameTable[index1].Name;
                                            if (name == "Subtitles" || name == "LocalizedSubtitles")
                                            {
                                                bool inArray1 = this.nameTable[index1].Name == "Subtitles";
                                                for (int index2 = 0; index2 < num4; ++index2)
                                                    this.Search(num3, inArray1);
                                            }
                                            else
                                                this.reader.Seek(num3, SeekOrigin.Begin);
                                        }
                                    }
                                    else
                                    {
                                        TString tstring = new TString();
                                        tstring.Read(this.reader);
                                        if (num1 == (short)0 & inArray && tstring.Str.Length > 0)
                                            this.allTexts.AppendLine(tstring.Str.Replace("\n", "{\\n}"));
                                    }
                                }
                                else
                                    this.reader.Seek(24L, SeekOrigin.Current);
                            }
                            else
                                this.reader.Seek(17L, SeekOrigin.Current);
                        }
                        else
                            this.reader.Seek(4L, SeekOrigin.Current);
                    }
                    else
                        Console.WriteLine("Error: Unknown Type {0}", (object)key);
                }
            }
        }

        public void Search(long endPos, ref int lineNumber, bool inArray)
        {
            while (this.reader.Position < endPos)
            {
                short key = this.reader.ReadValueS16(Tool.endian);
                this.writer.WriteValueS16(key, Tool.endian);
                if (key != (short)0)
                {
                    short num1 = this.reader.ReadValueS16(Tool.endian);
                    this.writer.WriteValueS16(num1, Tool.endian);
                    if (RDialogueEvent.typeDict.TryGetValue(key, out string str))
                    {
                        if (!(str == "IntProperty") && !(str == "FloatProperty"))
                        {
                            if (!(str == "BoolProperty"))
                            {
                                if (!(str == "NameProperty"))
                                {
                                    if (!(str == "StrProperty"))
                                    {
                                        if (!(str == "ArrayProperty"))
                                            throw new Exception("Unexpected field in RDialogueEvent class!!");
                                        int index1 = this.reader.ReadValueS32(Tool.endian);
                                        this.writer.WriteValueS32(index1, Tool.endian);
                                        this.writer.WriteBytes(this.reader.ReadBytes(4));
                                        int num2 = this.reader.ReadValueS32(Tool.endian);
                                        int num3 = this.reader.ReadValueS32(Tool.endian);
                                        long position1 = this.writer.Position;
                                        this.writer.WriteValueS32(num2, Tool.endian);
                                        this.writer.WriteValueS32(num3, Tool.endian);
                                        long endPos1 = (long)num2 + this.reader.Position;
                                        int num4 = this.reader.ReadValueS32(Tool.endian);
                                        this.writer.WriteValueS32(num4, Tool.endian);
                                        string name = this.nameTable[index1].Name;
                                        if (name == "Subtitles" || name == "LocalizedSubtitles")
                                        {
                                            bool inArray1 = this.nameTable[index1].Name == "Subtitles";
                                            for (int index2 = 0; index2 < num4; ++index2)
                                                this.Search(endPos1, ref lineNumber, inArray1);
                                            long position2 = this.writer.Position;
                                            this.writer.Seek(position1, SeekOrigin.Begin);
                                            this.writer.WriteValueS32((int)(position2 - position1 - 8L), Tool.endian);
                                            this.writer.Seek(position2, SeekOrigin.Begin);
                                        }
                                        else
                                            this.writer.WriteBytes(this.reader.ReadBytes(num2 - 4));
                                    }
                                    else
                                    {
                                        TString tstring = new TString(false);
                                        tstring.Read(this.reader);
                                        if (num1 == (short)0 & inArray && tstring.Str.Length > 0)
                                        {
                                            tstring.ToUnicode = true;
                                            tstring.Str = this.allTextLines[lineNumber++].Replace("{\\n}", "\n");
                                        }
                                        tstring.Write(this.writer);
                                    }
                                }
                                else
                                    this.writer.WriteBytes(this.reader.ReadBytes(24));
                            }
                            else
                                this.writer.WriteBytes(this.reader.ReadBytes(17));
                        }
                        else
                            this.writer.WriteBytes(this.reader.ReadBytes(4));
                    }
                    else
                        Console.WriteLine("Error: Unknown Type {0}", (object)key);
                }
            }
        }
    }
}
