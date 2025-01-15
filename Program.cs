using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Mono.Options;
using Newtonsoft.Json;
using pdf_merger.Extension;
using pdf_merger.Model;

namespace PdfUtil
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                PdfMergerParams pdfParams = ParseArgs(args);

                Console.WriteLine("PDF printing started");

                if (pdfParams.MaxFileSizeMb <= 0)
                {
                    pdfParams.MaxFileSizeMb = 99999;
                }

                if (pdfParams.Attachments == null)
                {
                    pdfParams.Attachments = new List<PdfMergerAttachment>();
                }

                var pdfUtils = new PdfExeUtils();

                pdfUtils.MergePdfFiles(pdfParams);

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace);

                return 1;
            }
        }

        private static PdfMergerParams ParseArgs(string[] args)
        {
            var pdfParams = new PdfMergerParams()
            {
                Attachments = new List<PdfMergerAttachment>(),
                InputFiles = new List<string>(),
                OutputPath = "",
                MaxFileSizeMb = 9999999,
                Metadata = new Dictionary<string, string>()
            };

            var optionSet = new OptionSet();

            optionSet.Add("c|config=", "Path to config.json file", (v) =>
            {
                string configPfad = v;

                if (!File.Exists(configPfad))
                {
                    throw new FileNotFoundException("File not found: " + configPfad);
                }

                string json = File.ReadAllText(configPfad);

                pdfParams = JsonConvert.DeserializeObject<PdfMergerParams>(json);

                if (pdfParams.Metadata == null)
                {
                    pdfParams.Metadata = new Dictionary<string, string>();
                }

                if (pdfParams.Attachments == null)
                {
                    pdfParams.Attachments = new List<PdfMergerAttachment>();
                }
            });

            optionSet.Add("s|src=", "Source files, separated by semikolon ; (pdf, jpg, png, tif)", (v) =>
            {
                pdfParams.InputFiles = v.Split(';').ToList();
            });

            optionSet.Add("d|dst=", "The destination file", (v) =>
            {
                pdfParams.OutputPath = v;
            });

            optionSet.Add("z|zfxml=", "ZUGFeRD xml file path", (v) =>
            {
                if (!File.Exists(v))
                {
                    throw new FileNotFoundException("ZUGFeRD file not found: " + v);
                }

                pdfParams.Attachments.Add(new PdfMergerAttachment()
                {
                    Key = "ZUGFeRD Invoice",
                    FilePath = v,
                    Filename = "factur-x.xml",
                    Description = "factur-x"
                });
            });

            optionSet.Add("m|MaxFileSizeMb=", "Max file size of outpud pdf file (MB)", (v) =>
            {
                pdfParams.MaxFileSizeMb = double.Parse(v, CultureInfo.InvariantCulture);
            });

            optionSet.Add("title=", "Metadata Title", (v) =>
            {
                pdfParams.Metadata["Title"] = v;
            });

            optionSet.Add("author=", "Metadata Author", (v) =>
            {
                pdfParams.Metadata["Author"] = v;
            });

            optionSet.Add("subject=", "Metadata Subject", (v) =>
            {
                pdfParams.Metadata["Subject"] = v;
            });

            optionSet.Add("keywords=", "Metadata Keywords", (v) =>
            {
                pdfParams.Metadata["Keywords"] = v;
            });

            optionSet.Add("creator=", "Metadata Creator", (v) =>
            {
                pdfParams.Metadata["Creator"] = v;
            });

            optionSet.Add("producer=", "Metadata Producer", (v) =>
            {
                pdfParams.Metadata["Producer"] = v;
            });

            List<string> extra = optionSet.Parse(args);

            if (pdfParams.InputFiles == null || pdfParams.InputFiles.Count == 0 || pdfParams.OutputPath.IsBlank())
            {
                Console.WriteLine("Invalid argument");
                optionSet.WriteOptionDescriptions(Console.Out);

                Console.WriteLine();
                Console.WriteLine("Example:");
                Console.WriteLine(@"pdf-merger.exe -s=""c:\temp\invoice.pdf:c:\temp\attachments.pdf"" -z=""C:\temp\zugferd.xml"" -d=""C:\temp\out.pdf"" -m=20");
                Console.WriteLine();

                throw new Exception("Invalid arguments");
            }

            return pdfParams;
        }
    }
}