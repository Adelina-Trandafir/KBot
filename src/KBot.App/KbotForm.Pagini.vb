Option Strict On
Imports KBot.Common

''' <summary>
''' The log and the version history as machete of their own (they used to be pages of the settings window),
''' and what the main window asks the shell when the application is about to close.
''' </summary>
Partial Public Class KbotForm

    Friend Const MachetaIstoricVersiuni As String = "istoric.versiuni"

    Private _jurnalPage As JurnalPage
    Private _istoricPage As IstoricVersiuniPage

    ''' <summary>True while an angajament is selected in the tree: the shell holds work the operator may not want to lose track of.</summary>
    Friend ReadOnly Property HasOpenWork As Boolean
        Get
            Return _currentInfo IsNot Nothing
        End Get
    End Property

    ''' <summary>«Jurnal activitate»: the log viewer as a page of the main window (one at a time).</summary>
    Private Sub DeschideJurnal()
        If _jurnalPage Is Nothing OrElse _jurnalPage.IsDisposed Then
            _jurnalPage = New JurnalPage(_apiClient)
        End If
        If Not ShowMachetaPage(_jurnalPage, MachetaRef.Create(MachetaJurnal)) Then
            _jurnalPage.Dispose()   ' another page refused to close: stay where we are
            _jurnalPage = Nothing
        End If
    End Sub

    ''' <summary>«Istoric versiuni»: what each version changed, as a page of the main window (one at a time).</summary>
    Friend Sub DeschideIstoricVersiuni()
        Try
            If _istoricPage Is Nothing OrElse _istoricPage.IsDisposed Then
                _istoricPage = New IstoricVersiuniPage()
            End If
            If Not ShowMachetaPage(_istoricPage, MachetaRef.Create(MachetaIstoricVersiuni)) Then
                _istoricPage.Dispose()
                _istoricPage = Nothing
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.DeschideIstoricVersiuni", ex)
            KBotMessage.Show(Me, "Istoricul versiunilor nu a putut fi deschis. Detalii în jurnalul de erori.",
                             "Istoric versiuni", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
End Class
