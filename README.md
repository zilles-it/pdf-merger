# pdf-merger
Command line tool for merging PDF/A and XML files (ZUGFeRD).
This application is provided for testing purposes without any warranty.

# PDF/A profile 
The ICC profile sRGB2014.icc must be downloaded separately and copied to the "Data" folder.

# Usage
pdf-merger.exe -s="c:\temp\invoice.pdf;c:\temp\attachments.pdf" -z="C:\temp\zugferd.xml" -d="C:\temp\out.pdf" -m=20

# Licence: AGPL 3.0
This app uses <a href="https://github.com/itext/itext7-dotnet">iText Core</a>.
