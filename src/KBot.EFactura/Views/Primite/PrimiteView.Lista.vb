Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text.RegularExpressions
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

' Slice 00EF-41 -- the list of all the invoices, shown when the ROOT «Toate facturile» of the tree is chosen (the pattern of the DDF and ORD views):
' a grid with a tick per invoice and two actions, «Listeaza» (print) and «Salveaza XML», on the ticked rows; the button of a row offers the same two for that row.
' The page (PrimiteListaPage) only collects the request; the work is here, with the API the view already has.
Partial Public Class PrimiteView

    Private Const RootKey As String = "R"

    ' True while the list takes the place of the bar of views and the pages.
    Private _listShown As Boolean
    Private _rootNode As AdvancedTreeControl.TreeItem

    ' ── Which surface is on screen ──────────────────────────────────────────────

    ' A link of an invoice with a reception was made or removed since the list was read: its column «Recepție asociată» is old (slice 00EF-42).
    Private _listStale As Boolean

    ' The root was chosen: the list replaces the bar and the pages. The supplier-grouped tree has no root, so it never gets here.
    ' When a link changed meanwhile the list is read again (the tree is rebuilt and refills the list).
    Private Async Function ShowListSurfaceAsync() As Task
        If _groupBySupplier Then Return
        ' The viewer lets go of the document before it is hidden (the same as when the window is put away).
        PutAway()
        _listShown = True
        pnlNav.Visible = False
        pnlPages.Visible = False
        pgLista.Visible = True
        FillList()
        If _listStale Then
            _listStale = False
            Await LoadAsync(_idDdf).ConfigureAwait(True)
        End If
    End Function

    ' An invoice was chosen (or there is nothing to list): the bar and the pages are back.
    Private Sub ShowDetailSurface()
        _listShown = False
        pgLista.Visible = False
        pnlNav.Visible = True
        pnlPages.Visible = True
    End Sub

    ' What the tree holds now, newest first (the filters of the header menu apply, as they do to the tree).
    Private Sub FillList()
        pgLista.SetItems(ShownItems().OrderByDescending(Function(k_x) k_x.DataFact).ThenByDescending(Function(k_x) k_x.IdPrimita))
    End Sub

    ' ── The two actions ─────────────────────────────────────────────────────────

    ' UI boundary (async Sub): logs and swallows; a failure is said to the operator.
    Private Async Sub PgLista_ListRequested(k_items As List(Of EFacturaPrimita)) Handles pgLista.ListRequested
        Try
            Await ListInvoicesAsync(k_items).ConfigureAwait(True)
        Catch ex As OperationCanceledException
            ' The host was closed.
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteView.PgLista_ListRequested", ex)
            If Not IsDisposed Then
                pgLista.SetBusy(False)
                KBotMessage.Show(FindForm(), "Listarea nu a putut porni: " & ex.Message, "Listează", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
        End Try
    End Sub

    ' UI boundary (async Sub): logs and swallows; a failure is said to the operator.
    Private Async Sub PgLista_SaveXmlRequested(k_items As List(Of EFacturaPrimita)) Handles pgLista.SaveXmlRequested
        Try
            Await SaveXmlFilesAsync(k_items).ConfigureAwait(True)
        Catch ex As OperationCanceledException
            ' The host was closed.
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteView.PgLista_SaveXmlRequested", ex)
            If Not IsDisposed Then
                pgLista.SetBusy(False)
                KBotMessage.Show(FindForm(), "Salvarea nu a putut porni: " & ex.Message, "Salvează XML", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
        End Try
    End Sub

    Private Shared Function InvoiceName(k_p As EFacturaPrimita) As String
        Return $"{k_p.NrFact} ({k_p.Furnizor})".Trim()
    End Function

    ''' <summary>
    ''' Makes each invoice ready (the classic invoice K-BOT draws from its stored data, the first view of «Factura PDF») and sends it to the printer
    ''' without opening it; the printer is asked once for the whole batch. Risky boundary (HTTP, disk, printer): each invoice that fails is
    ''' logged and named in a message at the end; the others go on.
    ''' </summary>
    Private Async Function ListInvoicesAsync(k_items As List(Of EFacturaPrimita)) As Task
        If k_items Is Nothing OrElse k_items.Count = 0 Then Return
        Dim k_choice As PrinterChoice = PdfPrinter.ChoosePrinter(FindForm())
        If k_choice Is Nothing Then Return
        Dim k_done As Integer = 0
        Dim k_failures As New List(Of String)()
        pgLista.SetBusy(True)
        Try
            For k_i As Integer = 0 To k_items.Count - 1
                Dim k_p As EFacturaPrimita = k_items(k_i)
                Dim k_id As Integer = k_p.IdPrimita
                pgLista.SetStatus($"Se pregătește și se trimite la imprimantă: {k_i + 1} din {k_items.Count} — {InvoiceName(k_p)}")
                Try
                    Dim k_detail As EFacturaPrimitaDetaliu = Await _gate.RunAsync(
                        Function() _api.GetPrimitaAsync(k_id, _cts.Token)).ConfigureAwait(True)
                    If IsDisposed Then Return
                    Dim k_label As String = Regex.Replace(k_p.NrFact, "[^A-Za-z0-9_.-]", "_")
                    TempPdfStore.EnsureRoot()
                    Dim k_path As String = TempPdfStore.PathFor($"primita_{k_label}_{DateTime.Now:HHmmssfff}.pdf")
                    FacturaPrimitaPdf.Write(k_path, k_detail)
                    For k_copy As Integer = 1 To k_choice.Copies
                        Await PdfPrinter.PrintAsync(k_path, k_choice.PrinterName).ConfigureAwait(True)
                    Next
                    k_done += 1
                Catch ex As Exception When Not TypeOf ex Is OperationCanceledException
                    GlobalErrorLog.Write("PrimiteView.ListInvoicesAsync", ex)
                    k_failures.Add($"{InvoiceName(k_p)}: {ex.Message}")
                End Try
            Next
        Finally
            If Not IsDisposed Then pgLista.SetBusy(False)
        End Try
        If IsDisposed Then Return
        pgLista.SetStatus($"{k_done} din {k_items.Count} facturi trimise la imprimantă.")
        If k_failures.Count > 0 Then
            KBotMessage.Show(FindForm(), "Nu toate facturile au putut fi listate:" & vbCrLf & vbCrLf & String.Join(vbCrLf, k_failures),
                             "Listează", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End If
    End Function

    ''' <summary>
    ''' One invoice: the usual «save as» window. Several: one folder is asked, and every XML goes in it under the name the single save would suggest
    ''' (a name already taken gets «(2)», «(3)»…, nothing is overwritten). Risky boundary (HTTP, disk): a failed invoice is logged and named at the end.
    ''' </summary>
    Private Async Function SaveXmlFilesAsync(k_items As List(Of EFacturaPrimita)) As Task
        If k_items Is Nothing OrElse k_items.Count = 0 Then Return
        If k_items.Count = 1 Then
            Dim k_one As EFacturaPrimita = k_items(0)
            Await SaveFileAsync(k_one, "xml", "Fișier XML (*.xml)|*.xml",
                                Function() _api.GetPrimitaXmlAsync(k_one.IdPrimita, _cts.Token)).ConfigureAwait(True)
            Return
        End If
        Dim k_folder As String
        Using k_dialog As New FolderBrowserDialog() With {
            .Description = "Alegeți dosarul în care se salvează XML-urile facturilor bifate.",
            .UseDescriptionForTitle = True, .ShowNewFolderButton = True}
            If Not String.IsNullOrEmpty(_lastXmlFolder) AndAlso Directory.Exists(_lastXmlFolder) Then k_dialog.SelectedPath = _lastXmlFolder
            If k_dialog.ShowDialog(FindForm()) <> DialogResult.OK Then Return
            k_folder = k_dialog.SelectedPath
        End Using
        _lastXmlFolder = k_folder

        Dim k_done As Integer = 0
        Dim k_failures As New List(Of String)()
        pgLista.SetBusy(True)
        Try
            For k_i As Integer = 0 To k_items.Count - 1
                Dim k_p As EFacturaPrimita = k_items(k_i)
                Dim k_id As Integer = k_p.IdPrimita
                pgLista.SetStatus($"Se salvează: {k_i + 1} din {k_items.Count} — {InvoiceName(k_p)}")
                Try
                    Dim k_bytes As Byte() = Await _gate.RunAsync(Function() _api.GetPrimitaXmlAsync(k_id, _cts.Token)).ConfigureAwait(True)
                    If IsDisposed Then Return
                    Dim k_target As String = UniquePath(k_folder, SuggestedName(k_p, "xml"))
                    File.WriteAllBytes(k_target, k_bytes)
                    k_done += 1
                Catch ex As Exception When Not TypeOf ex Is OperationCanceledException
                    GlobalErrorLog.Write("PrimiteView.SaveXmlFilesAsync", ex)
                    k_failures.Add($"{InvoiceName(k_p)}: {ex.Message}")
                End Try
            Next
        Finally
            If Not IsDisposed Then pgLista.SetBusy(False)
        End Try
        If IsDisposed Then Return
        pgLista.SetStatus($"{k_done} din {k_items.Count} XML-uri salvate în {k_folder}.")
        If k_failures.Count > 0 Then
            KBotMessage.Show(FindForm(), "Nu toate XML-urile au putut fi salvate:" & vbCrLf & vbCrLf & String.Join(vbCrLf, k_failures),
                             "Salvează XML", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End If
    End Function

    Private _lastXmlFolder As String = String.Empty

    ' A name that does not overwrite a file already in the folder: «name (2).xml», «name (3).xml»...
    Private Shared Function UniquePath(k_folder As String, k_fileName As String) As String
        Dim k_candidate As String = Path.Combine(k_folder, k_fileName)
        Dim k_n As Integer = 2
        Dim k_stem As String = Path.GetFileNameWithoutExtension(k_fileName)
        Dim k_ext As String = Path.GetExtension(k_fileName)
        While File.Exists(k_candidate)
            k_candidate = Path.Combine(k_folder, $"{k_stem} ({k_n.ToString(CultureInfo.InvariantCulture)}){k_ext}")
            k_n += 1
        End While
        Return k_candidate
    End Function

End Class
