using Capstone_360s.Models.FeedbackDb;
using iText.Layout;

namespace Capstone_360s.Interfaces.IService
{
    public interface IWritePdf<TDocument, TInversion> 
        where TDocument : class
        where TInversion : class
    {
        public delegate void WritePdfContent<TDocument>(Document document, TDocument documentMaterial);
        public Task<List<FeedbackPdf>> GeneratePdfs(IEnumerable<TInversion> invertedQualtrics, int currentRoundId);
        public Task<byte[]> WritePdfAsync(WritePdfContent<TDocument> pdfWriter, TDocument documentMaterial);
    }
}
