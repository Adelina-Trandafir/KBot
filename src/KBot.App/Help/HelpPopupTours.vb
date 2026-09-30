Option Strict On
Imports System.Runtime.InteropServices
Imports KBot.Controls

''' <summary>
''' Which guided tours the «?» popup offers (slice 0000-20): the tours of the application windows
''' on screen, grouped per window, top window first.
'''
''' <para>A window counts when it is visible, not minimized, and <c>Enabled</c> (a window behind
''' a modal dialog is disabled, so it cannot be used anyway); the help's own windows never count.
''' A tour belongs to a window when one of its screens (the tour's <c>screens:</c>, else its
''' topic's) is VISIBLE in it: the form itself, or a control or view inside it. For the main window
''' that is the selected view, since the others are hidden. A tour is offered once, on the top
''' window it belongs to. Only the parts this login may read.</para>
''' </summary>
Friend NotInheritable Class HelpPopupTours

    Private Sub New()
    End Sub

    ''' <summary>One window and its tours.</summary>
    Friend NotInheritable Class WindowTours
        Public Window As Form
        Public ReadOnly Tours As New List(Of HelpTour)()
    End Class

    Private Delegate Function EnumWindowsProc(hWnd As IntPtr, lParam As IntPtr) As Boolean

    <DllImport("user32.dll")>
    Private Shared Function EnumWindows(cb As EnumWindowsProc, lParam As IntPtr) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    ''' <summary>The windows with at least one tour, top first.</summary>
    Public Shared Function Collect(library As HelpLibrary, parts As IReadOnlyCollection(Of HelpPart)) As List(Of WindowTours)
        Dim result As New List(Of WindowTours)()
        Dim offered As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        Dim tours As List(Of HelpTour) = library.Tours.Where(Function(t) parts.Contains(t.Part)).ToList()
        If tours.Count = 0 Then Return result
        For Each f As Form In WindowsTopFirst()
            Dim group As New WindowTours With {.Window = f}
            For Each t As HelpTour In tours
                If offered.Contains(t.Id) Then Continue For
                If BelongsTo(library, t, f) Then
                    group.Tours.Add(t)
                    offered.Add(t.Id)
                End If
            Next
            If group.Tours.Count > 0 Then result.Add(group)
        Next
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

    ''' <summary>The application's usable windows, in z-order (top first).</summary>
    Private Shared Function WindowsTopFirst() As List(Of Form)
        Dim open As New Dictionary(Of IntPtr, Form)()
        For Each f As Form In Application.OpenForms.Cast(Of Form)().ToList()
            If f.IsHandleCreated AndAlso Usable(f) Then open(f.Handle) = f
        Next
        Dim ordered As New List(Of Form)()
        If open.Count = 0 Then Return ordered
        ' EnumWindows walks the top-level windows from the top of the z-order down.
        EnumWindows(Function(h, l)
                        Dim f As Form = Nothing
                        If open.TryGetValue(h, f) Then ordered.Add(f)
                        Return True
                    End Function, IntPtr.Zero)
        Return ordered
    End Function

    Private Shared Function Usable(f As Form) As Boolean
        If Not f.Visible OrElse Not f.Enabled OrElse f.WindowState = FormWindowState.Minimized Then Return False
        Return Not (TypeOf f Is HelpForm OrElse TypeOf f Is HelpTourBubble OrElse TypeOf f Is HelpTourFrame OrElse
                    TypeOf f Is KBotHelpPopup OrElse TypeOf f Is HelpCaptureForm OrElse TypeOf f Is HelpCapturePromptForm OrElse
                    TypeOf f Is HelpCaptureOverlay)
    End Function

End Class
