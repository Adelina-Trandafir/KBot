Option Strict On
Imports System.Collections.Generic
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
        Public Shared Function BuildPrelucrareCompleta(
                cod As String,
                Optional receptiiSarite As IEnumerable(Of Date) = Nothing) As JobRequest
            If String.IsNullOrWhiteSpace(cod) Then
                Throw New ArgumentException("Codul angajamentului este obligatoriu.", NameOf(cod))
            End If

            Dim job As New JobRequest With {
                .WorkflowName = "PrelucrareCompleta",
                .WflPath = WorkflowCatalog.ResolvePath(WorkflowCatalog.PrelucrareCompletaFile)
            }
            job.Parameters(WorkflowCatalog.VarCodAngajament) = cod
            PuneReceptiileSarite(job, receptiiSarite)
            Return job
        End Function

        ''' <summary>
        ''' Varianta REVERSE, pentru un angajament care ARE deja istoric local: identică în
        ''' secțiunile 0–2, dar citește istoricul de la ULTIMA pagină înapoi și se oprește
        ''' când coloana «Timp» ajunge la <paramref name="ultimaData"/>. Oglindește exact
        ''' Access FX_Angajament_InfoComplete (DMax("DataFX", "FX_Istoric", ...)).
        ''' </summary>
        Public Shared Function BuildPrelucrareCompletaReverse(
                cod As String, ultimaData As Date,
                Optional receptiiSarite As IEnumerable(Of Date) = Nothing) As JobRequest
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
            PuneReceptiileSarite(job, receptiiSarite)
            Return job
        End Function

        ''' <summary>
        ''' Reimprospatarea PARTIALA a receptiilor unui angajament (felia 0060) —
        ''' «adlop - Receptii Angajament.wfl», iconita din dreapta subsolului arborelui de
        ''' receptii. Acelasi drum prin site ca sectiunea 2 a prelucrarii complete, fara
        ''' indicatori si fara istoric.
        ''' </summary>
        ''' <param name="receptiiSarite">
        ''' Datele receptiilor pe care operatorul NU le-a bifat. Nothing sau lista goala = se
        ''' descarca toate.
        ''' </param>
        Public Shared Function BuildReceptiiAngajament(
                cod As String,
                Optional receptiiSarite As IEnumerable(Of Date) = Nothing) As JobRequest
            If String.IsNullOrWhiteSpace(cod) Then
                Throw New ArgumentException("Codul angajamentului este obligatoriu.", NameOf(cod))
            End If

            Dim job As New JobRequest With {
                .WorkflowName = "ReceptiiAngajament",
                .WflPath = WorkflowCatalog.ResolvePath(WorkflowCatalog.ReceptiiAngajamentFile)
            }
            job.Parameters(WorkflowCatalog.VarCodAngajament) = cod
            PuneReceptiileSarite(job, receptiiSarite)
            Return job
        End Function

        ''' <summary>
        ''' Reimprospatarea PARTIALA a rezervarilor (felia 0060) —
        ''' «adlop - Rezervari Angajament.wfl», iconita din dreapta subsolului arborelui de
        ''' rezervari. Antet + indicatori + istoric: FX_Rezervari se scrie pe server DIN
        ''' FX_Istoric (pasii 3c/3d), deci istoricul E sursa rezervarilor.
        ''' </summary>
        ''' <remarks>
        ''' Nu ia <c>DATA_IESIRE</c>: fluxul citeste istoricul INAINTE, nu in REVERSE. Motivul
        ''' e scris in .wfl — o data lipsa ar face oprirea sa se potriveasca cu primul rand.
        ''' </remarks>
        Public Shared Function BuildRezervariAngajament(cod As String) As JobRequest
            If String.IsNullOrWhiteSpace(cod) Then
                Throw New ArgumentException("Codul angajamentului este obligatoriu.", NameOf(cod))
            End If

            Dim job As New JobRequest With {
                .WorkflowName = "RezervariAngajament",
                .WflPath = WorkflowCatalog.ResolvePath(WorkflowCatalog.RezervariAngajamentFile)
            }
            job.Parameters(WorkflowCatalog.VarCodAngajament) = cod
            Return job
        End Function

        ''' <summary>
        ''' «adlop - Deschide Angajament.wfl» (slice 0074): search the angajament and open it in
        ''' «Modificare», then stop. No tables come back; the run either lands on the page or
        ''' fails (an &lt;Exit&gt; when the code is not in the FOREXE list).
        ''' </summary>
        Public Shared Function BuildDeschideAngajament(cod As String) As JobRequest
            If String.IsNullOrWhiteSpace(cod) Then
                Throw New ArgumentException("Codul angajamentului este obligatoriu.", NameOf(cod))
            End If

            Dim job As New JobRequest With {
                .WorkflowName = "DeschideAngajament",
                .WflPath = WorkflowCatalog.ResolvePath(WorkflowCatalog.DeschideAngajamentFile)
            }
            job.Parameters(WorkflowCatalog.VarCodAngajament) = cod
            Return job
        End Function

        ''' <summary>
        ''' Pune <c>RECEPTII_SARITE</c> pe lucrare — INTOTDEAUNA, chiar si goala.
        ''' </summary>
        ''' <remarks>
        ''' Un parametru nedat lasa in XML chiar textul <c>{{RECEPTII_SARITE}}</c>, fiindca
        ''' substitutia se face prin <c>WorkflowParser.ApplyVariables</c> peste continutul
        ''' fisierului. <c>IfVar</c> l-ar citi atunci ca valoare literala si nu s-ar potrivi cu
        ''' nicio data, deci s-ar descarca tot — corect, dar din intamplare. Aici se trimite
        ''' explicit sirul gol, ca «nu sari peste niciuna» sa fie o valoare, nu un accident.
        ''' </remarks>
        Private Shared Sub PuneReceptiileSarite(job As JobRequest,
                                                receptiiSarite As IEnumerable(Of Date))
            job.Parameters(WorkflowCatalog.VarReceptiiSarite) =
                WorkflowCatalog.ListaDatelorSarite(receptiiSarite)
        End Sub

    End Class
End Namespace
