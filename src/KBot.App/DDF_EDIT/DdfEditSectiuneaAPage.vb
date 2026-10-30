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
''' <para><b>Entering lines straight in the grid</b> (operator, 23.10.2026; replaces the line window
''' <see cref="DdfEditLinieAForm"/> of slice 0081-08 and the buttons «Adauga rand» / «Sterge randul»).
''' The grid always ends with ONE EMPTY ROW: picking a classification in its «Clasificatie» cell turns
''' it into a line (see <see cref="SchimbaClasificatia"/>) and a fresh empty row appears below.
''' The cell is a grouped combo (<see cref="KBotComboGroupedItem"/>, one folding header per SS and
''' per «in angajament» / «alte clasificatii») holding ONLY the classifications of the header's
''' program that no other line uses. Picking one fills budget, receptions, available, previous
''' value and remaining straight into the row. A line is removed with the button column «sterge».
''' There is deliberately no <c>btnClsf</c> -- a button with no behaviour is the silent no-op the
''' house rules forbid.</para>
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
    Private Const COL_DISPONIBIL As String = "disponibil"
    Private Const COL_VAL_PREC As String = "val_prec"
    Private Const COL_VAL_CUR As String = "val_cur"
    Private Const COL_VAL_RAMASA As String = "val_ramasa"
    Private Const COL_STERGE As String = "sterge"
    Private Const CAPTION_STERGE As String = "✕"

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
    ''' A locked grid has no empty row and no delete button.</summary>
    Private _doarCitire As Boolean

    ''' <summary>Slice 0081-09: what a NEW angajament's header still lacks. While it lacks
    ''' anything, the empty row's classification cell is read-only. Always empty for an angajament that exists.</summary>
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
    ''' form on every header edit; the empty row follows it. The program may have changed too, so
    ''' the classifications the combo offers are rebuilt.
    ''' </summary>
    Friend Sub SeteazaLipsurileAntetului(lipsuri As IEnumerable(Of String))
        _lipsuriAntet.Clear()
        If lipsuri IsNot Nothing Then _lipsuriAntet.AddRange(lipsuri)
        AplicaStareaRanduluiNou()
    End Sub

    ''' <summary>
    ''' The empty row takes a classification unless the grid is locked or -- for a new angajament --
    ''' the header is not complete yet (the status line says which); the combo is rebuilt for the
    ''' program as it is now.
    ''' </summary>
    Private Sub AplicaStareaRanduluiNou()
        If _doarCitire Then Return
        If _lipsuriAntet.Count > 0 Then
            lblStare.Text = "Completați întâi antetul: " & String.Join(", ", _lipsuriAntet) & "."
        ElseIf _draft IsNot Nothing Then
            ActualizeazaStarea()
        End If
        grd.Invalidate()
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
        grd.Column(COL_STERGE).Visible = If(_doarCitire, KBotColumnVisibility.Hidden, KBotColumnVisibility.Visible)
        ' The empty row follows the lock and, for a new angajament, a complete header.
        AplicaStareaRanduluiNou()

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
            ' The permanent empty row (Tag Nothing): where the next line starts.
            If Not _doarCitire Then grd.AddRow()
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
        r(COL_DISPONIBIL) = DdfSectiuneaAReguli.Disponibil(a.Buget, a.ValRec)
        r(COL_VAL_PREC) = a.ValPrec
        r(COL_VAL_CUR) = a.ValCur
        r(COL_VAL_RAMASA) = DdfSectiuneaAReguli.ValoareRamasa(a.Buget, a.ValRec, a.ValCur)
        r(COL_STERGE) = CAPTION_STERGE
    End Sub

    ''' <summary>What a line shows in the classification cell: the code (written as forexecab
    ''' writes it) plus the classification's name -- what the operator reads to tell two neighbouring
    ''' classifications apart. The name comes from the list when it is here, else from the line's
    ''' element of fundamentation.</summary>
    Private Function EtichetaClasificatiei(a As DdfDraftLinieA) As String
        If String.IsNullOrWhiteSpace(a.Clsf) Then Return String.Empty
        Dim k_c As DdfClasificatie = _clasificatii.FirstOrDefault(Function(k_x) a.IdClsf <> 0 AndAlso k_x.IdClsf = a.IdClsf)
        Return DdfSendInputs.ForexeClsf(a.Clsf) & " — " & If(k_c Is Nothing, a.ElementFund, k_c.Denumire)
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
                ' Only the count on the status line suffers; the combo is fed again on the next change.
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
        ' Whatever moved the status (the list, the program, the lines in use) moved the combo.
        ActualizeazaListaClasificatii()
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
        ' Operator, 26.09.2026: a classification already added never comes back in the list --
        ' matched by key AND by code + SS (the nomenclator can hold one code under several keys).
        Dim linii As IEnumerable(Of DdfDraftLinieA) =
            If(_draft Is Nothing, Enumerable.Empty(Of DdfDraftLinieA)(), _draft.LiniiA)
        Dim propria As Integer = If(linie Is Nothing, 0, linie.IdClsf)
        Dim peProgram As New HashSet(Of Integer)(ClasificatiileProgramului(_clasificatii).Select(Function(c) c.IdClsf))
        Return _clasificatii.Where(
            Function(c) Not c.EsteSeparator AndAlso Not DdfSectiuneaAReguli.EsteFolosita(c, linii, linie) AndAlso
                        (peProgram.Contains(c.IdClsf) OrElse (propria <> 0 AndAlso c.IdClsf = propria))).ToList()
    End Function

    ''' <summary>
    ''' Rebuilds what the «Clasificatie» combo offers (operator, 23.10.2026): ONLY the classifications of the header's
    ''' program (<see cref="ClasificatiileLibere"/>: on one of the program's SSs, none already used by
    ''' another line), the ones the angajament already has first, the proposed SS first among them.
    ''' Grouped: one folding header per SS and per «din angajament» / «alte clasificatii».
    ''' Called whenever the status line is (the list, the program or the lines in use moved).
    ''' </summary>
    Private Sub ActualizeazaListaClasificatii()
        Dim k_items As New List(Of Object)()
        If Not _doarCitire AndAlso _draft IsNot Nothing Then
            Dim k_libere As List(Of DdfClasificatie) = ClasificatiileLibere(Nothing)
            Dim k_propusa As String = If(SursaPropusa, String.Empty).Trim()
            If k_propusa.Length > 0 Then
                k_libere = k_libere.OrderBy(Function(k_x) If(DdfSectiuneaAReguli.AcelasiSs(k_x.Ss, k_propusa), 0, 1)).ToList()
            End If
            For Each k_c As DdfClasificatie In DdfSectiuneaAReguli.InAngajamentIntai(k_libere, _draft.Manual)
                k_items.Add(New KBotComboGroupedItem(EtichetaListei(k_c), GrupulClasificatiei(k_c), String.Empty, k_c))
            Next
        End If
        grd.Column(COL_CLASIFICATIE).ComboItems = k_items
    End Sub

    ''' <summary>What a classification shows in the list: the code (written as forexecab writes it), then the name.</summary>
    Private Shared Function EtichetaListei(k_c As DdfClasificatie) As String
        Return DdfSendInputs.ForexeClsf(k_c.Clsf) & " — " & k_c.Denumire
    End Function

    ''' <summary>The folding header a classification is listed under.</summary>
    Private Function GrupulClasificatiei(k_c As DdfClasificatie) As String
        Dim k_sursa As String = If(String.IsNullOrWhiteSpace(k_c.Ss), "fără sursă", "Sursa " & k_c.Ss.Trim())
        Return k_sursa & If(DdfSectiuneaAReguli.EsteInAngajament(k_c, _draft.Manual), " — din angajament", " — alte clasificații")
    End Function

    ''' <summary>
    ''' The classification cell was refused unless it holds a classification of the list: a picked item
    ''' passes; typed text must be the cell's own text, or name one item of the list (the item is then
    ''' put in its place). Empty text on the empty row is no choice at all.
    ''' </summary>
    Private Sub ValideazaClasificatia(e As KBotCellValidatingEventArgs)
        If TypeOf e.ProposedValue Is KBotComboGroupedItem Then Return
        Dim k_text As String = If(Convert.ToString(e.ProposedValue, _roCulture), String.Empty).Trim()
        Dim k_linie As DdfDraftLinieA = TryCast(grd.Rows(e.RowIndex).Tag, DdfDraftLinieA)
        If k_linie Is Nothing AndAlso k_text.Length = 0 Then Return
        If k_linie IsNot Nothing AndAlso String.Equals(k_text, EtichetaClasificatiei(k_linie), StringComparison.Ordinal) Then Return

        Dim k_gasit As KBotComboGroupedItem = Nothing
        Dim k_oferite As IList(Of Object) = grd.Column(COL_CLASIFICATIE).ComboItems
        If k_oferite IsNot Nothing Then
            k_gasit = k_oferite.OfType(Of KBotComboGroupedItem)().FirstOrDefault(
                Function(k_x) String.Equals(k_x.Text, k_text, StringComparison.OrdinalIgnoreCase))
        End If
        If k_gasit IsNot Nothing Then
            e.ProposedValue = k_gasit
            Return
        End If
        KBotMessage.Show(Me, $"Alegeți o clasificație din listă (clasificațiile programului «{_draft?.Program}» care nu sunt deja folosite).",
                         "Secțiunea A", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        e.Cancel = True
    End Sub

    ''' <summary>
    ''' A classification was picked in a row's cell. On the EMPTY row it starts a new line (which joins
    ''' the draft, with a fresh empty row below it, and the cursor goes to its value); on a line it
    ''' changes the line's classification. Either way budget, receptions, available, previous value and
    ''' remaining are written into the row at once. Nothing was picked = the cell goes back to what the
    ''' line says.
    ''' </summary>
    Private Sub SchimbaClasificatia(k_rand As KBotDataRow, k_valoare As Object)
        Dim k_item As KBotComboGroupedItem = TryCast(k_valoare, KBotComboGroupedItem)
        Dim k_c As DdfClasificatie = If(k_item Is Nothing, Nothing, TryCast(k_item.Tag, DdfClasificatie))
        Dim k_linie As DdfDraftLinieA = TryCast(k_rand.Tag, DdfDraftLinieA)

        If k_c Is Nothing Then
            _suspenda = True
            Try
                k_rand(COL_CLASIFICATIE) = If(k_linie Is Nothing, String.Empty, EtichetaClasificatiei(k_linie))
            Finally
                _suspenda = False
            End Try
            Return
        End If

        Dim k_nou As Boolean = k_linie Is Nothing
        Dim k_denumireAnterioara As String = String.Empty
        If k_nou Then
            k_linie = LinieNoua()
        Else
            Dim k_idAnterior As Integer = k_linie.IdClsf
            Dim k_anterioara As DdfClasificatie = _clasificatii.FirstOrDefault(Function(k_x) k_idAnterior <> 0 AndAlso k_x.IdClsf = k_idAnterior)
            If k_anterioara IsNot Nothing Then k_denumireAnterioara = k_anterioara.Denumire
        End If
        AplicaClasificatia(k_linie, k_c, k_denumireAnterioara)

        _suspenda = True
        Try
            If k_nou Then
                _draft.LiniiA.Add(k_linie)
                k_rand.Tag = k_linie
            End If
            ScrieRandul(k_rand, k_linie)
            If k_nou Then grd.AddRow()
        Finally
            _suspenda = False
        End Try
        AplicaGateulPartenerului()

        ActualizeazaStarea()
        If _draft.Manual Then ReincarcaListaManualaAsync()
        RaiseEvent DraftModificat(Me, EventArgs.Empty)

        If k_nou AndAlso IsHandleCreated Then
            Dim k_index As Integer = _draft.LiniiA.IndexOf(k_linie)
            BeginInvoke(New Action(Sub() EditeazaValoarea(k_index)))
        End If
    End Sub

    ''' <summary>
    ''' Writes a classification's values into a line -- the port of <c>cmbClsf_AfterUpdate</c>. The
    ''' derived values ride with the list (precomputed per classification for this angajament); an
    ''' indicator code is minted only when the angajament has none for the classification AND the line
    ''' does not already carry one for the SAME classification. The element of fundamentation starts as
    ''' the classification's name -- but a text the operator wrote is not overwritten by a later pick.
    ''' </summary>
    Private Sub AplicaClasificatia(k_linie As DdfDraftLinieA, k_c As DdfClasificatie, k_denumireAnterioara As String)
        Dim k_alta As Boolean = k_c.IdClsf <> k_linie.IdClsf
        Dim k_element As String = If(k_linie.ElementFund, String.Empty).Trim()
        If k_element.Length = 0 OrElse String.Equals(k_element, k_denumireAnterioara, StringComparison.Ordinal) Then
            k_linie.ElementFund = k_c.Denumire
        End If

        k_linie.IdClsf = k_c.IdClsf
        k_linie.Clsf = k_c.Clsf
        k_linie.Ss = k_c.Ss
        k_linie.IdUnitate = k_c.IdUnitate
        k_linie.ValPrec = k_c.ValPrec
        k_linie.ValRec = k_c.ValRec
        ' A line built here is never from a reservation: its budget is the angajament's
        ' FX_Indicatori.Credit_Bugetar for the classification (slice 0081-12).
        k_linie.Buget = k_c.Buget
        If Not String.IsNullOrWhiteSpace(k_c.CodIndicator) Then
            k_linie.CodIndicator = k_c.CodIndicator
        ElseIf k_alta OrElse String.IsNullOrWhiteSpace(k_linie.CodIndicator) Then
            Dim k_alteCoduri As List(Of String) = _draft.LiniiA.Where(Function(k_x) Not ReferenceEquals(k_x, k_linie)).
                Select(Function(k_x) k_x.CodIndicator).ToList()
            k_linie.CodIndicator = DdfCodIndicator.GenereazaUnic(LUNGIME_COD_INDICATOR, k_alteCoduri)
        End If
        ' The partner is the header's, the same on every line (slice 0081-09): LinieNoua puts it.
        k_linie.ValTot = Math.Round(k_linie.ValCur + k_linie.ValPrec, 2)
    End Sub

    ''' <summary>Opens the current value of a line for typing (after the classification was picked).</summary>
    ' Boundary UI (BeginInvoke callback): logged and swallowed.
    Private Sub EditeazaValoarea(k_index As Integer)
        Try
            If k_index < 0 OrElse k_index >= grd.RowCount Then Return
            grd.CurrentRowIndex = k_index
            grd.EnsureVisible(k_index)
            grd.EditCell(COL_VAL_CUR, k_index)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditSectiuneaAPage.EditeazaValoarea", ex)
        End Try
    End Sub

    ''' <summary>
    ''' A MANUAL angajament's list depends on its first line (see <see cref="AsiguraClasificatiileAsync"/>),
    ''' so every change of the lines fetches it again; the combo follows when it arrives.
    ''' </summary>
    ' Boundary UI async: logged; the combo keeps the list it had.
    Private Async Sub ReincarcaListaManualaAsync()
        Try
            If _draft Is Nothing OrElse Not _draft.Manual Then Return
            Await AsiguraClasificatiileAsync(fortat:=False).ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditSectiuneaAPage.ReincarcaListaManualaAsync", ex)
            lblStare.Text = "Clasificațiile nu au putut fi aduse de pe server."
        End Try
    End Sub

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
            If e.ColumnKey = COL_CLASIFICATIE Then
                ValideazaClasificatia(e)
                Return
            End If
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
                            "Valoarea totală (precedentă + curentă) nu poate fi mai mică decât valoarea recepțiilor!",
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
            If e.ColumnKey = COL_CLASIFICATIE Then
                SchimbaClasificatia(rand, e.NewValue)
                Return
            End If
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
                        ' The cell gets the number back (the editor committed the typed text),
                        ' and «Val. ramasa» follows it.
                        rand(COL_VAL_CUR) = a.ValCur
                        rand(COL_VAL_RAMASA) = DdfSectiuneaAReguli.ValoareRamasa(a.Buget, a.ValRec, a.ValCur)
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
    ''' The empty row takes only a classification (and only once the header is complete); its delete
    ''' button is dead and blank. A hint says what the row is for.
    ''' </summary>
    ' Boundary UI (paint path): logged and swallowed.
    Private Sub Grd_CellFormatting(sender As Object, e As KBotCellFormattingEventArgs) Handles grd.CellFormatting
        Try
            If e.RowIndex < 0 OrElse e.RowIndex >= grd.RowCount Then Return
            If grd.Rows(e.RowIndex).Tag IsNot Nothing Then Return
            Select Case e.ColumnKey
                Case COL_CLASIFICATIE
                    e.ReadOnlyCell = _lipsuriAntet.Count > 0
                    If _lipsuriAntet.Count = 0 AndAlso String.IsNullOrEmpty(e.Text) Then
                        e.Text = "Alegeți o clasificație pentru un rând nou…"
                        e.ForeColor = ThemeManager.Current.Palette.TextDimColor
                    End If
                Case COL_STERGE
                    e.Text = String.Empty
                    e.Enabled = False
                Case Else
                    e.ReadOnlyCell = True
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditSectiuneaAPage.Grd_CellFormatting", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Esc twice in a row. A line the operator started in this session (no key yet) goes away, without a question; a line that was already saved is left as it is
    ''' (its classification feeds other values, so the cells are not put back one by one).
    ''' </summary>
    ' Boundary UI (event handler): logged and swallowed.
    Private Sub Grd_RowCancelRequested(sender As Object, e As KBotRowCancelEventArgs) Handles grd.RowCancelRequested
        Try
            e.Handled = True
            If _doarCitire OrElse _draft Is Nothing OrElse e.RowIndex < 0 OrElse e.RowIndex >= grd.RowCount Then Return
            Dim a As DdfDraftLinieA = TryCast(grd.Rows(e.RowIndex).Tag, DdfDraftLinieA)
            If a Is Nothing OrElse a.IdSecA <> 0 Then Return
            _draft.LiniiA.Remove(a)
            grd.RemoveRowAt(e.RowIndex)
            ActualizeazaStarea()
            If _draft.Manual Then ReincarcaListaManualaAsync()
            RaiseEvent DraftModificat(Me, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditSectiuneaAPage.Grd_RowCancelRequested", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Removes a line (its button column). Its section-B twin goes with it, which needs no code here:
    ''' section B is rebuilt from section A by the form when <c>DraftModificat</c> fires.
    ''' </summary>
    ' Boundary UI (event handler): logged and shown.
    Private Sub Grd_ButtonClick(sender As Object, e As KBotButtonClickEventArgs) Handles grd.ButtonClick
        Try
            If e.ColumnKey <> COL_STERGE OrElse _doarCitire OrElse _draft Is Nothing Then Return
            If e.RowIndex < 0 OrElse e.RowIndex >= grd.RowCount Then Return
            Dim a As DdfDraftLinieA = TryCast(grd.Rows(e.RowIndex).Tag, DdfDraftLinieA)
            If a Is Nothing Then Return

            If KBotMessage.Show(Me, $"Ștergi rândul «{DdfSendInputs.ForexeClsf(a.Clsf)}»?", "Secțiunea A",
                               MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then
                Return
            End If

            _draft.LiniiA.Remove(a)
            grd.RemoveRowAt(e.RowIndex)
            ' A classification just went back into the pool of free ones.
            ActualizeazaStarea()
            If _draft.Manual Then ReincarcaListaManualaAsync()
            RaiseEvent DraftModificat(Me, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditSectiuneaAPage.Grd_ButtonClick", ex)
            KBotMessage.Show(Me, "Rândul nu a putut fi șters. Detalii în jurnalul de erori.",
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

        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditSectiuneaAPage.ApplyTheme", ex)
        End Try
    End Sub

    ' ---- The action row can sit on the form's bottom row, beside «Salveaza documentul» ---------------

    Private _actionsHost As TableLayoutPanel
    Private _actionsRowHeight As Single

    ''' <summary>Moves the status row into <paramref name="k_host"/> (column <paramref name="k_column"/>) so the buttons share one line.</summary>
    Friend Sub AttachActionsTo(k_host As TableLayoutPanel, k_column As Integer)
        If _actionsHost IsNot Nothing Then Return
        _actionsRowHeight = tlyRoot.RowStyles(1).Height
        tlyRoot.Controls.Remove(tlyButoane)
        tlyRoot.RowStyles(1).Height = 0
        k_host.Controls.Add(tlyButoane, k_column, 0)
        _actionsHost = k_host
        ThemeManager.Apply(tlyButoane)
    End Sub

    ''' <summary>Puts the action row back in this page (it leaves the page's tab).</summary>
    Friend Sub DetachActions()
        If _actionsHost Is Nothing Then Return
        _actionsHost.Controls.Remove(tlyButoane)
        tlyRoot.Controls.Add(tlyButoane, 0, 1)
        tlyRoot.RowStyles(1).Height = _actionsRowHeight
        _actionsHost = Nothing
    End Sub
End Class
