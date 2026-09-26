Option Strict On
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain
Imports KBot.Forexe
Imports KBot.Theming

''' <summary>
''' Slice 0081-04 -- the helpers shared by the DDF send (KbotForm.DdfSend.vb) and the Rezervari
''' menu follow-ups (KbotForm.DdfSendMenu.vb): the send API, codes, captures, grids, fresh reads
''' and landing on the revision. Split out of KbotForm.DdfSend.vb in slice 0086.
''' </summary>
Partial Public Class KbotForm

    Private Function CereApiulDeTrimitere() As IDdfSendApi
        Dim api As IDdfSendApi = TryCast(_apiClient, IDdfSendApi)
        If api Is Nothing Then Throw New InvalidOperationException("The API client does not implement IDdfSendApi.")
        Return api
    End Function

    ' The IDdfSendApi calls without a result, through the one 401 net.
    Private Function ApelTrimitereAsync(apel As Func(Of Task)) As Task(Of Boolean)
        Return WithReauth(Of Boolean)(
            Async Function()
                Await apel().ConfigureAwait(True)
                Return True
            End Function)
    End Function

    ''' <summary>Writes the codes forexecab gave; returns the angajament code now in force.</summary>
    Private Async Function SalveazaCoduriAsync(sendApi As IDdfSendApi, idrev As Integer, codReal As String,
                                               randuri As List(Of DdfCodRand), codCurent As String) As Task(Of String)
        If String.IsNullOrEmpty(codReal) AndAlso (randuri Is Nothing OrElse randuri.Count = 0) Then Return codCurent
        Dim lista As IReadOnlyList(Of DdfCodRand) = If(randuri, New List(Of DdfCodRand)())
        Dim cod As String = Await WithReauth(Of String)(
            Function() sendApi.SalveazaCoduriDdfAsync(idrev, codReal, lista, CancellationToken.None))
        Return If(String.IsNullOrWhiteSpace(cod), codCurent, cod.Trim())
    End Function

    ''' <summary>The forexecab row code of every line that has one and does not carry it yet.</summary>
    Private Shared Function CoduriRandurilor(grila As IReadOnlyList(Of RandTabel), linii As IEnumerable(Of DdfSendLine)) As List(Of DdfCodRand)
        Dim result As New List(Of DdfCodRand)()
        If grila Is Nothing OrElse grila.Count = 0 Then Return result
        For Each l As DdfSendLine In linii
            Dim code As String = DdfSendInputs.RowCode(grila, l)
            If code.Length = 0 OrElse code.StartsWith("!", StringComparison.Ordinal) Then Continue For
            If String.Equals(code, l.CodIndicator, StringComparison.Ordinal) Then Continue For
            result.Add(New DdfCodRand() With {.IdSecA = l.IdSecA, .CodIndicator = code})
        Next
        Return result
    End Function

    ''' <summary>Every <c>Poza_*</c> of a run, stored on the revision. A capture that fails to upload
    ''' is logged and the rest go on: the run already happened, losing all of them is worse.</summary>
    Private Async Function UrcaCapturileAsync(sendApi As IDdfSendApi, idrev As Integer, rezultat As JobResult) As Task
        Dim capturi As List(Of KeyValuePair(Of String, Byte())) = DdfSendInputs.Captures(rezultat?.Data)
        Dim esuate As Integer = 0
        For Each c As KeyValuePair(Of String, Byte()) In capturi
            Dim nume As String = DdfSendInputs.CaptureFileName(c.Key)
            Dim octeti As Byte() = c.Value
            Try
                Await WithReauth(Of Integer)(
                    Function() sendApi.UrcaCapturaDdfAsync(idrev, nume, octeti, CancellationToken.None))
            Catch ex As Exception
                GlobalErrorLog.Write("MainForm.UrcaCapturileAsync", ex)
                esuate += 1
            End Try
        Next
        If esuate > 0 Then
            KBotMessage.Show(Me, $"{esuate} din {capturi.Count} capturi FOREXE nu s-au putut salva pe revizie. " &
                            "Detalii în jurnalul de erori.", TitluTrimitere, MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End If
    End Function

    ''' <summary>The angajament code a «Creare Angajament» run read: CodAng_Final, else the last
    ''' CodAng_&lt;Cheie&gt; read after a row save. Empty when none.</summary>
    Private Shared Function CodulDinRezultat(rezultat As JobResult) As String
        If rezultat?.Data Is Nothing Then Return String.Empty
        Dim final As String = Nothing
        If rezultat.Data.TryGetValue(WorkflowCatalog.VarCodAngajamentFinal, final) Then
            Dim c As String = DdfSendInputs.CodeFromText(final)
            If c.Length > 0 Then Return c
        End If
        Dim ultim As String = String.Empty
        For Each kvp As KeyValuePair(Of String, String) In rezultat.Data
            If kvp.Key Is Nothing OrElse Not kvp.Key.StartsWith(WorkflowCatalog.VarCodAngajamentRandPrefix, StringComparison.Ordinal) Then Continue For
            Dim c As String = DdfSendInputs.CodeFromText(kvp.Value)
            If c.Length > 0 Then ultim = c
        Next
        Return ultim
    End Function

    Private Shared Function GrilaDinRezultat(rezultat As JobResult) As IReadOnlyList(Of RandTabel)
        Dim t As TabelRezultat = Nothing
        If rezultat?.Tables IsNot Nothing AndAlso rezultat.Tables.TryGetValue(WorkflowCatalog.TabelIndicatori, t) Then Return t
        Return New List(Of RandTabel)()
    End Function

    Private Shared Function GrilaIndicatorilor(pachet As PrelucrareRezultat) As IReadOnlyList(Of RandTabel)
        Dim t As TabelRezultat = Nothing
        If pachet?.Tabele IsNot Nothing AndAlso pachet.Tabele.TryGetValue(WorkflowCatalog.TabelIndicatori, t) AndAlso t IsNot Nothing Then Return t
        Return New List(Of RandTabel)()
    End Function

    ''' <summary>The reservations download (the same one as the Rezervari refresh icon).</summary>
    Private Async Function DescarcaRezervarileAsync(cod As String) As Task(Of PrelucrareRezultat)
        busyBar.Running = True
        Try
            Return Await _controller.DownloadRezervariAsync(cod).ConfigureAwait(True)
        Finally
            busyBar.Running = False
        End Try
    End Function

    Private Async Function RevizieProaspataAsync(cod As String, idrev As Integer) As Task(Of RevizieRow)
        Dim ddf As DdfInfo = Await WithReauth(Of DdfInfo)(Function() _apiClient.GetDdfAsync(cod, CancellationToken.None))
        Return ddf?.Revizii?.FirstOrDefault(Function(r) r.Idrev = idrev)
    End Function

    ''' <summary>The angajament's open revision (S0, S1, S1x, S2a) with the highest number, or Nothing.</summary>
    Private Async Function RevizieDeschisaAsync(cod As String) As Task(Of RevizieRow)
        Dim ddf As DdfInfo = Await WithReauth(Of DdfInfo)(Function() _apiClient.GetDdfAsync(cod, CancellationToken.None))
        Return ddf?.Revizii?.Where(Function(r) DdfRevisionStates.IsOpen(r.Stare)).
                             OrderByDescending(Function(r) r.NumarRev).FirstOrDefault()
    End Function

    ''' <summary>forexecab's state of the angajament: the tree's row when it has one, else the draft's.</summary>
    Private Function StareaAngajamentului(cod As String, draft As DdfDraft) As String
        Dim info As AngajamentTreeInfo = Nothing
        If _treeInfos.TryGetValue(cod, info) AndAlso info IsNot Nothing Then Return info.Stare
        Return If(draft?.Stare, String.Empty)
    End Function

    ''' <summary>
    ''' Lands the operator on the revision: the tree reloaded onto <paramref name="cod"/> (it may
    ''' be a new code), the DDF view opened and the revision selected.
    ''' </summary>
    Private Async Function ArataReviziaAsync(cod As String, idrev As Integer) As Task
        Await LoadTreeAsync(codDeSelectat:=cod).ConfigureAwait(True)
        If _currentInfo Is Nothing OrElse Not String.Equals(_currentInfo.CodAngajament, cod, StringComparison.OrdinalIgnoreCase) Then Return
        If Not IsViewEnabled("ddf", _currentInfo) Then Return
        If navViews.SelectedKey <> "ddf" Then navViews.SelectedKey = "ddf"
        TryCast(_activeView, DdfView)?.Reincarca(idrev)
    End Function

End Class
