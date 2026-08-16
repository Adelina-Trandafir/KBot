Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' BUTONUL DE STRÂNGERE al <see cref="KBotDataView"/> (slice 0028) — a treia apariție a
''' aceleiași piese, după <c>KBotNavList</c> și subsolul lui <c>AdvancedTreeControl</c>, cu
''' aceleași nume de proprietăți și aceleași reguli, ca operatorul să nu învețe trei contracte.
'''
''' <para><b>Ce e diferit față de surori.</b> O grilă andocată <c>Fill</c> într-o vedere nu câștigă
''' nimic dintr-o fâșie verticală de 100px, deci strângerea are aici o AXĂ
''' (<see cref="CollapseDirection"/>): pe orizontală se poartă exact ca arborele, pe verticală
''' pliază corpul și lasă antetul + subsolul — adică exact agregatele, care sunt lucrul pe care
''' vrei să-l vezi când ai închis lista.</para>
'''
''' <para><b>Regula gazdei</b> se păstrează întocmai: dacă lățimea (sau înălțimea) nu e a noastră
''' — <c>Dock</c>, ori ancorare pe amândouă laturile — NU scriem dimensiunea. Schimbăm starea și
''' ridicăm <see cref="CollapsedChanged"/>, iar gazda își mută splitter-ul. O scriere de
''' <c>Width</c> într-un părinte care face layout ține până la următoarea trecere, care o dă
''' înapoi: pâlpâie și nimic mai mult (vezi <c>MainForm.tree_CollapsedChanged</c>).</para>
''' </summary>
Partial Class KBotDataView

    ' ── Stare ───────────────────────────────────────────────────────────────────
    Private _collapsed As Boolean = False
    Private _collapseButton As Boolean = False
    Private _collapseButtonSize As Integer = 16
    Private _collapseButtonPosition As KBotFooterButtonPosition = KBotFooterButtonPosition.Right
    Private _collapseDirection As KBotCollapseDirection = KBotCollapseDirection.Horizontal
    Private _minimumCollapsedWidth As Integer = 100
    Private _collapseImageExpanded As Image = Nothing
    Private _collapseImageCollapsed As Image = Nothing
    Private _collapseButtonHover As Boolean = False

    ' Dimensiunile la care se întoarce desfășurarea. Se rețin la fiecare redimensionare făcută
    ' de ALTCINEVA decât noi (vezi RememberExpandedExtent).
    Private _expandedWidth As Integer = 0
    Private _expandedHeight As Integer = 0
    Private _applyingCollapseExtent As Boolean = False

    ''' <summary>Strângerea s-a schimbat — gazda își poate ajusta layout-ul (splitter etc.).</summary>
    Public Event CollapsedChanged(collapsed As Boolean)

    ' ══════════════════════════════════════════════════════════════════════════
    ' PROPRIETĂȚI
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Afișează în subsol butonul care strânge/desfășoară grila. Implicit False. Butonul se
    ''' desenează în banda de subsol, deci are nevoie de <see cref="FooterVisible"/>.
    ''' </summary>
    <Category("K-BOT: Collapse")>
    <Description("Afișează în subsol butonul care strânge/desfășoară grila. Are nevoie de FooterVisible.")>
    <DefaultValue(False)>
    Public Property CollapseButton As Boolean
        Get
            Return _collapseButton
        End Get
        Set(value As Boolean)
            If _collapseButton = value Then Return
            _collapseButton = value
            ' Fără buton nu mai există cale de întoarcere din starea strânsă: o desfacem noi,
            ' altfel grila ar rămâne pliată pentru totdeauna (aceeași grijă ca la NavList/arbore).
            If Not _collapseButton AndAlso _collapsed Then ApplyCollapsedState(False)
            Invalidate()
        End Set
    End Property

    ''' <summary>Latura (px) a butonului de strângere. Implicit 16.</summary>
    <Category("K-BOT: Collapse")>
    <Description("Latura (px) a butonului de strângere din subsol.")>
    <DefaultValue(16)>
    Public Property CollapseButtonSize As Integer
        Get
            Return _collapseButtonSize
        End Get
        Set(value As Integer)
            _collapseButtonSize = Math.Max(8, value)
            Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' Latura pe care stă butonul. Colțul lui e AL LUI: textul agregatelor se decupează înaintea
    ''' butonului, ca o sumă lungă să nu curgă pe sub el.
    ''' </summary>
    <Category("K-BOT: Collapse")>
    <Description("Latura pe care stă butonul de strângere. Colțul lui nu se împarte cu textul agregatelor.")>
    <DefaultValue(KBotFooterButtonPosition.Right)>
    Public Property CollapseButtonPosition As KBotFooterButtonPosition
        Get
            Return _collapseButtonPosition
        End Get
        Set(value As KBotFooterButtonPosition)
            _collapseButtonPosition = value
            Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' Axa pe care se strânge grila. Implicit <see cref="KBotCollapseDirection.Horizontal"/>,
    ''' adică exact ce fac arborele și bara de navigare.
    ''' </summary>
    <Category("K-BOT: Collapse")>
    <Description("Axa strângerii: pe lățime (ca arborele) sau pe înălțime (rămân antetul + subsolul).")>
    <DefaultValue(KBotCollapseDirection.Horizontal)>
    Public Property CollapseDirection As KBotCollapseDirection
        Get
            Return _collapseDirection
        End Get
        Set(value As KBotCollapseDirection)
            If _collapseDirection = value Then Return
            ' Schimbarea axei cât timp grila e STRÂNSĂ ar lăsa-o pliată pe axa veche și îngustă
            ' pe cea nouă: o desfacem întâi, apoi schimbăm axa. (Nu e un no-op tăcut — starea
            ' cerută se vede pe loc, iar CollapsedChanged spune gazdei ce s-a întâmplat.)
            Dim eraStransa As Boolean = _collapsed
            If eraStransa Then ApplyCollapsedState(False)
            _collapseDirection = value
            If eraStransa Then ApplyCollapsedState(True)
            Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' Lățimea (px) la care se strânge grila pe axa orizontală. Implicit 100 — aceeași valoare
    ''' ca la arbore. Nu are efect pe axa verticală, unde înălțimea strânsă e chiar suma benzilor.
    ''' </summary>
    <Category("K-BOT: Collapse")>
    <Description("Lățimea (px) la care se strânge grila pe orizontală. Implicit 100.")>
    <DefaultValue(100)>
    Public Property MinimumCollapsedWidth As Integer
        Get
            Return _minimumCollapsedWidth
        End Get
        Set(value As Integer)
            _minimumCollapsedWidth = Math.Max(16, value)
            If _collapsed Then ApplyCollapseExtent()
            Invalidate()
        End Set
    End Property

    ''' <summary>Pictograma butonului cât timp grila e DESFĂȘURATĂ; nesetată = unghi desenat.</summary>
    <Category("K-BOT: Collapse")>
    <Description("Pictograma butonului cât timp grila e desfășurată; nesetată = unghiul desenat.")>
    <DefaultValue(GetType(Image), Nothing)>
    Public Property CollapseExpandedImage As Image
        Get
            Return _collapseImageExpanded
        End Get
        Set(value As Image)
            If value Is _collapseImageExpanded Then Return
            _collapseImageExpanded = value
            Invalidate()
        End Set
    End Property

    Private Function ShouldSerializeCollapseExpandedImage() As Boolean
        Return _collapseImageExpanded IsNot Nothing
    End Function

    Private Sub ResetCollapseExpandedImage()
        CollapseExpandedImage = Nothing
    End Sub

    ''' <summary>Pictograma butonului cât timp grila e STRÂNSĂ; nesetată = unghi desenat.</summary>
    <Category("K-BOT: Collapse")>
    <Description("Pictograma butonului cât timp grila e strânsă; nesetată = unghiul desenat.")>
    <DefaultValue(GetType(Image), Nothing)>
    Public Property CollapseCollapsedImage As Image
        Get
            Return _collapseImageCollapsed
        End Get
        Set(value As Image)
            If value Is _collapseImageCollapsed Then Return
            _collapseImageCollapsed = value
            Invalidate()
        End Set
    End Property

    Private Function ShouldSerializeCollapseCollapsedImage() As Boolean
        Return _collapseImageCollapsed IsNot Nothing
    End Function

    Private Sub ResetCollapseCollapsedImage()
        CollapseCollapsedImage = Nothing
    End Sub

    ''' <summary>
    ''' Starea de strângere. STARE DE RULARE, nu valoare de designer (ca la arbore și la NavList):
    ''' serializată, ar îngheța formularul strâns și s-ar bate cu <c>Size</c>-ul scris tot de
    ''' designer. Setarea pe True fără buton ARUNCĂ — n-ar mai exista cale de întoarcere.
    ''' </summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Collapsed As Boolean
        Get
            Return _collapsed
        End Get
        Set(value As Boolean)
            If value = _collapsed Then Return
            If value AndAlso Not _collapseButton Then
                Throw New InvalidOperationException(
                    "Grila nu se poate strânge cât timp CollapseButton e False.")
            End If
            ApplyCollapsedState(value)
        End Set
    End Property

    ''' <summary>
    ''' Comută starea. Spre deosebire de setterul <see cref="Collapsed"/>, NU aruncă dacă butonul
    ''' lipsește: nu e o cerere din cod, ci apăsarea unui buton care oricum nu se desenează.
    ''' </summary>
    Public Sub ToggleCollapse()
        Try
            If Not _collapseButton Then Return
            ApplyCollapsedState(Not _collapsed)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.ToggleCollapse", ex)
        End Try
    End Sub

    ''' <summary>
    ''' True când NU noi hotărâm lățimea, ci layout-ul gazdei: orice <c>Dock</c> și orice ancorare
    ''' pe amândouă laturile pe orizontală.
    ''' </summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property HostOwnsWidth As Boolean
        Get
            If Dock <> DockStyle.None Then Return True
            Return (Anchor And AnchorStyles.Left) = AnchorStyles.Left AndAlso
                   (Anchor And AnchorStyles.Right) = AnchorStyles.Right
        End Get
    End Property

    ''' <summary>Perechea verticală a lui <see cref="HostOwnsWidth"/> (Dock, ori Top+Bottom).</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property HostOwnsHeight As Boolean
        Get
            If Dock <> DockStyle.None Then Return True
            Return (Anchor And AnchorStyles.Top) = AnchorStyles.Top AndAlso
                   (Anchor And AnchorStyles.Bottom) = AnchorStyles.Bottom
        End Get
    End Property

    ''' <summary>Ultima lățime avută DESFĂȘURAT — o citește gazda care ține lățimea.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property ExpandedWidth As Integer
        Get
            Return _expandedWidth
        End Get
    End Property

    ''' <summary>Ultima înălțime avută DESFĂȘURAT — perechea verticală a lui <see cref="ExpandedWidth"/>.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property ExpandedHeight As Integer
        Get
            Return _expandedHeight
        End Get
    End Property

    ''' <summary>
    ''' Înălțimea grilei strânse pe verticală: exact cele două benzi. O citește gazda care ține
    ''' înălțimea, ca să știe la cât să tragă splitter-ul.
    ''' </summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property CollapsedHeight As Integer
        Get
            Return Math.Max(1, HeaderBandHeight() + FooterBandHeight())
        End Get
    End Property

    ''' <summary>Corpul e pliat? (strâns ȘI pe axa verticală) — poarta pictării și a geometriei.</summary>
    Friend Function BodyIsCollapsed() As Boolean
        Return _collapsed AndAlso _collapseDirection = KBotCollapseDirection.Vertical
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' STRÂNGERE
    ' ══════════════════════════════════════════════════════════════════════════

    Private Sub ApplyCollapsedState(value As Boolean)
        If value = _collapsed Then Return
        ' Dimensiunea desfășurată se reține ÎNAINTE de strângere: după, e deja cea strânsă.
        If value Then RememberExpandedExtent()
        _collapsed = value
        ApplyCollapseExtent()
        ' Corpul apare/dispare (axa verticală), deci barele și geometria se refac.
        UpdateLayout()
        Invalidate()
        RaiseEvent CollapsedChanged(_collapsed)
    End Sub

    ''' <summary>
    ''' Aplică dimensiunea stării curente pe axa aleasă. Steagul oprește <c>OnResize</c> să
    ''' confunde dimensiunea strânsă cu «cea desfășurată».
    '''
    ''' NU se bate cu layout-ul gazdei: dacă dimensiunea de pe axa aceea nu e a noastră, nu
    ''' scriem nimic — starea se schimbă, <see cref="CollapsedChanged"/> se ridică, iar gazda își
    ''' mută splitter-ul.
    ''' </summary>
    Private Sub ApplyCollapseExtent()
        If _collapseDirection = KBotCollapseDirection.Horizontal Then
            If HostOwnsWidth Then Return
            Dim target As Integer = If(_collapsed, _minimumCollapsedWidth, _expandedWidth)
            If target <= 0 OrElse target = Width Then Return
            _applyingCollapseExtent = True
            Try
                Width = target
            Finally
                _applyingCollapseExtent = False
            End Try
        Else
            If HostOwnsHeight Then Return
            Dim target As Integer = If(_collapsed, CollapsedHeight, _expandedHeight)
            If target <= 0 OrElse target = Height Then Return
            _applyingCollapseExtent = True
            Try
                Height = target
            Finally
                _applyingCollapseExtent = False
            End Try
        End If
    End Sub

    ''' <summary>
    ''' Chemată din <c>OnResize</c>: dimensiunea la care se întoarce butonul = ultima avută
    ''' DESFĂȘURAT. Redimensionările făcute de noi nu contează — altfel prima strângere ar deveni
    ''' noua «dimensiune desfășurată» și grila n-ar mai reveni niciodată.
    ''' </summary>
    Friend Sub RememberExpandedExtent()
        If _applyingCollapseExtent OrElse _collapsed Then Return
        If Width > 0 Then _expandedWidth = Width
        If Height > 0 Then _expandedHeight = Height
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' GEOMETRIE + PICTARE
    ' ══════════════════════════════════════════════════════════════════════════

    ' Dreptunghiul butonului (gol dacă nu e cerut). Funcție PURĂ, folosită și de desen, și de
    ' hit-test, și de teste — o a doua formulă ar fi un buton care se desenează unde nu se apasă.
    Private Function ComputeCollapseButtonRect(bandRect As Rectangle) As Rectangle
        If Not _collapseButton OrElse bandRect.Height <= 0 Then Return Rectangle.Empty
        Dim side As Integer = Math.Min(ScaleDpi(_collapseButtonSize), Math.Max(1, bandRect.Height - 4))
        Dim top As Integer = bandRect.Top + (bandRect.Height - side) \ 2
        Dim marja As Integer = ScaleDpi(4)
        Dim left As Integer
        If _collapseButtonPosition = KBotFooterButtonPosition.Left Then
            left = marja
        Else
            left = Math.Max(0, bandRect.Right - marja - side)
        End If
        Return New Rectangle(left, top, side, side)
    End Function

    ''' <summary>Dreptunghiul curent al butonului de strângere (gol = nu se vede). Pentru gazdă/teste.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property CollapseButtonRect As Rectangle
        Get
            If Not _showFooter OrElse FooterBandHeight() <= 0 Then Return Rectangle.Empty
            Return ComputeCollapseButtonRect(CurrentFooterBandRect())
        End Get
    End Property

    ' Banda de subsol, în coordonate client.
    Private Function CurrentFooterBandRect() As Rectangle
        Return New Rectangle(0, FooterBandTop(), Math.Max(1, ClientSize.Width),
                             Math.Max(1, FooterBandHeight()))
    End Function

    ''' <summary>
    ''' Partea din bandă în care are voie să se scrie: banda minus colțul butonului. Latura pe
    ''' care stă butonul îi aparține — aceeași regulă ca la subsolul arborelui.
    ''' </summary>
    Friend Function FooterContentRect(bandRect As Rectangle) As Rectangle
        Dim buton As Rectangle = ComputeCollapseButtonRect(bandRect)
        If buton.IsEmpty Then Return bandRect
        Dim gap As Integer = ScaleDpi(4)
        If _collapseButtonPosition = KBotFooterButtonPosition.Left Then
            Dim stanga As Integer = buton.Right + gap
            Return New Rectangle(stanga, bandRect.Top, Math.Max(0, bandRect.Right - stanga), bandRect.Height)
        End If
        Dim dreapta As Integer = buton.Left - gap
        Return New Rectangle(bandRect.Left, bandRect.Top, Math.Max(0, dreapta - bandRect.Left), bandRect.Height)
    End Function

    ''' <summary>
    ''' Butonul: pictograma stării curente dacă operatorul a pus una, altfel unghiul desenat.
    ''' Unghiul arată ÎNCOTRO se duce grila, deci se rotește cu axa: «‹»/«›» pe orizontală,
    ''' «˄»/«˅» pe verticală.
    ''' </summary>
    Private Sub DrawCollapseButton(g As Graphics, r As Rectangle)
        Dim fore As Color = FooterForeResolved()

        If _collapseButtonHover Then
            Using b As New SolidBrush(Color.FromArgb(40, fore))
                Using path As GraphicsPath = RoundedRect(Rectangle.Inflate(r, 2, 2), ScaleDpi(3))
                    g.FillPath(b, path)
                End Using
            End Using
        End If

        Dim img As Image = If(_collapsed, _collapseImageCollapsed, _collapseImageExpanded)
        If img IsNot Nothing Then
            g.DrawImage(img, r)
            Return
        End If

        Dim oldSmooth As SmoothingMode = g.SmoothingMode
        g.SmoothingMode = SmoothingMode.AntiAlias
        Using pen As New Pen(fore, 2.0F)
            pen.StartCap = LineCap.Round
            pen.EndCap = LineCap.Round
            pen.LineJoin = LineJoin.Round
            Dim mx As Single = r.Left + r.Width / 2.0F
            Dim my As Single = r.Top + r.Height / 2.0F
            Dim dx As Single = r.Width / 4.0F
            Dim dy As Single = r.Height / 4.0F
            If _collapseDirection = KBotCollapseDirection.Horizontal Then
                If _collapsed Then
                    g.DrawLines(pen, {New PointF(mx - dx, my - dy), New PointF(mx + dx, my),
                                      New PointF(mx - dx, my + dy)})
                Else
                    g.DrawLines(pen, {New PointF(mx + dx, my - dy), New PointF(mx - dx, my),
                                      New PointF(mx + dx, my + dy)})
                End If
            Else
                If _collapsed Then
                    ' Strâns pe verticală: unghiul arată în JOS — «se desface înapoi la loc».
                    g.DrawLines(pen, {New PointF(mx - dx, my - dy), New PointF(mx, my + dy),
                                      New PointF(mx + dx, my - dy)})
                Else
                    g.DrawLines(pen, {New PointF(mx - dx, my + dy), New PointF(mx, my - dy),
                                      New PointF(mx + dx, my + dy)})
                End If
            End If
        End Using
        g.SmoothingMode = oldSmooth
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' MOUSE (chemat din partiala .Input)
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Apăsare în banda de subsol. Întoarce True dacă banda a consumat evenimentul — grila nu mai
    ''' caută nicio celulă sub el (subsolul nu e un rând).
    ''' </summary>
    Friend Function HandleFooterMouseDown(location As Point) As Boolean
        If Not _showFooter OrElse FooterBandHeight() <= 0 Then Return False
        If location.Y < FooterBandTop() Then Return False

        Dim buton As Rectangle = CollapseButtonRect
        If Not buton.IsEmpty AndAlso buton.Contains(location) Then
            If Not KBotDesignTime.IsDesignTime(Me) Then ToggleCollapse()
            Return True
        End If

        ' Pictograma titlului din subsol (slice 0028-02) — cealaltă piesă apăsabilă din bandă.
        HandleFooterIconMouseDown(location)
        Return True                       ' banda e a subsolului, oriunde s-ar fi apăsat
    End Function

    ''' <summary>Hover peste buton. Întoarce True dacă a cursorul e în banda de subsol.</summary>
    Friend Function HandleFooterMouseMove(location As Point) As Boolean
        Dim inFooter As Boolean = _showFooter AndAlso FooterBandHeight() > 0 AndAlso
                                  location.Y >= FooterBandTop()
        Dim buton As Rectangle = If(inFooter, CollapseButtonRect, Rectangle.Empty)
        Dim hover As Boolean = Not buton.IsEmpty AndAlso buton.Contains(location)
        If hover <> _collapseButtonHover Then
            _collapseButtonHover = hover
            RefreshCollapseTip(hover)   ' felia 0035: butonul își spune la ce folosește
            Invalidate()
        End If
        If inFooter Then UpdateFooterIconHover(location) Else ClearFooterIconHover()
        Return inFooter
    End Function

    Friend Sub HandleFooterMouseLeave()
        ClearFooterIconHover()
        HideButtonTip()
        If Not _collapseButtonHover Then Return
        _collapseButtonHover = False
        Invalidate()
    End Sub

End Class
