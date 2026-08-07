Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Design
Imports System.Threading
Imports System.Windows.Forms
Imports Xunit
Imports KBot.Theming

''' <summary>
''' English (slice 0025): the designer-authoring surface of <see cref="KBotNavList"/> — the
''' <c>Items</c> collection, <c>ISupportInitialize</c>, and the promise that NONE of the existing
''' runtime contract moved. Slice 0018 shipped separators / Near-Far / visibility with no tests at
''' all; that gap is backfilled here too.
'''
''' What these tests CANNOT prove: the Visual Studio round-trip itself (the "…" button, the
''' collection dialog, the lines written into <c>*.Designer.vb</c>, the red marker painted on the
''' design surface). Those are manual checks — see the worklog, where they are recorded as NOT
''' RUN. <see cref="StockCollectionEditor_IsResolvedForItems"/> is the closest programmatic proxy.
''' </summary>
Public Class KBotNavListTests

    ' The control is a WinForms control: create it on an STA thread, like the sibling suites.
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

    ' A bar with a real size, so RecalcLayout hands out non-empty slots.
    Private Shared Function NewSizedList() As KBotNavList
        Dim nav As New KBotNavList()
        nav.Size = New Size(170, 400)
        Return nav
    End Function

    ' The centre of an item's slot.
    Private Shared Function CentreOf(nav As KBotNavList, index As Integer) As Point
        Dim r As Rectangle = nav.DebugBounds(index)
        Return New Point(r.Left + r.Width \ 2, r.Top + r.Height \ 2)
    End Function

    ' ── Items: colecția conduce layout-ul ─────────────────────────────────────

    <Fact>
    Public Sub Items_AddRemoveClear_ChangeWhatIndexAtReturns()
        RunSta(Sub()
                   Using nav = NewSizedList()
                       nav.Items.Add(New KBotNavItem("a", "A"))
                       nav.Items.Add(New KBotNavItem("b", "B"))

                       ' Adding invalidated the layout: the slots exist without anyone painting.
                       Dim pA As Point = CentreOf(nav, 0)
                       Dim pB As Point = CentreOf(nav, 1)
                       Assert.Equal(0, nav.DebugIndexAt(pA))
                       Assert.Equal(1, nav.DebugIndexAt(pB))

                       ' Removing the first one slides «b» up into its slot.
                       nav.Items.RemoveAt(0)
                       Assert.Single(nav.Items)
                       Assert.Equal(0, nav.DebugIndexAt(pA))
                       Assert.Equal("b", nav.Items(nav.DebugIndexAt(pA)).Key)

                       ' Clearing leaves nothing to hit.
                       nav.Items.Clear()
                       Assert.Empty(nav.Items)
                       Assert.Equal(-1, nav.DebugIndexAt(pA))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Items_AddedThroughTheCollection_BehaveLikeAddItem()
        ' The designer path and the code path must produce the same control state.
        RunSta(Sub()
                   Using designerAuthored = NewSizedList(), codeAuthored = NewSizedList()
                       designerAuthored.Items.Add(New KBotNavItem("sumar", "Sumar"))
                       codeAuthored.AddItem("sumar", "Sumar")

                       Assert.Equal(codeAuthored.Items.Count, designerAuthored.Items.Count)
                       Assert.Equal(codeAuthored.Items(0).Key, designerAuthored.Items(0).Key)
                       Assert.Equal(codeAuthored.Items(0).Text, designerAuthored.Items(0).Text)
                       Assert.Equal(codeAuthored.Items(0).Align, designerAuthored.Items(0).Align)
                       Assert.Equal(codeAuthored.DebugBounds(0), designerAuthored.DebugBounds(0))

                       ' And the lookups reach it.
                       designerAuthored.SelectedKey = "sumar"
                       Assert.Equal("sumar", designerAuthored.SelectedKey)
                       designerAuthored.SetBadge("sumar", 3)
                       Assert.Equal(3, designerAuthored.Items(0).Badge)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Items_RejectsNothing()
        RunSta(Sub()
                   Using nav = NewSizedList()
                       Assert.Throws(Of ArgumentNullException)(Sub() nav.Items.Add(Nothing))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub NearFarOrdering_IsPreserved_IncludingASeparatorBetweenTheGroups()
        ' The MainForm shape: Near group at the top, separator + Far group anchored at the bottom.
        RunSta(Sub()
                   Using nav = NewSizedList()
                       nav.Items.Add(New KBotNavItem("sumar", "Sumar", KBotNavAlign.Near))
                       nav.Items.Add(New KBotNavItem("plati", "Plăți", KBotNavAlign.Near))
                       nav.Items.Add(New KBotNavItem With {.IsSeparator = True, .Align = KBotNavAlign.Far})
                       nav.Items.Add(New KBotNavItem("ddf", "DDF", KBotNavAlign.Far))
                       nav.Items.Add(New KBotNavItem("ord", "ORD", KBotNavAlign.Far))

                       ' Order in the collection is the order in the model.
                       Assert.Equal(New String() {"sumar", "plati", "ddf", "ord"},
                                    nav.Items.Where(Function(i) Not i.IsSeparator).
                                              Select(Function(i) i.Key).ToArray())

                       ' Geometry: Near flows from the top, Far is pushed to the bottom, and the
                       ' separator sits above the Far buttons.
                       Assert.True(nav.DebugBounds(0).Top < nav.DebugBounds(1).Top)
                       Assert.True(nav.DebugBounds(1).Bottom < nav.DebugBounds(2).Top)
                       Assert.True(nav.DebugBounds(2).Bottom <= nav.DebugBounds(3).Top)
                       Assert.True(nav.DebugBounds(3).Top < nav.DebugBounds(4).Top)
                       Assert.Equal(nav.Height - Dpi(nav, 6), nav.DebugBounds(4).Bottom)   ' 6 = marginea
                   End Using
               End Sub)
    End Sub

    ' ── ISupportInitialize ─────────────────────────────────────────────────────

    <Fact>
    Public Sub EndInit_ThrowsOnDuplicateKey()
        RunSta(Sub()
                   Using nav = NewSizedList()
                       Dim init As ISupportInitialize = nav
                       init.BeginInit()
                       nav.Items.Add(New KBotNavItem("a", "A"))
                       nav.Items.Add(New KBotNavItem("a", "A din nou"))
                       Assert.Throws(Of ArgumentException)(Sub() init.EndInit())
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub EndInit_ThrowsOnEmptyKeyOnANonSeparator()
        RunSta(Sub()
                   Using nav = NewSizedList()
                       Dim init As ISupportInitialize = nav
                       init.BeginInit()
                       nav.Items.Add(New KBotNavItem With {.Text = "fără cheie"})
                       Assert.Throws(Of ArgumentException)(Sub() init.EndInit())
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub EndInit_AssignsSeparatorKeys_WithoutCollidingWithAddSeparator()
        RunSta(Sub()
                   Using nav = NewSizedList()
                       Dim init As ISupportInitialize = nav
                       init.BeginInit()
                       nav.Items.Add(New KBotNavItem With {.IsSeparator = True})
                       nav.Items.Add(New KBotNavItem("a", "A"))
                       nav.Items.Add(New KBotNavItem With {.IsSeparator = True})
                       init.EndInit()

                       Assert.StartsWith("__sep_", nav.Items(0).Key)
                       Assert.StartsWith("__sep_", nav.Items(2).Key)
                       Assert.NotEqual(nav.Items(0).Key, nav.Items(2).Key)

                       ' A separator created afterwards in code must not reuse either key.
                       nav.AddSeparator()
                       Dim keys = nav.Items.Where(Function(i) i.IsSeparator).
                                            Select(Function(i) i.Key).ToArray()
                       Assert.Equal(3, keys.Length)
                       Assert.Equal(keys.Length, keys.Distinct(StringComparer.Ordinal).Count())
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub EndInit_LeavesAHandWrittenSeparatorKeyAlone_AndStillDoesNotCollide()
        RunSta(Sub()
                   Using nav = NewSizedList()
                       Dim init As ISupportInitialize = nav
                       init.BeginInit()
                       ' Someone typed a key on a separator in the designer. It is ignored anyway,
                       ' so it stays — but the auto-assigned ones must step around it.
                       nav.Items.Add(New KBotNavItem With {.IsSeparator = True, .Key = "__sep_1"})
                       nav.Items.Add(New KBotNavItem With {.IsSeparator = True})
                       init.EndInit()

                       Assert.Equal("__sep_1", nav.Items(0).Key)
                       Assert.NotEqual("__sep_1", nav.Items(1).Key)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub SelectedKey_SetDuringInit_DoesNotThrowOnAnUnknownKey_AndIsAppliedAtEndInit()
        ' The designer has no obligation to emit Items before SelectedKey.
        RunSta(Sub()
                   Using nav = NewSizedList()
                       Dim init As ISupportInitialize = nav
                       init.BeginInit()
                       nav.SelectedKey = "ddf"                       ' nothing exists yet
                       nav.Items.Add(New KBotNavItem("sumar", "Sumar"))
                       nav.Items.Add(New KBotNavItem("ddf", "DDF"))
                       init.EndInit()
                       Assert.Equal("ddf", nav.SelectedKey)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub SelectedKey_AppliedAtEndInit_RaisesSelectionChangedOnce()
        RunSta(Sub()
                   Using nav = NewSizedList()
                       Dim raised As Integer = 0
                       AddHandler nav.SelectionChanged, Sub(k As String) raised += 1

                       Dim init As ISupportInitialize = nav
                       init.BeginInit()
                       nav.SelectedKey = "sumar"
                       nav.Items.Add(New KBotNavItem("sumar", "Sumar"))
                       Assert.Equal(0, raised)                        ' nothing fires while initialising
                       init.EndInit()
                       Assert.Equal(1, raised)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub SelectedKey_StillThrowsOutsideInit()
        RunSta(Sub()
                   Using nav = NewSizedList()
                       nav.AddItem("sumar", "Sumar")
                       Assert.Throws(Of ArgumentException)(Sub() nav.SelectedKey = "nu-exista")
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub EndInit_AcceptsAValidBarWithSeparators()
        ' The MainForm shape must survive EndInit's validation untouched.
        RunSta(Sub()
                   Using nav = NewSizedList()
                       Dim init As ISupportInitialize = nav
                       init.BeginInit()
                       For Each k In {"sumar", "istoric", "rezervari", "receptii", "plati"}
                           nav.Items.Add(New KBotNavItem(k, k))
                       Next
                       nav.Items.Add(New KBotNavItem With {.IsSeparator = True, .Align = KBotNavAlign.Far})
                       nav.Items.Add(New KBotNavItem("ddf", "DDF", KBotNavAlign.Far))
                       nav.Items.Add(New KBotNavItem("ord", "ORD", KBotNavAlign.Far))
                       init.EndInit()

                       Assert.Equal(8, nav.Items.Count)
                       nav.SelectedKey = "sumar"
                       Assert.Equal("sumar", nav.SelectedKey)
                   End Using
               End Sub)
    End Sub

    ' ── Contractul de rulare, NESCHIMBAT ──────────────────────────────────────

    <Fact>
    Public Sub AddItem_StillRejectsEmptyAndDuplicateKeys()
        RunSta(Sub()
                   Using nav = NewSizedList()
                       nav.AddItem("a", "A")
                       Assert.Throws(Of ArgumentException)(Sub() nav.AddItem("", "X"))
                       Assert.Throws(Of ArgumentException)(Sub() nav.AddItem("   ", "X"))
                       Assert.Throws(Of ArgumentException)(Sub() nav.AddItem("a", "A din nou"))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub SetItemVisibleEnabledBadge_StillThrowOnUnknownKeys()
        RunSta(Sub()
                   Using nav = NewSizedList()
                       nav.AddItem("a", "A")
                       Assert.Throws(Of ArgumentException)(Sub() nav.SetItemVisible("zzz", False))
                       Assert.Throws(Of ArgumentException)(Sub() nav.SetItemEnabled("zzz", False))
                       Assert.Throws(Of ArgumentException)(Sub() nav.SetBadge("zzz", 3))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub ASeparatorKey_IsNeverReachableThroughALookup()
        ' FindIndex skips separators, so the internal «__sep_N» key is not part of the API.
        RunSta(Sub()
                   Using nav = NewSizedList()
                       nav.AddItem("a", "A")
                       nav.AddSeparator()
                       Dim sepKey As String = nav.Items(1).Key
                       Assert.StartsWith("__sep_", sepKey)
                       Assert.Throws(Of ArgumentException)(Sub() nav.SelectedKey = sepKey)
                       Assert.Throws(Of ArgumentException)(Sub() nav.SetItemEnabled(sepKey, False))
                   End Using
               End Sub)
    End Sub

    ' ── Vizibilitate (acoperirea lipsă din 0018) ──────────────────────────────

    <Fact>
    Public Sub AHiddenItem_GetsAnEmptyRectangle_AndIsSkippedByIndexAt()
        RunSta(Sub()
                   Using nav = NewSizedList()
                       nav.AddItem("a", "A")
                       nav.AddItem("b", "B")
                       nav.AddItem("c", "C")
                       Dim slotB As Point = CentreOf(nav, 1)

                       nav.SetItemVisible("b", False)
                       Assert.Equal(Rectangle.Empty, nav.DebugBounds(1))

                       ' «b» took no space, so «c» slid up into the slot «b» used to occupy.
                       Assert.Equal(2, nav.DebugIndexAt(slotB))
                       Assert.Equal("c", nav.Items(2).Key)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub AHiddenItem_IsSkippedByKeyboardNavigation()
        RunSta(Sub()
                   Using nav = NewSizedList()
                       nav.AddItem("a", "A")
                       nav.AddItem("b", "B")
                       nav.AddItem("c", "C")
                       nav.SetItemVisible("b", False)
                       nav.SelectedKey = "a"

                       nav.DebugKeyDown(Keys.Down)
                       Assert.Equal("c", nav.SelectedKey)      ' «b» sărit

                       nav.DebugKeyDown(Keys.Up)
                       Assert.Equal("a", nav.SelectedKey)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub AHiddenItem_CannotBeSelected()
        RunSta(Sub()
                   Using nav = NewSizedList()
                       nav.AddItem("a", "A")
                       nav.AddItem("b", "B")
                       nav.SetItemVisible("b", False)
                       Assert.Throws(Of ArgumentException)(Sub() nav.SelectedKey = "b")
                   End Using
               End Sub)
    End Sub

    ' ── Pictograma din stânga (0025-02) ───────────────────────────────────────

    ' The control scales every logical measurement to its DPI, so the expectations below have to
    ' scale too — a literal 20 would pass here and fail on a 150% display.
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

    <Fact>
    Public Sub AnItemWithoutAnImage_LeavesTheTextWhereItAlwaysWas()
        RunSta(Sub()
                   Using nav = NewSizedList()
                       nav.AddItem("a", "A")
                       Assert.Equal(Rectangle.Empty, nav.DebugIconRect(0))
                       ' 12 px logici de padding, exact ca înainte de felie.
                       Assert.Equal(nav.DebugBounds(0).Left + Dpi(nav, 12), nav.DebugTextLeft(0))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub AnImage_GetsASquareSlotOnTheLeft_AndPushesTheTextRight()
        RunSta(Sub()
                   Using nav = NewSizedList(), icon = NewIcon(64)
                       nav.AddItem("a", "A")
                       nav.Items(0).Image = icon
                       nav.InvalidateLayout()

                       Dim r As Rectangle = nav.DebugBounds(0)
                       Dim ir As Rectangle = nav.DebugIconRect(0)

                       Assert.False(ir.IsEmpty)
                       Assert.Equal(ir.Width, ir.Height)                        ' pătrat
                       Assert.Equal(Dpi(nav, 20), ir.Width)                     ' IconSize implicit
                       Assert.Equal(r.Left + Dpi(nav, 12), ir.Left)             ' lipit de padding-ul din stânga
                       Assert.Equal(r.Top + (r.Height - ir.Height) \ 2, ir.Top) ' centrat vertical
                       ' Sursa de 64px se scalează în pătrat — nu impune lățimea rândului.
                       Assert.Equal(r.Left + Dpi(nav, 12) + Dpi(nav, 20) + Dpi(nav, 8), nav.DebugTextLeft(0))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub IconSize_DrivesTheSlotAndTheTextOffset()
        RunSta(Sub()
                   Using nav = NewSizedList(), icon = NewIcon(16)
                       nav.AddItem("a", "A")
                       nav.Items(0).Image = icon
                       nav.IconSize = 14
                       Assert.Equal(Dpi(nav, 14), nav.DebugIconRect(0).Width)
                       Assert.Equal(nav.DebugBounds(0).Left + Dpi(nav, 12) + Dpi(nav, 14) + Dpi(nav, 8),
                                    nav.DebugTextLeft(0))

                       ' IconSize = 0 înseamnă «fără pictograme», nu «pictograme de zero pixeli
                       ' care lasă totuși o gaură»: textul revine la padding-ul simplu.
                       nav.IconSize = 0
                       Assert.Equal(Rectangle.Empty, nav.DebugIconRect(0))
                       Assert.Equal(nav.DebugBounds(0).Left + Dpi(nav, 12), nav.DebugTextLeft(0))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub TheIconSlotShrinks_WhenTheRowIsShorterThanTheNominalSide()
        ' Bară orizontală joasă: pictograma nu are voie să iasă din rând.
        RunSta(Sub()
                   Using nav = New KBotNavList(), icon = NewIcon(32)
                       nav.Orientation = KBotNavOrientation.Horizontal
                       nav.Size = New Size(400, 22)
                       nav.AddItem("a", "A")
                       nav.Items(0).Image = icon

                       Dim r As Rectangle = nav.DebugBounds(0)
                       Dim ir As Rectangle = nav.DebugIconRect(0)
                       Assert.True(ir.Width < Dpi(nav, 20), "slotul trebuie să se strângă sub latura nominală")
                       Assert.True(ir.Top >= r.Top AndAlso ir.Bottom <= r.Bottom)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub AnImage_WidensAHorizontalItem_ButNotAVerticalOne()
        RunSta(Sub()
                   Using icon = NewIcon(16)
                       ' Vertical: înălțimea rândului e fixă, deci pictograma nu schimbă extinderea.
                       Using nav = NewSizedList()
                           nav.AddItem("a", "A")
                           Dim before As Integer = nav.DebugBounds(0).Height
                           nav.Items(0).Image = icon
                           nav.InvalidateLayout()
                           Assert.Equal(before, nav.DebugBounds(0).Height)
                       End Using

                       ' Orizontal: lățimea se măsoară din conținut, deci crește cu slotul + spațiu.
                       Using nav = New KBotNavList()
                           nav.Orientation = KBotNavOrientation.Horizontal
                           nav.Size = New Size(600, 34)
                           nav.AddItem("a", "Un text destul de lung ca să nu lovească minimul")
                           Dim before As Integer = nav.DebugBounds(0).Width
                           nav.Items(0).Image = icon
                           nav.InvalidateLayout()
                           Assert.Equal(before + Dpi(nav, 20) + Dpi(nav, 8), nav.DebugBounds(0).Width)
                       End Using
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub ASeparator_IgnoresItsImage_LikeItIgnoresKeyAndText()
        RunSta(Sub()
                   Using nav = NewSizedList(), icon = NewIcon(16)
                       nav.AddItem("a", "A")
                       nav.AddSeparator()
                       nav.Items(1).Image = icon
                       nav.InvalidateLayout()
                       Assert.Equal(Rectangle.Empty, nav.DebugIconRect(1))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub PaintingAnItemWithAnImage_DoesNotThrow_EnabledOrDisabled()
        ' The disabled path runs a ColorMatrix through DrawImage; headless paint proves both
        ' branches execute (a bad matrix throws, it does not just look wrong).
        RunSta(Sub()
                   Using nav = NewSizedList(), icon = NewIcon(24)
                       nav.AddItem("a", "A")
                       nav.AddItem("b", "B")
                       nav.Items(0).Image = icon
                       nav.Items(1).Image = icon
                       nav.SetItemEnabled("b", False)
                       Using bmp As New Bitmap(nav.Width, nav.Height)
                           nav.DrawToBitmap(bmp, New Rectangle(0, 0, bmp.Width, bmp.Height))
                       End Using
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub TheItemDoesNotOwnTheImage_AndNeverDisposesIt()
        ' The image belongs to the caller or to the form's .resx; disposing the bar must not take
        ' it down, or a resource shared between two items dies with the first one.
        Dim icon As Bitmap = NewIcon(16)
        RunSta(Sub()
                   Using nav = NewSizedList()
                       nav.AddItem("a", "A")
                       nav.Items(0).Image = icon
                   End Using
               End Sub)
        Assert.Equal(16, icon.Width)          ' still alive after the bar was disposed
        icon.Dispose()
    End Sub

    ' ── Proxy pentru rotunda din designer ─────────────────────────────────────

    <Fact>
    Public Sub StockCollectionEditor_IsResolvedForItems()
        ' This is what makes the "…" button appear next to Items in the property grid: the stock
        ' CollectionEditor that TypeDescriptor registers intrinsically for ICollection. No custom
        ' editor, no design-time assembly. If this ever stops resolving, the property grid can no
        ' longer open the collection dialog and the whole premise of the slice is gone.
        Dim prop As PropertyDescriptor = TypeDescriptor.GetProperties(GetType(KBotNavList))("Items")
        Assert.NotNull(prop)
        Assert.Equal(DesignerSerializationVisibility.Content, prop.SerializationVisibility)

        Dim ed As Object = prop.GetEditor(GetType(UITypeEditor))
        Assert.NotNull(ed)
        Assert.Contains("CollectionEditor", ed.GetType().FullName)
    End Sub

    <Fact>
    Public Sub NavItem_ToString_TellsButtonsAndSeparatorsApart()
        ' The collection dialog's list shows ToString(); with one item type it is the only way to
        ' see what you are editing.
        Dim button As New KBotNavItem("ddf", "DDF", KBotNavAlign.Far)
        Assert.Contains("ddf", button.ToString())
        Assert.Contains("DDF", button.ToString())
        Assert.Contains("Far", button.ToString())

        Dim sep As New KBotNavItem With {.IsSeparator = True, .Align = KBotNavAlign.Far}
        Assert.Contains("separator", sep.ToString())

        Dim unkeyed As New KBotNavItem With {.Text = "X"}
        Assert.Contains("fără cheie", unkeyed.ToString())
    End Sub

End Class
