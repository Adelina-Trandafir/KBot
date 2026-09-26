Imports System.Threading
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain

''' <summary>
''' The angajamente list in the middle column (slice 0086 split out of KbotForm.vb): loading
''' it from GET /api/forexe/tree, filling it, the node click that drives the views, and the
''' two header buttons that belong to it (hidden angajamente, internal info window). The
''' column / sort options live in KbotForm.TreeOptions.vb, the footer icons in
''' KbotForm.Extrase.vb and KbotForm.Download.vb.
''' </summary>
Partial Public Class KbotForm

    ''' <summary>
    ''' Loads the tree from GET /api/forexe/tree for the selected period (year + SS), through
    ''' WithReauth -- the same single re-login path on 401. The database is not sent: the
    ''' server takes it from the session (one database = one unit). Busy bar for the length of
    ''' the call; errors are shown to the operator in Romanian, never swallowed and never
    ''' masked by an empty tree.
    ''' </summary>
    ''' <param name="pastreazaSelectia">
    ''' True = the node selected now stays selected after the reload, and the open view gets
    ''' its context (slice 0060). Asked for AFTER a write on the current angajament: the figures
    ''' changed, but the operator is still there. Default False -- on a period change or on the
    ''' first load there is no selection to keep, and an old one would belong to another year.
    ''' </param>
    ''' <param name="codDeSelectat">Slice 0081-02: a code to select after the reload even if it was
    ''' not in the tree before (a K-BOT angajament just created, or one whose «!» code was replaced
    ''' by forexecab's). Wins over <paramref name="pastreazaSelectia"/>.</param>
    Private Async Function LoadTreeAsync(Optional pastreazaSelectia As Boolean = False,
                                         Optional codDeSelectat As String = Nothing) As Task
        ' No year/SS, no query to run (empty combos = periods not read).
        If cboAn.SelectedItem Is Nothing OrElse cboSs.SelectedItem Is Nothing Then
            Return
        End If

        Dim an As Integer = CInt(cboAn.SelectedItem)
        ' Sorted by date the tree is a timeline of the whole year: every source, not only the
        ' SS in the combo (operator, 23.09.2026 -- slice 0777).
        Dim ss As String = If(AppSettings.Current.TreeSortIsDate, ApiClient.TreeAllSources, CStr(cboSs.SelectedItem))
        ' Read BEFORE the request: `PopulateTree` clears `_currentInfo`, so after it there is
        ' nowhere left to learn what was selected.
        Dim codSelectat As String = If(pastreazaSelectia AndAlso _currentInfo IsNot Nothing,
                                       _currentInfo.CodAngajament, Nothing)
        If Not String.IsNullOrWhiteSpace(codDeSelectat) Then codSelectat = codDeSelectat

        busyBar.Running = True
        Try
            Dim ct As CancellationToken = CancellationToken.None
            Dim rows As IReadOnlyList(Of AngajamentTreeInfo) =
                Await WithReauth(Of IReadOnlyList(Of AngajamentTreeInfo))(
                    Function() _apiClient.GetTreeAsync(an, ss, _includeHidden, ct))
            ' The rows are kept: the tree options menu re-lays them without a fresh request.
            _treeRows = rows
            PopulateTree(rows, codSelectat)
        Catch ex As Exception
            ' No silent net: an error (server down / 401 dead session / server defect after a
            ' re-login) is shown to the operator with the reason the server returned, not
            ' masked by an empty tree -- that would lie that the unit has no data.
            GlobalErrorLog.Write("MainForm.LoadTreeAsync", ex)
            KBotMessage.Show(Me,
                "Nu s-a putut încărca arborele de angajamente: " & ex.Message,
                "Angajamente", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            busyBar.Running = False
        End Try
    End Function

    ''' <summary>
    ''' Slice 0081-02: reloads the angajamente tree and selects <paramref name="cod"/> -- used when
    ''' the node did not exist before (a new K-BOT angajament) or changed its code (the send).
    ''' UI boundary (fire-and-forget from a Sub): LoadTreeAsync shows its own errors.
    ''' </summary>
    Private Async Sub ReincarcaArborelePe(cod As String)
        Try
            Await LoadTreeAsync(codDeSelectat:=cod).ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.ReincarcaArborelePe", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Clears and refills the list + the Cod -> info dictionary. Every row: caption =
    ''' Descriere (upper), CodAngajament cell, status icon (from Stare), refresh icon, bold when
    ''' it has sources (legacy behaviour), tooltip, Tag = Cod.
    ''' </summary>
    ''' <param name="codSelectat">
    ''' The angajament to re-select after the refill, or Nothing. A code that is no longer in
    ''' the tree is not an error: it behaves exactly like Nothing -- no view stays open on an
    ''' angajament that disappeared from the list.
    ''' </param>
    Private Sub PopulateTree(rows As IReadOnlyList(Of AngajamentTreeInfo),
                             Optional codSelectat As String = Nothing)
        Try
            ArgumentNullException.ThrowIfNull(rows)
            tree.Clear()
            _treeInfos.Clear()

            ' The view gate is applied ONCE, after the loop, when it is known whether the old
            ' node is still in the tree. Gating on Nothing up here, before the re-selection,
            ' used to hide every entry and push the nav onto «sumar» -- so a reload that KEPT
            ' the node still threw the operator off the view they were on (Rezervari,
            ' Receptii...) after every FOREXE refresh.

            Dim nodDeSelectat As AdvancedTreeControl.TreeItem = Nothing
            Dim infoDeSelectat As AngajamentTreeInfo = Nothing

            For Each info As AngajamentTreeInfo In SortRows(rows)
                Dim cod As String = If(info.CodAngajament, String.Empty)
                Dim caption As String = If(info.Descriere, String.Empty).Trim().ToUpperInvariant()

                Dim node As AdvancedTreeControl.TreeItem =
                    tree.AddItem("D_" & cod, caption,
                                 pLeftIconClosed:=FxIcons.StatusIcon(info.Stare),
                                 pRightIcon:=FxIcons.RefreshIcon(),
                                 pTag:=cod)

                ' Both cells are always written; which of them shows is the column set of the
                ' sort in force (slice 0777, ApplyTreeColumns) -- so a toggle needs no reload.
                node.Cells(COL_COD) = New AdvancedTreeControl.TreeItem.CellData With {.Value = cod}
                node.Cells(COL_SURSE) = New AdvancedTreeControl.TreeItem.CellData With {
                    .Value = FormatSurse(info.Surse)}
                node.Bold = info.AreIndicatori   ' legacy: bold = has sources (indicatori)
                node.Tooltip = TooltipFor(info)
                'node.ShowRightIconOnHover = True

                _treeInfos(cod) = info
                If codSelectat IsNot Nothing AndAlso
                   String.Equals(cod, codSelectat, StringComparison.OrdinalIgnoreCase) Then
                    nodDeSelectat = node
                    ' The info is taken FROM HERE, not through `_treeInfos(codSelectat)`: the
                    ' dictionary compares exactly, while the match above ignores case -- so a
                    ' key differing only in case would throw KeyNotFound.
                    infoDeSelectat = info
                End If
            Next

            ' The selection is put BACK at the end, with everything that hangs on it -- the
            ' view gate, the active view's context, the info window -- which is exactly what a
            ' click on the node does (`Tree_NodeMouseUp`). `SelectAndReveal`, not
            ' `SelectedNode`: written alone, the selection would be real and INVISIBLE, and the
            ' operator would see a list that jumped to the first row.
            If nodDeSelectat IsNot Nothing Then
                _currentInfo = infoDeSelectat
                tree.SelectAndReveal(nodDeSelectat)
                ' The nav key stays where it was: `ApplyViewGating` only falls back to «sumar»
                ' when the fresh flags no longer allow the open view.
                ApplyViewGating(infoDeSelectat)
                _activeView?.SetContext(infoDeSelectat)
            Else
                ' The old selection went away with the rows (or was not to be kept): no view
                ' stays open on an angajament that is no longer in the tree.
                _currentInfo = Nothing
                ApplyViewGating(Nothing)
            End If
            RefreshInfoForm()   ' the info window reflects the new selection (or its absence)

            tree.Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.PopulateTree", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' The hover label of a tree row. Until 22.09.2026 it was the Descriere alone -- the very
    ''' text already written ON the row, in capitals -- so the operator had nothing to gain by
    ''' waiting for it, and it was reachable only over the 14 px status icon
    ''' (<c>TooltipShowOnlyOnLeftIcon</c>). Both halves are fixed: it now shows over the whole
    ''' row, and it says what the row cannot fit -- code, state, dates, sources.
    ''' </summary>
    ''' <remarks>
    ''' Plain lines, not the XML table the Receptii tree uses: there is one node per angajament
    ''' here, nothing to put in columns. Empty fields are left out rather than shown blank.
    ''' </remarks>
    Private Shared Function TooltipFor(info As AngajamentTreeInfo) As String
        If info Is Nothing Then Return String.Empty
        Try
            Dim linii As New List(Of String)()
            Dim descriere As String = If(info.Descriere, String.Empty).Trim()
            If descriere.Length > 0 Then linii.Add(descriere)

            Dim cod As String = If(info.CodAngajament, String.Empty).Trim()
            If cod.Length > 0 Then linii.Add("Cod: " & cod)

            Dim stare As String = If(info.Stare, String.Empty).Trim()
            If stare.Length > 0 Then linii.Add("Stare: " & stare)

            If info.DataCreare.HasValue Then
                linii.Add("Creat: " & info.DataCreare.Value.ToString("dd.MM.yyyy"))
            End If
            ' Slice 0777: the moment the date sort orders on, with its time part.
            If info.DataAngajamentNou.HasValue Then
                linii.Add("Angajament nou (FOREXE): " & info.DataAngajamentNou.Value.ToString("dd.MM.yyyy HH:mm:ss"))
            End If
            If info.DataDefinitivare.HasValue Then
                linii.Add("Definitivat: " & info.DataDefinitivare.Value.ToString("dd.MM.yyyy"))
            End If

            Dim surse As String = FormatSurse(info.Surse)
            If surse.Length > 0 Then linii.Add("Surse: " & surse)
            If info.Ascuns Then linii.Add("Ascuns")

            Return String.Join(vbLf, linii)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.TooltipFor", ex)
            Return If(info.Descriere, String.Empty)
        End Try
    End Function

    ' The list selection pushes the context (AngajamentTreeInfo) to the active view.
    Private Sub Tree_NodeMouseUp(pNode As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles tree.NodeMouseUp
        Try
            Dim info As AngajamentTreeInfo = Nothing
            Dim cod As String = If(pNode Is Nothing, Nothing, TryCast(pNode.Tag, String))
            If cod IsNot Nothing Then
                _treeInfos.TryGetValue(cod, info)
            End If
            _currentInfo = info
            ' The node's flags decide which views are reachable (the Are* gate).
            ApplyViewGating(info)
            _activeView?.SetContext(info)
            ' An operator CLICK with the «Browser FOREXE» view open sends the robot after the
            ' node (slice 0074) - only a click, never the reload-driven SetContext above.
            TryCast(_activeView, BrowserView)?.DeschideSelectia()
            RefreshInfoForm()   ' the «Informatii interne» window, when open
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.tree_NodeMouseUp", ex)
        End Try
    End Sub

    ' NOTE (slice 0034): collapsing the tree LEFT the shell -- the footer button, the
    ' tree_CollapsedChanged handler and ClampSplitter were deleted, and the left corner of the
    ' footer became the statements button. The trees in the VIEWS keep their collapse; this
    ' one was only the shell's.

    ''' <summary>
    ''' The tree options (Access bOpt). Toggles showing the hidden angajamente (ASCUNS) and
    ''' re-reads the tree -- ASCUNS is a server-side filter (include_hidden), not a local one.
    ''' </summary>
    Private Async Sub BtnOpt_Click(sender As Object, e As EventArgs) Handles btnOpt.Click
        Try
            _includeHidden = Not _includeHidden
            Await LoadTreeAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.btnOpt_Click", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Opens (or brings to the front) the modeless «Informatii interne» window, which shows
    ''' every field + the Are* flags of the selected angajament. Modeless: Show, not
    ''' ShowDialog -- the operator can work in the shell with it open. It refreshes itself on
    ''' every tree selection (see RefreshInfoForm); its own refresh button re-reads the
    ''' selection through the _currentInfo provider.
    ''' </summary>
    Private Sub BtnInfo_Click(sender As Object, e As EventArgs) Handles btnInfo.Click
        Try
            If _infoForm Is Nothing OrElse _infoForm.IsDisposed Then
                _infoForm = New InternalInfoForm(Function() _currentInfo)
                ' Placed next to the tree card, inside the shell.
                Dim anchor As Point = PointToScreen(New Point(pnlWork.Left + 8, pnlWork.Top + 8))
                _infoForm.StartPosition = FormStartPosition.Manual
                _infoForm.Location = anchor
                _infoForm.ShowInfo(_currentInfo)
                _infoForm.Show(Me)   ' modeless, owned by the shell (closes with it)
            Else
                _infoForm.ShowInfo(_currentInfo)
                If _infoForm.WindowState = FormWindowState.Minimized Then _infoForm.WindowState = FormWindowState.Normal
                _infoForm.BringToFront()
                _infoForm.Activate()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.btnInfo_Click", ex)
        End Try
    End Sub

    ' Pushes the current context to the info window, when it is open.
    Private Sub RefreshInfoForm()
        Try
            If _infoForm IsNot Nothing AndAlso Not _infoForm.IsDisposed Then
                _infoForm.ShowInfo(_currentInfo)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.RefreshInfoForm", ex)
        End Try
    End Sub
End Class
