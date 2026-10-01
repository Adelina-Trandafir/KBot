Imports System.Threading
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain
Imports KBot.Forexe

''' <summary>
''' Slice 0100: several angajamente downloaded AT ONCE, one FOREXE tab each (multi-thread mode:
''' the server must allow it -- <c>Setari.Multithread</c>, slice 0100-02 -- and the operator switches it
''' on in «Setari -> Descarcari multiple»). Three doors lead here -- the «Actualizeaza
''' angajamente...» row of the tree menu (the operator ticks them), the update of the old ones right
''' after a FOREXE connection, and the question after the list refresh brought new ones -- and one
''' road runs from there: <see cref="ActualizeazaMaiMulteAsync"/>.
''' </summary>
Partial Public Class KbotForm

    ''' <summary>
    ''' Slice 0100-02: reads the settings the SERVER decides (table <c>Setari</c> of the connected unit) into
    ''' <see cref="ServerSettings"/>. Run after a login and after a change of unit. A failure is logged and
    ''' leaves every server-controlled setting OFF: a feature the server did not confirm is never on.
    ''' </summary>
    Private Async Function LoadServerSettingsAsync() As Task
        Try
            Dim api As ISetariApi = TryCast(_apiClient, ISetariApi)
            If api Is Nothing Then
                ServerSettings.Clear()
                Return
            End If
            ServerSettings.Apply(Await api.GetServerSettingsAsync(CancellationToken.None))
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.LoadServerSettingsAsync", ex)
            ServerSettings.Clear()
        End Try
    End Function

    ''' <summary>Called once from Load: the on-connect update follows the controller's «connected».</summary>
    Private Sub LeagaActualizareaMultipla()
        AddHandler _controller.Connected, AddressOf Controller_Connected
    End Sub

    Private Sub DezleagaActualizareaMultipla()
        RemoveHandler _controller.Connected, AddressOf Controller_Connected
    End Sub

    ' Raised right after a connection succeeded, still inside the flow that connected (which may
    ' itself be a queued robot task). The update is POSTED, and the posting is cut off from that
    ' flow's context: otherwise the queue would take it for part of the running task and run it in
    ' place, next to the very download that asked for the connection.
    Private Sub Controller_Connected(sender As Object, e As EventArgs)
        Try
            If Not AppSettings.Current.AutoUpdateOnConnectInEffect Then Return
            If IsDisposed OrElse Disposing OrElse Not IsHandleCreated Then Return
            Dim flow As AsyncFlowControl = ExecutionContext.SuppressFlow()
            Try
                BeginInvoke(New Action(AddressOf PornesteActualizareaLaConectare))
            Finally
                flow.Undo()
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.Controller_Connected", ex)
        End Try
    End Sub

    ''' <summary>
    ''' After a connection: the angajamente of the tree whose last saved update is older than the
    ''' configured number of days (and that were downloaded before) go to the multi-thread
    ''' download, with no question asked and every reception read. UI boundary (async Sub).
    ''' </summary>
    Private Async Sub PornesteActualizareaLaConectare()
        Try
            Dim s As AppSettings = AppSettings.Current
            If Not s.AutoUpdateOnConnectInEffect Then Return
            Dim zile As Integer = s.AutoUpdateDaysInEffect
            Dim acum As Date = Date.Now
            Dim vechi As List(Of String) =
                _treeInfos.Values.Where(Function(i) i.EsteNeactualizatDe(zile, acum)).
                                  Select(Function(i) i.CodAngajament).ToList()
            If vechi.Count = 0 Then
                _controller.SpuneStare($"La conectare: niciun angajament neactualizat de {zile} zile.")
                Return
            End If
            _controller.SpuneStare($"La conectare: {vechi.Count} angajamente neactualizate de {zile} zile se descarcă singure.")
            Await ActualizeazaMaiMulteAsync(vechi, "Actualizare la conectare", intrebaReceptii:=False)
        Catch ex As RobotTaskDroppedException
            ' Taken out of the queue by the operator: the console already said it.
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.PornesteActualizareaLaConectare", ex)
            KBotMessage.Show(Me, "Actualizarea automată la conectare a eșuat: " & ex.Message,
                            "FOREXE", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    ''' <summary>
    ''' The tree menu's «Actualizeaza angajamente...»: the operator ticks the angajamente, then the
    ''' ticked ones are downloaded together. UI boundary (async Sub started from a menu click).
    ''' </summary>
    Private Async Sub DeschideActualizareaMultipla()
        Try
            Dim s As AppSettings = AppSettings.Current
            If Not s.MultiThreadInEffect Then Return
            If _treeInfos.Count = 0 Then
                KBotMessage.Show(Me, "Arborele nu are niciun angajament de actualizat.", "Actualizare multiplă",
                                MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            Dim coduri As List(Of String)
            Using dlg As New ActualizareMultiplaForm(_treeInfos.Values.ToList(), s.AutoUpdateDaysInEffect)
                If dlg.ShowDialog(Me) <> DialogResult.OK Then Return
                coduri = dlg.Selectate.ToList()
            End Using
            If coduri.Count = 0 Then Return

            Await ActualizeazaMaiMulteAsync(coduri, "Actualizare multiplă", intrebaReceptii:=True)
        Catch ex As RobotTaskDroppedException
            ' Duplicate or taken out of the queue: the console already said it.
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.DeschideActualizareaMultipla", ex)
            KBotMessage.Show(Me, "Actualizarea multiplă a eșuat: " & ex.Message,
                            "FOREXE", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    ''' <summary>
    ''' After the list refresh added new angajamente to the tree: in multi-thread mode ONLY, asks
    ''' whether to download them now (operator, 01.10.2026). One-at-a-time mode never asks.
    ''' Called from inside the list's own robot task, so the download runs right after it.
    ''' </summary>
    Private Async Function IntreabaDespreAngajamenteleNoiAsync(coduriNoi As IReadOnlyList(Of String)) As Task
        Try
            If coduriNoi Is Nothing OrElse coduriNoi.Count = 0 Then Return
            If Not AppSettings.Current.MultiThreadInEffect Then Return
            If KBotMessage.Show(Me, "Dorești actualizarea angajamentelor noi?" & Environment.NewLine &
                                    $"({coduriNoi.Count} " & If(coduriNoi.Count = 1, "angajament nou)", "angajamente noi)"),
                                "Angajamente noi", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return
            Await ActualizeazaMaiMulteAsync(coduriNoi, "Actualizare angajamente noi", intrebaReceptii:=False)
        Catch ex As RobotTaskDroppedException
            ' Taken out of the queue by the operator: the console already said it.
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.IntreabaDespreAngajamenteleNoiAsync", ex)
            KBotMessage.Show(Me, "Actualizarea angajamentelor noi a eșuat: " & ex.Message,
                            "FOREXE", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Function

    ''' <summary>
    ''' THE multi-thread road: the codes become ONE robot task, which downloads them together (at
    ''' most the configured number of tabs; the rest wait in a FIFO queue), and only when every
    ''' download has ended works through the answers, one at a time, with the same two-phase ingest
    ''' as a single download.
    ''' </summary>
    ''' <param name="intrebaReceptii">
    ''' True = each angajament may open the reception picker (unless «Actualizeaza implicit toate
    ''' receptiile» is on); False = every reception is read, with no question (the automatic doors).
    ''' </param>
    Private Async Function ActualizeazaMaiMulteAsync(coduri As IEnumerable(Of String), eticheta As String,
                                                     intrebaReceptii As Boolean) As Task
        Dim lista As List(Of String) =
            coduri.Where(Function(c) Not String.IsNullOrWhiteSpace(c)).
                   Distinct(StringComparer.OrdinalIgnoreCase).ToList()
        If lista.Count = 0 Then Return
        Dim cheie As String = "multi|" & String.Join(",", lista.OrderBy(Function(c) c, StringComparer.OrdinalIgnoreCase)).GetHashCode()
        Await _robotQueue.RunAsync(cheie, $"{eticheta} ({lista.Count})",
                                   Function() ActualizeazaMaiMulteCuCoadaAsync(lista, intrebaReceptii))
    End Function

    ''' <summary>The task body: questions first, then the robot, then the ingest one by one.</summary>
    Private Async Function ActualizeazaMaiMulteCuCoadaAsync(lista As List(Of String), intrebaReceptii As Boolean) As Task
        Try
            ' 1. What each download needs to know BEFORE the robot starts.
            Dim cereri As New List(Of ParallelNodeRequest)()
            For Each cod As String In lista
                ' Links that do not close would make the save refuse after the robot ran.
                If Not Await AsocierePermiteAsync(cod, "Actualizarea multiplă") Then
                    _controller.SpuneStare($"«{cod}»: sărit din actualizarea multiplă (legăturile recepțiilor nu se închid).")
                    Continue For
                End If
                Dim sarite As New ReceptiiSarite()
                If intrebaReceptii Then
                    ' Nothing = the operator gave up the question, i.e. gave up this download. The
                    ' «update all receptions by default» switch is answered inside.
                    sarite = Await AlegeReceptiileDeSaritAsync(cod)
                    If sarite Is Nothing Then
                        _controller.SpuneStare($"«{cod}»: sărit din actualizarea multiplă (alegerea recepțiilor a fost închisă).")
                        Continue For
                    End If
                End If
                cereri.Add(New ParallelNodeRequest With {.Cod = cod, .Sarite = sarite})
            Next
            If cereri.Count = 0 Then Return

            ' 2. The robot: every download, several tabs at once. Nothing is processed meanwhile.
            Dim rezultate As List(Of ParallelNodeResult)
            busyBar.Running = True
            Try
                rezultate = Await _controller.DownloadNodesParallelAsync(
                    cereri, AppSettings.Current.DownloadThreadsInEffect,
                    Function(c, ct) WithReauth(Of IstoricInfo)(Function() _apiClient.GetIstoricAsync(c, ct)))
            Finally
                busyBar.Running = False
            End Try
            If rezultate Is Nothing Then
                ShowForexeFailure("Actualizare multiplă")
                Return
            End If

            ' 3. All downloads have ended: the answers, ONE AT A TIME, in the order they finished.
            Dim esecuri As New List(Of String)()
            For Each r As ParallelNodeResult In rezultate
                If r.Pachet Is Nothing Then
                    esecuri.Add(r.Failure)
                    SpuneCapturiNetrimise(r.Cod, "descărcarea din FOREXE nu a reușit")
                    Continue For
                End If
                If Not Await DuLaIngestieAsync(r.Cod, r.Pachet) Then
                    esecuri.Add($"«{r.Cod}»: descărcarea nu s-a salvat în K-BOT.")
                    SpuneCapturiNetrimise(r.Cod, "descărcarea nu s-a salvat în K-BOT")
                    Continue For
                End If
                Await TrimiteCapturileAsync(r.Cod, CapturaStore.FelReceptie)
                Await TrimiteCapturileAsync(r.Cod, CapturaStore.FelRezervare)
            Next

            ' A good run is SILENT; only what went wrong is shown (operator, 28.09.2026).
            If esecuri.Count > 0 Then
                Const MaxAfisate As Integer = 10
                Dim text As String = $"{esecuri.Count} din {rezultate.Count} angajamente nu s-au actualizat:" &
                                     Environment.NewLine & String.Join(Environment.NewLine,
                                         esecuri.Take(MaxAfisate).Select(Function(m) "• " & m))
                If esecuri.Count > MaxAfisate Then text &= Environment.NewLine & $"• … și încă {esecuri.Count - MaxAfisate}."
                KBotMessage.Show(Me, text, "Actualizare multiplă", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.ActualizeazaMaiMulteCuCoadaAsync", ex)
            Throw
        End Try
    End Function
End Class
