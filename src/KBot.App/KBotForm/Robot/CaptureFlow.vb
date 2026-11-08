Option Strict On
Imports System.Collections.Generic
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Forexe
Imports KBot.Theming

''' <summary>
''' The pictures of the FOREXE page (operator, 28.09.2026), taken while the operator works and
''' sent to the server once the ingest has written the rows they hang off. Slice 0086-02: moved
''' out of <c>KbotForm.ForexeWatch.vb</c> without any change of behaviour; the form keeps one
''' instance (<c>_captures</c>).
''' </summary>
''' <remarks>
''' Every method here is called from the UI thread and keeps the contract it had in the form:
''' a picture that cannot be taken or sent is SAID (console, <see cref="CapturiLog"/>) and never
''' thrown, because what the pictures illustrate is already saved.
''' </remarks>
Friend NotInheritable Class CaptureFlow

    Private ReadOnly _host As IShellHost
    Private ReadOnly _owner As IWin32Window
    Private ReadOnly _controller As ForexeController
    Private ReadOnly _capturiApi As IForexeCapturiApi
    Private ReadOnly _reservationSessionOpen As Func(Of String, Boolean)

    ''' <param name="k_reservationSessionOpen">
    ''' True when the angajament has reservation edits still kept (the watcher's memory): its
    ''' pictures are then NOT offered for deletion, because the «DA» that takes the rows is yet to come.
    ''' </param>
    Friend Sub New(k_host As IShellHost, k_owner As IWin32Window, k_controller As ForexeController,
                   k_capturiApi As IForexeCapturiApi, k_reservationSessionOpen As Func(Of String, Boolean))
        If k_host Is Nothing Then Throw New ArgumentNullException(NameOf(k_host))
        If k_owner Is Nothing Then Throw New ArgumentNullException(NameOf(k_owner))
        If k_controller Is Nothing Then Throw New ArgumentNullException(NameOf(k_controller))
        If k_reservationSessionOpen Is Nothing Then Throw New ArgumentNullException(NameOf(k_reservationSessionOpen))
        _host = k_host
        _owner = k_owner
        _controller = k_controller
        _capturiApi = k_capturiApi
        _reservationSessionOpen = k_reservationSessionOpen
    End Sub

    ''' <summary>
    ''' The two pictures the ALOP guide asks for a reception (p.21-22): the Recepții tab as
    ''' the save left it, then «Informații complete contract» with its table pushed to the
    ''' right end (K-BOT presses the button itself, behind the veil, and comes back with
    ''' «Înapoi»). A picture that cannot be taken is said on the console, never thrown: the
    ''' reception is already saved in FOREXE.
    ''' </summary>
    ''' <param name="k_marcaj">
    ''' Slice 02EF-10: the snapshot number (IDRH) K-BOT itself reserved for the reception when the robot writes it from the e-invoice window; the
    ''' pictures then carry it from the start. 0 (the operator's own save in the page) = the number is filled in later from the page's marker.
    ''' </param>
    Friend Async Function TakeReceptionPicturesAsync(cod As String, Optional k_marcaj As Integer = 0) As Task
        Try
            Dim k_receptii As CapturaForexe =
                Await _controller.IaCapturaAsync(cod, CapturaStore.FelReceptie, CapturaStore.MomentReceptii)
            Dim k_info As CapturaForexe = Await _controller.IaCapturaInfoCompleteAsync(cod)
            If k_marcaj > 0 Then
                If k_receptii IsNot Nothing Then CapturaStore.PuneMarcaj(k_receptii, k_marcaj)
                If k_info IsNot Nothing Then CapturaStore.PuneMarcaj(k_info, k_marcaj)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("CaptureFlow.TakeReceptionPicturesAsync", ex)
            _controller.SpuneStare("Capturile recepției nu s-au putut face: " & ex.Message)
        End Try
    End Function

    ''' <summary>
    ''' The pictures waiting on disk for this angajament go to the server, each on the number
    ''' the page's marker named (the DDF revision for a reservation, the reception snapshot
    ''' for a reception). One that goes is taken off the disk. The ones that do not go are
    ''' offered for deletion (operator, 28.09.2026 - the same question as for the signed PDFs
    ''' that did not reach the server); «Nu» keeps them for the next download of the angajament.
    ''' </summary>
    Friend Async Function SendAsync(cod As String, fel As String) As Task
        Const Sursa As String = "CaptureFlow.SendAsync"
        Dim ramase As New List(Of KeyValuePair(Of CapturaForexe, String))()
        Dim trimise As Integer = 0
        Try
            Dim lista As List(Of CapturaForexe) = _controller.CapturileDe(cod, fel)
            If lista.Count = 0 Then
                ' Case 1 (operator, 28.09.2026): said on the console and in the log, with what IS
                ' on disk - a picture kept under another code (or «fara_cod») shows up there.
                Dim mesaj As String = $"«{cod}»: nicio captură de {fel} de trimis — nu există niciuna în " &
                                      $"«{CapturaStore.FolderAngajament(cod)}»."
                _controller.SpuneStare(mesaj)
                CapturiLog.Write(Sursa, mesaj & $" Pe disc: {CapturaStore.Inventar()}.", KBotLogLevel.Warn)
                Return
            End If
            CapturiLog.Write(Sursa, $"«{cod}»: trimit {lista.Count} captură(i) de {fel}.")
            For Each captura As CapturaForexe In lista
                If captura.Marcaj <= 0 Then
                    ramase.Add(New KeyValuePair(Of CapturaForexe, String)(
                        captura, "nu are numărul pe care să se sprijine"))
                    CapturiLog.Write(Sursa, "NU se trimite (fără număr — marcajul nu a fost primit pentru acest cod): " &
                                            CapturaStore.Descrie(captura), KBotLogLevel.Warn)
                    Continue For
                End If
                Try
                    Dim octeti As Byte() = CapturaStore.CitesteOcteti(captura)
                    If octeti Is Nothing OrElse octeti.Length = 0 Then
                        ramase.Add(New KeyValuePair(Of CapturaForexe, String)(captura, "fișierul e gol"))
                        CapturiLog.Write(Sursa, "NU se trimite (fișier gol sau lipsă): " & CapturaStore.Descrie(captura),
                                         KBotLogLevel.Warn)
                        Continue For
                    End If
                    Dim receptie As Boolean = String.Equals(fel, CapturaStore.FelReceptie, StringComparison.OrdinalIgnoreCase)
                    CapturiLog.Write(Sursa, $"PUT /api/forexe/capturi/{If(receptie, "receptie", "rezervare")}/{captura.Marcaj} " &
                                            $"({octeti.Length \ 1024} KB): {CapturaStore.Descrie(captura)}")
                    Dim id As Integer
                    If receptie Then
                        id = Await _host.WithReauth(Of Integer)(
                            Function() _capturiApi.UrcaCapturaReceptieAsync(
                                captura.Marcaj, cod, captura.Nume, captura.Moment, octeti, CancellationToken.None))
                    Else
                        id = Await _host.WithReauth(Of Integer)(
                            Function() _capturiApi.UrcaCapturaRezervareAsync(
                                captura.Marcaj, cod, captura.Nume, captura.Moment, octeti, CancellationToken.None))
                    End If
                    If id > 0 Then
                        CapturiLog.Write(Sursa, $"Salvată pe server cu cheia {id}; se șterge de pe disc: {captura.Nume}")
                        CapturaStore.Sterge(captura)
                        trimise += 1
                    Else
                        ramase.Add(New KeyValuePair(Of CapturaForexe, String)(captura, "serverul nu a întors cheia"))
                        CapturiLog.Write(Sursa, "Serverul nu a întors cheia: " & captura.Nume, KBotLogLevel.Warn)
                    End If
                Catch ex As Exception
                    ' One picture that will not go must not stop the others.
                    GlobalErrorLog.Write(Sursa, ex)
                    ramase.Add(New KeyValuePair(Of CapturaForexe, String)(captura, ex.Message))
                    CapturiLog.Write(Sursa, $"Trimiterea a eșuat ({ex.GetType().Name}): {ex.Message} — {captura.Nume}",
                                     KBotLogLevel.Error)
                End Try
            Next
        Catch ex As Exception
            GlobalErrorLog.Write(Sursa, ex)
            _controller.SpuneCapturi(Sursa, "Capturile nu au putut fi trimise: " & ex.Message, KBotLogLevel.Error)
            Return
        End Try

        If trimise > 0 Then
            _controller.SpuneStare($"{trimise} captură(i) din FOREXE au fost salvate pentru «{cod}».")
        End If
        CapturiLog.Write(Sursa, $"«{cod}», {fel}: {trimise} trimise, {ramase.Count} rămase pe disc.",
                         If(ramase.Count > 0, KBotLogLevel.Warn, KBotLogLevel.Info))
        If ramase.Count > 0 Then AskToDelete(cod, fel, ramase)
    End Function

    ''' <summary>
    ''' Asks whether to delete the pictures that did not reach the server. A reservation
    ''' session still being worked on is NOT asked about: its «before» picture waits for the
    ''' number the first save reserves, and its rows are not in K-BOT until «DA» - deleting
    ''' them now would take them out of the document.
    ''' </summary>
    Private Sub AskToDelete(cod As String, fel As String,
                            ramase As List(Of KeyValuePair(Of CapturaForexe, String)))
        Try
            Dim sesiuneDeschisa As Boolean =
                String.Equals(fel, CapturaStore.FelRezervare, StringComparison.OrdinalIgnoreCase) AndAlso
                _reservationSessionOpen(cod)
            Dim deIntrebat As New List(Of KeyValuePair(Of CapturaForexe, String))()
            For Each kvp As KeyValuePair(Of CapturaForexe, String) In ramase
                Dim c As CapturaForexe = kvp.Key
                ' The «before» picture of today with no number yet: the first save is still to come.
                Dim inainteFaraNumar As Boolean =
                    String.Equals(c.Moment, CapturaStore.MomentInainte, StringComparison.OrdinalIgnoreCase) AndAlso
                    c.Marcaj <= 0 AndAlso c.LuataLa.Date = Date.Today
                If sesiuneDeschisa OrElse inainteFaraNumar Then Continue For
                deIntrebat.Add(kvp)
            Next

            Dim folder As String = CapturaStore.FolderAngajament(cod)
            Dim inLucru As Integer = ramase.Count - deIntrebat.Count
            If inLucru > 0 Then
                _controller.SpuneCapturi("CaptureFlow.AskToDelete",
                    $"{inLucru} captură(i) ale sesiunii de rezervări în lucru rămân pe disc ({folder}) " &
                    $"— sesiune deschisă: {If(sesiuneDeschisa, "da", "nu")}; nu se întreabă de ștergere.")
            End If
            If deIntrebat.Count = 0 Then Return

            Dim nl As String = Environment.NewLine
            Dim randuri As IEnumerable(Of String) = deIntrebat.Select(
                Function(kvp) $" - {PictureLabel(kvp.Key)}, din {kvp.Key.LuataLa:dd.MM.yyyy HH:mm}: {kvp.Value}")
            Dim text As String =
                $"«{cod}»: {deIntrebat.Count} captură(i) din FOREXE NU au ajuns pe server:" & nl &
                String.Join(nl, randuri) & nl & nl &
                "Le ștergeți de pe acest calculator?" & nl &
                "Da = se șterg (nu vor mai apărea în documente); " &
                "Nu = rămân și se reîncearcă la următoarea descărcare a angajamentului."
            If KBotMessage.Show(_owner, text, "Capturi FOREXE", MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) <> DialogResult.Yes Then
                _controller.SpuneCapturi("CaptureFlow.AskToDelete",
                    $"Operatorul a ales «Nu»: {deIntrebat.Count} captură(i) rămân pe disc ({folder}).")
                Return
            End If

            CapturiLog.Write("CaptureFlow.AskToDelete",
                             $"Operatorul a ales «Da»: se șterg {deIntrebat.Count} captură(i) ale lui «{cod}».")
            For Each kvp As KeyValuePair(Of CapturaForexe, String) In deIntrebat
                CapturiLog.Write("CaptureFlow.AskToDelete",
                                 $"Șterg: {CapturaStore.Descrie(kvp.Key)} (motiv netrimis: {kvp.Value})")
                CapturaStore.Sterge(kvp.Key)
            Next
            ' Sterge logs and swallows; what is still on disk is said once.
            Dim neSterse As Integer = deIntrebat.Where(Function(kvp) IO.File.Exists(kvp.Key.Fisier)).Count()
            If neSterse > 0 Then
                CapturiLog.Write("CaptureFlow.AskToDelete",
                                 $"{neSterse} captură(i) nu s-au putut șterge din «{folder}».", KBotLogLevel.Error)
                KBotMessage.Show(_owner, $"{neSterse} captură(i) nu s-au putut șterge. Fișierele sunt în «{folder}».",
                                 "Capturi FOREXE", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("CaptureFlow.AskToDelete", ex)
        End Try
    End Sub

    ''' <summary>
    ''' The pictures of this angajament are NOT sent this time, and why - on the console and in
    ''' <see cref="CapturiLog"/>, only when there is something waiting on disk (a download of an
    ''' angajament nobody photographed has nothing to say here).
    ''' </summary>
    Friend Sub SayNotSent(cod As String, motiv As String)
        Try
            Dim asteapta As List(Of CapturaForexe) = CapturaStore.AleSale(cod)
            If asteapta.Count = 0 Then Return
            _controller.SpuneCapturi("CaptureFlow.SayNotSent",
                $"«{cod}»: {asteapta.Count} captură(i) NU se trimit acum — {motiv}. Rămân în " &
                $"«{CapturaStore.FolderAngajament(cod)}» pentru următoarea descărcare.", KBotLogLevel.Warn)
        Catch ex As Exception
            GlobalErrorLog.Write("CaptureFlow.SayNotSent", ex)
        End Try
    End Sub

    ''' <summary>The operator's name of one picture, for the deletion question.</summary>
    Private Shared Function PictureLabel(c As CapturaForexe) As String
        Select Case If(c.Moment, String.Empty).ToLowerInvariant()
            Case CapturaStore.MomentInainte : Return "rezervări, înainte de modificare"
            Case CapturaStore.MomentDupa : Return "rezervări, după modificare"
            Case CapturaStore.MomentReceptii : Return "recepții"
            Case CapturaStore.MomentInfoComplete : Return "informații complete contract"
            Case Else : Return If(String.IsNullOrEmpty(c.Moment), c.Nume, c.Moment)
        End Select
    End Function

End Class
