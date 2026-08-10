Option Strict On
Imports System.ComponentModel
Imports System.Drawing.Design
Imports Xunit
Imports KBot.Theming

''' <summary>
''' English (slice 0025): the designer-authoring surface of <see cref="KBotDataView"/> — the
''' <c>Columns</c> collection, the now-writable <see cref="KBotDataColumn.Key"/> /
''' <c>ColumnType</c>, and <c>ISupportInitialize</c>. The point of every assertion here is that a
''' column authored in the property grid ends up in EXACTLY the state a column added through
''' <c>AddColumn</c> would, and that <c>AddColumn</c>'s own contract did not move an inch.
'''
''' What these tests CANNOT prove: the Visual Studio round-trip itself. That is a manual check —
''' see the worklog, where it is recorded as NOT RUN.
''' </summary>
Public Class KBotDataColumnDesignerTests

    ' ── Constructorul fără parametri (cel folosit de dialogul de colecție) ────

    <Fact>
    Public Sub ParameterlessColumn_IsUsableOnceKeyAndTypeAreAssigned()
        Dim col As New KBotDataColumn()
        Assert.Null(col.Key)

        col.Key = "cod"
        col.HeaderText = "Cod"
        col.ColumnType = KBotColumnType.Text
        col.Width = 120

        Assert.Equal("cod", col.Key)
        Assert.Equal(KBotColumnType.Text, col.ColumnType)
        Assert.Equal(120, col.Width)

        Using dv As New KBotDataView()
            dv.Columns.Add(col)
            Assert.Same(col, dv.Column("cod"))
        End Using
    End Sub

    <Fact>
    Public Sub ParameterlessColumn_ClampsWidthLikeAnyOther()
        Dim col As New KBotDataColumn()
        col.Width = 5                            ' sub MinWidth
        Assert.Equal(40, col.Width)
        col.MaxWidth = 60
        col.Width = 1000
        Assert.Equal(60, col.Width)
    End Sub

    ' ── Key / ColumnType: scriibile, dar nu peste rânduri ────────────────────

    <Fact>
    Public Sub KeySetter_Succeeds_WhenTheGridHasNoRows_AndTheIndexFollows()
        Using dv As New KBotDataView()
            Dim col = dv.AddColumn("vechi", "Vechi", KBotColumnType.Text, 80)
            col.Key = "nou"

            Assert.Same(col, dv.Column("nou"))
            Assert.Throws(Of ArgumentException)(Function() dv.Column("vechi"))
        End Using
    End Sub

    <Fact>
    Public Sub KeySetter_Throws_WhenTheGridHasRows()
        ' Cell values live in KBotDataRow's dictionary keyed by the column key: renaming under
        ' data would orphan every stored cell and paint an empty column instead.
        Using dv As New KBotDataView()
            Dim col = dv.AddColumn("cod", "Cod", KBotColumnType.Text, 80)
            dv.AddRow()("cod") = "X"
            Assert.Throws(Of InvalidOperationException)(Sub() col.Key = "altceva")
            Assert.Equal("cod", col.Key)
        End Using
    End Sub

    <Fact>
    Public Sub ColumnTypeSetter_Throws_WhenTheGridHasRows()
        Using dv As New KBotDataView()
            Dim col = dv.AddColumn("cod", "Cod", KBotColumnType.Text, 80)
            dv.AddRow()
            Assert.Throws(Of InvalidOperationException)(Sub() col.ColumnType = KBotColumnType.CheckBox)
            Assert.Equal(KBotColumnType.Text, col.ColumnType)
        End Using
    End Sub

    <Fact>
    Public Sub ColumnTypeSetter_Succeeds_WhenTheGridHasNoRows()
        Using dv As New KBotDataView()
            Dim col = dv.AddColumn("bif", "Bifă", KBotColumnType.Text, 60)
            col.ColumnType = KBotColumnType.CheckBox
            Assert.Equal(KBotColumnType.CheckBox, col.ColumnType)
        End Using
    End Sub

    <Fact>
    Public Sub KeyAndColumnType_AreUnrestricted_WhileTheColumnHasNoOwner()
        ' The designer's situation: the column is edited before it belongs to anything.
        Dim col As New KBotDataColumn("a", "A", KBotColumnType.Text, 50)
        col.Key = "b"
        col.ColumnType = KBotColumnType.ProgressBar
        Assert.Equal("b", col.Key)
        Assert.Equal(KBotColumnType.ProgressBar, col.ColumnType)
    End Sub

    <Fact>
    Public Sub RemovingAColumn_ClearsItsOwner_SoTheGuardStopsApplying()
        Using dv As New KBotDataView()
            Dim col = dv.AddColumn("cod", "Cod", KBotColumnType.Text, 80)
            dv.AddRow()
            Assert.Throws(Of InvalidOperationException)(Sub() col.Key = "x")

            dv.Columns.Remove(col)
            col.Key = "x"                        ' free-floating again
            Assert.Equal("x", col.Key)
            Assert.Throws(Of ArgumentException)(Function() dv.Column("cod"))
        End Using
    End Sub

    ' ── Colecția rezolvă indexul și layout-ul singură ────────────────────────

    <Fact>
    Public Sub ColumnsAdd_BehavesIdenticallyToAddColumn()
        Using viaCollection As New KBotDataView(), viaAddColumn As New KBotDataView()
            viaCollection.Columns.Add(New KBotDataColumn("cod", "Cod", KBotColumnType.Text, 80))
            viaAddColumn.AddColumn("cod", "Cod", KBotColumnType.Text, 80)

            Assert.Equal(viaAddColumn.Columns.Count, viaCollection.Columns.Count)
            Assert.Equal(viaAddColumn.Columns(0).Key, viaCollection.Columns(0).Key)
            Assert.Equal(viaAddColumn.Columns(0).HeaderText, viaCollection.Columns(0).HeaderText)
            Assert.Equal(viaAddColumn.Columns(0).Width, viaCollection.Columns(0).Width)
            ' Reachable through the key index, which only a rebuild could have filled.
            Assert.NotNull(viaCollection.Column("cod"))
        End Using
    End Sub

    <Fact>
    Public Sub ColumnsRemoveAndClear_RebuildTheIndex()
        Using dv As New KBotDataView()
            dv.AddColumn("a", "A", KBotColumnType.Text, 50)
            dv.AddColumn("b", "B", KBotColumnType.Text, 50)

            dv.Columns.RemoveAt(0)
            Assert.Throws(Of ArgumentException)(Function() dv.Column("a"))
            Assert.NotNull(dv.Column("b"))

            dv.Columns.Clear()
            Assert.Empty(dv.Columns)
            Assert.Throws(Of ArgumentException)(Function() dv.Column("b"))
        End Using
    End Sub

    <Fact>
    Public Sub ColumnsAdd_RecomputesTotals()
        ' The totals band must be live for a column added straight into the collection, exactly
        ' as it is for one added through AddColumn.
        Using dv As New KBotDataView()
            dv.FooterVisible = True
            Dim col As New KBotDataColumn("val", "Valoare", KBotColumnType.Text, 80) With {
                .ValueType = KBotValueType.Number,
                .Aggregate = KBotAggregate.Sum
            }
            dv.Columns.Add(col)
            dv.BeginUpdate()
            dv.AddRow()("val") = 10
            dv.AddRow()("val") = 32
            dv.EndUpdate()
            Assert.Equal("42", dv.DebugFooterText("val"))
        End Using
    End Sub

    <Fact>
    Public Sub Columns_RejectsNothing()
        Using dv As New KBotDataView()
            Assert.Throws(Of ArgumentNullException)(Sub() dv.Columns.Add(Nothing))
        End Using
    End Sub

    ' ── AddColumn: contract NESCHIMBAT ───────────────────────────────────────

    <Fact>
    Public Sub AddColumn_IsUnchanged_SameThrowsSameReturnSameSideEffects()
        Using dv As New KBotDataView()
            Dim col = dv.AddColumn("a", "A", KBotColumnType.Text, 100)
            Assert.NotNull(col)
            Assert.Same(col, dv.Column("a"))
            Assert.Same(col, dv.Columns(0))
            Assert.Single(dv.Columns)

            Assert.Throws(Of ArgumentException)(Function() dv.AddColumn("a", "A2", KBotColumnType.Text, 100))
            Assert.Throws(Of ArgumentException)(Function() dv.AddColumn("", "X", KBotColumnType.Text, 100))
            Assert.Throws(Of ArgumentException)(Function() dv.AddColumn("   ", "X", KBotColumnType.Text, 100))
            Assert.Single(dv.Columns)                ' nicio coloană adăugată pe drum
        End Using
    End Sub

    ' ── ISupportInitialize ───────────────────────────────────────────────────

    <Fact>
    Public Sub EndInit_ThrowsOnDuplicateColumnKeys()
        Using dv As New KBotDataView()
            Dim init As ISupportInitialize = dv
            init.BeginInit()
            dv.Columns.Add(New KBotDataColumn("a", "A", KBotColumnType.Text, 50))
            dv.Columns.Add(New KBotDataColumn("a", "A din nou", KBotColumnType.Text, 50))
            Assert.Throws(Of ArgumentException)(Sub() init.EndInit())
        End Using
    End Sub

    <Fact>
    Public Sub EndInit_ThrowsOnAnEmptyColumnKey()
        Using dv As New KBotDataView()
            Dim init As ISupportInitialize = dv
            init.BeginInit()
            dv.Columns.Add(New KBotDataColumn())      ' cheia nu a fost tastată niciodată
            Assert.Throws(Of ArgumentException)(Sub() init.EndInit())
        End Using
    End Sub

    <Fact>
    Public Sub EndInit_AcceptsAValidSetAndLeavesTheGridUsable()
        Using dv As New KBotDataView()
            Dim init As ISupportInitialize = dv
            init.BeginInit()
            dv.Columns.Add(New KBotDataColumn("cod", "Cod", KBotColumnType.Text, 80))
            dv.Columns.Add(New KBotDataColumn("val", "Valoare", KBotColumnType.Text, 90))
            init.EndInit()

            Assert.Equal(2, dv.Columns.Count)
            Assert.NotNull(dv.Column("cod"))
            Assert.NotNull(dv.Column("val"))
            dv.AddRow()("val") = 1
            Assert.Equal(1, dv.RowCount)
        End Using
    End Sub

    ' ── Proxy pentru rotunda din designer ────────────────────────────────────

    <Fact>
    Public Sub StockCollectionEditor_IsResolvedForColumns()
        ' What makes the "…" button appear next to Columns in the property grid: the stock
        ' CollectionEditor TypeDescriptor registers intrinsically for ICollection.
        Dim prop As PropertyDescriptor = TypeDescriptor.GetProperties(GetType(KBotDataView))("Columns")
        Assert.NotNull(prop)
        Assert.Equal(DesignerSerializationVisibility.Content, prop.SerializationVisibility)

        Dim ed As Object = prop.GetEditor(GetType(UITypeEditor))
        Assert.NotNull(ed)
        Assert.Contains("CollectionEditor", ed.GetType().FullName)
    End Sub

    <Fact>
    Public Sub RowsAndRowCount_AreNeverSerializedByTheDesigner()
        ' Rows are runtime DATA. If the designer ever wrote them into InitializeComponent the
        ' form would carry a frozen snapshot of somebody's test data.
        For Each name In {"Rows", "RowCount", "CurrentRowIndex", "CurrentColumnKey", "CurrentRow", "IsEditing"}
            Dim prop As PropertyDescriptor = TypeDescriptor.GetProperties(GetType(KBotDataView))(name)
            Assert.NotNull(prop)
            Assert.False(prop.IsBrowsable, name & " nu trebuie să apară în grila de proprietăți.")
            Assert.Equal(DesignerSerializationVisibility.Hidden, prop.SerializationVisibility)
        Next
    End Sub

    <Fact>
    Public Sub ComboItemsAndTag_AreNeverSerialized()
        ' Neither can round-trip through InitializeComponent; a half-serialized combo source is
        ' worse than none.
        For Each name In {"ComboItems", "Tag", "IsEffectivelyVisible"}
            Dim prop As PropertyDescriptor = TypeDescriptor.GetProperties(GetType(KBotDataColumn))(name)
            Assert.NotNull(prop)
            Assert.False(prop.IsBrowsable, name & " nu trebuie să apară în grila de proprietăți.")
            Assert.Equal(DesignerSerializationVisibility.Hidden, prop.SerializationVisibility)
        Next
    End Sub

    <Fact>
    Public Sub FooterHeight_IsNotSerializedWhileItTracksTheHeader()
        ' The getter resolves to HeaderHeight when unset, so a DefaultValue would make the
        ' designer write the RESOLVED number and pin the band for good.
        Using dv As New KBotDataView()
            Dim prop As PropertyDescriptor = TypeDescriptor.GetProperties(dv)("FooterHeight")
            Assert.False(prop.ShouldSerializeValue(dv))
            Assert.Equal(dv.HeaderHeight, dv.FooterHeight)

            dv.FooterHeight = 44
            Assert.True(prop.ShouldSerializeValue(dv))
            Assert.Equal(44, dv.FooterHeight)

            prop.ResetValue(dv)
            Assert.False(prop.ShouldSerializeValue(dv))
            Assert.Equal(dv.HeaderHeight, dv.FooterHeight)
        End Using
    End Sub

    <Fact>
    Public Sub Column_ToString_ShowsKeyHeaderAndType()
        Dim col As New KBotDataColumn("val", "Valoare", KBotColumnType.ProgressBar, 90)
        Assert.Contains("val", col.ToString())
        Assert.Contains("Valoare", col.ToString())
        Assert.Contains("ProgressBar", col.ToString())
        Assert.Contains("fără cheie", New KBotDataColumn().ToString())
    End Sub

End Class
