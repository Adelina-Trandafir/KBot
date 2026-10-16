Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Domain
Imports KBot.EFactura
Imports KBot.Forexe

''' <summary>
''' Slices KE05 / KE07 -- the shell's FOREXE robot, given to the e-invoice views (<see cref="KeRobot"/>): it ADDS a reception in FOREXE or CHANGES one
''' (workflows «Receptie Noua» / «Receptie Modificata»), then reads the angajament back and takes the reception into K-BOT like any download.
''' Plan docs/KE_PLAN.md.
'''
''' <para><b>How a save is confirmed.</b> The workflow leaves the text of the site's success element (<c>MesajSalvare</c>). When it is there the
''' reception is saved. Only when it is NOT there, the read-back is also used as a check: the reception is saved if the history of the angajament
''' holds its K-BOT marker. Either way the read-back runs, because the new rows of the history have to come into K-BOT.</para>
''' </summary>
Partial Public Class KbotForm

    ''' <summary>The robot as the e-invoice views see it: one door, <see cref="SalveazaReceptieKeAsync"/>.</summary>
    Private NotInheritable Class KeRobotKbot
        Implements IKeRobot

        Private ReadOnly _owner As KbotForm

        Public Sub New(k_owner As KbotForm)
            ArgumentNullException.ThrowIfNull(k_owner)
            _owner = k_owner
        End Sub

        Public Function SalveazaReceptieAsync(k_cerere As KeReceptieDeSalvat, ct As CancellationToken) As Task(Of KeReceptieSalvata) _
            Implements IKeRobot.SalveazaReceptieAsync
            Return _owner.SalveazaReceptieKeAsync(k_cerere)
        End Function
    End Class

    ''' <summary>Hands the robot to the e-invoice views; called once from Load, with the other subscriptions.</summary>
    Private Sub LeagaRobotulKe()
        KeRobot.Current = New KeRobotKbot(Me)
    End Sub

    ''' <summary>The robot is the shell's own: it must not outlive it.</summary>
    Private Sub DezleagaRobotulKe()
        If TypeOf KeRobot.Current Is KeRobotKbot Then KeRobot.Current = Nothing
    End Sub

    ''' <summary>
    ''' Waits its turn in the robot queue (a DDF send does the same), then runs <see cref="SalveazaReceptieKeInCoadaAsync"/>. Rethrows what is
    ''' unexpected after logging it (the caller is the view's handler, which tells the operator); a task the operator dropped from the queue
    ''' is an answer, not a failure.
    ''' </summary>
    Private Async Function SalveazaReceptieKeAsync(k_cerere As KeReceptieDeSalvat) As Task(Of KeReceptieSalvata)
        Try
            ArgumentNullException.ThrowIfNull(k_cerere)
            Dim k_eticheta As String = If(String.IsNullOrWhiteSpace(k_cerere.Eticheta), k_cerere.CodAngajament, k_cerere.Eticheta)
            Dim k_rol As String = If(k_cerere.Modifica, "Modificare recepție", "Recepție nouă")
            Return Await _robotQueue.RunWithResultAsync(Of KeReceptieSalvata)(
                Nothing, $"{k_rol} «{k_eticheta}» pe «{k_cerere.CodAngajament}»",
                Function() SalveazaReceptieKeInCoadaAsync(k_cerere)).ConfigureAwait(True)
        Catch ex As RobotTaskDroppedException
            Return New KeReceptieSalvata() With {.Mesaj = "Operația a fost scoasă din coada robotului: nu s-a salvat nimic în FOREXE."}
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.SalveazaReceptieKeAsync", ex)
            Throw
        End Try
    End Function

    Private Async Function SalveazaReceptieKeInCoadaAsync(k_cerere As KeReceptieDeSalvat) As Task(Of KeReceptieSalvata)
        Dim k_cod As String = k_cerere.CodAngajament
        busyBar.Running = True
        Try
            ' 1. The workflow: adds or changes the reception in FOREXE.
            Dim k_job As JobRequest = If(k_cerere.Modifica, JobBuilder.BuildReceptieModificata(k_cerere), JobBuilder.BuildReceptieNoua(k_cerere))
            Dim k_run As JobResult = Await _controller.RuleazaTrimitereAsync(k_job, k_cod).ConfigureAwait(True)
            If k_run Is Nothing Then
                Return New KeReceptieSalvata() With {
                    .Mesaj = "Robotul FOREXE nu a pornit: " &
                             If(String.IsNullOrEmpty(_controller.LastFailure), "sesiunea FOREXE nu s-a deschis.", _controller.LastFailure)}
            End If
            If k_run.StoppedBeforeSave Then
                Return New KeReceptieSalvata() With {.Mesaj = "Mod probă: robotul s-a oprit înainte de salvare. Nu s-a salvat nimic în FOREXE."}
            End If
            Dim k_text As String = Nothing
            Dim k_seen As Boolean = k_run.Data.TryGetValue(WorkflowCatalog.VarMesajSalvare, k_text) AndAlso Not String.IsNullOrWhiteSpace(k_text)
            If k_cerere.Modifica AndAlso k_run.Success Then
                Dim k_found As String = Nothing
                If Not (k_run.Data.TryGetValue(WorkflowCatalog.VarGasita, k_found) AndAlso String.Equals(k_found, "1", StringComparison.Ordinal)) Then
                    Return New KeReceptieSalvata() With {
                        .Mesaj = "Recepția de modificat nu a fost găsită sigur în FOREXE (data, suma sau marcajul nu se potrivesc). Nu s-a salvat nimic."}
                End If
            End If

            ' 2. The read-back: the angajament's receptions of that day and the new rows of its history. It also tells whether the save happened
            '    when the site's message did not show up.
            _controller.SpuneStare($"Recepția «{k_cerere.Eticheta}» a fost trimisă în FOREXE — descarc «{k_cod}»...")
            Dim k_data As Date? = If(k_cerere.Modifica, k_cerere.DataTinta, CType(k_cerere.Data, Date?))
            Dim k_pachet As PrelucrareRezultat = Await _controller.DownloadReceptieEditataAsync(k_cod, k_data, CitesteIstoricul()).ConfigureAwait(True)
            Dim k_saved As Boolean = k_seen OrElse (k_pachet IsNot Nothing AndAlso ReceptiaEsteInPachet(k_pachet, k_cerere.Marcaj))
            If Not k_saved Then
                Return New KeReceptieSalvata() With {
                    .Mesaj = "Nu s-a putut confirma salvarea recepției: FOREXE nu a arătat mesajul de reușită" &
                             If(k_pachet Is Nothing, " și citirea înapoi a angajamentului a eșuat.", " și recepția nu apare la citirea înapoi.") &
                             " Verificați în FOREXE înainte să încercați din nou." &
                             If(k_run.Success, String.Empty, " Robotul a spus: " & k_run.Message)}
            End If

            ' 3. Into K-BOT (the same two-phase ingest every download uses).
            Dim k_taken As Boolean = False
            If k_pachet IsNot Nothing Then k_taken = Await DuLaIngestieAsync(k_cod, k_pachet).ConfigureAwait(True)
            Return New KeReceptieSalvata() With {
                .Salvata = True, .MesajVazut = k_seen, .Preluata = k_taken,
                .Mesaj = If(k_taken, "Recepția este salvată în FOREXE și citită în K-BOT.",
                            "Recepția este salvată în FOREXE; K-BOT nu a putut-o citi încă.")}
        Finally
            busyBar.Running = False
        End Try
    End Function

    ' True when a reception of the package carries the K-BOT marker in its description (whitespace ignored): the marker K-BOT wrote with the save.
    Private Shared Function ReceptiaEsteInPachet(k_pachet As PrelucrareRezultat, k_marcaj As String) As Boolean
        Dim k_wanted As String = CompactMarker(k_marcaj)
        If k_wanted.Length = 0 OrElse k_pachet?.Tabele Is Nothing Then Return False
        For Each k_kvp As KeyValuePair(Of String, TabelRezultat) In k_pachet.Tabele
            If Not k_kvp.Key.StartsWith("ListaReceptii", StringComparison.OrdinalIgnoreCase) OrElse k_kvp.Value Is Nothing Then Continue For
            For Each k_row As RandTabel In k_kvp.Value
                Dim k_cell As CelulaTabel = Nothing
                If k_row.TryGetValue("DescriereReceptie", k_cell) AndAlso k_cell IsNot Nothing AndAlso
                   CompactMarker(k_cell.TextSau(String.Empty)).Contains(k_wanted, StringComparison.Ordinal) Then Return True
            Next
        Next
        Return False
    End Function

    Private Shared Function CompactMarker(k_text As String) As String
        If String.IsNullOrEmpty(k_text) Then Return String.Empty
        Return New String(k_text.Where(Function(k_c) Not Char.IsWhiteSpace(k_c)).ToArray())
    End Function

End Class
