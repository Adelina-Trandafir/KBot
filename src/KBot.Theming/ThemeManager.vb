Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' Motorul central de teme (înlocuiește miezul vechiului KBotTheme). Palette+Style
''' driven, nu „If dark”. Punctul unic de intrare: <see cref="Apply"/>. Comutare live
''' prin <see cref="SetScheme"/> + evenimentul <see cref="ThemeChanged"/>.
'''
''' Difuzarea la comutare: reaplică pe reuniunea (registru de formulare tematizate ∪
''' Application.OpenForms), deduplicată — deci și formularele legacy (ne-migrate) se
''' re-tematizează, exact ca vechiul SetTheme. Formularele KBotThemedForm rulează
''' apoi DOAR OnThemeChanged() din handler-ul de eveniment (Apply deja s-a executat
''' în difuzare), evitând dublul Apply.
''' </summary>
Public Module ThemeManager

    Private _current As ThemeScheme = BuiltInSchemes.Classic()
    Private ReadOnly _userSchemes As New List(Of ThemeScheme)()
    Private ReadOnly _forms As New List(Of WeakReference(Of Form))()
    Private _initialized As Boolean = False

    ''' <summary>Schema activă curentă.</summary>
    Public ReadOnly Property Current As ThemeScheme
        Get
            Return _current
        End Get
    End Property

    ''' <summary>Cele 3 scheme built-in + orice scheme utilizator descoperite.</summary>
    Public ReadOnly Property AvailableSchemes As IReadOnlyList(Of ThemeScheme)
        Get
            Dim list As New List(Of ThemeScheme)(BuiltInSchemes.All())
            list.AddRange(_userSchemes)
            Return list
        End Get
    End Property

    ''' <summary>Ridicat DUPĂ ce Current s-a schimbat (Apply deja difuzat).</summary>
    Public Event ThemeChanged As EventHandler

    ''' <summary>
    ''' Încarcă schema persistată (sau default = Classic) + schemele utilizator din
    ''' AppData. Idempotent — apelabil o singură dată la pornire.
    ''' </summary>
    Public Sub Initialize()
        If _initialized Then Return
        _initialized = True

        ' Scheme utilizator (editor viitor). Un fișier corupt e sărit + logat, nu crapă pornirea.
        _userSchemes.Clear()
        _userSchemes.AddRange(ThemeStore.LoadUserSchemes())

        ' Numele schemei active persistat; fallback documentat = Classic.
        Dim activeName As String = ThemeStore.LoadActiveName()
        Dim resolved As ThemeScheme = ResolveByName(activeName)
        _current = If(resolved, BuiltInSchemes.Classic())
    End Sub

    ''' <summary>Rezolvă un nume la o schemă (built-in sau utilizator); Nothing dacă nu există.</summary>
    ''' <remarks>Friend pentru teste: contractul de fallback (nume necunoscut → Nothing → Classic).</remarks>
    Friend Function ResolveByName(name As String) As ThemeScheme
        If String.IsNullOrWhiteSpace(name) Then Return Nothing
        For Each s In AvailableSchemes
            If String.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase) Then
                Return s
            End If
        Next
        Return Nothing
    End Function

    ''' <summary>Aplică schema curentă unui control/formular (recursiv).</summary>
    Public Sub Apply(ctrl As Control)
        If ctrl Is Nothing Then Return
        ctrl.SuspendLayout()
        Try
            Traverse(ctrl)
        Finally
            ctrl.ResumeLayout(True)
        End Try

        ' Bară de titlu dark/light (DWM). Contract log-once în NativeMethods.
        Dim f As Form = TryCast(ctrl, Form)
        If f IsNot Nothing Then
            NativeMethods.SetTitleBarDark(f, _current.Style.DarkTitleBar)
        End If
    End Sub

    ''' <summary>Setează schema activă, o persistă, o difuzează și ridică ThemeChanged.</summary>
    Public Sub SetScheme(scheme As ThemeScheme)
        If scheme Is Nothing Then Throw New ArgumentNullException(NameOf(scheme))
        _current = scheme
        ThemeStore.SaveActive(scheme.Name)

        ' Difuzare: registru ∪ OpenForms, deduplicat pe identitate de referință.
        For Each f As Form In CollectTargets()
            Apply(f)
        Next

        RaiseEvent ThemeChanged(Nothing, EventArgs.Empty)
    End Sub

    ' Reuniunea formularelor tematizate înregistrate și a celor deschise (legacy incluse).
    Private Function CollectTargets() As List(Of Form)
        Dim targets As New List(Of Form)()
        PurgeDeadForms()
        For Each wr In _forms
            Dim f As Form = Nothing
            If wr.TryGetTarget(f) AndAlso f IsNot Nothing AndAlso Not targets.Contains(f) Then
                targets.Add(f)
            End If
        Next
        For Each f As Form In Application.OpenForms
            If f IsNot Nothing AndAlso Not targets.Contains(f) Then
                targets.Add(f)
            End If
        Next
        Return targets
    End Function

    ''' <summary>Înregistrează un formular tematizat (apelat de KBotThemedForm).</summary>
    Friend Sub RegisterForm(f As Form)
        If f Is Nothing Then Return
        PurgeDeadForms()
        For Each wr In _forms
            Dim existing As Form = Nothing
            If wr.TryGetTarget(existing) AndAlso existing Is f Then Return
        Next
        _forms.Add(New WeakReference(Of Form)(f))
    End Sub

    ''' <summary>Dezînregistrează un formular tematizat.</summary>
    Friend Sub UnregisterForm(f As Form)
        If f Is Nothing Then Return
        For i As Integer = _forms.Count - 1 To 0 Step -1
            Dim existing As Form = Nothing
            If Not _forms(i).TryGetTarget(existing) OrElse existing Is f Then
                _forms.RemoveAt(i)
            End If
        Next
    End Sub

    Private Sub PurgeDeadForms()
        For i As Integer = _forms.Count - 1 To 0 Step -1
            Dim existing As Form = Nothing
            If Not _forms(i).TryGetTarget(existing) OrElse existing Is Nothing Then
                _forms.RemoveAt(i)
            End If
        Next
    End Sub

    ' =========================================================================
    ' TRAVERSARE RECURSIVĂ (port verbatim din KBotTheme, cu excepțiile SplitContainer/TabControl)
    ' =========================================================================
    Private Sub Traverse(ctrl As Control)
        StyleControl(ctrl)

        If TypeOf ctrl Is SplitContainer Then
            Dim sc = DirectCast(ctrl, SplitContainer)
            For Each child As Control In sc.Panel1.Controls
                Traverse(child)
            Next
            For Each child As Control In sc.Panel2.Controls
                Traverse(child)
            Next
            Return
        End If

        If TypeOf ctrl Is TabControl Then
            Dim tc = DirectCast(ctrl, TabControl)
            For Each tp As TabPage In tc.TabPages
                Traverse(tp)
            Next
            Return
        End If

        For Each child As Control In ctrl.Controls
            Traverse(child)
        Next
    End Sub

    ' =========================================================================
    ' STILIZARE PER CONTROL — comută pe UseSystemColors, altfel merge pe paletă
    ' =========================================================================
    Private Sub StyleControl(ctrl As Control)
        If _current.Style.UseSystemColors Then
            StyleSystem(ctrl)
        Else
            StylePalette(ctrl, _current)
        End If
    End Sub

    ' ─────────────────── PALETĂ (Dark / Modern / scheme viitoare) ─────────────
    Private Sub StylePalette(ctrl As Control, scheme As ThemeScheme)
        Dim p As ThemePalette = scheme.Palette
        Dim st As ThemeStyleOptions = scheme.Style
        Dim listTheme As String = If(scheme.IsDark, "DarkMode_Explorer", "Explorer")
        Dim comboTheme As String = If(scheme.IsDark, "DarkMode_CFD", "Explorer")

        If TypeOf ctrl Is Form Then
            ctrl.BackColor = p.SurfaceColor
            ApplyBaseFont(ctrl, st)

        ElseIf TypeOf ctrl Is SplitContainer Then
            ctrl.BackColor = p.SurfaceColor
            DirectCast(ctrl, SplitContainer).Panel1.BackColor = p.SurfaceColor
            DirectCast(ctrl, SplitContainer).Panel2.BackColor = p.SurfaceColor

        ElseIf TypeOf ctrl Is TabControl Then
            Dim tc = DirectCast(ctrl, TabControl)
            tc.BackColor = p.SurfaceColor
            SetupTabOwnerDraw(tc, st.OwnerDrawTabs)

        ElseIf TypeOf ctrl Is TabPage Then
            ctrl.BackColor = p.SurfaceAltColor

        ElseIf TypeOf ctrl Is TableLayoutPanel Then
            ctrl.BackColor = p.SurfaceColor

        ElseIf TypeOf ctrl Is Panel Then
            ctrl.BackColor = p.SurfaceColor

        ElseIf TypeOf ctrl Is GroupBox Then
            ctrl.BackColor = p.SurfaceColor
            ctrl.ForeColor = p.TextColor

        ElseIf TypeOf ctrl Is Label Then
            ctrl.ForeColor = p.TextColor
            ctrl.BackColor = Color.Transparent

        ElseIf TypeOf ctrl Is CheckBox Then
            Dim chk = DirectCast(ctrl, CheckBox)
            If chk.Appearance = Appearance.Button Then
                chk.FlatStyle = FlatStyle.Flat
                chk.BackColor = p.ButtonBackColor
                chk.ForeColor = p.ButtonTextColor
                chk.FlatAppearance.BorderColor = p.ButtonBorderColor
                chk.FlatAppearance.MouseOverBackColor = p.ButtonHoverColor
                chk.FlatAppearance.CheckedBackColor = p.AccentColor
                chk.UseVisualStyleBackColor = False
            Else
                ctrl.ForeColor = p.TextColor
                ctrl.BackColor = Color.Transparent
            End If

        ElseIf TypeOf ctrl Is RadioButton Then
            ctrl.ForeColor = p.TextColor
            ctrl.BackColor = Color.Transparent

        ElseIf TypeOf ctrl Is Button Then
            Dim btn = DirectCast(ctrl, Button)
            If btn.Tag?.ToString() = "ThemeToggle" Then
                UpdateToggleButton(btn)
            ElseIf Not IsAccentButton(btn) Then
                If st.ButtonRender = ButtonRenderStyle.ModernOwnerDrawn Then
                    ModernRenderer.ApplyButton(btn, scheme)
                Else
                    ModernRenderer.DetachButton(btn)
                    btn.FlatStyle = FlatStyle.Flat
                    btn.BackColor = p.ButtonBackColor
                    btn.ForeColor = p.ButtonTextColor
                    btn.FlatAppearance.BorderColor = p.ButtonBorderColor
                    btn.FlatAppearance.MouseOverBackColor = p.ButtonHoverColor
                    btn.UseVisualStyleBackColor = False
                End If
            End If

        ElseIf TypeOf ctrl Is RichTextBox Then
            If ctrl.Tag?.ToString() = "SyntaxRTB" Then
                NativeMethods.ApplyWindowTheme(ctrl, listTheme)
            Else
                ctrl.BackColor = p.InputBackColor
                ctrl.ForeColor = p.InputTextColor
                NativeMethods.ApplyWindowTheme(ctrl, listTheme)
            End If

        ElseIf TypeOf ctrl Is CheckedListBox Then
            ctrl.BackColor = p.InputBackColor
            ctrl.ForeColor = p.InputTextColor
            NativeMethods.ApplyWindowTheme(ctrl, listTheme)

        ElseIf TypeOf ctrl Is ListBox Then
            ctrl.BackColor = p.InputBackColor
            ctrl.ForeColor = p.InputTextColor
            NativeMethods.ApplyWindowTheme(ctrl, listTheme)

        ElseIf TypeOf ctrl Is TreeView Then
            Dim tv = DirectCast(ctrl, TreeView)
            tv.BackColor = p.InputBackColor
            tv.ForeColor = p.InputTextColor
            NativeMethods.ApplyWindowTheme(tv, listTheme)

        ElseIf TypeOf ctrl Is ComboBox Then
            ctrl.BackColor = p.InputBackColor
            ctrl.ForeColor = p.InputTextColor
            NativeMethods.ApplyWindowTheme(ctrl, comboTheme)
            If st.FocusAccent Then ModernRenderer.AttachFocusAccent(ctrl, scheme) Else ModernRenderer.DetachFocusAccent(ctrl)

        ElseIf TypeOf ctrl Is MaskedTextBox Then
            ctrl.BackColor = p.InputBackColor
            ctrl.ForeColor = p.InputTextColor
            If st.FocusAccent Then ModernRenderer.AttachFocusAccent(ctrl, scheme) Else ModernRenderer.DetachFocusAccent(ctrl)

        ElseIf TypeOf ctrl Is TextBox Then
            ctrl.BackColor = p.InputBackColor
            ctrl.ForeColor = p.InputTextColor
            If st.FocusAccent Then ModernRenderer.AttachFocusAccent(ctrl, scheme) Else ModernRenderer.DetachFocusAccent(ctrl)

        ElseIf TypeOf ctrl Is NumericUpDown Then
            ctrl.BackColor = p.InputBackColor
            ctrl.ForeColor = p.InputTextColor

        ElseIf TypeOf ctrl Is ProgressBar Then
            ' ProgressBar — lăsăm stilul system (ca înainte).

        End If
    End Sub

    ' ─────────────────── SISTEM (Classic; port verbatim din StyleLight) ───────
    Private Sub StyleSystem(ctrl As Control)
        If TypeOf ctrl Is Form Then
            ctrl.BackColor = SystemColors.Control

        ElseIf TypeOf ctrl Is SplitContainer Then
            ctrl.BackColor = SystemColors.Control
            DirectCast(ctrl, SplitContainer).Panel1.BackColor = SystemColors.Control
            DirectCast(ctrl, SplitContainer).Panel2.BackColor = SystemColors.Control

        ElseIf TypeOf ctrl Is TabControl Then
            Dim tc = DirectCast(ctrl, TabControl)
            tc.BackColor = SystemColors.Control
            SetupTabOwnerDraw(tc, False)

        ElseIf TypeOf ctrl Is TabPage Then
            ctrl.BackColor = SystemColors.Control
            DirectCast(ctrl, TabPage).UseVisualStyleBackColor = True

        ElseIf TypeOf ctrl Is TableLayoutPanel Then
            ctrl.BackColor = SystemColors.Control

        ElseIf TypeOf ctrl Is Panel Then
            ctrl.BackColor = SystemColors.Control

        ElseIf TypeOf ctrl Is GroupBox Then
            ctrl.BackColor = SystemColors.Control
            ctrl.ForeColor = SystemColors.ControlText

        ElseIf TypeOf ctrl Is Label Then
            ctrl.ForeColor = SystemColors.ControlText
            ctrl.BackColor = Color.Transparent

        ElseIf TypeOf ctrl Is CheckBox Then
            Dim chk = DirectCast(ctrl, CheckBox)
            If chk.Appearance = Appearance.Button Then
                chk.FlatStyle = FlatStyle.Flat
                chk.BackColor = SystemColors.Control
                chk.ForeColor = SystemColors.ControlText
                chk.FlatAppearance.BorderColor = SystemColors.ControlDark
                chk.FlatAppearance.MouseOverBackColor = SystemColors.ControlLight
                chk.FlatAppearance.CheckedBackColor = SystemColors.Highlight
                chk.UseVisualStyleBackColor = False
            Else
                ctrl.ForeColor = SystemColors.ControlText
                ctrl.BackColor = Color.Transparent
                chk.UseVisualStyleBackColor = True
            End If

        ElseIf TypeOf ctrl Is RadioButton Then
            ctrl.ForeColor = SystemColors.ControlText
            ctrl.BackColor = Color.Transparent

        ElseIf TypeOf ctrl Is Button Then
            Dim btn = DirectCast(ctrl, Button)
            If btn.Tag?.ToString() = "ThemeToggle" Then
                UpdateToggleButton(btn)
            ElseIf Not IsAccentButton(btn) Then
                ModernRenderer.DetachButton(btn)
                btn.FlatStyle = FlatStyle.Standard
                btn.BackColor = SystemColors.Control
                btn.ForeColor = SystemColors.ControlText
                btn.UseVisualStyleBackColor = True
            End If

        ElseIf TypeOf ctrl Is RichTextBox Then
            If ctrl.Tag?.ToString() = "SyntaxRTB" Then
                NativeMethods.ApplyWindowTheme(ctrl, "Explorer")
            Else
                ctrl.BackColor = Color.White
                ctrl.ForeColor = SystemColors.WindowText
                NativeMethods.ApplyWindowTheme(ctrl, "Explorer")
            End If

        ElseIf TypeOf ctrl Is CheckedListBox Then
            ctrl.BackColor = SystemColors.Window
            ctrl.ForeColor = SystemColors.WindowText
            NativeMethods.ApplyWindowTheme(ctrl, "Explorer")

        ElseIf TypeOf ctrl Is ListBox Then
            ctrl.BackColor = SystemColors.Window
            ctrl.ForeColor = SystemColors.WindowText
            NativeMethods.ApplyWindowTheme(ctrl, "Explorer")

        ElseIf TypeOf ctrl Is TreeView Then
            Dim tv = DirectCast(ctrl, TreeView)
            tv.BackColor = SystemColors.Window
            tv.ForeColor = SystemColors.WindowText
            NativeMethods.ApplyWindowTheme(tv, "Explorer")

        ElseIf TypeOf ctrl Is ComboBox Then
            ModernRenderer.DetachFocusAccent(ctrl)
            ctrl.BackColor = SystemColors.Window
            ctrl.ForeColor = SystemColors.WindowText
            NativeMethods.ApplyWindowTheme(ctrl, "Explorer")

        ElseIf TypeOf ctrl Is MaskedTextBox Then
            ModernRenderer.DetachFocusAccent(ctrl)
            ctrl.BackColor = SystemColors.Window
            ctrl.ForeColor = SystemColors.WindowText

        ElseIf TypeOf ctrl Is TextBox Then
            ModernRenderer.DetachFocusAccent(ctrl)
            ctrl.BackColor = SystemColors.Window
            ctrl.ForeColor = SystemColors.WindowText

        ElseIf TypeOf ctrl Is NumericUpDown Then
            ctrl.BackColor = SystemColors.Window
            ctrl.ForeColor = SystemColors.WindowText

        End If
    End Sub

    ' Aplică fontul de bază al schemei pe formular (copiii moștenesc fontul ambiant).
    ' „Segoe UI Variable Text” lipsă => GDI cade elegant pe fontul default (fără excepție).
    Private Sub ApplyBaseFont(ctrl As Control, st As ThemeStyleOptions)
        If String.IsNullOrWhiteSpace(st.BaseFontName) OrElse st.BaseFontSize <= 0F Then Return
        Try
            ctrl.Font = New Font(st.BaseFontName, st.BaseFontSize, ctrl.Font.Style)
        Catch ex As Exception
            GlobalErrorLog.Write("ThemeManager.ApplyBaseFont", ex)
        End Try
    End Sub

    ' =========================================================================
    ' TABCONTROL OWNER DRAW (port din KBotTheme; culorile din paleta curentă)
    ' =========================================================================
    Private Sub SetupTabOwnerDraw(tc As TabControl, ownerDraw As Boolean)
        RemoveHandler tc.DrawItem, AddressOf OnDrawTab
        If ownerDraw Then
            tc.DrawMode = TabDrawMode.OwnerDrawFixed
            AddHandler tc.DrawItem, AddressOf OnDrawTab
        Else
            tc.DrawMode = TabDrawMode.Normal
        End If
        tc.Invalidate()
    End Sub

    Private Sub OnDrawTab(sender As Object, e As DrawItemEventArgs)
        If e.Index < 0 Then Return
        Dim tc = DirectCast(sender, TabControl)
        If e.Index >= tc.TabPages.Count Then Return
        Dim tp = tc.TabPages(e.Index)
        Dim isSelected = (e.Index = tc.SelectedIndex)
        Dim p As ThemePalette = _current.Palette

        Dim bgColor = If(isSelected, p.SurfaceAltColor, p.TabInactiveColor)
        Using bg As New SolidBrush(bgColor)
            e.Graphics.FillRectangle(bg, e.Bounds)
        End Using

        If isSelected Then
            Using accent As New SolidBrush(p.TabAccentColor)
                e.Graphics.FillRectangle(accent,
                    New Rectangle(e.Bounds.X, e.Bounds.Bottom - 2, e.Bounds.Width, 2))
            End Using
        End If

        Using txt As New SolidBrush(p.TextColor)
            Dim sf As New StringFormat() With {
                .Alignment = StringAlignment.Center,
                .LineAlignment = StringAlignment.Center,
                .FormatFlags = StringFormatFlags.NoWrap
            }
            e.Graphics.DrawString(tp.Text, tc.Font, txt,
                                  RectangleF.op_Implicit(e.Bounds), sf)
        End Using
    End Sub

    ' =========================================================================
    ' BUTON TOGGLE TEMĂ (port; „dark” = schema curentă e dark)
    ' =========================================================================
    Private Sub UpdateToggleButton(btn As Button)
        Dim p As ThemePalette = _current.Palette
        If _current.IsDark Then
            btn.Text = "☀️"
            btn.FlatStyle = FlatStyle.Flat
            btn.BackColor = p.ButtonBackColor
            btn.ForeColor = p.ButtonTextColor
            btn.FlatAppearance.BorderColor = p.ButtonBorderColor
            btn.FlatAppearance.MouseOverBackColor = p.ButtonHoverColor
            btn.UseVisualStyleBackColor = False
        Else
            btn.Text = "🌙"
            btn.FlatStyle = FlatStyle.Standard
            btn.BackColor = SystemColors.Control
            btn.ForeColor = SystemColors.ControlText
            btn.UseVisualStyleBackColor = True
        End If
    End Sub

    ' =========================================================================
    ' HELPER: buton cu culoare funcțională (verde/roșu/galben) — NU se re-tematizează
    ' =========================================================================
    Private Function IsAccentButton(btn As Button) As Boolean
        If btn.UseVisualStyleBackColor Then Return False
        Dim c = btn.BackColor
        Return c.GetSaturation() > 0.25F AndAlso
               c <> _current.Palette.ButtonBackColor AndAlso
               c <> Color.Transparent AndAlso
               c.ToArgb() <> SystemColors.Control.ToArgb()
    End Function

End Module
