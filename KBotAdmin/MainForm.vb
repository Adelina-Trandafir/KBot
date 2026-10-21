Option Strict On
Imports System.Globalization

''' <summary>
''' The main window (slice ME01, ribbon since ME01-05). Top: the ribbon, which holds every page and the actions that open a window.
''' Left: ONE tree with a tick on every row, chosen by the bar above it: the databases (one level under «&lt;TOATE BAZELE&gt;») and, under
''' each database, the accounts linked to it, or the users first and, under each user, the databases it is linked to. A tick under the tree
''' ticks every row. Right: the pages. The chosen database is the context every
''' page works in; a page learns about a change through <see cref="IAdminView.SelectionChanged"/>.
''' </summary>
Public Class MainForm

    Private Const AllKey As String = "*ALL*"
    Private Const DbTagPrefix As String = "db|"
    Private Const AccountTagPrefix As String = "acc|"
    Private Const UserTagPrefix As String = "usr|"

    Private ReadOnly _client As AdminClient
    Private ReadOnly _context As AdminContext
    Private ReadOnly _views As New Dictionary(Of String, Control)(StringComparer.Ordinal)
    Private _activeKey As String = String.Empty
    Private _accounts As New List(Of AccountLink)()
    Private _loading As Boolean
    Private _byUser As Boolean          ' the tree shows the users first (root) and their databases (leaves)
    Private _syncingAll As Boolean      ' the «select all» tick is being set from the tree, not by the operator

    Public Sub New(k_client As AdminClient)
        InitializeComponent()
        Icon = AdminIcon.Value
        capBar.IconImage = AdminIcon.Small()
        notify.Icon = AdminIcon.Value
        notify.Visible = True
        _client = k_client
        _context = New AdminContext(k_client)
        AddHandler _context.StatusChanged, AddressOf OnStatusChanged
        AddHandler _context.BusyChanged, AddressOf OnBusyChanged
        AddHandler _context.RefreshRequested, AddressOf OnRefreshRequested

        _views("comenzi") = viewCommands
        _views("sincronizare") = viewSync
        _views("drepturi") = viewRights
        _views("loguri") = viewLogs
        _views("audit") = viewAudit
        _views("vizite") = viewVisits
        _views("server") = viewServer
        _views("copii") = viewBackups
        _views("cereri") = viewRequests
        _views("publicare") = viewPublish
        _views("unice") = viewOnce
        _views("login") = viewLogin
        _views("vercon-reports") = viewVerconReports
        _views("demo") = viewDemo
        _views("copii-locale") = viewLocalBackup
        For Each k_view As Control In _views.Values
            DirectCast(k_view, IAdminView).Attach(_context)
        Next
    End Sub

    ' ------------------------------------------------------------------
    ' The window closes into the tray; only the tray menu closes the program
    ' ------------------------------------------------------------------

    Private Const WM_ACTIVATE As Integer = &H6
    Private Const WM_ACTIVATEAPP As Integer = &H1C
    Private Const WM_NCACTIVATE As Integer = &H86

    Private _exiting As Boolean
    Private _tipShown As Boolean

    ''' <summary>Coming back to the program (taskbar, Alt+Tab) must show an open message box, not hide it behind this window.</summary>
    Protected Overrides Sub WndProc(ByRef m As Message)
        TrayLog.Trace(m)
        MyBase.WndProc(m)
        If m.Msg = SingleInstance.ShowMessage Then
            ' A second launch asked this (maybe tray-hidden) instance to come forward.
            BeginInvoke(New Action(AddressOf ShowFromTray))
            Return
        End If
        If (m.Msg = WM_ACTIVATEAPP OrElse m.Msg = WM_ACTIVATE OrElse m.Msg = WM_NCACTIVATE) AndAlso IsHandleCreated Then
            BeginInvoke(New Action(AddressOf ModalGuard.RaiseModal))
        End If
    End Sub

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        MyBase.OnFormClosing(e)
        Try
            ' The X button and Alt+F4 only hide the window; Windows shutting down, or the tray's «Închide», really close it.
            If e.Cancel OrElse _exiting OrElse e.CloseReason <> CloseReason.UserClosing Then Return
            e.Cancel = True
            TrayLog.BeginClose(Me, "before hide")
            Dim k_area As Rectangle = Bounds
            Hide()
            TrayLog.Snapshot(Me, "after Hide")
            ShowInTaskbar = False
            TrayLog.Snapshot(Me, "after ShowInTaskbar=False")
            ' The maximized window leaves its picture on the screen (it vanishes at the first click on it): ask the desktop to repaint that area.
            BeginInvoke(New Action(Sub() RepaintDesktop(k_area)))
            LogWindowsLaterAsync()
            If Not _tipShown Then
                _tipShown = True
                notify.ShowBalloonTip(2500, "KBotAdmin", "Rulează în zona de notificări. Clic pe iconiță pentru meniu.", ToolTipIcon.Info)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.OnFormClosing", ex)
        End Try
    End Sub

    Private Const DWMWA_TRANSITIONS_FORCEDISABLED As Integer = 3

    <Runtime.InteropServices.DllImport("dwmapi.dll")>
    Private Shared Function DwmSetWindowAttribute(k_handle As IntPtr, k_attribute As Integer, ByRef k_value As Integer, k_size As Integer) As Integer
    End Function

    ''' <summary>
    ''' No fade animation when the window is hidden into the tray: the animation's picture stayed on the screen after the FIRST close (the handle
    ''' is made again right after the hide, when the taskbar button goes). Done for every handle, as ShowInTaskbar makes a new one.
    ''' </summary>
    Protected Overrides Sub OnHandleCreated(e As EventArgs)
        MyBase.OnHandleCreated(e)
        Try
            Dim k_on As Integer = 1
            DwmSetWindowAttribute(Handle, DWMWA_TRANSITIONS_FORCEDISABLED, k_on, 4)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.OnHandleCreated", ex)
        End Try
    End Sub

    Private Const RDW_INVALIDATE As Integer = &H1
    Private Const RDW_ERASE As Integer = &H4
    Private Const RDW_ALLCHILDREN As Integer = &H80
    Private Const RDW_UPDATENOW As Integer = &H100

    <Runtime.InteropServices.StructLayout(Runtime.InteropServices.LayoutKind.Sequential)>
    Private Structure NativeRect
        Public Left As Integer
        Public Top As Integer
        Public Right As Integer
        Public Bottom As Integer
    End Structure

    <Runtime.InteropServices.DllImport("user32.dll")>
    Private Shared Function RedrawWindow(k_handle As IntPtr, ByRef k_rect As NativeRect, k_region As IntPtr, k_flags As Integer) As Boolean
    End Function

    ''' <summary>Repaints a screen area of the desktop and of every window under it (what a click on a stale picture does).</summary>
    Private Shared Sub RepaintDesktop(k_area As Rectangle)
        Try
            Dim k_rect As New NativeRect With {.Left = k_area.Left, .Top = k_area.Top, .Right = k_area.Right, .Bottom = k_area.Bottom}
            RedrawWindow(IntPtr.Zero, k_rect, IntPtr.Zero, RDW_INVALIDATE Or RDW_ERASE Or RDW_ALLCHILDREN Or RDW_UPDATENOW)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.RepaintDesktop", ex)
        End Try
    End Sub

    ''' <summary>The windows of this program right after a close, 0.4 s and 2 s later (see <see cref="TrayLog"/>).</summary>
    Private Async Sub LogWindowsLaterAsync()
        Try
            TrayLog.SnapshotWindows("right after close")
            Await Task.Delay(400)
            TrayLog.Snapshot(Me, "0.4 s after close")
            TrayLog.SnapshotWindows("0.4 s after close")
            Await Task.Delay(1600)
            TrayLog.Snapshot(Me, "2 s after close")
            TrayLog.SnapshotWindows("2 s after close")
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.LogWindowsLaterAsync", ex)
        End Try
    End Sub

    Private Sub ShowFromTray()
        Try
            TrayLog.Write("----- ShowFromTray")
            ShowInTaskbar = True
            Show()
            If WindowState = FormWindowState.Minimized Then WindowState = FormWindowState.Normal
            Activate()
            BringToFront()
            TrayLog.Snapshot(Me, "after ShowFromTray")
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.ShowFromTray", ex)
        End Try
    End Sub

    Private Sub Notify_MouseUp(k_sender As Object, e As MouseEventArgs) Handles notify.MouseUp
        Try
            ' The icon's menu opens on a left click too (WinForms opens it on the right click only).
            If e.Button = MouseButtons.Left Then
                GetType(NotifyIcon).GetMethod("ShowContextMenu", Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic).Invoke(notify, Nothing)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.Notify_MouseUp", ex)
        End Try
    End Sub

    Private Sub MnuShow_Click(k_sender As Object, e As EventArgs) Handles mnuShow.Click
        ShowFromTray()
    End Sub

    Private Sub MnuExit_Click(k_sender As Object, e As EventArgs) Handles mnuExit.Click
        Try
            If busy.Running AndAlso Not UiRun.Confirm("O operație rulează încă pe server. Închid programul oricum?", "KBotAdmin") Then Return
            _exiting = True
            notify.Visible = False
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.MnuExit_Click", ex)
        End Try
    End Sub

    Protected Overrides Async Sub OnShown(e As EventArgs)
        MyBase.OnShown(e)
        Try
            Await RefreshAllAsync()
            Await RemindLocalBackupAsync()
        Catch ex As Exception
            ' UI boundary (async void): shown to the operator, logged, never rethrown.
            GlobalErrorLog.Write("MainForm.OnShown", ex)
        End Try
    End Sub

    ''' <summary>
    ''' At start-up (ME02): when no local copy of the databases was made TODAY, says so and offers to make one now. The copies are never
    ''' made by themselves; this message is the only prompt. The check opens the store with the key kept on this PC, if there is one.
    ''' </summary>
    Private Async Function RemindLocalBackupAsync() As Task
        Try
            Dim k_settings As AdminSettings = _client.Settings
            Dim k_text As String
            Dim k_canRun As Boolean = True
            If String.IsNullOrWhiteSpace(k_settings.BackupRoot) OrElse Not BackupStore.Exists(k_settings.BackupRoot) Then
                k_text = "Nu există încă nicio copie locală a bazelor de pe server. O faceți acum?"
            Else
                Dim k_store As BackupStore = _context.LocalStore
                If k_store Is Nothing Then
                    Dim k_keys As BackupKeys = BackupKeyring.TryLoad(BackupStore.ReadStoreId(k_settings.BackupRoot))
                    If k_keys IsNot Nothing Then
                        Try
                            k_store = BackupStore.OpenWithKeys(k_settings.BackupRoot, k_keys)
                        Catch ex As BackupException
                            GlobalErrorLog.Write("MainForm.RemindLocalBackupAsync", ex)
                        End Try
                    End If
                End If
                If k_store Is Nothing Then
                    k_text = "Nu pot verifica dacă s-a făcut azi o copie locală (cheia nu e păstrată pe acest calculator). Deschid pagina «Copii locale», ca să scrieți parola?"
                    k_canRun = False
                Else
                    _context.LocalStore = k_store
                    Dim k_madeToday As Boolean = Await Task.Run(Function() k_store.ListSnapshots().Items.Any(Function(k_m) k_m.FinishedAt.Date = DateTime.Today))
                    If k_madeToday Then Return
                    k_text = "Azi nu s-a făcut încă nicio copie locală a bazelor de pe server. O faceți acum?"
                End If
            End If
            If Not UiRun.Confirm(k_text, "Copie locală") Then Return
            Await ShowViewAsync("copii-locale")
            If k_canRun Then Await viewLocalBackup.RunNowAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.RemindLocalBackupAsync", ex)
        End Try
    End Function

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            ' The form background IS the 1px outline of the window (Padding(1, 2, 1, 2)).
            BackColor = ThemeManager.Current.Palette.BorderColor
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.OnThemeChanged", ex)
        End Try
    End Sub

    ' ------------------------------------------------------------------
    ' Status and busy
    ' ------------------------------------------------------------------

    Private Sub OnStatusChanged(k_text As String)
        lblStatus.Text = k_text
    End Sub

    Private Sub OnBusyChanged(k_busy As Boolean)
        busy.Running = k_busy
    End Sub

    Private Async Sub OnRefreshRequested()
        Try
            Await RefreshAllAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.OnRefreshRequested", ex)
        End Try
    End Sub

    ' ------------------------------------------------------------------
    ' The two trees
    ' ------------------------------------------------------------------

    ''' <summary>Reloads the databases and the accounts of the chosen database.</summary>
    Private Async Function RefreshAllAsync() As Task
        If _loading Then Return
        _loading = True
        Try
            Await UiRun.Guard("MainForm.RefreshAllAsync", _context,
                Async Function()
                    _context.Say("Se citesc bazele de date...")
                    Dim k_dbs As DatabasesResult = Await _client.CallAsync(Of DatabasesResult)("databases")
                    k_dbs.Databases.RemoveAll(Function(k_d) Not k_d.Exists) ' a database that is not on the server appears nowhere
                    _context.Databases = k_dbs.Databases
                    Dim k_acc As AccountsResult = Await _client.CallAsync(Of AccountsResult)("accounts")
                    _accounts = k_acc.Accounts
                    BuildDbTree()
                    ShowPendingRequests(Math.Max(0, k_dbs.PendingRequests))
                    Dim k_errors As Integer = k_dbs.Databases.Sum(Function(k_d) k_d.Err24)
                    capBar.Text = "KBotAdmin - " & k_dbs.Databases.Count.ToString(CultureInfo.InvariantCulture) & " baze, " &
                                  k_errors.ToString(CultureInfo.InvariantCulture) & " erori în ultimele 24 h"
                    _context.Say("Gata. Server: " & k_dbs.ServerTime.Replace("T", " "))
                End Function)
        Finally
            _loading = False
        End Try
    End Function

    Private Sub BuildDbTree()
        Dim k_open As HashSet(Of String) = OpenBranches()
        Dim k_checked As HashSet(Of String) = CheckedLeaves()
        treeDb.Clear()
        treeDb.RootExpander = _byUser   ' the single root of the database view stays open; the users are roots that must open
        Dim k_select As AdvancedTreeControl.TreeItem = If(_byUser, BuildUserBranches(k_open), BuildDatabaseBranches(k_open))
        If Not _byUser AndAlso k_select.Level < 2 Then _context.SelectedAccount = String.Empty   ' the chosen account is gone (or none was chosen)
        If k_select IsNot Nothing Then treeDb.SelectAndReveal(k_select)
        For Each k_node As AdvancedTreeControl.TreeItem In AllNodes(treeDb.Items).ToList()
            If k_node.Children.Count = 0 AndAlso k_checked.Contains(Convert.ToString(k_node.Tag, CultureInfo.InvariantCulture)) Then
                treeDb.SetItemCheckState(k_node, AdvancedTreeControl.TreeCheckState.Checked)   ' also recalculates the rows above it
            End If
        Next
        PublishChecks()
    End Sub

    ''' <summary>Root «all databases», a branch per database, the accounts of each database as its leaves.</summary>
    Private Function BuildDatabaseBranches(k_open As HashSet(Of String)) As AdvancedTreeControl.TreeItem
        Dim k_root As AdvancedTreeControl.TreeItem = treeDb.AddItem(AllKey, "<b>" & AdminContext.AllDatabasesCaption & "</b>",
                                                                  Nothing, Nothing, Nothing, Nothing, AllKey, True)
        k_root.HasCheckBox = True
        Dim k_select As AdvancedTreeControl.TreeItem = k_root
        For Each k_db As DatabaseInfo In _context.Databases
            Dim k_right As String = k_db.Name
            If k_db.SyncPending > 0 Then k_right = "schemă: " & k_db.SyncPending.ToString(CultureInfo.InvariantCulture) & "  " & k_right
            Dim k_item As AdvancedTreeControl.TreeItem = treeDb.AddItem(DbKey(k_db.Db), k_db.Db & "~~~" & k_right, k_root,
                                                                      Nothing, Nothing, Nothing, DbTagPrefix & k_db.Db)
            k_item.HasCheckBox = True
            DescribeNode(k_item, k_db)
            Dim k_isChosen As Boolean = String.Equals(k_db.Db, _context.SelectedDb, StringComparison.OrdinalIgnoreCase)
            If k_isChosen Then k_select = k_item
            Dim k_dbName As String = k_db.Db
            ' The accounts of this database are its leaves. The same account shows under every database it is linked to.
            For Each k_link As AccountLink In _accounts.Where(Function(k_a) String.Equals(k_a.Dc, k_dbName, StringComparison.OrdinalIgnoreCase)).
                                                       OrderBy(Function(k_a) k_a.Un, StringComparer.OrdinalIgnoreCase)
                Dim k_extra As String = If(k_link.Roles.Count > 0, "  +" & k_link.Roles.Count.ToString(CultureInfo.InvariantCulture), String.Empty)
                Dim k_leaf As AdvancedTreeControl.TreeItem = treeDb.AddItem(AccountKey(k_db.Db, k_link.Un), k_link.Un & "~~~" & k_link.Rol & k_extra, k_item,
                                                                          Nothing, Nothing, Nothing, AccountTagPrefix & k_db.Db & "|" & k_link.Un)
                k_leaf.HasCheckBox = True
                If k_isChosen AndAlso _context.SelectedAccount <> String.Empty AndAlso
                   String.Equals(k_link.Un, _context.SelectedAccount, StringComparison.OrdinalIgnoreCase) Then k_select = k_leaf
            Next
            k_item.Expanded = k_isChosen OrElse k_open.Contains(DbTagPrefix & k_db.Db)
        Next
        Return k_select
    End Function

    ''' <summary>A root per user, the databases it is linked to as its leaves (only databases that exist on the server). Nothing, when nothing in the tree matches the choice.</summary>
    Private Function BuildUserBranches(k_open As HashSet(Of String)) As AdvancedTreeControl.TreeItem
        Dim k_known As New Dictionary(Of String, DatabaseInfo)(StringComparer.OrdinalIgnoreCase)
        For Each k_db As DatabaseInfo In _context.Databases
            k_known(k_db.Db) = k_db
        Next
        Dim k_select As AdvancedTreeControl.TreeItem = Nothing
        Dim k_users As IEnumerable(Of IGrouping(Of String, AccountLink)) = _accounts.Where(Function(k_a) k_known.ContainsKey(k_a.Dc)).
            GroupBy(Function(k_a) k_a.Un, StringComparer.OrdinalIgnoreCase).OrderBy(Function(k_g) k_g.Key, StringComparer.OrdinalIgnoreCase)
        For Each k_group As IGrouping(Of String, AccountLink) In k_users
            Dim k_links As List(Of AccountLink) = k_group.OrderBy(Function(k_a) k_a.Dc, StringComparer.OrdinalIgnoreCase).ToList()
            Dim k_user As AdvancedTreeControl.TreeItem = treeDb.AddItem(UserTagPrefix & k_group.Key, k_group.Key & "~~~" & k_links.Count.ToString(CultureInfo.InvariantCulture) & " baze",
                                                                      Nothing, Nothing, Nothing, Nothing, UserTagPrefix & k_group.Key,
                                                                      k_open.Contains(UserTagPrefix & k_group.Key))
            k_user.HasCheckBox = True
            Dim k_isChosenUser As Boolean = _context.SelectedAccount <> String.Empty AndAlso
                                            String.Equals(k_group.Key, _context.SelectedAccount, StringComparison.OrdinalIgnoreCase)
            If k_isChosenUser AndAlso _context.SelectedDb = String.Empty Then k_select = k_user
            For Each k_link As AccountLink In k_links
                Dim k_extra As String = If(k_link.Roles.Count > 0, "  +" & k_link.Roles.Count.ToString(CultureInfo.InvariantCulture), String.Empty)
                Dim k_leaf As AdvancedTreeControl.TreeItem = treeDb.AddItem(AccountKey(k_link.Dc, k_link.Un), k_link.Dc & "~~~" & k_link.Rol & k_extra, k_user,
                                                                          Nothing, Nothing, Nothing, AccountTagPrefix & k_link.Dc & "|" & k_link.Un)
                k_leaf.HasCheckBox = True
                DescribeNode(k_leaf, k_known(k_link.Dc))
                If k_isChosenUser AndAlso String.Equals(k_link.Dc, _context.SelectedDb, StringComparison.OrdinalIgnoreCase) Then k_select = k_leaf
            Next
        Next
        If k_select Is Nothing AndAlso _context.SelectedAccount <> String.Empty Then _context.SelectedAccount = String.Empty   ' that user is gone
        Return k_select
    End Function

    ''' <summary>The tooltip and the colour of a database row (red: errors, amber: warnings or schema waiting).</summary>
    Private Shared Sub DescribeNode(k_node As AdvancedTreeControl.TreeItem, k_db As DatabaseInfo)
        Dim k_pal As ThemePalette = ThemeManager.Current.Palette
        k_node.Tooltip = DescribeDb(k_db)
        If k_db.Err24 > 0 OrElse k_db.SyncErrors > 0 Then
            k_node.NodeForeColor = k_pal.ErrorColor
        ElseIf k_db.Warn24 > 0 OrElse k_db.SyncPending > 0 Then
            k_node.NodeForeColor = k_pal.WarningColor
        End If
    End Sub

    Private Shared Function DbKey(k_db As String) As String
        Return DbTagPrefix & k_db
    End Function

    Private Shared Function AccountKey(k_db As String, k_un As String) As String
        Return AccountTagPrefix & k_db & "|" & k_un
    End Function

    Private Shared Iterator Function AllNodes(k_nodes As IEnumerable(Of AdvancedTreeControl.TreeItem)) As IEnumerable(Of AdvancedTreeControl.TreeItem)
        For Each k_node As AdvancedTreeControl.TreeItem In k_nodes
            Yield k_node
            For Each k_child As AdvancedTreeControl.TreeItem In AllNodes(k_node.Children)
                Yield k_child
            Next
        Next
    End Function

    ''' <summary>The tags of the branches that are open now, so a reload of the tree does not fold them.</summary>
    Private Function OpenBranches() As HashSet(Of String)
        Dim k_open As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each k_node As AdvancedTreeControl.TreeItem In AllNodes(treeDb.Items)
            If k_node.Expanded AndAlso k_node.Children.Count > 0 Then k_open.Add(Convert.ToString(k_node.Tag, CultureInfo.InvariantCulture))
        Next
        Return k_open
    End Function

    ''' <summary>The tags of the ticked rows that have no rows under them; ticking these again rebuilds every tick above them.</summary>
    Private Function CheckedLeaves() As HashSet(Of String)
        Dim k_set As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each k_node As AdvancedTreeControl.TreeItem In AllNodes(treeDb.Items)
            If k_node.Children.Count = 0 AndAlso k_node.CheckState = AdvancedTreeControl.TreeCheckState.Checked Then
                k_set.Add(Convert.ToString(k_node.Tag, CultureInfo.InvariantCulture))
            End If
        Next
        Return k_set
    End Function

    ''' <summary>
    ''' Tells the context which databases and accounts are ticked (a ticked row; in the user view a ticked database leaf is a database, in the
    ''' database view a ticked account leaf is an account), and brings the «select all» tick in line with the tree.
    ''' </summary>
    Private Sub PublishChecks()
        Dim k_dbs As New List(Of String)()
        Dim k_accounts As New List(Of String)()
        For Each k_node As AdvancedTreeControl.TreeItem In AllNodes(treeDb.Items)
            If k_node.CheckState <> AdvancedTreeControl.TreeCheckState.Checked Then Continue For
            Dim k_tag As String = Convert.ToString(k_node.Tag, CultureInfo.InvariantCulture)
            If k_tag.StartsWith(DbTagPrefix, StringComparison.Ordinal) Then
                k_dbs.Add(k_tag.Substring(DbTagPrefix.Length))
            ElseIf k_tag.StartsWith(UserTagPrefix, StringComparison.Ordinal) Then
                k_accounts.Add(k_tag.Substring(UserTagPrefix.Length))
            ElseIf k_tag.StartsWith(AccountTagPrefix, StringComparison.Ordinal) Then
                Dim k_parts As String() = k_tag.Substring(AccountTagPrefix.Length).Split({"|"c}, 2)
                If k_parts.Length = 2 Then
                    If _byUser Then k_dbs.Add(k_parts(0)) Else k_accounts.Add(k_parts(1))
                End If
            End If
        Next
        _context.CheckedDatabases = k_dbs.Distinct(StringComparer.OrdinalIgnoreCase).ToList()
        _context.CheckedAccounts = k_accounts.Distinct(StringComparer.OrdinalIgnoreCase).ToList()
        Dim k_all As Boolean = treeDb.Items.Count > 0 AndAlso treeDb.Items.All(Function(k_i) k_i.CheckState = AdvancedTreeControl.TreeCheckState.Checked)
        _syncingAll = True
        Try
            chkAll.Checked = k_all
        Finally
            _syncingAll = False
        End Try
    End Sub

    Private Sub NavMode_SelectionChanged(k_key As String) Handles navMode.SelectionChanged
        Try
            Dim k_byUser As Boolean = String.Equals(k_key, "utilizatori", StringComparison.Ordinal)
            If k_byUser = _byUser Then Return
            _byUser = k_byUser
            If _context Is Nothing OrElse _loading Then Return   ' the bar raises this once while the window is being built
            BuildDbTree()
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.NavMode_SelectionChanged", ex)
        End Try
    End Sub

    Private Sub TreeDb_NodeChecked(k_node As AdvancedTreeControl.TreeItem) Handles treeDb.NodeChecked
        Try
            PublishChecks()
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.TreeDb_NodeChecked", ex)
        End Try
    End Sub

    Private Sub ChkAll_CheckedChanged(k_sender As Object, e As EventArgs) Handles chkAll.CheckedChanged
        Try
            If _syncingAll Then Return
            Dim k_state As AdvancedTreeControl.TreeCheckState = If(chkAll.Checked, AdvancedTreeControl.TreeCheckState.Checked, AdvancedTreeControl.TreeCheckState.Unchecked)
            For Each k_root As AdvancedTreeControl.TreeItem In treeDb.Items.ToList()
                treeDb.SetItemCheckState(k_root, k_state)   ' the state goes down to every row under it
            Next
            PublishChecks()
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.ChkAll_CheckedChanged", ex)
        End Try
    End Sub

    Private Shared Function DescribeDb(k_db As DatabaseInfo) As String
        Dim k_text As New System.Text.StringBuilder()
        k_text.Append(k_db.Db).Append(" - ").AppendLine(k_db.Name)
        k_text.Append("Conturi: ").Append(k_db.Accounts).Append(",  tabele: ").Append(k_db.Tables).Append(",  ")
        k_text.Append(k_db.Mb.ToString("0.#", CultureInfo.InvariantCulture)).AppendLine(" MB")
        k_text.Append("Ultima scriere: ").AppendLine(If(String.IsNullOrEmpty(k_db.LastWrite), "-", k_db.LastWrite.Replace("T", " ")))
        k_text.Append("Erori 24 h: ").Append(k_db.Err24).Append(",  avertismente: ").Append(k_db.Warn24).AppendLine()
        k_text.Append("Schemă în așteptare: ").Append(k_db.SyncPending)
        If k_db.SyncDestructive > 0 Then k_text.Append(" (").Append(k_db.SyncDestructive).Append(" distructive)")
        If k_db.SyncErrors > 0 Then k_text.Append(", ").Append(k_db.SyncErrors).Append(" cu eroare")
        If Not k_db.InCai Then k_text.AppendLine().Append("Atenție: baza nu e în registrul CAI.")
        Return k_text.ToString()
    End Function

    Private Sub TreeDb_NodeMouseUp(k_node As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles treeDb.NodeMouseUp
        TakeSelection(k_node)
    End Sub

    Private Sub TreeDb_KeyUp(k_sender As Object, e As KeyEventArgs) Handles treeDb.KeyUp
        TakeSelection(treeDb.SelectedNode)
    End Sub

    ''' <summary>A click on the root, on a database or on an account under a database. An account also chooses its database.</summary>
    Private Sub TakeSelection(k_node As AdvancedTreeControl.TreeItem)
        Try
            If k_node Is Nothing OrElse _loading Then Return
            Dim k_tag As String = Convert.ToString(k_node.Tag, CultureInfo.InvariantCulture)
            Dim k_db As String
            Dim k_account As String = String.Empty
            If k_tag = AllKey Then
                k_db = String.Empty
            ElseIf k_tag.StartsWith(AccountTagPrefix, StringComparison.Ordinal) Then
                Dim k_parts As String() = k_tag.Substring(AccountTagPrefix.Length).Split({"|"c}, 2)
                If k_parts.Length < 2 Then Return
                k_db = k_parts(0)
                k_account = k_parts(1)
            ElseIf k_tag.StartsWith(DbTagPrefix, StringComparison.Ordinal) Then
                k_db = k_tag.Substring(DbTagPrefix.Length)
            ElseIf k_tag.StartsWith(UserTagPrefix, StringComparison.Ordinal) Then
                k_db = String.Empty   ' a user is not in one database: every database is the context
                k_account = k_tag.Substring(UserTagPrefix.Length)
            Else
                Return
            End If
            Dim k_dbChanged As Boolean = Not String.Equals(k_db, _context.SelectedDb, StringComparison.OrdinalIgnoreCase)
            Dim k_accountChanged As Boolean = Not String.Equals(k_account, _context.SelectedAccount, StringComparison.OrdinalIgnoreCase)
            If Not k_dbChanged AndAlso Not k_accountChanged Then Return
            _context.SelectedDb = k_db
            _context.SelectedAccount = k_account
            If k_account <> String.Empty Then
                _context.Say("Cont ales: " & k_account & " în " & k_db)
            Else
                _context.Say("Baza aleasă: " & If(k_db = String.Empty, AdminContext.AllDatabasesCaption, k_db))
            End If
            If k_dbChanged Then
                NotifyActiveView()
            ElseIf _activeKey = "drepturi" Then
                viewRights.SelectionChanged()   ' only the account changed: the rights page just moves to its row
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.TakeSelection", ex)
        End Try
    End Sub

    ' ------------------------------------------------------------------
    ' The pages
    ' ------------------------------------------------------------------

    ''' <summary>A ribbon button: a page to open, or an action of the rights page (which opens that page first).</summary>
    Private Async Sub Ribbon_ItemClicked(k_sender As Object, e As KBotRibbonItemClickedEventArgs) Handles ribbon.ItemClicked
        Try
            Select Case e.Key
                Case "rights_ad"
                    Await ShowRightsActionAsync(AddressOf viewRights.OpenAdechitAsync)
                Case "rights_vr"
                    Await ShowRightsActionAsync(AddressOf viewRights.OpenVerconAsync)
                Case "acct_add"
                    Await ShowRightsActionAsync(AddressOf viewRights.AddAccountAsync)
                Case "acct_grant"
                    Await ShowRightsActionAsync(AddressOf viewRights.GrantDatabaseAsync)
                Case "acct_role"
                    Await ShowRightsActionAsync(AddressOf viewRights.ChangeRoleAsync)
                Case Else
                    Await ShowViewAsync(e.Key)
            End Select
        Catch ex As Exception
            ' UI boundary (async void): the page reports its own problems; this is the last wall.
            GlobalErrorLog.Write("MainForm.Ribbon_ItemClicked", ex)
        End Try
    End Sub

    ''' <summary>The rights page is the one that knows the chosen row: open it if it is not open, then run the action.</summary>
    Private Async Function ShowRightsActionAsync(k_action As Func(Of Task)) As Task
        If _activeKey <> "drepturi" Then Await ShowViewAsync("drepturi")
        Await k_action()
    End Function

    ''' <summary>Shows the page with this key (hides the others), opens the ribbon tab that holds its button, and lets the page load.</summary>
    Private Async Function ShowViewAsync(k_key As String) As Task
        If Not _views.ContainsKey(k_key) Then Throw New ArgumentException("Pagina necunoscuta: " & k_key, NameOf(k_key))
        _activeKey = k_key
        For Each k_pair As KeyValuePair(Of String, Control) In _views
            k_pair.Value.Visible = String.Equals(k_pair.Key, k_key, StringComparison.Ordinal)
        Next
        _views(k_key).BringToFront()
        ButtonFit.Apply(_views(k_key))
        SelectRibbonTabOf(k_key)
        Await DirectCast(_views(k_key), IAdminView).ActivatedAsync()
    End Function

    Private Sub SelectRibbonTabOf(k_key As String)
        For k_index As Integer = 0 To ribbon.Tabs.Count - 1
            For Each k_group As KBotRibbonGroup In ribbon.Tabs(k_index).Groups
                If k_group.Buttons.Any(Function(k_b) String.Equals(k_b.Key, k_key, StringComparison.Ordinal)) Then
                    ribbon.SelectedTabIndex = k_index
                    Return
                End If
            Next
        Next
    End Sub

    ''' <summary>The number of new-account requests waiting, written on the «Cereri» button (the ribbon has no badge).</summary>
    Private Sub ShowPendingRequests(k_count As Integer)
        Dim k_button As KBotRibbonButton = ribbon.FindButton("cereri")
        If k_button Is Nothing Then Return
        k_button.Text = If(k_count > 0, "Cereri (" & k_count.ToString(CultureInfo.InvariantCulture) & ")", "Cereri")
        ribbon.InvalidateLayout()
    End Sub

    Private Sub NotifyActiveView()
        If _activeKey = String.Empty OrElse Not _views.ContainsKey(_activeKey) Then Return
        DirectCast(_views(_activeKey), IAdminView).SelectionChanged()
    End Sub

End Class
