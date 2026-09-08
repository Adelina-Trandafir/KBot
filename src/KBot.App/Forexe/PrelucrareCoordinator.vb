Option Strict On
Imports System.Collections.Generic
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain

''' <summary>
''' Drumul dus-întors al ingestiei FOREXE: trimite, iar dacă serverul răspunde că o
''' clasificație se potrivește cu mai multe unități, întreabă operatorul și trimite din nou
''' ACELEAȘI date, cu alegerile atașate.
''' </summary>
''' <remarks>
''' <para>Serverul NU ține nimic între cele două încercări — la 409 tranzacția e derulată
''' înapoi și nu s-a scris nimic —, deci a doua cerere poartă tot pachetul, nu un jeton de
''' reluare. De-asta clasa asta ține pachetul și îl retrimite, în loc să ceară serverului
''' să continue.</para>
''' <para>Frontieră UI: deschide un dialog modal, deci trebuie chemată de pe firul UI.</para>
''' <para>ATENȚIE — în felia 0048-02 NIMIC nu cheamă încă această clasă. Pașii 3–8 ai
''' ingestiei nu sunt portați, iar postarea automată a unei conducte pe jumătate la fiecare
''' descărcare nu e o decizie de luat pe nevăzute. Legarea în
''' <c>ForexeController.DownloadNodeAsync</c> (§9 din plan) vine odată cu restul pașilor.</para>
''' </remarks>
Public NotInheritable Class PrelucrareCoordinator

    ''' <summary>
    ''' Câte runde de întrebări se acceptă înainte de a considera că serverul se învârte în
    ''' cerc. În mod normal e nevoie de UNA: serverul adună TOATE perechile ambigue într-o
    ''' singură trecere, înainte de orice scriere. O a doua rundă ar însemna că a apărut o
    ''' ambiguitate nouă între cele două cereri; a zecea ar însemna o buclă.
    ''' </summary>
    Public Const MaxRunde As Integer = 5

    Private ReadOnly _api As IApiClient
    Private ReadOnly _intreaba As Func(Of AlegereNecesara, String, Integer, Integer, AlegereUnitate)

    ''' <param name="api">Clientul HTTP.</param>
    ''' <param name="intreaba">
    ''' Cum se pune întrebarea: (întrebare, cod angajament, a câta, din câte) → răspuns, sau
    ''' Nothing dacă operatorul a renunțat. Nothing aici înseamnă «deschide
    ''' <see cref="AlegereUnitateForm"/>», adică exact ce vrea aplicația reală. Parametrul
    ''' există fiindcă <c>ShowDialog</c> e modal: fără acest cui, bucla de mai jos nu s-ar
    ''' putea verifica decât deschizând ferestre pe ecranul operatorului în timpul testelor.
    ''' Același tipar ca <c>citesteIstoric</c> din <c>ForexeController.DownloadNodeAsync</c>.
    ''' </param>
    Public Sub New(api As IApiClient,
                   Optional intreaba As Func(Of AlegereNecesara, String, Integer, Integer, AlegereUnitate) = Nothing)
        ArgumentNullException.ThrowIfNull(api)
        _api = api
        _intreaba = If(intreaba, AddressOf DeschideDialogul)
    End Sub

    ' Calea reală: dialogul modal. Owner-ul se ia din formularul activ — coordonatorul nu
    ' ține o fereastră, iar la momentul întrebării cea activă e shell-ul.
    Private Shared Function DeschideDialogul(necesara As AlegereNecesara, cod As String,
                                             pozitie As Integer, total As Integer) As AlegereUnitate
        Using dlg As New AlegereUnitateForm(necesara, cod, pozitie, total)
            If dlg.ShowDialog(Form.ActiveForm) <> DialogResult.OK Then Return Nothing
            Return dlg.Rezultat
        End Using
    End Function

    ' NU EXISTĂ O TRIMITERE ÎNTR-O SINGURĂ FAZĂ (felia 0055). A existat una — `TrimiteAsync`,
    ' din felia 0048-02, scrisă înainte ca ruta să capete cele două faze — și era o capcană:
    ' clientul ei nu punea deloc câmpul «mod», iar serverul citește lipsa lui ca «propunere»
    ' («Tăcerea nu are voie să însemne „salvează"»). Rula deci toți pașii, derula tranzacția
    ' înapoi și răspundea 200; clientul citea acel 200 ca `PrelucrareStare.Salvat` și raporta
    ' o salvare care nu se întâmplase. S-a retras cu tot cu `IApiClient.TrimitePrelucrareAsync`.
    ' Drumul întreg trece azi prin cele două metode de mai jos și prin `AsociereForm`.

    ''' <summary>
    ''' FAZA UNU (felia 0048-03): cere propunerea, întrebând operatorul ori de câte ori
    ''' serverul are nevoie de o alegere de unitate — exact aceeași buclă ca
    ''' <see cref="TrimiteAsync"/>, fiindcă 409 ALEGERE_UNITATE se poate declanșa și aici.
    '''
    ''' <para>Rezultatul poartă și alegerile făcute, în <paramref name="alegeriFacute"/>:
    ''' ele trebuie RETRIMISE la salvare. Bifa «nu mă mai întreba» se scrie în
    ''' <c>FX_Alegeri_Unitate</c> înăuntrul tranzacției, deci se derulează înapoi împreună
    ''' cu propunerea, iar serverul nu și-o amintește. Fără ele, faza de salvare ar primi
    ''' din nou 409 pentru o întrebare la care operatorul a răspuns deja.</para>
    '''
    ''' <para>Nimic din felia asta nu cheamă metoda din fluxul de descărcare — se exercită
    ''' din <c>KBot.DevHarness</c>. Legarea vine în 0048-04, împreună cu formularul.</para>
    ''' </summary>
    ''' <returns>Propunerea, sau Nothing dacă operatorul a renunțat la o întrebare.</returns>
    Public Async Function CerePropunereAsync(rezultat As PrelucrareRezultat,
                                             alegeriFacute As List(Of AlegereUnitate),
                                             ct As CancellationToken) As Task(Of PrelucrarePropunere)
        Try
            ArgumentNullException.ThrowIfNull(rezultat)
            ArgumentNullException.ThrowIfNull(alegeriFacute)

            For runda As Integer = 1 To MaxRunde
                ' Fără ConfigureAwait(False): continuarea trebuie să se întoarcă pe firul
                ' UI, altfel ShowDialog de mai jos ar rula pe fir greșit.
                Dim raspuns As PrelucrareRaspuns =
                    Await _api.CerePropunereAsync(rezultat, alegeriFacute, ct)

                If raspuns Is Nothing Then Return Nothing
                If raspuns.Stare = PrelucrareStare.Propunere Then Return raspuns.Propunere

                Dim raspunsuriNoi As List(Of AlegereUnitate) =
                    IntreabaOperatorul(raspuns, rezultat.CodAngajament)
                ' Nothing = a renunțat. Nimic nu s-a scris — propunerea oricum nu scrie.
                If raspunsuriNoi Is Nothing Then Return Nothing
                alegeriFacute.AddRange(raspunsuriNoi)
            Next

            Throw New InvalidOperationException(
                $"Propunerea a cerut alegerea unității de mai mult de {MaxRunde} ori pentru " &
                $"«{rezultat.CodAngajament}». S-a oprit; nu s-a scris nimic.")
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("PrelucrareCoordinator.CerePropunereAsync", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' FAZA DOI: retrimite ACELAȘI pachet, cu amprenta și hotărârile, iar serverul comite —
    ''' cu aceeași buclă de întrebări ca faza întâi.
    '''
    ''' <para><b>De ce are și ea buclă.</b> 409 ALEGERE_UNITATE se poate declanșa și la
    ''' salvare: pașii rulează din nou, iar între cele două faze poate să fi apărut o
    ''' clasificație nouă. Fără bucla asta, apelantul ar primi un răspuns cu
    ''' <c>Stare = AlegereUnitate</c> pe care l-ar lua drept succes — adică exact minciuna
    ''' pentru care s-a retras <c>TrimiteAsync</c>.</para>
    '''
    ''' <para>Alegerile se adaugă în <paramref name="alegeriFacute"/>, aceeași listă care a
    ''' trecut prin faza întâi: bifa «nu mă mai întreba» s-a derulat înapoi cu propunerea,
    ''' deci serverul n-o ține minte.</para>
    ''' </summary>
    ''' <returns>
    ''' Răspunsul de succes, sau <c>Nothing</c> dacă operatorul a renunțat la o întrebare —
    ''' caz în care NU s-a scris nimic: ultimul răspuns al serverului a fost un 409 cu
    ''' tranzacția deja derulată înapoi.
    ''' </returns>
    Public Async Function SalveazaAsync(rezultat As PrelucrareRezultat,
                                        amprenta As String,
                                        decizii As IReadOnlyList(Of DecizieAsociere),
                                        alegeriFacute As List(Of AlegereUnitate),
                                        ct As CancellationToken) As Task(Of PrelucrareRaspuns)
        Try
            ArgumentNullException.ThrowIfNull(rezultat)
            ArgumentNullException.ThrowIfNull(decizii)
            ArgumentNullException.ThrowIfNull(alegeriFacute)

            For runda As Integer = 1 To MaxRunde
                ' Fără ConfigureAwait(False): continuarea trebuie să se întoarcă pe firul
                ' UI, altfel ShowDialog de mai jos ar rula pe fir greșit.
                Dim raspuns As PrelucrareRaspuns =
                    Await _api.SalveazaAsociereaAsync(rezultat, amprenta, decizii, alegeriFacute, ct)

                If raspuns Is Nothing OrElse raspuns.Stare <> PrelucrareStare.AlegereUnitate Then
                    Return raspuns
                End If

                Dim raspunsuriNoi As List(Of AlegereUnitate) =
                    IntreabaOperatorul(raspuns, rezultat.CodAngajament)
                ' Nothing = a renunțat. Nu se retrimite nimic; nimic nu s-a scris.
                If raspunsuriNoi Is Nothing Then Return Nothing
                alegeriFacute.AddRange(raspunsuriNoi)
            Next

            Throw New InvalidOperationException(
                $"Salvarea a cerut alegerea unității de mai mult de {MaxRunde} ori pentru " &
                $"«{rezultat.CodAngajament}». S-a oprit; nu s-a scris nimic.")
        Catch ex As ApiException
            ' Excepție tipată, tratată de apelant (401 -> WithReauth, STARE_MODIFICATA ->
            ' mesajul propriu al formularului): control-flow.
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("PrelucrareCoordinator.SalveazaAsync", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Deschide dialogul o dată pentru fiecare întrebare. Întoarce Nothing la prima
    ''' renunțare — o alegere lipsă nu se poate compensa, iar restul întrebărilor nu mai au
    ''' rost dacă salvarea oricum nu va porni.
    ''' </summary>
    Private Function IntreabaOperatorul(raspuns As PrelucrareRaspuns,
                                        cod As String) As List(Of AlegereUnitate)
        Dim rezultate As New List(Of AlegereUnitate)()
        Dim total As Integer = raspuns.AlegeriNecesare.Count
        For i As Integer = 0 To total - 1
            Dim ales As AlegereUnitate = _intreaba(raspuns.AlegeriNecesare(i), cod, i + 1, total)
            If ales Is Nothing Then Return Nothing
            rezultate.Add(ales)
        Next
        Return rezultate
    End Function

End Class
