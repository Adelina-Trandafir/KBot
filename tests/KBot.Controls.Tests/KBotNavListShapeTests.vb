Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Threading
Imports System.Windows.Forms
Imports Xunit
Imports KBot.Theming

''' <summary>
''' English (slice 0025-08): the shape of a nav button — <c>ItemCornerRadius</c> (per BAR, not per
''' item) and <c>ItemGradient</c>, both shared by the buttons, the corner button and the collapsed
''' bar's floating label.
'''
''' <c>ThemeShapes.FillModern</c> is the one place where the gradient is decided, and it is the one
''' thing in this whole slice family that CAN be checked headless for real: a bitmap says whether
''' the top is lighter than the bottom. Everything else here is «the number the bar resolved»,
''' which is not the same as «it looks right on screen».
''' </summary>
Public Class KBotNavListShapeTests

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

    Private Shared Function NewBar() As KBotNavList
        Dim nav As New KBotNavList()
        nav.Size = New Size(170, 400)
        nav.AddItem("a", "Sumar")
        nav.AddItem("b", "Istoric")
        Return nav
    End Function

    ' ── ItemCornerRadius ───────────────────────────────────────────────────────

    <Fact>
    Public Sub Defaults_FollowTheThemeAndCarryAModestGradient()
        RunSta(Sub()
                   Using nav = NewBar()
                       Assert.Equal(-1, nav.ItemCornerRadius)      ' -1 = «ia raza schemei»
                       Assert.Equal(14, nav.ItemGradient)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub MinusOne_TakesTheRadiusFromTheActiveScheme()
        ' Asta e ce face proprietatea implicită să URMEZE tema: Classic/Dark sunt pătrate (0),
        ' Modern e rotunjită (8), și bara se schimbă odată cu schema fără să i se ceară nimic.
        RunSta(Sub()
                   Using nav = NewBar()
                       nav.ApplyTheme(BuiltInSchemes.Classic())
                       Assert.Equal(0, nav.DebugItemRadius())

                       nav.ApplyTheme(BuiltInSchemes.Modern())
                       Assert.Equal(Dpi(nav, 8), nav.DebugItemRadius())
                       Assert.Equal(8, BuiltInSchemes.Modern().Style.CornerRadius)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub AnExplicitRadius_BeatsTheScheme_InBothDirections()
        RunSta(Sub()
                   Using nav = NewBar()
                       ' Rotunjit peste o schemă pătrată…
                       nav.ApplyTheme(BuiltInSchemes.Classic())
                       nav.ItemCornerRadius = 10
                       Assert.Equal(Dpi(nav, 10), nav.DebugItemRadius())

                       ' …și drept peste una rotunjită (0 e o valoare, nu «nesetat»).
                       nav.ApplyTheme(BuiltInSchemes.Modern())
                       nav.ItemCornerRadius = 0
                       Assert.Equal(0, nav.DebugItemRadius())

                       ' Înapoi la «ia din schemă».
                       nav.ItemCornerRadius = -1
                       Assert.Equal(Dpi(nav, 8), nav.DebugItemRadius())
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub OutOfRangeValues_AreClamped_NotThrown()
        ' Un setter de dimensiune care aruncă ar rupe InitializeComponent la o valoare greșită
        ' din designer — aceeași regulă ca la IconSize / ItemWidth / ItemPadding.
        RunSta(Sub()
                   Using nav = NewBar()
                       nav.ItemCornerRadius = -99
                       Assert.Equal(-1, nav.ItemCornerRadius)

                       nav.ItemGradient = 500
                       Assert.Equal(100, nav.ItemGradient)
                       nav.ItemGradient = -7
                       Assert.Equal(0, nav.ItemGradient)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub TheRadiusIsOnTheBar_NotOnTheItem()
        ' Cerință explicită a operatorului: «per navlist (not per button)». Dacă cineva adaugă
        ' vreodată o rază pe element, testul ăsta cade și decizia se reia conștient.
        Dim onItem As PropertyDescriptor =
            TypeDescriptor.GetProperties(GetType(KBotNavItem))("ItemCornerRadius")
        Assert.Null(onItem)

        Dim onBar As PropertyDescriptor =
            TypeDescriptor.GetProperties(GetType(KBotNavList))("ItemCornerRadius")
        Assert.NotNull(onBar)
        Assert.True(onBar.IsBrowsable)

        Dim grad As PropertyDescriptor =
            TypeDescriptor.GetProperties(GetType(KBotNavList))("ItemGradient")
        Assert.NotNull(grad)
        Assert.True(grad.IsBrowsable)
    End Sub

    ' ── Eticheta primește exact aceeași formă ──────────────────────────────────

    <Fact>
    Public Sub TheFloatingLabel_UsesTheSameRadiusAndGradientAsTheButtons()
        ' Eticheta trebuie să fie butonul care se desfășoară: o rază sau un gradient care nu se
        ' potrivesc ar strica iluzia mai rău decât lipsa lor.
        RunSta(Sub()
                   Using icon = NewIcon(16), nav = NewBar()
                       nav.Items(0).Image = icon
                       nav.Collapsible = True
                       nav.CollapseState = KBotNavCollapseState.Icons
                       nav.ApplyTheme(BuiltInSchemes.Modern())
                       nav.ItemCornerRadius = 6
                       nav.ItemGradient = 30

                       Dim st As KBotNavFlyoutStyle = nav.DebugFlyoutStyle(0)
                       Assert.Equal(nav.DebugItemRadius(), st.Radius)
                       Assert.Equal(30, st.GradientStrength)
                   End Using
               End Sub)
    End Sub

    ' ── Gradientul, verificat pe pixeli (singurul verdict vizual posibil aici) ──

    <Fact>
    Public Sub FillModern_WithZeroStrength_IsExactlyAFlatFill()
        ' Implicitul feliilor de dinainte: 0 => bit cu bit ce se picta cu SolidBrush.
        Dim baseColor As Color = Color.FromArgb(90, 110, 140)
        Using bmp As New Bitmap(20, 40)
            Using g = Graphics.FromImage(bmp)
                g.Clear(Color.Black)
                Dim r As New Rectangle(0, 0, 20, 40)
                Using path As GraphicsPath = ThemeShapes.RoundedRect(r, 0)
                    ThemeShapes.FillModern(g, path, r, baseColor, 0)
                End Using
            End Using
            Assert.Equal(baseColor.ToArgb(), bmp.GetPixel(10, 2).ToArgb())
            Assert.Equal(baseColor.ToArgb(), bmp.GetPixel(10, 37).ToArgb())
        End Using
    End Sub

    <Fact>
    Public Sub FillModern_LightensTheTopAndDarkensTheBottom()
        Dim baseColor As Color = Color.FromArgb(90, 110, 140)
        Using bmp As New Bitmap(20, 40)
            Using g = Graphics.FromImage(bmp)
                g.Clear(Color.Black)
                Dim r As New Rectangle(0, 0, 20, 40)
                Using path As GraphicsPath = ThemeShapes.RoundedRect(r, 0)
                    ThemeShapes.FillModern(g, path, r, baseColor, 40)
                End Using
            End Using
            Dim top As Color = bmp.GetPixel(10, 1)
            Dim bottom As Color = bmp.GetPixel(10, 38)
            Assert.True(top.R > baseColor.R, $"sus ar trebui să fie mai deschis: {top.R} vs {baseColor.R}")
            Assert.True(bottom.R < baseColor.R, $"jos ar trebui să fie mai închis: {bottom.R} vs {baseColor.R}")
            ' Asimetric prin construcție: se deschide mai mult decât se închide, altfel arată a XP.
            Assert.True(top.R - baseColor.R > baseColor.R - bottom.R,
                        "capătul de sus trebuie să se depărteze mai mult de bază decât cel de jos")
        End Using
    End Sub

    <Fact>
    Public Sub FillModern_DerivesBothEndsFromTheBase_SoItWorksOnADarkScheme()
        ' Nicio culoare nouă: pe un fundal aproape negru capătul de sus tot se deschide.
        Dim dark As Color = Color.FromArgb(32, 34, 38)
        Using bmp As New Bitmap(20, 40)
            Using g = Graphics.FromImage(bmp)
                g.Clear(Color.Red)
                Dim r As New Rectangle(0, 0, 20, 40)
                Using path As GraphicsPath = ThemeShapes.RoundedRect(r, 0)
                    ThemeShapes.FillModern(g, path, r, dark, 50)
                End Using
            End Using
            Assert.True(bmp.GetPixel(10, 1).R > dark.R)
            Assert.True(bmp.GetPixel(10, 38).R <= dark.R)
        End Using
    End Sub

    <Fact>
    Public Sub FillModern_ClampsTheStrength_AndSurvivesADegenerateRectangle()
        Using bmp As New Bitmap(4, 1)
            Using g = Graphics.FromImage(bmp)
                Dim r As New Rectangle(0, 0, 4, 1)
                Using path As GraphicsPath = ThemeShapes.RoundedRect(r, 0)
                    ThemeShapes.FillModern(g, path, r, Color.SlateGray, 999)     ' se limitează la 100
                    ThemeShapes.FillModern(g, path, r, Color.SlateGray, -5)      ' se limitează la 0
                    ThemeShapes.FillModern(g, path, New Rectangle(0, 0, 0, 0), Color.SlateGray, 40)
                End Using
            End Using
        End Using
    End Sub

    ' ── Pictare ────────────────────────────────────────────────────────────────

    <Fact>
    Public Sub PaintingTheBar_WithEveryRadiusAndGradient_DoesNotThrow()
        RunSta(Sub()
                   Using icon = NewIcon(16), nav = NewBar()
                       nav.Items(0).Image = icon
                       nav.SetBadge("a", 3)
                       nav.SelectedKey = "a"
                       nav.Collapsible = True
                       nav.ApplyTheme(BuiltInSchemes.Modern())

                       For Each radius As Integer In {-1, 0, 4, 999}
                           For Each grad As Integer In {0, 14, 100}
                               nav.ItemCornerRadius = radius
                               nav.ItemGradient = grad
                               Using bmp As New Bitmap(nav.Width, nav.Height)
                                   nav.DrawToBitmap(bmp, New Rectangle(0, 0, bmp.Width, bmp.Height))
                               End Using
                           Next
                       Next
                   End Using
               End Sub)
    End Sub

End Class
