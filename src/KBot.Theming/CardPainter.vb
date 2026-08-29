Option Strict On
Imports System.Drawing
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' Gives a card panel its rounded corners and its shadow, from the ENGINE — no view and no form
''' takes part. This is the whole of decision D-6 of slice 0049: thirteen views would otherwise
''' each need the same paint code, and each would drift.
'''
''' <para><b>Why two handlers and not one.</b> A WinForms child control has no alpha against its
''' parent, so a card cannot punch its own corners transparent, and it cannot draw outside its own
''' rectangle either. So the corners are painted by the CARD (filling the four wedges with the
''' canvas colour, which is what makes them read as round) and the shadow is painted by the
''' PARENT, under the card's bounds, before the card paints itself.</para>
'''
''' <para><b>Which panels.</b> Only <c>Tag = "CardSurface"</c>. The older <c>Tag = "Card"</c> keeps
''' meaning exactly what it meant — «paint me the secondary surface colour» — and sits on 25 panels
''' including the root panel and the header and status strips, none of which are cards in the
''' visual sense. Marking the real cards separately was the alternative to teaching the engine a
''' geometric guess, which got two dialogs wrong.</para>
'''
''' <para><b>Attach is idempotent, detach is complete.</b> Applying a scheme twice does not stack a
''' second handler, and moving to a scheme without cards removes every handler and puts the
''' parent's padding back. The hooks live in a <see cref="ConditionalWeakTable"/> keyed on the
''' control, so a disposed view takes its entry with it.</para>
''' </summary>
Friend Module CardPainter

    ''' <summary>The Tag value that asks for the card treatment.</summary>
    Public Const CardSurfaceTag As String = "CardSurface"

    ' Everything one card needed, kept so detach can undo exactly what attach did.
    Private NotInheritable Class Hooks
        Public Card As Control
        Public Parent As Control
        Public OnCardPaint As PaintEventHandler
        Public OnParentPaint As PaintEventHandler
        Public OnCardMoved As EventHandler
        Public HadGutter As Boolean
        Public ParentPaddingBefore As Padding
    End Class

    Private ReadOnly _hooks As New ConditionalWeakTable(Of Control, Hooks)()

    ' Live tally, for the test that proves three scheme switches do not stack handlers.
    ' A ConditionalWeakTable cannot be counted, so attach and detach keep the number.
    Private _attached As Integer = 0

    ''' <summary>How many cards currently carry paint hooks.</summary>
    Public ReadOnly Property AttachedCount As Integer
        Get
            Return _attached
        End Get
    End Property

    ''' <summary>True for a panel marked <see cref="CardSurfaceTag"/>.</summary>
    Public Function IsCardSurface(ctrl As Control) As Boolean
        If ctrl Is Nothing Then Return False
        If TypeOf ctrl IsNot Panel AndAlso TypeOf ctrl IsNot TableLayoutPanel Then Return False
        Return ctrl.Tag IsNot Nothing AndAlso
               String.Equals(ctrl.Tag.ToString(), CardSurfaceTag, StringComparison.Ordinal)
    End Function

    ''' <summary>
    ''' Brings <paramref name="ctrl"/> into line with <paramref name="scheme"/>: hooks attached if
    ''' the scheme asks for cards, removed if it does not. Safe on any control — anything that is
    ''' not a card surface is ignored.
    ''' </summary>
    Public Sub Sync(ctrl As Control, scheme As ThemeScheme)
        If ctrl Is Nothing OrElse scheme Is Nothing OrElse scheme.Style Is Nothing Then Return
        If Not IsCardSurface(ctrl) Then Return
        Try
            Dim st As ThemeStyleOptions = scheme.Style
            Dim wantsCard As Boolean = st.CardRadius > 0 OrElse
                                       (st.CardShadow > 0 AndAlso st.CardShadowOpacity > 0)
            If wantsCard Then
                Attach(ctrl, st)
            Else
                Detach(ctrl)
            End If
        Catch ex As Exception
            ' A card that fails to gain its hooks is a flat card, not a broken window.
            GlobalErrorLog.Write("CardPainter.Sync", ex)
        End Try
    End Sub

    ''' <summary>Removes the hooks from every card under <paramref name="root"/>, inclusive.</summary>
    Public Sub DetachAll(root As Control)
        If root Is Nothing Then Return
        Try
            If IsCardSurface(root) Then Detach(root)
            For Each child As Control In root.Controls
                DetachAll(child)
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("CardPainter.DetachAll", ex)
        End Try
    End Sub

    ' ── attach / detach ───────────────────────────────────────────────────────

    Private Sub Attach(card As Control, st As ThemeStyleOptions)
        Dim existing As Hooks = Nothing
        If _hooks.TryGetValue(card, existing) Then
            ' Already hooked. The scheme's numbers are read at PAINT time rather than captured
            ' here, so there is nothing to refresh — only the gutter, and a repaint.
            ApplyGutter(existing, st)
            card.Invalidate()
            existing.Parent?.Invalidate()
            Return
        End If

        Dim host As Control = card.Parent
        Dim h As New Hooks With {
            .Card = card,
            .Parent = host,
            .ParentPaddingBefore = If(host IsNot Nothing, host.Padding, New Padding(0))
        }

        h.OnCardPaint = Sub(sender As Object, e As PaintEventArgs) PaintCard(card, e)
        AddHandler card.Paint, h.OnCardPaint

        If host IsNot Nothing Then
            h.OnParentPaint = Sub(sender As Object, e As PaintEventArgs) PaintShadows(host, e)
            AddHandler host.Paint, h.OnParentPaint

            ' The shadow lives in the parent's pixels, so it goes stale the moment the card moves
            ' or resizes — a splitter drag, most obviously. This is a repaint hook, not new
            ' behaviour: it invalidates and does nothing else.
            h.OnCardMoved = Sub(sender As Object, e As EventArgs) host.Invalidate()
            AddHandler card.SizeChanged, h.OnCardMoved
            AddHandler card.LocationChanged, h.OnCardMoved

            SetDoubleBuffered(host)
        End If

        SetDoubleBuffered(card)
        ApplyGutter(h, st)

        _hooks.Add(card, h)
        _attached += 1
        card.Invalidate()
        host?.Invalidate()
    End Sub

    Private Sub Detach(card As Control)
        Dim h As Hooks = Nothing
        If Not _hooks.TryGetValue(card, h) Then Return

        If h.OnCardPaint IsNot Nothing Then RemoveHandler card.Paint, h.OnCardPaint
        If h.Parent IsNot Nothing Then
            If h.OnParentPaint IsNot Nothing Then RemoveHandler h.Parent.Paint, h.OnParentPaint
            If h.OnCardMoved IsNot Nothing Then
                RemoveHandler card.SizeChanged, h.OnCardMoved
                RemoveHandler card.LocationChanged, h.OnCardMoved
            End If
            If h.HadGutter Then h.Parent.Padding = h.ParentPaddingBefore
            h.Parent.Invalidate()
        End If

        _hooks.Remove(card)
        _attached = Math.Max(0, _attached - 1)
        card.Invalidate()
    End Sub

    ' The card must not sit flush against the parent's edge, or its shadow has nowhere to fall.
    ' CardGutter = 0 means «do not touch the parent's padding», which is what every neutral scheme
    ' asks for, so nothing authored in a designer moves.
    Private Sub ApplyGutter(h As Hooks, st As ThemeStyleOptions)
        If h.Parent Is Nothing Then Return
        If st.CardGutter <= 0 Then
            If h.HadGutter Then
                h.Parent.Padding = h.ParentPaddingBefore
                h.HadGutter = False
            End If
            Return
        End If
        Dim g As Integer = ThemeShapes.ScaleDpi(h.Parent, st.CardGutter)
        Dim wanted As New Padding(Math.Max(h.ParentPaddingBefore.Left, g),
                                  Math.Max(h.ParentPaddingBefore.Top, g),
                                  Math.Max(h.ParentPaddingBefore.Right, g),
                                  Math.Max(h.ParentPaddingBefore.Bottom, g))
        If h.Parent.Padding <> wanted Then h.Parent.Padding = wanted
        h.HadGutter = True
    End Sub

    ' ── painting ──────────────────────────────────────────────────────────────

    ' Fill first, corners second: the corner wedges have to go OVER the fill, or the fill covers
    ' them straight back up.
    Private Sub PaintCard(card As Control, e As PaintEventArgs)
        Try
            Dim scheme As ThemeScheme = ThemeManager.Current
            If scheme Is Nothing Then Return
            Dim radius As Integer = ThemeShapes.ScaleDpi(card, scheme.Style.CardRadius)
            Dim bounds As Rectangle = card.ClientRectangle
            ThemeShapes.FillCard(e.Graphics, bounds, radius,
                                 scheme.Palette.CardColor, scheme.Palette.CardBorderColor)
            ThemeShapes.PaintCardCorners(e.Graphics, bounds, radius, scheme.Palette.CanvasColor)
        Catch ex As Exception
            ' Paint boundary: a throw from here takes the process down.
            GlobalErrorLog.Write("CardPainter.PaintCard", ex)
        End Try
    End Sub

    Private Sub PaintShadows(host As Control, e As PaintEventArgs)
        Try
            Dim scheme As ThemeScheme = ThemeManager.Current
            If scheme Is Nothing Then Return
            Dim st As ThemeStyleOptions = scheme.Style
            If st.CardShadow <= 0 OrElse st.CardShadowOpacity <= 0 Then Return

            Dim size As Integer = ThemeShapes.ScaleDpi(host, st.CardShadow)
            Dim radius As Integer = ThemeShapes.ScaleDpi(host, st.CardRadius)
            For Each child As Control In host.Controls
                If Not IsCardSurface(child) Then Continue For
                If Not child.Visible Then Continue For
                ThemeShapes.DrawCardShadow(e.Graphics, child.Bounds, radius,
                                           scheme.Palette.ShadowColor, size, st.CardShadowOpacity)
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("CardPainter.PaintShadows", ex)
        End Try
    End Sub

    ' Control.DoubleBuffered is Protected and these are stock Panels, so there is no subclass to
    ' set it on, and no house helper for it either — checked across src/, where every other use is
    ' inside a control that owns itself. Reflection is the remaining route. Failing is survivable:
    ' the card flickers a little more while resizing, nothing else.
    Private Sub SetDoubleBuffered(ctrl As Control)
        Try
            Dim prop As PropertyInfo = GetType(Control).GetProperty(
                "DoubleBuffered", BindingFlags.Instance Or BindingFlags.NonPublic)
            prop?.SetValue(ctrl, True, Nothing)
        Catch ex As Exception
            GlobalErrorLog.Write("CardPainter.SetDoubleBuffered", ex)
        End Try
    End Sub

End Module
