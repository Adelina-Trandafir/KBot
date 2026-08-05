Option Strict On
Imports System.Drawing
Imports System.Windows.Forms

''' <summary>
''' The three things <see cref="AdobeReaderHost"/> needs from the panel it embeds into: a window
''' handle to reparent onto, a client size to compute geometry against, and a way to ask for a
''' repaint.
'''
''' Behind an interface so the orchestration is testable without creating a real control and forcing
''' its handle — which needs an STA thread and a message pump, neither of which belongs in a test
''' about which Win32 calls happen in which order (slice 0024-03).
''' </summary>
Public Interface IHostSurface
    ReadOnly Property Handle As IntPtr
    ReadOnly Property ClientSize As Size
    Sub Invalidate()
End Interface

''' <summary>The real surface: a WinForms <see cref="Control"/>.</summary>
Public NotInheritable Class ControlHostSurface
    Implements IHostSurface

    Private ReadOnly _control As Control

    Public Sub New(control As Control)
        If control Is Nothing Then Throw New ArgumentNullException(NameOf(control))
        _control = control
    End Sub

    ''' <summary>The control behind this surface, for callers that still need it (resize events).</summary>
    Public ReadOnly Property Control As Control
        Get
            Return _control
        End Get
    End Property

    Public ReadOnly Property Handle As IntPtr Implements IHostSurface.Handle
        Get
            Return _control.Handle
        End Get
    End Property

    Public ReadOnly Property ClientSize As Size Implements IHostSurface.ClientSize
        Get
            Return _control.ClientSize
        End Get
    End Property

    Public Sub Invalidate() Implements IHostSurface.Invalidate
        _control.Invalidate(True)
    End Sub

End Class
