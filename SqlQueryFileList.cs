using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SqlQueryFileList
{
    public class SqlQueryFileList
    {
        private List<string> QueryStatement { get; set; }
        private string TargetDirectory { get; set; }
        private List<ColumnEnum> ColumnEnums { get; set; } = new List<ColumnEnum>();
        private ColumnEnum OrderBy { get; set; } = ColumnEnum.NAME;
        private bool IsDescending { get; set; }
        private Block Block { get; set; }
        private List<FileInfo> FileInfoList { get; set; }
        private int NameLength { get; set; }
        private int SizeLength { get; set; }
        private int FileLastWriteTimeLength { get; set; }
        private int ExtensionLength { get; set; }
        private ColumnEnum LikeColumn { get; set; }
        private Like Like { get; set; }
        private string LikeString { get; set; }
        public SqlQueryFileList(string[] args)
        {
            //! 串聯傳入的參數並以空格區隔
            //! 初步階段四個欄位
            //! NAME
            //! SIZE
            //! FILE_LAST_WRITE_TIME
            //! EXTENSION
            this.QueryStatement = string.Join(' ', args)
            .Replace("*", "NAME,SIZE,FILE_LAST_WRITE_TIME,EXTENSION")
            .Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            this.ParseQueryStatement();
            this.FileInfoList = Directory.GetFiles(this.TargetDirectory).ToList().Select(x => new FileInfo(x)).ToList();
            this.VagueSearch();
            this.SetOrder();
            this.SetColumnWidth();
            this.Print();
        }
        private void VagueSearch()
        {
            if (this.Like != Like.NONE)
            {
                this.FileInfoList = this.FileInfoList.Where(
x => x.Name.Where((_, index) =>
this.LikeString.SkipWhile((p, pIndex) =>
    p == '%' || (pIndex + index < x.Name.Length && p == x.Name[index + pIndex])).All(p => p == '%')).Any()
|| this.LikeString.StartsWith('%') && x.Name.EndsWith(this.LikeString.Substring(1))
|| this.LikeString.EndsWith('%') && x.Name.StartsWith(this.LikeString.Substring(0, this.LikeString.Length - 1))
                          ).Select(x => x).ToList();
            }
        }

        private void SetColumnWidth()
        {
            this.NameLength = this.FileInfoList.OrderByDescending(x => x.Name.Length).First().Name.Length;
            this.SizeLength = this.FileInfoList.OrderByDescending(x => x.Length).First().Length.ToString().Length;
            this.FileLastWriteTimeLength = 20;
            int temp = this.FileInfoList.OrderByDescending(x => x.Extension.Length).First().Extension.Length;
            this.ExtensionLength = temp > 9 ? temp : 9;
        }

        private void Print()
        {
            string s = string.Empty;
            for (int i = 0; i < this.ColumnEnums.Count; i++)
            {
                s += this.ColumnEnums[i] switch
                {
                    ColumnEnum.NAME => $"{ColumnEnum.NAME.ToString().PadLeft(this.NameLength, ' ')}",
                    ColumnEnum.SIZE => $"{ColumnEnum.SIZE.ToString().PadLeft(this.SizeLength, ' ')}",
                    ColumnEnum.FILE_LAST_WRITE_TIME => $"{ColumnEnum.FILE_LAST_WRITE_TIME.ToString().PadLeft(this.FileLastWriteTimeLength, ' ')}",
                    ColumnEnum.EXTENSION => $"{ColumnEnum.EXTENSION.ToString().PadLeft(this.ExtensionLength, ' ')}",
                    _ => string.Empty
                } + (i < this.ColumnEnums.Count - 1 ? "|" : string.Empty);
            }
            Console.WriteLine(s);
            Console.WriteLine(string.Empty.PadLeft(s.Length, '-'));
            for (int i = 0; i < this.FileInfoList.Count; i++)
            {
                s = string.Empty;
                for (int j = 0; j < this.ColumnEnums.Count; j++)
                {
                    s += this.ColumnEnums[j] switch
                    {
                        ColumnEnum.NAME => this.FileInfoList[i].Name.PadLeft(this.NameLength, ' '),
                        ColumnEnum.SIZE => this.FileInfoList[i].Length.ToString().PadLeft(this.SizeLength, ' '),
                        ColumnEnum.FILE_LAST_WRITE_TIME => this.FileInfoList[i].LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss").PadLeft(this.FileLastWriteTimeLength, ' '),
                        ColumnEnum.EXTENSION => this.FileInfoList[i].Extension.PadLeft(this.ExtensionLength, ' '),
                        _ => string.Empty
                    } + (j < ColumnEnums.Count - 1 ? "|" : string.Empty);

                }
                Console.WriteLine(s);
            }
        }

        private void SetOrder()
        {
            switch (this.OrderBy)
            {
                case ColumnEnum.NAME:
                    if (this.IsDescending)
                    {
                        this.FileInfoList = this.FileInfoList.OrderByDescending(x => x.Name).ToList();
                    }
                    else
                    {
                        this.FileInfoList = this.FileInfoList.OrderBy(x => x.Name).ToList();
                    }
                    break;
                case ColumnEnum.SIZE:
                    if (this.IsDescending)
                    {
                        this.FileInfoList = this.FileInfoList.OrderByDescending(x => x.Length).ToList();
                    }
                    else
                    {
                        this.FileInfoList = this.FileInfoList.OrderBy(x => x.Length).ToList();
                    }
                    break;
                case ColumnEnum.FILE_LAST_WRITE_TIME:
                    if (this.IsDescending)
                    {
                        this.FileInfoList = this.FileInfoList.OrderByDescending(x => x.LastWriteTime).ToList();
                    }
                    else
                    {
                        this.FileInfoList = this.FileInfoList.OrderBy(x => x.LastWriteTime).ToList();
                    }
                    break;
                case ColumnEnum.EXTENSION:
                    if (this.IsDescending)
                    {
                        this.FileInfoList = this.FileInfoList.OrderByDescending(x => x.Extension).ToList();
                    }
                    else
                    {
                        this.FileInfoList = this.FileInfoList.OrderBy(x => x.Extension).ToList();
                    }
                    break;
            }
        }

        private void ParseQueryStatement()
        {
            for (int i = 0; i < this.QueryStatement.Count; i++)
            {
            Re:
                switch (this.Block)
                {
                    case Block.SELECT:
                        if (this.QueryStatement[i].Equals("NAME", StringComparison.CurrentCultureIgnoreCase))
                        {
                            this.ColumnEnums.Add(ColumnEnum.NAME);
                        }
                        else if (this.QueryStatement[i].Equals("SIZE", StringComparison.CurrentCultureIgnoreCase))
                        {
                            this.ColumnEnums.Add(ColumnEnum.SIZE);
                        }
                        else if (this.QueryStatement[i].Equals("FILE_LAST_WRITE_TIME", StringComparison.CurrentCultureIgnoreCase))
                        {
                            this.ColumnEnums.Add(ColumnEnum.FILE_LAST_WRITE_TIME);
                        }
                        else if (this.QueryStatement[i].Equals("EXTENSION", StringComparison.CurrentCultureIgnoreCase))
                        {
                            this.ColumnEnums.Add(ColumnEnum.EXTENSION);
                        }
                        else if (this.QueryStatement[i].Equals("FROM", StringComparison.CurrentCultureIgnoreCase))
                        {
                            this.Block++;
                            goto Re;
                        }
                        break;
                    case Block.FROM:
                        if (this.QueryStatement[i] == ".")
                        {
                            this.TargetDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                        }
                        else if (this.QueryStatement[i].Equals("WHERE", StringComparison.CurrentCultureIgnoreCase))
                        {
                            this.Block++;
                            goto Re;
                        }
                        else if (this.QueryStatement[i].Equals("ORDER", StringComparison.CurrentCultureIgnoreCase))
                        {
                            this.Block += 2;
                            goto Re;
                        }
                        break;
                    case Block.WHERE:
                        if (this.QueryStatement[i].Equals("WHERE", StringComparison.CurrentCultureIgnoreCase))
                        {

                            if (this.QueryStatement[i + 2].Equals("LIKE", StringComparison.CurrentCultureIgnoreCase))
                            {
                                string targetString = this.QueryStatement[i + 3].Replace("'", string.Empty);

                                this.Like = Like.START;
                                this.LikeString = targetString;
                                if (this.QueryStatement[i + 1].Equals("NAME", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    this.LikeColumn = ColumnEnum.NAME;
                                }
                                else if (this.QueryStatement[i + 1].Equals("SIZE", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    this.LikeColumn = ColumnEnum.SIZE;
                                }
                                else if (this.QueryStatement[i + 1].Equals("FILE_LAST+WRITE_TIME", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    this.LikeColumn = ColumnEnum.FILE_LAST_WRITE_TIME;
                                }
                                else if (this.QueryStatement[i + 1].Equals("EXTENSION", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    this.LikeColumn = ColumnEnum.EXTENSION;
                                }
                            }
                        }
                        else if (this.QueryStatement[i].Equals("ORDER", StringComparison.CurrentCultureIgnoreCase))
                        {
                            this.Block++;
                            goto Re;
                        }
                        break;
                    case Block.ORDER:
                        if (this.QueryStatement[i].Equals("ORDER", StringComparison.CurrentCultureIgnoreCase)
                              && this.QueryStatement[i + 1].Equals("BY", StringComparison.CurrentCultureIgnoreCase))
                        {
                            if (this.QueryStatement[i + 2].Equals("NAME", StringComparison.CurrentCultureIgnoreCase))
                            {
                                this.OrderBy = ColumnEnum.NAME;
                            }
                            else if (this.QueryStatement[i + 2].Equals("SIZE", StringComparison.CurrentCultureIgnoreCase))
                            {
                                this.OrderBy = ColumnEnum.SIZE;
                            }
                            else if (this.QueryStatement[i + 2].Equals("FILE_LAST_WRITE_TIME", StringComparison.CurrentCultureIgnoreCase))
                            {
                                this.OrderBy = ColumnEnum.FILE_LAST_WRITE_TIME;
                            }
                            else if (this.QueryStatement[i + 2].Equals("EXTENSION", StringComparison.CurrentCultureIgnoreCase))
                            {
                                this.OrderBy = ColumnEnum.EXTENSION;
                            }
                        }
                        else if (this.QueryStatement[i].Equals("DESC", StringComparison.CurrentCultureIgnoreCase))
                        {
                            this.IsDescending = true;
                        }
                        else if (this.QueryStatement[i].Equals("ASC", StringComparison.CurrentCultureIgnoreCase))
                        {
                            this.IsDescending = false;
                        }
                        break;
                }
            }
        }
    }
}