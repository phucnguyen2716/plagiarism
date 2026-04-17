using System;
using System.Text;
using UglyToad.PdfPig;

namespace PlagiarismApi.Services
{
    public interface IPdfService
    {
        string ExtractText(string filePath);
    }

    public class PdfService : IPdfService
    {
        public string ExtractText(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
                return string.Empty;

            var sb = new StringBuilder();
            try
            {
                using var pdf = PdfDocument.Open(filePath);
                foreach (var page in pdf.GetPages())
                {
                    sb.AppendLine(page.Text);
                }
            }
            catch (Exception)
            {
                // Log error
                return string.Empty;
            }

            return sb.ToString();
        }
    }
}
