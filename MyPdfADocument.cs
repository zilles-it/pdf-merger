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
