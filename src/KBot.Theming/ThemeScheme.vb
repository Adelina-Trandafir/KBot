Imports System.Drawing

''' <summary>
''' O schemă completă de temă: nume + dark/light + paletă + opțiuni de stil.
''' Unitatea pe care o editează viitorul editor de teme; complet round-trippabilă
''' JSON (System.Text.Json, culori hex).
''' </summary>
Public NotInheritable Class ThemeScheme
    Public Property Name As String = "Untitled"
    Public Property IsDark As Boolean = False
    Public Property Palette As ThemePalette = New ThemePalette()
    Public Property Style As ThemeStyleOptions = New ThemeStyleOptions()

    Public Sub New()
    End Sub

    Public Sub New(name As String, isDark As Boolean, palette As ThemePalette, style As ThemeStyleOptions)
        Me.Name = name
        Me.IsDark = isDark
        Me.Palette = palette
        Me.Style = style
    End Sub

    ' SLICE-0130: the quiet-border rule. Classic and Modern share one border vocabulary:
    ' the OUTER frame of a control is SystemColors.InactiveBorder, the INNER lines of a grid
    ' (cell and header separators) are SystemColors.ActiveBorder. Dark and Colorful are untouched.

    ''' <summary>True for the built-in Classic and Modern schemes (the ones the quiet-border rule covers).</summary>
    Public Function UsesQuietBorders() As Boolean
        If IsDark Then Return False
        Return String.Equals(Name, BuiltInSchemes.ClassicName, StringComparison.OrdinalIgnoreCase) OrElse
               String.Equals(Name, BuiltInSchemes.ModernName, StringComparison.OrdinalIgnoreCase)
    End Function

    ''' <summary>
    ''' The background of a plain <c>Panel</c> (and of <c>KBotTableLayoutPanel</c> when unpinned): the one rule for every
    ''' panel in the app. Classic = <c>SystemColors.Window</c>, Dark = SurfaceAlt (its window slot), any other scheme = Surface.
    ''' A <c>Tag = "Card"</c> panel keeps SurfaceAlt (ThemeManager.IsCard). SLICE-0129-02.
    ''' </summary>
    Public Function PanelBack() As Color
        If Style IsNot Nothing AndAlso Style.UseSystemColors Then Return SystemColors.Window
        Return If(IsDark, Palette.SurfaceAltColor, Palette.SurfaceColor)
    End Function

    ''' <summary>ActiveBorder (#B4B4B4) 40% of the way to white: lighter than ActiveBorder, darker than InactiveBorder (#F4F7FC, too pale).</summary>
    Public Shared ReadOnly SoftBorder As Color = Color.FromArgb(210, 210, 210)

    ''' <summary>The outer border of a control: ActiveBorder on Classic / Modern, else <paramref name="k_fallback"/>.</summary>
    Public Function OuterBorder(k_fallback As Color) As Color
        Return If(UsesQuietBorders(), SystemColors.ActiveBorder, k_fallback)
    End Function

    ''' <summary>An inner line of a grid (between cells, between headers): <see cref="SoftBorder"/> on Classic / Modern, else <paramref name="k_fallback"/>.</summary>
    Public Function InnerLine(k_fallback As Color) As Color
        Return If(UsesQuietBorders(), SoftBorder, k_fallback)
    End Function
End Class
