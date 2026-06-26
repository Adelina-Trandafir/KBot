Imports System.Drawing
Imports System.Windows.Forms
Imports System.Runtime.InteropServices

''' <summary>
''' Modul central pentru tema vizuală KBOT (dark / light).
''' SetTheme(True)  → dark (tema din ResendForm)
''' SetTheme(False) → light (culorile default VB.NET)
''' Apelați ApplyTheme(Me) în constructorul / Load-ul fiecărui formular
''' pentru a aplica tema curentă imediat la deschidere.
''' </summary>
Public Module KBotTheme

    ' =========================================================================
    ' P/INVOKE
    ' =========================================================================
    <DllImport("uxtheme.dll", CharSet:=CharSet.Unicode)>
    Private Function SetWindowTheme(hWnd As IntPtr, pszSubAppName As String, pszSubIdList As String) As Integer
    End Function

    <DllImport("dwmapi.dll")>
    Private Sub DwmSetWindowAttribute(hwnd As IntPtr, dwAttribute As Integer,
                                      ByRef pvAttribute As Integer, cbAttribute As Integer)
    End Sub

    ' =========================================================================
    ' CULORI DARK (identice cu cele din ResendForm)
    ' =========================================================================
    Public ReadOnly CLR_BG As Color = Color.FromArgb(28, 28, 28)
    Public ReadOnly CLR_BG_PANEL As Color = Color.FromArgb(45, 45, 48)
    Public ReadOnly CLR_FG As Color = Color.FromArgb(210, 210, 210)
    Public ReadOnly CLR_FG_DIM As Color = Color.FromArgb(115, 115, 115)
    Public ReadOnly CLR_BTN As Color = Color.FromArgb(62, 62, 66)
    Public ReadOnly CLR_BTN_BORDER As Color = Color.FromArgb(85, 85, 88)
    Public ReadOnly CLR_BTN_HOVER As Color = Color.FromArgb(75, 75, 80)
    Public ReadOnly CLR_TAB_INACTIVE As Color = Color.FromArgb(37, 37, 38)
    Public ReadOnly CLR_TAB_ACCENT As Color = Color.FromArgb(0, 122, 204)

    ' =========================================================================
    ' STARE
    ' =========================================================================
    Private _isDark As Boolean = False

    Public ReadOnly Property IsDark As Boolean
        Get
            Return _isDark
        End Get
    End Property

    ' =========================================================================
    ' API PUBLIC
    ' =========================================================================

    ''' <summary>Setează tema și o aplică imediat la toate formularele deschise.</summary>
    Public Sub SetTheme(dark As Boolean)
        _isDark = dark
        ' Snapshot — evită modificarea colecției în timpul iterației
        Dim openForms As New List(Of Form)
        For Each f As Form In Application.OpenForms
            openForms.Add(f)
        Next
        For Each f In openForms
            ApplyTheme(f)
            ' WorkflowEditorForm — re-aplică schema de culori + re-highlight sintaxă.
            ' Bloc dezactivat temporar: WorkflowEditorForm trăiește în KBot.Forexe.Editor,
            ' neimportat încă (vezi src/KBot.Forexe.Editor/README.md). De restaurat la importul Editor-ului.
            'If TypeOf f Is WorkflowEditorForm Then
            '    DirectCast(f, WorkflowEditorForm).ApplyEditorTheme()
            'End If
        Next
        ' Actualizează paleta de culori pentru logger
        RichTextBoxLogger.SetColorScheme(dark)
    End Sub

    ''' <summary>Aplică tema curentă la un singur formular / control.</summary>
    Public Sub ApplyTheme(ctrl As Control)
        ctrl.SuspendLayout()
        Try
            Traverse(ctrl)
        Finally
            ctrl.ResumeLayout(True)
        End Try

        ' Bara de titlu dark/light pe Windows 10 v2004+
        If TypeOf ctrl Is Form Then
            Try
                Dim dv As Integer = If(_isDark, 1, 0)
                DwmSetWindowAttribute(ctrl.Handle, 20, dv, 4)
            Catch
            End Try
        End If
    End Sub

    ' =========================================================================
    ' TRAVERSARE RECURSIVĂ
    ' =========================================================================
    Private Sub Traverse(ctrl As Control)
        StyleControl(ctrl)

        ' SplitContainer — tratare specială: Panel1/Panel2 sunt în Controls
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

        ' TabControl — tratare specială: TabPage-urile sunt în TabPages, NU Controls
        If TypeOf ctrl Is TabControl Then
            Dim tc = DirectCast(ctrl, TabControl)
            For Each tp As TabPage In tc.TabPages
                Traverse(tp)
            Next
            Return
        End If

        ' Recursie generică
        For Each child As Control In ctrl.Controls
            Traverse(child)
        Next
    End Sub

    ' =========================================================================
    ' STILIZARE PER CONTROL
    ' =========================================================================
    Private Sub StyleControl(ctrl As Control)
        If _isDark Then
            StyleDark(ctrl)
        Else
            StyleLight(ctrl)
        End If
    End Sub

    ' ─────────────────── DARK ────────────────────────────────────────────────
    Private Sub StyleDark(ctrl As Control)
        If TypeOf ctrl Is Form Then
            ctrl.BackColor = CLR_BG_PANEL

        ElseIf TypeOf ctrl Is SplitContainer Then
            ctrl.BackColor = CLR_BG_PANEL            ' bara separatoare
            DirectCast(ctrl, SplitContainer).Panel1.BackColor = CLR_BG_PANEL
            DirectCast(ctrl, SplitContainer).Panel2.BackColor = CLR_BG_PANEL

        ElseIf TypeOf ctrl Is TabControl Then
            Dim tc = DirectCast(ctrl, TabControl)
            tc.BackColor = CLR_BG_PANEL
            SetupTabOwnerDraw(tc, True)

        ElseIf TypeOf ctrl Is TabPage Then
            ctrl.BackColor = CLR_BG

        ElseIf TypeOf ctrl Is TableLayoutPanel Then
            ctrl.BackColor = CLR_BG_PANEL

        ElseIf TypeOf ctrl Is Panel Then
            ctrl.BackColor = CLR_BG_PANEL

        ElseIf TypeOf ctrl Is GroupBox Then
            ctrl.BackColor = CLR_BG_PANEL
            ctrl.ForeColor = CLR_FG

        ElseIf TypeOf ctrl Is Label Then
            ctrl.ForeColor = CLR_FG
            ctrl.BackColor = Color.Transparent

        ElseIf TypeOf ctrl Is CheckBox Then
            Dim chk = DirectCast(ctrl, CheckBox)
            If chk.Appearance = Appearance.Button Then
                ' Checkbox-button toggle — stilizat ca buton flat
                chk.FlatStyle = FlatStyle.Flat
                chk.BackColor = CLR_BTN
                chk.ForeColor = CLR_FG
                chk.FlatAppearance.BorderColor = CLR_BTN_BORDER
                chk.FlatAppearance.MouseOverBackColor = CLR_BTN_HOVER
                chk.FlatAppearance.CheckedBackColor = CLR_TAB_ACCENT
                chk.UseVisualStyleBackColor = False
            Else
                ctrl.ForeColor = CLR_FG
                ctrl.BackColor = Color.Transparent
            End If

        ElseIf TypeOf ctrl Is RadioButton Then
            ctrl.ForeColor = CLR_FG
            ctrl.BackColor = Color.Transparent

        ElseIf TypeOf ctrl Is Button Then
            Dim btn = DirectCast(ctrl, Button)
            If btn.Tag?.ToString() = "ThemeToggle" Then
                UpdateToggleButton(btn)
            ElseIf Not IsAccentButton(btn) Then
                btn.FlatStyle = FlatStyle.Flat
                btn.BackColor = CLR_BTN
                btn.ForeColor = CLR_FG
                btn.FlatAppearance.BorderColor = CLR_BTN_BORDER
                btn.FlatAppearance.MouseOverBackColor = CLR_BTN_HOVER
                btn.UseVisualStyleBackColor = False
            End If

        ElseIf TypeOf ctrl Is RichTextBox Then
            If ctrl.Tag?.ToString() = "SyntaxRTB" Then
                ' Editor cu sintaxă — culorile sunt gestionate de ApplyEditorTheme(), nu le atingem
                TrySetWindowTheme(ctrl, "DarkMode_Explorer")
            Else
                ctrl.BackColor = CLR_BG
                ctrl.ForeColor = CLR_FG
                TrySetWindowTheme(ctrl, "DarkMode_Explorer")
            End If

        ElseIf TypeOf ctrl Is CheckedListBox Then
            ctrl.BackColor = CLR_BG
            ctrl.ForeColor = CLR_FG
            TrySetWindowTheme(ctrl, "DarkMode_Explorer")

        ElseIf TypeOf ctrl Is ListBox Then
            ctrl.BackColor = CLR_BG
            ctrl.ForeColor = CLR_FG
            TrySetWindowTheme(ctrl, "DarkMode_Explorer")

        ElseIf TypeOf ctrl Is TreeView Then
            Dim tv = DirectCast(ctrl, TreeView)
            tv.BackColor = CLR_BG
            tv.ForeColor = CLR_FG
            TrySetWindowTheme(tv, "DarkMode_Explorer")

        ElseIf TypeOf ctrl Is ComboBox Then
            ctrl.BackColor = CLR_BG
            ctrl.ForeColor = CLR_FG
            TrySetWindowTheme(ctrl, "DarkMode_CFD")

        ElseIf TypeOf ctrl Is MaskedTextBox Then
            ctrl.BackColor = CLR_BG
            ctrl.ForeColor = CLR_FG

        ElseIf TypeOf ctrl Is TextBox Then
            ctrl.BackColor = CLR_BG
            ctrl.ForeColor = CLR_FG

        ElseIf TypeOf ctrl Is NumericUpDown Then
            ctrl.BackColor = CLR_BG
            ctrl.ForeColor = CLR_FG

        ElseIf TypeOf ctrl Is ProgressBar Then
            ' ProgressBar — lăsăm stilul system

        End If
    End Sub

    ' ─────────────────── LIGHT (default VB.NET) ──────────────────────────────
    Private Sub StyleLight(ctrl As Control)
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
                ' Checkbox-button toggle — revert la stilul sistem light
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
                btn.BackColor = SystemColors.Control
                btn.ForeColor = SystemColors.ControlText
                btn.UseVisualStyleBackColor = True
            End If

        ElseIf TypeOf ctrl Is RichTextBox Then
            If ctrl.Tag?.ToString() = "SyntaxRTB" Then
                ' Editor cu sintaxă — culorile sunt gestionate de ApplyEditorTheme(), nu le atingem
                TrySetWindowTheme(ctrl, "Explorer")
            Else
                ctrl.BackColor = Color.White
                ctrl.ForeColor = SystemColors.WindowText
                TrySetWindowTheme(ctrl, "Explorer")
            End If

        ElseIf TypeOf ctrl Is CheckedListBox Then
            ctrl.BackColor = SystemColors.Window
            ctrl.ForeColor = SystemColors.WindowText
            TrySetWindowTheme(ctrl, "Explorer")

        ElseIf TypeOf ctrl Is ListBox Then
            ctrl.BackColor = SystemColors.Window
            ctrl.ForeColor = SystemColors.WindowText
            TrySetWindowTheme(ctrl, "Explorer")

        ElseIf TypeOf ctrl Is TreeView Then
            Dim tv = DirectCast(ctrl, TreeView)
            tv.BackColor = SystemColors.Window
            tv.ForeColor = SystemColors.WindowText
            TrySetWindowTheme(tv, "Explorer")

        ElseIf TypeOf ctrl Is ComboBox Then
            ctrl.BackColor = SystemColors.Window
            ctrl.ForeColor = SystemColors.WindowText
            TrySetWindowTheme(ctrl, "Explorer")

        ElseIf TypeOf ctrl Is MaskedTextBox Then
            ctrl.BackColor = SystemColors.Window
            ctrl.ForeColor = SystemColors.WindowText

        ElseIf TypeOf ctrl Is TextBox Then
            ctrl.BackColor = SystemColors.Window
            ctrl.ForeColor = SystemColors.WindowText

        ElseIf TypeOf ctrl Is NumericUpDown Then
            ctrl.BackColor = SystemColors.Window
            ctrl.ForeColor = SystemColors.WindowText

        End If
    End Sub

    ' =========================================================================
    ' TABCONTROL OWNER DRAW (header-uri tab-uri colorate)
    ' =========================================================================
    Private Sub SetupTabOwnerDraw(tc As TabControl, dark As Boolean)
        RemoveHandler tc.DrawItem, AddressOf OnDrawTab
        If dark Then
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

        ' Fundal tab
        Dim bgColor = If(isSelected, CLR_BG, CLR_TAB_INACTIVE)
        Using bg As New SolidBrush(bgColor)
            e.Graphics.FillRectangle(bg, e.Bounds)
        End Using

        ' Linie accent albastru jos pentru tab activ
        If isSelected Then
            Using accent As New SolidBrush(CLR_TAB_ACCENT)
                e.Graphics.FillRectangle(accent,
                    New Rectangle(e.Bounds.X, e.Bounds.Bottom - 2, e.Bounds.Width, 2))
            End Using
        End If

        ' Text tab
        Using txt As New SolidBrush(CLR_FG)
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
    ' BUTON TOGGLE TEMĂ
    ' =========================================================================
    Private Sub UpdateToggleButton(btn As Button)
        If _isDark Then
            btn.Text = "☀️"
            btn.FlatStyle = FlatStyle.Flat
            btn.BackColor = CLR_BTN
            btn.ForeColor = CLR_FG
            btn.FlatAppearance.BorderColor = CLR_BTN_BORDER
            btn.FlatAppearance.MouseOverBackColor = CLR_BTN_HOVER
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
    ' HELPER: SetWindowTheme pentru scrollbar-uri native
    ' =========================================================================
    Private Sub TrySetWindowTheme(ctrl As Control, theme As String)
        Try
            SetWindowTheme(ctrl.Handle, theme, Nothing)
        Catch
        End Try
    End Sub

    ' =========================================================================
    ' HELPER: este buton cu culoare funcțională (verde/roșu/galben) ?
    ' Aceste butoane NU se schimbă cu tema.
    ' =========================================================================
    Private Function IsAccentButton(btn As Button) As Boolean
        If btn.UseVisualStyleBackColor Then Return False
        Dim c = btn.BackColor
        ' Culoare saturată (verde, roșu, galben/portocaliu) → este accent
        Return c.GetSaturation() > 0.25F AndAlso
               c <> CLR_BTN AndAlso
               c <> Color.Transparent AndAlso
               c.ToArgb() <> SystemColors.Control.ToArgb()
    End Function

End Module
