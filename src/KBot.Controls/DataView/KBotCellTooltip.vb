Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' Setările eticheteti plutitoare de celulă a <see cref="KBotDataView"/> (slice 0028) —
''' „obiectul de tooltip” expus de grilă, prin <see cref="KBotDataView.CellTooltip"/>.
'''
''' E o clasă de SETĂRI, nu fereastra: fereastra (<see cref="KBotCellTooltipWindow"/>) e un
''' detaliu intern, care se creează la prima afișare și moare cu grila. Așa proprietățile se pot
''' autoriza din grila de proprietăți a designerului (obiect imbricat, serializat pe conținut),
''' fără ca designerul să încerce vreodată să instanțieze o fereastră.
'''
''' <para><b>Culorile goale înseamnă „din temă”</b>, ca peste tot în K-BOT: grila le rezolvă la
''' pictare din schema activă, iar o culoare pusă explicit câștigă. Perechile
''' ShouldSerialize/Reset sunt obligatorii — vezi regula casei despre valorile rezolvate pe care
''' Visual Studio le îngheață în <c>.Designer.vb</c>.</para>
''' </summary>
<TypeConverter(GetType(ExpandableObjectConverter))>
Public NotInheritable Class KBotCellTooltipOptions

    Private _enabled As Boolean = True
    Private _delay As Integer = 450
    Private _maxWidth As Integer = 480
    Private _backColor As Color = Color.Empty
    Private _foreColor As Color = Color.Empty
    Private _borderColor As Color = Color.Empty
    Private _font As Font = Nothing
    Private _cornerRadius As Integer = 4

    ''' <summary>Grila care ne deține — ca o schimbare de setare să repicteze/închidă eticheta.</summary>
    Friend Property Owner As KBotDataView

    ''' <summary>
    ''' Eticheta apare deloc? Implicit True. Stinsă, o celulă tăiată rămâne tăiată — ceea ce e
    ''' alegerea corectă pentru o grilă cu coloane oricum auto-dimensionate.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Afișează eticheta plutitoare peste celulele al căror text nu încape.")>
    <DefaultValue(True)>
    Public Property Enabled As Boolean
        Get
            Return _enabled
        End Get
        Set(value As Boolean)
            If _enabled = value Then Return
            _enabled = value
            If Not _enabled Then Owner?.CancelCellTooltip()
        End Set
    End Property

    ''' <summary>Cât stă cursorul pe celulă înainte să iasă eticheta (ms). Implicit 450.</summary>
    <Category("K-BOT")>
    <Description("Întârzierea (ms) după care apare eticheta. Implicit 450.")>
    <DefaultValue(450)>
    Public Property Delay As Integer
        Get
            Return _delay
        End Get
        Set(value As Integer)
            _delay = Math.Max(1, value)
        End Set
    End Property

    ''' <summary>
    ''' Lățimea maximă (px) a etichetei. Un text mai lung se rupe pe mai multe rânduri, nu se
    ''' taie: eticheta există tocmai ca să arate ce nu încăpea.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Lățimea maximă (px) a etichetei; peste ea, textul se rupe pe rânduri. Implicit 480.")>
    <DefaultValue(480)>
    Public Property MaxWidth As Integer
        Get
            Return _maxWidth
        End Get
        Set(value As Integer)
            _maxWidth = Math.Max(80, value)
        End Set
    End Property

    ''' <summary>Fundalul etichetei. Gol (implicit) = din schema activă.</summary>
    <Category("K-BOT")>
    <Description("Fundalul etichetei. Gol = culoarea din schema activă.")>
    Public Property BackColor As Color
        Get
            Return _backColor
        End Get
        Set(value As Color)
            _backColor = value
        End Set
    End Property

    Private Function ShouldSerializeBackColor() As Boolean
        Return _backColor <> Color.Empty
    End Function

    Private Sub ResetBackColor()
        _backColor = Color.Empty
    End Sub

    ''' <summary>Culoarea textului. Gol (implicit) = din schema activă.</summary>
    <Category("K-BOT")>
    <Description("Culoarea textului din etichetă. Gol = culoarea din schema activă.")>
    Public Property ForeColor As Color
        Get
            Return _foreColor
        End Get
        Set(value As Color)
            _foreColor = value
        End Set
    End Property

    Private Function ShouldSerializeForeColor() As Boolean
        Return _foreColor <> Color.Empty
    End Function

    Private Sub ResetForeColor()
        _foreColor = Color.Empty
    End Sub

    ''' <summary>Culoarea conturului. Gol (implicit) = din schema activă.</summary>
    <Category("K-BOT")>
    <Description("Culoarea conturului etichetei. Gol = culoarea din schema activă.")>
    Public Property BorderColor As Color
        Get
            Return _borderColor
        End Get
        Set(value As Color)
            _borderColor = value
        End Set
    End Property

    Private Function ShouldSerializeBorderColor() As Boolean
        Return _borderColor <> Color.Empty
    End Function

    Private Sub ResetBorderColor()
        _borderColor = Color.Empty
    End Sub

    ''' <summary>
    ''' Fontul etichetei. <c>Nothing</c> (implicit) = fontul CELULEI, ceea ce e și ideea: eticheta
    ''' arată același text, cu aceleași litere, doar întreg.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Fontul etichetei. Nesetat = fontul celulei peste care stă cursorul.")>
    Public Property Font As Font
        Get
            Return _font
        End Get
        Set(value As Font)
            _font = value
        End Set
    End Property

    Private Function ShouldSerializeFont() As Boolean
        Return _font IsNot Nothing
    End Function

    Private Sub ResetFont()
        _font = Nothing
    End Sub

    ''' <summary>Raza colțurilor rotunjite (px). Implicit 4; 0 = colțuri drepte.</summary>
    <Category("K-BOT")>
    <Description("Raza colțurilor etichetei (px). 0 = colțuri drepte.")>
    <DefaultValue(4)>
    Public Property CornerRadius As Integer
        Get
            Return _cornerRadius
        End Get
        Set(value As Integer)
            _cornerRadius = Math.Max(0, value)
        End Set
    End Property

    Public Overrides Function ToString() As String
        Return If(_enabled, "Pornit", "Stins")
    End Function

End Class

''' <summary>
''' FEREASTRA etichetei de celulă — sora lui <c>TreeNodeFlyout</c> / <c>KBotNavFlyout</c>, cu
''' aceleași trucuri și din aceleași motive: <c>WS_EX_NOACTIVATE</c> +
''' <see cref="ShowWithoutActivation"/> (nu fură focusul), <c>WS_EX_TOOLWINDOW</c> (fără buton în
''' bara de activități) și <c>HTTRANSPARENT</c> pe <c>WM_NCHITTEST</c>, ca mouse-ul să TREACĂ PRIN
''' ea la grila de dedesubt.
'''
''' Ultima parte nu e cosmetică: eticheta se așază lângă celula peste care stă cursorul și o poate
''' atinge. Fără ea, hover-ul s-ar pierde în clipa în care apare fereastra, ceea ce ar ascunde-o,
''' ceea ce ar readuce hover-ul… la infinit.
'''
''' NU e un <c>ToolTip</c> WinForms: acela nu poate fi tematizat (culorile lui vin din sistem), nu
''' se poate rotunji și nu poate purta fontul celulei.
''' </summary>
Friend NotInheritable Class KBotCellTooltipWindow
    Inherits Form

    Private Const WM_NCHITTEST As Integer = &H84
    Private Const HTTRANSPARENT As Integer = -1
    Private Const WS_EX_NOACTIVATE As Integer = &H8000000
    Private Const WS_EX_TOOLWINDOW As Integer = &H80

    ''' <summary>Marginea interioară: aceeași sus/jos și stânga/dreapta ca la o celulă.</summary>
    Friend Shared ReadOnly PaddingSize As New Size(8, 5)

    Private _text As String = String.Empty
    Private _font As Font
    Private _fore As Color = SystemColors.InfoText
    Private _fill As Color = SystemColors.Info
    Private _border As Color = SystemColors.ActiveBorder
    Private _radius As Integer = 4

    Public Sub New()
        FormBorderStyle = FormBorderStyle.None
        ShowInTaskbar = False
        StartPosition = FormStartPosition.Manual
        ControlBox = False
        MinimizeBox = False
        MaximizeBox = False
        Text = String.Empty
        ' Fără autoscalare: grila ne dă Bounds în px DEJA scalați; o a doua ajustare ar muta
        ' eticheta de lângă celula ei.
        AutoScaleMode = AutoScaleMode.None
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer, True)
    End Sub

    Protected Overrides ReadOnly Property ShowWithoutActivation As Boolean
        Get
            Return True
        End Get
    End Property

    Protected Overrides ReadOnly Property CreateParams As CreateParams
        Get
            Dim cp As CreateParams = MyBase.CreateParams
            cp.ExStyle = cp.ExStyle Or WS_EX_NOACTIVATE Or WS_EX_TOOLWINDOW
            Return cp
        End Get
    End Property

    ' Regula casei lasă WndProc pe plasa globală Application.ThreadException: un Try/Catch aici ar
    ' risca să rupă contractul de mesaje al ferestrei.
    Protected Overrides Sub WndProc(ByRef m As Message)
        MyBase.WndProc(m)
        If m.Msg = WM_NCHITTEST Then m.Result = New IntPtr(HTTRANSPARENT)
    End Sub

    ''' <summary>Ce scrie și cum arată. Fontul e ÎMPRUMUTAT — fereastra nu-l deține.</summary>
    Friend Sub SetContent(text As String, font As Font, fore As Color, fill As Color,
                          border As Color, radius As Integer)
        _text = If(text, String.Empty)
        _font = font
        _fore = fore
        _fill = fill
        _border = border
        _radius = Math.Max(0, radius)
        BackColor = fill
        Invalidate()
    End Sub

    ' Colțurile rotunjite se taie din FEREASTRĂ, nu doar din desen: altfel în colțuri s-ar vedea
    ' dreptunghiul ferestrei peste grila de dedesubt.
    Protected Overrides Sub OnSizeChanged(e As EventArgs)
        MyBase.OnSizeChanged(e)
        Try
            If _radius <= 0 OrElse ClientSize.Width <= 0 OrElse ClientSize.Height <= 0 Then
                Region = Nothing
                Return
            End If
            Using path As GraphicsPath = ThemeShapes.RoundedRect(
                    New Rectangle(0, 0, ClientSize.Width, ClientSize.Height), _radius)
                Region = New Region(path)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("KBotCellTooltipWindow.OnSizeChanged", ex)
        End Try
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Try
            Dim g As Graphics = e.Graphics
            g.SmoothingMode = SmoothingMode.AntiAlias

            ' -1 pe fiecare latură ca și conturul să intre în fereastră, nu pe jumătate afară.
            Dim r As New Rectangle(0, 0, Math.Max(1, ClientSize.Width - 1), Math.Max(1, ClientSize.Height - 1))
            Using path As GraphicsPath = ThemeShapes.RoundedRect(r, Math.Max(1, _radius))
                Using b As New SolidBrush(_fill)
                    g.FillPath(b, path)
                End Using
                ' Conturul e ce o desprinde de grila de dedesubt — fundalul singur ar face-o să
                ' pară o pată de aceeași culoare cu rândul.
                Using pen As New Pen(_border)
                    g.DrawPath(pen, path)
                End Using
            End Using

            If String.IsNullOrEmpty(_text) OrElse _font Is Nothing Then Return
            Dim textRect As New Rectangle(PaddingSize.Width, PaddingSize.Height,
                                          Math.Max(0, ClientSize.Width - 2 * PaddingSize.Width),
                                          Math.Max(0, ClientSize.Height - 2 * PaddingSize.Height))
            TextRenderer.DrawText(g, _text, _font, textRect, _fore,
                TextFormatFlags.Left Or TextFormatFlags.Top Or TextFormatFlags.WordBreak)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotCellTooltipWindow.OnPaint", ex)
        End Try
    End Sub

End Class
