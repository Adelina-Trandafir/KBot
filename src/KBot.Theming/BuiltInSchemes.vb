''' <summary>
''' Cele patru scheme compilate în: Classic (SystemColors), Dark (baseline-ul dark
''' actual), Modern (light, plat, rotunjit) și Colorful (felia 0028 — păstrează culorile
''' din designer). Fiecare apel întoarce o instanță NOUĂ (schemele sunt mutabile prin
''' design — editorul le va modifica), deci nu partajăm referințe între apelanți.
''' </summary>
Public Module BuiltInSchemes

    Public Const ClassicName As String = "Classic"
    Public Const DarkName As String = "Dark"
    Public Const ModernName As String = "Modern"
    Public Const ColorfulName As String = "Colorful"

    ''' <summary>
    ''' Numele schemei așa cum îl vede OPERATORUL — în română.
    '''
    ''' Numele din <see cref="ThemeScheme.Name"/> e o CHEIE: cu el se persistă schema activă
    ''' (<c>ThemeStore.SaveActive</c>) și tot cu el se rezolvă înapoi
    ''' (<c>ThemeManager.ResolveByName</c>). Traduse la sursă, cheile s-ar schimba, iar prima
    ''' pornire după actualizare n-ar mai găsi schema salvată și ar cădea pe Classic. Deci cheia
    ''' rămâne în engleză și DOAR eticheta se traduce — exact despărțirea pe care o are deja
    ''' <c>CustomPopupItem</c> între <c>Key</c> și <c>Text</c>.
    '''
    ''' O schemă de utilizator (venită din AppData) își păstrează numele: e ales de operator, deci
    ''' e deja în limba lui.
    ''' </summary>
    Public Function DisplayName(schemeName As String) As String
        If String.IsNullOrWhiteSpace(schemeName) Then Return String.Empty
        Select Case schemeName.Trim().ToLowerInvariant()
            Case "classic" : Return "Clasic"
            Case "dark" : Return "Întunecat"
            Case "modern" : Return "Modern"
            Case "colorful" : Return "Colorat"
            Case Else : Return schemeName
        End Select
    End Function

    ''' <summary>
    ''' Schema implicită de prim-boot (vezi ThemeManager.Initialize). Modern: sub Classic
    ''' (UseSystemColors=True) nu se pictează nimic custom, deci cardul borderless ar
    ''' apărea ca un dialog system gri — vrem look-ul modern din start.
    ''' </summary>
    Public ReadOnly Property DefaultSchemeName As String
        Get
            Return ModernName
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
            .Accent = "#185FA5", .AccentText = "#FFFFFF", .AccentHover = "#378ADD",
            .TabAccent = "#185FA5", .TabInactive = "#ECECEC",
            .[Error] = "#C42B1C", .Success = "#0F7B0F", .Warning = "#C07000",
            .FocusRing = "#185FA5", .DisabledText = "#A0A0A0"
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

    ''' <summary>
    ''' Colorful — schema care NU rescrie culorile controalelor: <c>PreserveDesignerColors</c>
    ''' pune motorul pe «restaurează instantaneul din <see cref="DesignerBaseline"/>» în loc de
    ''' «scrie paleta». Ce vede operatorul e exact ce a autorit în designer, formular cu formular.
    '''
    ''' Paleta de mai jos NU e decorativă degeaba: controalele K-BOT care își pictează singure
    ''' interiorul (arbore, grilă, listă de navigare, benzi) au nevoie de sloturi pentru părțile
    ''' care nu sunt proprietăți de designer — hover, selecție, linii, inele de focus. Ele iau
    ''' culorile de aici; orice a fost ales explicit în designer câștigă oricum, prin contractul
    ''' «<c>Color.Empty</c> = din temă» al fiecărui control.
    '''
    ''' Două opțiuni de stil sunt deliberat NEUTRE: <c>ButtonRender = System</c> și
    ''' <c>FocusAccent = False</c>, fiindcă amândouă ar picta peste alegerile operatorului; iar
    ''' <c>BaseFontSize = 0</c> înseamnă «nu atinge fontul», la fel ca la Classic.
    ''' </summary>
    Public Function Colorful() As ThemeScheme
        Dim p As New ThemePalette With {
            .Surface = "#F4F6FB", .SurfaceAlt = "#FFFFFF",
            .Text = "#16233A", .TextDim = "#5C6B85", .Border = "#C7D2E4",
            .InputBack = "#FFFFFF", .InputText = "#16233A", .InputBorder = "#A9BBD6",
            .ButtonBack = "#E8EEF9", .ButtonBorder = "#A9BBD6",
            .ButtonHover = "#D6E4FA", .ButtonPressed = "#BBD3F6", .ButtonText = "#16233A",
            .Accent = "#2F6FED", .AccentText = "#FFFFFF", .AccentHover = "#5A8FF5",
            .TabAccent = "#E8590C", .TabInactive = "#E3EAF6",
            .[Error] = "#D62828", .Success = "#2B9348", .Warning = "#F08C00",
            .FocusRing = "#2F6FED", .DisabledText = "#97A3B6"
        }
        Dim s As New ThemeStyleOptions With {
            .UseSystemColors = False,
            .FlatControls = False,
            .ButtonRender = ButtonRenderStyle.System,
            .CornerRadius = 6,
            .BaseFontName = "Segoe UI",
            .BaseFontSize = 0F,
            .ControlPadding = New PaddingDto(0),
            .FocusAccent = False,
            .DarkTitleBar = False,
            .OwnerDrawTabs = False,
            .PreserveDesignerColors = True
        }
        Return New ThemeScheme(ColorfulName, False, p, s)
    End Function

    ''' <summary>Cele patru scheme built-in, în ordinea de afișare.</summary>
    Public Function All() As IReadOnlyList(Of ThemeScheme)
        Return New ThemeScheme() {Classic(), Dark(), Modern(), Colorful()}
    End Function

End Module
