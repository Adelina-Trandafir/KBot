Option Strict On
Imports System.Globalization

''' <summary>
''' The rights per account and per application (slice ME01). A row is one account in one database; the check
''' columns are the ADECHIT roles (Roluri / Utilizatori_Roluri on the server). Ticking a box gives the right on
''' the server at once, clearing it takes it away; a refusal puts the box back. K-BOT's own role is the
''' «Rol K-BOT» column and changes through the dialog.
''' </summary>
Public Class RightsView
    Implements IAdminView

    Private Const AdPrefix As String = "AD_"
    Private Const EditClosedRole As String = "AD_EDIT_INCHISA"

    Private _context As AdminContext
    Private _roles As New RolesResult()
    Private _links As New List(Of AccountLink)()
    Private _filling As Boolean

    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub Attach(k_context As AdminContext) Implements IAdminView.Attach
        _context = k_context
    End Sub

    Public Async Function ActivatedAsync() As Task Implements IAdminView.ActivatedAsync
        Await LoadAsync()
    End Function

    Public Sub SelectionChanged() Implements IAdminView.SelectionChanged
        Try
            ' A new database: reload its rows. A new account in the tree: just move to its row.
            If _context.SelectedAccount <> String.Empty Then SelectAccountRow()
            If NeedsReload() Then BeginLoad()
        Catch ex As Exception
            GlobalErrorLog.Write("RightsView.SelectionChanged", ex)
        End Try
    End Sub

    Private _loadedFor As String = Nothing

    Private Function NeedsReload() As Boolean
        Return _loadedFor Is Nothing OrElse Not String.Equals(_loadedFor, _context.SelectedDb, StringComparison.OrdinalIgnoreCase)
    End Function

    Private Async Sub BeginLoad()
        Try
            Await LoadAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("RightsView.BeginLoad", ex)
        End Try
    End Sub

    ' ------------------------------------------------------------------
    ' Loading
    ' ------------------------------------------------------------------

    Private Async Function LoadAsync() As Task
        Await UiRun.Guard("RightsView.LoadAsync", _context,
            Async Function()
                _context.Say("Se citesc conturile și drepturile...")
                _roles = Await _context.Client.CallAsync(Of RolesResult)("roles")
                Dim k_args As String() = If(_context.IsAll, New String() {}, New String() {"--db", _context.SelectedDb})
                Dim k_result As AccountsResult = Await _context.Client.CallAsync(Of AccountsResult)("accounts", k_args)
                _links = k_result.Accounts
                _loadedFor = _context.SelectedDb
                Fill()
                _context.Say("Gata.")
            End Function)
    End Function

    Private Function ServerHasRole(k_cod As String) As Boolean
        Return _roles.Roles.Any(Function(k_r) String.Equals(k_r.Cod, k_cod, StringComparison.Ordinal))
    End Function

    Private Sub Fill()
        _filling = True
        grid.BeginUpdate()
        Try
            grid.ClearRows()
            For Each k_link As AccountLink In _links.OrderBy(Function(k_l) k_l.Un, StringComparer.OrdinalIgnoreCase).ThenBy(Function(k_l) k_l.Dc, StringComparer.OrdinalIgnoreCase)
                Dim k_row As KBotDataRow = grid.AddRow()
                k_row("un") = k_link.Un
                k_row("dc") = k_link.Dc
                k_row("rol") = k_link.Rol
                For Each k_column As KBotDataColumn In grid.Columns
                    If k_column.Key.StartsWith(AdPrefix, StringComparison.Ordinal) Then
                        k_row(k_column.Key) = k_link.Roles.Contains(k_column.Key)
                    End If
                Next
                k_row.Tag = k_link
            Next
        Finally
            grid.EndUpdate()
            _filling = False
        End Try
        Dim k_missing As Boolean = Not ServerHasRole(EditClosedRole)
        btnEnsure.Visible = k_missing
        lblState.Text = _links.Count.ToString(CultureInfo.InvariantCulture) & " conturi-în-baze" &
                        If(_context.IsAll, " (toate bazele).", " în " & _context.SelectedDb & ".") &
                        If(k_missing, "  Rolul «Editare lună închisă» nu există încă pe server.", String.Empty)
        SelectAccountRow()
    End Sub

    Private Sub SelectAccountRow()
        If _context.SelectedAccount = String.Empty Then Return
        For k_i As Integer = 0 To grid.RowCount - 1
            If String.Equals(Convert.ToString(grid.Rows(k_i)("un"), CultureInfo.InvariantCulture), _context.SelectedAccount, StringComparison.OrdinalIgnoreCase) Then
                grid.CurrentRowIndex = k_i
                Return
            End If
        Next
    End Sub

    Private Function CurrentLink() As AccountLink
        Dim k_row As KBotDataRow = grid.CurrentRow
        Return If(k_row Is Nothing, Nothing, TryCast(k_row.Tag, AccountLink))
    End Function

    ' ------------------------------------------------------------------
    ' A tick changes
    ' ------------------------------------------------------------------

    Private Async Sub Grid_CellValueChanged(k_sender As Object, e As KBotCellValueEventArgs) Handles grid.CellValueChanged
        Try
            If _filling OrElse Not e.ColumnKey.StartsWith(AdPrefix, StringComparison.Ordinal) Then Return
            Dim k_link As AccountLink = TryCast(grid.Rows(e.RowIndex).Tag, AccountLink)
            If k_link Is Nothing Then Return
            Dim k_on As Boolean = Convert.ToBoolean(e.NewValue, CultureInfo.InvariantCulture)
            Dim k_previous As Boolean = Convert.ToBoolean(e.OldValue, CultureInfo.InvariantCulture)
            Dim k_cod As String = e.ColumnKey
            Dim k_index As Integer = e.RowIndex

            If Not ServerHasRole(k_cod) Then
                Revert(k_index, k_cod, k_previous)
                KBotMessage.Show("Rolul " & k_cod & " nu există pe server. Apăsați butonul de creare a rolului, de jos.",
                                 "Drepturi", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            Dim k_ok As Boolean = False
            Await UiRun.Guard("RightsView.Grid_CellValueChanged", _context,
                Async Function()
                    Dim k_verb As String = If(k_on, "grant", "revoke")
                    _context.Say(If(k_on, "Se dă ", "Se retrage ") & k_cod & " pentru " & k_link.Un & " în " & k_link.Dc & "...")
                    Await _context.Client.CallAsync(Of RoleChange)(k_verb, "--un", k_link.Un, "--dc", k_link.Dc, "--cod", k_cod)
                    If k_on Then
                        If Not k_link.Roles.Contains(k_cod) Then k_link.Roles.Add(k_cod)
                    Else
                        k_link.Roles.Remove(k_cod)
                    End If
                    k_ok = True
                    _context.Say("Gata: " & k_cod & If(k_on, " dat", " retras") & " pentru " & k_link.Un & ".")
                End Function)
            If Not k_ok Then Revert(k_index, k_cod, k_previous)
        Catch ex As Exception
            GlobalErrorLog.Write("RightsView.Grid_CellValueChanged", ex)
        End Try
    End Sub

    Private Sub Revert(k_rowIndex As Integer, k_key As String, k_value As Boolean)
        _filling = True
        Try
            grid(k_key, k_rowIndex) = k_value
        Finally
            _filling = False
        End Try
    End Sub

    ' ------------------------------------------------------------------
    ' Buttons
    ' ------------------------------------------------------------------

    Private Async Sub BtnRefresh_Click(k_sender As Object, e As EventArgs) Handles btnRefresh.Click
        Try
            Await LoadAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("RightsView.BtnRefresh_Click", ex)
        End Try
    End Sub

    Private Async Sub BtnAllAd_Click(k_sender As Object, e As EventArgs) Handles btnAllAd.Click
        Await ChangeAllAsync("grant", "Toate drepturile ADECHIT", True)
    End Sub

    Private Async Sub BtnNoAd_Click(k_sender As Object, e As EventArgs) Handles btnNoAd.Click
        Await ChangeAllAsync("revoke", "Retragerea drepturilor ADECHIT", False)
    End Sub

    Private Async Function ChangeAllAsync(k_verb As String, k_caption As String, k_give As Boolean) As Task
        Try
            Dim k_link As AccountLink = CurrentLink()
            If k_link Is Nothing Then
                KBotMessage.Show("Alegeți întâi un rând din tabel.", "Drepturi", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            If Not UiRun.Confirm(If(k_give, "Dau contului ", "Retrag de la contul ") & k_link.Un & " toate drepturile ADECHIT în " & k_link.Dc & "?", k_caption) Then Return
            Await UiRun.Guard("RightsView.ChangeAllAsync", _context,
                Async Function()
                    Await _context.Client.CallAsync(Of RoleChange)(k_verb, "--un", k_link.Un, "--dc", k_link.Dc, "--cod", "@AD")
                    _context.Say("Gata: drepturile ADECHIT ale lui " & k_link.Un & " în " & k_link.Dc & " au fost " & If(k_give, "date.", "retrase."))
                End Function)
            Await LoadAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("RightsView.ChangeAllAsync", ex)
        End Try
    End Function

    Private Function KbRoleNames() As List(Of String)
        Return _roles.Roles.Where(Function(k_r) k_r.Section = "KB").
            Select(Function(k_r) Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(k_r.Cod.Substring(3).ToLowerInvariant())).ToList()
    End Function

    ''' <summary>Gives an existing account access to one more database: only the databases it is not linked to are offered.</summary>
    Private Async Sub BtnGrant_Click(k_sender As Object, e As EventArgs) Handles btnGrant.Click
        Try
            Dim k_row As AccountLink = CurrentLink()
            Dim k_un As String = If(k_row IsNot Nothing, k_row.Un, _context.SelectedAccount)
            If String.IsNullOrWhiteSpace(k_un) Then
                KBotMessage.Show("Alegeți întâi contul: un rând din tabel sau un cont din arborele din stânga.", "Drepturi", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            If _roles.Roles.Count = 0 Then Await LoadAsync()
            Dim k_free As List(Of String) = Nothing
            Dim k_defaultRole As String = "Contabil"
            Await UiRun.Guard("RightsView.BtnGrant_Click", _context,
                Async Function()
                    _context.Say("Se citesc bazele contului " & k_un & "...")
                    Dim k_all As AccountsResult = Await _context.Client.CallAsync(Of AccountsResult)("accounts")
                    Dim k_mine As List(Of AccountLink) = k_all.Accounts.Where(Function(k_a) String.Equals(k_a.Un, k_un, StringComparison.OrdinalIgnoreCase)).ToList()
                    If k_mine.Count > 0 Then k_defaultRole = k_mine(0).Rol
                    Dim k_taken As New HashSet(Of String)(k_mine.Select(Function(k_a) k_a.Dc), StringComparer.OrdinalIgnoreCase)
                    k_free = _context.Databases.Select(Function(k_d) k_d.Db).Where(Function(k_d) Not k_taken.Contains(k_d)).ToList()
                    _context.Say("Gata.")
                End Function)
            If k_free Is Nothing Then Return
            If k_free.Count = 0 Then
                KBotMessage.Show("Contul " & k_un & " are deja acces la toate bazele.", "Drepturi", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            Using k_form As New AccountForm(KbRoleNames(), k_free, k_un, _context.SelectedDb, k_defaultRole, True)
                If k_form.ShowDialog(FindForm()) <> DialogResult.OK Then Return
                Dim k_db As String = k_form.Db
                Dim k_rol As String = k_form.Rol
                If Not UiRun.Confirm("Dau contului " & k_un & " acces la baza " & k_db & ", cu rolul K-BOT " & k_rol & "?", "Acces la o bază") Then Return
                Await UiRun.Guard("RightsView.BtnGrant_Click", _context,
                    Async Function()
                        Await _context.Client.CallRawAsync("acct-add", "--un", k_un, "--dc", k_db, "--rol", k_rol)
                        _context.Say("Contul " & k_un & " are acum acces la " & k_db & " ca " & k_rol & ".")
                    End Function)
            End Using
            _context.RequestRefresh()
            Await LoadAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("RightsView.BtnGrant_Click", ex)
        End Try
    End Sub

    Private Async Sub BtnAdd_Click(k_sender As Object, e As EventArgs) Handles btnAdd.Click
        Try
            If _roles.Roles.Count = 0 Then Await LoadAsync()
            Dim k_dbs As List(Of String) = _context.Databases.Select(Function(k_d) k_d.Db).ToList()
            Using k_form As New AccountForm(KbRoleNames(), k_dbs, String.Empty, _context.SelectedDb, "Contabil")
                If k_form.ShowDialog(FindForm()) <> DialogResult.OK Then Return
                Dim k_un As String = k_form.Un
                Dim k_db As String = k_form.Db
                Dim k_rol As String = k_form.Rol
                Await UiRun.Guard("RightsView.BtnAdd_Click", _context,
                    Async Function()
                        Await _context.Client.CallRawAsync("acct-add", "--un", k_un, "--dc", k_db, "--rol", k_rol)
                        _context.Say("Contul " & k_un & " a fost legat de " & k_db & " ca " & k_rol & ".")
                    End Function)
            End Using
            _context.RequestRefresh()
            Await LoadAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("RightsView.BtnAdd_Click", ex)
        End Try
    End Sub

    Private Async Sub BtnRole_Click(k_sender As Object, e As EventArgs) Handles btnRole.Click
        Try
            Dim k_link As AccountLink = CurrentLink()
            If k_link Is Nothing Then
                KBotMessage.Show("Alegeți întâi un rând din tabel.", "Drepturi", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            Using k_form As New AccountForm(KbRoleNames(), New List(Of String) From {k_link.Dc}, k_link.Un, k_link.Dc, k_link.Rol)
                If k_form.ShowDialog(FindForm()) <> DialogResult.OK Then Return
                Dim k_rol As String = k_form.Rol
                Await UiRun.Guard("RightsView.BtnRole_Click", _context,
                    Async Function()
                        Await _context.Client.CallRawAsync("acct-role", "--un", k_link.Un, "--dc", k_link.Dc, "--rol", k_rol)
                        _context.Say("Rolul K-BOT al lui " & k_link.Un & " în " & k_link.Dc & " este acum " & k_rol & ".")
                    End Function)
            End Using
            _context.RequestRefresh()
            Await LoadAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("RightsView.BtnRole_Click", ex)
        End Try
    End Sub

    Private Async Sub BtnRemove_Click(k_sender As Object, e As EventArgs) Handles btnRemove.Click
        Try
            Dim k_link As AccountLink = CurrentLink()
            If k_link Is Nothing Then
                KBotMessage.Show("Alegeți întâi un rând din tabel.", "Drepturi", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            If Not UiRun.Confirm("Scot contul " & k_link.Un & " din baza " & k_link.Dc & "?" & vbLf &
                                 "Nu va mai putea lucra în această bază și își pierde drepturile ADECHIT din ea.", "Scoate din bază") Then Return
            Await UiRun.Guard("RightsView.BtnRemove_Click", _context,
                Async Function()
                    Await _context.Client.CallRawAsync("acct-remove", "--un", k_link.Un, "--dc", k_link.Dc)
                    _context.Say("Contul " & k_link.Un & " a fost scos din " & k_link.Dc & ".")
                End Function)
            _context.RequestRefresh()
            Await LoadAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("RightsView.BtnRemove_Click", ex)
        End Try
    End Sub

    Private Async Sub BtnEnsure_Click(k_sender As Object, e As EventArgs) Handles btnEnsure.Click
        Try
            If Not UiRun.Confirm("Creez pe server rolul «Editare lună închisă» (AD_EDIT_INCHISA)?" & vbLf &
                                 "Rolul nu face nimic până nu e verificat și în ADECHIT și nu e bifat unui cont.", "Rol nou") Then Return
            Await UiRun.Guard("RightsView.BtnEnsure_Click", _context,
                Async Function()
                    Await _context.Client.CallRawAsync("ensure-role", "--cod", EditClosedRole)
                    _context.Say("Rolul " & EditClosedRole & " există acum pe server.")
                End Function)
            Await LoadAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("RightsView.BtnEnsure_Click", ex)
        End Try
    End Sub

End Class
