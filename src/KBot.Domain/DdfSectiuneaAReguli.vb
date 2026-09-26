Option Strict On
Imports System.Collections.Generic
Imports System.Linq

''' <summary>
''' Slice 0081-09 -- one row of <c>AVACONT_COMUN.DefaProgram</c>: a program and one source /
''' sector (SS = Sursa + Sectorul, the <c>DefaSursaSector.SursaSector</c> code, e.g. «02A») it may
''' use. Read through <c>GET /api/forexe/ddf/surse-program</c>.
''' </summary>
Public NotInheritable Class DdfSursaProgram
    Public Property Program As String = String.Empty
    Public Property Ss As String = String.Empty
    ''' <summary><c>DefaSursaSector.Denumire</c>; may be empty.</summary>
    Public Property Denumire As String = String.Empty
End Class

''' <summary>
''' Slice 0081-09 -- the pure rules behind adding a line to section A: which source / sector
''' (SS) a line may take, which classifications that leaves, and when the header is complete
''' enough for lines at all. No I/O -> no Try/Catch (house rule).
'''
''' <para><b>Where the SS comes from.</b> From the document's PROGRAM, through
''' <c>AVACONT_COMUN.DefaProgram</c> (0000000000 -> 02A / 02E, 0000002510 -> 01A). The same for a
''' new angajament and for a new revision: an angajament may have lines on several SSs, as long
''' as they belong to its program. The SS chosen in K-BOT's main window is only the one proposed.</para>
''' </summary>
Public NotInheritable Class DdfSectiuneaAReguli

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Are two program codes the same program? Trimmed, and compared as numbers when both are
    ''' digits only («2510» = «0000002510»).
    ''' </summary>
    Public Shared Function AcelasiProgram(a As String, b As String) As Boolean
        Dim x As String = If(a, String.Empty).Trim()
        Dim y As String = If(b, String.Empty).Trim()
        If String.Equals(x, y, StringComparison.OrdinalIgnoreCase) Then Return True
        If x.Length = 0 OrElse y.Length = 0 Then Return False
        If x.All(AddressOf Char.IsDigit) AndAlso y.All(AddressOf Char.IsDigit) Then
            Return String.Equals(x.TrimStart("0"c), y.TrimStart("0"c), StringComparison.Ordinal)
        End If
        Return False
    End Function

    ''' <summary>Are two SS codes the same? Trimmed, case ignored.</summary>
    Public Shared Function AcelasiSs(a As String, b As String) As Boolean
        Return String.Equals(If(a, String.Empty).Trim(), If(b, String.Empty).Trim(),
                             StringComparison.OrdinalIgnoreCase)
    End Function

    ''' <summary>
    ''' The rows of <paramref name="surse"/> (all of <c>DefaProgram</c>) that belong to
    ''' <paramref name="program"/>: distinct by SS, in order. Empty when the program is empty or
    ''' not in the table.
    ''' </summary>
    Public Shared Function SurseAleProgramului(surse As IEnumerable(Of DdfSursaProgram),
                                               program As String) As List(Of DdfSursaProgram)
        Dim rezultat As New List(Of DdfSursaProgram)()
        If surse Is Nothing OrElse String.IsNullOrWhiteSpace(program) Then Return rezultat
        For Each s As DdfSursaProgram In surse
            If s Is Nothing OrElse String.IsNullOrWhiteSpace(s.Ss) Then Continue For
            If Not AcelasiProgram(s.Program, program) Then Continue For
            If rezultat.Any(Function(r) AcelasiSs(r.Ss, s.Ss)) Then Continue For
            rezultat.Add(s)
        Next
        Return rezultat
    End Function

    ''' <summary>
    ''' The classifications of one SS. An empty <paramref name="ss"/> filters nothing. The
    ''' separator row never passes.
    ''' </summary>
    Public Shared Function ClasificatiileSursei(clasificatii As IEnumerable(Of DdfClasificatie),
                                                ss As String) As List(Of DdfClasificatie)
        If clasificatii Is Nothing Then Return New List(Of DdfClasificatie)()
        Return clasificatii.Where(
            Function(c) c IsNot Nothing AndAlso Not c.EsteSeparator AndAlso
                        (String.IsNullOrWhiteSpace(ss) OrElse AcelasiSs(c.Ss, ss))).ToList()
    End Function

    ''' <summary>
    ''' The classifications of ANY of the program's SSs -- what section A can offer at all.
    ''' The separator never passes.
    ''' </summary>
    Public Shared Function ClasificatiileProgramului(clasificatii As IEnumerable(Of DdfClasificatie),
                                                     surseProgram As IEnumerable(Of DdfSursaProgram)) As List(Of DdfClasificatie)
        If clasificatii Is Nothing Then Return New List(Of DdfClasificatie)()
        Dim surse As List(Of DdfSursaProgram) = If(surseProgram, Enumerable.Empty(Of DdfSursaProgram)()).ToList()
        Return clasificatii.Where(
            Function(c) c IsNot Nothing AndAlso Not c.EsteSeparator AndAlso
                        surse.Any(Function(s) AcelasiSs(s.Ss, c.Ss))).ToList()
    End Function

    ''' <summary>
    ''' What the header of a NEW angajament still lacks before section A can take lines: the
    ''' program (it decides the SSs), the compartment, the object and -- when the document is tied
    ''' to a partner -- the partner, which every line inherits. Empty = complete. The texts are
    ''' the operator's.
    ''' </summary>
    Public Shared Function LipsuriAntet(program As String, comp As String, obiect As String,
                                        partAng As Boolean, codFiscal As String) As List(Of String)
        Dim lipsuri As New List(Of String)()
        If String.IsNullOrWhiteSpace(program) Then lipsuri.Add("programul")
        If String.IsNullOrWhiteSpace(comp) Then lipsuri.Add("compartimentul")
        If String.IsNullOrWhiteSpace(obiect) Then lipsuri.Add("obiectul")
        If partAng AndAlso String.IsNullOrWhiteSpace(codFiscal) Then lipsuri.Add("partenerul")
        Return lipsuri
    End Function

    ''' <summary>
    ''' The lines whose SS does not belong to <paramref name="program"/> -- e.g. a new
    ''' angajament's program changed after they were added. 1-based line numbers. Nothing is
    ''' checked when the map is unknown (<paramref name="surse"/> empty): no rule is better than a
    ''' wrong one.
    ''' </summary>
    Public Shared Function LiniiCuAltaSursa(linii As IEnumerable(Of DdfDraftLinieA),
                                            surse As IEnumerable(Of DdfSursaProgram),
                                            program As String) As List(Of Integer)
        Dim rezultat As New List(Of Integer)()
        If linii Is Nothing OrElse surse Is Nothing OrElse Not surse.Any() Then Return rezultat
        Dim aleProgramului As List(Of DdfSursaProgram) = SurseAleProgramului(surse, program)
        Dim nr As Integer = 0
        For Each l As DdfDraftLinieA In linii
            nr += 1
            If l Is Nothing Then Continue For
            If Not aleProgramului.Any(Function(s) AcelasiSs(s.Ss, l.Ss)) Then rezultat.Add(nr)
        Next
        Return rezultat
    End Function
End Class
