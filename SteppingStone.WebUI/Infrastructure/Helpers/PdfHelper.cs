using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using iTextSharp.text;
using iTextSharp.text.pdf;
using MagicApps.Infrastructure.Services;
using MagicApps.Models;
using Microsoft.AspNet.Identity.Owin;
using SteppingStone.Domain.Context;
using SteppingStone.Domain.Entities;

namespace SteppingStone.WebUI.Infrastructure.Helpers
{
    
    public class PdfHelper
    {
        private ApplicationDbContext db;

        public string ServiceUserId;

        public PdfHelper(string serviceUserId)
        {
            this.db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
            this.ServiceUserId = serviceUserId;
        }

        
        public UpsertModel CreatePaymentsReport(IEnumerable<Student> students)
        {
            var upsert = new UpsertModel();

            try
            {
                string docName = @"Payments_Report.pdf";

                docName = FileService.RemoveIllegalCharacters(docName, true);

                string destinationFolder = "~/App_Data";//Settings.DOCFOLDER;

                string logo = HttpContext.Current.Server.MapPath("~/Content/Imgs/ss_logo.png");

                // Create a temp folder if not there
                string temp_folder = FileService.CreateFolder(string.Format(@"{0}\Reports\Temp", destinationFolder));

                // Work on the Temp File
                string abs_TempDoc = String.Format(@"{0}\{1}", temp_folder, docName);

                destinationFolder = Settings.DOCFOLDER;

                // Save to New File
                string abs_NewDoc = String.Format(@"{0}\Reports\{1}", destinationFolder, docName);

                // Delete the old temp file
                FileService.DeleteFile(abs_TempDoc);

                // Create a document
                var doc = new Document(PageSize.A4);

                // Make landscape
                doc.SetPageSize(PageSize.A4.Rotate());

                // Create the document object
                PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(abs_TempDoc, FileMode.Create));

                // Events
                ITextEvents header = new ITextEvents();
                writer.PageEvent = header;

                // Open for editing/creation
                doc.Open();

                // Lets go!
                // ++++++++++++++++++++++++++++++++++++++++++++
                // +++++++++++++++++ START ++++++++++++++++++++
                PdfPTable table;
                List<float> cellWidths;
                PdfPCell cell;
                //Paragraph paragraph;
                //Chunk chunk;

                //SteppingStone Logo
                doc.Add(ImageCell(logo));

                doc.Add(ParseHeading(string.Format("{0} Fees Report ", Settings.COMPANY_NAME)));

                cellWidths = new List<float> { 3f, 17f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f  };
                table = SetTable(cellWidths.ToArray(), true, true);

                table.AddCell(SmallBlackLabelCell("#"));
                table.AddCell(SmallBlackLabelCell("Pupil name"));
                table.AddCell(SmallBlackLabelCell("1st Installment"));
                table.AddCell(SmallBlackLabelCell("2nd Installment"));
                table.AddCell(SmallBlackLabelCell("Transport"));
                table.AddCell(SmallBlackLabelCell("Swimming"));
                table.AddCell(SmallBlackLabelCell("Old Debt"));
                table.AddCell(SmallBlackLabelCell("Total"));
                table.AddCell(SmallBlackLabelCell("Total Pay't"));
                table.AddCell(SmallBlackLabelCell("Total Balance"));

                int counter = 0;
                double OldDebt = 0;
                double AmountToPay = 0;
                double PaymentsTotal = 0;
                double Outstanding = 0;
                double firstPayts = 0;
                double otherPayts = 0;
                double Transport = 0;
                double Swimming = 0;

                foreach (var student in students)
                {
                    counter++;
                    table.AddCell(DefaultCell(counter.ToString()));
                    table.AddCell(DefaultCell(student.FullName));

                    table.AddCell(DefaultCell(student.FirstCurrentTermPayment.ToString("n0")));
                    firstPayts = firstPayts + student.FirstCurrentTermPayment;

                    table.AddCell(DefaultCell(student.OtherCurrentTermPayments.ToString("n0")));
                    otherPayts = otherPayts + student.OtherCurrentTermPayments;

                    table.AddCell(DefaultCell(student.Transport.ToString("n0")));
                    Transport = Transport + student.Transport;

                    table.AddCell(DefaultCell(student.Swimming.ToString("n0")));
                    Swimming = Swimming + student.Swimming;
                    // increment totals
                    table.AddCell(DefaultCell(student.OldDebt.ToString("n0")));
                    OldDebt = OldDebt + student.OldDebt;

                    table.AddCell(DefaultCell(student.AmountToPay.ToString("n0")));
                    AmountToPay = AmountToPay + student.AmountToPay;

                    table.AddCell(DefaultCell(student.StudentTermPaymentsTotal().ToString("n0")));
                    PaymentsTotal = PaymentsTotal + student.StudentTermPaymentsTotal();

                    table.AddCell(DefaultCell(student.Outstanding.ToString("n0")));
                    Outstanding = Outstanding + student.Outstanding;
                }

                cell = SmallBlackLabelCell(counter.ToString() + " Pupils");
                cell.Colspan = 2;
                table.AddCell(cell);
                table.AddCell(SmallBlackLabelCell(firstPayts.ToString("n0")));
                table.AddCell(SmallBlackLabelCell(otherPayts.ToString("n0")));
                table.AddCell(SmallBlackLabelCell(Transport.ToString("n0")));
                table.AddCell(SmallBlackLabelCell(Swimming.ToString("n0")));
                table.AddCell(SmallBlackLabelCell(OldDebt.ToString("n0")));
                table.AddCell(SmallBlackLabelCell(AmountToPay.ToString("n0")));
                table.AddCell(SmallBlackLabelCell(PaymentsTotal.ToString("n0")));
                table.AddCell(SmallBlackLabelCell(Outstanding.ToString("n0")));

                doc.Add(table);

                
                // +++++++++++++++++ FINISH +++++++++++++++++++
                // ++++++++++++++++++++++++++++++++++++++++++++

                // Close and Save the document
                doc.Close();
                doc.Dispose();
                writer.Dispose();

                // Delete the saved file
                FileService.DeleteFile(abs_NewDoc);

                // Save the temp file to the save file
                File.Copy(abs_TempDoc, abs_NewDoc);

                upsert.RecordId = 1.ToString();
                upsert.ErrorMsg = docName;
            }
            catch (Exception ex)
            {

                upsert.ErrorMsg = ex.Message;
            }


