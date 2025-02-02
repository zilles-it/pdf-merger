﻿/*
    This file is part of the pdf-merger project.
    Copyright (c) 2025 ZILLES-IT GmbH

    AGPL licensing:
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Mono.Options;
using Newtonsoft.Json;
using pdf_merger.Extension;
using pdf_merger.Model;
using PdfUtil;

namespace pdf_merger
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                PrintLicenseText();

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

                var pdfUtils = new PdfMergerUtil();

                pdfUtils.MergePdfFiles(pdfParams);

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace);

                return 1;
            }
        }

        private static void PrintLicenseText()
        {
            string text = @"**************************************************
  This application uses software licensed under the
  GNU Affero General Public License (AGPL) Version 3.

  The AGPL ensures that the source code of the utilized
  open-source components is available to everyone.

  For information about the components used and access
  to the source code, please visit:
  <URL to the license page or source code repository>

  For more details about the AGPL, visit:
  https://www.gnu.org/licenses/agpl-3.0.html
  
  Source code of this application is available:
  https://github.com/zilles-it/pdf-merger/
**************************************************
";
            Console.WriteLine(text);
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

            optionSet.Add("c |config=", "Path to config.json file", (v) =>
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