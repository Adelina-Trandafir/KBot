Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' Randare „modern” (nivel-b) pentru controale stock, în loc (fără a înlocui
''' controalele în toată soluția): butoane plate cu colțuri rotunjite + hover/pressed
''' din paletă, plus underline accent pe focus la inputuri. Handler-ele sunt atașate
''' idempotent (guard prin ConditionalWeakTable) și scoase la HandleDestroyed, ca
''' re-tematizarea live să nu dubleze abonarea sau să scurgă. Orice corp de pictură e
''' învelit Try/Catch → GlobalErrorLog (o excepție de paint nu trebuie să dărâme firul UI).
''' </summary>
Public Module ModernRenderer

    ' Stare per-buton: evită re-atașarea handler-elor la fiecare re-tematizare.
    Private NotInheritable Class ButtonState
        Public Attached As Boolean
        Public Scheme As ThemeScheme

        ''' <summary>
        ''' Marginea și înălțimea AUTORATE (cele din designer), reținute înainte de prima scriere a
        ''' temei. Fără ele, schema modernă și-ar compune propriul rezultat: umplutura ei ar rămâne
        ''' pe buton la întoarcerea în Classic, iar înălțimea crescută n-ar mai avea la ce reveni.
        ''' Același tipar ca lățimea autorată a unei coloane (felia 0028-05).
        ''' </summary>
        Public HasAuthored As Boolean
        Public AuthoredPadding As Padding
        Public AuthoredHeight As Integer
    End Class

    Private ReadOnly _buttons As New ConditionalWeakTable(Of Button, ButtonState)()

    ' Inputuri cu underline accent atașat (marker; valoarea e irelevantă).
    Private ReadOnly _focusInputs As New ConditionalWeakTable(Of Control, Object)()
    ' Părinți al căror Paint desenează underline-ul (marker, ca să atașăm o singură dată).
    Private ReadOnly _focusParents As New ConditionalWeakTable(Of Control, Object)()

    ' Un singur control are focus la un moment dat — suficient pentru underline.
    Private _focusedInput As Control = Nothing
    Private _focusScheme As ThemeScheme = Nothing

    ' =========================================================================
    ' BUTOANE
    ' =========================================================================
    ''' <summary>Aplică randarea modern pe un buton (plat, rotunjit, hover/pressed).</summary>
    Public Sub ApplyButton(btn As Button, scheme As ThemeScheme)
        If btn Is Nothing OrElse scheme Is Nothing Then Return
        Dim st As ButtonState = _buttons.GetValue(btn, Function(b) New ButtonState())
        st.Scheme = scheme

        ' Instantaneul valorilor autorate, ÎNAINTE de orice scriere a temei (idempotent).
        If Not st.HasAuthored Then
            st.HasAuthored = True
            st.AuthoredPadding = btn.Padding
            st.AuthoredHeight = btn.Height
        End If

        Dim p As ThemePalette = scheme.Palette
        btn.FlatStyle = FlatStyle.Flat
        btn.FlatAppearance.BorderSize = 1
        btn.FlatAppearance.BorderColor = p.ButtonBorderColor
        btn.FlatAppearance.MouseOverBackColor = p.ButtonHoverColor
        btn.FlatAppearance.MouseDownBackColor = p.ButtonPressedColor
        btn.BackColor = p.ButtonBackColor
        btn.ForeColor = p.ButtonTextColor
        btn.UseVisualStyleBackColor = False
        If scheme.Style.PaddingValue <> Padding.Empty Then
            btn.Padding = scheme.Style.PaddingValue
            ' …iar butonul crește cât să încapă ȘI umplutura, ȘI textul. Umplutura schemei moderne
            ' (12,8,12,8) mănâncă 16px din înălțime: pe un buton autorat la 32px mai rămâneau 16, în
            ' care un rând de text nu încape — și textul dispărea la comutarea temei. O temă are voie
            ' să ceară aer în jurul textului, dar nu are voie să ascundă textul.
            FitHeightToPaddingAndText(btn, st)
        End If

        If Not st.Attached Then
            st.Attached = True
            AddHandler btn.Resize, AddressOf OnButtonResize
            AddHandler btn.HandleCreated, AddressOf OnButtonResize
            AddHandler btn.HandleDestroyed, AddressOf OnButtonDestroyed
            AddHandler btn.Paint, AddressOf OnButtonPaint
        End If

        UpdateButtonRegion(btn, scheme)
    End Sub

    ''' <summary>
    ''' A plain text button is painted by us (anti-aliased rounded outline) instead of being cut with a window region. A region is
    ''' a hard-edged, pixel-stepped clip: it also erases the corners of the rectangular flat border Windows draws, so the sides had
    ''' an outline and the corners had none. Buttons with an image keep the old region path (their content is not ours to redraw).
    ''' </summary>
    Private Function PaintsItself(btn As Button, scheme As ThemeScheme) As Boolean
        ' Radius 0 = square corners: the stock flat button is already right, leave it alone.
        Return scheme IsNot Nothing AndAlso scheme.Style.CornerRadius > 0 AndAlso
               btn.Image Is Nothing AndAlso btn.BackgroundImage Is Nothing AndAlso btn.ImageList Is Nothing
    End Function

    ' Runs AFTER the stock button painted itself (the Paint event is raised last, inside the same double-buffered pass), so this
    ' repaints the whole face: the surface behind the corners, the rounded fill and outline, then the caption.
    Private Sub OnButtonPaint(sender As Object, e As PaintEventArgs)
        Try
            Dim btn As Button = TryCast(sender, Button)
            If btn Is Nothing Then Return
            Dim st As ButtonState = Nothing
            If Not _buttons.TryGetValue(btn, st) OrElse Not PaintsItself(btn, st.Scheme) Then Return
            Dim p As ThemePalette = st.Scheme.Palette
            Dim g As Graphics = e.Graphics

            Dim inside As Boolean = btn.Enabled AndAlso btn.ClientRectangle.Contains(btn.PointToClient(Control.MousePosition))
            Dim pressed As Boolean = inside AndAlso (Control.MouseButtons And MouseButtons.Left) <> 0
            Dim back As Color
            Dim border As Color
            Dim text As Color
            If Not btn.Enabled Then
                back = p.SurfaceAltColor
                border = p.BorderColor
                text = p.DisabledTextColor
            Else
                back = If(pressed, btn.FlatAppearance.MouseDownBackColor, If(inside, btn.FlatAppearance.MouseOverBackColor, btn.BackColor))
                border = If(btn.Focused, p.AccentColor, btn.FlatAppearance.BorderColor)
                text = btn.ForeColor
            End If

            g.Clear(ThemeShapes.BackdropColor(btn))
            g.SmoothingMode = SmoothingMode.AntiAlias
            ' Pixel centres on half-integers: a 1px outline on a half coordinate fills one whole row and the corner arc keeps the
            ' weight of the sides (see ThemeShapes.RoundedRectF).
            g.PixelOffsetMode = PixelOffsetMode.Half
            Dim thick As Integer = Math.Max(1, btn.FlatAppearance.BorderSize)
            Dim radius As Integer = ScaleForDpi(btn, st.Scheme.Style.CornerRadius)
            Using path As GraphicsPath = ThemeShapes.RoundedRectF(ThemeShapes.FrameBounds(btn.Size, thick), radius)
                Using b As New SolidBrush(back)
                    g.FillPath(b, path)
                End Using
                Using pen As New Pen(border, CSng(thick))
                    g.DrawPath(pen, path)
                End Using
            End Using
            g.PixelOffsetMode = PixelOffsetMode.Default

            Dim area As Rectangle = Rectangle.Round(New RectangleF(
                btn.Padding.Left, btn.Padding.Top,
                Math.Max(1, btn.Width - btn.Padding.Horizontal), Math.Max(1, btn.Height - btn.Padding.Vertical)))
            Dim flags As TextFormatFlags = TextFlags(btn.TextAlign) Or TextFormatFlags.EndEllipsis
            If Not btn.UseMnemonic Then flags = flags Or TextFormatFlags.NoPrefix
            TextRenderer.DrawText(g, If(btn.Text, String.Empty), btn.Font, area, text, flags)
        Catch ex As Exception
            GlobalErrorLog.Write("ModernRenderer.OnButtonPaint", ex)
        End Try
    End Sub

    Private Function TextFlags(align As ContentAlignment) As TextFormatFlags
        Dim flags As TextFormatFlags = TextFormatFlags.SingleLine
        Select Case align
            Case ContentAlignment.TopLeft, ContentAlignment.MiddleLeft, ContentAlignment.BottomLeft
                flags = flags Or TextFormatFlags.Left
            Case ContentAlignment.TopRight, ContentAlignment.MiddleRight, ContentAlignment.BottomRight
                flags = flags Or TextFormatFlags.Right
            Case Else
                flags = flags Or TextFormatFlags.HorizontalCenter
        End Select
        Select Case align
            Case ContentAlignment.TopLeft, ContentAlignment.TopCenter, ContentAlignment.TopRight
                flags = flags Or TextFormatFlags.Top
            Case ContentAlignment.BottomLeft, ContentAlignment.BottomCenter, ContentAlignment.BottomRight
                flags = flags Or TextFormatFlags.Bottom
            Case Else
                flags = flags Or TextFormatFlags.VerticalCenter
        End Select
        Return flags
    End Function

    ''' <summary>
    ''' Cât trebuie să fie ÎNALT butonul ca să încapă un rând de text peste umplutura cerută de
    ''' schemă: umplutura + textul + cele două chenare. Se ia MAXIMUL cu înălțimea autorată — o temă
    ''' poate mări un buton ca să-i intre textul, dar n-are voie să strice o înălțime aleasă anume
    ''' (aceeași regulă ca la lățimea coloanelor: se crește la nevoie, nu se rescrie designul).
    ''' </summary>
    Private Sub FitHeightToPaddingAndText(btn As Button, st As ButtonState)
        Try
            If btn.AutoSize Then Return                  ' WinForms se ocupă singur
            ' Când butonul e andocat pe o latură verticală (sau umple), înălțimea lui e a
            ' PĂRINTELUI, nu a lui: o scriere aici ar fi ștearsă de următorul layout.
            If btn.Dock = DockStyle.Left OrElse btn.Dock = DockStyle.Right OrElse
               btn.Dock = DockStyle.Fill Then Return

            Dim text As String = If(String.IsNullOrEmpty(btn.Text), "Wg", btn.Text)
            Dim textH As Integer = TextRenderer.MeasureText(text, btn.Font).Height
            Dim chenar As Integer = If(btn.FlatStyle = FlatStyle.Flat, 2 * btn.FlatAppearance.BorderSize, 2)
            Dim nevoie As Integer = btn.Padding.Vertical + textH + chenar

            Dim inaltime As Integer = Math.Max(st.AuthoredHeight, nevoie)
            If btn.Height <> inaltime Then btn.Height = inaltime
        Catch ex As Exception
            GlobalErrorLog.Write("ModernRenderer.FitHeightToPaddingAndText", ex)
        End Try
    End Sub

    ''' <summary>Scoate randarea modern (revenire la buton standard). Idempotent.</summary>
    Public Sub DetachButton(btn As Button)
        If btn Is Nothing Then Return
        Dim st As ButtonState = Nothing
        If Not _buttons.TryGetValue(btn, st) Then Return

        ' Marginea și înălțimea autorate se dau ÎNAPOI: altfel ieșirea din schema modernă ar lăsa
        ' butonul cu umplutura ei (12,8,12,8) și cu înălțimea crescută pentru ea — adică schema ar
        ' rescrie permanent designul, exact ce nu are voie să facă.
        If st.HasAuthored Then
            btn.Padding = st.AuthoredPadding
            If Not btn.AutoSize AndAlso btn.Dock <> DockStyle.Left AndAlso
               btn.Dock <> DockStyle.Right AndAlso btn.Dock <> DockStyle.Fill Then
                btn.Height = st.AuthoredHeight
            End If
        End If

        If st.Attached Then
            RemoveHandler btn.Resize, AddressOf OnButtonResize
            RemoveHandler btn.HandleCreated, AddressOf OnButtonResize
            RemoveHandler btn.HandleDestroyed, AddressOf OnButtonDestroyed
            RemoveHandler btn.Paint, AddressOf OnButtonPaint
        End If
        _buttons.Remove(btn)
        btn.Invalidate()
        Try
            btn.Region = Nothing
        Catch ex As Exception
            GlobalErrorLog.Write("ModernRenderer.DetachButton", ex)
        End Try
    End Sub

    Private Sub OnButtonResize(sender As Object, e As EventArgs)
        Dim btn = TryCast(sender, Button)
        If btn Is Nothing Then Return
        Dim st As ButtonState = Nothing
        If _buttons.TryGetValue(btn, st) AndAlso st.Scheme IsNot Nothing Then
            UpdateButtonRegion(btn, st.Scheme)
        End If
    End Sub

    Private Sub OnButtonDestroyed(sender As Object, e As EventArgs)
        Dim btn = TryCast(sender, Button)
        If btn IsNot Nothing Then DetachButton(btn)
    End Sub

    ' Setează Region la un dreptunghi rotunjit; rază scalată la DPI. 0 => fără rotunjire.
    Private Sub UpdateButtonRegion(btn As Button, scheme As ThemeScheme)
        Try
            ' A plain text button is painted by us (OnButtonPaint): no hard-edged window region.
            If PaintsItself(btn, scheme) Then
                btn.Region = Nothing
                btn.Invalidate()
                Return
            End If
            Dim radiusLogical As Integer = scheme.Style.CornerRadius
            If radiusLogical <= 0 OrElse btn.Width <= 0 OrElse btn.Height <= 0 Then
                btn.Region = Nothing
                Return
            End If
            Dim radius As Integer = ScaleForDpi(btn, radiusLogical)
            Dim diameter As Integer = Math.Min(radius * 2, Math.Min(btn.Width, btn.Height))
            If diameter <= 0 Then
                btn.Region = Nothing
                Return
            End If
            Using path As GraphicsPath = RoundedRect(New Rectangle(0, 0, btn.Width, btn.Height), diameter)
                btn.Region = New Region(path)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("ModernRenderer.UpdateButtonRegion", ex)
        End Try
    End Sub

    ' =========================================================================
    ' UNDERLINE ACCENT PE FOCUS (TextBox / ComboBox / MaskedTextBox)
    ' =========================================================================
    ''' <summary>Atașează underline accent pe focus. Idempotent.</summary>
    Public Sub AttachFocusAccent(ctrl As Control, scheme As ThemeScheme)
        If ctrl Is Nothing OrElse scheme Is Nothing Then Return
        Dim dummy As Object = Nothing
        If Not _focusInputs.TryGetValue(ctrl, dummy) Then
            _focusInputs.Add(ctrl, New Object())
            AddHandler ctrl.Enter, AddressOf OnInputEnter
            AddHandler ctrl.Leave, AddressOf OnInputLeave
            AddHandler ctrl.HandleDestroyed, AddressOf OnInputDestroyed
        End If
        ' Reține schema curentă pentru culoarea accentului la următoarea pictură.
        _focusScheme = scheme
    End Sub

    ''' <summary>Scoate underline-ul accent (revenire fără focus custom). Idempotent.</summary>
    Public Sub DetachFocusAccent(ctrl As Control)
        If ctrl Is Nothing Then Return
        Dim dummy As Object = Nothing
        If _focusInputs.TryGetValue(ctrl, dummy) Then
            RemoveHandler ctrl.Enter, AddressOf OnInputEnter
            RemoveHandler ctrl.Leave, AddressOf OnInputLeave
            RemoveHandler ctrl.HandleDestroyed, AddressOf OnInputDestroyed
            _focusInputs.Remove(ctrl)
        End If
        If _focusedInput Is ctrl Then
            _focusedInput = Nothing
            InvalidateUnderline(ctrl)
        End If
    End Sub

    Private Sub OnInputEnter(sender As Object, e As EventArgs)
        Dim ctrl = TryCast(sender, Control)
        If ctrl Is Nothing Then Return
        _focusedInput = ctrl
        EnsureParentPaint(ctrl.Parent)
        InvalidateUnderline(ctrl)
    End Sub

    Private Sub OnInputLeave(sender As Object, e As EventArgs)
        Dim ctrl = TryCast(sender, Control)
        If ctrl Is Nothing Then Return
        If _focusedInput Is ctrl Then _focusedInput = Nothing
        InvalidateUnderline(ctrl)
    End Sub

    Private Sub OnInputDestroyed(sender As Object, e As EventArgs)
        Dim ctrl = TryCast(sender, Control)
        If ctrl IsNot Nothing Then DetachFocusAccent(ctrl)
    End Sub

    ' Atașează (o singură dată) handler-ul de Paint pe părinte care desenează underline-ul.
    Private Sub EnsureParentPaint(parent As Control)
        If parent Is Nothing Then Return
        Dim dummy As Object = Nothing
        If _focusParents.TryGetValue(parent, dummy) Then Return
        _focusParents.Add(parent, New Object())
        AddHandler parent.Paint, AddressOf OnParentPaint
    End Sub

    Private Sub OnParentPaint(sender As Object, e As PaintEventArgs)
        Try
            Dim parent = TryCast(sender, Control)
            If parent Is Nothing Then Return
            Dim ctrl As Control = _focusedInput
            If ctrl Is Nothing OrElse Not ctrl.Visible Then Return
            If ctrl.Parent IsNot parent Then Return
            Dim scheme As ThemeScheme = If(_focusScheme, ThemeManager.Current)
            Dim thickness As Integer = Math.Max(2, ScaleForDpi(parent, 2))
            Dim y As Integer = ctrl.Bottom - thickness
            Using b As New SolidBrush(scheme.Palette.FocusRingColor)
                e.Graphics.FillRectangle(b, ctrl.Left, y, ctrl.Width, thickness)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("ModernRenderer.OnParentPaint", ex)
        End Try
    End Sub

    Private Sub InvalidateUnderline(ctrl As Control)
        Try
            Dim parent As Control = ctrl.Parent
            If parent Is Nothing Then Return
            Dim thickness As Integer = Math.Max(2, ScaleForDpi(parent, 2))
            parent.Invalidate(New Rectangle(ctrl.Left, ctrl.Bottom - thickness, ctrl.Width, thickness + 1))
        Catch ex As Exception
            GlobalErrorLog.Write("ModernRenderer.InvalidateUnderline", ex)
        End Try
    End Sub

    ' =========================================================================
    ' HELPERS
    ' =========================================================================
    Private Function ScaleForDpi(ctrl As Control, logical As Integer) As Integer
        Dim dpi As Integer = 96
        Try
            dpi = ctrl.DeviceDpi
        Catch
            dpi = 96
        End Try
        Return CInt(Math.Round(logical * dpi / 96.0))
    End Function

    Private Function RoundedRect(bounds As Rectangle, diameter As Integer) As GraphicsPath
        Dim path As New GraphicsPath()
        Dim arc As New Rectangle(bounds.Location, New Size(diameter, diameter))
        path.AddArc(arc, 180, 90)                                   ' stânga-sus
        arc.X = bounds.Right - diameter
        path.AddArc(arc, 270, 90)                                   ' dreapta-sus
        arc.Y = bounds.Bottom - diameter
        path.AddArc(arc, 0, 90)                                     ' dreapta-jos
        arc.X = bounds.Left
        path.AddArc(arc, 90, 90)                                    ' stânga-jos
        path.CloseFigure()
        Return path
    End Function

End Module
