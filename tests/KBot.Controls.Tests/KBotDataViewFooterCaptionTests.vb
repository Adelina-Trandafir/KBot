Imports System.Drawing
Imports KBot.Theming
Imports Xunit

''' <summary>
''' Tests for the footer caption zone (slice 0028-02): the text + icon that live in the left part of
''' the footer band. The one rule worth pinning down is WHERE THE ZONE STOPS — at the first
''' aggregated column, never further, so a caption can not read like the label of somebody's
''' total. The rest covers the icon's rectangle (drawn only when it fits whole, and clickable
''' exactly where it is drawn) and the collapse button's corner staying its own.
''' </summary>
Public Class KBotDataViewFooterCaptionTests

    Private Shared Function Icon(side As Integer) As Image
        Return New Bitmap(side, side)
    End Function

    ' Trei coloane de 120, a doua agregată => zona de titlu ține de la 0 la 120.
    Private Shared Function Grid() As KBotDataView
        Dim dv As New KBotDataView()
        dv.Size = New Size(600, 300)
        dv.AutoSizeColumnsMode = KBotAutoSizeMode.None
        dv.ApplyTheme(BuiltInSchemes.Classic())
        dv.AddColumn("cod", "Cod", KBotColumnType.Text, 120)
        Dim s = dv.AddColumn("suma", "Suma", KBotColumnType.Text, 120)
        s.ValueType = KBotValueType.Number
        s.Aggregate = KBotAggregate.Sum
        dv.AddColumn("obs", "Observații", KBotColumnType.Text, 120)
        dv.FooterVisible = True
        Return dv
    End Function

    Private Shared Function Band(dv As KBotDataView) As Rectangle
        Return New Rectangle(0, dv.ClientSize.Height - dv.FooterHeight, dv.ClientSize.Width, dv.FooterHeight)
    End Function

    ' ── Zona ─────────────────────────────────────────────────────────────────────

    <Fact>
    Public Sub TheZoneStopsAtTheFirstAggregatedColumn()
        Using dv = Grid()
            Dim zone As Rectangle = dv.FooterCaptionZone(Band(dv))
            Assert.Equal(0, zone.Left)
            Assert.Equal(120, zone.Right)              ' marginea stângă a coloanei «suma»
        End Using
    End Sub

    <Fact>
    Public Sub WithoutAnyAggregate_TheZoneIsTheWholeBand()
        Using dv = Grid()
            dv.Column("suma").Aggregate = KBotAggregate.None

            Dim zone As Rectangle = dv.FooterCaptionZone(Band(dv))
            Assert.Equal(0, zone.Left)
            Assert.Equal(dv.ClientSize.Width, zone.Right)
        End Using
    End Sub

    <Fact>
    Public Sub AnAggregateFurtherRight_GivesTheCaptionMoreRoom()
        Using dv = Grid()
            dv.Column("suma").Aggregate = KBotAggregate.None
            dv.Column("obs").Aggregate = KBotAggregate.Count

            Assert.Equal(240, dv.FooterCaptionZone(Band(dv)).Right)
        End Using
    End Sub

    <Fact>
    Public Sub TheCollapseButtonsCornerIsNotSharedWithTheCaption()
        Using dv = Grid()
            Dim fara As Integer = dv.FooterCaptionZone(Band(dv)).Left
            Assert.Equal(0, fara)

            dv.CollapseButton = True
            dv.CollapseButtonPosition = KBotFooterButtonPosition.Left
            ' Butonul stă în stânga, deci titlul începe DUPĂ el, nu sub el.
            Assert.True(dv.FooterCaptionZone(Band(dv)).Left > fara)
        End Using
    End Sub

    ' ── Pictograma ───────────────────────────────────────────────────────────────

    <Fact>
    Public Sub NoIcon_NoRectangle_NoClick()
        Using dv = Grid()
            dv.FooterCaption = "Total pe lună"
            Assert.True(dv.FooterLeftIconRect.IsEmpty)
            Assert.False(dv.HandleFooterIconMouseDown(New Point(10, Band(dv).Top + 5)))
        End Using
    End Sub

    <Fact>
    Public Sub TheIconSitsAtTheStartOfTheZone()
        Using dv = Grid()
            dv.FooterCaption = "Total pe lună"
            dv.FooterLeftIcon = Icon(16)

            Dim r As Rectangle = dv.FooterLeftIconRect
            Assert.False(r.IsEmpty)
            Assert.Equal(KBotDataColumn.HeaderIconPad, r.Left)
            Assert.Equal(New Size(16, 16), r.Size)
            Assert.True(r.Right <= dv.FooterCaptionZone(Band(dv)).Right)
        End Using
    End Sub

    <Fact>
    Public Sub AnIconThatDoesNotFitTheZoneIsNotDrawnAtAll()
        Using dv = Grid()
            dv.FooterLeftIcon = Icon(16)
            ' Prima coloană devine agregată: zona se strânge la lățime zero.
            dv.Column("cod").Aggregate = KBotAggregate.Count

            Assert.Equal(0, dv.FooterCaptionZone(Band(dv)).Width)
            Assert.True(dv.FooterLeftIconRect.IsEmpty)
        End Using
    End Sub

    <Fact>
    Public Sub FooterHidden_MeansNoIconRectangle()
        Using dv = Grid()
            dv.FooterLeftIcon = Icon(16)
            dv.FooterVisible = False
            Assert.True(dv.FooterLeftIconRect.IsEmpty)
        End Using
    End Sub

    <Fact>
    Public Sub ClickingTheIconRaisesTheEvent()
        Using dv = Grid()
            dv.FooterCaption = "Total pe lună"
            dv.FooterLeftIcon = Icon(16)

            Dim apasari As Integer = 0
            AddHandler dv.FooterLeftIconClicked, Sub(s, e) apasari += 1

            Dim r As Rectangle = dv.FooterLeftIconRect
            Assert.True(dv.HandleFooterIconMouseDown(New Point(r.Left + 8, r.Top + 8)))
            Assert.Equal(1, apasari)

            ' Lângă pictogramă, în bandă: nu e apăsarea ei.
            Assert.False(dv.HandleFooterIconMouseDown(New Point(r.Right + 40, r.Top + 8)))
            Assert.Equal(1, apasari)
        End Using
    End Sub

    <Fact>
    Public Sub HoverFollowsTheCursorOverTheIcon()
        Using dv = Grid()
            dv.FooterLeftIcon = Icon(16)
            Dim r As Rectangle = dv.FooterLeftIconRect

            Assert.True(dv.UpdateFooterIconHover(New Point(r.Left + 8, r.Top + 8)))
            Assert.True(dv.FooterIconHovered)

            Assert.False(dv.UpdateFooterIconHover(New Point(r.Right + 40, r.Top + 8)))
            Assert.False(dv.FooterIconHovered)
        End Using
    End Sub

    <Fact>
    Public Sub HoverColor_ComesFromTheThemeUntilItIsPinned()
        Using dv = Grid()
            Assert.Equal(Color.Empty, dv.FooterLeftIconHoverColor)
            Assert.NotEqual(Color.Empty, dv.FooterIconHoverResolved())

            dv.FooterLeftIconHoverColor = Color.Red
            Assert.Equal(Color.Red, dv.FooterIconHoverResolved())
        End Using
    End Sub

    <Fact>
    Public Sub PaintingWithACaptionDoesNotThrow()
        Using dv = Grid()
            dv.FooterCaption = "Un titlu de subsol destul de lung ca să se taie"
            dv.FooterLeftIcon = Icon(16)
            dv.CollapseButton = True
            dv.AddRow() : dv("suma", 0) = 10.0

            Using bmp As New Bitmap(dv.Width, dv.Height)
                dv.DrawToBitmap(bmp, New Rectangle(0, 0, dv.Width, dv.Height))
            End Using
        End Using
    End Sub

End Class
