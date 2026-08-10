Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Theming
Imports Xunit

''' <summary>
''' Slice 0028: butonul de strângere al grilei — a treia apariție a piesei, după
''' <c>KBotNavList</c> și subsolul arborelui, deci verificată pe aceleași reguli:
''' <list type="bullet">
''' <item>starea se comută doar prin buton (fără buton, setterul ARUNCĂ — n-ar mai exista cale
''' de întoarcere);</item>
''' <item>gazda care ține dimensiunea nu e călcată: <c>Dock</c>/ancorare pe amândouă laturile =>
''' nu se scrie <c>Width</c>/<c>Height</c>, se ridică doar evenimentul;</item>
''' <item>lățimea desfășurată se ține minte, ca desfacerea să aibă unde se întoarce;</item>
''' <item>pe axa verticală corpul chiar DISPARE (zero rânduri pictate), rămân cele două benzi.</item>
''' </list>
''' </summary>
Public Class KBotDataViewCollapseTests

    Private Shared Function Grid() As KBotDataView
        Dim dv As New KBotDataView()
        dv.Size = New Size(500, 300)
        dv.ApplyTheme(BuiltInSchemes.Classic())
        dv.AddColumn("cod", "Cod", KBotColumnType.Text, 120)
        dv.FooterVisible = True
        dv.CollapseButton = True
        For i As Integer = 1 To 40
            dv.AddRow()("cod") = "R" & i.ToString()
        Next
        Return dv
    End Function

    Private Shared Sub PaintOnce(dv As KBotDataView)
        Using bmp As New Bitmap(dv.Width, dv.Height)
            dv.DrawToBitmap(bmp, New Rectangle(0, 0, dv.Width, dv.Height))
        End Using
    End Sub

    ' ── Contractul stării ────────────────────────────────────────────────────────

    <Fact>
    Public Sub Collapse_WithoutTheButton_Throws()
        Using dv As New KBotDataView()
            Assert.False(dv.CollapseButton)
            Assert.Throws(Of InvalidOperationException)(Sub() dv.Collapsed = True)
            Assert.False(dv.Collapsed)
        End Using
    End Sub

    <Fact>
    Public Sub ToggleCollapse_WithoutTheButton_IsANoOp_NotAThrow()
        ' ToggleCollapse e apăsarea unui buton, nu o cerere din cod: fără buton nu are ce apăsa.
        Using dv As New KBotDataView()
            dv.ToggleCollapse()
            Assert.False(dv.Collapsed)
        End Using
    End Sub

    <Fact>
    Public Sub TurningTheButtonOff_UnfoldsTheGrid_SoItCannotStayStuck()
        Using dv = Grid()
            dv.ToggleCollapse()
            Assert.True(dv.Collapsed)
            dv.CollapseButton = False
            Assert.False(dv.Collapsed)          ' altfel ar rămâne strânsă pentru totdeauna
        End Using
    End Sub

    <Fact>
    Public Sub CollapsedChanged_FiresOncePerRealChange()
        Using dv = Grid()
            Dim stari As New List(Of Boolean)()
            AddHandler dv.CollapsedChanged, Sub(c As Boolean) stari.Add(c)
            dv.ToggleCollapse()
            dv.ToggleCollapse()
            dv.Collapsed = False                ' deja desfăcută: nicio ridicare
            Assert.Equal(New Boolean() {True, False}, stari.ToArray())
        End Using
    End Sub

    ' ── Axa orizontală (contractul arborelui) ────────────────────────────────────

    <Fact>
    Public Sub Horizontal_CollapseNarrowsToMinimum_AndRestoresTheExpandedWidth()
        Using dv = Grid()
            dv.MinimumCollapsedWidth = 100
            Dim latimeInitiala As Integer = dv.Width

            dv.ToggleCollapse()
            Assert.Equal(100, dv.Width)
            Assert.Equal(latimeInitiala, dv.ExpandedWidth)

            dv.ToggleCollapse()
            Assert.Equal(latimeInitiala, dv.Width)
        End Using
    End Sub

    <Fact>
    Public Sub Horizontal_DockedGrid_KeepsItsWidth_AndOnlyRaisesTheEvent()
        ' Regula gazdei: într-un părinte care face layout, un Width scris de noi ține până la
        ' următoarea trecere, care îl dă înapoi — adică pâlpâie. Gazda mută splitter-ul.
        Using host As New Panel()
            host.Size = New Size(400, 300)
            Using dv = Grid()
                dv.Dock = DockStyle.Fill
                host.Controls.Add(dv)
                Assert.True(dv.HostOwnsWidth)

                Dim ridicat As Boolean = False
                AddHandler dv.CollapsedChanged, Sub(c As Boolean) ridicat = True
                Dim latime As Integer = dv.Width
                dv.ToggleCollapse()

                Assert.True(dv.Collapsed)
                Assert.True(ridicat)
                Assert.Equal(latime, dv.Width)      ' nu ne batem cu layout-ul gazdei
            End Using
        End Using
    End Sub

    <Fact>
    Public Sub Horizontal_ResizeWhileCollapsed_DoesNotBecomeTheNewExpandedWidth()
        Using dv = Grid()
            Dim latimeInitiala As Integer = dv.Width
            dv.ToggleCollapse()
            dv.Width = 130                       ' cineva o mai îngustează cât e strânsă
            dv.ToggleCollapse()
            Assert.Equal(latimeInitiala, dv.Width)
        End Using
    End Sub

    ' ── Axa verticală ────────────────────────────────────────────────────────────

    <Fact>
    Public Sub Vertical_CollapseLeavesOnlyTheTwoBands_AndPaintsNoRows()
        Using dv = Grid()
            dv.CollapseDirection = KBotCollapseDirection.Vertical
            PaintOnce(dv)
            Assert.True(dv.DebugLastPaintedDataRows > 0)

            dv.ToggleCollapse()
            Assert.Equal(dv.HeaderHeight + dv.FooterHeight, dv.CollapsedHeight)
            Assert.Equal(dv.CollapsedHeight, dv.Height)

            PaintOnce(dv)
            Assert.Equal(0, dv.DebugLastPaintedDataRows)   ' corpul nu e ascuns, ci absent
            Assert.False(dv.vScroll.Visible)               ' n-a rămas nicio bară atârnată
            Assert.False(dv.hScroll.Visible)
        End Using
    End Sub

    <Fact>
    Public Sub Vertical_RestoresTheExpandedHeight()
        Using dv = Grid()
            dv.CollapseDirection = KBotCollapseDirection.Vertical
            Dim inaltimeInitiala As Integer = dv.Height
            dv.ToggleCollapse()
            Assert.True(dv.Height < inaltimeInitiala)
            dv.ToggleCollapse()
            Assert.Equal(inaltimeInitiala, dv.Height)
        End Using
    End Sub

    <Fact>
    Public Sub ChangingTheAxisWhileCollapsed_DoesNotLeaveTheGridFoldedOnTheOldAxis()
        Using dv = Grid()
            Dim latime As Integer = dv.Width
            Dim inaltime As Integer = dv.Height
            dv.ToggleCollapse()                            ' strânsă pe orizontală
            Assert.Equal(dv.MinimumCollapsedWidth, dv.Width)

            dv.CollapseDirection = KBotCollapseDirection.Vertical

            Assert.True(dv.Collapsed)                      ' rămâne strânsă, dar pe noua axă
            Assert.Equal(latime, dv.Width)                 ' lățimea s-a întors
            Assert.Equal(dv.CollapsedHeight, dv.Height)
            dv.ToggleCollapse()
            Assert.Equal(inaltime, dv.Height)
        End Using
    End Sub

    ' ── Butonul: geometrie și apăsare ────────────────────────────────────────────

    <Fact>
    Public Sub ButtonRect_IsEmptyWithoutAFooterBand()
        Using dv = Grid()
            dv.FooterVisible = False
            Assert.True(dv.CollapseButtonRect.IsEmpty)     ' butonul locuiește în subsol
        End Using
    End Sub

    <Fact>
    Public Sub ButtonRect_SitsInsideTheFooterBand_OnTheChosenSide()
        Using dv = Grid()
            Dim banda As Integer = dv.Height - dv.FooterHeight
            Dim r As Rectangle = dv.CollapseButtonRect
            Assert.False(r.IsEmpty)
            Assert.True(r.Top >= banda, "butonul a ieșit din banda de subsol")
            Assert.True(r.Right <= dv.Width)
            Assert.True(r.Left > dv.Width \ 2, "implicit butonul stă în dreapta")

            dv.CollapseButtonPosition = KBotFooterButtonPosition.Left
            r = dv.CollapseButtonRect
            Assert.True(r.Left < dv.Width \ 2, "mutat pe stânga, butonul a rămas în dreapta")
        End Using
    End Sub

    <Fact>
    Public Sub FooterContentRect_GivesTheButtonsCornerAway()
        ' Latura pe care stă butonul e a lui: textul agregatelor se decupează înaintea lui,
        ' altfel o sumă lungă ar curge pe sub buton.
        Using dv = Grid()
            Dim banda As New Rectangle(0, dv.Height - dv.FooterHeight, dv.Width, dv.FooterHeight)
            Dim continut As Rectangle = dv.FooterContentRect(banda)
            Assert.True(continut.Right < dv.CollapseButtonRect.Left)

            dv.CollapseButtonPosition = KBotFooterButtonPosition.Left
            continut = dv.FooterContentRect(banda)
            Assert.True(continut.Left > dv.CollapseButtonRect.Right)
        End Using
    End Sub

    <Fact>
    Public Sub ClickingTheButton_Toggles_AndTheFooterBandNeverSelectsARow()
        Using dv = Grid()
            Dim r As Rectangle = dv.CollapseButtonRect
            Dim mijloc As New Point(r.Left + r.Width \ 2, r.Top + r.Height \ 2)

            Assert.True(dv.HandleFooterMouseDown(mijloc))
            Assert.True(dv.Collapsed)

            ' Apăsarea în bandă, dar lângă buton: banda o consumă (nu e un rând), fără să comute.
            Dim langa As New Point(4, r.Top + r.Height \ 2)
            Assert.True(dv.HandleFooterMouseDown(langa))
            Assert.True(dv.Collapsed)

            ' Deasupra benzii: subsolul nu mai are treabă cu evenimentul.
            Assert.False(dv.HandleFooterMouseDown(New Point(4, 4)))
        End Using
    End Sub

End Class
