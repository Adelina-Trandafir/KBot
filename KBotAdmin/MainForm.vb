Option Strict On
Imports System.Globalization

''' <summary>
''' The main window (slice ME01). Left: the databases (one level under «&lt;TOATE BAZELE&gt;») and the
''' accounts linked to the chosen database. Right: the pages. The chosen database is the context every
''' page works in; a page learns about a change through <see cref="IAdminView.SelectionChanged"/>.
''' </summary>
Public Class MainForm

    Private Const AllKey As String = "*ALL*"

    Private ReadOnly _client As AdminClient
    Private ReadOnly _context As AdminContext
    Private ReadOnly _views As New Dictionary(Of String, Control)(StringComparer.Ordinal)
    Private _activeKey As String = String.Empty
    Private _accounts As New List(Of AccountLink)()
    Private _loading As Boolean

    Public Sub New(k_client As AdminClient)
        InitializeComponent()
        _client = k_client
        _context = New AdminContext(k_client)
        AddHandler _context.StatusChanged, AddressOf OnStatusChanged
        AddHandler _context.BusyChanged, AddressOf OnBusyChanged
        AddHandler _context.RefreshRequested, AddressOf OnRefreshRequested

        _views("comenzi") = viewCommands
        _views("diferenta") = viewDiff
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
        For Each k_view As Control In _views.Values
            DirectCast(k_view, IAdminView).Attach(_context)
        Next
    End Sub

    Protected Overrides Async Sub OnShown(e As EventArgs)
        MyBase.OnShown(e)
        Try
            Await RefreshAllAsync()
        Catch ex As Exception
            ' UI boundary (async void): shown to the operator, logged, never rethrown.
            GlobalErrorLog.Write("MainForm.OnShown", ex)
        End Try
    End Sub

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
                    _context.Databases = k_dbs.Databases
                    Dim k_acc As AccountsResult = Await _client.CallAsync(Of AccountsResult)("accounts")
                    _accounts = k_acc.Accounts
                    BuildDbTree()
                    BuildAccountTree()
                    navViews.SetBadge("cereri", Math.Max(0, k_dbs.PendingRequests))
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
        Dim k_pal As ThemePalette = ThemeManager.Current.Palette
        treeDb.Clear()
        Dim k_root As AdvancedTreeControl.TreeItem = treeDb.AddItem(AllKey, "<b>" & AdminContext.AllDatabasesCaption & "</b>",
                                                                  Nothing, Nothing, Nothing, Nothing, AllKey, True)
        Dim k_select As AdvancedTreeControl.TreeItem = k_root
        For Each k_db As DatabaseInfo In _context.Databases
            Dim k_right As String = k_db.Name
            If k_db.SyncPending > 0 Then k_right = "schemă: " & k_db.SyncPending.ToString(CultureInfo.InvariantCulture) & "  " & k_right
            Dim k_item As AdvancedTreeControl.TreeItem = treeDb.AddItem(k_db.Db, k_db.Db & "~~~" & k_right, k_root,
                                                                      Nothing, Nothing, Nothing, k_db.Db)
            k_item.Tooltip = DescribeDb(k_db)
            If k_db.Err24 > 0 OrElse k_db.SyncErrors > 0 Then
                k_item.NodeForeColor = k_pal.ErrorColor
            ElseIf k_db.Warn24 > 0 OrElse k_db.SyncPending > 0 Then
                k_item.NodeForeColor = k_pal.WarningColor
            End If
            If String.Equals(k_db.Db, _context.SelectedDb, StringComparison.OrdinalIgnoreCase) Then k_select = k_item
        Next
        treeDb.SelectAndReveal(k_select)
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

    Private Sub BuildAccountTree()
        treeAcc.Clear()
        Dim k_shown As IEnumerable(Of AccountLink) = _accounts.Where(
            Function(k_a) _context.IsAll OrElse String.Equals(k_a.Dc, _context.SelectedDb, StringComparison.OrdinalIgnoreCase))
        Dim k_seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each k_link As AccountLink In k_shown.OrderBy(Function(k_a) k_a.Un, StringComparer.OrdinalIgnoreCase)
            If _context.IsAll Then
                ' All databases: one row per account, with the number of databases it is linked to.
                If Not k_seen.Add(k_link.Un) Then Continue For
                Dim k_count As Integer = _accounts.Where(Function(k_a) String.Equals(k_a.Un, k_link.Un, StringComparison.OrdinalIgnoreCase)).Count()
                treeAcc.AddItem(k_link.Un, k_link.Un & "~~~" & k_count.ToString(CultureInfo.InvariantCulture) & " baze",
                                Nothing, Nothing, Nothing, Nothing, k_link.Un)
            Else
                Dim k_extra As String = If(k_link.Roles.Count > 0, "  +" & k_link.Roles.Count.ToString(CultureInfo.InvariantCulture), String.Empty)
                treeAcc.AddItem(k_link.Un, k_link.Un & "~~~" & k_link.Rol & k_extra, Nothing, Nothing, Nothing, Nothing, k_link.Un)
            End If
        Next
        _context.SelectedAccount = String.Empty
    End Sub

    Private Sub TreeDb_NodeMouseUp(k_node As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles treeDb.NodeMouseUp
        TakeDbSelection(k_node)
    End Sub

    Private Sub TreeDb_KeyUp(k_sender As Object, e As KeyEventArgs) Handles treeDb.KeyUp
        TakeDbSelection(treeDb.SelectedNode)
    End Sub

    Private Sub TakeDbSelection(k_node As AdvancedTreeControl.TreeItem)
        Try
            If k_node Is Nothing OrElse _loading Then Return
            Dim k_tag As String = Convert.ToString(k_node.Tag, CultureInfo.InvariantCulture)
            Dim k_db As String = If(k_tag = AllKey, String.Empty, k_tag)
            If String.Equals(k_db, _context.SelectedDb, StringComparison.OrdinalIgnoreCase) Then Return
            _context.SelectedDb = k_db
            BuildAccountTree()
            _context.Say("Baza aleasă: " & If(k_db = String.Empty, AdminContext.AllDatabasesCaption, k_db))
            NotifyActiveView()
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.TakeDbSelection", ex)
        End Try
    End Sub

    Private Sub TreeAcc_NodeMouseUp(k_node As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles treeAcc.NodeMouseUp
        TakeAccountSelection(k_node)
    End Sub

    Private Sub TreeAcc_KeyUp(k_sender As Object, e As KeyEventArgs) Handles treeAcc.KeyUp
        TakeAccountSelection(treeAcc.SelectedNode)
    End Sub

    Private Sub TakeAccountSelection(k_node As AdvancedTreeControl.TreeItem)
        Try
            If k_node Is Nothing Then Return
            _context.SelectedAccount = Convert.ToString(k_node.Tag, CultureInfo.InvariantCulture)
            If _activeKey = "drepturi" Then viewRights.SelectionChanged()
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.TakeAccountSelection", ex)
        End Try
    End Sub

    ' ------------------------------------------------------------------
    ' The pages
    ' ------------------------------------------------------------------

    Private Async Sub NavViews_SelectionChanged(k_key As String) Handles navViews.SelectionChanged
        Try
            If Not _views.ContainsKey(k_key) Then Return
            _activeKey = k_key
            For Each k_pair As KeyValuePair(Of String, Control) In _views
                k_pair.Value.Visible = String.Equals(k_pair.Key, k_key, StringComparison.Ordinal)
            Next
            _views(k_key).BringToFront()
            Await DirectCast(_views(k_key), IAdminView).ActivatedAsync()
        Catch ex As Exception
            ' UI boundary (async void): the page reports its own problems; this is the last wall.
            GlobalErrorLog.Write("MainForm.NavViews_SelectionChanged", ex)
        End Try
    End Sub

    Private Sub NotifyActiveView()
        If _activeKey = String.Empty OrElse Not _views.ContainsKey(_activeKey) Then Return
        DirectCast(_views(_activeKey), IAdminView).SelectionChanged()
    End Sub

End Class
