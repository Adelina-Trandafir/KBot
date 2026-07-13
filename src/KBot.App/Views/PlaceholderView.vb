Option Strict On
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' Vedere-schelet folosită pentru TOATE cele șase vederi în felia de scaffolding:
''' „«{Nume}» — în lucru" + CodAngajament când există context. Vederile reale o vor
''' înlocui una câte una în felii viitoare, fără a atinge shell-ul.
''' </summary>
Public Class PlaceholderView
    Implements IAngajamentView, IThemedControl

    Private ReadOnly _viewKey As String
    Private ReadOnly _displayName As String
    Private _info As AngajamentTreeInfo

    Public Sub New(viewKey As String, displayName As String)
        If String.IsNullOrWhiteSpace(viewKey) Then Throw New ArgumentException("Cheie vidă.", NameOf(viewKey))
        If String.IsNullOrWhiteSpace(displayName) Then Throw New ArgumentException("Nume vid.", NameOf(displayName))
        InitializeComponent()
        _viewKey = viewKey
        _displayName = displayName
        UpdateText()
    End Sub

    Public ReadOnly Property ViewKey As String Implements IAngajamentView.ViewKey
        Get
            Return _viewKey
        End Get
    End Property

    Public Sub SetContext(info As AngajamentTreeInfo) Implements IAngajamentView.SetContext
        _info = info
        UpdateText()
    End Sub

    Private Sub UpdateText()
        Dim text As String = $"«{_displayName}» — în lucru"
        If _info IsNot Nothing AndAlso Not String.IsNullOrEmpty(_info.CodAngajament) Then
            text &= Environment.NewLine & Environment.NewLine & "Angajament: " & _info.CodAngajament
        End If
        lblMessage.Text = text
    End Sub

    ''' <summary>Reaplică culorile schemei (vederea stă pe cardul viewHost — SurfaceAlt).</summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        If scheme Is Nothing Then Return
        BackColor = scheme.Palette.SurfaceAltColor
        lblMessage.ForeColor = scheme.Palette.TextDimColor
        lblMessage.BackColor = scheme.Palette.SurfaceAltColor
    End Sub

End Class
