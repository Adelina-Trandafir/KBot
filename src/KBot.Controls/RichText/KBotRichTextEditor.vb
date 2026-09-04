Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Text
Imports System.Runtime.InteropServices
Imports System.Windows.Forms
Imports KBot.Theming

''' <summary>
''' A themed rich-text editor -- the port of the <c>RTB</c> form from the <c>VBA_DDF_INFO</c>
''' project into an embeddable control (slice 0051).
'''
''' <para><b>Why this and not a plain text box.</b> The long description of a DDF revision is
''' stored twice: <c>FX_DDF_REV.Desc_Lunga</c> holds the RTF and <c>Desc_Lunga_ANSI</c> the
''' plain-text rendition of the same content. Access wrote both -- the RTF from
''' <c>rtbExplicatieLunga.Rtf</c> and the plain text from <c>.Text</c> -- and BOTH are still
''' written, because the signed XFA document is fed from the plain-text one and cannot take
''' RTF control words. So this control has to produce both, which means it has to be a real
''' rich-text surface, not a text box.</para>
'''
''' <para><b>What was left behind.</b> Everything in <c>Start.vb</c> and most of
''' <c>Helpers.vb</c>: the Access COM connection, <c>SetParent</c>, the parent-window monitor
''' and the handlers that pushed values back into Access controls. And the ATTACHMENT
''' buttons, deliberately -- the «Fisiere» page owns attachments now, so a second, half-wired
''' way to add them would be a trap rather than a convenience.</para>
'''
''' <para><b>The toolbar buttons do not take the focus</b>
''' (<see cref="KBotNoFocusButton"/>). Without that, clicking "bold" would move the focus off
''' the editor, the selection would collapse, and the command would apply to a caret instead
''' of to the words the operator picked.</para>
'''
''' <para><b>Everything the operator can see is a published property</b> -- the band metrics,
''' the paddings, the colours, the combo font, the button icons, the footer. They live in
''' <c>KBotRichTextEditor.Properties.vb</c>; the colour contract and
''' <see cref="ApplyTheme"/> in <c>KBotRichTextEditor.Theming.vb</c>. Nothing is laid out by a
''' TableLayoutPanel any more: <see cref="RebuildLayout"/> is the single place that turns those
''' numbers into rectangles, so a metric changed at design time or at runtime lands the same
''' way.</para>
'''
''' <para>Implements <see cref="IThemedControl"/> because it owns child controls: without it,
''' <c>ThemeManager.Traverse</c> plus the generic rules would repaint them wrongly. Implements
''' <see cref="IDpiScaledControl"/> because it keeps its own pixel metrics (C2): every public
''' number is LOGICAL at 96 dpi and gets scaled at layout time, never written back.</para>
''' </summary>
<ToolboxItem(True)>
<DefaultEvent("ContinutModificat")>
Public Class KBotRichTextEditor
    Implements IThemedControl, IDpiScaledControl

    ''' <summary>The sizes offered by the size picker. Deliberately a short, ordinary list:
    ''' the original had no size picker at all, and an open-ended one invites documents that
    ''' will not lay out.</summary>
    Private Shared ReadOnly FONT_SIZES As Single() = {8.0F, 9.0F, 10.0F, 11.0F, 12.0F, 14.0F,
                                                      16.0F, 18.0F, 20.0F, 24.0F, 28.0F, 36.0F}

    ' Repopulating the pickers raises SelectedIndexChanged, and those are not the operator's
    ' choices. Without this guard, moving the caret across a document would rewrite its
    ' formatting behind the operator's back.
    Private _suspended As Boolean

    ' RebuildLayout writes child bounds, and every write raises OnLayout again. The flag makes
    ' the pass single-shot instead of re-entrant.
    Private _layingOut As Boolean

    ''' <summary>Raised whenever the content changes -- the host marks the draft unsaved.</summary>
    Public Event ContinutModificat As EventHandler

    Public Sub New()
        ' The bands paint over the whole surface, so the control has to buffer its own frame:
        ' without this the editor border flickers on every keystroke that scrolls the text.
        SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer Or
                 ControlStyles.ResizeRedraw, True)
        InitializeComponent()
        FillPickers()
        ApplyButtonIcons()
        ApplyCollapseTooltip()
        ApplyFooterTexts(0, 0, 0.0R)
        ' The «no scheme loaded» look, so the designer surface and the bench are readable
        ' before ThemeManager ever reaches this control.
        ApplyResolvedColors()
        ' The bound list outlives the editor; its events have to be let go of on the way out.
        AddHandler Disposed, AddressOf HandleSelfDisposed
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' The two faces of the content
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' The content as RTF -- what goes into <c>FX_DDF_REV.Desc_Lunga</c>.
    '''
    ''' <para>The setter accepts plain text too: anything that does not start with
    ''' <c>{\rtf</c> is loaded as plain text rather than rejected. That is the original's
    ''' behaviour (<c>RTB_Load</c>) and it matters, because rows written before the rich-text
    ''' editor existed hold plain text in this column.</para>
    ''' </summary>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Rtf As String
        Get
            Return rtb.Rtf
        End Get
        Set(value As String)
            Try
                _suspended = True
                If String.IsNullOrEmpty(value) Then
                    rtb.Clear()
                ElseIf value.TrimStart().StartsWith("{\rtf", StringComparison.OrdinalIgnoreCase) Then
                    rtb.Rtf = value
                Else
                    rtb.Text = value
                End If
            Catch ex As ArgumentException
                ' Malformed RTF: RichTextBox throws rather than showing anything. Falling back
                ' to plain text keeps the operator's words on screen -- losing them silently
                ' would be far worse than showing the markup.
                rtb.Text = value
            Finally
                _suspended = False
            End Try
            ' Loading is not an edit, so no ContinutModificat -- but the counters and the
            ' toolbar still have to tell the truth about what is now on screen.
            RefreshStatistics()
            RefreshToolbarState()
        End Set
    End Property

    ''' <summary>
    ''' The same content as PLAIN TEXT -- what goes into <c>FX_DDF_REV.Desc_Lunga_ANSI</c>,
    ''' and from there, through the read route of slice 0020, into the signed XFA document.
    ''' </summary>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property TextSimplu As String
        Get
            Return rtb.Text
        End Get
    End Property

    ''' <summary>Can the operator type? Set from the header's enablement rules.</summary>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Editabil As Boolean
        Get
            Return Not rtb.ReadOnly
        End Get
        Set(value As Boolean)
            rtb.ReadOnly = Not value
            ' The COLLAPSE button is deliberately not in this list: folding the editor away is
            ' a way of looking at the form, not a way of changing the document.
            For Each c As Control In New Control() {btnBold, btnItalic, btnUnderline,
                                                    btnTextColor, btnHighlight, cmbFont, cmbSize}
                c.Enabled = value
            Next
        End Set
    End Property

    ''' <summary>The editing surface itself -- for a host that needs Find, Select or Undo.</summary>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property TextBox As RichTextBox
        Get
            Return rtb
        End Get
    End Property

    ' ══════════════════════════════════════════════════════════════════════════
    ' The pickers
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>Fills the font and size pickers once, at construction.</summary>
    Private Sub FillPickers()
        Try
            _suspended = True
            cmbFont.Items.Clear()
            ' `InstalledFontCollection` and not `FontFamily.Families`: the latter is a cached
            ' snapshot that can hold families the process cannot actually render.
            Using collection As New InstalledFontCollection()
                For Each family As FontFamily In collection.Families
                    If family.IsStyleAvailable(FontStyle.Regular) Then cmbFont.Items.Add(family.Name)
                Next
            End Using

            cmbSize.Items.Clear()
            ' CURRENT culture, and the same one everywhere the list is written, read back and
            ' searched: the sizes are text the operator reads, and once EnsureSizeListed can add
            ' a fractional one, an invariant list would show «12.2» on a Romanian machine that
            ' writes 12,2 everywhere else.
            For Each size As Single In FONT_SIZES
                cmbSize.Items.Add(size.ToString(Globalization.CultureInfo.CurrentCulture))
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichTextEditor.FillPickers", ex)
            Throw
        Finally
            _suspended = False
        End Try
    End Sub

    ''' <summary>
    ''' Reflects the selection's formatting in the toolbar.
    '''
    ''' <para>A MIXED selection (two words with different fonts) gives
    ''' <c>SelectionFont = Nothing</c>. The original fell back to the control's own font, and
    ''' so does this -- showing nothing at all would read as "no formatting", which is a
    ''' different and wrong claim.</para>
    ''' </summary>
    Private Sub RefreshToolbarState()
        If rtb Is Nothing OrElse cmbFont Is Nothing Then Return

        Dim f As Font = rtb.SelectionFont
        If f Is Nothing Then f = rtb.Font

        ApplyPressedLook(btnBold, f.Bold)
        ApplyPressedLook(btnItalic, f.Italic)
        ApplyPressedLook(btnUnderline, f.Underline)

        ' The guard, not RemoveHandler/AddHandler as the original did: a `Handles` clause
        ' cannot be detached, and a flag says what it means at the point that matters.
        _suspended = True
        Try
            Dim iFamily As Integer = cmbFont.Items.IndexOf(f.Name)
            If iFamily >= 0 Then cmbFont.SelectedIndex = iFamily
            Dim iSize As Integer = EnsureSizeListed(f.Size)
            If iSize >= 0 Then cmbSize.SelectedIndex = iSize
        Finally
            _suspended = False
        End Try
    End Sub

    ''' <summary>
    ''' The index of <paramref name="size"/> in the size picker, adding it in sorted position
    ''' if the list does not hold it yet.
    '''
    ''' <para>The offered list stays the short, ordinary one -- this only makes sure the picker
    ''' can SHOW the size the document actually uses. Without it the box sits empty the moment
    ''' the base font is anything but a round number, which every DPI or text-size setting makes
    ''' the normal case (a 11 pt font at 110% is 12.1 pt), and an empty picker reads as broken.</para>
    ''' </summary>
    Private Function EnsureSizeListed(size As Single) As Integer
        Dim wanted As String = size.ToString(Globalization.CultureInfo.CurrentCulture)
        Dim i As Integer = cmbSize.Items.IndexOf(wanted)
        If i >= 0 Then Return i

        For j As Integer = 0 To cmbSize.Items.Count - 1
            Dim listed As Single
            If Not Single.TryParse(TryCast(cmbSize.Items(j), String), Globalization.NumberStyles.Float,
                                   Globalization.CultureInfo.CurrentCulture, listed) Then Continue For
            If listed > size Then
                cmbSize.Items.Insert(j, wanted)
                Return j
            End If
        Next
        Return cmbSize.Items.Add(wanted)
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' The commands -- ported from `ToggleStyle` and `PickColor`
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>Turns one style on or off over the selection.</summary>
    Private Sub ToggleStyle(st As FontStyle)
        Dim f As Font = rtb.SelectionFont
        If f Is Nothing Then f = rtb.Font
        Dim wanted As FontStyle = If((f.Style And st) = st, f.Style And Not st, f.Style Or st)
        rtb.SelectionFont = New Font(f, wanted)
        RefreshToolbarState()
        RaiseEvent ContinutModificat(Me, EventArgs.Empty)
    End Sub

    ''' <summary>Asks for a colour and applies it to the text or to its background.</summary>
    Private Sub PickColor(forBackground As Boolean)
        Using cd As New ColorDialog()
            cd.Color = If(forBackground, rtb.SelectionBackColor, rtb.SelectionColor)
            cd.FullOpen = True
            If cd.ShowDialog(Me) <> DialogResult.OK Then Return
            If forBackground Then
                rtb.SelectionBackColor = cd.Color
            Else
                rtb.SelectionColor = cd.Color
            End If
        End Using
        RaiseEvent ContinutModificat(Me, EventArgs.Empty)
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' Layout -- the ONE place the published metrics turn into rectangles
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Places the two bands, the editing surface and everything inside the bands, from the
    ''' logical metrics scaled once (C2).
    '''
    ''' <para>The header lays out left to right: the five command buttons, a gap, the two
    ''' pickers; the collapse button is pinned to the RIGHT edge and the pickers are clipped
    ''' before it, so a narrow editor loses picker width rather than hiding the only control
    ''' that can bring the body back.</para>
    ''' </summary>
    Private Sub RebuildLayout()
        If _layingOut Then Return
        If rtb Is Nothing OrElse pnlHeader Is Nothing OrElse pnlFooter Is Nothing Then Return

        _layingOut = True
        Try
            Dim headerH As Integer = If(_headerVisible, Sc(_headerHeight), 0)
            Dim footerH As Integer = If(_footerVisible AndAlso Not _collapsed, Sc(_footerHeight), 0)

            pnlHeader.Visible = _headerVisible
            pnlHeader.SetBounds(0, 0, Width, headerH)

            pnlFooter.Visible = _footerVisible AndAlso Not _collapsed
            pnlFooter.SetBounds(0, Math.Max(0, Height - footerH), Width, footerH)

            ' The editing surface: what is left between the bands, minus our own frame.
            Dim border As Integer = Sc(_editorBorderWidth)
            Dim bodyTop As Integer = headerH
            Dim bodyHeight As Integer = Math.Max(0, Height - headerH - footerH)
            rtb.Visible = Not _collapsed AndAlso bodyHeight > border * 2
            If rtb.Visible Then
                rtb.SetBounds(border, bodyTop + border,
                              Math.Max(0, Width - border * 2),
                              Math.Max(0, bodyHeight - border * 2))
                ApplyEditorPadding()
            End If

            If _headerVisible Then LayoutHeader(headerH)
            If pnlFooter.Visible Then LayoutFooter(footerH)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichTextEditor.RebuildLayout", ex)
        Finally
            _layingOut = False
        End Try
        Invalidate()
    End Sub

    ''' <summary>Places the toolbar inside the header band.</summary>
    Private Sub LayoutHeader(headerH As Integer)
        Dim pad As Padding = ScPadding(_headerPadding)
        Dim sep As Integer = Sc(_headerSeparatorWidth)
        Dim inner As Integer = Math.Max(0, headerH - pad.Vertical - sep)

        Dim btn As Size = New Size(Sc(_buttonSize.Width), Sc(_buttonSize.Height))
        Dim btnH As Integer = If(btn.Height <= 0, inner, Math.Min(btn.Height, inner))
        Dim btnW As Integer = Math.Max(1, btn.Width)
        Dim btnTop As Integer = pad.Top + Math.Max(0, (inner - btnH) \ 2)

        Dim gap As Integer = Sc(_buttonSpacing)
        Dim x As Integer = pad.Left

        For Each b As KBotNoFocusButton In CommandButtons()
            b.Padding = ScPadding(_buttonPadding)
            b.SetBounds(x, btnTop, btnW, btnH)
            x += btnW + gap
        Next

        ' The collapse button owns the right edge; nothing else is allowed into its corner.
        Dim rightLimit As Integer = Math.Max(pad.Left, Width - pad.Right)
        btnCollapse.Visible = _collapseButton
        If _collapseButton Then
            rightLimit -= btnW
            btnCollapse.Padding = ScPadding(_buttonPadding)
            btnCollapse.SetBounds(Math.Max(pad.Left, rightLimit), btnTop, btnW, btnH)
            rightLimit = Math.Max(pad.Left, rightLimit - gap)
        End If

        ' The pickers. Their height is a metric of its own (0 = fill the band).
        Dim cmbH As Integer = If(_comboHeight > 0, Math.Min(Sc(_comboHeight), inner), inner)
        Dim cmbTop As Integer = pad.Top + Math.Max(0, (inner - cmbH) \ 2)
        Dim cmbGap As Integer = Sc(_comboSpacing)

        x += Math.Max(0, Sc(_groupSpacing) - gap)
        Dim fontW As Integer = Math.Max(0, Math.Min(Sc(_fontComboWidth), rightLimit - x))
        cmbFont.SetBounds(x, cmbTop, fontW, cmbH)
        cmbFont.Visible = fontW > 0
        x += fontW + cmbGap

        Dim sizeW As Integer = Math.Max(0, Math.Min(Sc(_sizeComboWidth), rightLimit - x))
        cmbSize.SetBounds(x, cmbTop, sizeW, cmbH)
        cmbSize.Visible = sizeW > 0
    End Sub

    ''' <summary>Places the three counters inside the footer band, left to right.</summary>
    Private Sub LayoutFooter(footerH As Integer)
        Dim pad As Padding = ScPadding(_footerPadding)
        Dim sep As Integer = Sc(_footerSeparatorWidth)
        Dim top As Integer = pad.Top + sep
        Dim h As Integer = Math.Max(0, footerH - pad.Vertical - sep)
        Dim gap As Integer = Sc(_footerItemSpacing)
        Dim x As Integer = pad.Left

        For Each l As Label In New Label() {lblChars, lblWords, lblSize}
            Dim w As Integer = MeasureFooterLabel(l)
            l.SetBounds(x, top, w, h)
            x += w + gap
        Next
    End Sub

    ''' <summary>
    ''' The width one counter needs, so the three of them do not drift apart when the numbers
    ''' grow. Measured, never guessed at a fixed column width.
    '''
    ''' <para><c>TextRenderer</c> and NOT <c>Graphics.MeasureString</c>: a <c>Label</c> draws
    ''' with GDI unless <c>UseCompatibleTextRendering</c> is on, and the GDI+ measurement comes
    ''' back narrower -- which cost «151 caractere» its word on the first render.</para>
    ''' </summary>
    Private Function MeasureFooterLabel(l As Label) As Integer
        Try
            Return TextRenderer.MeasureText(l.Text, l.Font).Width + Sc(4)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichTextEditor.MeasureFooterLabel", ex)
            Return Sc(90)
        End Try
    End Function

    ''' <summary>All five formatting buttons, in the order they appear.</summary>
    Private Function CommandButtons() As KBotNoFocusButton()
        Return New KBotNoFocusButton() {btnBold, btnItalic, btnUnderline, btnTextColor, btnHighlight}
    End Function

    ''' <summary>A logical pixel number at the control's scale (C2).</summary>
    Private Function Sc(logical As Integer) As Integer
        Return ThemeShapes.ScaleDpi(Me, logical)
    End Function

    ''' <summary>The same for a whole padding -- one place, so no side is forgotten.</summary>
    Private Function ScPadding(p As Padding) As Padding
        Return New Padding(Sc(p.Left), Sc(p.Top), Sc(p.Right), Sc(p.Bottom))
    End Function


    ''' <summary>
    ''' The keys are resolved AGAIN the moment the control gets a window handle.
    '''
    ''' <para><b>Why this is not belt and braces.</b> A generated <c>.Designer.vb</c> creates
    ''' the <c>ImageList</c> component empty, writes our properties in alphabetical order --
    ''' <c>BoldImageKey</c> before <c>Images</c>, and <c>Images</c> itself while the list is
    ''' still empty -- and only further down loads its <c>ImageStream</c> and names its keys.
    ''' Resolving at set time alone therefore left every button on its lettered fallback
    ''' (B, I, U, A, ▨) at runtime, on a form whose designer showed the icons perfectly. By the
    ''' time a handle is needed -- shown, or drawn into a bitmap -- InitializeComponent has run
    ''' to its end.</para>
    ''' </summary>
    Protected Overrides Sub OnHandleCreated(e As EventArgs)
        MyBase.OnHandleCreated(e)
        Try
            ApplyButtonIcons()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichTextEditor.OnHandleCreated", ex)
        End Try
    End Sub

    ''' <summary>The list belongs to the host and outlives us: let go of its events.</summary>
    Private Sub HandleSelfDisposed(sender As Object, e As EventArgs)
        Try
            DetachImages()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichTextEditor.HandleSelfDisposed", ex)
        End Try
    End Sub
    Protected Overrides Sub OnLayout(e As LayoutEventArgs)
        MyBase.OnLayout(e)
        Try
            RebuildLayout()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichTextEditor.OnLayout", ex)
        End Try
    End Sub

    Protected Overrides Sub OnSizeChanged(e As EventArgs)
        MyBase.OnSizeChanged(e)
        Try
            ' The expanded height is remembered whenever SOMEBODY ELSE resizes us, so that
            ' expanding again comes back to the size the operator had (the grid's rule).
            If Not _applyingCollapseExtent AndAlso Not _collapsed Then _expandedHeight = Height
            RebuildLayout()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichTextEditor.OnSizeChanged", ex)
        End Try
    End Sub

    ''' <summary>DPI or operator scale changed: every metric is recomputed from its LOGICAL
    ''' value, never composed over the already-scaled one (C2).</summary>
    Public Sub RefreshDpiMetrics() Implements IDpiScaledControl.RefreshDpiMetrics
        Try
            ApplyBandSeparators()
            RebuildLayout()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichTextEditor.RefreshDpiMetrics", ex)
        End Try
    End Sub

    Protected Overrides Sub OnDpiChangedAfterParent(e As EventArgs)
        MyBase.OnDpiChangedAfterParent(e)
        Try
            RefreshDpiMetrics()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichTextEditor.OnDpiChangedAfterParent", ex)
        End Try
    End Sub

    ''' <summary>The frame around the editing surface -- ours, not the system's (see the
    ''' Designer note on <c>rtb.BorderStyle</c>).</summary>
    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        Try
            If _collapsed Then Return
            Dim w As Integer = Sc(_editorBorderWidth)
            If w <= 0 Then Return
            Dim colour As Color = ResolvedEditorBorderColor()
            If colour.IsEmpty Then Return

            Dim top As Integer = If(_headerVisible, Sc(_headerHeight), 0)
            Dim bottom As Integer = Height - If(_footerVisible, Sc(_footerHeight), 0)
            Dim r As New Rectangle(0, top, Width, Math.Max(0, bottom - top))
            If r.Height <= 0 Then Return

            Using p As New Pen(colour, w)
                p.Alignment = Drawing2D.PenAlignment.Inset
                e.Graphics.DrawRectangle(p, r.X, r.Y, r.Width - 1, r.Height - 1)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichTextEditor.OnPaint", ex)
        End Try
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' The padding INSIDE the editing surface
    ' ══════════════════════════════════════════════════════════════════════════

    <StructLayout(LayoutKind.Sequential)>
    Private Structure RECT
        Public Left As Integer
        Public Top As Integer
        Public Right As Integer
        Public Bottom As Integer
    End Structure

    ' EM_SETRECT. A RichTextBox ignores `Padding` -- the text is laid out inside a FORMATTING
    ' rectangle that only this message moves, which is why the inner padding is not a plain
    ' property assignment.
    Private Const EM_SETRECT As Integer = &HB3

    <DllImport("user32.dll", CharSet:=CharSet.Auto, SetLastError:=False)>
    Private Shared Function SendMessage(hWnd As IntPtr, msg As Integer, wParam As IntPtr,
                                        ByRef lParam As RECT) As IntPtr
    End Function

    ''' <summary>Pushes <see cref="EditorPadding"/> into the editing surface's formatting
    ''' rectangle. Interop -> log and re-throw is the rule, but this one is called from layout,
    ''' so it logs and returns: a refused message must not take the form down.</summary>
    Private Sub ApplyEditorPadding()
        Try
            If Not rtb.IsHandleCreated Then Return
            Dim pad As Padding = ScPadding(_editorPadding)
            Dim r As New RECT With {
                .Left = pad.Left,
                .Top = pad.Top,
                .Right = Math.Max(pad.Left + 1, rtb.ClientSize.Width - pad.Right),
                .Bottom = Math.Max(pad.Top + 1, rtb.ClientSize.Height - pad.Bottom)
            }
            SendMessage(rtb.Handle, EM_SETRECT, IntPtr.Zero, r)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichTextEditor.ApplyEditorPadding", ex)
        End Try
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' The footer counters
    ' ══════════════════════════════════════════════════════════════════════════

    Private _chars As Integer
    Private _words As Integer
    Private _kilobytes As Double

    ''' <summary>Characters in the plain text -- the number the footer shows.</summary>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property CharacterCount As Integer
        Get
            Return _chars
        End Get
    End Property

    ''' <summary>Words in the plain text -- runs separated by whitespace.</summary>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property WordCount As Integer
        Get
            Return _words
        End Get
    End Property

    ''' <summary>
    ''' The size the footer reports, in KB. It is the size of the RTF and NOT of the plain
    ''' text: the RTF is what goes into <c>Desc_Lunga</c>, so it is the number that says
    ''' whether a description is getting out of hand.
    ''' </summary>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property SizeKilobytes As Double
        Get
            Return _kilobytes
        End Get
    End Property

    ''' <summary>Recomputes the three counters and writes them into the footer. Public because
    ''' a host that loaded content through <see cref="Rtf"/> may want them at once.</summary>
    Public Sub RefreshStatistics()
        Try
            Dim plain As String = If(rtb.Text, String.Empty)
            _chars = plain.Length
            _words = CountWords(plain)

            ' The RTF is asked for ONCE, here, and not by three callers: on a long description
            ' the string is megabytes and building it is the expensive part.
            Dim rtfText As String = If(rtb.Rtf, String.Empty)
            _kilobytes = System.Text.Encoding.UTF8.GetByteCount(rtfText) / 1024.0R

            ApplyFooterTexts(_chars, _words, _kilobytes)
            If pnlFooter.Visible Then LayoutFooter(pnlFooter.Height)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichTextEditor.RefreshStatistics", ex)
        End Try
    End Sub

    ''' <summary>Words = runs of non-whitespace. Deliberately the plainest rule there is: a
    ''' cleverer one would disagree with whatever the operator counts by hand.</summary>
    Private Shared Function CountWords(text As String) As Integer
        If String.IsNullOrWhiteSpace(text) Then Return 0
        Dim n As Integer = 0
        Dim inWord As Boolean = False
        For Each ch As Char In text
            If Char.IsWhiteSpace(ch) Then
                inWord = False
            ElseIf Not inWord Then
                inWord = True
                n += 1
            End If
        Next
        Return n
    End Function

    ''' <summary>Writes the three formats. Operator-visible strings, so Romanian with its
    ''' diacritics (RULE 0's one exception).</summary>
    Private Sub ApplyFooterTexts(chars As Integer, words As Integer, kb As Double)
        Try
            Dim ci As Globalization.CultureInfo = Globalization.CultureInfo.CurrentCulture
            lblChars.Text = String.Format(ci, _footerCharactersFormat, chars)
            lblWords.Text = String.Format(ci, _footerWordsFormat, words)
            lblSize.Text = String.Format(ci, _footerSizeFormat, kb)
        Catch ex As FormatException
            ' A format the operator typed into the property grid that String.Format cannot use.
            ' The counters fall back to the bare numbers rather than leaving the band empty.
            GlobalErrorLog.Write("KBotRichTextEditor.ApplyFooterTexts", ex)
            lblChars.Text = chars.ToString(Globalization.CultureInfo.CurrentCulture)
            lblWords.Text = words.ToString(Globalization.CultureInfo.CurrentCulture)
            lblSize.Text = kb.ToString("N1", Globalization.CultureInfo.CurrentCulture)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichTextEditor.ApplyFooterTexts", ex)
        End Try
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' Handlers -- UI boundaries: log and swallow, they have nobody to throw to
    ' ══════════════════════════════════════════════════════════════════════════

    Private Sub BtnBold_Click(sender As Object, e As EventArgs) Handles btnBold.Click
        Try
            ToggleStyle(FontStyle.Bold)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichTextEditor.BtnBold_Click", ex)
        End Try
    End Sub

    Private Sub BtnItalic_Click(sender As Object, e As EventArgs) Handles btnItalic.Click
        Try
            ToggleStyle(FontStyle.Italic)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichTextEditor.BtnItalic_Click", ex)
        End Try
    End Sub

    Private Sub BtnUnderline_Click(sender As Object, e As EventArgs) Handles btnUnderline.Click
        Try
            ToggleStyle(FontStyle.Underline)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichTextEditor.BtnUnderline_Click", ex)
        End Try
    End Sub

    Private Sub BtnTextColor_Click(sender As Object, e As EventArgs) Handles btnTextColor.Click
        Try
            PickColor(forBackground:=False)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichTextEditor.BtnTextColor_Click", ex)
        End Try
    End Sub

    Private Sub BtnHighlight_Click(sender As Object, e As EventArgs) Handles btnHighlight.Click
        Try
            PickColor(forBackground:=True)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichTextEditor.BtnHighlight_Click", ex)
        End Try
    End Sub

    Private Sub BtnCollapse_Click(sender As Object, e As EventArgs) Handles btnCollapse.Click
        Try
            ToggleCollapse()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichTextEditor.BtnCollapse_Click", ex)
        End Try
    End Sub

    Private Sub CmbFont_SelectedIndexChanged(sender As Object, e As EventArgs) _
        Handles cmbFont.SelectedIndexChanged
        Try
            If _suspended Then Return
            Dim family As String = TryCast(cmbFont.SelectedItem, String)
            If String.IsNullOrEmpty(family) Then Return

            Dim f As Font = rtb.SelectionFont
            If f Is Nothing Then f = rtb.Font
            ' Keep the size and the style; only the family changes.
            rtb.SelectionFont = New Font(family, f.Size, f.Style)
            ' The focus goes back so the caret stays visible and the next command still has a
            ' selection to work on (the original did this in `NoFocusComboBox`).
            rtb.Focus()
            RaiseEvent ContinutModificat(Me, EventArgs.Empty)
        Catch ex As ArgumentException
            ' A family that cannot render the current style. Reported, never swallowed into a
            ' half-applied format.
            GlobalErrorLog.Write("KBotRichTextEditor.CmbFont_SelectedIndexChanged", ex)
            MessageBox.Show(Me, "Fontul ales nu poate reda stilul curent al textului.",
                            "Font", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichTextEditor.CmbFont_SelectedIndexChanged", ex)
        End Try
    End Sub

    Private Sub CmbSize_SelectedIndexChanged(sender As Object, e As EventArgs) _
        Handles cmbSize.SelectedIndexChanged
        Try
            If _suspended Then Return
            Dim chosen As String = TryCast(cmbSize.SelectedItem, String)
            Dim size As Single
            If Not Single.TryParse(chosen, Globalization.NumberStyles.Float,
                                   Globalization.CultureInfo.CurrentCulture, size) Then Return

            Dim f As Font = rtb.SelectionFont
            If f Is Nothing Then f = rtb.Font
            rtb.SelectionFont = New Font(f.FontFamily, size, f.Style)
            rtb.Focus()
            RaiseEvent ContinutModificat(Me, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichTextEditor.CmbSize_SelectedIndexChanged", ex)
        End Try
    End Sub

    Private Sub Rtb_SelectionChanged(sender As Object, e As EventArgs) Handles rtb.SelectionChanged
        Try
            RefreshToolbarState()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichTextEditor.Rtb_SelectionChanged", ex)
        End Try
    End Sub

    Private Sub Rtb_TextChanged(sender As Object, e As EventArgs) Handles rtb.TextChanged
        Try
            ' The counters are recomputed on the timer, not here -- see the tmrStats note in
            ' the Designer. Restarting it coalesces a burst of typing into one pass.
            If _footerVisible AndAlso Not KBotDesignTime.IsDesignTime(Me) Then
                tmrStats.Stop()
                tmrStats.Start()
            End If

            ' Loading a value is not the operator editing, so it must not mark the draft dirty.
            If _suspended Then Return
            RaiseEvent ContinutModificat(Me, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichTextEditor.Rtb_TextChanged", ex)
        End Try
    End Sub

    Private Sub Rtb_HandleCreated(sender As Object, e As EventArgs) Handles rtb.HandleCreated
        Try
            ' EM_SETRECT needs a window; the first layout pass usually runs before there is one.
            ApplyEditorPadding()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichTextEditor.Rtb_HandleCreated", ex)
        End Try
    End Sub

    Private Sub TmrStats_Tick(sender As Object, e As EventArgs) Handles tmrStats.Tick
        Try
            tmrStats.Stop()
            RefreshStatistics()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichTextEditor.TmrStats_Tick", ex)
        End Try
    End Sub
End Class
