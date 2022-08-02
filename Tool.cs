using Gibbed.IO;
using System;
using System.IO;
using System.Text;

namespace BatAkTool
{
    internal class Tool
    {
        public static readonly int GAME_VER = -2132606113;
        public static readonly Endian endian = Endian.Little;
        public static readonly string EXTENSION = ".upk";
        public static int Language = 0; // english

        public static void Main(string[] args)
        {
            Console.WriteLine("Batman Arkham Knight Subtitle Tool - v1.7");
            Console.WriteLine("By NoobInCoding (thanks to fillmsn & celikeins)");
            string Strs = "";
            if (args.Length == 2 || args.Length == 3)
            {
                if (args.Length == 3 && int.TryParse(args[2], out int value))
                {
                    Language = value;   
                }
                if (Directory.Exists(args[1]))
                {
                    string[] Files = Directory.GetFiles(args[1], "*.upk",SearchOption.AllDirectories);
                    if (args[0] == "-e")
                    {
                        foreach (string fileName in Files)
                        {
                            FileInfo fileInfo = new FileInfo(fileName);
                            //Export

                            string Str = Tool.Export(fileInfo, Language);
                            if (Str != "")
                            {
                                Strs += "[Path]" + Path.GetFileName(fileInfo.Name) + "[Path]" + Environment.NewLine + Str;

                            }

                        }
                        File.WriteAllText(args[1] + "\\" + "ExportedTexts" + ".txt", Strs, Encoding.Unicode);
                    }
                    else if (args[0] == "-i")
                    {
                        Directory.CreateDirectory(args[1] + "_New");
                        string[] TranslatedFile = File.ReadAllText(Path.Combine(args[1] , "ExportedTexts.txt")).Split(new[] { "[Path]" }, StringSplitOptions.None);
                        for (int i = 0; i < TranslatedFile.Length/2; i ++)
                        {
                            string file = TranslatedFile[i * 2 + 1];
                            string strs = TranslatedFile[i * 2 + 2];
                            FileInfo fileInfo = new FileInfo(args[1] + "\\" + file);
                            if (fileInfo.Length > 0L)
                            {
                                Tool.Import(fileInfo, strs , Language, Path.Combine(args[1] + "_New", Path.GetFileName(fileInfo.FullName)));
                            }
                            else
                            {
                                Console.WriteLine("The file {0} has zero bytes!", (object)fileInfo.Name);
                            }
                        }


                    }
                }
                else if (File.Exists(args[1]))
                {
                    string fileName = args[1];
                    FileInfo fileInfo = new FileInfo(fileName);
                    if (args[0] == "-e")
                    {
                        string Str = Tool.Export(fileInfo, Language);
                        if (Str != "")
                        {
                            File.WriteAllText(fileInfo.DirectoryName + "\\" + Path.GetFileNameWithoutExtension(fileInfo.Name) + ".txt", Str, Encoding.Unicode);
                        }

                    }
                    else if (args[0] == "-i")
                    {
                        if (fileInfo.Length > 0L)
                        {
                            Tool.Import(fileInfo, File.ReadAllText(Path.ChangeExtension(fileInfo.FullName, ".txt")),Language, Path.Combine(Path.GetDirectoryName(fileInfo.FullName), Path.GetFileName(fileInfo.FullName) + "_New"));

                        }
                        else
                        {
                            Console.WriteLine("The file {0} has zero bytes!", (object)fileInfo.Name);
                        }

                    }
                    else
                    {
                        Console.WriteLine("The file {0} is not supported! this tool only support upk files!", (object)fileInfo.Name);
                    }


                }



                Console.Write("DONE!");
            }
            else
            {
                Console.WriteLine("USAGE:");
                Console.WriteLine("  Export Folder : {0} -e <FolderName Or FileName> [Language]", AppDomain.CurrentDomain.FriendlyName);
                Console.WriteLine("  Import Folder : {0} -i <FolderName Or FileName> [Language]", AppDomain.CurrentDomain.FriendlyName);
                Console.WriteLine("");
                Console.WriteLine("NOTES:");
                Console.WriteLine(" *The tool ONLY works with uncompressed upk files. You can use the 'decompress' tool (by glidor) to decompress the files");
                Console.WriteLine(" *For Import Folder/File , the exported text file must be inside/besides the selected folder/file");
                Console.WriteLine(" *Language Is Default on English but if you want to change it you must enter a number between 0 to 11");
                Console.ReadKey();
            }


        }

