Imports KBot.Common
Imports KBot.Controls

' Slice 0097-09: a frozen MRU selection while Ctrl is held; only open machete are eligible.
Partial Public Class AvacontMain
    Private ReadOnly _navigationMru As New List(Of String)()
    Private _navigationFilter As NavigationSwitchFilter
    Private _switchingHistory As Boolean
    Private _switchEntries As New List(Of MachetaEntry)()
    Private _switchReturnFocus As Control
    Private _navigationTabDown As Boolean

    Private Sub InitNavigationSwitcher()
        _navigationFilter = New NavigationSwitchFilter(Me)
        Application.AddMessageFilter(_navigationFilter)
    End Sub

    Private Sub TearDownNavigationSwitcher()
        If _navigationFilter IsNot Nothing Then Application.RemoveMessageFilter(_navigationFilter)
        _navigationFilter = Nothing
        _navigationTabDown = False
        ResetNavigationSwitch()
    End Sub

    Private Function FrontNavigationId() As String
        If _page IsNot Nothing AndAlso Not _page.IsDisposed Then
            Dim k_ref As MachetaRef = Nothing
            Return If(_pageRefs.TryGetValue(_page, k_ref), k_ref?.Id, String.Empty)
        End If
        Return If(_baseIsShell, ShellRef.Id, String.Empty)
    End Function

    Private Sub RememberNavigationFront()
        Dim k_id As String = FrontNavigationId()
        If String.IsNullOrEmpty(k_id) Then Return
        _navigationMru.RemoveAll(Function(k_old) OpenNavigationTarget(k_old) Is Nothing)
        _navigationMru.Remove(k_id)
        _navigationMru.Insert(0, k_id)
    End Sub

    Private Function OpenNavigationTarget(k_id As String) As Form
        If _baseIsShell AndAlso String.Equals(k_id, ShellRef.Id, StringComparison.Ordinal) AndAlso Not _kbot.IsDisposed Then Return _kbot
        For Each k_page As Form In _pages
            If k_page.IsDisposed Then Continue For
            Dim k_ref As MachetaRef = Nothing
            If _pageRefs.TryGetValue(k_page, k_ref) AndAlso String.Equals(k_ref.Id, k_id, StringComparison.Ordinal) Then Return k_page
        Next
        Return Nothing
    End Function

    Private Function SwitchCandidates() As List(Of MachetaEntry)
        Dim k_entries As List(Of MachetaEntry) = _registry.History.Recent().
            Where(Function(k_entry) k_entry.IsOpen AndAlso OpenNavigationTarget(k_entry.Id) IsNot Nothing).ToList()
        Dim k_ids As New HashSet(Of String)(k_entries.Select(Function(k_entry) k_entry.Id), StringComparer.Ordinal)
        _navigationMru.RemoveAll(Function(k_id) Not k_ids.Contains(k_id))
        Return k_entries.OrderBy(Function(k_entry)
                                     Dim k_rank As Integer = _navigationMru.IndexOf(k_entry.Id)
                                     Return If(k_rank >= 0, k_rank, Integer.MaxValue)
                                 End Function).ThenByDescending(Function(k_entry) k_entry.Moment).ToList()
    End Function

    Private Function FocusedNavigationControl() As Control
        Dim k_control As Control = Me
        Do
            Dim k_container As ContainerControl = TryCast(k_control, ContainerControl)
            If k_container Is Nothing OrElse k_container.ActiveControl Is Nothing Then Return k_control
            k_control = k_container.ActiveControl
        Loop
    End Function

    Private Sub StartNavigationSwitch(k_reverse As Boolean)
        If _historyPopup IsNot Nothing AndAlso Not _historyPopup.IsDisposed Then _historyPopup.Close()
        RememberNavigationFront()
        Dim k_entries As List(Of MachetaEntry) = SwitchCandidates()
        If k_entries.Count = 0 Then Return
        Dim k_current As String = FrontNavigationId()
        If k_entries.Count = 1 AndAlso String.Equals(k_entries(0).Id, k_current, StringComparison.Ordinal) Then Return
        CaptureFront()
        _switchReturnFocus = FocusedNavigationControl()
        _switchEntries = k_entries
        _switchingHistory = True
        _historyAnchor = Me
        _historyPopup = CreateNavigationPopup()
        _historyPopup.PreviewColumns = 3
        For Each k_entry As MachetaEntry In _switchEntries
            _historyPopup.Items.Add(New CustomPopupItem(k_entry.Id, k_entry.Title.Replace("&", "&&"), NavigationHistoryIcon(k_entry)) With {
                .Tag = k_entry, .PreviewImage = PreviewFor(k_entry)})
        Next
        Dim k_index As Integer = _switchEntries.FindIndex(Function(k_entry) String.Equals(k_entry.Id, k_current, StringComparison.Ordinal))
        _historyPopup.SelectedIndex = If(k_index >= 0,
            (k_index + If(k_reverse, k_entries.Count - 1, 1)) Mod k_entries.Count,
            If(k_reverse, k_entries.Count - 1, 0))
        _historyPopup.ShowCentered(Me)
        ' A quick chord may already have released Ctrl while the first frame was being prepared.
        If (Control.ModifierKeys And Keys.Control) = Keys.None Then FinishNavigationSwitch(True)
    End Sub

    Private Sub AdvanceNavigationSwitch(k_reverse As Boolean)
        If Not _switchingHistory OrElse _historyPopup Is Nothing OrElse _historyPopup.IsDisposed Then Return
        Dim k_count As Integer = _switchEntries.Count
        If k_count = 0 Then Return
        _historyPopup.SelectedIndex = (_historyPopup.SelectedIndex + If(k_reverse, k_count - 1, 1)) Mod k_count
    End Sub

    Private Sub ValidateNavigationSwitch()
        If Not _switchingHistory Then Return
        Dim k_open As New HashSet(Of String)(_registry.History.Recent().Where(Function(k_entry) k_entry.IsOpen).
            Select(Function(k_entry) k_entry.Id), StringComparer.Ordinal)
        If _switchEntries.Any(Function(k_entry) Not k_open.Contains(k_entry.Id) OrElse OpenNavigationTarget(k_entry.Id) Is Nothing) Then
            FinishNavigationSwitch(False)
        End If
    End Sub

    Private Sub ResetNavigationSwitch()
        _switchingHistory = False
        _switchEntries = New List(Of MachetaEntry)()
        _switchReturnFocus = Nothing
    End Sub

    Private Sub FinishNavigationSwitch(k_commit As Boolean, Optional k_closePopup As Boolean = True)
        If Not _switchingHistory Then Return
        Dim k_entry As MachetaEntry = If(k_commit, TryCast(_historyPopup?.SelectedItem?.Tag, MachetaEntry), Nothing)
        Dim k_target As Form = If(k_entry Is Nothing, Nothing, OpenNavigationTarget(k_entry.Id))
        Dim k_focus As Control = _switchReturnFocus
        ResetNavigationSwitch()
        If k_closePopup AndAlso _historyPopup IsNot Nothing AndAlso Not _historyPopup.IsDisposed Then _historyPopup.Close()
        HideNavigationPreview()
        If IsDisposed OrElse Disposing OrElse Not IsHandleCreated Then Return
        If k_closePopup Then
            ' Commit before the next chord is processed, so rapid repeated chords toggle the last two pages.
            CompleteNavigationSwitch(k_target, k_entry, k_focus)
        Else
            ' A mouse/Enter selection is still unwinding CustomPopup.CloseWith.
            BeginInvoke(New MethodInvoker(Sub() CompleteNavigationSwitch(k_target, k_entry, k_focus)))
        End If
    End Sub

    Private Sub CompleteNavigationSwitch(k_target As Form, k_entry As MachetaEntry, k_focus As Control)
        Try
            If IsDisposed OrElse Disposing OrElse _closing OrElse Not Enabled Then Return
            If k_target IsNot Nothing AndAlso Not k_target.IsDisposed AndAlso k_entry IsNot Nothing AndAlso
               OpenNavigationTarget(k_entry.Id) Is k_target Then
                If ReferenceEquals(k_target, _kbot) Then
                    ShowShell()
                Else
                    ShowPage(k_target)
                End If
                k_target.Select()
            ElseIf k_focus IsNot Nothing AndAlso Not k_focus.IsDisposed AndAlso k_focus.CanFocus Then
                k_focus.Focus()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("AvacontMain.CompleteNavigationSwitch", ex)
        End Try
    End Sub

    Private Function OwnsNavigationKey(k_handle As IntPtr) As Boolean
        Dim k_control As Control = Control.FromChildHandle(k_handle)
        Do While k_control IsNot Nothing
            If ReferenceEquals(k_control, Me) OrElse ReferenceEquals(k_control, _historyPopup) Then Return True
            k_control = k_control.Parent
        Loop
        Return False
    End Function

    Private Function FilterNavigationKey(ByRef k_message As Message) As Boolean
        Const WM_KEYDOWN As Integer = &H100
        Const WM_KEYUP As Integer = &H101
        Const WM_SYSKEYDOWN As Integer = &H104
        Const WM_SYSKEYUP As Integer = &H105
        Dim k_down As Boolean = k_message.Msg = WM_KEYDOWN OrElse k_message.Msg = WM_SYSKEYDOWN
        Dim k_up As Boolean = k_message.Msg = WM_KEYUP OrElse k_message.Msg = WM_SYSKEYUP
        If Not k_down AndAlso Not k_up Then Return False
        If IsDisposed OrElse Disposing OrElse _closing OrElse Not Visible OrElse Not Enabled OrElse Not OwnsNavigationKey(k_message.HWnd) Then Return False
        Dim k_key As Keys = CType(k_message.WParam.ToInt32() And &HFFFF, Keys)
        If k_up AndAlso k_key = Keys.Tab AndAlso _navigationTabDown Then
            _navigationTabDown = False
            Return True
        End If
        Dim k_modifiers As Keys = Control.ModifierKeys
        If _switchingHistory Then
            If k_up AndAlso (k_key = Keys.ControlKey OrElse k_key = Keys.LControlKey OrElse k_key = Keys.RControlKey) AndAlso
               (k_modifiers And Keys.Control) = Keys.None Then
                FinishNavigationSwitch(True)
                Return True
            End If
            If k_down AndAlso k_key = Keys.Escape Then
                FinishNavigationSwitch(False)
                Return True
            End If
            If k_down AndAlso k_key = Keys.Enter Then
                FinishNavigationSwitch(True)
                Return True
            End If
        End If
        If Not k_down OrElse k_key <> Keys.Tab OrElse (k_modifiers And Keys.Control) = Keys.None OrElse
           (k_modifiers And Keys.Alt) <> Keys.None Then Return False
        If _switchingHistory Then
            _navigationTabDown = True
            AdvanceNavigationSwitch((k_modifiers And Keys.Shift) <> Keys.None)
        Else
            Dim k_active As Form = Form.ActiveForm
            If Not ReferenceEquals(k_active, Me) AndAlso Not ReferenceEquals(k_active, _historyPopup) AndAlso Not ContainsFocus Then Return False
            _navigationTabDown = True
            StartNavigationSwitch((k_modifiers And Keys.Shift) <> Keys.None)
        End If
        Return True
    End Function

    Private NotInheritable Class NavigationSwitchFilter
        Implements IMessageFilter
        Private ReadOnly _main As AvacontMain

        Public Sub New(k_main As AvacontMain)
            _main = k_main
        End Sub

        Public Function PreFilterMessage(ByRef m As Message) As Boolean Implements IMessageFilter.PreFilterMessage
            Try
                Return _main.FilterNavigationKey(m)
            Catch ex As Exception
                _main.ResetNavigationSwitch()
                If _main._historyPopup IsNot Nothing AndAlso Not _main._historyPopup.IsDisposed Then _main._historyPopup.Close()
                GlobalErrorLog.Write("AvacontMain.NavigationSwitchFilter", ex)
                Return False
            End Try
        End Function
    End Class
End Class