            return upsert;
        }

        public UpsertModel CreateClassesReport(IEnumerable<ClassLevel> classes)
        {
            var upsert = new UpsertModel();

            try
            {
                string docName = @"Classes_Report.pdf";

                docName = FileService.RemoveIllegalCharacters(docName, true);

                string destinationFolder = Settings.DOCFOLDER;

                string logo = HttpContext.Current.Server.MapPath("~/Content/Imgs/glo_ss_logo.png");

                // Create a temp folder if not there
                string temp_folder = FileService.CreateFolder(string.Format(@"{0}\Reports\Temp", destinationFolder));

                // Work on the Temp File
                string abs_TempDoc = String.Format(@"{0}\{1}", temp_folder, docName);

                // Save to New File
                string abs_NewDoc = String.Format(@"{0}\Reports\{1}", destinationFolder, docName);

                // Delete the old temp file
                FileService.DeleteFile(abs_TempDoc);

                // Create a document
                var doc = new Document(PageSize.A4);

                // Make landscape
                doc.SetPageSize(PageSize.A4.Rotate());

                // Create the document object
                PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(abs_TempDoc, FileMode.Create));

                // Events
                ITextEvents header = new ITextEvents();
                writer.PageEvent = header;

                // Open for editing/creation
                doc.Open();

                // Lets go!
                // ++++++++++++++++++++++++++++++++++++++++++++
                // +++++++++++++++++ START ++++++++++++++++++++
                PdfPTable table;
                List<float> cellWidths;
                PdfPCell cell;
                //Paragraph paragraph;
                //Chunk chunk;

                //SteppingStone Logo
                doc.Add(ImageCell(logo));

                doc.Add(ParseHeading(string.Format("{0} Class Report ", Settings.COMPANY_NAME)));

                cellWidths = new List<float> { 3f, 17f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f };
                table = SetTable(cellWidths.ToArray(), true, true);

                table.AddCell(SmallBlackLabelCell("#"));
                table.AddCell(SmallBlackLabelCell("Class"));
                table.AddCell(SmallBlackLabelCell("No. Pupils"));
                table.AddCell(SmallBlackLabelCell("Total Payments (Ugx)"));
                table.AddCell(SmallBlackLabelCell("Balance (Ugx)"));
                table.AddCell(SmallBlackLabelCell("Payment Ratio"));

                int counter = 0;
                double PaymentsTotal = 0;
                double Outstanding = 0;
                double Students = 0;

                foreach (var classLevel in classes)
                {
                    counter++;
                    table.AddCell(DefaultCell(counter.ToString()));

                    table.AddCell(DefaultCell(classLevel.GetClass()));

                    table.AddCell(DefaultCell(classLevel.StudentsThisTerm.Count().ToString()));
                    Students = Students + classLevel.StudentsThisTerm.Count();

                    table.AddCell(DefaultCell(classLevel.GetTermTotal()));
                    PaymentsTotal = PaymentsTotal + classLevel.TotalRevenue;

                    table.AddCell(DefaultCell(classLevel.Outstanding.ToString("n0")));
                    Outstanding = Outstanding + classLevel.Outstanding;

                    table.AddCell(DefaultCell(classLevel.Ratio.ToString()));
                    
                }

                cell = SmallBlackLabelCell(counter.ToString() + " Classes");
                cell.Colspan = 2;
                table.AddCell(cell);
                table.AddCell(SmallBlackLabelCell(Students.ToString("n0")));
                table.AddCell(SmallBlackLabelCell(PaymentsTotal.ToString("n0")));
                table.AddCell(SmallBlackLabelCell(Outstanding.ToString("n0")));

                doc.Add(table);


                // +++++++++++++++++ FINISH +++++++++++++++++++
                // ++++++++++++++++++++++++++++++++++++++++++++

                // Close and Save the document
                doc.Close();
                doc.Dispose();
                writer.Dispose();

                // Delete the saved file
                FileService.DeleteFile(abs_NewDoc);

                // Save the temp file to the save file
                File.Copy(abs_TempDoc, abs_NewDoc);

                upsert.RecordId = 1.ToString();
                upsert.ErrorMsg = docName;
            }
            catch (Exception ex)
            {

                upsert.ErrorMsg = ex.Message;
            }


