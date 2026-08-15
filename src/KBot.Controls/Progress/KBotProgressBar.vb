Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' Bară de progres DETERMINATĂ (0..100), tematizată — fratele lui <see cref="KBotBusyBar"/>,
''' care e cel indeterminat. Înlocuiește <c>System.Windows.Forms.ProgressBar</c>, pe care
''' tema nu-l poate atinge (e desenat de Windows, deci rămâne verde pe schemă întunecată).
'''
''' <para>Culorile lăsate goale vin din temă; una pusă în designer câștigă — convenția casei
''' (<c>Color.Empty</c> = «automat»), cu perechea <c>ShouldSerialize</c>/<c>Reset</c> pentru
''' fiecare, ca designerul să nu îngheţe paleta curentă în <c>.Designer.vb</c>.</para>
''' </summary>
<ToolboxItem(True)>
Public NotInheritable Class KBotProgressBar
    Inherits Control
    Implements IThemedControl

    ' Culorile din temă (actualizate de ApplyTheme); cele „pinuite" sunt separat, mai jos.
    Private _accentTema As Color = Color.DodgerBlue
    Private _trackTema As Color = Color.Gainsboro
    Private _textTema As Color = Color.Black

    ' Alegerile operatorului din designer. Empty = urmează tema.
    Private _accentPinuit As Color = Color.Empty
    Private _trackPinuit As Color = Color.Empty
    Private _textPinuit As Color = Color.Empty

    Private _value As Integer = 0
    Private _maximum As Integer = 100
    Private _radius As Integer = 3
    Private _showText As Boolean = False
    ' Operatorul a scris el fontul? (vezi ShouldSerializeFont — regula casei)
    Private _fontPinuit As Boolean

    Public Sub New()
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw, True)
        Height = 18
    End Sub

    ' ── Valoare ──────────────────────────────────────────────────────────

    ''' <summary>Progresul curent, prins în intervalul 0..<see cref="Maximum"/>.</summary>
    <Category("K-BOT")>
    <Description("Progresul curent (0..Maximum). Valorile din afara intervalului se prind, nu aruncă.")>
    <DefaultValue(0)>
    Public Property Value As Integer
        Get
            Return _value
        End Get
        Set(v As Integer)
            ' Prindem, nu aruncăm: sursa e un IProgress(Of Integer) venit de pe firul robotului,
            ' iar o excepție acolo ar opri descărcarea din cauza unei bare de progres.
            Dim nou As Integer = Math.Max(0, Math.Min(_maximum, v))
            If _value = nou Then Return
            _value = nou
            Invalidate()
        End Set
    End Property

    <Category("K-BOT")>
    <Description("Capătul de sus al intervalului de progres.")>
    <DefaultValue(100)>
    Public Property Maximum As Integer
        Get
            Return _maximum
        End Get
        Set(v As Integer)
            Dim nou As Integer = Math.Max(1, v)
            If _maximum = nou Then Return
            _maximum = nou
            If _value > _maximum Then _value = _maximum
            Invalidate()
        End Set
    End Property

    ''' <summary>Fracția umplută, 0..1. Publică pentru probe și pentru gazde.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property Fraction As Double
        Get
            If _maximum <= 0 Then Return 0.0
            Return _value / CDbl(_maximum)
        End Get
    End Property

    ' ── Aspect ───────────────────────────────────────────────────────────

    <Category("K-BOT")>
    <Description("Raza colțurilor rotunjite (px logici). 0 = colțuri drepte.")>
    <DefaultValue(3)>
    Public Property CornerRadius As Integer
        Get
            Return _radius
        End Get
        Set(v As Integer)
            Dim nou As Integer = Math.Max(0, v)
            If _radius = nou Then Return
            _radius = nou
            Invalidate()
        End Set
    End Property

    <Category("K-BOT")>
    <Description("Scrie procentul peste bară.")>
    <DefaultValue(False)>
    Public Property ShowPercentText As Boolean
        Get
            Return _showText
        End Get
        Set(v As Boolean)
            If _showText = v Then Return
            _showText = v
            Invalidate()
        End Set
    End Property

    <Category("K-BOT")>
    <Description("Culoarea porțiunii umplute; goală = AccentColor din temă.")>
    Public Property BarColor As Color
        Get
            Return If(_accentPinuit <> Color.Empty, _accentPinuit, _accentTema)
        End Get
        Set(v As Color)
            _accentPinuit = v
            Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeBarColor() As Boolean
        Return _accentPinuit <> Color.Empty
    End Function
    Public Sub ResetBarColor()
        _accentPinuit = Color.Empty
        Invalidate()
    End Sub

    <Category("K-BOT")>
    <Description("Culoarea șinei (porțiunea goală); goală = SurfaceAltColor din temă.")>
    Public Property TrackColor As Color
        Get
            Return If(_trackPinuit <> Color.Empty, _trackPinuit, _trackTema)
        End Get
        Set(v As Color)
            _trackPinuit = v
            Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeTrackColor() As Boolean
        Return _trackPinuit <> Color.Empty
    End Function
    Public Sub ResetTrackColor()
        _trackPinuit = Color.Empty
        Invalidate()
    End Sub

    <Category("K-BOT")>
    <Description("Culoarea procentului scris peste bară; goală = TextColor din temă.")>
    Public Property PercentTextColor As Color
        Get
            Return If(_textPinuit <> Color.Empty, _textPinuit, _textTema)
        End Get
        Set(v As Color)
            _textPinuit = v
            Invalidate()
        End Set
    End Property
    Public Function ShouldSerializePercentTextColor() As Boolean
        Return _textPinuit <> Color.Empty
    End Function
    Public Sub ResetPercentTextColor()
        _textPinuit = Color.Empty
        Invalidate()
    End Sub

    ' Fontul: nu poate purta <DefaultValue> (atributul cere o constantă), deci are nevoie de
    ' perechea ShouldSerialize/Reset — altfel designerul îl scrie în FIECARE gazdă (regula casei).
    Public Overrides Property Font As Font
        Get
            Return MyBase.Font
        End Get
        Set(v As Font)
            _fontPinuit = True
            MyBase.Font = v
        End Set
    End Property
    Public Function ShouldSerializeFont() As Boolean
        Return _fontPinuit
    End Function
    Public Overrides Sub ResetFont()
        _fontPinuit = False
        MyBase.ResetFont()
    End Sub

    ' ── Temă ─────────────────────────────────────────────────────────────

    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        If scheme Is Nothing Then Return
        Dim p As ThemePalette = scheme.Palette
        _accentTema = p.AccentColor
        _trackTema = p.SurfaceAltColor
        _textTema = p.TextColor
        Invalidate()
    End Sub

    ' ── Pictură ──────────────────────────────────────────────────────────

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Try
            Dim g As Graphics = e.Graphics
            g.SmoothingMode = SmoothingMode.AntiAlias

            Dim zona As New Rectangle(0, 0, Math.Max(0, Width - 1), Math.Max(0, Height - 1))
            If zona.Width <= 0 OrElse zona.Height <= 0 Then Return

            Dim raza As Integer = ThemeShapes.ScaleDpi(Me, _radius)

            ' Șina.
            Using cale As GraphicsPath = ThemeShapes.RoundedRect(zona, raza)
                Using b As New SolidBrush(TrackColor)
                    g.FillPath(b, cale)
                End Using
            End Using

            ' Umplerea. Se decupează la forma șinei, ca marginea rotunjită să rămână rotunjită
            ' și la 100% — o umplere dreptunghiulară ar scoate colțuri pătrate peste ea.
            Dim latime As Integer = CInt(Math.Round(zona.Width * Fraction))
            If latime > 0 Then
                Using cale As GraphicsPath = ThemeShapes.RoundedRect(zona, raza)
                    Dim vechiClip As Region = g.Clip.Clone()
                    g.SetClip(cale, CombineMode.Intersect)
                    Using b As New SolidBrush(BarColor)
                        g.FillRectangle(b, zona.X, zona.Y, latime, zona.Height)
                    End Using
                    g.Clip = vechiClip
                End Using
            End If

            If _showText Then
                Dim procent As Integer = CInt(Math.Round(Fraction * 100))
                TextRenderer.DrawText(g, procent.ToString(Globalization.CultureInfo.InvariantCulture) & "%",
                                      Font, zona, PercentTextColor,
                                      TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter)
            End If
        Catch ex As Exception
            ' Fără log din procesul designer-ului (vezi KBotDesignTime), ca la KBotBusyBar.
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotProgressBar.OnPaint", ex)
        End Try
    End Sub

End Class
