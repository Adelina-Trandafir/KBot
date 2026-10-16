Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

' Slices KE01..KE07 -- the links of the shown invoice with receptions: the view «Receptii», the classifications of the lines and the
' windows that link to an existing reception, make a new one or change one. The rules (amounts with VAT, what is left, who may write)
' are the server's; the robot that writes in FOREXE is the shell's (<see cref="KeRobot"/>). Plan: docs/KE_PLAN.md.
Partial Public Class PrimiteView

    Private Const ViewReceptii As String = "receptii"

    Private Shared ReadOnly _markerPattern As New Regex("\(IDRH:\s*(\d+);\s*IDR:\s*(\d+)\)", RegexOptions.CultureInvariant)

    Private _ke As EFacturaKeStare
    Private _keSeq As Integer
    Private _keBusy As Boolean
    Private _keCod As String = String.Empty

    ' ── Reading ─────────────────────────────────────────────────────────────────

    ' Reads the state of the links of the shown invoice and puts it on screen. A failure becomes a line in the view; nothing is thrown.
    Private Async Function LoadKeAsync(k_id As Integer) As Task
        Try
            Dim k_mySeq As Integer = Interlocked.Increment(_keSeq)
            Dim k_state As EFacturaKeStare = Await _gate.RunAsync(Function() _api.GetKeAsync(k_id, _cts.Token)).ConfigureAwait(True)
            If IsDisposed OrElse k_mySeq <> _keSeq OrElse k_id <> _shownId Then Return
            ApplyKe(k_state)
        Catch ex As OperationCanceledException
            ' The host was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("PrimiteView.LoadKeAsync", ex)
            If Not IsDisposed Then ShowKeProblem(ex.Message)
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteView.LoadKeAsync", ex)
            If Not IsDisposed Then ShowKeProblem("Legăturile cu recepțiile nu au putut fi citite. Detalii în jurnalul de erori.")
        End Try
    End Function

    Private Sub ShowKeProblem(k_text As String)
        _ke = Nothing
        navDetaliu.SetItemVisible(ViewReceptii, True)
        pgReceptii.ShowProblem(k_text)
    End Sub

    Private Sub ApplyKe(k_state As EFacturaKeStare)
        _ke = k_state
        ' An invoice that is already linked goes on with the angajament of its first link.
        If String.IsNullOrEmpty(_keCod) AndAlso k_state.Legaturi.Count > 0 Then _keCod = k_state.Legaturi(0).CodAngajament
        navDetaliu.SetItemVisible(ViewReceptii, True)
        pgReceptii.ShowState(k_state, KeRobot.Current IsNot Nothing)
        FillKeLines(k_state)
    End Sub

    ' The lines of the first view get their value with VAT and their classification (the server answers them in the same order).
    Private Sub FillKeLines(k_state As EFacturaKeStare)
        Dim k_rows As IReadOnlyList(Of KBotDataRow) = gridLinii().Rows
        If k_rows.Count <> k_state.Linii.Count Then Return
        For k_i As Integer = 0 To k_rows.Count - 1
            k_rows(k_i)("cuTva") = CDbl(k_state.Linii(k_i).ValoareCuTva)
            k_rows(k_i)("clsf") = k_state.Linii(k_i).Clsf
        Next
        gridLinii().Invalidate()
    End Sub

    Private Sub ClearKe()
        Interlocked.Increment(_keSeq)
        _ke = Nothing
        ' The angajament chosen last belongs to the invoice it was chosen on: another invoice starts without one.
        _keCod = String.Empty
        pgReceptii.Clear()
        navDetaliu.SetItemVisible(ViewReceptii, False)
    End Sub

    Private Function InvoiceLabel() As String
        Dim k_f As EFacturaPrimita = _detail?.Factura
        If k_f Is Nothing Then Return String.Empty
        Return $"{k_f.NrFact} ({k_f.Furnizor})".Trim()
    End Function

    Private Sub Notify(k_text As String, k_icon As MessageBoxIcon)
        KBotMessage.Show(FindForm(), k_text, "Recepții", MessageBoxButtons.OK, k_icon)
    End Sub

    ' ── The buttons of the view ─────────────────────────────────────────────────

    ' UI boundary (event handler): logs and swallows; the window the accountant chose the classifications in.
    Private Sub PgReceptii_ClassificationsRequested() Handles pgReceptii.ClassificationsRequested
        Try
            If _ke Is Nothing OrElse _shownId = 0 Then Return
            Using k_f As New KeClasificariForm(_api, _gate, _shownId, InvoiceLabel(), _ke, _keCod)
                If k_f.ShowDialog(FindForm()) <> DialogResult.OK OrElse k_f.Salvata Is Nothing Then Return
                If k_f.CodAngajament.Length > 0 Then _keCod = k_f.CodAngajament
                ApplyKe(k_f.Salvata)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteView.PgReceptii_ClassificationsRequested", ex)
            Notify("Clasificațiile liniilor nu au putut fi alese. Detalii în jurnalul de erori.", MessageBoxIcon.Error)
        End Try
    End Sub

    ' UI boundary (async Sub): the accountant wants a link of one of the three kinds.
    Private Async Sub PgReceptii_LinkRequested(k_tip As String) Handles pgReceptii.LinkRequested
        Try
            Await LinkAsync(k_tip).ConfigureAwait(True)
        Catch ex As OperationCanceledException
            ' The host was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("PrimiteView.PgReceptii_LinkRequested", ex)
            Notify(ex.Message, MessageBoxIcon.Warning)
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteView.PgReceptii_LinkRequested", ex)
            Notify("Legarea facturii de recepție nu s-a putut face. Detalii în jurnalul de erori.", MessageBoxIcon.Error)
        Finally
            _keBusy = False
            If Not IsDisposed Then pgReceptii.SetBusy(False)
        End Try
    End Sub

    ' UI boundary (async Sub): removes one link row after a question.
    Private Async Sub PgReceptii_RemoveRequested(k_idLegatura As Integer) Handles pgReceptii.RemoveRequested
        Try
            If _ke Is Nothing OrElse _shownId = 0 OrElse _keBusy Then Return
            If KBotMessage.Show(FindForm(), "Scoateți legătura dintre factură și recepție? Recepția din FOREXE nu se atinge.", "Recepții",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return
            Dim k_id As Integer = _shownId
            Dim k_state As EFacturaKeStare = Await _gate.RunAsync(
                Function() _api.RemoveKeLegaturaAsync(k_id, k_idLegatura, _cts.Token)).ConfigureAwait(True)
            If IsDisposed OrElse k_id <> _shownId Then Return
            ApplyKe(k_state)
        Catch ex As OperationCanceledException
            ' The host was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("PrimiteView.PgReceptii_RemoveRequested", ex)
            Notify(ex.Message, MessageBoxIcon.Warning)
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteView.PgReceptii_RemoveRequested", ex)
            Notify("Legătura nu a putut fi scoasă. Detalii în jurnalul de erori.", MessageBoxIcon.Error)
        End Try
    End Sub

    ' ── Linking ─────────────────────────────────────────────────────────────────

    Private Async Function LinkAsync(k_tip As String) As Task
        If _ke Is Nothing OrElse _detail Is Nothing OrElse _shownId = 0 OrElse _keBusy Then Return
        If Not _ke.PoateScrie Then
            Notify("Numai un utilizator cu rolul «Contabil» poate lega facturile de recepții.", MessageBoxIcon.Information)
            Return
        End If
        If k_tip <> EFacturaKeTip.Existenta AndAlso KeRobot.Current Is Nothing Then
            Notify("Robotul FOREXE nu este disponibil, deci nu se poate face sau modifica o recepție. Puteți lega doar de o recepție existentă.", MessageBoxIcon.Information)
            Return
        End If
        Dim k_invoice As EFacturaPrimita = _detail.Factura
        Dim k_id As Integer = _shownId
        Dim k_description As String = $"plata factura {k_invoice.NrFact}" &
            If(k_invoice.DataFact.HasValue, "/" & k_invoice.DataFact.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), String.Empty)
        Using k_f As New KeLegaForm(_api, _gate, k_id, InvoiceLabel(), k_tip, _ke, _keCod,
                                    If(k_invoice.DataFact, Date.Today), k_description)
            If k_f.ShowDialog(FindForm()) <> DialogResult.OK OrElse k_f.Cerere Is Nothing Then Return
            _keBusy = True
            pgReceptii.SetBusy(True)
            Dim k_request As EFacturaKeCerere = k_f.Cerere
            _keCod = k_request.CodAngajament

            ' 1. The server's check: the indicator exists, the amounts fit. A problem stops here, before anything reaches FOREXE.
            Dim k_check As EFacturaKeVerificare = Await _gate.RunAsync(
                Function() _api.VerifyKeAsync(k_id, k_request, _cts.Token)).ConfigureAwait(True)
            If IsDisposed OrElse k_id <> _shownId Then Return
            If Not k_check.Ok Then
                Notify(String.Join(Environment.NewLine, k_check.Probleme), MessageBoxIcon.Warning)
                Return
            End If
            If k_check.Avertismente.Count > 0 AndAlso KBotMessage.Show(FindForm(),
                    String.Join(Environment.NewLine, k_check.Avertismente) & Environment.NewLine & Environment.NewLine & "Continuați?",
                    "Recepții", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then Return

            ' 2. The link itself.
            Select Case k_tip
                Case EFacturaKeTip.Existenta
                    Await WriteLinkAsync(k_id, k_request).ConfigureAwait(True)
                Case EFacturaKeTip.Noua
                    Await NewReceptionAsync(k_id, k_f, k_request, k_check).ConfigureAwait(True)
                Case EFacturaKeTip.Modificata
                    Await ChangeReceptionAsync(k_id, k_f, k_request, k_check).ConfigureAwait(True)
                Case Else
                    Throw New ArgumentException("Unknown kind of link: " & k_tip, NameOf(k_tip))
            End Select
        End Using
    End Function

    Private Async Function WriteLinkAsync(k_id As Integer, k_request As EFacturaKeCerere) As Task
        Dim k_state As EFacturaKeStare = Await _gate.RunAsync(
            Function() _api.AddKeLegaturiAsync(k_id, k_request, _cts.Token)).ConfigureAwait(True)
        If IsDisposed OrElse k_id <> _shownId Then Return
        ApplyKe(k_state)
    End Function

    ' Reserves the numbers the K-BOT marker carries; the marker is written on the site with the reception, so the download that follows
    ' knows whose it is. Nothing is reserved when the marker route is not there.
    Private NotInheritable Class KeMarker
        Public Property Text As String = String.Empty
        Public Property Idrh As Integer
        Public Property Idr As Integer
    End Class

    Private Async Function ReserveMarkerAsync(k_cod As String) As Task(Of KeMarker)
        Dim k_marker As IMarcajApi = TryCast(_api, IMarcajApi)
        If k_marker Is Nothing Then Throw New InvalidOperationException("The API client does not implement IMarcajApi.")
        Dim k_text As String = Await _gate.RunAsync(Function() k_marker.RezervaMarcajAsync("receptie", k_cod, _cts.Token)).ConfigureAwait(True)
        Dim k_match As Match = _markerPattern.Match(k_text)
        If Not k_match.Success Then Throw New ApiException("Serverul a trimis un marcaj necunoscut pentru recepție.")
        Return New KeMarker() With {.Text = k_text, .Idrh = Integer.Parse(k_match.Groups(1).Value, CultureInfo.InvariantCulture),
                                    .Idr = Integer.Parse(k_match.Groups(2).Value, CultureInfo.InvariantCulture)}
    End Function

    Private Shared Function RowsOf(k_check As EFacturaKeVerificare, k_modify As Boolean) As List(Of KeReceptieRand)
        Dim k_rows As New List(Of KeReceptieRand)()
        For Each k_r As EFacturaKeRand In k_check.Randuri
            ' a new reception takes the allocated amount; a changed one takes its new value on that indicator
            Dim k_value As Decimal = If(k_modify, If(k_r.ValoareNoua, k_r.Suma), k_r.Suma)
            k_rows.Add(New KeReceptieRand() With {.ClsfSal = k_r.ClsfSal, .Clsf = k_r.Clsf, .CodIndicator = k_r.CodIndicator, .Valoare = k_value})
        Next
        Return k_rows
    End Function

    ' A NEW reception: the robot adds it in FOREXE with the marker in its description; only when it is saved is the link written.
    Private Async Function NewReceptionAsync(k_id As Integer, k_form As KeLegaForm, k_request As EFacturaKeCerere,
                                             k_check As EFacturaKeVerificare) As Task
        Dim k_marker As KeMarker = Await ReserveMarkerAsync(k_request.CodAngajament).ConfigureAwait(True)
        Dim k_job As New KeReceptieDeSalvat() With {
            .CodAngajament = k_request.CodAngajament, .Modifica = False, .TipReceptie = k_form.TipReceptie, .Data = k_form.DataReceptie,
            .Descriere = (k_form.DescriereReceptie & " " & k_marker.Text).Trim(), .Marcaj = k_marker.Text, .Eticheta = InvoiceLabel()}
        k_job.Randuri.AddRange(RowsOf(k_check, False))
        Dim k_saved As KeReceptieSalvata = Await KeRobot.Current.SalveazaReceptieAsync(k_job, _cts.Token).ConfigureAwait(True)
        If IsDisposed OrElse k_id <> _shownId Then Return
        If k_saved Is Nothing OrElse Not k_saved.Salvata Then
            Notify(If(k_saved Is Nothing, "Recepția nu s-a putut salva în FOREXE.", k_saved.Mesaj), MessageBoxIcon.Warning)
            Return
        End If
        k_request.Idrh = k_marker.Idrh
        k_request.Idr = k_marker.Idr
        Await WriteLinkAsync(k_id, k_request).ConfigureAwait(True)
        If Not k_saved.Preluata Then
            Notify("Recepția este salvată în FOREXE, dar K-BOT nu a citit-o încă. Legătura așteaptă descărcarea angajamentului.", MessageBoxIcon.Information)
        End If
    End Function

    ' A reception CHANGED: the robot finds its row (by the marker K-BOT wrote, else by date and total), checks it, raises the values and saves.
    Private Async Function ChangeReceptionAsync(k_id As Integer, k_form As KeLegaForm, k_request As EFacturaKeCerere,
                                                k_check As EFacturaKeVerificare) As Task
        Dim k_rec As EFacturaKeReceptie = k_form.Receptie
        If k_rec Is Nothing OrElse Not k_rec.DataR.HasValue Then
            Notify("Recepția aleasă nu are dată; nu poate fi găsită în FOREXE.", MessageBoxIcon.Warning)
            Return
        End If
        Dim k_marker As KeMarker = Await ReserveMarkerAsync(k_request.CodAngajament).ConfigureAwait(True)
        Dim k_job As New KeReceptieDeSalvat() With {
            .CodAngajament = k_request.CodAngajament, .Modifica = True, .Marcaj = k_marker.Text, .DataTinta = k_rec.DataR, .SumaTinta = k_rec.Total,
            .MarcajRegex = MarkerRegex(k_rec), .Eticheta = InvoiceLabel()}
        k_job.Randuri.AddRange(RowsOf(k_check, True))
        Dim k_saved As KeReceptieSalvata = Await KeRobot.Current.SalveazaReceptieAsync(k_job, _cts.Token).ConfigureAwait(True)
        If IsDisposed OrElse k_id <> _shownId Then Return
        If k_saved Is Nothing OrElse Not k_saved.Salvata Then
            Notify(If(k_saved Is Nothing, "Recepția nu s-a putut modifica în FOREXE.", k_saved.Mesaj), MessageBoxIcon.Warning)
            Return
        End If
        Await WriteLinkAsync(k_id, k_request).ConfigureAwait(True)
        If Not k_saved.Preluata Then
            Notify("Recepția este modificată în FOREXE, dar K-BOT nu a citit-o încă. Descărcați angajamentul ca să vedeți valoarea nouă.", MessageBoxIcon.Information)
        End If
    End Function

    ' The pattern of the marker numbers K-BOT may have written in the description of a reception; empty when it never wrote one.
    ' No colon in it: the workflow's IfVar splits its comparison on colons, and the dot stands for the one after «IDRH».
    Private Shared Function MarkerRegex(k_rec As EFacturaKeReceptie) As String
        If k_rec.Idrhs.Count = 0 Then Return String.Empty
        Return "IDRH.\s*(" & String.Join("|", k_rec.Idrhs.Select(Function(k_n) k_n.ToString(CultureInfo.InvariantCulture))) & ")\b"
    End Function

End Class
