using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace MagicApps.Models.CKEditor
{
    public class Upload
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public long FileSize { get; set; }
        public string Dimensions { get; set; }
    }

    public class UploadDS
    {
        public ICollection GetDataSet(string type, string search)
        {
            ArrayList ar = new ArrayList();
            Upload upload;
            string folder = String.Format("/content/uploads/{0}", type);
            string abs_folder = HttpContext.Current.Server.MapPath(folder);

            FileInfo fInfo;

            if (Directory.Exists(abs_folder)) {
                string[] files;

                if (!string.IsNullOrEmpty(search)) {
                    files = Directory.GetFiles(abs_folder).Where(fileName => fileName.ToLower().Contains(search.ToLower())).ToArray();
                }
                else {
                    files = files = Directory.GetFiles(abs_folder);
                }

                foreach (string f in files) {
                    fInfo = new FileInfo(f);
                    upload = new Upload {
                        Name = fInfo.Name, FileSize  = (fInfo.Length / 1024), Url = String.Format("{0}/{1}", folder, fInfo.Name)
                    };

                    if (type == "imgs") {
                        using (System.Drawing.Image img = System.Drawing.Image.FromFile(HttpContext.Current.Server.MapPath(upload.Url))) {
                            upload.Dimensions = String.Format("{0}px x {1}px", img.Width, img.Height);
                        }
                    }

                    ar.Add(upload);
                }
            }

            return ar;
        }

        public int DeleteFile(string url)
        {
            string f = HttpContext.Current.Server.MapPath(url);
            int r = 0;

            if (File.Exists(f)) {
                try {
                    File.Delete(f);
                    r = 1;
                } catch {
                    r = 2;
                }
            }

            return r;
        }
    }
}