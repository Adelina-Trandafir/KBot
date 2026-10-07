#If DEBUG Then
Option Strict On
Imports System.Collections.Generic
Imports System.Drawing
Imports System.IO
Imports System.IO.Compression
Imports System.Text
Imports KBot.Theming

' Slice 0078-15: the two compared trees of the ActiveX log viewer, written side by side into an .xlsx -- left tree in
' columns A-C (root / element / line), a filled separator column D, right tree in E-G. Matching nodes share a row.
' Written by hand (a zip of SpreadsheetML parts, inline strings): the solution has no Excel library and a bench tool is
' no reason to add one.

''' <summary>One row of the export: three cells per side, each with its status (or a header).</summary>
Friend NotInheritable Class CompareExportRow
    ''' <summary>0-2 = left (root, element, line), 3-5 = right.</summary>
    Public ReadOnly Property Cells As String() = New String(5) {}
    Public ReadOnly Property Statuses As CompareStatus() = New CompareStatus(5) {}
    ''' <summary>A header row or a root row: bold.</summary>
    Public Property Bold As Boolean
    ''' <summary>A header row: plain text colour.</summary>
    Public Property Header As Boolean

    Public Sub SetCell(k_index As Integer, k_text As String, k_status As CompareStatus)
        Cells(k_index) = k_text
        Statuses(k_index) = k_status
    End Sub
End Class

