using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace MagicApps.Infrastructure.Services
{
    public class FileStreamService
    {
        private HttpPostedFileBase FileStream;

        public FileStreamService(HttpPostedFileBase fileStream)
        {
            this.FileStream = fileStream;
        }
        
        public string ResizeImgStream(int imgSize, string imgFolder, string imgName = "")
        {
            Bitmap image = new Bitmap(FileStream.InputStream);

            // Some images sent from mobile devices have a rotation value associated with them that reverts to landscape
            // on save. In these cases, rotate the image back
            RotateFlipType rft = RotateFlipType.RotateNoneFlipNone;
            PropertyItem[] properties = image.PropertyItems;

            foreach (PropertyItem p in properties) {
                if (p.Id == 274) {
                    short orientation = BitConverter.ToInt16(p.Value, 0);

                    switch (orientation) {
                        case 1:
                            rft = RotateFlipType.RotateNoneFlipNone;
                            break;
                        case 3:
                            rft = RotateFlipType.Rotate180FlipNone;
                            break;
                        case 6:
                            rft = RotateFlipType.Rotate90FlipNone;
                            break;
                        case 8:
                            rft = RotateFlipType.Rotate270FlipNone;
                            break;
                    }
                }
            }

            if (rft != RotateFlipType.RotateNoneFlipNone) {
                image.RotateFlip(rft);
            }

            string abs_folder = FileService.CreateFolder(imgFolder);

            if (imgName == "") {
                imgName = FileService.RemoveIllegalCharacters(FileStream.FileName, true).ToLower();
            }

            // Shorten the file name to less than 80 characters
            imgName = TruncateName(imgName);

            imgName = String.Format("{0}/{1}", imgFolder, imgName);
            string abs_imgName = HttpContext.Current.Server.MapPath(imgName);

            if (image.Width > imgSize || image.Height > imgSize) {
                decimal thumbnailSize = (decimal)imgSize;
                decimal newWidth, newHeight;

                if (image.Width > image.Height) {
                    newWidth = thumbnailSize;
                    newHeight = Decimal.Multiply(image.Height, (thumbnailSize / image.Width));
                }
                else {
                    newWidth = Decimal.Multiply(image.Width, (thumbnailSize / image.Height));
                    newHeight = thumbnailSize;
                }

                int iNewWidth, iNewHeight;

                iNewWidth = Convert.ToInt32(newWidth);
                iNewHeight = Convert.ToInt32(newHeight);

                Size newSize = new Size(iNewWidth, iNewHeight);
                Bitmap thumbnailBitmap = new Bitmap(image, newSize);

                var thumbnailGraph = Graphics.FromImage(thumbnailBitmap);
                thumbnailGraph.CompositingQuality = CompositingQuality.HighQuality;
                thumbnailGraph.SmoothingMode = SmoothingMode.HighQuality;
                thumbnailGraph.InterpolationMode = InterpolationMode.HighQualityBilinear;

                var imageRectangle = new Rectangle(0, 0, iNewWidth, iNewHeight);
                thumbnailGraph.DrawImage(image, imageRectangle);

                MemoryStream newStream = new MemoryStream();

                FileService.DeleteFile(abs_imgName);
                thumbnailBitmap.Save(abs_imgName, image.RawFormat);

                thumbnailGraph.Dispose();
                thumbnailBitmap.Dispose();
                image.Dispose();
            }
            else {
                FileService.DeleteFile(abs_imgName);
                FileStream.SaveAs(abs_imgName);
            }

            return imgName;
        }

        public byte[] ResizeImgStream(int imgSize)
        {
            Bitmap image = new Bitmap(FileStream.InputStream);

            if (image.Width > imgSize || image.Height > imgSize) {
                decimal thumbnailSize = (decimal)imgSize;
                decimal newWidth, newHeight;

                if (image.Width > image.Height) {
                    newWidth = thumbnailSize;
                    newHeight = Decimal.Multiply(image.Height, (thumbnailSize / image.Width));
                }
                else {
                    newWidth = Decimal.Multiply(image.Width, (thumbnailSize / image.Height));
                    newHeight = thumbnailSize;
                }

                int iNewWidth, iNewHeight;

                iNewWidth = Convert.ToInt32(newWidth);
                iNewHeight = Convert.ToInt32(newHeight);

                Size newSize = new Size(iNewWidth, iNewHeight);
                Bitmap thumbnailBitmap = new Bitmap(image, newSize);

                var thumbnailGraph = Graphics.FromImage(thumbnailBitmap);
                thumbnailGraph.CompositingQuality = CompositingQuality.HighQuality;
                thumbnailGraph.SmoothingMode = SmoothingMode.HighQuality;
                thumbnailGraph.InterpolationMode = InterpolationMode.HighQualityBilinear;

                var imageRectangle = new Rectangle(0, 0, iNewWidth, iNewHeight);
                thumbnailGraph.DrawImage(image, imageRectangle);

                MemoryStream newStream = new MemoryStream();

                thumbnailBitmap.Save(newStream, image.RawFormat);

                thumbnailGraph.Dispose();
                thumbnailBitmap.Dispose();
                image.Dispose();

                return newStream.ToArray();
            }
            else {
                ImageConverter converter = new ImageConverter();
                return (byte[])converter.ConvertTo(image, typeof(byte[]));
            }
        }

        public byte[] Save()
        {
            byte[] buffer = new byte[FileStream.InputStream.Length];
            byte[] myFile;

            using (MemoryStream ms = new MemoryStream(buffer)) {
                ms.Read(buffer, 0, (int)ms.Length);
                myFile = ms.ToArray();
                ms.Close();
            }

            return myFile;
        }

        public string Save(string docFolder, string docName = "")
        {
            string abs_folder = FileService.CreateFolder(docFolder);

            if (String.IsNullOrEmpty(docName)) {
                docName = FileService.RemoveIllegalCharacters(FileStream.FileName, true).ToLower();
            }

            docName = String.Format("{0}/{1}", docFolder, docName);

            // Shorten the file name to less than 80 characters
            docName = TruncateName(docName);

            string abs_docName = HttpContext.Current.Server.MapPath(docName);

            try {
                FileService.DeleteFile(abs_docName);
                FileStream.SaveAs(abs_docName);
                return docName;
            }
            catch (Exception ex) {
                return String.Format("Error: {0}", ex.Message);
            }
        }

        private string TruncateName(string name)
        {
            // Shorten the file name to less than 80 characters
            if (name.Length > 75) {
                string extension = Path.GetExtension(name);
                name = name.Substring(0, 75) + extension;
            }

            return name;
        }
    }
}