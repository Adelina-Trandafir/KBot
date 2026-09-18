Option Strict On
Imports System
Imports System.Drawing
Imports System.Text
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' The bench of slice 0066 -- <see cref="KBotTableLayoutPanel"/> with its measures computed per
''' DPI the way the tree and the grid compute theirs. Every number the slice promises is written
''' in the journal next to the state that produced it, so the verdict is not a guess:
''' <list type="bullet">
''' <item><b>Logical in, device out.</b> The probe table is authored at 90/180/12/110 columns,
''' 32/32/40/24 rows and padding 8, all LOGICAL. The readout prints, for each, authored → live
''' and the scale in force. On a 150% monitor the live numbers must be exactly 1.5x (whole
''' pixels), not the platform's 1.43 x 1.67.</item>
''' <item><b>Same factor as the tree.</b> The tree on the right has <c>ItemHeight = 32</c>, the
''' same logical number as the first two rows: its scaled row and the table's live row must be
''' equal, under every scaling mode and text size.</item>
''' <item><b>The scaling modes</b>: Automatic, Fixed 100% and Manual x N, applied through
''' <c>AppScaling.Configure</c> like the options form does. Under Fixed 100% the table must go
''' back to its designer pixels while the platform-scaled controls around it stay large -- the
''' documented compromise of that mode, visible here on purpose.</item>
''' <item><b>Idempotence.</b> Switch scheme, drag the text size, change the mode, switch back:
''' the live values must be the same function of the logical ones every time, never a value
''' grown from a previous pass.</item>
''' <item><b>The runtime API</b>: collapse/expand a row and a column, rewrite row 0 to 60 logical
''' and back, rewrite the padding to 24 logical and back -- each survives the next scheme or
''' scale pass without any reset.</item>
''' </list>
''' Scheme, scaling mode and text size are put back on close: a bench never leaves a setting
''' behind.
''' </summary>
Public NotInheritable Class TableLayoutHarnessForm

    Private ReadOnly _log As Action(Of String)
    Private ReadOnly _originalScheme As ThemeScheme
    Private ReadOnly _originalTextScale As Single
    Private ReadOnly _originalMode As ScalingMode
    Private ReadOnly _originalFactor As Single
    Private _suppress As Boolean

    Public Sub New(log As Action(Of String))
        _log = log
        _originalScheme = ThemeManager.Current
        _originalTextScale = AppScaling.TextScale
        _originalMode = AppScaling.Mode
        _originalFactor = AppScaling.ManualFactor
        InitializeComponent()
        _suppress = True
        Try
            trkText.Value = Math.Max(trkText.Minimum, Math.Min(trkText.Maximum, CInt(Math.Round(AppScaling.TextScale * 100))))
            lblTextValue.Text = trkText.Value & "%"
            rdoAuto.Checked = (AppScaling.Mode = ScalingMode.Automatic)
            rdoFixed.Checked = (AppScaling.Mode = ScalingMode.Fixed100)
            rdoManual.Checked = (AppScaling.Mode = ScalingMode.Manual)
            numFactor.Value = CDec(Math.Max(CSng(numFactor.Minimum), Math.Min(CSng(numFactor.Maximum), AppScaling.ManualFactor)))
            chkThemedBorder.Checked = tlyProbe.ThemedCellBorder
            chkScaleStyles.Checked = tlyProbe.ScaleAbsoluteStyles
            chkAutoFit.Checked = tlyProbe.AutoFitToTheme
            PopulateTree()
        Finally
            _suppress = False
        End Try
    End Sub

    ' Six rows, so the tree has something to measure against the table's rows.
    Private Sub PopulateTree()
        Dim root As AdvancedTreeControl.TreeItem = treeProbe.AddItem("R", "Rânduri de 32px logic", Nothing, pExpanded:=True)
        For i As Integer = 1 To 5
            treeProbe.AddItem("R" & i, "rândul " & i, root)
        Next
    End Sub

    ' ── Lifecycle ────────────────────────────────────────────────────────────────

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)   ' theme, fit, placement -- all done by the base
        Try
            AddHandler AppScaling.ScalingChanged, AddressOf OnScalingChangedReadout
            lblActive.Text = "activ: " & ThemeManager.Current.Name
            Note("deschis pe " & Screen.FromHandle(Handle).DeviceName & " (DeviceDpi " & DeviceDpi & ")")
            Readout("la deschidere")
        Catch ex As Exception
            GlobalErrorLog.Write("TableLayoutHarnessForm.OnLoad", ex)
        End Try
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        Try
            RemoveHandler AppScaling.ScalingChanged, AddressOf OnScalingChangedReadout
            ' Put back what the bench moved. Scheme, mode and text size persist, so the file
            ' ends as it began.
            If _originalScheme IsNot Nothing AndAlso Not ReferenceEquals(ThemeManager.Current, _originalScheme) Then
                ThemeManager.SetScheme(_originalScheme)
            End If
            If Math.Abs(AppScaling.TextScale - _originalTextScale) > 0.0001F Then AppScaling.SetTextScale(_originalTextScale)
            If AppScaling.Mode <> _originalMode OrElse Math.Abs(AppScaling.ManualFactor - _originalFactor) > 0.0001F Then
                AppScaling.Configure(_originalMode, _originalFactor)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("TableLayoutHarnessForm.OnFormClosed", ex)
        Finally
            MyBase.OnFormClosed(e)
        End Try
    End Sub

    ' The base refits AFTER OnThemeChanged; the readout is posted so it reads the values the
    ' theme pass produced, not the ones before it.
    Protected Overrides Sub OnThemeChanged()
        MyBase.OnThemeChanged()
        Try
            lblActive.Text = "activ: " & ThemeManager.Current.Name
            If IsHandleCreated Then BeginInvoke(Sub() Readout("după schema " & ThemeManager.Current.Name))
        Catch ex As Exception
            GlobalErrorLog.Write("TableLayoutHarnessForm.OnThemeChanged", ex)
        End Try
    End Sub

    Private Sub OnScalingChangedReadout(sender As Object, e As EventArgs)
        Try
            If IsHandleCreated Then BeginInvoke(Sub() Readout("după scalare (" & ModeText() & ", text " & CInt(Math.Round(AppScaling.TextScale * 100)) & "%)"))
        Catch ex As Exception
            GlobalErrorLog.Write("TableLayoutHarnessForm.OnScalingChangedReadout", ex)
        End Try
    End Sub

    ' ── Scheme ───────────────────────────────────────────────────────────────────

    Private Sub OnClassic(sender As Object, e As EventArgs) Handles btnClassic.Click
        Switch(BuiltInSchemes.Classic())
    End Sub

    Private Sub OnDark(sender As Object, e As EventArgs) Handles btnDark.Click
        Switch(BuiltInSchemes.Dark())
    End Sub

    Private Sub OnModern(sender As Object, e As EventArgs) Handles btnModern.Click
        Switch(BuiltInSchemes.Modern())
    End Sub

    Private Sub OnColorful(sender As Object, e As EventArgs) Handles btnColorful.Click
        Switch(BuiltInSchemes.Colorful())
    End Sub

    Private Sub Switch(scheme As ThemeScheme)
        Try
            Note("comută schema → " & scheme.Name)
            ThemeManager.SetScheme(scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("TableLayoutHarnessForm.Switch", ex)
            Note("EROARE la comutare: " & ex.Message)
        End Try
    End Sub

    ' ── Scaling mode and text size ───────────────────────────────────────────────

    Private Sub btnApplyMode_Click(sender As Object, e As EventArgs) Handles btnApplyMode.Click
        Try
            Dim mode As ScalingMode = If(rdoFixed.Checked, ScalingMode.Fixed100, If(rdoManual.Checked, ScalingMode.Manual, ScalingMode.Automatic))
            Dim factor As Single = CSng(numFactor.Value)
            Note("scara K-BOT → " & mode.ToString() & If(mode = ScalingMode.Manual, " x" & factor.ToString("0.00"), ""))
            AppScaling.Configure(mode, factor)   ' broadcasts to every open form, this one included
        Catch ex As Exception
            GlobalErrorLog.Write("TableLayoutHarnessForm.btnApplyMode_Click", ex)
            Note("EROARE la schimbarea scării: " & ex.Message)
        End Try
    End Sub

    Private Sub trkText_ValueChanged(sender As Object, e As EventArgs) Handles trkText.ValueChanged
        lblTextValue.Text = trkText.Value & "%"
    End Sub

    ' Applied at the END of the gesture, like the options form: applying rescales this very
    ' window, and the slider would move under the finger.
    Private Sub trkText_GestureDone(sender As Object, e As EventArgs) Handles trkText.MouseUp, trkText.KeyUp, trkText.Leave
        Try
            If _suppress Then Return
            Dim wanted As Single = trkText.Value / 100.0F
            If Math.Abs(wanted - AppScaling.TextScale) < 0.0001F Then Return
            Note("mărime text → " & trkText.Value & "%")
            AppScaling.SetTextScale(wanted)
        Catch ex As Exception
            GlobalErrorLog.Write("TableLayoutHarnessForm.trkText_GestureDone", ex)
            Note("EROARE la scalare: " & ex.Message)
        End Try
    End Sub

    ' ── The table's switches ─────────────────────────────────────────────────────

    Private Sub chkThemedBorder_CheckedChanged(sender As Object, e As EventArgs) Handles chkThemedBorder.CheckedChanged
        If _suppress Then Return
        tlyProbe.ThemedCellBorder = chkThemedBorder.Checked
        Note("linii de celulă din temă: " & If(tlyProbe.ThemedCellBorder, "DA", "NU (culorile sistemului)"))
    End Sub

    Private Sub chkScaleStyles_CheckedChanged(sender As Object, e As EventArgs) Handles chkScaleStyles.CheckedChanged
        If _suppress Then Return
        tlyProbe.ScaleAbsoluteStyles = chkScaleStyles.Checked
        Note("măsurile fixe + marginea la scara K-BOT: " & If(tlyProbe.ScaleAbsoluteStyles, "DA", "NU (rămân pixelii din designer)"))
        Readout("după comutatorul de scară al tabelului")
    End Sub

    Private Sub chkAutoFit_CheckedChanged(sender As Object, e As EventArgs) Handles chkAutoFit.CheckedChanged
        If _suppress Then Return
        tlyProbe.AutoFitToTheme = chkAutoFit.Checked
        Note("rândurile fixe cresc la conținut: " & If(tlyProbe.AutoFitToTheme, "DA", "NU"))
        Readout("după comutatorul de potrivire")
    End Sub

    ' ── The runtime API ──────────────────────────────────────────────────────────

    Private Sub btnCollapseRow_Click(sender As Object, e As EventArgs) Handles btnCollapseRow.Click
        Try
            lblBanda.Visible = False
            tlyProbe.SetRowCollapsed(3, True)
            Note("rândul 3 strâns prin SetRowCollapsed (autorat " & tlyProbe.DebugAuthoredRow(3).ToString("0") & " → acum " & tlyProbe.RowStyles(3).Height.ToString("0") & ")")
            Readout("după strângerea rândului 3")
        Catch ex As Exception
            GlobalErrorLog.Write("TableLayoutHarnessForm.btnCollapseRow_Click", ex)
            Note("EROARE: " & ex.Message)
        End Try
    End Sub

    Private Sub btnExpandRow_Click(sender As Object, e As EventArgs) Handles btnExpandRow.Click
        Try
            If Not tlyProbe.IsRowCollapsed(3) Then
                Note("rândul 3 nu e strâns -- nimic de desfăcut")
                Return
            End If
            tlyProbe.SetRowCollapsed(3, False)
            lblBanda.Visible = True
            Note("rândul 3 desfăcut (autorat " & tlyProbe.DebugAuthoredRow(3).ToString("0") & " → acum " & tlyProbe.RowStyles(3).Height.ToString("0") & ")")
            Readout("după desfacerea rândului 3")
        Catch ex As Exception
            GlobalErrorLog.Write("TableLayoutHarnessForm.btnExpandRow_Click", ex)
            Note("EROARE: " & ex.Message)
        End Try
    End Sub

    Private Sub btnCollapseCol_Click(sender As Object, e As EventArgs) Handles btnCollapseCol.Click
        Try
            lblData.Visible = False
            lblSuma.Visible = False
            btnRenunta.Visible = False
            tlyProbe.SetColumnCollapsed(3, True)
            Note("coloana 3 strânsă prin SetColumnCollapsed (autorat " & tlyProbe.DebugAuthoredColumn(3).ToString("0") & " → acum " & tlyProbe.ColumnStyles(3).Width.ToString("0") & ")")
            Readout("după strângerea coloanei 3")
        Catch ex As Exception
            GlobalErrorLog.Write("TableLayoutHarnessForm.btnCollapseCol_Click", ex)
            Note("EROARE: " & ex.Message)
        End Try
    End Sub

    Private Sub btnExpandCol_Click(sender As Object, e As EventArgs) Handles btnExpandCol.Click
        Try
            If Not tlyProbe.IsColumnCollapsed(3) Then
                Note("coloana 3 nu e strânsă -- nimic de desfăcut")
                Return
            End If
            tlyProbe.SetColumnCollapsed(3, False)
            lblData.Visible = True
            lblSuma.Visible = True
            btnRenunta.Visible = True
            Note("coloana 3 desfăcută (autorat " & tlyProbe.DebugAuthoredColumn(3).ToString("0") & " → acum " & tlyProbe.ColumnStyles(3).Width.ToString("0") & ")")
            Readout("după desfacerea coloanei 3")
        Catch ex As Exception
            GlobalErrorLog.Write("TableLayoutHarnessForm.btnExpandCol_Click", ex)
            Note("EROARE: " & ex.Message)
        End Try
    End Sub

    Private Sub btnRowTall_Click(sender As Object, e As EventArgs) Handles btnRowTall.Click
        WriteRow0(60.0F)
    End Sub

    Private Sub btnRowBack_Click(sender As Object, e As EventArgs) Handles btnRowBack.Click
        WriteRow0(32.0F)
    End Sub

    Private Sub WriteRow0(logical As Single)
        Try
            tlyProbe.SetRowHeight(0, logical)
            Note("SetRowHeight(0, " & logical.ToString("0") & ") → live " & tlyProbe.RowStyles(0).Height.ToString("0") & " (scara " & tlyProbe.DpiScale.ToString("0.00") & ")")
            Readout("după SetRowHeight")
        Catch ex As Exception
            GlobalErrorLog.Write("TableLayoutHarnessForm.WriteRow0", ex)
            Note("EROARE: " & ex.Message)
        End Try
    End Sub

    Private Sub btnPadWide_Click(sender As Object, e As EventArgs) Handles btnPadWide.Click
        WritePadding(24)
    End Sub

    Private Sub btnPadBack_Click(sender As Object, e As EventArgs) Handles btnPadBack.Click
        WritePadding(8)
    End Sub

    Private Sub WritePadding(logical As Integer)
        Try
            tlyProbe.Padding = New Padding(logical)
            Note("Padding = " & logical & " logic → live " & PaddingText(tlyProbe.PaddingPx))
            Readout("după Padding")
        Catch ex As Exception
            GlobalErrorLog.Write("TableLayoutHarnessForm.WritePadding", ex)
            Note("EROARE: " & ex.Message)
        End Try
    End Sub

    Private Sub btnReadout_Click(sender As Object, e As EventArgs) Handles btnReadout.Click
        Readout("la cerere")
    End Sub

    ' ── Readout ──────────────────────────────────────────────────────────────────

    ''' <summary>
    ''' One journal line per fact and the same text on the table's own face: the scale in force,
    ''' every fixed column and row as authored → live, the padding, the host's fixed column, and
    ''' the tree's row next to the table's row 0.
    ''' </summary>
    Private Sub Readout(moment As String)
        Try
            If IsDisposed OrElse Not IsHandleCreated Then Return
            Dim k As Single = tlyProbe.DpiScale
            Dim sb As New StringBuilder()
            sb.Append("scara ").Append(k.ToString("0.00")).Append(" (").Append(ModeText()).Append(", DeviceDpi ").Append(DeviceDpi).Append(", text ").Append(CInt(Math.Round(AppScaling.TextScale * 100))).Append("%)")
            sb.Append("   fereastra: AutoScale ").Append(AutoScaleMode.ToString()).Append(" ").Append(AutoScaleDimensions.Width.ToString("0")).Append(" dpi, client ").Append(SizeText(ClientSize))
            sb.Append(", zoom pus ").Append(AppScaling.AppliedZoomOf(Me).ToString("0.00")).Append(", tabel scris la ").Append(tlyProbe.DesignDpi.ToString("0")).Append(" dpi")
            sb.AppendLine()

            sb.Append("coloane autorat→acum: ")
            For i As Integer = 0 To tlyProbe.ColumnStyles.Count - 1
                If tlyProbe.ColumnStyles(i).SizeType <> SizeType.Absolute Then Continue For
                sb.Append(i).Append(":").Append(tlyProbe.DebugAuthoredColumn(i).ToString("0")).Append("→").Append(tlyProbe.ColumnStyles(i).Width.ToString("0"))
                If tlyProbe.IsColumnCollapsed(i) Then sb.Append("(strânsă)")
                sb.Append("  ")
            Next
            sb.AppendLine()

            sb.Append("rânduri autorat→acum: ")
            For i As Integer = 0 To tlyProbe.RowStyles.Count - 1
                If tlyProbe.RowStyles(i).SizeType <> SizeType.Absolute Then Continue For
                sb.Append(i).Append(":").Append(tlyProbe.DebugAuthoredRow(i).ToString("0")).Append("→").Append(tlyProbe.RowStyles(i).Height.ToString("0"))
                If tlyProbe.IsRowCollapsed(i) Then sb.Append("(strâns)")
                sb.Append("  ")
            Next
            sb.AppendLine()

            sb.Append("padding ").Append(PaddingText(tlyProbe.Padding)).Append(" → ").Append(PaddingText(tlyProbe.PaddingPx))
            sb.Append("   gazda col.1: ").Append(tlyHost.DebugAuthoredColumn(1).ToString("0")).Append("→").Append(tlyHost.ColumnStyles(1).Width.ToString("0"))
            sb.AppendLine()

            Dim treeRow As Integer = CInt(Math.Round(treeProbe.ItemHeight * treeProbe.DpiScaleY))
            Dim tableRow As Integer = CInt(Math.Round(tlyProbe.RowStyles(0).Height))
            sb.Append("arbore ItemHeight ").Append(treeProbe.ItemHeight).Append("→").Append(treeRow)
            sb.Append("  vs  rândul 0 al tabelului ").Append(tableRow)
            sb.Append(If(treeRow = tableRow, "  = ACELAȘI", "  ≠ DIFERIT"))
            sb.Append("   cerere tabel ").Append(SizeText(tlyProbe.GetPreferredSize(Size.Empty)))

            lblInfo.Text = sb.ToString()
            For Each line As String In sb.ToString().Split({Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
                Note("[" & moment & "] " & line)
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("TableLayoutHarnessForm.Readout", ex)
        End Try
    End Sub

    Private Shared Function ModeText() As String
        Select Case AppScaling.Mode
            Case ScalingMode.Fixed100 : Return "fix 100%"
            Case ScalingMode.Manual : Return "manual x" & AppScaling.ManualFactor.ToString("0.00")
            Case Else : Return "automat"
        End Select
    End Function

    Private Shared Function PaddingText(p As Padding) As String
        If p.Left = p.Top AndAlso p.Top = p.Right AndAlso p.Right = p.Bottom Then Return p.Left.ToString()
        Return p.Left & "," & p.Top & "," & p.Right & "," & p.Bottom
    End Function

    Private Shared Function SizeText(s As Size) As String
        Return s.Width & "×" & s.Height
    End Function

    Private Sub Note(text As String)
        Try
            _log(text)
            lstLog.Items.Insert(0, DateTime.Now.ToString("HH:mm:ss") & "  " & text)
        Catch ex As Exception
            GlobalErrorLog.Write("TableLayoutHarnessForm.Note", ex)
        End Try
    End Sub

End Class
