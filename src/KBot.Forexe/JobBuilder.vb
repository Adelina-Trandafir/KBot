Option Strict On
Imports KBot.Common

Namespace KBot.Forexe

    ' Construiește JobRequest-uri din SessionContext (înlocuiește clsJobTask/AddVariable
    ' din VBA — în arhitectura nouă "task"-ul este JobRequest, iar variabilele merg în
    ' JobRequest.Parameters). Atributul receive="true" al workflow-ului trăiește în .wfl,
    ' nu în JobRequest (parsat ca Workflow.Receive).
    Public NotInheritable Class JobBuilder

        Private Sub New()
        End Sub

        ''' <summary>
        ''' Job ListaAngajamente cu cele 4 variabile verificate
        ''' (FX_ListaAngajamente_Descarcare / mdl_FX_Tasks_Send):
        ''' DATA_INCEPUT/DATA_SFARSIT (dd.MM.yyyy, fără conversie de locale),
        ''' COD_PROGRAM, SURSA — toate din SessionContext.
        ''' </summary>
        Public Shared Function BuildListaAngajamente(session As SessionContext) As JobRequest
            If session Is Nothing Then Throw New ArgumentNullException(NameOf(session))

            Dim an As String = session.An.ToString(Globalization.CultureInfo.InvariantCulture)

            Dim job As New JobRequest With {
                .WorkflowName = "ListaAngajamente",
                .WflPath = WorkflowCatalog.ResolvePath(WorkflowCatalog.ListaAngajamenteFile)
            }
            job.Parameters("DATA_INCEPUT") = "01.01." & an
            job.Parameters("DATA_SFARSIT") = "31.12." & an
            job.Parameters("COD_PROGRAM") = session.CodProgram
            job.Parameters("SURSA") = session.SectorSursa
            Return job
        End Function

    End Class
End Namespace
