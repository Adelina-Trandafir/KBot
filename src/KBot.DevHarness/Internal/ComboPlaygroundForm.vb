Imports System
Imports System.Text
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

' The KBotComboBox bench.
'
' WHY A PROPERTY GRID AND NOT A HANDFUL OF SWITCHES. The combo publishes around a dozen editable
' properties (colours, CornerRadius, Editable, LimitToList, TextOffsetY...), each carrying its own
' Category and Description. A PropertyGrid bound to the control shows them ALL, grouped exactly
' as Visual Studio groups them, and falls behind nothing the next property adds.
'
' Above it sit only the things a property grid CANNOT give: reloading the sample list, clearing
' the selection, and the application scale -- because the answer to "did TextOffsetY actually move the
' edit box" is only visible by reading the native EDIT's real rectangle back from Windows, next
' to the logical number that was typed.
'
' Every handler is a UI boundary: log and SWALLOW (never throw into the message loop).
Public NotInheritable Class ComboPlaygroundForm

    Private ReadOnly _log As Action(Of String)
    Private ReadOnly _originalScheme As ThemeScheme
    Private ReadOnly _originalMode As ScalingMode
    Private ReadOnly _originalFactor As Single
    Private _loading As Boolean = True     ' while True, control -> quick-switch syncing is suspended

    Private Shared ReadOnly SAMPLE_ITEMS As String() = {
        "Achiziții publice", "Angajamente bugetare", "Buget local", "Contabilitate",
        "Deconturi", "Executie bugetara", "Facturi furnizori", "Investiții",
        "Ordonanțare", "Rezervări de credite", "Salarizare", "Trezorerie"}

    Public Sub New(log As Action(Of String))
        _log = log
        _originalScheme = ThemeManager.Current
        _originalMode = AppScaling.Mode
        _originalFactor = AppScaling.ManualFactor

        InitializeComponent()

        cboScaling.Items.AddRange(New Object() {"Automat (DPI-ul ecranului)", "Fix 100%", "Manual"})
        cboScaling.SelectedIndex = CInt(AppScaling.Mode)
        numManual.Value = CDec(AppScaling.ManualFactor)

        prop.SelectedObject = cbo
        JumpToKBotCategories()
        LoadSample()

        _loading = False
        UpdateDependentControls()
        RefreshReadout()
    End Sub

    ' ── Opening and closing ──────────────────────────────────────────────────────

    ' The theme AND the scale are persisted operator settings. The bench moves them so their
    ' effect can be seen, so the bench is also the one that puts them back -- otherwise a
    ' five-minute look at a control would rewrite the operator's preferences.
    Private Sub OnClosedRestore(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        Try
            If AppScaling.Mode <> _originalMode OrElse AppScaling.ManualFactor <> _originalFactor Then
                AppScaling.Configure(_originalMode, _originalFactor)
            End If
            If _originalScheme IsNot Nothing AndAlso Not ReferenceEquals(ThemeManager.Current, _originalScheme) Then
                ThemeManager.SetScheme(_originalScheme)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("ComboPlaygroundForm.OnClosedRestore", ex)
        End Try
    End Sub

    ' Once the window has finished resizing, the combo's real rectangle is settled: only then is
    ' the measurement true.
    Private Sub OnResizeEndRefresh(sender As Object, e As EventArgs) Handles MyBase.ResizeEnd
        RefreshReadout()
    End Sub

    ' The first true measurement: before Shown there is no native EDIT handle yet.
    Private Sub OnShownRefresh(sender As Object, e As EventArgs) Handles MyBase.Shown
        RefreshReadout()
    End Sub

    ''' <summary>
    ''' The property grid's colours do not go through the theme's generic rules -- it owns
    ''' surfaces of its own (the view, the help pane, the lines) -- so they are written here, on
    ''' every scheme change.
    ''' </summary>
    Protected Overrides Sub OnThemeChanged()
        MyBase.OnThemeChanged()
        Try
            Dim p As ThemePalette = ThemeManager.Current.Palette
            prop.ViewBackColor = p.InputBackColor
            prop.ViewForeColor = p.InputTextColor
            prop.LineColor = p.BorderColor
            prop.CategoryForeColor = p.TextColor
            prop.HelpBackColor = p.SurfaceColor
            prop.HelpForeColor = p.TextDimColor
            prop.BackColor = p.SurfaceColor
            lblReadout.ForeColor = p.TextColor
        Catch ex As Exception
            GlobalErrorLog.Write("ComboPlaygroundForm.OnThemeChanged", ex)
        End Try
    End Sub

    ' ── The switches above the grid ──────────────────────────────────────────────

    Private Sub chkEditable_CheckedChanged(sender As Object, e As EventArgs) Handles chkEditable.CheckedChanged
        Apply(Sub() cbo.Editable = chkEditable.Checked)
    End Sub

    Private Sub chkLimitToList_CheckedChanged(sender As Object, e As EventArgs) Handles chkLimitToList.CheckedChanged
        Apply(Sub() cbo.LimitToList = chkLimitToList.Checked)
    End Sub

    Private Sub numTextOffsetY_ValueChanged(sender As Object, e As EventArgs) Handles numTextOffsetY.ValueChanged
        Apply(Sub() cbo.TextOffsetY = CInt(numTextOffsetY.Value))
    End Sub

    Private Sub btnReloadItems_Click(sender As Object, e As EventArgs) Handles btnReloadItems.Click
        Apply(AddressOf LoadSample)
    End Sub

    Private Sub btnClearSelection_Click(sender As Object, e As EventArgs) Handles btnClearSelection.Click
        Apply(Sub() cbo.SelectedIndex = -1)
    End Sub

    ' The splitter moves the grid's edge too, but raises no ResizeEnd on the FORM -- without this
    ' the measurement would be reading rectangles from before the drag.
    Private Sub splLeft_SplitterMoved(sender As Object, e As SplitterEventArgs) Handles splLeft.SplitterMoved
        RefreshReadout()
    End Sub

    ' ── Theme and scale ──────────────────────────────────────────────────────────

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
            RefreshReadout()
        Catch ex As Exception
            GlobalErrorLog.Write("ComboPlaygroundForm.SwitchScheme", ex)
        End Try
    End Sub

    Private Sub cboScaling_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboScaling.SelectedIndexChanged
        Apply(AddressOf ApplyScaling)
    End Sub

    Private Sub numManual_ValueChanged(sender As Object, e As EventArgs) Handles numManual.ValueChanged
        Apply(AddressOf ApplyScaling)
    End Sub

    ''' <summary>
    ''' Moves the application-wide scale. It is the only answer to «why did the text move further
    ''' than I typed»: <c>TextOffsetY</c> is a LOGICAL measure at 96 dpi, so on a 150 % screen the
    ''' nudge reaches the glass multiplied by 1.5.
    ''' </summary>
    Private Sub ApplyScaling()
        AppScaling.Configure(CType(cboScaling.SelectedIndex, ScalingMode), CSng(numManual.Value))
        _log($"scară → {cboScaling.SelectedItem} ({AppScaling.FactorFor(cbo):0.00})")
    End Sub

    ' ── The property grid ────────────────────────────────────────────────────────

    ' A property written in the grid can change anything else (Editable recreates the HWND,
    ' TextOffsetY moves the edit box), so both the quick switches and the measurement are taken again.
    Private Sub prop_PropertyValueChanged(sender As Object, e As PropertyValueChangedEventArgs) Handles prop.PropertyValueChanged
        Try
            _log("proprietate: " & e.ChangedItem.Label & " = " & Convert.ToString(e.ChangedItem.Value))
            SyncQuickControls()
            UpdateDependentControls()
            RefreshReadout()
        Catch ex As Exception
            GlobalErrorLog.Write("ComboPlaygroundForm.prop_PropertyValueChanged", ex)
        End Try
    End Sub

    ' ── Helpers ──────────────────────────────────────────────────────────────────

    ''' <summary>One change (suppressed while loading), then everything read back.</summary>
    Private Sub Apply(action As Action)
        If _loading Then Return
        Try
            action()
            prop.Refresh()
            SyncQuickControls()
            UpdateDependentControls()
            RefreshReadout()
        Catch ex As Exception
            GlobalErrorLog.Write("ComboPlaygroundForm.Apply", ex)
        End Try
    End Sub

    ''' <summary>The quick switches read the combo back, without writing to it.</summary>
    Private Sub SyncQuickControls()
        Dim wasLoading As Boolean = _loading
        _loading = True
        Try
            chkEditable.Checked = cbo.Editable
            chkLimitToList.Checked = cbo.LimitToList
            numTextOffsetY.Value = Math.Max(numTextOffsetY.Minimum, Math.Min(numTextOffsetY.Maximum, CDec(cbo.TextOffsetY)))
        Finally
            _loading = wasLoading
        End Try
    End Sub

    ''' <summary>Turns off whatever has no effect in the current state, so the panel cannot
    ''' lie about what it controls.</summary>
    Private Sub UpdateDependentControls()
        lblManual.Enabled = (AppScaling.Mode = ScalingMode.Manual)
        numManual.Enabled = (AppScaling.Mode = ScalingMode.Manual)
        ' LimitToList and TextOffsetY both have nothing to do without a native EDIT to act on.
        chkLimitToList.Enabled = cbo.Editable
        lblTextOffsetY.Enabled = cbo.Editable
        numTextOffsetY.Enabled = cbo.Editable
    End Sub

    ''' <summary>
    ''' Brings the grid to the first «K-BOT ...» category. Sorted by category, the list opens on
    ''' Accessibility, Appearance, Behavior -- the properties inherited from ComboBox, which are
    ''' not the ones the bench is opened for.
    ''' </summary>
    Private Sub JumpToKBotCategories()
        Try
            Dim root As GridItem = prop.SelectedGridItem
            If root Is Nothing Then Return
            While root.Parent IsNot Nothing
                root = root.Parent
            End While
            For Each g As GridItem In root.GridItems
                If g.GridItemType = GridItemType.Category AndAlso
                   g.Label IsNot Nothing AndAlso g.Label.StartsWith("K-BOT", StringComparison.Ordinal) Then
                    g.Expanded = True
                    prop.SelectedGridItem = g
                    Return
                End If
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("ComboPlaygroundForm.JumpToKBotCategories", ex)
        End Try
    End Sub

    Private Sub LoadSample()
        cbo.DataSource = Nothing
        cbo.Items.Clear()
        cbo.Items.AddRange(SAMPLE_ITEMS)
        If cbo.Items.Count > 0 Then cbo.SelectedIndex = 0
    End Sub

    ''' <summary>
    ''' What ACTUALLY happened: the scale factor, the combo's own face rectangle, and -- only
    ''' when <see cref="KBotComboBox.Editable"/> is on -- the native EDIT's real rectangle read
    ''' back from Windows, next to the logical <c>TextOffsetY</c> that was typed. That is the only
    ''' proof the nudge moved anything at all.
    ''' </summary>
    Private Sub RefreshReadout()
        Try
            Dim factor As Single = AppScaling.FactorFor(cbo)
            Dim sb As New StringBuilder()

            sb.AppendLine($"Scară: {cboScaling.SelectedItem}  •  factor {factor:0.00}  •  DeviceDpi {cbo.DeviceDpi}")
            sb.AppendLine($"Combo: {cbo.Width}×{cbo.Height} px  •  Editable {cbo.Editable}  •  LimitToList {cbo.LimitToList}")
            sb.AppendLine($"Selecție: index {cbo.SelectedIndex}  •  text «{cbo.Text}»")

            If Not cbo.Editable Then
                sb.Append("TextOffsetY: fără efect -- combo-ul nu e Editable (nu există EDIT nativ).")
            Else
                Dim editRect As Rectangle = NativeMethods.GetComboEditBounds(cbo)
                Dim offsetPx As Integer = ThemeShapes.ScaleDpi(cbo, cbo.TextOffsetY)
                sb.Append($"TextOffsetY: {cbo.TextOffsetY} logic → {offsetPx} px  •  EDIT real: " &
                          If(editRect.IsEmpty, "(indisponibil)",
                             $"{editRect.Width}×{editRect.Height} px la ({editRect.Left},{editRect.Top})"))
            End If

            lblReadout.Text = sb.ToString()
        Catch ex As Exception
            GlobalErrorLog.Write("ComboPlaygroundForm.RefreshReadout", ex)
        End Try
    End Sub
End Class
