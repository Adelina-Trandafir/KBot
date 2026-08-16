Option Strict On
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' RÂNDUL-CURSOR al meniului (felia 0036-01): un rând care se TRAGE în loc să se apese.
'''
''' <para><b>De ce în meniu și nu doar într-o fereastră de opțiuni.</b> Mărimea textului se
''' potrivește din ochi, nu din cifre: tragi până arată bine. Un reglaj din ochi cere ca efectul
''' să se vadă ÎN TIMP CE tragi, deci meniul nu are voie să se închidă la prima mișcare — de aceea
''' cursorul nu trece prin <c>ItemClicked</c> (care închide), ci prin
''' <see cref="CustomPopup.SliderValueChanged"/>.</para>
'''
''' <para><b>Trei drumuri egale către aceeași valoare</b>, ca la orice cursor de sistem: tragere cu
''' mouse-ul, săgeți stânga/dreapta, și Home/End pentru capete — dar Home/End rămân ale MENIULUI
''' cât timp evidențierea nu e pe un cursor, altfel navigarea obișnuită ar deveni imprevizibilă.
''' </para>
'''
''' <para>Geometria: eticheta la stânga (măsurată din text), valoarea la dreapta (lățime fixă, ca
''' șina să nu-și schimbe lungimea când valoarea trece de la «100%» la «95%» — o șină care
''' tresare sub cursor e cel mai ușor fel de a rata pasul dorit), șina pe ce rămâne la mijloc.</para>
''' </summary>
Partial Public Class CustomPopup

    ' Măsuri LOGICE (px @96dpi) — se scalează la pictare, ca tot restul meniului.
    Private Const SliderTrackHeightLogical As Integer = 4
    Private Const SliderThumbWidthLogical As Integer = 10
    Private Const SliderThumbHeightLogical As Integer = 16
    Private Const SliderGapLogical As Integer = 8
    Private Const SliderValueWidthLogical As Integer = 44

    ''' <summary>Șina cea mai scurtă care mai poate fi trasă cu folos — croiala meniului o rezervă.</summary>
    Friend Const SliderMinTrackLogical As Integer = 90

    ''' <summary>
    ''' Cât mută o săgeată. 5 și nu 1: cursorul din meniu e pentru reglaj din ochi, iar un pas de
    ''' 1% ar cere douăzeci și cinci de apăsări ca să treci de la 100% la 125%. Reglajul fin are
    ''' deja un loc — câmpul cu cifre din fereastra de opțiuni.
    ''' </summary>
    Friend Const SliderKeyStep As Integer = 5

    ''' <summary>
    ''' S-a mutat un cursor. Ridicat la FIECARE pas al tragerii — deci e evenimentul pentru ce e
    ''' IEFTIN: o etichetă, o previzualizare desenată, un număr afișat.
    '''
    ''' <para><b>Nu-l folosi pentru lucrul greu.</b> Vezi <see cref="SliderValueCommitted"/>.</para>
    ''' </summary>
    <System.ComponentModel.Category("K-BOT")>
    <System.ComponentModel.Description("Ridicat la fiecare schimbare de valoare a unui rând-cursor (inclusiv în timpul tragerii). Pentru reacții ieftine.")>
    Public Event SliderValueChanged As EventHandler(Of CustomPopupItemEventArgs)

    ''' <summary>
    ''' S-a TERMINAT de mutat un cursor: la ridicarea butonului care încheie o tragere, sau la
    ''' ridicarea tastei care a mutat valoarea. Aici se pune lucrul GREU.
    '''
    ''' <para><b>De ce există.</b> Prima formă a rândului-cursor avea un singur eveniment, ridicat
    ''' la fiecare pixel, iar gazda rescria din el fonturile întregii aplicații. Două urmări, ambele
    ''' văzute pe ecran: era de nefolosit ca viteză, și — mai rău — MENIUL DISPĂREA. Cauza:
    ''' rescrierea fonturilor reașază toate ferestrele deschise, fereastra de dedesubt se
    ''' reactivează, iar meniul se închide singur pe <c>Deactivate</c>, exact cum face orice meniu
    ''' de sistem la un clic în afară. Cu munca mutată la sfârșitul gestului, tragerea decurge
    ''' netulburată.</para>
    '''
    ''' <para>Se ridică DOAR dacă valoarea chiar s-a schimbat față de începutul gestului — o
    ''' apăsare care n-a mișcat nimic nu e o comandă.</para>
    ''' </summary>
    <System.ComponentModel.Category("K-BOT")>
    <System.ComponentModel.Description("Ridicat după ce s-a terminat mutarea unui rând-cursor (ridicare de buton sau de tastă). Aici se pune lucrul greu.")>
    Public Event SliderValueCommitted As EventHandler(Of CustomPopupItemEventArgs)

    ' Cursorul aflat sub tragere (-1 = niciunul). Cât e ridicat, mișcările de mouse merg la el și
    ' NU mai mută evidențierea: degetul e pe șină, nu pe meniu.
    Private _draggingSlider As Integer = -1

    ' Valoarea de la ÎNCEPUTUL gestului, ca predarea să știe dacă s-a schimbat ceva. Fără ea, o
    ' apăsare fără mișcare ar trece drept comandă.
    Private _sliderValueAtGestureStart As Integer = 0

    ' Cursorul mutat de la TASTATURĂ și încă nepredat (-1 = niciunul). Săgețile sosesc una câte
    ' una, deci gestul se încheie la ridicarea tastei, nu la fiecare apăsare — altfel ținerea
    ' săgeții apăsate ar comanda lucrul greu de zeci de ori.
    Private _keyboardSlider As Integer = -1

    ' Ridicat cât ține predarea. Gazda face în handler lucruri care REAȘAZĂ ferestrele (rescrie
    ' fonturile aplicației), iar asta reactivează fereastra de dedesubt și ne-ar închide pe
    ' Deactivate — vezi OnDeactivate din partiala principală.
    Private _committingSlider As Boolean = False

    ''' <summary>
    ''' Se predă chiar acum o valoare de cursor? Cât e True, meniul NU se închide pe
    ''' <c>Deactivate</c>: pierderea activării vine din propria noastră comandă, nu dintr-un clic
    ''' al operatorului în afară.
    ''' </summary>
    Friend ReadOnly Property IsCommittingSlider As Boolean
        Get
            Return _committingSlider
        End Get
    End Property

    ''' <summary>
    ''' Cusătură de test: pierderea activării, fără o fereastră adevărată care s-o producă.
    '''
    ''' Regula «meniul NU se închide cât timp își predă propria comandă» n-are cum fi ținută fix
    ''' altfel — ar cere două ferestre pe ecran și o schimbare reală de activare, adică exact
    ''' genul de probă care nu se poate rula fără ochi. <c>Friend</c>, ca
    ''' <c>ConstruiesteElementeleMeniului</c> de pe bara de titlu, și din același motiv.
    ''' </summary>
    Friend Sub TestDeactivate()
        OnDeactivate(EventArgs.Empty)
    End Sub

    ''' <summary>Se trage chiar acum un cursor? (citit de mouse-ul din partiala .Input)</summary>
    Friend ReadOnly Property IsDraggingSlider As Boolean
        Get
            Return _draggingSlider >= 0
        End Get
    End Property

    ''' <summary>Rândul e un cursor valid?</summary>
    Friend Function IsSliderRow(index As Integer) As Boolean
        If index < 0 OrElse index >= Items.Count Then Return False
        Dim it As CustomPopupItem = Items(index)
        Return it.IsSlider AndAlso Not it.IsSeparator
    End Function

    ' ── Geometrie ────────────────────────────────────────────────────────────────

    ''' <summary>
    ''' Șina cursorului, în coordonate de CONȚINUT (fără derulare). Goală dacă rândul nu e cursor
    ''' sau dacă nu mai rămâne loc de șină după etichetă și valoare.
    ''' </summary>
    Friend Function SliderTrackBounds(index As Integer) As Rectangle
        If Not IsSliderRow(index) Then Return Rectangle.Empty
        Dim r As Rectangle = RowBounds(index)
        If r.IsEmpty Then Return Rectangle.Empty

        Dim padX As Integer = ThemeShapes.ScaleDpi(Me, PadXLogical)
        Dim gap As Integer = ThemeShapes.ScaleDpi(Me, SliderGapLogical)
        Dim valW As Integer = ThemeShapes.ScaleDpi(Me, SliderValueWidthLogical)
        Dim eticheta As Integer = SliderLabelWidth(index)

        Dim stanga As Integer = r.Left + padX + IconGutter() + eticheta + gap
        Dim dreapta As Integer = r.Right - padX - valW - gap
        Dim latime As Integer = dreapta - stanga
        If latime <= 0 Then Return Rectangle.Empty

        Dim h As Integer = ThemeShapes.ScaleDpi(Me, SliderTrackHeightLogical)
        Return New Rectangle(stanga, r.Top + (r.Height - h) \ 2, latime, h)
    End Function

    ' Lățimea etichetei din stânga. 0 pentru un cursor fără text — atunci șina ia tot rândul.
    Private Function SliderLabelWidth(index As Integer) As Integer
        Dim text As String = If(Items(index).Text, String.Empty)
        If text.Length = 0 Then Return 0
        Return TextRenderer.MeasureText(PopupMnemonic.Strip(text), Font,
                                        New Size(Integer.MaxValue, Integer.MaxValue),
                                        MeasureFlags()).Width
    End Function

    ''' <summary>
    ''' Valoarea care corespunde unui X de pe ecran. Se măsoară pe șina UTILĂ — cea din care s-a
    ''' scăzut lățimea degetului — altfel capătul din dreapta n-ar putea fi atins niciodată:
    ''' degetul se desenează CENTRAT pe poziție, deci la 100% jumătate din el ar ieși din șină.
    ''' </summary>
    Friend Function SliderValueAt(index As Integer, clientX As Integer) As Integer
        Dim sina As Rectangle = SliderTrackBounds(index)
        If sina.IsEmpty Then Return Items(index).SliderValue

        Dim deget As Integer = ThemeShapes.ScaleDpi(Me, SliderThumbWidthLogical)
        Dim utila As Integer = sina.Width - deget
        If utila <= 0 Then Return Items(index).SliderValue

        Dim x As Integer = clientX - (sina.Left + deget \ 2)
        Dim fractie As Double = Math.Max(0.0, Math.Min(1.0, x / CDbl(utila)))

        Dim it As CustomPopupItem = Items(index)
        Return it.SliderMinimum + CInt(Math.Round(fractie * (it.SliderMaximum - it.SliderMinimum)))
    End Function

    ' Poziția (în client) a degetului pentru valoarea curentă.
    Private Function SliderThumbRect(index As Integer, sina As Rectangle) As Rectangle
        Dim latime As Integer = ThemeShapes.ScaleDpi(Me, SliderThumbWidthLogical)
        Dim inaltime As Integer = ThemeShapes.ScaleDpi(Me, SliderThumbHeightLogical)
        ' Degetul nu are voie să iasă din rând: cu un font mic și un rând strâns, înălțimea lui
        ' logică (16) poate depăși slotul, iar el s-ar picta peste rândurile vecine.
        Dim rand As Rectangle = RowBounds(index)
        If Not rand.IsEmpty Then inaltime = Math.Min(inaltime, Math.Max(1, rand.Height - 2))
        Dim utila As Integer = Math.Max(0, sina.Width - latime)
        Dim x As Integer = sina.Left + CInt(Math.Round(Items(index).SliderFraction * utila))
        Return New Rectangle(x, sina.Top + (sina.Height - inaltime) \ 2, latime, inaltime)
    End Function

    ' ── Schimbarea valorii ───────────────────────────────────────────────────────

    ''' <summary>
    ''' Așază valoarea unui cursor și anunță — o singură dată, și DOAR dacă s-a schimbat ceva.
    ''' Garda contează: tragerea produce zeci de mesaje de mouse pe același pixel, iar fiecare ar
    ''' fi ridicat evenimentul, deci gazda ar fi rescris fonturile întregii aplicații de zeci de
    ''' ori pe secundă.
    ''' </summary>
    Friend Sub SetSliderValue(index As Integer, value As Integer)
        Try
            If Not IsSliderRow(index) Then Return
            Dim it As CustomPopupItem = Items(index)
            Dim vechi As Integer = it.SliderValue
            it.SliderValue = value                  ' setter-ul limitează
            If it.SliderValue = vechi Then Return

            Invalidate()
            RaiseEvent SliderValueChanged(Me, New CustomPopupItemEventArgs(it, index))
        Catch ex As Exception
            ' Frontieră: drumul vine din mouse/tastatură, iar gazda face lucruri reale în handler
            ' (rescrie fonturi). Un throw de acolo ar ieși prin bucla de mesaje.
            GlobalErrorLog.Write("CustomPopup.SetSliderValue", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Un pas de săgeată pe cursorul evidențiat. Întoarce False dacă nu e niciunul. Valoarea se
    ''' mută pe loc, dar PREDAREA vine abia la ridicarea tastei (vezi <see cref="CommitKeyboardSlider"/>).
    ''' </summary>
    Friend Function NudgeSelectedSlider(delta As Integer) As Boolean
        If Not IsSliderRow(SelectedIndex) Then Return False
        If _keyboardSlider <> SelectedIndex Then
            _keyboardSlider = SelectedIndex
            _sliderValueAtGestureStart = Items(SelectedIndex).SliderValue
        End If
        SetSliderValue(SelectedIndex, Items(SelectedIndex).SliderValue + delta)
        Return True
    End Function

    ''' <summary>
    ''' Sfârșitul unui gest de TASTATURĂ (ridicarea săgeții / a lui Home / End). Idempotentă: se
    ''' cheamă la orice ridicare de tastă, inclusiv la cele care n-au atins niciun cursor.
    ''' </summary>
    Friend Sub CommitKeyboardSlider()
        If _keyboardSlider < 0 Then Return
        Dim index As Integer = _keyboardSlider
        _keyboardSlider = -1
        CommitSlider(index)
    End Sub

    ''' <summary>Începe tragerea și sare pe loc la valoarea de sub cursor, ca orice cursor de sistem.</summary>
    Friend Sub BeginSliderDrag(index As Integer, clientX As Integer)
        If Not IsSliderRow(index) Then Return
        _draggingSlider = index
        _sliderValueAtGestureStart = Items(index).SliderValue
        Capture = True
        SetSliderValue(index, SliderValueAt(index, clientX))
    End Sub

    ''' <summary>Continuă tragerea pornită mai devreme.</summary>
    Friend Sub UpdateSliderDrag(clientX As Integer)
        If _draggingSlider < 0 Then Return
        SetSliderValue(_draggingSlider, SliderValueAt(_draggingSlider, clientX))
    End Sub

    ''' <summary>
    ''' Termină tragerea ȘI predă valoarea. Idempotentă — se cheamă și de pe drumuri care n-au
    ''' tras nimic.
    ''' </summary>
    Friend Sub EndSliderDrag()
        If _draggingSlider < 0 Then Return
        Dim index As Integer = _draggingSlider
        _draggingSlider = -1
        Capture = False
        CommitSlider(index)
    End Sub

    ''' <summary>
    ''' Predă valoarea gazdei — o singură dată, la sfârșitul gestului, și doar dacă s-a schimbat
    ''' ceva față de începutul lui.
    '''
    ''' Frontieră: gazda face aici lucrul greu (rescrie fonturile aplicației), iar un throw de
    ''' acolo ar ieși prin bucla de mesaje.
    ''' </summary>
    Private Sub CommitSlider(index As Integer)
        Try
            If Not IsSliderRow(index) Then Return
            Dim it As CustomPopupItem = Items(index)
            If it.SliderValue = _sliderValueAtGestureStart Then Return

            _sliderValueAtGestureStart = it.SliderValue
            _committingSlider = True
            Try
                RaiseEvent SliderValueCommitted(Me, New CustomPopupItemEventArgs(it, index))
            Finally
                ' Coborât ORICUM: lăsat ridicat de o excepție, meniul n-ar mai putea fi închis
                ' printr-un clic în afară — ar părea agățat pe ecran.
                _committingSlider = False
            End Try

            ' Munca gazdei ne-a putut fura activarea (i-am cerut să reașeze toate ferestrele).
            ' O luăm înapoi, ca meniul să rămână folosibil pentru pasul următor — asta e chiar
            ' plângerea de la care a pornit trecerea asta.
            If Not IsDisposed AndAlso Visible Then Activate()
        Catch ex As Exception
            GlobalErrorLog.Write("CustomPopup.CommitSlider", ex)
        End Try
    End Sub

    ' ── Pictura ──────────────────────────────────────────────────────────────────

    ''' <summary>
    ''' Desenează un rând-cursor. Chemat DOAR din <c>OnPaint</c>, care e deja înfășurat — vezi
    ''' regula de acoperire tranzitivă din CLAUDE.md.
    ''' </summary>
    Friend Sub DrawSliderRow(g As Graphics, index As Integer, r As Rectangle, padX As Integer, gutter As Integer)
        Dim it As CustomPopupItem = Items(index)
        Dim evidentiat As Boolean = (index = SelectedIndex)
        Dim fore As Color = If(it.Enabled, EffectiveItemForeColor, EffectiveDisabledForeColor)

        ' Evidențierea unui cursor e mai discretă decât a unui rând: rândul ăsta nu se alege, deci
        ' un fundal plin ar promite o apăsare care nu face nimic. Doar șina se aprinde (mai jos).
        Dim eticheta As String = PopupMnemonic.Strip(If(it.Text, String.Empty))
        If eticheta.Length > 0 Then
            Dim tr As New Rectangle(r.Left + padX + gutter, r.Top, SliderLabelWidth(index), r.Height)
            TextRenderer.DrawText(g, eticheta, Font, tr, fore, MeasureFlags())
        End If

        Dim sina As Rectangle = SliderTrackBounds(index)
        If Not sina.IsEmpty Then
            Dim raza As Integer = Math.Max(1, sina.Height \ 2)

            ' Partea nefolosită a șinei: culoarea separatorului — aceeași linie fină cu care e
            ' desenată despărțirea dintre grupuri, deci nicio culoare nouă în meniu.
            Using path As GraphicsPath = ThemeShapes.RoundedRect(sina, raza)
                Using b As New SolidBrush(EffectiveSeparatorColor)
                    g.FillPath(b, path)
                End Using
            End Using

            Dim deget As Rectangle = SliderThumbRect(index, sina)

            ' Partea parcursă, până la mijlocul degetului.
            Dim panaLa As Integer = deget.Left + deget.Width \ 2 - sina.Left
            If panaLa > 0 Then
                Dim umplut As New Rectangle(sina.Left, sina.Top, Math.Min(panaLa, sina.Width), sina.Height)
                Using path As GraphicsPath = ThemeShapes.RoundedRect(umplut, raza)
                    Using b As New SolidBrush(If(it.Enabled, EffectiveHighlightBackColor, EffectiveDisabledForeColor))
                        g.FillPath(b, path)
                    End Using
                End Using
            End If

            ' Degetul: aceeași umplere «modern» ca a rândului evidențiat, deci se potrivește cu
            ' restul meniului fără să introducă o culoare a lui.
            Dim razaDeget As Integer = Math.Max(2, ThemeShapes.ScaleDpi(Me, 3))
            Using path As GraphicsPath = ThemeShapes.RoundedRect(deget, razaDeget)
                ThemeShapes.FillModern(g, path, deget,
                                       If(it.Enabled, EffectiveHighlightBackColor, EffectiveSeparatorColor),
                                       ItemGradient)
                ' Conturul desprinde degetul de șina de sub el, care e din aceeași familie de culori.
                Using pen As New Pen(If(evidentiat, EffectiveItemForeColor, EffectiveBorderColor))
                    g.DrawPath(pen, path)
                End Using
            End Using
        End If

        ' Valoarea, la dreapta, aliniată la dreapta ca cifrele să nu joace de la un pas la altul.
        Dim valW As Integer = ThemeShapes.ScaleDpi(Me, SliderValueWidthLogical)
        Dim vr As New Rectangle(r.Right - padX - valW, r.Top, valW, r.Height)
        TextRenderer.DrawText(g, SliderValueText(it), Font, vr, fore,
                              TextFormatFlags.Right Or TextFormatFlags.VerticalCenter Or
                              TextFormatFlags.SingleLine Or TextFormatFlags.NoPrefix)
    End Sub

    ''' <summary>
    ''' Cum se scrie valoarea. Meniul nu știe ce înseamnă numărul, deci pune doar «%» — cursorul
    ''' pe care îl folosim (mărimea textului) e în procente, iar un cursor viitor cu altă unitate
    ''' își va aduce propriul format printr-o proprietate, nu prin ghicit aici.
    ''' </summary>
    Private Shared Function SliderValueText(it As CustomPopupItem) As String
        Return it.SliderValue.ToString(Globalization.CultureInfo.CurrentCulture) & "%"
    End Function

End Class
