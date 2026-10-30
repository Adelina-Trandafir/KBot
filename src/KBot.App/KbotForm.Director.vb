Option Strict On
Imports KBot.Common
Imports KBot.Domain

''' <summary>
''' The director's list of documents waiting for the signature (slice 0081-06), opened from the ribbon button of the
''' director's main window (<see cref="DirectorMode"/>) as a window of its own, on top of the shell, and from the tray
''' application's request (<see cref="RaspundeCereriiDinTray"/>, slice TRAY-01).
''' </summary>
Partial Public Class KbotForm

    ' Re-entry guard: a second press while the window is open does nothing.
    Private _directorDeSemnatDeschis As Boolean
    ' The open list, to point it at another document when the tray asks while it is open.
    Private _directorDeSemnat As DirectorForm

    ''' <summary>
    ''' Opens the list modally, with the revision <paramref name="k_idrev"/> already picked when it is not 0 (the tray's
    ''' click on a row). The shell is put away first (its Adobe closed): the list's document view shares the one Acrobat
    ''' broker. UI boundary: logged and shown.
    ''' </summary>
    Friend Sub DeschideDirectorDeSemnat(Optional k_idrev As Integer = 0)
        Try
            If Not DirectorForm.AreListaDeSemnat(_session) Then Return
            If _directorDeSemnatDeschis Then
                ' Already open: the tray's click on a document points the open list at it.
                If k_idrev > 0 AndAlso _directorDeSemnat IsNot Nothing Then _directorDeSemnat.SelecteazaRevizia(k_idrev)
                Return
            End If
            _directorDeSemnatDeschis = True
            Try
                PutAway()
                Using k_form As New DirectorForm(_apiClient, _session, _authApi, _loginFactory)
                    k_form.IdrevInitial = k_idrev
                    _directorDeSemnat = k_form
                    k_form.ShowDialog(If(HostForm, CType(Me, Form)))
                End Using
            Finally
                _directorDeSemnat = Nothing
                _directorDeSemnatDeschis = False
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("KbotForm.DeschideDirectorDeSemnat", ex)
            KBotMessage.Show(Me, "Lista documentelor de semnat nu a putut fi deschisă. Detalii în jurnalul de erori.",
                             "Documente de semnat", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>
    ''' The tray application asked for the list of documents to sign, maybe with one document picked (<c>TrayRequest</c>).
    ''' A document of another unit than the one in the session is reached by opening that unit first, the way the unit
    ''' selector does it; only a unit where the role is «Director» can be asked for. Other requests (the main window only) and
    ''' other roles need nothing: the window is already up. UI boundary: logged and shown.
    ''' </summary>
    Friend Async Sub RaspundeCereriiDinTray(k_request As TrayRequest)
        Try
            If k_request Is Nothing OrElse Not DirectorForm.AreListaDeSemnat(_session) Then Return
            If Not String.Equals(k_request.Action, TrayRequest.ActionSign, StringComparison.Ordinal) Then Return

            Dim k_dc As String = If(k_request.Dc, String.Empty)
            If k_dc.Length > 0 AndAlso Not String.Equals(k_dc, _session.DbName, StringComparison.Ordinal) Then
                ' An open list belongs to the unit in the session: it is not pulled to another unit under the operator's hands.
                If _directorDeSemnatDeschis OrElse _schimbaUnitatea OrElse _controller.IsBusy Then Return
                Dim k_unit As UnitInfo = _unitati?.FirstOrDefault(
                    Function(k_u) String.Equals(k_u.DC, k_dc, StringComparison.Ordinal) AndAlso DirectorForm.AreListaRol(k_u.Rol))
                If k_unit IsNot Nothing Then Await SchimbaUnitateaAsync(k_unit)
            End If
            DeschideDirectorDeSemnat(k_request.Idrev)
        Catch ex As Exception
            GlobalErrorLog.Write("KbotForm.RaspundeCereriiDinTray", ex)
            KBotMessage.Show(Me, "Documentul cerut din zona de notificări nu a putut fi deschis. Detalii în jurnalul de erori.",
                             "Documente de semnat", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub
End Class
