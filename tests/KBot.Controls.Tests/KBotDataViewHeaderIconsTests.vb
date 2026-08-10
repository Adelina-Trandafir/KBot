Imports System.Drawing
Imports KBot.Theming
Imports Xunit

''' <summary>
''' Tests for the per-column header icons (slice 0028-02): the layout of the three pieces, the
''' ORDER OF SACRIFICE when the column narrows (caption first, then the left icon, never the
''' right one), the width floor those icons impose, and the hit-test/event of the right icon.
'''
''' The layout is asserted through <c>ComputeHeaderCellLayout</c> — the same pure function the
''' painter and the hit-test both call, so a passing test means the icon is drawn exactly where
''' it is clicked. Auto-sizing is switched OFF wherever a width is asserted, otherwise the
''' ToContent pass would re-measure the column out from under the assertion.
''' </summary>
Public Class KBotDataViewHeaderIconsTests

    Private Const Pad As Integer = KBotDataColumn.HeaderIconPad     ' 8
    Private Const Gap As Integer = KBotDataColumn.HeaderIconGap     ' 4

    Private Shared Function Icon(side As Integer) As Image
        Return New Bitmap(side, side)
    End Function

    Private Shared Function Grid() As KBotDataView
        Dim dv As New KBotDataView()
        dv.Size = New Size(600, 300)
        dv.AutoSizeColumnsMode = KBotAutoSizeMode.None
        dv.ApplyTheme(BuiltInSchemes.Classic())
        Return dv
    End Function

    ' ── Layout ───────────────────────────────────────────────────────────────────

    <Fact>
    Public Sub BothIcons_SitAtTheEnds_CaptionInBetween()
        Using dv = Grid()
            Dim col = dv.AddColumn("a", "Antet", KBotColumnType.Text, 200)
            col.HeaderLeftIcon = Icon(16)
            col.HeaderRightIcon = Icon(16)

            Dim cell As New Rectangle(0, 0, 200, 30)
            Dim l = KBotDataView.ComputeHeaderCellLayout(col, cell, Pad, Gap)

            Assert.Equal(Pad, l.LeftIcon.Left)
            Assert.Equal(200 - Pad - 16, l.RightIcon.Left)
            ' Textul stă între ele, cu câte un spațiu de fiecare parte.
            Assert.Equal(Pad + 16 + Gap, l.Text.Left)
            Assert.Equal(l.RightIcon.Left - Gap, l.Text.Right)
            ' Ambele centrate pe verticală.
            Assert.Equal((30 - 16) \ 2, l.LeftIcon.Top)
            Assert.Equal((30 - 16) \ 2, l.RightIcon.Top)
        End Using
    End Sub

    <Fact>
    Public Sub NoIcons_LayoutIsTheOldPaddedRectangle()
        Using dv = Grid()
            Dim col = dv.AddColumn("a", "Antet", KBotColumnType.Text, 200)
            Dim l = KBotDataView.ComputeHeaderCellLayout(col, New Rectangle(0, 0, 200, 30), Pad, Gap)

            Assert.True(l.LeftIcon.IsEmpty)
            Assert.True(l.RightIcon.IsEmpty)
            Assert.Equal(Pad, l.Text.Left)
            Assert.Equal(200 - 2 * Pad, l.Text.Width)
        End Using
    End Sub

    <Fact>
    Public Sub Narrowing_CutsTheCaptionFirst_ThenTheLeftIcon()
        Using dv = Grid()
            Dim col = dv.AddColumn("a", "Antet", KBotColumnType.Text, 200)
            col.HeaderLeftIcon = Icon(16)
            col.HeaderRightIcon = Icon(16)

            ' Exact pe podea: textul e ras complet, dar AMBELE pictograme sunt încă acolo.
            Dim pePodea = KBotDataView.ComputeHeaderCellLayout(col, New Rectangle(0, 0, col.EffectiveMinWidth, 30), Pad, Gap)
            Assert.Equal(0, pePodea.Text.Width)
            Assert.False(pePodea.LeftIcon.IsEmpty)
            Assert.False(pePodea.RightIcon.IsEmpty)

            ' Sub podea (un dreptunghi impus din afară): cade cea din STÂNGA, cea din dreapta rămâne.
            Dim subPodea = KBotDataView.ComputeHeaderCellLayout(col, New Rectangle(0, 0, 30, 30), Pad, Gap)
            Assert.True(subPodea.LeftIcon.IsEmpty)
            Assert.False(subPodea.RightIcon.IsEmpty)
        End Using
    End Sub

    <Fact>
    Public Sub OnlyRightIcon_LeavesTheWholeLeftSideToTheCaption()
        Using dv = Grid()
            Dim col = dv.AddColumn("a", "Antet", KBotColumnType.Text, 200)
            col.HeaderRightIcon = Icon(16)

            Dim l = KBotDataView.ComputeHeaderCellLayout(col, New Rectangle(0, 0, 200, 30), Pad, Gap)
            Assert.True(l.LeftIcon.IsEmpty)
            Assert.Equal(Pad, l.Text.Left)
            Assert.Equal(l.RightIcon.Left - Gap, l.Text.Right)
        End Using
    End Sub

    ' ── Podeaua de lățime ────────────────────────────────────────────────────────

    <Fact>
    Public Sub Icons_RaiseTheEffectiveMinimumWidth()
        Using dv = Grid()
            Dim col = dv.AddColumn("a", "Antet", KBotColumnType.Text, 200)
            Assert.Equal(col.MinWidth, col.EffectiveMinWidth)       ' fără pictograme: MinWidth curat

            col.HeaderLeftIcon = Icon(16)
            col.HeaderRightIcon = Icon(16)
            Assert.Equal(2 * Pad + 16 + Gap + 16, col.EffectiveMinWidth)
            Assert.Equal(col.EffectiveMinWidth, col.HeaderIconsWidth)
        End Using
    End Sub

    <Fact>
    Public Sub Width_CannotBeSetBelowTheIconFloor()
        Using dv = Grid()
            Dim col = dv.AddColumn("a", "Antet", KBotColumnType.Text, 200)
            col.HeaderLeftIcon = Icon(24)
            col.HeaderRightIcon = Icon(24)
            ' Mărimea DESENATĂ e proprietatea, nu mărimea naturală a bitmap-ului: pictograma se
            ' scalează la ce cere coloana, deci podeaua se ia din proprietate.
            col.HeaderLeftIconSize = New Size(24, 24)
            col.HeaderRightIconSize = New Size(24, 24)

            col.Width = 10
            Assert.Equal(col.EffectiveMinWidth, col.Width)
            Assert.Equal(2 * Pad + 24 + Gap + 24, col.Width)
        End Using
    End Sub

    <Fact>
    Public Sub SettingIcons_PushesAnAlreadyNarrowColumnUp()
        Using dv = Grid()
            Dim col = dv.AddColumn("a", "Antet", KBotColumnType.Text, 40)
            Assert.Equal(40, col.Width)

            col.HeaderRightIcon = Icon(32)
            col.HeaderLeftIcon = Icon(32)
            col.HeaderLeftIconSize = New Size(32, 32)
            col.HeaderRightIconSize = New Size(32, 32)
            ' Coloana nu rămâne la 40 până la următorul layout: podeaua se aplică pe loc.
            Assert.Equal(2 * Pad + 32 + Gap + 32, col.Width)
        End Using
    End Sub

    <Fact>
    Public Sub TheFloorBeatsMaxWidth()
        Using dv = Grid()
            Dim col = dv.AddColumn("a", "Antet", KBotColumnType.Text, 200)
            col.HeaderLeftIcon = Icon(16)
            col.HeaderRightIcon = Icon(16)
            col.MaxWidth = 20                                  ' plafon imposibil

            Assert.Equal(col.EffectiveMinWidth, col.Width)     ' podeaua câștigă
        End Using
    End Sub

    <Fact>
    Public Sub ShrinkToFit_NeverGoesUnderTheIconFloor()
        Using dv = Grid()
            dv.Size = New Size(200, 300)                       ' mult prea îngust pentru 3×200
            dv.ColumnFillMode = KBotFillMode.Proportional
            For Each k In New String() {"a", "b", "c"}
                Dim c = dv.AddColumn(k, "Antet " & k, KBotColumnType.Text, 200)
                c.HeaderLeftIcon = Icon(16)
                c.HeaderRightIcon = Icon(16)
            Next
            dv.AutoSizeColumns()

            For Each c In dv.Columns
                Assert.True(c.Width >= c.EffectiveMinWidth,
                            $"Coloana «{c.Key}» a coborât la {c.Width}, sub podeaua {c.EffectiveMinWidth}.")
            Next
        End Using
    End Sub

    ' ── Hit-test + eveniment ─────────────────────────────────────────────────────

    <Fact>
    Public Sub RightIconRect_IsEmptyWithoutAnIcon_AndHitTestFindsNothing()
        Using dv = Grid()
            dv.AddColumn("a", "Antet", KBotColumnType.Text, 200)
            Assert.True(dv.DebugHeaderRightIconRect("a").IsEmpty)

            Dim r As Rectangle = Rectangle.Empty
            Assert.Null(dv.HeaderIconTarget(New Point(190, 15), r))
        End Using
    End Sub

    <Fact>
    Public Sub HitTest_FindsTheColumnUnderTheRightIcon()
        Using dv = Grid()
            Dim col = dv.AddColumn("a", "Antet", KBotColumnType.Text, 200)
            col.HeaderRightIcon = Icon(16)
            dv.AddColumn("b", "Alta", KBotColumnType.Text, 200)

            Dim iconRect As Rectangle = dv.DebugHeaderRightIconRect("a")
            Assert.False(iconRect.IsEmpty)

            Dim gasit As Rectangle = Rectangle.Empty
            Dim tinta = dv.HeaderIconTarget(New Point(iconRect.Left + 8, iconRect.Top + 8), gasit)
            Assert.NotNull(tinta)
            Assert.Equal("a", tinta.Key)
            Assert.Equal(iconRect, gasit)

            ' Titlul coloanei nu e pictogramă: acolo nu se apasă nimic.
            Assert.Null(dv.HeaderIconTarget(New Point(20, iconRect.Top + 8), gasit))
        End Using
    End Sub

    <Fact>
    Public Sub Click_RaisesHeaderRightIconClicked_WithKeyAndBounds()
        Using dv = Grid()
            Dim col = dv.AddColumn("a", "Antet", KBotColumnType.Text, 200)
            col.HeaderRightIcon = Icon(16)

            Dim cheie As String = Nothing
            Dim margini As Rectangle = Rectangle.Empty
            AddHandler dv.HeaderRightIconClicked,
                Sub(s, e)
                    cheie = e.ColumnKey
                    margini = e.IconBounds
                End Sub

            Dim iconRect As Rectangle = dv.DebugHeaderRightIconRect("a")
            Assert.True(dv.HandleHeaderIconMouseDown(New Point(iconRect.Left + 8, iconRect.Top + 8)))
            Assert.Equal("a", cheie)
            Assert.Equal(iconRect, margini)

            ' În afara pictogramei apăsarea nu e consumată (rămâne pentru redimensionare/selecție).
            Assert.False(dv.HandleHeaderIconMouseDown(New Point(20, iconRect.Top + 8)))
        End Using
    End Sub

    <Fact>
    Public Sub HoverColor_ComesFromTheThemeUntilTheColumnPinsOne()
        Using dv = Grid()
            Dim col = dv.AddColumn("a", "Antet", KBotColumnType.Text, 200)
            Assert.Equal(Color.Empty, col.HeaderRightIconHoverColor)      ' gol = din temă
            Assert.NotEqual(Color.Empty, dv.HeaderIconHoverResolved(col))

            col.HeaderRightIconHoverColor = Color.Red
            Assert.Equal(Color.Red, dv.HeaderIconHoverResolved(col))
        End Using
    End Sub

    <Fact>
    Public Sub PaintingWithIconsDoesNotThrow()
        Using dv = Grid()
            Dim col = dv.AddColumn("a", "Un antet destul de lung", KBotColumnType.Text, 200)
            col.HeaderLeftIcon = Icon(16)
            col.HeaderRightIcon = Icon(16)
            dv.AddColumn("b", "Alta", KBotColumnType.Text, 60)
            dv.AddRow()

            Using bmp As New Bitmap(dv.Width, dv.Height)
                dv.DrawToBitmap(bmp, New Rectangle(0, 0, dv.Width, dv.Height))
            End Using
        End Using
    End Sub

End Class
