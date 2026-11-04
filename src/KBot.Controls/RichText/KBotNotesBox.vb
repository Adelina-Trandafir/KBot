Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' A read-only rich text surface for a short plain text (release notes, a description): word-wrapped, proportional font,
''' the scroll bars are our own <see cref="KBotScrollBar"/>s (through <see cref="KBotRichScrollBox"/>), colours come from the
''' active scheme until the operator pins one. It is the text sibling of <see cref="KBotLogBox"/> (which is for log lines).
''' </summary>
<ToolboxItem(True)>
Public Class KBotNotesBox
    Inherits KBotRichScrollBox
    Implements IThemedControl

    Private _backPinned As Boolean
    Private _forePinned As Boolean
    Private _fontPinned As Boolean
    Private _applying As Boolean

    Public Sub New()
        MyBase.New()
        MyBase.ReadOnly = True
        MyBase.DetectUrls = False
        MyBase.WordWrap = True
        MyBase.BorderStyle = BorderStyle.None
        MyBase.HideSelection = False
        MyBase.ScrollBars = RichTextBoxScrollBars.Vertical
        MyBase.Font = New Font("Segoe UI", 9.75F)
        If Not KBotDesignTime.IsDesignTime(Me) Then ApplyColors()
    End Sub

    ''' <summary>The wheel goes to the focused control, and a read-only box is never clicked into: take the focus when the mouse comes over it.</summary>
    Protected Overrides Sub OnMouseEnter(e As EventArgs)
        MyBase.OnMouseEnter(e)
        Try
            If Not Focused AndAlso Not KBotDesignTime.IsDesignTime(Me) AndAlso FindForm() IsNot Nothing AndAlso
               FindForm().ContainsFocus Then Focus()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotNotesBox.OnMouseEnter", ex)
        End Try
    End Sub

    ''' <summary>Replaces the text (plain; line breaks kept) and scrolls to the top.</summary>
    Public Sub ShowText(k_text As String)
        Try
            Text = If(k_text, String.Empty)
            SelectionStart = 0
            SelectionLength = 0
            ScrollToCaret()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotNotesBox.ShowText", ex)
            Throw
        End Try
    End Sub

    ' ------------------------------------------------------------------
    ' Theme and designer serialization (C1, C4, C5)
    ' ------------------------------------------------------------------

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
            ApplyBarTheme(scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotNotesBox.ApplyTheme", ex)
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
            MyBase.Font = If(k_value, New Font("Segoe UI", 9.75F))
        End Set
    End Property

    Public Function ShouldSerializeFont() As Boolean
        Return _fontPinned
    End Function

    Public Overrides Sub ResetFont()
        _fontPinned = False
        MyBase.Font = New Font("Segoe UI", 9.75F)
    End Sub

End Class
