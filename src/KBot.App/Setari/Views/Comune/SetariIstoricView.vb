Option Strict On
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' «Istoric versiuni» (01.10.2026): what every version of K-BOT changed, newest first, from the
''' <c>NOUTATI.md</c> that ships next to the program. Read-only, in the themed rich-text box:
''' a bold line per version, one bullet per change, the text wrapped to the page's width.
''' </summary>
Public Class SetariIstoricView
    Implements ISetariView, IThemedContainer

    Public Event StatusChanged(text As String) Implements ISetariView.StatusChanged
    Public Event BusyChanged(busy As Boolean) Implements ISetariView.BusyChanged

    Public Sub New()
        InitializeComponent()
        edIstoric.Editabil = False
    End Sub

    Public ReadOnly Property ViewKey As String Implements ISetariView.ViewKey
        Get
            Return "istoric"
        End Get
    End Property

    Public Function CanClose() As Boolean Implements ISetariView.CanClose
        Return True
    End Function

    ''' <summary>Re-reads the file each time the page comes on screen (cheap, and always current).</summary>
    Public Sub Activated() Implements ISetariView.Activated
        Try
            ReleaseNotesText.Fill(edIstoric.TextBox, ReleaseNotesText.HistoryBlocks(0))
        Catch ex As Exception
            ' UI boundary (page switch): a throw would break the switch.
            GlobalErrorLog.Write("SetariIstoricView.Activated", ex)
            RaiseEvent StatusChanged("Istoricul versiunilor nu a putut fi afișat. Detalii în jurnalul de erori.")
        End Try
    End Sub

    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            BackColor = scheme.Palette.SurfaceAltColor
        Catch ex As Exception
            GlobalErrorLog.Write("SetariIstoricView.ApplyTheme", ex)
        End Try
    End Sub

End Class
