Option Strict On
Imports System.Collections.Generic

''' <summary>
''' Rezultatul BRUT al unei prelucrări complete FOREXE, în forma în care se salvează pe
''' disc și în care se trimite la <c>POST /api/forexe/prelucrare</c>: cele cinci tabele
''' (vezi <c>WorkflowCatalog.PrelucrareCompletaTables</c>) plus scalarii citiți din
''' antetul angajamentului. POCO — fără logică, deci fără Try/Catch (regula casei).
''' </summary>
''' <remarks>
''' A stat până la felia 0048-02 în <c>KBot.App\Forexe\WorkflowResultStore.vb</c>. S-a
''' mutat aici fiindcă <c>KBot.Api</c> are nevoie de el ca să compună cererea de ingestie,
''' iar <c>KBot.Api</c> nu poate referi <c>KBot.App</c> (ar fi o referință inversă). Nu e
''' un tip nou și nu i s-a schimbat nimic — doar proiectul.
''' </remarks>
Public NotInheritable Class PrelucrareRezultat
    Public Property CodAngajament As String = String.Empty
    Public Property Moment As DateTime
    ''' <summary>Mesajul workflow-ului care l-a produs (completă vs. REVERSE).</summary>
    Public Property Workflow As String = String.Empty
    Public Property Scalari As New Dictionary(Of String, String)
    Public Property Tabele As New Dictionary(Of String, List(Of Dictionary(Of String, String)))
End Class
