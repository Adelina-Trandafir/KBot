Option Strict On
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' The shell's half of the in-page watcher (slice 0073). The floating K-BOT menu inside the
''' FOREXE browser reports the operator's own operations - a new angajament, a reservation
''' row, a reception - through <c>ForexeWatch.js</c> -> <c>WorkflowExecutor</c> ->
''' <c>ForexeRunner</c> -> <c>ForexeController.OperatiuneCapturata</c>. When one FINISHES
''' (its save was confirmed by the page) the shell does, by itself, exactly what the node's
''' download icon does: downloads the angajament from FOREXE, takes it through the two-phase
''' ingest, and then opens the angajament's history cut to the minutes the operator worked.
'''
''' <para><b>A new angajament</b> has no node yet, so the angajamente list is synchronised
''' first (the same road as the tree footer icon), the codes that were not in the tree before
''' are taken as the new ones, and each is downloaded in turn - one final save makes one
''' angajament, whether it had one indicator row or several.</para>
'''
''' <para><b>One at a time.</b> A captured operation that arrives while another is still
''' being downloaded is told so and dropped: the robot has one browser, and the operator can
''' always press the node icon later.</para>
''' </summary>
Partial Public Class KbotForm

    ' A captured operation is being downloaded / ingested right now.
    Private _urmarireInLucru As Boolean

    ''' <summary>Subscribes to the coordinator; called once from Load, after Bind.</summary>
    Private Sub LeagaUrmarirea()
        AddHandler _controller.OperatiuneCapturata, AddressOf Controller_OperatiuneCapturata
    End Sub

    ''' <summary>The coordinator is a singleton: a subscription left behind would keep the shell alive.</summary>
    Private Sub DezleagaUrmarirea()
        RemoveHandler _controller.OperatiuneCapturata, AddressOf Controller_OperatiuneCapturata
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        Try
            DezleagaUrmarirea()
            DezleagaBrowserul()
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.OnFormClosed", ex)
        End Try
        MyBase.OnFormClosed(e)
    End Sub

    ' Comes from the Playwright callback thread: onto the UI thread, then act.
    Private Sub Controller_OperatiuneCapturata(sender As Object, ev As ForexeWatchEvent)
        Try
            If ev Is Nothing Then Return
            If IsDisposed OrElse Disposing OrElse Not IsHandleCreated Then Return
            BeginInvoke(Sub() TrateazaOperatiuneaCapturata(ev))
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.Controller_OperatiuneCapturata", ex)
        End Try
    End Sub

    ' UI boundary (async Sub started from BeginInvoke): log and tell, never rethrow.
    Private Async Sub TrateazaOperatiuneaCapturata(ev As ForexeWatchEvent)
        Try
            Select Case ev.Kind
                Case ForexeWatchEventKind.Finished
                    Await PreiaOperatiuneaAsync(ev)
                Case ForexeWatchEventKind.PageOpened
                    ' Slice 0074: the page shows another angajament - the tree follows it,
                    ' the robot stays put (KbotForm.Browser.vb).
                    TrateazaPaginaDeschisa(ev.CodAngajament)
                Case Else
                    ' Started / Cancelled / Info are already on the console, written by the
                    ' executor; nothing for the shell to do with them.
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.TrateazaOperatiuneaCapturata", ex)
            KBotMessage.Show(Me, "Preluarea operațiunii din FOREXE a eșuat: " & ex.Message,
                            "FOREXE", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    ''' <summary>
    ''' A finished operation: find the angajament code(s), download and ingest each one, then
    ''' open its history for the interval. Throws to the caller on anything unexpected.
    ''' </summary>
    Private Async Function PreiaOperatiuneaAsync(ev As ForexeWatchEvent) As Task
        If _urmarireInLucru Then
            KBotMessage.Show(Me,
                $"«{ev.Label}» s-a salvat în FOREXE, dar o operațiune anterioară e încă în lucru." &
                Environment.NewLine &
                "Descărcați angajamentul din iconița nodului când se termină.",
                "FOREXE", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        _urmarireInLucru = True
        Try
            Dim panaLa As Date = If(ev.FinishedAt, Date.Now)
            Dim deLa As Date = If(ev.StartedAt, panaLa)

            Dim coduri As List(Of String)
            If ev.Operation = ForexeOperationKind.Angajament Then
                coduri = Await CoduriNoiDupaSincronizareAsync(ev.CodEfectiv)
            Else
                Dim cod As String = ev.CodEfectiv
                If String.IsNullOrEmpty(cod) AndAlso _currentInfo IsNot Nothing Then
                    ' The page header was not readable at either end; the node the operator has
                    ' selected in the tree is the next best fact, and it is said out loud.
                    cod = If(_currentInfo.CodAngajament, String.Empty)
                    If Not String.IsNullOrEmpty(cod) Then
                        _controller.SpuneStare($"Pagina FOREXE nu a arătat codul; folosesc nodul selectat «{cod}».")
                    End If
                End If
                coduri = New List(Of String)()
                If Not String.IsNullOrEmpty(cod) Then coduri.Add(cod)
            End If

            If coduri.Count = 0 Then
                KBotMessage.Show(Me,
                    $"«{ev.Label}» s-a salvat în FOREXE, dar nu am putut afla codul angajamentului." &
                    Environment.NewLine &
                    "Descărcați-l din iconița nodului din listă.",
                    "FOREXE", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            For Each cod As String In coduri
                _controller.SpuneStare($"«{ev.Label}» salvată în FOREXE — descarc «{cod}»...")
                Dim pachet As PrelucrareRezultat = Await DescarcaNodulAsync(cod)
                ' Nothing = the robot did not start or failed; it already said why on the console.
                If pachet Is Nothing Then
                    ShowForexeFailure("FOREXE")
                    Continue For
                End If
                Await DuLaIngestieAsync(cod, pachet)
                DeschideIstoricInterval(cod, deLa, panaLa, ev.Label)
            Next
        Finally
            _urmarireInLucru = False
        End Try
    End Function

    ''' <summary>
    ''' The node download, without the «which receptions to skip» question: the operator
    ''' just finished working, everything is fresh, everything is wanted.
    ''' </summary>
    Private Async Function DescarcaNodulAsync(cod As String) As Task(Of PrelucrareRezultat)
        busyBar.Running = True
        Try
            Return Await _controller.DownloadNodeAsync(
                cod,
                Function(c, ct) WithReauth(Of IstoricInfo)(Function() _apiClient.GetIstoricAsync(c, ct)),
                Nothing)
        Finally
            busyBar.Running = False
        End Try
    End Function

    ''' <summary>
    ''' Synchronises the angajamente list (the tree footer's road, minus its message box) and
    ''' returns the codes that were NOT in the tree before. <paramref name="codDinPagina"/>,
    ''' the code the FOREXE page showed after the save, is always included when present: the
    ''' page is the primary witness, the list diff the second.
    ''' </summary>
    Private Async Function CoduriNoiDupaSincronizareAsync(codDinPagina As String) As Task(Of List(Of String))
        Dim inainte As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each info As AngajamentTreeInfo In _treeInfos.Values
            If Not String.IsNullOrEmpty(info.CodAngajament) Then inainte.Add(info.CodAngajament)
        Next

        Dim noi As New List(Of String)()
        If Not String.IsNullOrEmpty(codDinPagina) Then noi.Add(codDinPagina)

        Dim mapate As List(Of Angajament)
        busyBar.Running = True
        Try
            mapate = Await _controller.DownloadListaAsync()
        Finally
            busyBar.Running = False
        End Try
        If mapate Is Nothing Then
            ShowForexeFailure("Listă angajamente")
            Return noi
        End If

        ' With no DbName (no login -- possible only in the Debug harness) the list cannot be
        ' written, and an angajament without a header cannot be ingested either.
        If String.IsNullOrEmpty(_session.DbName) Then
            KBotMessage.Show(Me,
                "Lista a fost descărcată, dar nu poate fi trimisă pe server: sesiunea nu are baza " &
                "unității (necesită login). Angajamentul nou nu poate fi preluat.",
                "Listă angajamente", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return New List(Of String)()
        End If

        Dim rezultat As AngajamenteAdaugate
        busyBar.Running = True
        Try
            rezultat = Await WithReauth(Of AngajamenteAdaugate)(
                Function() _apiClient.AdaugaAngajamenteNoiAsync(_session.DbName, mapate, CancellationToken.None))
        Finally
            busyBar.Running = False
        End Try
        If rezultat.Inserate > 0 Then Await LoadTreeAsync(pastreazaSelectia:=True)

        For Each a As Angajament In mapate
            Dim cod As String = If(a.CodAngajament, String.Empty).Trim()
            If String.IsNullOrEmpty(cod) OrElse inainte.Contains(cod) Then Continue For
            If Not noi.Contains(cod, StringComparer.OrdinalIgnoreCase) Then noi.Add(cod)
        Next
        Return noi
    End Function

    ''' <summary>
    ''' The history of the angajament, cut to the operator's minutes in the browser. Modeless,
    ''' owned by the shell, disposed on close - one window per captured operation.
    ''' </summary>
    Private Sub DeschideIstoricInterval(cod As String, deLa As Date, panaLa As Date, eticheta As String)
        Try
            Dim f As New IstoricIntervalForm(_apiClient,
                                             Function(op) WithReauth(Of IstoricInfo)(op),
                                             cod, deLa, panaLa, eticheta)
            AddHandler f.FormClosed, Sub(s, e) f.Dispose()
            f.Show(Me)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.DeschideIstoricInterval", ex)
            KBotMessage.Show(Me, "Fereastra de istoric nu s-a putut deschide: " & ex.Message,
                            "FOREXE", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

End Class
