Imports System.Drawing
Imports KBot.Theming
Imports Xunit

''' <summary>
''' Teste pentru slice 0010-06 (editare), conduse prin punctele de intrare interne
''' <c>BeginEdit</c>/<c>CommitEdit</c>/<c>CancelEdit</c> — singurul mod de a exercita editarea
''' headless (editorii reali au nevoie de buclă de mesaje pentru focus/taste).
''' </summary>
Public Class KBotDataViewEditingTests

    Private Shared Function Grid() As KBotDataView
        Dim dv As New KBotDataView()
        dv.Size = New Size(500, 300)
        dv.ApplyTheme(BuiltInSchemes.Classic())
        dv.AddColumn("txt", "Text", KBotColumnType.Text, 120)
        dv.AddColumn("cmb", "Combo", KBotColumnType.Combo, 120)
        dv.AddColumn("chk", "Check", KBotColumnType.CheckBox, 60)
        dv.AddRow()
        dv.AddRow()
        Return dv
    End Function

    ' ── CanEdit ─────────────────────────────────────────────────────────────────

    <Fact>
    Public Sub CanEdit_TextAndComboOnly()
        Using dv = Grid()
            Assert.True(dv.CanEdit("txt", 0))
            Assert.True(dv.CanEdit("cmb", 0))
            Assert.False(dv.CanEdit("chk", 0))       ' bifa se comută, nu se editează
        End Using
    End Sub

    <Fact>
    Public Sub CanEdit_FalseWhenGridIsReadOnly()
        Using dv = Grid()
            dv.ReadOnlyGrid = True                    ' modul folosit de vederea Sumar
            Assert.False(dv.CanEdit("txt", 0))
        End Using
    End Sub

    <Fact>
    Public Sub CanEdit_FalseWhenColumnIsReadOnlyOrDisabled()
        Using dv = Grid()
            dv.Column("txt").ReadOnly = True
            Assert.False(dv.CanEdit("txt", 0))

            dv.Column("cmb").Enabled = False
            Assert.False(dv.CanEdit("cmb", 0))
        End Using
    End Sub

    <Fact>
    Public Sub CanEdit_FalseWhenRowOrCellIsDisabled()
        Using dv = Grid()
            dv.Rows(0).Enabled = False
            Assert.False(dv.CanEdit("txt", 0))
            Assert.True(dv.CanEdit("txt", 1))

            AddHandler dv.CellFormatting,
                Sub(s, e)
                    If e.ColumnKey = "txt" AndAlso e.RowIndex = 1 Then e.Enabled = False
                End Sub
            Assert.False(dv.CanEdit("txt", 1))
        End Using
    End Sub

    ' ── Commit ──────────────────────────────────────────────────────────────────

    <Fact>
    Public Sub Commit_WritesValueMarksDirtyAndRaisesValueChanged()
        Using dv = Grid()
            Dim changes As Integer = 0
            Dim oldSeen As Object = Nothing
            Dim newSeen As Object = Nothing
            AddHandler dv.CellValueChanged,
                Sub(s, e)
                    changes += 1
                    oldSeen = e.OldValue
                    newSeen = e.NewValue
                End Sub

            dv("txt", 0) = "initial"                  ' încărcare => nu murdărește
            Assert.Empty(dv.GetDirtyRows())

            Assert.True(dv.BeginEdit("txt", 0))
            Assert.True(dv.IsEditing)
            dv.editText.Text = "editat"
            Assert.True(dv.CommitEdit())

            Assert.False(dv.IsEditing)
            Assert.Equal("editat", dv("txt", 0))
            Assert.Equal(1, changes)
            Assert.Equal("initial", oldSeen)
            Assert.Equal("editat", newSeen)

            ' Exact rândul editat apare ca „murdar”.
            Dim dirty = dv.GetDirtyRows()
            Assert.Single(dirty)
            Assert.Same(dv.Rows(0), dirty(0))
        End Using
    End Sub

    <Fact>
    Public Sub Commit_VetoKeepsEditorOpenAndLeavesValueUntouched()
        Using dv = Grid()
            dv("txt", 0) = "initial"
            AddHandler dv.CellValidating, Sub(s, e) e.Cancel = True

            Assert.True(dv.BeginEdit("txt", 0))
            dv.editText.Text = "respins"
            Assert.False(dv.CommitEdit())

            Assert.True(dv.IsEditing, "editorul rămâne deschis după veto")
            Assert.Equal("initial", dv("txt", 0))
            Assert.Empty(dv.GetDirtyRows())
        End Using
    End Sub

    ' Regresie: „ProposedValue = proposedValue” nekalificat în constructor era un NO-OP
    ' (VB e case-insensitive), deci commit-ul scria mereu Nothing în celulă.
    <Fact>
    Public Sub ValidatingArgs_CarryTheProposedValue()
        Dim args As New KBotCellValidatingEventArgs("k", 3, "propus")
        Assert.Equal("k", args.ColumnKey)
        Assert.Equal(3, args.RowIndex)
        Assert.Equal("propus", args.ProposedValue)
        Assert.False(args.Cancel)
    End Sub

    <Fact>
    Public Sub Commit_HandlerCanCoerceTheProposedValue()
        Using dv = Grid()
            AddHandler dv.CellValidating,
                Sub(s, e) e.ProposedValue = e.ProposedValue.ToString().Trim().ToUpperInvariant()

            dv.BeginEdit("txt", 0)
            dv.editText.Text = "  abc  "
            Assert.True(dv.CommitEdit())
            Assert.Equal("ABC", dv("txt", 0))
        End Using
    End Sub

    <Fact>
    Public Sub Cancel_DiscardsAndRaisesNothing()
        Using dv = Grid()
            dv("txt", 0) = "initial"
            Dim changes As Integer = 0
            AddHandler dv.CellValueChanged, Sub(s, e) changes += 1

            dv.BeginEdit("txt", 0)
            dv.editText.Text = "aruncat"
            dv.CancelEdit()

            Assert.False(dv.IsEditing)
            Assert.Equal("initial", dv("txt", 0))
            Assert.Equal(0, changes)
            Assert.Empty(dv.GetDirtyRows())
        End Using
    End Sub

    ' ── Un singur editor viu / auto-commit ──────────────────────────────────────

    <Fact>
    Public Sub BeginEdit_OnAnotherCell_CommitsThePreviousOne()
        Using dv = Grid()
            dv.BeginEdit("txt", 0)
            dv.editText.Text = "primul"
            dv.BeginEdit("txt", 1)                    ' comite implicit celula precedentă

            Assert.Equal("primul", dv("txt", 0))
            Assert.True(dv.IsEditing)
        End Using
    End Sub

    <Fact>
    Public Sub MovingCurrentCell_CommitsOpenEditor()
        Using dv = Grid()
            dv.BeginEdit("txt", 0)
            dv.editText.Text = "comis la mutare"
            dv.CurrentRowIndex = 1                    ' mutarea comite

            Assert.False(dv.IsEditing)
            Assert.Equal("comis la mutare", dv("txt", 0))
        End Using
    End Sub

    <Fact>
    Public Sub VetoedCommit_BlocksTheSelectionMove()
        Using dv = Grid()
            AddHandler dv.CellValidating, Sub(s, e) e.Cancel = True
            dv.CurrentRowIndex = 0
            dv.BeginEdit("txt", 0)
            dv.editText.Text = "respins"

            dv.CurrentRowIndex = 1                    ' trebuie blocată de veto
            Assert.Equal(0, dv.CurrentRowIndex)
            Assert.True(dv.IsEditing)
        End Using
    End Sub

    <Fact>
    Public Sub BeginEdit_RefusesNonEditableCell()
        Using dv = Grid()
            Assert.False(dv.BeginEdit("chk", 0))
            Assert.False(dv.IsEditing)
        End Using
    End Sub

    <Fact>
    Public Sub CommitWithoutEditing_IsHarmless()
        Using dv = Grid()
            Assert.True(dv.CommitEdit())              ' nimic deschis => succes trivial
            dv.CancelEdit()                           ' idem, fără excepție
            Assert.False(dv.IsEditing)
        End Using
    End Sub

    ' ── Dirty ───────────────────────────────────────────────────────────────────

    <Fact>
    Public Sub ClearDirty_ResetsTheWholeGrid()
        Using dv = Grid()
            dv.BeginEdit("txt", 0)
            dv.editText.Text = "x"
            dv.CommitEdit()
            dv.ActivateCell("chk", 1)                 ' comutare => și ea murdărește
            Assert.Equal(2, dv.GetDirtyRows().Count)

            dv.ClearDirty()
            Assert.Empty(dv.GetDirtyRows())
        End Using
    End Sub

    <Fact>
    Public Sub BulkLoad_LeavesEverythingClean()
        Using dv = Grid()
            dv.BeginUpdate()
            For i As Integer = 0 To 1
                dv("txt", i) = "incarcat " & i.ToString()
                dv("chk", i) = True
            Next
            dv.EndUpdate()
            Assert.Empty(dv.GetDirtyRows())
        End Using
    End Sub

End Class
