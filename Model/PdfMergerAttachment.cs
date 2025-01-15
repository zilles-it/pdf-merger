namespace pdf_merger.Model
{
    public class PdfMergerAttachment
    {
        /// <summary>
        /// e.g. ZUGFeRD Invoice
        /// </summary>
        public string Key { get; set; }

        public string FilePath { get; set; }

        public string Filename { get; set; }

        public string Description { get; set; }
    }
}