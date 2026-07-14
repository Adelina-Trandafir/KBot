Option Strict On
Imports KBot.Common
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
        Try
            _info = info
            UpdateText()
        Catch ex As Exception
            GlobalErrorLog.Write("PlaceholderView.SetContext", ex)
            Throw
        End Try
    End Sub

    Private Sub UpdateText()
        Try
            Dim text As String = $"«{_displayName}» — în lucru"
            If _info IsNot Nothing AndAlso Not String.IsNullOrEmpty(_info.CodAngajament) Then
                text &= Environment.NewLine & Environment.NewLine & "Angajament: " & _info.CodAngajament
            End If
            lblMessage.Text = text
        Catch ex As Exception
            GlobalErrorLog.Write("PlaceholderView.UpdateText", ex)
            Throw
        End Try
    End Sub

    ''' <summary>Reaplică culorile schemei (vederea stă pe cardul viewHost — SurfaceAlt).</summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            BackColor = scheme.Palette.SurfaceAltColor
            lblMessage.ForeColor = scheme.Palette.TextDimColor
            lblMessage.BackColor = scheme.Palette.SurfaceAltColor
        Catch ex As Exception
            ' Boundary UI (cascada de tema): logam si inghitim.
            GlobalErrorLog.Write("PlaceholderView.ApplyTheme", ex)
        End Try
    End Sub

End Class
