Option Strict On
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' The one answer to "which monitor does the application live on?" (slice 0062).
'''
''' <para><b>Why it exists.</b> <c>Form.CenterToScreen</c> -- the path behind
''' <c>FormStartPosition.CenterScreen</c>, and the fallback of <c>CenterParent</c> when a form has
''' no owner -- centres on <c>Screen.FromPoint(Control.MousePosition)</c>. That is why a dialog
''' opened from a window on the left monitor appears on the right one when the mouse happens to
''' rest there. Every themed form asks here instead, and the answer is anchored to the window the
''' application registered as its main one.</para>
'''
''' <para><b>The chain</b>, in <see cref="Reference"/>: (1) the registered window, if alive and
''' with a handle; (2) the first form in <c>Application.OpenForms</c> with a created handle,
''' skipping the form being placed (the very first window would otherwise centre on itself, i.e.
''' on the mouse monitor); (3) <c>Screen.PrimaryScreen</c>. Never the mouse.</para>
'''
''' <para>The registered window is held through a <c>WeakReference</c>: a module-level strong
''' reference to a form keeps its whole control tree alive after it closes -- the same reason
''' <c>AppScaling.Broadcast</c> walks <c>OpenForms</c> instead of holding targets.</para>
'''
''' <para>Shared state, touched only from the UI thread.</para>
''' </summary>
Public NotInheritable Class AppScreen

    Private Sub New()
    End Sub

    Private Shared _reference As WeakReference(Of Form)

    ''' <summary>Registers the application's main window. A later call replaces the earlier one.</summary>
    Public Shared Sub SetReference(main As Form)
        If main Is Nothing Then Throw New ArgumentNullException(NameOf(main))
        _reference = New WeakReference(Of Form)(main)
    End Sub

    ''' <summary>
    ''' Forgets the registered window -- only if it is still <paramref name="main"/>. A form that
    ''' was replaced as reference must not clear its successor on its way out.
    ''' </summary>
    Public Shared Sub ClearReference(main As Form)
        If main Is Nothing OrElse _reference Is Nothing Then Return
        Dim current As Form = Nothing
        If _reference.TryGetTarget(current) AndAlso current Is main Then _reference = Nothing
    End Sub

    ''' <summary>The registered window, or Nothing when none is alive (test seam).</summary>
    Public Shared Function ReferenceForm() As Form
        If _reference Is Nothing Then Return Nothing
        Dim f As Form = Nothing
        If _reference.TryGetTarget(f) AndAlso f IsNot Nothing AndAlso Not f.IsDisposed Then Return f
        Return Nothing
    End Function

    ''' <summary>
    ''' The screen the application lives on. <paramref name="exclude"/> is the form being placed:
    ''' it is never its own reference. Logs and falls back to the primary screen on any failure --
    ''' a broken diagnostic is not allowed to stop a window from being placed.
    ''' </summary>
    Public Shared Function Reference(Optional exclude As Form = Nothing) As Screen
        Try
            Dim main As Form = ReferenceForm()
            If main IsNot Nothing AndAlso main IsNot exclude AndAlso main.IsHandleCreated Then
                Return Screen.FromHandle(main.Handle)
            End If

            For Each f As Form In Application.OpenForms
                If f Is Nothing OrElse f Is exclude OrElse f.IsDisposed OrElse Not f.IsHandleCreated Then Continue For
                Return Screen.FromHandle(f.Handle)
            Next

            Return Screen.PrimaryScreen
        Catch ex As Exception
            GlobalErrorLog.Write("AppScreen.Reference", ex)
            Return Screen.PrimaryScreen
        End Try
    End Function

    ''' <summary>
    ''' Centres <paramref name="target"/> on the reference screen's working area. Does nothing on
    ''' a window that is not <c>Normal</c> -- documented, not silent: a maximised window has
    ''' nothing to centre. Window I/O: logs and rethrows.
    ''' </summary>
    Public Shared Sub Center(target As Form)
        If target Is Nothing Then Throw New ArgumentNullException(NameOf(target))
        Try
            If target.WindowState <> FormWindowState.Normal Then Return
            Dim area As Rectangle = Reference(target).WorkingArea
            target.Location = CenteredIn(area, target.Size)
        Catch ex As Exception
            GlobalErrorLog.Write("AppScreen.Center", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Keeps a window on the screen it is currently on, after its size changed: the top-left
    ''' corner stays where it was unless the window now runs off the working area, in which case
    ''' it is pushed back -- never further than the top-left of the area.
    ''' </summary>
    Public Shared Sub KeepOnScreen(target As Form)
        If target Is Nothing Then Throw New ArgumentNullException(NameOf(target))
        Try
            If target.WindowState <> FormWindowState.Normal Then Return
            Dim area As Rectangle = If(target.IsHandleCreated,
                                       Screen.FromHandle(target.Handle).WorkingArea,
                                       Reference(target).WorkingArea)
            Dim wanted As Point = ClampedIn(area, target.Bounds)
            If wanted <> target.Location Then target.Location = wanted
        Catch ex As Exception
            GlobalErrorLog.Write("AppScreen.KeepOnScreen", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Pure: the top-left that centres <paramref name="size"/> in <paramref name="area"/>, then
    ''' clamped so the top-left corner stays inside the area. A window larger than the area
    ''' sacrifices its bottom-right: the title bar and the first controls are top-left, and the
    ''' opposite clamp would leave a window with no title to drag.
    ''' </summary>
    Public Shared Function CenteredIn(area As Rectangle, size As Size) As Point
        Dim x As Integer = area.Left + (area.Width - size.Width) \ 2
        Dim y As Integer = area.Top + (area.Height - size.Height) \ 2
        Return ClampedIn(area, New Rectangle(New Point(x, y), size))
    End Function

    ''' <summary>
    ''' Pure: the top-left of <paramref name="bounds"/> moved the least distance that keeps the
    ''' window inside <paramref name="area"/>; when it cannot fit, the top-left corner wins.
    ''' </summary>
    Public Shared Function ClampedIn(area As Rectangle, bounds As Rectangle) As Point
        Dim x As Integer = Math.Min(bounds.Left, area.Right - bounds.Width)
        Dim y As Integer = Math.Min(bounds.Top, area.Bottom - bounds.Height)
        x = Math.Max(area.Left, x)
        y = Math.Max(area.Top, y)
        Return New Point(x, y)
    End Function

End Class
