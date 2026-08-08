Option Strict On
Imports System.Threading
Imports Xunit
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' English (slice 0025, step 7): `MainForm`'s nav bar is authored in the DESIGNER
''' (`MainForm.Designer.vb`), not built in the load path. This suite is the guard on that
''' migration — it is the only thing standing between a designer regeneration that silently drops
''' or reorders entries and a shell whose views cannot be reached.
'''
''' It matters more than it looks: `ApplyViewGating` and `IsViewEnabled` key off these exact
''' strings, and `IsViewEnabled` throws `ArgumentException` on a key it does not recognise. A
''' dropped entry is not a cosmetic loss.
'''
''' Diacritics are asserted as LITERAL characters, which is the plan's step-7 check: if the
''' designer ever wrote `\uXXXX` escapes or HTML entities instead, VB would carry them through as
''' plain text and these assertions would fail.
''' </summary>
Public Class MainFormNavItemsTests

    ' MainForm is a WinForms form: build it on an STA thread. Its constructor only runs
    ' InitializeComponent plus field assignments (no null guards), so the five dependencies can be
    ' Nothing — nothing here touches them.
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

    <Fact>
    Public Sub Designer_AuthorsTheEightNavEntries_InOrder()
        RunSta(Sub()
                   Using f As New MainForm(Nothing, Nothing, Nothing, Nothing, Nothing)
                       Dim items = f.navViews.Items
                       Assert.Equal(8, items.Count)

                       ' Cele cinci butoane Near, în ordinea paginilor din Access.
                       Assert.Equal(New String() {"sumar", "istoric", "rezervari", "receptii", "plati"},
                                    items.Take(5).Select(Function(i) i.Key).ToArray())
                       For i As Integer = 0 To 4
                           Assert.False(items(i).IsSeparator)
                           Assert.Equal(KBotNavAlign.Near, items(i).Align)
                       Next

                       ' Separatorul care desprinde grupul de la baza barei.
                       Assert.True(items(5).IsSeparator)
                       Assert.Equal(KBotNavAlign.Far, items(5).Align)

                       ' DDF/ORD, ancorate la capăt.
                       Assert.Equal("ddf", items(6).Key)
                       Assert.Equal("ord", items(7).Key)
                       Assert.Equal(KBotNavAlign.Far, items(6).Align)
                       Assert.Equal(KBotNavAlign.Far, items(7).Align)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Designer_WroteLiteralDiacritics_NotEscapes()
        RunSta(Sub()
                   Using f As New MainForm(Nothing, Nothing, Nothing, Nothing, Nothing)
                       Dim byKey = f.navViews.Items.Where(Function(i) Not i.IsSeparator).
                                                    ToDictionary(Function(i) i.Key, Function(i) i.Text)
                       Assert.Equal("Sumar", byKey("sumar"))
                       Assert.Equal("Istoric", byKey("istoric"))
                       Assert.Equal("Rezervări", byKey("rezervari"))
                       Assert.Equal("Recepții", byKey("receptii"))
                       Assert.Equal("Plăți", byKey("plati"))
                       Assert.Equal("Doc. Fundamentare", byKey("ddf"))
                       Assert.Equal("Ordonanțare", byKey("ord"))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Designer_AuthoredAnImage_OnEveryNonSeparatorEntry()
        ' Pictogramele vin din DOUĂ locuri: «sumar» din MainForm.resx (resursă locală), restul din
        ' My Project\Resources.resx prin accesorul KBot.App.Resources. A doua cale se rupe tăcut
        ' dacă un nume din .resx nu mai are proprietate tipizată — aici se vede.
        RunSta(Sub()
                   Using f As New MainForm(Nothing, Nothing, Nothing, Nothing, Nothing)
                       For Each it In f.navViews.Items.Where(Function(i) Not i.IsSeparator)
                           Assert.NotNull(it.Image)
                       Next
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub EndInit_RanInTheDesigner_SoTheSeparatorHasAnInternalKey()
        ' The designer does not write a key on a separator; EndInit assigns «__sep_N». If the
        ' BeginInit/EndInit pair ever goes missing from InitializeComponent, this is what notices.
        RunSta(Sub()
                   Using f As New MainForm(Nothing, Nothing, Nothing, Nothing, Nothing)
                       Assert.StartsWith("__sep_", f.navViews.Items(5).Key)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub TheDesignerAuthoredKeys_AreTheOnesTheGatingUses()
        ' ApplyViewGating calls SetItemVisible with these keys, and SetItemVisible throws on an
        ' unknown one. Proving the lookups resolve is proving the gating cannot break at startup.
        RunSta(Sub()
                   Using f As New MainForm(Nothing, Nothing, Nothing, Nothing, Nothing)
                       For Each key In {"istoric", "rezervari", "receptii", "plati", "ddf", "ord"}
                           f.navViews.SetItemVisible(key, False)
                           f.navViews.SetItemVisible(key, True)
                       Next
                       ' «sumar» has no Are* flag but must still be selectable — it is the fallback.
                       f.navViews.SelectedKey = "sumar"
                       Assert.Equal("sumar", f.navViews.SelectedKey)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub TheLoadPath_NoLongerAddsItems()
        ' Re-adding any of the eight from code would hit AddItem's duplicate-key throw on the very
        ' first run. This pins the reason the AddItem block had to go.
        RunSta(Sub()
                   Using f As New MainForm(Nothing, Nothing, Nothing, Nothing, Nothing)
                       Assert.Throws(Of ArgumentException)(Sub() f.navViews.AddItem("sumar", "Sumar"))
                       Assert.Throws(Of ArgumentException)(Sub() f.navViews.AddItem("ddf", "DDF", KBotNavAlign.Far))
                   End Using
               End Sub)
    End Sub

End Class
