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
    Private ReadOnly _created As New HashSet(Of Version)()

    Public Sub New()
        InitializeComponent()
        If Not KBotDesignTime.IsDesignTime(Me) Then
            gridNotes.Column(KBotDataView.CheckColumnKey).ValueType = KBotValueType.Boolean
        End If
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
            ' versions created here but still without a note are not in the file yet: they stay until the first note is written
            _versions = _file.Versions.Append(ReleasePreview.CurrentVersion(k_root)).Concat(_created).Distinct().
                OrderByDescending(Function(k_version) k_version).ToArray()
            FillGrid()
            RefillCombo(TargetVersion())
            cmbTarget.Enabled = True
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
            If gridNotes.SortDirection = KBotSortDirection.None Then gridNotes.ApplySort(ColOrder, KBotSortDirection.Ascending)
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
        k_row.IsDraft = k_note Is Nothing
        k_row(KBotDataView.CheckColumnKey) = False
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
            k_row.IsDraft = False
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

    Private Sub CmbTarget_TextChanged(k_sender As Object, e As EventArgs) Handles cmbTarget.TextChanged
        UpdateMoveButton()
    End Sub

    ''' <summary>The version in the combo: the picked one, or the number typed by hand (1.2 / 1.2.3 / 1.2.3.4, a missing part is 0). Nothing when it is not a version.</summary>
    Private Function TargetVersion() As Version
        Dim k_picked As Version = TryCast(cmbTarget.SelectedItem, Version)
        If k_picked IsNot Nothing Then Return k_picked
        Dim k_text As String = cmbTarget.Text.Trim()
        Dim k_typed As Version = Nothing
        If Regex.IsMatch(k_text, "^\d+(\.\d+){1,3}$") AndAlso Version.TryParse(k_text, k_typed) Then Return ReleasePreview.Normalize(k_typed)
        Return Nothing
    End Function

    ''' <summary>Puts the versions in the combo again and selects <paramref name="k_select"/> when it is one of them.</summary>
    Private Sub RefillCombo(k_select As Version)
        cmbTarget.Items.Clear()
        For Each k_version As Version In _versions
            cmbTarget.Items.Add(k_version)
        Next
        If k_select IsNot Nothing AndAlso _versions.Contains(k_select) Then cmbTarget.SelectedItem = k_select
    End Sub

    Private Sub UpdateMoveButton()
        Dim k_target As Version = TargetVersion()
        btnMove.Enabled = _file IsNot Nothing AndAlso k_target IsNot Nothing AndAlso CheckedNotes().Any(Function(k_note) k_note.ReleaseVersion <> k_target)
        btnCreate.Enabled = _file IsNot Nothing AndAlso k_target IsNot Nothing AndAlso Not _versions.Contains(k_target)
    End Sub

    ''' <summary>Adds a version that has no section in the file yet; the section is written with its first note.</summary>
    Private Sub AddVersion(k_version As Version)
        _created.Add(k_version)
        _versions = _versions.Append(k_version).Distinct().OrderByDescending(Function(k_item) k_item).ToArray()
        FillGrid()
        RefillCombo(k_version)
    End Sub

    Private Sub BtnCreate_Click(k_sender As Object, e As EventArgs) Handles btnCreate.Click
        Try
            If _file Is Nothing OrElse Not gridNotes.CommitPendingEdit() Then Return
            Dim k_version As Version = TargetVersion()
            If k_version Is Nothing OrElse _versions.Contains(k_version) Then Return
            AddVersion(k_version)
            UpdateMoveButton()
            lblStatus.Text = $"Versiunea {k_version} a fost creată. Deschideți-o în listă și scrieți prima noutate pe rândul gol; secțiunea se scrie în fișier odată cu ea."
        Catch ex As Exception
            ShowError("Versiunea nu a putut fi creată", ex)
        End Try
    End Sub

    Private Sub BtnMove_Click(k_sender As Object, e As EventArgs) Handles btnMove.Click
        If _file Is Nothing OrElse Not gridNotes.CommitPendingEdit() Then Return
        Dim k_target As Version = TargetVersion()
        If k_target Is Nothing Then Return
        Try
            Dim k_count As Integer = _file.Move(CheckedNotes(), k_target)
            If k_count = 0 Then Return
            ' a version typed by hand gets its section from the move itself
            If _versions.Contains(k_target) Then
                FillGrid()
            Else
                AddVersion(k_target)
            End If
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
