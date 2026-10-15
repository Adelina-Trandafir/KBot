Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports iTextSharp.text
Imports iTextSharp.text.pdf
Imports KBot.Common
Imports KBot.Domain

''' <summary>
''' The CLASSIC invoice of a received e-invoice, drawn by K-BOT from what the server keeps and reads out of the XML. The layout is the
''' one the operator asked for (the paper invoice of the Access system), black and white, no colour: on top three boxes — the supplier,
''' «Numărul / FACTURĂ / date» in the middle, the buyer — then the table of the lines, the total box with the VAT, the note that the
''' invoice was taken from E-Factura with the ANAF request id, and the notes of the invoice and the payment accounts in small type.
''' It is not the file the supplier sent and not the one ANAF draws; it only says what the XML says. Written to a file because the
''' viewer opens files. A risky boundary (file I/O, the PDF library): logs and rethrows.
''' </summary>
Friend NotInheritable Class FacturaPrimitaPdf

    Private Sub New()
    End Sub

    Private Shared ReadOnly _ro As New CultureInfo("ro-RO")
    Private Shared ReadOnly _grey As New BaseColor(235, 235, 235)

    ''' <summary>The fonts of this page besides the Arial set: the typewriter face of the top boxes and the serif of the E-Factura note.</summary>
    Private NotInheritable Class Faces

        Friend ReadOnly Mono As Font
        Friend ReadOnly MonoBold As Font
        Friend ReadOnly MonoTitle As Font
        Friend ReadOnly MonoSmall As Font
        Friend ReadOnly SerifBold As Font

        Friend Sub New()
            Dim k_dir As String = Environment.GetFolderPath(Environment.SpecialFolder.Fonts)
            Dim k_regular As BaseFont = LoadFont(Path.Combine(k_dir, "cour.ttf"), BaseFont.COURIER)
            Dim k_bold As BaseFont = LoadFont(Path.Combine(k_dir, "courbd.ttf"), BaseFont.COURIER_BOLD)
            Mono = New Font(k_regular, 9.0F)
            MonoBold = New Font(k_bold, 9.0F)
            MonoTitle = New Font(k_bold, 24.0F)
            MonoSmall = New Font(k_regular, 8.0F, Font.NORMAL, New BaseColor(70, 70, 70))
            SerifBold = New Font(BaseFont.CreateFont(BaseFont.TIMES_BOLD, BaseFont.CP1250, BaseFont.NOT_EMBEDDED), 8.5F)
        End Sub

        ' The Windows file when it is there (embedded: the Romanian letters are right), else the standard face.
        Private Shared Function LoadFont(k_file As String, k_standard As String) As BaseFont
            Try
                If File.Exists(k_file) Then Return BaseFont.CreateFont(k_file, BaseFont.IDENTITY_H, BaseFont.EMBEDDED)
            Catch ex As Exception
                GlobalErrorLog.Write("FacturaPrimitaPdf.Faces.LoadFont", ex)
            End Try
            Return BaseFont.CreateFont(k_standard, BaseFont.CP1250, BaseFont.NOT_EMBEDDED)
        End Function

    End Class

    ''' <summary>Writes the classic invoice of <paramref name="k_detail"/> to <paramref name="k_path"/>.</summary>
    Friend Shared Sub Write(k_path As String, k_detail As EFacturaPrimitaDetaliu)
        Try
            ArgumentException.ThrowIfNullOrWhiteSpace(k_path)
            ArgumentNullException.ThrowIfNull(k_detail)
            Dim k_f As New FacturaPdf.Fonts()
            Dim k_m As New Faces()
            Using k_stream As New FileStream(k_path, FileMode.Create, FileAccess.Write, FileShare.Read)
                Dim k_doc As New Document(PageSize.A4, 36.0F, 36.0F, 36.0F, 42.0F)
                PdfWriter.GetInstance(k_doc, k_stream)
                k_doc.Open()
                Try
                    k_doc.Add(TopBoxes(k_f, k_m, k_detail))
                    k_doc.Add(Spacer(14.0F))
                    k_doc.Add(LinesTable(k_f, k_detail))
                    k_doc.Add(BottomBlock(k_f, k_m, k_detail))
                    k_doc.Add(Spacer(8.0F))
                    For Each k_note As String In k_detail.Note
                        k_doc.Add(New Paragraph(k_f.T(k_note), k_m.MonoSmall) With {.Leading = 11.0F})
                    Next
                    For Each k_pay As EFacturaPrimitaPlata In k_detail.Plati
                        k_doc.Add(New Paragraph("PaymentMeansCode " & k_pay.Cod, k_m.MonoSmall) With {.Leading = 11.0F})
                        k_doc.Add(New Paragraph("PayeeFinancialAccount " & k_pay.Cont, k_m.MonoSmall) With {.Leading = 11.0F})
                    Next
                Finally
                    k_doc.Close()
                End Try
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("FacturaPrimitaPdf.Write", ex)
            Throw
        End Try
    End Sub

    ' ── The three boxes on top ──────────────────────────────────────────────────

    Private Shared Function TopBoxes(k_f As FacturaPdf.Fonts, k_m As Faces, k_detail As EFacturaPrimitaDetaliu) As PdfPTable
        Dim k_table As New PdfPTable(3) With {.WidthPercentage = 100.0F}
        k_table.SetWidths(New Single() {33.0F, 34.0F, 33.0F})
        k_table.AddCell(SupplierCell(k_f, k_detail))
        k_table.AddCell(CenterCell(k_f, k_m, k_detail.Factura))
        k_table.AddCell(BuyerCell(k_f, k_m, k_detail.Cumparator))
        Return k_table
    End Function

    Private Shared Function SupplierCell(k_f As FacturaPdf.Fonts, k_detail As EFacturaPrimitaDetaliu) As PdfPCell
        Dim k_invoice As EFacturaPrimita = k_detail.Factura
        Dim k_cell As New PdfPCell() With {.Padding = 8.0F, .BorderWidth = 0.8F}
        k_cell.AddElement(New Paragraph(k_f.T(k_invoice.Furnizor), k_f.Big) With {.Alignment = Element.ALIGN_CENTER})
        If Not String.IsNullOrWhiteSpace(k_invoice.Cui) Then
            k_cell.AddElement(Mixed(k_f, "Cod Fiscal: ", k_invoice.Cui.Trim(), Element.ALIGN_CENTER))
        End If
        If Not String.IsNullOrWhiteSpace(k_invoice.Adresa) Then
            k_cell.AddElement(New Paragraph(k_f.T(k_invoice.Adresa.Trim()), k_f.Normal) With {.Alignment = Element.ALIGN_CENTER})
        End If
        Dim k_account As EFacturaPrimitaPlata = k_detail.Plati.FirstOrDefault()
        If k_account IsNot Nothing Then k_cell.AddElement(Mixed(k_f, "Cont: ", k_account.Cont, Element.ALIGN_CENTER))
        Return k_cell
    End Function

    ' A label in the normal face followed by its value in bold, on one line.
    Private Shared Function Mixed(k_f As FacturaPdf.Fonts, k_label As String, k_value As String, k_align As Integer) As Paragraph
        Dim k_p As New Paragraph() With {.Alignment = k_align}
        k_p.Add(New Chunk(k_f.T(k_label), k_f.Normal))
        k_p.Add(New Chunk(k_f.T(k_value), k_f.Bold))
        Return k_p
    End Function

    Private Shared Function CenterCell(k_f As FacturaPdf.Fonts, k_m As Faces, k_invoice As EFacturaPrimita) As PdfPCell
        Dim k_cell As New PdfPCell() With {.Border = Rectangle.NO_BORDER, .Padding = 6.0F}
        Dim k_number As New Paragraph() With {.Alignment = Element.ALIGN_CENTER, .SpacingAfter = 10.0F}
        k_number.Add(New Chunk(k_f.T("Numărul  "), k_m.MonoBold))
        k_number.Add(New Chunk(k_invoice.NrFact, k_m.Mono))
        k_cell.AddElement(k_number)
        Dim k_title As String = If(String.Equals(k_invoice.Tip, "NC", StringComparison.OrdinalIgnoreCase), "NOTĂ DE CREDITARE", "FACTURĂ")
        k_cell.AddElement(New Paragraph(k_f.T(k_title), k_m.MonoTitle) With {.Alignment = Element.ALIGN_CENTER, .SpacingAfter = 4.0F})
        If k_invoice.DataFact.HasValue Then
            k_cell.AddElement(New Paragraph(k_invoice.DataFact.Value.ToString("dd.MM.yyyy", _ro), k_m.Mono) With {.Alignment = Element.ALIGN_CENTER})
        End If
        Return k_cell
    End Function

    Private Shared Function BuyerCell(k_f As FacturaPdf.Fonts, k_m As Faces, k_buyer As EFacturaPrimitaParte) As PdfPCell
        Dim k_cell As New PdfPCell() With {.Padding = 8.0F, .BorderWidth = 0.8F}
        Dim k_name As New Paragraph() With {.SpacingAfter = 6.0F}
        k_name.Add(New Chunk(k_f.T("Cumpărător   "), k_m.Mono))
        Dim k_bold As New Font(k_m.MonoBold)
        k_bold.SetStyle(Font.UNDERLINE)
        k_name.Add(New Chunk(k_f.T(k_buyer.Nume), k_bold))
        k_cell.AddElement(k_name)
        If Not String.IsNullOrWhiteSpace(k_buyer.Cui) Then k_cell.AddElement(MonoPair(k_f, k_m, "Cod Fiscal ", k_buyer.Cui.Trim()))
        If Not String.IsNullOrWhiteSpace(k_buyer.Adresa) Then k_cell.AddElement(MonoPair(k_f, k_m, "Adresa:    ", k_buyer.Adresa.Trim()))
        Return k_cell
    End Function

    Private Shared Function MonoPair(k_f As FacturaPdf.Fonts, k_m As Faces, k_label As String, k_value As String) As Paragraph
        Dim k_p As New Paragraph() With {.SpacingAfter = 3.0F}
        k_p.Add(New Chunk(k_f.T(k_label), k_m.Mono))
        k_p.Add(New Chunk(k_f.T(k_value), k_m.MonoBold))
        Return k_p
    End Function

    ' ── The lines ───────────────────────────────────────────────────────────────

    Private Shared Function LinesTable(k_f As FacturaPdf.Fonts, k_detail As EFacturaPrimitaDetaliu) As PdfPTable
        Dim k_table As New PdfPTable(6) With {.WidthPercentage = 100.0F, .HeaderRows = 1}
        k_table.SetWidths(New Single() {7.0F, 43.0F, 9.0F, 13.0F, 14.0F, 14.0F})
        Dim k_heads As String() = {"Nr." & vbLf & "Crt.", "Denumirea produselor" & vbLf & "sau a serviciilor", "U.M.", "Cantitatea",
                                   "Preț unitar" & vbLf & "- lei -", "Valoarea" & vbLf & "- lei -"}
        For Each k_head As String In k_heads
            k_table.AddCell(New PdfPCell(New Phrase(k_f.T(k_head), k_f.Bold)) With {
                .BackgroundColor = _grey, .Padding = 5.0F, .HorizontalAlignment = Element.ALIGN_CENTER, .VerticalAlignment = Element.ALIGN_MIDDLE})
        Next
        For Each k_line As EFacturaPrimitaLinie In k_detail.Linii
            Dim k_name As String = If(String.IsNullOrWhiteSpace(k_line.Explicatie), k_line.Denumire, k_line.Denumire & vbLf & k_line.Explicatie)
            k_table.AddCell(Body(k_f, k_line.NrLinie, Element.ALIGN_CENTER))
            k_table.AddCell(Body(k_f, k_name, Element.ALIGN_LEFT))
            k_table.AddCell(Body(k_f, k_line.Unit, Element.ALIGN_CENTER))
            k_table.AddCell(Body(k_f, k_line.Cant.ToString("N2", _ro), Element.ALIGN_RIGHT))
            k_table.AddCell(Body(k_f, k_line.Pret.ToString("N2", _ro), Element.ALIGN_RIGHT))
            k_table.AddCell(Body(k_f, k_line.Valoare.ToString("N2", _ro), Element.ALIGN_RIGHT))
        Next
        Return k_table
    End Function

    Private Shared Function Body(k_f As FacturaPdf.Fonts, k_text As String, k_align As Integer) As PdfPCell
        Return New PdfPCell(New Phrase(k_f.T(k_text), k_f.Normal)) With {
            .Padding = 5.0F, .HorizontalAlignment = k_align, .VerticalAlignment = Element.ALIGN_MIDDLE}
    End Function

    ' ── Under the table: the E-Factura note on the left, the total box on the right ─────────────────────

    Private Shared Function BottomBlock(k_f As FacturaPdf.Fonts, k_m As Faces, k_detail As EFacturaPrimitaDetaliu) As PdfPTable
        Dim k_table As New PdfPTable(2) With {.WidthPercentage = 100.0F}
        k_table.SetWidths(New Single() {60.0F, 40.0F})
        Dim k_left As New PdfPCell() With {.Border = Rectangle.NO_BORDER, .PaddingTop = 3.0F}
        k_left.AddElement(New Paragraph(k_f.T("Factură preluată din sistemul E-Factura ANAF. Datele sunt introduse de vânzător!"), k_m.SerifBold) With {.SpacingAfter = 4.0F})
        k_left.AddElement(New Paragraph(k_f.T("ID SOLICITARE ANAF: " & k_detail.Factura.IdSol), k_m.SerifBold))
        k_table.AddCell(k_left)

        Dim k_total As New PdfPTable(2) With {.WidthPercentage = 100.0F}
        k_total.SetWidths(New Single() {55.0F, 45.0F})
        k_total.AddCell(New PdfPCell(New Phrase(k_f.T("TOTAL DE PLATĂ"), k_f.Bold)) With {.Padding = 5.0F, .BackgroundColor = _grey})
        k_total.AddCell(New PdfPCell(New Phrase(k_detail.Factura.Total.ToString("N2", _ro), k_f.Big)) With {
            .Padding = 5.0F, .HorizontalAlignment = Element.ALIGN_CENTER, .BackgroundColor = _grey})
        k_total.AddCell(New PdfPCell(New Phrase(k_f.T("din care TVA:"), k_f.Normal)) With {.Padding = 5.0F, .HorizontalAlignment = Element.ALIGN_CENTER})
        k_total.AddCell(New PdfPCell(New Phrase(k_detail.Factura.Tva.ToString("N2", _ro), k_f.Normal)) With {
            .Padding = 5.0F, .HorizontalAlignment = Element.ALIGN_CENTER})
        Dim k_right As New PdfPCell(k_total) With {.Border = Rectangle.NO_BORDER, .Padding = 0.0F}
        k_table.AddCell(k_right)
        Return k_table
    End Function

    Private Shared Function Spacer(k_height As Single) As Paragraph
        Return New Paragraph(" ") With {.SpacingAfter = k_height, .Leading = 1.0F}
    End Function

End Class
