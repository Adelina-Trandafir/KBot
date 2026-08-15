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

        ''' <summary>
        ''' Prelucrarea COMPLETĂ a unui angajament (fără istoric local): antetul din .well,
        ''' indicatorii cu bugetul fiecăruia, recepțiile cu detaliul fiecăreia și istoricul.
        ''' Fișierul consumă o singură variabilă — {{COD_ANGAJAMENT}} — și scrie CINCI tabele,
        ''' nu unul (vezi WorkflowCatalog.PrelucrareCompletaTables).
        ''' </summary>
        Public Shared Function BuildPrelucrareCompleta(cod As String) As JobRequest
            If String.IsNullOrWhiteSpace(cod) Then
                Throw New ArgumentException("Codul angajamentului este obligatoriu.", NameOf(cod))
            End If

            Dim job As New JobRequest With {
                .WorkflowName = "PrelucrareCompleta",
                .WflPath = WorkflowCatalog.ResolvePath(WorkflowCatalog.PrelucrareCompletaFile)
            }
            job.Parameters(WorkflowCatalog.VarCodAngajament) = cod
            Return job
        End Function

        ''' <summary>
        ''' Varianta REVERSE, pentru un angajament care ARE deja istoric local: identică în
        ''' secțiunile 0–2, dar citește istoricul de la ULTIMA pagină înapoi și se oprește
        ''' când coloana «Timp» ajunge la <paramref name="ultimaData"/>. Oglindește exact
        ''' Access FX_Angajament_InfoComplete (DMax("DataFX", "FX_Istoric", ...)).
        ''' </summary>
        Public Shared Function BuildPrelucrareCompletaReverse(cod As String, ultimaData As Date) As JobRequest
            If String.IsNullOrWhiteSpace(cod) Then
                Throw New ArgumentException("Codul angajamentului este obligatoriu.", NameOf(cod))
            End If

            Dim job As New JobRequest With {
                .WorkflowName = "PrelucrareCompletaReverse",
                .WflPath = WorkflowCatalog.ResolvePath(WorkflowCatalog.PrelucrareCompletaReverseFile)
            }
            job.Parameters(WorkflowCatalog.VarCodAngajament) = cod
            ' Invariant, nu locale: valoarea ajunge într-o expresie regulată comparată cu
            ' textul din pagină, deci un separator schimbat de Windows ar rupe oprirea.
            job.Parameters(WorkflowCatalog.VarDataIesire) =
                ultimaData.ToString(WorkflowCatalog.DataIesireFormat,
                                    Globalization.CultureInfo.InvariantCulture)
            Return job
        End Function

    End Class
End Namespace
