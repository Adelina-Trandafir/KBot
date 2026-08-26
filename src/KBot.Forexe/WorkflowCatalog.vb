Option Strict On
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

        ' Variabilele consumate de cele două .wfl (verificate în fișiere).
        Public Const VarCodAngajament As String = "COD_ANGAJAMENT"
        Public Const VarDataIesire As String = "DATA_IESIRE"

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

        ''' <summary>Calea absolută a unui .wfl din folderul Workflows de lângă executabil.</summary>
        Public Shared Function ResolvePath(fileName As String) As String
            Return Path.Combine(KBotPaths.FolderWorkflows, fileName)
        End Function

    End Class
End Namespace
