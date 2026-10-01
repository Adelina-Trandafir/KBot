Option Strict On
Imports KBot.Controls

''' <summary>
''' Which guided tours the «?» popup offers (slice 0000-20, narrowed in 0000-27): ONLY those of the
''' window whose «?» was pressed -- the form itself and the views / controls inside it. Any other
''' window (owned by it or not, visible or not) never counts: it has its own «?».
'''
''' <para>A tour belongs to the window when one of its screens (the tour's <c>screens:</c>, else its
''' topic's) is VISIBLE in it. For the main window that is the selected view, since the others are
''' hidden. Only the parts this login may read.</para>
''' </summary>
Friend NotInheritable Class HelpPopupTours

    Private Sub New()
    End Sub

    ''' <summary>One window and its tours.</summary>
    Friend NotInheritable Class WindowTours
        Public Window As Form
        Public ReadOnly Tours As New List(Of HelpTour)()
    End Class

    ''' <summary>
    ''' The tours of <paramref name="root"/> alone: a list of at most one window, empty when the
    ''' window is unusable or has no tour.
    ''' </summary>
    Public Shared Function Collect(library As HelpLibrary, parts As IReadOnlyCollection(Of HelpPart), root As Form) As List(Of WindowTours)
        Dim result As New List(Of WindowTours)()
        If root Is Nothing OrElse Not Usable(root) Then Return result
        Dim group As New WindowTours With {.Window = root}
        For Each t As HelpTour In library.Tours.Where(Function(x) parts.Contains(x.Part))
            If BelongsTo(library, t, root) Then group.Tours.Add(t)
        Next
        If group.Tours.Count > 0 Then result.Add(group)
        Return result
    End Function

    ' The screens a tour goes with: its own, else those of its topic.
    Private Shared Function BelongsTo(library As HelpLibrary, tour As HelpTour, window As Form) As Boolean
        Dim keys As IEnumerable(Of String) = tour.Screens
        If tour.Screens.Count = 0 Then
            Dim topic As HelpTopic = library.Find(tour.TopicId)
            If topic Is Nothing Then Return False
            keys = topic.Screens
        End If
        For Each k As String In keys
            If HelpTourRunner.FindInWindow(window, k) IsNot Nothing Then Return True
        Next
        Return False
    End Function

    Private Shared Function Usable(f As Form) As Boolean
        If Not f.Visible OrElse Not f.Enabled OrElse f.WindowState = FormWindowState.Minimized Then Return False
        Return Not (TypeOf f Is HelpForm OrElse TypeOf f Is HelpTourBubble OrElse TypeOf f Is HelpTourFrame OrElse
                    TypeOf f Is KBotHelpPopup OrElse TypeOf f Is HelpCaptureForm OrElse TypeOf f Is HelpCapturePromptForm OrElse
                    TypeOf f Is HelpCaptureOverlay)
    End Function

End Class
