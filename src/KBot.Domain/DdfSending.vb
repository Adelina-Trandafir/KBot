Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Text

' Slice 0081 -- the pure pieces of the SENDING flow (KBOT -> forexecab): how a new draft is
' built for the two new entry points, what the Rezervari footer menu offers, and (0081-04)
' what the workflows are fed. No I/O -> no Try/Catch (house rule).

''' <summary>
''' The drafts the two new entry points open the editor on (slice 0081-02). Built on the
''' client: nothing is proposed by the server, because nothing exists yet to propose from.
''' </summary>
Public NotInheritable Class DdfDraftFactory

    ''' <summary><c>FX_DDF_REV.Tip</c> of every revision K-BOT writes by hand -- Access's
    ''' <c>FX_Adaugare_ANG</c> used the same literal.</summary>
    Public Const ManualRevisionType As String = "Manual"
    ''' <summary><c>FX_DDF.Stare</c> / <c>FX_Angajamente.Stare</c> of an angajament that exists
    ''' only in K-BOT (Access: <c>rcDDF!Stare = "MANUAL"</c>).</summary>
    Public Const ManualAngajamentState As String = "MANUAL"
    ''' <summary>Length of the random part of a manual angajament code
    ''' (Access: <c>"!" &amp; GenerateUniqueSequence(10)</c>).</summary>
    Public Const ManualCodeLength As Integer = 10

    Private Sub New()
    End Sub

    ''' <summary>A fresh manual angajament code: «!» + ten characters, as Access minted it.</summary>
    Public Shared Function NewManualCode() As String
        Return "!" & DdfCodIndicator.Genereaza(ManualCodeLength)
    End Function

    ''' <summary>
    ''' «Angajament nou» -- the port of Access's <c>FX_Adaugare_ANG</c>: a new document on a new,
    ''' K-BOT-only angajament (code «!…»), revision 0, section A empty. The save creates the
    ''' <c>FX_Angajamente</c> / <c>FX_Indicatori</c> rows (its Manual branch); forexecab hears of
    ''' it only at «Trimite in FOREXE».
    ''' </summary>
    Public Shared Function ForNewAngajament(cod As String, dc As String, program As String,
                                            azi As Date) As DdfDraft
        If String.IsNullOrWhiteSpace(cod) OrElse Not cod.StartsWith("!", StringComparison.Ordinal) Then
            Throw New ArgumentException("A new angajament code must start with '!'.", NameOf(cod))
        End If
        Dim d As New DdfDraft() With {
            .CodAngajament = cod,
            .Dc = If(dc, String.Empty),
            .Program = If(program, String.Empty),
            .DataCreare = azi.Date,
            .Stare = ManualAngajamentState,
            .Manual = True,
            .Incarcat = False,
            .Preluat = False,
            .Buget = False,
            .Nou = True,
            .RevizieNoua = True,
            .AngajamentNou = True}
        d.Revizie = New DdfDraftRevizie() With {
            .CodAngajament = cod,
            .NumarRev = 0,
            .DataRev = azi.Date,
            .Tip = ManualRevisionType}
        Return d
    End Function

    ''' <summary>
    ''' «Adauga rezervare» on an angajament that already has a document: a NEW revision on the
    ''' same header, section A EMPTY -- the operator adds the lines, and the classification list
    ''' brings each one's previous value. <paramref name="ultima"/> is the last revision, read
    ''' through the editor's own route; only its header is kept.
    ''' </summary>
    Public Shared Function ForAddedReservation(ultima As DdfDraft, azi As Date) As DdfDraft
        ArgumentNullException.ThrowIfNull(ultima)
        ' «Manual» drives the save's branch that writes FX_Angajamente / FX_Indicatori from
        ' section A. Once forexecab has given the angajament its real code (0081-04), those rows
        ' belong to the import: a later revision must not overwrite them with its deltas.
        ' FX_DDF.Manual itself stays 1 (the update of an existing header does not touch it).
        Dim d As New DdfDraft() With {
            .Iddf = ultima.Iddf,
            .Cual = ultima.Cual,
            .CodAngajament = ultima.CodAngajament,
            .Comp = ultima.Comp,
            .Salarii = ultima.Salarii,
            .DataCreare = ultima.DataCreare,
            .Dc = ultima.Dc,
            .Program = ultima.Program,
            .DataDef = ultima.DataDef,
            .Incarcat = ultima.Incarcat,
            .Preluat = ultima.Preluat,
            .Buget = ultima.Buget,
            .Manual = ultima.Manual AndAlso ultima.CodAngajament.StartsWith("!", StringComparison.Ordinal),
            .ObiectDdf = ultima.ObiectDdf,
            .Stare = ultima.Stare,
            .PartAng = ultima.PartAng,
            .CodFiscal = ultima.CodFiscal,
            .NumePartener = ultima.NumePartener,
            .Nou = False,
            .RevizieNoua = True}
        d.Revizie = New DdfDraftRevizie() With {
            .Iddf = ultima.Iddf,
            .CodAngajament = ultima.CodAngajament,
            .NumarRev = ultima.Revizie.NumarRev + 1,
            .DataRev = azi.Date,
            .Tip = ManualRevisionType}
        Return d
    End Function

    ''' <summary>
    ''' «Adauga rezervare» on an angajament forexecab has but K-BOT holds no document for: its
    ''' revision 0 (the parent document's S3, «carried over»), header from <c>FX_Angajamente</c>
    ''' as <c>/genereaza</c> builds it for a first revision.
    ''' </summary>
    Public Shared Function ForFirstRevisionOfExisting(cod As String, dc As String, program As String,
                                                      descriere As String, dataCreare As Date?,
                                                      stare As String, azi As Date) As DdfDraft
        If String.IsNullOrWhiteSpace(cod) Then Throw New ArgumentException("The angajament code is missing.", NameOf(cod))
        Dim d As New DdfDraft() With {
            .CodAngajament = cod,
            .Dc = If(dc, String.Empty),
            .Program = If(program, String.Empty),
            .DataCreare = If(dataCreare, azi.Date),
            .ObiectDdf = If(descriere, String.Empty),
            .Stare = If(stare, String.Empty),
            .Manual = cod.StartsWith("!", StringComparison.Ordinal),
            .Incarcat = True,
            .Preluat = True,
            .Buget = True,
            .Nou = True,
            .RevizieNoua = True}
        d.Revizie = New DdfDraftRevizie() With {
            .CodAngajament = cod,
            .NumarRev = 0,
            .DataRev = azi.Date,
            .Tip = ManualRevisionType}
        Return d
    End Function
End Class

''' <summary>An angajament's state in forexecab, read from <c>FX_Angajamente.Stare</c>.</summary>
Public Enum ForexeAngajamentState
    Unknown = 0
    ''' <summary>K-BOT-only (code «!…» / «MANUAL»): forexecab does not know it yet.</summary>
    NotInForexe = 1
    ''' <summary>«Initial»: created, not definitivat.</summary>
    Initial = 2
    ''' <summary>«In definitivare».</summary>
    InDefinitivare = 3
    ''' <summary>«In derulare».</summary>
    InDerulare = 4
    ''' <summary>«Anulat» / «Reziliat» / «Suspendat»: nothing more is done on it.</summary>
    Closed = 5
End Enum

''' <summary>The one option the Rezervari footer menu shows (plan 0081-04, the menu table).</summary>
Public Enum RezervariMenuOption
    None = 0
    Definitiveaza = 1
    Deruleaza = 2
    GenereazaPdfFinal = 3
    AdaugaRezervare = 4
End Enum

''' <summary>
''' The Rezervari tree footer LEFT icon: which single option it offers (operator, 25.09.2026).
''' Two rules decide it -- one open revision at a time, and variant (a): a new angajament's
''' Rev 0 gets its final PDF only after «Deruleaza».
''' </summary>
Public NotInheritable Class RezervariMenu

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Reads <c>FX_Angajamente.Stare</c>. The text is compared without diacritics and case, so
    ''' «In derulare», «In derulare» and «IN DERULARE» are one state.
    ''' </summary>
    Public Shared Function ParseState(stare As String, cod As String) As ForexeAngajamentState
        If Not String.IsNullOrEmpty(cod) AndAlso cod.StartsWith("!", StringComparison.Ordinal) Then
            Return ForexeAngajamentState.NotInForexe
        End If
        Dim t As String = Plain(stare)
        If t.Length = 0 Then Return ForexeAngajamentState.Unknown
        If t.Contains("manual") Then Return ForexeAngajamentState.NotInForexe
        If t.Contains("anulat") OrElse t.Contains("reziliat") OrElse t.Contains("suspendat") Then
            Return ForexeAngajamentState.Closed
        End If
        If t.Contains("definitivare") Then Return ForexeAngajamentState.InDefinitivare
        If t.Contains("derulare") Then Return ForexeAngajamentState.InDerulare
        If t.Contains("initial") Then Return ForexeAngajamentState.Initial
        Return ForexeAngajamentState.Unknown
    End Function

    ' Lower case, no diacritics, trimmed.
    Private Shared Function Plain(text As String) As String
        If String.IsNullOrWhiteSpace(text) Then Return String.Empty
        Dim decomposed As String = text.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD)
        Dim sb As New StringBuilder(decomposed.Length)
        For Each ch As Char In decomposed
            If CharUnicodeInfo.GetUnicodeCategory(ch) <> UnicodeCategory.NonSpacingMark Then sb.Append(ch)
        Next
        Return sb.ToString().Normalize(NormalizationForm.FormC)
    End Function

    ''' <summary>
    ''' The option for one angajament.
    ''' </summary>
    ''' <param name="state">The angajament's forexecab state.</param>
    ''' <param name="documentManual">The document was created in K-BOT («Angajament nou»):
    ''' <c>FX_DDF.Manual</c>. Only such a document's Rev 0 gets «Definitiveaza» / «Deruleaza».</param>
    ''' <param name="revizii">Every revision of the document (may be empty).</param>
    Public Shared Function Decide(state As ForexeAngajamentState, documentManual As Boolean,
                                  revizii As IEnumerable(Of RevizieRow)) As RezervariMenuOption
        If state = ForexeAngajamentState.Closed OrElse state = ForexeAngajamentState.NotInForexe OrElse
           state = ForexeAngajamentState.Unknown Then
            Return RezervariMenuOption.None
        End If

        Dim lista As List(Of RevizieRow) = If(revizii, Enumerable.Empty(Of RevizieRow)()).ToList()
        Dim deschisa As RevizieRow = lista.Where(Function(r) DdfRevisionStates.IsOpen(r.Stare)).
                                           OrderByDescending(Function(r) r.NumarRev).
                                           FirstOrDefault()
        If deschisa IsNot Nothing Then
            ' S0 / S1 / S1x: finish it from the DDF, nothing here.
            If deschisa.Stare <> DdfRevisionState.SentInProgress Then Return RezervariMenuOption.None
            If documentManual AndAlso deschisa.NumarRev = 0 Then
                Select Case state
                    Case ForexeAngajamentState.Initial : Return RezervariMenuOption.Definitiveaza
                    Case ForexeAngajamentState.InDefinitivare : Return RezervariMenuOption.Deruleaza
                    Case ForexeAngajamentState.InDerulare : Return RezervariMenuOption.GenereazaPdfFinal
                End Select
                Return RezervariMenuOption.None
            End If
            ' Any other sent revision still waiting for its final PDF (a send whose final PDF
            ' failed): the PDF can be generated once the angajament is running.
            Return If(state = ForexeAngajamentState.InDerulare, RezervariMenuOption.GenereazaPdfFinal,
                      RezervariMenuOption.None)
        End If

        ' No open revision: a running angajament takes a new reservation. K-BOT does not finish
        ' angajamente it did not create, so «Initial» / «In definitivare» without a Rev 0 here
        ' offer nothing.
        Return If(state = ForexeAngajamentState.InDerulare, RezervariMenuOption.AdaugaRezervare,
                  RezervariMenuOption.None)
    End Function

    ''' <summary>The menu text of an option.</summary>
    Public Shared Function Label(opt As RezervariMenuOption) As String
        Select Case opt
            Case RezervariMenuOption.Definitiveaza : Return "Definitivează"
            Case RezervariMenuOption.Deruleaza : Return "Derulează"
            Case RezervariMenuOption.GenereazaPdfFinal : Return "Generează PDF final"
            Case RezervariMenuOption.AdaugaRezervare : Return "Adaugă rezervare"
            Case RezervariMenuOption.None : Return String.Empty
            Case Else
                Throw New ArgumentOutOfRangeException(NameOf(opt), opt, "Unknown Rezervari menu option.")
        End Select
    End Function
End Class
