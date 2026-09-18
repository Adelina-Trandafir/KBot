Option Strict On
Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' Keeps a themed form at least as large as its base, and as large as its themed content
''' demands (slice 0062). The form-level sibling of <see cref="ThemeTableFit"/>.
'''
''' <para><b>The base.</b> <c>KBotThemedForm</c>'s constructor puts the scheme font on the form
''' BEFORE the derived <c>InitializeComponent</c> runs, so the platform's font autoscale already
''' has that font when it rescales the designer's <c>ClientSize</c>. There is no moment at
''' runtime where the literal designer size exists untouched -- so the base is, honestly, the
''' size right after <c>InitializeComponent</c>, captured at <c>OnCreateControl</c>, before <c>Load</c>: after the
''' platform autoscale, before the theme traverses the tree and before <c>ModernRenderer</c> grows
''' the buttons. The growth then covers exactly what does NOT go through autoscale: renderer
''' padding, corner radii, and the measures we draw ourselves through <c>AppScaling</c>.</para>
'''
''' <para><b>The rule</b>, in <see cref="Fit"/>: Max(base, demand) then Max(.., MinimumSize)
''' then Min(.., working area). A scheme switch changes the base font, so autoscale rewrites the
''' client size; a smaller font would shrink the window below its base, and Max(base, demand)
''' brings it back. The clamp to the working area is a CLAMP, not a throw -- but it is written
''' down (error log + operator log) with the form name and both sizes, so it is not silent.</para>
'''
''' <para><b>The snapshot is never rewritten.</b> Without that, the second scheme switch would
''' measure over the result of the first and the window would grow forever -- the bug that
''' <see cref="ThemeTableFit"/> and <see cref="FontBaseline"/> each had to fix once.</para>
'''
''' <para><b>The demand</b> is measured on the form's fit root (<c>KBotThemedForm.FitRoot</c>, by
''' default its single <c>Dock = Fill</c> child) through <see cref="ContentDemand"/>, never on the
''' form and never through a plain panel's <c>GetPreferredSize</c>: neither answers about content. Whatever the client area holds around
''' that root (docked bands, padding) is added as measured. The operator's own enlargement is
''' honoured too: the current client size is part of the demand, so a fit never shrinks a window
''' someone made larger.</para>
'''
''' <para><c>Control.Visible</c> is never read on this path: the getter answers about the parent
''' chain, so on a form not yet shown everything reports False (slices 0030 and 0049-02).</para>
'''
''' <para>Shared state, touched only from the UI thread.</para>
''' </summary>
Public NotInheritable Class ThemeFormFit

    Private Sub New()
    End Sub

    ''' <summary>What was captured for a form. Not a model: Friend, and invisible to callers.</summary>
    Friend NotInheritable Class FitSnapshot
        Public ClientSize As Size
        Public MinimumSize As Size
        Public ScaleAtCapture As Single
    End Class

    ' ConditionalWeakTable: a closed form is not kept alive by its snapshot, and nothing has to be
    ' cleared on close (Forget exists for the explicit path and for tests).
    Private Shared ReadOnly _snapshots As New ConditionalWeakTable(Of Form, FitSnapshot)()

    Private Shared _baseline As FormFitBaseline = FormFitBaseline.Scaled

    ''' <summary>
    ''' How the base follows the scale. Writing persists (theme.json, read-modify-write through
    ''' <see cref="ThemeStore"/>) and refits every open themed form. Load without persisting goes
    ''' through <see cref="LoadFrom"/>.
    ''' </summary>
    Public Shared Property Baseline As FormFitBaseline
        Get
            Return _baseline
        End Get
        Set(value As FormFitBaseline)
            If Not [Enum].IsDefined(GetType(FormFitBaseline), value) Then
                Throw New ArgumentException("Unknown FormFitBaseline: " & CInt(value), NameOf(value))
            End If
            If value = _baseline Then Return
            _baseline = value
            Try
                ThemeStore.SaveFormFitBaseline(value)
                RefitOpenForms()
            Catch ex As Exception
                GlobalErrorLog.Write("ThemeFormFit.Baseline", ex)
                Throw
            End Try
        End Set
    End Property

    ''' <summary>Sets the value read from file -- no persist, no refit (<c>ThemeManager.Initialize</c>).</summary>
    Friend Shared Sub LoadFrom(value As FormFitBaseline)
        _baseline = If([Enum].IsDefined(GetType(FormFitBaseline), value), value, FormFitBaseline.Scaled)
    End Sub

    ' Every open themed form recomputes its size against the new definition of the base.
    Private Shared Sub RefitOpenForms()
        Dim forms As New List(Of Form)()
        For Each f As Form In Application.OpenForms
            If f IsNot Nothing AndAlso Not f.IsDisposed Then forms.Add(f)
        Next
        For Each f As Form In forms
            Dim themed As KBotThemedForm = TryCast(f, KBotThemedForm)
            If themed IsNot Nothing AndAlso Not themed.IsDisposed Then themed.RefitToTheme()
        Next
    End Sub

    ''' <summary>
    ''' Remembers the form's client size, minimum size and the scale of the moment. Idempotent:
    ''' only the first call for a form stores anything.
    '''
    ''' <para>The scale stored is the SCREEN part only (<see cref="AppScaling.ScreenFactorFor"/>):
    ''' measured, the client size right after <c>InitializeComponent</c> already carries the DPI
    ''' (the platform autoscale ran) but not the operator's text size, which the theme applies
    ''' later, in <c>OnLoad</c>. Storing the full factor would make a form captured at 125% text
    ''' shrink BELOW its designer size when the operator went back to 100%.</para>
    ''' </summary>
    Public Shared Sub Capture(target As Form)
        If target Is Nothing Then Throw New ArgumentNullException(NameOf(target))
        Dim existing As FitSnapshot = Nothing
        If _snapshots.TryGetValue(target, existing) Then Return
        _snapshots.Add(target, New FitSnapshot With {
            .ClientSize = target.ClientSize,
            .MinimumSize = target.MinimumSize,
            .ScaleAtCapture = AppScaling.ScreenFactorFor(target)})
    End Sub

    ''' <summary>True once <see cref="Capture"/> ran for the form.</summary>
    Public Shared Function HasSnapshot(target As Form) As Boolean
        If target Is Nothing Then Return False
        Dim snap As FitSnapshot = Nothing
        Return _snapshots.TryGetValue(target, snap)
    End Function

    ''' <summary>The captured client size, or <c>Size.Empty</c> when nothing was captured (readouts, tests).</summary>
    Public Shared Function CapturedClientSize(target As Form) As Size
        Dim snap As FitSnapshot = Nothing
        If target Is Nothing OrElse Not _snapshots.TryGetValue(target, snap) Then Return Size.Empty
        Return snap.ClientSize
    End Function

    ''' <summary>The scale factor at capture, or 0 when nothing was captured.</summary>
    Public Shared Function CapturedScale(target As Form) As Single
        Dim snap As FitSnapshot = Nothing
        If target Is Nothing OrElse Not _snapshots.TryGetValue(target, snap) Then Return 0F
        Return snap.ScaleAtCapture
    End Function

    ''' <summary>Forgets the form's snapshot.</summary>
    Public Shared Sub Forget(target As Form)
        If target Is Nothing Then Return
        _snapshots.Remove(target)
    End Sub

    ''' <summary>
    ''' Pure: the base at the current scale. <see cref="FormFitBaseline.DesignerRaw"/> returns the
    ''' captured size untouched; <see cref="FormFitBaseline.Scaled"/> multiplies it by
    ''' (scale now / scale at capture). A zero or absurd capture scale counts as 1.
    ''' </summary>
    Public Shared Function ScaledBaseline(captured As Size, scaleAtCapture As Single, scaleNow As Single,
                                          mode As FormFitBaseline) As Size
        If mode = FormFitBaseline.DesignerRaw Then Return captured
        Dim was As Single = If(scaleAtCapture > 0F AndAlso Not Single.IsNaN(scaleAtCapture) AndAlso
                               Not Single.IsInfinity(scaleAtCapture), scaleAtCapture, 1.0F)
        Dim now As Single = If(scaleNow > 0F AndAlso Not Single.IsNaN(scaleNow) AndAlso
                               Not Single.IsInfinity(scaleNow), scaleNow, 1.0F)
        Dim ratio As Single = now / was
        Return New Size(CInt(Math.Round(captured.Width * ratio)), CInt(Math.Round(captured.Height * ratio)))
    End Function

    ''' <summary>
    ''' Pure: Max(baseline, demand), then Max(.., minimum), then Min(.., work). A non-positive
    ''' component of <paramref name="work"/> means "no limit on that axis". Idempotent:
    ''' Fit(Fit(x)) = Fit(x).
    ''' </summary>
    Public Shared Function Fit(baseline As Size, demand As Size, minimum As Size, work As Size) As Size
        Dim w As Integer = Math.Max(baseline.Width, demand.Width)
        Dim h As Integer = Math.Max(baseline.Height, demand.Height)
        w = Math.Max(w, minimum.Width)
        h = Math.Max(h, minimum.Height)
        If work.Width > 0 Then w = Math.Min(w, work.Width)
        If work.Height > 0 Then h = Math.Min(h, work.Height)
        Return New Size(w, h)
    End Function

    ' ── Measuring content ──────────────────────────────────────────────────────

    ' Per type: is GetPreferredSize the one from Control (the anchored-children union)?
    Private Shared ReadOnly _ownPreferredSize As New Dictionary(Of Type, Boolean)()

    ''' <summary>
    ''' A control whose preferred size answers about ITS content: it overrides
    ''' <c>GetPreferredSize</c> or <c>GetPreferredSizeCore</c> with an answer of its own (buttons,
    ''' labels, text boxes, up-downs, pictures...). Everything else inherits <c>Control</c>'s
    ''' answer -- the union of its children's bounds, or, with no children, its own current size
    ''' -- which says nothing about content. Measured on the log viewer: a drawn control with no
    ''' children (<c>KBotCaptionBar</c>, 1165x70) "prefers" exactly its bounds.
    '''
    ''' <para>Combo boxes and date pickers are the exception the other way: no own answer, but a
    ''' height the FONT decides and a table cannot stretch; <see cref="ContentDemand"/> reads that
    ''' height directly.</para>
    ''' </summary>
    Public Shared Function IsLeaf(c As Control) As Boolean
        If c Is Nothing Then Return False
        ' An own override wins whatever the base type: a Panel subclass that answers for itself
        ' knows its content better than a dock walk would.
        If HasOwnPreferredSize(c.GetType()) Then Return True
        Return False
    End Function

    ' Cached per type: does the type (or a base other than the WinForms containers) override
    ' GetPreferredSize or GetPreferredSizeCore? Both are checked because WinForms itself splits
    ' them: ButtonBase and Label override the public one, TextBox / NumericUpDown / PictureBox only
    ' the protected core.
    Private Shared Function HasOwnPreferredSize(t As Type) As Boolean
        Dim own As Boolean
        If _ownPreferredSize.TryGetValue(t, own) Then Return own
        Try
            Dim pub As Reflection.MethodInfo = t.GetMethod(NameOf(Control.GetPreferredSize), {GetType(Size)})
            Dim core As Reflection.MethodInfo = t.GetMethod("GetPreferredSizeCore",
                Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Public,
                Nothing, {GetType(Size)}, Nothing)
            own = IsOwnDeclaration(pub) OrElse IsOwnDeclaration(core)
        Catch ex As Exception
            GlobalErrorLog.Write("ThemeFormFit.HasOwnPreferredSize", ex)
            own = False
        End Try
        _ownPreferredSize(t) = own
        Return own
    End Function

    Private Shared Function IsOwnDeclaration(m As Reflection.MethodInfo) As Boolean
        If m Is Nothing Then Return False
        Dim d As Type = m.DeclaringType
        Return d IsNot GetType(Control) AndAlso d IsNot GetType(ScrollableControl) AndAlso
               d IsNot GetType(ContainerControl) AndAlso d IsNot GetType(Panel) AndAlso d IsNot GetType(GroupBox)
    End Function

    ' Font-driven height, no own preferred size, cannot be stretched by a table.
    Private Shared Function IsFixedHeightLeaf(c As Control) As Boolean
        Return TypeOf c Is ComboBox OrElse TypeOf c Is DateTimePicker
    End Function


    ''' <summary>
    ''' What a control's CONTENT asks for, in pixels, without ever reading the control's own
    ''' current size.
    '''
    ''' <para>Why not <c>GetPreferredSize</c>: measured, a <c>Panel</c> holding a non-AutoSize
    ''' child reports padding only (24x24 for <c>Padding = 12</c>), and a control with anchored
    ''' children reports its bounds. Both are useless as a demand -- the first never grows, the
    ''' second never shrinks back. So a container is walked by its dock structure: Top/Bottom
    ''' bands add their height, Left/Right bands their width, the Fill child adds its own
    ''' demand, and an un-anchored child at a fixed position adds its authored extent. Layout
    ''' panels (<c>TableLayoutPanel</c>, <c>FlowLayoutPanel</c>) and leaves answer through
    ''' <c>GetPreferredSize</c>, which for them comes from content.</para>
    '''
    ''' <para>It is an estimate that may ASK for a little more than strictly needed (dock order
    ''' is not replayed); it never asks for less than the content, and the fit takes Max with the
    ''' base, so an over-ask costs a few pixels, an under-ask costs clipped controls.</para>
    ''' </summary>
    Public Shared Function ContentDemand(c As Control) As Size
        If c Is Nothing Then Return Size.Empty
        Try
            Dim sc As SplitContainer = TryCast(c, SplitContainer)
            If sc IsNot Nothing Then Return SplitDemand(sc)

            Dim tc As TabControl = TryCast(c, TabControl)
            If tc IsNot Nothing Then Return TabDemand(tc)

            If IsFixedHeightLeaf(c) Then
                ' The font decides the height; the width is authored unless docking stretches it.
                Dim stretched As Boolean = c.Dock = DockStyle.Fill OrElse c.Dock = DockStyle.Top OrElse c.Dock = DockStyle.Bottom
                Return New Size(If(stretched, 0, c.Width), c.Height)
            End If

            Dim btn As Button = TryCast(c, Button)
            If btn IsNot Nothing AndAlso btn.Dock <> DockStyle.None Then Return ButtonDemand(btn)

            If TypeOf c Is TableLayoutPanel OrElse TypeOf c Is FlowLayoutPanel OrElse IsLeaf(c) Then
                Return c.GetPreferredSize(Size.Empty)
            End If

            ' A control that paints itself and OWNS its child controls (IThemedControl, not a
            ' container, not a composite UserControl) has no content a dock walk could see: its
            ' children are the scrollbars and search boxes it positions from its own size, so the
            ' walk would only echo the cell it sits in -- measured in slice 0066, a 300px table
            ' column holding a tree grew to 628 from the tree's search box. It asks for nothing.
            If TypeOf c Is IThemedControl AndAlso Not TypeOf c Is IThemedContainer AndAlso Not TypeOf c Is ContainerControl Then
                Return Size.Empty
            End If

            Return DockedDemand(c)
        Catch ex As Exception
            GlobalErrorLog.Write("ThemeFormFit.ContentDemand", ex)
            Throw
        End Try
    End Function

    ' A Button docked to anything answers GetPreferredSize with its BOUNDS (measured in slice
    ' 0066: 300x100 in a 300x100 cell, on every FlatStyle, AutoSize or not) -- the one leaf whose
    ' own answer echoes the cell, so a fixed row holding an OK button could never come back from
    ' a growth. Measured by hand then, with the formula ModernRenderer uses to size an undocked
    ' button: padding + one line of text + the two borders. The width is nothing when docking
    ' stretches it (Fill/Top/Bottom), text plus padding otherwise.
    Private Shared Function ButtonDemand(btn As Button) As Size
        Dim text As String = If(String.IsNullOrEmpty(btn.Text), "Wg", btn.Text)
        Dim t As Size = TextRenderer.MeasureText(text, btn.Font)
        Dim border As Integer = If(btn.FlatStyle = FlatStyle.Flat, 2 * btn.FlatAppearance.BorderSize, 2)
        Dim h As Integer = btn.Padding.Vertical + t.Height + border
        Dim stretchedW As Boolean = btn.Dock = DockStyle.Fill OrElse btn.Dock = DockStyle.Top OrElse btn.Dock = DockStyle.Bottom
        Dim w As Integer = If(stretchedW, 0, btn.Padding.Horizontal + t.Width + border)
        Return New Size(w, h)
    End Function

    ' A container walked by its dock structure (see ContentDemand).
    ' <paramref name="root"/>, when given, is the child whose content IS the fill term, whatever
    ' its Dock (a form that names an unusual FitRoot); it is skipped in the walk.
    Private Shared Function DockedDemand(c As Control, Optional root As Control = Nothing) As Size
        Dim fillW As Integer = 0, fillH As Integer = 0
        Dim bandsH As Integer = 0, bandsW As Integer = 0
        Dim widestBand As Integer = 0, tallestBand As Integer = 0
        Dim fixedRight As Integer = 0, fixedBottom As Integer = 0

        If root IsNot Nothing Then
            Dim rd As Size = ContentDemand(root)
            fillW = rd.Width
            fillH = rd.Height
        End If

        For Each child As Control In c.Controls
            If child Is root Then Continue For
            Dim d As Size = ContentDemand(child)
            Select Case child.Dock
                Case DockStyle.Fill
                    fillW = Math.Max(fillW, d.Width)
                    fillH = Math.Max(fillH, d.Height)
                Case DockStyle.Top, DockStyle.Bottom
                    ' A band keeps its authored height unless its content asks for more.
                    bandsH += Math.Max(child.Height, d.Height)
                    widestBand = Math.Max(widestBand, d.Width)
                Case DockStyle.Left, DockStyle.Right
                    bandsW += Math.Max(child.Width, d.Width)
                    tallestBand = Math.Max(tallestBand, d.Height)
                Case Else
                    ' Fixed position: its authored extent counts -- unless it is anchored to the
                    ' far edge, in which case it follows the container and says nothing.
                    If (child.Anchor And AnchorStyles.Right) = 0 Then fixedRight = Math.Max(fixedRight, child.Right)
                    If (child.Anchor And AnchorStyles.Bottom) = 0 Then fixedBottom = Math.Max(fixedBottom, child.Bottom)
            End Select
        Next

        Dim w As Integer = Math.Max(bandsW + fillW, widestBand) + c.Padding.Horizontal
        Dim h As Integer = bandsH + Math.Max(fillH, tallestBand) + c.Padding.Vertical
        w = Math.Max(w, fixedRight + c.Padding.Right)
        h = Math.Max(h, fixedBottom + c.Padding.Bottom)
        Return New Size(w, h)
    End Function

    Private Shared Function SplitDemand(sc As SplitContainer) As Size
        Dim d1 As Size = If(sc.Panel1Collapsed, Size.Empty, DockedDemand(sc.Panel1))
        Dim d2 As Size = If(sc.Panel2Collapsed, Size.Empty, DockedDemand(sc.Panel2))
        If sc.Orientation = Orientation.Vertical Then
            Return New Size(d1.Width + d2.Width + sc.SplitterWidth, Math.Max(d1.Height, d2.Height))
        End If
        Return New Size(Math.Max(d1.Width, d2.Width), d1.Height + d2.Height + sc.SplitterWidth)
    End Function

    Private Shared Function TabDemand(tc As TabControl) As Size
        Dim pages As New Size(0, 0)
        For Each tp As TabPage In tc.TabPages
            Dim d As Size = DockedDemand(tp)
            pages.Width = Math.Max(pages.Width, d.Width)
            pages.Height = Math.Max(pages.Height, d.Height)
        Next
        ' The header strip and the frame: whatever the control keeps outside its display area.
        Dim chrome As New Size(Math.Max(0, tc.Width - tc.DisplayRectangle.Width),
                               Math.Max(0, tc.Height - tc.DisplayRectangle.Height))
        Return New Size(pages.Width + chrome.Width, pages.Height + chrome.Height)
    End Function

    ''' <summary>
    ''' What the content asks for, in CLIENT pixels: the form walked as a container -- the root's
    ''' content demand as the fill term, every docked band at Max(its height, its content), the
    ''' form padding -- plus the current client size, so an operator's enlargement is never
    ''' undone. A band is measured by content and not by height on purpose: a table docked Top
    ''' whose fixed rows grew under Modern keeps its authored height and squeezes its last row;
    ''' only the walk sees that the form must grow. Public so the bench can show the number.
    ''' </summary>
    Public Shared Function Demand(target As Form, root As Control) As Size
        If target Is Nothing Then Throw New ArgumentNullException(NameOf(target))
        If root Is Nothing Then Throw New ArgumentNullException(NameOf(root))
        Try
            Dim content As Size = DockedDemand(target, root)
            Return New Size(Math.Max(content.Width, target.ClientSize.Width),
                            Math.Max(content.Height, target.ClientSize.Height))
        Catch ex As Exception
            GlobalErrorLog.Write("ThemeFormFit.Demand", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Resizes the form to <see cref="Fit"/> of its scaled base, its content demand, its
    ''' <c>MinimumSize</c> and the working area of the screen it is on. One write of
    ''' <c>ClientSize</c>, between SuspendLayout/ResumeLayout, and only when the size changes.
    ''' Returns True when it did.
    '''
    ''' <para>Exits without doing anything at design time or when the window is not
    ''' <c>Normal</c>. A missing snapshot is a programming error and throws: it means
    ''' <see cref="Capture"/> was never called, i.e. the form does not go through the right
    ''' base.</para>
    ''' </summary>
    Public Shared Function Apply(target As Form, root As Control) As Boolean
        If target Is Nothing Then Throw New ArgumentNullException(NameOf(target))
        If root Is Nothing Then Throw New ArgumentNullException(NameOf(root))
        Try
            If KBotDesignTime.IsDesignTime(target) Then Return False
            If target.WindowState <> FormWindowState.Normal Then Return False

            Dim snap As FitSnapshot = Nothing
            If Not _snapshots.TryGetValue(target, snap) Then
                Throw New InvalidOperationException(
                    "ThemeFormFit.Capture was never called for «" & target.Name & "» -- the form does not go through KBotThemedForm.OnCreateControl.")
            End If

            Dim nonClient As New Size(Math.Max(0, target.Width - target.ClientSize.Width),
                                      Math.Max(0, target.Height - target.ClientSize.Height))

            Dim floor As Size = ScaledBaseline(snap.ClientSize, snap.ScaleAtCapture, AppScaling.FactorFor(target), _baseline)
            Dim demand As Size = ThemeFormFit.Demand(target, root)

            Dim minimum As Size = Size.Empty
            If Not target.MinimumSize.IsEmpty Then
                minimum = New Size(Math.Max(0, target.MinimumSize.Width - nonClient.Width),
                                   Math.Max(0, target.MinimumSize.Height - nonClient.Height))
            End If

            Dim area As Rectangle = If(target.IsHandleCreated,
                                       Screen.FromHandle(target.Handle).WorkingArea,
                                       AppScreen.Reference(target).WorkingArea)
            Dim work As New Size(Math.Max(1, area.Width - nonClient.Width),
                                 Math.Max(1, area.Height - nonClient.Height))
            ' A MaximumSize below the working area is a tighter ceiling from the same family:
            ' Windows enforces it through WM_GETMINMAXINFO whether we ask or not, so asking for
            ' more would only be refused silently. Fold it in, so the clamp is ours and is logged.
            If Not target.MaximumSize.IsEmpty Then
                If target.MaximumSize.Width > 0 Then work.Width = Math.Min(work.Width, Math.Max(1, target.MaximumSize.Width - nonClient.Width))
                If target.MaximumSize.Height > 0 Then work.Height = Math.Min(work.Height, Math.Max(1, target.MaximumSize.Height - nonClient.Height))
            End If

            Dim wanted As Size = Fit(floor, demand, minimum, Size.Empty)
            Dim result As Size = Fit(floor, demand, minimum, work)

            If wanted <> result Then
                ' Clamped, and said out loud: the operator log carries the sentence, the error
                ' log the detail. Neither stops the window from opening.
                Dim text As String = "Fereastra «" & target.Text & "» ar avea nevoie de " &
                    wanted.Width & "×" & wanted.Height & " px, dar ecranul oferă doar " &
                    result.Width & "×" & result.Height & " px; a fost tăiată la ecran."
                OperatorLog.Write("ThemeFormFit.Apply", target.Name, text, KBotLogLevel.Warn)
                GlobalErrorLog.Write("ThemeFormFit.Apply",
                    New InvalidOperationException("Form «" & target.Name & "» clamped to the working area: wanted " &
                                                  wanted.ToString() & ", got " & result.ToString() & "."))
            End If

            If result = target.ClientSize Then Return False
            target.SuspendLayout()
            Try
                target.ClientSize = result
            Finally
                target.ResumeLayout(True)
            End Try
            Return True
        Catch ex As Exception
            GlobalErrorLog.Write("ThemeFormFit.Apply", ex)
            Throw
        End Try
    End Function

End Class
