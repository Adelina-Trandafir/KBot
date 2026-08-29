Option Strict On
Imports Xunit
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' Slice 0049: a scheme may ask for taller rows, but only for as long as it is the active one.
'''
''' <para><b>The trap this is written against.</b> Both <c>ItemHeight</c> and <c>RowHeight</c> are
''' ordinary designer properties, so a naive «apply the scheme's number» would write straight into
''' them — and the control would then remember that number as the operator's own choice. One trip
''' through «Modern» would leave every grid on 40px rows for good, including under Classic, and
''' the only way back would be editing a designer file. Hence the authored value and the return
''' path; these tests are the proof that the return path exists.</para>
'''
''' <para>Controls are built but never shown — no window reaches the screen.</para>
''' </summary>
Public Class SchemeRowHeightTests

    ' ── AdvancedTreeControl ───────────────────────────────────────────────────

    <Fact>
    Public Sub Tree_TakesTheSchemeRowHeight_AndGivesItBack()
        Using tree As New AdvancedTreeControl()
            tree.ItemHeight = 24                       ' as MainForm's designer authors it
            Assert.Equal(24, tree.ItemHeight)

            tree.ApplyTheme(BuiltInSchemes.Modern())
            Assert.Equal(BuiltInSchemes.Modern().Style.ListRowHeight, tree.ItemHeight)

            tree.ApplyTheme(BuiltInSchemes.Classic())
            Assert.Equal(24, tree.ItemHeight)
        End Using
    End Sub

    <Fact>
    Public Sub Tree_NeutralSchemesLeaveTheAuthoredHeightAlone()
        For Each s As ThemeScheme In New ThemeScheme() {BuiltInSchemes.Classic(),
                                                        BuiltInSchemes.Dark(),
                                                        BuiltInSchemes.Colorful()}
            Using tree As New AdvancedTreeControl()
                tree.ItemHeight = 24
                tree.ApplyTheme(s)
                Assert.Equal(24, tree.ItemHeight)
            End Using
        Next
    End Sub

    ''' <summary>
    ''' Three round trips. If the scheme's height ever leaked into the authored value, the second
    ''' Classic would answer 34 rather than 24 and this would catch it.
    ''' </summary>
    <Fact>
    Public Sub Tree_SurvivesRepeatedSchemeSwitching()
        Using tree As New AdvancedTreeControl()
            tree.ItemHeight = 24
            For i As Integer = 1 To 3
                tree.ApplyTheme(BuiltInSchemes.Modern())
                Assert.Equal(BuiltInSchemes.Modern().Style.ListRowHeight, tree.ItemHeight)
                tree.ApplyTheme(BuiltInSchemes.Classic())
                Assert.Equal(24, tree.ItemHeight)
            Next
        End Using
    End Sub

    ''' <summary>A height set by the operator WHILE Modern is active is still theirs afterwards.</summary>
    <Fact>
    Public Sub Tree_AnOperatorChoiceUnderModern_BecomesTheNewReturnPoint()
        Using tree As New AdvancedTreeControl()
            tree.ItemHeight = 24
            tree.ApplyTheme(BuiltInSchemes.Modern())
            tree.ItemHeight = 30                       ' the operator, not the theme
            tree.ApplyTheme(BuiltInSchemes.Classic())
            Assert.Equal(30, tree.ItemHeight)
        End Using
    End Sub

    ' ── KBotDataView ──────────────────────────────────────────────────────────

    <Fact>
    Public Sub Grid_TakesTheSchemeHeights_AndGivesThemBack()
        Using grid As New KBotDataView()
            grid.RowHeight = 28
            grid.HeaderHeight = 30

            Dim modern As ThemeScheme = BuiltInSchemes.Modern()
            grid.ApplyTheme(modern)
            Assert.Equal(modern.Style.GridRowHeight, grid.RowHeight)
            Assert.Equal(modern.Style.GridHeaderHeight, grid.HeaderHeight)

            grid.ApplyTheme(BuiltInSchemes.Dark())
            Assert.Equal(28, grid.RowHeight)
            Assert.Equal(30, grid.HeaderHeight)
        End Using
    End Sub

    <Fact>
    Public Sub Grid_NeutralSchemesLeaveTheAuthoredHeightsAlone()
        For Each s As ThemeScheme In New ThemeScheme() {BuiltInSchemes.Classic(),
                                                        BuiltInSchemes.Dark(),
                                                        BuiltInSchemes.Colorful()}
            Using grid As New KBotDataView()
                grid.RowHeight = 26
                grid.HeaderHeight = 32
                grid.ApplyTheme(s)
                Assert.Equal(26, grid.RowHeight)
                Assert.Equal(32, grid.HeaderHeight)
            End Using
        Next
    End Sub

    ' ── KBotNavList ───────────────────────────────────────────────────────────

    ''' <summary>
    ''' The nav bar has no public item-height property, so the scheme is the only place the number
    ''' can come from. The visible effect is the bar's preferred size, which is what is checked.
    ''' </summary>
    <Fact>
    Public Sub NavList_ItemHeightFollowsTheScheme()
        Using nav As New KBotNavList()
            nav.Orientation = KBotNavOrientation.Vertical
            nav.AddItem("a", "Unu")
            nav.AddItem("b", "Doi")

            nav.ApplyTheme(BuiltInSchemes.Classic())
            nav.DebugEnsureLayout()
            Dim clasic As Integer = nav.Items(0).Bounds.Height

            nav.ApplyTheme(BuiltInSchemes.Modern())
            nav.DebugEnsureLayout()
            Dim modern As Integer = nav.Items(0).Bounds.Height

            Assert.True(modern > clasic,
                        $"Modern must ask for taller items: classic={clasic}, modern={modern}")

            ' … and going back gives the historic height back, not a sticky one.
            nav.ApplyTheme(BuiltInSchemes.Classic())
            nav.DebugEnsureLayout()
            Assert.Equal(clasic, nav.Items(0).Bounds.Height)
        End Using
    End Sub

End Class
