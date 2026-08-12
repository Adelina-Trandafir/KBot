Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' Partea de TEMĂ a <see cref="KBotDataView"/>: maparea sloturilor din <see cref="ThemePalette"/>
''' pe rolurile grilei, plus cache-ul de resurse GDI (pensule/creioane) recreat la fiecare
''' <c>ApplyTheme</c>. Paleta nu are sloturi dedicate de grilă, deci rolurile derivate se obțin
''' prin <c>Blend</c> — NICIO culoare literală în sursă.
''' </summary>
Partial Class KBotDataView

    ' ── Culori pe roluri (setate în ApplyTheme; default = SystemColors) ──────────
    Private _cHeaderBack As Color
    Private _cHeaderText As Color
    Private _cHeaderSep As Color
    Private _cHeaderBaseline As Color
    ' Subsolul are roluri PROPRII (slice 0028): până acum împrumuta sloturile antetului, deci
    ' o schemă nu putea distinge banda de sumar de banda de titluri nici dacă voia.
    Private _cFooterBack As Color
    Private _cFooterText As Color
    Private _cFooterSep As Color
    Private _cFooterBaseline As Color
    ' Benzile de GRUP (slice 0029) au și ele roluri proprii — nici antet, nici subsol de grilă:
    ' o schemă trebuie să poată distinge „titlul unei secțiuni” de „titlurile coloanelor”. Nuanța
    ' pe niveluri (nivelul 0 cel mai apăsat) NU se ține aici: ea se calculează la pictare, din
    ' aceste două, ca un nivel adăugat să nu ceară un slot nou de paletă.
    Private _cGroupHeaderBack As Color
    Private _cGroupHeaderText As Color
    Private _cGroupFooterBack As Color
    Private _cGroupFooterText As Color
    Private _cGroupSep As Color
    ' Capătul degradeului benzilor + dacă el se folosește. Ambele vin din STILUL schemei
    ' (ButtonRender/CornerRadius), nu dintr-un „if Modern” scris în control.
    Private _cHeaderGradientEnd As Color
    Private _cFooterGradientEnd As Color
    Private _bandGradient As Boolean = False
    Private _cRowBack As Color
    Private _cRowAltBack As Color
    Private _cSelBack As Color
    Private _cSelAltBack As Color
    Private _cSelText As Color
    Private _cGridLine As Color
    Private _cCellText As Color
    Private _cCheckBorder As Color
    Private _cCheckFill As Color
    Private _cCheckMark As Color
    Private _cComboChevron As Color
    Private _cOptionBorder As Color
    Private _cOptionFill As Color
    Private _cOptionDot As Color
    Private _cButtonFace As Color
    Private _cButtonBorder As Color
    Private _cButtonText As Color
    Private _cProgressTrack As Color
    Private _cProgressFill As Color
    Private _cDisabledText As Color
    Private _cDisabledWash As Color

    ' ── Resurse GDI cache-uite (recreate în ApplyTheme, eliberate în Dispose) ─────
    Private _bRowBack As SolidBrush
    Private _bRowAltBack As SolidBrush
    Private _bSelBack As SolidBrush
    Private _bSelAltBack As SolidBrush
    Private _bHeaderBack As SolidBrush
    Private _bFooterBack As SolidBrush
    Private _bCheckFill As SolidBrush
    Private _bComboChevron As SolidBrush
    Private _bOptionFill As SolidBrush
    Private _bOptionDot As SolidBrush
    Private _bButtonFace As SolidBrush
    Private _bProgressTrack As SolidBrush
    Private _bProgressFill As SolidBrush
    Private _bDisabledWash As SolidBrush
    Private _bDisabledMark As SolidBrush
    Private _pDisabledMark As Pen
    Private _pBorder As Pen
    Private _pHeaderSep As Pen
    Private _pFooterSep As Pen
    Private _pFooterBaseline As Pen
    Private _pGridLine As Pen
    Private _pGroupSep As Pen
    Private _pCheckBorder As Pen
    Private _pCheckFill As Pen
    Private _pHeaderBaseline As Pen
    Private _pOptionBorder As Pen
    Private _pOptionFill As Pen
    Private _pButtonBorder As Pen

    ' Fonturile benzilor, derivate LAZY din stilul schemei (nu din fontul ambient, și cu atât mai
    ' puțin dintr-un nume de familie scris în sursă — vezi BuildBandFont). Se aruncă la fiecare
    ' ApplyTheme / OnFontChanged și se reconstruiesc la prima pictare.
    Private _headerFont As Font
    Private _footerFont As Font

    ' Fontul de bază al schemei active. Gol / 0 => se ia fontul ambient al controlului.
    Private _schemeFontName As String = String.Empty
    Private _schemeFontSize As Single = 0F

    ' ── Ce a fixat OPERATORUL în designer (Color.Empty / Nothing = „din temă”) ────
    ' Regula casei: o proprietate care se poate seta din designer are nevoie de perechea
    ' ShouldSerialize/Reset, altfel Visual Studio scrie valoarea REZOLVATĂ în .Designer.vb și
    ' de-atunci încolo ea trece drept alegerea deliberată a operatorului.
    Private _headerBackPinned As Color = Color.Empty
    Private _headerForePinned As Color = Color.Empty
    Private _headerFontPinned As Font = Nothing
    Private _footerBackPinned As Color = Color.Empty
    Private _footerForePinned As Color = Color.Empty
    Private _footerFontPinned As Font = Nothing

    ''' <summary>
    ''' Schema activă e ÎNTUNECATĂ? Cât timp e True, culorile fixate în designer se IGNORĂ și
    ''' benzile iau culorile paletei (vezi <see cref="DarkOverridesDesignerColors"/>).
    ''' </summary>
    Private _schemeIsDark As Boolean = False

    ''' <summary>
    ''' Reaplică culorile schemei active. Boundary de temă/pictare: logăm și ÎNGHIȚIM —
    ''' o excepție aici ar rupe traversarea ThemeManager pentru tot formularul.
    ''' </summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            ' Se citește ÎNAINTEA culorilor: *Resolved() îl consultă, iar RebuildThemeResources de
            ' mai jos cheamă deja HeaderBackResolved / FooterBackResolved.
            _schemeIsDark = scheme.IsDark

            ' Stilul schemei conduce fonturile benzilor ȘI dacă ele sunt în degrade: schema
            ' Modern aduce «Segoe UI Variable Text» la 9pt și randare owner-drawn, deci benzile
            ' se schimbă cu tema, nu rămân cu un font scris în control (bug-ul de până în 0028,
            ' unde antetul era mereu „Segoe UI Semibold”, indiferent de schemă).
            Dim st As ThemeStyleOptions = If(scheme.Style, New ThemeStyleOptions())
            _schemeFontName = If(st.BaseFontName, String.Empty)
            _schemeFontSize = st.BaseFontSize
            _bandGradient = (st.ButtonRender = ButtonRenderStyle.ModernOwnerDrawn) OrElse st.CornerRadius > 0
            DisposeBandFonts()

            ' Antet.
            _cHeaderBack = p.SurfaceAltColor
            _cHeaderText = p.TextColor
            _cHeaderSep = p.BorderColor
            _cHeaderBaseline = p.AccentColor
            _cHeaderGradientEnd = Blend(p.SurfaceAltColor, p.SurfaceColor, 0.65)

            ' Subsol — banda de sumar. Aceleași sloturi, dar spălate spre accent, ca ochiul să
            ' vadă din prima că jos e altceva decât un rând de date.
            _cFooterBack = Blend(p.SurfaceAltColor, p.AccentColor, 0.1)
            _cFooterText = p.TextColor
            _cFooterSep = p.BorderColor
            _cFooterBaseline = p.AccentColor
            _cFooterGradientEnd = Blend(_cFooterBack, p.SurfaceColor, 0.55)

            ' Benzile de grup (slice 0029). Antetul de grup e spălat mai TARE spre accent decât
            ' subsolul grilei (0,1): el desparte secțiuni, nu încheie o pagină, deci trebuie să se
            ' vadă de la prima privire că acolo începe altceva. Subsolul de grup e sora lui, mai
            ' potolită — un total de secțiune nu are voie să tragă ochiul mai mult decât totalul general.
            _cGroupHeaderBack = Blend(p.SurfaceAltColor, p.AccentColor, 0.28)
            _cGroupHeaderText = p.TextColor
            _cGroupFooterBack = Blend(p.SurfaceAltColor, p.AccentColor, 0.16)
            _cGroupFooterText = p.TextColor
            _cGroupSep = p.BorderColor

            ' Zona de date.
            _cRowBack = p.InputBackColor
            _cRowAltBack = Blend(p.InputBackColor, p.SurfaceColor, 0.5)
            _cCellText = p.TextColor
            _cGridLine = p.BorderColor

            ' Selecție: spălare ușoară de accent peste fundalul REAL al rândului, ca textul
            ' să rămână lizibil (de aceea două variante: rând normal / rând alternant).
            _cSelBack = Blend(_cRowBack, p.AccentColor, 0.18)
            _cSelAltBack = Blend(_cRowAltBack, p.AccentColor, 0.18)
            _cSelText = p.TextColor

            ' Bifă / opțiune — aceleași convenții de accent.
            _cCheckBorder = p.BorderColor
            _cCheckFill = p.AccentColor
            _cCheckMark = p.AccentTextColor
            _cOptionBorder = p.BorderColor
            _cOptionFill = p.AccentColor
            _cOptionDot = p.AccentTextColor

            ' Combo / buton / bară de progres.
            _cComboChevron = p.TextDimColor
            _cButtonFace = p.ButtonBackColor
            _cButtonBorder = p.ButtonBorderColor
            _cButtonText = p.ButtonTextColor
            _cProgressTrack = Blend(_cRowBack, p.BorderColor, 0.4)
            _cProgressFill = p.AccentColor

            ' Dezactivat: text șters + o spălare FAINT spre suprafață (nu un gri opac).
            _cDisabledText = p.DisabledTextColor
            _cDisabledWash = Blend(_cRowBack, p.SurfaceColor, 0.4)

            ' Editorii flotanți (controale reale) — tematizați direct.
            editText.BackColor = p.InputBackColor
            editText.ForeColor = p.InputTextColor
            editCombo.BackColor = p.InputBackColor
            editCombo.ForeColor = p.InputTextColor
            editCombo.FlatStyle = FlatStyle.Flat

            BackColor = _cRowBack
            ApplyScrollBarTheme()
            RebuildThemeResources()
            ' English (slice 0013): theme changes can swap fonts, so re-measure the columns.
            UpdateLayout()
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.ApplyTheme", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Trece barele de derulare pe varianta întunecată a temei vizuale Windows (slice 0028-03).
    '''
    ''' <para><b>De ce nu sunt pictate de noi.</b> <c>VScrollBar</c>/<c>HScrollBar</c> sunt ferestre
    ''' native: fața lor o desenează Windows, nu <c>OnPaint</c>-ul nostru, deci nicio culoare din
    ''' paletă n-ar ajunge la ele. <c>SetWindowTheme</c> cu «DarkMode_Explorer» e exact trucul pe
    ''' care ThemeManager îl folosește deja pentru liste și pentru <c>KBotComboBox</c> — nu ne
    ''' aduce culorile schemei, ci griul întunecat al Windows-ului, dar aceea e singura variantă
    ''' care nu cere un control de derulare scris de la zero.</para>
    '''
    ''' <para><b>Limita, spusă pe față:</b> barele urmează DOAR perechea întuneric/lumină. Sub o
    ''' schemă colorată ele rămân cele de sistem, iar accentul paletei nu ajunge niciodată pe ele.
    ''' Un <c>KBotScrollBar</c> owner-drawn ar rezolva și asta — vezi worklog-ul feliei.</para>
    '''
    ''' <para>Se re-aplică și la <c>OnHandleCreated</c>: <c>SetWindowTheme</c> cere un handle, iar
    ''' prima aplicare de temă poate cădea înaintea lui.</para>
    ''' </summary>
    Private Sub ApplyScrollBarTheme()
        Dim tema As String = If(_schemeIsDark, "DarkMode_Explorer", "Explorer")
        NativeMethods.ApplyWindowTheme(vScroll, tema)
        NativeMethods.ApplyWindowTheme(hScroll, tema)
    End Sub

    Protected Overrides Sub OnHandleCreated(e As EventArgs)
        MyBase.OnHandleCreated(e)
        Try
            ApplyScrollBarTheme()
        Catch ex As Exception
            ' Boundary UI: crearea handle-ului nu are voie să arunce în bucla de mesaje.
            GlobalErrorLog.Write("KBotDataView.OnHandleCreated", ex)
        End Try
    End Sub

    ' Culorile pre-temă (până la primul ApplyTheme): SystemColors, ca randarea în designer.
    Private Sub SetDefaultColors()
        _cHeaderBack = SystemColors.Control
        _cHeaderText = SystemColors.ControlText
        _cHeaderSep = SystemColors.ControlDark
        _cHeaderBaseline = SystemColors.Highlight
        _cHeaderGradientEnd = Blend(SystemColors.Control, SystemColors.Window, 0.65)
        _cFooterBack = Blend(SystemColors.Control, SystemColors.Highlight, 0.1)
        _cFooterText = SystemColors.ControlText
        _cFooterSep = SystemColors.ControlDark
        _cFooterBaseline = SystemColors.Highlight
        _cFooterGradientEnd = Blend(_cFooterBack, SystemColors.Window, 0.55)
        _cGroupHeaderBack = Blend(SystemColors.Control, SystemColors.Highlight, 0.28)
        _cGroupHeaderText = SystemColors.ControlText
        _cGroupFooterBack = Blend(SystemColors.Control, SystemColors.Highlight, 0.16)
        _cGroupFooterText = SystemColors.ControlText
        _cGroupSep = SystemColors.ControlDark
        _cRowBack = SystemColors.Window
        _cRowAltBack = Blend(SystemColors.Window, SystemColors.Control, 0.5)
        _cSelBack = Blend(SystemColors.Window, SystemColors.Highlight, 0.18)
        _cSelAltBack = Blend(_cRowAltBack, SystemColors.Highlight, 0.18)
        _cSelText = SystemColors.WindowText
        _cCellText = SystemColors.WindowText
        _cGridLine = SystemColors.ControlLight
        _cCheckBorder = SystemColors.ControlDark
        _cCheckFill = SystemColors.Highlight
        _cCheckMark = SystemColors.HighlightText
        _cOptionBorder = SystemColors.ControlDark
        _cOptionFill = SystemColors.Highlight
        _cOptionDot = SystemColors.HighlightText
        _cComboChevron = SystemColors.GrayText
        _cButtonFace = SystemColors.Control
        _cButtonBorder = SystemColors.ControlDark
        _cButtonText = SystemColors.ControlText
        _cProgressTrack = Blend(SystemColors.Window, SystemColors.ControlDark, 0.4)
        _cProgressFill = SystemColors.Highlight
        _cDisabledText = SystemColors.GrayText
        _cDisabledWash = Blend(SystemColors.Window, SystemColors.Control, 0.4)
        BackColor = _cRowBack
    End Sub

    ' Recreează pensulele/creioanele din culorile curente (eliberează-le pe cele vechi).
    Private Sub RebuildThemeResources()
        DisposeThemeResources()
        _bRowBack = New SolidBrush(_cRowBack)
        _bRowAltBack = New SolidBrush(_cRowAltBack)
        _bSelBack = New SolidBrush(_cSelBack)
        _bSelAltBack = New SolidBrush(_cSelAltBack)
        _bHeaderBack = New SolidBrush(HeaderBackResolved())
        _bFooterBack = New SolidBrush(FooterBackResolved())
        _bCheckFill = New SolidBrush(_cCheckFill)
        _bComboChevron = New SolidBrush(_cComboChevron)
        _bOptionFill = New SolidBrush(_cOptionFill)
        _bOptionDot = New SolidBrush(_cOptionDot)
        _bButtonFace = New SolidBrush(_cButtonFace)
        _bProgressTrack = New SolidBrush(_cProgressTrack)
        _bProgressFill = New SolidBrush(_cProgressFill)
        _bDisabledWash = New SolidBrush(_cDisabledWash)
        _bDisabledMark = New SolidBrush(_cDisabledText)
        _pDisabledMark = New Pen(_cDisabledText)
        _pBorder = New Pen(_cHeaderSep)
        _pHeaderSep = New Pen(_cHeaderSep)
        _pFooterSep = New Pen(_cFooterSep)
        _pFooterBaseline = New Pen(_cFooterBaseline, 2.0F)
        _pGridLine = New Pen(_cGridLine)
        _pGroupSep = New Pen(_cGroupSep)
        _pCheckBorder = New Pen(_cCheckBorder)
        _pCheckFill = New Pen(_cCheckFill)
        _pHeaderBaseline = New Pen(_cHeaderBaseline, 2.0F)
        _pOptionBorder = New Pen(_cOptionBorder)
        _pOptionFill = New Pen(_cOptionFill)
        _pButtonBorder = New Pen(_cButtonBorder)
    End Sub

    ' Eliberează resursele GDI cache-uite + fontul de antet (fără scurgeri).
    Private Sub DisposeThemeResources()
        _bRowBack?.Dispose() : _bRowBack = Nothing
        _bRowAltBack?.Dispose() : _bRowAltBack = Nothing
        _bSelBack?.Dispose() : _bSelBack = Nothing
        _bSelAltBack?.Dispose() : _bSelAltBack = Nothing
        _bHeaderBack?.Dispose() : _bHeaderBack = Nothing
        _bFooterBack?.Dispose() : _bFooterBack = Nothing
        _bCheckFill?.Dispose() : _bCheckFill = Nothing
        _bComboChevron?.Dispose() : _bComboChevron = Nothing
        _bOptionFill?.Dispose() : _bOptionFill = Nothing
        _bOptionDot?.Dispose() : _bOptionDot = Nothing
        _bButtonFace?.Dispose() : _bButtonFace = Nothing
        _bProgressTrack?.Dispose() : _bProgressTrack = Nothing
        _bProgressFill?.Dispose() : _bProgressFill = Nothing
        _bDisabledWash?.Dispose() : _bDisabledWash = Nothing
        _bDisabledMark?.Dispose() : _bDisabledMark = Nothing
        _pDisabledMark?.Dispose() : _pDisabledMark = Nothing
        _pBorder?.Dispose() : _pBorder = Nothing
        _pHeaderSep?.Dispose() : _pHeaderSep = Nothing
        _pFooterSep?.Dispose() : _pFooterSep = Nothing
        _pFooterBaseline?.Dispose() : _pFooterBaseline = Nothing
        _pGridLine?.Dispose() : _pGridLine = Nothing
        _pGroupSep?.Dispose() : _pGroupSep = Nothing
        _pCheckBorder?.Dispose() : _pCheckBorder = Nothing
        _pCheckFill?.Dispose() : _pCheckFill = Nothing
        _pHeaderBaseline?.Dispose() : _pHeaderBaseline = Nothing
        _pOptionBorder?.Dispose() : _pOptionBorder = Nothing
        _pOptionFill?.Dispose() : _pOptionFill = Nothing
        _pButtonBorder?.Dispose() : _pButtonBorder = Nothing
        DisposeBandFonts()
    End Sub

    ' Fonturile DERIVATE sunt ale noastre (le eliberăm); cele fixate de operator
    ' (_headerFontPinned/_footerFontPinned) sunt ale lui — nu se ating aici.
    Private Sub DisposeBandFonts()
        _headerFont?.Dispose() : _headerFont = Nothing
        _footerFont?.Dispose() : _footerFont = Nothing
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' FONTURILE BENZILOR — rezolvate din temă, cu ultimul cuvânt la operator
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>Fontul cu care se scriu titlurile de coloană (fixat de operator sau din temă).</summary>
    Friend Function ResolvedHeaderFont() As Font
        If _headerFontPinned IsNot Nothing Then Return _headerFontPinned
        If _headerFont Is Nothing Then _headerFont = BuildBandFont()
        Return _headerFont
    End Function

    ''' <summary>Fontul cu care se scriu agregatele din subsol (fixat de operator sau din temă).</summary>
    Friend Function ResolvedFooterFont() As Font
        If _footerFontPinned IsNot Nothing Then Return _footerFontPinned
        If _footerFont Is Nothing Then _footerFont = BuildBandFont()
        Return _footerFont
    End Function

    ''' <summary>
    ''' Fontul cu care se scrie titlul UNEI coloane: al ei, dacă și l-a cerut
    ''' (<see cref="KBotDataColumn.HeaderFont"/>), altfel al benzii. Precedența stă AICI și numai
    ''' aici: o citesc pictarea, măsurarea la conținut și înălțimea benzii de antet, iar dacă
    ''' vreuna dintre ele ar citi direct <see cref="ResolvedHeaderFont"/>, coloana ar fi scrisă cu
    ''' un font și măsurată cu altul — adică fie tăiată cu elipsă, fie cu ultimul rând de titlu
    ''' sub linia de bază.
    ''' </summary>
    Friend Function HeaderFontFor(col As KBotDataColumn) As Font
        If col IsNot Nothing AndAlso col.HeaderFont IsNot Nothing Then Return col.HeaderFont
        Return ResolvedHeaderFont()
    End Function

    ''' <summary>
    ''' Fontul cu care se scriu celulele unei coloane: al ei
    ''' (<see cref="KBotDataColumn.ColumnFont"/>), altfel al grilei. Aceeași regulă ca la antet, pe
    ''' cealaltă față a coloanei — și tot un singur loc, citit de pictare, de măsurare și de
    ''' verificarea depășirii care aprinde eticheta.
    ''' </summary>
    Friend Function CellFontFor(col As KBotDataColumn) As Font
        If col IsNot Nothing AndAlso col.ColumnFont IsNot Nothing Then Return col.ColumnFont
        Return Font
    End Function

    ''' <summary>
    ''' Fontul unei benzi: familia și mărimea DIN SCHEMĂ (<c>Style.BaseFontName</c> /
    ''' <c>BaseFontSize</c>), în varianta semibold dacă familia are una instalată, altfel bold.
    '''
    ''' Semibold-ul se caută ca FAMILIE separată («Segoe UI Semibold», «Segoe UI Variable Text
    ''' Semibold»), pentru că așa îl expune Windows. Verificarea se face în lista familiilor
    ''' instalate, nu construind fontul și prinzând excepția: GDI+ nu aruncă pentru o familie
    ''' necunoscută, ci cade tăcut pe alta — adică exact felul de eșec pe care nu-l vezi.
    ''' </summary>
    Private Function BuildBandFont() As Font
        Dim numeBaza As String = If(String.IsNullOrWhiteSpace(_schemeFontName), Font.Name, _schemeFontName)
        Dim marime As Single = If(_schemeFontSize > 0F, _schemeFontSize, Font.Size)
        Dim semibold As String = numeBaza & " Semibold"
        If FamilyExists(semibold) Then Return New Font(semibold, marime)
        If FamilyExists(numeBaza) Then Return New Font(numeBaza, marime, FontStyle.Bold)
        ' Nici familia schemei nu e instalată: fontul ambient în bold, ca să rămână lizibil.
        Return New Font(Font, FontStyle.Bold)
    End Function

    Private Shared Function FamilyExists(name As String) As Boolean
        If String.IsNullOrWhiteSpace(name) Then Return False
        For Each f As FontFamily In FontFamily.Families
            If String.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase) Then Return True
        Next
        Return False
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' ASPECTUL BENZILOR — proprietăți de designer (gol = din temă)
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Sub o schemă ÎNTUNECATĂ, culorile fixate în designer se ignoră (slice 0028-03).
    '''
    ''' <para>Regula obișnuită a casei e inversă — «orice culoare pusă explicit câștigă» — și rămâne
    ''' inversă pe schemele luminoase. Excepția e cerută de ce se întâmplă altfel: paleta de
    ''' designer se autorează pe fundal deschis, iar o bandă de antet lăsată albă peste un corp
    ''' devenit aproape negru nu e „alegerea operatorului respectată”, e o grilă imposibil de
    ''' citit. Sub întuneric, contrastul bate preferința; la lumină, preferința bate implicitul.</para>
    '''
    ''' <para>Se aplică numai CULORILOR. Fontul fixat rămâne al operatorului în orice schemă: un
    ''' font nu devine ilizibil pe fundal închis, deci n-are de ce să fie luat înapoi.</para>
    ''' </summary>
    Friend ReadOnly Property DarkOverridesDesignerColors As Boolean
        Get
            Return _schemeIsDark
        End Get
    End Property

    Friend Function HeaderBackResolved() As Color
        If _schemeIsDark Then Return _cHeaderBack
        Return If(_headerBackPinned = Color.Empty, _cHeaderBack, _headerBackPinned)
    End Function

    Friend Function HeaderForeResolved() As Color
        If _schemeIsDark Then Return _cHeaderText
        Return If(_headerForePinned = Color.Empty, _cHeaderText, _headerForePinned)
    End Function

    Friend Function FooterBackResolved() As Color
        If _schemeIsDark Then Return _cFooterBack
        Return If(_footerBackPinned = Color.Empty, _cFooterBack, _footerBackPinned)
    End Function

    Friend Function FooterForeResolved() As Color
        If _schemeIsDark Then Return _cFooterText
        Return If(_footerForePinned = Color.Empty, _cFooterText, _footerForePinned)
    End Function

    ''' <summary>Fundalul benzii de antet. <c>Color.Empty</c> (implicit) = din schema activă.</summary>
    <Category("K-BOT: Header")>
    <Description("Fundalul benzii de antet. Gol = culoarea din schema activă.")>
    Public Property HeaderBackColor As Color
        Get
            Return _headerBackPinned
        End Get
        Set(value As Color)
            If _headerBackPinned = value Then Return
            _headerBackPinned = value
            RebuildThemeResources()
            Invalidate()
        End Set
    End Property

    Private Function ShouldSerializeHeaderBackColor() As Boolean
        Return _headerBackPinned <> Color.Empty
    End Function

    Private Sub ResetHeaderBackColor()
        HeaderBackColor = Color.Empty
    End Sub

    ''' <summary>Culoarea titlurilor de coloană. <c>Color.Empty</c> (implicit) = din schema activă.</summary>
    <Category("K-BOT: Header")>
    <Description("Culoarea textului din antet. Gol = culoarea din schema activă.")>
    Public Property HeaderForeColor As Color
        Get
            Return _headerForePinned
        End Get
        Set(value As Color)
            If _headerForePinned = value Then Return
            _headerForePinned = value
            Invalidate()
        End Set
    End Property

    Private Function ShouldSerializeHeaderForeColor() As Boolean
        Return _headerForePinned <> Color.Empty
    End Function

    Private Sub ResetHeaderForeColor()
        HeaderForeColor = Color.Empty
    End Sub

    ''' <summary>
    ''' Fontul benzii de antet. <c>Nothing</c> (implicit) = derivat din schema activă (vezi
    ''' <see cref="BuildBandFont"/>). Perechea ShouldSerialize/Reset e obligatorie: <c>Font</c> nu
    ''' poate purta <c>DefaultValue</c>, deci fără ea designerul ar scrie fontul rezolvat în
    ''' fiecare formular-gazdă și schimbarea temei n-ar mai ajunge niciodată la antet.
    ''' </summary>
    <Category("K-BOT: Header")>
    <Description("Fontul benzii de antet. Nesetat = fontul schemei active, în semibold.")>
    Public Property HeaderFont As Font
        Get
            Return _headerFontPinned
        End Get
        Set(value As Font)
            If _headerFontPinned Is value Then Return
            _headerFontPinned = value
            UpdateLayout()
            Invalidate()
        End Set
    End Property

    Private Function ShouldSerializeHeaderFont() As Boolean
        Return _headerFontPinned IsNot Nothing
    End Function

    Private Sub ResetHeaderFont()
        HeaderFont = Nothing
    End Sub

    ''' <summary>Fundalul benzii de subsol. <c>Color.Empty</c> (implicit) = din schema activă.</summary>
    <Category("K-BOT: Footer")>
    <Description("Fundalul benzii de subsol. Gol = culoarea din schema activă.")>
    Public Property FooterBackColor As Color
        Get
            Return _footerBackPinned
        End Get
        Set(value As Color)
            If _footerBackPinned = value Then Return
            _footerBackPinned = value
            RebuildThemeResources()
            Invalidate()
        End Set
    End Property

    Private Function ShouldSerializeFooterBackColor() As Boolean
        Return _footerBackPinned <> Color.Empty
    End Function

    Private Sub ResetFooterBackColor()
        FooterBackColor = Color.Empty
    End Sub

    ''' <summary>Culoarea agregatelor din subsol. <c>Color.Empty</c> (implicit) = din schema activă.</summary>
    <Category("K-BOT: Footer")>
    <Description("Culoarea textului din subsol. Gol = culoarea din schema activă.")>
    Public Property FooterForeColor As Color
        Get
            Return _footerForePinned
        End Get
        Set(value As Color)
            If _footerForePinned = value Then Return
            _footerForePinned = value
            Invalidate()
        End Set
    End Property

    Private Function ShouldSerializeFooterForeColor() As Boolean
        Return _footerForePinned <> Color.Empty
    End Function

    Private Sub ResetFooterForeColor()
        FooterForeColor = Color.Empty
    End Sub

    ''' <summary>Fontul benzii de subsol. <c>Nothing</c> (implicit) = derivat din schema activă.</summary>
    <Category("K-BOT: Footer")>
    <Description("Fontul benzii de subsol. Nesetat = fontul schemei active, în semibold.")>
    Public Property FooterFont As Font
        Get
            Return _footerFontPinned
        End Get
        Set(value As Font)
            If _footerFontPinned Is value Then Return
            _footerFontPinned = value
            UpdateLayout()
            Invalidate()
        End Set
    End Property

    Private Function ShouldSerializeFooterFont() As Boolean
        Return _footerFontPinned IsNot Nothing
    End Function

    Private Sub ResetFooterFont()
        FooterFont = Nothing
    End Sub

    Protected Overrides Sub OnFontChanged(e As EventArgs)
        MyBase.OnFontChanged(e)
        Try
            ' Fontul ambient e plasa de siguranță a fonturilor de bandă (schemă fără familie
            ' instalată / fără BaseFontSize), deci ele se reconstruiesc odată cu el.
            DisposeBandFonts()
            ' English (slice 0013): a new font changes measured content widths — re-layout.
            UpdateLayout()
            Invalidate()
        Catch ex As Exception
            ' Boundary UI: a font change must not throw into the message loop.
            GlobalErrorLog.Write("KBotDataView.OnFontChanged", ex)
        End Try
    End Sub

    ' ── Ajutoare pure (ThemeShapes din KBot.Theming e Friend, invizibil de aici) ──

    ''' <summary>Amestec liniar între două culori: t=0 => a, t=1 => b (t limitat la 0..1).</summary>
    Private Shared Function Blend(a As Color, b As Color, t As Double) As Color
        Dim tt As Double = Math.Max(0.0, Math.Min(1.0, t))
        Dim r As Integer = CInt(CDbl(a.R) + (CDbl(b.R) - a.R) * tt)
        Dim g As Integer = CInt(CDbl(a.G) + (CDbl(b.G) - a.G) * tt)
        Dim bl As Integer = CInt(CDbl(a.B) + (CDbl(b.B) - a.B) * tt)
        Return Color.FromArgb(r, g, bl)
    End Function

    ''' <summary>Cale dreptunghi cu colțuri rotunjite (radius deja în px scalați).</summary>
    Private Shared Function RoundedRect(bounds As Rectangle, radius As Integer) As GraphicsPath
        Dim path As New GraphicsPath()
        Dim d As Integer = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height))
        If d <= 0 Then
            path.AddRectangle(bounds)
            Return path
        End If
        Dim arc As New Rectangle(bounds.Location, New Size(d, d))
        path.AddArc(arc, 180, 90)
        arc.X = bounds.Right - d
        path.AddArc(arc, 270, 90)
        arc.Y = bounds.Bottom - d
        path.AddArc(arc, 0, 90)
        arc.X = bounds.Left
        path.AddArc(arc, 90, 90)
        path.CloseFigure()
        Return path
    End Function

End Class
