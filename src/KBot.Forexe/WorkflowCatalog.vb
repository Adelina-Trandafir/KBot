Option Strict On
Imports System.Collections.Generic
Imports System.IO
Imports KBot.Common   ' KBotPaths — singurul rezolvator de căi (decizia D-O).

Namespace KBot.Forexe

    ' Catalog al workflow-urilor cunoscute + rezolvarea căii lor pe disc.
    ' .wfl-urile sunt copiate lângă executabil în folderul "Workflows" — folder pe care
    ' operatorul îl poate muta din `settings.json` (decizia D-O). Calea se rezolvă
    ' EXCLUSIV prin `ResolvePath`, deci prin `KBotPaths`.
    Public NotInheritable Class WorkflowCatalog

        Private Sub New()
        End Sub

        ' VERIFICAT: fișierul .wfl real din Workflows\ (copiat în output la publish,
        ' vezi publish-debug.ps1 §5). Workflow-ul scrie rezultatul tabelar în variabila
        ' "ListaAngajamente" (ScrapeTable saveTo), paginat cu a[rel='next'].
        Public Const ListaAngajamenteFile As String = "adlop - Lista Angajamente Curente.wfl"

        ' Workflow-ul de conectare. A fost compus de mână în DOUĂ locuri (ForexeController
        ' și ForexeConnectTest) până pe 26.08.2026; acum e o constantă, ca toate celelalte,
        ' iar calea se rezolvă prin `ResolvePath` — deci prin KBotPaths, ca orice altă cale.
        Public Const ConectareFile As String = "adlop - Conectare.wfl"

        ' Numele variabilei tabelare produse de .wfl (ScrapeTable saveTo). Cheie în
        ' JobResult.Tables după RunJobAsync.
        Public Const ListaAngajamenteTable As String = "ListaAngajamente"

        ' VERIFICAT pe fișierele reale (felia 0034): fluxul „prelucrare completă" al unui
        ' angajament. Perechea înainte/înapoi din Access FX_Angajament_InfoComplete:
        ' fără istoric local -> varianta completă; cu istoric -> varianta REVERSE, care
        ' merge înapoi prin paginile de istoric și se oprește la {{DATA_IESIRE}}.
        Public Const PrelucrareCompletaFile As String = "adlop - Prelucrare Completa.wfl"
        Public Const PrelucrareCompletaReverseFile As String = "adlop - Prelucrare Completa Reverse.wfl"

        ' VERIFIED against the real files (slice 0060): the two PARTIAL refreshes, each one a
        ' cut of the complete flow rather than a new road through the site.
        '   * Receptii  = section 0 (header) + section 2 (receptions, with their detail).
        '   * Rezervari = section 0 + section 1 (indicators + budget) + section 4 (history),
        '                 because FX_Rezervari is written FROM FX_Istoric (server steps 3c/3d),
        '                 not from a page of its own.
        ' They exist so the footer icon of each view can refresh its own family without paying
        ' for the other sections -- the reception detail alone costs one page load per
        ' reception, and that is where the minutes go.
        Public Const ReceptiiAngajamentFile As String = "adlop - Receptii Angajament.wfl"
        Public Const RezervariAngajamentFile As String = "adlop - Rezervari Angajament.wfl"

        ' Slice 0074: opens ONE angajament in «Modificare» and stops there - the preamble of
        ' the complete flow (reset, search, the row's action menu, «Modificare»), nothing read.
        ' Run by the shell's «Browser FOREXE» view whenever a node is picked in the tree, so
        ' the operator lands on the page they would otherwise have searched for by hand.
        Public Const DeschideAngajamentFile As String = "adlop - Deschide Angajament.wfl"

        ' Slice 0076: the two flows run after the operator SAVED by hand in the «Browser
        ' FOREXE» view. Both start on the page the operator is on (no reset, no search, no
        ' «Modificare») and both read the history backwards like the REVERSE flow.
        '   * Receptie Editata  = the header + ONE reception (the saved one) + the history.
        '   * Rezervari Editate = the header + the history. The indicator rows come from the
        '                         page itself, kept in memory after each save.
        ' The footer icons keep their own files above: they can be pressed with the browser
        ' anywhere, so those still search and open the angajament.
        Public Const ReceptieEditataFile As String = "adlop - Receptie Editata.wfl"
        Public Const RezervariEditateFile As String = "adlop - Rezervari Editate.wfl"

        ''' <summary>Which reception «Receptie Editata» reads: its date, or <see cref="ReceptieTintaUltima"/>.</summary>
        Public Const VarReceptieTinta As String = "RECEPTIE_TINTA"
        ''' <summary>A NEW reception: the flow reads the last row of the list.</summary>
        Public Const ReceptieTintaUltima As String = "ULTIMA"

        ''' <summary>
        ''' <c>DATA_IESIRE</c> when K-BOT has NO history of the angajament yet: a pattern that
        ''' matches nothing, so the reverse read (<c>exitIfCellEquals="Timp:~:^{{DATA_IESIRE}}"</c>)
        ''' never stops early and the whole history is read. An EMPTY value would do the
        ''' opposite - «^» matches every row, so the read would stop at the first one.
        ''' </summary>
        Public Const DataIesireNiciuna As String = "(?!)"

        ' Variabilele consumate de cele două .wfl (verificate în fișiere).
        Public Const VarCodAngajament As String = "COD_ANGAJAMENT"
        Public Const VarDataIesire As String = "DATA_IESIRE"

        ''' <summary>
        ''' The receptions the operator did NOT tick, as the workflow reads them: their dates,
        ''' comma-separated, in the site's own spelling.
        ''' </summary>
        ''' <remarks>
        ''' <para><b>Why DATES.</b> The whole pipeline already names a reception by its date: the
        ''' server matches a payload row to a stored reception with
        ''' <c>DATE(DataR) = %s AND Sters = 0</c> and takes the first candidate
        ''' (<c>step4b_receptii_prelucrare</c>), and slice 0058 named them to the operator by date
        ''' and value for the same reason. A row index would be a second, weaker identity that only
        ''' holds while the site's ordering does.</para>
        ''' <para><b>Empty means «skip nothing»</b>, and the parameter is ALWAYS sent, even empty: a
        ''' placeholder left unsubstituted would reach <c>IfVar</c> as the literal text
        ''' <c>{{RECEPTII_SARITE}}</c>, which happens to be safe here (it matches no date, so
        ''' everything is downloaded) but only by accident.</para>
        ''' </remarks>
        Public Const VarReceptiiSarite As String = "RECEPTII_SARITE"

        ''' <summary>
        ''' Formatul EXACT al lui DATA_IESIRE, copiat din Access mdl_FX_Tasks_Send:
        ''' <c>Format(lastDate, "DD\/MM\/YYYY HH\:MM\:SS")</c>. Nu e o alegere estetică —
        ''' valoarea intră într-o expresie regulată (<c>exitIfCellEquals="Timp:~:^{{DATA_IESIRE}}"</c>)
        ''' comparată cu coloana «Timp» așa cum o scrie FOREXE. Invariant, nu locale.
        ''' </summary>
        Public Const DataIesireFormat As String = "dd/MM/yyyy HH:mm:ss"

        ' Tabelele (ScrapeTable saveTo) produse de cele două fluxuri de prelucrare completă.
        ' ATENȚIE: NU e un singur tabel, cum era la ListaAngajamente — sunt cinci, plus
        ' scalari citiți cu <Read saveTo>. Vezi WorkflowResultStore, care le salvează pe toate.
        Public Shared ReadOnly PrelucrareCompletaTables As String() = {
            "TabelIndicatori", "BugetIndicator", "ListaReceptii", "Detaliu", "TabelIstoric"
        }

        ''' <summary>
        ''' Formatul EXACT in care site-ul scrie coloana «Data» din <c>ListaReceptii</c>:
        ''' <c>zz/ll/aaaa</c>. Verificat pe serverul care o citeste --
        ''' <c>fx_receptii_parse_ro_date</c> sparge dupa "/" si cere exact trei bucati.
        ''' Invariant, nu locale: valoarea intra intr-o expresie regulata comparata cu textul
        ''' din pagina, deci un separator schimbat de Windows ar rupe potrivirea.
        ''' </summary>
        Public Const DataReceptieFormat As String = "dd/MM/yyyy"

        ''' <summary>
        ''' Datele de sarit, in forma in care le citeste <c>&lt;IfVar&gt;</c> din .wfl: separate
        ''' prin virgula, fara spatii. O lista goala (sau Nothing) da sirul gol, adica «nu sari
        ''' peste niciuna».
        ''' </summary>
        Public Shared Function ListaDatelorSarite(dateSarite As IEnumerable(Of Date)) As String
            If dateSarite Is Nothing Then Return String.Empty
            Dim vazute As New List(Of String)()
            For Each d As Date In dateSarite
                Dim text As String = d.ToString(DataReceptieFormat,
                                                Globalization.CultureInfo.InvariantCulture)
                If Not vazute.Contains(text) Then vazute.Add(text)
            Next
            Return String.Join(",", vazute)
        End Function

        ''' <summary>Tabelele produse de fluxul PARTIAL de receptii.</summary>
        Public Shared ReadOnly ReceptiiAngajamentTables As String() = {
            "ListaReceptii", "Detaliu"
        }

        ''' <summary>Tabelele produse de fluxul PARTIAL de rezervari.</summary>
        Public Shared ReadOnly RezervariAngajamentTables As String() = {
            "TabelIndicatori", "BugetIndicator", "TabelIstoric"
        }

        ' Slice 0081: the SENDING workflows (KBOT -> forexecab). They live in the «Creare» sub-folder
        ' of Workflows (copied there by KBot.Forexe.vbproj), hence the folder in the name.
        Public Const CreareAngajamentFile As String = "Creare\adlop - Creare Angajament.wfl"
        Public Const IncarcaRezervareFile As String = "Creare\adlop - Incarca Rezervare.wfl"
        Public Const DefinitivareAngajamentFile As String = "Creare\adlop - Definitivare Angajament.wfl"
        Public Const DerulareAngajamentFile As String = "Creare\adlop - Derulare Angajament.wfl"

        ' Their variables (checked in the files).
        Public Const VarDescriereAngajament As String = "DESCRIERE_ANGAJAMENT"
        Public Const VarDateReceptie As String = "DATE_RECEPTIE"
        Public Const VarDateModificare As String = "DATE_MODIFICARE"
        Public Const VarCapturaInfoComplete As String = "CAPTURA_INFO_COMPLETE"
        Public Const VarMotivDefinitivare As String = "MOTIV_DEFINITIVARE"
        Public Const VarMotivDerulare As String = "MOTIV_DERULARE"
        ''' <summary>What «Creare Angajament» reads last: the new angajament's code.</summary>
        Public Const VarCodAngajamentFinal As String = "CodAng_Final"
        ''' <summary>Per-row prefix of the angajament header text «Creare Angajament» reads after each save.</summary>
        Public Const VarCodAngajamentRandPrefix As String = "CodAng_"
        ''' <summary>Per-row prefix of the row code «Incarca Rezervare» reads for a NEW row.</summary>
        Public Const VarCodIndicatorRandPrefix As String = "CodIndicator_"
        ''' <summary>The indicator table «Creare Angajament» scrapes at its end.</summary>
        Public Const TabelIndicatori As String = "TabelIndicatori"

        ''' <summary>Calea absolută a unui .wfl din folderul Workflows de lângă executabil.</summary>
        Public Shared Function ResolvePath(fileName As String) As String
            Return Path.Combine(KBotPaths.FolderWorkflows, fileName)
        End Function

    End Class
End Namespace
