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
''' Slice 00EF-09 -- the two PDFs K-BOT draws itself for an issued invoice (the third one, the invoice as ANAF draws it, is ANAF's):
''' <list type="bullet">
''' <item><see cref="WriteInvoice"/> -- the CLASSIC invoice, the paper-style document with the issuer, the customer, the lines and
''' the total. It is built from what K-BOT stores; it is NOT the file ANAF holds.</item>
''' <item><see cref="WriteError"/> -- the reason ANAF gave for refusing an invoice, as a page. ANAF's answer comes as an XML report;
''' the server reads the reasons out of it when it checks the state and keeps them as text with the invoice, and the page is made
''' from that text.</item>
''' </list>
''' The text is set in Arial from the Windows fonts folder (embedded, so the ș and ț of Romanian are right); when that font is not
''' there the standard Helvetica is used and the two comma letters are written with their cedilla twins.
'''
''' <para>Written to a file, never returned: the viewer opens files. Both are risky boundaries (file I/O, the PDF library): they log
''' and rethrow.</para>
''' </summary>
Friend NotInheritable Class FacturaPdf

    Private Sub New()
    End Sub

    Private Shared ReadOnly _ro As New CultureInfo("ro-RO")

    ' ── The fonts ───────────────────────────────────────────────────────────────

    Private NotInheritable Class Fonts

        Friend ReadOnly Normal As Font
        Friend ReadOnly Bold As Font
        Friend ReadOnly Small As Font
        Friend ReadOnly Title As Font
        Friend ReadOnly Big As Font
        Friend ReadOnly Muted As Font
        Private ReadOnly _fold As Boolean

        Friend Sub New()
            Dim k_regular As BaseFont = Nothing
            Dim k_bold As BaseFont = Nothing
            Try
                Dim k_dir As String = Environment.GetFolderPath(Environment.SpecialFolder.Fonts)
                Dim k_r As String = Path.Combine(k_dir, "arial.ttf")
                Dim k_b As String = Path.Combine(k_dir, "arialbd.ttf")
                If File.Exists(k_r) AndAlso File.Exists(k_b) Then
                    k_regular = BaseFont.CreateFont(k_r, BaseFont.IDENTITY_H, BaseFont.EMBEDDED)
                    k_bold = BaseFont.CreateFont(k_b, BaseFont.IDENTITY_H, BaseFont.EMBEDDED)
                End If
            Catch ex As Exception
                ' The standard font is the fallback; the document is still made.
                GlobalErrorLog.Write("FacturaPdf.Fonts", ex)
                k_regular = Nothing
                k_bold = Nothing
            End Try
            If k_regular Is Nothing OrElse k_bold Is Nothing Then
                k_regular = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED)
                k_bold = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1250, BaseFont.NOT_EMBEDDED)
                _fold = True
            End If
            Normal = New Font(k_regular, 9.0F)
            Bold = New Font(k_bold, 9.0F)
            Small = New Font(k_regular, 7.5F, Font.NORMAL, New BaseColor(90, 90, 90))
            Title = New Font(k_bold, 18.0F)
            Big = New Font(k_bold, 12.0F)
            Muted = New Font(k_regular, 9.0F, Font.NORMAL, New BaseColor(90, 90, 90))
        End Sub

        ''' <summary>The text as the font can write it (ș and ț become their cedilla twins on the standard font).</summary>
        Friend Function T(k_text As String) As String
            If k_text Is Nothing Then Return String.Empty
            If Not _fold Then Return k_text
            Return k_text.Replace(ChrW(&H219), ChrW(&H15F)).Replace(ChrW(&H21B), ChrW(&H163)).
                          Replace(ChrW(&H218), ChrW(&H15E)).Replace(ChrW(&H21A), ChrW(&H162))
        End Function

    End Class

    ' ── The classic invoice ─────────────────────────────────────────────────────

    ''' <summary>Writes the classic invoice of <paramref name="k_factura"/> (the detail: customer and lines) to <paramref name="k_path"/>.</summary>
    ''' <param name="k_furnizor">The unit as the issuer; Nothing = the issuer block says nothing is filled in.</param>
    Friend Shared Sub WriteInvoice(k_path As String, k_factura As EFacturaFactura, k_furnizor As EFacturaFurnizor)
        Try
            ArgumentException.ThrowIfNullOrWhiteSpace(k_path)
            ArgumentNullException.ThrowIfNull(k_factura)
            Dim k_f As New Fonts()
            Using k_stream As New FileStream(k_path, FileMode.Create, FileAccess.Write, FileShare.Read)
                Dim k_doc As New Document(PageSize.A4, 36.0F, 36.0F, 36.0F, 42.0F)
                PdfWriter.GetInstance(k_doc, k_stream)
                k_doc.Open()
                Try
                    k_doc.Add(TitleBlock(k_f, k_factura))
                    k_doc.Add(Spacer(10.0F))
                    k_doc.Add(PartiesBlock(k_f, k_factura, k_furnizor))
                    k_doc.Add(Spacer(12.0F))
                    k_doc.Add(LinesTable(k_f, k_factura))
                    k_doc.Add(Spacer(6.0F))
                    k_doc.Add(TotalBlock(k_f, k_factura))
                    k_doc.Add(Spacer(10.0F))
                    AddNotes(k_doc, k_f, k_factura)
                    k_doc.Add(Spacer(14.0F))
                    k_doc.Add(New Paragraph(k_f.T("Document întocmit de K-BOT, în formatul clasic al facturii. Fișierul păstrat de ANAF este factura electronică; " &
                                                  "o vedeți la «Factură ANAF»."), k_f.Small))
                Finally
                    k_doc.Close()
                End Try
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("FacturaPdf.WriteInvoice", ex)
            Throw
        End Try
    End Sub

    Private Shared Function TitleBlock(k_f As Fonts, k_factura As EFacturaFactura) As PdfPTable
        Dim k_table As New PdfPTable(2) With {.WidthPercentage = 100.0F}
        k_table.SetWidths(New Single() {60.0F, 40.0F})
        Dim k_title As String = "FACTURĂ"
        If k_factura.EsteStorno Then
            k_title = "FACTURĂ DE STORNARE"
        ElseIf k_factura.TipFactura = "384" Then
            k_title = "FACTURĂ CORECTATĂ"
        End If
        Dim k_left As New PdfPCell(New Phrase(k_f.T(k_title), k_f.Title)) With {.Border = Rectangle.NO_BORDER, .VerticalAlignment = Element.ALIGN_MIDDLE}
        Dim k_right As New PdfPCell() With {.Border = Rectangle.NO_BORDER, .HorizontalAlignment = Element.ALIGN_RIGHT}
        k_right.AddElement(New Paragraph(k_f.T("Seria și numărul: " & k_factura.Eticheta), k_f.Big) With {.Alignment = Element.ALIGN_RIGHT})
        k_right.AddElement(New Paragraph(k_f.T("Data: " & k_factura.DataFactura.ToString("dd.MM.yyyy", _ro)), k_f.Normal) With {.Alignment = Element.ALIGN_RIGHT})
        If k_factura.EsteStorno Then
            Dim k_other As String = If(String.IsNullOrWhiteSpace(k_factura.SerieFacturaA), k_factura.NumarFacturaA,
                                       k_factura.SerieFacturaA.Trim() & "_" & k_factura.NumarFacturaA)
            If k_other.Length > 0 Then
                k_right.AddElement(New Paragraph(k_f.T("Stornează factura " & k_other), k_f.Normal) With {.Alignment = Element.ALIGN_RIGHT})
            End If
        End If
        k_table.AddCell(k_left)
        k_table.AddCell(k_right)
        Return k_table
    End Function

    Private Shared Function PartiesBlock(k_f As Fonts, k_factura As EFacturaFactura, k_furnizor As EFacturaFurnizor) As PdfPTable
        Dim k_table As New PdfPTable(2) With {.WidthPercentage = 100.0F}
        k_table.SetWidths(New Single() {50.0F, 50.0F})
        k_table.AddCell(PartyCell(k_f, "FURNIZOR", IssuerLines(k_furnizor)))
        k_table.AddCell(PartyCell(k_f, "CUMPĂRĂTOR", CustomerLines(k_factura.Client)))
        Return k_table
    End Function

    Private Shared Function PartyCell(k_f As Fonts, k_title As String, k_lines As List(Of String)) As PdfPCell
        Dim k_cell As New PdfPCell() With {.Padding = 6.0F, .BorderColor = New BaseColor(160, 160, 160)}
        k_cell.AddElement(New Paragraph(k_f.T(k_title), k_f.Bold) With {.SpacingAfter = 3.0F})
        For Each k_line As String In k_lines
            k_cell.AddElement(New Paragraph(k_f.T(k_line), k_f.Normal))
        Next
        Return k_cell
    End Function

    Private Shared Function IssuerLines(k_furnizor As EFacturaFurnizor) As List(Of String)
        Dim k_lines As New List(Of String)()
        If k_furnizor Is Nothing Then
            k_lines.Add("Datele unității emitente nu sunt completate.")
            Return k_lines
        End If
        k_lines.Add(k_furnizor.Denumire)
        AddIf(k_lines, "CIF: ", k_furnizor.CodFiscal)
        AddIf(k_lines, "Adresa: ", JoinParts(k_furnizor.Adresa, k_furnizor.Orasul, k_furnizor.Judetul))
        AddIf(k_lines, "Telefon: ", k_furnizor.Telefon)
        AddIf(k_lines, "E-mail: ", k_furnizor.Mail)
        Return k_lines
    End Function

    Private Shared Function CustomerLines(k_client As EFacturaClient) As List(Of String)
        Dim k_lines As New List(Of String)()
        If k_client Is Nothing Then
            k_lines.Add("Clientul nu este ales.")
            Return k_lines
        End If
        k_lines.Add(k_client.DenumireClient)
        AddIf(k_lines, If(k_client.Cnp, "CNP: ", "CIF: "), (k_client.IndFiscal & k_client.CodFiscal).Trim())
        AddIf(k_lines, "Adresa: ", JoinParts(k_client.Adresa, k_client.Orasul, k_client.Judetul))
        AddIf(k_lines, "Sector: ", k_client.Sector)
        AddIf(k_lines, "Cont: ", k_client.Cont)
        AddIf(k_lines, "Banca: ", k_client.Banca)
        Return k_lines
    End Function

    Private Shared Sub AddIf(k_lines As List(Of String), k_label As String, k_value As String)
        If Not String.IsNullOrWhiteSpace(k_value) Then k_lines.Add(k_label & k_value.Trim())
    End Sub

    Private Shared Function JoinParts(ParamArray k_parts As String()) As String
        Return String.Join(", ", k_parts.Where(Function(k_p) Not String.IsNullOrWhiteSpace(k_p)).Select(Function(k_p) k_p.Trim()))
    End Function

    Private Shared Function LinesTable(k_f As Fonts, k_factura As EFacturaFactura) As PdfPTable
        Dim k_table As New PdfPTable(6) With {.WidthPercentage = 100.0F, .HeaderRows = 1}
        k_table.SetWidths(New Single() {6.0F, 42.0F, 8.0F, 12.0F, 16.0F, 16.0F})
        Dim k_heads As String() = {"Nr.", "Denumirea produselor / serviciilor", "U.M.", "Cantitate", "Preț unitar", "Valoare"}
        For k_i As Integer = 0 To k_heads.Length - 1
            k_table.AddCell(New PdfPCell(New Phrase(k_f.T(k_heads(k_i)), k_f.Bold)) With {
                .BackgroundColor = New BaseColor(235, 235, 235), .Padding = 4.0F,
                .HorizontalAlignment = If(k_i >= 3, Element.ALIGN_RIGHT, Element.ALIGN_LEFT)})
        Next
        Dim k_nr As Integer = 0
        For Each k_line As EFacturaLinie In k_factura.Linii
            k_nr += 1
            k_table.AddCell(BodyCell(k_f, If(String.IsNullOrWhiteSpace(k_line.NrCrt), k_nr.ToString(_ro), k_line.NrCrt), Element.ALIGN_LEFT))
            k_table.AddCell(BodyCell(k_f, k_line.Continut, Element.ALIGN_LEFT))
            k_table.AddCell(BodyCell(k_f, k_line.Um, Element.ALIGN_LEFT))
            k_table.AddCell(BodyCell(k_f, k_line.Cant.ToString("#,##0.###", _ro), Element.ALIGN_RIGHT))
            k_table.AddCell(BodyCell(k_f, k_line.PU.ToString("#,##0.00##", _ro), Element.ALIGN_RIGHT))
            k_table.AddCell(BodyCell(k_f, k_line.Valoare.ToString("N2", _ro), Element.ALIGN_RIGHT))
        Next
        Return k_table
    End Function

    Private Shared Function BodyCell(k_f As Fonts, k_text As String, k_align As Integer) As PdfPCell
        Return New PdfPCell(New Phrase(k_f.T(k_text), k_f.Normal)) With {.Padding = 4.0F, .HorizontalAlignment = k_align}
    End Function

    Private Shared Function TotalBlock(k_f As Fonts, k_factura As EFacturaFactura) As PdfPTable
        Dim k_total As Decimal = If(k_factura.Linii.Count > 0, k_factura.Linii.Sum(Function(k_l) k_l.Valoare), k_factura.Total)
        Dim k_table As New PdfPTable(2) With {.WidthPercentage = 50.0F, .HorizontalAlignment = Element.ALIGN_RIGHT}
        k_table.SetWidths(New Single() {55.0F, 45.0F})
        k_table.AddCell(New PdfPCell(New Phrase(k_f.T("TOTAL DE PLATĂ (RON)"), k_f.Bold)) With {
            .Padding = 6.0F, .BackgroundColor = New BaseColor(235, 235, 235)})
        k_table.AddCell(New PdfPCell(New Phrase(k_total.ToString("N2", _ro), k_f.Big)) With {
            .Padding = 6.0F, .HorizontalAlignment = Element.ALIGN_RIGHT, .BackgroundColor = New BaseColor(235, 235, 235)})
        Return k_table
    End Function

    Private Shared Sub AddNotes(k_doc As Document, k_f As Fonts, k_factura As EFacturaFactura)
        If Not String.IsNullOrWhiteSpace(k_factura.ContPlata) Then
            k_doc.Add(New Paragraph(k_f.T("Cont pentru plată (IBAN): " & k_factura.ContPlata.Trim()), k_f.Normal))
        End If
        If Not String.IsNullOrWhiteSpace(k_factura.BT_13) Then
            k_doc.Add(New Paragraph(k_f.T("Referința comenzii: " & k_factura.BT_13.Trim()), k_f.Normal))
        End If
        If Not String.IsNullOrWhiteSpace(k_factura.Comentarii) Then
            k_doc.Add(New Paragraph(k_f.T("Observații: " & k_factura.Comentarii.Trim()), k_f.Normal))
        End If
        k_doc.Add(New Paragraph(k_f.T("Valorile sunt în RON, fără TVA."), k_f.Muted))
    End Sub

    ' ── The ANAF error ──────────────────────────────────────────────────────────

    ''' <summary>
    ''' Writes the page of an invoice ANAF refused: the invoice in a few lines and the reason ANAF gave
    ''' (<see cref="EFacturaFactura.EroareAnaf"/>), the list of reasons set out one under the other.
    ''' </summary>
    Friend Shared Sub WriteError(k_path As String, k_factura As EFacturaFactura)
        Try
            ArgumentException.ThrowIfNullOrWhiteSpace(k_path)
            ArgumentNullException.ThrowIfNull(k_factura)
            Dim k_f As New Fonts()
            Using k_stream As New FileStream(k_path, FileMode.Create, FileAccess.Write, FileShare.Read)
                Dim k_doc As New Document(PageSize.A4, 36.0F, 36.0F, 36.0F, 42.0F)
                PdfWriter.GetInstance(k_doc, k_stream)
                k_doc.Open()
                Try
                    k_doc.Add(New Paragraph(k_f.T("ANAF a refuzat factura " & k_factura.Eticheta), k_f.Title) With {.SpacingAfter = 8.0F})
                    k_doc.Add(New Paragraph(k_f.T("Data facturii: " & k_factura.DataFactura.ToString("dd.MM.yyyy", _ro)), k_f.Normal))
                    k_doc.Add(New Paragraph(k_f.T("Client: " & k_factura.ClientDenumire), k_f.Normal))
                    k_doc.Add(New Paragraph(k_f.T("Total: " & k_factura.Total.ToString("N2", _ro) & " RON"), k_f.Normal) With {.SpacingAfter = 14.0F})
                    k_doc.Add(New Paragraph(k_f.T("Motivul refuzului"), k_f.Big) With {.SpacingAfter = 6.0F})
                    Dim k_intro As String = String.Empty
                    Dim k_reasons As List(Of String) = SplitReasons(k_factura.EroareAnaf, k_intro)
                    If k_intro.Length > 0 Then k_doc.Add(New Paragraph(k_f.T(k_intro), k_f.Normal) With {.SpacingAfter = 6.0F})
                    For Each k_reason As String In k_reasons
                        k_doc.Add(New Paragraph(k_f.T("• " & k_reason), k_f.Normal) With {.IndentationLeft = 12.0F, .SpacingAfter = 3.0F})
                    Next
                    If k_intro.Length = 0 AndAlso k_reasons.Count = 0 Then
                        k_doc.Add(New Paragraph(k_f.T("ANAF nu a dat un motiv care să fi fost păstrat odată cu factura."), k_f.Normal))
                    End If
                    k_doc.Add(Spacer(14.0F))
                    k_doc.Add(New Paragraph(k_f.T("Document întocmit de K-BOT din motivul păstrat la verificarea stării facturii. " &
                                                  "O factură refuzată rămâne în evidență și nu se retrimite; se întocmește una nouă."), k_f.Small))
                Finally
                    k_doc.Close()
                End Try
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("FacturaPdf.WriteError", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' «ANAF a refuzat factura (stare «nok»). Motive: a; b; c. Identificator descărcare: 12.» → the sentence before the reasons
    ''' (<paramref name="k_intro"/>) and the reasons one by one. A text without that shape is the introduction as it is.
    ''' </summary>
    Private Shared Function SplitReasons(k_text As String, ByRef k_intro As String) As List(Of String)
        Dim k_reasons As New List(Of String)()
        k_intro = String.Empty
        If String.IsNullOrWhiteSpace(k_text) Then Return k_reasons
        Const k_marker As String = "Motive:"
        Dim k_at As Integer = k_text.IndexOf(k_marker, StringComparison.Ordinal)
        If k_at < 0 Then
            k_intro = k_text.Trim()
            Return k_reasons
        End If
        k_intro = k_text.Substring(0, k_at).Trim()
        Dim k_rest As String = k_text.Substring(k_at + k_marker.Length)
        Dim k_idAt As Integer = k_rest.IndexOf("Identificator", StringComparison.Ordinal)
        If k_idAt >= 0 Then
            Dim k_tail As String = k_rest.Substring(k_idAt).Trim()
            k_rest = k_rest.Substring(0, k_idAt)
            If k_tail.Length > 0 Then k_reasons.Add(k_tail)
        End If
        Dim k_list As New List(Of String)()
        For Each k_piece As String In k_rest.Split(";"c)
            Dim k_clean As String = k_piece.Trim().TrimEnd("."c).Trim()
            If k_clean.Length > 0 Then k_list.Add(k_clean)
        Next
        k_list.AddRange(k_reasons)
        Return k_list
    End Function

    Private Shared Function Spacer(k_height As Single) As Paragraph
        Return New Paragraph(" ") With {.SpacingAfter = k_height, .Leading = 1.0F}
    End Function

End Class
