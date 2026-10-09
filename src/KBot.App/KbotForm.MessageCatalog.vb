Option Strict On
Imports KBot.Common
Imports KBot.Controls

' Slice 0112 -- the «ADMIN > Mesaje (catalog)» row of the header menu: the debug bench that lists and edits
' every message box of the application (KBot.DevHarness\Internal\MessageCatalogForm). Like the tutorial
' designer it exists in a Debug build only -- the harness assembly is not part of a Release.
Partial Public Class KbotForm

    Private Const MessageCatalogMenuKey As String = "admin_mesaje"

    Friend Shared ReadOnly Property MessageCatalogAvailable As Boolean
        Get
#If DEBUG Then
            Return True
#Else
            Return False
#End If
        End Get
    End Property

    Private Sub DeschideCatalogulDeMesaje()
#If DEBUG Then
        Try
            Using k_form As New Global.KBot.DevHarness.MessageCatalogForm()
                k_form.ShowDialog(Me)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.DeschideCatalogulDeMesaje", ex)
            KBotMessage.Show(Me, "Catalogul de mesaje nu a putut fi deschis. Detalii în jurnalul de erori.",
                             "Mesaje", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
#End If
    End Sub

End Class
