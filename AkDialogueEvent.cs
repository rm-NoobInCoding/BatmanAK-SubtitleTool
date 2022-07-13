using Gibbed.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BatAkTool
{
    internal class AkDialogueEvent
    {
        public static readonly string KEY = nameof(AkDialogueEvent);
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
        private readonly int LangNumber;

        public AkDialogueEvent(Stream reader, TName[] nameTable, StringBuilder outputStream, int lang)
        {
            this.reader = reader;
            this.nameTable = nameTable;
            allTexts = outputStream;
            LangNumber = lang;
        }

        public AkDialogueEvent(Stream reader, Stream writer, TName[] nameTable, string[] texts ,int lang)
        {
            this.reader = reader;
            this.writer = writer;
            this.nameTable = nameTable;
            allTextLines = texts;
            LangNumber = lang;
        }

        public void Search(long endPos, bool inArray)
        {
            while (reader.Position < endPos)
            {
                short key = reader.ReadValueS16(Tool.endian);
                if (key != (short)0)
                {
                    short num1 = reader.ReadValueS16(Tool.endian);
                    if (AkDialogueEvent.typeDict.TryGetValue(key, out string str))
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
                                            throw new Exception("Unexpected field in AkDialogueEvent class!!");
                                        int index1 = reader.ReadValueS32(Tool.endian);
                                        reader.Seek(4L, SeekOrigin.Current);
                                        int num2 = reader.ReadValueS32(Tool.endian);
                                        reader.Seek(4L, SeekOrigin.Current); //null after file size
                                        long offset = (long)num2 + reader.Position;
                                        int num3 = reader.ReadValueS32(Tool.endian);

                                        if (num3 > 0)
                                        {
                                            if (nameTable[index1].Name == "DialogueText")
                                            {

                                                for (int index2 = 0; index2 < num3; ++index2)
                                                {
                                                    TString tstring = new TString();
                                                    tstring.Read(reader);
                                                    if (tstring.Str.Length > 0 && index2 == LangNumber)
                                                        allTexts.AppendLine(RemoveNewLine(tstring.Str));
                                                }
                                            }
                                            else
                                                reader.Seek(offset, SeekOrigin.Begin);
                                        }
                                    }
                                    else
                                    {
                                        TString tstring = new TString();
                                        tstring.Read(reader);
                                        if (num1 == (short)0 & inArray && tstring.Str.Length > 0)
                                            allTexts.AppendLine(RemoveNewLine(tstring.Str));
                                    }
                                }
                                else
                                    reader.Seek(24L, SeekOrigin.Current);
                            }
                            else
                                reader.Seek(4L, SeekOrigin.Current);
                        }
                        else
                            reader.Seek(4L, SeekOrigin.Current);
                    }
                    else
                        Console.WriteLine("Error: Unknown Type {0}", (object)key);
                }
            }
        }

        public void Search(long endPos, ref int lineNumber, bool inArray)
        {
            while (reader.Position < endPos)
            {
                short key = reader.ReadValueS16(Tool.endian);
                writer.WriteValueS16(key, Tool.endian);
                if (key != (short)0)
                {
                    short num1 = reader.ReadValueS16(Tool.endian);
                    writer.WriteValueS16(num1, Tool.endian);
                    if (AkDialogueEvent.typeDict.TryGetValue(key, out string str))
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
                                            throw new Exception("Unexpected field in AkDialogueEvent class!!");
                                        int index1 = reader.ReadValueS32(Tool.endian);
                                        writer.WriteValueS32(index1, Tool.endian);
                                        writer.WriteBytes(reader.ReadBytes(4));
                                        int num2 = reader.ReadValueS32(Tool.endian);
                                        int num3 = reader.ReadValueS32(Tool.endian);//zero
                                        long position1 = writer.Position; //before Size
                                        writer.WriteValueS32(num2, Tool.endian);
                                        writer.WriteValueS32(num3, Tool.endian);
                                        _ = reader.Position; //After File Size
                                        int num4 = reader.ReadValueS32(Tool.endian);
                                        writer.WriteValueS32(num4, Tool.endian);
                                        if (nameTable[index1].Name == "DialogueText")
                                        {
                                            for (int index2 = 0; index2 < num4; ++index2)
                                            {
                                                TString tstring = new TString(false);
                                                tstring.Read(reader);
                                                tstring.ToUnicode = true;
                                                if(index2 == LangNumber)
                                                {
                                                    if (tstring.Str.Length > 0)
                                                    {
                                                        
                                                        tstring.Str = AddNewLine(allTextLines[lineNumber++]);
                                                    }
                                                }
                                                
                                                tstring.Write(writer);
                                            }

                                            long position3 = writer.Position;
                                            writer.Seek(position1, SeekOrigin.Begin);
                                            writer.WriteValueS32((int)(position3 - position1 - 8L), Tool.endian);
                                            writer.Seek(position3, SeekOrigin.Begin);
                                        }
                                        else
                                            writer.WriteBytes(reader.ReadBytes(num2 - 4));
                                    }
                                    else
                                    {
                                        TString tstring = new TString(false);
                                        tstring.Read(reader);
                                        if (num1 == (short)0 & inArray && tstring.Str.Length > 0)
                                        {
                                            tstring.ToUnicode = true;
                                            tstring.Str = AddNewLine(allTextLines[lineNumber++]);
                                        }
                                        tstring.Write(writer);
                                        //
                                    }
                                }
                                else
                                    writer.WriteBytes(reader.ReadBytes(24));
                            }
                            else
                                writer.WriteBytes(reader.ReadBytes(4));
                        }
                        else
                            writer.WriteBytes(reader.ReadBytes(4));
                    }
                    else
                        Console.WriteLine("Error: Unknown Type {0}", (object)key);
                }
            }
        }

        private string RemoveNewLine(string str)
        {
            string ret = str;
            ret = ret.Replace("\r\n", "<cf>");
            ret = ret.Replace("\n", "<lf>");
            ret = ret.Replace("\r", "<cr>");
            return ret;
        }
        private string AddNewLine(string str)
        {
            string Text = str;
            Text = Text.Replace("<cf>", "\r\n");
            Text = Text.Replace("<lf>", "\n");
            Text = Text.Replace("<cr>", "\r");
            return Text;
        }
    }
}
