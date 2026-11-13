Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.IO.Compression
Imports System.Security
Imports System.Text
Imports KBot.Common
Imports KBot.Domain

''' <summary>
''' «Exporta in Excel» of «Modificare operatii» (slice AV02-05; the Access Export_ModificareNC1280): the operations given, in the order given,
''' with a total row and an autofilter. The Access export wrote the note number over the title row and ignored the filters on screen; here the
''' caller passes exactly the rows to write. The .xlsx is written by hand (zip of a few XML parts), as <see cref="PlanContExcel"/> does.
''' </summary>
Public NotInheritable Class ModificareOperatiiExcel

    Private Sub New()
    End Sub

    Private Shared ReadOnly Headers As String() = {
        "NC", "Document", "Data", "Clasificație", "Fel", "Jurnal", "Explicație", "Cont debit", "Cont credit", "Sumă", "CH", "CF", "Ordonanțare", "Recepție"}
    Private Shared ReadOnly Widths As Double() = {8, 16, 12, 18, 16, 8, 50, 22, 22, 16, 5, 5, 22, 28}
    Private Const AmountColumn As Integer = 9

    ''' <summary>The workbook, as the bytes of an .xlsx file.</summary>
    Public Shared Function Build(k_ops As IReadOnlyList(Of OperatieContabila)) As Byte()
        Try
            ArgumentNullException.ThrowIfNull(k_ops)
            Using k_stream As New MemoryStream()
                Using k_zip As New ZipArchive(k_stream, ZipArchiveMode.Create, True)
                    AddPart(k_zip, "[Content_Types].xml", ContentTypes)
                    AddPart(k_zip, "_rels/.rels", RootRels)
                    AddPart(k_zip, "xl/workbook.xml", Workbook)
                    AddPart(k_zip, "xl/_rels/workbook.xml.rels", WorkbookRels)
                    AddPart(k_zip, "xl/styles.xml", Styles)
                    AddPart(k_zip, "xl/worksheets/sheet1.xml", Sheet(k_ops))
                End Using
                Return k_stream.ToArray()
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("ModificareOperatiiExcel.Build", ex)
            Throw
        End Try
    End Function

    Private Shared Sub AddPart(k_zip As ZipArchive, k_name As String, k_xml As String)
        Dim k_entry As ZipArchiveEntry = k_zip.CreateEntry(k_name, CompressionLevel.Optimal)
        Using k_writer As New StreamWriter(k_entry.Open(), New UTF8Encoding(False))
            k_writer.Write(k_xml)
        End Using
    End Sub

    Private Shared Function Sheet(k_ops As IReadOnlyList(Of OperatieContabila)) As String
        Dim k_b As New StringBuilder()
        k_b.Append("<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>")
        k_b.Append("<worksheet xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"">")
        k_b.Append("<sheetViews><sheetView workbookViewId=""0""><pane ySplit=""1"" topLeftCell=""A2"" activePane=""bottomLeft"" state=""frozen""/></sheetView></sheetViews>")
        k_b.Append("<cols>")
        For k_i As Integer = 0 To Widths.Length - 1
            k_b.Append($"<col min=""{k_i + 1}"" max=""{k_i + 1}"" width=""{Widths(k_i).ToString(CultureInfo.InvariantCulture)}"" customWidth=""1""/>")
        Next
        k_b.Append("</cols><sheetData>")

        k_b.Append("<row r=""1"">")
        For k_i As Integer = 0 To Headers.Length - 1
            k_b.Append(TextCell(k_i, 1, Headers(k_i), 2))
        Next
        k_b.Append("</row>")

        Dim k_total As Decimal
        Dim k_r As Integer = 2
        For Each k_op As OperatieContabila In k_ops
            k_b.Append($"<row r=""{k_r}"">")
            k_b.Append(NumberCell(0, k_r, k_op.NumarNC, 0))
            k_b.Append(TextCell(1, k_r, k_op.NumarDocument, 0))
            If k_op.DataDocument.HasValue Then k_b.Append(DateCell(2, k_r, k_op.DataDocument.Value))
            k_b.Append(TextCell(3, k_r, k_op.Clsf, 0))
            k_b.Append(TextCell(4, k_r, k_op.FelDocument, 0))
            k_b.Append(TextCell(5, k_r, k_op.Jurnal, 0))
            k_b.Append(TextCell(6, k_r, k_op.Explicatie, 0))
            k_b.Append(TextCell(7, k_r, k_op.Debit, 0))
            k_b.Append(TextCell(8, k_r, k_op.Credit, 0))
            k_b.Append(NumberCell(AmountColumn, k_r, k_op.Suma, 1))
            k_b.Append(TextCell(10, k_r, If(k_op.Chelt, "X", String.Empty), 0))
            k_b.Append(TextCell(11, k_r, If(k_op.Cfpp, "X", String.Empty), 0))
            k_b.Append(TextCell(12, k_r, ModificareOperatiiForm.OrdText(k_op.Ord), 0))
            k_b.Append(TextCell(13, k_r, ModificareOperatiiForm.ReceptieText(k_op.Receptie), 0))
            k_b.Append("</row>")
            k_total += k_op.Suma
            k_r += 1
        Next

        If k_ops.Count > 0 Then
            Dim k_col As String = ColumnName(AmountColumn)
            k_b.Append($"<row r=""{k_r}"">")
            k_b.Append(TextCell(0, k_r, "TOTAL", 2))
            k_b.Append($"<c r=""{k_col}{k_r}"" s=""3""><f>SUBTOTAL(9,{k_col}2:{k_col}{k_r - 1})</f><v>{k_total.ToString("0.00", CultureInfo.InvariantCulture)}</v></c>")
            k_b.Append("</row>")
        End If

        k_b.Append("</sheetData>")
        k_b.Append($"<autoFilter ref=""A1:{ColumnName(Headers.Length - 1)}{Math.Max(1, k_r - 1)}""/>")
        k_b.Append("</worksheet>")
        Return k_b.ToString()
    End Function

    Private Shared Function ColumnName(k_index As Integer) As String
        Return Chr(Asc("A"c) + k_index).ToString()
    End Function

    Private Shared Function TextCell(k_col As Integer, k_row As Integer, k_text As String, k_style As Integer) As String
        If String.IsNullOrEmpty(k_text) Then Return String.Empty
        Return $"<c r=""{ColumnName(k_col)}{k_row}"" s=""{k_style}"" t=""inlineStr""><is><t xml:space=""preserve"">{SecurityElement.Escape(k_text)}</t></is></c>"
    End Function

    Private Shared Function NumberCell(k_col As Integer, k_row As Integer, k_value As Decimal, k_style As Integer) As String
        Return $"<c r=""{ColumnName(k_col)}{k_row}"" s=""{k_style}""><v>{k_value.ToString("0.##", CultureInfo.InvariantCulture)}</v></c>"
    End Function

    ' A date is stored as the serial number of Excel (days since 30.12.1899) with the date style.
    Private Shared Function DateCell(k_col As Integer, k_row As Integer, k_value As Date) As String
        Dim k_serial As Integer = CInt((k_value.Date - New Date(1899, 12, 30)).TotalDays)
        Return $"<c r=""{ColumnName(k_col)}{k_row}"" s=""4""><v>{k_serial.ToString(CultureInfo.InvariantCulture)}</v></c>"
    End Function

    Private Const ContentTypes As String =
        "<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>" &
        "<Types xmlns=""http://schemas.openxmlformats.org/package/2006/content-types"">" &
        "<Default Extension=""rels"" ContentType=""application/vnd.openxmlformats-package.relationships+xml""/>" &
        "<Default Extension=""xml"" ContentType=""application/xml""/>" &
        "<Override PartName=""/xl/workbook.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml""/>" &
        "<Override PartName=""/xl/worksheets/sheet1.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml""/>" &
        "<Override PartName=""/xl/styles.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml""/>" &
        "</Types>"

    Private Const RootRels As String =
        "<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>" &
        "<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">" &
        "<Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"" Target=""xl/workbook.xml""/>" &
        "</Relationships>"

    Private Const Workbook As String =
        "<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>" &
        "<workbook xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"" xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships"">" &
        "<sheets><sheet name=""Operatii contabile"" sheetId=""1"" r:id=""rId1""/></sheets></workbook>"

    Private Const WorkbookRels As String =
        "<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>" &
        "<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">" &
        "<Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet"" Target=""worksheets/sheet1.xml""/>" &
        "<Relationship Id=""rId2"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles"" Target=""styles.xml""/>" &
        "</Relationships>"

    ' Styles: 0 plain, 1 amount (numFmt 4 = #,##0.00), 2 bold (headers, «TOTAL»), 3 bold amount, 4 date (numFmt 164 = dd.mm.yyyy).
    Private Const Styles As String =
        "<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>" &
        "<styleSheet xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"">" &
        "<numFmts count=""1""><numFmt numFmtId=""164"" formatCode=""dd\.mm\.yyyy""/></numFmts>" &
        "<fonts count=""2""><font><sz val=""11""/><name val=""Calibri""/></font><font><b/><sz val=""11""/><name val=""Calibri""/></font></fonts>" &
        "<fills count=""2""><fill><patternFill patternType=""none""/></fill><fill><patternFill patternType=""gray125""/></fill></fills>" &
        "<borders count=""1""><border><left/><right/><top/><bottom/><diagonal/></border></borders>" &
        "<cellStyleXfs count=""1""><xf numFmtId=""0"" fontId=""0"" fillId=""0"" borderId=""0""/></cellStyleXfs>" &
        "<cellXfs count=""5"">" &
        "<xf numFmtId=""0"" fontId=""0"" fillId=""0"" borderId=""0"" xfId=""0""/>" &
        "<xf numFmtId=""4"" fontId=""0"" fillId=""0"" borderId=""0"" xfId=""0"" applyNumberFormat=""1""/>" &
        "<xf numFmtId=""0"" fontId=""1"" fillId=""0"" borderId=""0"" xfId=""0"" applyFont=""1""/>" &
        "<xf numFmtId=""4"" fontId=""1"" fillId=""0"" borderId=""0"" xfId=""0"" applyNumberFormat=""1"" applyFont=""1""/>" &
        "<xf numFmtId=""164"" fontId=""0"" fillId=""0"" borderId=""0"" xfId=""0"" applyNumberFormat=""1""/>" &
        "</cellXfs>" &
        "<cellStyles count=""1""><cellStyle name=""Normal"" xfId=""0"" builtinId=""0""/></cellStyles>" &
        "</styleSheet>"

End Class
