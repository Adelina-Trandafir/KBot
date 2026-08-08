Imports System.ComponentModel

''' <summary>
''' Tematizarea arborelui. Până aici arborele era singurul control K-BOT NEtematizat, iar
''' shell-ul îi împingea culorile prin proprietăți — ceea ce a produs exact bug-ul raportat de
''' operator: culorile puse în designer erau rescrise la rulare. Două cauze, ambele rezolvate aici:
'''
'''  1. <c>ThemeManager.Traverse</c> RECURGE în copiii oricărui control care nu e
'''     <see cref="IThemedControl"/>. Copiii interni ai benzii de căutare (TextBox-ul, eticheta,
'''     butonul ✕) cădeau pe regulile generice — TextBox → <c>InputBackColor</c>, Label →
'''     <c>TextColor</c>/Transparent — deci <c>SearchBoxBackColor</c> era pierdut la fiecare
'''     aplicare de temă. Implementând <see cref="IThemedControl"/>, traversarea se OPREȘTE aici
'''     (vezi comentariul din interfață: aceeași capcană a lovit deja TextBox-ul din KBotTextField).
'''  2. <c>MainForm.OnThemeChanged</c> (și cele patru vederi) suprascriau explicit culorile.
'''     Nu mai fac asta — arborele își ia singur paleta.
'''
''' CONTRACTUL DE CULOARE: fiecare culoare publică a arborelui folosește <c>Color.Empty</c> drept
''' «auto». Auto = valoarea din câmpul-pereche <c>_auto*</c> de mai jos, pe care
''' <see cref="ApplyTheme"/> o rescrie din paletă. O culoare aleasă în designer NU e Empty, deci
''' câștigă mereu — regula cerută. Valorile inițiale ale câmpurilor <c>_auto*</c> sunt exact
''' culorile hardcodate dinainte de tematizare, ca un host NEtematizat (bancul de probă, calea
''' FOREXE/VBA) să arate neschimbat. Perechea <c>ShouldSerialize*</c>/<c>Reset*</c> din partiala
''' .Properties face ca designerul să scrie o linie DOAR pentru o alegere reală a operatorului.
''' </summary>
Partial Public Class AdvancedTreeControl
    Implements IThemedControl

    ' ── Culorile «auto» (fallback pentru orice proprietate lăsată Empty) ──────────
    Private _autoHeaderBack As Color = Color.FromArgb(222, 222, 222)
    Private _autoHeaderFore As Color = Color.FromArgb(50, 50, 60)
    Private _autoSearchBack As Color = Color.FromArgb(222, 222, 222)
    Private _autoSearchBoxBack As Color = Color.Empty      ' Empty ⇒ cade pe Me.BackColor
    Private _autoHoverBack As Color = Color.FromArgb(230, 240, 255)
    Private _autoSelectedBack As Color = Color.FromArgb(200, 220, 255)
    Private _autoSelectedBorder As Color = Color.FromArgb(150, 180, 255)
    Private _autoLine As Color = Color.FromArgb(160, 160, 160)
    Private _autoBorder As Color = Color.Transparent
    Private _autoTooltipBack As Color = Color.FromArgb(255, 255, 232)
    Private _autoTooltipFore As Color = Color.FromArgb(50, 50, 60)

    ' Suprafața/textul nodurilor sunt Control.BackColor/ForeColor. Tema le rescrie DOAR cât timp
    ' operatorul nu le-a fixat el în designer — de aceea reținem cine a scris ultimul.
    Private _backColorPinned As Boolean = False
    Private _foreColorPinned As Boolean = False
    Private _autoNodeBack As Color = Color.White     ' implicitul istoric al arborelui
    Private _autoNodeFore As Color = Color.Empty     ' Empty ⇒ lasă Control să decidă

    <Category("K-BOT Arbore - Culori")>
    <Description("Fundalul zonei de noduri; nefixat aici, urmează tema.")>
    Public Overrides Property BackColor As Color
        Get
            Return MyBase.BackColor
        End Get
        Set(value As Color)
            _backColorPinned = True
            MyBase.BackColor = value
        End Set
    End Property

    ''' <summary>
    ''' CRITIC — fără perechea asta, culoarea din temă se scurge în fișierul de designer al
    ''' formularului gazdă. <c>Control.ShouldSerializeBackColor</c> întoarce True de îndată ce
    ''' proprietatea a fost SCRISĂ vreodată, iar noi o scriem de două ori pe cont propriu: în
    ''' constructor (albul implicit) și în <see cref="ApplyTheme"/>. Visual Studio ar serializa
    ''' atunci un «tree.BackColor = …» pe care nimeni nu l-a ales, iar la următoarea încărcare
    ''' linia ar trece prin setterul public și ar FIXA culoarea — exact bug-ul pe care felia 0027
    ''' îl repară. Așa a ajuns «tree.BackColor = Color.White» în toate cele cinci designere.
    ''' Adevărul e steagul de fixare, nu punga de proprietăți a lui Control.
    ''' </summary>
    Public Function ShouldSerializeBackColor() As Boolean
        Return _backColorPinned
    End Function

    Public Overrides Sub ResetBackColor()
        _backColorPinned = False
        If _autoNodeBack <> Color.Empty Then
            MyBase.BackColor = _autoNodeBack
        Else
            MyBase.ResetBackColor()
        End If
        Me.Invalidate()
    End Sub

    <Category("K-BOT Arbore - Culori")>
    <Description("Culoarea textului de nod; nefixată aici, urmează tema.")>
    Public Overrides Property ForeColor As Color
        Get
            Return MyBase.ForeColor
        End Get
        Set(value As Color)
            _foreColorPinned = True
            MyBase.ForeColor = value
        End Set
    End Property

    ''' <summary>Perechea lui <see cref="ShouldSerializeBackColor"/>, din același motiv.</summary>
    Public Function ShouldSerializeForeColor() As Boolean
        Return _foreColorPinned
    End Function

    Public Overrides Sub ResetForeColor()
        _foreColorPinned = False
        If _autoNodeFore <> Color.Empty Then
            MyBase.ForeColor = _autoNodeFore
        Else
            MyBase.ResetForeColor()
        End If
        Me.Invalidate()
    End Sub

    ' ── Font: EXACT aceeași capcană ca BackColor ─────────────────────────────────
    ' Constructorul pune «Segoe UI, 9», deci Control.ShouldSerializeFont răspundea True și
    ' designerul scria fontul în fiecare formular gazdă. Consecința nu e doar zgomot: un Font
    ' fixat NU mai moștenește fontul ambiant, iar ThemeManager.ApplyBaseFont setează fontul
    ' schemei pe FORMULAR și se bazează pe moștenire — deci arborele ar fi rămas surd la el.
    Private _fontPinned As Boolean = False

    <Category("K-BOT Arbore")>
    <Description("Fontul ambiant al controlului (etichete, casetă de căutare); nefixat aici, urmează tema.")>
    Public Overrides Property Font As Font
        Get
            Return MyBase.Font
        End Get
        Set(value As Font)
            _fontPinned = True
            MyBase.Font = value
        End Set
    End Property

    Public Function ShouldSerializeFont() As Boolean
        Return _fontPinned
    End Function

    Public Overrides Sub ResetFont()
        _fontPinned = False
        MyBase.ResetFont()
        Me.Invalidate()
    End Sub

    ''' <summary>
    ''' Reaplică schema. Culorile fixate în designer nu se ating; restul iau paleta. Repictăm și
    ''' copiii interni ai benzii de căutare — nimeni altcineva nu mai ajunge la ei, fiindcă
    ''' traversarea se oprește la acest control.
    ''' </summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette

            _autoHeaderBack = p.SurfaceAltColor
            _autoHeaderFore = p.TextColor
            _autoSearchBack = p.SurfaceAltColor
            _autoSearchBoxBack = p.InputBackColor
            _autoHoverBack = p.ButtonHoverColor
            _autoSelectedBack = p.ButtonPressedColor
            _autoSelectedBorder = p.AccentColor
            _autoLine = p.BorderColor
            _autoBorder = Color.Transparent
            _autoTooltipBack = p.SurfaceColor
            _autoTooltipFore = p.TextColor

            ' MyBase, nu Me: scrisul propriu al temei NU are voie să treacă drept alegere a
            ' operatorului (vezi ShouldSerializeBackColor).
            _autoNodeBack = p.SurfaceAltColor
            _autoNodeFore = p.TextColor
            If Not _backColorPinned Then MyBase.BackColor = _autoNodeBack
            If Not _foreColorPinned Then MyBase.ForeColor = _autoNodeFore

            ' Bara de derulare urmează întunecimea schemei dacă operatorul a lăsat Explorer
            ' (implicitul); o alegere explicită Default/DarkMode rămâne a lui.
            If _scrollBarTheme <> En_ScrollBarTheme.Default Then
                _scrollBarTheme = If(scheme.IsDark, En_ScrollBarTheme.DarkMode, En_ScrollBarTheme.Explorer)
                ApplyScrollBarTheme()
            End If

            RestyleSearchChildren()
            Me.Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("AdvancedTreeControl.ApplyTheme", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Culorile copiilor reali ai benzii de căutare. Apelată din ApplyTheme și din setterele
    ''' de culoare — e singurul loc care le atinge, de când traversarea temei nu mai intră aici.
    ''' </summary>
    Friend Sub RestyleSearchChildren()
        If _searchTextBox IsNot Nothing Then
            _searchTextBox.BackColor = SearchBoxBackColor
            _searchTextBox.ForeColor = Me.ForeColor
        End If
        If _searchBarLabel IsNot Nothing Then
            _searchBarLabel.BackColor = SearchBackColor
            _searchBarLabel.ForeColor = SearchBarLabelForeColor
        End If
        If _searchClearBtn IsNot Nothing Then
            _searchClearBtn.BackColor = SearchBoxBackColor
            _searchClearBtn.ForeColor = Me.ForeColor
        End If
    End Sub

    ''' <summary>
    ''' Capătul implicit al degradeului de antet: spre ALB dacă baza e deschisă, spre NEGRU dacă
    ''' e închisă — adică «spre alb pe temă luminoasă, spre negru pe temă întunecată» fără ca
    ''' arborele să trebuiască să știe ce temă rulează. Amestec parțial, ca banda să rămână o
    ''' nuanță a culorii de bază, nu un gradient spre alb/negru pur.
    ''' </summary>
    Friend Shared Function AutoGradientEnd(baseColor As Color) As Color
        Dim spre As Color = If(Luminance(baseColor) >= 0.5F, Color.White, Color.Black)
        Return Blend(baseColor, spre, 0.55F)
    End Function

    ' Luminanță percepută (ITU-R BT.601), 0..1.
    Friend Shared Function Luminance(c As Color) As Single
        Return (0.299F * c.R + 0.587F * c.G + 0.114F * c.B) / 255.0F
    End Function

    ' Amestec liniar: 0 = complet «de la», 1 = complet «spre».
    ' CInt pe FIECARE componentă înainte de scădere: în VB, Byte - Byte rămâne Byte, deci o
    ' diferență negativă (amestec spre o culoare mai închisă) ar arunca OverflowException.
    Friend Shared Function Blend(dela As Color, spre As Color, cantitate As Single) As Color
        Dim t As Single = Math.Max(0.0F, Math.Min(1.0F, cantitate))
        Return Color.FromArgb(255,
                              Canal(dela.R, spre.R, t),
                              Canal(dela.G, spre.G, t),
                              Canal(dela.B, spre.B, t))
    End Function

    Private Shared Function Canal(dela As Byte, spre As Byte, t As Single) As Integer
        Dim v As Single = CInt(dela) + (CInt(spre) - CInt(dela)) * t
        Return Math.Max(0, Math.Min(255, CInt(Math.Round(v))))
    End Function
End Class
