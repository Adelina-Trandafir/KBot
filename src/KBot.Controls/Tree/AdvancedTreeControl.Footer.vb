Imports System.ComponentModel
Imports System.Drawing.Drawing2D

''' <summary>
''' SUBSOLUL arborelui — sora benzii de antet (partiala .Header), plus singura piesă fără
''' corespondent sus: BUTONUL DE STRÂNGERE.
'''
''' Strângerea aduce arborele la <see cref="MinimumCollapsedWidth"/> (implicit 100px) exact ca
''' <c>KBotNavList</c>: lățimea desfășurată se ține minte la fiecare redimensionare făcută de
''' altcineva decât noi, ca butonul să aibă unde se întoarce. Din același motiv ca acolo,
''' <c>Collapsed</c> NU se serializează — e stare de rulare, nu valoare de designer.
'''
''' <para><b>Ce vede operatorul cât e strâns.</b> Cu <see cref="CollapsedFlyout"/> (implicit True),
''' hover-ul pe un nod scoate spre dreapta <see cref="TreeNodeFlyout"/>: rândul ÎNTREG, cu iconița
''' la același X și textul îmbogățit desfășurându-se, FĂRĂ ca arborele să se lățească. Fără el, un
''' arbore strâns la 100px e o coloană de text tăiat.</para>
''' </summary>
Partial Public Class AdvancedTreeControl

    ' ══════════════ FOOTER — STARE ══════════════
    Private _collapsed As Boolean = False
    Private _expandedWidth As Integer = 0            ' lățimea la care se întoarce desfășurarea
    Private _applyingCollapseExtent As Boolean = False
    Private _footerButtonHover As Boolean = False

    ' Nodul plutitor al arborelui strâns. Aceleași piese ca la KBotNavList: un cronometru de
    ' așteptare (cât stă cursorul înainte să iasă) și unul de animație (desfășurarea spre dreapta).
    Private Const FlyoutTickMs As Integer = 15
    Private _flyoutEnabled As Boolean = True
    Private _flyoutSelectedNode As Boolean = False    ' nodul selectat NU iese, implicit
    Private _flyoutDelay As Integer = 250
    Private _flyoutSlide As Integer = 120
    Private _flyout As TreeNodeFlyout
    Private _flyoutDelayTimer As Timer
    Private _flyoutAnimTimer As Timer
    Private _flyoutItem As TreeItem = Nothing        ' nodul pentru care iese
    Private _flyoutProgress As Double = 0.0          ' 0 = doar rândul strâns, 1 = rândul întreg
    Private _flyoutFullWidth As Integer = 0          ' măsurat O DATĂ, la fixarea țintei
    Private _flyoutParts As List(Of RichTextPart) = Nothing
    Private _flyoutBaseFont As Font = Nothing        ' fontul derivat (bold/italic de nod), al nostru

    ''' <summary>Evenimentul de strângere/desfășurare — gazda își poate ajusta layout-ul.</summary>
    Public Event CollapsedChanged(collapsed As Boolean)

    ''' <summary>Înălțimea ocupată jos de banda de subsol (0 dacă e ascunsă).</summary>
    Friend ReadOnly Property FooterOffset As Integer
        Get
            Return If(_footerVisible, _footerHeight, 0)
        End Get
    End Property

    ' ══════════════════════════════════════════════════════════════════
    ' FOOTER — DRAWING
    ' ══════════════════════════════════════════════════════════════════
    Friend Sub DrawFooter(g As Graphics)
        Dim bandRect As New Rectangle(0, Math.Max(0, Me.Height - _footerHeight),
                                      Math.Max(1, Me.Width), Math.Max(1, _footerHeight))

        ' Fundal — plin sau în degrade din FooterBackColor spre FooterGradientEndColor.
        If _footerBackStyle = En_HeaderBackStyle.Solid Then
            Using bg As New SolidBrush(FooterBackColor)
                g.FillRectangle(bg, bandRect)
            End Using
        Else
            Dim directie As LinearGradientMode =
                If(_footerBackStyle = En_HeaderBackStyle.GradientHorizontal,
                   LinearGradientMode.Horizontal, LinearGradientMode.Vertical)
            Using bg As New LinearGradientBrush(bandRect, FooterBackColor,
                                                FooterGradientEndColor, directie)
                g.FillRectangle(bg, bandRect)
            End Using
        End If

        ' Separator sus — perechea liniei de sub antet.
        Using sep As New Pen(Color.FromArgb(60, FooterForeColor))
            g.DrawLine(sep, 0, bandRect.Top, Me.Width, bandRect.Top)
        End Using

        Dim midY As Integer = bandRect.Top + (_footerHeight \ 2)

        ' ── Butonul de strângere (stânga sau dreapta) ────────────────────
        ' Dreptunghiul vine din ACEEAȘI funcție pe care o folosește hit-testul — desenul nu-l
        ' „publică" într-un câmp, fiindcă atunci apăsarea ar depinde de o repictare anterioară.
        Dim butonRect As Rectangle = ComputeFooterButtonRect(bandRect)
        If Not butonRect.IsEmpty Then DrawCollapseButton(g, butonRect)

        Dim x As Integer = PADDING_TREE_START
        Dim rx As Integer = Me.Width - PADDING_TREE_END

        If Not butonRect.IsEmpty Then
            If _footerCollapseButtonPosition = En_FooterButtonPosition.Left Then
                x = butonRect.Right + PADDING_ICON_GAP
            Else
                rx = butonRect.Left - PADDING_ICON_GAP
            End If
        End If

        ' ── Iconițele de capăt ───────────────────────────────────────────
        ' Regula e simetrică: butonul de strângere ia latura pe care stă, iar iconița de acolo
        ' se ignoră. Nu se înghesuie două lucruri în același colț — vezi FooterIconSuppressed*.
        If ShowFooterLeftIcon() Then
            Dim iy As Integer = midY - (_footerIconSize.Height \ 2)
            g.DrawImage(_footerLeftIcon, x, iy, _footerIconSize.Width, _footerIconSize.Height)
            x += _footerIconSize.Width + PADDING_ICON_GAP
        End If

        If ShowFooterRightIcon() Then
            Dim r As Rectangle = ComputeFooterRightIconRect(bandRect)
            g.DrawImage(_footerRightIcon, r)
            rx = r.Left - PADDING_ICON_GAP
        End If

        ' ── Caption (text îmbogățit, în spațiul rămas) ───────────────────
        Dim availW As Integer = Math.Max(0, rx - x)
        If String.IsNullOrEmpty(_footerCaption) OrElse availW <= 0 Then Return

        Dim fmt As StringFormat = StringFormat.GenericTypographic
        fmt.FormatFlags = fmt.FormatFlags Or StringFormatFlags.MeasureTrailingSpaces
        Dim parts As List(Of RichTextPart) = ParseRichText(_footerCaption, Me.FooterCaptionFont,
                                                           Me.FooterCaptionForeColor)
        Dim oldClip As Region = g.Clip.Clone()
        g.SetClip(New Rectangle(x, bandRect.Top, availW, _footerHeight))

        Dim latimeTotala As Single = 0
        Dim inaltimeMax As Single = 0
        For Each part In parts
            Dim sz As SizeF = g.MeasureString(part.Text, part.Font, PointF.Empty, fmt)
            latimeTotala += sz.Width
            inaltimeMax = Math.Max(inaltimeMax, sz.Height)
        Next

        Dim cx As Single = AlignStartX(_footerTextAlign, x, availW, latimeTotala)
        Dim cy As Single = bandRect.Top + AlignStartY(_footerTextAlign, _footerHeight, inaltimeMax)

        ' Plaja proprie a etichetei (gol = fără): se desenează O DATĂ, sub tot textul.
        If _footerCaptionBackColor <> Color.Empty Then
            Using b As New SolidBrush(_footerCaptionBackColor)
                g.FillRectangle(b, cx, bandRect.Top, Math.Min(latimeTotala, availW), _footerHeight)
            End Using
        End If

        For Each part In parts
            Dim sz As SizeF = g.MeasureString(part.Text, part.Font, PointF.Empty, fmt)
            If cx + sz.Width > x + availW Then Exit For
            If part.HasBackColor Then
                Using b As New SolidBrush(part.BackColor)
                    g.FillRectangle(b, cx, bandRect.Top, sz.Width, _footerHeight)
                End Using
            End If
            Using b As New SolidBrush(part.ForeColor)
                g.DrawString(part.Text, part.Font, b, cx, cy, fmt)
            End Using
            cx += sz.Width
        Next
        g.Clip = oldClip
    End Sub

    ''' <summary>
    ''' Butonul: pictograma stării curente dacă operatorul a pus una, altfel unghiul desenat
    ''' («‹» strâns → «se desface spre dreapta», «›» desfășurat → «se strânge»).
    ''' </summary>
    Private Sub DrawCollapseButton(g As Graphics, r As Rectangle)
        If _footerButtonHover Then
            Using b As New SolidBrush(Color.FromArgb(40, FooterForeColor))
                Using path As GraphicsPath = GetRoundedRect(Rectangle.Inflate(r, 2, 2), 3)
                    g.FillPath(b, path)
                End Using
            End Using
        End If

        Dim img As Image = If(_collapsed, _footerCollapseCollapsedImage, _footerCollapseExpandedImage)
        If img IsNot Nothing Then
            g.DrawImage(img, r)
            Return
        End If

        Dim oldSmooth As SmoothingMode = g.SmoothingMode
        g.SmoothingMode = SmoothingMode.AntiAlias
        Using pen As New Pen(FooterForeColor, 2.0F)
            pen.StartCap = LineCap.Round
            pen.EndCap = LineCap.Round
            pen.LineJoin = LineJoin.Round
            Dim mx As Single = r.Left + r.Width / 2.0F
            Dim my As Single = r.Top + r.Height / 2.0F
            Dim dx As Single = r.Width / 4.0F
            Dim dy As Single = r.Height / 4.0F
            If _collapsed Then
                g.DrawLines(pen, {New PointF(mx - dx, my - dy), New PointF(mx + dx, my),
                                  New PointF(mx - dx, my + dy)})
            Else
                g.DrawLines(pen, {New PointF(mx + dx, my - dy), New PointF(mx - dx, my),
                                  New PointF(mx + dx, my + dy)})
            End If
        End Using
        g.SmoothingMode = oldSmooth
    End Sub

    ' Dreptunghiul butonului (gol dacă nu e cerut). Ținut ca funcție pură: îl folosesc și desenul,
    ' și hit-testul, și testele — o a doua formulă ar fi un buton care se desenează unde nu se apasă.
    Private Function ComputeFooterButtonRect(bandRect As Rectangle) As Rectangle
        If Not _footerCollapseButton Then Return Rectangle.Empty
        Dim side As Integer = Math.Min(_footerCollapseButtonSize, Math.Max(1, _footerHeight - 4))
        Dim top As Integer = bandRect.Top + (bandRect.Height - side) \ 2
        Dim left As Integer
        If _footerCollapseButtonPosition = En_FooterButtonPosition.Left Then
            left = PADDING_TREE_START
        Else
            left = Math.Max(0, Me.Width - PADDING_TREE_END - side)
        End If
        Return New Rectangle(left, top, side, side)
    End Function

    ''' <summary>
    ''' Butonul de strângere e cerut ȘI stă pe latura dată? Purtătorul regulii simetrice: latura
    ''' pe care stă butonul îi aparține, iar iconița de capăt de acolo nu se mai desenează.
    ''' </summary>
    Private Function ButonPeLatura(latura As En_FooterButtonPosition) As Boolean
        Return _footerCollapseButton AndAlso _footerCollapseButtonPosition = latura
    End Function

    ''' <summary>Iconița din stânga se vede? (există ȘI butonul nu i-a luat colțul)</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property ShowFooterLeftIcon As Boolean
        Get
            Return _footerLeftIcon IsNot Nothing AndAlso Not ButonPeLatura(En_FooterButtonPosition.Left)
        End Get
    End Property

    ''' <summary>Iconița din dreapta se vede? (există ȘI butonul nu i-a luat colțul)</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property ShowFooterRightIcon As Boolean
        Get
            Return _footerRightIcon IsNot Nothing AndAlso Not ButonPeLatura(En_FooterButtonPosition.Right)
        End Get
    End Property

    ' Dreptunghiul iconiței din dreapta. Ca la buton: funcție pură, folosită și de desen și de
    ' hit-test — o a doua formulă ar fi o iconiță care se desenează unde nu se apasă.
    Private Function ComputeFooterRightIconRect(bandRect As Rectangle) As Rectangle
        If Not ShowFooterRightIcon() Then Return Rectangle.Empty
        Dim latime As Integer = _footerIconSize.Width
        Dim inaltime As Integer = _footerIconSize.Height
        Dim top As Integer = bandRect.Top + (bandRect.Height - inaltime) \ 2
        Return New Rectangle(Math.Max(0, Me.Width - PADDING_TREE_END - latime), top, latime, inaltime)
    End Function

    ''' <summary>Dreptunghiul curent al iconiței din dreapta subsolului (gol = nu se vede).</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property FooterRightIconRect As Rectangle
        Get
            If Not _footerVisible Then Return Rectangle.Empty
            Return ComputeFooterRightIconRect(New Rectangle(0, Math.Max(0, Me.Height - _footerHeight),
                                                            Math.Max(1, Me.Width), Math.Max(1, _footerHeight)))
        End Get
    End Property

    ''' <summary>Dreptunghiul curent al butonului de strângere (gol = nu există). Pentru teste/gazdă.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property FooterCollapseButtonRect As Rectangle
        Get
            If Not _footerVisible Then Return Rectangle.Empty
            Return ComputeFooterButtonRect(New Rectangle(0, Math.Max(0, Me.Height - _footerHeight),
                                                         Math.Max(1, Me.Width), Math.Max(1, _footerHeight)))
        End Get
    End Property

    ' Alinierea textului — aceleași două funcții ca la antet, scoase în comun ca subsolul să nu
    ' aibă o a doua interpretare a lui ContentAlignment.
    Friend Shared Function AlignStartX(aliniere As ContentAlignment, stanga As Integer,
                                       disponibil As Integer, latime As Single) As Single
        Select Case aliniere
            Case ContentAlignment.TopCenter, ContentAlignment.MiddleCenter, ContentAlignment.BottomCenter
                Return stanga + Math.Max(0.0F, (disponibil - latime) / 2.0F)
            Case ContentAlignment.TopRight, ContentAlignment.MiddleRight, ContentAlignment.BottomRight
                Return stanga + Math.Max(0.0F, disponibil - latime)
            Case Else
                Return stanga
        End Select
    End Function

    Friend Shared Function AlignStartY(aliniere As ContentAlignment, inaltimeBanda As Integer,
                                       inaltime As Single) As Single
        Select Case aliniere
            Case ContentAlignment.TopLeft, ContentAlignment.TopCenter, ContentAlignment.TopRight
                Return 0.0F
            Case ContentAlignment.BottomLeft, ContentAlignment.BottomCenter, ContentAlignment.BottomRight
                Return Math.Max(0.0F, inaltimeBanda - inaltime)
            Case Else
                Return Math.Max(0.0F, (inaltimeBanda - inaltime) / 2.0F)
        End Select
    End Function

    ' ══════════════════════════════════════════════════════════════════
    ' FOOTER — MOUSE (chemate din partiala .Overrides)
    ' ══════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Click în banda de subsol. Întoarce True dacă subsolul a consumat evenimentul — arborele
    ''' nu mai caută niciun nod sub el.
    ''' </summary>
    Friend Function HandleFooterMouseDown(location As Point, e As MouseEventArgs) As Boolean
        If Not _footerVisible Then Return False
        If location.Y < Me.Height - _footerHeight Then Return False

        Dim butonRect As Rectangle = FooterCollapseButtonRect
        If Not butonRect.IsEmpty AndAlso butonRect.Contains(location) Then
            If Not KBotDesignTime.IsDesignTime(Me) Then ToggleCollapse()
            Return True
        End If

        Dim iconRect As Rectangle = FooterRightIconRect
        If Not iconRect.IsEmpty AndAlso iconRect.Contains(location) Then
            If Not KBotDesignTime.IsDesignTime(Me) Then RaiseEvent FooterRightIconClicked(e)
        End If
        Return True                       ' banda e a subsolului, indiferent unde s-a apăsat
    End Function

    ''' <summary>
    ''' Hover peste subsol și peste nodul plutitor. Întoarce True dacă cursorul e în bandă (deci
    ''' nu e peste niciun nod).
    ''' </summary>
    Friend Function HandleFooterMouseMove(location As Point) As Boolean
        Dim inFooter As Boolean = _footerVisible AndAlso location.Y >= Me.Height - _footerHeight
        Dim butonRect As Rectangle = If(inFooter, FooterCollapseButtonRect, Rectangle.Empty)
        Dim hover As Boolean = Not butonRect.IsEmpty AndAlso butonRect.Contains(location)
        If hover <> _footerButtonHover Then
            _footerButtonHover = hover
            Me.Invalidate()
        End If
        ' Cursorul în bandă = niciun nod survolat, deci nicio etichetă plutitoare.
        UpdateCollapsedFlyout(If(inFooter, Nothing, CollapsedFlyoutTargetAt(location)))
        Return inFooter
    End Function

    Friend Sub HandleFooterMouseLeave()
        If _footerButtonHover Then
            _footerButtonHover = False
            Me.Invalidate()
        End If
        CancelCollapsedFlyout()
    End Sub

    ' ══════════════════════════════════════════════════════════════════
    ' STRÂNGERE
    ' ══════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Comută starea de strângere. Spre deosebire de setterul <see cref="Collapsed"/>, NU aruncă
    ''' dacă butonul lipsește: nu e o cerere de la cod, ci apăsarea unui buton care oricum nu se
    ''' desenează fără <see cref="FooterCollapseButton"/>.
    ''' </summary>
    Public Sub ToggleCollapse()
        Try
            If Not _footerCollapseButton Then Return
            ApplyCollapsedState(Not _collapsed)
        Catch ex As Exception
            GlobalErrorLog.Write("AdvancedTreeControl.ToggleCollapse", ex)
        End Try
    End Sub

    Private Sub ApplyCollapsedState(value As Boolean)
        If value = _collapsed Then Return
        ' Lățimea desfășurată se reține ÎNAINTE de strângere: după, Width e deja cea strânsă.
        If value AndAlso Me.Width > 0 Then _expandedWidth = Me.Width
        _collapsed = value
        If Not _collapsed Then CancelCollapsedFlyout()   ' desfășurat, eticheta n-are rost
        ApplyCollapseExtent()
        RefreshScrollVisibility()
        Me.Invalidate()
        RaiseEvent CollapsedChanged(_collapsed)
    End Sub

    ''' <summary>
    ''' True când NU noi hotărâm lățimea, ci layout-ul gazdei: orice <c>Dock</c> și orice ancorare
    ''' pe amândouă laturile. Într-un asemenea host, o scriere de <c>Width</c> ține exact până la
    ''' următoarea trecere de layout, care o dă înapoi — adică fix ce s-a văzut în <c>MainForm</c>
    ''' (arbore <c>Dock=Fill</c> în <c>split.Panel1</c>): se strângea și se desfăcea instantaneu.
    ''' Acolo strângerea NU se face scriind <c>Width</c>, ci mutând splitter-ul gazdei — de aceea
    ''' există <see cref="CollapsedChanged"/> și <see cref="ExpandedWidth"/>.
    ''' </summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property HostOwnsWidth As Boolean
        Get
            If Me.Dock <> DockStyle.None Then Return True
            Return (Me.Anchor And AnchorStyles.Left) = AnchorStyles.Left AndAlso
                   (Me.Anchor And AnchorStyles.Right) = AnchorStyles.Right
        End Get
    End Property

    ''' <summary>
    ''' Ultima lățime avută DESFĂȘURAT — lățimea la care trebuie readus arborele. O citește gazda
    ''' care-i ține lățimea (vezi <see cref="HostOwnsWidth"/>) ca să știe unde să pună splitter-ul.
    ''' </summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property ExpandedWidth As Integer
        Get
            Return _expandedWidth
        End Get
    End Property

    ''' <summary>
    ''' Aplică lățimea stării curente. Steagul oprește <c>OnResize</c> să confunde lățimea strânsă
    ''' cu «lățimea desfășurată».
    '''
    ''' NU se bate cu layout-ul gazdei: dacă lățimea nu e a noastră (<see cref="HostOwnsWidth"/>),
    ''' nu scriem nimic — starea se schimbă, <see cref="CollapsedChanged"/> se ridică, iar gazda
    ''' își mută splitter-ul. Un <c>Width</c> scris acolo n-ar fi decât o pâlpâire.
    ''' </summary>
    Private Sub ApplyCollapseExtent()
        If HostOwnsWidth Then Return
        Dim target As Integer = If(_collapsed, _minimumCollapsedWidth, _expandedWidth)
        If target <= 0 OrElse target = Me.Width Then Return
        _applyingCollapseExtent = True
        Try
            Me.Width = target
        Finally
            _applyingCollapseExtent = False
        End Try
    End Sub

    ''' <summary>
    ''' Chemată din <c>OnResize</c>: lățimea la care se întoarce butonul = ultima lățime avută
    ''' DESFĂȘURAT. Redimensionările făcute de noi (<c>_applyingCollapseExtent</c>) nu contează —
    ''' altfel prima strângere ar deveni noua «lățime desfășurată» și arborele n-ar mai reveni.
    ''' </summary>
    Friend Sub RememberExpandedWidth()
        If _applyingCollapseExtent OrElse _collapsed Then Return
        If Me.Width > 0 Then _expandedWidth = Me.Width
    End Sub

    ' ══════════════════════════════════════════════════════════════════
    ' NODUL PLUTITOR (arbore strâns)
    ' ══════════════════════════════════════════════════════════════════
    ' Ca la KBotNavList: geometria stă în funcții pure, folosite și la afișare și de teste.
    ' Fereastra (TreeNodeFlyout) e doar randare — decizia «pentru cine, cât de desfășurat, unde»
    ' se ia AICI și rămâne calculabilă fără ecran.

    ''' <summary>
    ''' X-ul de la care începe TEXTUL unui nod — aceeași formulă ca în <c>DrawContent</c>. Nodul
    ''' plutitor trebuie să-l folosească pe ACELAȘI, altfel textul ar sări la stânga/dreapta în
    ''' clipa în care iese fereastra și iluzia de «rând care se desface» s-ar rupe.
    ''' </summary>
    Friend Function NodeTextStartX(it As TreeItem) As Integer
        Dim gridLeft As Integer = (it.Level * Indent) + PADDING_TREE_START
        Dim xBase As Integer = If(it.Level = 0 AndAlso Not _RootExpander,
                                  gridLeft, gridLeft + Indent + PADDING_EXPANDER_GAP)
        If NodeHasCheckControl(it) Then xBase += _checkBoxSize + PADDING_CHECKBOX_GAP
        If it.LeftIconClosed IsNot Nothing AndAlso _hasNodeIcons Then
            Return xBase + LeftIconSize.Width + PADDING_ICON_GAP
        End If
        Return xBase
    End Function

    ''' <summary>Dreptunghiul iconiței din stânga, relativ la rând (gol = nicio iconiță).</summary>
    Friend Function NodeIconRect(it As TreeItem) As Rectangle
        If Not _hasNodeIcons Then Return Rectangle.Empty
        Dim icon As Image = If(it.Expanded, it.LeftIconOpen, it.LeftIconClosed)
        If icon Is Nothing Then icon = If(it.LeftIconClosed, it.LeftIconOpen)
        If icon Is Nothing Then Return Rectangle.Empty
        Dim gridLeft As Integer = (it.Level * Indent) + PADDING_TREE_START
        Dim xBase As Integer = If(it.Level = 0 AndAlso Not _RootExpander,
                                  gridLeft, gridLeft + Indent + PADDING_EXPANDER_GAP)
        If NodeHasCheckControl(it) Then xBase += _checkBoxSize + PADDING_CHECKBOX_GAP
        Return New Rectangle(xBase, (ItemHeight - LeftIconSize.Height) \ 2,
                             LeftIconSize.Width, LeftIconSize.Height)
    End Function

    ''' <summary>
    ''' Pentru ce nod ar trebui să iasă eticheta la un punct dat (Nothing = niciunul). Cere: arbore
    ''' STRÂNS, etichetă activată, nu design time, un nod sub cursor — și ca acel nod să aibă voie
    ''' (vezi <see cref="FlyoutSelectedNode"/>: nodul selectat e sărit, implicit).
    ''' </summary>
    Friend Function CollapsedFlyoutTargetAt(location As Point) As TreeItem
        If Not _flyoutEnabled Then Return Nothing
        If Not _collapsed Then Return Nothing
        If KBotDesignTime.IsDesignTime(Me) Then Return Nothing
        Dim it As TreeItem = HitTestItem(location)
        If FlyoutSuppressedFor(it) Then Return Nothing
        Return it
    End Function

    ''' <summary>
    ''' Nodul ăsta e scutit de etichetă? Doar cel SELECTAT, și doar cât
    ''' <see cref="FlyoutSelectedNode"/> e False. E o suprimare pe UN rând, nu o stingere a
    ''' etichetei: vecinii lui ies în continuare.
    ''' </summary>
    Friend Function FlyoutSuppressedFor(it As TreeItem) As Boolean
        If it Is Nothing Then Return False
        Return Not _flyoutSelectedNode AndAlso it Is pSelectedItem
    End Function

    ''' <summary>
    ''' Retrage eticheta dacă nodul pentru care ieșise tocmai a devenit scutit — cazul obișnuit e
    ''' «am survolat un rând, a ieșit eticheta, am dat clic pe el»: din clipa în care e selectat,
    ''' eticheta n-are ce mai arăta. Chemată din <c>OnMouseDown</c> și din setterul proprietății.
    ''' </summary>
    Friend Sub EnsureCollapsedFlyoutStillAllowed()
        If _flyoutItem Is Nothing Then Return
        If FlyoutSuppressedFor(_flyoutItem) Then CancelCollapsedFlyout()
    End Sub

    ''' <summary>
    ''' Lățimea COMPLETĂ a etichetei: textul măsurat de la <see cref="NodeTextStartX"/> + aer la
    ''' dreapta. Măsurată O SINGURĂ DATĂ, la fixarea țintei — nu la fiecare cadru.
    ''' </summary>
    Private Function ComputeFlyoutFullWidth(it As TreeItem) As Integer
        Dim total As Single = 0
        If _flyoutParts IsNot Nothing Then
            If Me.IsHandleCreated Then
                ' Aceeași măsurătoare ca desenul (GDI+), deci lățimea e exactă.
                Using g As Graphics = Me.CreateGraphics()
                    Dim fmt As StringFormat = StringFormat.GenericTypographic
                    fmt.FormatFlags = fmt.FormatFlags Or StringFormatFlags.MeasureTrailingSpaces
                    For Each part In _flyoutParts
                        total += g.MeasureString(part.Text, part.Font, PointF.Empty, fmt).Width
                    Next
                End Using
            Else
                ' Fără fereastră (bancul de probă, headless) CreateGraphics ar forța un handle
                ' doar ca să măsoare. TextRenderer nu cere niciunul, iar diferența GDI↔GDI+ e de
                ' câțiva px ÎN PLUS — adică joc, nu text tăiat.
                For Each part In _flyoutParts
                    total += TextRenderer.MeasureText(part.Text, part.Font).Width
                Next
            End If
        End If
        Return NodeTextStartX(it) + CInt(Math.Ceiling(total)) + PADDING_TREE_END
    End Function

    ''' <summary>
    ''' Dreptunghiul etichetei în coordonatele CLIENT ale arborelui, la un progres dat (0..1).
    ''' Pleacă EXACT din rândul strâns și crește doar spre dreapta.
    ''' </summary>
    Friend Function FlyoutClientBounds(it As TreeItem, progress As Double) As Rectangle
        If it Is Nothing Then Return Rectangle.Empty
        Dim y As Integer = GetItemY(it)
        If y < 0 Then Return Rectangle.Empty
        Dim t As Double = Math.Max(0.0, Math.Min(1.0, progress))
        Dim w As Integer = Me.Width + CInt(Math.Round((_flyoutFullWidth - Me.Width) * t))
        Return New Rectangle(0, y, Math.Max(Me.Width, w), ItemHeight)
    End Function

    ' Cursorul s-a mutat pe alt nod (sau pe niciunul): reprogramează. Același nod = nu se atinge
    ' nimic, altfel fiecare pixel de mișcare ar reporni temporizarea.
    Private Sub UpdateCollapsedFlyout(target As TreeItem)
        If target Is _flyoutItem Then Return
        CancelCollapsedFlyout()
        If target Is Nothing Then Return

        _flyoutItem = target
        PrepareFlyoutContent(target)
        If _flyoutDelay <= 0 Then
            BeginFlyoutSlide()
            Return
        End If
        EnsureFlyoutTimers()
        _flyoutDelayTimer.Interval = _flyoutDelay
        _flyoutDelayTimer.Start()
    End Sub

    ' Textul îmbogățit + lățimea completă, calculate O DATĂ per nod survolat. Fonturile ies din
    ' ParseRichText ALOCATE, deci sunt ale noastre: le eliberează CancelCollapsedFlyout.
    Private Sub PrepareFlyoutContent(it As TreeItem)
        Dim baseColor As Color = If(it.NodeForeColor <> Color.Empty, it.NodeForeColor,
                                    If(Me.ForeColor <> Color.Empty, Me.ForeColor, Color.Black))
        Dim style As FontStyle = Me.Font.Style
        If it.Bold Then style = style Or FontStyle.Bold
        If it.Italic Then style = style Or FontStyle.Italic
        Dim baseFont As Font = Me.Font
        If style <> Me.Font.Style Then
            _flyoutBaseFont = New Font(Me.Font, style)
            baseFont = _flyoutBaseFont
        End If

        ' Eticheta e exact atât de lată cât îi trebuie, deci n-are zonă dreaptă rezervată:
        ' separatorul «~~~» al rândului devine simplu spațiu.
        Dim caption As String = If(it.Caption, String.Empty).Replace("~~~", "    ")
        _flyoutParts = ParseRichText(caption, baseFont, baseColor)
        _flyoutFullWidth = ComputeFlyoutFullWidth(it)
    End Sub

    ''' <summary>Ascunde eticheta și uită nodul pentru care ieșise. Sigur de chemat oricând.</summary>
    Friend Sub CancelCollapsedFlyout()
        _flyoutDelayTimer?.Stop()
        _flyoutAnimTimer?.Stop()
        _flyoutItem = Nothing
        _flyoutProgress = 0.0
        _flyoutFullWidth = 0
        If _flyoutParts IsNot Nothing Then
            For Each part In _flyoutParts
                part.Font?.Dispose()
            Next
            _flyoutParts = Nothing
        End If
        _flyoutBaseFont?.Dispose()
        _flyoutBaseFont = Nothing
        If _flyout IsNot Nothing AndAlso Not _flyout.IsDisposed AndAlso _flyout.Visible Then _flyout.Hide()
    End Sub

    ' Pornește desfășurarea. Starea (nodul + progresul) avansează INDEPENDENT de existența unei
    ' ferestre — headless și în designer nu se afișează nimic, dar calculul rămâne verificabil.
    Private Sub BeginFlyoutSlide()
        _flyoutProgress = If(_flyoutSlide <= 0, 1.0, 0.0)
        RenderCollapsedFlyout()
        If _flyoutSlide <= 0 Then Return
        EnsureFlyoutTimers()
        _flyoutAnimTimer.Interval = FlyoutTickMs
        _flyoutAnimTimer.Start()
    End Sub

    Private Sub EnsureFlyoutTimers()
        If _flyoutDelayTimer Is Nothing Then
            _flyoutDelayTimer = New Timer()
            AddHandler _flyoutDelayTimer.Tick, AddressOf FlyoutDelayTick
        End If
        If _flyoutAnimTimer Is Nothing Then
            _flyoutAnimTimer = New Timer() With {.Interval = FlyoutTickMs}
            AddHandler _flyoutAnimTimer.Tick, AddressOf FlyoutAnimTick
        End If
    End Sub

    ' Cronometru = graniță de UI: se loghează și se înghite (n-are cui să arunce mai departe).
    Private Sub FlyoutDelayTick(sender As Object, e As EventArgs)
        Try
            _flyoutDelayTimer.Stop()
            If _flyoutItem Is Nothing Then Return
            BeginFlyoutSlide()
        Catch ex As Exception
            GlobalErrorLog.Write("AdvancedTreeControl.FlyoutDelayTick", ex)
        End Try
    End Sub

    Private Sub FlyoutAnimTick(sender As Object, e As EventArgs)
        Try
            AdvanceFlyout()
        Catch ex As Exception
            GlobalErrorLog.Write("AdvancedTreeControl.FlyoutAnimTick", ex)
        End Try
    End Sub

    ' Un pas de desfășurare. Scos din handler ca testele să-l poată chema direct, fără cronometru.
    Private Sub AdvanceFlyout()
        If _flyoutItem Is Nothing Then
            _flyoutAnimTimer?.Stop()
            Return
        End If
        _flyoutProgress += CDbl(FlyoutTickMs) / Math.Max(1, _flyoutSlide)
        If _flyoutProgress >= 1.0 Then
            _flyoutProgress = 1.0
            _flyoutAnimTimer?.Stop()
        End If
        RenderCollapsedFlyout()
    End Sub

    ' Partea care chiar atinge ecranul. Se retrage tăcut când nu există fereastră de arătat
    ' (arbore fără handle / nepus pe un formular / design time) — nu e o eroare, e absența unui ecran.
    Private Sub RenderCollapsedFlyout()
        If _flyoutItem Is Nothing OrElse Not _collapsed Then Return
        ' Selecția se poate schimba și fără mouse (tastatură, cod) cât eticheta se desfășoară.
        If FlyoutSuppressedFor(_flyoutItem) Then
            CancelCollapsedFlyout()
            Return
        End If
        If KBotDesignTime.IsDesignTime(Me) Then Return
        If Not IsHandleCreated OrElse Not Visible Then Return
        Dim host As Form = FindForm()
        If host Is Nothing Then Return

        Dim rect As Rectangle = FlyoutClientBounds(_flyoutItem, _flyoutProgress)
        If rect.IsEmpty Then Return

        If _flyout Is Nothing OrElse _flyout.IsDisposed Then _flyout = New TreeNodeFlyout()
        _flyout.SetContent(BuildFlyoutStyle(_flyoutItem))
        _flyout.Bounds = RectangleToScreen(rect)
        If Not _flyout.Visible Then _flyout.Show(host)
    End Sub

    ''' <summary>
    ''' Culorile/măsurile rândului, gata calculate. Aceleași reguli ca în <c>DrawSelection</c> —
    ''' selectat = fundal de selecție + contur de selecție, restul = fundal de hover (nodul E
    ''' survolat, prin definiție).
    ''' </summary>
    Private Function BuildFlyoutStyle(it As TreeItem) As TreeNodeFlyoutStyle
        Dim selectat As Boolean = (it Is pSelectedItem)
        Dim icon As Image = Nothing
        If _hasNodeIcons Then
            icon = If(it.Expanded, it.LeftIconOpen, it.LeftIconClosed)
            If icon Is Nothing Then icon = If(it.LeftIconClosed, it.LeftIconOpen)
        End If
        Return New TreeNodeFlyoutStyle With {
            .Fill = If(selectat, SelectedBackColor, HoverBackColor),
            .Border = If(selectat, SelectedBorderColor, LineColor),
            .Radius = SELECTION_CORNER_RADIUS,
            .ItemHeight = ItemHeight,
            .IconRect = NodeIconRect(it),
            .Icon = icon,
            .TextX = NodeTextStartX(it),
            .Parts = _flyoutParts}
    End Function

    ' ── Cârlige de test (headless): starea etichetei fără ecran ──────────────────
    Friend Function DebugFlyoutItem() As TreeItem
        Return _flyoutItem
    End Function

    Friend Function DebugFlyoutProgress() As Double
        Return _flyoutProgress
    End Function

    Friend Function DebugFlyoutFullWidth() As Integer
        Return _flyoutFullWidth
    End Function

    Friend Sub DebugFlyoutFireDelay()
        FlyoutDelayTick(Nothing, EventArgs.Empty)
    End Sub

    Friend Sub DebugFlyoutTick()
        AdvanceFlyout()
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then
            CancelCollapsedFlyout()
            _flyoutDelayTimer?.Dispose()
            _flyoutDelayTimer = Nothing
            _flyoutAnimTimer?.Dispose()
            _flyoutAnimTimer = Nothing
            _flyout?.Dispose()
            _flyout = Nothing
        End If
        MyBase.Dispose(disposing)
    End Sub
End Class
