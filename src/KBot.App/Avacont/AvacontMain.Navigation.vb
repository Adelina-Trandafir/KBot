Imports System.Globalization
Imports KBot.Common
Imports KBot.Controls

' Slice 0097-09: both history shortcuts share the registry, popup and preview providers.
Partial Public Class AvacontMain
    Private Const KeyNavigationHistory As String = "navigare_istoric"
    Private _historyPopup As CustomPopup
    Private _historyAnchor As Control
    Private _fillingHistoryPopup As Boolean
    Private _historyPreviewKey As String = String.Empty

    Private Sub InitNavigationHistory()
        AddHandler _kbot.capBar.HistoryButtonClick, AddressOf NavigationHistory_Click
        AddHandler _kbot.capBar.HistoryButtonHoverChanged, AddressOf NavigationHistory_HoverChanged
        ApplyNavigationHistorySettings()
        InitNavigationSwitcher()
    End Sub

    Private Sub TearDownNavigationHistory()
        TearDownNavigationSwitcher()
        RemoveHandler _kbot.capBar.HistoryButtonClick, AddressOf NavigationHistory_Click
        RemoveHandler _kbot.capBar.HistoryButtonHoverChanged, AddressOf NavigationHistory_HoverChanged
        If _historyPopup IsNot Nothing AndAlso Not _historyPopup.IsDisposed Then _historyPopup.Close()
        _historyPopup = Nothing
    End Sub

    Private Sub ApplyNavigationHistorySettings()
        Dim k_settings As AppSettings = AppSettings.Current
        _kbot.capBar.ShowHistoryButton = k_settings.ShowNavigationHistoryCaption
        Dim k_button As KBotRibbonButton = ribbon.FindButton(KeyNavigationHistory)
        If k_button IsNot Nothing AndAlso k_button.Visible <> k_settings.ShowNavigationHistoryRibbon Then
            k_button.Visible = k_settings.ShowNavigationHistoryRibbon
            ribbon.InvalidateLayout()
        End If
        If _historyPopup Is Nothing OrElse _historyPopup.IsDisposed Then Return
        If (ReferenceEquals(_historyAnchor, _kbot.capBar) AndAlso Not k_settings.ShowNavigationHistoryCaption) OrElse
           (ReferenceEquals(_historyAnchor, ribbon) AndAlso Not k_settings.ShowNavigationHistoryRibbon) Then _historyPopup.Close()
    End Sub

    ' Text is sorted as Romanian text inside the two explicitly requested status groups.
    Private Function NavigationHistoryEntries() As IEnumerable(Of MachetaEntry)
        Return _registry.History.Recent().OrderByDescending(Function(k_entry) k_entry.IsOpen).
            ThenBy(Function(k_entry) k_entry.Title, StringComparer.Create(CultureInfo.GetCultureInfo("ro-RO"), True)).ToList()
    End Function

    Private Function NavigationHistoryRows() As IEnumerable(Of KBotSidePanelRow)
        Dim k_rows As New List(Of KBotSidePanelRow)()
        For Each k_entry As MachetaEntry In NavigationHistoryEntries()
            Dim k_this As MachetaEntry = k_entry
            Dim k_row As New KBotSidePanelRow(k_entry.Title, String.Empty, "macheta") With {
                .Tag = k_entry, .Italic = Not k_entry.IsOpen}
            If k_entry.IsOpen Then
                k_row.ToolTipHeader = k_entry.Title
                k_row.ToolTipImageProvider = Function() PreviewFor(k_this)
            End If
            k_rows.Add(k_row)
        Next
        Return k_rows
    End Function

    Private Function NavigationHistoryIcon(k_entry As MachetaEntry) As Image
        Dim k_key As String = k_entry.TypeKey
        Select Case k_key
            Case KbotForm.MachetaAngajamente : k_key = "pagina_angajamente"
            Case KbotForm.MachetaEFacturaVanzare : k_key = "efactura_vanzare"
            Case KbotForm.MachetaEFacturaCumparare : k_key = "efactura_cumparare"
            Case KbotForm.MachetaEFacturaVercon : k_key = "efactura_vercon"
            Case KbotForm.MachetaAdechitRapoarte : k_key = "adechit_rapoarte"
            Case KbotForm.MachetaVerconRapoarte : k_key = "vercon_rapoarte"
        End Select
        Dim k_button As KBotRibbonButton = ribbon.FindButton(k_key)
        If k_button?.Image IsNot Nothing Then Return k_button.Image
        Dim k_icon As Image = TryCast(My.Resources.Resources.ResourceManager.GetObject(_registry.Find(k_entry.TypeKey).IconKey), Image)
        Return If(k_icon, My.Resources.Resources.application)
    End Function

    Private Sub RefreshNavigationHistory()
        If _historyPopup Is Nothing OrElse _historyPopup.IsDisposed Then Return
        If _switchingHistory Then
            ValidateNavigationSwitch()
            Return
        End If
        HideNavigationPreview()
        _fillingHistoryPopup = True
        Try
            Dim k_selected As String = _historyPopup.SelectedKey
            _historyPopup.SelectedIndex = -1
            _historyPopup.Items.Clear()
            Dim k_openSeen As Boolean
            Dim k_separatorAdded As Boolean
            For Each k_entry As MachetaEntry In NavigationHistoryEntries()
                If Not k_entry.IsOpen AndAlso k_openSeen AndAlso Not k_separatorAdded Then
                    _historyPopup.Items.Add(CustomPopupItem.Separator())
                    k_separatorAdded = True
                End If
                k_openSeen = k_openSeen OrElse k_entry.IsOpen
                _historyPopup.Items.Add(New CustomPopupItem(k_entry.Id, k_entry.Title.Replace("&", "&&"), NavigationHistoryIcon(k_entry)) With {
                    .Tag = k_entry, .Italic = Not k_entry.IsOpen})
            Next
            If _historyPopup.Items.Count = 0 Then
                _historyPopup.Items.Add(New CustomPopupItem("history_empty", "Nicio machetă deschisă încă") With {.Enabled = False})
            ElseIf Not String.IsNullOrEmpty(k_selected) AndAlso _historyPopup.ContainsKey(k_selected) Then
                _historyPopup.SelectedKey = k_selected
            End If
        Finally
            _fillingHistoryPopup = False
        End Try
    End Sub

    Private Sub NavigationHistory_Click(sender As Object, e As EventArgs)
        OpenNavigationHistory(_kbot.capBar, _kbot.capBar.HistoryButtonBounds)
    End Sub

    Private Sub OpenNavigationHistory(k_anchor As Control, k_bounds As Rectangle)
        Try
            HideNavigationPreview()
            If _historyPopup IsNot Nothing AndAlso Not _historyPopup.IsDisposed Then
                _historyPopup.Close()
                Return
            End If
            If CustomPopup.ClosedJustNow OrElse k_bounds.IsEmpty Then Return
            _historyAnchor = k_anchor
            _historyPopup = CreateNavigationPopup()
            RefreshNavigationHistory()
            _historyPopup.ShowBelow(k_anchor, k_bounds)
        Catch ex As Exception
            If _historyPopup IsNot Nothing AndAlso Not _historyPopup.IsDisposed Then _historyPopup.Dispose()
            GlobalErrorLog.Write("AvacontMain.OpenNavigationHistory", ex)
        End Try
    End Sub

    Private Function CreateNavigationPopup() As CustomPopup
        Dim k_popup As New CustomPopup() With {
            .Text = "Istoricul navigării", .MinimumPopupWidth = 180, .MaximumPopupWidth = 180,
            .FontSizeOffset = 2.0F, .IconBarVisible = True}
        AddHandler k_popup.ItemClicked, AddressOf NavigationHistory_ItemClicked
        AddHandler k_popup.SelectedItemChanged, AddressOf NavigationHistory_SelectedChanged
        AddHandler k_popup.MouseMove, AddressOf NavigationHistory_MouseMove
        AddHandler k_popup.MouseLeave, Sub() HideNavigationPreview()
        AddHandler k_popup.MouseWheel, Sub() HideNavigationPreview()
        AddHandler k_popup.FormClosed, AddressOf NavigationPopup_Closed
        Return k_popup
    End Function

    Private Sub NavigationPopup_Closed(sender As Object, e As FormClosedEventArgs)
        ResetNavigationSwitch()
        Try
            HideNavigationPreview()
        Catch ex As Exception
            GlobalErrorLog.Write("AvacontMain.NavigationPopup_Closed", ex)
        End Try
    End Sub

    Private Sub NavigationHistory_ItemClicked(sender As Object, e As CustomPopupItemEventArgs)
        Try
            If _switchingHistory Then
                FinishNavigationSwitch(True, False)
                Return
            End If
            Dim k_entry As MachetaEntry = TryCast(e.Item.Tag, MachetaEntry)
            If k_entry IsNot Nothing Then BeginInvoke(New MethodInvoker(Sub() OpenMachetaAsync(k_entry)))
        Catch ex As Exception
            GlobalErrorLog.Write("AvacontMain.NavigationHistory_ItemClicked", ex)
        End Try
    End Sub

    Private Sub NavigationHistory_SelectedChanged(sender As Object, e As EventArgs)
        Try
            HideNavigationPreview()
            If _switchingHistory Then Return
            If _fillingHistoryPopup OrElse _historyPopup Is Nothing OrElse _historyPopup.IsDisposed Then Return
            Dim k_item As CustomPopupItem = _historyPopup.SelectedItem
            Dim k_entry As MachetaEntry = TryCast(k_item?.Tag, MachetaEntry)
            If k_entry Is Nothing Then Return
            Dim k_bounds As Rectangle = _historyPopup.RowScreenBounds(k_item.Key)
            If k_bounds.IsEmpty Then Return
            _historyPreviewKey = k_item.Key
            tips.ShowAt(_historyPopup, New KBotToolTipContent() With {
                .HeaderText = k_entry.Title, .BodyImage = If(k_entry.IsOpen, PreviewFor(k_entry), Nothing)},
                New Point(k_bounds.Right, k_bounds.Top))
        Catch ex As Exception
            GlobalErrorLog.Write("AvacontMain.NavigationHistory_SelectedChanged", ex)
        End Try
    End Sub

    Private Sub HideNavigationPreview()
        tips.HideNow()
        _historyPreviewKey = String.Empty
    End Sub

    Private Sub NavigationHistory_MouseMove(sender As Object, e As MouseEventArgs)
        Try
            If _switchingHistory Then Return
            If _historyPopup Is Nothing OrElse _historyPopup.IsDisposed Then Return
            Dim k_item As CustomPopupItem = _historyPopup.SelectedItem
            If k_item Is Nothing Then Return
            Dim k_bounds As Rectangle = _historyPopup.RowScreenBounds(k_item.Key)
            If Not k_bounds.Contains(_historyPopup.PointToScreen(e.Location)) Then
                HideNavigationPreview()
            ElseIf Not String.Equals(_historyPreviewKey, k_item.Key, StringComparison.Ordinal) Then
                NavigationHistory_SelectedChanged(sender, EventArgs.Empty)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("AvacontMain.NavigationHistory_MouseMove", ex)
        End Try
    End Sub

    Private Sub NavigationHistory_HoverChanged(sender As Object, e As EventArgs)
        Try
            tips.HideNow()
            If Not _kbot.capBar.HistoryButtonHot Then Return
            If _historyPopup IsNot Nothing AndAlso Not _historyPopup.IsDisposed Then Return
            Dim k_bounds As Rectangle = _kbot.capBar.HistoryButtonBounds
            tips.ShowAt(_kbot.capBar, New KBotToolTipContent() With {
                .HeaderText = "Istoricul navigării",
                .Text = "Revino la o machetă deschisă sau redeschide una din lista «Machete deschise»."},
                _kbot.capBar.PointToScreen(New Point(k_bounds.Left, k_bounds.Bottom)))
        Catch ex As Exception
            GlobalErrorLog.Write("AvacontMain.NavigationHistory_HoverChanged", ex)
        End Try
    End Sub
End Class
