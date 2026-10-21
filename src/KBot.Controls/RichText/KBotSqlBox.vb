Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Runtime.InteropServices
Imports System.Text.RegularExpressions
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' An editable text surface for SQL (slice ME01-05): keywords in the accent colour, strings in the success colour,
''' numbers in the warning colour, comments dim, quoted names (backticks) in the text colour but bold, and the placeholders
''' of the VERCON queries (<c>{{TABLE:...}}</c>, <c>%(name)s</c>) in the error colour so they stand out. Monospace, no word
''' wrap, a tab is kept as a tab. Colours come from the active scheme until the operator pins one (C4).
'''
''' <para>The colouring is applied through the selection, a moment (<see cref="HighlightDelayMs"/>) after the last
''' keystroke, with painting and the caret kept as they were; it never changes the text. A paste is always taken as
''' plain text, so what the SQL was copied from does not bring its own fonts and colours along.</para>
''' </summary>
<ToolboxItem(True)>
Public Class KBotSqlBox
    Inherits RichTextBox
    Implements IThemedControl

    ''' <summary>Past this length the colouring is skipped: it would make typing lag, and no query is this long.</summary>
    Private Const MaxHighlightLength As Integer = 200000

    ''' <summary>The wait after the last keystroke before the text is coloured, in ms.</summary>
    Public Const HighlightDelayMs As Integer = 180

    Private Const WM_SETREDRAW As Integer = &HB
    Private Const EM_GETSCROLLPOS As Integer = &H4DD
    Private Const EM_SETSCROLLPOS As Integer = &H4DE

    <DllImport("user32.dll", CharSet:=CharSet.Auto)>
    Private Shared Function SendMessage(k_handle As IntPtr, k_msg As Integer, k_wParam As Integer, ByRef k_lParam As Point) As Integer
    End Function

    <DllImport("user32.dll", CharSet:=CharSet.Auto)>
    Private Shared Function SendMessage(k_handle As IntPtr, k_msg As Integer, k_wParam As Integer, k_lParam As Integer) As Integer
    End Function

    Private Shared ReadOnly Keywords As String = "SELECT|FROM|WHERE|AND|OR|NOT|IN|IS|NULL|LIKE|BETWEEN|EXISTS|JOIN|INNER|LEFT|RIGHT|OUTER|CROSS|ON|USING|AS|" &
        "GROUP|BY|ORDER|HAVING|LIMIT|OFFSET|UNION|ALL|DISTINCT|CASE|WHEN|THEN|ELSE|END|ASC|DESC|WITH|" &
        "INSERT|INTO|VALUES|UPDATE|SET|DELETE|CREATE|ALTER|DROP|TABLE|VIEW|INDEX|TRUNCATE|" &
        "COUNT|SUM|AVG|MIN|MAX|COALESCE|IFNULL|NULLIF|IF|CAST|CONCAT|SUBSTRING|ROUND|DATE|YEAR|MONTH|TRUE|FALSE"

    ' One pass over the text; the first group that matches names the colour. Comments and strings come first so a keyword
    ' inside them is not coloured as a keyword.
    Private Shared ReadOnly Tokens As New Regex(
        "(?<c>--[^\n]*|/\*.*?(\*/|\z))|(?<s>'(?:[^'\\\n]|\\.|'')*'?|""(?:[^""\\\n]|\\.)*""?)|(?<q>`[^`\n]*`?)|" &
        "(?<p>\{\{[^}\n]*\}\}|%\([A-Za-z_][A-Za-z0-9_]*\)s)|(?<n>\b\d+(?:\.\d+)?\b)|(?<k>\b(?:" & Keywords & ")\b)",
        RegexOptions.IgnoreCase Or RegexOptions.Singleline Or RegexOptions.CultureInvariant Or RegexOptions.Compiled)

    Private WithEvents _timer As Timer
    Private _backPinned As Boolean
    Private _forePinned As Boolean
    Private _fontPinned As Boolean
    Private _applying As Boolean
    Private _highlighting As Boolean

    Public Sub New()
        MyBase.New()
        MyBase.DetectUrls = False
        MyBase.WordWrap = False
        MyBase.BorderStyle = BorderStyle.None
        MyBase.HideSelection = False
        MyBase.AcceptsTab = True
        MyBase.ScrollBars = RichTextBoxScrollBars.ForcedBoth
        MyBase.Font = New Font("Consolas", 9.75F)
        _timer = New Timer() With {.Interval = HighlightDelayMs}
        If Not KBotDesignTime.IsDesignTime(Me) Then ApplyColors()
    End Sub

    Protected Overrides Sub Dispose(k_disposing As Boolean)
        If k_disposing AndAlso _timer IsNot Nothing Then
            _timer.Dispose()
            _timer = Nothing
        End If
        MyBase.Dispose(k_disposing)
    End Sub

    ' ------------------------------------------------------------------
    ' Typing and pasting
    ' ------------------------------------------------------------------

    Protected Overrides Sub OnTextChanged(k_e As EventArgs)
        MyBase.OnTextChanged(k_e)
        If _highlighting OrElse KBotDesignTime.IsDesignTime(Me) OrElse _timer Is Nothing Then Return
        _timer.Stop()
        _timer.Start()
    End Sub

    Protected Overrides Sub OnKeyDown(k_e As KeyEventArgs)
        Try
            If k_e.Control AndAlso Not k_e.Alt AndAlso k_e.KeyCode = Keys.V AndAlso Not MyBase.ReadOnly Then
                ' Always plain text: pasting rich text would bring the fonts and colours of its source.
                If Clipboard.ContainsText() Then SelectedText = Clipboard.GetText()
                k_e.SuppressKeyPress = True
                k_e.Handled = True
                Return
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KBotSqlBox.OnKeyDown", ex)
        End Try
        MyBase.OnKeyDown(k_e)
    End Sub

    Private Sub Timer_Tick(k_sender As Object, k_e As EventArgs) Handles _timer.Tick
        Try
            _timer.Stop()
            Highlight()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotSqlBox.Timer_Tick", ex)
        End Try
    End Sub

    ' ------------------------------------------------------------------
    ' Colouring
    ' ------------------------------------------------------------------

    ''' <summary>Colours the whole text now (it also happens by itself a moment after every change).</summary>
    Public Sub Highlight()
        If _highlighting OrElse Not IsHandleCreated OrElse KBotDesignTime.IsDesignTime(Me) Then Return
        If TextLength = 0 OrElse TextLength > MaxHighlightLength Then Return
        _highlighting = True
        Dim k_selStart As Integer = SelectionStart
        Dim k_selLength As Integer = SelectionLength
        Dim k_scroll As Point
        SendMessage(Handle, EM_GETSCROLLPOS, 0, k_scroll)
        SendMessage(Handle, WM_SETREDRAW, 0, 0)
        Try
            Dim k_pal As ThemePalette = ThemeManager.Current.Palette
            Dim k_plain As Color = EffectiveFore()
            Dim k_regular As Font = Font
            Using k_bold As New Font(k_regular, FontStyle.Bold)
                Me.Select(0, TextLength)
                SelectionColor = k_plain
                SelectionFont = k_regular
                For Each k_match As Match In Tokens.Matches(Text)
                    Me.Select(k_match.Index, k_match.Length)
                    If k_match.Groups("c").Success Then
                        SelectionColor = k_pal.TextDimColor
                    ElseIf k_match.Groups("s").Success Then
                        SelectionColor = k_pal.SuccessColor
                    ElseIf k_match.Groups("q").Success Then
                        SelectionFont = k_bold
                    ElseIf k_match.Groups("p").Success Then
                        SelectionColor = k_pal.ErrorColor
                    ElseIf k_match.Groups("n").Success Then
                        SelectionColor = k_pal.WarningColor
                    Else
                        SelectionColor = k_pal.AccentColor
                        SelectionFont = k_bold
                    End If
                Next
                Me.Select(k_selStart, k_selLength)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("KBotSqlBox.Highlight", ex)
            Throw
        Finally
            SendMessage(Handle, EM_SETSCROLLPOS, 0, k_scroll)
            SendMessage(Handle, WM_SETREDRAW, 1, 0)
            Invalidate()
            _highlighting = False
        End Try
    End Sub

    ' ------------------------------------------------------------------
    ' Theme and designer serialization (C1, C4, C5)
    ' ------------------------------------------------------------------

    Private Function EffectiveFore() As Color
        Return If(_forePinned, MyBase.ForeColor, ThemeManager.Current.Palette.TextColor)
    End Function

    Private Sub ApplyColors()
        _applying = True
        Try
            Dim k_pal As ThemePalette = ThemeManager.Current.Palette
            If Not _backPinned Then MyBase.BackColor = k_pal.InputBackColor
            If Not _forePinned Then MyBase.ForeColor = k_pal.TextColor
        Finally
            _applying = False
        End Try
    End Sub

    Public Sub ApplyTheme(k_scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            ApplyColors()
            Highlight()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotSqlBox.ApplyTheme", ex)
        End Try
    End Sub

    Public Overrides Property BackColor As Color
        Get
            Return MyBase.BackColor
        End Get
        Set(k_value As Color)
            If Not _applying Then _backPinned = (k_value <> Color.Empty)
            MyBase.BackColor = k_value
        End Set
    End Property

    Public Function ShouldSerializeBackColor() As Boolean
        Return _backPinned
    End Function

    Public Overrides Sub ResetBackColor()
        _backPinned = False
        ApplyColors()
    End Sub

    Public Overrides Property ForeColor As Color
        Get
            Return MyBase.ForeColor
        End Get
        Set(k_value As Color)
            If Not _applying Then _forePinned = (k_value <> Color.Empty)
            MyBase.ForeColor = k_value
        End Set
    End Property

    Public Function ShouldSerializeForeColor() As Boolean
        Return _forePinned
    End Function

    Public Overrides Sub ResetForeColor()
        _forePinned = False
        ApplyColors()
    End Sub

    Public Overrides Property Font As Font
        Get
            Return MyBase.Font
        End Get
        Set(k_value As Font)
            _fontPinned = (k_value IsNot Nothing)
            MyBase.Font = If(k_value, New Font("Consolas", 9.75F))
        End Set
    End Property

    Public Function ShouldSerializeFont() As Boolean
        Return _fontPinned
    End Function

    Public Overrides Sub ResetFont()
        _fontPinned = False
        MyBase.Font = New Font("Consolas", 9.75F)
    End Sub

End Class
