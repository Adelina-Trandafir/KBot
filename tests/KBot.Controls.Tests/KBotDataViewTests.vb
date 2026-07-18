Imports KBot.Theming
Imports Xunit

''' <summary>
''' Teste pentru suprafața publică non-vizuală a KBotDataView (slice 0010-01). Controlul se
''' instanțiază fără handle (nu-l afișăm), deci se testează colecțiile, validările și
''' cache-ul de temă — nu pictarea.
''' </summary>
Public Class KBotDataViewTests

    <Fact>
    Public Sub AddColumn_RejectsDuplicateAndEmptyKey()
        Using dv As New KBotDataView()
            dv.AddColumn("a", "A", KBotColumnType.Text, 100)
            Assert.Throws(Of ArgumentException)(Function() dv.AddColumn("a", "A2", KBotColumnType.Text, 100))
            Assert.Throws(Of ArgumentException)(Function() dv.AddColumn("", "X", KBotColumnType.Text, 100))
        End Using
    End Sub

    <Fact>
    Public Sub Column_LookupKnownAndUnknown()
        Using dv As New KBotDataView()
            Dim c = dv.AddColumn("cod", "Cod", KBotColumnType.Text, 80)
            Assert.Same(c, dv.Column("cod"))
            Assert.Throws(Of ArgumentException)(Function() dv.Column("altceva"))
        End Using
    End Sub

    <Fact>
    Public Sub Columns_KeepInsertionOrder()
        Using dv As New KBotDataView()
            dv.AddColumn("a", "A", KBotColumnType.Text, 50)
            dv.AddColumn("b", "B", KBotColumnType.Text, 50)
            dv.AddColumn("c", "C", KBotColumnType.Text, 50)
            Assert.Equal(3, dv.Columns.Count)
            Assert.Equal("a", dv.Columns(0).Key)
            Assert.Equal("c", dv.Columns(2).Key)
        End Using
    End Sub

    <Fact>
    Public Sub Rows_AddClearAndCount()
        Using dv As New KBotDataView()
            dv.AddRow()
            dv.AddRow()
            Assert.Equal(2, dv.RowCount)
            dv.ClearRows()
            Assert.Equal(0, dv.RowCount)
        End Using
    End Sub

    ' Contract fixat în 0010-06: scrierea prin API e ÎNCĂRCARE, nu editare de operator.
    <Fact>
    Public Sub Item_GetSetThroughControl_DoesNotDirtyTheRow()
        Using dv As New KBotDataView()
            dv.AddColumn("a", "A", KBotColumnType.Text, 50)
            dv.AddRow()
            Assert.Null(dv("a", 0))
            dv("a", 0) = "valoare"
            Assert.Equal("valoare", dv("a", 0))
            Assert.Empty(dv.GetDirtyRows())
        End Using
    End Sub

    <Fact>
    Public Sub FrozenColumnCount_ClampsNegative()
        Using dv As New KBotDataView()
            dv.FrozenColumnCount = -3
            Assert.Equal(0, dv.FrozenColumnCount)
            dv.FrozenColumnCount = 2
            Assert.Equal(2, dv.FrozenColumnCount)
        End Using
    End Sub

    <Fact>
    Public Sub RowAndHeaderHeight_Clamp()
        Using dv As New KBotDataView()
            dv.RowHeight = 0
            Assert.True(dv.RowHeight >= 1)
            dv.HeaderHeight = -5
            Assert.Equal(0, dv.HeaderHeight)
        End Using
    End Sub

    <Fact>
    Public Sub ApplyTheme_NullIsNoOp_SchemeDoesNotThrow()
        Using dv As New KBotDataView()
            dv.ApplyTheme(Nothing)                 ' fără excepție, fără efect
            dv.ApplyTheme(BuiltInSchemes.Classic())
            dv.ApplyTheme(BuiltInSchemes.Dark())   ' re-aplicarea eliberează + recreează resursele GDI
        End Using
    End Sub

End Class
