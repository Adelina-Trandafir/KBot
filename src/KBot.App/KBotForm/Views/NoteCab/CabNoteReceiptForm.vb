Option Strict On
Imports System.Diagnostics
Imports System.IO
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' «Nota trimisă în CAB» (slice 0088-04, operator 28.09.2026) -- the answer to the upload of a
''' correction note, in its own window instead of a message box, with «Verifică recipisa»: K-BOT
''' looks in the FOREXE SNM inbox (category 1) for the message of the upload's registration index,
''' downloads the receipt and stores it on the server (FX_NoteCAB_Recipisa, on the note's PDF).
'''
''' <para>Opened in two ways: right after an upload (the answer and its index), and from the
''' «Recipisă» page of the notes view («Validează documentul») when the note has no receipt yet --
''' then the headline says so and the index is the one stored on the note.</para>
'''
''' <para>The search itself lives in the shell (it owns the FOREXE coordinator and the API client):
''' <paramref name="verify"/> takes the index and returns what happened. The window only shows.</para>
''' </summary>
Public Class CabNoteReceiptForm

    Private ReadOnly _note As CabCorrectionNote
    Private ReadOnly _isError As Boolean
    Private ReadOnly _verify As Func(Of String, Task(Of CabReceiptCheck))
    Private _busy As Boolean
    Private _localPath As String

    ''' <summary>The receipt stored on the server by this window; Nothing = none.</summary>
    Public ReadOnly Property SavedReceipt As CabNoteReceipt

    ''' <param name="headline">One line: what happened («Nota nr. 3 a fost trimisă în FOREXE.»).</param>
    ''' <param name="details">FOREXE's answer, or the reason of the failure.</param>
    ''' <param name="isError">The upload failed: the headline is shown in the error colour.</param>
    ''' <param name="index">The registration index, when known (editable).</param>
    Public Sub New(note As CabCorrectionNote, headline As String, details As String, isError As Boolean,
                   index As String, verify As Func(Of String, Task(Of CabReceiptCheck)))
        ArgumentNullException.ThrowIfNull(note)
        ArgumentNullException.ThrowIfNull(verify)
        InitializeComponent()
        _note = note
        _isError = isError
        _verify = verify
        lblTitlu.Text = If(headline, String.Empty)
        txtRaspuns.Text = If(String.IsNullOrWhiteSpace(details), "(FOREXE nu a afișat niciun text)",
                             details.Replace(vbCrLf, vbLf).Replace(vbLf, Environment.NewLine))
        txtIndex.Text = If(index, String.Empty)
        capBar.Text = $"K-BOT — Nota de corecție nr. {note.NoteNumber} în CAB"
        Text = capBar.Text
        ' Operator (28.09.2026): a note that has its receipt has nothing left to check.
        If note.Receipt IsNot Nothing Then ShowReceiptStored(note.Receipt)
    End Sub

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim p As ThemePalette = ThemeManager.Current?.Palette
            If p Is Nothing Then Return
            lblTitlu.ForeColor = If(_isError, p.ErrorColor, p.SuccessColor)
            btnVerifica.BackColor = p.AccentColor
            btnVerifica.ForeColor = p.AccentTextColor
            btnVerifica.FlatAppearance.BorderColor = p.AccentColor
        Catch ex As Exception
            GlobalErrorLog.Write("CabNoteReceiptForm.OnThemeChanged", ex)
        End Try
    End Sub

    ' UI boundary: log and tell.
    Private Async Sub btnVerifica_Click(sender As Object, e As EventArgs) Handles btnVerifica.Click
        Try
            If _busy Then Return
            Dim index As String = If(txtIndex.Text, String.Empty).Trim()
            If Not CabCorrectionNoteRules.IsRegistrationIndex(index) Then
                lblStare.Text = "Scrieți indexul de înregistrare FOREXE (doar cifre, ex. 1230450081)."
                txtIndex.Focus()
                Return
            End If
            SetBusy(True)
            lblStare.Text = $"Caut recipisa pentru indexul {index} în FOREXE…"
            Dim check As CabReceiptCheck = Await _verify(index).ConfigureAwait(True)
            If IsDisposed Then Return
            lblStare.Text = If(check?.Message, String.Empty)
            If check IsNot Nothing AndAlso check.Found Then
                _SavedReceipt = check.Receipt
                ShowReceiptStored(check.Receipt)
                lblStare.Text = check.Message
            End If
            ' A receipt downloaded but refused by the server is still on this computer.
            If check IsNot Nothing AndAlso Not String.IsNullOrEmpty(check.LocalPath) Then _localPath = check.LocalPath
            btnDeschide.Enabled = Not String.IsNullOrEmpty(_localPath) AndAlso File.Exists(_localPath)
        Catch ex As Exception
            GlobalErrorLog.Write("CabNoteReceiptForm.btnVerifica_Click", ex)
            If Not IsDisposed Then lblStare.Text = "Căutarea recipisei s-a oprit. Detalii în jurnalul de erori."
        Finally
            If Not IsDisposed Then SetBusy(False)
        End Try
    End Sub

    ' UI boundary: log and tell.
    Private Sub btnDeschide_Click(sender As Object, e As EventArgs) Handles btnDeschide.Click
        Try
            If String.IsNullOrEmpty(_localPath) OrElse Not File.Exists(_localPath) Then Return
            Process.Start(New ProcessStartInfo(_localPath) With {.UseShellExecute = True})
        Catch ex As Exception
            GlobalErrorLog.Write("CabNoteReceiptForm.btnDeschide_Click", ex)
            lblStare.Text = "Recipisa nu a putut fi deschisă: " & ex.Message
        End Try
    End Sub

    ' The receipt is on the server: «Verifică recipisa» goes away, the index is fixed.
    Private Sub ShowReceiptStored(receipt As CabNoteReceipt)
        btnVerifica.Visible = False
        txtIndex.Text = receipt.RegistrationIndex
        txtIndex.ReadOnly = True
        Dim local As String = CabNoteFiles.ReceiptPath(_note, receipt.RegistrationIndex, receipt.FileName)
        If String.IsNullOrEmpty(_localPath) AndAlso Not String.IsNullOrEmpty(local) AndAlso File.Exists(local) Then _localPath = local
        btnDeschide.Enabled = Not String.IsNullOrEmpty(_localPath)
        lblStare.Text = $"Recipisa pentru indexul {receipt.RegistrationIndex} este pe server (pagina «Recipisă» a notei)."
    End Sub

    Private Sub SetBusy(busy As Boolean)
        _busy = busy
        btnVerifica.Enabled = Not busy
        btnInchide.Enabled = Not busy
        txtIndex.Enabled = Not busy
        UseWaitCursor = busy
    End Sub

    ' The window cannot close while the robot is searching (the answer would have no one to land on).
    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        If _busy Then e.Cancel = True
        MyBase.OnFormClosing(e)
    End Sub

End Class

''' <summary>What one «Verifică recipisa» did (slice 0088-04). POCO.</summary>
Public NotInheritable Class CabReceiptCheck
    ''' <summary>The receipt was found, downloaded and stored on the server.</summary>
    Public Property Found As Boolean
    ''' <summary>The Romanian line the window shows.</summary>
    Public Property Message As String = String.Empty
    ''' <summary>The receipt as the server stored it (Found only).</summary>
    Public Property Receipt As CabNoteReceipt
    ''' <summary>The receipt's copy on this computer (Found only).</summary>
    Public Property LocalPath As String = String.Empty
End Class
