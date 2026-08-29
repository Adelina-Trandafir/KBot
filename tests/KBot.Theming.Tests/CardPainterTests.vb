Option Strict On
Imports System.Drawing
Imports System.Windows.Forms
Imports Xunit
Imports KBot.Theming

''' <summary>
''' The attach/detach contract of the card painter.
'''
''' <para>The failure this guards against is not hypothetical. <c>ThemeManager.Apply</c> runs on
''' every scheme change AND on every form that opens, so a hook added without first checking
''' whether one is already there would stack up: after three switches a card would paint itself
''' four times per repaint, each pass drawing the same shadow on top of the last until the edge
''' went solid. The tally on <see cref="CardPainter.AttachedCount"/> exists for exactly this test.</para>
'''
''' <para>These tests build controls but never show a window — no Handle is created, so nothing
''' appears on the operator's screen.</para>
''' </summary>
Public Class CardPainterTests

    Private Shared Function CardOnHost(tag As String) As Panel
        Dim host As New Panel() With {.Size = New Size(400, 300)}
        Dim card As New Panel() With {.Size = New Size(200, 150), .Location = New Point(20, 20), .Tag = tag}
        host.Controls.Add(card)
        Return card
    End Function

    <Fact>
    Public Sub IsCardSurface_OnlyAnswersForTheNewTag()
        Assert.True(CardPainter.IsCardSurface(New Panel() With {.Tag = "CardSurface"}))
        ' The older tag keeps its old meaning — surface colour only, no corners and no shadow.
        Assert.False(CardPainter.IsCardSurface(New Panel() With {.Tag = "Card"}))
        Assert.False(CardPainter.IsCardSurface(New Panel()))
        Assert.False(CardPainter.IsCardSurface(New Label() With {.Tag = "CardSurface"}))
        Assert.False(CardPainter.IsCardSurface(Nothing))
    End Sub

    <Fact>
    Public Sub Sync_IgnoresAnythingThatIsNotACardSurface()
        Dim before As Integer = CardPainter.AttachedCount
        CardPainter.Sync(New Panel() With {.Tag = "Card"}, BuiltInSchemes.Modern())
        CardPainter.Sync(New Button(), BuiltInSchemes.Modern())
        Assert.Equal(before, CardPainter.AttachedCount)
    End Sub

    ''' <summary>Checklist item 5: Modern → Classic → Modern, three times, does not stack hooks.</summary>
    <Fact>
    Public Sub SwitchingSchemesRepeatedly_DoesNotStackHooks()
        Dim card As Panel = CardOnHost("CardSurface")
        Dim modern As ThemeScheme = BuiltInSchemes.Modern()
        Dim classic As ThemeScheme = BuiltInSchemes.Classic()
        Dim baseline As Integer = CardPainter.AttachedCount

        Try
            For i As Integer = 1 To 3
                CardPainter.Sync(card, modern)
                Assert.Equal(baseline + 1, CardPainter.AttachedCount)
                CardPainter.Sync(card, classic)
                Assert.Equal(baseline, CardPainter.AttachedCount)
            Next

            ' And applying the SAME scheme twice in a row is a no-op, not a second hook.
            CardPainter.Sync(card, modern)
            CardPainter.Sync(card, modern)
            Assert.Equal(baseline + 1, CardPainter.AttachedCount)
        Finally
            CardPainter.Sync(card, classic)
        End Try
    End Sub

    ''' <summary>
    ''' The gutter is the parent's padding, and it has to come back. A scheme that widens it and is
    ''' then switched away from must leave the parent laid out exactly as the designer authored it.
    ''' </summary>
    <Fact>
    Public Sub Gutter_IsAppliedToTheParent_AndPutBackOnDetach()
        Dim card As Panel = CardOnHost("CardSurface")
        Dim host As Control = card.Parent
        host.Padding = New Padding(3)
        Dim authored As Padding = host.Padding

        CardPainter.Sync(card, BuiltInSchemes.Modern())
        Assert.True(host.Padding.Left > authored.Left,
                    $"The card gutter never reached the parent: {host.Padding}")

        CardPainter.Sync(card, BuiltInSchemes.Classic())
        Assert.Equal(authored, host.Padding)
    End Sub

    ''' <summary>A neutral scheme asks for no cards, so it must attach nothing in the first place.</summary>
    <Fact>
    Public Sub NeutralScheme_AttachesNothing()
        Dim card As Panel = CardOnHost("CardSurface")
        Dim host As Control = card.Parent
        Dim authored As Padding = host.Padding
        Dim baseline As Integer = CardPainter.AttachedCount

        For Each s As ThemeScheme In New ThemeScheme() {BuiltInSchemes.Classic(),
                                                        BuiltInSchemes.Dark(),
                                                        BuiltInSchemes.Colorful()}
            CardPainter.Sync(card, s)
            Assert.Equal(baseline, CardPainter.AttachedCount)
            Assert.Equal(authored, host.Padding)
        Next
    End Sub

    ''' <summary>
    ''' Checklist item 5, second half: the tinted-icon cache must not grow without bound across
    ''' scheme changes. <c>ThemeManager</c> empties it on every switch; here we prove the emptying
    ''' works, since a cache keyed on a colour nobody uses any more is pure leak.
    ''' </summary>
    <Fact>
    Public Sub IconTint_CacheIsEmptiedOnClear()
        Using src As New Bitmap(16, 16)
            IconTint.Tint(src, Color.Red)
            IconTint.Tint(src, Color.Blue)
            Assert.True(IconTint.CacheCount >= 2)

            IconTint.Clear()
            Assert.Equal(0, IconTint.CacheCount)
        End Using
    End Sub

    ''' <summary>Asking twice for the same tint returns the same bitmap, not a second one.</summary>
    <Fact>
    Public Sub IconTint_ReusesTheCachedResult()
        IconTint.Clear()
        Using src As New Bitmap(16, 16)
            Dim first As Image = IconTint.Tint(src, Color.Green)
            Dim second As Image = IconTint.Tint(src, Color.Green)
            Assert.Same(first, second)
            Assert.Equal(1, IconTint.CacheCount)
        End Using
        IconTint.Clear()
    End Sub

    ''' <summary>A missing image is not an error — it is simply nothing to draw.</summary>
    <Fact>
    Public Sub IconTint_PassesNothingThrough()
        Assert.Null(IconTint.Tint(Nothing, Color.Red))
    End Sub

End Class
