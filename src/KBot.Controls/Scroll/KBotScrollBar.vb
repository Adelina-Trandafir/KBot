Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' Bară de derulare desenată de noi — fratele tematizat al lui <c>VScrollBar</c>/<c>HScrollBar</c>.
'''
''' <para><b>De ce există.</b> Barele native sunt ferestre desenate de Windows: nicio culoare din
''' paletă nu ajunge la ele, iar <c>SetWindowTheme("DarkMode_Explorer")</c> — trucul folosit până
''' acum în <c>KBotDataView</c> — le dă doar griul întunecat al sistemului, niciodată accentul
''' schemei. Nota din <c>KBotDataView.Theming.vb</c> anunța exact controlul ăsta.</para>
'''
''' <para>Semantica valorilor e a lui <c>System.Windows.Forms.ScrollBar</c>, ca să poată înlocui
''' una nativă fără recalcularea gazdei: intervalul util e <c>Minimum .. Maximum - LargeChange + 1</c>,
''' iar lungimea cursorului e fracția <c>LargeChange / (Maximum - Minimum + 1)</c> din șină.</para>
'''
''' <para>Culorile lăsate goale vin din temă; una pusă în designer câștigă — convenția casei
''' (<c>Color.Empty</c> = «automat»), cu perechea <c>ShouldSerialize</c>/<c>Reset</c> pentru fiecare.</para>
'''
''' <para>Bara NU e selectabilă: stă lângă un câmp de editare, iar un Tab care aterizează pe ea ar
''' fi o surpriză. Rotița și tastatura rămân ale gazdei.</para>
''' </summary>
<ToolboxItem(True)>
<DefaultEvent("Scroll")>
Public NotInheritable Class KBotScrollBar
    Inherits Control
    Implements IThemedControl

    ''' <summary>Grosimea implicită (px LOGICI) — cât ține gazda pentru bandă.</summary>
    Public Const GrosimeImplicita As Integer = 12

    ' ── Culorile din temă (scrise de ApplyTheme) ──────────────────────────────
    Private _sinaTema As Color = Color.Gainsboro
    Private _cursorTema As Color = Color.Silver
    Private _cursorHoverTema As Color = Color.DodgerBlue
    Private _sagetiTema As Color = Color.Gray

    ' ── Alegerile operatorului (Empty = urmează tema) ─────────────────────────
    Private _sinaPinuit As Color = Color.Empty
    Private _cursorPinuit As Color = Color.Empty
    Private _cursorHoverPinuit As Color = Color.Empty
    Private _sagetiPinuit As Color = Color.Empty

    ' ── Interval ──────────────────────────────────────────────────────────────
    Private _minim As Integer = 0
    Private _maxim As Integer = 100
    Private _valoare As Integer = 0
    Private _pasMic As Integer = 1
    Private _pasMare As Integer = 10

    ' ── Aspect ────────────────────────────────────────────────────────────────
    Private _orientare As Orientation = Orientation.Vertical
    Private _cuSageti As Boolean = True
    Private _lungimeMinimaCursor As Integer = 18   ' px LOGICI
    Private _razaCursor As Integer = 3             ' px LOGICI
    Private _margineCursor As Integer = 2          ' px LOGICI, aerul dintre cursor și șină

    ' ── Stare de interacțiune ─────────────────────────────────────────────────
    Private _hoverCursor As Boolean
    Private _trage As Boolean
    Private _decalajTragere As Integer
    Private _valoareLaApasare As Integer
    Private ReadOnly _repetare As New Timer()
    Private _actiuneRepetata As ScrollEventType = ScrollEventType.EndScroll

    ''' <summary>S-a derulat (aceeași semnătură ca la bara nativă).</summary>
    Public Event Scroll As ScrollEventHandler

    ''' <summary>S-a schimbat <see cref="Value"/> — din orice cauză, inclusiv programatic.</summary>
    Public Event ValueChanged As EventHandler

    Public Sub New()
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw Or
                 ControlStyles.SupportsTransparentBackColor, True)
        SetStyle(ControlStyles.Selectable, False)
        TabStop = False
        _repetare.Interval = 350
        AddHandler _repetare.Tick, AddressOf OnRepetareTick
    End Sub

    ''' <summary>
    ''' Dimensiunea de pornire. E dată de AICI, nu scrisă în constructor: o scriere pe
    ''' <c>Size</c> face <c>ShouldSerializeSize</c> să răspundă True, iar designerul ar tipări o
    ''' linie <c>Size</c> în fiecare gazdă (regula casei despre proprietățile care nu pot purta
    ''' <c>DefaultValue</c>).
    ''' </summary>
    Protected Overrides ReadOnly Property DefaultSize As Size
        Get
            Return New Size(GrosimeImplicita, 80)
        End Get
    End Property

    ' ═══ Interval ════════════════════════════════════════════════════════════

    <Category("K-BOT")>
    <Description("Capătul de jos al intervalului.")>
    <DefaultValue(0)>
    Public Property Minimum As Integer
        Get
            Return _minim
        End Get
        Set(v As Integer)
            If _minim = v Then Return
            _minim = v
            If _maxim < _minim Then _maxim = _minim
            ClampValoare()
            Invalidate()
        End Set
    End Property

    <Category("K-BOT")>
    <Description("Capătul de sus al intervalului.")>
    <DefaultValue(100)>
    Public Property Maximum As Integer
        Get
            Return _maxim
        End Get
        Set(v As Integer)
            Dim nou As Integer = Math.Max(_minim, v)
            If _maxim = nou Then Return
            _maxim = nou
            ClampValoare()
            Invalidate()
        End Set
    End Property

    ''' <summary>Poziția curentă, prinsă în <c>Minimum .. <see cref="MaxValue"/></c>.</summary>
    <Category("K-BOT")>
    <Description("Poziția curentă. Valorile din afara intervalului se prind, nu aruncă.")>
    <DefaultValue(0)>
    Public Property Value As Integer
        Get
            Return _valoare
        End Get
        Set(v As Integer)
            Dim nou As Integer = Math.Max(_minim, Math.Min(MaxValue, v))
            If _valoare = nou Then Return
            _valoare = nou
            Invalidate()
            RaiseEvent ValueChanged(Me, EventArgs.Empty)
        End Set
    End Property

    <Category("K-BOT")>
    <Description("Pasul unei săgeți (o linie).")>
    <DefaultValue(1)>
    Public Property SmallChange As Integer
        Get
            Return _pasMic
        End Get
        Set(v As Integer)
            _pasMic = Math.Max(1, v)
        End Set
    End Property

    ''' <summary>
    ''' Cât se vede deodată — pasul unei pagini ȘI fracția de șină ocupată de cursor.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Cât se vede deodată: pasul de pagină și, totodată, lungimea cursorului.")>
    <DefaultValue(10)>
    Public Property LargeChange As Integer
        Get
            Return _pasMare
        End Get
        Set(v As Integer)
            Dim nou As Integer = Math.Max(1, v)
            If _pasMare = nou Then Return
            _pasMare = nou
            ClampValoare()
            Invalidate()
        End Set
    End Property

    ''' <summary>Cea mai mare valoare la care se poate ajunge: <c>Maximum - LargeChange + 1</c>.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property MaxValue As Integer
        Get
            Return Math.Max(_minim, _maxim - _pasMare + 1)
        End Get
    End Property

    ''' <summary>Are ce derula? (conținutul depășește fereastra)</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property IsScrollable As Boolean
        Get
            Return MaxValue > _minim
        End Get
    End Property

    ''' <summary>Pune interval și poziție dintr-o singură scriere, cu O SINGURĂ invalidare.</summary>
    Public Sub SetRange(minim As Integer, maxim As Integer, pasMare As Integer, valoare As Integer)
        Try
            _minim = minim
            _maxim = Math.Max(minim, maxim)
            _pasMare = Math.Max(1, pasMare)
            Dim vechi As Integer = _valoare
            _valoare = Math.Max(_minim, Math.Min(MaxValue, valoare))
            Invalidate()
            If _valoare <> vechi Then RaiseEvent ValueChanged(Me, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotScrollBar.SetRange", ex)
        End Try
    End Sub

    Private Sub ClampValoare()
        Dim nou As Integer = Math.Max(_minim, Math.Min(MaxValue, _valoare))
        If nou = _valoare Then Return
        _valoare = nou
        RaiseEvent ValueChanged(Me, EventArgs.Empty)
    End Sub

    ' ═══ Aspect ══════════════════════════════════════════════════════════════

    <Category("K-BOT")>
    <Description("Verticală sau orizontală.")>
    <DefaultValue(Orientation.Vertical)>
    Public Property Orientation As Orientation
        Get
            Return _orientare
        End Get
        Set(v As Orientation)
            If _orientare = v Then Return
            _orientare = v
            Invalidate()
        End Set
    End Property

    <Category("K-BOT")>
    <Description("Desenează săgețile de la capete.")>
    <DefaultValue(True)>
    Public Property ShowArrows As Boolean
        Get
            Return _cuSageti
        End Get
        Set(v As Boolean)
            If _cuSageti = v Then Return
            _cuSageti = v
            Invalidate()
        End Set
    End Property

    <Category("K-BOT")>
    <Description("Lungimea minimă a cursorului (px logici, se scalează la DPI).")>
    <DefaultValue(18)>
    Public Property MinimumThumbLength As Integer
        Get
            Return _lungimeMinimaCursor
        End Get
        Set(v As Integer)
            Dim nou As Integer = Math.Max(4, v)
            If _lungimeMinimaCursor = nou Then Return
            _lungimeMinimaCursor = nou
            Invalidate()
        End Set
    End Property

    <Category("K-BOT")>
    <Description("Raza colțurilor cursorului (px logici). 0 = colțuri drepte.")>
    <DefaultValue(3)>
    Public Property ThumbCornerRadius As Integer
        Get
            Return _razaCursor
        End Get
        Set(v As Integer)
            Dim nou As Integer = Math.Max(0, v)
            If _razaCursor = nou Then Return
            _razaCursor = nou
            Invalidate()
        End Set
    End Property

    <Category("K-BOT")>
    <Description("Aerul dintre cursor și marginile șinei (px logici).")>
    <DefaultValue(2)>
    Public Property ThumbPadding As Integer
        Get
            Return _margineCursor
        End Get
        Set(v As Integer)
            Dim nou As Integer = Math.Max(0, v)
            If _margineCursor = nou Then Return
            _margineCursor = nou
            Invalidate()
        End Set
    End Property

    ' ═══ Culori ══════════════════════════════════════════════════════════════

    <Category("K-BOT")>
    <Description("Culoarea șinei; goală = din temă.")>
    Public Property TrackColor As Color
        Get
            Return If(_sinaPinuit <> Color.Empty, _sinaPinuit, _sinaTema)
        End Get
        Set(v As Color)
            _sinaPinuit = v
            Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeTrackColor() As Boolean
        Return _sinaPinuit <> Color.Empty
    End Function
    Public Sub ResetTrackColor()
        _sinaPinuit = Color.Empty
        Invalidate()
    End Sub

    <Category("K-BOT")>
    <Description("Culoarea cursorului; goală = din temă.")>
    Public Property ThumbColor As Color
        Get
            Return If(_cursorPinuit <> Color.Empty, _cursorPinuit, _cursorTema)
        End Get
        Set(v As Color)
            _cursorPinuit = v
            Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeThumbColor() As Boolean
        Return _cursorPinuit <> Color.Empty
    End Function
    Public Sub ResetThumbColor()
        _cursorPinuit = Color.Empty
        Invalidate()
    End Sub

    <Category("K-BOT")>
    <Description("Culoarea cursorului sub mouse / în tragere; goală = accentul temei.")>
    Public Property ThumbHoverColor As Color
        Get
            Return If(_cursorHoverPinuit <> Color.Empty, _cursorHoverPinuit, _cursorHoverTema)
        End Get
        Set(v As Color)
            _cursorHoverPinuit = v
            Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeThumbHoverColor() As Boolean
        Return _cursorHoverPinuit <> Color.Empty
    End Function
    Public Sub ResetThumbHoverColor()
        _cursorHoverPinuit = Color.Empty
        Invalidate()
    End Sub

    <Category("K-BOT")>
    <Description("Culoarea săgeților; goală = din temă.")>
    Public Property ArrowColor As Color
        Get
            Return If(_sagetiPinuit <> Color.Empty, _sagetiPinuit, _sagetiTema)
        End Get
        Set(v As Color)
            _sagetiPinuit = v
            Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeArrowColor() As Boolean
        Return _sagetiPinuit <> Color.Empty
    End Function
    Public Sub ResetArrowColor()
        _sagetiPinuit = Color.Empty
        Invalidate()
    End Sub

    ' ═══ Temă ════════════════════════════════════════════════════════════════

    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        If scheme Is Nothing Then Return
        Dim p As ThemePalette = scheme.Palette
        ' Șina stă lipită de suprafață, cursorul trebuie să se vadă pe ea în AMBELE sensuri:
        ' pe schema întunecată «mai deschis decât fundalul», pe cea luminoasă «mai închis».
        ' De aceea plecăm de la Border/TextDim, nu de la o constantă.
        _sinaTema = ThemeShapes.Blend(p.SurfaceColor, p.BorderColor, 0.35)
        _cursorTema = ThemeShapes.Blend(p.BorderColor, p.TextDimColor, 0.45)
        _cursorHoverTema = p.AccentColor
        _sagetiTema = p.TextDimColor
        Invalidate()
    End Sub

    ' ═══ Geometrie ═══════════════════════════════════════════════════════════

    Private Function EsteVerticala() As Boolean
        Return _orientare = Orientation.Vertical
    End Function

    ''' <summary>Latura unei săgeți (0 dacă nu încap).</summary>
    Private Function LaturaSageata() As Integer
        If Not _cuSageti Then Return 0
        Dim grosime As Integer = If(EsteVerticala(), Width, Height)
        Dim lungime As Integer = If(EsteVerticala(), Height, Width)
        ' Două săgeți plus un rest de șină; sub pragul ăsta banda rămâne fără ele.
        If lungime < grosime * 3 Then Return 0
        Return grosime
    End Function

    ''' <summary>Șina — banda dintre săgeți.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property TrackBounds As Rectangle
        Get
            Dim a As Integer = LaturaSageata()
            If EsteVerticala() Then
                Return New Rectangle(0, a, Width, Math.Max(0, Height - 2 * a))
            End If
            Return New Rectangle(a, 0, Math.Max(0, Width - 2 * a), Height)
        End Get
    End Property

    ''' <summary>Cursorul. Gol (<c>Rectangle.Empty</c>) când nu e nimic de derulat.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property ThumbBounds As Rectangle
        Get
            Dim sina As Rectangle = TrackBounds
            If sina.Width <= 0 OrElse sina.Height <= 0 Then Return Rectangle.Empty
            If Not IsScrollable Then Return Rectangle.Empty

            Dim vert As Boolean = EsteVerticala()
            Dim lungimeSina As Integer = If(vert, sina.Height, sina.Width)
            Dim interval As Integer = Math.Max(1, _maxim - _minim + 1)
            Dim fractie As Double = Math.Min(1.0, _pasMare / CDbl(interval))

            Dim lungimeMin As Integer = Math.Min(lungimeSina, ThemeShapes.ScaleDpi(Me, _lungimeMinimaCursor))
            Dim lungime As Integer = Math.Max(lungimeMin, CInt(Math.Round(lungimeSina * fractie)))
            lungime = Math.Min(lungime, lungimeSina)

            Dim cursa As Integer = lungimeSina - lungime
            Dim span As Integer = MaxValue - _minim
            Dim decalaj As Integer = If(span > 0, CInt(Math.Round(cursa * ((_valoare - _minim) / CDbl(span)))), 0)

            Dim aer As Integer = ThemeShapes.ScaleDpi(Me, _margineCursor)
            If vert Then
                Dim latime As Integer = Math.Max(1, sina.Width - 2 * aer)
                Return New Rectangle(sina.X + aer, sina.Y + decalaj, latime, lungime)
            End If
            Dim inaltime As Integer = Math.Max(1, sina.Height - 2 * aer)
            Return New Rectangle(sina.X + decalaj, sina.Y + aer, lungime, inaltime)
        End Get
    End Property

    Private Function SageataInceput() As Rectangle
        Dim a As Integer = LaturaSageata()
        If a = 0 Then Return Rectangle.Empty
        Return New Rectangle(0, 0, If(EsteVerticala(), Width, a), If(EsteVerticala(), a, Height))
    End Function

    Private Function SageataSfarsit() As Rectangle
        Dim a As Integer = LaturaSageata()
        If a = 0 Then Return Rectangle.Empty
        If EsteVerticala() Then Return New Rectangle(0, Height - a, Width, a)
        Return New Rectangle(Width - a, 0, a, Height)
    End Function

    ' ═══ Pictură ═════════════════════════════════════════════════════════════

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Try
            Dim g As Graphics = e.Graphics
            g.SmoothingMode = SmoothingMode.AntiAlias

            Using b As New SolidBrush(TrackColor)
                g.FillRectangle(b, ClientRectangle)
            End Using

            Dim cursor As Rectangle = ThumbBounds
            If cursor.Width > 0 AndAlso cursor.Height > 0 Then
                Dim culoare As Color = If(_hoverCursor OrElse _trage, ThumbHoverColor, ThumbColor)
                Dim raza As Integer = ThemeShapes.ScaleDpi(Me, _razaCursor)
                Using cale As GraphicsPath = ThemeShapes.RoundedRect(cursor, raza)
                    Using b As New SolidBrush(culoare)
                        g.FillPath(b, cale)
                    End Using
                End Using
            End If

            If LaturaSageata() > 0 Then
                DeseneazaSageata(g, SageataInceput(), True)
                DeseneazaSageata(g, SageataSfarsit(), False)
            End If
        Catch ex As Exception
            ' Fără log din procesul designer-ului (vezi KBotDesignTime), ca la KBotProgressBar.
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotScrollBar.OnPaint", ex)
        End Try
    End Sub

    ' Triunghi plin, orientat spre capătul pe care stă. Transitiv acoperit de Try-ul din OnPaint.
    Private Sub DeseneazaSageata(g As Graphics, zona As Rectangle, inceput As Boolean)
        If zona.Width <= 0 OrElse zona.Height <= 0 Then Return
        Dim cx As Single = zona.Left + zona.Width / 2.0F
        Dim cy As Single = zona.Top + zona.Height / 2.0F
        Dim r As Single = Math.Min(zona.Width, zona.Height) * 0.24F
        If r < 1.5F Then Return

        Dim puncte As PointF()
        If EsteVerticala() Then
            puncte = If(inceput,
                        New PointF() {New PointF(cx, cy - r), New PointF(cx - r, cy + r), New PointF(cx + r, cy + r)},
                        New PointF() {New PointF(cx, cy + r), New PointF(cx - r, cy - r), New PointF(cx + r, cy - r)})
        Else
            puncte = If(inceput,
                        New PointF() {New PointF(cx - r, cy), New PointF(cx + r, cy - r), New PointF(cx + r, cy + r)},
                        New PointF() {New PointF(cx + r, cy), New PointF(cx - r, cy - r), New PointF(cx - r, cy + r)})
        End If
        Using b As New SolidBrush(ArrowColor)
            g.FillPolygon(b, puncte)
        End Using
    End Sub

    ' ═══ Mouse ═══════════════════════════════════════════════════════════════

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        Try
            If e.Button <> MouseButtons.Left Then Return
            _valoareLaApasare = _valoare

            Dim cursor As Rectangle = ThumbBounds
            If cursor.Contains(e.Location) Then
                _trage = True
                _decalajTragere = If(EsteVerticala(), e.Y - cursor.Y, e.X - cursor.X)
                Capture = True
                Invalidate()
                Return
            End If

            If SageataInceput().Contains(e.Location) Then
                PornesteRepetare(ScrollEventType.SmallDecrement)
                Return
            End If
            If SageataSfarsit().Contains(e.Location) Then
                PornesteRepetare(ScrollEventType.SmallIncrement)
                Return
            End If

            If TrackBounds.Contains(e.Location) AndAlso IsScrollable Then
                Dim inainte As Boolean = If(EsteVerticala(), e.Y > cursor.Bottom, e.X > cursor.Right)
                PornesteRepetare(If(inainte, ScrollEventType.LargeIncrement, ScrollEventType.LargeDecrement))
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KBotScrollBar.OnMouseDown", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)
        Try
            If _trage Then
                MutaCursorLa(If(EsteVerticala(), e.Y, e.X))
                Return
            End If
            Dim peste As Boolean = ThumbBounds.Contains(e.Location)
            If peste <> _hoverCursor Then
                _hoverCursor = peste
                Invalidate()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KBotScrollBar.OnMouseMove", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        MyBase.OnMouseUp(e)
        Try
            OpresteRepetare()
            If _trage Then
                _trage = False
                Capture = False
                Invalidate()
                RidicaScroll(ScrollEventType.EndScroll, _valoareLaApasare)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KBotScrollBar.OnMouseUp", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)
        If _hoverCursor Then
            _hoverCursor = False
            Invalidate()
        End If
    End Sub

    ''' <summary>
    ''' Trage cursorul astfel încât punctul apucat să ajungă la <paramref name="pozitie"/>.
    ''' Public pentru probe: tragerea reală nu se poate simula din test fără fereastră.
    ''' </summary>
    Public Sub MutaCursorLa(pozitie As Integer)
        Try
            Dim sina As Rectangle = TrackBounds
            Dim cursor As Rectangle = ThumbBounds
            If cursor.Width <= 0 OrElse cursor.Height <= 0 Then Return

            Dim vert As Boolean = EsteVerticala()
            Dim lungimeSina As Integer = If(vert, sina.Height, sina.Width)
            Dim lungimeCursor As Integer = If(vert, cursor.Height, cursor.Width)
            Dim cursa As Integer = lungimeSina - lungimeCursor
            If cursa <= 0 Then Return

            Dim start As Integer = If(vert, sina.Y, sina.X)
            Dim decalaj As Integer = Math.Max(0, Math.Min(cursa, pozitie - _decalajTragere - start))
            Dim span As Integer = MaxValue - _minim
            Dim nou As Integer = _minim + CInt(Math.Round(span * (decalaj / CDbl(cursa))))
            If nou = _valoare Then Return

            Value = nou
            RaiseEvent Scroll(Me, New ScrollEventArgs(ScrollEventType.ThumbTrack, _valoare, _valoare,
                                                      If(vert, ScrollOrientation.VerticalScroll, ScrollOrientation.HorizontalScroll)))
        Catch ex As Exception
            GlobalErrorLog.Write("KBotScrollBar.MutaCursorLa", ex)
        End Try
    End Sub

    ''' <summary>Un pas (săgeată sau pagină), cu ridicarea evenimentului. Public pentru probe.</summary>
    Public Sub Pas(tip As ScrollEventType)
        Try
            Dim vechi As Integer = _valoare
            Select Case tip
                Case ScrollEventType.SmallDecrement : Value = _valoare - _pasMic
                Case ScrollEventType.SmallIncrement : Value = _valoare + _pasMic
                Case ScrollEventType.LargeDecrement : Value = _valoare - _pasMare
                Case ScrollEventType.LargeIncrement : Value = _valoare + _pasMare
                Case Else : Return
            End Select
            If _valoare <> vechi Then RidicaScroll(tip, vechi)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotScrollBar.Pas", ex)
        End Try
    End Sub

    Private Sub RidicaScroll(tip As ScrollEventType, vechi As Integer)
        RaiseEvent Scroll(Me, New ScrollEventArgs(tip, vechi, _valoare,
                                                  If(EsteVerticala(), ScrollOrientation.VerticalScroll, ScrollOrientation.HorizontalScroll)))
    End Sub

    Private Sub PornesteRepetare(tip As ScrollEventType)
        Pas(tip)
        _actiuneRepetata = tip
        _repetare.Interval = 350
        _repetare.Start()
    End Sub

    Private Sub OpresteRepetare()
        _repetare.Stop()
        _actiuneRepetata = ScrollEventType.EndScroll
    End Sub

    Private Sub OnRepetareTick(sender As Object, e As EventArgs)
        Try
            ' Prima repetare are pauza lungă (350 ms), următoarele vin des — ca la bara nativă.
            _repetare.Interval = 60
            Pas(_actiuneRepetata)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotScrollBar.OnRepetareTick", ex)
        End Try
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing Then
                RemoveHandler _repetare.Tick, AddressOf OnRepetareTick
                _repetare.Dispose()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KBotScrollBar.Dispose", ex)
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

End Class
