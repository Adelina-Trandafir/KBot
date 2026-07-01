Option Strict On
Imports System.IO

Namespace KBot.Forexe

    ' Catalog al workflow-urilor cunoscute + rezolvarea căii lor pe disc.
    ' .wfl-urile sunt copiate lângă executabil în folderul "Workflows"
    ' (vezi MainForm.btnConnect_Click care rezolvă "adlop - Conectare.wfl" la fel).
    Public NotInheritable Class WorkflowCatalog

        Private Sub New()
        End Sub

        ' VERIFICAT: fișierul .wfl real din Workflows\ (copiat în output la publish,
        ' vezi publish-debug.ps1 §5). Workflow-ul scrie rezultatul tabelar în variabila
        ' "ListaAngajamente" (ScrapeTable saveTo), paginat cu a[rel='next'].
        Public Const ListaAngajamenteFile As String = "adlop - Lista Angajamente Curente.wfl"

        ' Numele variabilei tabelare produse de .wfl (ScrapeTable saveTo). Cheie în
        ' JobResult.Tables după RunJobAsync.
        Public Const ListaAngajamenteTable As String = "ListaAngajamente"

        ''' <summary>Calea absolută a unui .wfl din folderul Workflows de lângă executabil.</summary>
        Public Shared Function ResolvePath(fileName As String) As String
            Return Path.Combine(AppContext.BaseDirectory, "Workflows", fileName)
        End Function

    End Class
End Namespace
