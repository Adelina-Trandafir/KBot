Option Strict On
Imports System.ComponentModel
Imports System.ComponentModel.Design
Imports System.Drawing
Imports System.Drawing.Design
Imports System.Windows.Forms
Imports KBot.Theming

''' <summary>
''' CONȚINUTUL unei etichete: ce scrie, pentru un anume control (sau pentru o anume zonă dintr-un
''' control desenat de noi — un buton din antetul arborelui, de exemplu, care nu e un
''' <see cref="Control"/> și n-are cum să fie extins).
'''
''' <para>Fiecare câmp are aceeași regulă: <c>Nothing</c> înseamnă „ia din stil". Așa un control
''' poate schimba DOAR titlul, păstrând restul înfățișării comune, iar un altul poate schimba
''' DOAR pictograma.</para>
''' </summary>
Public NotInheritable Class KBotToolTipContent

    ''' <summary>Textul CORPULUI (acceptă marcaje). Fără el, eticheta nu are corp.</summary>
    Public Property Text As String = String.Empty

    ''' <summary>Titlul din antet; <c>Nothing</c> = cel din stil.</summary>
    Public Property HeaderText As String = Nothing

    ''' <summary>Textul din subsol; <c>Nothing</c> = cel din stil.</summary>
    Public Property FooterText As String = Nothing

    ''' <summary>Pictograma din antet; <c>Nothing</c> = cea din stil.</summary>
    Public Property HeaderIcon As Image = Nothing

    ''' <summary>Pictograma din subsol; <c>Nothing</c> = cea din stil.</summary>
    Public Property FooterIcon As Image = Nothing

    ''' <summary>Stilul propriu al acestui conținut; <c>Nothing</c> = stilul componentei.</summary>
    Public Property Style As KBotToolTipStyle = Nothing

    Friend Function EffectiveHeaderText(st As KBotToolTipStyle) As String
        Return If(HeaderText, If(st Is Nothing, String.Empty, st.Header.Text))
    End Function

    Friend Function EffectiveFooterText(st As KBotToolTipStyle) As String
        Return If(FooterText, If(st Is Nothing, String.Empty, st.Footer.Text))
    End Function

    Friend Function EffectiveHeaderIcon(st As KBotToolTipStyle) As Image
        Return If(HeaderIcon, If(st Is Nothing, Nothing, st.Header.Icon))
    End Function

    Friend Function EffectiveFooterIcon(st As KBotToolTipStyle) As Image
        Return If(FooterIcon, If(st Is Nothing, Nothing, st.Footer.Icon))
    End Function

    ''' <summary>Are ceva de arătat? O etichetă complet goală nu se deschide.</summary>
    Friend Function IsEmpty(st As KBotToolTipStyle) As Boolean
        Return String.IsNullOrEmpty(Text) AndAlso
               String.IsNullOrEmpty(EffectiveHeaderText(st)) AndAlso
               String.IsNullOrEmpty(EffectiveFooterText(st)) AndAlso
               EffectiveHeaderIcon(st) Is Nothing AndAlso
               EffectiveFooterIcon(st) Is Nothing
    End Function

End Class

