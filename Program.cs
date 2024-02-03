using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Authentication.ExtendedProtection;
namespace SqlQueryFileList
{
    public enum ColumnEnum
    {
        NAME,
        SIZE,
        FILE_LAST_WRITE_TIME,
        EXTENSION
    }
    public class Program
    {

        public static void Main(string[] args)
        {
            //! 串聯傳入的參數並以空格區隔
            //! NAME
            //! SIZE
            //! FILE_LAST_WRITE_TIME
            //! EXTENSION
            List<string> ls = string.Join(' ', args).Replace("*", "NAME,SIZE,FILE_LAST_WRITE_TIME,EXTENSION").Split(new char[] { ',', ' ' }).ToList();
            //! 確認User 要查詢的欄位
            string targetDirectory = string.Empty;
            List<ColumnEnum> columnEnums = new List<ColumnEnum>();
            for (int i = 0; i < ls.Count; i++)
            {
                if (ls[i] == ".")
                {
                    targetDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                }
                else if (ls[i].ToUpper() == "NAME")
                {
                    columnEnums.Add(ColumnEnum.NAME);
                }
                else if (ls[i].ToUpper() == "SIZE")
                {
                    columnEnums.Add(ColumnEnum.SIZE);
                }
                else if (ls[i].ToUpper() == "FILE_LAST_WRITE_TIME")
                {
                    columnEnums.Add(ColumnEnum.FILE_LAST_WRITE_TIME);
                }
                else if (ls[i].ToUpper() == "EXTENSION")
                {
                    columnEnums.Add(ColumnEnum.EXTENSION);
                }
            }
            List<FileInfo> fileInfos = Directory.GetFiles(targetDirectory).ToList().Select(x => new FileInfo(x)).ToList();
            int nameLength = fileInfos.OrderByDescending(x => x.Name.Length).First().Name.Length;
            int sizeLength = fileInfos.OrderByDescending(x => x.Length).First().Length.ToString().Length;
            int LastWriteTimeLength = 20;
            int extensionLength = fileInfos.OrderByDescending(x => x.Extension.Length).First().Extension.Length > 9 ?
                                    fileInfos.OrderByDescending(x => x.Extension.Length).First().Extension.Length : 9;
            string s = string.Empty;
            for (int i = 0; i < columnEnums.Count; i++)
            {
                switch (columnEnums[i])
                {
                    case ColumnEnum.NAME:
                        s += $"{ColumnEnum.NAME.ToString().PadLeft(nameLength, ' ')}";
                        break;
                    case ColumnEnum.SIZE:
                        s += $"{ColumnEnum.SIZE.ToString().PadLeft(sizeLength, ' ')}";
                        break;
                    case ColumnEnum.FILE_LAST_WRITE_TIME:
                        s += $"{ColumnEnum.FILE_LAST_WRITE_TIME.ToString().PadLeft(LastWriteTimeLength, ' ')}";
                        break;
                    case ColumnEnum.EXTENSION:
                        s += $"{ColumnEnum.EXTENSION.ToString().PadLeft(extensionLength, ' ')}";
                        break;
                }
                if (i < columnEnums.Count - 1)
                {
                    s += "|";
                }
            }
            Console.WriteLine(s);
            Console.WriteLine(string.Empty.PadLeft(s.Length, '-'));
            for (int i = 0; i < fileInfos.Count; i++)
            {
                string t = string.Empty;
                for (int j = 0; j < columnEnums.Count; j++)
                {
                    switch (columnEnums[j])
                    {
                        case ColumnEnum.NAME:
                            t += fileInfos[i].Name.PadLeft(nameLength, ' ');
                            break;
                        case ColumnEnum.SIZE:
                            t += fileInfos[i].Length.ToString().PadLeft(sizeLength, ' ');
                            break;
                        case ColumnEnum.FILE_LAST_WRITE_TIME:
                            t += fileInfos[i].LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss").PadLeft(LastWriteTimeLength, ' ');
                            break;
                        case ColumnEnum.EXTENSION:
                            t += fileInfos[i].Extension.PadLeft(extensionLength, ' ');
                            break;
                    }
                    if (j < columnEnums.Count - 1)
                    {
                        t += "|";
                    }
                }
                Console.WriteLine(t);
            }
        }
    }
}