        private static string Export(FileInfo fileInfo, int Language)
        {
            Stream reader = (Stream)File.OpenRead(fileInfo.FullName);
            UpkPackage upkPackage = new UpkPackage(reader);
            upkPackage.Read();
            bool flag = false;
            for (int index = 0; index < upkPackage.importCount; ++index)
            {
                if (upkPackage.nameTable[upkPackage.importTable[index].objName].Name == AkDialogueEvent.KEY)
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                //Console.WriteLine("File {0} does not import {1}!", (object)fileInfo.Name, (object)AkDialogueEvent.KEY);
                reader.Close();
            }
            else
            {
                StringBuilder outputStream = new StringBuilder();
                AkDialogueEvent akDialogueEvent = new AkDialogueEvent(reader, upkPackage.nameTable, outputStream, Language);
                //Console.WriteLine("Extracting texts...");
                for (int index = 0; index < upkPackage.exportCount; ++index)
                {
                    if (upkPackage.ReadObjectIndex(upkPackage.exportTable[index].classObj) == AkDialogueEvent.KEY)
                    {
                        reader.Seek((long)upkPackage.exportTable[index].offset, SeekOrigin.Begin);
                        long endPos = (long)(upkPackage.exportTable[index].offset + upkPackage.exportTable[index].size);
                        akDialogueEvent.Search(endPos, false);
                    }
                }
                reader.Close();
                //Console.WriteLine();
                if (outputStream.Length != 0)
                {
                    Console.WriteLine("Text File {0} successfully extracted!", (object)fileInfo.Name);
                    return outputStream.ToString();
                }
                else
                {
                    Console.WriteLine("File {0} does not have any text!", (object)fileInfo.Name);

                }

            }
            return "";
        }

        private static void Import(FileInfo fileInfo, string strs, int Language , string newpath)
        {
            string path = Path.ChangeExtension(fileInfo.FullName, Tool.EXTENSION);
            if (File.Exists(path))
            {
                if (File.Exists(newpath)) File.Delete(newpath);
                Stream stream1 = (Stream)File.Open(newpath, FileMode.Create);
                Stream stream2 = File.OpenRead(path);
                long offset1 = stream2.Length;
                UpkPackage upkPackage = new UpkPackage(stream2, stream1);
                upkPackage.Read();
                string[] texts = strs.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                int lineNumber = 1;
                AkDialogueEvent akDialogueEvent = new AkDialogueEvent(stream2, stream1, upkPackage.nameTable, texts, Language);
                for (int index = 0; index < upkPackage.exportCount; ++index)
                {
                    long offset2 = (long)upkPackage.exportTable[index].offset;
                    long size = (long)upkPackage.exportTable[index].size;
                    stream1.Seek(offset2, SeekOrigin.Begin);
                    stream2.Seek(offset2, SeekOrigin.Begin);
                    stream1.WriteBytes(stream2.ReadBytes(upkPackage.exportTable[index].size));
                    if (upkPackage.ReadObjectIndex(upkPackage.exportTable[index].classObj) == AkDialogueEvent.KEY)
                    {
                        long endPos = offset2 + size;
                        stream1.Seek(offset1, SeekOrigin.Begin);
                        stream2.Seek(offset2, SeekOrigin.Begin);
                        akDialogueEvent.Search(endPos, ref lineNumber, false);
                        upkPackage.exportTable[index].offset = (int)offset1;
                        upkPackage.exportTable[index].size = (int)(stream1.Position - offset1);
                        offset1 = stream1.Position;
                    }
                }
                upkPackage.Write();
                stream2.Close();
                stream1.Close();
                Console.WriteLine("Text File {0} successfully Imported!", (object)fileInfo.Name);
            }
            else
                Console.WriteLine("File {0} was not found!", (object)Path.GetFileName(path));
        }


    }
}
