Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' Eticheta casei: un <c>Label</c> obișnuit care își desenează SINGUR chenarul — culoare și
''' grosime alese de operator, cu colțuri rotunjite dacă se cere.
'''
''' <para><b>De ce nu <c>BorderStyle</c>.</b> Cele trei valori native (<c>None</c>/<c>FixedSingle</c>/
''' <c>Fixed3D</c>) sunt desenate de Windows în culorile SISTEMULUI: pe schema întunecată rămâne
''' un chenar gri deschis, iar grosimea nu se poate atinge deloc. De aceea proprietatea moștenită
''' e ascunsă (<c>Browsable(False)</c>) și ținută pe <c>None</c>, iar chenarul îl pictăm noi.</para>
'''
''' <para>Moștenește <c>Label</c>, nu <c>Control</c>: <c>AutoSize</c>, <c>TextAlign</c>,
''' <c>UseMnemonic</c> și măsurarea textului sunt deja rezolvate acolo și nu are rost rescrise.
''' <c>GetPreferredSize</c> adaugă doar grosimea chenarului, ca la <c>AutoSize = True</c> textul
''' să nu se atingă de linie.</para>
'''
''' <para>Culorile lăsate goale vin din temă; una pusă în designer câștigă — convenția casei
''' (<c>Color.Empty</c> = «automat»), cu perechea <c>ShouldSerialize</c>/<c>Reset</c> pentru fiecare,
''' inclusiv pentru <c>BackColor</c>/<c>ForeColor</c> moștenite (regula casei: un control care își
''' SCRIE singur aceste proprietăți răspunde din steagul lui, nu din al bazei — altfel designerul
''' îngheață paleta curentă în <c>.Designer.vb</c>).</para>
'''
''' <para>Grosimea și raza sunt în px LOGICI (96 dpi) și se scalează la pictare — regula casei.</para>
''' </summary>
<ToolboxItem(True)>
Public NotInheritable Class KBotLabel
    Inherits Label
    Implements IThemedControl

    ' ── Culorile din temă ─────────────────────────────────────────────────────
    Private _chenarTema As Color = Color.Gray
    Private _textTema As Color = Color.Black
    Private _fundalTema As Color = Color.Transparent

    ' ── Alegerile operatorului ────────────────────────────────────────────────
    Private _chenarPinuit As Color = Color.Empty
    Private _textPinuit As Boolean
    Private _fundalPinuit As Boolean
    Private _fontPinuit As Boolean

    ' ── Aspect (px LOGICI) ────────────────────────────────────────────────────
    Private _grosimeChenar As Integer = 1
    Private _raza As Integer = 0

    Public Sub New()
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw Or
                 ControlStyles.SupportsTransparentBackColor, True)
        MyBase.BorderStyle = BorderStyle.None
    End Sub

    ' ═══ Chenar ══════════════════════════════════════════════════════════════

    <Category("K-BOT")>
    <Description("Culoarea chenarului; goală = BorderColor din temă.")>
    Public Property BorderColor As Color
        Get
            Return If(_chenarPinuit <> Color.Empty, _chenarPinuit, _chenarTema)
        End Get
        Set(v As Color)
            _chenarPinuit = v
            Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeBorderColor() As Boolean
        Return _chenarPinuit <> Color.Empty
    End Function
    Public Sub ResetBorderColor()
        _chenarPinuit = Color.Empty
        Invalidate()
    End Sub

    ''' <summary>Grosimea chenarului în px LOGICI. 0 = fără chenar.</summary>
    <Category("K-BOT")>
    <Description("Grosimea chenarului (px logici). 0 = fără chenar.")>
    <DefaultValue(1)>
    Public Property BorderWidth As Integer
        Get
            Return _grosimeChenar
        End Get
        Set(v As Integer)
            Dim nou As Integer = Math.Max(0, v)
            If _grosimeChenar = nou Then Return
            _grosimeChenar = nou
            ' Chenarul intră în dimensiunea preferată: la AutoSize eticheta trebuie să crească.
            If AutoSize Then Size = GetPreferredSize(Size.Empty)
            Invalidate()
        End Set
    End Property

    <Category("K-BOT")>
    <Description("Raza colțurilor (px logici). 0 = colțuri drepte.")>
    <DefaultValue(0)>
    Public Property CornerRadius As Integer
        Get
            Return _raza
        End Get
        Set(v As Integer)
            Dim nou As Integer = Math.Max(0, v)
            If _raza = nou Then Return
            _raza = nou
            Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' Chenarul nativ NU se folosește: e desenat de Windows, în culorile sistemului. Rămâne pe
    ''' <c>None</c>; folosiți <see cref="BorderColor"/> + <see cref="BorderWidth"/>.
    ''' </summary>
    <Browsable(False)>
    <EditorBrowsable(EditorBrowsableState.Never)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Shadows Property BorderStyle As System.Windows.Forms.BorderStyle
        Get
            Return MyBase.BorderStyle
        End Get
        Set(v As System.Windows.Forms.BorderStyle)
            ' Fără no-op tăcut (regula casei): cine cere un chenar nativ trebuie să afle de ce nu-l primește.
            If v <> System.Windows.Forms.BorderStyle.None Then
                Throw New ArgumentException("KBotLabel nu folosește chenarul nativ; folosiți BorderColor și BorderWidth.", NameOf(v))
            End If
            MyBase.BorderStyle = v
        End Set
    End Property

    ' ═══ Proprietăți ambientale: steag propriu, ca designerul să nu le înghețe ═

    Public Overrides Property ForeColor As Color
        Get
            Return MyBase.ForeColor
        End Get
        Set(v As Color)
            _textPinuit = True
            MyBase.ForeColor = v
        End Set
    End Property
    Public Function ShouldSerializeForeColor() As Boolean
        Return _textPinuit
    End Function
    Public Overrides Sub ResetForeColor()
        _textPinuit = False
        MyBase.ResetForeColor()
        AplicaCulorileTemei()
    End Sub

    Public Overrides Property BackColor As Color
        Get
            Return MyBase.BackColor
        End Get
        Set(v As Color)
            _fundalPinuit = True
            MyBase.BackColor = v
        End Set
    End Property
    Public Function ShouldSerializeBackColor() As Boolean
        Return _fundalPinuit
    End Function
    Public Overrides Sub ResetBackColor()
        _fundalPinuit = False
        MyBase.ResetBackColor()
        AplicaCulorileTemei()
    End Sub

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

    ' ═══ Temă ════════════════════════════════════════════════════════════════

    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            _chenarTema = p.BorderColor
            _textTema = p.TextColor
            ' Eticheta stă pe suprafața gazdei: fundalul rămâne transparent, ca regula generică
            ' de Label din ThemeManager. Cine vrea altceva îl pune în designer și câștigă.
            _fundalTema = Color.Transparent
            AplicaCulorileTemei()
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotLabel.ApplyTheme", ex)
        End Try
    End Sub

    Private Sub AplicaCulorileTemei()
        If Not _textPinuit Then MyBase.ForeColor = _textTema
        If Not _fundalPinuit Then MyBase.BackColor = _fundalTema
    End Sub

    ' ═══ Măsurare și pictură ═════════════════════════════════════════════════

    ''' <summary>Dimensiunea preferată a lui <c>Label</c>, plus chenarul de pe ambele laturi.</summary>
    Public Overrides Function GetPreferredSize(proposedSize As Size) As Size
        Try
            Dim baza As Size = MyBase.GetPreferredSize(proposedSize)
            Dim g As Integer = ThemeShapes.ScaleDpi(Me, _grosimeChenar)
            If g <= 0 Then Return baza
            Return New Size(baza.Width + 2 * g, baza.Height + 2 * g)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotLabel.GetPreferredSize", ex)
            Return MyBase.GetPreferredSize(proposedSize)
        End Try
    End Function

    ''' <summary>
    ''' Fundalul. Cu colțuri rotunjite îl pictăm noi (umplerea dreptunghiulară a bazei ar scoate
    ''' colțuri pătrate pe sub arcul chenarului); fără rotunjire rămâne al bazei.
    ''' </summary>
    Protected Overrides Sub OnPaintBackground(e As PaintEventArgs)
        Try
            If _raza <= 0 Then
                MyBase.OnPaintBackground(e)
                Return
            End If

            ' Sub colțul rotunjit trebuie să se vadă gazda, nu fundalul nostru: cerem ca părintele
            ' să picteze întâi, apoi umplem doar forma.
            Dim transparent As Boolean = BackColor.A < 255
            If transparent Then
                MyBase.OnPaintBackground(e)
            Else
                Dim g As Graphics = e.Graphics
                g.SmoothingMode = SmoothingMode.AntiAlias
                Dim zona As New Rectangle(0, 0, Math.Max(0, Width - 1), Math.Max(0, Height - 1))
                If zona.Width <= 0 OrElse zona.Height <= 0 Then Return
                Using cale As GraphicsPath = ThemeShapes.RoundedRect(zona, ThemeShapes.ScaleDpi(Me, _raza))
                    Using b As New SolidBrush(BackColor)
                        g.FillPath(b, cale)
                    End Using
                End Using
            End If
        Catch ex As Exception
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotLabel.OnPaintBackground", ex)
        End Try
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Try
            ' Textul îl scrie baza (aliniere, mnemonice, elipsă); noi punem chenarul peste.
            MyBase.OnPaint(e)

            Dim grosime As Integer = ThemeShapes.ScaleDpi(Me, _grosimeChenar)
            If grosime <= 0 Then Return

            Dim g As Graphics = e.Graphics
            g.SmoothingMode = SmoothingMode.AntiAlias

            ' Conturul se desenează pe MIJLOCUL liniei: cadrul se strânge cu jumătate de grosime,
            ' altfel jumătate din linie ar cădea în afara controlului.
            Dim jum As Integer = Math.Max(0, (grosime - 1) \ 2)
            Dim zona As New Rectangle(jum, jum,
                                      Math.Max(0, Width - 1 - 2 * jum),
                                      Math.Max(0, Height - 1 - 2 * jum))
            If zona.Width <= 0 OrElse zona.Height <= 0 Then Return

            Using cale As GraphicsPath = ThemeShapes.RoundedRect(zona, ThemeShapes.ScaleDpi(Me, _raza))
                Using pen As New Pen(BorderColor, grosime)
                    g.DrawPath(pen, cale)
                End Using
            End Using
        Catch ex As Exception
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotLabel.OnPaint", ex)
        End Try
    End Sub

End Class
