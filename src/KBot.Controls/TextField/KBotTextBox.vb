Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.InteropServices
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' Caseta de text a casei: un <c>TextBox</c> fără chenar așezat într-un cadru care pictează
''' conturul (CULOARE și GROSIME alese de operator) și, pentru varianta multilinie, DOUĂ
''' <see cref="KBotScrollBar"/> desenate de noi — deci tematizate, inclusiv pe schema întunecată.
'''
''' <para><b>Diferența față de <see cref="KBotTextField"/>:</b> acela e câmpul de formular de pe
''' <c>LoginForm</c> (o linie, contur de 1px, ochi de parolă, fără derulare). Ăsta e caseta
''' generală — multilinie, cu chenar reglabil și bare proprii.</para>
'''
''' <para><b>Cum se derulează fără barele native.</b> <c>TextBox</c>-ul intern rămâne cu
''' <c>ScrollBars = None</c>, deci Windows nu-i desenează nicio bandă; poziția o citim și o
''' mișcăm prin mesajele controlului de editare: <c>EM_GETLINECOUNT</c> / <c>EM_GETFIRSTVISIBLELINE</c>
''' / <c>EM_LINESCROLL</c> pe verticală, iar pe orizontală decalajul ADEVĂRAT vine din
''' <c>EM_POSFROMCHAR</c> pe primul caracter al liniei vizibile (nu-l ținem noi într-un câmp, fiindcă
''' și cursorul de text mută vederea, iar un câmp propriu s-ar învechi în tăcere).</para>
'''
''' <para>Culorile lăsate goale vin din temă; una pusă în designer câștigă — convenția casei
''' (<c>Color.Empty</c> = «automat»), cu perechea <c>ShouldSerialize</c>/<c>Reset</c> pentru fiecare,
''' inclusiv pentru <c>BackColor</c>/<c>ForeColor</c>/<c>Font</c> moștenite (regula casei: un control
''' care își SCRIE singur aceste proprietăți trebuie să răspundă din steagul lui, nu din al bazei).</para>
'''
''' <para>Măsurile în px sunt LOGICE (96 dpi) și se scalează la pictare/așezare — regula casei.</para>
''' </summary>
<ToolboxItem(True)>
<DefaultProperty("Text")>
<DefaultEvent("TextChanged")>
Public NotInheritable Class KBotTextBox
    Inherits Control
    Implements IThemedControl

    ' ── Mesaje ale controlului de editare (user32) ────────────────────────────
    Private Const EM_GETRECT As Integer = &HB2
    Private Const EM_LINESCROLL As Integer = &HB6
    Private Const EM_GETLINECOUNT As Integer = &HBA
    Private Const EM_LINEINDEX As Integer = &HBB
    Private Const EM_GETFIRSTVISIBLELINE As Integer = &HCE
    Private Const EM_POSFROMCHAR As Integer = &HD6

    <DllImport("user32.dll", CharSet:=CharSet.Auto)>
    Private Shared Function SendMessage(hWnd As IntPtr, msg As Integer, wParam As IntPtr, lParam As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll", CharSet:=CharSet.Auto)>
    Private Shared Function SendMessage(hWnd As IntPtr, msg As Integer, wParam As IntPtr, ByRef lParam As RECT) As IntPtr
    End Function

    <StructLayout(LayoutKind.Sequential)>
    Private Structure RECT
        Public Left As Integer
        Public Top As Integer
        Public Right As Integer
        Public Bottom As Integer
    End Structure

    ' ── Copiii ────────────────────────────────────────────────────────────────
    Private ReadOnly _inner As New EditIntern()
    Private ReadOnly _vBar As New KBotScrollBar()
    Private ReadOnly _hBar As New KBotScrollBar()

    ' ── Culorile din temă ─────────────────────────────────────────────────────
    Private _fundalTema As Color = Color.White
    Private _textTema As Color = Color.Black
    Private _chenarTema As Color = Color.Gray
    Private _chenarFocusTema As Color = Color.DodgerBlue

    ' ── Alegerile operatorului (Empty = urmează tema) ─────────────────────────
    Private _chenarPinuit As Color = Color.Empty
    Private _chenarFocusPinuit As Color = Color.Empty
    Private _fundalPinuit As Boolean
    Private _textPinuit As Boolean
    Private _fontPinuit As Boolean

    ' ── Aspect ────────────────────────────────────────────────────────────────
    Private _grosimeChenar As Integer = 1        ' px LOGICI
    Private _grosimeChenarFocus As Integer = 1   ' px LOGICI
    Private _raza As Integer = 4                 ' px LOGICI
    Private _paddingIntern As Integer = 6        ' px LOGICI
    Private _grosimeBara As Integer = KBotScrollBar.GrosimeImplicita  ' px LOGICI
    Private _bare As System.Windows.Forms.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
    Private _ascundeBareleNefolosite As Boolean = True

    ' ── Stare ─────────────────────────────────────────────────────────────────
    Private _areFocus As Boolean
    Private _sincronizez As Boolean
    ' Cea mai lungă linie, în pixeli. Sincronizarea barelor se cheamă la FIECARE mesaj care ar
    ' putea muta vederea (inclusiv mișcarea mouse-ului cu butonul apăsat), iar măsurarea tuturor
    ' rândurilor la fiecare astfel de mesaj ar fi cea mai scumpă operație din control. -1 = de
    ' recalculat; se golește doar când se schimbă textul sau fontul.
    Private _latimeMaximaCache As Integer = -1

    ''' <summary>KeyDown-ul casetei interne, re-ridicat pe cadru (cadrul nu primește focus).</summary>
    Public Event FieldKeyDown As KeyEventHandler

    Public Sub New()
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw Or
                 ControlStyles.SupportsTransparentBackColor, True)
        SetStyle(ControlStyles.Selectable, False)
        TabStop = False

        _inner.BorderStyle = BorderStyle.None
        _inner.Multiline = True
        _inner.WordWrap = True
        _inner.ScrollBars = System.Windows.Forms.ScrollBars.None
        AddHandler _inner.Enter, AddressOf OnInnerEnter
        AddHandler _inner.Leave, AddressOf OnInnerLeave
        AddHandler _inner.KeyDown, AddressOf OnInnerKeyDown
        AddHandler _inner.TextChanged, AddressOf OnInnerTextChanged
        AddHandler _inner.ViewChanged, AddressOf OnInnerViewChanged

        _vBar.Orientation = Orientation.Vertical
        _hBar.Orientation = Orientation.Horizontal
        _vBar.Visible = False
        _hBar.Visible = False
        AddHandler _vBar.Scroll, AddressOf OnVBarScroll
        AddHandler _hBar.Scroll, AddressOf OnHBarScroll

        Controls.Add(_inner)
        Controls.Add(_vBar)
        Controls.Add(_hBar)
    End Sub

    ''' <summary>
    ''' Dimensiunea de pornire. E dată de AICI, nu scrisă în constructor: o scriere pe
    ''' <c>Size</c> face <c>ShouldSerializeSize</c> să răspundă True, iar designerul ar tipări o
    ''' linie <c>Size</c> în fiecare gazdă (regula casei).
    ''' </summary>
    Protected Overrides ReadOnly Property DefaultSize As Size
        Get
            Return New Size(200, 96)
        End Get
    End Property

    ' ═══ Suprafața de text ═══════════════════════════════════════════════════

    <Category("K-BOT")>
    <Description("Textul casetei.")>
    <Browsable(True)>
    <EditorBrowsable(EditorBrowsableState.Always)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)>
    Public Overrides Property Text As String
        Get
            Return _inner.Text
        End Get
        Set(value As String)
            _inner.Text = value
        End Set
    End Property

    <Category("K-BOT")>
    <Description("Textul pe mai multe rânduri.")>
    <DefaultValue(True)>
    Public Property Multiline As Boolean
        Get
            Return _inner.Multiline
        End Get
        Set(v As Boolean)
            If _inner.Multiline = v Then Return
            _inner.Multiline = v
            RefaLayout()
        End Set
    End Property

    <Category("K-BOT")>
    <Description("Rupe rândurile la marginea casetei. False = derulare pe orizontală.")>
    <DefaultValue(True)>
    Public Property WordWrap As Boolean
        Get
            Return _inner.WordWrap
        End Get
        Set(v As Boolean)
            If _inner.WordWrap = v Then Return
            _inner.WordWrap = v
            RefaLayout()
        End Set
    End Property

    <Category("K-BOT")>
    <Description("Care bare de derulare sunt permise. Cele desenate de noi, nu cele native.")>
    <DefaultValue(System.Windows.Forms.ScrollBars.Vertical)>
    Public Property ScrollBars As System.Windows.Forms.ScrollBars
        Get
            Return _bare
        End Get
        Set(v As System.Windows.Forms.ScrollBars)
            If _bare = v Then Return
            _bare = v
            RefaLayout()
        End Set
    End Property

    <Category("K-BOT")>
    <Description("Ascunde bara când nu e nimic de derulat.")>
    <DefaultValue(True)>
    Public Property AutoHideScrollBars As Boolean
        Get
            Return _ascundeBareleNefolosite
        End Get
        Set(v As Boolean)
            If _ascundeBareleNefolosite = v Then Return
            _ascundeBareleNefolosite = v
            RefaLayout()
        End Set
    End Property

    <Category("K-BOT")>
    <Description("Grosimea barelor de derulare (px logici).")>
    <DefaultValue(KBotScrollBar.GrosimeImplicita)>
    Public Property ScrollBarThickness As Integer
        Get
            Return _grosimeBara
        End Get
        Set(v As Integer)
            Dim nou As Integer = Math.Max(4, v)
            If _grosimeBara = nou Then Return
            _grosimeBara = nou
            RefaLayout()
        End Set
    End Property

    <Category("K-BOT")>
    <Description("Caseta nu poate fi editată.")>
    <DefaultValue(False)>
    Public Property [ReadOnly] As Boolean
        Get
            Return _inner.ReadOnly
        End Get
        Set(v As Boolean)
            _inner.ReadOnly = v
        End Set
    End Property

    <Category("K-BOT")>
    <Description("Lungimea maximă a textului (0 = fără limită).")>
    <DefaultValue(32767)>
    Public Property MaxLength As Integer
        Get
            Return _inner.MaxLength
        End Get
        Set(v As Integer)
            _inner.MaxLength = v
        End Set
    End Property

    <Category("K-BOT")>
    <Description("Textul palid arătat cât timp caseta e goală.")>
    <DefaultValue("")>
    Public Property PlaceholderText As String
        Get
            Return _inner.PlaceholderText
        End Get
        Set(v As String)
            _inner.PlaceholderText = v
        End Set
    End Property

    <Category("K-BOT")>
    <Description("Alinierea textului în casetă.")>
    <DefaultValue(HorizontalAlignment.Left)>
    Public Property TextAlign As HorizontalAlignment
        Get
            Return _inner.TextAlign
        End Get
        Set(v As HorizontalAlignment)
            _inner.TextAlign = v
        End Set
    End Property

    <Category("K-BOT")>
    <Description("Ascunde caracterele (parolă). Doar pe o singură linie.")>
    <DefaultValue(False)>
    Public Property UseSystemPasswordChar As Boolean
        Get
            Return _inner.UseSystemPasswordChar
        End Get
        Set(v As Boolean)
            _inner.UseSystemPasswordChar = v
        End Set
    End Property

    ''' <summary>Rândurile textului (delegat la caseta internă).</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Lines As String()
        Get
            Return _inner.Lines
        End Get
        Set(v As String())
            _inner.Lines = v
        End Set
    End Property

    ''' <summary>Caseta internă, pentru ce nu e scos la suprafață (SelectionStart, AppendText…).</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property InnerTextBox As TextBox
        Get
            Return _inner
        End Get
    End Property

    ''' <summary>Bara verticală desenată de noi (probe și gazde care vor s-o citească).</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property VerticalScrollBar As KBotScrollBar
        Get
            Return _vBar
        End Get
    End Property

    ''' <summary>Bara orizontală desenată de noi.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property HorizontalScrollBar As KBotScrollBar
        Get
            Return _hBar
        End Get
    End Property

    ''' <summary>Dă focus casetei interne (cadrul nu e selectabil).</summary>
    Public Sub FocusInput()
        _inner.Focus()
    End Sub

    ''' <summary>Adaugă text la sfârșit și duce vederea acolo (jurnal, consolă).</summary>
    Public Sub AppendText(text As String)
        Try
            _inner.AppendText(text)
            SincronizeazaBare()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotTextBox.AppendText", ex)
            Throw
        End Try
    End Sub

    ' ═══ Chenar ══════════════════════════════════════════════════════════════

    <Category("K-BOT")>
    <Description("Culoarea chenarului; goală = InputBorderColor din temă.")>
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

    <Category("K-BOT")>
    <Description("Culoarea chenarului cât timp caseta are focus; goală = accentul temei.")>
    Public Property FocusBorderColor As Color
        Get
            Return If(_chenarFocusPinuit <> Color.Empty, _chenarFocusPinuit, _chenarFocusTema)
        End Get
        Set(v As Color)
            _chenarFocusPinuit = v
            Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeFocusBorderColor() As Boolean
        Return _chenarFocusPinuit <> Color.Empty
    End Function
    Public Sub ResetFocusBorderColor()
        _chenarFocusPinuit = Color.Empty
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
            RefaLayout()
        End Set
    End Property

    ''' <summary>
    ''' Grosimea chenarului la focus (px LOGICI). Se ține separat ca îngroșarea să nu MIȘTE
    ''' textul: așezarea rezervă întotdeauna maximul dintre cele două.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Grosimea chenarului la focus (px logici).")>
    <DefaultValue(1)>
    Public Property FocusBorderWidth As Integer
        Get
            Return _grosimeChenarFocus
        End Get
        Set(v As Integer)
            Dim nou As Integer = Math.Max(0, v)
            If _grosimeChenarFocus = nou Then Return
            _grosimeChenarFocus = nou
            RefaLayout()
        End Set
    End Property

    <Category("K-BOT")>
    <Description("Raza colțurilor (px logici). 0 = colțuri drepte.")>
    <DefaultValue(4)>
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

    <Category("K-BOT")>
    <Description("Aerul dintre chenar și text (px logici).")>
    <DefaultValue(6)>
    Public Property TextPadding As Integer
        Get
            Return _paddingIntern
        End Get
        Set(v As Integer)
            Dim nou As Integer = Math.Max(0, v)
            If _paddingIntern = nou Then Return
            _paddingIntern = nou
            RefaLayout()
        End Set
    End Property

    ' ═══ Proprietăți ambientale: steag propriu, ca designerul să nu le înghețe ═

    Public Overrides Property BackColor As Color
        Get
            Return MyBase.BackColor
        End Get
        Set(v As Color)
            _fundalPinuit = True
            MyBase.BackColor = v
            _inner.BackColor = v
            Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeBackColor() As Boolean
        Return _fundalPinuit
    End Function
    Public Overrides Sub ResetBackColor()
        _fundalPinuit = False
        MyBase.ResetBackColor()
        AplicaFundalTema()
    End Sub

    Public Overrides Property ForeColor As Color
        Get
            Return MyBase.ForeColor
        End Get
        Set(v As Color)
            _textPinuit = True
            MyBase.ForeColor = v
            _inner.ForeColor = v
            Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeForeColor() As Boolean
        Return _textPinuit
    End Function
    Public Overrides Sub ResetForeColor()
        _textPinuit = False
        MyBase.ResetForeColor()
        AplicaFundalTema()
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
            _fundalTema = p.InputBackColor
            _textTema = p.InputTextColor
            _chenarTema = p.InputBorderColor
            _chenarFocusTema = p.AccentColor
            AplicaFundalTema()
            ' Barele sunt copii IThemedControl, dar ThemeManager NU coboară în ele decât prin
            ' ApplyToNestedThemed; le dăm schema direct, ca să fie corecte și când cadrul e
            ' tematizat de mână (banc de probă, previzualizare).
            _vBar.ApplyTheme(scheme)
            _hBar.ApplyTheme(scheme)
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotTextBox.ApplyTheme", ex)
        End Try
    End Sub

    ' Culorile temei ajung pe caseta internă DOAR unde operatorul n-a pus nimic.
    Private Sub AplicaFundalTema()
        If Not _fundalPinuit Then
            MyBase.BackColor = _fundalTema
            _inner.BackColor = _fundalTema
        End If
        If Not _textPinuit Then
            MyBase.ForeColor = _textTema
            _inner.ForeColor = _textTema
        End If
    End Sub

    ' ═══ Așezare ═════════════════════════════════════════════════════════════

    ''' <summary>Grosimea de chenar REZERVATĂ (px scalați): maximul dintre normal și focus.</summary>
    Private Function ChenarRezervat() As Integer
        Return ThemeShapes.ScaleDpi(Me, Math.Max(_grosimeChenar, _grosimeChenarFocus))
    End Function

    ''' <summary>Dreptunghiul dinăuntrul chenarului și al aerului — acolo stau textul și barele.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property ContentBounds As Rectangle
        Get
            Dim m As Integer = ChenarRezervat() + ThemeShapes.ScaleDpi(Me, _paddingIntern)
            Return New Rectangle(m, m, Math.Max(0, Width - 2 * m), Math.Max(0, Height - 2 * m))
        End Get
    End Property

    Private Sub RefaLayout()
        Try
            AsazaCopiii()
            SincronizeazaBare()
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotTextBox.RefaLayout", ex)
        End Try
    End Sub

    Private Sub AsazaCopiii()
        Dim zona As Rectangle = ContentBounds
        If zona.Width <= 0 OrElse zona.Height <= 0 Then Return

        Dim grosime As Integer = ThemeShapes.ScaleDpi(Me, _grosimeBara)
        Dim cuV As Boolean = _vBar.Visible
        Dim cuH As Boolean = _hBar.Visible

        Dim latimeText As Integer = Math.Max(0, zona.Width - If(cuV, grosime, 0))
        Dim inaltimeText As Integer = Math.Max(0, zona.Height - If(cuH, grosime, 0))

        ' Pe o singură linie caseta internă își impune înălțimea; o centrăm în zonă.
        If _inner.Multiline Then
            _inner.SetBounds(zona.X, zona.Y, latimeText, inaltimeText)
        Else
            Dim sus As Integer = zona.Y + Math.Max(0, (zona.Height - _inner.Height) \ 2)
            _inner.SetBounds(zona.X, sus, latimeText, _inner.Height)
        End If

        If cuV Then _vBar.SetBounds(zona.Right - grosime, zona.Y, grosime, inaltimeText)
        If cuH Then _hBar.SetBounds(zona.X, zona.Bottom - grosime, latimeText, grosime)
    End Sub

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        Try
            RefaLayout()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotTextBox.OnResize", ex)
        End Try
    End Sub

    Protected Overrides Sub OnFontChanged(e As EventArgs)
        MyBase.OnFontChanged(e)
        Try
            _inner.Font = Font
            _latimeMaximaCache = -1
            RefaLayout()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotTextBox.OnFontChanged", ex)
        End Try
    End Sub

    Protected Overrides Sub OnHandleCreated(e As EventArgs)
        MyBase.OnHandleCreated(e)
        Try
            ' Abia acum DeviceDpi spune adevărul (regula casei) — și abia acum caseta internă
            ' are handle, deci abia acum mesajele EM_* răspund cinstit.
            RefaLayout()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotTextBox.OnHandleCreated", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Cât de înaltă trebuie să fie caseta ca să încapă un rând pe fontul CURENT, plus chenarul
    ''' și aerul. O citește <c>ThemeTableFit</c> — o schemă poate schimba fontul de bază.
    ''' </summary>
    Public Overrides Function GetPreferredSize(proposedSize As Size) As Size
        Try
            Dim m As Integer = ChenarRezervat() + ThemeShapes.ScaleDpi(Me, _paddingIntern)
            Return New Size(Width, _inner.PreferredHeight + 2 * m)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotTextBox.GetPreferredSize", ex)
            Return MyBase.GetPreferredSize(proposedSize)
        End Try
    End Function

    ' ═══ Pictură ═════════════════════════════════════════════════════════════

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Try
            Dim g As Graphics = e.Graphics
            g.SmoothingMode = SmoothingMode.AntiAlias

            Dim grosime As Integer = ThemeShapes.ScaleDpi(Me, If(_areFocus, _grosimeChenarFocus, _grosimeChenar))
            Dim raza As Integer = ThemeShapes.ScaleDpi(Me, _raza)

            ' Conturul se desenează PE mijlocul liniei, deci cadrul se strânge cu jumătate de
            ' grosime ca să nu iasă din control.
            Dim jum As Integer = Math.Max(0, (grosime - 1) \ 2)
            Dim zona As New Rectangle(jum, jum,
                                      Math.Max(0, Width - 1 - 2 * jum),
                                      Math.Max(0, Height - 1 - 2 * jum))
            If zona.Width <= 0 OrElse zona.Height <= 0 Then Return

            Using cale As GraphicsPath = ThemeShapes.RoundedRect(zona, raza)
                Using b As New SolidBrush(BackColor)
                    g.FillPath(b, cale)
                End Using
                If grosime > 0 Then
                    Using pen As New Pen(If(_areFocus, FocusBorderColor, BorderColor), grosime)
                        g.DrawPath(pen, cale)
                    End Using
                End If
            End Using
        Catch ex As Exception
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotTextBox.OnPaint", ex)
        End Try
    End Sub

    ' ═══ Derulare ════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Recitește vederea casetei interne și pune barele pe ea (interval, poziție, vizibilitate).
    ''' Idempotentă: se cheamă la fiecare tastă, la fiecare click și la fiecare redimensionare.
    ''' </summary>
    Public Sub SincronizeazaBare()
        Try
            If _sincronizez Then Return
            _sincronizez = True
            Try
                Dim vreaV As Boolean = _inner.Multiline AndAlso
                                       (_bare = System.Windows.Forms.ScrollBars.Vertical OrElse _bare = System.Windows.Forms.ScrollBars.Both)
                Dim vreaH As Boolean = _inner.Multiline AndAlso Not _inner.WordWrap AndAlso
                                       (_bare = System.Windows.Forms.ScrollBars.Horizontal OrElse _bare = System.Windows.Forms.ScrollBars.Both)

                Dim schimbat As Boolean = False

                ' ── Verticala: în LINII, exact unitatea lui EM_LINESCROLL ──
                If vreaV Then
                    Dim total As Integer = NumarLinii()
                    Dim vizibile As Integer = Math.Max(1, LiniiVizibile())
                    _vBar.SmallChange = 1
                    _vBar.SetRange(0, Math.Max(0, total - 1), vizibile, PrimaLinieVizibila())
                    Dim seVede As Boolean = (Not _ascundeBareleNefolosite) OrElse _vBar.IsScrollable
                    If _vBar.Visible <> seVede Then
                        _vBar.Visible = seVede
                        schimbat = True
                    End If
                ElseIf _vBar.Visible Then
                    _vBar.Visible = False
                    schimbat = True
                End If

                ' ── Orizontala: în PIXELI, cu decalajul citit din control ──
                If vreaH Then
                    Dim latimeText As Integer = LatimeCeaMaiLungaLinie()
                    Dim fereastra As Integer = Math.Max(1, _inner.ClientSize.Width)
                    _hBar.SmallChange = Math.Max(1, LatimeCaracter())
                    _hBar.SetRange(0, Math.Max(0, latimeText - 1), fereastra, DecalajOrizontal())
                    Dim seVede As Boolean = (Not _ascundeBareleNefolosite) OrElse _hBar.IsScrollable
                    If _hBar.Visible <> seVede Then
                        _hBar.Visible = seVede
                        schimbat = True
                    End If
                ElseIf _hBar.Visible Then
                    _hBar.Visible = False
                    schimbat = True
                End If

                ' O bară apărută/dispărută schimbă lățimea textului => reașezare, o singură dată.
                If schimbat Then AsazaCopiii()
            Finally
                _sincronizez = False
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("KBotTextBox.SincronizeazaBare", ex)
        End Try
    End Sub

    Private Function AreHandleIntern() As Boolean
        Return _inner.IsHandleCreated
    End Function

    Private Function NumarLinii() As Integer
        If Not AreHandleIntern() Then Return Math.Max(1, _inner.Lines.Length)
        Return Math.Max(1, SendMessage(_inner.Handle, EM_GETLINECOUNT, IntPtr.Zero, IntPtr.Zero).ToInt32())
    End Function

    Private Function PrimaLinieVizibila() As Integer
        If Not AreHandleIntern() Then Return 0
        Return Math.Max(0, SendMessage(_inner.Handle, EM_GETFIRSTVISIBLELINE, IntPtr.Zero, IntPtr.Zero).ToInt32())
    End Function

    Private Function InaltimeLinie() As Integer
        Return Math.Max(1, _inner.Font.Height)
    End Function

    Private Function LiniiVizibile() As Integer
        Return Math.Max(1, _inner.ClientSize.Height \ InaltimeLinie())
    End Function

    Private Function LatimeCaracter() As Integer
        Return Math.Max(1, TextRenderer.MeasureText("0123456789", _inner.Font).Width \ 10)
    End Function

    ' Cea mai lungă linie, în pixeli — atât cât are de mers bara orizontală. Rezultatul se ține
    ' în cache: se schimbă doar la text sau font, nu la fiecare derulare.
    Private Function LatimeCeaMaiLungaLinie() As Integer
        If _latimeMaximaCache >= 0 Then Return _latimeMaximaCache
        Dim maxim As Integer = 0
        For Each linie As String In _inner.Lines
            Dim w As Integer = TextRenderer.MeasureText(linie, _inner.Font).Width
            If w > maxim Then maxim = w
        Next
        _latimeMaximaCache = maxim
        Return maxim
    End Function

    ''' <summary>
    ''' Decalajul orizontal REAL, în pixeli: unde a ajuns primul caracter al liniei vizibile față
    ''' de marginea de formatare. Îl citim, nu îl ținem — și cursorul de text mută vederea.
    ''' </summary>
    Private Function DecalajOrizontal() As Integer
        If Not AreHandleIntern() Then Return 0
        Dim linie As Integer = PrimaLinieVizibila()
        Dim indice As Integer = SendMessage(_inner.Handle, EM_LINEINDEX, New IntPtr(linie), IntPtr.Zero).ToInt32()
        If indice < 0 Then Return 0

        Dim pozitie As Integer = SendMessage(_inner.Handle, EM_POSFROMCHAR, New IntPtr(indice), IntPtr.Zero).ToInt32()
        If pozitie = -1 Then Return 0
        Dim x As Integer = CInt(CShort(pozitie And &HFFFF))

        Dim r As RECT = Nothing
        SendMessage(_inner.Handle, EM_GETRECT, IntPtr.Zero, r)
        Return Math.Max(0, r.Left - x)
    End Function

    ' Mută vederea la valoarea cerută de bară. Verticala e în linii, orizontala în pixeli
    ' convertiți în caractere — apoi RECITIM adevărul, ca bara să nu rămână lângă text.
    Private Sub OnVBarScroll(sender As Object, e As ScrollEventArgs)
        Try
            If Not AreHandleIntern() Then Return
            Dim delta As Integer = _vBar.Value - PrimaLinieVizibila()
            If delta <> 0 Then SendMessage(_inner.Handle, EM_LINESCROLL, IntPtr.Zero, New IntPtr(delta))
        Catch ex As Exception
            GlobalErrorLog.Write("KBotTextBox.OnVBarScroll", ex)
        End Try
    End Sub

    Private Sub OnHBarScroll(sender As Object, e As ScrollEventArgs)
        Try
            If Not AreHandleIntern() Then Return
            Dim deltaPx As Integer = _hBar.Value - DecalajOrizontal()
            Dim caractere As Integer = CInt(Math.Round(deltaPx / CDbl(LatimeCaracter())))
            If caractere <> 0 Then SendMessage(_inner.Handle, EM_LINESCROLL, New IntPtr(caractere), IntPtr.Zero)
            ' Derularea se face în caractere, deci poziția ATINSĂ nu e fix cea cerută: punem pe
            ' bară ce s-a întâmplat cu adevărat, altfel cursorul barei ar fugi de text.
            _hBar.Value = DecalajOrizontal()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotTextBox.OnHBarScroll", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseWheel(e As MouseEventArgs)
        MyBase.OnMouseWheel(e)
        Try
            If Not _vBar.Visible Then Return
            Dim linii As Integer = SystemInformation.MouseWheelScrollLines
            If linii <= 0 Then linii = 3
            Dim pasi As Integer = -(e.Delta \ SystemInformation.MouseWheelScrollDelta) * linii
            If pasi = 0 Then Return
            _vBar.Value = _vBar.Value + pasi
            OnVBarScroll(_vBar, New ScrollEventArgs(ScrollEventType.ThumbPosition, _vBar.Value))
        Catch ex As Exception
            GlobalErrorLog.Write("KBotTextBox.OnMouseWheel", ex)
        End Try
    End Sub

    ' ═══ Evenimentele casetei interne ════════════════════════════════════════

    Private Sub OnInnerEnter(sender As Object, e As EventArgs)
        _areFocus = True
        Invalidate()
    End Sub

    Private Sub OnInnerLeave(sender As Object, e As EventArgs)
        _areFocus = False
        Invalidate()
    End Sub

    Private Sub OnInnerKeyDown(sender As Object, e As KeyEventArgs)
        RaiseEvent FieldKeyDown(Me, e)
    End Sub

    ' Textul intern s-a schimbat => și al cadrului, fiindcă `Text` e delegat acolo (vezi
    ' KBotTextField: fără re-ridicare, un `Handles txt.TextChanged` din designer n-ar porni).
    Private Sub OnInnerTextChanged(sender As Object, e As EventArgs)
        Try
            _latimeMaximaCache = -1
            OnTextChanged(EventArgs.Empty)
            SincronizeazaBare()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotTextBox.OnInnerTextChanged", ex)
        End Try
    End Sub

    Private Sub OnInnerViewChanged(sender As Object, e As EventArgs)
        Try
            SincronizeazaBare()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotTextBox.OnInnerViewChanged", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Caseta internă. Singurul motiv pentru care e o clasă separată: fără bare native nu există
    ''' niciun <c>WM_VSCROLL</c> de ascultat, deci trebuie să aflăm ALTFEL că s-a mutat vederea —
    ''' din mesajele care o pot muta (tastă, click, rotiță, lipire, derulare la cursor).
    ''' </summary>
    Private NotInheritable Class EditIntern
        Inherits TextBox

        Private Const WM_KEYDOWN As Integer = &H100
        Private Const WM_KEYUP As Integer = &H101
        Private Const WM_CHAR As Integer = &H102
        Private Const WM_LBUTTONDOWN As Integer = &H201
        Private Const WM_LBUTTONUP As Integer = &H202
        Private Const WM_MOUSEMOVE As Integer = &H200
        Private Const WM_MOUSEWHEEL As Integer = &H20A
        Private Const WM_PASTE As Integer = &H302
        Private Const WM_CUT As Integer = &H300
        Private Const WM_SIZE As Integer = &H5
        Private Const EM_SCROLLCARET As Integer = &HB7
        Private Const EM_LINESCROLL As Integer = &HB6

        ''' <summary>Vederea S-AR PUTEA să se fi mutat — gazda recitește și pune barele la loc.</summary>
        Public Event ViewChanged As EventHandler

        ' WndProc rămâne NEîmbrăcat în Try (regula casei: contractul de mesaje al ferestrei nu se
        ' rupe; plasa e Application.ThreadException).
        Protected Overrides Sub WndProc(ByRef m As Message)
            MyBase.WndProc(m)
            Select Case m.Msg
                Case WM_KEYDOWN, WM_KEYUP, WM_CHAR, WM_LBUTTONDOWN, WM_LBUTTONUP, WM_MOUSEMOVE,
                     WM_MOUSEWHEEL, WM_PASTE, WM_CUT, WM_SIZE, EM_SCROLLCARET, EM_LINESCROLL
                    RaiseEvent ViewChanged(Me, EventArgs.Empty)
            End Select
        End Sub

    End Class

End Class
