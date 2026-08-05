Option Strict On
Imports System.Collections.Generic
Imports System.Drawing
Imports KBot.Controls

''' <summary>One window in the fake desktop.</summary>
Public NotInheritable Class FakeWindow

    Public Property Handle As IntPtr
    Public Property Pid As Integer
    ''' <summary>Must contain «Acrobat» to pass AdobeWindowHosting.IsAdobeWindowClass.</summary>
    Public Property ClassName As String = "AcrobatSDIWindow"
    Public Property Title As String = ""
    Public Property Visible As Boolean = True
    Public Property Alive As Boolean = True
    Public Property Parent As IntPtr = IntPtr.Zero
    Public Property Style As Long

    ''' <summary>Enumerations remaining before this window appears. 0 = visible to the search now.</summary>
    Public Property AppearsAfterSweeps As Integer = 0

    ''' <summary>False to model a window that ignores WM_CLOSE (mode B's fallback path).</summary>
    Public Property ClosesOnRequest As Boolean = True

End Class

''' <summary>
''' A desktop made of records. Everything <see cref="INativeWindows"/> can do is applied to the
''' list and APPENDED TO <see cref="Calls"/>, because several of the assertions in this suite are
''' negative: the whole point of slice 0024-03 is that teardown must NOT call SetParent back to the
''' original parent and must NOT restore the original style. Only a recorded call log can prove a
''' call did not happen.
''' </summary>
Public NotInheritable Class FakeNativeWindows
    Implements INativeWindows

    Public ReadOnly Windows As New List(Of FakeWindow)()
    Public ReadOnly Calls As New List(Of String)()

    Public Function Add(handle As Integer, pid As Integer,
                        Optional title As String = "",
                        Optional visible As Boolean = True,
                        Optional className As String = "AcrobatSDIWindow") As FakeWindow
        Dim w As New FakeWindow() With {
            .Handle = New IntPtr(handle), .Pid = pid, .Title = title,
            .Visible = visible, .ClassName = className}
        Windows.Add(w)
        Return w
    End Function

    Public Function Find(handle As IntPtr) As FakeWindow
        For Each w As FakeWindow In Windows
            If w.Handle = handle Then Return w
        Next
        Return Nothing
    End Function

    ''' <summary>True when any recorded call starts with <paramref name="prefix"/>.</summary>
    Public Function Called(prefix As String) As Boolean
        For Each c As String In Calls
            If c.StartsWith(prefix, StringComparison.Ordinal) Then Return True
        Next
        Return False
    End Function

    Public Function EnumTopLevelWindows() As IReadOnlyList(Of IntPtr) Implements INativeWindows.EnumTopLevelWindows
        Calls.Add("EnumTopLevelWindows")
        Dim visible As New List(Of IntPtr)()
        For Each w As FakeWindow In Windows
            If Not w.Alive Then Continue For
            If w.AppearsAfterSweeps > 0 Then
                ' Model a window that takes a few polls to turn up.
                w.AppearsAfterSweeps -= 1
                Continue For
            End If
            visible.Add(w.Handle)
        Next
        Return visible
    End Function

    Public Function OwnerPid(hwnd As IntPtr) As Integer Implements INativeWindows.OwnerPid
        Dim w As FakeWindow = Find(hwnd)
        Return If(w Is Nothing, 0, w.Pid)
    End Function

    Public Function GetClass(hwnd As IntPtr) As String Implements INativeWindows.GetClass
        Dim w As FakeWindow = Find(hwnd)
        Return If(w Is Nothing, "", w.ClassName)
    End Function

    Public Function GetTitle(hwnd As IntPtr) As String Implements INativeWindows.GetTitle
        Dim w As FakeWindow = Find(hwnd)
        Return If(w Is Nothing, "", w.Title)
    End Function

    Public Function IsWindow(hwnd As IntPtr) As Boolean Implements INativeWindows.IsWindow
        Dim w As FakeWindow = Find(hwnd)
        Return w IsNot Nothing AndAlso w.Alive
    End Function

    Public Function IsWindowVisible(hwnd As IntPtr) As Boolean Implements INativeWindows.IsWindowVisible
        Dim w As FakeWindow = Find(hwnd)
        Return w IsNot Nothing AndAlso w.Visible
    End Function

    Public Sub ShowWindow(hwnd As IntPtr, command As Integer) Implements INativeWindows.ShowWindow
        Calls.Add($"ShowWindow({hwnd},{command})")
        Dim w As FakeWindow = Find(hwnd)
        If w IsNot Nothing Then w.Visible = (command <> 0)
    End Sub

    Public Function GetParent(hwnd As IntPtr) As IntPtr Implements INativeWindows.GetParent
        Dim w As FakeWindow = Find(hwnd)
        Return If(w Is Nothing, IntPtr.Zero, w.Parent)
    End Function

    Public Function SetParent(child As IntPtr, newParent As IntPtr) As IntPtr Implements INativeWindows.SetParent
        Calls.Add($"SetParent({child},{newParent})")
        Dim w As FakeWindow = Find(child)
        Dim previous As IntPtr = If(w Is Nothing, IntPtr.Zero, w.Parent)
        If w IsNot Nothing Then w.Parent = newParent
        Return previous
    End Function

    Public Function GetWindowLongPtr(hwnd As IntPtr, index As Integer) As IntPtr Implements INativeWindows.GetWindowLongPtr
        Dim w As FakeWindow = Find(hwnd)
        Return If(w Is Nothing, IntPtr.Zero, New IntPtr(w.Style))
    End Function

    Public Function SetWindowLongPtr(hwnd As IntPtr, index As Integer, value As IntPtr) As IntPtr Implements INativeWindows.SetWindowLongPtr
        Calls.Add($"SetWindowLongPtr({hwnd},{index},{value.ToInt64()})")
        Dim w As FakeWindow = Find(hwnd)
        Dim previous As Long = If(w Is Nothing, 0L, w.Style)
        If w IsNot Nothing Then w.Style = value.ToInt64()
        Return New IntPtr(previous)
    End Function

    Public Sub MoveWindow(hwnd As IntPtr, x As Integer, y As Integer, w As Integer, h As Integer,
                          repaint As Boolean) Implements INativeWindows.MoveWindow
        Calls.Add($"MoveWindow({hwnd},{x},{y},{w},{h})")
    End Sub

    Public Sub SetWindowPos(hwnd As IntPtr, insertAfter As IntPtr, x As Integer, y As Integer,
                            cx As Integer, cy As Integer, flags As UInteger) Implements INativeWindows.SetWindowPos
        Calls.Add($"SetWindowPos({hwnd},{x},{y},{cx},{cy})")
    End Sub

    Public Function PostMessage(hwnd As IntPtr, msg As UInteger, wParam As IntPtr,
                                lParam As IntPtr) As Boolean Implements INativeWindows.PostMessage
        Calls.Add($"PostMessage({hwnd},{msg})")
        Dim w As FakeWindow = Find(hwnd)
        If w Is Nothing Then Return False
        ' WM_CLOSE on a well-behaved window destroys it; a stubborn one simply ignores it.
        If msg = &H10UI AndAlso w.ClosesOnRequest Then w.Alive = False
        Return True
    End Function

End Class

''' <summary>A launcher that starts nothing and remembers everything.</summary>
Public NotInheritable Class FakeAdobeLauncher
    Implements IAdobeLauncher

    ''' <summary>Nothing models «no Adobe installed on this machine».</summary>
    Public Property PathToReturn As String = "C:\Fake\Acrobat.exe"
    ''' <summary>PIDs handed out by successive Start calls.</summary>
    Public Property NextPid As Integer = 1000
    Public Property ThrowOnStart As Boolean = False

    Public ReadOnly Started As New List(Of String)()
    Public ReadOnly Killed As New List(Of Integer)()
    Public ReadOnly Exited As New HashSet(Of Integer)()

    Public Function ResolvePath() As String Implements IAdobeLauncher.ResolvePath
        Return PathToReturn
    End Function

    Public Function Start(exePath As String, arguments As String) As Integer Implements IAdobeLauncher.Start
        If ThrowOnStart Then Throw New InvalidOperationException("pornire eșuată (test)")
        Started.Add(exePath & " " & arguments)
        NextPid += 1
        Return NextPid
    End Function

    Public Function HasExited(pid As Integer) As Boolean Implements IAdobeLauncher.HasExited
        Return Exited.Contains(pid)
    End Function

    Public Sub Kill(pid As Integer) Implements IAdobeLauncher.Kill
        Killed.Add(pid)
        Exited.Add(pid)
    End Sub

End Class

''' <summary>A host panel that is just a handle and a size.</summary>
Public NotInheritable Class FakeHostSurface
    Implements IHostSurface

    Public Sub New(Optional handle As Integer = 9000, Optional width As Integer = 1000,
                   Optional height As Integer = 800)
        _handle = New IntPtr(handle)
        _size = New Size(width, height)
    End Sub

    Private ReadOnly _handle As IntPtr
    Private ReadOnly _size As Size
    Public Property Invalidations As Integer

    Public ReadOnly Property Handle As IntPtr Implements IHostSurface.Handle
        Get
            Return _handle
        End Get
    End Property

    Public ReadOnly Property ClientSize As Size Implements IHostSurface.ClientSize
        Get
            Return _size
        End Get
    End Property

    Public Sub Invalidate() Implements IHostSurface.Invalidate
        Invalidations += 1
    End Sub

End Class
