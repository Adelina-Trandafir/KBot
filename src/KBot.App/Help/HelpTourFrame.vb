Option Strict On
Imports System.Drawing
Imports KBot.Theming

''' <summary>
''' The coloured ring a guided tour draws around the control it talks about (slice 0000-04).
''' A top-most window shaped as a hollow rectangle (its <see cref="Region"/> has the middle cut out),
''' so the control inside stays visible and clickable. It never takes the focus and lets clicks
''' through. No child controls: a plain <see cref="Form"/>, not a themed dialog.
''' </summary>
Friend NotInheritable Class HelpTourFrame
    Inherits Form

    Private Const WS_EX_TOOLWINDOW As Integer = &H80
    Private Const WS_EX_NOACTIVATE As Integer = &H8000000
    Private Const WS_EX_TRANSPARENT As Integer = &H20
    Private Const WS_EX_TOPMOST As Integer = &H8

    Public Sub New()
        FormBorderStyle = FormBorderStyle.None
        StartPosition = FormStartPosition.Manual
        ShowInTaskbar = False
        TopMost = True
        AutoScaleMode = AutoScaleMode.None
        BackColor = ThemeManager.Current.Palette.AccentColor
    End Sub

    Protected Overrides ReadOnly Property ShowWithoutActivation As Boolean
        Get
            Return True
        End Get
    End Property

    Protected Overrides ReadOnly Property CreateParams As CreateParams
        Get
            Dim cp As CreateParams = MyBase.CreateParams
            cp.ExStyle = cp.ExStyle Or WS_EX_TOOLWINDOW Or WS_EX_NOACTIVATE Or WS_EX_TRANSPARENT Or WS_EX_TOPMOST
            Return cp
        End Get
    End Property

    ''' <summary>Puts the ring around <paramref name="target"/> (screen coordinates).</summary>
    Public Sub Surround(target As Rectangle)
        Dim thick As Integer = Math.Max(3, CInt(Math.Round(3 * DeviceDpi / 96.0)))
        Dim gap As Integer = thick
        Dim outer As Rectangle = Rectangle.Inflate(target, gap + thick, gap + thick)
        Bounds = outer
        Dim inner As New Rectangle(thick, thick, outer.Width - 2 * thick, outer.Height - 2 * thick)
        Dim shape As New Region(New Rectangle(Point.Empty, outer.Size))
        shape.Exclude(inner)
        Dim old As Region = Region
        Region = shape
        old?.Dispose()
        BackColor = ThemeManager.Current.Palette.AccentColor
        If Not Visible Then Show()
    End Sub

End Class
