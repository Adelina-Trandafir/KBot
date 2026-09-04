Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Text
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

' The KBotRichTextEditor bench.
'
' WHY A PROPERTY GRID AND NOT FIFTY SWITCHES. The editor publishes around fifty editable
' properties, each carrying its own Category and Description. A PropertyGrid bound to the control
' shows them ALL, grouped exactly as Visual Studio groups them, and exercises the design-time
' surface on top of that: the key converters (the icon-key drop-downs), the ShouldSerialize
' behaviour and the default values. A hand-written list would cover less and would fall behind
' the first property somebody adds.
'
' Above it sit only the things a property grid CANNOT give: which ImageList is bound, in which
' ORDER it is bound (the .Designer.vb order is the one that used to hide the icons), and the
' application scale -- because every published height here is LOGICAL at 96 dpi, and the question
' "why is the header taller than the number I typed" is only answerable with the scale factor
' next to it.
'
' Every handler is a UI boundary: log and SWALLOW (never throw into the message loop).
Public NotInheritable Class RichTextPlaygroundForm

    Private ReadOnly _log As Action(Of String)
    Private ReadOnly _originalScheme As ThemeScheme
    Private ReadOnly _originalMode As ScalingMode
    Private ReadOnly _originalFactor As Single
    Private ReadOnly _sets As New List(Of ImageList)()
    Private _loading As Boolean = True     ' while True, control -> editor syncing is suspended

    ' The keys the editor looks for, in the order the buttons appear in the header.
    Private Shared ReadOnly KEYS As String() = {"bold", "italic", "underline",
                                                "text_forecolor", "text_backcolor", "collapse"}

    ' The letter drawn inside each generated icon, and its ink. Same order as KEYS.
    Private Shared ReadOnly GLYPHS As String() = {"B", "I", "U", "A", "H", "v"}
    Private Shared ReadOnly INKS As Color() = {Color.SteelBlue, Color.Firebrick, Color.SeaGreen,
                                               Color.DarkOrange, Color.MediumPurple, Color.DimGray}

    Public Sub New(log As Action(Of String))
        _log = log
        _originalScheme = ThemeManager.Current
        _originalMode = AppScaling.Mode
        _originalFactor = AppScaling.ManualFactor

        InitializeComponent()

        cboIconSet.Items.AddRange(New Object() {"(fără) — literele de rezervă", "16 px", "24 px",
                                                "64 px (mai mari decât butonul)", "amestecate (16 și 64)"})
        cboIconOrder.Items.AddRange(New Object() {"lista plină, apoi legată",
                                                  "cheile întâi, lista goală, umplută după (ordinea din .Designer.vb)"})
        cboLayout.Items.AddRange(New Object() {"Original", "Stretch", "Zoom", "Tile"})
        cboScaling.Items.AddRange(New Object() {"Automat (DPI-ul ecranului)", "Fix 100%", "Manual"})

        cboIconSet.SelectedIndex = 2
        cboIconOrder.SelectedIndex = 0
        cboLayout.SelectedIndex = CInt(edt.ButtonImageLayout)
        cboScaling.SelectedIndex = CInt(AppScaling.Mode)
        numManual.Value = CDec(AppScaling.ManualFactor)

        prop.SelectedObject = edt
        JumpToKBotCategories()
        LoadSample()
        BindIcons()

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
            edt.Images = Nothing
            For Each il As ImageList In _sets
                il.Dispose()
            Next
            _sets.Clear()

            If AppScaling.Mode <> _originalMode OrElse AppScaling.ManualFactor <> _originalFactor Then
                AppScaling.Configure(_originalMode, _originalFactor)
            End If
            If _originalScheme IsNot Nothing AndAlso Not ReferenceEquals(ThemeManager.Current, _originalScheme) Then
                ThemeManager.SetScheme(_originalScheme)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("RichTextPlaygroundForm.OnClosedRestore", ex)
        End Try
    End Sub

    ' Once the window has finished resizing, the header widths are settled: only then is the
    ' measurement true. That is how the pickers can be watched shrinking on a narrow window.
    Private Sub OnResizeEndRefresh(sender As Object, e As EventArgs) Handles MyBase.ResizeEnd
        RefreshReadout()
    End Sub

    ' The first true measurement: before Shown there are no laid-out rectangles yet.
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
            GlobalErrorLog.Write("RichTextPlaygroundForm.OnThemeChanged", ex)
        End Try
    End Sub

    ' ── Icons ────────────────────────────────────────────────────────────────────

    ''' <summary>
    ''' A drawn icon, not one out of a resx: the bench has to be able to hand out the SAME
    ''' picture at 16, 24 and 64 px, because the difference between <c>Original</c> and
    ''' <c>Zoom</c> only shows when the picture is not exactly the size of the button.
    ''' </summary>
    Private Shared Function DrawIcon(side As Integer, glyph As String, ink As Color) As Bitmap
        Dim bmp As New Bitmap(side, side)
        Using g As Graphics = Graphics.FromImage(bmp)
            g.SmoothingMode = SmoothingMode.AntiAlias
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias
            g.Clear(Color.Transparent)
            Using b As New SolidBrush(Color.FromArgb(40, ink))
                g.FillEllipse(b, 0, 0, side - 1, side - 1)
            End Using
            Using p As New Pen(ink, Math.Max(1.0F, side / 16.0F))
                g.DrawEllipse(p, 0, 0, side - 1, side - 1)
            End Using
            Using f As New Font("Segoe UI", side * 0.5F, FontStyle.Bold, GraphicsUnit.Pixel)
                Using b As New SolidBrush(ink)
                    Using sf As New StringFormat() With {.Alignment = StringAlignment.Center,
                                                         .LineAlignment = StringAlignment.Center}
                        g.DrawString(glyph, f, b, New RectangleF(0, 0, side, side), sf)
                    End Using
                End Using
            End Using
        End Using
        Return bmp
    End Function

    ''' <summary>A complete six-icon set, already filled and owned by the bench.</summary>
    Private Function NewSet(sides As Integer()) As ImageList
        Dim il As New ImageList()
        _sets.Add(il)
        FillSet(il, sides)
        Return il
    End Function

    ''' <summary>
    ''' Fills a list. <c>ImageSize</c> is the LARGEST side asked for, because an ImageList
    ''' resizes everything it is given to its own size: pinned at 16, the mixed set would come
    ''' back all-16 and would mix nothing.
    ''' </summary>
    Private Shared Sub FillSet(il As ImageList, sides As Integer())
        il.ColorDepth = ColorDepth.Depth32Bit
        il.ImageSize = New Size(sides.Max(), sides.Max())
        For i As Integer = 0 To KEYS.Length - 1
            il.Images.Add(KEYS(i), DrawIcon(sides(i Mod sides.Length), GLYPHS(i), INKS(i)))
        Next
    End Sub

    ''' <summary>
    ''' Binds the chosen set, in the chosen order.
    '''
    ''' <para>The second order is the one Visual Studio writes into a <c>.Designer.vb</c>: the
    ''' keys before the list (alphabetically), the list still empty when it is bound, the
    ''' pictures and the key names only afterwards. That is where the «the buttons show B, I, U»
    ''' bug lived.</para>
    ''' </summary>
    Private Sub BindIcons()
        Try
            edt.Images = Nothing
            For Each il As ImageList In _sets
                il.Dispose()
            Next
            _sets.Clear()

            Dim sides As Integer() = SidesForSelection()
            If sides Is Nothing Then
                ClearKeys()
                edt.RefreshButtonIcons()
                Return
            End If

            If cboIconOrder.SelectedIndex = 1 Then
                SetKeys()
                Dim empty As New ImageList()
                _sets.Add(empty)
                edt.Images = empty
                FillSet(empty, sides)
            Else
                edt.Images = NewSet(sides)
                SetKeys()
            End If

            ' The moment a real form would be getting its handle, and would therefore re-resolve
            ' on its own. Here it is asked for, because this editor was created long ago.
            edt.RefreshButtonIcons()
        Catch ex As Exception
            GlobalErrorLog.Write("RichTextPlaygroundForm.BindIcons", ex)
        End Try
    End Sub

    ''' <summary>The icon sides behind the chosen set, or <c>Nothing</c> for «no icons».</summary>
    Private Function SidesForSelection() As Integer()
        Select Case cboIconSet.SelectedIndex
            Case 1 : Return New Integer() {16}
            Case 2 : Return New Integer() {24}
            Case 3 : Return New Integer() {64}
            Case 4 : Return New Integer() {16, 64}
            Case Else : Return Nothing
        End Select
    End Function

    Private Sub SetKeys()
        edt.BoldImageKey = "bold"
        edt.ItalicImageKey = "italic"
        edt.UnderlineImageKey = "underline"
        edt.TextColorImageKey = "text_forecolor"
        edt.HighlightImageKey = "text_backcolor"
        edt.CollapseExpandedImageKey = "collapse"
        edt.CollapseCollapsedImageKey = "collapse"
    End Sub

    Private Sub ClearKeys()
        edt.BoldImageKey = String.Empty
        edt.ItalicImageKey = String.Empty
        edt.UnderlineImageKey = String.Empty
        edt.TextColorImageKey = String.Empty
        edt.HighlightImageKey = String.Empty
        edt.CollapseExpandedImageKey = String.Empty
        edt.CollapseCollapsedImageKey = String.Empty
    End Sub

    ' ── The switches above the grid ──────────────────────────────────────────────

    Private Sub cboIconSet_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboIconSet.SelectedIndexChanged
        Apply(AddressOf BindIcons)
    End Sub

    Private Sub cboIconOrder_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboIconOrder.SelectedIndexChanged
        Apply(AddressOf BindIcons)
    End Sub

    Private Sub cboLayout_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboLayout.SelectedIndexChanged
        Apply(Sub() edt.ButtonImageLayout = CType(cboLayout.SelectedIndex, RichTextImageLayout))
    End Sub

    Private Sub chkEditabil_CheckedChanged(sender As Object, e As EventArgs) Handles chkEditabil.CheckedChanged
        Apply(Sub() edt.Editabil = chkEditabil.Checked)
    End Sub

    Private Sub chkCollapsed_CheckedChanged(sender As Object, e As EventArgs) Handles chkCollapsed.CheckedChanged
        Apply(Sub()
                  ' Collapsed THROWS without the collapse button -- the box is only enabled when
                  ' it is allowed (UpdateDependentControls), and the guard repeats the rule here.
                  If edt.CollapseButton Then edt.Collapsed = chkCollapsed.Checked
              End Sub)
    End Sub

    Private Sub btnSample_Click(sender As Object, e As EventArgs) Handles btnSample.Click
        Apply(AddressOf LoadSample)
    End Sub

    ' The splitter moves the editor's edge too, but raises no ResizeEnd on the FORM -- without
    ' this the measurement would be reading rectangles from before the drag.
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
            GlobalErrorLog.Write("RichTextPlaygroundForm.SwitchScheme", ex)
        End Try
    End Sub

    Private Sub cboScaling_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboScaling.SelectedIndexChanged
        Apply(AddressOf ApplyScaling)
    End Sub

    Private Sub numManual_ValueChanged(sender As Object, e As EventArgs) Handles numManual.ValueChanged
        Apply(AddressOf ApplyScaling)
    End Sub

    ''' <summary>
    ''' Moves the application-wide scale. It is the only answer to «why is the header taller than
    ''' I wrote it»: <c>HeaderHeight</c> is a LOGICAL measure at 96 dpi, so on a 150 % screen it
    ''' reaches the glass multiplied by 1.5. On «Fix 100%» it comes back to the typed number.
    ''' </summary>
    Private Sub ApplyScaling()
        AppScaling.Configure(CType(cboScaling.SelectedIndex, ScalingMode), CSng(numManual.Value))
        _log($"scară → {cboScaling.SelectedItem} ({AppScaling.FactorFor(edt):0.00})")
    End Sub

    ' ── The property grid ────────────────────────────────────────────────────────

    ' A property written in the grid can change anything else (a height, a picture, the collapsed
    ' state), so both the quick switches and the measurement are taken again.
    Private Sub prop_PropertyValueChanged(sender As Object, e As PropertyValueChangedEventArgs) Handles prop.PropertyValueChanged
        Try
            _log("proprietate: " & e.ChangedItem.Label & " = " & Convert.ToString(e.ChangedItem.Value))
            SyncQuickControls()
            UpdateDependentControls()
            RefreshReadout()
        Catch ex As Exception
            GlobalErrorLog.Write("RichTextPlaygroundForm.prop_PropertyValueChanged", ex)
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
            GlobalErrorLog.Write("RichTextPlaygroundForm.Apply", ex)
        End Try
    End Sub

    ''' <summary>The quick switches read the editor back, without writing to it.</summary>
    Private Sub SyncQuickControls()
        Dim wasLoading As Boolean = _loading
        _loading = True
        Try
            cboLayout.SelectedIndex = CInt(edt.ButtonImageLayout)
            chkEditabil.Checked = edt.Editabil
            chkCollapsed.Checked = edt.Collapsed
        Finally
            _loading = wasLoading
        End Try
    End Sub

    ''' <summary>Turns off whatever has no effect in the current state, so the panel cannot
    ''' lie about what it controls.</summary>
    Private Sub UpdateDependentControls()
        lblManual.Enabled = (AppScaling.Mode = ScalingMode.Manual)
        numManual.Enabled = (AppScaling.Mode = ScalingMode.Manual)
        ' Without the collapse button there is no way back -- the setter THROWS, so the box goes.
        chkCollapsed.Enabled = edt.CollapseButton
        ' Binding order means nothing when there is nothing to bind.
        lblIconOrder.Enabled = cboIconSet.SelectedIndex > 0
        cboIconOrder.Enabled = cboIconSet.SelectedIndex > 0
    End Sub

    ''' <summary>
    ''' Brings the grid to the first «K-BOT ...» category. Sorted by category, the list opens on
    ''' Accessibility, Appearance, Behavior -- the properties inherited from UserControl, which
    ''' are not the ones the bench is opened for.
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
            GlobalErrorLog.Write("RichTextPlaygroundForm.JumpToKBotCategories", ex)
        End Try
    End Sub

    Private Sub LoadSample()
        edt.Rtf = "Descriere lungă de probă." & vbCrLf &
                  "Selectează un rând și apasă pe B / I / U din antet." & vbCrLf & vbCrLf &
                  "Rândul al treilea e aici doar ca subsolul să aibă ce număra."
    End Sub

    ''' <summary>The editor's header band, found by type (it is not published).</summary>
    Private Function HeaderBand() As KBotRichTextBand
        For Each c As Control In edt.Controls
            Dim band As KBotRichTextBand = TryCast(c, KBotRichTextBand)
            If band IsNot Nothing AndAlso band.Top = 0 Then Return band
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' What ACTUALLY happened: the scale factor, the header's real height in pixels next to the
    ''' logical number that was typed, the button size, and -- button by button -- whether a
    ''' picture landed on it or the fallback letter did. Those are the two questions this bench
    ''' exists to answer.
    ''' </summary>
    Private Sub RefreshReadout()
        Try
            Dim factor As Single = AppScaling.FactorFor(edt)
            Dim band As KBotRichTextBand = HeaderBand()
            Dim sb As New StringBuilder()

            sb.AppendLine($"Scară: {cboScaling.SelectedItem}  •  factor {factor:0.00}  •  DeviceDpi {edt.DeviceDpi}")
            sb.AppendLine($"Antet: HeaderHeight = {edt.HeaderHeight} logic → {If(band Is Nothing, 0, band.Height)} px pe sticlă" &
                          $"  •  vizibil {edt.HeaderVisible}")
            sb.AppendLine($"Butoane: ButtonSize = {edt.ButtonSize.Width}×{edt.ButtonSize.Height} logic" &
                          $"  •  ButtonImageLayout = {edt.ButtonImageLayout}")

            If band Is Nothing Then
                sb.Append("Pictograme: banda de antet nu a fost găsită.")
            Else
                sb.Append(IconLine(band))
            End If

            lblReadout.Text = sb.ToString()
        Catch ex As Exception
            GlobalErrorLog.Write("RichTextPlaygroundForm.RefreshReadout", ex)
        End Try
    End Sub

    ''' <summary>
    ''' One entry per header button, in the order they sit on screen; the last one is the
    ''' collapse button, pinned to the right edge.
    '''
    ''' <para>Deliberately NOT filtered on <c>Visible</c>: before the window is shown, a child
    ''' reports <c>Visible = False</c>, and the line would come out empty exactly when it is read
    ''' for the first time.</para>
    ''' </summary>
    Private Function IconLine(band As KBotRichTextBand) As String
        Dim buttons As New List(Of KBotNoFocusButton)()
        For Each c As Control In band.Controls
            Dim b As KBotNoFocusButton = TryCast(c, KBotNoFocusButton)
            If b IsNot Nothing Then buttons.Add(b)
        Next
        buttons.Sort(Function(x, y) x.Left.CompareTo(y.Left))
        If Not edt.CollapseButton AndAlso buttons.Count > 0 Then
            buttons.RemoveAt(buttons.Count - 1)
        End If

        Dim line As New StringBuilder("Pictograme: ")
        For Each b As KBotNoFocusButton In buttons
            If b.Image Is Nothing Then
                line.Append($"[literă {b.Text}] ")
            Else
                line.Append($"[imagine {b.Image.Width}×{b.Image.Height} în {b.Width}×{b.Height}] ")
            End If
        Next
        Return line.ToString()
    End Function
End Class
