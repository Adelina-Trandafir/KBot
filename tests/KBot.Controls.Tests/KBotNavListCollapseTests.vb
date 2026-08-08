Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Threading
Imports System.Windows.Forms
Imports Xunit
Imports KBot.Theming

''' <summary>
''' English (slice 0025-05): the two capabilities added to <see cref="KBotNavList"/> in this pass —
''' <c>ItemPadding</c> (the air around the buttons, which used to be a hard-coded 6 px margin) and
''' the collapse button in a corner, cycling Expanded → Icons → Complete → Expanded.
'''
''' What these tests CANNOT prove: how any of it LOOKS. The chevron glyph, the hover fill, the
''' centred icon and the first-letter fallback are exercised only through a headless
''' <c>DrawToBitmap</c>, which proves they do not throw — not that they read well on screen. That
''' verdict is still owed (see the worklog).
''' </summary>
Public Class KBotNavListCollapseTests

    Private Shared Sub RunSta(body As Action)
        Dim err As Exception = Nothing
        Dim t As New Thread(Sub()
                                Try
                                    body()
                                Catch ex As Exception
                                    err = ex
                                End Try
                            End Sub)
        t.SetApartmentState(ApartmentState.STA)
        t.Start()
        t.Join()
        If err IsNot Nothing Then Throw New Xunit.Sdk.XunitException(err.ToString())
    End Sub

    Private Shared Function Dpi(nav As KBotNavList, logical As Integer) As Integer
        Return ThemeShapes.ScaleDpi(nav, logical)
    End Function

    Private Shared Function NewSizedList() As KBotNavList
        Dim nav As New KBotNavList()
        nav.Size = New Size(170, 400)
        Return nav
    End Function

    Private Shared Function NewIcon(side As Integer) As Bitmap
        Dim bmp As New Bitmap(side, side)
        Using g = Graphics.FromImage(bmp)
            g.Clear(Color.Magenta)
        End Using
        Return bmp
    End Function

    ' Latura barei pe axa care se strânge, în cele două stări strânse. Aceleași formule ca în
    ' control — scrise cu Dpi() pe fiecare termen, ca să nu pice pe un ecran la 150%.
    Private Shared Function CompleteExtent(nav As KBotNavList) As Integer
        Return Dpi(nav, 18) + 2 * Dpi(nav, 6)
    End Function

    Private Shared Function IconsExtent(nav As KBotNavList) As Integer
        Return nav.ItemPadding.Left + nav.ItemPadding.Right + Dpi(nav, nav.IconSize) + 2 * Dpi(nav, 8)
    End Function

    ' ── ItemPadding ────────────────────────────────────────────────────────────

    <Fact>
    Public Sub ItemPadding_DefaultsToSix_WhichIsExactlyTheOldFixedMargin()
        RunSta(Sub()
                   Using nav = NewSizedList()
                       Assert.Equal(New Padding(6), nav.ItemPadding)
                       nav.AddItem("a", "A", KBotNavAlign.Near)
                       nav.AddItem("z", "Z", KBotNavAlign.Far)

                       Assert.Equal(Dpi(nav, 6), nav.DebugBounds(0).Left)
                       Assert.Equal(Dpi(nav, 6), nav.DebugBounds(0).Top)
                       Assert.Equal(nav.Width - 2 * Dpi(nav, 6), nav.DebugBounds(0).Width)
                       Assert.Equal(nav.Height - Dpi(nav, 6), nav.DebugBounds(1).Bottom)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub ItemPadding_DrivesTheColumnOnAVerticalBar_PerSide()
        RunSta(Sub()
                   Using nav = NewSizedList()
                       nav.AddItem("a", "A", KBotNavAlign.Near)
                       nav.AddItem("z", "Z", KBotNavAlign.Far)
                       nav.ItemPadding = New Padding(10, 20, 4, 30)

                       Assert.Equal(Dpi(nav, 10), nav.DebugBounds(0).Left)
                       Assert.Equal(nav.Width - Dpi(nav, 10) - Dpi(nav, 4), nav.DebugBounds(0).Width)
                       Assert.Equal(Dpi(nav, 20), nav.DebugBounds(0).Top)
                       Assert.Equal(nav.Height - Dpi(nav, 30), nav.DebugBounds(1).Bottom)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub ItemPadding_SwapsRoles_OnAHorizontalBar()
        RunSta(Sub()
                   Using nav = New KBotNavList()
                       nav.Orientation = KBotNavOrientation.Horizontal
                       nav.Size = New Size(600, 80)
                       nav.AddItem("a", "A", KBotNavAlign.Near)
                       nav.AddItem("z", "Z", KBotNavAlign.Far)
                       nav.ItemPadding = New Padding(10, 20, 4, 30)

                       ' Axa principală e X: Left/Right sunt capetele.
                       Assert.Equal(Dpi(nav, 10), nav.DebugBounds(0).Left)
                       Assert.Equal(nav.Width - Dpi(nav, 4), nav.DebugBounds(1).Right)
                       ' Axa transversală e Y: Top/Bottom dau înălțimea butonului.
                       Assert.Equal(Dpi(nav, 20), nav.DebugBounds(0).Top)
                       Assert.Equal(nav.Height - Dpi(nav, 20) - Dpi(nav, 30), nav.DebugBounds(0).Height)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub ItemPadding_ClampsNegativesToZero()
        RunSta(Sub()
                   Using nav = NewSizedList()
                       nav.AddItem("a", "A")
                       nav.ItemPadding = New Padding(-5, -1, 0, 0)
                       Assert.Equal(New Padding(0, 0, 0, 0), nav.ItemPadding)
                       Assert.Equal(0, nav.DebugBounds(0).Left)
                       Assert.Equal(nav.Width, nav.DebugBounds(0).Width)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub TheInheritedPadding_IsHiddenFromTheDesigner()
        ' Control.Padding nu face nimic pe o bară owner-drawn; dacă rămânea în grilă, prima
        ' încercare de a regla aerul din jurul butoanelor mergea pe proprietatea greșită.
        Dim prop As PropertyDescriptor = TypeDescriptor.GetProperties(GetType(KBotNavList))("Padding")
        Assert.NotNull(prop)
        Assert.False(prop.IsBrowsable)
        Assert.Equal(DesignerSerializationVisibility.Hidden, prop.SerializationVisibility)

        Dim itemPad As PropertyDescriptor = TypeDescriptor.GetProperties(GetType(KBotNavList))("ItemPadding")
        Assert.NotNull(itemPad)
        Assert.True(itemPad.IsBrowsable)
    End Sub

    ' ── Butonul din colț ───────────────────────────────────────────────────────

    <Fact>
    Public Sub Collapsible_DefaultsToFalse_AndChangesNothing()
        RunSta(Sub()
                   Using nav = NewSizedList()
                       Assert.False(nav.Collapsible)
                       Assert.Equal(KBotNavCollapseState.Expanded, nav.CollapseState)
                       Assert.Equal(KBotNavCorner.TopRight, nav.CollapseCorner)

                       nav.AddItem("a", "A")
                       Assert.Equal(Rectangle.Empty, nav.DebugCollapseButtonRect())
                       Assert.Equal(Dpi(nav, 6), nav.DebugBounds(0).Top)      ' nicio bandă rezervată
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub TheButton_SitsInTheChosenCorner()
        RunSta(Sub()
                   Using nav = NewSizedList()
                       nav.Collapsible = True
                       Dim side As Integer = Dpi(nav, 18)
                       Dim gap As Integer = Dpi(nav, 6)

                       nav.CollapseCorner = KBotNavCorner.TopLeft
                       Assert.Equal(New Rectangle(gap, gap, side, side), nav.DebugCollapseButtonRect())

                       nav.CollapseCorner = KBotNavCorner.TopRight
                       Assert.Equal(New Rectangle(nav.Width - gap - side, gap, side, side),
                                    nav.DebugCollapseButtonRect())

                       nav.CollapseCorner = KBotNavCorner.BottomLeft
                       Assert.Equal(New Rectangle(gap, nav.Height - gap - side, side, side),
                                    nav.DebugCollapseButtonRect())

                       nav.CollapseCorner = KBotNavCorner.BottomRight
                       Assert.Equal(New Rectangle(nav.Width - gap - side, nav.Height - gap - side, side, side),
                                    nav.DebugCollapseButtonRect())
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub TheButton_ReservesABand_AtTheEndItsCornerIsOn()
        RunSta(Sub()
                   Using nav = NewSizedList()
                       nav.AddItem("a", "A", KBotNavAlign.Near)
                       nav.AddItem("z", "Z", KBotNavAlign.Far)
                       Dim band As Integer = Dpi(nav, 18) + 2 * Dpi(nav, 6)

                       ' Colț de sus => banda e sus: primul buton coboară, cel «Far» stă pe loc.
                       nav.Collapsible = True
                       nav.CollapseCorner = KBotNavCorner.TopRight
                       Assert.Equal(Dpi(nav, 6) + band, nav.DebugBounds(0).Top)
                       Assert.Equal(nav.Height - Dpi(nav, 6), nav.DebugBounds(1).Bottom)

                       ' Colț de jos => banda e jos: exact invers.
                       nav.CollapseCorner = KBotNavCorner.BottomLeft
                       Assert.Equal(Dpi(nav, 6), nav.DebugBounds(0).Top)
                       Assert.Equal(nav.Height - Dpi(nav, 6) - band, nav.DebugBounds(1).Bottom)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub TheButton_ReservesTheBandOnTheMainAxis_OnAHorizontalBarToo()
        RunSta(Sub()
                   Using nav = New KBotNavList()
                       nav.Orientation = KBotNavOrientation.Horizontal
                       nav.Size = New Size(600, 40)
                       nav.AddItem("a", "A")
                       nav.Collapsible = True
                       Dim band As Integer = Dpi(nav, 18) + 2 * Dpi(nav, 6)

                       ' Pe orizontală decid colțurile stânga/dreapta, nu sus/jos.
                       nav.CollapseCorner = KBotNavCorner.TopLeft
                       Assert.Equal(Dpi(nav, 6) + band, nav.DebugBounds(0).Left)

                       nav.CollapseCorner = KBotNavCorner.BottomRight
                       Assert.Equal(Dpi(nav, 6), nav.DebugBounds(0).Left)
                   End Using
               End Sub)
    End Sub

    ' ── Ciclul de strângere ────────────────────────────────────────────────────

    <Fact>
    Public Sub ClickingTheButton_CyclesIconsThenCompleteThenBackToTheInitialSize()
        RunSta(Sub()
                   Using nav = NewSizedList(), icon = NewIcon(16)
                       nav.AddItem("a", "A")
                       nav.Items(0).Image = icon
                       nav.Collapsible = True
                       Dim initial As Integer = nav.Width

                       Dim br As Rectangle = nav.DebugCollapseButtonRect()
                       Dim hit As New Point(br.Left + br.Width \ 2, br.Top + br.Height \ 2)

                       nav.DebugClickAt(hit)
                       Assert.Equal(KBotNavCollapseState.Icons, nav.CollapseState)
                       Assert.Equal(IconsExtent(nav), nav.Width)

                       ' Butonul se mută odată cu bara (colțul e din dreapta) — reluăm punctul.
                       br = nav.DebugCollapseButtonRect()
                       nav.DebugClickAt(New Point(br.Left + br.Width \ 2, br.Top + br.Height \ 2))
                       Assert.Equal(KBotNavCollapseState.Complete, nav.CollapseState)
                       Assert.Equal(CompleteExtent(nav), nav.Width)

                       br = nav.DebugCollapseButtonRect()
                       nav.DebugClickAt(New Point(br.Left + br.Width \ 2, br.Top + br.Height \ 2))
                       Assert.Equal(KBotNavCollapseState.Expanded, nav.CollapseState)
                       Assert.Equal(initial, nav.Width)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub WithoutAnyIcon_TheCycleSkipsTheIconsStep()
        RunSta(Sub()
                   Using nav = NewSizedList()
                       nav.AddItem("a", "A")
                       nav.Collapsible = True
                       Assert.False(nav.IconsCollapseAvailable)

                       nav.ToggleCollapse()
                       Assert.Equal(KBotNavCollapseState.Complete, nav.CollapseState)
                       nav.ToggleCollapse()
                       Assert.Equal(KBotNavCollapseState.Expanded, nav.CollapseState)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub IconsMode_ShrinksTheBarAndTheButtons_ToTheIconWidth()
        RunSta(Sub()
                   Using nav = NewSizedList(), icon = NewIcon(16)
                       nav.AddItem("a", "A")
                       nav.AddItem("b", "B")
                       nav.Items(0).Image = icon
                       nav.Items(1).Image = icon
                       nav.Collapsible = True
                       nav.CollapseState = KBotNavCollapseState.Icons

                       Assert.Equal(IconsExtent(nav), nav.Width)
                       Dim expected As Integer = Dpi(nav, 20) + 2 * Dpi(nav, 8)
                       For i As Integer = 0 To 1
                           Assert.Equal(expected, nav.DebugBounds(i).Width)
                       Next

                       ' Pictograma se centrează — nu mai are text lângă care să stea.
                       Dim r As Rectangle = nav.DebugBounds(0)
                       Dim ir As Rectangle = nav.DebugIconRect(0)
                       Assert.Equal(r.Left + (r.Width - ir.Width) \ 2, ir.Left)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub IconsMode_OverridesAutoSizeAndItemWidth()
        ' Bara S-A strâns la lățimea unei pictograme: nicio cerere de lățime nu mai are ce cere.
        RunSta(Sub()
                   Using nav = NewSizedList(), icon = NewIcon(16)
                       nav.AddItem("a", "Un text considerabil mai lung")
                       nav.AddItem("b", "B")
                       nav.Items(0).Image = icon
                       nav.Items(0).AutoSize = True
                       nav.ItemWidth = 140
                       nav.Collapsible = True
                       nav.CollapseState = KBotNavCollapseState.Icons

                       Dim expected As Integer = Dpi(nav, 20) + 2 * Dpi(nav, 8)
                       Assert.Equal(expected, nav.DebugBounds(0).Width)
                       Assert.Equal(expected, nav.DebugBounds(1).Width)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub CompleteMode_LeavesNoSlots_SoNothingCanBeClicked()
        RunSta(Sub()
                   Using nav = NewSizedList()
                       nav.AddItem("a", "A")
                       nav.AddItem("b", "B")
                       nav.Collapsible = True
                       Dim centre As New Point(nav.Width \ 2, nav.Height \ 2)

                       nav.CollapseState = KBotNavCollapseState.Complete
                       Assert.Equal(CompleteExtent(nav), nav.Width)
                       Assert.Equal(Rectangle.Empty, nav.DebugBounds(0))
                       Assert.Equal(Rectangle.Empty, nav.DebugBounds(1))
                       Assert.Equal(-1, nav.DebugIndexAt(centre))
                       Assert.Equal(-1, nav.DebugIndexAt(New Point(2, 2)))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Expanding_RestoresTheSizeTheOperatorHadSet_NotTheOneFromTheConstructor()
        RunSta(Sub()
                   Using nav = NewSizedList()
                       nav.AddItem("a", "A")
                       nav.Collapsible = True
                       nav.Width = 240                       ' operatorul a lățit bara

                       nav.ToggleCollapse()                  ' Complete (n-are pictograme)
                       Assert.Equal(CompleteExtent(nav), nav.Width)
                       nav.ToggleCollapse()                  ' înapoi
                       Assert.Equal(240, nav.Width)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub AHorizontalBar_CollapsesItsHeight_NotItsWidth()
        RunSta(Sub()
                   Using nav = New KBotNavList()
                       nav.Orientation = KBotNavOrientation.Horizontal
                       nav.Size = New Size(600, 40)
                       nav.AddItem("a", "A")
                       nav.Collapsible = True

                       nav.ToggleCollapse()
                       Assert.Equal(KBotNavCollapseState.Complete, nav.CollapseState)
                       Assert.Equal(CompleteExtent(nav), nav.Height)
                       Assert.Equal(600, nav.Width)

                       nav.ToggleCollapse()
                       Assert.Equal(40, nav.Height)
                   End Using
               End Sub)
    End Sub

    ' ── Stări imposibile ───────────────────────────────────────────────────────

    <Fact>
    Public Sub CollapseState_ThrowsWhenTheBarIsNotCollapsible()
        RunSta(Sub()
                   Using nav = NewSizedList()
                       nav.AddItem("a", "A")
                       Assert.Throws(Of InvalidOperationException)(
                           Sub() nav.CollapseState = KBotNavCollapseState.Complete)
                       Assert.Throws(Of InvalidOperationException)(Sub() nav.ToggleCollapse())
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub CollapseState_ThrowsForIconsWhenNoButtonHasAnIcon()
        ' Setter-ul aruncă (regula casei: fără no-op-uri tăcute); butonul din colț, în schimb,
        ' sare starea — vezi WithoutAnyIcon_TheCycleSkipsTheIconsStep.
        RunSta(Sub()
                   Using nav = NewSizedList()
                       nav.AddItem("a", "A")
                       nav.Collapsible = True
                       Assert.Throws(Of InvalidOperationException)(
                           Sub() nav.CollapseState = KBotNavCollapseState.Icons)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub IconsMode_IsNeverAvailableOnAHorizontalBar_NorWithIconSizeZero()
        RunSta(Sub()
                   Using nav = NewSizedList(), icon = NewIcon(16)
                       nav.AddItem("a", "A")
                       nav.Items(0).Image = icon
                       Assert.True(nav.IconsCollapseAvailable)

                       nav.IconSize = 0
                       Assert.False(nav.IconsCollapseAvailable)
                       nav.IconSize = 20

                       nav.Orientation = KBotNavOrientation.Horizontal
                       Assert.False(nav.IconsCollapseAvailable)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub AHiddenButtonsIcon_DoesNotUnlockIconsMode()
        RunSta(Sub()
                   Using nav = NewSizedList(), icon = NewIcon(16)
                       nav.AddItem("a", "A")
                       nav.Items(0).Image = icon
                       nav.SetItemVisible("a", False)
                       Assert.False(nav.IconsCollapseAvailable)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub TurningTheAxis_WhileCollapsed_DowngradesIconsAndRestoresTheOldAxis()
        RunSta(Sub()
                   Using nav = NewSizedList(), icon = NewIcon(16)
                       nav.AddItem("a", "A")
                       nav.Items(0).Image = icon
                       nav.Collapsible = True
                       nav.CollapseState = KBotNavCollapseState.Icons

                       nav.Orientation = KBotNavOrientation.Horizontal
                       ' «Icons» nu există pe orizontală.
                       Assert.Equal(KBotNavCollapseState.Complete, nav.CollapseState)
                       ' Lățimea (axa veche) s-a întors la mărimea inițială…
                       Assert.Equal(170, nav.Width)
                       ' …iar înălțimea (axa nouă) s-a strâns.
                       Assert.Equal(CompleteExtent(nav), nav.Height)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Collapsible_TurnedOff_ExpandsImmediately()
        RunSta(Sub()
                   Using nav = NewSizedList()
                       nav.AddItem("a", "A")
                       nav.Collapsible = True
                       nav.CollapseState = KBotNavCollapseState.Complete

                       nav.Collapsible = False
                       Assert.Equal(KBotNavCollapseState.Expanded, nav.CollapseState)
                       Assert.Equal(170, nav.Width)
                       Assert.Equal(Rectangle.Empty, nav.DebugCollapseButtonRect())
                   End Using
               End Sub)
    End Sub

    ' ── Evenimentul ────────────────────────────────────────────────────────────

    <Fact>
    Public Sub CollapseStateChanged_FiresOncePerStep_AndNotForANoOp()
        RunSta(Sub()
                   Using nav = NewSizedList(), icon = NewIcon(16)
                       nav.AddItem("a", "A")
                       nav.Items(0).Image = icon
                       nav.Collapsible = True

                       Dim seen As New List(Of KBotNavCollapseState)()
                       AddHandler nav.CollapseStateChanged, Sub(s As KBotNavCollapseState) seen.Add(s)

                       nav.ToggleCollapse()
                       nav.ToggleCollapse()
                       nav.ToggleCollapse()
                       nav.CollapseState = KBotNavCollapseState.Expanded     ' deja acolo

                       Assert.Equal(New KBotNavCollapseState() {KBotNavCollapseState.Icons,
                                                                KBotNavCollapseState.Complete,
                                                                KBotNavCollapseState.Expanded},
                                    seen.ToArray())
                   End Using
               End Sub)
    End Sub

    ' ── Pictare (doar «nu aruncă») ─────────────────────────────────────────────

    <Fact>
    Public Sub PaintingEveryCollapsedState_DoesNotThrow()
        RunSta(Sub()
                   Using icon = NewIcon(24)
                       For Each corner As KBotNavCorner In {KBotNavCorner.TopLeft, KBotNavCorner.TopRight,
                                                            KBotNavCorner.BottomLeft, KBotNavCorner.BottomRight}
                           Using nav = NewSizedList()
                               nav.AddItem("a", "A")
                               nav.AddItem("b", "B")            ' fără pictogramă => cade pe inițială
                               nav.AddSeparator()
                               nav.Items(0).Image = icon
                               nav.SetBadge("a", 4)
                               nav.SelectedKey = "a"
                               nav.Collapsible = True
                               nav.CollapseCorner = corner

                               For Each state As KBotNavCollapseState In {KBotNavCollapseState.Icons,
                                                                          KBotNavCollapseState.Complete,
                                                                          KBotNavCollapseState.Expanded}
                                   nav.CollapseState = state
                                   Using bmp As New Bitmap(Math.Max(1, nav.Width), Math.Max(1, nav.Height))
                                       nav.DrawToBitmap(bmp, New Rectangle(0, 0, bmp.Width, bmp.Height))
                                   End Using
                               Next
                           End Using
                       Next
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub PaintingAHorizontalCollapsedBar_DoesNotThrow()
        RunSta(Sub()
                   Using nav = New KBotNavList()
                       nav.Orientation = KBotNavOrientation.Horizontal
                       nav.Size = New Size(600, 40)
                       nav.AddItem("a", "A")
                       nav.Collapsible = True
                       nav.CollapseState = KBotNavCollapseState.Complete
                       Using bmp As New Bitmap(nav.Width, Math.Max(1, nav.Height))
                           nav.DrawToBitmap(bmp, New Rectangle(0, 0, bmp.Width, bmp.Height))
                       End Using
                   End Using
               End Sub)
    End Sub

End Class
