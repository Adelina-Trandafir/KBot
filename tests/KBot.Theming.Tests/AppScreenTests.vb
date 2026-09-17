Option Strict On
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Theming
Imports Xunit

''' <summary>
''' Slice 0062: the pure placement rules of <see cref="AppScreen"/> -- centre, then clamp so the
''' top-left corner stays on the screen -- and the reference chain that never asks the mouse.
''' </summary>
Public Class AppScreenTests

    <Fact>
    Public Sub CenteredIn_EvenArea_IsExactlyCentered()
        Dim p As Point = AppScreen.CenteredIn(New Rectangle(0, 0, 1000, 600), New Size(400, 200))
        Assert.Equal(New Point(300, 200), p)
    End Sub

    <Fact>
    Public Sub CenteredIn_OddArea_IntegerDivisionStaysInsideTheArea()
        Dim area As New Rectangle(0, 0, 1001, 601)
        Dim size As New Size(401, 201)
        Dim p As Point = AppScreen.CenteredIn(area, size)
        Assert.True(p.X >= area.Left AndAlso p.X + size.Width <= area.Right)
        Assert.True(p.Y >= area.Top AndAlso p.Y + size.Height <= area.Bottom)
        Assert.Equal(New Point(300, 200), p)
    End Sub

    <Fact>
    Public Sub CenteredIn_WiderThanArea_ClampsToLeftEdge()
        Dim area As New Rectangle(0, 0, 1000, 600)
        Dim p As Point = AppScreen.CenteredIn(area, New Size(1400, 200))
        Assert.Equal(area.Left, p.X)
        Assert.Equal(200, p.Y)
    End Sub

    <Fact>
    Public Sub CenteredIn_TallerThanArea_ClampsToTopEdge()
        Dim area As New Rectangle(0, 0, 1000, 600)
        Dim p As Point = AppScreen.CenteredIn(area, New Size(400, 900))
        Assert.Equal(300, p.X)
        Assert.Equal(area.Top, p.Y)
    End Sub

    <Fact>
    Public Sub CenteredIn_SecondaryMonitorOrigin_IsHonoured()
        ' A monitor to the left of the primary has negative coordinates; one to the right starts
        ' at the primary's width. Both must come out inside THEIR area, not the primary's.
        Dim right As New Rectangle(1920, 0, 1280, 1024)
        Assert.Equal(New Point(1920 + 440, 412), AppScreen.CenteredIn(right, New Size(400, 200)))

        Dim left As New Rectangle(-1280, 0, 1280, 1024)
        Assert.Equal(New Point(-1280 + 440, 412), AppScreen.CenteredIn(left, New Size(400, 200)))
    End Sub

    <Fact>
    Public Sub ClampedIn_InsideAlready_DoesNotMove()
        Dim area As New Rectangle(0, 0, 1000, 600)
        Assert.Equal(New Point(100, 50), AppScreen.ClampedIn(area, New Rectangle(100, 50, 400, 200)))
    End Sub

    <Fact>
    Public Sub ClampedIn_RunningOffBottomRight_IsPushedBack_TopLeftWinsWhenTooBig()
        Dim area As New Rectangle(0, 0, 1000, 600)
        Assert.Equal(New Point(600, 400), AppScreen.ClampedIn(area, New Rectangle(800, 500, 400, 200)))
        ' Larger than the area on both axes: the top-left corner is what survives.
        Assert.Equal(New Point(0, 0), AppScreen.ClampedIn(area, New Rectangle(300, 300, 1400, 900)))
    End Sub

    <Fact>
    Public Sub Reference_WithoutAnyForm_IsThePrimaryScreen_NeverTheMouse()
        AppScreen.ClearReference(AppScreen.ReferenceForm())
        Assert.Same(Screen.PrimaryScreen, AppScreen.Reference())
    End Sub

    <Fact>
    Public Sub Reference_RegisteredForm_IsUsed_AndClearedOnlyByItself()
        Using main As New Form()
            Using other As New Form()
                AppScreen.SetReference(main)
                Assert.Same(main, AppScreen.ReferenceForm())

                ' Somebody else cannot clear it...
                AppScreen.ClearReference(other)
                Assert.Same(main, AppScreen.ReferenceForm())

                ' ...the registered form can.
                AppScreen.ClearReference(main)
                Assert.Null(AppScreen.ReferenceForm())
            End Using
        End Using
    End Sub

    <Fact>
    Public Sub Reference_ExcludesTheFormBeingPlaced()
        Using main As New Form()
            AppScreen.SetReference(main)
            Dim h As IntPtr = main.Handle   ' a reference needs a handle to name a screen
            Try
                ' Placing the reference itself: it is not its own answer, and with no other open
                ' form the chain lands on the primary screen.
                Assert.Same(Screen.PrimaryScreen, AppScreen.Reference(main))
                Assert.Equal(Screen.FromHandle(h).DeviceName, AppScreen.Reference().DeviceName)
            Finally
                AppScreen.ClearReference(main)
            End Try
        End Using
    End Sub

End Class