            return upsert;
        }


        private Paragraph ParseParagraph(string text)
        {
            Paragraph paragraph = new Paragraph(text);
            paragraph.SpacingAfter = 15;
            return paragraph;
        }

        private Paragraph ParseParagraph(Chunk chunk)
        {
            Paragraph paragraph = new Paragraph(chunk);
            paragraph.SpacingAfter = 15;
            return paragraph;
        }

        private Paragraph ParsePageHeading(string text)
        {
            //Font font = FontFactory.GetFont("Arial", Font.BOLD, Font.UNDERLINE);
            //font.Size = 24;
            Font font = FontFactory.GetFont(BaseFont.HELVETICA, 24, Font.BOLD);
            Paragraph paragraph = new Paragraph(text, font);
            paragraph.SpacingAfter = 25;
            paragraph.Alignment = Element.ALIGN_CENTER;
            return paragraph;
        }

        private Paragraph ParseHeading(string text)
        {
            Font font = FontFactory.GetFont(BaseFont.HELVETICA, 18, Font.BOLD);
            Paragraph paragraph = new Paragraph(text, font);
            paragraph.Alignment = Element.ALIGN_CENTER;
            paragraph.SpacingAfter = 15;
            return paragraph;
        }

        private Chunk BoldText(string text)
        {
            Font font = FontFactory.GetFont(BaseFont.HELVETICA, 12, Font.BOLD);
            return new Chunk(text, font);
        }

        private PdfPTable SetTable(int columns)
        {
            PdfPTable table = new PdfPTable(columns);

            table.WidthPercentage = 100;

            return table;
        }

        private PdfPTable SetTable(float[] columnWidths, bool percentages, bool border)
        {
            PdfPTable table = new PdfPTable(columnWidths);

            if (border)
            {
                SetTableParams(table);
            }
            else
            {
                table.DefaultCell.BorderColor = BaseColor.WHITE;
                table.DefaultCell.BorderWidth = 0;
            }

            if (percentages)
            {
                table.WidthPercentage = 100;
                //table.SetWidthPercentage(columnWidths, PageSize.A4);
            }
            else
            {
                table.TotalWidth = columnWidths.Sum();
                table.SetWidths(columnWidths);
            }

            return table;
        }

        private void SetTableParams(PdfPTable table)
        {
            table.DefaultCell.Padding = 5;
            table.DefaultCell.BorderColor = new BaseColor(204, 204, 204);
            table.DefaultCell.BorderWidth = 1;
            table.HorizontalAlignment = Element.ALIGN_LEFT;
            table.SpacingAfter = 20;

        }

        private PdfPCell StandardCell(string text)
        {            
            PdfPCell cell = new PdfPCell(new Phrase(text));

            cell.BorderColor = new BaseColor(204, 204, 204);
            cell.BorderWidth = 1;
            cell.Padding = 5;

            return cell;
        }

        private PdfPCell DefaultCell(string text)
        {
            Font font = FontFactory.GetFont(BaseFont.HELVETICA, 8, Font.BOLD);
          
            PdfPCell cell = new PdfPCell(new Phrase(text, font));
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.Padding = 2;

            return cell;
        }

        private PdfPCell SmallBlackLabelCell(string text)
        {
            Font font = FontFactory.GetFont(BaseFont.HELVETICA, 8, Font.BOLD);
            font.SetColor(255, 255, 255);

            PdfPCell cell = new PdfPCell(new Phrase(text, font));
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.BackgroundColor = new BaseColor(0, 0, 0);
            cell.Padding = 2;

            return cell;
        }

        private PdfPCell AnswerCell(string text)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text));

            cell.BorderColor = new BaseColor(204, 204, 204);
            cell.BorderWidth = 1;
            cell.Padding = 5;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;

            return cell;
        }

        private PdfPCell LabelCell(string text)
        {
            PdfPCell cell = StandardCell(text);

            cell.BackgroundColor = new BaseColor(241, 241, 241);

            return cell;
        }

        private Image ImageCell(string path)
        {
            Image img = Image.GetInstance(path);

            //resize image 
            img.ScaleToFit(140f, 70f);

            //space around image
            img.SpacingAfter = 5;
            img.SpacingBefore = 10;

            //align image
            img.Alignment = Element.ALIGN_CENTER;

            return img;
        }

        public Activity CreateActivity(string title, string description)
        {
            var activity = new Activity {
                Title = title,
                Description = description,
                RecordedById = ServiceUserId
            };
            db.Activities.Add(activity);
            return activity;
        }
    }

    public class ITextEvents : PdfPageEventHelper
    {

        public ITextEvents()
        {
        }

        // This is the contentbyte object of the writer
        PdfContentByte cb;
        // we will put the final number of pages in a template
        PdfTemplate template;
        // this is the BaseFont we are going to use for the header / footer
        BaseFont baseFont = null;
        // This keeps track of the creation time
        DateTime PrintTime = DateTime.Now;

        // we override the onOpenDocument method
        public override void OnOpenDocument(PdfWriter writer, Document document)
        {
            string error;

            try {
                PrintTime = DateTime.Now;
                baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                cb = writer.DirectContent;
                template = cb.CreateTemplate(50, 50);
            }
            catch (DocumentException de) {
                error = de.Message;
            }
            catch (System.IO.IOException ioe) {
                error = ioe.Message;
            }
        }

        public override void OnStartPage(PdfWriter writer, Document document)
        {
            base.OnStartPage(writer, document);
            Rectangle pageSize = document.PageSize;
            string showTxt = "-";
            cb.BeginText();
            cb.SetFontAndSize(baseFont, 8);
            cb.SetTextMatrix(pageSize.GetLeft(35), pageSize.GetTop(20));
      
            cb.ShowText(showTxt);
            cb.SetRGBColorFill(100, 100, 100);
            cb.EndText();

            string name = "-";
            cb.BeginText();
            cb.SetFontAndSize(baseFont, 8);
            cb.ShowTextAligned(PdfContentByte.ALIGN_RIGHT,
                name,
                pageSize.GetRight(35),
                pageSize.GetTop(20), 0);
            cb.EndText();

            // Otherwise, changes text of main body
            cb.SetRGBColorFill(0, 0, 0);
        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            base.OnEndPage(writer, document);
            int pageN = writer.PageNumber;
            String text = "Page " + pageN;
            float len = baseFont.GetWidthPoint(text, 8);

            Rectangle pageSize = document.PageSize;
            cb.SetRGBColorFill(100, 100, 100);
            cb.BeginText();
            cb.SetFontAndSize(baseFont, 8);
            cb.SetTextMatrix(pageSize.GetLeft(35), pageSize.GetBottom(20));
            cb.ShowText(text);
            cb.EndText();
            cb.AddTemplate(template, pageSize.GetLeft(35) + len, pageSize.GetBottom(20));

            cb.BeginText();
            cb.SetFontAndSize(baseFont, 8);
            cb.ShowTextAligned(PdfContentByte.ALIGN_RIGHT,
                "Last updated: " + PrintTime.ToString("dd/MM/yyyy HH:mm"),
                pageSize.GetRight(35),
                pageSize.GetBottom(20), 0);
            cb.EndText();

            //address on footer
            string address = String.Join(", ", Settings.COMPANY_ADDRESS);
            cb.BeginText();
            cb.SetFontAndSize(baseFont, 8);
            cb.ShowTextAligned(PdfContentByte.ALIGN_CENTER,
                address,
                pageSize.GetLeft(250),
                pageSize.GetBottom(20), 0);
            cb.EndText();
        }

        public override void OnCloseDocument(PdfWriter writer, Document document)
        {
            base.OnCloseDocument(writer, document);
            //template.BeginText();
            // template.SetFontAndSize(bf, 8);
            // template.SetTextMatrix(0, 0);
            // template.ShowText("" + (writer.PageNumber - 1));
            // template.EndText();
        }
    }
}