''' <summary>
''' Cele trei scheme compilate în: Classic (SystemColors), Dark (baseline-ul dark
''' actual), Modern (light, plat, rotunjit). Fiecare apel întoarce o instanță NOUĂ
''' (schemele sunt mutabile prin design — editorul le va modifica), deci nu partajăm
''' referințe între apelanți.
''' </summary>
Public Module BuiltInSchemes

    Public Const ClassicName As String = "Classic"
    Public Const DarkName As String = "Dark"
    Public Const ModernName As String = "Modern"

    ''' <summary>
    ''' Schema implicită de prim-boot (vezi ThemeManager.Initialize). Classic =
    ''' „arată ca designerul”: SystemColors, zero pictură custom.
    ''' </summary>
    Public ReadOnly Property DefaultSchemeName As String
        Get
            Return ClassicName
        End Get
    End Property

    ''' <summary>Classic — culori system, fără pictură custom. Reproduce look-ul default VB.NET.</summary>
    Public Function Classic() As ThemeScheme
        Dim p As New ThemePalette With {
            .Surface = "#F0F0F0", .SurfaceAlt = "#FFFFFF",
            .Text = "#000000", .TextDim = "#6D6D6D", .Border = "#B4B4B4",
            .InputBack = "#FFFFFF", .InputText = "#000000", .InputBorder = "#7A7A7A",
            .ButtonBack = "#E1E1E1", .ButtonBorder = "#ADADAD",
            .ButtonHover = "#E5F1FB", .ButtonPressed = "#CCE4F7", .ButtonText = "#000000",
            .Accent = "#0078D7", .AccentText = "#FFFFFF", .AccentHover = "#1C97EA",
            .TabAccent = "#0078D7", .TabInactive = "#F0F0F0",
            .[Error] = "#BE1E1E", .Success = "#009933", .Warning = "#E18C00",
            .FocusRing = "#0078D7", .DisabledText = "#6D6D6D"
        }
        Dim s As New ThemeStyleOptions With {
            .UseSystemColors = True,
            .FlatControls = False,
            .ButtonRender = ButtonRenderStyle.System,
            .CornerRadius = 0,
            .BaseFontName = "Segoe UI",
            .BaseFontSize = 0F,
            .ControlPadding = New PaddingDto(0),
            .FocusAccent = False,
            .DarkTitleBar = False,
            .OwnerDrawTabs = False
        }
        Return New ThemeScheme(ClassicName, False, p, s)
    End Function

    ''' <summary>
    ''' Dark — paleta = constantele CLR_* legacy, mapate pe sloturile noi. Reproduce
    ''' exact look-ul dark actual (baseline de regresie). Vezi facada KBotTheme pentru
    ''' corespondența CLR_* → slot.
    ''' </summary>
    Public Function Dark() As ThemeScheme
        Dim p As New ThemePalette With {
            .Surface = "#2D2D30",       ' CLR_BG_PANEL — fundal formular/panel
            .SurfaceAlt = "#1C1C1C",    ' CLR_BG — fundal secundar
            .Text = "#D2D2D2",          ' CLR_FG
            .TextDim = "#737373",       ' CLR_FG_DIM
            .Border = "#555558",        ' CLR_BTN_BORDER
            .InputBack = "#1C1C1C",     ' CLR_BG — inputuri + tab activ + tabpage
            .InputText = "#D2D2D2",     ' CLR_FG
            .InputBorder = "#555558",
            .ButtonBack = "#3E3E42",    ' CLR_BTN
            .ButtonBorder = "#555558",  ' CLR_BTN_BORDER
            .ButtonHover = "#4B4B50",   ' CLR_BTN_HOVER
            .ButtonPressed = "#2D2D30",
            .ButtonText = "#D2D2D2",    ' CLR_FG
            .Accent = "#007ACC",        ' CLR_TAB_ACCENT
            .AccentText = "#FFFFFF",
            .AccentHover = "#1C97EA",
            .TabAccent = "#007ACC",     ' CLR_TAB_ACCENT
            .TabInactive = "#252526",   ' CLR_TAB_INACTIVE
            .[Error] = "#F07878", .Success = "#3FB950", .Warning = "#E18C00",
            .FocusRing = "#007ACC", .DisabledText = "#737373"
        }
        Dim s As New ThemeStyleOptions With {
            .UseSystemColors = False,
            .FlatControls = True,
            .ButtonRender = ButtonRenderStyle.Flat,
            .CornerRadius = 0,
            .BaseFontName = "Segoe UI",
            .BaseFontSize = 0F,
            .ControlPadding = New PaddingDto(0),
            .FocusAccent = False,
            .DarkTitleBar = True,
            .OwnerDrawTabs = True
        }
        Return New ThemeScheme(DarkName, True, p, s)
    End Function

    ''' <summary>
    ''' Modern — paletă light modernă, controale plate, colțuri rotunjite, Segoe UI
    ''' Variable, focus accent. Payload-ul vizual care omoară look-ul „1998”.
    ''' </summary>
    Public Function Modern() As ThemeScheme
        Dim p As New ThemePalette With {
            .Surface = "#FAFAFA", .SurfaceAlt = "#FFFFFF",
            .Text = "#1E1E1E", .TextDim = "#6E6E6E", .Border = "#E2E2E2",
            .InputBack = "#FFFFFF", .InputText = "#1E1E1E", .InputBorder = "#CCCCCC",
            .ButtonBack = "#F3F3F3", .ButtonBorder = "#D0D0D0",
            .ButtonHover = "#E8F1FB", .ButtonPressed = "#CCE4F7", .ButtonText = "#1E1E1E",
            .Accent = "#007ACC", .AccentText = "#FFFFFF", .AccentHover = "#1C97EA",
            .TabAccent = "#007ACC", .TabInactive = "#ECECEC",
            .[Error] = "#C42B1C", .Success = "#0F7B0F", .Warning = "#C07000",
            .FocusRing = "#007ACC", .DisabledText = "#A0A0A0"
        }
        Dim s As New ThemeStyleOptions With {
            .UseSystemColors = False,
            .FlatControls = True,
            .ButtonRender = ButtonRenderStyle.ModernOwnerDrawn,
            .CornerRadius = 8,
            .BaseFontName = "Segoe UI Variable Text",
            .BaseFontSize = 9.0F,
            .ControlPadding = New PaddingDto(12, 8, 12, 8),
            .FocusAccent = True,
            .DarkTitleBar = False,
            .OwnerDrawTabs = False
        }
        Return New ThemeScheme(ModernName, False, p, s)
    End Function

    ''' <summary>Cele trei scheme built-in, în ordinea de afișare.</summary>
    Public Function All() As IReadOnlyList(Of ThemeScheme)
        Return New ThemeScheme() {Classic(), Dark(), Modern()}
    End Function

End Module
