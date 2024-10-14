using iText.Layout;

namespace Capstone_360s.Interfaces.IService
{
    public interface IWritePdf<T> where T : class
    {
        public delegate void WritePdfContent<T>(Document document, T documentMaterial);
        public Task<byte[]> WritePdfAsync(WritePdfContent<T> pdfWriter, T documentMaterial);
    }
}
