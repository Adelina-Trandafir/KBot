Option Strict On
Imports System.Collections.Generic
Imports System.Linq
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' «Pagina FOREXE» (operator, 21.09.2026): the CSS rules K-BOT writes into every FOREXE page
''' (<see cref="AppSettings.ForexePageStyles"/>). The list on the left is read-only but for
''' the «Activ» tick; the selected rule is edited in the four fields on the right
''' (<see cref="RegulaPaginaEditor"/>), which write into it as the operator types. «Regulă
''' nouă…» opens <see cref="RegulaPaginaForm"/> - the tree of the open page plus the same
''' three fields. Nothing leaves this page until «Salvează și aplică», which writes the
''' settings and pushes them into the page that is open right now, if any.
'''
''' <para>The grid is a VIEW of <c>_rules</c>: each row's Tag is its rule; the editor's
''' changes are painted back into the row; add / delete rebuild the rows from the list
''' (the grid has no row removal of its own).</para>
''' </summary>
Public Class SetariPaginaView
    Implements ISetariView, IThemedContainer

    Private Const COL_ACTIV As String = "activ"
    Private Const COL_NOTA As String = "nota"
    Private Const COL_SELECTOR As String = "selector"
    Private Const COL_PAGINA As String = "pagina"

    Private ReadOnly _controller As ForexeController
    Private _rules As New List(Of PageStyleRule)()
    Private _dirty As Boolean
    Private _filling As Boolean

    Public Event StatusChanged(text As String) Implements ISetariView.StatusChanged
    Public Event BusyChanged(busy As Boolean) Implements ISetariView.BusyChanged

    Public Sub New(controller As ForexeController)
        ArgumentNullException.ThrowIfNull(controller)
        InitializeComponent()
        _controller = controller
        editor.Hint = String.Empty   ' the page's own hint says it; the panel keeps its height
    End Sub

    Public ReadOnly Property ViewKey As String Implements ISetariView.ViewKey
        Get
            Return "pagina"
        End Get
    End Property

    ''' <summary>Unsaved rows are asked about, not dropped in silence.</summary>
    Public Function CanClose() As Boolean Implements ISetariView.CanClose
        Try
            If Not _dirty Then Return True
            Return KBotMessage.Show(FindForm(),
                "Regulile paginii au modificări nesalvate. Închizi fără să le salvezi?",
                "Pagina FOREXE", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes
        Catch ex As Exception
            GlobalErrorLog.Write("SetariPaginaView.CanClose", ex)
            Return True
        End Try
    End Function

    Public Sub Activated() Implements ISetariView.Activated
        Try
            If _dirty Then Return   ' the operator is mid-edit; do not reload under them
            _rules = PageStyleRule.CloneList(AppSettings.Current.ForexePageStyles)
            FillGrid()
            If grila.RowCount > 0 Then grila.CurrentRowIndex = 0
            BindEditor()
            UpdateButtons()
        Catch ex As Exception
            GlobalErrorLog.Write("SetariPaginaView.Activated", ex)
        End Try
    End Sub

    ' ---------------- grid <-> list ----------------

    Private Sub FillGrid()
        _filling = True
        grila.BeginUpdate()
        Try
            grila.ClearRows()
            For Each r As PageStyleRule In _rules
                Dim row As KBotDataRow = grila.AddRow()
                row.Tag = r
                PaintRow(row, r)
            Next
        Finally
            grila.EndUpdate()
            _filling = False
        End Try
    End Sub

    Private Shared Sub PaintRow(row As KBotDataRow, r As PageStyleRule)
        row(COL_ACTIV) = r.Enabled
        row(COL_NOTA) = r.Note
        row(COL_SELECTOR) = r.Selector
        row(COL_PAGINA) = If(String.IsNullOrWhiteSpace(r.Page), "(toate)", r.Page)
    End Sub

    Private Function CurrentRule() As PageStyleRule
        Dim i As Integer = grila.CurrentRowIndex
        If i < 0 OrElse i >= grila.RowCount Then Return Nothing
        Return TryCast(grila.Rows(i).Tag, PageStyleRule)
    End Function

    ' The right panel follows the selected row.
    Private Sub BindEditor()
        editor.Rule = CurrentRule()
    End Sub

    ' Only the «Activ» tick is edited in the grid itself; the text columns are read-only.
    Private Sub Grila_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) Handles grila.CellValueChanged
        Try
            If _filling OrElse e Is Nothing Then Return
            If e.RowIndex < 0 OrElse e.RowIndex >= grila.RowCount Then Return
            Dim r As PageStyleRule = TryCast(grila.Rows(e.RowIndex).Tag, PageStyleRule)
            If r Is Nothing OrElse e.ColumnKey <> COL_ACTIV Then Return
            r.Enabled = TypeOf e.NewValue Is Boolean AndAlso CBool(e.NewValue)
            MarkDirty()
        Catch ex As Exception
            GlobalErrorLog.Write("SetariPaginaView.Grila_CellValueChanged", ex)
        End Try
    End Sub

    Private Sub Grila_SelectionChanged(sender As Object, e As EventArgs) Handles grila.SelectionChanged
        Try
            BindEditor()
            UpdateButtons()
        Catch ex As Exception
            GlobalErrorLog.Write("SetariPaginaView.Grila_SelectionChanged", ex)
        End Try
    End Sub

    ' The editor wrote into the rule: the row shows it at once.
    Private Sub Editor_RuleChanged(rule As PageStyleRule) Handles editor.RuleChanged
        Try
            If rule Is Nothing Then Return
            For i As Integer = 0 To grila.RowCount - 1
                If grila.Rows(i).Tag Is rule Then
                    PaintRow(grila.Rows(i), rule)
                    grila.InvalidateRow(i)
                    Exit For
                End If
            Next
            MarkDirty()
        Catch ex As Exception
            GlobalErrorLog.Write("SetariPaginaView.Editor_RuleChanged", ex)
        End Try
    End Sub

    Private Sub MarkDirty()
        _dirty = True
        UpdateButtons()
    End Sub

    Private Sub UpdateButtons()
        btnSterge.Enabled = CurrentRule() IsNot Nothing
        btnSalveaza.Enabled = _dirty
    End Sub

    ' ---------------- buttons ----------------

    ' «Regula noua…»: the window with the tree of the page; the rule it returns goes at the end.
    Private Sub BtnAdauga_Click(sender As Object, e As EventArgs) Handles btnAdauga.Click
        Try
            Using dlg As New RegulaPaginaForm(_controller, New PageStyleRule("", "", ""))
                If dlg.ShowDialog(FindForm()) <> DialogResult.OK Then Return
                AddRule(dlg.Rule)
                RaiseEvent StatusChanged($"Regulă nouă pentru «{dlg.Rule.Selector}». Se pune în pagină la «Salvează și aplică».")
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("SetariPaginaView.BtnAdauga_Click", ex)
            RaiseEvent StatusChanged("Fereastra regulii noi nu s-a putut deschide: " & ex.Message)
        End Try
    End Sub

    Private Sub AddRule(r As PageStyleRule)
        _rules.Add(r)
        FillGrid()
        grila.CurrentRowIndex = grila.RowCount - 1
        grila.EnsureVisible(grila.RowCount - 1)
        BindEditor()
        MarkDirty()
    End Sub

    Private Sub BtnSterge_Click(sender As Object, e As EventArgs) Handles btnSterge.Click
        Try
            Dim i As Integer = grila.CurrentRowIndex
            If i < 0 OrElse i >= _rules.Count Then Return
            _rules.RemoveAt(i)
            FillGrid()
            If grila.RowCount > 0 Then grila.CurrentRowIndex = Math.Min(i, grila.RowCount - 1)
            BindEditor()
            MarkDirty()
        Catch ex As Exception
            GlobalErrorLog.Write("SetariPaginaView.BtnSterge_Click", ex)
        End Try
    End Sub

    Private Sub BtnImplicite_Click(sender As Object, e As EventArgs) Handles btnImplicite.Click
        Try
            If KBotMessage.Show(FindForm(),
                    "Lista de acum se înlocuiește cu regulile K-BOT de la început. Continui?",
                    "Regulile implicite", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return
            _rules = PageStyleRule.Defaults()
            FillGrid()
            If grila.RowCount > 0 Then grila.CurrentRowIndex = 0
            BindEditor()
            MarkDirty()
            RaiseEvent StatusChanged("Regulile implicite sunt în listă; «Salvează și aplică» le pune în pagină.")
        Catch ex As Exception
            GlobalErrorLog.Write("SetariPaginaView.BtnImplicite_Click", ex)
        End Try
    End Sub

    Private Async Sub BtnSalveaza_Click(sender As Object, e As EventArgs) Handles btnSalveaza.Click
        Try
            ' A rule with no selector cannot go into a stylesheet; say it instead of skipping.
            Dim blank As Integer = _rules.Where(Function(r) r.Enabled AndAlso String.IsNullOrWhiteSpace(r.Selector)).Count()
            If blank > 0 Then
                KBotMessage.Show(FindForm(),
                    $"{blank} rând(uri) active nu au selector. Completează-l sau debifează rândul.",
                    "Pagina FOREXE", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If
            Dim copy As AppSettings = AppSettings.Current.Clone()
            copy.ForexePageStyles = PageStyleRule.CloneList(_rules)
            copy.Save()
            _dirty = False
            UpdateButtons()
            RaiseEvent BusyChanged(True)
            Dim inPage As Boolean
            Try
                inPage = Await _controller.AplicaSetarilePaginiiAsync()
            Finally
                RaiseEvent BusyChanged(False)
            End Try
            RaiseEvent StatusChanged(If(inPage,
                                        "Regulile sunt salvate și puse în pagina FOREXE deschisă.",
                                        "Regulile sunt salvate; intră în pagină la următoarea deschidere a browserului."))
        Catch ex As Exception
            GlobalErrorLog.Write("SetariPaginaView.BtnSalveaza_Click", ex)
            RaiseEvent StatusChanged("Regulile nu au putut fi salvate: " & ex.Message)
        End Try
    End Sub

    ' ---------------- theme ----------------

    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            BackColor = p.SurfaceAltColor
            tlyBody.BackColor = p.SurfaceAltColor
            tlyMijloc.BackColor = p.SurfaceAltColor
            tlyButoane.BackColor = p.SurfaceAltColor
            lblTitlu.ForeColor = p.TextColor
            lblTitlu.BackColor = Color.Transparent
            lblHint.ForeColor = p.TextDimColor
            lblHint.BackColor = Color.Transparent
            ButtonStyles.ApplySecondary(btnAdauga, scheme)
            ButtonStyles.ApplySecondary(btnSterge, scheme)
            ButtonStyles.ApplySecondary(btnImplicite, scheme)
            ButtonStyles.ApplyPrimary(btnSalveaza, scheme)
            grila.ApplyTheme(scheme)
            editor.ApplyTheme(scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("SetariPaginaView.ApplyTheme", ex)
        End Try
    End Sub

End Class
