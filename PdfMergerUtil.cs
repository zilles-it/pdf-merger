/*
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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Filespec;
using iText.Kernel.XMP.Impl;
using iText.Pdfa;
using pdf_merger;
using pdf_merger.Extension;
using pdf_merger.Model;

namespace PdfUtil
{
    public class PdfMergerUtil
    {
        private const double MB = 1024 * 1024;

        private PdfWriter writer = null;
        private PdfOutputIntent intent = null;
        private PdfADocument targetPdfDocument = null;
        private FileStream fs = null;

        private int counter = 1;

        public void MergePdfFiles(PdfMergerParams pdfParams)
        {
            counter = 1;

            string pdfPfad = pdfParams.OutputPath;

            if (string.IsNullOrWhiteSpace(pdfPfad))
            {
                pdfPfad = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-zzz");
            }

            if (!pdfPfad.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                pdfPfad += ".pdf";
            }

            // Group the input files so that the respective PDF file size is not exceeded
            var inputFileInfos = pdfParams.InputFiles.Where(p => p.IsNotBlank()).Select(p => new FileInfo(p)).ToList();

            // First check whether all files exist
            foreach (FileInfo fileInfo in inputFileInfos)
            {
                if (!fileInfo.Exists)
                {
                    throw new Exception("File not found: " + fileInfo.FullName);
                }
            }

            string directory = System.IO.Path.GetDirectoryName(pdfPfad);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            double currentFileSize = 0;
            bool newPdfFileRequired = true;

            foreach (FileInfo fileInfo in inputFileInfos)
            {
                Console.WriteLine("Processing: " + fileInfo.Name);

                double dateiGroesse = fileInfo.Length;

                if (currentFileSize > 0 && pdfParams.MaxFileSizeMb < 99999 && pdfParams.MaxFileSizeMb > 0)
                {
                    if (currentFileSize + dateiGroesse > pdfParams.MaxFileSizeMb * MB)
                    {
                        // PDF wäre zu gro0. Neues PDF erstellen
                        newPdfFileRequired = true;
                        currentFileSize = 0;
                    }
                }

                currentFileSize += dateiGroesse;

                if (newPdfFileRequired)
                {
                    if (writer != null)
                    {
                        SaveCurrentFile(pdfParams);
                    }

                    newPdfFileRequired = false;

                    InitNextPdfFile(pdfPfad);
                }

                string extension = fileInfo.Extension.TrimToEmpty().ToLower();

                Stream stream = null;

                if (extension == ".pdf")
                {
                    stream = File.OpenRead(fileInfo.FullName);
                }
                else if (extension == ".jpeg" || extension == ".jpg" || extension == ".png" || extension == ".gif" || extension == ".bmp")
                {
                    stream = new MemoryStream();

                    using (PdfWriter tmpWriter = new PdfWriter(stream))
                    {
                        tmpWriter.SetCloseStream(false);

                        using (PdfDocument tmpPdfDoc = new PdfDocument(tmpWriter))
                        {
                            tmpPdfDoc.SetCloseWriter(false);

                            using (var tmpDocument = new iText.Layout.Document(tmpPdfDoc))
                            {
                                // Bild zur neuen Seite hinzufügen
                                ImageData imageData = ImageDataFactory.Create(fileInfo.FullName);
                                var image = new iText.Layout.Element.Image(imageData);
                                image.SetAutoScale(true);
                                tmpDocument.Add(image);
                            }

                        }
                    }

                    stream.Seek(0, SeekOrigin.Begin);
                }
                else if (extension == ".tif" || extension == ".tiff")
                {
                    List<MemoryStream> images = GetAllImages(fileInfo.FullName);

                    stream = new MemoryStream();

                    using (PdfWriter tmpWriter = new PdfWriter(stream))
                    {
                        tmpWriter.SetCloseStream(false);

                        using (PdfDocument tmpPdfDoc = new PdfDocument(tmpWriter))
                        {
                            tmpPdfDoc.SetCloseWriter(false);

                            using (var tmpDocument = new iText.Layout.Document(tmpPdfDoc))
                            {
                                foreach (var imageStream in images)
                                {
                                    byte[] imageBytes = imageStream.ToArray();

                                    ImageData imageData = ImageDataFactory.Create(imageBytes);
                                    var image = new iText.Layout.Element.Image(imageData);
                                    image.SetAutoScale(true);
                                    tmpDocument.Add(image);
                                }
                            }
                        }
                    }

                    stream.Seek(0, SeekOrigin.Begin);
                }
                else
                {
                    continue;
                }

                if (stream != null)
                {
                    PdfReader reader = new PdfReader(stream);
                    PdfDocument pdfDoc = new PdfDocument(reader);

                    pdfDoc.CopyPagesTo(1, pdfDoc.GetNumberOfPages(), targetPdfDocument);

                    pdfDoc.Close();
                    reader.Close();

                    stream.Dispose();
                }
            }

            SaveCurrentFile(pdfParams);
        }

        private void InitNextPdfFile(string pdfPfad)
        {
            string path;

            if (counter == 1)
            {
                path = pdfPfad;
            }
            else
            {
                int posPunkt = pdfPfad.LastIndexOf(".");
                path = pdfPfad.Left(posPunkt) + "-" + counter + ".pdf";
            }

            string iccProfilePath = @"Data\sRGB2014.icc";

            writer = new PdfWriter(path);

            fs = new FileStream(iccProfilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            intent = new PdfOutputIntent("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", fs);

            targetPdfDocument = new MyPdfADocument(writer, PdfAConformance.PDF_A_3B, intent)
            {
                IsCheckIsoConformanceEnabled = false
            };

            counter++;
        }

        private void SaveCurrentFile(PdfMergerParams pdfParams)
        {
            bool isZugferd = false;

            foreach (var att in pdfParams.Attachments)
            {
                if (att.Key == "ZUGFeRD Invoice")
                {
                    isZugferd = true;
                }

                // Embed ZUGFeRD XML file
                byte[] bytes = File.ReadAllBytes(att.FilePath);
                PdfFileSpec fileSpec = PdfFileSpec.CreateEmbeddedFileSpec(targetPdfDocument, bytes, att.Description, att.Filename, null, PdfName.ApplicationXml);
                fileSpec.Put(PdfName.AFRelationship, new PdfName("Alternative"));
                targetPdfDocument.AddFileAttachment(att.Key, fileSpec);

                // XML im Dokument referenzieren
                var root = targetPdfDocument.GetCatalog();
                PdfArray afArray = new PdfArray();
                afArray.Add(fileSpec.GetPdfObject().GetIndirectReference());
                root.Put(new PdfName("AF"), afArray);
            }

            if (pdfParams.Metadata != null)
            {
                var docInfo = targetPdfDocument.GetDocumentInfo();

                docInfo.SetMoreInfo(pdfParams.Metadata);
            }

            if (isZugferd)
            {
                // XMP-Metadaten erstellen
                SetXMPMetadata(targetPdfDocument, pdfParams);
            }

            targetPdfDocument.Close();
            targetPdfDocument = null;

            writer.Dispose();
            writer = null;

            intent = null;

            fs?.Dispose();
            fs = null;
        }

        private void SetXMPMetadata(PdfADocument pdfDoc, PdfMergerParams pdfParams)
        {

            // string xml = XMPSerializerHelper.SerializeToString(xmpMetadata, null);
            //var xmp = pdfDoc.GetXmpMetadata() as XMPMetaImpl;

            //if (xmp == null)
            //{
            //    xmp = XMPMetaFactory.Create() as XMPMetaImpl;
            //}

            string datum = DateTime.Now.ToString("o");
            string username = Environment.UserName.EscapeXml();

            string xml = $@"<?xpacket begin=""﻿"" id=""W5M0MpCehiHzreSzNTczkc9d""?>
<x:xmpmeta xmlns:x=""adobe:ns:meta/"" x:xmptk=""Adobe XMP Core 5.1.0-jc003"">
  <rdf:RDF xmlns:rdf=""http://www.w3.org/1999/02/22-rdf-syntax-ns#"">
    <rdf:Description rdf:about=""""
        xmlns:pdf=""http://ns.adobe.com/pdf/1.3/""
        xmlns:xmp=""http://ns.adobe.com/xap/1.0/""
        xmlns:fx=""urn:factur-x:pdfa:CrossIndustryDocument:invoice:1p0#""
        xmlns:dc=""http://purl.org/dc/elements/1.1/""
        xmlns:pdfaExtension=""http://www.aiim.org/pdfa/ns/extension/""
        xmlns:pdfaSchema=""http://www.aiim.org/pdfa/ns/schema#""
        xmlns:pdfaProperty=""http://www.aiim.org/pdfa/ns/property#""
        xmlns:pdfaid=""http://www.aiim.org/pdfa/ns/id/""
      pdf:Producer=""pdf-merger""
      pdf:Keywords=""""
      xmp:ModifyDate=""{datum}""
      xmp:CreateDate=""{datum}""
      xmp:MetadataDate=""{datum}""
      xmp:CreatorTool=""pdf-merger""
      fx:ConformanceLevel=""EXTENDED""
      fx:DocumentFileName=""factur-x.xml""
      fx:DocumentType=""INVOICE""
      fx:Version=""1.2""
      dc:format=""application/pdf""
      pdfaid:conformance=""B""
      pdfaid:part=""3"">
      <dc:creator>
        <rdf:Seq>
          <rdf:li xml:lang=""x-default"">{username}</rdf:li>
        </rdf:Seq>
      </dc:creator>
      <dc:description>
        <rdf:Alt>
          <rdf:li xml:lang=""x-default""/>
        </rdf:Alt>
      </dc:description>
      <dc:title>
        <rdf:Alt>
          <rdf:li xml:lang=""x-default"">Rechnung</rdf:li>
        </rdf:Alt>
      </dc:title>
      <pdfaExtension:schemas>
        <rdf:Bag>
          <rdf:li>
            <rdf:Description
              pdfaSchema:schema=""Factur-X PDFA Extension Schema""
              pdfaSchema:namespaceURI=""urn:factur-x:pdfa:CrossIndustryDocument:invoice:1p0#""
              pdfaSchema:prefix=""fx"">
            <pdfaSchema:property>
              <rdf:Seq>
                <rdf:li
                  pdfaProperty:name=""DocumentFileName""
                  pdfaProperty:valueType=""Text""
                  pdfaProperty:category=""external""
                  pdfaProperty:description=""name of the embedded XML invoice file""/>
                <rdf:li
                  pdfaProperty:name=""DocumentType""
                  pdfaProperty:valueType=""Text""
                  pdfaProperty:category=""external""
                  pdfaProperty:description=""INVOICE""/>
                <rdf:li
                  pdfaProperty:name=""Version""
                  pdfaProperty:valueType=""Text""
                  pdfaProperty:category=""external""
                  pdfaProperty:description=""The actual version of the Factur-X data""/>
                <rdf:li
                  pdfaProperty:name=""ConformanceLevel""
                  pdfaProperty:valueType=""Text""
                  pdfaProperty:category=""external""
                  pdfaProperty:description=""The conformance level of the Factur-X data""/>
              </rdf:Seq>
            </pdfaSchema:property>
            </rdf:Description>
          </rdf:li>
        </rdf:Bag>
      </pdfaExtension:schemas>
    </rdf:Description>
  </rdf:RDF>
</x:xmpmeta>
<?xpacket end=""w""?>";

            var xmp = iText.Kernel.XMP.Impl.XMPMetaParser.Parse(xml, null) as XMPMetaImpl;

            pdfDoc.SetXmpMetadata(xmp);
        }

        private static List<MemoryStream> GetAllImages(string file)
        {
            List<MemoryStream> images = new List<MemoryStream>();

            using (Bitmap bitmap = (Bitmap)System.Drawing.Image.FromFile(file))
            {
                int count = bitmap.GetFrameCount(FrameDimension.Page);

                for (int idx = 0; idx < count; idx++)
                {
                    // save each frame to a bytestream
                    bitmap.SelectActiveFrame(FrameDimension.Page, idx);
                    MemoryStream byteStream = new MemoryStream();
                    bitmap.Save(byteStream, ImageFormat.Tiff);

                    // and then create a new Image from it
                    images.Add(byteStream);
                }
            }

            return images;
        }
    }
}