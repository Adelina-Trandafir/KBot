Imports System
Imports System.Collections.Generic
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

' Playground KBotDataView: fiecare comutator din panoul stânga scrie într-o proprietate a grilei
' și repictează live. Fiind KBotThemedForm, butoanele de temă re-tematizează și grila.
' Toți handlerii sunt boundary UI: loghează și ÎNGHIT (nu aruncă în bucla de mesaje).
Public NotInheritable Class DataViewPlaygroundForm

    Private ReadOnly _log As Action(Of String)
    Private ReadOnly _originalScheme As ThemeScheme
    Private _loading As Boolean = True     ' cât e True, sincronizarea controale→grilă e suspendată
    Private ReadOnly _stari As String() = {"Nou", "În lucru", "Definitivat", "Anulat"}

    Public Sub New(log As Action(Of String))
        _log = log
        _originalScheme = ThemeManager.Current
        InitializeComponent()
        cboAutoSize.Items.AddRange(New Object() {"None", "ToContent"})
        cboFill.Items.AddRange(New Object() {"None", "FirstColumn", "LastColumn", "Proportional"})
        cboRowCount.Items.AddRange(New Object() {"12", "200", "5000"})
        BuildColumns()
        SeedRows(12)
        PopulateColumnCombo()
        SyncGridControls()
        _loading = False
        LoadColumnInspector()
        RefreshInfo()
    End Sub

    Private Sub OnClosedRestore(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        If _originalScheme IsNot Nothing AndAlso Not ReferenceEquals(ThemeManager.Current, _originalScheme) Then
            ThemeManager.SetScheme(_originalScheme)
        End If
    End Sub

    ' La terminarea redimensionării ferestrei layout-ul (deci și auto-hide) e complet: abia
    ' atunci info-ul e corect. Așa se vede live cum coloanele AutoHide dispar când îngustezi.
    Private Sub OnResizeEndRefresh(sender As Object, e As EventArgs) Handles MyBase.ResizeEnd
        RefreshInfo()
    End Sub

    ' ── Construcția grilei ───────────────────────────────────────────────────────
    Private Sub BuildColumns()
        grid.BeginUpdate()
        grid.AddColumn("nr", "Nr.", KBotColumnType.Text, 70)
        grid.AddColumn("cod", "Cod indicator", KBotColumnType.Text, 150)
        Dim stare = grid.AddColumn("stare", "Stare", KBotColumnType.Combo, 120)
        stare.ComboItems = New List(Of Object)(_stari)   ' forma sigură (String() nu e IList(Of Object))
        grid.AddColumn("activ", "Activ", KBotColumnType.CheckBox, 60)
        grid.AddColumn("optA", "Var. A", KBotColumnType.OptionButton, 70).OptionGroup = "varianta"
        grid.AddColumn("optB", "Var. B", KBotColumnType.OptionButton, 70).OptionGroup = "varianta"
        grid.AddColumn("det", "Detalii", KBotColumnType.Button, 100)
        Dim prg = grid.AddColumn("prog", "Execuție", KBotColumnType.ProgressBar, 140)
        prg.ProgressMin = 0 : prg.ProgressMax = 100
        For c As Integer = 0 To 5
            Dim col = grid.AddColumn("val" & c.ToString(), "Valoare " & c.ToString(),
                                     KBotColumnType.Text, 110)
            col.FormatString = "N2"
            col.TextAlign = ContentAlignment.MiddleRight
        Next
        grid.FrozenColumnCount = 1
        grid.EndUpdate()
    End Sub

    ' Reîncarcă rândurile. O valoare deliberat FOARTE lată e pusă la rândul 300, ca diferența
    ' dintre AutoSizeSampleRows = 200 (o ratează) și 0 (o prinde) să fie vizibilă.
    Private Sub SeedRows(count As Integer)
        Try
            grid.BeginUpdate()
            grid.ClearRows()
            For r As Integer = 0 To count - 1
                Dim row = grid.AddRow()
                row("nr") = r + 1
                row("cod") = If(r = 300, "COD-FOARTE-LAT-" & New String("X"c, 40), "IND-" & (r + 1).ToString("D5"))
                row("stare") = _stari(r Mod _stari.Length)
                row("activ") = (r Mod 3 = 0)
                row("optA") = (r Mod 2 = 0)
                row("optB") = (r Mod 2 <> 0)
                row("prog") = (r * 13) Mod 101
                For c As Integer = 0 To 5
                    Dim v As Double = (r * 7.5 + c * 1.25)
                    row("val" & c.ToString()) = If(r Mod 7 = 0, -v, v)
                Next
            Next
            grid.EndUpdate()
        Catch ex As Exception
            GlobalErrorLog.Write("DataViewPlaygroundForm.SeedRows", ex)
        End Try
    End Sub

    Private Sub PopulateColumnCombo()
        cboColumn.Items.Clear()
        For Each c In grid.Columns
            cboColumn.Items.Add(c.HeaderText & "  [" & c.Key & "]")
        Next
        If cboColumn.Items.Count > 0 Then cboColumn.SelectedIndex = 0
    End Sub

    ' ── Formatare condiționată (ca să se VADĂ Enabled/disable la runtime) ────────
    Private Sub Grid_RowFormatting(sender As Object, e As KBotRowFormattingEventArgs) Handles grid.RowFormatting
        Dim s As Object = e.Row("stare")
        If s IsNot Nothing AndAlso String.Equals(s.ToString(), "Anulat", StringComparison.Ordinal) Then
            e.Enabled = False
        End If
    End Sub

    Private Sub Grid_CellFormatting(sender As Object, e As KBotCellFormattingEventArgs) Handles grid.CellFormatting
        If Not e.ColumnKey.StartsWith("val", StringComparison.Ordinal) Then Return
        Dim v As Object = e.Value
        If v Is Nothing Then Return
        Dim d As Double
        If Double.TryParse(v.ToString(), d) AndAlso d < 0 Then
            e.ForeColor = ThemeManager.Current.Palette.ErrorColor
        End If
    End Sub

    ' Feedback vizibil ca butonul de comandă să fie testabil (butonul e acțiune, rămâne activ
    ' și cu ReadOnlyGrid). Boundary UI: loghează și înghite.
    Private Sub Grid_ButtonClick(sender As Object, e As KBotButtonClickEventArgs) Handles grid.ButtonClick
        Try
            lblInfo.Text = $"Buton «{e.ColumnKey}» apăsat pe rândul {e.RowIndex + 1}"
            _log($"ButtonClick: {e.ColumnKey} @ rând {e.RowIndex + 1}")
        Catch ex As Exception
            GlobalErrorLog.Write("DataViewPlaygroundForm.Grid_ButtonClick", ex)
        End Try
    End Sub

    ' ── Temă ─────────────────────────────────────────────────────────────────────
    Private Sub btnClassic_Click(sender As Object, e As EventArgs) Handles btnClassic.Click
        SwitchScheme(BuiltInSchemes.Classic())
    End Sub
    Private Sub btnDark_Click(sender As Object, e As EventArgs) Handles btnDark.Click
        SwitchScheme(BuiltInSchemes.Dark())
    End Sub
    Private Sub btnModern_Click(sender As Object, e As EventArgs) Handles btnModern.Click
        SwitchScheme(BuiltInSchemes.Modern())
    End Sub
    Private Sub SwitchScheme(scheme As ThemeScheme)
        Try
            ThemeManager.SetScheme(scheme)
            _log("temă → " & scheme.Name)
            RefreshInfo()
        Catch ex As Exception
            GlobalErrorLog.Write("DataViewPlaygroundForm.SwitchScheme", ex)
        End Try
    End Sub

    ' ── Comutatoare grid-wide ────────────────────────────────────────────────────
    Private Sub cboAutoSize_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboAutoSize.SelectedIndexChanged
        Apply(Sub() grid.AutoSizeColumnsMode = CType(cboAutoSize.SelectedIndex, KBotAutoSizeMode))
    End Sub
    Private Sub cboFill_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboFill.SelectedIndexChanged
        Apply(Sub() grid.ColumnFillMode = CType(cboFill.SelectedIndex, KBotFillMode))
    End Sub
    Private Sub numSample_ValueChanged(sender As Object, e As EventArgs) Handles numSample.ValueChanged
        Apply(Sub() grid.AutoSizeSampleRows = CInt(numSample.Value))
    End Sub
    Private Sub numFrozen_ValueChanged(sender As Object, e As EventArgs) Handles numFrozen.ValueChanged
        Apply(Sub() grid.FrozenColumnCount = CInt(numFrozen.Value))
    End Sub
    Private Sub numRowH_ValueChanged(sender As Object, e As EventArgs) Handles numRowH.ValueChanged
        Apply(Sub() grid.RowHeight = CInt(numRowH.Value))
    End Sub
    Private Sub numHeaderH_ValueChanged(sender As Object, e As EventArgs) Handles numHeaderH.ValueChanged
        Apply(Sub() grid.HeaderHeight = CInt(numHeaderH.Value))
    End Sub
    Private Sub chkHeader_CheckedChanged(sender As Object, e As EventArgs) Handles chkHeader.CheckedChanged
        Apply(Sub() grid.ShowHeader = chkHeader.Checked)
    End Sub
    Private Sub chkAlt_CheckedChanged(sender As Object, e As EventArgs) Handles chkAlt.CheckedChanged
        Apply(Sub() grid.AlternatingRows = chkAlt.Checked)
    End Sub
    Private Sub chkReadOnly_CheckedChanged(sender As Object, e As EventArgs) Handles chkReadOnly.CheckedChanged
        Apply(Sub() grid.ReadOnlyGrid = chkReadOnly.Checked)
    End Sub
    Private Sub btnAutoSize_Click(sender As Object, e As EventArgs) Handles btnAutoSize.Click
        Apply(Sub() grid.AutoSizeColumns())
    End Sub
    Private Sub btnReset_Click(sender As Object, e As EventArgs) Handles btnReset.Click
        Apply(Sub() grid.ResetColumnSizing())
    End Sub
    Private Sub cboRowCount_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboRowCount.SelectedIndexChanged
        Apply(Sub() SeedRows(Integer.Parse(CStr(cboRowCount.SelectedItem))))
    End Sub

    ' ── Inspector de coloană ─────────────────────────────────────────────────────
    Private Sub cboColumn_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboColumn.SelectedIndexChanged
        LoadColumnInspector()
    End Sub
    Private Sub chkColVisible_CheckedChanged(sender As Object, e As EventArgs) Handles chkColVisible.CheckedChanged
        ApplyColumn()
    End Sub
    Private Sub chkColEnabled_CheckedChanged(sender As Object, e As EventArgs) Handles chkColEnabled.CheckedChanged
        ApplyColumn()
    End Sub
    Private Sub chkColReadOnly_CheckedChanged(sender As Object, e As EventArgs) Handles chkColReadOnly.CheckedChanged
        ApplyColumn()
    End Sub
    Private Sub chkColAutoHide_CheckedChanged(sender As Object, e As EventArgs) Handles chkColAutoHide.CheckedChanged
        ApplyColumn()
    End Sub
    Private Sub numColWidth_ValueChanged(sender As Object, e As EventArgs) Handles numColWidth.ValueChanged
        ApplyColumn()
    End Sub
    Private Sub numColMin_ValueChanged(sender As Object, e As EventArgs) Handles numColMin.ValueChanged
        ApplyColumn()
    End Sub
    Private Sub numColMax_ValueChanged(sender As Object, e As EventArgs) Handles numColMax.ValueChanged
        ApplyColumn()
    End Sub

    Private Function SelectedColumn() As KBotDataColumn
        Dim i As Integer = cboColumn.SelectedIndex
        If i < 0 OrElse i >= grid.Columns.Count Then Return Nothing
        Return grid.Columns(i)
    End Function

    Private Sub LoadColumnInspector()
        Dim col = SelectedColumn()
        If col IsNot Nothing Then
            _loading = True
            Try
                chkColVisible.Checked = col.Visible
                chkColEnabled.Checked = col.Enabled
                chkColReadOnly.Checked = col.[ReadOnly]
                chkColAutoHide.Checked = col.AutoHide
                SetNum(numColWidth, col.Width)
                SetNum(numColMin, col.MinWidth)
                ' MaxWidth = Integer.MaxValue (sau peste raza numericului) => 0 «neplafonat».
                SetNum(numColMax, If(col.MaxWidth > CInt(numColMax.Maximum), 0, col.MaxWidth))
            Finally
                _loading = False
            End Try
        End If
        UpdateDependentControls()
    End Sub

    ' Activează/dezactivează controalele care NU au efect în starea curentă, ca panoul să
    ' reflecte DOAR combinațiile valide (ex. eșantionul contează doar la ToContent).
    Private Sub UpdateDependentControls()
        Dim toContent As Boolean = (grid.AutoSizeColumnsMode = KBotAutoSizeMode.ToContent)
        Dim hasFill As Boolean = (grid.ColumnFillMode <> KBotFillMode.None)

        ' Eșantionarea alimentează DOAR trecerea de măsurare la conținut.
        lblSample.Enabled = toContent
        numSample.Enabled = toContent

        ' Înălțimea antetului n-are efect dacă antetul e ascuns.
        lblHeaderH.Enabled = grid.ShowHeader
        numHeaderH.Enabled = grid.ShowHeader

        ' AutoSizeColumns() e no-op în modul pur manual (None + None fără umplere).
        btnAutoSize.Enabled = toContent OrElse hasFill
        ' ResetColumnSizing() (curăță UserSized => re-măsoară) contează doar la ToContent.
        btnReset.Enabled = toContent

        ' Lățimea manuală a coloanei e suprascrisă de măsurare => editabilă doar în modul None.
        lblColWidth.Enabled = Not toContent
        numColWidth.Enabled = Not toContent

        ' ReadOnly are efect doar pe coloane editabile / comutabile (nu Button/ProgressBar).
        Dim col = SelectedColumn()
        chkColReadOnly.Enabled = col IsNot Nothing AndAlso
            (col.ColumnType = KBotColumnType.Text OrElse col.ColumnType = KBotColumnType.Combo OrElse
             col.ColumnType = KBotColumnType.CheckBox OrElse col.ColumnType = KBotColumnType.OptionButton)
    End Sub

    Private Sub ApplyColumn()
        If _loading Then Return
        Dim col = SelectedColumn()
        If col Is Nothing Then Return
        Try
            col.MinWidth = CInt(numColMin.Value)
            col.MaxWidth = If(numColMax.Value = 0D, Integer.MaxValue, CInt(numColMax.Value))
            col.Width = CInt(numColWidth.Value)
            col.Visible = chkColVisible.Checked
            col.Enabled = chkColEnabled.Checked
            col.[ReadOnly] = chkColReadOnly.Checked
            col.AutoHide = chkColAutoHide.Checked
            grid.AutoSizeColumns()     ' modelul coloanei nu are back-reference => forțăm trecerea
            RefreshInfo()
            UpdateDependentControls()  ' re-evaluează activările (fără a rescrie valorile în curs de editare)
        Catch ex As Exception
            GlobalErrorLog.Write("DataViewPlaygroundForm.ApplyColumn", ex)
        End Try
    End Sub

    ' ── Ajutoare ─────────────────────────────────────────────────────────────────
    ' Aplică o schimbare grid-wide (suprimată în timpul încărcării), apoi reîmprospătează info.
    Private Sub Apply(action As Action)
        If _loading Then Return
        Try
            action()
            RefreshInfo()
            ' O schimbare grid-wide poate re-măsura lățimile și schimba ce controale au sens:
            ' reîncarcă inspectorul (valori) + re-evaluează activările.
            LoadColumnInspector()
        Catch ex As Exception
            GlobalErrorLog.Write("DataViewPlaygroundForm.Apply", ex)
        End Try
    End Sub

    Private Sub SyncGridControls()
        cboAutoSize.SelectedIndex = CInt(grid.AutoSizeColumnsMode)
        cboFill.SelectedIndex = CInt(grid.ColumnFillMode)
        SetNum(numSample, grid.AutoSizeSampleRows)
        SetNum(numFrozen, grid.FrozenColumnCount)
        SetNum(numRowH, grid.RowHeight)
        SetNum(numHeaderH, grid.HeaderHeight)
        chkHeader.Checked = grid.ShowHeader
        chkAlt.Checked = grid.AlternatingRows
        chkReadOnly.Checked = grid.ReadOnlyGrid
        cboRowCount.SelectedIndex = 0
    End Sub

    Private Shared Sub SetNum(n As NumericUpDown, value As Integer)
        Dim v As Decimal = value
        If v < n.Minimum Then v = n.Minimum
        If v > n.Maximum Then v = n.Maximum
        n.Value = v
    End Sub

    ' Rezumat live: câte coloane AFIȘATE (efectiv), câte ascunse automat, Σlățimi vs lățimea
    ' client (util pentru fill/overflow/auto-hide). „Afișate” = IsEffectivelyVisible.
    Private Sub RefreshInfo()
        Dim shownN As Integer = 0
        Dim autoHiddenN As Integer = 0
        Dim sumW As Integer = 0
        For Each c In grid.Columns
            If c.IsEffectivelyVisible Then
                shownN += 1
                sumW += c.Width
            ElseIf c.Visible Then
                autoHiddenN += 1     ' vizibilă pentru caller, dar ascunsă automat (nu încape)
            End If
        Next
        Dim hidden As String = If(autoHiddenN > 0, $" • {autoHiddenN} ascunse auto", String.Empty)
        lblInfo.Text = $"{grid.RowCount:N0} rânduri • {shownN} col. afișate{hidden} • Σlățimi={sumW}px • client={grid.ClientSize.Width}px"
    End Sub
End Class
