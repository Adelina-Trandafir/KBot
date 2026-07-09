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
End Class
