Option Strict On

''' <summary>
''' Ce s-a ales din meniul butonului de temă al barei de titlu (vezi
''' <c>KBotCaptionBar.ThemeSchemeChanged</c>).
'''
''' Schimbarea propriu-zisă e deja făcută când ajunge evenimentul: bara a chemat
''' <c>ThemeManager.SetScheme</c>, iar acesta a difuzat schema PESTE TOT (registrul de formulare
''' tematizate ∪ <c>Application.OpenForms</c>). Deci gazda NU trebuie să reaplice nimic — cine
''' vrea ceva în plus (o culoare semantică proprie, o pictogramă care depinde de schemă) o face
''' aici, la fel ca în <c>OnThemeChanged</c>.
''' </summary>
Public NotInheritable Class ThemeSchemeChangedEventArgs
    Inherits EventArgs

    Public Sub New(scheme As ThemeScheme)
        ' Câmpul din spatele proprietății ReadOnly: în VB proprietatea nu se poate atribui nici
        ' măcar din constructor (vezi CustomPopupItemEventArgs).
        _Scheme = scheme
    End Sub

    ''' <summary>Schema tocmai aplicată (niciodată Nothing).</summary>
    Public ReadOnly Property Scheme As ThemeScheme

End Class
