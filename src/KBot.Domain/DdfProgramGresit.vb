' POCOs for slice 0114-27: the program of an angajament's DDF header does not match the program of the SS saved in its data.
' Pure models (no I/O) -> no Try/Catch (house rule: simple POCOs).

''' <summary>Where the data disagree with the DDF header: the indicators, a saved DDF revision or a saved ordonantare.</summary>
Public Enum DdfProgramSursa
    ''' <summary>The SS of the angajament's indicators (<c>FX_Indicatori.SS</c>).</summary>
    Indicatori = 0
    ''' <summary>The lines of a saved DDF revision (<c>FX_DDF_REV_SA.SS</c>).</summary>
    Revizie = 1
    ''' <summary>The lines of a saved ordonantare (<c>FX_ORD_TBL.CodSSI</c>, first three characters).</summary>
    Ordonantare = 2
End Enum

''' <summary>One place whose SS calls for another program than the DDF header holds.</summary>
Public NotInheritable Class DdfProgramDovada
    Public Property Sursa As DdfProgramSursa

    ''' <summary>The revision number or the ordonantare number; empty for the indicators.</summary>
    Public Property Numar As String = String.Empty

    ''' <summary>The SS found there (the indicators: all of them, joined by «,»).</summary>
    Public Property Ss As String = String.Empty

    ''' <summary>The program that SS calls for; empty when the SSs span several programs.</summary>
    Public Property Program As String = String.Empty
End Class

''' <summary>
''' An angajament whose <c>FX_DDF.Program</c> (the DDF header) differs from the program of the SS saved in its indicators, its DDF
''' revisions or its ordonantari (slice 0114-27). The rule is <c>01A</c> -> <c>0000002510</c>, anything else -> <c>0000000000</c>. Comes
''' from the tree route (<c>ProgramGresit</c>, so the main tree can colour the row and say what is wrong) and from the repair route.
''' </summary>
Public NotInheritable Class DdfProgramGresit
    ''' <summary>The program the DDF header holds now (may be empty).</summary>
    Public Property Actual As String = String.Empty

    ''' <summary>The one correct program; empty when the data call for more than one (nothing is guessed).</summary>
    Public Property Asteptat As String = String.Empty

    ''' <summary>Every place that disagrees with the header.</summary>
    Public ReadOnly Property Dovezi As New List(Of DdfProgramDovada)()
End Class

''' <summary>Result of putting the program of one angajament's DDF header right (same shape for the dry run and the real run).</summary>
Public NotInheritable Class DdfProgramRepairResult
    Public Property Cod As String = String.Empty

    ''' <summary>False = dry run: nothing was written.</summary>
    Public Property Applied As Boolean

    ''' <summary>1 when the header is wrong and can be put right, 0 otherwise.</summary>
    Public Property ToRepair As Integer

    ''' <summary>1 when it was written (0 on a dry run).</summary>
    Public Property Repaired As Integer

    Public Property Current As String = String.Empty
    Public Property Expected As String = String.Empty
    Public Property Sources As String = String.Empty
    Public ReadOnly Property Details As New List(Of DdfProgramDovada)()
    Public ReadOnly Property Warnings As New List(Of String)()
End Class
