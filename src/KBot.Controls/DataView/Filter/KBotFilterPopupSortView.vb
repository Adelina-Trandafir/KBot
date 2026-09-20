Option Strict On
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' The «Sortare» tab of the column menu: ascending / descending, plus «reseteaza sortarea».
''' Sorting is a COMMAND, not a choice to confirm -- the popup applies it at once and closes,
''' exactly as Access does -- so this tab has no state of its own beyond the direction the
''' column is already sorted on (marked with a tick).
'''
''' <para>The row captions depend on the column type («A → Z» on text, «de la mic la mare» on
''' numbers) and come from <see cref="KBotFilterEngine.SortCaption"/> at construction; the
''' controls themselves are the designer's (<c>KBotFilterPopupSortView.Designer.vb</c>).</para>
''' </summary>
Friend Class KBotFilterPopupSortView
    Implements IKBotFilterMenuView, IThemedContainer

    Private ReadOnly _currentSort As KBotSortDirection

    ''' <summary>The operator clicked one of the three rows.</summary>
    Friend Event SortRequested(direction As KBotSortDirection)

    Friend Sub New(valueType As KBotValueType, currentSort As KBotSortDirection)
        InitializeComponent()
        _currentSort = currentSort

        btnSortAsc.Text = KBotFilterEngine.SortCaption(valueType, KBotSortDirection.Ascending) &
                          SortMark(KBotSortDirection.Ascending)
        btnSortDesc.Text = KBotFilterEngine.SortCaption(valueType, KBotSortDirection.Descending) &
                           SortMark(KBotSortDirection.Descending)
        btnSortClear.Enabled = _currentSort <> KBotSortDirection.None
    End Sub

    Public ReadOnly Property ViewKey As String Implements IKBotFilterMenuView.ViewKey
        Get
            Return "sortare"
        End Get
    End Property

    Public ReadOnly Property ShowsCommandBar As Boolean Implements IKBotFilterMenuView.ShowsCommandBar
        Get
            Return False
        End Get
    End Property

    Public Sub Activated() Implements IKBotFilterMenuView.Activated
        ' Nothing to focus: three rows, any of them closes the menu.
    End Sub

    Public Function RequiredHeight() As Integer Implements IKBotFilterMenuView.RequiredHeight
        ' The fixed rows take the scheme's measure first (its padding and font); measuring
        ' before that would size the window on rows that change right after.
        tlySort.RefitToTheme()
        PerformLayout()
        Return KBotFilterMenuLayout.FixedRowsHeight(tlySort)
    End Function

    ' The active direction is marked, so the operator sees what the column is already sorted on.
    Private Function SortMark(direction As KBotSortDirection) As String
        Return If(_currentSort = direction, "   ✓", String.Empty)
    End Function

    ''' <summary>
    ''' The semantic colours the generic per-type rules cannot know: the surface the rows sit
    ''' on, the separator (a Panel, because the Label rule would make a 1px line transparent)
    ''' and the menu rows themselves. The rest comes from <c>ThemeManager.Apply</c>.
    ''' </summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            BackColor = p.SurfaceAltColor
            tlySort.BackColor = p.SurfaceAltColor
            sepSort.BackColor = p.BorderColor
            KBotFilterMenuLayout.ApplyMenuRow(btnSortAsc, p, p.TextColor)
            KBotFilterMenuLayout.ApplyMenuRow(btnSortDesc, p, p.TextColor)
            KBotFilterMenuLayout.ApplyMenuRow(btnSortClear, p, p.ErrorColor)
        Catch ex As Exception
            ' Theme boundary: log and swallow -- a throw here would break the scheme switch.
            GlobalErrorLog.Write("KBotFilterPopupSortView.ApplyTheme", ex)
        End Try
    End Sub

    Private Sub BtnSortAsc_Click(sender As Object, e As EventArgs) Handles btnSortAsc.Click
        RaiseEvent SortRequested(KBotSortDirection.Ascending)
    End Sub

    Private Sub BtnSortDesc_Click(sender As Object, e As EventArgs) Handles btnSortDesc.Click
        RaiseEvent SortRequested(KBotSortDirection.Descending)
    End Sub

    Private Sub BtnSortClear_Click(sender As Object, e As EventArgs) Handles btnSortClear.Click
        RaiseEvent SortRequested(KBotSortDirection.None)
    End Sub

End Class
