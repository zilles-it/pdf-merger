using System.Collections.Generic;

namespace pdf_merger.Model
{
    public class PdfMergerParams
    {
        public string OutputPath { get; set; }

        public List<string> InputFiles { get; set; }

        public double MaxFileSizeMb { get; set; }

        public List<PdfMergerAttachment> Attachments { get; set; }

        public Dictionary<string, string> Metadata { get; set; }
    }
}