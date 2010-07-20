/*
 XsltDb is powerful XSLT module for DotnetNuke.
 It offers safe database access, SEO-friendly AJAX support,
 visitor interactions, environment integration (dnn properties,
 request, cookie, session and form values), regular expressions, etc.

 Author:
 
    Anton Burtsev
    burtsev@yandex.ru

 Project home page: 
 
    http://xsltdb.codeplex.com
*/

using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Security;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using DotNetNuke.Data;
using DotNetNuke.Common;
using DotNetNuke.Entities.Host;

namespace Findy.XsltDb
{
    public class FileMan
    {
        private string FileShare;
        public FileMan(string fileShare)
        {
            FileShare = fileShare;
        }

        private static bool NeedMap(string path)
        {
            if (path.IndexOf("\\\\") == 0)
                return false;
            if (path.IndexOf(":\\") == 1)
                return false;
            return true;
        }
        private string MapPath(string fn)
        {
            return Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, fn.Replace("/", "\\"));
        }

        private string GetF(string fn)
        {
            if (!NeedMap(fn)) return fn;
            return MapPath(fn);
        }

        private string SavePicture(byte[] pic, string path, bool append)
        {
            path = PreparePath(path);
            string fn = GetF(Path.Combine(FileShare, path));
            using (FileStream fs = append ? new FileStream(fn, FileMode.Append, FileAccess.Write) : File.Create(fn))
                fs.Write(pic, 0, pic.Length);
            return path;
        }

        public string PreparePath(string path)
        {
            if (path == "")
                path = null;
            if (path == null || path.StartsWith("."))
            {
                string ext = path == null ? ".xsltdbfile" : path;
                DateTime dt = DateTime.Today;
                path =
                    dt.Year.ToString().PadLeft(4, '0') + "\\" +
                    dt.Month.ToString().PadLeft(2, '0') + "\\" +
                    dt.Day.ToString().PadLeft(2, '0');

                if (!Directory.Exists(GetF(Path.Combine(FileShare, path))))
                    Directory.CreateDirectory(GetF(Path.Combine(FileShare, path)));

                path = Path.Combine(path, Guid.NewGuid().ToString() + ext);
            }
            else
            {
                if (!File.Exists(GetF(Path.Combine(FileShare, path))))
                {
                    string p = GetF(Path.Combine(FileShare, path));
                    string dir = p.Substring(0, p.LastIndexOf("\\"));
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);
                }
            }
            return path;
        }

        public string PrepareDir(string dir)
        {
            PreparePath(dir);
            string full = FullName(dir);
            if ( !Directory.Exists(full) )
                Directory.CreateDirectory(full);
            return dir;
        }

        public string BuildInsideDir()
        {
            DateTime dt = DateTime.Today;

            string path =
    dt.Year.ToString().PadLeft(4, '0') + "\\" +
    dt.Month.ToString().PadLeft(2, '0') + "\\" +
    dt.Day.ToString().PadLeft(2, '0') + "\\" +
    Guid.NewGuid().ToString();

            string fp = GetF(Path.Combine(FileShare, path));
            if (!Directory.Exists(fp))
                Directory.CreateDirectory(fp);

            return fp;



        }

        public string SavePicture(byte[] pic)
        {
            return SavePicture(pic, null, false);
        }
        public string SavePicture(byte[] pic, string path)
        {
            return SavePicture(pic, path, false);
        }

        public void AppendPicture(byte[] pic, string path)
        {
            SavePicture(pic, path, true);
        }

        public byte[] LoadPicture(string Path)
        {
            return LoadLocalFile(GetF(FileShare + Path));
        }

        private byte[] LoadLocalFile(string path)
        {
            byte[] fileBytes = null;

            if (!File.Exists(path))
                return fileBytes;

            using (FileStream fs = File.OpenRead(path))
            {
                fileBytes = new byte[fs.Length];
                fs.Read(fileBytes, 0, fileBytes.Length);
            }

            return fileBytes;
        }

        public string ClonePicture(string path)
        {
            byte[] pic = LoadPicture(path);
            if (pic == null)
                throw new ApplicationException("Picture not found");

            return SavePicture(pic, Path.GetExtension(path));
        }


        internal void DeleteFile(string path)
        {
            string file = GetF(Path.Combine(FileShare, path));
            if (File.Exists(file))
                File.Delete(file);
        }

        internal void DeleteDir(string path)
        {
            string dir = GetF(Path.Combine(FileShare, path));
            if (Directory.Exists(dir))
                Directory.Delete(dir, true);
        }

        //internal bool Exists(string path)
        //{
        //    return File.Exists(GetFN(Path.Combine(FileShare, path)));
        //}

        internal string FullName(string path)
        {
            return GetF(Path.Combine(FileShare, path));
        }

        internal string RelativeName(string FullName)
        {
            string root = GetF(FileShare);
            return FullName.Substring(root.Length + 1);
        }
    }
}