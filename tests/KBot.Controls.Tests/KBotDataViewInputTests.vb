Imports System.Drawing
Imports KBot.Theming
Imports Xunit

''' <summary>
''' Teste pentru slice 0010-05: matematica de navigație, selecția și comutarea celulelor.
''' Nu se pot trimite taste reale fără buclă de mesaje, deci se testează punctele de intrare
''' pe care le folosesc handler-ii (NextEnabledColumn, ActivateCell, EnsureVisible, setter-ele
''' de selecție) — adică exact logica, fără WinForms în cale.
''' </summary>
Public Class KBotDataViewInputTests

    Private Shared Function Grid(Optional rows As Integer = 3) As KBotDataView
        Dim dv As New KBotDataView()
        dv.Size = New Size(500, 300)
        dv.ApplyTheme(BuiltInSchemes.Classic())
        dv.AddColumn("a", "A", KBotColumnType.Text, 100)
        dv.AddColumn("b", "B", KBotColumnType.Text, 100)
        dv.AddColumn("c", "C", KBotColumnType.Text, 100)
        For i As Integer = 1 To rows
            dv.AddRow()
        Next
        Return dv
    End Function

    ' ── NextEnabledColumn ───────────────────────────────────────────────────────

    <Fact>
    Public Sub NextEnabledColumn_MovesOneStep()
        Using dv = Grid()
            Assert.Equal("b", dv.NextEnabledColumn("a", 1).Key)
            Assert.Equal("a", dv.NextEnabledColumn("b", -1).Key)
        End Using
    End Sub

    <Fact>
    Public Sub NextEnabledColumn_SkipsDisabledColumns()
        Using dv = Grid()
            dv.Column("b").Enabled = False
            Assert.Equal("c", dv.NextEnabledColumn("a", 1).Key)     ' sare peste „b”
            Assert.Equal("a", dv.NextEnabledColumn("c", -1).Key)
        End Using
    End Sub

    <Fact>
    Public Sub NextEnabledColumn_SkipsHiddenColumns()
        Using dv = Grid()
            dv.Column("b").Visible = False
            Assert.Equal("c", dv.NextEnabledColumn("a", 1).Key)
        End Using
    End Sub

    <Fact>
    Public Sub NextEnabledColumn_DoesNotWrapAtEdges()
        Using dv = Grid()
            Assert.Null(dv.NextEnabledColumn("c", 1))     ' capătul din dreapta
            Assert.Null(dv.NextEnabledColumn("a", -1))    ' capătul din stânga
        End Using
    End Sub

    <Fact>
    Public Sub NextEnabledColumn_WithoutStart_TakesFirstOrLastEnabled()
        Using dv = Grid()
            dv.Column("a").Enabled = False
            Assert.Equal("b", dv.NextEnabledColumn(Nothing, 1).Key)
            Assert.Equal("c", dv.NextEnabledColumn(Nothing, -1).Key)
        End Using
    End Sub

    ' ── Selecție ────────────────────────────────────────────────────────────────

    <Fact>
    Public Sub Selection_RaisesEventOnlyOnRealChange()
        Using dv = Grid()
            Dim raises As Integer = 0
            AddHandler dv.SelectionChanged, Sub(s, e) raises += 1

            dv.CurrentRowIndex = 1
            Assert.Equal(1, raises)
            dv.CurrentRowIndex = 1          ' aceeași valoare => fără eveniment
            Assert.Equal(1, raises)
            dv.CurrentColumnKey = "b"
            Assert.Equal(2, raises)
        End Using
    End Sub

    <Fact>
    Public Sub Selection_OutOfRangeRowBecomesNoSelection()
        Using dv = Grid()
            dv.CurrentRowIndex = 99
            Assert.Equal(-1, dv.CurrentRowIndex)
            Assert.Null(dv.CurrentRow)
        End Using
    End Sub

    <Fact>
    Public Sub Selection_CurrentRowFollowsIndex()
        Using dv = Grid()
            dv.CurrentRowIndex = 2
            Assert.Same(dv.Rows(2), dv.CurrentRow)
        End Using
    End Sub

    <Fact>
    Public Sub Selection_UnknownColumnKeyThrows()
        Using dv = Grid()
            Assert.Throws(Of ArgumentException)(Sub() dv.CurrentColumnKey = "inexistent")
        End Using
    End Sub

    ' ── EnsureVisible ───────────────────────────────────────────────────────────

    <Fact>
    Public Sub EnsureVisible_ScrollsDownToReachFarRow()
        Using dv = Grid(500)                       ' mult peste o fereastră
            Assert.Equal(0, dv.vScroll.Value)
            dv.EnsureVisible(400)
            Assert.True(dv.vScroll.Value > 0, "trebuie să fi derulat în jos")

            ' Rândul 400 trebuie să încapă acum complet în fereastră.
            Dim top As Integer = 400 * dv.RowHeight
            Assert.True(top >= dv.vScroll.Value, "rândul nu iese pe sus")
            Assert.True(top + dv.RowHeight <= dv.vScroll.Value + dv.vScroll.LargeChange,
                        "rândul nu iese pe jos")
        End Using
    End Sub

    <Fact>
    Public Sub EnsureVisible_ScrollsBackUp()
        Using dv = Grid(500)
            dv.EnsureVisible(400)
            dv.EnsureVisible(0)
            Assert.Equal(0, dv.vScroll.Value)
        End Using
    End Sub

    <Fact>
    Public Sub EnsureVisible_IgnoresOutOfRangeIndex()
        Using dv = Grid(500)
            dv.EnsureVisible(99999)                ' nu aruncă, nu derulează
            Assert.Equal(0, dv.vScroll.Value)
        End Using
    End Sub

    ' ── Comutare / acționare ────────────────────────────────────────────────────

    <Fact>
    Public Sub ActivateCell_TogglesCheckBoxAndRaisesValueChanged()
        Using dv As New KBotDataView()
            dv.AddColumn("chk", "Chk", KBotColumnType.CheckBox, 60)
            dv.AddRow()
            Dim changes As Integer = 0
            Dim lastNew As Object = Nothing
            AddHandler dv.CellValueChanged,
                Sub(s, e)
                    changes += 1
                    lastNew = e.NewValue
                End Sub

            dv.ActivateCell("chk", 0)
            Assert.Equal(True, dv("chk", 0))
            Assert.Equal(1, changes)
            Assert.Equal(True, lastNew)

            dv.ActivateCell("chk", 0)              ' comută înapoi
            Assert.Equal(False, dv("chk", 0))
            Assert.Equal(2, changes)
        End Using
    End Sub

    <Fact>
    Public Sub ActivateCell_OptionSelectsAndClearsSibling()
        Using dv As New KBotDataView()
            dv.AddColumn("x", "X", KBotColumnType.OptionButton, 60).OptionGroup = "g"
            dv.AddColumn("y", "Y", KBotColumnType.OptionButton, 60).OptionGroup = "g"
            dv.AddRow()

            dv.ActivateCell("x", 0)
            Assert.Equal(True, dv("x", 0))

            dv.ActivateCell("y", 0)
            Assert.Equal(True, dv("y", 0))
            Assert.Equal(False, dv("x", 0))        ' sora din grup s-a stins
        End Using
    End Sub

    <Fact>
    Public Sub ActivateCell_OptionAlreadySetIsNoOp()
        Using dv As New KBotDataView()
            dv.AddColumn("x", "X", KBotColumnType.OptionButton, 60).OptionGroup = "g"
            dv.AddRow()
            dv.ActivateCell("x", 0)
            Dim changes As Integer = 0
            AddHandler dv.CellValueChanged, Sub(s, e) changes += 1
            dv.ActivateCell("x", 0)                ' un radio nu se de-bifează
            Assert.Equal(0, changes)
            Assert.Equal(True, dv("x", 0))
        End Using
    End Sub

    <Fact>
    Public Sub ActivateCell_ButtonRaisesButtonClickAndDoesNotDirtyRow()
        Using dv As New KBotDataView()
            dv.AddColumn("btn", "Buton", KBotColumnType.Button, 80)
            dv.AddRow()
            Dim clicks As Integer = 0
            AddHandler dv.ButtonClick, Sub(s, e) clicks += 1

            dv.ActivateCell("btn", 0)
            Assert.Equal(1, clicks)
            Assert.False(dv.Rows(0).IsDirty, "butonul nu ține valoare, deci nu murdărește rândul")
        End Using
    End Sub

    <Fact>
    Public Sub ActivateCell_DisabledCellDoesNothing()
        Using dv As New KBotDataView()
            dv.AddColumn("chk", "Chk", KBotColumnType.CheckBox, 60)
            dv.AddRow()
            dv.Column("chk").Enabled = False
            Dim changes As Integer = 0
            AddHandler dv.CellValueChanged, Sub(s, e) changes += 1

            dv.ActivateCell("chk", 0)
            Assert.Equal(0, changes)
            Assert.Null(dv("chk", 0))              ' valoarea a rămas neatinsă
        End Using
    End Sub

    <Fact>
    Public Sub ActivateCell_DisabledRowDoesNothing()
        Using dv As New KBotDataView()
            dv.AddColumn("chk", "Chk", KBotColumnType.CheckBox, 60)
            dv.AddRow()
            dv.Rows(0).Enabled = False
            dv.ActivateCell("chk", 0)
            Assert.Null(dv("chk", 0))
        End Using
    End Sub

End Class
