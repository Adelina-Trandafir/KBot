Option Strict On
Imports System.Globalization

''' <summary>
''' The rights of ONE application (ADECHIT or VERCON) for ONE account in ONE database, the roles one under the other:
''' «Explicație» and a check box «Bifă». A tick gives the right on the server at once and a cleared tick takes it away;
''' a refusal puts the box back. A click on the title of «Bifă» ticks every role (or, when all are ticked, clears them
''' all) with ONE call to the server. The server work is done by the caller (<c>k_apply</c>); this form only shows the
''' state and asks (ME01).
''' </summary>
Public Class RightsForm

    Private ReadOnly _link As AccountLink
    Private ReadOnly _apply As Func(Of AccountLink, IReadOnlyList(Of String), Boolean, Task(Of Boolean))
    Private _filling As Boolean
    Private _busy As Boolean

    ''' <param name="k_application">The application's name as the operator knows it (ADECHIT, VERCON).</param>
    ''' <param name="k_link">The account in one database; its <c>Roles</c> hold what it has now and are updated by the caller.</param>
    ''' <param name="k_roles">The roles of the application, in the order to show them.</param>
    ''' <param name="k_apply">Gives (True) or takes away (False) the listed role codes; answers whether the server accepted.</param>
    Public Sub New(k_application As String, k_link As AccountLink, k_roles As IReadOnlyList(Of RoleInfo),
                   k_apply As Func(Of AccountLink, IReadOnlyList(Of String), Boolean, Task(Of Boolean)))
        InitializeComponent()
        Icon = AdminIcon.Value
        _link = k_link
        _apply = k_apply
        Text = "Drepturi " & k_application
        lblInfo.Text = k_link.Un & "  ·  baza " & k_link.Dc & vbLf & "Bifa dă dreptul pe loc. Click pe titlul coloanei «Bifă»: bifează / debifează tot."
        _filling = True
        grid.BeginUpdate()
        Try
            For Each k_role As RoleInfo In k_roles
                Dim k_row As KBotDataRow = grid.AddRow()
                k_row("expl") = k_role.Name
                k_row("bifa") = k_link.Roles.Contains(k_role.Cod)
                k_row.Tag = k_role.Cod
            Next
        Finally
            grid.EndUpdate()
            _filling = False
        End Try
    End Sub

    Private Function CodeAt(k_index As Integer) As String
        Return Convert.ToString(grid.Rows(k_index).Tag, CultureInfo.InvariantCulture)
    End Function

    Private Sub SetBox(k_index As Integer, k_value As Boolean)
        _filling = True
        Try
            grid("bifa", k_index) = k_value
        Finally
            _filling = False
        End Try
    End Sub

    ''' <summary>One box ticked or cleared by the operator.</summary>
    Private Async Sub Grid_CellValueChanged(k_sender As Object, e As KBotCellValueEventArgs) Handles grid.CellValueChanged
        Try
            If _filling OrElse _busy OrElse e.ColumnKey <> "bifa" Then Return
            Dim k_on As Boolean = Convert.ToBoolean(e.NewValue, CultureInfo.InvariantCulture)
            Dim k_index As Integer = e.RowIndex
            _busy = True
            Try
                Dim k_ok As Boolean = Await _apply(_link, New List(Of String) From {CodeAt(k_index)}, k_on)
                If Not k_ok Then SetBox(k_index, Not k_on)
            Finally
                _busy = False
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("RightsForm.Grid_CellValueChanged", ex)
        End Try
    End Sub

    ''' <summary>A click on the title of «Bifă»: tick everything, or clear everything when everything is already ticked.</summary>
    Private Async Sub Grid_HeaderClick(k_sender As Object, e As KBotCellEventArgs) Handles grid.HeaderClick
        Try
            If _busy OrElse e.ColumnKey <> "bifa" OrElse grid.RowCount = 0 Then Return
            Dim k_all As Boolean = True
            For k_i As Integer = 0 To grid.RowCount - 1
                If Not Convert.ToBoolean(grid.Rows(k_i)("bifa"), CultureInfo.InvariantCulture) Then
                    k_all = False
                    Exit For
                End If
            Next
            Dim k_target As Boolean = Not k_all
            Dim k_changes As New List(Of Integer)()
            For k_i As Integer = 0 To grid.RowCount - 1
                If Convert.ToBoolean(grid.Rows(k_i)("bifa"), CultureInfo.InvariantCulture) <> k_target Then k_changes.Add(k_i)
            Next
            If k_changes.Count = 0 Then Return
            _busy = True
            Try
                Dim k_ok As Boolean = Await _apply(_link, k_changes.Select(Function(k_i) CodeAt(k_i)).ToList(), k_target)
                If k_ok Then
                    For Each k_i As Integer In k_changes
                        SetBox(k_i, k_target)
                    Next
                    grid.Invalidate()
                End If
            Finally
                _busy = False
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("RightsForm.Grid_HeaderClick", ex)
        End Try
    End Sub

End Class
