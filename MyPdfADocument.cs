using iText.Kernel.Pdf;
using iText.Kernel.Validation;
using iText.Pdfa;

namespace pdf_merger
{
    internal class MyPdfADocument : PdfADocument
    {
        public bool IsCheckIsoConformanceEnabled { get; set; } = true;

        public MyPdfADocument(PdfReader reader, PdfWriter writer) : base(reader, writer)
        {
        }

        public MyPdfADocument(PdfWriter writer, PdfAConformance aConformance, PdfOutputIntent outputIntent) : base(writer, aConformance, outputIntent)
        {
        }

        public MyPdfADocument(PdfReader reader, PdfWriter writer, StampingProperties properties) : base(reader, writer, properties)
        {
        }

        public MyPdfADocument(PdfWriter writer, PdfAConformance aConformance, PdfOutputIntent outputIntent, DocumentProperties properties) : base(writer, aConformance, outputIntent, properties)
        {
        }

        public override void CheckIsoConformance(IValidationContext validationContext)
        {
            if (IsCheckIsoConformanceEnabled)
            {
                base.CheckIsoConformance(validationContext);
            }
        }
    }
}
