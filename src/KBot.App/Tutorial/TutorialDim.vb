Option Strict On
Imports System.Drawing
Imports KBot.Common

''' <summary>
''' The dark veil of an interactive tutorial (slice 000T): a borderless window over one host window,
''' semi-transparent black, shaped with holes (<see cref="Region"/>) where the user may act. A click
''' on the veil is not an action the tutorial expects: it raises <see cref="Clicked"/>. A click in a
''' hole reaches the control below. It never takes the focus. Created AFTER the host window is shown
''' and owned by it, so a modal host does not disable it.
''' </summary>
Friend NotInheritable Class TutorialDim
    Inherits Form

    Private Const WS_EX_TOOLWINDOW As Integer = &H80
    Private Const WS_EX_NOACTIVATE As Integer = &H8000000

    ''' <summary>The user pressed the mouse on the veil (outside every hole).</summary>
    Public Event Clicked()

    Public Sub New()
        FormBorderStyle = FormBorderStyle.None
        StartPosition = FormStartPosition.Manual
        ShowInTaskbar = False
        AutoScaleMode = AutoScaleMode.None
        BackColor = Color.Black
        Opacity = 0.45
    End Sub

    Protected Overrides ReadOnly Property ShowWithoutActivation As Boolean
        Get
            Return True
        End Get
    End Property

    Protected Overrides ReadOnly Property CreateParams As CreateParams
        Get
            Dim k_cp As CreateParams = MyBase.CreateParams
            k_cp.ExStyle = k_cp.ExStyle Or WS_EX_TOOLWINDOW Or WS_EX_NOACTIVATE
            Return k_cp
        End Get
    End Property

    ''' <summary>
    ''' Covers <paramref name="k_area"/> (screen coordinates) except <paramref name="k_holes"/> (screen
    ''' coordinates, each grown a little so the ring around a hole is not veiled) and
    ''' <paramref name="k_keepOut"/> (the host's caption bar: moving, minimizing and closing it
    ''' stay free). Empty <paramref name="k_keepOut"/> = nothing kept out.
    ''' </summary>
    Public Sub Cover(k_area As Rectangle, k_holes As IEnumerable(Of Rectangle), k_keepOut As Rectangle)
        Try
            Bounds = k_area
            Dim k_shape As New Region(New Rectangle(Point.Empty, k_area.Size))
            Dim k_grow As Integer = CInt(Math.Round(4 * DeviceDpi / 96.0))
            For Each k_hole As Rectangle In k_holes
                Dim k_local As Rectangle = Rectangle.Inflate(k_hole, k_grow, k_grow)
                k_local.Offset(-k_area.Left, -k_area.Top)
                k_shape.Exclude(k_local)
            Next
            If Not k_keepOut.IsEmpty Then
                Dim k_bar As Rectangle = k_keepOut
                k_bar.Offset(-k_area.Left, -k_area.Top)
                k_shape.Exclude(k_bar)
            End If
            Dim k_old As Region = Region
            Region = k_shape
            k_old?.Dispose()
        Catch ex As Exception
            GlobalErrorLog.Write("TutorialDim.Cover", ex)
            Throw
        End Try
    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        Try
            RaiseEvent Clicked()
        Catch ex As Exception
            GlobalErrorLog.Write("TutorialDim.OnMouseDown", ex)
        End Try
    End Sub

End Class
