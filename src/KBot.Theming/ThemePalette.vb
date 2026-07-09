Imports System.Drawing
Imports System.Text.Json.Serialization

''' <summary>
''' Sloturile semantice de culoare ale unei scheme. Stocate ca hex "#RRGGBB"
''' (proprietăți String, serializabile JSON), expuse și ca <see cref="Color"/>
''' prin proprietăți *Color read-only (marcate JsonIgnore — nu se serializează,
''' se derivă din hex). Superset care acoperă tot ce folosește styler-ul curent
''' plus extrele „modern”.
''' </summary>
Public NotInheritable Class ThemePalette

    ' ── Suprafețe & text ──────────────────────────────────────────────────────
    Public Property Surface As String = "#F0F0F0"       ' fundal formular / panel
    Public Property SurfaceAlt As String = "#FFFFFF"     ' fundal secundar (card, tab activ)
    Public Property Text As String = "#000000"
    Public Property TextDim As String = "#737373"
    Public Property Border As String = "#B4B4B4"

    ' ── Câmpuri de input ──────────────────────────────────────────────────────
    Public Property InputBack As String = "#FFFFFF"
    Public Property InputText As String = "#000000"
    Public Property InputBorder As String = "#B4B4B4"

    ' ── Butoane ───────────────────────────────────────────────────────────────
    Public Property ButtonBack As String = "#E1E1E1"
    Public Property ButtonBorder As String = "#ADADAD"
    Public Property ButtonHover As String = "#E5F1FB"
    Public Property ButtonPressed As String = "#CCE4F7"
    Public Property ButtonText As String = "#000000"

    ' ── Accent ────────────────────────────────────────────────────────────────
    Public Property Accent As String = "#007ACC"
    Public Property AccentText As String = "#FFFFFF"
    Public Property AccentHover As String = "#1C97EA"

    ' ── Tab-uri ───────────────────────────────────────────────────────────────
    Public Property TabAccent As String = "#007ACC"
    Public Property TabInactive As String = "#252526"

    ' ── Stări ─────────────────────────────────────────────────────────────────
    Public Property [Error] As String = "#BE1E1E"
    Public Property Success As String = "#009933"
    Public Property Warning As String = "#E18C00"
    Public Property FocusRing As String = "#007ACC"
    Public Property DisabledText As String = "#737373"

    ' ── Accesori Color (derivați din hex; excluși din JSON) ────────────────────
    <JsonIgnore> Public ReadOnly Property SurfaceColor As Color
        Get
            Return ColorHex.FromHex(Surface)
        End Get
    End Property
    <JsonIgnore> Public ReadOnly Property SurfaceAltColor As Color
        Get
            Return ColorHex.FromHex(SurfaceAlt)
        End Get
    End Property
    <JsonIgnore> Public ReadOnly Property TextColor As Color
        Get
            Return ColorHex.FromHex(Text)
        End Get
    End Property
    <JsonIgnore> Public ReadOnly Property TextDimColor As Color
        Get
            Return ColorHex.FromHex(TextDim)
        End Get
    End Property
    <JsonIgnore> Public ReadOnly Property BorderColor As Color
        Get
            Return ColorHex.FromHex(Border)
        End Get
    End Property
    <JsonIgnore> Public ReadOnly Property InputBackColor As Color
        Get
            Return ColorHex.FromHex(InputBack)
        End Get
    End Property
    <JsonIgnore> Public ReadOnly Property InputTextColor As Color
        Get
            Return ColorHex.FromHex(InputText)
        End Get
    End Property
    <JsonIgnore> Public ReadOnly Property InputBorderColor As Color
        Get
            Return ColorHex.FromHex(InputBorder)
        End Get
    End Property
    <JsonIgnore> Public ReadOnly Property ButtonBackColor As Color
        Get
            Return ColorHex.FromHex(ButtonBack)
        End Get
    End Property
    <JsonIgnore> Public ReadOnly Property ButtonBorderColor As Color
        Get
            Return ColorHex.FromHex(ButtonBorder)
        End Get
    End Property
    <JsonIgnore> Public ReadOnly Property ButtonHoverColor As Color
        Get
            Return ColorHex.FromHex(ButtonHover)
        End Get
    End Property
    <JsonIgnore> Public ReadOnly Property ButtonPressedColor As Color
        Get
            Return ColorHex.FromHex(ButtonPressed)
        End Get
    End Property
    <JsonIgnore> Public ReadOnly Property ButtonTextColor As Color
        Get
            Return ColorHex.FromHex(ButtonText)
        End Get
    End Property
    <JsonIgnore> Public ReadOnly Property AccentColor As Color
        Get
            Return ColorHex.FromHex(Accent)
        End Get
    End Property
    <JsonIgnore> Public ReadOnly Property AccentTextColor As Color
        Get
            Return ColorHex.FromHex(AccentText)
        End Get
    End Property
    <JsonIgnore> Public ReadOnly Property AccentHoverColor As Color
        Get
            Return ColorHex.FromHex(AccentHover)
        End Get
    End Property
    <JsonIgnore> Public ReadOnly Property TabAccentColor As Color
        Get
            Return ColorHex.FromHex(TabAccent)
        End Get
    End Property
    <JsonIgnore> Public ReadOnly Property TabInactiveColor As Color
        Get
            Return ColorHex.FromHex(TabInactive)
        End Get
    End Property
    <JsonIgnore> Public ReadOnly Property ErrorColor As Color
        Get
            Return ColorHex.FromHex([Error])
        End Get
    End Property
    <JsonIgnore> Public ReadOnly Property SuccessColor As Color
        Get
            Return ColorHex.FromHex(Success)
        End Get
    End Property
    <JsonIgnore> Public ReadOnly Property WarningColor As Color
        Get
            Return ColorHex.FromHex(Warning)
        End Get
    End Property
    <JsonIgnore> Public ReadOnly Property FocusRingColor As Color
        Get
            Return ColorHex.FromHex(FocusRing)
        End Get
    End Property
    <JsonIgnore> Public ReadOnly Property DisabledTextColor As Color
        Get
            Return ColorHex.FromHex(DisabledText)
        End Get
    End Property

End Class
