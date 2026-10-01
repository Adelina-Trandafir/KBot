Option Strict On
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' The update offer (replaces the plain message box, 01.10.2026): the headline in bold, one
''' bullet per change from the release notes, then the size and the question. The notes sit in the
''' themed read-only <c>KBotRichTextEditor</c>: it follows the theme (dark or not) and wraps the
''' text to its width, so nothing needs a horizontal scroll.
''' <para>Result: <c>OK</c> = the operator accepts, <c>Cancel</c> = refuses. Without a «no» text it
''' is a plain reader with one «close» button («Ce e nou?»).</para>
''' </summary>
Public Class UpdateOfferForm

    Private ReadOnly _blocks As List(Of NoteBlock)

    ''' <param name="noText">Nothing = no question and a single button (<paramref name="yesText"/>).</param>
    Friend Sub New(title As String, blocks As List(Of NoteBlock), question As String,
                   yesText As String, noText As String)
        InitializeComponent()
        _blocks = If(blocks, New List(Of NoteBlock)())
        capBar.Text = title
        Text = title
        btnDa.Text = yesText
        edNoutati.Editabil = False
        If noText Is Nothing Then
            lblIntrebare.Visible = False
            btnNu.Visible = False
            btnDa.Left = btnNu.Left
            CancelButton = btnDa
        Else
            lblIntrebare.Text = If(question, String.Empty)
            btnNu.Text = noText
        End If
        Try
            capBar.IconImage = My.Resources.kbot_64
        Catch ex As Exception
            ' The icon is cosmetic; its absence must not stop the offer.
            GlobalErrorLog.Write("UpdateOfferForm.New", ex)
        End Try
    End Sub

    ' Filled after Load: the theme and the text-size zoom have set the font by then, and the
    ' paragraphs derive their fonts from it.
    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        Try
            ReleaseNotesText.Fill(edNoutati.TextBox, _blocks)
        Catch ex As Exception
            GlobalErrorLog.Write("UpdateOfferForm.OnLoad", ex)
        End Try
    End Sub

    ''' <summary>«Ce e nou?»: the last versions, read-only, one «Închide» button.</summary>
    Friend Shared Sub ShowRecent(owner As IWin32Window)
        Try
            Using dlg As New UpdateOfferForm("Ce e nou?",
                    ReleaseNotesText.HistoryBlocks(ReleaseNotesText.RecentVersions), Nothing, "Închide", Nothing)
                If owner Is Nothing Then dlg.ShowDialog() Else dlg.ShowDialog(owner)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("UpdateOfferForm.ShowRecent", ex)
            Throw
        End Try
    End Sub

End Class
