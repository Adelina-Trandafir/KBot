Option Strict On
Imports System.Windows.Forms

''' <summary>
''' The application's help system, as seen from the controls (slice 0000-01). Implemented in
''' KBot.App (<c>HelpService</c>), which knows the topics, the login role and the help window.
''' </summary>
Public Interface IKBotHelpProvider

    ''' <summary>
    ''' Opens the help for <paramref name="origin"/>: the focused control on F1, the caption bar
    ''' on its «?» button. The provider walks up from it to the nearest control that has a topic.
    ''' </summary>
    Sub ShowHelp(origin As Control)

    ''' <summary>
    ''' Slice 0000-20: the «?» button of a caption bar. Opens the help popup (search, the topic of
    ''' the screen, the guided tours) under <paramref name="anchorScreenRect"/>, the button's
    ''' rectangle in screen coordinates. F1 keeps going to <see cref="ShowHelp"/>.
    ''' </summary>
    Sub ShowHelpMenu(origin As Control, anchorScreenRect As Drawing.Rectangle)

End Interface

''' <summary>
''' Slice 0000-23: a control that paints its own buttons (tree, grid, caption bar, nav list) tells
''' a guided tour where each of them is, so the tour bubble can point at one button instead of the
''' whole control. Part names are fixed ASCII keys per control family (listed on each
''' implementation and in <c>HelpContent\README.md</c>).
''' </summary>
Public Interface IKBotHelpParts

    ''' <summary>
    ''' Where <paramref name="part"/> is now, in client coordinates. <c>Rectangle.Empty</c> = the
    ''' part exists for this kind of control but is not on screen now (no icon set, band hidden,
    ''' scrolled away). An unknown part name throws <see cref="ArgumentException"/>.
    ''' </summary>
    Function HelpPartBounds(part As String) As Drawing.Rectangle

    ''' <summary>
    ''' Shows a part that normally appears only under the mouse (<paramref name="show"/> = True),
    ''' so the tour can point at it, and puts it back (False). Parts that are always drawn ignore
    ''' it. An unknown part name throws <see cref="ArgumentException"/>.
    ''' </summary>
    Sub SetHelpPartDemo(part As String, show As Boolean)

End Interface

''' <summary>
''' Slice 0000-23: a control that paints text itself (so the help capture cannot read it from a
''' <c>Text</c> property) reports where the text it shows is sensitive -- the capture blurs those
''' places before the picture is saved.
''' </summary>
Public Interface IKBotCaptureRedaction

    ''' <summary>
    ''' The client rectangles of the TEXT (never a whole bar, row or menu) this control shows now for
    ''' which <paramref name="isSensitive"/> answers True, plus anything that is sensitive by its
    ''' nature (the unit selector of the caption bar). Only what is visible now.
    ''' <paramref name="isSensitive"/> takes (context, text): the context is what the text is -- a
    ''' column's key and header, empty when the control has no such notion -- so a plain number can
    ''' be sensitive in a «Cod fiscal» column and harmless anywhere else.
    ''' </summary>
    Function SensitiveRegions(isSensitive As Func(Of String, String, Boolean)) As IEnumerable(Of Drawing.Rectangle)

End Interface

''' <summary>
''' The one place a control asks for help (slice 0000-01). Lives in Theming, next to
''' <see cref="ThemeManager"/>, because the caption bar (KBot.Controls) cannot reference KBot.App,
''' where the topics and the help window are. The application sets <see cref="Provider"/> once at
''' startup; until then (DevHarness, designer) there is no help and the «?» button stays hidden.
''' </summary>
Public NotInheritable Class KBotHelp

    Private Sub New()
    End Sub

    ''' <summary>The application's help. Nothing = no help installed.</summary>
    Public Shared Property Provider As IKBotHelpProvider

    ''' <summary>True once the application has installed its help.</summary>
    Public Shared ReadOnly Property IsAvailable As Boolean
        Get
            Return Provider IsNot Nothing
        End Get
    End Property

    ''' <summary>
    ''' Opens the help for <paramref name="origin"/>. Callers check <see cref="IsAvailable"/>
    ''' first (the «?» button is not even drawn without it), so a call with no provider is a bug.
    ''' </summary>
    Public Shared Sub Request(origin As Control)
        Dim p As IKBotHelpProvider = Provider
        If p Is Nothing Then Throw New InvalidOperationException("KBotHelp.Request called with no help provider installed.")
        p.ShowHelp(origin)
    End Sub

    ''' <summary>
    ''' Slice 0000-20: the help popup for <paramref name="origin"/>, placed under
    ''' <paramref name="anchorScreenRect"/>. Same contract as <see cref="Request"/>: callers check
    ''' <see cref="IsAvailable"/> first.
    ''' </summary>
    Public Shared Sub RequestMenu(origin As Control, anchorScreenRect As Drawing.Rectangle)
        Dim p As IKBotHelpProvider = Provider
        If p Is Nothing Then Throw New InvalidOperationException("KBotHelp.RequestMenu called with no help provider installed.")
        p.ShowHelpMenu(origin, anchorScreenRect)
    End Sub

End Class
