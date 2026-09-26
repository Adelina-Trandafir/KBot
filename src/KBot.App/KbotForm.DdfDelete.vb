Imports System.Threading
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain

''' <summary>
''' THE DDF EDITOR -- the three deletes (revision, document, month) and what is refreshed after
''' any DDF write (slice 0086 split out of KbotForm.vb). The dispatch is in KbotForm.Ddf.vb.
''' </summary>
Partial Public Class KbotForm

    ''' <summary>
    ''' «Sterge revizia» -- the port of <c>FX_Stergere_Revizie</c>. The confirmation names the
    ''' revision, the way Access's <c>FX_Info_DDF</c> summary did.
    ''' </summary>
    Private Async Function StergeRevizieDdfAsync(revizie As RevizieRow) As Task
        If revizie Is Nothing OrElse revizie.Idrev <= 0 Then
            KBotMessage.Show(Me, "Selectați o revizie din arbore.", "Document de fundamentare",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        ' Slice 0081-02: a revision K-BOT sent (or began to send) has changed forexecab; deleting
        ' it here would leave the two out of step (the golden rule, D8).
        If revizie.StareTrimitere <> DdfSendStage.NotSent Then
            KBotMessage.Show(Me, $"Revizia este «{DdfRevisionStates.Label(revizie.Stare)}»: a modificat deja " &
                            "FOREXE, deci nu se mai șterge din K-BOT.", "Șterge revizia",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim intrebare As String =
            "Dorești ștergerea Reviziei curente?" & vbCrLf & vbCrLf &
            RezumatRevizie(revizie) & vbCrLf & vbCrLf &
            "Odată cu ea se șterg rândurile din secțiunile A și B și fișierele atașate." & vbCrLf &
            "Rezervările acoperite redevin fără DDF."

        If KBotMessage.Show(Me, intrebare, "Șterge revizia",
                           MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then
            Return
        End If

        busyBar.Running = True
        Dim rez As DdfStergereRezultat
        Try
            Dim idrev As Integer = revizie.Idrev
            rez = Await WithReauth(Of DdfStergereRezultat)(
                Function() _apiClient.DeleteDdfRevizieAsync(idrev, CancellationToken.None))
        Finally
            busyBar.Running = False
        End Try

        AratatRezultatulStergerii(rez, "Șterge revizia")
        ' The server decides whether the DOCUMENT survived: deleting one revision leaves it
        ' standing even when it was the last, while the document and month deletes can take it.
        DupaScriereaDdf(cod:=rez?.Cod, documentSters:=(rez IsNot Nothing AndAlso rez.DocumentSters))
    End Function

    ''' <summary>
    ''' «Sterge documentul» -- the port of <c>FX_Stergere_DDF</c>.
    '''
    ''' <para>Refused while any ordonantare points at the document. That refusal exists twice
    ''' over: <c>FX_ORD.IDDF</c> is a RESTRICT foreign key, so the database would stop it
    ''' anyway -- the server's guard is the readable version of the same "no", and it arrives
    ''' here as an <see cref="ApiException"/> with the Romanian message.</para>
    ''' </summary>
    Private Async Function StergeDocumentDdfAsync(revizie As RevizieRow) As Task
        If revizie Is Nothing OrElse revizie.Iddf <= 0 Then
            KBotMessage.Show(Me, "Selectați o revizie din arbore.", "Document de fundamentare",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim intrebare As String =
            "Dorești ștergerea Documentului curent?" & vbCrLf & vbCrLf &
            RezumatRevizie(revizie) & vbCrLf & vbCrLf &
            "Se șterg TOATE reviziile documentului, cu rândurile și fișierele lor." & vbCrLf &
            "Rezervările acoperite redevin fără DDF."

        If KBotMessage.Show(Me, intrebare, "Șterge documentul",
                           MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then
            Return
        End If

        busyBar.Running = True
        Dim rez As DdfStergereRezultat
        Try
            Dim iddf As Integer = revizie.Iddf
            rez = Await WithReauth(Of DdfStergereRezultat)(
                Function() _apiClient.DeleteDdfAsync(iddf, CancellationToken.None))
        Finally
            busyBar.Running = False
        End Try

        AratatRezultatulStergerii(rez, "Șterge documentul")
        DupaScriereaDdf(cod:=rez?.Cod, documentSters:=(rez IsNot Nothing AndAlso rez.DocumentSters))
    End Function

    ''' <summary>
    ''' «Sterge TOATE reviziile lunii» -- the port of <c>FX_Stergere_Revizii</c>.
    '''
    ''' <para>When the month holds EVERY revision the document has, the server deletes the
    ''' DOCUMENT instead of the last revision and says so in the result. That is Access's
    ''' behaviour, kept -- an empty document pointing at nothing is worse than none.</para>
    ''' </summary>
    Private Async Function StergeLunaDdfAsync(comanda As DdfComanda) As Task
        If comanda.Iddf <= 0 OrElse comanda.An <= 0 OrElse comanda.Luna < 1 OrElse comanda.Luna > 12 Then
            KBotMessage.Show(Me, "Selectați o lună din arbore.", "Document de fundamentare",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim intrebare As String =
            "Dorești ștergerea tuturor Reviziilor din luna selectată?" & vbCrLf & vbCrLf &
            $"Luna {comanda.Luna:00}.{comanda.An}, angajamentul {comanda.Cod}." & vbCrLf & vbCrLf &
            "Dacă luna conține toate reviziile documentului, se șterge documentul întreg." & vbCrLf &
            "Rezervările acoperite redevin fără DDF."

        If KBotMessage.Show(Me, intrebare, "Șterge reviziile lunii",
                           MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then
            Return
        End If

        busyBar.Running = True
        Dim rez As DdfStergereRezultat
        Try
            Dim iddf As Integer = comanda.Iddf
            Dim an As Integer = comanda.An
            Dim luna As Integer = comanda.Luna
            rez = Await WithReauth(Of DdfStergereRezultat)(
                Function() _apiClient.DeleteDdfLunaAsync(iddf, an, luna, CancellationToken.None))
        Finally
            busyBar.Running = False
        End Try

        AratatRezultatulStergerii(rez, "Șterge reviziile lunii")
        DupaScriereaDdf(cod:=rez?.Cod, documentSters:=(rez IsNot Nothing AndAlso rez.DocumentSters))
    End Function

    ''' <summary>The <c>FX_Info_DDF</c> summary, built client-side from what the view already
    ''' holds rather than fetched: every field in it is already on the revision row.</summary>
    Private Shared Function RezumatRevizie(revizie As RevizieRow) As String
        Dim ro As New Globalization.CultureInfo("ro-RO")
        Dim data As String = If(revizie.DataRev.HasValue,
                                revizie.DataRev.Value.ToString("dd.MM.yyyy"), "fără dată")
        Dim descriere As String = If(String.IsNullOrWhiteSpace(revizie.DescScurta),
                                     "fără descriere", revizie.DescScurta)
        Return $"Revizia nr. {revizie.NumarRev} din {data}, în valoare de " &
               $"{revizie.TotalRevizie.ToString("N2", ro)} lei." & vbCrLf & descriere
    End Function

    ''' <summary>Reports what actually went, with real counts rather than a bare "done".</summary>
    Private Sub AratatRezultatulStergerii(rez As DdfStergereRezultat, titlu As String)
        If rez Is Nothing Then Return
        Dim ce As String = If(rez.DocumentSters, "Documentul de fundamentare a fost șters.",
                                                 "Reviziile au fost șterse.")
        KBotMessage.Show(Me,
            ce & vbCrLf &
            $"Revizii: {rez.Revizii} · secțiunea A: {rez.LiniiA} · secțiunea B: {rez.LiniiB} · " &
            $"fișiere: {rez.Atasamente}." & vbCrLf &
            $"Rezervări redevenite fără DDF: {rez.RezervariEliberate}.",
            titlu, MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    ''' <summary>
    ''' Refreshes what a DDF write invalidated. The reservations view is refreshed too: a save
    ''' marks reservations as having a DDF and a delete releases them again, so what it shows
    ''' is no longer true either.
    ''' </summary>
    ''' <param name="idrevSalvat">The revision to land on after a save; 0 after a delete.</param>
    ''' <param name="cod">The angajament the write was for. The view gate is only touched when
    ''' the selected node is still that one -- flipping the flag on somebody else's angajament
    ''' would be worse than leaving it stale.</param>
    ''' <param name="iddfDocument">The document key after a save, which also means «the document
    ''' exists»; 0 when this write says nothing about whether it does.</param>
    ''' <param name="documentSters">The whole document went. Only the document delete and the
    ''' month delete can say this -- deleting a single revision leaves the document standing,
    ''' even when it was the last one.</param>
    Private Sub DupaScriereaDdf(Optional idrevSalvat As Integer = 0,
                                Optional cod As String = Nothing,
                                Optional iddfDocument As Integer = 0,
                                Optional documentSters As Boolean = False)
        Try
            TryCast(_activeView, DdfView)?.Reincarca(idrevSalvat)

            ' The reservations view too: a save marks reservations as having a DDF and a delete
            ' releases them again, so the "+" icon that view draws -- now the trigger for the
            ' editor -- is no longer where it belongs. Only the ACTIVE view is refreshed:
            ' `ActivateView` calls `SetContext` on every activation, so an inactive one reloads
            ' by itself when the operator switches to it.
            TryCast(_activeView, RezervariView)?.Reincarca()

            ActualizeazaPoartaDdf(cod, iddfDocument, documentSters)
        Catch ex As Exception
            ' UI boundary: the refresh failed, but the WRITE already succeeded. Logged, and we
            ' carry on -- a throw here would make it look as though the save had failed.
            GlobalErrorLog.Write("MainForm.DupaScriereaDdf", ex)
        End Try
    End Sub

    ''' <summary>
    ''' The «ddf» entry in the vertical navigation, after a write changed whether the
    ''' angajament has a document at all.
    '''
    ''' <para><b>Why it is needed.</b> <c>ApplyViewGating</c> reads
    ''' <c>AngajamentTreeInfo.AreDDF</c>, which is filled once, when the tree is loaded, and
    ''' is <c>FX_Angajamente.IDDF IS NOT NULL</c> on the server. The FIRST document of an
    ''' angajament (revision 0, added from the reservations tree) writes that <c>IDDF</c>, but
    ''' nothing told the shell -- so the entry stayed hidden and the document the operator had
    ''' just saved could not be opened until the next year/SS change reloaded the tree.</para>
    '''
    ''' <para>Raised LOCALLY, exactly as <see cref="DupaScriereaOrdonantarii"/> raises
    ''' <c>AreORD</c>: a <c>LoadTreeAsync</c> from here would clear the selection and drop the
    ''' operator back onto «sumar» the moment they saved. <c>IDDF</c> is carried along with
    ''' the flag rather than left to drift, because that column is what the flag MEANS.</para>
    ''' </summary>
    Private Sub ActualizeazaPoartaDdf(cod As String, iddfDocument As Integer, documentSters As Boolean)
        If _currentInfo Is Nothing Then Return
        ' Nothing to say about the document either way -- a single revision went, and the
        ' document is still standing.
        If iddfDocument <= 0 AndAlso Not documentSters Then Return

        ' The write may have been for an angajament the operator has since moved off. Silence
        ' beats flipping the flag on the wrong node.
        If Not String.IsNullOrEmpty(cod) AndAlso
           Not String.Equals(cod, _currentInfo.CodAngajament, StringComparison.OrdinalIgnoreCase) Then
            Return
        End If

        Dim areAcum As Boolean = Not documentSters
        _currentInfo.IDDF = If(documentSters, CType(Nothing, Long?), CLng(iddfDocument))
        If _currentInfo.AreDDF = areAcum Then Return

        _currentInfo.AreDDF = areAcum
        ' Also re-pushes «sumar» when the active view was the one that just disappeared.
        ApplyViewGating(_currentInfo)
    End Sub
End Class
