Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' «Grupe de angajamente» (slice 0008-02, operator 09.10.2026): a modal window that edits the
''' groups of angajamente. A tree on the left (first row «Angajamente negrupate», then the groups,
''' each in its own colour, «+ Adăugare grupă» in its footer); on the right the group's name, its
''' colour (colour picker) and a grid of angajamente (tick, code, name, number of indicators with a
''' tooltip that lists them, editable alias).
'''
''' <para><b>Three modes.</b> «Angajamente negrupate»: the angajamente that are in no group, which
''' the operator drags (drag and drop) onto a group of the tree - saved at once. An existing group:
''' only its angajamente, all ticked; unticking one makes it disappear from the list (it leaves the
''' group on «Salvează»). A new group: ALL angajamente, unticked; «Salvează» needs a name, a colour
''' (black by default) and at least one tick.</para>
'''
''' <para>The alias is saved as soon as the cell is left (it belongs to the angajament, not to a
''' group). <see cref="Modificat"/> tells the shell that something was written, so it reloads its
''' group menu and its tree.</para>
''' </summary>
Partial Public Class GrupeForm

    Private Const ColBifa As String = "bifa"
    Private Const ColCod As String = "cod"
    Private Const ColDenumire As String = "denumire"
    Private Const ColIndicatori As String = "indicatori"
    Private Const ColAlias As String = "alias"
    Private Const DragFormat As String = "KBot.GrupeCod"
    Private Const KeyNegrupate As String = "NEG"
    Private Const KeyNoua As String = "NOUA"

    Private Enum Mod_
        Negrupate
        Existenta
        Noua
    End Enum

    Private ReadOnly _api As IGrupeApi
    Private ReadOnly _gate As ReauthGate
    Private _catalog As GrupeCatalog = New GrupeCatalog()
    Private _mod As Mod_ = Mod_.Negrupate
    Private _current As GrupaInfo
    Private _currentNode As AdvancedTreeControl.TreeItem
    ' The codes ticked in the group being edited (displayed ones; hidden members are kept apart).
    Private ReadOnly _bifate As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
    Private _culoare As Color = Color.Black
    Private _dirty As Boolean
    Private _loading As Boolean
    Private _busy As Boolean
    Private _noua As Boolean
    Private _swatches As New List(Of Bitmap)()
    ' Drag from the grid: the code of the pressed row and where it was pressed.
    Private _dragCod As String
    Private _dragOrigin As Point
    ' The row whose indicators the tooltip shows now (code), to avoid re-showing on every pixel.
    Private _tipCod As String

    ''' <summary>True when something was written to the server (a group, a drop, an alias).</summary>
    Public ReadOnly Property Modificat As Boolean

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
        _gate = ReauthGate.Direct
    End Sub

    Public Sub New(k_api As IGrupeApi, k_gate As ReauthGate)
        ArgumentNullException.ThrowIfNull(k_api)
        ArgumentNullException.ThrowIfNull(k_gate)
        InitializeComponent()
        _api = k_api
        _gate = k_gate
    End Sub

    Private Sub GrupeForm_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try
            If _api Is Nothing Then Return
            ShowMode(Mod_.Negrupate, Nothing)
            LoadCatalog(KeyNegrupate)
        Catch ex As Exception
            GlobalErrorLog.Write("GrupeForm.GrupeForm_Load", ex)
        End Try
    End Sub

    ' ── Loading ─────────────────────────────────────────────────────────────────

    ' UI boundary: logs and shows the error; started without await.
    Private Async Sub LoadCatalog(k_selectKey As String)
        Try
            SetBusy(True, "Se încarcă grupele…")
            Dim k_catalog As GrupeCatalog = Await _gate.RunAsync(
                Function() _api.GetGrupeAsync(CancellationToken.None)).ConfigureAwait(True)
            If IsDisposed Then Return
            _catalog = k_catalog
            BuildTree(k_selectKey)
            SetStatus($"{k_catalog.Grupe.Count} grupe, {k_catalog.Angajamente.Count} angajamente.")
        Catch ex As ApiException
            GlobalErrorLog.Write("GrupeForm.LoadCatalog", ex)
            If Not IsDisposed Then SetStatus(ex.Message)
        Catch ex As Exception
            GlobalErrorLog.Write("GrupeForm.LoadCatalog", ex)
            If Not IsDisposed Then SetStatus("Grupele nu au putut fi încărcate. Detalii în jurnalul de erori.")
        Finally
            If Not IsDisposed Then SetBusy(False, Nothing)
        End Try
    End Sub

    Private Shared Function Escape(k_text As String) As String
        Return If(k_text, String.Empty).Replace("<", "‹").Replace(">", "›").Replace("~~~", "~ ~ ~")
    End Function

    ''' <summary>Fills the tree and selects the row with <paramref name="k_selectKey"/> (or the first).</summary>
    Private Sub BuildTree(k_selectKey As String)
        tree.Clear()
        _currentNode = Nothing
        Dim k_old As List(Of Bitmap) = _swatches
        _swatches = New List(Of Bitmap)()

        Dim k_root As AdvancedTreeControl.TreeItem = tree.AddItem(KeyNegrupate, "<b>Angajamente negrupate</b>",
                                                                  pLeftIconClosed:=My.Resources.Resources.folder_open)
        k_root.Tag = KeyNegrupate
        k_root.Tooltip = "Angajamentele care nu fac parte din nicio grupă." & vbLf &
                         "Trageți un rând peste o grupă ca să-l adăugați în ea."
        Dim k_select As AdvancedTreeControl.TreeItem = k_root

        For Each k_g As GrupaInfo In GrupeUi.Ordonate(_catalog.Grupe)
            Dim k_color As Color = GrupeUi.HexToColor(k_g.Culoare)
            Dim k_bmp As Bitmap = GrupeUi.Swatch(k_color)
            _swatches.Add(k_bmp)
            Dim k_node As AdvancedTreeControl.TreeItem = tree.AddItem(
                $"G{k_g.IdGr}", $"{Escape(k_g.Denumire)} ({k_g.Coduri.Count})", pLeftIconClosed:=k_bmp)
            k_node.Tag = k_g
            k_node.NodeForeColor = k_color
            If String.Equals(k_node.Key, k_selectKey, StringComparison.Ordinal) Then k_select = k_node
        Next

        If _noua Then
            Dim k_new As AdvancedTreeControl.TreeItem = tree.AddItem(KeyNoua, "Grupă nouă")
            k_new.Tag = KeyNoua
            k_new.Italic = True
            If String.Equals(KeyNoua, k_selectKey, StringComparison.Ordinal) Then k_select = k_new
        End If

        For Each k_b As Bitmap In k_old
            k_b.Dispose()
        Next

        tree.SelectAndReveal(k_select)
        _currentNode = k_select
        ShowNode(k_select)
        tree.Invalidate()
    End Sub

    ' ── Showing one row of the tree ─────────────────────────────────────────────

    Private Sub ShowNode(k_node As AdvancedTreeControl.TreeItem)
        Dim k_grupa As GrupaInfo = TryCast(k_node.Tag, GrupaInfo)
        If k_grupa IsNot Nothing Then
            ShowMode(Mod_.Existenta, k_grupa)
        ElseIf String.Equals(TryCast(k_node.Tag, String), KeyNoua, StringComparison.Ordinal) Then
            ShowMode(Mod_.Noua, Nothing)
        Else
            ShowMode(Mod_.Negrupate, Nothing)
        End If
    End Sub

    Private Sub ShowMode(k_mode As Mod_, k_grupa As GrupaInfo)
        _loading = True
        Try
            _mod = k_mode
            _current = k_grupa
            _bifate.Clear()
            gridAng.Column(ColBifa).Visible = If(k_mode = Mod_.Negrupate, KBotColumnVisibility.Hidden, KBotColumnVisibility.Visible)
            Dim k_group As Boolean = k_mode <> Mod_.Negrupate
            txtDenumire.Enabled = k_group
            btnCuloare.Enabled = k_group

            Select Case k_mode
                Case Mod_.Negrupate
                    txtDenumire.Text = String.Empty
                    SetCuloare(Color.Black)
                    lblAngajamente.Text = "Angajamente negrupate — trageți un rând peste o grupă din arbore"
                Case Mod_.Existenta
                    txtDenumire.Text = k_grupa.Denumire
                    SetCuloare(GrupeUi.HexToColor(k_grupa.Culoare))
                    For Each k_c As String In k_grupa.Coduri
                        _bifate.Add(k_c)
                    Next
                    lblAngajamente.Text = "Angajamentele grupei — debifați un angajament ca să-l scoateți din grupă"
                Case Else
                    txtDenumire.Text = String.Empty
                    SetCuloare(Color.Black)
                    lblAngajamente.Text = "Bifați angajamentele care fac parte din grupă"
            End Select
            FillGrid()
            _dirty = False
            RefreshButtons()
        Finally
            _loading = False
        End Try
    End Sub

    Private Sub FillGrid()
        Dim k_grupate As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        If _mod = Mod_.Negrupate Then
            For Each k_g As GrupaInfo In _catalog.Grupe
                For Each k_c As String In k_g.Coduri
                    k_grupate.Add(k_c)
                Next
            Next
        End If

        gridAng.BeginUpdate()
        Try
            gridAng.ClearRows()
            For Each k_a As GrupaAngajament In _catalog.Angajamente
                Select Case _mod
                    Case Mod_.Negrupate
                        If k_grupate.Contains(k_a.Cod) Then Continue For
                    Case Mod_.Existenta
                        If Not _bifate.Contains(k_a.Cod) Then Continue For
                End Select
                Dim k_row As KBotDataRow = gridAng.AddRow()
                k_row(ColBifa) = _bifate.Contains(k_a.Cod)
                k_row(ColCod) = k_a.Cod
                k_row(ColDenumire) = k_a.Descriere
                k_row(ColIndicatori) = k_a.Indicatori.Count
                k_row(ColAlias) = k_a.AliasAng
            Next
        Finally
            gridAng.EndUpdate()
        End Try
    End Sub

    Private Sub SetCuloare(k_color As Color)
        _culoare = k_color
        btnCuloare.Text = GrupeUi.ColorToHex(k_color)
        btnCuloare.BackColor = k_color
        btnCuloare.ForeColor = If(k_color.GetBrightness() > 0.55F, Color.Black, Color.White)
    End Sub

    ' ── Tree ────────────────────────────────────────────────────────────────────

    Private Sub Tree_NodeMouseUp(k_node As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles tree.NodeMouseUp
        Try
            If k_node Is Nothing OrElse k_node Is _currentNode OrElse _busy Then Return
            ChangeSelection(k_node)
        Catch ex As Exception
            GlobalErrorLog.Write("GrupeForm.Tree_NodeMouseUp", ex)
        End Try
    End Sub

    Private Sub ChangeSelection(k_node As AdvancedTreeControl.TreeItem)
        If Not ConfirmDiscard() Then
            If _currentNode IsNot Nothing Then tree.SelectAndReveal(_currentNode)
            Return
        End If
        If _noua AndAlso Not String.Equals(TryCast(k_node.Tag, String), KeyNoua, StringComparison.Ordinal) Then
            ' Leaving an unsaved new group: its row goes away.
            _noua = False
            BuildTree(k_node.Key)
            Return
        End If
        _currentNode = k_node
        ShowNode(k_node)
    End Sub

    ''' <summary>True = go on (nothing to lose, or the operator agreed to lose it).</summary>
    Private Function ConfirmDiscard() As Boolean
        If Not _dirty Then Return True
        Return KBotMessage.Show(Me, "Există modificări nesalvate în grupă. Renunțați la ele?", "Grupe de angajamente",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button2) = DialogResult.Yes
    End Function

    Private Sub Tree_NodeDragStarting(sender As Object, e As TreeDragStartEventArgs) Handles tree.NodeDragStarting
        ' The tree is only a drop target here: its rows are not dragged.
        e.Cancel = True
    End Sub

    Private Sub Tree_FooterLeftIconClicked(e As MouseEventArgs) Handles tree.FooterLeftIconClicked
        Try
            If _busy Then Return
            If _noua Then
                If _currentNode IsNot Nothing AndAlso Not String.Equals(_currentNode.Key, KeyNoua, StringComparison.Ordinal) Then
                    ChangeSelection(tree.Items.First(Function(k_i) String.Equals(k_i.Key, KeyNoua, StringComparison.Ordinal)))
                End If
                Return
            End If
            If Not ConfirmDiscard() Then Return
            _noua = True
            BuildTree(KeyNoua)
            _dirty = False
            RefreshButtons()
            txtDenumire.Focus()
        Catch ex As Exception
            GlobalErrorLog.Write("GrupeForm.Tree_FooterLeftIconClicked", ex)
        End Try
    End Sub

    ' ── Drag and drop: an ungrouped angajament onto a group ─────────────────────

    Private Sub Tree_ExternalDragOver(sender As Object, e As TreeExternalDragEventArgs) Handles tree.ExternalDragOver
        Try
            If Not e.Data.GetDataPresent(DragFormat) Then Return
            If TypeOf e.Target.Tag Is GrupaInfo Then
                e.Allow = True
            Else
                e.Motiv = "Aruncați angajamentul peste una dintre grupe."
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("GrupeForm.Tree_ExternalDragOver", ex)
        End Try
    End Sub

    Private Sub Tree_ExternalDropped(sender As Object, e As TreeExternalDragEventArgs) Handles tree.ExternalDropped
        Try
            Dim k_grupa As GrupaInfo = TryCast(e.Target.Tag, GrupaInfo)
            Dim k_cod As String = TryCast(e.Data.GetData(DragFormat), String)
            If k_grupa Is Nothing OrElse String.IsNullOrEmpty(k_cod) Then Return
            AdaugaInGrupa(k_grupa, k_cod)
        Catch ex As Exception
            GlobalErrorLog.Write("GrupeForm.Tree_ExternalDropped", ex)
        End Try
    End Sub

    ' UI boundary: logs and shows the error; started without await.
    Private Async Sub AdaugaInGrupa(k_grupa As GrupaInfo, k_cod As String)
        Try
            SetBusy(True, "Se adaugă angajamentul în grupă…")
            Await _gate.RunAsync(Of Object)(Async Function()
                                                Await _api.AdaugaInGrupaAsync(k_grupa.IdGr, k_cod, CancellationToken.None).ConfigureAwait(False)
                                                Return Nothing
                                            End Function).ConfigureAwait(True)
            If IsDisposed Then Return
            _Modificat = True
            k_grupa.Coduri = k_grupa.Coduri.Concat({k_cod}).ToList()
            RaiseEvent TutorialSignal(SignalAngajamentAdaugat)
            ' The row leaves the ungrouped list; the group's count in the tree is refreshed.
            BuildTree(KeyNegrupate)
            SetStatus($"«{k_cod}» a fost adăugat în grupa «{k_grupa.Denumire}».")
        Catch ex As ApiException
            GlobalErrorLog.Write("GrupeForm.AdaugaInGrupa", ex)
            KBotMessage.Show(Me, ex.Message, "Grupe de angajamente", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As Exception
            GlobalErrorLog.Write("GrupeForm.AdaugaInGrupa", ex)
            KBotMessage.Show(Me, "Angajamentul nu a putut fi adăugat în grupă. Detalii în jurnalul de erori.",
                             "Grupe de angajamente", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            If Not IsDisposed Then SetBusy(False, Nothing)
        End Try
    End Sub

    Private Sub GridAng_MouseDown(sender As Object, e As MouseEventArgs) Handles gridAng.MouseDown
        Try
            _dragCod = Nothing
            If _mod <> Mod_.Negrupate OrElse e.Button <> MouseButtons.Left Then Return
            Dim k_row As Integer = gridAng.RowIndexAt(e.Location)
            If k_row < 0 OrElse String.Equals(gridAng.ColumnKeyAt(e.Location), ColAlias, StringComparison.Ordinal) Then Return
            _dragCod = Convert.ToString(gridAng(ColCod, k_row), CultureInfo.InvariantCulture)
            _dragOrigin = e.Location
        Catch ex As Exception
            GlobalErrorLog.Write("GrupeForm.GridAng_MouseDown", ex)
        End Try
    End Sub

    Private Sub GridAng_MouseUp(sender As Object, e As MouseEventArgs) Handles gridAng.MouseUp
        _dragCod = Nothing
    End Sub

    Private Sub GridAng_MouseMove(sender As Object, e As MouseEventArgs) Handles gridAng.MouseMove
        Try
            ' Start of a drag: the pressed row moved far enough.
            If _dragCod IsNot Nothing AndAlso (e.Button And MouseButtons.Left) = MouseButtons.Left Then
                Dim k_limit As Size = SystemInformation.DragSize
                If Math.Abs(e.X - _dragOrigin.X) >= k_limit.Width OrElse Math.Abs(e.Y - _dragOrigin.Y) >= k_limit.Height Then
                    Dim k_cod As String = _dragCod
                    _dragCod = Nothing
                    HideIndicatoriTip()
                    gridAng.DoDragDrop(New DataObject(DragFormat, k_cod), DragDropEffects.Move)
                End If
                Return
            End If
            ShowIndicatoriTip(e.Location)
        Catch ex As Exception
            GlobalErrorLog.Write("GrupeForm.GridAng_MouseMove", ex)
        End Try
    End Sub

    Private Sub GridAng_MouseLeave(sender As Object, e As EventArgs) Handles gridAng.MouseLeave
        HideIndicatoriTip()
    End Sub

    ' ── Tooltip: the indicators of an angajament (and their classifications) ────

    Private Sub ShowIndicatoriTip(k_point As Point)
        If Not String.Equals(gridAng.ColumnKeyAt(k_point), ColIndicatori, StringComparison.Ordinal) Then
            HideIndicatoriTip()
            Return
        End If
        Dim k_row As Integer = gridAng.RowIndexAt(k_point)
        If k_row < 0 Then
            HideIndicatoriTip()
            Return
        End If
        Dim k_cod As String = Convert.ToString(gridAng(ColCod, k_row), CultureInfo.InvariantCulture)
        If String.Equals(k_cod, _tipCod, StringComparison.Ordinal) Then Return
        _tipCod = k_cod

        Dim k_ang As GrupaAngajament = _catalog.Angajamente.FirstOrDefault(
            Function(k_a) String.Equals(k_a.Cod, k_cod, StringComparison.OrdinalIgnoreCase))
        If k_ang Is Nothing Then Return
        Dim k_lines As List(Of String) = k_ang.Indicatori.Select(Function(k_i)
                                                                     Dim k_line As String = If(String.IsNullOrEmpty(k_i.Clsf), "?", k_i.Clsf)
                                                                     If Not String.IsNullOrEmpty(k_i.Denumire) Then k_line &= " — " & k_i.Denumire
                                                                     If Not String.IsNullOrEmpty(k_i.Ss) Then k_line &= " (" & k_i.Ss & ")"
                                                                     Return k_line
                                                                 End Function).ToList()
        If k_lines.Count = 0 Then k_lines.Add("Fără indicatori.")
        Dim k_content As New KBotToolTipContent() With {
            .HeaderText = $"Indicatori — {k_cod}",
            .Text = String.Join(vbLf, k_lines)}
        tips.ShowAt(gridAng, k_content, New Point(System.Windows.Forms.Cursor.Position.X + 14, System.Windows.Forms.Cursor.Position.Y + 20))
    End Sub

    Private Sub HideIndicatoriTip()
        If _tipCod Is Nothing Then Return
        _tipCod = Nothing
        tips.HideNow()
    End Sub

    ' ── Grid edits: tick and alias ──────────────────────────────────────────────

    Private Sub GridAng_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) Handles gridAng.CellValueChanged
        Try
            If _loading Then Return
            Dim k_cod As String = Convert.ToString(gridAng(ColCod, e.RowIndex), CultureInfo.InvariantCulture)
            If String.Equals(e.ColumnKey, ColBifa, StringComparison.Ordinal) Then
                Dim k_on As Boolean = Convert.ToBoolean(e.NewValue, CultureInfo.InvariantCulture)
                If k_on Then _bifate.Add(k_cod) Else _bifate.Remove(k_cod)
                SetDirty(True)
                If Not k_on AndAlso _mod = Mod_.Existenta Then RaiseEvent TutorialSignal(SignalAngajamentScos)
                ' In an existing group an unticked angajament disappears from the list - after the grid
                ' has finished handling this very click.
                If Not k_on AndAlso _mod = Mod_.Existenta Then
                    BeginInvoke(New Action(Sub() RemoveRowOf(k_cod)))
                End If
            ElseIf String.Equals(e.ColumnKey, ColAlias, StringComparison.Ordinal) Then
                SaveAlias(k_cod, Convert.ToString(e.NewValue, CultureInfo.InvariantCulture), Convert.ToString(e.OldValue, CultureInfo.InvariantCulture))
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("GrupeForm.GridAng_CellValueChanged", ex)
        End Try
    End Sub

    Private Sub RemoveRowOf(k_cod As String)
        Try
            For k_i As Integer = 0 To gridAng.RowCount - 1
                If String.Equals(Convert.ToString(gridAng(ColCod, k_i), CultureInfo.InvariantCulture), k_cod, StringComparison.OrdinalIgnoreCase) Then
                    gridAng.RemoveRowAt(k_i)
                    Return
                End If
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("GrupeForm.RemoveRowOf", ex)
        End Try
    End Sub

    ' UI boundary: logs and shows the error; started without await.
    Private Async Sub SaveAlias(k_cod As String, k_new As String, k_old As String)
        Dim k_value As String = If(k_new, String.Empty).Trim()
        Try
            Await _gate.RunAsync(Of Object)(Async Function()
                                                Await _api.SaveAliasAsync(k_cod, k_value, CancellationToken.None).ConfigureAwait(False)
                                                Return Nothing
                                            End Function).ConfigureAwait(True)
            If IsDisposed Then Return
            _Modificat = True
            Dim k_ang As GrupaAngajament = _catalog.Angajamente.FirstOrDefault(
                Function(k_a) String.Equals(k_a.Cod, k_cod, StringComparison.OrdinalIgnoreCase))
            If k_ang IsNot Nothing Then k_ang.AliasAng = k_value
            SetStatus($"Aliasul angajamentului {k_cod} a fost salvat.")
        Catch ex As Exception
            GlobalErrorLog.Write("GrupeForm.SaveAlias", ex)
            If IsDisposed Then Return
            ' The server refused (or could not be reached): the cell goes back to what the server has.
            _loading = True
            Try
                For k_i As Integer = 0 To gridAng.RowCount - 1
                    If String.Equals(Convert.ToString(gridAng(ColCod, k_i), CultureInfo.InvariantCulture), k_cod, StringComparison.OrdinalIgnoreCase) Then
                        gridAng(ColAlias, k_i) = If(k_old, String.Empty)
                        Exit For
                    End If
                Next
            Finally
                _loading = False
            End Try
            KBotMessage.Show(Me, If(TypeOf ex Is ApiException, ex.Message, "Aliasul nu a putut fi salvat. Detalii în jurnalul de erori."),
                             "Grupe de angajamente", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    ' ── Name and colour ─────────────────────────────────────────────────────────

    Private Sub TxtDenumire_TextChanged(sender As Object, e As EventArgs) Handles txtDenumire.TextChanged
        If Not _loading Then SetDirty(True)
    End Sub

    Private Sub BtnCuloare_Click(sender As Object, e As EventArgs) Handles btnCuloare.Click
        Try
            Using k_dlg As New ColorDialog() With {.Color = _culoare, .FullOpen = True, .AnyColor = True}
                If k_dlg.ShowDialog(Me) <> DialogResult.OK Then Return
                SetCuloare(k_dlg.Color)
                SetDirty(True)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("GrupeForm.BtnCuloare_Click", ex)
        End Try
    End Sub

    ' ── Save ────────────────────────────────────────────────────────────────────

    Private Sub BtnSalveaza_Click(sender As Object, e As EventArgs) Handles btnSalveaza.Click
        SaveGrupa()
    End Sub

    ' UI boundary: logs and shows the error; started without await.
    Private Async Sub SaveGrupa()
        Try
            If _mod = Mod_.Negrupate OrElse _busy Then Return
            Dim k_name As String = txtDenumire.Text.Trim()
            If k_name.Length = 0 Then
                KBotMessage.Show(Me, "Introduceți denumirea grupei.", "Grupe de angajamente", MessageBoxButtons.OK, MessageBoxIcon.Information)
                txtDenumire.Focus()
                Return
            End If
            If _bifate.Count = 0 Then
                KBotMessage.Show(Me, "Bifați cel puțin un angajament în grupă.", "Grupe de angajamente", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            ' Members the window does not list (hidden / cancelled angajamente) stay in the group.
            Dim k_coduri As New List(Of String)(_bifate)
            If _mod = Mod_.Existenta Then
                Dim k_vizibile As New HashSet(Of String)(_catalog.Angajamente.Select(Function(k_a) k_a.Cod), StringComparer.OrdinalIgnoreCase)
                k_coduri.AddRange(_current.Coduri.Where(Function(k_c) Not k_vizibile.Contains(k_c)))
            End If
            Dim k_idgr As Integer? = If(_mod = Mod_.Existenta, CType(_current.IdGr, Integer?), Nothing)
            Dim k_hex As String = GrupeUi.ColorToHex(_culoare)

            SetBusy(True, "Se salvează grupa…")
            Dim k_saved As Integer = Await _gate.RunAsync(
                Function() _api.SaveGrupaAsync(k_idgr, k_name, k_hex, k_coduri, CancellationToken.None)).ConfigureAwait(True)
            If IsDisposed Then Return
            _Modificat = True
            _noua = False
            _dirty = False
            LoadCatalog($"G{k_saved}")
            SetStatus($"Grupa «{k_name}» a fost salvată.")
        Catch ex As ApiException
            GlobalErrorLog.Write("GrupeForm.SaveGrupa", ex)
            KBotMessage.Show(Me, ex.Message, "Grupe de angajamente", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As Exception
            GlobalErrorLog.Write("GrupeForm.SaveGrupa", ex)
            KBotMessage.Show(Me, "Grupa nu a putut fi salvată. Detalii în jurnalul de erori.",
                             "Grupe de angajamente", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            If Not IsDisposed Then SetBusy(False, Nothing)
        End Try
    End Sub

    ' ── Exit ────────────────────────────────────────────────────────────────────

    Private Sub BtnIesire_Click(sender As Object, e As EventArgs) Handles btnIesire.Click
        Close()
    End Sub

    Private Sub GrupeForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        Try
            If e.CloseReason = CloseReason.UserClosing AndAlso Not ConfirmDiscard() Then e.Cancel = True
        Catch ex As Exception
            GlobalErrorLog.Write("GrupeForm.GrupeForm_FormClosing", ex)
        End Try
    End Sub

    Private Sub GrupeForm_FormClosed(sender As Object, e As FormClosedEventArgs) Handles Me.FormClosed
        Try
            For Each k_b As Bitmap In _swatches
                k_b.Dispose()
            Next
            _swatches.Clear()
        Catch ex As Exception
            GlobalErrorLog.Write("GrupeForm.GrupeForm_FormClosed", ex)
        End Try
    End Sub

    ' ── State of the buttons ────────────────────────────────────────────────────

    Private Sub SetDirty(k_value As Boolean)
        _dirty = k_value
        RefreshButtons()
    End Sub

    Private Sub SetBusy(k_busy As Boolean, k_status As String)
        _busy = k_busy
        UseWaitCursor = k_busy
        RefreshButtons()
        If k_status IsNot Nothing Then SetStatus(k_status)
    End Sub

    Private Sub RefreshButtons()
        btnSalveaza.Enabled = _mod <> Mod_.Negrupate AndAlso Not _busy
        tree.Enabled = Not _busy
    End Sub

    Private Sub SetStatus(k_text As String)
        lblStare.Text = If(k_text, String.Empty)
    End Sub

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim k_scheme As ThemeScheme = ThemeManager.Current
            Dim k_p As ThemePalette = k_scheme.Palette
            ' The form background IS the 1px outline of the window (Padding(1)).
            BackColor = k_p.BorderColor
            For Each k_c As Control In {tlyMain, pnlCard, tlyBody, tlyDetalii, tlySubsol}
                k_c.BackColor = k_p.SurfaceAltColor
            Next
            lblAngajamente.ForeColor = k_p.TextColor
            lblStare.ForeColor = k_p.TextDimColor
            ButtonStyles.ApplyPrimary(btnSalveaza, k_scheme)
            ButtonStyles.ApplySecondary(btnIesire, k_scheme)
            SetCuloare(_culoare)
        Catch ex As Exception
            GlobalErrorLog.Write("GrupeForm.OnThemeChanged", ex)
        End Try
    End Sub

End Class
