Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' The HELP button of the caption bar (slice 0000-01): a «?» glyph right next to the control box
''' (left of minimize / maximize, or of close when those are hidden), which opens the help for
''' the window through <see cref="KBotHelp"/>.
'''
''' <para><b>On by default.</b> Every K-BOT window gets help without touching its designer file;
''' a window that should not have it turns <see cref="ShowHelpButton"/> off. The button is drawn
''' only once the application has installed its help (<see cref="KBotHelp.IsAvailable"/>), so the
''' DevHarness bench and the VS designer never show a button that does nothing.</para>
''' </summary>
Partial Public NotInheritable Class KBotCaptionBar

    Private _showHelpButton As Boolean = True
    Private _helpButtonHover As Boolean = False

    ''' <summary>Shows the «?» help button when the application has help installed.</summary>
    <Category("K-BOT")>
    <Description("Arată butonul de ajutor «?» lângă cutia de control, când aplicația are ajutorul instalat. Implicit True.")>
    <DefaultValue(True)>
    Public Property ShowHelpButton As Boolean
        Get
            Return _showHelpButton
        End Get
        Set(value As Boolean)
            If value = _showHelpButton Then Return
            _showHelpButton = value
            Invalidate()
        End Set
    End Property

    ''' <summary>The help button's rectangle in client coordinates; Empty when it is not shown.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property HelpButtonBounds As Rectangle
        Get
            If Not HelpButtonVisible() Then Return Rectangle.Empty
            Return HelpButtonRect()
        End Get
    End Property

    ' Drawn only when the property is on AND there is help to open.
    Private Function HelpButtonVisible() As Boolean
        Return _showHelpButton AndAlso KBotHelp.IsAvailable
    End Function

    ' First free slot after the control box (close, maximize, minimize).
    Private Function ControlBoxSlots() As Integer
        Dim slotIndex As Integer = 1 ' close is always slot 0
        If _showMinimize Then slotIndex += 1
        If _showMaximize Then slotIndex += 1
        Return slotIndex
    End Function

    Private Function HelpButtonRect() As Rectangle
        Return SlotRect(ControlBoxSlots())
    End Function

    ''' <summary>
    ''' A «?» in the glyph colour, sized like the other glyphs. Called only from OnPaint, which is
    ''' already wrapped (transitive coverage).
    ''' </summary>
    Private Sub DrawHelpButton(g As Graphics)
        Dim r As Rectangle = HelpButtonRect()
        If _helpButtonHover Then
            Using hb As New SolidBrush(_optBtnHoverColor)
                g.FillRectangle(hb, r)
            End Using
        End If
        Dim px As Single = Math.Max(8.0F, Math.Min(Height - ThemeShapes.ScaleDpi(Me, 14), ThemeShapes.ScaleDpi(Me, 22)) * 0.72F)
        Using f As New Font(Font.FontFamily, px, FontStyle.Bold, GraphicsUnit.Pixel)
            TextRenderer.DrawText(g, "?", f, r, _glyphColor,
                                  TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or TextFormatFlags.NoPadding)
        End Using
    End Sub

    ''' <summary>The «?» click: help for the window. The provider picks the topic from the focused control.</summary>
    Private Sub HelpButtonClicked()
        If Not KBotHelp.IsAvailable Then Return
        KBotHelp.Request(Me)
    End Sub

End Class
