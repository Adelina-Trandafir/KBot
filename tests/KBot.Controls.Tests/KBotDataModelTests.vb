Imports Xunit

''' <summary>
''' Teste pentru modelele nelegate ale KBotDataView (slice 0010-01): coloană + rând.
''' Logică pură, fără handle de fereastră.
''' </summary>
Public Class KBotDataModelTests

    ' ── KBotDataColumn ──────────────────────────────────────────────────────────

    <Fact>
    Public Sub Column_CtorRejectsEmptyKey()
        Assert.Throws(Of ArgumentException)(Function() New KBotDataColumn("", "H", KBotColumnType.Text, 100))
        Assert.Throws(Of ArgumentException)(Function() New KBotDataColumn("   ", "H", KBotColumnType.Text, 100))
    End Sub

    <Fact>
    Public Sub Column_WidthNeverBelowMinWidth()
        Dim c = New KBotDataColumn("k", "H", KBotColumnType.Text, 10)   ' sub MinWidth-ul implicit (40)
        Assert.Equal(40, c.Width)
        c.Width = 5
        Assert.Equal(40, c.Width)
        c.Width = 120
        Assert.Equal(120, c.Width)
    End Sub

    <Fact>
    Public Sub Column_RaisingMinWidthPushesWidth()
        Dim c = New KBotDataColumn("k", "H", KBotColumnType.Text, 60)
        c.MinWidth = 100
        Assert.Equal(100, c.Width)
    End Sub

    ' Regresie: „HeaderText = headerText” nekalificat era un NO-OP (VB e case-insensitive,
    ' parametrul ascunde proprietatea), deci TOATE antetele rămâneau Nothing.
    <Fact>
    Public Sub Column_HeaderTextIsActuallyStored()
        Dim c = New KBotDataColumn("cod", "Cod indicator", KBotColumnType.Text, 80)
        Assert.Equal("Cod indicator", c.HeaderText)
        Assert.Equal(String.Empty, New KBotDataColumn("x", Nothing, KBotColumnType.Text, 10).HeaderText)
    End Sub

    <Fact>
    Public Sub Column_KeyAndTypeAreFixed()
        Dim c = New KBotDataColumn("cod", "Cod", KBotColumnType.Combo, 80)
        Assert.Equal("cod", c.Key)
        Assert.Equal("Cod", c.HeaderText)
        Assert.Equal(KBotColumnType.Combo, c.ColumnType)
        Assert.True(c.Visible)
        Assert.True(c.Enabled)
        Assert.True(c.Resizable)
        Assert.False(c.Frozen)
    End Sub

    ' ── KBotDataRow ─────────────────────────────────────────────────────────────

    <Fact>
    Public Sub Row_MissingKeyReturnsNothing()
        Dim r = New KBotDataRow()
        Assert.Null(r("inexistent"))
        Assert.False(r.HasValue("inexistent"))
        Assert.False(r.IsDirty)
    End Sub

    ' Contract fixat în 0010-06: scrierea programatică e ÎNCĂRCARE de date, NU editare,
    ' deci nu ridică IsDirty. Doar commit-ul de editare și comutările îl ridică.
    <Fact>
    Public Sub Row_SetStoresWithoutMarkingDirty()
        Dim r = New KBotDataRow()
        r("a") = "x"
        Assert.Equal("x", r("a"))
        Assert.True(r.HasValue("a"))
        Assert.False(r.IsDirty)
    End Sub

    <Fact>
    Public Sub Row_MarkCleanResetsDirty()
        Dim r = New KBotDataRow()
        r.IsDirty = True
        r.MarkClean()
        Assert.False(r.IsDirty)
    End Sub

    <Fact>
    Public Sub Row_StoredNothingIsDistinctFromAbsent()
        Dim r = New KBotDataRow()
        r("a") = Nothing
        Assert.True(r.HasValue("a"))     ' există cu valoarea Nothing…
        Assert.False(r.HasValue("b"))    ' …față de o cheie absentă
    End Sub

    <Fact>
    Public Sub Row_SetEmptyKeyThrows()
        Dim r = New KBotDataRow()
        Assert.Throws(Of ArgumentException)(Sub() r(Nothing) = 1)
        Assert.Throws(Of ArgumentException)(Sub() r("") = 1)
    End Sub

End Class
