Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' Bară de ocupare indeterminată (3px logici): un segment accent alunecă înainte și
''' înapoi cât timp <see cref="Running"/> e True. Înlocuiește ProgressBar-ul marquee.
''' Când e oprită nu pictează nimic peste track (invizibilă pe card).
''' </summary>
<ToolboxItem(False)>
Public NotInheritable Class KBotBusyBar
    Inherits Control
    Implements IThemedControl

    Private ReadOnly _timer As New Timer()
    Private _accent As Color = Color.DodgerBlue
    Private _track As Color = Color.White
    Private _pos As Double = 0.0        ' 0..1, poziția centrului segmentului
    Private _dir As Integer = 1
    Private _running As Boolean = False

    Public Sub New()
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw, True)
        Height = 3
        _timer.Interval = 15
        AddHandler _timer.Tick, AddressOf OnTick
    End Sub

    ''' <summary>Pornește/oprește animația.</summary>
    Public Property Running As Boolean
        Get
            Return _running
        End Get
        Set(value As Boolean)
            If _running = value Then Return
            _running = value
            If value Then
                _pos = 0.0
                _dir = 1
                _timer.Start()
            Else
                _timer.Stop()
            End If
            Invalidate()
        End Set
    End Property

    ''' <summary>Reaplică culorile schemei.</summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        If scheme Is Nothing Then Return
        Dim p As ThemePalette = scheme.Palette
        _accent = p.AccentColor
        _track = p.SurfaceAltColor
        BackColor = _track
        Invalidate()
    End Sub

    Private Sub OnTick(sender As Object, e As EventArgs)
        _pos += 0.02 * _dir
        If _pos >= 1.0 Then
            _pos = 1.0
            _dir = -1
        ElseIf _pos <= 0.0 Then
            _pos = 0.0
            _dir = 1
        End If
        Invalidate()
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Try
            Dim g As Graphics = e.Graphics
            g.Clear(_track)
            If Not _running OrElse Width <= 0 Then Return

            Dim segW As Integer = Math.Max(ThemeShapes.ScaleDpi(Me, 40), CInt(Width * 0.3))
            Dim travel As Integer = Width - segW
            Dim x As Integer = CInt(travel * _pos)
            Using b As New SolidBrush(_accent)
                g.FillRectangle(b, x, 0, segW, Height)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("KBotBusyBar.OnPaint", ex)
        End Try
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then
            _timer.Stop()
            RemoveHandler _timer.Tick, AddressOf OnTick
            _timer.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

End Class
