Imports System
Imports System.Diagnostics
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

' Proba vizuală a KBotDataView (slice 0010-02): încarcă date sintetice masive
' (ROWS × COLS) și lasă operatorul să deruleze, să comute tema și să dea verdictul.
' Fiind KBotThemedForm, comutarea temei re-tematizează live și grila (IThemedControl).
Public NotInheritable Class DataViewHarnessForm

    Private Const ROWS As Integer = 5000
    Private Const COLS As Integer = 20

    Private ReadOnly _log As Action(Of String)
    Private ReadOnly _originalScheme As ThemeScheme

    Public Sub New(log As Action(Of String))
        _log = log
        _originalScheme = ThemeManager.Current   ' de restaurat la închidere
        InitializeComponent()
        SeedSyntheticData()
    End Sub

    ' Restaurează schema activă dinainte de probă (proba nu schimbă tema aplicației).
    Private Sub OnClosedRestore(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        If _originalScheme IsNot Nothing AndAlso Not ReferenceEquals(ThemeManager.Current, _originalScheme) Then
            ThemeManager.SetScheme(_originalScheme)
        End If
    End Sub

    ' Umple grila cu ROWS × COLS celule sintetice și cronometrează încărcarea.
    Private Sub SeedSyntheticData()
        Try
            Dim sw As Stopwatch = Stopwatch.StartNew()

            grid.BeginUpdate()

            ' Câte una din fiecare din cele ȘASE tipuri de coloană (acceptanța 0010-03),
            ' apoi restul până la COLS ca text numeric. Prima coloană e înghețată, ca să se
            ' vadă banda non-scrolling la derulare orizontală.
            grid.AddColumn("nr", "Nr.", KBotColumnType.Text, 70).Aggregate = KBotAggregate.Count
            grid.AddColumn("cod", "Cod indicator", KBotColumnType.Text, 150)
            ' Coloană deliberat prea îngustă pentru textul ei (MaxWidth oprește auto-size-ul din
            ' a o lăți): singurul fel în care se poate VEDEA eticheta plutitoare de celulă
            ' (slice 0028) — stai cu cursorul pe o denumire tăiată.
            Dim den = grid.AddColumn("den", "Denumire", KBotColumnType.Text, 160)
            den.MaxWidth = 160
            den.ValueType = KBotValueType.Text
            den.Aggregate = KBotAggregate.CountDistinct
            grid.AddColumn("stare", "Stare", KBotColumnType.Combo, 120)
            grid.AddColumn("activ", "Activ", KBotColumnType.CheckBox, 60)
            grid.AddColumn("optA", "Var. A", KBotColumnType.OptionButton, 70).OptionGroup = "varianta"
            grid.AddColumn("optB", "Var. B", KBotColumnType.OptionButton, 70).OptionGroup = "varianta"
            grid.AddColumn("det", "Detalii", KBotColumnType.Button, 100)
            Dim prg = grid.AddColumn("prog", "Execuție", KBotColumnType.ProgressBar, 140)
            prg.ProgressMin = 0
            prg.ProgressMax = 100
            For c As Integer = 8 To COLS - 1
                Dim col = grid.AddColumn("c" & c.ToString(), "Valoare " & c.ToString(),
                                         KBotColumnType.Text, 110)
                col.FormatString = "N2"
                col.TextAlign = ContentAlignment.MiddleRight
                ' English (slice 0017-01): numeric columns sum in the footer band; the first one
                ' averages, so the harness eyeballs Sum, Count and Average side by side.
                ' ValueType comes FIRST — Sum/Average are only offered to numeric columns (0028).
                col.ValueType = KBotValueType.Number
                col.Aggregate = If(c = 8, KBotAggregate.Average, KBotAggregate.Sum)
            Next
            grid.FrozenColumnCount = 1

            Dim stari As String() = {"Nou", "În lucru", "Definitivat", "Anulat"}
            For r As Integer = 0 To ROWS - 1
                Dim row = grid.AddRow()
                row("nr") = r + 1
                row("cod") = "IND-" & (r + 1).ToString("D5")
                row("den") = "Denumire lungă de indicator bugetar, poziția " & (r + 1).ToString() &
                             " din nomenclatorul de probă"
                row("stare") = stari(r Mod stari.Length)
                row("activ") = (r Mod 3 = 0)
                row("optA") = (r Mod 2 = 0)
                row("optB") = (r Mod 2 <> 0)
                row("prog") = (r * 13) Mod 101
                For c As Integer = 8 To COLS - 1
                    ' Din când în când valori NEGATIVE, ca regula de formatare condiționată
                    ' (text roșu) să aibă ce colora.
                    Dim v As Double = (r * 7.5 + c * 1.25)
                    row("c" & c.ToString()) = If(r Mod 7 = 0, -v, v)
                Next
            Next

            grid.EndUpdate()
            sw.Stop()

            lblInfo.Text = $"{ROWS:N0} rânduri × {COLS} coloane — încărcate în {sw.ElapsedMilliseconds} ms"
            _log($"KBotDataView: {ROWS} × {COLS} încărcate în {sw.ElapsedMilliseconds} ms")
            _log("Derulează vertical/orizontal; prima coloană trebuie să rămână înghețată.")
        Catch ex As Exception
            GlobalErrorLog.Write("DataViewHarnessForm.SeedSyntheticData", ex)
            Throw
        End Try
    End Sub

    ' ── Reguli de formatare condiționată (acceptanța 0010-04) ───────────────────

    ' Stare = «Anulat» => tot rândul devine inert (și se pictează șters).
    Private Sub Grid_RowFormatting(sender As Object, e As KBotRowFormattingEventArgs) Handles grid.RowFormatting
        Dim stare As Object = e.Row("stare")
        If stare IsNot Nothing AndAlso String.Equals(stare.ToString(), "Anulat", StringComparison.Ordinal) Then
            e.Enabled = False
        End If
    End Sub

    ' Valorile numerice negative se scriu cu roșul de EROARE al paletei (nu o culoare literală).
    Private Sub Grid_CellFormatting(sender As Object, e As KBotCellFormattingEventArgs) Handles grid.CellFormatting
        If Not e.ColumnKey.StartsWith("c", StringComparison.Ordinal) Then Return
        Dim v As Object = e.Value
        If v Is Nothing Then Return
        Dim d As Double
        If Double.TryParse(v.ToString(), d) AndAlso d < 0 Then
            e.ForeColor = ThemeManager.Current.Palette.ErrorColor
        End If
    End Sub

    ' Marchează coloanele numerice ca AutoHide și pune fill pe ultima coloană, ca îngustarea
    ' ferestrei să le ascundă pe rând, iar ultima să umple golul. Debifat => revine la normal.
    Private Sub chkAutoHide_CheckedChanged(sender As Object, e As EventArgs) Handles chkAutoHide.CheckedChanged
        Try
            For Each c In grid.Columns
                c.AutoHide = chkAutoHide.Checked AndAlso c.Key.StartsWith("c", StringComparison.Ordinal)
            Next
            grid.ColumnFillMode = If(chkAutoHide.Checked, KBotFillMode.LastColumn, KBotFillMode.None)
            grid.AutoSizeColumns()
            _log("AutoHide pe coloanele numerice = " & chkAutoHide.Checked.ToString() &
                 " (fill = " & grid.ColumnFillMode.ToString() & "); îngustează fereastra ca să vezi efectul")
        Catch ex As Exception
            GlobalErrorLog.Write("DataViewHarnessForm.chkAutoHide_CheckedChanged", ex)
        End Try
    End Sub

    ' Comută banda de totaluri (slice 0017-01): agregatele sunt deja setate pe coloane la seed,
    ' deci aici doar se arată/ascunde rândul pinat.
    Private Sub chkTotals_CheckedChanged(sender As Object, e As EventArgs) Handles chkTotals.CheckedChanged
        Try
            grid.FooterVisible = chkTotals.Checked
            _log("FooterVisible = " & grid.FooterVisible.ToString() &
                 " (Nr.=Count, coloanele numerice=Sum, prima=Average)")
        Catch ex As Exception
            GlobalErrorLog.Write("DataViewHarnessForm.chkTotals_CheckedChanged", ex)
        End Try
    End Sub

    ' Butonul de strângere din subsol (slice 0028). Trăiește în banda de subsol, deci o
    ' aprinde și pe aceea — fără subsol n-ar avea unde se desena.
    Private Sub chkCollapseButton_CheckedChanged(sender As Object, e As EventArgs) Handles chkCollapseButton.CheckedChanged
        Try
            If chkCollapseButton.Checked AndAlso Not chkTotals.Checked Then chkTotals.Checked = True
            grid.CollapseButton = chkCollapseButton.Checked
            _log("CollapseButton = " & grid.CollapseButton.ToString() &
                 " (apasă unghiul din colțul subsolului; axa = " & grid.CollapseDirection.ToString() & ")")
        Catch ex As Exception
            GlobalErrorLog.Write("DataViewHarnessForm.chkCollapseButton_CheckedChanged", ex)
        End Try
    End Sub

    ' Axa strângerii: pe lățime (ca arborele) sau pe înălțime (rămân antetul + subsolul).
    Private Sub chkCollapseVertical_CheckedChanged(sender As Object, e As EventArgs) Handles chkCollapseVertical.CheckedChanged
        Try
            grid.CollapseDirection = If(chkCollapseVertical.Checked,
                                        KBotCollapseDirection.Vertical, KBotCollapseDirection.Horizontal)
            _log("CollapseDirection = " & grid.CollapseDirection.ToString() &
                 If(chkCollapseVertical.Checked,
                    " (strânsă, rămân cele două benzi — agregatele stau sub ochi)",
                    " (strânsă la MinimumCollapsedWidth = " & grid.MinimumCollapsedWidth.ToString() & "px)"))
        Catch ex As Exception
            GlobalErrorLog.Write("DataViewHarnessForm.chkCollapseVertical_CheckedChanged", ex)
        End Try
    End Sub

    ' Grila e andocată în harness, deci lățimea/înălțimea sunt ale GAZDEI: controlul doar ridică
    ' evenimentul, iar strângerea o face gazda. Aici e suficient s-o spunem în jurnal — mutarea
    ' unui splitter e treaba lui MainForm, nu a bancului de probă.
    Private Sub Grid_CollapsedChanged(collapsed As Boolean) Handles grid.CollapsedChanged
        Try
            _log("CollapsedChanged -> " & collapsed.ToString() &
                 " (HostOwnsWidth = " & grid.HostOwnsWidth.ToString() &
                 ", HostOwnsHeight = " & grid.HostOwnsHeight.ToString() & ")")
        Catch ex As Exception
            GlobalErrorLog.Write("DataViewHarnessForm.Grid_CollapsedChanged", ex)
        End Try
    End Sub

    ' Comută derularea orizontală pe coloane (aliniere la margini) vs. pixel cu pixel.
    Private Sub chkScrollByColumn_CheckedChanged(sender As Object, e As EventArgs) Handles chkScrollByColumn.CheckedChanged
        Try
            grid.ScrollByColumn = chkScrollByColumn.Checked
            _log("ScrollByColumn = " & grid.ScrollByColumn.ToString())
        Catch ex As Exception
            GlobalErrorLog.Write("DataViewHarnessForm.chkScrollByColumn_CheckedChanged", ex)
        End Try
    End Sub

    Private Sub btnClassic_Click(sender As Object, e As EventArgs) Handles btnClassic.Click
        SwitchScheme(BuiltInSchemes.Classic())
    End Sub

    Private Sub btnDark_Click(sender As Object, e As EventArgs) Handles btnDark.Click
        SwitchScheme(BuiltInSchemes.Dark())
    End Sub

    Private Sub btnModern_Click(sender As Object, e As EventArgs) Handles btnModern.Click
        SwitchScheme(BuiltInSchemes.Modern())
    End Sub

    ' Boundary UI (handler de buton): loghează și înghite — nu arunca în bucla de mesaje.
    Private Sub SwitchScheme(scheme As ThemeScheme)
        Try
            ThemeManager.SetScheme(scheme)
            _log("temă → " & scheme.Name)
        Catch ex As Exception
            GlobalErrorLog.Write("DataViewHarnessForm.SwitchScheme", ex)
        End Try
    End Sub

End Class
