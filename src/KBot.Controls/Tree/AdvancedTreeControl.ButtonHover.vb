Imports System.ComponentModel
Imports System.Drawing.Drawing2D

''' <summary>
''' SURVOLAREA BUTOANELOR arborelui, într-un singur fișier — culorile ȘI starea de hover.
'''
''' «Buton» = orice zonă apăsabilă care ridică ceva: iconița de căutare și cea din dreapta
''' antetului, iconița din dreapta subsolului, butonul de strângere și iconița din dreapta unui
''' nod. Până acum doar butonul de strângere avea o reacție la survolare, și aia scrisă în cod
''' (<c>Color.FromArgb(40, FooterForeColor)</c>); restul nu dădeau niciun semn că se pot apăsa.
'''
''' CONTRACTUL DE CULOARE e cel al restului arborelui: <c>Color.Empty</c> = «auto», adică plaja
''' translucidă calculată din culoarea de prim-plan a benzii pe care stă butonul. O culoare aleasă
''' în designer câștigă. Perechea ShouldSerialize/Reset ține designerul să nu scrie o linie pentru
''' o culoare pe care n-a ales-o nimeni.
'''
''' Culoarea se desenează ca o PLAJĂ sub pictogramă (dreptunghi rotunjit), nu ca o tentă peste ea:
''' pictogramele sunt ale operatorului și n-avem voie să le schimbăm culorile.
''' </summary>
Partial Public Class AdvancedTreeControl

    ' Cât se umflă plaja față de pictogramă și cât de rotunjită e. Aceleași valori pe toate
    ' butoanele — un buton care se aprinde altfel decât vecinul lui arată ca o scăpare.
    Private Const BUTTON_HOVER_INFLATE As Integer = 2
    Private Const BUTTON_HOVER_RADIUS As Integer = 3

    ' Cât de opacă e plaja «auto» peste banda de dedesubt.
    Private Const BUTTON_HOVER_AUTO_ALPHA As Integer = 40

    ' ── Starea de survolare (cine e sub cursor acum) ──────────────────────────
    Private _headerSearchIconHover As Boolean = False
    Private _headerRightIconHover As Boolean = False
    Private _footerRightIconHover As Boolean = False
    Private _footerLeftIconHover As Boolean = False
    Private _nodeRightIconHover As Boolean = False

    ' ── Culorile ──────────────────────────────────────────────────────────────
    Private _headerSearchIconHoverColor As Color = Color.Empty
    Private _headerRightIconHoverColor As Color = Color.Empty
    Private _footerRightIconHoverColor As Color = Color.Empty
    Private _footerLeftIconHoverColor As Color = Color.Empty
    Private _footerCollapseButtonHoverColor As Color = Color.Empty
    Private _nodeRightIconHoverColor As Color = Color.Empty
    Private _searchClearButtonHoverColor As Color = Color.Empty

    ''' <summary>Plaja «auto»: culoarea de prim-plan a benzii, translucidă.</summary>
    Private Shared Function AutoHover(foreColor As Color) As Color
        Return Color.FromArgb(BUTTON_HOVER_AUTO_ALPHA, foreColor)
    End Function

    <Category("K-BOT: Buttons")>
    <Description("Fundalul iconiței de căutare din antet, la survolare; gol = automat din HeaderForeColor.")>
    Public Property HeaderSearchIconHoverColor As Color
        Get
            Return If(_headerSearchIconHoverColor <> Color.Empty,
                      _headerSearchIconHoverColor, AutoHover(HeaderForeColor))
        End Get
        Set(value As Color)
            _headerSearchIconHoverColor = value
            Me.Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeHeaderSearchIconHoverColor() As Boolean
        Return _headerSearchIconHoverColor <> Color.Empty
    End Function
    Public Sub ResetHeaderSearchIconHoverColor()
        _headerSearchIconHoverColor = Color.Empty
        Me.Invalidate()
    End Sub

    <Category("K-BOT: Buttons")>
    <Description("Fundalul iconiței din dreapta antetului, la survolare; gol = automat din HeaderForeColor.")>
    Public Property HeaderRightIconHoverColor As Color
        Get
            Return If(_headerRightIconHoverColor <> Color.Empty,
                      _headerRightIconHoverColor, AutoHover(HeaderForeColor))
        End Get
        Set(value As Color)
            _headerRightIconHoverColor = value
            Me.Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeHeaderRightIconHoverColor() As Boolean
        Return _headerRightIconHoverColor <> Color.Empty
    End Function
    Public Sub ResetHeaderRightIconHoverColor()
        _headerRightIconHoverColor = Color.Empty
        Me.Invalidate()
    End Sub

    <Category("K-BOT: Buttons")>
    <Description("Fundalul iconiței din dreapta subsolului, la survolare; gol = automat din FooterForeColor.")>
    Public Property FooterRightIconHoverColor As Color
        Get
            Return If(_footerRightIconHoverColor <> Color.Empty,
                      _footerRightIconHoverColor, AutoHover(FooterForeColor))
        End Get
        Set(value As Color)
            _footerRightIconHoverColor = value
            Me.Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeFooterRightIconHoverColor() As Boolean
        Return _footerRightIconHoverColor <> Color.Empty
    End Function
    Public Sub ResetFooterRightIconHoverColor()
        _footerRightIconHoverColor = Color.Empty
        Me.Invalidate()
    End Sub

    <Category("K-BOT: Buttons")>
    <Description("Fundalul iconiței din stânga subsolului, la survolare; gol = automat din FooterForeColor.")>
    Public Property FooterLeftIconHoverColor As Color
        Get
            Return If(_footerLeftIconHoverColor <> Color.Empty,
                      _footerLeftIconHoverColor, AutoHover(FooterForeColor))
        End Get
        Set(value As Color)
            _footerLeftIconHoverColor = value
            Me.Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeFooterLeftIconHoverColor() As Boolean
        Return _footerLeftIconHoverColor <> Color.Empty
    End Function
    Public Sub ResetFooterLeftIconHoverColor()
        _footerLeftIconHoverColor = Color.Empty
        Me.Invalidate()
    End Sub

    <Category("K-BOT: Buttons")>
    <Description("Fundalul butonului de strângere, la survolare; gol = automat din FooterForeColor.")>
    Public Property FooterCollapseButtonHoverColor As Color
        Get
            Return If(_footerCollapseButtonHoverColor <> Color.Empty,
                      _footerCollapseButtonHoverColor, AutoHover(FooterForeColor))
        End Get
        Set(value As Color)
            _footerCollapseButtonHoverColor = value
            Me.Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeFooterCollapseButtonHoverColor() As Boolean
        Return _footerCollapseButtonHoverColor <> Color.Empty
    End Function
    Public Sub ResetFooterCollapseButtonHoverColor()
        _footerCollapseButtonHoverColor = Color.Empty
        Me.Invalidate()
    End Sub

    <Category("K-BOT: Buttons")>
    <Description("Fundalul iconiței din dreapta unui nod, la survolarea ei; gol = automat din ForeColor.")>
    Public Property NodeRightIconHoverColor As Color
        Get
            Return If(_nodeRightIconHoverColor <> Color.Empty,
                      _nodeRightIconHoverColor, AutoHover(Me.ForeColor))
        End Get
        Set(value As Color)
            _nodeRightIconHoverColor = value
            Me.Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeNodeRightIconHoverColor() As Boolean
        Return _nodeRightIconHoverColor <> Color.Empty
    End Function
    Public Sub ResetNodeRightIconHoverColor()
        _nodeRightIconHoverColor = Color.Empty
        Me.Invalidate()
    End Sub

    ''' <summary>
    ''' Butonul ✕ al benzii de căutare e singurul buton care e un CONTROL copil (un
    ''' <see cref="Label"/>), nu o zonă desenată de noi. Un Label n-are stare de survolare, deci
    ''' i-o dăm noi din MouseEnter/MouseLeave, schimbându-i fundalul. Gol = fundalul casetei de
    ''' căutare amestecat un pic spre culoarea textului.
    ''' </summary>
    <Category("K-BOT: Buttons")>
    <Description("Fundalul butonului ✕ din banda de căutare, la survolare; gol = automat din SearchBoxBackColor.")>
    Public Property SearchClearButtonHoverColor As Color
        Get
            If _searchClearButtonHoverColor <> Color.Empty Then Return _searchClearButtonHoverColor
            Return Blend(SearchBoxBackColor, Me.ForeColor, 0.15F)
        End Get
        Set(value As Color)
            _searchClearButtonHoverColor = value
            ApplyClearButtonHoverColor()
            Me.Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeSearchClearButtonHoverColor() As Boolean
        Return _searchClearButtonHoverColor <> Color.Empty
    End Function
    Public Sub ResetSearchClearButtonHoverColor()
        _searchClearButtonHoverColor = Color.Empty
        ApplyClearButtonHoverColor()
        Me.Invalidate()
    End Sub

    ''' <summary>
    ''' Fundalul curent al butonului ✕: culoarea de survolare cât cursorul e pe el, altfel fundalul
    ''' casetei. E singurul loc care scrie BackColor-ul lui — orice altă scriere l-ar face să
    ''' rămână aprins după ce cursorul a plecat.
    ''' </summary>
    Friend Sub ApplyClearButtonHoverColor()
        If _searchClearBtn Is Nothing Then Return
        _searchClearBtn.BackColor = If(_searchClearHover, SearchClearButtonHoverColor, SearchBoxBackColor)
    End Sub

    Private _searchClearHover As Boolean = False

    ''' <summary>Leagă survolarea butonului ✕. Chemată o dată, la crearea lui.</summary>
    Friend Sub HookClearButtonHover(btn As Label)
        If btn Is Nothing Then Return
        AddHandler btn.MouseEnter, AddressOf OnSearchClearBtnMouseEnter
        AddHandler btn.MouseLeave, AddressOf OnSearchClearBtnMouseLeave
    End Sub

    Private Sub OnSearchClearBtnMouseEnter(sender As Object, e As EventArgs)
        Try
            _searchClearHover = True
            ApplyClearButtonHoverColor()
        Catch ex As Exception
            ' Frontieră UI: logăm și înghițim (un handler nu poate arunca mai departe).
            GlobalErrorLog.Write("AdvancedTreeControl.OnSearchClearBtnMouseEnter", ex)
        End Try
    End Sub

    Private Sub OnSearchClearBtnMouseLeave(sender As Object, e As EventArgs)
        Try
            _searchClearHover = False
            ApplyClearButtonHoverColor()
        Catch ex As Exception
            GlobalErrorLog.Write("AdvancedTreeControl.OnSearchClearBtnMouseLeave", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Plaja de sub o pictogramă survolată. Un singur loc pentru toate butoanele desenate de noi,
    ''' ca ele să se aprindă la fel. Culoare complet transparentă = butonul nu dă niciun semn
    ''' (operatorul poate stinge efectul punând un Color.Transparent explicit).
    ''' </summary>
    Friend Sub DrawButtonHover(g As Graphics, r As Rectangle, culoare As Color)
        If r.IsEmpty OrElse culoare.A = 0 Then Return
        Dim oldSmooth As SmoothingMode = g.SmoothingMode
        g.SmoothingMode = SmoothingMode.AntiAlias
        ' Umflarea și raza sunt LOGICE: pictograma crește cu scara, deci și inelul de sub ea —
        ' altfel la 150% plaja s-ar lipi de marginile iconiței.
        Dim umflare As Integer = SX(BUTTON_HOVER_INFLATE)
        Using b As New SolidBrush(culoare)
            Using path As GraphicsPath = GetRoundedRect(
                    Rectangle.Inflate(r, umflare, umflare),
                    SX(BUTTON_HOVER_RADIUS))
                g.FillPath(b, path)
            End Using
        End Using
        g.SmoothingMode = oldSmooth
    End Sub

    ''' <summary>
    ''' Survolarea butoanelor din ANTET. Chemată din <c>OnMouseMove</c>; repictează doar când
    ''' starea chiar se schimbă, altfel arborele s-ar invalida la fiecare pixel de mișcare.
    ''' </summary>
    Friend Sub UpdateHeaderButtonHover(location As Point)
        Dim peCautare As Boolean = _headerVisible AndAlso _headerSearchIcon IsNot Nothing AndAlso
                                   _headerSearchIconRect.Contains(location)
        Dim peDreapta As Boolean = _headerVisible AndAlso _headerRightIcon IsNot Nothing AndAlso
                                   _headerRightIconRect.Contains(location)
        If peCautare = _headerSearchIconHover AndAlso peDreapta = _headerRightIconHover Then Return
        _headerSearchIconHover = peCautare
        _headerRightIconHover = peDreapta
        RefreshButtonTip()        ' aceeași schimbare de stare hrănește și eticheta butonului
        Me.Invalidate()
    End Sub

    ''' <summary>
    ''' Dreptunghiul iconiței din dreapta unui nod, în coordonatele controlului. ACEEAȘI formulă
    ''' pe care o folosește <c>DrawRightIcon</c> — o a doua ar însemna o plajă care se aprinde
    ''' unde nu e pictograma.
    ''' </summary>
    Friend Function NodeRightIconRect(it As TreeItem) As Rectangle
        If it Is Nothing OrElse it.RightIcon Is Nothing Then Return Rectangle.Empty
        Dim scrollW As Integer = ScrollBarWidth
        Dim rx As Integer = Me.Width - _rightIconSize.Width - RightIconRightPaddingPx - scrollW
        Dim ry As Integer = GetItemY(it) + (_itemHeight - _rightIconSize.Height) \ 2
        Return New Rectangle(rx, ry, _rightIconSize.Width, _rightIconSize.Height)
    End Function

    ''' <summary>Survolarea iconiței din dreapta NODULUI de sub cursor.</summary>
    Friend Sub UpdateNodeRightIconHover(location As Point)
        Dim peIcon As Boolean = False
        If pHoveredItem IsNot Nothing AndAlso pHoveredItem.RightIcon IsNot Nothing Then
            peIcon = NodeRightIconRect(pHoveredItem).Contains(location)
        End If
        If peIcon = _nodeRightIconHover Then Return
        _nodeRightIconHover = peIcon
        Me.Invalidate()
    End Sub

    ''' <summary>Stinge toate stările de survolare (cursorul a plecat din control).</summary>
    Friend Sub ClearButtonHover()
        If Not (_headerSearchIconHover OrElse _headerRightIconHover OrElse
                _footerRightIconHover OrElse _footerLeftIconHover OrElse _nodeRightIconHover) Then Return
        _headerSearchIconHover = False
        _headerRightIconHover = False
        _footerRightIconHover = False
        _footerLeftIconHover = False
        _nodeRightIconHover = False
        HideButtonTip()
        Me.Invalidate()
    End Sub

End Class
