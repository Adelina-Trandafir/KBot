Option Strict On
Imports System.Drawing
Imports Xunit
Imports KBot.Theming

''' <summary>
''' Pixel tests for the three card primitives.
'''
''' <para>The one that matters most is the <b>flat path</b>: radius 0 must produce the same bitmap
''' as a plain <c>FillRectangle</c>, and shadow size 0 must leave the surface untouched. Those two
''' are what make «Classic and Dark render exactly as before» a measured fact rather than a hope —
''' the neutral schemes take precisely these paths.</para>
'''
''' <para>What they do NOT prove: that radius 14 looks right, or that the shadow is not too heavy.
''' Those need an operator looking at the running application.</para>
''' </summary>
Public Class ThemeShapesCardTests

    Private Const W As Integer = 60
    Private Const H As Integer = 40

    Private Shared Function Render(paint As Action(Of Graphics)) As Bitmap
        Dim bmp As New Bitmap(W, H)
        Using g As Graphics = Graphics.FromImage(bmp)
            g.Clear(Color.Magenta)          ' a colour no test uses, so «untouched» is visible
            paint(g)
        End Using
        Return bmp
    End Function

    Private Shared Sub AssertSame(a As Bitmap, b As Bitmap)
        Assert.Equal(a.Width, b.Width)
        Assert.Equal(a.Height, b.Height)
        For y As Integer = 0 To a.Height - 1
            For x As Integer = 0 To a.Width - 1
                Assert.True(a.GetPixel(x, y) = b.GetPixel(x, y),
                            $"Pixels differ at ({x},{y}): {a.GetPixel(x, y)} <> {b.GetPixel(x, y)}")
            Next
        Next
    End Sub

    ''' <summary>Radius 0 = the plain rectangle fill, to the pixel. No anti-aliased edge anywhere.</summary>
    <Fact>
    Public Sub FillCard_WithZeroRadius_EqualsAPlainFillRectangle()
        Dim r As New Rectangle(5, 5, 40, 25)
        Using viaCard As Bitmap = Render(Sub(g) ThemeShapes.FillCard(g, r, 0, Color.White, Color.Empty))
            Using viaRect As Bitmap = Render(Sub(g)
                                                 Using b As New SolidBrush(Color.White)
                                                     g.FillRectangle(b, r)
                                                 End Using
                                             End Sub)
                AssertSame(viaCard, viaRect)
            End Using
        End Using
    End Sub

    ''' <summary>Shadow size 0 must not touch a single pixel — the whole surface stays as it was.</summary>
    <Fact>
    Public Sub DrawCardShadow_WithZeroSize_DrawsNothing()
        Dim r As New Rectangle(10, 10, 30, 20)
        Using drawn As Bitmap = Render(Sub(g) ThemeShapes.DrawCardShadow(g, r, 8, Color.Black, 0, 50))
            Using untouched As Bitmap = Render(Sub(g) g.Flush())
                AssertSame(drawn, untouched)
            End Using
        End Using
    End Sub

    ''' <summary>Opacity 0 is the same statement as size 0, and has to behave the same way.</summary>
    <Fact>
    Public Sub DrawCardShadow_WithZeroOpacity_DrawsNothing()
        Dim r As New Rectangle(10, 10, 30, 20)
        Using drawn As Bitmap = Render(Sub(g) ThemeShapes.DrawCardShadow(g, r, 8, Color.Black, 10, 0))
            Using untouched As Bitmap = Render(Sub(g) g.Flush())
                AssertSame(drawn, untouched)
            End Using
        End Using
    End Sub

    ''' <summary>Radius 0 leaves the corners alone: there is no wedge to fill.</summary>
    <Fact>
    Public Sub PaintCardCorners_WithZeroRadius_DrawsNothing()
        Dim r As New Rectangle(0, 0, W, H)
        Using drawn As Bitmap = Render(Sub(g) ThemeShapes.PaintCardCorners(g, r, 0, Color.Blue))
            Using untouched As Bitmap = Render(Sub(g) g.Flush())
                AssertSame(drawn, untouched)
            End Using
        End Using
    End Sub

    ''' <summary>
    ''' With a radius, the corner pixel belongs to the canvas and the centre belongs to the card.
    ''' That is the whole trick, and it is the thing that breaks if the fill and the corner pass
    ''' ever get swapped.
    ''' </summary>
    <Fact>
    Public Sub FillThenCorners_LeavesCanvasInTheCornerAndCardInTheMiddle()
        Dim r As New Rectangle(0, 0, W, H)
        Using bmp As Bitmap = Render(Sub(g)
                                         ThemeShapes.FillCard(g, r, 12, Color.White, Color.Empty)
                                         ThemeShapes.PaintCardCorners(g, r, 12, Color.Blue)
                                     End Sub)
            Assert.Equal(Color.Blue.ToArgb(), bmp.GetPixel(0, 0).ToArgb())
            Assert.Equal(Color.White.ToArgb(), bmp.GetPixel(W \ 2, H \ 2).ToArgb())
        End Using
    End Sub

    ''' <summary>The shadow has to be darker close to the card than far from it, or it is not a shadow.</summary>
    <Fact>
    Public Sub DrawCardShadow_FadesOutwards()
        Dim r As New Rectangle(20, 15, 20, 10)
        Using bmp As Bitmap = Render(Sub(g) ThemeShapes.DrawCardShadow(g, r, 0, Color.Black, 8, 100))
            Dim near As Color = bmp.GetPixel(r.X - 1, r.Y + 5)
            Dim far As Color = bmp.GetPixel(r.X - 7, r.Y + 5)
            ' Painted over magenta, so «darker» reads as a lower red channel.
            Assert.True(near.R < far.R, $"The shadow does not fade outwards: near={near}, far={far}")
            Assert.True(far.R <= Color.Magenta.R)
        End Using
    End Sub

End Class
