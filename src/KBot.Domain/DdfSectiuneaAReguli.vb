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

    ''' <summary>
    ''' Is <paramref name="c"/> already on one of <paramref name="linii"/> (other than
    ''' <paramref name="exceptie"/>)? Same key (<c>IDClsf</c>), OR same classification code on the
    ''' same SS -- the nomenclator can hold the same code under several keys (one per unit), and
    ''' the operator sees one classification, not two keys (operator, 26.09.2026: the ones already
    ''' added never come back in the list).
    ''' </summary>
    Public Shared Function EsteFolosita(c As DdfClasificatie, linii As IEnumerable(Of DdfDraftLinieA),
                                        Optional exceptie As DdfDraftLinieA = Nothing) As Boolean
        If c Is Nothing OrElse linii Is Nothing Then Return False
        Dim cod As String = CodComparabil(c.Clsf)
        For Each l As DdfDraftLinieA In linii
            If l Is Nothing OrElse ReferenceEquals(l, exceptie) Then Continue For
            If c.IdClsf <> 0 AndAlso l.IdClsf = c.IdClsf Then Return True
            ' A line with no SS (written before lines carried one) matches on the code alone.
            If cod.Length > 0 AndAlso cod = CodComparabil(l.Clsf) AndAlso
               (String.IsNullOrWhiteSpace(l.Ss) OrElse AcelasiSs(c.Ss, l.Ss)) Then Return True
        Next
        Return False
    End Function

    ''' <summary>A classification code reduced to its digits, written the forexecab way (without
    ''' Access's «.02» after the chapter), so both spellings compare equal.</summary>
    Private Shared Function CodComparabil(clsf As String) As String
        Return DdfSendInputs.DigitsAndLetters(DdfSendInputs.ForexeClsf(clsf))
    End Function

    ''' <summary>
    ''' Is the classification already part of the angajament? Yes when an earlier revision gave it
    ''' an indicator code (<c>FX_DDF_REV_SA</c>) or -- for an angajament downloaded from forexecab --
    ''' when it is on <c>FX_Indicatori</c> (the server's <c>SortOrd</c> 1). The MANUAL list marks
    ''' every row <c>SortOrd</c> 1, so there only the indicator code counts. The separator never is.
    ''' </summary>
    Public Shared Function EsteInAngajament(c As DdfClasificatie, manual As Boolean) As Boolean
        If c Is Nothing OrElse c.EsteSeparator Then Return False
        If Not String.IsNullOrWhiteSpace(c.CodIndicator) Then Return True
        Return Not manual AndAlso c.SortOrd = 1
    End Function

    ''' <summary>
    ''' The list with the classifications already in the angajament first, then the ones it has not
    ''' used; each group keeps the order it came in. What the line window's combo shows.
    ''' </summary>
    Public Shared Function InAngajamentIntai(clasificatii As IEnumerable(Of DdfClasificatie),
                                             manual As Boolean) As List(Of DdfClasificatie)
        If clasificatii Is Nothing Then Return New List(Of DdfClasificatie)()
        Dim lista As List(Of DdfClasificatie) = clasificatii.Where(Function(c) c IsNot Nothing).ToList()
        Dim rezultat As List(Of DdfClasificatie) = lista.Where(Function(c) EsteInAngajament(c, manual)).ToList()
        rezultat.AddRange(lista.Where(Function(c) Not EsteInAngajament(c, manual)))
        Return rezultat
    End Function

    ''' <summary>
    ''' «Foloseste indicatorii existenti»: one section-A line for every classification already in
    ''' the angajament, CURRENT VALUE 0 -- the operator types the values and the save drops the lines
    ''' left at 0. Only the classifications on one of the program's SSs (nothing is filtered while
    ''' the map is unknown), each once, none already in section A. The line takes the angajament's
    ''' indicator code, or a fresh one when the classification has none yet (the line window's rule).
    ''' </summary>
    ''' <returns>The lines, in list order; they are NOT added to the draft.</returns>
    Public Shared Function LiniiDinIndicatori(draft As DdfDraft,
                                              clasificatii As IEnumerable(Of DdfClasificatie),
                                              surseProgram As IEnumerable(Of DdfSursaProgram),
                                              lungimeCod As Integer) As List(Of DdfDraftLinieA)
        ArgumentNullException.ThrowIfNull(draft)
        Dim rezultat As New List(Of DdfDraftLinieA)()
        If clasificatii Is Nothing Then Return rezultat

        Dim surse As List(Of DdfSursaProgram) = SurseAleProgramului(surseProgram, draft.Program)
        Dim hartaCunoscuta As Boolean = surseProgram IsNot Nothing AndAlso surseProgram.Any()
        Dim folosite As New List(Of DdfDraftLinieA)(draft.LiniiA)
        Dim coduri As New List(Of String)(draft.LiniiA.Select(Function(l) l.CodIndicator))
        Dim cuPartener As Boolean = draft.PartAng AndAlso Not String.IsNullOrWhiteSpace(draft.CodFiscal)
        Dim tempId As Integer = draft.UrmatorulTempId()

        For Each c As DdfClasificatie In clasificatii
            If Not EsteInAngajament(c, draft.Manual) Then Continue For
            If EsteFolosita(c, folosite) Then Continue For
            If hartaCunoscuta AndAlso Not surse.Any(Function(s) AcelasiSs(s.Ss, c.Ss)) Then Continue For

            Dim cod As String = If(String.IsNullOrWhiteSpace(c.CodIndicator),
                                   DdfCodIndicator.GenereazaUnic(lungimeCod, coduri),
                                   c.CodIndicator)
            coduri.Add(cod)
            Dim a As New DdfDraftLinieA() With {
                .TempId = tempId,
                .CodAngajament = draft.CodAngajament,
                .CodIndicator = cod,
                .IdClsf = c.IdClsf,
                .Clsf = c.Clsf,
                .Ss = c.Ss,
                .IdUnitate = c.IdUnitate,
                .ElementFund = c.Denumire,
                .ValPrec = c.ValPrec,
                .ValRec = c.ValRec,
                .Buget = c.Buget,
                .ValCur = 0.0R,
                .ValTot = Math.Round(c.ValPrec, 2)}
            If cuPartener Then
                a.CodPartener = draft.CodFiscal
                a.PartInd = True
            End If
            rezultat.Add(a)
            folosite.Add(a)
            tempId -= 1
        Next
        Return rezultat
    End Function

    ''' <summary>
    ''' The angajament's program(s), from its source of truth (operator, 26.09.2026):
    ''' <c>FX_Indicatori.SS</c> JOIN <c>AVACONT_COMUN.DefaProgram</c> on
    ''' <c>CONCAT(Sursa, Sectorul)</c>. Distinct, in the order met. An SS not in the map adds
    ''' nothing, so empty = the program cannot be known; more than one = the indicators span programs.
    ''' </summary>
    ''' <param name="surseIndicatori">The distinct <c>FX_Indicatori.SS</c> of the angajament, joined
    ''' by «;» or «,» (the tree node's <c>Surse</c>).</param>
    ''' <param name="defaProgram">Every row of <c>DefaProgram</c> (program, SS).</param>
    Public Shared Function ProgrameleIndicatorilor(surseIndicatori As String,
                                                   defaProgram As IEnumerable(Of DdfSursaProgram)) As List(Of String)
        Dim rezultat As New List(Of String)()
        If String.IsNullOrWhiteSpace(surseIndicatori) OrElse defaProgram Is Nothing Then Return rezultat
        Dim harta As List(Of DdfSursaProgram) = defaProgram.Where(
            Function(s) s IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(s.Program)).ToList()
        For Each ss As String In surseIndicatori.Split({";"c, ","c}, StringSplitOptions.RemoveEmptyEntries)
            For Each rand As DdfSursaProgram In harta
                If Not AcelasiSs(rand.Ss, ss) Then Continue For
                If rezultat.Any(Function(p) AcelasiProgram(p, rand.Program)) Then Continue For
                rezultat.Add(rand.Program.Trim())
            Next
        Next
        Return rezultat
    End Function

    ''' <summary>«Disponibil» (operator, 26.09.2026): budget minus receptions, 2 decimals.</summary>
    Public Shared Function Disponibil(buget As Double, valRec As Double) As Double
        Return Math.Round(buget - valRec, 2)
    End Function

    ''' <summary>«Valoare ramasa» (operator, 26.09.2026): what is left of the available amount after
    ''' the current value -- <c>Disponibil - ValCur</c>, 2 decimals. NOT the old «valoarea ramasa»
    ''' of Access (<c>ValPrec + ValCur</c>, kept on the line as <c>ValTot</c>).</summary>
    Public Shared Function ValoareRamasa(buget As Double, valRec As Double, valCur As Double) As Double
        Return Math.Round(Disponibil(buget, valRec) - valCur, 2)
    End Function

    ''' <summary>The section-A lines whose current value is 0 -- the ones the save drops.</summary>
    Public Shared Function LiniiCuValoareZero(linii As IEnumerable(Of DdfDraftLinieA)) As List(Of DdfDraftLinieA)
        If linii Is Nothing Then Return New List(Of DdfDraftLinieA)()
        Return linii.Where(Function(l) l IsNot Nothing AndAlso Math.Round(l.ValCur, 2) = 0.0R).ToList()
    End Function
End Class
