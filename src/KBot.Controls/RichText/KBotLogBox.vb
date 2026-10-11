Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Globalization
Imports System.Text
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>One line of a log, as <see cref="KBotLogBox"/> shows it (slice ME01).</summary>
Public NotInheritable Class KBotLogLine

    ''' <summary>The time, already formatted by the caller (it is shown as is).</summary>
    Public Property Time As String = String.Empty

    ''' <summary>The severity (KBot.Common.KBotLogLevel; Unknown and Trace are shown as Debug).</summary>
    Public Property Level As KBotLogLevel = KBotLogLevel.Info

    ''' <summary>Where it came from (file, user, database...). Shown dim, after the level. May be empty.</summary>
    Public Property Source As String = String.Empty

    ''' <summary>The message (first line).</summary>
    Public Property Text As String = String.Empty

    ''' <summary>More lines (a traceback...). Shown indented under the message. May be empty.</summary>
    Public Property Detail As String = String.Empty

End Class

''' <summary>
''' A read-only rich text surface for logs (slice ME01): per line the TIME in the accent colour, the
''' level as a short tag, the source dim, then the message; an error line is painted in the theme's
''' error colour (a dark red in the Classic scheme), a warning in the warning colour, and a detail
''' (traceback) hangs under its line in the dim colour.
'''
''' <para>The text is built as ONE RTF document and assigned at once: appending thousands of
''' coloured runs one by one through the selection is orders of magnitude slower.
''' <see cref="SetLines"/> replaces the content and, when the view was at the bottom (or
''' <paramref name="stickToBottom"/> asks), keeps it at the bottom as a tail view should.</para>
''' </summary>
<ToolboxItem(True)>
Public Class KBotLogBox
    Inherits RichTextBox
    Implements IThemedControl

    Private ReadOnly _lines As New List(Of KBotLogLine)()
    Private _backPinned As Boolean
    Private _forePinned As Boolean
    Private _fontPinned As Boolean
    Private _applying As Boolean

    Public Sub New()
        MyBase.New()
        MyBase.ReadOnly = True
        MyBase.DetectUrls = False
        MyBase.WordWrap = False
        MyBase.BorderStyle = BorderStyle.None
        MyBase.HideSelection = False
        MyBase.ScrollBars = RichTextBoxScrollBars.ForcedBoth
        MyBase.Font = New Font("Consolas", 9.75F)
        If Not KBotDesignTime.IsDesignTime(Me) Then ApplyColors()
    End Sub

    ''' <summary>
    ''' The wheel goes to the control that has the focus, and a read-only log is never clicked into
    ''' by the operator who only wants to scroll it: take the focus when the mouse comes over it.
    ''' </summary>
    Protected Overrides Sub OnMouseEnter(e As EventArgs)
        MyBase.OnMouseEnter(e)
        Try
            If Not Focused AndAlso Not KBotDesignTime.IsDesignTime(Me) AndAlso FindForm() IsNot Nothing AndAlso
               FindForm().ContainsFocus Then Focus()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotLogBox.OnMouseEnter", ex)
        End Try
    End Sub

    ' ------------------------------------------------------------------
    ' Content
    ' ------------------------------------------------------------------

    ''' <summary>The lines now shown, oldest first.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property LogLines As IReadOnlyList(Of KBotLogLine)
        Get
            Return _lines
        End Get
    End Property

    ''' <summary>Replaces the content. <paramref name="k_stickToBottom"/>: scroll to the end afterwards.</summary>
    Public Sub SetLines(k_lines As IEnumerable(Of KBotLogLine), Optional k_stickToBottom As Boolean = True)
        If k_lines Is Nothing Then Throw New ArgumentNullException(NameOf(k_lines))
        Try
            _lines.Clear()
            _lines.AddRange(k_lines)
            Render(k_stickToBottom)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotLogBox.SetLines", ex)
            Throw
        End Try
    End Sub

    ''' <summary>Empties the box.</summary>
    Public Sub ClearLines()
        _lines.Clear()
        Render(False)
    End Sub

    ''' <summary>True when the view is scrolled to the end (or the box is empty).</summary>
    Public Function IsAtBottom() As Boolean
        If Not IsHandleCreated OrElse TextLength = 0 Then Return True
        Dim k_last As Point = GetPositionFromCharIndex(Math.Max(0, TextLength - 1))
        Return k_last.Y <= ClientSize.Height + 2
    End Function

    ''' <summary>The plain text of the lines (for "copy" and "export").</summary>
    Public Function ToPlainText() As String
        Dim k_sb As New StringBuilder()
        For Each k_line As KBotLogLine In _lines
            k_sb.Append(k_line.Time).Append("  ").Append(LevelTag(k_line.Level)).Append("  ")
            If k_line.Source <> String.Empty Then k_sb.Append(k_line.Source).Append("  ")
            k_sb.AppendLine(k_line.Text)
            If k_line.Detail <> String.Empty Then
                For Each k_row As String In k_line.Detail.Replace(vbCr, String.Empty).Split(ControlChars.Lf)
                    k_sb.Append("      ").AppendLine(k_row)
                Next
            End If
        Next
        Return k_sb.ToString()
    End Function

    Private Sub Render(k_stickToBottom As Boolean)
        If KBotDesignTime.IsDesignTime(Me) Then Return
        Dim k_stick As Boolean = k_stickToBottom OrElse IsAtBottom()
        SuspendLayout()
        Try
            Rtf = BuildRtf()
            If k_stick AndAlso IsHandleCreated Then
                SelectionStart = TextLength
                SelectionLength = 0
                ScrollToCaret()
            End If
        Finally
            ResumeLayout()
        End Try
    End Sub

    ' ------------------------------------------------------------------
    ' RTF
    ' ------------------------------------------------------------------

    ' Colour table slots (1-based in RTF; slot 0 is the default).
    Private Const CBody As Integer = 1
    Private Const CTime As Integer = 2
    Private Const CDim As Integer = 3
    Private Const CError As Integer = 4
    Private Const CWarn As Integer = 5
    Private Const CInfo As Integer = 6

    Private Function BuildRtf() As String
        Dim k_pal As ThemePalette = ThemeManager.Current.Palette
        Dim k_points As Integer = CInt(Math.Round(Font.SizeInPoints * 2.0F))
        Dim k_sb As New StringBuilder(_lines.Count * 120 + 512)
        k_sb.Append("{\rtf1\ansi\deff0{\fonttbl{\f0\fmodern ").Append(Font.Name).Append(";}}")
        k_sb.Append("{\colortbl ;")
        AppendColor(k_sb, EffectiveFore())
        AppendColor(k_sb, k_pal.AccentColor)
        AppendColor(k_sb, k_pal.TextDimColor)
        AppendColor(k_sb, k_pal.ErrorColor)
        AppendColor(k_sb, k_pal.WarningColor)
        AppendColor(k_sb, k_pal.TextDimColor)
        k_sb.Append("}\f0\fs").Append(k_points.ToString(CultureInfo.InvariantCulture)).Append(" ")
        For Each k_line As KBotLogLine In _lines
            AppendLine(k_sb, k_line)
        Next
        k_sb.Append("}")
        Return k_sb.ToString()
    End Function

    Private Shared Sub AppendLine(k_sb As StringBuilder, k_line As KBotLogLine)
        Dim k_textSlot As Integer = CBody
        Dim k_tagSlot As Integer = CInfo
        Select Case k_line.Level
            Case KBotLogLevel.Error
                k_textSlot = CError
                k_tagSlot = CError
            Case KBotLogLevel.Warn
                k_textSlot = CWarn
                k_tagSlot = CWarn
            Case KBotLogLevel.Debug, KBotLogLevel.Trace, KBotLogLevel.Unknown
                k_textSlot = CDim
                k_tagSlot = CDim
        End Select
        k_sb.Append("\cf").Append(CTime).Append(" ").Append(Escape(k_line.Time)).Append("  ")
        k_sb.Append("\b\cf").Append(k_tagSlot).Append(" ").Append(LevelTag(k_line.Level)).Append("\b0  ")
        If k_line.Source <> String.Empty Then
            k_sb.Append("\cf").Append(CDim).Append(" ").Append(Escape(k_line.Source)).Append("  ")
        End If
        k_sb.Append("\cf").Append(k_textSlot).Append(" ").Append(Escape(k_line.Text)).Append("\par ")
        If k_line.Detail <> String.Empty Then
            k_sb.Append("\cf").Append(CDim).Append(" ")
            For Each k_row As String In k_line.Detail.Replace(vbCr, String.Empty).Split(ControlChars.Lf)
                k_sb.Append("      ").Append(Escape(k_row)).Append("\par ")
            Next
        End If
    End Sub

    Private Shared Function LevelTag(k_level As KBotLogLevel) As String
        Select Case k_level
            Case KBotLogLevel.Error : Return "ERR "
            Case KBotLogLevel.Warn : Return "WARN"
            Case KBotLogLevel.Debug, KBotLogLevel.Trace, KBotLogLevel.Unknown : Return "DBG "
            Case Else : Return "INFO"
        End Select
    End Function

    Private Shared Sub AppendColor(k_sb As StringBuilder, k_color As Color)
        k_sb.Append("\red").Append(k_color.R).Append("\green").Append(k_color.G).Append("\blue").Append(k_color.B).Append(";")
    End Sub

    ' RTF escaping: the control characters, and everything above ASCII as a signed 16-bit \u.
    Private Shared Function Escape(k_text As String) As String
        If String.IsNullOrEmpty(k_text) Then Return String.Empty
        Dim k_sb As New StringBuilder(k_text.Length + 8)
        For Each k_ch As Char In k_text
            Select Case k_ch
                Case "\"c : k_sb.Append("\\")
                Case "{"c : k_sb.Append("\{")
                Case "}"c : k_sb.Append("\}")
                Case ControlChars.Tab : k_sb.Append("\tab ")
                Case Else
                    If AscW(k_ch) < 32 Then
                        ' other control characters have no place in a log view
                    ElseIf AscW(k_ch) > 126 Then
                        k_sb.Append("\u").Append(CShort(AscW(k_ch)).ToString(CultureInfo.InvariantCulture)).Append("?")
                    Else
                        k_sb.Append(k_ch)
                    End If
            End Select
        Next
        Return k_sb.ToString()
    End Function

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

    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            ApplyColors()
            If _lines.Count > 0 Then Render(IsAtBottom())
        Catch ex As Exception
            GlobalErrorLog.Write("KBotLogBox.ApplyTheme", ex)
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
