Imports System.Threading
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain

''' <summary>
''' THE UNIT SELECTOR in the caption bar (slice 0097). An operator with two or more units sees
''' «K-BOT — [unit ▾]» instead of the plain unit name; choosing another unit opens its database
''' exactly as the login does (new session on the chosen DC, year/SS catalogue, tree), without
''' asking for the password again: <c>POST /api/auth/switch-unit</c> works on the live token.
''' With a single unit the title stays the plain «K-BOT — name» label.
''' </summary>
Partial Public Class KbotForm

    ' The operator's units, from /api/auth/my-units. Empty = unknown / not read.
    Private _unitati As IReadOnlyList(Of UnitInfo) = Array.Empty(Of UnitInfo)()
    ' A switch is running: a second choice waits for it to end.
    Private _schimbaUnitatea As Boolean

    ''' <summary>
    ''' Reads the operator's units and shows the selector when there are two or more. Never
    ''' throws: without the list the title simply stays the plain unit name.
    ''' </summary>
    Private Async Function IncarcaUnitatileAsync() As Task
        Try
            _unitati = Await WithReauth(Of IReadOnlyList(Of UnitInfo))(
                Function() _authApi.GetMyUnitsAsync(_session.Token, CancellationToken.None))
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.IncarcaUnitatileAsync", ex)
            _unitati = Array.Empty(Of UnitInfo)()
        End Try
        ArataUnitateaInTitlu()
    End Function

    ''' <summary>
    ''' The caption: the selector over the units when there are two or more (and the current one
    ''' is among them), otherwise the plain «K-BOT — name» as before.
    ''' </summary>
    Private Sub ArataUnitateaInTitlu()
        Try
            Dim dc As String = _session.DbName
            ' A director chooses among the units where the role is «Director» (the others open as an accountant's).
            Dim k_units As IEnumerable(Of UnitInfo) = If(_unitati, Array.Empty(Of UnitInfo)())
            If ModDirector Then k_units = k_units.Where(Function(u) EsteUnitateDirector(u))
            Dim k_list As List(Of UnitInfo) = k_units.ToList()
            If k_list.Count >= 2 AndAlso
               k_list.Any(Function(u) String.Equals(u.DC, dc, StringComparison.Ordinal)) Then
                capBar.Text = "K-BOT"
                capBar.SetSelectorItems(
                    k_list.Select(Function(u) New KeyValuePair(Of String, String)(u.DC, If(u.NumeUnitate, u.DC))),
                    dc)
            Else
                capBar.ClearSelector()
                capBar.Text = If(String.IsNullOrEmpty(_session.NumeUnitate), "K-BOT", "K-BOT — " & _session.NumeUnitate)
            End If
        Catch ex As Exception
            ' The title is cosmetic: logged, never a reason to stop the shell.
            GlobalErrorLog.Write("MainForm.ArataUnitateaInTitlu", ex)
        End Try
    End Sub

    ''' <summary>
    ''' The operator picked another unit. A live FOREXE session is closed first, silently: it
    ''' belongs to the current unit, and what it downloads is written to the database of the
    ''' session -- switching under it would put one unit's data into another's. Refused only while
    ''' a FOREXE operation is running.
    ''' UI boundary: logged and shown.
    ''' </summary>
    Private Async Sub CapBar_SelectorChanged(sender As Object, e As CaptionSelectorChangedEventArgs) Handles capBar.SelectorChanged
        Try
            If _schimbaUnitatea OrElse e Is Nothing Then Return
            ' The year / SS selectors of the same bar raise the same event (KbotForm.Periods.vb).
            If Not String.Equals(e.Selector, KBotCaptionBar.SelectorUnit, StringComparison.Ordinal) Then Return
            Dim tinta As UnitInfo = _unitati?.FirstOrDefault(Function(u) String.Equals(u.DC, e.Key, StringComparison.Ordinal))
            If tinta Is Nothing Then Return

            ' A running FOREXE operation is writing into the current unit: it has to end first.
            If _controller.IsBusy Then
                KBotMessage.Show(Me, "O operație FOREXE este în curs. Unitatea se poate schimba după ce se termină.",
                                "Schimbă unitatea", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            ' A unit where the role is «Director» opens for a director only (his window is narrowed to the documents); an
            ' accountant's shell does not become one in the middle of the session.
            If EsteUnitateDirector(tinta) <> ModDirector Then
                KBotMessage.Show(Me, If(ModDirector,
                                        $"Pe unitatea «{tinta.NumeUnitate}» nu aveți rolul «Director»: ea se deschide cu o autentificare nouă, ca contabil.",
                                        $"Pe unitatea «{tinta.NumeUnitate}» aveți rolul «Director»: ea se deschide cu o autentificare nouă, ca director."),
                                "Schimbă unitatea", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            Await SchimbaUnitateaAsync(tinta)
        Catch ex As ApiException
            GlobalErrorLog.Write("MainForm.CapBar_SelectorChanged", ex)
            KBotMessage.Show(Me, "Unitatea nu a putut fi schimbată: " & ex.Message,
                            "Schimbă unitatea", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.CapBar_SelectorChanged", ex)
            KBotMessage.Show(Me, "Unitatea nu a putut fi schimbată. Detalii în jurnalul de erori.",
                            "Schimbă unitatea", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    ''' <summary>
    ''' Opens <paramref name="tinta"/> in the session (the selector's switch, and the tray's request for a document of another
    ''' unit). The caller has checked that no switch is running and that the unit fits the window's mode; errors reach the
    ''' caller, which tells the operator.
    ''' </summary>
    Private Async Function SchimbaUnitateaAsync(tinta As UnitInfo) As Task
        _schimbaUnitatea = True
        busyBar.Running = True
        Try
            ' The FOREXE session belongs to the unit being left: closed first, silently (operator,
            ' 30.09.2026 -- «se deconectează și apoi se schimbă unitatea»).
            ' The remembered certificate is forgotten only when the operator asked for it
            ' («Setari → FOREXE», off by default).
            If _controller.IsConnected Then
                Await _controller.DisconnectAsync(AppSettings.Current.ForexeForgetCertificateOnUnitSwitch)
            End If

            Dim dc As String = tinta.DC
            Dim result As LoginResult = Await WithReauth(Of LoginResult)(
                Function() _authApi.SwitchUnitAsync(_session.Token, dc, Environment.MachineName, CancellationToken.None))
            _session.Populate(_session.OperatorName, result.Token, result.SessionContext)
            _session.LastSS = result.LastSS
            RetineUnitateaAleasa(dc)
            ArataUnitateaInTitlu()
            Await DeschideUnitateaCurentaAsync()
        Finally
            busyBar.Running = False
            _schimbaUnitatea = False
        End Try
    End Function

    ' Is the operator's role on this unit «Director»?
    Private Shared Function EsteUnitateDirector(k_unit As UnitInfo) As Boolean
        Return k_unit IsNot Nothing AndAlso String.Equals(k_unit.Rol, DirectorForm.RolDirector, StringComparison.OrdinalIgnoreCase)
    End Function

    ' The unit remembered for the next login, as the login form does after a success.
    Private Sub RetineUnitateaAleasa(dc As String)
        Try
            If AppSettings.Current.RememberLastLogin Then
                LastLoginStore.Save(_session.OperatorName, If(AppSettings.Current.RememberLastUnit, dc, Nothing))
            End If
        Catch ex As Exception
            ' Already logged by Save; a convenience file is not a reason to undo the switch.
        End Try
    End Sub

    ''' <summary>
    ''' What the shell does after a login, on the unit now in the session: the node selection and
    ''' the views close, the year/SS catalogue and the tree are read again, and the menu's
    ''' «Operatiuni necorelate» mark is recomputed. The old unit's rows never stay on screen, even
    ''' when the new unit has no configured periods.
    ''' </summary>
    Private Async Function DeschideUnitateaCurentaAsync() As Task
        Try
            ' The old unit's year / SS leave the bar until the new catalogue is read.
            capBar.ClearSelector(KBotCaptionBar.SelectorYear)
            capBar.ClearSelector(KBotCaptionBar.SelectorSector)
            _periods = Nothing
            _treeRows = Array.Empty(Of AngajamentTreeInfo)()
            _formularNouActiv = False   ' another unit: the «Angajament nou» form state does not carry over
            PopulateTree(_treeRows, Nothing)
            navViews.SelectedKey = If(ModDirector, String.Empty, "sumar")   ' a director has no «sumar»: the empty page
            ApplyViewGating(Nothing)

            Await LoadPeriodsAsync()
            ' Slice 0100-02: another unit, another Setari table.
            Await LoadServerSettingsAsync()
            Await LoadTreeAsync()
            Await RefreshUncorrelatedMarkAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.DeschideUnitateaCurentaAsync", ex)
            Throw
        End Try
    End Function
End Class
