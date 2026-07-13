Option Strict On
Imports System.Windows.Forms

''' <summary>
''' Stilurile de buton ale casei — extrase din LoginForm (ApplyPrimaryButtons /
''' ApplySecondaryButton) ca să fie refolosite de orice shell (MainForm etc.).
''' Se apelează din OnThemeChanged (după ThemeManager.Apply), la fiecare comutare.
''' </summary>
Public Module ButtonStyles

    ''' <summary>
    ''' Buton primar = accent plin (din paleta activă); colțuri rotunjite când schema
    ''' folosește randarea modernă owner-drawn.
    ''' </summary>
    Public Sub ApplyPrimary(b As Button, scheme As ThemeScheme)
        If b Is Nothing Then Throw New ArgumentNullException(NameOf(b))
        If scheme Is Nothing Then Throw New ArgumentNullException(NameOf(scheme))
        Dim p As ThemePalette = scheme.Palette

        If scheme.Style.ButtonRender = ButtonRenderStyle.ModernOwnerDrawn Then
            ModernRenderer.ApplyButton(b, scheme)   ' colțuri rotunjite + handlere
        Else
            ModernRenderer.DetachButton(b)          ' scoate rotunjirea dacă venim din Modern
            b.FlatStyle = FlatStyle.Flat
            b.FlatAppearance.BorderSize = 1
        End If
        ' Peste randarea de bază, pictăm accentul (buton primar).
        b.BackColor = p.AccentColor
        b.ForeColor = p.AccentTextColor
        b.FlatAppearance.BorderColor = p.AccentColor
        b.FlatAppearance.MouseOverBackColor = p.AccentHoverColor
        b.FlatAppearance.MouseDownBackColor = p.AccentColor
        b.UseVisualStyleBackColor = False
    End Sub

    ''' <summary>
    ''' Buton secundar — plat, fără chenar, fundal SurfaceAlt, text accent. Nu trebuie
    ''' să concureze cu acțiunea primară.
    ''' </summary>
    Public Sub ApplySecondary(b As Button, scheme As ThemeScheme)
        If b Is Nothing Then Throw New ArgumentNullException(NameOf(b))
        If scheme Is Nothing Then Throw New ArgumentNullException(NameOf(scheme))
        Dim p As ThemePalette = scheme.Palette

        ModernRenderer.DetachButton(b)
        b.FlatStyle = FlatStyle.Flat
        b.FlatAppearance.BorderSize = 0
        b.BackColor = p.SurfaceAltColor
        b.ForeColor = p.AccentColor
        b.FlatAppearance.MouseOverBackColor = p.ButtonHoverColor
        b.FlatAppearance.MouseDownBackColor = p.ButtonPressedColor
        b.UseVisualStyleBackColor = False
    End Sub

End Module
