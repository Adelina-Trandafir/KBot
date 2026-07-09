Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Theming

''' <summary>
''' FACADĂ de compatibilitate. Suprafața publică e identică cu cea istorică, dar
''' delegă la motorul din KBot.Theming (ThemeManager). Toate call-site-urile existente
''' (WicketMonitorForm, KBOT_STANDALONE, RichTextBoxLogger, LoginForm) compilează și se
''' comportă corect FĂRĂ nicio editare.
'''
''' SetTheme(True/False) mapează pe cele două scheme built-in: Dark / Classic.
''' Constantele CLR_* devin proprietăți care întorc slotul corespunzător din paleta
''' schemei ACTIVE — evaluate de apelanți doar în ramura IsDark (If-ul VB e
''' scurt-circuitat), unde paleta Dark reproduce exact valorile legacy.
''' </summary>
Public Module KBotTheme

    ' =========================================================================
    ' STARE (delegată)
    ' =========================================================================
    Public ReadOnly Property IsDark As Boolean
        Get
            Return ThemeManager.Current.IsDark
        End Get
    End Property

    ' =========================================================================
    ' CULORI — proprietăți care oglindesc slotul din paleta activă.
    ' Corespondența CLR_* → slot (valorile Dark == constantele legacy):
    '   CLR_BG          → SurfaceAlt   (#1C1C1C dark)   — fundal secundar / inputuri
    '   CLR_BG_PANEL    → Surface      (#2D2D30 dark)   — fundal formular / panel
    '   CLR_FG          → Text         (#D2D2D2 dark)
    '   CLR_FG_DIM      → TextDim      (#737373 dark)
    '   CLR_BTN         → ButtonBack   (#3E3E42 dark)
    '   CLR_BTN_BORDER  → ButtonBorder (#555558 dark)
    '   CLR_BTN_HOVER   → ButtonHover  (#4B4B50 dark)
    '   CLR_TAB_INACTIVE→ TabInactive  (#252526 dark)
    '   CLR_TAB_ACCENT  → TabAccent    (#007ACC dark)
    ' =========================================================================
    Public ReadOnly Property CLR_BG As Color
        Get
            Return ThemeManager.Current.Palette.SurfaceAltColor
        End Get
    End Property
    Public ReadOnly Property CLR_BG_PANEL As Color
        Get
            Return ThemeManager.Current.Palette.SurfaceColor
        End Get
    End Property
    Public ReadOnly Property CLR_FG As Color
        Get
            Return ThemeManager.Current.Palette.TextColor
        End Get
    End Property
    Public ReadOnly Property CLR_FG_DIM As Color
        Get
            Return ThemeManager.Current.Palette.TextDimColor
        End Get
    End Property
    Public ReadOnly Property CLR_BTN As Color
        Get
            Return ThemeManager.Current.Palette.ButtonBackColor
        End Get
    End Property
    Public ReadOnly Property CLR_BTN_BORDER As Color
        Get
            Return ThemeManager.Current.Palette.ButtonBorderColor
        End Get
    End Property
    Public ReadOnly Property CLR_BTN_HOVER As Color
        Get
            Return ThemeManager.Current.Palette.ButtonHoverColor
        End Get
    End Property
    Public ReadOnly Property CLR_TAB_INACTIVE As Color
        Get
            Return ThemeManager.Current.Palette.TabInactiveColor
        End Get
    End Property
    Public ReadOnly Property CLR_TAB_ACCENT As Color
        Get
            Return ThemeManager.Current.Palette.TabAccentColor
        End Get
    End Property

    ' =========================================================================
    ' API PUBLIC (delegat)
    ' =========================================================================

    ''' <summary>Setează tema și o aplică imediat la toate formularele deschise.</summary>
    ''' <remarks>Comutatorul binar istoric mapează pe schemele Dark / Classic.</remarks>
    Public Sub SetTheme(dark As Boolean)
        ThemeManager.SetScheme(If(dark, BuiltInSchemes.Dark(), BuiltInSchemes.Classic()))
    End Sub

    ''' <summary>Aplică tema curentă la un singur formular / control.</summary>
    Public Sub ApplyTheme(ctrl As Control)
        ThemeManager.Apply(ctrl)
    End Sub

    ''' <summary>
    ''' Conectează subsistemele din KBot.Forexe la evenimentul de temă (o singură dată,
    ''' de la Program.vb). Motorul NU cunoaște Forexe; loggerul reacționează prin eveniment.
    ''' </summary>
    Public Sub WireSubsystems()
        ' Sincronizează paleta loggerului cu tema curentă imediat…
        RichTextBoxLogger.SetColorScheme(ThemeManager.Current.IsDark)
        ' …și la fiecare comutare ulterioară.
        AddHandler ThemeManager.ThemeChanged,
            Sub(sender, e) RichTextBoxLogger.SetColorScheme(ThemeManager.Current.IsDark)
    End Sub

End Module