''' <summary>
''' ETICHETA PLUTITOARE K-BOT — înlocuitorul tematizat al lui
''' <see cref="System.Windows.Forms.ToolTip"/>, cu antet (pictogramă + titlu), corp cu text
''' îmbogățit, subsol și linie despărțitoare între secțiunile care se văd.
'''
''' <para><b>Se folosește exact ca ToolTip-ul din WinForms:</b> se pune o componentă pe formular
''' și fiecare control capătă în grila de proprietăți «Text etichetă pe …», «Titlu etichetă pe …»,
''' «Subsol etichetă pe …». Diferența e că înfățișarea nu mai e a componentei singure:</para>
''' <list type="bullet">
''' <item>fiecare control poate primi <b>stilul lui</b> — <see cref="SetStyleFor"/> — deci două
''' butoane de pe ACELAȘI formular pot avea etichete care arată complet diferit, fără al doilea
''' obiect de tooltip;</item>
''' <item>se pot pune și mai multe componente pe formular, dacă e mai limpede așa (una pentru
''' butoanele de comandă, alta pentru avertizări).</item>
''' </list>
'''
''' <para><b>Controalele desenate de noi</b> (arborele, grila) n-au sub-controale: butoanele lor
''' de antet sunt zone pictate. Pentru ele există <see cref="ShowAt"/> / <see cref="HideNow"/>,
''' pe care controlul le cheamă singur când survolarea intră/iese dintr-o astfel de zonă.</para>
'''
''' <para><b>Temă.</b> Culorile nescrise se rezolvă la FIECARE afișare din
''' <c>ThemeManager.Current</c>, deci o comutare de schemă se vede fără cod în plus. O culoare
''' pusă în designer câștigă și rămâne câștigătoare — regula casei.</para>
''' </summary>
<ProvideProperty("ToolTipText", GetType(Control))>
<ProvideProperty("ToolTipHeader", GetType(Control))>
<ProvideProperty("ToolTipFooter", GetType(Control))>
<ToolboxItemFilter("System.Windows.Forms")>
Public Class KBotToolTip
    Inherits Component
    Implements IExtenderProvider

    ' Ce s-a cerut pentru fiecare control extins. Un control fără intrare aici n-are etichetă.
    Private ReadOnly _texte As New Dictionary(Of Control, KBotToolTipContent)()

    Private ReadOnly _style As New KBotToolTipStyle()
    Private _fereastra As KBotToolTipWindow
    Private ReadOnly _intarziere As New Timer()
    Private _tintaAsteptata As Control
    Private _continutAsteptat As KBotToolTipContent
    Private _pozitieAsteptata As Point
    Private _fontAsteptat As Font

    ' What the label SAYS on screen, and what it has been asked to say — not just which object
    ' carries it. The controls we draw ourselves (the tree, the grid, the chart, the lanes) own ONE
    ' content object and rewrite it before every request, so a guard on the reference alone answers
    ' "already on screen" for the second and every later thing that control shows: the label sticks
    ' on the first one until the pointer leaves the whole control. The fingerprint is what tells
    ' "the same label" apart from "different text in the same object".
    Private _shownFingerprint As String
    Private _pendingFingerprint As String

    Private _active As Boolean = True
    Private _initialDelay As Integer = 500
    Private _autoPopDelay As Integer = 8000

    Public Sub New()
        _style.Owner = Me
        AddHandler _intarziere.Tick, AddressOf IntarziereTick
    End Sub

    Public Sub New(container As IContainer)
        Me.New()
        container?.Add(Me)
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' PROPRIETĂȚI
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Stilul IMPLICIT — cel folosit de orice control care n-a primit unul propriu prin
    ''' <see cref="SetStyleFor"/>.
    ''' </summary>
    <Category("K-BOT Etichetă")>
    <Description("Înfățișarea implicită a etichetei: culori, contur, rotunjire, antet, subsol, linie.")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)>
    Public ReadOnly Property Style As KBotToolTipStyle
        Get
            Return _style
        End Get
    End Property

    ''' <summary>Componenta arată etichete? Stinsă, nimic nu mai apare (util la depanare).</summary>
    <Category("K-BOT Etichetă")>
    <Description("Componenta afișează etichete. Stinsă, nicio etichetă nu mai apare.")>
    <DefaultValue(True)>
    Public Property Active As Boolean
        Get
            Return _active
        End Get
        Set(value As Boolean)
            _active = value
            If Not _active Then HideNow()
        End Set
    End Property

    ''' <summary>Cât stă cursorul pe control înainte să apară eticheta (ms). Implicit 500.</summary>
    <Category("K-BOT Etichetă")>
    <Description("Întârzierea (ms) după care apare eticheta. Implicit 500.")>
    <DefaultValue(500)>
    Public Property InitialDelay As Integer
        Get
            Return _initialDelay
        End Get
        Set(value As Integer)
            _initialDelay = Math.Max(1, value)
        End Set
    End Property

    ''' <summary>
    ''' După cât se stinge singură (ms). <c>0</c> = nu se stinge singură; dispare oricum când
    ''' cursorul părăsește controlul.
    ''' </summary>
    <Category("K-BOT Etichetă")>
    <Description("După cât (ms) se stinge singură eticheta. 0 = doar la ieșirea cursorului.")>
    <DefaultValue(8000)>
    Public Property AutoPopDelay As Integer
        Get
            Return _autoPopDelay
        End Get
        Set(value As Integer)
            _autoPopDelay = Math.Max(0, value)
        End Set
    End Property

    ' ══════════════════════════════════════════════════════════════════════════
    ' EXTENDER — proprietățile care apar pe FIECARE control al formularului
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>Extindem orice control care nu suntem noi înșine.</summary>
    Public Function CanExtend(extendee As Object) As Boolean Implements IExtenderProvider.CanExtend
        Return TypeOf extendee Is Control AndAlso Not ReferenceEquals(extendee, Me)
    End Function

    ''' <summary>Textul din CORPUL etichetei pentru controlul dat. Gol = fără etichetă.</summary>
    <Category("K-BOT Etichetă")>
    <Description("Textul etichetei (mai multe rânduri; acceptă <b>, <i>, <u>, <color=#…>, <back=#…>).")>
    <Editor(GetType(MultilineStringEditor), GetType(UITypeEditor))>
    <DefaultValue("")>
    Public Function GetToolTipText(ctrl As Control) As String
        Dim c As KBotToolTipContent = Lookup(ctrl)
        Return If(c Is Nothing, String.Empty, If(c.Text, String.Empty))
    End Function

    ''' <summary>Scrie textul din corpul etichetei pentru controlul dat.</summary>
    Public Sub SetToolTipText(ctrl As Control, value As String)
        Try
            Dim c As KBotToolTipContent = EnsureEntry(ctrl, Not String.IsNullOrEmpty(value))
            If c Is Nothing Then Return
            c.Text = If(value, String.Empty)
            DropIfEmpty(ctrl, c)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotToolTip.SetToolTipText", ex)
            Throw
        End Try
    End Sub

    ''' <summary>Titlul din antet pentru controlul dat. Gol = cel din stil.</summary>
    <Category("K-BOT Etichetă")>
    <Description("Titlul din antetul etichetei. Gol = titlul din stil.")>
    <DefaultValue("")>
    Public Function GetToolTipHeader(ctrl As Control) As String
        Dim c As KBotToolTipContent = Lookup(ctrl)
        Return If(c Is Nothing, String.Empty, If(c.HeaderText, String.Empty))
    End Function

    ''' <summary>Scrie titlul din antet pentru controlul dat.</summary>
    Public Sub SetToolTipHeader(ctrl As Control, value As String)
        Try
            Dim c As KBotToolTipContent = EnsureEntry(ctrl, Not String.IsNullOrEmpty(value))
            If c Is Nothing Then Return
            c.HeaderText = If(String.IsNullOrEmpty(value), Nothing, value)
            DropIfEmpty(ctrl, c)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotToolTip.SetToolTipHeader", ex)
            Throw
        End Try
    End Sub

    ''' <summary>Textul din subsol pentru controlul dat. Gol = cel din stil.</summary>
    <Category("K-BOT Etichetă")>
    <Description("Textul din subsolul etichetei. Gol = subsolul din stil.")>
    <DefaultValue("")>
    Public Function GetToolTipFooter(ctrl As Control) As String
        Dim c As KBotToolTipContent = Lookup(ctrl)
        Return If(c Is Nothing, String.Empty, If(c.FooterText, String.Empty))
    End Function

    ''' <summary>Scrie textul din subsol pentru controlul dat.</summary>
    Public Sub SetToolTipFooter(ctrl As Control, value As String)
        Try
            Dim c As KBotToolTipContent = EnsureEntry(ctrl, Not String.IsNullOrEmpty(value))
            If c Is Nothing Then Return
            c.FooterText = If(String.IsNullOrEmpty(value), Nothing, value)
            DropIfEmpty(ctrl, c)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotToolTip.SetToolTipFooter", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Pictograma din antet, pentru un control anume. Nu e proprietate de extender (o imagine pe
    ''' control ar umple <c>.resx</c>-ul formularului cu copii ale aceleiași pictograme); se pune
    ''' din cod, de obicei lângă textul etichetei.
    ''' </summary>
    Public Sub SetIconFor(ctrl As Control, icon As Image)
        Try
            Dim c As KBotToolTipContent = EnsureEntry(ctrl, icon IsNot Nothing)
            If c Is Nothing Then Return
            c.HeaderIcon = icon
            DropIfEmpty(ctrl, c)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotToolTip.SetIconFor", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' STILUL propriu al unui control — miezul cerinței «două controale de pe același formular,
    ''' două înfățișări». <c>Nothing</c> îl întoarce la stilul componentei.
    '''
    ''' <para>Se pornește de obicei de la o copie a stilului comun:
    ''' <c>Dim s = tt.Style.Clone() : s.Header.BackColor = … : tt.SetStyleFor(btn, s)</c>.</para>
    ''' </summary>
    Public Sub SetStyleFor(ctrl As Control, style As KBotToolTipStyle)
        Try
            Dim c As KBotToolTipContent = EnsureEntry(ctrl, style IsNot Nothing)
            If c Is Nothing Then Return
            c.Style = style
            DropIfEmpty(ctrl, c)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotToolTip.SetStyleFor", ex)
            Throw
        End Try
    End Sub

    ''' <summary>Stilul cu care s-ar desena eticheta controlului dat (al lui, altfel cel comun).</summary>
    Public Function GetStyleFor(ctrl As Control) As KBotToolTipStyle
        Dim c As KBotToolTipContent = Lookup(ctrl)
        Return If(c IsNot Nothing AndAlso c.Style IsNot Nothing, c.Style, _style)
    End Function

    ' ── Evidența internă ──────────────────────────────────────────────────────

    Private Function Lookup(ctrl As Control) As KBotToolTipContent
        If ctrl Is Nothing Then Return Nothing
        Dim c As KBotToolTipContent = Nothing
        _texte.TryGetValue(ctrl, c)
        Return c
    End Function

    ' Creează intrarea (și abonează survolarea) doar dacă chiar se cere ceva. Fără „create"
    ' n-am abona niciodată un control care primește o valoare goală la încărcarea formularului.
    Private Function EnsureEntry(ctrl As Control, create As Boolean) As KBotToolTipContent
        If ctrl Is Nothing Then Return Nothing
        Dim c As KBotToolTipContent = Lookup(ctrl)
        If c IsNot Nothing Then Return c
        If Not create Then Return Nothing
        c = New KBotToolTipContent()
        _texte(ctrl) = c
        Hook(ctrl)
        Return c
    End Function

    ' O intrare rămasă fără nimic de arătat se scoate, ca să nu ținem controlul viu degeaba
    ' (dicționarul l-ar ține referit după ce formularul lui s-a închis).
    Private Sub DropIfEmpty(ctrl As Control, c As KBotToolTipContent)
        If ctrl Is Nothing OrElse c Is Nothing Then Return
        If c.Style IsNot Nothing Then Return
        If Not c.IsEmpty(_style) Then Return
        If String.IsNullOrEmpty(c.Text) AndAlso c.HeaderText Is Nothing AndAlso
           c.FooterText Is Nothing AndAlso c.HeaderIcon Is Nothing AndAlso c.FooterIcon Is Nothing Then
            Unhook(ctrl)
            _texte.Remove(ctrl)
        End If
    End Sub

    Private Sub Hook(ctrl As Control)
        AddHandler ctrl.MouseEnter, AddressOf ControlMouseEnter
        AddHandler ctrl.MouseLeave, AddressOf ControlMouseLeave
        AddHandler ctrl.MouseDown, AddressOf ControlMouseDown
        AddHandler ctrl.Disposed, AddressOf ControlDisposed
    End Sub

    Private Sub Unhook(ctrl As Control)
        RemoveHandler ctrl.MouseEnter, AddressOf ControlMouseEnter
        RemoveHandler ctrl.MouseLeave, AddressOf ControlMouseLeave
        RemoveHandler ctrl.MouseDown, AddressOf ControlMouseDown
        RemoveHandler ctrl.Disposed, AddressOf ControlDisposed
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' SURVOLAREA controalelor extinse (limite de UI: se loghează și se înghite)
    ' ══════════════════════════════════════════════════════════════════════════

    Private Sub ControlMouseEnter(sender As Object, e As EventArgs)
        Try
            Dim ctrl As Control = TryCast(sender, Control)
            If ctrl Is Nothing Then Return
            Dim c As KBotToolTipContent = Lookup(ctrl)
            If c Is Nothing Then Return
            Schedule(ctrl, c, Cursor.Position, ctrl.Font)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotToolTip.ControlMouseEnter", ex)
        End Try
    End Sub

    Private Sub ControlMouseLeave(sender As Object, e As EventArgs)
        Try
            HideNow()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotToolTip.ControlMouseLeave", ex)
        End Try
    End Sub

    ' Un click înseamnă „am înțeles, fă ce ți-am cerut": eticheta n-are ce căuta peste rezultatul
    ' apăsării.
    Private Sub ControlMouseDown(sender As Object, e As MouseEventArgs)
        Try
            HideNow()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotToolTip.ControlMouseDown", ex)
        End Try
    End Sub

    Private Sub ControlDisposed(sender As Object, e As EventArgs)
        Try
            Dim ctrl As Control = TryCast(sender, Control)
            If ctrl Is Nothing Then Return
            Unhook(ctrl)
            _texte.Remove(ctrl)
            If ReferenceEquals(_tintaAsteptata, ctrl) Then HideNow()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotToolTip.ControlDisposed", ex)
        End Try
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' API pentru CONTROALELE DESENATE (butoane care nu sunt Control)
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Programează o etichetă pentru o ZONĂ dintr-un control desenat de noi: butoanele din
    ''' antetul arborelui sau al grilei, care nu sunt controale și n-au cum să fie extinse.
    ''' Apariția respectă aceeași întârziere ca oriunde.
    ''' </summary>
    ''' <param name="owner">Controlul care găzduiește zona (dă fontul implicit).</param>
    ''' <param name="content">Ce scrie eticheta.</param>
    ''' <param name="screenPos">Poziția (ecran) lângă care se așază.</param>
    Public Sub ShowAt(owner As Control, content As KBotToolTipContent, screenPos As Point)
        Try
            If content Is Nothing Then Return
            Schedule(owner, content, screenPos, If(owner Is Nothing, Nothing, owner.Font))
        Catch ex As Exception
            GlobalErrorLog.Write("KBotToolTip.ShowAt", ex)
            Throw
        End Try
    End Sub

    ''' <summary>Stinge eticheta acum și anulează o apariție programată.</summary>
    Public Sub HideNow()
        Try
            _intarziere.Stop()
            _tintaAsteptata = Nothing
            _continutAsteptat = Nothing
            _fontAsteptat = Nothing
            _shownFingerprint = Nothing
            _pendingFingerprint = Nothing
            _fereastra?.HideTip()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotToolTip.HideNow", ex)
        End Try
    End Sub

    ' ── Programarea apariției ─────────────────────────────────────────────────

    Private Sub Schedule(owner As Control, content As KBotToolTipContent, screenPos As Point, f As Font)
        If Not _active Then Return
        Dim st As KBotToolTipStyle = If(content.Style, _style)
        If content.IsEmpty(st) Then Return

        Dim fingerprint As String = FingerprintOf(content)

        ' Same target, SAME TEXT, already on screen: do not blink it. The text is part of the
        ' condition and not just the object, because the controls we draw ourselves rewrite one
        ' content object before every request — a guard on the reference alone would stick the
        ' label on the first thing that control ever showed.
        If ReferenceEquals(_continutAsteptat, content) AndAlso
           String.Equals(fingerprint, _shownFingerprint, StringComparison.Ordinal) AndAlso
           _fereastra IsNot Nothing AndAlso _fereastra.Visible Then Return

        _tintaAsteptata = owner
        _continutAsteptat = content
        _pozitieAsteptata = screenPos
        _fontAsteptat = f
        _pendingFingerprint = fingerprint
        _intarziere.Stop()

        ' The label is ALREADY open and something else has come under the pointer: it changes now.
        ' A second delay would leave the name of one thing standing over another for half a second,
        ' which is the one thing a label must never do.
        If _fereastra IsNot Nothing AndAlso Not _fereastra.IsDisposed AndAlso _fereastra.Visible Then
            ShowScheduled()
            Return
        End If

        _intarziere.Interval = _initialDelay
        _intarziere.Start()
    End Sub

    Private Sub IntarziereTick(sender As Object, e As EventArgs)
        Try
            _intarziere.Stop()
            ShowScheduled()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotToolTip.IntarziereTick", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Puts the scheduled label on screen, if it still makes sense. One place, reached both from
    ''' the delay's tick and from a swap on a label that is already open.
    ''' </summary>
    Private Sub ShowScheduled()
        If Not _active OrElse _continutAsteptat Is Nothing Then Return

        ' Between the scheduling and now the pointer may have left the control. A label appearing
        ' over a control nobody is on any more is a stray label.
        If _tintaAsteptata IsNot Nothing Then
            If _tintaAsteptata.IsDisposed OrElse Not _tintaAsteptata.Visible Then Return
            If Not _tintaAsteptata.ClientRectangle.Contains(
                    _tintaAsteptata.PointToClient(Cursor.Position)) Then Return
        End If

        Dim st As KBotToolTipStyle = If(_continutAsteptat.Style, _style)
        EnsureWindow().ShowTip(_continutAsteptat, st, _fontAsteptat, Cursor.Position, _autoPopDelay)
        _shownFingerprint = _pendingFingerprint
    End Sub

    ''' <summary>What the label SAYS, as one string — the key that tells it has changed.</summary>
    Private Shared Function FingerprintOf(content As KBotToolTipContent) As String
        If content Is Nothing Then Return Nothing
        Return String.Concat(content.HeaderText, ChrW(1), content.Text, ChrW(1), content.FooterText)
    End Function

    ' O singură fereastră per componentă, creată la prima afișare și moartă odată cu ea: la
    ' design time n-o instanțiem NICIODATĂ (designerul n-are voie să deschidă ferestre).
    Private Function EnsureWindow() As KBotToolTipWindow
        If _fereastra Is Nothing OrElse _fereastra.IsDisposed Then
            _fereastra = New KBotToolTipWindow()
        End If
        Return _fereastra
    End Function

    ' Chemată de obiectul de stil când i s-a schimbat ceva: o etichetă deschisă trebuie refăcută
    ' cu noile valori, nu lăsată să mintă până la următoarea survolare.
    Friend Sub OnStyleChanged()
        Try
            If _fereastra IsNot Nothing AndAlso Not _fereastra.IsDisposed AndAlso _fereastra.Visible Then
                _fereastra.HideTip()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KBotToolTip.OnStyleChanged", ex)
        End Try
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing Then
                _intarziere.Stop()
                _intarziere.Dispose()
                For Each ctrl As Control In New List(Of Control)(_texte.Keys)
                    Unhook(ctrl)
                Next
                _texte.Clear()
                _fereastra?.Dispose()
                _fereastra = Nothing
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KBotToolTip.Dispose", ex)
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

End Class
