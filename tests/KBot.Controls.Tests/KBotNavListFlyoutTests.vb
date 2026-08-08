Option Strict On
Imports System.Drawing
Imports System.Threading
Imports System.Windows.Forms
Imports Xunit
Imports KBot.Theming

''' <summary>
''' English (slice 0025-07): the floating label that slides out of a COLLAPSED
''' <see cref="KBotNavList"/> when the cursor rests on a button — the replacement for a stock
''' (yellow, unthemeable) <c>ToolTip</c>.
'''
''' The window itself (<c>KBotNavFlyout</c>) is never SHOWN here: <c>RenderFlyout</c> backs out
''' when the bar has no handle and no parent form, which is exactly the headless case. What these
''' tests pin is everything that decides the window's existence and shape — who gets a label, when
''' the timer arms, how far the slide has travelled, and the rectangle at every progress. Painting
''' is exercised through a detached window + <c>DrawToBitmap</c>, which proves «does not throw»,
''' not «looks right».
'''
''' What is NOT proven and cannot be, here: that the window appears where the bar thinks it does,
''' that it never steals focus, and that <c>HTTRANSPARENT</c> really lets the mouse fall through to
''' the button underneath. All three are on-screen verdicts (see the worklog).
''' </summary>
Public Class KBotNavListFlyoutTests

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

    Private Shared Function NewIcon(side As Integer) As Bitmap
        Dim bmp As New Bitmap(side, side)
        Using g = Graphics.FromImage(bmp)
            g.Clear(Color.Magenta)
        End Using
        Return bmp
    End Function

    Private Shared Function Centre(r As Rectangle) As Point
        Return New Point(r.Left + r.Width \ 2, r.Top + r.Height \ 2)
    End Function

    ''' <summary>
    ''' O bară strânsă la pictograme, cu trei butoane („a" cu pictogramă, „b" fără, „c" fără text)
    ''' și un separator. Pictograma se lasă în seama apelantului (Using), ca la celelalte teste.
    ''' </summary>
    Private Shared Function NewCollapsedBar(icon As Bitmap) As KBotNavList
        Dim nav As New KBotNavList()
        nav.Size = New Size(170, 400)
        nav.AddItem("a", "Sumar")
        nav.AddItem("b", "Istoric")
        nav.AddSeparator()
        nav.AddItem("c", "")
        nav.Items(0).Image = icon
        nav.Items(1).Image = icon
        nav.Collapsible = True
        nav.CollapseState = KBotNavCollapseState.Icons
        Return nav
    End Function

    ' ── Cine primește etichetă ─────────────────────────────────────────────────

    <Fact>
    Public Sub Defaults_AreOnWithASmallDelayAndASmallSlide()
        RunSta(Sub()
                   Using nav = New KBotNavList()
                       Assert.True(nav.CollapsedFlyout)
                       Assert.Equal(250, nav.FlyoutDelay)
                       Assert.Equal(120, nav.FlyoutSlideDuration)
                       Assert.Equal(-1, nav.DebugFlyoutIndex())
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub AnExpandedBar_NeverShowsALabel()
        ' Butoanele își scriu textul singure — o etichetă peste ele ar fi zgomot.
        RunSta(Sub()
                   Using icon = NewIcon(16), nav = NewCollapsedBar(icon)
                       Dim hit As Point = Centre(nav.DebugBounds(0))
                       Assert.Equal(0, nav.DebugFlyoutTargetAt(hit))

                       nav.CollapseState = KBotNavCollapseState.Expanded
                       Assert.Equal(-1, nav.DebugFlyoutTargetAt(Centre(nav.DebugBounds(0))))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub ASeparator_NeverGetsALabel()
        RunSta(Sub()
                   Using icon = NewIcon(16), nav = NewCollapsedBar(icon)
                       Dim sep As Rectangle = nav.DebugBounds(2)
                       Assert.False(sep.IsEmpty)
                       Assert.Equal(-1, nav.DebugFlyoutTargetAt(Centre(sep)))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub AButtonWithoutText_GetsNoLabel_BecauseThereIsNothingToReveal()
        RunSta(Sub()
                   Using icon = NewIcon(16), nav = NewCollapsedBar(icon)
                       Assert.Equal(-1, nav.DebugFlyoutTargetAt(Centre(nav.DebugBounds(3))))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub AHiddenButton_GetsNoLabel()
        RunSta(Sub()
                   Using icon = NewIcon(16), nav = NewCollapsedBar(icon)
                       Dim before As Rectangle = nav.DebugBounds(1)
                       nav.SetItemVisible("b", False)
                       Assert.Equal(Rectangle.Empty, nav.DebugBounds(1))
                       ' Slotul lui a rămas gol și l-au ocupat separatorul + butonul fără text,
                       ' care nu primesc etichetă niciunul.
                       Assert.Equal(-1, nav.DebugFlyoutTargetAt(Centre(before)))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub ADisabledButton_DOES_GetALabel_UnlikeHover()
        ' Hover-ul obișnuit sare butoanele stinse (nu se pot apăsa). Eticheta nu: tocmai pe o bară
        ' strânsă e cel mai greu de ghicit ce e butonul stins de sub cursor.
        RunSta(Sub()
                   Using icon = NewIcon(16), nav = NewCollapsedBar(icon)
                       nav.SetItemEnabled("b", False)
                       Dim hit As Point = Centre(nav.DebugBounds(1))
                       Assert.Equal(1, nav.DebugFlyoutTargetAt(hit))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub TurningTheLabelOff_StopsIt_AndRetractsWhatIsOut()
        RunSta(Sub()
                   Using icon = NewIcon(16), nav = NewCollapsedBar(icon)
                       nav.FlyoutDelay = 0
                       nav.DebugMouseMoveTo(Centre(nav.DebugBounds(0)))
                       Assert.Equal(0, nav.DebugFlyoutIndex())

                       nav.CollapsedFlyout = False
                       Assert.Equal(-1, nav.DebugFlyoutIndex())
                       Assert.Equal(-1, nav.DebugFlyoutTargetAt(Centre(nav.DebugBounds(0))))
                   End Using
               End Sub)
    End Sub

    ' ── Geometria: pleacă din buton și crește DOAR spre dreapta ────────────────

    <Fact>
    Public Sub AtZeroProgress_TheLabelIsExactlyTheCollapsedButton()
        ' Ăsta e trucul întregii felii: la progres 0 eticheta e nedeosebită de buton, deci
        ' desfășurarea pare a butonului, nu a unei note lipite alături.
        RunSta(Sub()
                   Using icon = NewIcon(16), nav = NewCollapsedBar(icon)
                       Assert.Equal(nav.DebugBounds(0), nav.DebugFlyoutClientBounds(0, 0.0))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub TheLabel_GrowsOnlyRightwards_AndKeepsTheButtonsRowExactly()
        RunSta(Sub()
                   Using icon = NewIcon(16), nav = NewCollapsedBar(icon)
                       Dim btn As Rectangle = nav.DebugBounds(0)
                       Dim full As Integer = nav.DebugFlyoutFullWidth(0)

                       For Each p As Double In {0.0, 0.25, 0.5, 1.0}
                           Dim r As Rectangle = nav.DebugFlyoutClientBounds(0, p)
                           Assert.Equal(btn.Left, r.Left)          ' stânga NU se mișcă
                           Assert.Equal(btn.Top, r.Top)
                           Assert.Equal(btn.Height, r.Height)      ' e rândul butonului, nu o casetă
                           Assert.InRange(r.Width, btn.Width, full)
                       Next

                       Assert.Equal(btn.Width, nav.DebugFlyoutClientBounds(0, 0.0).Width)
                       Assert.Equal(full, nav.DebugFlyoutClientBounds(0, 1.0).Width)
                       Assert.True(full > btn.Width, "eticheta completă trebuie să fie mai lată decât butonul strâns")
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub TheFullWidth_IsTheRailPlusTheCaption_PlusTheBadgeWhenThereIsOne()
        RunSta(Sub()
                   Using icon = NewIcon(16), nav = NewCollapsedBar(icon)
                       Dim rail As Integer = nav.DebugBounds(0).Width
                       Dim text As Integer = TextRenderer.MeasureText("Sumar", New Font("Segoe UI Semibold", nav.Font.Size)).Width
                       Assert.Equal(rail + text + Dpi(nav, 12), nav.DebugFlyoutFullWidth(0))

                       nav.SetBadge("a", 7)
                       Assert.Equal(rail + text + Dpi(nav, 12) + Dpi(nav, 26), nav.DebugFlyoutFullWidth(0))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub TheProgressIsClamped_SoAStrayValueCannotOverrunTheLabel()
        RunSta(Sub()
                   Using icon = NewIcon(16), nav = NewCollapsedBar(icon)
                       Assert.Equal(nav.DebugFlyoutClientBounds(0, 0.0), nav.DebugFlyoutClientBounds(0, -3.0))
                       Assert.Equal(nav.DebugFlyoutClientBounds(0, 1.0), nav.DebugFlyoutClientBounds(0, 9.0))
                   End Using
               End Sub)
    End Sub

    ' ── Cronometrul: întâi așteaptă, apoi se desfășoară ────────────────────────

    <Fact>
    Public Sub Hovering_ArmsTheDelay_AndOnlyThenSlides()
        RunSta(Sub()
                   Using icon = NewIcon(16), nav = NewCollapsedBar(icon)
                       nav.DebugMouseMoveTo(Centre(nav.DebugBounds(0)))
                       Assert.Equal(0, nav.DebugFlyoutIndex())
                       Assert.Equal(0.0, nav.DebugFlyoutProgress())

                       nav.DebugFlyoutFireDelay()
                       Assert.Equal(0.0, nav.DebugFlyoutProgress())   ' desfășurarea începe de la buton

                       nav.DebugFlyoutTick()
                       Assert.True(nav.DebugFlyoutProgress() > 0.0)
                       Assert.True(nav.DebugFlyoutProgress() < 1.0)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub TheSlide_ReachesExactlyOne_AndStopsThere()
        RunSta(Sub()
                   Using icon = NewIcon(16), nav = NewCollapsedBar(icon)
                       nav.FlyoutDelay = 0
                       nav.DebugMouseMoveTo(Centre(nav.DebugBounds(0)))

                       ' 120 ms / 15 ms pe pas = 8 pași; mai batem câțiva ca să se vadă că nu trece de 1.
                       For i As Integer = 1 To 20
                           nav.DebugFlyoutTick()
                       Next
                       Assert.Equal(1.0, nav.DebugFlyoutProgress())
                       Assert.Equal(0, nav.DebugFlyoutIndex())
                       Assert.Equal(nav.DebugFlyoutFullWidth(0), nav.DebugFlyoutClientBounds(0, nav.DebugFlyoutProgress()).Width)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub ZeroDelay_StartsTheSlideOnTheMoveItself()
        RunSta(Sub()
                   Using icon = NewIcon(16), nav = NewCollapsedBar(icon)
                       nav.FlyoutDelay = 0
                       nav.DebugMouseMoveTo(Centre(nav.DebugBounds(1)))
                       Assert.Equal(1, nav.DebugFlyoutIndex())
                       nav.DebugFlyoutTick()
                       Assert.True(nav.DebugFlyoutProgress() > 0.0)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub ZeroSlideDuration_JumpsStraightToTheFullLabel()
        RunSta(Sub()
                   Using icon = NewIcon(16), nav = NewCollapsedBar(icon)
                       nav.FlyoutDelay = 0
                       nav.FlyoutSlideDuration = 0
                       nav.DebugMouseMoveTo(Centre(nav.DebugBounds(0)))
                       Assert.Equal(1.0, nav.DebugFlyoutProgress())
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub NegativeTimings_AreClampedToZero_LikeEveryOtherMeasureOnTheBar()
        RunSta(Sub()
                   Using nav = New KBotNavList()
                       nav.FlyoutDelay = -40
                       nav.FlyoutSlideDuration = -1
                       Assert.Equal(0, nav.FlyoutDelay)
                       Assert.Equal(0, nav.FlyoutSlideDuration)
                   End Using
               End Sub)
    End Sub

    ' ── Când se retrage ────────────────────────────────────────────────────────

    <Fact>
    Public Sub MovingWithinTheSameButton_DoesNotRestartTheSlide()
        ' Altfel fiecare pixel de mișcare ar reporni temporizarea și eticheta n-ar ieși niciodată.
        RunSta(Sub()
                   Using icon = NewIcon(16), nav = NewCollapsedBar(icon)
                       nav.FlyoutDelay = 0
                       Dim r As Rectangle = nav.DebugBounds(0)
                       nav.DebugMouseMoveTo(Centre(r))
                       nav.DebugFlyoutTick()
                       nav.DebugFlyoutTick()
                       Dim travelled As Double = nav.DebugFlyoutProgress()
                       Assert.True(travelled > 0.0)

                       nav.DebugMouseMoveTo(New Point(r.Left + 2, r.Top + 2))
                       Assert.Equal(0, nav.DebugFlyoutIndex())
                       Assert.Equal(travelled, nav.DebugFlyoutProgress())
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub MovingToAnotherButton_RestartsFromThatButton()
        RunSta(Sub()
                   Using icon = NewIcon(16), nav = NewCollapsedBar(icon)
                       nav.FlyoutDelay = 0
                       nav.DebugMouseMoveTo(Centre(nav.DebugBounds(0)))
                       nav.DebugFlyoutTick()
                       Assert.True(nav.DebugFlyoutProgress() > 0.0)

                       nav.DebugMouseMoveTo(Centre(nav.DebugBounds(1)))
                       Assert.Equal(1, nav.DebugFlyoutIndex())
                       Assert.Equal(0.0, nav.DebugFlyoutProgress())
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub LeavingTheBar_RetractsTheLabel()
        RunSta(Sub()
                   Using icon = NewIcon(16), nav = NewCollapsedBar(icon)
                       nav.FlyoutDelay = 0
                       nav.DebugMouseMoveTo(Centre(nav.DebugBounds(0)))
                       Assert.Equal(0, nav.DebugFlyoutIndex())

                       nav.DebugMouseLeave()
                       Assert.Equal(-1, nav.DebugFlyoutIndex())
                       Assert.Equal(0.0, nav.DebugFlyoutProgress())
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub TheCollapseButton_TakesPrecedence_OverTheLabel()
        RunSta(Sub()
                   Using icon = NewIcon(16), nav = NewCollapsedBar(icon)
                       nav.FlyoutDelay = 0
                       nav.DebugMouseMoveTo(Centre(nav.DebugCollapseButtonRect()))
                       Assert.Equal(-1, nav.DebugFlyoutIndex())
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub AnythingThatMovesTheSlots_RetractsTheLabel()
        ' Eticheta ține minte un INDEX și un dreptunghi din layout-ul vechi.
        RunSta(Sub()
                   Using icon = NewIcon(16), nav = NewCollapsedBar(icon)
                       nav.FlyoutDelay = 0

                       nav.DebugMouseMoveTo(Centre(nav.DebugBounds(0)))
                       nav.SetItemVisible("b", False)
                       Assert.Equal(-1, nav.DebugFlyoutIndex())

                       nav.DebugMouseMoveTo(Centre(nav.DebugBounds(0)))
                       Assert.Equal(0, nav.DebugFlyoutIndex())
                       nav.ItemPadding = New Padding(10)
                       Assert.Equal(-1, nav.DebugFlyoutIndex())
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub ExpandingTheBar_RetractsTheLabel()
        RunSta(Sub()
                   Using icon = NewIcon(16), nav = NewCollapsedBar(icon)
                       nav.FlyoutDelay = 0
                       nav.DebugMouseMoveTo(Centre(nav.DebugBounds(0)))
                       Assert.Equal(0, nav.DebugFlyoutIndex())

                       nav.ToggleCollapse()
                       Assert.Equal(KBotNavCollapseState.Expanded, nav.CollapseState)
                       Assert.Equal(-1, nav.DebugFlyoutIndex())
                   End Using
               End Sub)
    End Sub

    ' ── Fereastra: doar «nu aruncă» ────────────────────────────────────────────

    <Fact>
    Public Sub PaintingTheLabelWindow_DoesNotThrow_ForEveryFlavourOfButton()
        RunSta(Sub()
                   Using icon = NewIcon(24), nav = NewCollapsedBar(icon)
                       nav.SetBadge("a", 12)          ' cu pastilă
                       nav.SelectedKey = "a"          ' selectat => accent + semibold
                       nav.SetItemEnabled("b", False) ' stins => pictogramă estompată

                       For Each index As Integer In {0, 1}
                           Using fly As Form = nav.DebugCreateFlyoutWindow(index)
                               Using bmp As New Bitmap(Math.Max(1, fly.Width), Math.Max(1, fly.Height))
                                   fly.DrawToBitmap(bmp, New Rectangle(0, 0, bmp.Width, bmp.Height))
                               End Using
                           End Using
                       Next
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub TheBar_DoesNotDisposeTheIconItHandedToTheLabel()
        ' Fereastra e proprietatea barei, pictograma NU — ea e a apelantului, ca peste tot.
        RunSta(Sub()
                   Using icon = NewIcon(16)
                       Using nav = NewCollapsedBar(icon)
                           nav.FlyoutDelay = 0
                           nav.DebugMouseMoveTo(Centre(nav.DebugBounds(0)))
                       End Using
                       Assert.Equal(16, icon.Width)      ' aruncă dacă bitmap-ul a fost eliberat
                   End Using
               End Sub)
    End Sub

End Class
