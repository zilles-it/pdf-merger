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