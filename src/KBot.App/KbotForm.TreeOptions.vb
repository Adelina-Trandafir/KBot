Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain

''' <summary>
''' Slice 0777: the options of the main tree -- its order and its two columns -- opened from
''' the right icon of the tree's header (and from the hidden «Sortare» button, which shows
''' the same menu).
'''
''' <para><b>One store.</b> Everything lives in <see cref="AppSettings"/>: the sort
''' (<c>TreeSort</c>) and, PER SORT, whether CODANGAJAMENT and SURSE are shown. The operator's
''' defaults (23.09.2026): by name = code on, sources off; by date = code off, sources on.
''' The menu and the KBOT tab of the application settings page write the same keys, and the shell
''' follows <see cref="AppSettings.Changed"/>, so a change made on either side shows at once.</para>
'''
''' <para><b>The two sorts show different rows.</b> By name = the SS chosen in the combo; by
''' date = EVERY source of the year (<c>ss=*</c>, operator 23.09.2026). So a change of sort
''' reloads from the server; a change of column only re-lays. The selected node comes back
''' where it was when it is still in the list (<c>PopulateTree</c> re-selects it).</para>
''' </summary>
Partial Public Class KbotForm

    ' Menu row keys of the tree options menu.
    Private Const TREE_SORT_NAME As String = "sort-name"
    Private Const TREE_SORT_DATE As String = "sort-date"
    Private Const TREE_COL_COD As String = "col-cod"
    Private Const TREE_COL_SURSE As String = "col-surse"

    ' Cell keys of the two columns (the ColumnDef.Name the cells are matched on).
    Private Const COL_COD As String = "CodAngajament"
    Private Const COL_SURSE As String = "Surse"

    ' Names are compared the way the operator reads them: Romanian order, case ignored.
    Private Shared ReadOnly NameComparer As StringComparer =
        StringComparer.Create(CultureInfo.GetCultureInfo("ro-RO"), ignoreCase:=True)

    ' What the tree was last laid out with, so an AppSettings.Changed raised for an unrelated
    ' switch (the console, the browser button...) does not re-lay the tree for nothing.
    Private _appliedSortIsDate As Boolean?
    Private _appliedDescending As Boolean?
    Private _appliedShowCod As Boolean?
    Private _appliedShowSurse As Boolean?
    Private _appliedCodWidth As Integer?
    Private _appliedSurseWidth As Integer?

    ''' <summary>Called once from Load: columns as the store says, then follow its changes.</summary>
    Private Sub LeagaOptiunileArborelui()
        ApplyTreeColumns(AppSettings.Current)
        AddHandler AppSettings.Changed, AddressOf AppSettings_TreeOptionsChanged
    End Sub

    ''' <summary>AppSettings is static: a subscription left behind would keep the shell alive.</summary>
    Private Sub DezleagaOptiunileArborelui()
        RemoveHandler AppSettings.Changed, AddressOf AppSettings_TreeOptionsChanged
    End Sub

    ' Raised on the thread that saved (the settings window or this menu, both UI); marshalled anyway.
    Private Sub AppSettings_TreeOptionsChanged(sender As Object, e As EventArgs)
        Try
            If IsDisposed OrElse Not IsHandleCreated Then Return
            If InvokeRequired Then
                BeginInvoke(New Action(AddressOf ApplyTreeOptionsFromStore))
            Else
                ApplyTreeOptionsFromStore()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.AppSettings_TreeOptionsChanged", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Re-lays the tree when (and only when) the sort or a column changed. UI boundary
    ''' (reached from an event): logs and swallows.
    ''' </summary>
    Private Sub ApplyTreeOptionsFromStore()
        Try
            Dim s As AppSettings = AppSettings.Current
            Dim sortChanged As Boolean = Not Nullable.Equals(_appliedSortIsDate, s.TreeSortIsDate)
            Dim colsChanged As Boolean = Not Nullable.Equals(_appliedShowCod, s.TreeShowCod) OrElse
                                         Not Nullable.Equals(_appliedShowSurse, s.TreeShowSurse) OrElse
                                         Not Nullable.Equals(_appliedCodWidth, s.TreeCodColumnWidth) OrElse
                                         Not Nullable.Equals(_appliedSurseWidth, s.TreeSurseColumnWidth)
            Dim directionChanged As Boolean = Not Nullable.Equals(_appliedDescending, s.TreeSortDescending)
            If Not sortChanged AndAlso Not colsChanged AndAlso Not directionChanged Then Return

            If colsChanged Then ApplyTreeColumns(s)
            _appliedSortIsDate = s.TreeSortIsDate
            _appliedDescending = s.TreeSortDescending

            If sortChanged AndAlso _treeRows IsNot Nothing Then
                ' The two sorts do not show the same rows: by date = every source of the
                ' year, by name = the SS in the combo. So a change of sort asks the server
                ' again; the selected node stays selected if it is still in the list.
                ReincarcaArboreleDupaSortare()
            ElseIf directionChanged AndAlso _treeRows IsNot Nothing Then
                ' Same rows, other direction: re-laid from the kept rows, selection kept.
                PopulateTree(_treeRows, If(_currentInfo Is Nothing, Nothing, _currentInfo.CodAngajament))
            Else
                tree.Invalidate()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.ApplyTreeOptionsFromStore", ex)
        End Try
    End Sub

    ' Async boundary (fire-and-forget from a settings event): LoadTreeAsync already shows its
    ' own failures to the operator; anything else is logged and swallowed.
    Private Async Sub ReincarcaArboreleDupaSortare()
        Try
            Await LoadTreeAsync(pastreazaSelectia:=True)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.ReincarcaArboreleDupaSortare", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Puts the columns the store asks for under the sort in force: CODANGAJAMENT, SURSE,
    ''' both, or none (the caption, the Descriere, then takes the whole row). The cells are
    ''' written for every row anyway (<c>PopulateTree</c>), so showing a column needs no reload.
    ''' </summary>
    Private Sub ApplyTreeColumns(s As AppSettings)
        Try
            ArgumentNullException.ThrowIfNull(s)
            Dim cols As New List(Of ColumnDef)()
            If s.TreeShowCod Then cols.Add(TextColumn(COL_COD, "Cod angajament", s.TreeCodColumnWidth))
            If s.TreeShowSurse Then cols.Add(TextColumn(COL_SURSE, "Surse", s.TreeSurseColumnWidth))
            tree.ConfigureListMode(cols)

            _appliedShowCod = s.TreeShowCod
            _appliedShowSurse = s.TreeShowSurse
            _appliedCodWidth = s.TreeCodColumnWidth
            _appliedSurseWidth = s.TreeSurseColumnWidth
            If Not _appliedSortIsDate.HasValue Then _appliedSortIsDate = s.TreeSortIsDate
            If Not _appliedDescending.HasValue Then _appliedDescending = s.TreeSortDescending
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.ApplyTreeColumns", ex)
            Throw
        End Try
    End Sub

    Private Shared Function TextColumn(name As String, header As String, width As Integer) As ColumnDef
        Return New ColumnDef With {
            .Name = name,
            .Header = header,
            .Width = width,
            .ColType = En_ColType.ColType_Text,
            .Align = En_ColAlign.ColAlign_Left,
            .HeaderBackColor = Color.Empty,
            .HeaderForeColor = Color.Empty,
            .HeaderAlign = En_ColAlign.ColAlign_Inherit}
    End Function

    ''' <summary>
    ''' The rows in the order the store asks for.
    ''' <list type="bullet">
    ''' <item>By name: Descriere in Romanian order, the code breaking ties.</item>
    ''' <item>By date: DataCreare. A row whose date was not downloaded yet goes LAST, and
    ''' those rows are ordered by name among themselves, whatever the direction.</item>
    ''' </list>
    ''' Direction: <c>TreeSortDescending</c>, ascending by default (operator, 23.09.2026). The
    ''' code breaks every remaining tie, so two loads never swap rows under the operator.
    ''' </summary>
    Private Function SortRows(rows As IReadOnlyList(Of AngajamentTreeInfo)) As IEnumerable(Of AngajamentTreeInfo)
        Try
            If rows Is Nothing Then Return Array.Empty(Of AngajamentTreeInfo)()
            rows = OneRowPerAngajament(rows)

            Dim s As AppSettings = AppSettings.Current
            Dim byName As Func(Of AngajamentTreeInfo, String) = Function(i) If(i.Descriere, String.Empty).Trim()
            Dim byCod As Func(Of AngajamentTreeInfo, String) = Function(i) If(i.CodAngajament, String.Empty)

            If Not s.TreeSortIsDate Then
                Dim byNameOrdered As IOrderedEnumerable(Of AngajamentTreeInfo) =
                    If(s.TreeSortDescending,
                       rows.OrderByDescending(byName, NameComparer),
                       rows.OrderBy(byName, NameComparer))
                Return byNameOrdered.ThenBy(byCod, StringComparer.OrdinalIgnoreCase).ToList()
            End If

            Dim withDate As IEnumerable(Of AngajamentTreeInfo) = rows.Where(Function(i) i.DataCreare.HasValue)
            Dim byDate As Func(Of AngajamentTreeInfo, Date) = Function(i) i.DataCreare.Value
            Dim dated As IEnumerable(Of AngajamentTreeInfo) =
                If(s.TreeSortDescending,
                   withDate.OrderByDescending(byDate),
                   withDate.OrderBy(byDate)).
                     ThenBy(byName, NameComparer).
                     ThenBy(byCod, StringComparer.OrdinalIgnoreCase)
            Dim undated As IEnumerable(Of AngajamentTreeInfo) =
                rows.Where(Function(i) Not i.DataCreare.HasValue).
                     OrderBy(byName, NameComparer).
                     ThenBy(byCod, StringComparer.OrdinalIgnoreCase)
            Return dated.Concat(undated).ToList()
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.SortRows", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' One tree row per angajament, whatever the list holds. An angajament can have several
    ''' sources (operator, 23.09.2026): the server already folds them into one row
    ''' (GROUP_CONCAT over FX_Indicatori, CodAngajament is the primary key), but a second row
    ''' with the same code would collide on the node key, so any repeat is merged here: the
    ''' first row is kept and gets the union of the sources.
    ''' </summary>
    Private Shared Function OneRowPerAngajament(rows As IReadOnlyList(Of AngajamentTreeInfo)) As IReadOnlyList(Of AngajamentTreeInfo)
        Dim result As New List(Of AngajamentTreeInfo)(rows.Count)
        Dim byCod As New Dictionary(Of String, AngajamentTreeInfo)(StringComparer.OrdinalIgnoreCase)
        For Each info As AngajamentTreeInfo In rows
            If info Is Nothing Then Continue For
            Dim cod As String = If(info.CodAngajament, String.Empty).Trim()
            Dim first As AngajamentTreeInfo = Nothing
            If byCod.TryGetValue(cod, first) Then
                first.Surse = FormatSurse(first.Surse & ";" & info.Surse)
                Continue For
            End If
            byCod(cod) = info
            result.Add(info)
        Next
        Return result
    End Function

    ''' <summary>
    ''' The sources of one angajament as the operator reads them: each SS once, in order,
    ''' separated by «,» (the server sends them joined by «;»). Empty stays empty.
    ''' </summary>
    Private Shared Function FormatSurse(raw As String) As String
        If String.IsNullOrWhiteSpace(raw) Then Return String.Empty
        Dim parts As IEnumerable(Of String) =
            raw.Split({";"c, ","c}, StringSplitOptions.RemoveEmptyEntries).
                Select(Function(p) p.Trim()).
                Where(Function(p) p.Length > 0).
                Distinct(StringComparer.OrdinalIgnoreCase).
                OrderBy(Function(p) p, StringComparer.OrdinalIgnoreCase)
        Return String.Join(",", parts)
    End Function

    ' ---------------- the menu ----------------

    ''' <summary>The right icon of the tree's header opens the tree options menu under it.</summary>
    Private Sub Tree_HeaderRightIconClicked(e As MouseEventArgs) Handles tree.HeaderRightIconClicked
        Try
            ' A second press on the icon CLOSES the menu -- see CapBar_OptionButtonClick.
            If CustomPopup.ClosedJustNow Then Return
            Dim ancora As Rectangle = tree.HeaderRightIconRect
            If ancora.IsEmpty Then Return
            ShowTreeOptionsMenu(tree, ancora)
        Catch ex As Exception
            ' UI boundary (event handler): log and swallow.
            GlobalErrorLog.Write("MainForm.Tree_HeaderRightIconClicked", ex)
        End Try
    End Sub

    ''' <summary>The «Sortare» button (Access btnSort) opens the same menu.</summary>
    Private Sub BtnSort_Click(sender As Object, e As EventArgs) Handles btnSort.Click
        Try
            If CustomPopup.ClosedJustNow Then Return
            ShowTreeOptionsMenu(btnSort, btnSort.ClientRectangle)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.btnSort_Click", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Two sort rows (the one in force checked), a separator, two column rows (checked =
    ''' shown under the sort in force). The icons are placeholders the operator may change.
    ''' </summary>
    Private Sub ShowTreeOptionsMenu(anchor As Control, anchorRect As Rectangle)
        Try
            Dim s As AppSettings = AppSettings.Current
            Dim coloane As Image = My.Resources.Resources.cells
            Dim rows As New List(Of CustomPopupItem) From {
                New CustomPopupItem(TREE_SORT_NAME, "Sortare după &nume", My.Resources.Resources.vertical) With {
                    .Checked = Not s.TreeSortIsDate},
                New CustomPopupItem(TREE_SORT_DATE, "Sortare după &data creării", My.Resources.Resources.calendar) With {
                    .Checked = s.TreeSortIsDate},
                CustomPopupItem.Separator(),
                New CustomPopupItem(TREE_COL_COD, "Afișare coloana &CODANGAJAMENT", coloane) With {
                    .Checked = s.TreeShowCod},
                New CustomPopupItem(TREE_COL_SURSE, "Afișare coloana &SURSE", coloane) With {
                    .Checked = s.TreeShowSurse}
            }

            ' NOT in a «Using»: shown modeless, the popup disposes itself when it closes.
            Dim menu As New CustomPopup(rows)
            AddHandler menu.ItemClicked, AddressOf TreeOptionsMenu_ItemClicked
            menu.ShowBelow(anchor, anchorRect)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.ShowTreeOptionsMenu", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Writes the choice into the store; <see cref="AppSettings_TreeOptionsChanged"/> then
    ''' re-lays the tree, exactly as when the settings page changes it. A column row flips the
    ''' column for the sort IN FORCE only -- each sort keeps its own pair.
    ''' </summary>
    Private Sub TreeOptionsMenu_ItemClicked(sender As Object, e As CustomPopupItemEventArgs)
        Try
            Dim copie As AppSettings = AppSettings.Current.Clone()
            Select Case e.Item.Key
                Case TREE_SORT_NAME
                    If Not copie.TreeSortIsDate Then Return
                    copie.TreeSort = AppSettings.TreeSortName
                Case TREE_SORT_DATE
                    If copie.TreeSortIsDate Then Return
                    copie.TreeSort = AppSettings.TreeSortDate
                Case TREE_COL_COD
                    If copie.TreeSortIsDate Then
                        copie.TreeDateShowCod = Not copie.TreeDateShowCod
                    Else
                        copie.TreeNameShowCod = Not copie.TreeNameShowCod
                    End If
                Case TREE_COL_SURSE
                    If copie.TreeSortIsDate Then
                        copie.TreeDateShowSurse = Not copie.TreeDateShowSurse
                    Else
                        copie.TreeNameShowSurse = Not copie.TreeNameShowSurse
                    End If
                Case Else
                    ' No silent no-ops: a row added to the menu and forgotten here must show.
                    Throw New ArgumentException("Rând necunoscut în meniul arborelui: «" & e.Item.Key & "».")
            End Select
            copie.Save()
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.TreeOptionsMenu_ItemClicked", ex)
            KBotMessage.Show(Me, "Opțiunea nu a putut fi aplicată: " & ex.Message, "Arbore angajamente",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

End Class
