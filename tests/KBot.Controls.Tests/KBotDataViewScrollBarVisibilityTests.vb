Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Theming
Imports Xunit

''' <summary>
''' The bars' shown state is the grid's own, not <c>Control.Visible</c>. <c>Visible</c> reports
''' the EFFECTIVE visibility (False under any hidden ancestor), so a relayout that ran while the
''' grid sat on a hidden page used to read "bar already hidden", skip the write, and the stale
''' bar came back with the page -- until a resize ran the same code while visible.
''' </summary>
Public Class KBotDataViewScrollBarVisibilityTests

    Private Shared Function GridInPanel(panel As Panel) As KBotDataView
        Dim dv As New KBotDataView()
        dv.AutoSizeColumnsMode = KBotAutoSizeMode.None
        dv.ColumnFillMode = KBotFillMode.None
        dv.Size = New Size(400, 200)
        dv.ApplyTheme(BuiltInSchemes.Classic())
        dv.AddColumn("c0", "C0", KBotColumnType.Text, 100)
        panel.Controls.Add(dv)
        Return dv
    End Function

    Private Shared Sub Fill(dv As KBotDataView, rows As Integer)
        dv.BeginUpdate()
        Try
            dv.ClearRows()
            For i As Integer = 1 To rows
                dv.AddRow()("c0") = i.ToString()
            Next
        Finally
            dv.EndUpdate()
        End Try
    End Sub

    <Fact>
    Public Sub VerticalBar_HidesWhenRowsShrink_WhileHostIsHidden()
        Using host As New Panel()
            host.Size = New Size(500, 300)
            Dim dv = GridInPanel(host)

            Fill(dv, 200)                                   ' plenty: the bar is needed
            Assert.True(dv.vScroll.Visible)

            host.Visible = False                            ' the page goes away
            Fill(dv, 2)                                     ' ...and the data shrinks meanwhile
            host.Visible = True                             ' the page comes back

            ' The bar must NOT come back with the page: the hide asked for while hidden lands.
            Assert.False(dv.vScroll.Visible)
            Assert.Equal(0, dv.vScroll.Value)
        End Using
    End Sub

    <Fact>
    Public Sub VerticalBar_ShowsWhenRowsGrow_WhileHostIsHidden()
        Using host As New Panel()
            host.Size = New Size(500, 300)
            Dim dv = GridInPanel(host)

            Fill(dv, 2)
            Assert.False(dv.vScroll.Visible)

            host.Visible = False
            Fill(dv, 200)
            host.Visible = True

            Assert.True(dv.vScroll.Visible)
        End Using
    End Sub

    <Fact>
    Public Sub HorizontalBar_HidesWhenColumnsShrink_WhileHostIsHidden()
        Using host As New Panel()
            host.Size = New Size(500, 300)
            Dim dv = GridInPanel(host)
            dv.Size = New Size(250, 200)
            For i As Integer = 1 To 4                       ' 5 x 100 = 500 > 250
                dv.AddColumn("c" & i.ToString(), "C" & i.ToString(), KBotColumnType.Text, 100)
            Next
            Fill(dv, 2)
            Assert.True(dv.hScroll.Visible)

            host.Visible = False
            For i As Integer = 1 To 4
                dv.Column("c" & i.ToString()).Visible = False   ' back to 100 <= 250
            Next
            host.Visible = True

            Assert.False(dv.hScroll.Visible)
        End Using
    End Sub

End Class
