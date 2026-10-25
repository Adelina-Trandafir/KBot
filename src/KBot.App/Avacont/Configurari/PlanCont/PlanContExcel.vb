Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.IO.Compression
Imports System.Linq
Imports System.Security
Imports System.Text
Imports KBot.Common
Imports KBot.Domain

''' <summary>
''' «Exporta in Excel» of the «Plan de conturi» window (slice AV01-04): the accounts of the subunit on screen with the opening
''' balances and the cumulated turnovers of the working year, DEBIT before CREDIT, amounts as numbers shown with two decimals.
''' The .xlsx is written by hand (zip of a few XML parts): no library, no server round trip.
''' </summary>
Public NotInheritable Class PlanContExcel

    Private Sub New()
    End Sub

    Private Shared ReadOnly Headers As String() = {
        "Simbol cont", "Cont de bază", "Sector", "Sursă", "Clsf. funcțională", "Clsf. economică", "Analitic", "Denumire",
        "Fel", "Activ", "S.I. Debitor", "S.I. Creditor", "R.P. Debitor", "R.P. Creditor"}
    Private Shared ReadOnly Widths As Double() = {22, 13, 8, 8, 16, 16, 12, 60, 6, 8, 18, 18, 18, 18}
    Private Const FirstAmount As Integer = 10   ' 0-based index of «S.I. Debitor»

    ''' <summary>A file name part without the characters Windows refuses.</summary>
    Public Shared Function SafeName(k_name As String) As String
        Dim k_bad As Char() = Path.GetInvalidFileNameChars()
        Dim k_clean As String = New String(If(k_name, "plan").Select(Function(k_c) If(Array.IndexOf(k_bad, k_c) >= 0, "_"c, k_c)).ToArray())
        Return If(k_clean.Length = 0, "plan", k_clean)
    End Function

    ''' <summary>The workbook, as the bytes of an .xlsx file.</summary>
    Public Shared Function Build(k_ctx As PlanContContext) As Byte()
        Try
            ArgumentNullException.ThrowIfNull(k_ctx)
            Dim k_rows As List(Of ContPlan) = k_ctx.VisibleAccounts().OrderBy(Function(k_c) k_c.Simbol, StringComparer.Ordinal).ToList()
            Using k_stream As New MemoryStream()
                Using k_zip As New ZipArchive(k_stream, ZipArchiveMode.Create, True)
                    AddPart(k_zip, "[Content_Types].xml", ContentTypes)
                    AddPart(k_zip, "_rels/.rels", RootRels)
                    AddPart(k_zip, "xl/workbook.xml", Workbook)
                    AddPart(k_zip, "xl/_rels/workbook.xml.rels", WorkbookRels)
                    AddPart(k_zip, "xl/styles.xml", Styles)
                    AddPart(k_zip, "xl/worksheets/sheet1.xml", Sheet(k_ctx, k_rows))
                End Using
                Return k_stream.ToArray()
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("PlanContExcel.Build", ex)
            Throw
        End Try
    End Function

    Private Shared Sub AddPart(k_zip As ZipArchive, k_name As String, k_xml As String)
        Dim k_entry As ZipArchiveEntry = k_zip.CreateEntry(k_name, CompressionLevel.Optimal)
        Using k_writer As New StreamWriter(k_entry.Open(), New UTF8Encoding(False))
            k_writer.Write(k_xml)
        End Using
    End Sub

    Private Shared Function Sheet(k_ctx As PlanContContext, k_rows As List(Of ContPlan)) As String
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

        Dim k_totals(3) As Decimal
        Dim k_r As Integer = 2
        For Each k_c As ContPlan In k_rows
            Dim k_sold As SoldCont = k_ctx.SoldOf(k_c.IdCont)
            Dim k_amounts() As Decimal = {k_sold.SoldDebitor, k_sold.SoldCreditor, k_sold.RulajDebitor, k_sold.RulajCreditor}
            Dim k_ss As String = If(k_c.SursaSector, String.Empty)
            k_b.Append($"<row r=""{k_r}"">")
            k_b.Append(TextCell(0, k_r, k_c.Simbol, 0))
            k_b.Append(NumberCell(1, k_r, k_c.Cont, 0))
            k_b.Append(TextCell(2, k_r, If(k_ss.Length >= 2, k_ss.Substring(0, 2), String.Empty), 0))
            k_b.Append(TextCell(3, k_r, If(k_ss.Length >= 3, k_ss.Substring(2), String.Empty), 0))
            k_b.Append(TextCell(4, k_r, k_c.ClsfF, 0))
            k_b.Append(TextCell(5, k_r, k_c.ClsfE, 0))
            k_b.Append(TextCell(6, k_r, k_c.Analitic, 0))
            k_b.Append(TextCell(7, k_r, k_c.Denumire, 0))
            k_b.Append(TextCell(8, k_r, k_c.Fel, 0))
            k_b.Append(TextCell(9, k_r, If(k_c.Activ, "Da", "Nu"), 0))
            For k_i As Integer = 0 To 3
                k_b.Append(NumberCell(FirstAmount + k_i, k_r, k_amounts(k_i), 1))
                k_totals(k_i) += k_amounts(k_i)
            Next
            k_b.Append("</row>")
            k_r += 1
        Next

        ' A total row under the amounts (formulas, with the figure already computed so a viewer that does not calculate shows it).
        If k_rows.Count > 0 Then
            k_b.Append($"<row r=""{k_r}"">")
            k_b.Append(TextCell(0, k_r, "Total", 2))
            For k_i As Integer = 0 To 3
                Dim k_col As String = ColumnName(FirstAmount + k_i)
                k_b.Append($"<c r=""{k_col}{k_r}"" s=""3""><f>SUM({k_col}2:{k_col}{k_r - 1})</f><v>{k_totals(k_i).ToString("0.00", CultureInfo.InvariantCulture)}</v></c>")
            Next
            k_b.Append("</row>")
        End If

        k_b.Append("</sheetData>")
        k_b.Append($"<autoFilter ref=""A1:{ColumnName(Headers.Length - 1)}{k_r - 1}""/>")
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
        "<sheets><sheet name=""Plan de conturi"" sheetId=""1"" r:id=""rId1""/></sheets></workbook>"

    Private Const WorkbookRels As String =
        "<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>" &
        "<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">" &
        "<Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet"" Target=""worksheets/sheet1.xml""/>" &
        "<Relationship Id=""rId2"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles"" Target=""styles.xml""/>" &
        "</Relationships>"

    ' Styles: 0 plain, 1 amount (numFmt 4 = #,##0.00), 2 bold (headers, «Total»), 3 bold amount.
    Private Const Styles As String =
        "<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>" &
        "<styleSheet xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"">" &
        "<fonts count=""2""><font><sz val=""11""/><name val=""Calibri""/></font><font><b/><sz val=""11""/><name val=""Calibri""/></font></fonts>" &
        "<fills count=""2""><fill><patternFill patternType=""none""/></fill><fill><patternFill patternType=""gray125""/></fill></fills>" &
        "<borders count=""1""><border><left/><right/><top/><bottom/><diagonal/></border></borders>" &
        "<cellStyleXfs count=""1""><xf numFmtId=""0"" fontId=""0"" fillId=""0"" borderId=""0""/></cellStyleXfs>" &
        "<cellXfs count=""4"">" &
        "<xf numFmtId=""0"" fontId=""0"" fillId=""0"" borderId=""0"" xfId=""0""/>" &
        "<xf numFmtId=""4"" fontId=""0"" fillId=""0"" borderId=""0"" xfId=""0"" applyNumberFormat=""1""/>" &
        "<xf numFmtId=""0"" fontId=""1"" fillId=""0"" borderId=""0"" xfId=""0"" applyFont=""1""/>" &
        "<xf numFmtId=""4"" fontId=""1"" fillId=""0"" borderId=""0"" xfId=""0"" applyNumberFormat=""1"" applyFont=""1""/>" &
        "</cellXfs>" &
        "<cellStyles count=""1""><cellStyle name=""Normal"" xfId=""0"" builtinId=""0""/></cellStyles>" &
        "</styleSheet>"

End Class
