using iTextSharp.text;
using iTextSharp.text.pdf;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Entities;
using System.Net;

namespace NewGarasAPI.Models.AccountAndFinance
{
    public class HeaderFooter3 : PdfPageEventHelper
    {
        //private Helper.Helper _helper;
        private GarasTestContext _Context;
        private HttpRequest Request;

        public HeaderFooter3(HttpRequest request, GarasTestContext context)
        {
            Request = request;   
            _Context = context;
            //_helper = new Helper.Helper();
        }
        //public override void OnEndPage(PdfWriter writer, Document document)
        //{
        //    //base.OnEndPage(writer,  document);()


        //    //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
        //    //WebHeaderCollection headers = request.Headers;
        //    HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers,ref _Context);

        //    //PdfPTable tbHeader = new PdfPTable(1);
        //    //tbHeader.TotalWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin;
        //    //tbHeader.DefaultCell.Border = 0;

        //    //tbHeader.AddCell(new Paragraph());


        //    //PdfPCell _cell = new PdfPCell(new Paragraph());
        //    //_cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //    //tbHeader.AddCell(_cell);
        //    //tbHeader.AddCell(new Paragraph());

        //    //tbHeader.WriteSelectedRows(0, -1, document.LeftMargin, writer.PageSize.GetBottom(document.BottomMargin) , writer.DirectContent);

        //    //PdfPCell _cell = new PdfPCell(new Paragraph());
        //    //PdfPTable tbFooter = new PdfPTable(1);
        //    //tbFooter.TotalWidth = document.PageSize.Width  - document.LeftMargin  - document.RightMargin;
        //    //tbFooter.DefaultCell.Border = 0;
        //    //tbFooter.AddCell(new Paragraph());
        //    //var dateTime2 = DateTime.Now.ToString("dd-MM-yyyy").Split(' ')[0];

        //    //_cell = new PdfPCell(new Paragraph("Generated at: " + dateTime2));
        //    //_cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //    //_cell.Border = 0;

        //    //tbFooter.AddCell(_cell);



        //    //_cell = new PdfPCell(new Paragraph("Page" + " " + writer.PageNumber));
        //    //_cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //    //_cell.Border = 0;

        //    //tbFooter.AddCell(_cell);

        //    //tbFooter.WriteSelectedRows(0, -1, document.LeftMargin  , writer.PageSize.GetBottom(document.BottomMargin) + 5, writer.DirectContent);


        //    var UserName = Helper.Common.GetUserName(validation.userID,_Context);
        //    var dateTime2 = DateTime.Now.ToString();
        //    ColumnText.ShowTextAligned(writer.DirectContent, Element.ALIGN_LEFT, new Phrase("Generated at: " + dateTime2 + " " + "By " + " " + UserName, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000")))), 18, 10, 0);
        //    ColumnText.ShowTextAligned(writer.DirectContent, Element.ALIGN_RIGHT, new Phrase("page " + document.PageNumber, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000")))), 825, 10, 0);


        //}

    }
}
