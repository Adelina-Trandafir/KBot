Option Strict On
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain

' Slice 00EF-09 -- the views of the invoice (the horizontal bar, the pattern of the DDF view: one page on screen at a time; every view is a
' page of its own, a control with its own designer, declared in this window's designer) and the documents of three of them:
'   «Factură PDF»   (pgPdf)     the CLASSIC invoice, drawn by K-BOT (FacturaPdf.WriteInvoice) -- an accepted invoice
'   «Factură ANAF»  (pgAnaf)    the invoice as ANAF draws it: the server downloads the accepted file from ANAF and has it turned into a PDF
'                               (GET /facturi/{id}/pdf-anaf) -- an accepted invoice
'   «Eroare ANAF»   (pgEroare)  the reason ANAF gave when it refused the invoice, as a page (FacturaPdf.WriteError) -- a refused invoice
' Each of the three has its OWN embedded viewer (made the first time its view opens, so Adobe does not start with the window).
Partial Public Class FacturiForm

    ' A newer request for a document wins over an older answer.
    Private _pdfSeq As Integer

    Private Shared Function IsPdfView(k_key As String) As Boolean
        Return String.Equals(k_key, ViewPdf, StringComparison.Ordinal) OrElse
               String.Equals(k_key, ViewAnaf, StringComparison.Ordinal) OrElse
               String.Equals(k_key, ViewEroare, StringComparison.Ordinal)
    End Function

    ''' <summary>The document page of a PDF view (Nothing for the other views).</summary>
    Private Function PdfPageOf(k_key As String) As VanzarePdfPage
        Select Case k_key
            Case ViewPdf : Return pgPdf
            Case ViewAnaf : Return pgAnaf
            Case ViewEroare : Return pgEroare
            Case Else : Return Nothing
        End Select
    End Function

    ''' <summary>Chooses a view on the bar (the page follows through <see cref="NavDetaliu_SelectionChanged"/>).</summary>
    Private Sub SelectView(k_key As String)
        navDetaliu.SelectedKey = k_key
    End Sub

    Private Sub NavDetaliu_SelectionChanged(k_key As String) Handles navDetaliu.SelectionChanged
        Try
            ShowPage(k_key)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiForm.NavDetaliu_SelectionChanged", ex)
        End Try
    End Sub

    ''' <summary>The page of a view on screen, the others away; a document view also opens its document.</summary>
    Private Sub ShowPage(k_key As String)
        pgGenerale.Visible = String.Equals(k_key, ViewGenerale, StringComparison.Ordinal)
        pgCumparator.Visible = String.Equals(k_key, ViewCumparator, StringComparison.Ordinal)
        pgAtasamente.Visible = String.Equals(k_key, ViewAtasamente, StringComparison.Ordinal)
        pgContinut.Visible = String.Equals(k_key, ViewContinut, StringComparison.Ordinal)
        pgPdf.Visible = String.Equals(k_key, ViewPdf, StringComparison.Ordinal)
        pgAnaf.Visible = String.Equals(k_key, ViewAnaf, StringComparison.Ordinal)
        pgEroare.Visible = String.Equals(k_key, ViewEroare, StringComparison.Ordinal)
        If IsPdfView(k_key) Then RefreshPdfView()
    End Sub

    ''' <summary>
    ''' Opens the document of the document view on screen for the shown invoice (nothing when another view is on screen, or no saved
    ''' invoice is shown). A failure becomes a line inside that view's viewer. UI boundary (async Sub): logs and swallows.
    ''' </summary>
    Private Async Sub RefreshPdfView()
        Dim k_page As VanzarePdfPage = Nothing
        Try
            Dim k_key As String = navDetaliu.SelectedKey
            k_page = PdfPageOf(k_key)
            If k_page Is Nothing Then Return
            Dim k_f As EFacturaFactura = _current
            If k_f Is Nothing OrElse _mode <> EditMode.Viewing Then Return
            Dim k_stamp As String = $"{k_f.IdFactura}|{k_f.Stare}|{k_f.Total}|{k_f.DataFactura:yyyyMMdd}|{k_f.Linii.Count}"
            If String.Equals(k_stamp, k_page.Stamp, StringComparison.Ordinal) Then Return
            Dim k_seq As Integer = Interlocked.Increment(_pdfSeq)
            Dim k_path As String
            Select Case k_key
                Case ViewPdf
                    k_path = NewPdfPath("factura", k_f)
                    FacturaPdf.WriteInvoice(k_path, k_f, _furnizor)
                Case ViewEroare
                    k_path = NewPdfPath("eroare", k_f)
                    FacturaPdf.WriteError(k_path, k_f)
                Case Else
                    k_page.ShowNotice("Se aduce factura de la ANAF…")
                    Dim k_bytes As Byte() = Await _gate.RunAsync(Function() _api.GetAnafPdfAsync(k_f.IdFactura, _cts.Token)).ConfigureAwait(True)
                    If IsDisposed OrElse k_seq <> _pdfSeq Then Return
                    k_path = NewPdfPath("anaf", k_f)
                    File.WriteAllBytes(k_path, k_bytes)
            End Select
            If IsDisposed OrElse k_seq <> _pdfSeq Then Return
            k_page.ShowDocument(k_path)
            k_page.Stamp = k_stamp
        Catch ex As OperationCanceledException
            ' The window was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("FacturiForm.RefreshPdfView", ex)
            If Not IsDisposed AndAlso k_page IsNot Nothing Then k_page.ShowNotice(ex.Message)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiForm.RefreshPdfView", ex)
            If Not IsDisposed AndAlso k_page IsNot Nothing Then k_page.ShowNotice("Documentul nu a putut fi pregătit. Detalii în jurnalul de erori.")
        End Try
    End Sub

    ''' <summary>A new file in the PDF work area (emptied when K-BOT starts); the time in the name keeps it apart from a file Adobe still holds.</summary>
    Private Shared Function NewPdfPath(k_kind As String, k_f As EFacturaFactura) As String
        TempPdfStore.EnsureRoot()
        Dim k_label As String = Regex.Replace(k_f.Eticheta, "[^A-Za-z0-9_.-]", "_")
        Return TempPdfStore.PathFor($"{k_kind}_{k_label}_{DateTime.Now:HHmmssfff}.pdf")
    End Function

End Class
