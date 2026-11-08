Option Strict On
Imports System.Globalization
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' The print list of slice 0099: what ORD and DDF show on the right when a NON-leaf node of the tree
''' (a month, or the «Toate…» root) is selected, instead of the document pages. One row per revision /
''' ordonantare under the node, with a tick to select it, its signatures, when it was signed, whether it
''' was printed («Listat») and how many times.
'''
''' <para>Two actions in the footer, both on the TICKED rows: «Generează și imprimă» (the documents
''' are made ready and sent to the printer without being opened) and «Salvează local» (copies in a
''' folder the operator picks).</para>
'''
''' <para><b>«Listat» by hand.</b> Ticking «Listat» on a row whose count is 0 asks once whether to mark
''' the document as printed; «Da» sends one counted print to the server, «Nu» takes the tick back. No
''' message afterwards -- only an error is said. A row that was printed already (count above 0) has the
''' box locked: the count only goes up.</para>
'''
''' <para>The page knows nothing about DDF or ORD: the owning view hands it <see cref="PrintListItem"/>
''' rows, each carrying the way to get its PDF. The server call is the same <see cref="IPrintCountApi"/>
''' the print watcher of the Adobe pane uses.</para>
''' </summary>
Friend Class PrintListPage
    Implements IThemedControl

    Private Const COL_SEL As String = "sel"
    Private Const COL_DOC As String = "doc"
    Private Const COL_SEM As String = "sem"
    Private Const COL_DATA As String = "data"
    Private Const COL_LISTAT As String = "listat"
    Private Const COL_NR As String = "nr"

    Private Const MENU_ALL As String = "toate"
    Private Const MENU_UNLISTED As String = "nelistate"

    Private Shared ReadOnly _roCulture As New CultureInfo("ro-RO")

    Private _counter As IPrintCountApi
    Private _items As New List(Of PrintListItem)()
    ' A print / save / mark is running: the grid and the buttons are locked.
    Private _busy As Boolean
    ' The page is writing cells itself: its own writes must not be taken for the operator's.
    Private _suppress As Boolean
    Private _lastFolder As String

    Public Sub New()
        InitializeComponent()
        UpdateButtons()
    End Sub

    ''' <summary>The client's print-count route; Nothing (a test double) = prints are not counted and
    ''' «Listat» cannot be set by hand.</summary>
    Public Sub Initialize(counter As IPrintCountApi)
        _counter = counter
    End Sub

    ''' <summary>Replaces the rows. Nothing ticked afterwards.</summary>
    Public Sub SetItems(items As IEnumerable(Of PrintListItem))
        Try
            _items = If(items Is Nothing, New List(Of PrintListItem)(), items.ToList())
            _suppress = True
            grila.BeginUpdate()
            Try
                grila.ClearRows()
                For Each item As PrintListItem In _items
                    Dim row As KBotDataRow = grila.AddRow()
                    row.Tag = item
                    FillRow(row, item, selected:=False)
                Next
            Finally
                grila.EndUpdate()
                _suppress = False
            End Try
            UpdateButtons()
            lblStatus.Text = StatusText()
        Catch ex As Exception
            GlobalErrorLog.Write("PrintListPage.SetItems", ex)
            Throw
        End Try
    End Sub

    Private Sub FillRow(row As KBotDataRow, item As PrintListItem, selected As Boolean)
        row(COL_SEL) = selected
        row(COL_DOC) = item.Label
        row(COL_SEM) = item.SignaturesText
        row(COL_DATA) = If(item.SignedAt.HasValue, item.SignedAt.Value.ToString("dd.MM.yyyy HH:mm", _roCulture), String.Empty)
        row(COL_LISTAT) = item.IsListed
        row(COL_NR) = item.PrintCount
    End Sub

    ' ── Selection ────────────────────────────────────────────────────────────
    Private Shared Function IsTicked(value As Object) As Boolean
        Return TypeOf value Is Boolean AndAlso CBool(value)
    End Function

    Private Function SelectedItems() As List(Of PrintListItem)
        Dim result As New List(Of PrintListItem)()
        For i As Integer = 0 To grila.RowCount - 1
            Dim row As KBotDataRow = grila.Rows(i)
            Dim item As PrintListItem = TryCast(row.Tag, PrintListItem)
            If item IsNot Nothing AndAlso IsTicked(row(COL_SEL)) Then result.Add(item)
        Next
        Return result
    End Function

    Private Function StatusText() As String
        If _items.Count = 0 Then Return "Nu sunt documente de listat."
        Dim n As Integer = SelectedItems().Count
        If n = 0 Then Return "Nimic bifat."
        Return $"{n} din {_items.Count} documente bifate."
    End Function

    Private Sub UpdateButtons()
        Dim any As Boolean = Not _busy AndAlso SelectedItems().Count > 0
        btnImprima.Enabled = any
        btnSalveaza.Enabled = any
    End Sub

    ' The header icon of the tick column: two ways to tick / untick in bulk.
    Private Sub grila_HeaderRightIconClicked(sender As Object, e As KBotColumnEventArgs) Handles grila.HeaderRightIconClicked
        Try
            If _busy OrElse Not String.Equals(e.ColumnKey, COL_SEL, StringComparison.Ordinal) Then Return
            Dim intrari As New List(Of CustomPopupItem) From {
                New CustomPopupItem(MENU_ALL, "Selectează / deselectează toate"),
                New CustomPopupItem(MENU_UNLISTED, "Selectează / deselectează doar cele nelistate")
            }
            Dim meniu As New CustomPopup(intrari)
            AddHandler meniu.ItemClicked, Sub(s As Object, ev As CustomPopupItemEventArgs) ApplyBulkChoice(ev.Item.Key)
            meniu.ShowBelow(grila, e.IconBounds)
        Catch ex As Exception
            GlobalErrorLog.Write("PrintListPage.grila_HeaderRightIconClicked", ex)
        End Try
    End Sub

    ''' <summary>
    ''' «All»: ticks every row, or unticks every row when all are ticked already. «Unlisted»: the same
    ''' over the rows whose count is 0 only -- the listed rows keep whatever the operator set.
    ''' </summary>
    Private Sub ApplyBulkChoice(key As String)
        Try
            Dim onlyUnlisted As Boolean
            Select Case key
                Case MENU_ALL : onlyUnlisted = False
                Case MENU_UNLISTED : onlyUnlisted = True
                Case Else
                    Throw New ArgumentException($"Alegere necunoscută: {key}", NameOf(key))
            End Select

            Dim scope As New List(Of KBotDataRow)()
            For i As Integer = 0 To grila.RowCount - 1
                Dim row As KBotDataRow = grila.Rows(i)
                Dim item As PrintListItem = TryCast(row.Tag, PrintListItem)
                If item Is Nothing Then Continue For
                If onlyUnlisted AndAlso item.IsListed Then Continue For
                scope.Add(row)
            Next
            If scope.Count = 0 Then Return

            Dim target As Boolean = Not scope.All(Function(r) IsTicked(r(COL_SEL)))
            _suppress = True
            grila.BeginUpdate()
            Try
                For Each row As KBotDataRow In scope
                    row(COL_SEL) = target
                Next
            Finally
                grila.EndUpdate()
                _suppress = False
            End Try
            UpdateButtons()
            lblStatus.Text = StatusText()
        Catch ex As Exception
            GlobalErrorLog.Write("PrintListPage.ApplyBulkChoice", ex)
        End Try
    End Sub

    ' «Listat» is locked once the document was printed: the count only goes up.
    Private Sub grila_CellFormatting(sender As Object, e As KBotCellFormattingEventArgs) Handles grila.CellFormatting
        Try
            If Not String.Equals(e.ColumnKey, COL_LISTAT, StringComparison.Ordinal) Then Return
            Dim item As PrintListItem = TryCast(e.Row?.Tag, PrintListItem)
            If item IsNot Nothing AndAlso item.IsListed Then e.Enabled = False
        Catch ex As Exception
            GlobalErrorLog.Write("PrintListPage.grila_CellFormatting", ex)
        End Try
    End Sub

    Private Sub grila_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) Handles grila.CellValueChanged
        Try
            If _suppress Then Return
            Select Case e.ColumnKey
                Case COL_SEL
                    UpdateButtons()
                    lblStatus.Text = StatusText()
                Case COL_LISTAT
                    OnListatChanged(e.RowIndex)
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("PrintListPage.grila_CellValueChanged", ex)
        End Try
    End Sub

    ' ── «Listat» ticked by hand ──────────────────────────────────────────────
    ' Only a row with count 0 reaches here (the box of the others is locked). Ticked -> ask once;
    ' «Da» counts one print, «Nu» takes the tick back. No message after success.
    Private Async Sub OnListatChanged(rowIndex As Integer)
        Dim row As KBotDataRow = Nothing
        Dim item As PrintListItem = Nothing
        Try
            If rowIndex < 0 OrElse rowIndex >= grila.RowCount Then Return
            row = grila.Rows(rowIndex)
            item = TryCast(row.Tag, PrintListItem)
            If item Is Nothing OrElse item.IsListed Then Return
            If Not IsTicked(row(COL_LISTAT)) Then Return

            Dim answer As DialogResult = KBotMessage.Show(FindForm(),
                $"Marchezi documentul «{item.Label}» ca fiind listat?",
                "Document listat", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            If answer <> DialogResult.Yes Then
                SetListat(row, False)
                Return
            End If

            SetBusy(True)
            Try
                Await RecordAsync(item).ConfigureAwait(True)
            Finally
                SetBusy(False)
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("PrintListPage.OnListatChanged", ex)
            If row IsNot Nothing Then SetListat(row, False)
            KBotMessage.Show(FindForm(),
                "Documentul nu a putut fi marcat ca listat: " & ex.Message,
                "Document listat", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub SetListat(row As KBotDataRow, value As Boolean)
        _suppress = True
        Try
            row(COL_LISTAT) = value
        Finally
            _suppress = False
        End Try
        grila.Invalidate()
    End Sub

    ''' <summary>
    ''' One more print of <paramref name="item"/> on the server, then the row shows it. The server
    ''' answers with the count of the ROW it raised (the stored PDF or the document row), not the
    ''' total of the document, so the total is raised here by one. Boundary (HTTP): logs, re-throws.
    ''' </summary>
    Private Async Function RecordAsync(item As PrintListItem) As Task
        Try
            If _counter Is Nothing Then
                Throw New InvalidOperationException("Numărarea tipăririlor nu este disponibilă în acest context.")
            End If
            Dim result As PrintCountResult =
                Await _counter.RecordPrintAsync(item.Kind, item.Id, CancellationToken.None).ConfigureAwait(True)
            If result Is Nothing OrElse Not result.Counted Then
                Throw New InvalidOperationException("Serverul nu a putut număra tipărirea acestui document.")
            End If
            item.PrintCount += 1
            item.CountChanged?.Invoke(item.PrintCount)
            RefreshRowOf(item)
        Catch ex As Exception
            GlobalErrorLog.Write("PrintListPage.RecordAsync", ex)
            Throw
        End Try
    End Function

    Private Sub RefreshRowOf(item As PrintListItem)
        For i As Integer = 0 To grila.RowCount - 1
            Dim row As KBotDataRow = grila.Rows(i)
            If Not ReferenceEquals(row.Tag, item) Then Continue For
            _suppress = True
            Try
                FillRow(row, item, IsTicked(row(COL_SEL)))
            Finally
                _suppress = False
            End Try
            grila.InvalidateRow(i)
            Return
        Next
    End Sub

    ' ── The two footer actions ───────────────────────────────────────────────
    Private Sub SetBusy(busy As Boolean)
        _busy = busy
        grila.ReadOnlyGrid = busy
        UpdateButtons()
    End Sub

    Private Async Sub btnImprima_Click(sender As Object, e As EventArgs) Handles btnImprima.Click
        Try
            Dim chosen As List(Of PrintListItem) = SelectedItems()
            If chosen.Count = 0 OrElse _busy Then Return
            Dim choice As PrinterChoice = PdfPrinter.ChoosePrinter(FindForm())
            If choice Is Nothing Then Return

            Dim done As Integer = 0
            Dim failures As New List(Of String)()
            SetBusy(True)
            Try
                For i As Integer = 0 To chosen.Count - 1
                    Dim item As PrintListItem = chosen(i)
                    lblStatus.Text = $"Se pregătește și se trimite la imprimantă: {i + 1} din {chosen.Count} — {item.Label}"
                    Dim printed As Boolean = False
                    Try
                        Dim path As String = Await item.ObtainPdfAsync.Invoke().ConfigureAwait(True)
                        For copy As Integer = 1 To choice.Copies
                            Await PdfPrinter.PrintAsync(path, choice.PrinterName).ConfigureAwait(True)
                        Next
                        printed = True
                        Await RecordAsync(item).ConfigureAwait(True)
                        done += 1
                    Catch ex As Exception
                        GlobalErrorLog.Write("PrintListPage.btnImprima_Click", ex)
                        failures.Add(If(printed,
                            $"{item.Label}: tipărit, dar nu a putut fi numărat — {ex.Message}",
                            $"{item.Label}: {ex.Message}"))
                    End Try
                Next
            Finally
                SetBusy(False)
            End Try

            lblStatus.Text = $"{done} din {chosen.Count} documente trimise la imprimantă."
            If failures.Count > 0 Then
                KBotMessage.Show(FindForm(),
                    "Nu toate documentele au putut fi tipărite:" & vbCrLf & vbCrLf & String.Join(vbCrLf, failures),
                    "Generează și imprimă", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("PrintListPage.btnImprima_Click", ex)
            SetBusy(False)
            KBotMessage.Show(FindForm(), "Tipărirea nu a putut porni: " & ex.Message,
                             "Generează și imprimă", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Async Sub btnSalveaza_Click(sender As Object, e As EventArgs) Handles btnSalveaza.Click
        Try
            Dim chosen As List(Of PrintListItem) = SelectedItems()
            If chosen.Count = 0 OrElse _busy Then Return

            Dim folder As String
            Using dlg As New FolderBrowserDialog()
                dlg.Description = "Alegeți dosarul în care se salvează documentele bifate."
                dlg.UseDescriptionForTitle = True
                dlg.ShowNewFolderButton = True
                If Not String.IsNullOrEmpty(_lastFolder) AndAlso IO.Directory.Exists(_lastFolder) Then
                    dlg.SelectedPath = _lastFolder
                End If
                If dlg.ShowDialog(FindForm()) <> DialogResult.OK Then Return
                folder = dlg.SelectedPath
            End Using
            _lastFolder = folder

            Dim done As Integer = 0
            Dim failures As New List(Of String)()
            SetBusy(True)
            Try
                For i As Integer = 0 To chosen.Count - 1
                    Dim item As PrintListItem = chosen(i)
                    lblStatus.Text = $"Se salvează: {i + 1} din {chosen.Count} — {item.Label}"
                    Try
                        Dim path As String = Await item.ObtainPdfAsync.Invoke().ConfigureAwait(True)
                        Dim target As String = UniquePath(folder, IO.Path.GetFileName(path))
                        Await Task.Run(Sub() IO.File.Copy(path, target, overwrite:=False)).ConfigureAwait(True)
                        done += 1
                    Catch ex As Exception
                        GlobalErrorLog.Write("PrintListPage.btnSalveaza_Click", ex)
                        failures.Add($"{item.Label}: {ex.Message}")
                    End Try
                Next
            Finally
                SetBusy(False)
            End Try

            lblStatus.Text = $"{done} din {chosen.Count} documente salvate în {folder}."
            If failures.Count > 0 Then
                KBotMessage.Show(FindForm(),
                    "Nu toate documentele au putut fi salvate:" & vbCrLf & vbCrLf & String.Join(vbCrLf, failures),
                    "Salvează local", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("PrintListPage.btnSalveaza_Click", ex)
            SetBusy(False)
            KBotMessage.Show(FindForm(), "Salvarea nu a putut porni: " & ex.Message,
                             "Salvează local", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    ' A name that does not overwrite a file already in the folder: «name (2).pdf», «name (3).pdf»…
    Private Shared Function UniquePath(folder As String, fileName As String) As String
        Dim candidate As String = IO.Path.Combine(folder, fileName)
        Dim n As Integer = 2
        Dim stem As String = IO.Path.GetFileNameWithoutExtension(fileName)
        Dim ext As String = IO.Path.GetExtension(fileName)
        While IO.File.Exists(candidate)
            candidate = IO.Path.Combine(folder, $"{stem} ({n}){ext}")
            n += 1
        End While
        Return candidate
    End Function

    ''' <summary>Cascade: the page's own chrome; the grid themes itself.</summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            BackColor = p.SurfaceAltColor
            pnlJos.BackColor = p.SurfaceAltColor
            pnlGap.BackColor = p.SurfaceAltColor
            lblStatus.ForeColor = p.TextDimColor
            lblStatus.BackColor = p.SurfaceAltColor
        Catch ex As Exception
            GlobalErrorLog.Write("PrintListPage.ApplyTheme", ex)
        End Try
    End Sub

End Class
