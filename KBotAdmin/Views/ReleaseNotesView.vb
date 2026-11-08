Option Strict On
Imports System.Globalization
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions

''' <summary>SLICE-ME01-23. The NOUTATI page of Publicare.</summary>
Public Class ReleaseNotesView
    Implements IAdminView

    Private Const ColVersion As String = "version"
    Private Const ColOrder As String = "order"
    Private Const ColNote As String = "note"
    Private Shared ReadOnly RomanianOrder As StringComparer = StringComparer.Create(CultureInfo.GetCultureInfo("ro-RO"), True)
    Private _file As ReleaseNotesFile
    Private _versions As Version() = Array.Empty(Of Version)()

    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub Attach(k_context As AdminContext) Implements IAdminView.Attach
    End Sub

    Public Sub SelectionChanged() Implements IAdminView.SelectionChanged
    End Sub

    Public Function ActivatedAsync() As Task Implements IAdminView.ActivatedAsync
        If Not gridNotes.CommitPendingEdit() Then Return Task.CompletedTask
        Try
            Dim k_root As String = ScriptCatalog.FindRepoRoot()
            If k_root.Length = 0 Then Throw New DirectoryNotFoundException("Nu găsesc folderul proiectului K-BOT.")
            _file = New ReleaseNotesFile(k_root)
            _versions = _file.Versions.Append(ReleasePreview.CurrentVersion(k_root)).Distinct().
                OrderByDescending(Function(k_version) k_version).ToArray()
            FillGrid()
            Dim k_previous As Version = TryCast(cmbTarget.SelectedItem, Version)
            cmbTarget.Items.Clear()
            For Each k_version As Version In _versions
                cmbTarget.Items.Add(k_version)
            Next
            If _versions.Contains(k_previous) Then cmbTarget.SelectedItem = k_previous
            cmbTarget.Enabled = _versions.Length > 1
            lblStatus.Text = String.Empty
        Catch ex As Exception
            _file = Nothing
            _versions = Array.Empty(Of Version)()
            gridNotes.ClearRows()
            cmbTarget.Items.Clear()
            cmbTarget.Enabled = False
            ShowError("Noutățile nu au putut fi încărcate", ex)
        End Try
        UpdateMoveButton()
        Return Task.CompletedTask
    End Function

    Private Sub FillGrid()
        Dim k_byVersion = _file.Notes.ToLookup(Function(k_note) k_note.ReleaseVersion)
        gridNotes.BeginUpdate()
        Try
            gridNotes.ClearRows()
            For Each k_version As Version In _versions
                Dim k_key As String = VersionKey(k_version)
                Dim k_order As Integer = 0
                For Each k_note In k_byVersion(k_version).OrderBy(Function(k_entry) k_entry.NoteText, RomanianOrder)
                    AddRow(k_key, k_order, k_note)
                    k_order += 1
                Next
                AddRow(k_key, Integer.MaxValue)
            Next
            gridNotes.ApplySort(ColOrder, KBotSortDirection.Ascending)
        Finally
            gridNotes.EndUpdate()
        End Try
    End Sub

    Private Sub AddRow(k_versionKey As String, k_order As Integer, Optional k_note As ReleaseNotesFile.Block = Nothing)
        Dim k_row As KBotDataRow = gridNotes.AddRow()
        k_row(ColVersion) = k_versionKey
        k_row(ColOrder) = k_order
        k_row(ColNote) = If(k_note?.NoteText, String.Empty)
        k_row.Tag = k_note
    End Sub

    Private Shared Function VersionKey(k_version As Version) As String
        ' Fixed-width components let the grid sort the version numerically and keep its group key stable.
        Return String.Join(".", {k_version.Major, k_version.Minor, k_version.Build, k_version.Revision}.
            Select(Function(k_part) k_part.ToString("D10", CultureInfo.InvariantCulture)))
    End Function

    Private Sub GridNotes_GroupFormatting(k_sender As Object, e As KBotGroupFormattingEventArgs) Handles gridNotes.GroupFormatting
        e.Caption = "Versiunea " & Version.Parse(CStr(e.Value)).ToString()
    End Sub

    Private Sub GridNotes_CellFormatting(k_sender As Object, e As KBotCellFormattingEventArgs) Handles gridNotes.CellFormatting
        If e.ColumnKey = ColNote Then
            e.ReadOnlyCell = e.Row.Tag IsNot Nothing
        ElseIf e.ColumnKey = KBotDataView.CheckColumnKey AndAlso e.Row.Tag Is Nothing Then
            e.Enabled = False
            e.HideGlyph = True
        End If
    End Sub

    Private Sub GridNotes_CellValidating(k_sender As Object, e As KBotCellValidatingEventArgs) Handles gridNotes.CellValidating
        If e.ColumnKey <> ColNote Then Return
        Try
            Dim k_text As String = Regex.Replace(CStr(e.ProposedValue).Trim(), "[\r\n]+", " ")
            k_text = Regex.Replace(k_text, "^[-*•]\s+", "")
            ' Match the plain Romanian text expected by the release scripts and the installer.
            k_text = Regex.Replace(k_text.Normalize(NormalizationForm.FormD), "[\u0302\u0306\u0326\u0327]", "").Normalize().Trim()
            e.ProposedValue = k_text
            If k_text.Length = 0 Then Return
            Dim k_row As KBotDataRow = gridNotes.Rows(e.RowIndex)
            Dim k_version As Version = Version.Parse(CStr(k_row(ColVersion)))
            k_row.Tag = _file.Add(k_version, k_text)
            lblStatus.Text = $"Noutatea a fost adăugată în versiunea {k_version}."
        Catch ex As Exception
            e.Cancel = True
            ShowError("Noutatea nu a putut fi salvată", ex)
        End Try
    End Sub

    Private Sub GridNotes_CellValueChanged(k_sender As Object, e As KBotCellValueEventArgs) Handles gridNotes.CellValueChanged
        If e.ColumnKey <> ColNote Then Return
        Dim k_row As KBotDataRow = gridNotes.Rows(e.RowIndex)
        k_row.MarkClean()
        If k_row.Tag Is Nothing Then Return

        Dim k_key As String = CStr(k_row(ColVersion))
        gridNotes.BeginUpdate()
        Try
            gridNotes.SetRowChecked(e.RowIndex, False)
            Dim k_order As Integer = 0
            For Each k_noteRow In gridNotes.Rows.Where(Function(k_item) CStr(k_item(ColVersion)) = k_key AndAlso k_item.Tag IsNot Nothing).
                OrderBy(Function(k_item) CStr(k_item(ColNote)), RomanianOrder)
                k_noteRow(ColOrder) = k_order
                k_order += 1
            Next
            AddRow(k_key, Integer.MaxValue)
        Finally
            gridNotes.EndUpdate()
        End Try
        UpdateMoveButton()
    End Sub

    Private Sub GridNotes_RowChecked(k_sender As Object, e As KBotRowCheckedEventArgs) Handles gridNotes.RowChecked
        If e.Pending <> 0 Then Return
        ' The grid counts all model rows for the group checkbox. Mirror the group's state in its hidden draft checkbox.
        Dim k_key As String = CStr(gridNotes.Rows(e.RowIndex)(ColVersion))
        Dim k_draft As Integer = -1
        Dim k_allChecked As Boolean = True
        For k_i As Integer = 0 To gridNotes.RowCount - 1
            Dim k_row As KBotDataRow = gridNotes.Rows(k_i)
            If CStr(k_row(ColVersion)) <> k_key Then Continue For
            If k_row.Tag Is Nothing Then
                k_draft = k_i
            Else
                k_allChecked = k_allChecked AndAlso gridNotes.GetRowChecked(k_i)
            End If
        Next
        If k_draft >= 0 Then gridNotes.SetRowChecked(k_draft, k_allChecked)
        gridNotes.ClearDirty()
        UpdateMoveButton()
    End Sub

    Private Function CheckedNotes() As IEnumerable(Of ReleaseNotesFile.Block)
        Return gridNotes.GetCheckedRows().Select(Function(k_i) TryCast(gridNotes.Rows(k_i).Tag, ReleaseNotesFile.Block)).
            Where(Function(k_note) k_note IsNot Nothing)
    End Function

    Private Sub CmbTarget_SelectedIndexChanged(k_sender As Object, e As EventArgs) Handles cmbTarget.SelectedIndexChanged
        UpdateMoveButton()
    End Sub

    Private Sub UpdateMoveButton()
        Dim k_target As Version = TryCast(cmbTarget.SelectedItem, Version)
        btnMove.Enabled = k_target IsNot Nothing AndAlso CheckedNotes().Any(Function(k_note) k_note.ReleaseVersion <> k_target)
    End Sub

    Private Sub BtnMove_Click(k_sender As Object, e As EventArgs) Handles btnMove.Click
        If _file Is Nothing OrElse Not gridNotes.CommitPendingEdit() Then Return
        Dim k_target As Version = TryCast(cmbTarget.SelectedItem, Version)
        If k_target Is Nothing Then Return
        Try
            Dim k_count As Integer = _file.Move(CheckedNotes(), k_target)
            If k_count = 0 Then Return
            FillGrid()
            UpdateMoveButton()
            lblStatus.Text = If(k_count = 1, "O noutate mutată", $"Noutăți mutate ({k_count})") & $" în versiunea {k_target}."
        Catch ex As Exception
            ShowError("Noutățile nu au putut fi mutate", ex)
        End Try
    End Sub

    Private Sub ShowError(k_action As String, ex As Exception)
        GlobalErrorLog.Write("ReleaseNotesView", ex)
        lblStatus.Text = k_action & ". " & ex.Message
    End Sub
End Class
