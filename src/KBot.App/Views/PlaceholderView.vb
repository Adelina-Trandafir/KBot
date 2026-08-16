Option Strict On
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' Vedere-schelet folosită pentru TOATE cele șase vederi în felia de scaffolding:
''' „«{Nume}» — în lucru" + CodAngajament când există context. Vederile reale o vor
''' înlocui una câte una în felii viitoare, fără a atinge shell-ul. Azi mai sunt trei
''' (Indicatori / Revizii / Partener — vezi <c>MainForm.CreateView</c>).
'''
''' BANDA DE OCUPARE (cerere de operator, 2026-08-15): peste text stă un
''' <see cref="KBotBusyBar"/>, ca vederea să arate că aplicația LUCREAZĂ, nu doar să scrie
''' asta. Banda merge doar cât timp vederea e PE ECRAN: shell-ul își ține vederile create și
''' doar le ascunde, iar trei cronometre de 15 ms care se învârt în spatele altei vederi
''' n-ar picta nimic și ar consuma degeaba. Vezi <see cref="Placeholder_VisibleChanged"/>.
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
        ' Vederea se creează ascunsă (shell-ul o arată după ce o andochează), deci pornirea NU se
        ' face aici — o face VisibleChanged. Dacă vine deja vizibilă, tot el o prinde.
        busy.Running = Visible
    End Sub

    ''' <summary>
    ''' Banda merge doar cât timp vederea e pe ecran. Graniță UI: logăm și înghițim.
    ''' </summary>
    Private Sub Placeholder_VisibleChanged(sender As Object, e As EventArgs) Handles Me.VisibleChanged
        Try
            busy.Running = Visible
        Catch ex As Exception
            GlobalErrorLog.Write("PlaceholderView.VisibleChanged", ex)
        End Try
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
            ' Vederea e IThemedControl, deci ThemeManager NU recurge în copiii ei — banda își ia
            ' accentul doar dacă i-l dăm noi.
            busy.ApplyTheme(scheme)
        Catch ex As Exception
            ' Boundary UI (cascada de tema): logam si inghitim.
            GlobalErrorLog.Write("PlaceholderView.ApplyTheme", ex)
        End Try
    End Sub

End Class
