using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using MagicApps.Infrastructure.Services;

namespace MagicApps.Models
{
    public class FileUploadModel
    {
        public FileUploadModel()
        {
            MaxImgSize = Convert.ToInt16(System.Configuration.ConfigurationManager.AppSettings["Settings.CMS.UploadImgSize"]);
        }

        public string Folder { get; set; }

        public HttpPostedFileBase NewFile { get; set; }

        public string NewFileName { get; set; }

        public string ExistingFile { get; set; }

        public int MaxImgSize { get; set; }

        [Display(Name = "Remove this file?")]
        public bool RemoveExistingFile { get; set; }

        public bool HasFile()
        {
            if (!string.IsNullOrEmpty(ExistingFile) || NewFile != null) {
                return true;
            }
            else {
                return false;
            }
        }

        public string SaveOrRemoveFile()
        {
            string docName;

            if (RemoveExistingFile && !String.IsNullOrEmpty(ExistingFile)) {
                DeleteFile(ExistingFile);
                docName = null;
            }
            else if (this.NewFile != null) {
                docName = new FileStreamService(NewFile).Save(Folder, NewFileName);
            }
            else if (!String.IsNullOrEmpty(ExistingFile)) {
                docName = ExistingFile;
            }
            else {
                docName = null;
            }

            return docName;
        }

        public string SaveOrRemoveImg()
        {
            string imgName;

            if (RemoveExistingFile && !String.IsNullOrEmpty(ExistingFile)) {
                DeleteFile(ExistingFile);
                imgName = null;
            }
            else if (this.NewFile != null) {
                imgName = new FileStreamService(NewFile).ResizeImgStream(MaxImgSize, Folder);
            }
            else if (!String.IsNullOrEmpty(ExistingFile)) {
                imgName = ExistingFile;
            }
            else {
                imgName = null;
            }

            return imgName;
        } 

        private void DeleteFile(string file)
        {
            var abs_Path = HttpContext.Current.Server.MapPath(ExistingFile);
            FileService.DeleteFile(abs_Path);
        }
    }
}