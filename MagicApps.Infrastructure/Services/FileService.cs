using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace MagicApps.Infrastructure.Services
{
    public class FileService
    {
        public static bool DeleteFile(string absolutePath)
        {
            try {
                if (File.Exists(absolutePath)) {
                    File.Delete(absolutePath);
                }
                return true;
            }
            catch {
                return false;
            }
        }

        public static string CreateFolder(string virtualPath)
        {
            string absPath = HttpContext.Current.Server.MapPath(virtualPath);

            if (!System.IO.Directory.Exists(absPath)) {
                System.IO.Directory.CreateDirectory(absPath);
            }

            return absPath;
        }

        public static string CreateFolderAbsolute(string absolutePath)
        {
            if (!System.IO.Directory.Exists(absolutePath)) {
                System.IO.Directory.CreateDirectory(absolutePath);
            }

            return absolutePath;
        }

        public static void DeleteFolder(string virtualPath)
        {
            string absPath = HttpContext.Current.Server.MapPath(virtualPath);

            if (System.IO.Directory.Exists(absPath)) {
                System.IO.Directory.Delete(absPath, true);
            }
        }

        public static string RemoveIllegalCharacters(string input, bool isFile)
        {
            input = input.Trim();
            input = input.Replace(" ", "-");

            string r = "";
            char c;
            int charInt;
            string fileExt;

            if (isFile) {
                char[] splitParam = new char[] { '.' };
                string[] parts = input.Split(splitParam);
                fileExt = "." + parts.GetValue(parts.Length - 1).ToString();
            }
            else {
                fileExt = "";
            }

            //40-41 = ()
            //45 = -
            //48-57 = 0-9
            //65-90 = A-Z
            //97-122 = a-z
            //91 = [
            //93 = ]
            //95 = _

            for (int x = 0; x < (input.Length - fileExt.Length); x++) {
                c = input[x];
                charInt = (int)c;

                if ((charInt == 40) || (charInt == 41) || (charInt == 45)
                    || (charInt >= 48 && charInt <= 57) || (charInt >= 65 && charInt <= 90) || (charInt >= 97 && charInt <= 122)
                    || (charInt == 91) || (charInt == 93) || (charInt == 95)) {
                    r += c.ToString();
                }
            }

            r = r.Replace("--", "-");
            r += fileExt;

            return r;
        }

        public static bool ValidateFileName(string fileName, string[] extensions)
        {
            bool isValid = false;

            fileName = fileName.ToLower();
            string extension = System.IO.Path.GetExtension(fileName);

            if (extensions.Contains(extension)) {
                isValid = true;
            }

            return isValid;
        }

        //public static IEnumerable<FileUpload> GetFiles(string folder)
        //{
        //    string abs_folder = CreateFolder(folder);
        //    DirectoryInfo dir = new DirectoryInfo(abs_folder);
        //    IEnumerable<FileInfo> fileList = dir.GetFiles("*.*", SearchOption.TopDirectoryOnly);
        //    IEnumerable<FileUpload> fileQuery =
        //            from file in fileList
        //            orderby file.Name
        //            select new FileUpload {
        //                Name = file.Name,
        //                VirtualPath = String.Format("{0}/{1}", folder, file.Name)
        //            };

        //    return fileQuery;
        //}

        public static string FileSizeKb(string file)
        {
            string abs_file = HttpContext.Current.Server.MapPath(file);
            var info = new FileInfo(abs_file);
            long len = info.Length / 1024;

            return String.Format("{0:0.##} Kb", len);
        }

        public static string ConvertFileToBase64(string absolutePath)
        {
            using (System.Drawing.Image image = System.Drawing.Image.FromFile(absolutePath)) {
                using (MemoryStream m = new MemoryStream()) {
                    image.Save(m, image.RawFormat);
                    byte[] imageBytes = m.ToArray();
                    string base64String = Convert.ToBase64String(imageBytes);
                    return base64String;
                }
            }
        }
    }
}