Imports System.Windows.Forms

''' <summary>
''' Formular de bază care se auto-tematizează. Formularele noi îl moștenesc și primesc
''' tema gratis; cele existente migrează incremental (schimbă „Inherits Form” →
''' „Inherits KBot.Theming.KBotThemedForm” în .Designer.vb și șterg apelul propriu de
''' ApplyTheme din Load).
'''
''' La comutare live, difuzarea din ThemeManager.SetScheme deja a reaplicat tema pe
''' acest formular (e în lista de ținte), deci handler-ul de eveniment rulează DOAR
''' <see cref="OnThemeChanged"/> — fără dublu Apply.
''' </summary>
Public Class KBotThemedForm
    Inherits Form

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        ThemeManager.RegisterForm(Me)
        ThemeManager.Apply(Me)
        OnThemeChanged()
        AddHandler ThemeManager.ThemeChanged, AddressOf HandleThemeChanged
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        RemoveHandler ThemeManager.ThemeChanged, AddressOf HandleThemeChanged
        ThemeManager.UnregisterForm(Me)
        MyBase.OnFormClosed(e)
    End Sub

    Private Sub HandleThemeChanged(sender As Object, e As EventArgs)
        ' Apply s-a executat deja în difuzarea SetScheme; aici doar re-citim culorile semantice.
        OnThemeChanged()
    End Sub

    ''' <summary>
    ''' Suprascrie ca să re-aplici culori semantice theme-aware după o comutare de schemă
    ''' (ex. WicketMonitorForm cu proprietățile sale Clr*).
    ''' </summary>
    Protected Overridable Sub OnThemeChanged()
    End Sub

End Class
