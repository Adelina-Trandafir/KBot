Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' «Rand din Sectiunea A» (slice 0081-08, operator 26.09.2026): one line of section A, every
''' value it expects, in one window. «Adauga rand» opens it on a new line, a double click on a
''' line's classification opens it on that line.
'''
''' <para><b>It works on a COPY.</b> The line the caller hands in is copied on the way in and
''' the edited copy comes back through <see cref="Linie"/> only after OK, so «Renunta» leaves the
''' draft exactly as it was -- the old path added an empty line first and left it behind when the
''' operator changed their mind.</para>
'''
''' <para><b>The refusals are section A's own</b> (the ports of <c>cmbClsf_BeforeUpdate</c>,
''' <c>Form_BeforeUpdate</c>, <c>ValCur_BeforeUpdate</c>), all named in ONE message rather than
''' one box per problem.</para>
'''
''' <para>No network: the classifications arrive already fetched and already filtered (the ones
''' the OTHER lines use are not offered). The separator row of the server list is not offered
''' either -- it is a caption, not a classification, and in a list that narrows as the operator
''' types it would only be noise.</para>
'''
''' <para><b>Source / sector first</b> (slice 0081-09). The classification list shows only the
''' classifications of the SS in the first field. The SSs are those of the document's program
''' (<c>AVACONT_COMUN.DefaProgram</c>: 0000000000 -> 02A / 02E, 0000002510 -> 01A), for a new
''' angajament and a new revision alike; the field is never locked, only tied to the program.
''' None given = nothing is filtered. The partner is not a field
''' here: it is the header's, the same on every line, and the page puts it on the line.</para>
''' </summary>
Public Class DdfEditLinieAForm

    Private Shared ReadOnly _roCulture As New CultureInfo("ro-RO")

    ''' <summary>How many characters a minted indicator code has after «!» -- the same constant
    ''' as the page's (Access: <c>"!" &amp; GenerateUniqueSequence(3)</c>).</summary>
    Private Const LUNGIME_COD_INDICATOR As Integer = 3

    Private ReadOnly _linie As DdfDraftLinieA
    ''' <summary>Every classification the caller offered, of every SS.</summary>
    Private ReadOnly _toate As New List(Of DdfClasificatie)()
    ''' <summary>The ones of the selected SS -- what the classification combo holds, in its order.</summary>
    Private ReadOnly _oferite As New List(Of DdfClasificatie)()
    Private ReadOnly _surse As List(Of DdfSursaProgram)
    Private ReadOnly _sursaPropusa As String
    Private ReadOnly _alteCoduri As List(Of String)
    Private ReadOnly _nou As Boolean
    Private ReadOnly _manual As Boolean
    ''' <summary>The classification the line had when the window opened (0 = none).</summary>
    Private ReadOnly _idClsfInitial As Integer
    Private _aleasa As DdfClasificatie
    Private _seIncarca As Boolean

    ' The keys of `grdValori`'s columns -- identical to the designer's.
    Private Const COL_BUGET As String = "buget"
    Private Const COL_VAL_REC As String = "val_rec"
    Private Const COL_DISPONIBIL As String = "disponibil"
    Private Const COL_VAL_PREC As String = "val_prec"
    Private Const COL_VAL_CUR As String = "val_cur"
    Private Const COL_VAL_RAMASA As String = "val_ramasa"

    ''' <summary>The current value as last committed in the grid (0 = none yet).</summary>
    Private _valCur As Double
    ''' <summary>The window opens on a line that already has its classification: the value's
    ''' editor is opened in <c>OnShown</c> (it needs a visible grid).</summary>
    Private _editeazaValoareaLaAfisare As Boolean

    ''' <summary>The edited line. Meaningful only after <c>DialogResult.OK</c>.</summary>
    Public ReadOnly Property Linie As DdfDraftLinieA
        Get
            Return _linie
        End Get
    End Property

    ''' <param name="linie">The line to edit; it is COPIED, never written.</param>
    ''' <param name="clasificatii">What may be chosen: the server list minus what the other lines
    ''' use (the line's own classification included).</param>
    ''' <param name="alteCoduriIndicator">The indicator codes of the OTHER lines, so a minted
    ''' code does not collide with one of them.</param>
    ''' <param name="surse">The SSs the line may take: the <c>DefaProgram</c> rows of the
    ''' document's program. None = no SS known, nothing is filtered.</param>
    ''' <param name="sursaPropusa">The SS to preselect (K-BOT's selected SS); the line's own SS wins
    ''' over it. Ignored when it is not among <paramref name="surse"/>.</param>
    ''' <param name="nou">A new line (caption «Rand nou», button «Adauga randul») or a change.</param>
    ''' <param name="manual">The angajament is a K-BOT one («!»): decides which classifications
    ''' count as already in the angajament (<see cref="DdfSectiuneaAReguli.EsteInAngajament"/>).</param>
    Public Sub New(linie As DdfDraftLinieA, clasificatii As IEnumerable(Of DdfClasificatie),
                   alteCoduriIndicator As IEnumerable(Of String), surse As IEnumerable(Of DdfSursaProgram),
                   sursaPropusa As String, nou As Boolean, manual As Boolean)
        ArgumentNullException.ThrowIfNull(linie)
        InitializeComponent()
        _linie = Copiaza(linie)
        _idClsfInitial = linie.IdClsf
        _manual = manual
        If clasificatii IsNot Nothing Then
            ' Operator, 26.09.2026: the classifications the angajament already has come first, then
            ' the ones it has not used -- in the dropdown and in the find-as-you-type list alike.
            _toate.AddRange(DdfSectiuneaAReguli.InAngajamentIntai(
                clasificatii.Where(Function(c) c IsNot Nothing AndAlso Not c.EsteSeparator), manual))
        End If
        _surse = If(surse, Enumerable.Empty(Of DdfSursaProgram)()).
                 Where(Function(s) s IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(s.Ss)).ToList()
        _sursaPropusa = If(sursaPropusa, String.Empty).Trim()
        _alteCoduri = If(alteCoduriIndicator, Enumerable.Empty(Of String)()).ToList()
        _nou = nou
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' Opening
    ' ══════════════════════════════════════════════════════════════════════════

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        Try
            _seIncarca = True
            Try
                capBar.Text = If(_nou, "K-BOT — Rând nou în secțiunea A", "K-BOT — Rândul din secțiunea A")
                Text = capBar.Text
                btnOk.Text = If(_nou, "Adaugă rândul", "Păstrează rândul")

                ' The SS first: it decides what the classification list holds. The line's own SS
                ' wins, then the one proposed, then the only one. The field is NOT locked (operator,
                ' 26.09.2026): it is tied to the program, whatever its number of SSs.
                cmbSursa.Items.Clear()
                For Each s As DdfSursaProgram In _surse
                    cmbSursa.Items.Add(If(String.IsNullOrWhiteSpace(s.Denumire), s.Ss, s.Ss & " — " & s.Denumire))
                Next
                Dim iSursa As Integer = _surse.FindIndex(Function(s) DdfSectiuneaAReguli.AcelasiSs(s.Ss, _linie.Ss))
                If iSursa < 0 Then iSursa = _surse.FindIndex(Function(s) DdfSectiuneaAReguli.AcelasiSs(s.Ss, _sursaPropusa))
                If iSursa < 0 AndAlso _surse.Count = 1 Then iSursa = 0
                If iSursa >= 0 Then cmbSursa.SelectedIndex = iSursa
                cmbSursa.Enabled = _surse.Count > 0

                txtElement.Text = _linie.ElementFund
                txtParametrii.Text = _linie.ParametriiFund
                ' The values' one row (slice 0081-12): written by `ArataValorile`.
                _valCur = _linie.ValCur
                grdValori.ClearRows()
                grdValori.AddRow()
            Finally
                _seIncarca = False
            End Try

            Dim initiala As DdfClasificatie = _toate.FirstOrDefault(
                Function(c) c.IdClsf = _linie.IdClsf AndAlso _linie.IdClsf <> 0)
            AplicaSursa(initiala)

            ' Straight to what is still missing: the SS when several are offered and none is
            ' chosen, then the classification (Access's `Clsf_Enter` dropped the combo open for the
            ' same reason), otherwise the value -- whose editor opens once the window is shown.
            If _surse.Count > 0 AndAlso SursaAleasa().Length = 0 Then
                ActiveControl = cmbSursa
            ElseIf _nou OrElse _aleasa Is Nothing Then
                ActiveControl = cmbClasificatie
            Else
                ActiveControl = grdValori
                _editeazaValoareaLaAfisare = True
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditLinieAForm.OnLoad", ex)
            lblStare.Text = "Fereastra nu s-a putut pregăti. Detalii în jurnalul de erori."
        End Try
    End Sub

    Protected Overrides Sub OnShown(e As EventArgs)
        MyBase.OnShown(e)
        Try
            If _editeazaValoareaLaAfisare Then grdValori.EditCell(COL_VAL_CUR, 0)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditLinieAForm.OnShown", ex)
        End Try
    End Sub

    ''' <summary>The SS in the first field, or empty when none is chosen.</summary>
    Private Function SursaAleasa() As String
        Dim i As Integer = cmbSursa.SelectedIndex
        If i < 0 OrElse i >= _surse.Count Then Return String.Empty
        Return _surse(i).Ss.Trim()
    End Function

    ''' <summary>
    ''' Refills the classification list with the classifications of the chosen SS and keeps
    ''' <paramref name="pastreaza"/> selected when it is still among them. The line's own
    ''' classification always stays offered, even when its SS is another one (a line written
    ''' before the SS filter existed must still open).
    ''' </summary>
    Private Sub AplicaSursa(pastreaza As DdfClasificatie)
        Dim ss As String = SursaAleasa()
        Dim trebuieSursa As Boolean = _surse.Count > 0 AndAlso ss.Length = 0

        _oferite.Clear()
        If Not trebuieSursa Then
            _oferite.AddRange(_toate.Where(
                Function(c) String.IsNullOrWhiteSpace(ss) OrElse
                            DdfSectiuneaAReguli.AcelasiSs(c.Ss, ss) OrElse
                            (_idClsfInitial <> 0 AndAlso c.IdClsf = _idClsfInitial)))
        End If

        _seIncarca = True
        Try
            cmbClasificatie.Items.Clear()
            For Each c As DdfClasificatie In _oferite
                cmbClasificatie.Items.Add(Eticheta(c))
            Next
            ' `_oferite` keeps `_toate`'s order (already in the angajament first); the find list
            ' is told where that group ends, so typing does not mix the two.
            cmbClasificatie.FindFirstGroupCount =
                _oferite.Where(Function(c) DdfSectiuneaAReguli.EsteInAngajament(c, _manual)).Count()
            Dim i As Integer = If(pastreaza Is Nothing, -1, _oferite.FindIndex(Function(c) c.IdClsf = pastreaza.IdClsf))
            If i >= 0 Then
                cmbClasificatie.SelectedIndex = i
                _aleasa = _oferite(i)
            Else
                cmbClasificatie.SelectedIndex = -1
                cmbClasificatie.Text = String.Empty
                _aleasa = Nothing
            End If
        Finally
            _seIncarca = False
        End Try

        cmbClasificatie.Enabled = _oferite.Count > 0
        If trebuieSursa Then
            lblStare.Text = "Alegeți întâi sursa / sectorul."
        ElseIf _oferite.Count = 0 Then
            lblStare.Text = If(ss.Length = 0,
                "Nu există nicio clasificație de ales pentru acest angajament.",
                $"Nu există nicio clasificație de ales pentru sursa «{ss}».")
        Else
            lblStare.Text = $"{_oferite.Count} clasificații de ales."
        End If
        ArataClasificatia()
    End Sub

    ' Boundary UI (event handler): logged and swallowed.
    Private Sub CmbSursa_SelectedIndexChanged(sender As Object, e As EventArgs) _
        Handles cmbSursa.SelectedIndexChanged
        Try
            If _seIncarca Then Return
            AplicaSursa(_aleasa)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditLinieAForm.CmbSursa_SelectedIndexChanged", ex)
        End Try
    End Sub

    ''' <summary>What a classification shows in the list: the code, then the name -- what the
    ''' operator reads to tell two neighbouring classifications apart. The code is written as
    ''' forexecab writes it («65.04.02.20.01.01»), without Access's «.02» after the chapter.</summary>
    Private Shared Function Eticheta(c As DdfClasificatie) As String
        Return DdfSendInputs.ForexeClsf(c.Clsf) & " — " & c.Denumire
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' The classification and the values that follow from it
    ' ══════════════════════════════════════════════════════════════════════════

    Private Sub CmbClasificatie_SelectedIndexChanged(sender As Object, e As EventArgs) _
        Handles cmbClasificatie.SelectedIndexChanged
        Try
            If _seIncarca Then Return
            Dim i As Integer = cmbClasificatie.SelectedIndex
            If i < 0 OrElse i >= _oferite.Count Then Return
            Dim anterioara As DdfClasificatie = _aleasa
            _aleasa = _oferite(i)

            ' The element of fundamentation starts as the classification's name -- but a text the
            ' operator wrote themselves is not overwritten by a later pick.
            Dim element As String = If(txtElement.Text, String.Empty).Trim()
            If element.Length = 0 OrElse
               (anterioara IsNot Nothing AndAlso String.Equals(element, anterioara.Denumire, StringComparison.Ordinal)) Then
                txtElement.Text = _aleasa.Denumire
            End If

            ArataClasificatia()
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditLinieAForm.CmbClasificatie_SelectedIndexChanged", ex)
        End Try
    End Sub

    ''' <summary>The derived values of the chosen classification (or of the line, before one is
    ''' chosen): name and indicator code, then the values' row.</summary>
    Private Sub ArataClasificatia()
        If _aleasa Is Nothing Then
            lblDenumire.Text = "—"
            lblCodIndicator.Text = If(String.IsNullOrWhiteSpace(_linie.CodIndicator), "—", _linie.CodIndicator)
        Else
            lblDenumire.Text = _aleasa.Denumire
            lblCodIndicator.Text = If(String.IsNullOrWhiteSpace(_aleasa.CodIndicator),
                                      "unul nou, la adăugarea rândului",
                                      _aleasa.CodIndicator)
        End If
        ArataValorile()
    End Sub

    ''' <summary>
    ''' The values' one row: budget, receptions, available (budget - receptions), previous value,
    ''' current value, remaining (available - current). Only the current value is typed; the rest
    ''' follow the classification (or the line, before one is chosen).
    ''' </summary>
    Private Sub ArataValorile()
        If grdValori.RowCount = 0 Then Return
        Dim r As KBotDataRow = grdValori.Rows(0)
        Dim buget As Double = Bugetul()
        Dim valRec As Double = ValoareaReceptiilor()
        r(COL_BUGET) = buget
        r(COL_VAL_REC) = valRec
        r(COL_DISPONIBIL) = DdfSectiuneaAReguli.Disponibil(buget, valRec)
        r(COL_VAL_PREC) = ValoareaPrecedenta()
        r(COL_VAL_CUR) = _valCur
        r(COL_VAL_RAMASA) = DdfSectiuneaAReguli.ValoareRamasa(buget, valRec, _valCur)
        grdValori.ClearDirty()
        grdValori.Invalidate()
    End Sub

    ''' <summary>The current value, parsed ro-RO («1.234,56»); empty = 0. A value that is not a
    ''' number is refused in the cell, so it never reaches <see cref="_valCur"/>.</summary>
    Private Sub GrdValori_CellValidating(sender As Object, e As KBotCellValidatingEventArgs) _
        Handles grdValori.CellValidating
        Try
            If e.ColumnKey <> COL_VAL_CUR Then Return
            Dim valoare As Double
            If Not CitesteNumar(e.ProposedValue, valoare) Then
                KBotMessage.Show(Me, "Valoarea curentă nu este un număr.", "Secțiunea A",
                                 MessageBoxButtons.OK, MessageBoxIcon.Warning)
                e.Cancel = True
                Return
            End If
            e.ProposedValue = valoare
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditLinieAForm.GrdValori_CellValidating", ex)
            ' A validator that threw must not let the value through.
            e.Cancel = True
        End Try
    End Sub

    ' Boundary UI (event handler): logged and swallowed.
    Private Sub GrdValori_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) _
        Handles grdValori.CellValueChanged
        Try
            If e.ColumnKey <> COL_VAL_CUR Then Return
            Dim valoare As Double
            If Not CitesteNumar(e.NewValue, valoare) Then Return
            _valCur = valoare
            ArataValorile()
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditLinieAForm.GrdValori_CellValueChanged", ex)
        End Try
    End Sub

    Private Shared Function CitesteNumar(valoare As Object, ByRef numar As Double) As Boolean
        If valoare Is Nothing OrElse TypeOf valoare Is DBNull Then
            numar = 0.0R
            Return True
        End If
        If TypeOf valoare Is Double Then
            numar = DirectCast(valoare, Double)
            Return True
        End If
        Dim t As String = Convert.ToString(valoare, _roCulture).Trim()
        If t.Length = 0 Then
            numar = 0.0R
            Return True
        End If
        Return Double.TryParse(t, NumberStyles.Number, _roCulture, numar)
    End Function

    Private Function Bugetul() As Double
        Return If(_aleasa IsNot Nothing, _aleasa.Buget, _linie.Buget)
    End Function

    Private Function ValoareaPrecedenta() As Double
        Return If(_aleasa IsNot Nothing, _aleasa.ValPrec, _linie.ValPrec)
    End Function

    Private Function ValoareaReceptiilor() As Double
        Return If(_aleasa IsNot Nothing, _aleasa.ValRec, _linie.ValRec)
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' OK
    ' ══════════════════════════════════════════════════════════════════════════

    Private Sub BtnOk_Click(sender As Object, e As EventArgs) Handles btnOk.Click
        Try
            ' The combo gives its verdict on typed text only when it is left; OK can be reached
            ' with the focus still in it (Enter / AcceptButton).
            cmbClasificatie.CommitText()
            ' The same for the value typed in the grid: Enter on AcceptButton never reaches the
            ' grid's editor. A refused value keeps the window open (the grid said why).
            If Not grdValori.CommitPendingEdit() Then Return

            Dim probleme As New List(Of String)()
            If _surse.Count > 0 AndAlso SursaAleasa().Length = 0 Then
                probleme.Add("Alegeți sursa / sectorul.")
            ElseIf _aleasa Is Nothing Then
                probleme.Add("Alegeți o clasificație din listă.")
            End If
            If String.IsNullOrWhiteSpace(txtElement.Text) Then
                probleme.Add("Elementul de fundamentare este un câmp obligatoriu!")
            End If
            Dim valCur As Double = _valCur
            If valCur = 0.0R Then
                probleme.Add("Valoarea curentă este un câmp obligatoriu!")
            ElseIf valCur < 0.0R AndAlso
                   Math.Round(valCur + ValoareaPrecedenta(), 2) < Math.Round(ValoareaReceptiilor(), 2) Then
                ' `ValPrec + ValCur` (the line's ValTot) -- NOT the grid's «Val. ramasa».
                probleme.Add("Valoarea totală (precedentă + curentă) nu poate fi mai mică decât valoarea recepțiilor!")
            End If

            If probleme.Count > 0 Then
                KBotMessage.Show(Me, String.Join(vbCrLf, probleme), "Secțiunea A",
                                 MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            ScrieInLinie(valCur)
            DialogResult = DialogResult.OK
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditLinieAForm.BtnOk_Click", ex)
            KBotMessage.Show(Me, "Rândul nu a putut fi pregătit. Detalii în jurnalul de erori.",
                             "Secțiunea A", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>
    ''' Writes the fields into the copy -- the port of <c>cmbClsf_AfterUpdate</c> plus the text
    ''' fields. The classification's derived values ride with the list (precomputed per
    ''' classification for this angajament); an indicator code is minted only when the
    ''' angajament has none for the classification AND the line does not already carry one for
    ''' the SAME classification.
    ''' </summary>
    Private Sub ScrieInLinie(valCur As Double)
        Dim c As DdfClasificatie = _aleasa
        Dim alta As Boolean = c.IdClsf <> _idClsfInitial

        _linie.IdClsf = c.IdClsf
        _linie.Clsf = c.Clsf
        _linie.Ss = c.Ss
        _linie.IdUnitate = c.IdUnitate
        _linie.ValPrec = c.ValPrec
        _linie.ValRec = c.ValRec
        ' Slice 0081-12: a line built here is never from a reservation, so its budget is the
        ' angajament's FX_Indicatori.Credit_Bugetar for the classification.
        _linie.Buget = c.Buget
        If Not String.IsNullOrWhiteSpace(c.CodIndicator) Then
            _linie.CodIndicator = c.CodIndicator
        ElseIf alta OrElse String.IsNullOrWhiteSpace(_linie.CodIndicator) Then
            _linie.CodIndicator = DdfCodIndicator.GenereazaUnic(LUNGIME_COD_INDICATOR, _alteCoduri)
        End If

        _linie.ElementFund = txtElement.Text.Trim()
        _linie.ParametriiFund = If(txtParametrii.Text, String.Empty).Trim()
        ' The partner is NOT written here (slice 0081-09): it is the header's, the same on every
        ' line, and the page puts it on the line from the header.
        _linie.ValCur = valCur
        _linie.ValTot = Math.Round(valCur + _linie.ValPrec, 2)
    End Sub

    ''' <summary>A field-by-field copy of a line. <see cref="DdfDraftLinieA"/> is a plain class
    ''' with no clone of its own; every settable property is listed here.</summary>
    Friend Shared Function Copiaza(a As DdfDraftLinieA) As DdfDraftLinieA
        Dim b As New DdfDraftLinieA()
        CopiazaIn(a, b)
        Return b
    End Function

    ''' <summary>Writes every field of <paramref name="sursa"/> into <paramref name="tinta"/> --
    ''' used by the page to take an edited copy back into the line the draft holds.</summary>
    Friend Shared Sub CopiazaIn(sursa As DdfDraftLinieA, tinta As DdfDraftLinieA)
        tinta.TempId = sursa.TempId
        tinta.IdSecA = sursa.IdSecA
        tinta.CodAngajament = sursa.CodAngajament
        tinta.CodIndicator = sursa.CodIndicator
        tinta.IdClsf = sursa.IdClsf
        tinta.Clsf = sursa.Clsf
        tinta.Ss = sursa.Ss
        tinta.IdUnitate = sursa.IdUnitate
        tinta.ElementFund = sursa.ElementFund
        tinta.ParametriiFund = sursa.ParametriiFund
        tinta.CodPartener = sursa.CodPartener
        tinta.IdPartener = sursa.IdPartener
        tinta.PartInd = sursa.PartInd
        tinta.ValPrec = sursa.ValPrec
        tinta.ValCur = sursa.ValCur
        tinta.ValTot = sursa.ValTot
        tinta.Ramane = sursa.Ramane
        tinta.Buget = sursa.Buget
        tinta.ValRec = sursa.ValRec
        tinta.GrpIdrz = sursa.GrpIdrz
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' Theming
    ' ══════════════════════════════════════════════════════════════════════════

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim scheme As ThemeScheme = ThemeManager.Current
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            tlyCorp.BackColor = p.SurfaceAltColor
            For Each lbl As Label In {lblDenumire, lblCodIndicator}
                lbl.ForeColor = p.TextDimColor
                lbl.BackColor = Color.Transparent
            Next
            lblStare.ForeColor = p.TextDimColor
            lblStare.BackColor = Color.Transparent
            ButtonStyles.ApplySecondary(btnRenunta, scheme)
            ButtonStyles.ApplyPrimary(btnOk, scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditLinieAForm.OnThemeChanged", ex)
        End Try
    End Sub
End Class
