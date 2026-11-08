Option Strict On
Imports System.Collections.Generic
Imports System.Linq
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' «Extrase» (slice 0080-02, operator 24.09.2026): the columns of the four statement grids
''' (<see cref="ExtraseGrid"/>) -- which are shown and in what order, separately for the Extrase
''' view and the «Extrase de cont» window. The four lists are edited here side by side (the
''' combo switches between them) and written together by «Salvează»; the open view and window
''' follow at once through <see cref="AppSettings.Changed"/>.
''' </summary>
Public Class SetariExtraseView
    Implements ISetariView, IThemedContainer

    Private Const COL_AFISATA As String = "afisata"
    Private Const COL_COLOANA As String = "coloana"

    ''' <summary>One line of the list: a catalogue column and whether it is shown. POCO.</summary>
    Private NotInheritable Class Linie
        Public Property Key As String
        Public Property Caption As String
        Public Property Afisata As Boolean
    End Class

    ''' <summary>A combo entry: the grid and its caption. POCO.</summary>
    Private NotInheritable Class GrilaItem
        Public ReadOnly Property Grid As ExtraseGrid
        Private ReadOnly _text As String
        Public Sub New(grid As ExtraseGrid, text As String)
            Me.Grid = grid
            _text = text
        End Sub
        Public Overrides Function ToString() As String
            Return _text
        End Function
    End Class

    Private ReadOnly _liste As New Dictionary(Of ExtraseGrid, List(Of Linie))()
    Private _dirty As Boolean
    Private _filling As Boolean

    Public Event StatusChanged(text As String) Implements ISetariView.StatusChanged
    Public Event BusyChanged(busy As Boolean) Implements ISetariView.BusyChanged

    Public Sub New()
        InitializeComponent()
        cboGrila.Items.Add(New GrilaItem(ExtraseGrid.ViewHeaders, "Vederea Extrase — antetele (conturile extrasului)"))
        cboGrila.Items.Add(New GrilaItem(ExtraseGrid.ViewOperations, "Vederea Extrase — operațiunile"))
        cboGrila.Items.Add(New GrilaItem(ExtraseGrid.WindowHeaders, "Fereastra «Extrase de cont» — antetele"))
        cboGrila.Items.Add(New GrilaItem(ExtraseGrid.WindowOperations, "Fereastra «Extrase de cont» — operațiunile"))
    End Sub

    Public ReadOnly Property ViewKey As String Implements ISetariView.ViewKey
        Get
            Return "extrase"
        End Get
    End Property

    Public Function CanClose() As Boolean Implements ISetariView.CanClose
        Try
            If Not _dirty Then Return True
            Return KBotMessage.Show(FindForm(),
                "Coloanele extraselor au modificări nesalvate. Închizi fără să le salvezi?",
                "Extrase", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes
        Catch ex As Exception
            GlobalErrorLog.Write("SetariExtraseView.CanClose", ex)
            Return True
        End Try
    End Function

    Public Sub Activated() Implements ISetariView.Activated
        Try
            If _dirty Then Return   ' mid-edit: do not reload under the operator
            Dim s As AppSettings = AppSettings.Current
            For Each g As ExtraseGrid In [Enum].GetValues(GetType(ExtraseGrid))
                _liste(g) = BuildList(g, s.ExtraseColumnsFor(g))
            Next
            If cboGrila.SelectedIndex < 0 Then
                cboGrila.SelectedIndex = 0      ' raises SelectedIndexChanged -> FillGrid
            Else
                FillGrid()
            End If
            UpdateButtons()
        Catch ex As Exception
            GlobalErrorLog.Write("SetariExtraseView.Activated", ex)
        End Try
    End Sub

    ' Chosen keys first, in their order; the rest of the catalogue after them, unticked.
    Private Shared Function BuildList(grid As ExtraseGrid, chosen As List(Of String)) As List(Of Linie)
        Dim catalog As IReadOnlyList(Of ExtraseColumnInfo) = ExtraseColumns.Catalog(grid)
        Dim byKey As Dictionary(Of String, ExtraseColumnInfo) = catalog.ToDictionary(Function(c) c.Key, StringComparer.Ordinal)
        Dim result As New List(Of Linie)()
        For Each k As String In chosen
            result.Add(New Linie With {.Key = k, .Caption = byKey(k).Caption, .Afisata = True})
        Next
        For Each c As ExtraseColumnInfo In catalog
            If Not chosen.Contains(c.Key) Then
                result.Add(New Linie With {.Key = c.Key, .Caption = c.Caption, .Afisata = False})
            End If
        Next
        Return result
    End Function

    Private ReadOnly Property CurrentGrid As ExtraseGrid
        Get
            Dim item As GrilaItem = TryCast(cboGrila.SelectedItem, GrilaItem)
            Return If(item Is Nothing, ExtraseGrid.ViewHeaders, item.Grid)
        End Get
    End Property

    Private Function CurrentList() As List(Of Linie)
        Dim list As List(Of Linie) = Nothing
        If _liste.TryGetValue(CurrentGrid, list) Then Return list
        Return New List(Of Linie)()
    End Function

    Private Sub FillGrid(Optional selectIndex As Integer = -1)
        _filling = True
        grila.BeginUpdate()
        Try
            grila.ClearRows()
            For Each l As Linie In CurrentList()
                Dim row As KBotDataRow = grila.AddRow()
                row.Tag = l
                row(COL_AFISATA) = l.Afisata
                row(COL_COLOANA) = l.Caption
            Next
        Finally
            grila.EndUpdate()
            _filling = False
        End Try
        If selectIndex >= 0 AndAlso selectIndex < grila.RowCount Then
            grila.CurrentRowIndex = selectIndex
            grila.EnsureVisible(selectIndex)
        End If
        UpdateButtons()
    End Sub

    Private Sub CboGrila_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboGrila.SelectedIndexChanged
        Try
            FillGrid(0)
        Catch ex As Exception
            GlobalErrorLog.Write("SetariExtraseView.CboGrila_SelectedIndexChanged", ex)
        End Try
    End Sub

    Private Sub Grila_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) Handles grila.CellValueChanged
        Try
            If _filling OrElse e Is Nothing OrElse e.ColumnKey <> COL_AFISATA Then Return
            If e.RowIndex < 0 OrElse e.RowIndex >= grila.RowCount Then Return
            Dim l As Linie = TryCast(grila.Rows(e.RowIndex).Tag, Linie)
            If l Is Nothing Then Return
            l.Afisata = TypeOf e.NewValue Is Boolean AndAlso CBool(e.NewValue)
            MarkDirty()
        Catch ex As Exception
            GlobalErrorLog.Write("SetariExtraseView.Grila_CellValueChanged", ex)
        End Try
    End Sub

    Private Sub Grila_SelectionChanged(sender As Object, e As EventArgs) Handles grila.SelectionChanged
        Try
            UpdateButtons()
        Catch ex As Exception
            GlobalErrorLog.Write("SetariExtraseView.Grila_SelectionChanged", ex)
        End Try
    End Sub

    Private Sub MarkDirty()
        _dirty = True
        UpdateButtons()
    End Sub

    Private Sub UpdateButtons()
        Dim i As Integer = grila.CurrentRowIndex
        btnSus.Enabled = i > 0
        btnJos.Enabled = i >= 0 AndAlso i < grila.RowCount - 1
        btnSalveaza.Enabled = _dirty
    End Sub

    Private Sub Muta(delta As Integer)
        Dim list As List(Of Linie) = CurrentList()
        Dim i As Integer = grila.CurrentRowIndex
        Dim j As Integer = i + delta
        If i < 0 OrElse j < 0 OrElse j >= list.Count Then Return
        Dim l As Linie = list(i)
        list.RemoveAt(i)
        list.Insert(j, l)
        FillGrid(j)
        MarkDirty()
    End Sub

    Private Sub BtnSus_Click(sender As Object, e As EventArgs) Handles btnSus.Click
        Try
            Muta(-1)
        Catch ex As Exception
            GlobalErrorLog.Write("SetariExtraseView.BtnSus_Click", ex)
        End Try
    End Sub

    Private Sub BtnJos_Click(sender As Object, e As EventArgs) Handles btnJos.Click
        Try
            Muta(1)
        Catch ex As Exception
            GlobalErrorLog.Write("SetariExtraseView.BtnJos_Click", ex)
        End Try
    End Sub

    Private Sub BtnImplicite_Click(sender As Object, e As EventArgs) Handles btnImplicite.Click
        Try
            Dim g As ExtraseGrid = CurrentGrid
            _liste(g) = BuildList(g, ExtraseColumns.Defaults(g))
            FillGrid(0)
            MarkDirty()
            RaiseEvent StatusChanged("Coloanele implicite sunt în listă; «Salvează» le aplică.")
        Catch ex As Exception
            GlobalErrorLog.Write("SetariExtraseView.BtnImplicite_Click", ex)
        End Try
    End Sub

    Private Sub BtnSalveaza_Click(sender As Object, e As EventArgs) Handles btnSalveaza.Click
        Try
            ' A grid with no column cannot be what anyone chose: say which one, save nothing.
            For i As Integer = 0 To cboGrila.Items.Count - 1
                Dim item As GrilaItem = DirectCast(cboGrila.Items(i), GrilaItem)
                If Not _liste(item.Grid).Any(Function(l) l.Afisata) Then
                    cboGrila.SelectedIndex = i
                    KBotMessage.Show(FindForm(),
                        $"Grila «{item}» nu are nicio coloană bifată. Bifați cel puțin una.",
                        "Extrase", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    Return
                End If
            Next
            Dim copy As AppSettings = AppSettings.Current.Clone()
            For Each kv In _liste
                copy.SetExtraseColumns(kv.Key, kv.Value.Where(Function(l) l.Afisata).Select(Function(l) l.Key))
            Next
            copy.Save()
            _dirty = False
            UpdateButtons()
            RaiseEvent StatusChanged("Coloanele extraselor sunt salvate și aplicate.")
        Catch ex As Exception
            GlobalErrorLog.Write("SetariExtraseView.BtnSalveaza_Click", ex)
            RaiseEvent StatusChanged("Coloanele nu au putut fi salvate: " & ex.Message)
        End Try
    End Sub

    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            BackColor = p.SurfaceAltColor
            tlyBody.BackColor = p.SurfaceAltColor
            tlyGrila.BackColor = p.SurfaceAltColor
            tlyButoane.BackColor = p.SurfaceAltColor
            lblTitlu.ForeColor = p.TextColor
            lblTitlu.BackColor = Color.Transparent
            lblHint.ForeColor = p.TextDimColor
            lblHint.BackColor = Color.Transparent
            lblGrila.ForeColor = p.TextColor
            lblGrila.BackColor = Color.Transparent
            cboGrila.ApplyTheme(scheme)
            ButtonStyles.ApplySecondary(btnSus, scheme)
            ButtonStyles.ApplySecondary(btnJos, scheme)
            ButtonStyles.ApplySecondary(btnImplicite, scheme)
            ButtonStyles.ApplyPrimary(btnSalveaza, scheme)
            grila.ApplyTheme(scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("SetariExtraseView.ApplyTheme", ex)
        End Try
    End Sub

End Class
