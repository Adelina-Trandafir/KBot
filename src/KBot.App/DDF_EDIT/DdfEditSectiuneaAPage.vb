Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' «Sectiunea A» of the DDF editor (slice 0051) -- the port of <c>frmFX_DDF_REV_SECT_A</c>.
''' The only grid in the editor that can be edited at all.
'''
''' <para><b>And even it is locked for a document generated from reservations.</b> When
''' <see cref="DdfDraft.DinRezervari"/> is true the grid is read-only and both buttons are off:
''' the lines are what the reservations made them, and the post-save <c>FX_Rezervari</c> update
''' writes back against those same reservations, so a retyped value would put the document and
''' its reservations in a position to disagree. The grid unlocks for a MANUALLY BUILT document
''' -- a path that does not exist yet, and that needs no change here when it arrives. See
''' <c>AplicaModulDeEditare</c>.</para>
'''
''' <para><b>What is editable when it is not locked:</b> the classification, the element of fundamentation, the
''' parameters, the partner (only when the document is tied to one) and the CURRENT VALUE.
''' Everything else is derived: <c>Clsf</c> follows the classification, the previous value and
''' the receptions are read sums, and the total is
''' <c>Round(ValCur + ValPrec, 2)</c> -- two figures that say the same thing must not be able
''' to contradict each other on screen.</para>
'''
''' <para><b>Changing anything here rewrites section B</b> (decision D8). The page does not do
''' that itself: it raises <c>DraftModificat</c>, and the form rebuilds section B from section
''' A. One place, so the two can never drift.</para>
'''
''' <para><b>The picker's seam</b> (decision D16). The classification choice sits behind one
''' private method, <see cref="AlegeClasificatieAsync"/>. Since slice 0081-08 it opens the line
''' window <see cref="DdfEditLinieAForm"/> (every value of the line; the classification a
''' find-as-you-type combo with the classification mask) -- from «Adauga rand» for a new line and
''' from a double click on «Clsf» for an existing one. The in-grid combo column «Clasificatie»
''' stays hidden in the designer and is no longer fed. There is deliberately no <c>btnClsf</c>
''' -- a button with no behaviour is the silent no-op the house rules forbid.</para>
'''
''' <para>The page makes NO network requests: it asks the form for the classification list
''' through <see cref="SursaClasificatiilor"/>, which keeps its constructor parameterless and
''' therefore designable.</para>
''' </summary>
Public Class DdfEditSectiuneaAPage
    Implements IDdfEditPage, IThemedControl

    ' The column keys. The columns themselves are declared in the .Designer.vb; these are only
    ' the names the cells are written through, and must stay identical to the designer's.
    Private Const COL_CLASIFICATIE As String = "clasificatie"
    Private Const COL_CLSF As String = "clsf"
    Private Const COL_ELEMENT As String = "element_fund"
    Private Const COL_PARAMETRII As String = "parametrii_fund"
    Private Const COL_PARTENER As String = "cod_partener"
    Private Const COL_BUGET As String = "buget"
    Private Const COL_VAL_REC As String = "val_rec"
    Private Const COL_VAL_PREC As String = "val_prec"
    Private Const COL_VAL_CUR As String = "val_cur"
    Private Const COL_VAL_TOT As String = "val_tot"

    ''' <summary>How many characters the minted indicator code has, after the "!" prefix.
    ''' Access wrote <c>"!" &amp; GenerateUniqueSequence(3)</c>.</summary>
    Private Const LUNGIME_COD_INDICATOR As Integer = 3

    Private Shared ReadOnly _roCulture As New CultureInfo("ro-RO")

    Private _draft As DdfDraft
    ''' <summary>Every classification the server offered, in its order, filtered locally
    ''' against what section A already uses. See <see cref="AsiguraClasificatiileAsync"/> for
    ''' when it is fetched again.</summary>
    Private ReadOnly _clasificatii As New List(Of DdfClasificatie)()
    ' Filling the grid raises the cell events, and those are not the operator's edits.
    Private _suspenda As Boolean
    Private _sAuAdusClasificatiile As Boolean
    ''' <summary>Is the grid locked? True for a document generated from <c>FX_Rezervari</c>.
    ''' The only thing that turns «Adauga rand» off (slice 0081-08).</summary>
    Private _doarCitire As Boolean

    ''' <summary>Slice 0081-09: what a NEW angajament's header still lacks. While it lacks
    ''' anything, «Adauga rand» is off. Always empty for an angajament that exists.</summary>
    Private ReadOnly _lipsuriAntet As New List(Of String)()
    ''' <summary>Slice 0081-09: <c>AVACONT_COMUN.DefaProgram</c> as last fetched (all programs).</summary>
    Private ReadOnly _surseProgram As New List(Of DdfSursaProgram)()

    ''' <summary>
    ''' How the page gets the classification list. Set by the form, because
    ''' <see cref="IDdfEditPage"/> forbids the page its own api client -- and because a
    ''' parameterless constructor is what lets the page open in the Visual Studio designer.
    ''' </summary>
    Public Property SursaClasificatiilor As Func(Of Task(Of List(Of DdfClasificatie)))

    Public Event DraftModificat As EventHandler Implements IDdfEditPage.DraftModificat

    Public Sub New()
        InitializeComponent()
    End Sub

    Public ReadOnly Property PageKey As String Implements IDdfEditPage.PageKey
        Get
            Return "sectiunea-a"
        End Get
    End Property

    ''' <summary>
    ''' Slice 0081-09: how the page gets <c>AVACONT_COMUN.DefaProgram</c> (program -&gt; SS). A line
    ''' takes its SS from the rows of the document's program -- for a new angajament and for a new
    ''' revision alike. Set by the form (the page makes no requests of its own).
    ''' </summary>
    Public Property SursaSurselorProgramelor As Func(Of Task(Of List(Of DdfSursaProgram)))

    ''' <summary>Slice 0081-09: the SS chosen in K-BOT's main window -- proposed in the line window
    ''' when it is one of the program's, never imposed.</summary>
    Public Property SursaPropusa As String = String.Empty

    ''' <summary>
    ''' Slice 0081-09: what a new angajament's header still lacks (empty = complete). Pushed by the
    ''' form on every header edit; «Adauga rand» follows it.
    ''' </summary>
    Friend Sub SeteazaLipsurileAntetului(lipsuri As IEnumerable(Of String))
        _lipsuriAntet.Clear()
        If lipsuri IsNot Nothing Then _lipsuriAntet.AddRange(lipsuri)
        AplicaButonulAdauga()
    End Sub

    ''' <summary>
    ''' «Adauga rand» is on unless the grid is locked or -- for a new angajament -- the header is
    ''' not complete yet; the tooltip says which.
    ''' </summary>
    Private Sub AplicaButonulAdauga()
        btnAdauga.Enabled = Not _doarCitire AndAlso _lipsuriAntet.Count = 0
        If _doarCitire Then Return
        If _lipsuriAntet.Count > 0 Then
            Dim mesaj As String = "Completați întâi antetul: " & String.Join(", ", _lipsuriAntet) & "."
            tips.SetToolTipText(btnAdauga, mesaj)
            lblStare.Text = mesaj
        Else
            tips.SetToolTipText(btnAdauga, "Deschide fereastra unui rând nou în secțiunea A." & vbLf &
                                           "Clasificațiile deja folosite nu apar în listă.")
            If _draft IsNot Nothing Then ActualizeazaStarea()
        End If
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' The grid
    ' ══════════════════════════════════════════════════════════════════════════

    Public Sub SetDraft(draft As DdfDraft) Implements IDdfEditPage.SetDraft
        Try
            _draft = draft
            ' BEFORE the fill, not after: the lock moves the classification column's TYPE, and a
            ' column's type only moves while the grid has no rows.
            AplicaModulDeEditare()
            UmpleGrila()
            ' The list is fetched once per page, on first use, and only when there is a
            ' document to fetch it for -- and only when the grid can actually be edited: on a
            ' locked document the list would buy nothing and the round trip would be waste.
            ' Fire-and-forget with its own error handling: a `SetDraft` that awaited would
            ' block the page switch.
            If _draft IsNot Nothing AndAlso Not _doarCitire AndAlso Not _sAuAdusClasificatiile Then
                _sAuAdusClasificatiile = True
                AduClasificatiileLaDeschidere()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditSectiuneaAPage.SetDraft", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Locks or unlocks the grid.
    '''
    ''' <para><b>A DDF generated from reservations is NOT edited by hand.</b> Its lines are what
    ''' the reservations made them, and the reservations are what the post-save
    ''' <c>FX_Rezervari</c> update writes back against; letting the operator retype a value here
    ''' would put the document and the reservations it was built from in a position to
    ''' disagree. So the whole grid goes read-only and both buttons go with it -- adding a line
    ''' by hand is exactly what the lock exists to prevent.</para>
    '''
    ''' <para>The grid unlocks for a MANUALLY BUILT document. That path does not exist yet; when
    ''' it arrives it will arrive as <see cref="DdfDraft.DinRezervari"/> answering False, and
    ''' nothing here has to change.</para>
    ''' </summary>
    Private Sub AplicaModulDeEditare()
        _doarCitire = _draft IsNot Nothing AndAlso _draft.DinRezervari
        grd.ReadOnlyGrid = _doarCitire
        btnSterge.Enabled = Not _doarCitire
        ' Slice 0081-08: «Adauga rand» does not follow the classification list (off when the
        ' server list was empty or every classification was used left the operator a dead button
        ' and no reason; the line window says why instead). Slice 0081-09: it follows the lock
        ' and, for a new angajament, a complete header.
        AplicaButonulAdauga()

        If _doarCitire Then
            lblStare.Text = "Documentul este generat din rezervări: rândurile nu se modifică aici."
        End If
        ' The partner cell has a gate of its own, and the lock is above it.
        AplicaGateulPartenerului()
    End Sub

    ''' <summary>Writes the draft's section A into the grid.</summary>
    Private Sub UmpleGrila()
        _suspenda = True
        grd.BeginUpdate()
        Try
            grd.ClearRows()
            If _draft Is Nothing Then Return

            For Each a As DdfDraftLinieA In _draft.LiniiA
                Dim r As KBotDataRow = grd.AddRow()
                ' The domain object hangs off the row, so an edit writes into the graph
                ' rather than into a copy of it.
                r.Tag = a
                ScrieRandul(r, a)
            Next
            AplicaGateulPartenerului()
            grd.ClearDirty()
        Finally
            grd.EndUpdate()
            _suspenda = False
        End Try
    End Sub

    Private Sub ScrieRandul(r As KBotDataRow, a As DdfDraftLinieA)
        r(COL_CLASIFICATIE) = EtichetaClasificatiei(a)
        r(COL_CLSF) = a.Clsf
        r(COL_ELEMENT) = a.ElementFund
        r(COL_PARAMETRII) = a.ParametriiFund
        r(COL_PARTENER) = a.CodPartener
        r(COL_BUGET) = a.Buget
        r(COL_VAL_REC) = a.ValRec
        r(COL_VAL_PREC) = a.ValPrec
        r(COL_VAL_CUR) = a.ValCur
        r(COL_VAL_TOT) = a.ValTot
    End Sub

    ''' <summary>What a line shows in the classification cell. The code plus the name, which
    ''' is what the operator reads to tell two neighbouring classifications apart.</summary>
    Private Shared Function EtichetaClasificatiei(a As DdfDraftLinieA) As String
        If String.IsNullOrWhiteSpace(a.Clsf) Then Return String.Empty
        Return a.Clsf & " — " & a.ElementFund
    End Function

    ''' <summary>
    ''' The Partener cell follows two flags at once, exactly as Access did
    ''' (<c>Form_Load</c>: <c>CodPartener.Enabled = Me!PartInd</c>, under a header-level
    ''' <c>PartAng</c>). Off unless the DOCUMENT has a partner AND the LINE carries its own --
    ''' and off outright while the grid is locked.
    ''' </summary>
    Private Sub AplicaGateulPartenerului()
        Dim col As KBotDataColumn = grd.Column(COL_PARTENER)
        If col Is Nothing Then Return
        col.ReadOnly = _doarCitire OrElse _draft Is Nothing OrElse Not _draft.PartAng
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' The classification list
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' The first fetch, when the page opens: only so the status line can say how many
    ''' classifications there are. The line window fetches again when it needs to (see
    ''' <see cref="AsiguraClasificatiileAsync"/>), so a failure here costs nothing but the count.
    ''' </summary>
    ' Boundary UI async: logged and shown; nothing to throw to.
    Private Async Sub AduClasificatiileLaDeschidere()
        Try
            Try
                Await AsiguraSurseleProgramelorAsync().ConfigureAwait(True)
            Catch ex As Exception
                ' Only the count on the status line suffers; «Adauga rand» fetches again.
                GlobalErrorLog.Write("DdfEditSectiuneaAPage.AduClasificatiileLaDeschidere/surse-program", ex)
            End Try
            Await AsiguraClasificatiileAsync(fortat:=True).ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditSectiuneaAPage.AduClasificatiileLaDeschidere", ex)
            lblStare.Text = "Clasificațiile nu au putut fi aduse de pe server."
        End Try
    End Sub

    ''' <summary>
    ''' Makes sure the classification list is here, and says on the status line what it holds.
    '''
    ''' <para>Fetched again (a) when forced, (b) when the last fetch failed or came back empty --
    ''' «Adauga rand» is never dead just because the first try was unlucky -- and (c) for a
    ''' MANUAL angajament every time: its list is restricted to the <c>Titlu</c> of the first line
    ''' (Access, <c>qFX_DDF_SA_CLSF_MANUAL</c>), which changes as soon as that line exists.</para>
    '''
    ''' <para>Throws on a failed fetch (the callers are UI boundaries and say so).</para>
    ''' </summary>
    Private Async Function AsiguraClasificatiileAsync(fortat As Boolean) As Task
        If SursaClasificatiilor Is Nothing Then
            Throw New InvalidOperationException("The classification source was not set by the form.")
        End If
        Dim trebuie As Boolean = fortat OrElse _clasificatii.Count = 0 OrElse
                                 (_draft IsNot Nothing AndAlso _draft.Manual)
        If Not trebuie Then Return

        Dim lista As List(Of DdfClasificatie) = Await SursaClasificatiilor.Invoke().ConfigureAwait(True)
        _clasificatii.Clear()
        If lista IsNot Nothing Then _clasificatii.AddRange(lista)
        ActualizeazaStarea()
    End Function

    ''' <summary>
    ''' The status line: how many classifications are still free. It tells the empty cases apart
    ''' -- a server that sent nothing, a list with nothing on the program's SSs, and a section A
    ''' that already uses everything are three different things.
    ''' </summary>
    Private Sub ActualizeazaStarea()
        If _doarCitire Then Return
        If _lipsuriAntet.Count > 0 Then
            ' The header message stays until the header is complete: it is the only reason the
            ' button is off.
            lblStare.Text = "Completați întâi antetul: " & String.Join(", ", _lipsuriAntet) & "."
            Return
        End If
        Dim toate As Integer = _clasificatii.Where(Function(c) Not c.EsteSeparator).Count()
        Dim peProgram As Integer = ClasificatiileProgramului(_clasificatii).Count
        Dim libere As Integer = ClasificatiileLibere(Nothing).Count
        If toate = 0 Then
            lblStare.Text = "Serverul nu a trimis nicio clasificație pentru acest angajament."
        ElseIf peProgram = 0 Then
            lblStare.Text = $"Nicio clasificație nu are o sursă a programului «{_draft?.Program}»."
        ElseIf libere = 0 Then
            lblStare.Text = "Toate clasificațiile angajamentului sunt deja folosite."
        Else
            lblStare.Text = $"{libere} clasificații disponibile."
        End If
    End Sub

    ''' <summary>
    ''' Slice 0081-09: the SSs of the document's program (<c>DefaProgram</c>), from the copy the
    ''' page last fetched. Empty while nothing was fetched.
    ''' </summary>
    Private Function SurseleProgramului() As List(Of DdfSursaProgram)
        If _draft Is Nothing Then Return New List(Of DdfSursaProgram)()
        Return DdfSectiuneaAReguli.SurseAleProgramului(_surseProgram, _draft.Program)
    End Function

    ''' <summary>
    ''' Slice 0081-09: the classifications on one of the program's SSs. While the program map is
    ''' not known, nothing is filtered -- the status line must not claim «none» for a map that
    ''' simply has not arrived.
    ''' </summary>
    Private Function ClasificatiileProgramului(lista As IEnumerable(Of DdfClasificatie)) As List(Of DdfClasificatie)
        If _surseProgram.Count = 0 Then Return DdfSectiuneaAReguli.ClasificatiileSursei(lista, String.Empty)
        Return DdfSectiuneaAReguli.ClasificatiileProgramului(lista, SurseleProgramului())
    End Function

    ''' <summary>
    ''' Slice 0081-09: makes sure the program -&gt; SS map is here (fetched once; again only after
    ''' an empty or failed answer). Throws on a failed fetch.
    ''' </summary>
    Private Async Function AsiguraSurseleProgramelorAsync() As Task
        If _surseProgram.Count > 0 Then Return
        If SursaSurselorProgramelor Is Nothing Then
            Throw New InvalidOperationException("The program source map was not set by the form.")
        End If
        Dim lista As List(Of DdfSursaProgram) = Await SursaSurselorProgramelor.Invoke().ConfigureAwait(True)
        _surseProgram.Clear()
        If lista IsNot Nothing Then _surseProgram.AddRange(lista)
    End Function

    ''' <summary>
    ''' The classifications a line may choose from: every one the server offered, minus those the
    ''' OTHER lines already use (Access: <c>Not In (SELECT IdClsf FROM tmpFX_DDF_REV_SA)</c> --
    ''' there is no staging table any more, so the filter is local), and -- slice 0081-09 -- only
    ''' those on one of the document program's SSs. The line window narrows further to the SS the
    ''' operator picks. The separator stays out.
    ''' </summary>
    ''' <param name="linie">The line being edited (its own classification stays offered, whatever
    ''' its SS), or <c>Nothing</c> for a new line.</param>
    Private Function ClasificatiileLibere(linie As DdfDraftLinieA) As List(Of DdfClasificatie)
        Dim folosite As New HashSet(Of Integer)()
        If _draft IsNot Nothing Then
            For Each a As DdfDraftLinieA In _draft.LiniiA
                If ReferenceEquals(a, linie) Then Continue For
                folosite.Add(a.IdClsf)
            Next
        End If
        Dim propria As Integer = If(linie Is Nothing, 0, linie.IdClsf)
        Dim peProgram As New HashSet(Of Integer)(ClasificatiileProgramului(_clasificatii).Select(Function(c) c.IdClsf))
        Return _clasificatii.Where(
            Function(c) Not c.EsteSeparator AndAlso Not folosite.Contains(c.IdClsf) AndAlso
                        (peProgram.Contains(c.IdClsf) OrElse (propria <> 0 AndAlso c.IdClsf = propria))).ToList()
    End Function

    ''' <summary>
    ''' THE SEAM FOR THE CLASSIFICATION PICKER (decision D16).
    '''
    ''' <para>Every path that lets the operator choose a classification for a line goes through
    ''' here. Slice 0081-08: it opens <see cref="DdfEditLinieAForm"/> -- every value of the line in
    ''' one window, the classification a find-as-you-type combo with the classification mask. A new
    ''' line (<paramref name="linie"/> = <c>Nothing</c>) joins the draft only on OK; an existing one
    ''' takes the edited copy back only on OK.</para>
    '''
    ''' <para>Slice 0081-09: the window first asks for the source / sector, out of the SSs of the
    ''' document's program (<c>DefaProgram</c>) -- a new angajament and a new revision alike, since
    ''' one angajament may carry lines on several SSs of its program.</para>
    '''
    ''' <para>Throws on a failed fetch of the list (the callers are UI boundaries).</para>
    ''' </summary>
    Private Async Function AlegeClasificatieAsync(linie As DdfDraftLinieA) As Task
        If _doarCitire OrElse _draft Is Nothing Then Return
        Dim nou As Boolean = linie Is Nothing
        If nou AndAlso _lipsuriAntet.Count > 0 Then Return

        Await AsiguraClasificatiileAsync(fortat:=False).ConfigureAwait(True)
        Try
            Await AsiguraSurseleProgramelorAsync().ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditSectiuneaAPage.AlegeClasificatieAsync/surse-program", ex)
        End Try

        Dim surse As List(Of DdfSursaProgram) = SurseleProgramului()
        If surse.Count = 0 Then
            Dim motiv As String = If(_surseProgram.Count = 0,
                "Sursele programelor (AVACONT_COMUN.DefaProgram) nu au putut fi aduse de pe server.",
                $"Programul «{_draft.Program}» nu are nicio sursă / sector în DefaProgram.")
            KBotMessage.Show(Me, motiv & vbCrLf & vbCrLf &
                             "Fără sursă nu se poate alege clasificația rândului.",
                             "Secțiunea A", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        ActualizeazaStarea()

        Dim oferite As List(Of DdfClasificatie) = ClasificatiileLibere(linie)
        If oferite.Count = 0 Then
            Dim motiv As String
            If Not _clasificatii.Any(Function(c) Not c.EsteSeparator) Then
                motiv = $"Serverul nu a trimis nicio clasificație pentru angajamentul «{_draft.CodAngajament}»."
            ElseIf ClasificatiileProgramului(_clasificatii).Count = 0 Then
                motiv = $"Nicio clasificație a angajamentului nu are o sursă a programului «{_draft.Program}» " &
                        $"({String.Join(", ", surse.Select(Function(s) s.Ss))})."
            Else
                motiv = "Toate clasificațiile angajamentului sunt deja folosite în secțiunea A."
            End If
            KBotMessage.Show(Me, motiv & vbCrLf & vbCrLf & "Nu se poate adăuga un rând nou.",
                             "Secțiunea A", MessageBoxButtons.OK, MessageBoxIcon.Information)
            If nou Then Return
        End If

        Dim deEditat As DdfDraftLinieA = If(linie, LinieNoua())
        Dim alteCoduri As IEnumerable(Of String) =
            _draft.LiniiA.Where(Function(l) Not ReferenceEquals(l, linie)).Select(Function(l) l.CodIndicator)

        Using f As New DdfEditLinieAForm(deEditat, oferite, alteCoduri, surse, SursaPropusa, nou)
            If f.ShowDialog(Me) <> DialogResult.OK Then Return
            If nou Then
                _draft.LiniiA.Add(f.Linie)
            Else
                DdfEditLinieAForm.CopiazaIn(f.Linie, linie)
            End If
        End Using

        UmpleGrila()
        ActualizeazaStarea()
        RaiseEvent DraftModificat(Me, EventArgs.Empty)

        Dim i As Integer = _draft.LiniiA.IndexOf(If(linie, _draft.LiniiA.Last()))
        If i >= 0 AndAlso i < grd.RowCount Then
            grd.CurrentRowIndex = i
            grd.EnsureVisible(i)
        End If
    End Function

    ''' <summary>
    ''' A new line before its classification -- the port of <c>Form_BeforeInsert</c>: the
    ''' angajament code, a fresh indicator code and, when the document has one, the header's
    ''' partner. The keys stay temporary until the save maps them.
    ''' </summary>
    Private Function LinieNoua() As DdfDraftLinieA
        Dim a As New DdfDraftLinieA() With {
            .TempId = _draft.UrmatorulTempId(),
            .CodAngajament = _draft.CodAngajament,
            .CodIndicator = DdfCodIndicator.GenereazaUnic(
                LUNGIME_COD_INDICATOR, _draft.LiniiA.Select(Function(l) l.CodIndicator))}
        If _draft.PartAng AndAlso Not String.IsNullOrWhiteSpace(_draft.CodFiscal) Then
            a.CodPartener = _draft.CodFiscal
            a.PartInd = True
        End If
        Return a
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' Editing
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' The refusals, ported one for one from Access.
    '''
    ''' <para><c>cmbClsf_BeforeUpdate</c> (the separator is refused) lives in the line window
    ''' now, which never offers the separator at all (slice 0081-08).
    ''' <c>Form_BeforeUpdate</c>: an empty element of fundamentation is refused.
    ''' <c>ValCur_BeforeUpdate</c>: a current value of 0 is refused, and a NEGATIVE one that
    ''' would take the remaining value below the receptions is refused too -- money already
    ''' received cannot be un-committed.</para>
    ''' </summary>
    Private Sub Grd_CellValidating(sender As Object, e As KBotCellValidatingEventArgs) _
        Handles grd.CellValidating
        Try
            If _suspenda Then Return
            If e.RowIndex < 0 OrElse e.RowIndex >= grd.RowCount Then Return
            Dim a As DdfDraftLinieA = TryCast(grd.Rows(e.RowIndex).Tag, DdfDraftLinieA)
            If a Is Nothing Then Return

            Select Case e.ColumnKey
                Case COL_ELEMENT
                    If String.IsNullOrWhiteSpace(TryCast(e.ProposedValue, String)) Then
                        KBotMessage.Show(Me, "Elementul de fundamentare este un câmp obligatoriu!",
                                        "Secțiunea A", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        e.Cancel = True
                    End If

                Case COL_VAL_CUR
                    Dim valoare As Double
                    If Not Double.TryParse(Convert.ToString(e.ProposedValue, _roCulture),
                                           NumberStyles.Any, _roCulture, valoare) Then
                        KBotMessage.Show(Me, "Valoarea curentă nu este un număr.",
                                        "Secțiunea A", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        e.Cancel = True
                        Return
                    End If
                    If valoare = 0.0R Then
                        KBotMessage.Show(Me, "Valoarea curentă este un câmp obligatoriu!",
                                        "Secțiunea A", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        e.Cancel = True
                        Return
                    End If
                    If valoare < 0.0R AndAlso
                       Math.Round(valoare + a.ValPrec, 2) < Math.Round(a.ValRec, 2) Then
                        KBotMessage.Show(Me,
                            "Valoarea rămasă nu poate fi mai mică decât valoarea recepțiilor!",
                            "Secțiunea A", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        e.Cancel = True
                    End If
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditSectiuneaAPage.Grd_CellValidating", ex)
            ' A validator that threw must not let the value through: refusing is the safe
            ' side of the choice.
            e.Cancel = True
        End Try
    End Sub

    ''' <summary>Writes a committed cell into the graph and re-derives what follows from it.</summary>
    Private Sub Grd_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) _
        Handles grd.CellValueChanged
        Try
            If _suspenda Then Return
            If e.RowIndex < 0 OrElse e.RowIndex >= grd.RowCount Then Return
            Dim rand As KBotDataRow = grd.Rows(e.RowIndex)
            Dim a As DdfDraftLinieA = TryCast(rand.Tag, DdfDraftLinieA)
            If a Is Nothing Then Return

            Select Case e.ColumnKey
                Case COL_ELEMENT
                    a.ElementFund = Convert.ToString(e.NewValue, _roCulture)

                Case COL_PARAMETRII
                    a.ParametriiFund = Convert.ToString(e.NewValue, _roCulture)

                Case COL_PARTENER
                    a.CodPartener = Convert.ToString(e.NewValue, _roCulture)
                    ' A line that carries its own partner is what `PartInd` means.
                    a.PartInd = Not String.IsNullOrWhiteSpace(a.CodPartener)

                Case COL_VAL_CUR
                    ' `ValCur_AfterUpdate`: the total is recomputed, never typed.
                    a.ValCur = Convert.ToDouble(e.NewValue, _roCulture)
                    a.ValTot = Math.Round(a.ValCur + a.ValPrec, 2)
                    _suspenda = True
                    Try
                        rand(COL_VAL_TOT) = a.ValTot
                    Finally
                        _suspenda = False
                    End Try

                Case Else
                    Return
            End Select

            RaiseEvent DraftModificat(Me, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditSectiuneaAPage.Grd_CellValueChanged", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Adds a line: opens the line window on a new line (slice 0081-08). The line joins section
    ''' A only when the window is confirmed -- «Renunta» leaves nothing behind.
    ''' </summary>
    ' Boundary UI async: logged and shown.
    Private Async Sub BtnAdauga_Click(sender As Object, e As EventArgs) Handles btnAdauga.Click
        Try
            If _draft Is Nothing Then Return
            btnAdauga.Enabled = False
            Try
                Await AlegeClasificatieAsync(Nothing).ConfigureAwait(True)
            Finally
                AplicaButonulAdauga()
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditSectiuneaAPage.BtnAdauga_Click", ex)
            KBotMessage.Show(Me, "Rândul nu a putut fi adăugat (lista de clasificații nu a putut fi adusă?)." &
                            " Detalii în jurnalul de erori.",
                            "Secțiunea A", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>
    ''' Removes a line. Its section-B twin goes with it, which needs no code here: section B
    ''' is rebuilt from section A by the form when <c>DraftModificat</c> fires.
    ''' </summary>
    Private Sub BtnSterge_Click(sender As Object, e As EventArgs) Handles btnSterge.Click
        Try
            If _draft Is Nothing Then Return
            Dim i As Integer = grd.CurrentRowIndex
            If i < 0 OrElse i >= grd.RowCount Then
                KBotMessage.Show(Me, "Selectează întâi rândul de șters.", "Secțiunea A",
                                MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            Dim a As DdfDraftLinieA = TryCast(grd.Rows(i).Tag, DdfDraftLinieA)
            If a Is Nothing Then Return

            If KBotMessage.Show(Me, $"Ștergi rândul «{a.Clsf}»?", "Secțiunea A",
                               MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then
                Return
            End If

            _draft.LiniiA.Remove(a)
            UmpleGrila()
            ' A classification just went back into the pool of free ones.
            ActualizeazaStarea()
            RaiseEvent DraftModificat(Me, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditSectiuneaAPage.BtnSterge_Click", ex)
            KBotMessage.Show(Me, "Rândul nu a putut fi șters. Detalii în jurnalul de erori.",
                            "Secțiunea A", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>
    ''' Double-clicking a line's classification opens the line window on it -- the same seam.
    ''' Slice 0081-08: the column the operator SEES is «Clsf»; «Clasificatie» (the old in-grid
    ''' combo) is hidden in the designer, so a handler that listened only to it never fired.
    ''' Both keys open it now. The other editable cells keep their in-place editing.
    ''' </summary>
    ' Boundary UI async: logged and shown.
    Private Async Sub Grd_CellDoubleClick(sender As Object, e As KBotCellEventArgs) Handles grd.CellDoubleClick
        Try
            If e.ColumnKey <> COL_CLASIFICATIE AndAlso e.ColumnKey <> COL_CLSF Then Return
            If _doarCitire Then Return
            If e.RowIndex < 0 OrElse e.RowIndex >= grd.RowCount Then Return
            Dim a As DdfDraftLinieA = TryCast(grd.Rows(e.RowIndex).Tag, DdfDraftLinieA)
            If a Is Nothing Then Return
            Await AlegeClasificatieAsync(a).ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditSectiuneaAPage.Grd_CellDoubleClick", ex)
            KBotMessage.Show(Me, "Rândul nu a putut fi deschis. Detalii în jurnalul de erori.",
                            "Secțiunea A", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' Theming
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>Required: this page owns child controls, so the generic traversal would
    ''' repaint them wrongly without it.</summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette

            BackColor = p.SurfaceAltColor
            tlyRoot.BackColor = p.SurfaceAltColor
            tlyButoane.BackColor = p.SurfaceAltColor
            lblStare.ForeColor = p.TextDimColor
            lblStare.BackColor = Color.Transparent

            ' The buttons under the grid go through the HOUSE STYLES, exactly as the ORD
            ' editor's pages do. They have to be styled by hand: `ThemeManager.Traverse` does
            ' not carry the generic rules into the children of a control that is itself an
            ' `IThemedControl` -- and this page is one -- so they would stay system grey.
            ' Writing raw palette colours here instead of calling `ButtonStyles` was the
            ' incongruence: it skips `ModernRenderer`, so these came out flat squares next to
            ' the ORD editor's rounded buttons, and no button carried the accent.
            ' «Adauga» is the action, so it takes the accent; «Sterge» stays secondary -- a
            ' destructive button is not dressed in the colour that invites the finger.
            ButtonStyles.ApplyPrimary(btnAdauga, scheme)
            ButtonStyles.ApplySecondary(btnSterge, scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditSectiuneaAPage.ApplyTheme", ex)
        End Try
    End Sub
End Class
