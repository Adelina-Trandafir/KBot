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
''' <para><b>The tree picker's seam</b> (decision D16). The classification choice sits behind
''' one private method, <see cref="AlegeClasificatie"/>. Today it opens the combo in the grid;
''' the tree picker later replaces that method's body and NOTHING else. There is deliberately
''' no <c>btnClsf</c> in the meantime -- a button with no behaviour is the silent no-op the
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
    Private Const COL_VAL_PREC As String = "val_prec"
    Private Const COL_VAL_CUR As String = "val_cur"
    Private Const COL_VAL_TOT As String = "val_tot"

    ''' <summary>How many characters the minted indicator code has, after the "!" prefix.
    ''' Access wrote <c>"!" &amp; GenerateUniqueSequence(3)</c>.</summary>
    Private Const LUNGIME_COD_INDICATOR As Integer = 3

    ''' <summary>The synthetic separator row of the classification list. Choosing it is
    ''' refused -- the port of <c>cmbClsf_BeforeUpdate</c>.</summary>
    Private Const CLSF_SEPARATOR As Integer = -1

    Private Shared ReadOnly _roCulture As New CultureInfo("ro-RO")

    Private _draft As DdfDraft
    ''' <summary>Every classification the server offered, in its order. Fetched once per
    ''' page, then filtered locally against what section A already uses.</summary>
    Private ReadOnly _clasificatii As New List(Of DdfClasificatie)()
    ' Filling the grid raises the cell events, and those are not the operator's edits.
    Private _suspenda As Boolean
    Private _sAuAdusClasificatiile As Boolean
    ''' <summary>Is the grid locked? True for a document generated from <c>FX_Rezervari</c>.
    ''' Kept as a field because two other methods (<c>ReimprospateazaCombo</c>,
    ''' <c>AduClasificatiile</c>) also write <c>btnAdauga.Enabled</c>, and the lock has to win
    ''' over both.</summary>
    Private _doarCitire As Boolean

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
                AduClasificatiile()
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

        ' The combo chevron promises a list. On a locked grid nothing opens when it is clicked,
        ' so the column paints as plain text instead -- a mark that does nothing is the same
        ' silent no-op as a button that does nothing. The type only moves while the grid has no
        ' rows, which is why this runs before the fill.
        Dim tipDorit As KBotColumnType = If(_doarCitire, KBotColumnType.Text, KBotColumnType.Combo)
        Dim colClsf As KBotDataColumn = grd.Column(COL_CLASIFICATIE)
        If colClsf IsNot Nothing AndAlso colClsf.ColumnType <> tipDorit Then
            grd.ClearRows()
            colClsf.ColumnType = tipDorit
        End If

        If _doarCitire Then
            btnAdauga.Enabled = False
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

    Private Shared Function EtichetaClasificatiei(c As DdfClasificatie) As String
        If c.EsteSeparator Then Return c.Denumire
        Return c.Clsf & " — " & c.Denumire
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

    ' Boundary UI async: logged and shown; nothing to throw to.
    Private Async Sub AduClasificatiile()
        Try
            If SursaClasificatiilor Is Nothing Then
                ' Not a silent degradation: without the list the operator cannot add a line,
                ' and saying so beats a button that does nothing.
                lblStare.Text = "Lista de clasificații nu este disponibilă în acest context."
                btnAdauga.Enabled = False
                Return
            End If

            Dim sarcina As Task(Of List(Of DdfClasificatie)) = SursaClasificatiilor.Invoke()
            Dim lista As List(Of DdfClasificatie) = Await sarcina.ConfigureAwait(True)
            _clasificatii.Clear()
            If lista IsNot Nothing Then _clasificatii.AddRange(lista)
            ReimprospateazaCombo()
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditSectiuneaAPage.AduClasificatiile", ex)
            lblStare.Text = "Clasificațiile nu au putut fi aduse de pe server."
            btnAdauga.Enabled = False
        End Try
    End Sub

    ''' <summary>
    ''' Fills the combo column with the classifications NOT already used in section A.
    '''
    ''' <para>Access did that exclusion in SQL, with
    ''' <c>Not In (SELECT IdClsf FROM tmpFX_DDF_REV_SA)</c>. There is no staging table any
    ''' more, so the filter is local and is re-applied after every add and every delete --
    ''' which is also why the draft is never sent to the classifications route.</para>
    '''
    ''' <para>The separator row is kept and shown; picking it is refused in
    ''' <see cref="Grd_CellValidating"/>.</para>
    ''' </summary>
    Private Sub ReimprospateazaCombo()
        ' A locked grid has no combo to fill and no line to add.
        If _doarCitire Then Return
        Dim col As KBotDataColumn = grd.Column(COL_CLASIFICATIE)
        If col Is Nothing Then Return

        Dim folosite As New HashSet(Of Integer)()
        If _draft IsNot Nothing Then
            For Each a As DdfDraftLinieA In _draft.LiniiA
                folosite.Add(a.IdClsf)
            Next
        End If

        Dim elemente As New List(Of Object)()
        For Each c As DdfClasificatie In _clasificatii
            ' The separator survives the filter -- it is not a classification.
            If Not c.EsteSeparator AndAlso folosite.Contains(c.IdClsf) Then Continue For
            elemente.Add(EtichetaClasificatiei(c))
        Next
        col.ComboItems = elemente

        Dim disponibile As Integer = _clasificatii.Where(
            Function(c) Not c.EsteSeparator AndAlso Not folosite.Contains(c.IdClsf)).Count()
        btnAdauga.Enabled = disponibile > 0
        lblStare.Text = If(disponibile > 0,
                           $"{disponibile} clasificații disponibile.",
                           "Toate clasificațiile angajamentului sunt deja folosite.")
    End Sub

    ''' <summary>The classification behind a combo label, or <c>Nothing</c>.</summary>
    Private Function ClasificatiaDupaEticheta(eticheta As String) As DdfClasificatie
        If String.IsNullOrWhiteSpace(eticheta) Then Return Nothing
        For Each c As DdfClasificatie In _clasificatii
            If String.Equals(EtichetaClasificatiei(c), eticheta, StringComparison.Ordinal) Then Return c
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' THE SEAM FOR THE TREE PICKER (decision D16).
    '''
    ''' <para>Every path that lets the operator choose a classification for a line goes
    ''' through here. Today the implementation opens the combo already in the grid; when the
    ''' tree picker arrives it replaces THIS METHOD'S BODY and nothing else -- no new button,
    ''' no second code path, no branch to keep in step.</para>
    ''' </summary>
    Private Sub AlegeClasificatie(indexRand As Integer)
        If _doarCitire Then Return
        If indexRand < 0 OrElse indexRand >= grd.RowCount Then Return
        grd.CurrentRowIndex = indexRand
        grd.CurrentColumnKey = COL_CLASIFICATIE
        grd.EnsureVisible(indexRand)
        grd.Focus()
    End Sub

    ''' <summary>
    ''' Applies a chosen classification to a line -- the port of <c>cmbClsf_AfterUpdate</c>.
    '''
    ''' <para>Access looked the three derived values up one at a time, with its own queries.
    ''' They ride down with the list instead, precomputed per classification for this
    ''' angajament, so the pick costs no round trip and the page stays network-free.</para>
    '''
    ''' <para>When no indicator exists for the classification yet, one is minted:
    ''' <c>"!" &amp; GenerateUniqueSequence(3)</c>, the ported Access algorithm, re-drawn
    ''' until it does not collide with a code already in the draft.</para>
    ''' </summary>
    Private Sub AplicaClasificatia(a As DdfDraftLinieA, c As DdfClasificatie)
        a.IdClsf = c.IdClsf
        a.IdClsfAcc = c.IdClsfAcc
        a.Clsf = c.Clsf
        a.Ss = c.Ss
        a.IdUnitate = c.IdUnitate
        a.ElementFund = c.Denumire

        a.ValPrec = c.ValPrec
        a.ValRec = c.ValRec
        If String.IsNullOrWhiteSpace(c.CodIndicator) Then
            a.CodIndicator = DdfCodIndicator.GenereazaUnic(
                LUNGIME_COD_INDICATOR,
                _draft.LiniiA.Select(Function(l) l.CodIndicator))
        Else
            a.CodIndicator = c.CodIndicator
        End If

        a.ValTot = Math.Round(a.ValCur + a.ValPrec, 2)
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' Editing
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' The refusals, ported one for one from Access.
    '''
    ''' <para><c>cmbClsf_BeforeUpdate</c>: the separator (<c>-1</c>) is refused.
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
                Case COL_CLASIFICATIE
                    Dim c As DdfClasificatie = ClasificatiaDupaEticheta(TryCast(e.ProposedValue, String))
                    If c Is Nothing Then Return
                    If c.IdClsf = CLSF_SEPARATOR Then
                        MessageBox.Show(Me, "Rândul acesta este doar un separator, nu o clasificație.",
                                        "Clasificație", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        e.Cancel = True
                    End If

                Case COL_ELEMENT
                    If String.IsNullOrWhiteSpace(TryCast(e.ProposedValue, String)) Then
                        MessageBox.Show(Me, "Elementul de fundamentare este un câmp obligatoriu!",
                                        "Secțiunea A", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        e.Cancel = True
                    End If

                Case COL_VAL_CUR
                    Dim valoare As Double
                    If Not Double.TryParse(Convert.ToString(e.ProposedValue, _roCulture),
                                           NumberStyles.Any, _roCulture, valoare) Then
                        MessageBox.Show(Me, "Valoarea curentă nu este un număr.",
                                        "Secțiunea A", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        e.Cancel = True
                        Return
                    End If
                    If valoare = 0.0R Then
                        MessageBox.Show(Me, "Valoarea curentă este un câmp obligatoriu!",
                                        "Secțiunea A", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        e.Cancel = True
                        Return
                    End If
                    If valoare < 0.0R AndAlso
                       Math.Round(valoare + a.ValPrec, 2) < Math.Round(a.ValRec, 2) Then
                        MessageBox.Show(Me,
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
                Case COL_CLASIFICATIE
                    Dim c As DdfClasificatie = ClasificatiaDupaEticheta(TryCast(e.NewValue, String))
                    If c Is Nothing OrElse c.EsteSeparator Then Return
                    AplicaClasificatia(a, c)
                    _suspenda = True
                    Try
                        ScrieRandul(rand, a)
                    Finally
                        _suspenda = False
                    End Try
                    ' A classification just left the pool of unused ones.
                    ReimprospateazaCombo()

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
    ''' Adds a line -- the port of <c>Form_BeforeInsert</c>. A new row inherits the header's
    ''' partner when the document has one, plus the angajament code and a fresh indicator
    ''' code; the keys stay temporary until the save maps them.
    ''' </summary>
    Private Sub BtnAdauga_Click(sender As Object, e As EventArgs) Handles btnAdauga.Click
        Try
            If _draft Is Nothing Then Return

            Dim a As New DdfDraftLinieA() With {
                .TempId = _draft.UrmatorulTempId(),
                .CodAngajament = _draft.CodAngajament,
                .CodIndicator = DdfCodIndicator.GenereazaUnic(
                    LUNGIME_COD_INDICATOR, _draft.LiniiA.Select(Function(l) l.CodIndicator))}

            If _draft.PartAng AndAlso Not String.IsNullOrWhiteSpace(_draft.CodFiscal) Then
                a.CodPartener = _draft.CodFiscal
                a.PartInd = True
            End If

            _draft.LiniiA.Add(a)
            UmpleGrila()
            ReimprospateazaCombo()
            RaiseEvent DraftModificat(Me, EventArgs.Empty)

            ' Straight into the classification picker -- an empty line is of no use until it
            ' has one, and Access did the same (`Clsf_Enter` dropped the combo open).
            AlegeClasificatie(grd.RowCount - 1)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditSectiuneaAPage.BtnAdauga_Click", ex)
            MessageBox.Show(Me, "Rândul nu a putut fi adăugat. Detalii în jurnalul de erori.",
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
                MessageBox.Show(Me, "Selectează întâi rândul de șters.", "Secțiunea A",
                                MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            Dim a As DdfDraftLinieA = TryCast(grd.Rows(i).Tag, DdfDraftLinieA)
            If a Is Nothing Then Return

            If MessageBox.Show(Me, $"Ștergi rândul «{a.Clsf}»?", "Secțiunea A",
                               MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then
                Return
            End If

            _draft.LiniiA.Remove(a)
            UmpleGrila()
            ReimprospateazaCombo()
            RaiseEvent DraftModificat(Me, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditSectiuneaAPage.BtnSterge_Click", ex)
            MessageBox.Show(Me, "Rândul nu a putut fi șters. Detalii în jurnalul de erori.",
                            "Secțiunea A", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>Double-clicking the classification cell opens the picker -- the same seam.</summary>
    Private Sub Grd_CellDoubleClick(sender As Object, e As KBotCellEventArgs) Handles grd.CellDoubleClick
        Try
            If e.ColumnKey <> COL_CLASIFICATIE Then Return
            AlegeClasificatie(e.RowIndex)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditSectiuneaAPage.Grd_CellDoubleClick", ex)
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