Friend NotInheritable Class ActivexCompareExcel

    Private Sub New()
    End Sub

    ' Fonts: (plain, bold) x (text, same, differs, missing, dim); cell style = bold * 5 + colour; 10 = separator.
    Private Const ColourCount As Integer = 5
    Private Const SeparatorStyle As Integer = 10
    ' Sheet columns per cell index (D is the separator).
    Private Shared ReadOnly ColumnOf As String() = {"A", "B", "C", "E", "F", "G"}

    ''' <summary>Writes the workbook. Risky boundary (file I/O): the caller logs.</summary>
    Public Shared Sub Write(k_path As String, k_rows As List(Of CompareExportRow))
        If File.Exists(k_path) Then File.Delete(k_path)
        Using k_zip As ZipArchive = ZipFile.Open(k_path, ZipArchiveMode.Create)
            AddPart(k_zip, "[Content_Types].xml",
                "<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>" &
                "<Types xmlns=""http://schemas.openxmlformats.org/package/2006/content-types"">" &
                "<Default Extension=""rels"" ContentType=""application/vnd.openxmlformats-package.relationships+xml""/>" &
                "<Default Extension=""xml"" ContentType=""application/xml""/>" &
                "<Override PartName=""/xl/workbook.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml""/>" &
                "<Override PartName=""/xl/worksheets/sheet1.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml""/>" &
                "<Override PartName=""/xl/styles.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml""/>" &
                "</Types>")
            AddPart(k_zip, "_rels/.rels",
                "<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>" &
                "<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">" &
                "<Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"" Target=""xl/workbook.xml""/>" &
                "</Relationships>")
            AddPart(k_zip, "xl/workbook.xml",
                "<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>" &
                "<workbook xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"" xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships"">" &
                "<sheets><sheet name=""Comparatie"" sheetId=""1"" r:id=""rId1""/></sheets></workbook>")
            AddPart(k_zip, "xl/_rels/workbook.xml.rels",
                "<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>" &
                "<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">" &
                "<Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet"" Target=""worksheets/sheet1.xml""/>" &
                "<Relationship Id=""rId2"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles"" Target=""styles.xml""/>" &
                "</Relationships>")
            AddPart(k_zip, "xl/styles.xml", StylesXml())
            AddPart(k_zip, "xl/worksheets/sheet1.xml", SheetXml(k_rows))
        End Using
    End Sub

    Private Shared Sub AddPart(k_zip As ZipArchive, k_name As String, k_xml As String)
        Dim k_entry As ZipArchiveEntry = k_zip.CreateEntry(k_name, CompressionLevel.Optimal)
        Using k_stream As Stream = k_entry.Open()
            Using k_writer As New StreamWriter(k_stream, New UTF8Encoding(False))
                k_writer.Write(k_xml)
            End Using
        End Using
    End Sub

    ' The colours are the theme's (same / differs / missing / dim), as on screen.
    Private Shared Function StylesXml() As String
        Dim k_palette As ThemePalette = ThemeManager.Current.Palette
        Dim k_colours As Color() = {k_palette.TextColor, k_palette.SuccessColor, k_palette.WarningColor,
                                    k_palette.ErrorColor, k_palette.TextDimColor}
        Dim k_xml As New StringBuilder()
        k_xml.Append("<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>")
        k_xml.Append("<styleSheet xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"">")
        k_xml.Append($"<fonts count=""{2 * ColourCount}"">")
        For k_bold As Integer = 0 To 1
            For Each k_colour As Color In k_colours
                k_xml.Append("<font>").Append(If(k_bold = 1, "<b/>", "")).Append("<sz val=""10""/>")
                k_xml.Append($"<color rgb=""{Argb(k_colour)}""/><name val=""Segoe UI""/></font>")
            Next
        Next
        k_xml.Append("</fonts>")
        k_xml.Append("<fills count=""3""><fill><patternFill patternType=""none""/></fill><fill><patternFill patternType=""gray125""/></fill>")
        k_xml.Append($"<fill><patternFill patternType=""solid""><fgColor rgb=""{Argb(k_palette.BorderColor)}""/><bgColor indexed=""64""/></patternFill></fill></fills>")
        k_xml.Append("<borders count=""1""><border><left/><right/><top/><bottom/><diagonal/></border></borders>")
        k_xml.Append("<cellStyleXfs count=""1""><xf numFmtId=""0"" fontId=""0"" fillId=""0"" borderId=""0""/></cellStyleXfs>")
        k_xml.Append($"<cellXfs count=""{2 * ColourCount + 1}"">")
        For k_font As Integer = 0 To 2 * ColourCount - 1
            k_xml.Append($"<xf numFmtId=""0"" fontId=""{k_font}"" fillId=""0"" borderId=""0"" xfId=""0"" applyFont=""1""/>")
        Next
        k_xml.Append("<xf numFmtId=""0"" fontId=""0"" fillId=""2"" borderId=""0"" xfId=""0"" applyFill=""1""/>")
        k_xml.Append("</cellXfs>")
        k_xml.Append("<cellStyles count=""1""><cellStyle name=""Normal"" xfId=""0"" builtinId=""0""/></cellStyles>")
        k_xml.Append("</styleSheet>")
        Return k_xml.ToString()
    End Function

    Private Shared Function Argb(k_colour As Color) As String
        Return $"FF{k_colour.R:X2}{k_colour.G:X2}{k_colour.B:X2}"
    End Function

    Private Shared Function ColourIndex(k_status As CompareStatus) As Integer
        Select Case k_status
            Case CompareStatus.Same : Return 1
            Case CompareStatus.Differs : Return 2
            Case CompareStatus.Missing : Return 3
            Case Else : Return 4
        End Select
    End Function

    Private Shared Function SheetXml(k_rows As List(Of CompareExportRow)) As String
        Dim k_xml As New StringBuilder(k_rows.Count * 300)
        k_xml.Append("<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>")
        k_xml.Append("<worksheet xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"" xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships"">")
        ' The two header rows stay in view.
        k_xml.Append("<sheetViews><sheetView workbookViewId=""0""><pane ySplit=""2"" topLeftCell=""A3"" activePane=""bottomLeft"" state=""frozen""/></sheetView></sheetViews>")
        k_xml.Append("<cols>")
        k_xml.Append("<col min=""1"" max=""1"" width=""24"" customWidth=""1""/><col min=""2"" max=""2"" width=""42"" customWidth=""1""/>")
        k_xml.Append("<col min=""3"" max=""3"" width=""110"" customWidth=""1""/>")
        k_xml.Append($"<col min=""4"" max=""4"" width=""2"" style=""{SeparatorStyle}"" customWidth=""1""/>")
        k_xml.Append("<col min=""5"" max=""5"" width=""24"" customWidth=""1""/><col min=""6"" max=""6"" width=""42"" customWidth=""1""/>")
        k_xml.Append("<col min=""7"" max=""7"" width=""110"" customWidth=""1""/>")
        k_xml.Append("</cols><sheetData>")
        For k_r As Integer = 0 To k_rows.Count - 1
            Dim k_row As CompareExportRow = k_rows(k_r)
            Dim k_number As Integer = k_r + 1
            k_xml.Append($"<row r=""{k_number}"">")
            For k_c As Integer = 0 To 5
                ' The separator cell of every row sits between the two sides (column order inside a row matters).
                If k_c = 3 Then k_xml.Append($"<c r=""D{k_number}"" s=""{SeparatorStyle}""/>")
                Dim k_text As String = k_row.Cells(k_c)
                If String.IsNullOrEmpty(k_text) Then Continue For
                Dim k_style As Integer = If(k_row.Bold, ColourCount, 0) + If(k_row.Header, 0, ColourIndex(k_row.Statuses(k_c)))
                k_xml.Append($"<c r=""{ColumnOf(k_c)}{k_number}"" t=""inlineStr"" s=""{k_style}""><is><t xml:space=""preserve"">")
                k_xml.Append(XmlText(k_text)).Append("</t></is></c>")
            Next
            k_xml.Append("</row>")
        Next
        k_xml.Append("</sheetData></worksheet>")
        Return k_xml.ToString()
    End Function

    ' Escapes XML's own characters and drops the control characters XML does not allow; Excel's cell limit is 32767.
    Private Shared Function XmlText(k_value As String) As String
        If k_value.Length > 32000 Then k_value = k_value.Substring(0, 32000) & "…"
        Dim k_out As New StringBuilder(k_value.Length + 16)
        For Each k_char As Char In k_value
            Select Case k_char
                Case "&"c : k_out.Append("&amp;")
                Case "<"c : k_out.Append("&lt;")
                Case ">"c : k_out.Append("&gt;")
                Case Else
                    If AscW(k_char) < 32 AndAlso k_char <> ControlChars.Tab Then Continue For
                    k_out.Append(k_char)
            End Select
        Next
        Return k_out.ToString()
    End Function

End Class
#End If
