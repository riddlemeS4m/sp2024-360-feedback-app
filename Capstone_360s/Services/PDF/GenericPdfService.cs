using Capstone_360s.Interfaces.IService;
using Capstone_360s.Models.CapstoneRoster;
using Capstone_360s.Models.Survey;
using iText.Kernel.Pdf;
using iText.Layout;
namespace Capstone_360s.Services.PDF
{
    public class GenericPdfService<T> : IWritePdf<T> where T : class
    {
        private readonly ILogger<GenericPdfService<T>> _logger;

        public GenericPdfService(ILogger<GenericPdfService<T>> logger)
        {
            _logger = logger;
        }

        public async Task<byte[]> WritePdfAsync(IWritePdf<T>.WritePdfContent<T> pdfWriter, T documentContent)
        {
            _logger.LogInformation("Writing PDF...");

            // Create a MemoryStream to hold the PDF in memory
            using var memoryStream = new MemoryStream();

            // Initialize PDF writer and document
            var writer = new PdfWriter(memoryStream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            // Start writing the pdf

            pdfWriter(document, documentContent);

            // Stop writing the pdf

            // Close document
            document.Close();

            // Return the PDF as a byte array
            return memoryStream.ToArray();
        }
    }
}
