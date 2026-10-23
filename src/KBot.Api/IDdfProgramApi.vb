Option Strict On
Imports System.Collections.Generic
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Domain

''' <summary>
''' Slice 0081-09: the program -&gt; source / sector map of section A
''' (<c>GET /api/forexe/ddf/surse-program</c>, <c>AVACONT_COMUN.DefaProgram</c>). Kept out of
''' <see cref="IApiClient"/> like <see cref="IDdfSendApi"/>, so the nine test fakes of the big
''' interface need not learn it; the editor asks for it with <c>TryCast</c>. Throws
''' <see cref="ApiException"/> on a non-2xx, carrying the server's Romanian text.
''' </summary>
Public Interface IDdfProgramApi
    ''' <summary>Every (program, SS, caption) row; the caller keeps its program's.</summary>
    Function GetDdfSurseProgramAsync(ct As CancellationToken) As Task(Of List(Of DdfSursaProgram))

    ''' <summary>
    ''' Slice 0114-27: <c>POST /api/forexe/ddf/repara-program</c> -- puts the program of <paramref name="cod"/>'s document
    ''' right (the one of its indicators' SS: 01A -> 0000002510, otherwise 0000000000). <paramref name="apply"/> = False is the
    ''' dry run (nothing is written); True writes <c>FX_DDF.Program</c> only.
    ''' </summary>
    Function RepairDdfProgramAsync(cod As String, apply As Boolean, ct As CancellationToken) As Task(Of DdfProgramRepairResult)
End Interface
