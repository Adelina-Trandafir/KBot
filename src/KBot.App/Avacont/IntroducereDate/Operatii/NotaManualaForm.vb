Option Strict On
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain

''' <summary>SLICE-AV08-10 — fereastra modală care găzduiește editorul și ribbonul notei manuale.</summary>
Public Class NotaManualaForm
    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub New(api As INotaManualaApi, gate As ReauthGate, sid As Integer, an As Integer, idOper As Integer?)
        Me.New()
        notaManualaView.Configureaza(api, gate, sid, an, idOper)
    End Sub

    Public ReadOnly Property Salvat As Boolean
        Get
            Return notaManualaView.Salvat
        End Get
    End Property

    Public ReadOnly Property Rezultat As NotaManualaRezultat
        Get
            Return notaManualaView.Rezultat
        End Get
    End Property

    Protected Overrides Async Sub OnShown(e As EventArgs)
        MyBase.OnShown(e)
        Await notaManualaView.IncarcaDateAsync()
    End Sub

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        If notaManualaView.SalvareInCurs Then e.Cancel = True
        MyBase.OnFormClosing(e)
    End Sub

    Private Sub NotaManualaView_TextChanged(sender As Object, e As EventArgs) Handles notaManualaView.TextChanged
        If String.IsNullOrEmpty(notaManualaView.Text) Then Return
        Text = notaManualaView.Text
        capBar.Text = Text
    End Sub

    'Private Sub NotaManualaView_StareActiuniSchimbata(sender As Object, e As EventArgs) Handles notaManualaView.StareActiuniSchimbata
    '    For Each buton As KBotRibbonButton In ribbon.PinnedButtons
    '        Dim stare As ViewButtonState = notaManualaView.StareActiune(buton.Key)
    '        buton.Visible = stare <> ViewButtonState.Hidden
    '        buton.Enabled = stare = ViewButtonState.Enabled
    '    Next
    '    ribbon.InvalidateLayout()
    'End Sub

    Private Sub NotaManualaView_SalvareFinalizata(sender As Object, e As EventArgs) Handles notaManualaView.SalvareFinalizata
        DialogResult = DialogResult.OK
    End Sub

    Private Async Sub Ribbon_ItemClicked(sender As Object, e As KBotRibbonItemClickedEventArgs)
        Try
            Await notaManualaView.ExecutaActiuneAsync(e.Key)
        Catch ex As Exception
            notaManualaView.ArataEroare(ex)
        End Try
    End Sub
End Class
