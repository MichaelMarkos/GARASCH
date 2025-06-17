using iTextSharp.text;
using iTextSharp.text.pdf;

namespace NewGarasAPI.Models.AccountAndFinance
{
    public class HeaderFooter2 : PdfPageEventHelper
    {
        public override void OnEndPage(PdfWriter writer, Document document)
        {
            var dateTime2 = DateTime.Now.ToString();
            ColumnText.ShowTextAligned(writer.DirectContent, Element.ALIGN_LEFT, new Phrase("Generated at: " + dateTime2, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000")))), 18, 10, 0);
            ColumnText.ShowTextAligned(writer.DirectContent, Element.ALIGN_LEFT, new Phrase("Generated at: " + dateTime2, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000")))), 18, 10, 0);
            ColumnText.ShowTextAligned(writer.DirectContent, Element.ALIGN_RIGHT, new Phrase("page " + document.PageNumber, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000")))), 600, 10, 0);
        }
    }
}
