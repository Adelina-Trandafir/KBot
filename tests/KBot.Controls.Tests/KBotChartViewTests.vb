Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Threading
Imports System.Windows.Forms
Imports Xunit
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' The contract of <see cref="KBotChartView"/>: designer cleanliness, the rounded value axis, the
''' projection of points, the single-select band, key validation, hit testing and the fact that
''' applying a theme does not pin anything into a host form.
'''
''' What these tests CANNOT prove: the round trip through the real Visual Studio designer (the
''' collection dialog, the lines written into <c>*.Designer.vb</c>, the red frame painted on the
''' design surface) and what the chart actually looks like on a screen. Those stay manual checks,
''' recorded in the worklog as NOT RUN — the same limitation acknowledged for every other control
''' in this project.
''' </summary>
Public Class KBotChartViewTests

    ' The control is WinForms: it is built on an STA thread, like the sister suites.
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

    ' A chart with a real size, so RecalcLayout produces a non-empty plot.
    Private Shared Function NewSizedChart() As KBotChartView
        Dim chart As New KBotChartView()
        chart.Size = New Size(480, 260)
        Return chart
    End Function

    Private Shared Function CentreOfTab(chart As KBotChartView, index As Integer) As Point
        Dim r As Rectangle = chart.DebugTabBounds(index)
        Return New Point(r.Left + r.Width \ 2, r.Top + r.Height \ 2)
    End Function

    ' ── 1. Designer cleanliness ───────────────────────────────────────────────

    ''' <summary>
    ''' A freshly dropped chart must write ZERO property lines into the host form. This is the path
    ''' Visual Studio actually takes (<c>TypeDescriptor</c>), not our own ShouldSerialize methods —
    ''' calling those directly would prove nothing.
    ''' </summary>
    <Fact>
    Public Sub FreshChart_SerializesNothing()
        RunSta(Sub()
                   Using chart = New KBotChartView()
                       Dim props As PropertyDescriptorCollection = TypeDescriptor.GetProperties(chart)
                       For Each name As String In {"BackColor", "ForeColor", "Font", "Size",
                                                   "HeaderFont", "AxisFont", "TabIconSize",
                                                   "HeaderBackColor", "HeaderTextColor",
                                                   "HeaderSeparatorColor", "PlotBackColor",
                                                   "BorderColor", "AxisColor", "AxisTextColor",
                                                   "GridColor", "LegendTextColor", "EmptyTextColor"}
                           Dim pd As PropertyDescriptor = props(name)
                           Assert.True(pd IsNot Nothing, $"{name} is missing from the property grid")
                           Assert.False(pd.ShouldSerializeValue(chart), $"{name} is serialized for nothing")
                       Next
                   End Using
               End Sub)
    End Sub

    ''' <summary>
    ''' Applying a theme writes colours through <c>MyBase</c>, so it must NOT make the host form
    ''' freeze them. This is the defect the pin flags exist for.
    ''' </summary>
    <Fact>
    Public Sub ApplyTheme_DoesNotPinInheritedProperties()
        RunSta(Sub()
                   Using chart = NewSizedChart()
                       chart.ApplyTheme(BuiltInSchemes.Classic())
                       Dim props As PropertyDescriptorCollection = TypeDescriptor.GetProperties(chart)
                       Assert.False(props("BackColor").ShouldSerializeValue(chart))
                       Assert.False(props("ForeColor").ShouldSerializeValue(chart))

                       ' A colour the operator sets explicitly does win, and keeps winning.
                       chart.BackColor = Color.Fuchsia
                       Assert.True(props("BackColor").ShouldSerializeValue(chart))
                       chart.ResetBackColor()
                       Assert.False(props("BackColor").ShouldSerializeValue(chart))
                   End Using
               End Sub)
    End Sub

    ' ── 2. The value axis ─────────────────────────────────────────────────────

    <Fact>
    Public Sub ValueAxis_RoundsOutToWholeTicks()
        RunSta(Sub()
                   Using chart = NewSizedChart()
                       Dim s = chart.AddSeries("a", "Alfa")
                       s.AddPoint(New Date(2026, 1, 1), 0)
                       s.AddPoint(New Date(2026, 2, 1), 3271)

                       Dim axis As Double() = chart.DebugValueAxis()
                       Assert.Equal(0.0, axis(0))
                       Assert.Equal(4000.0, axis(1))
                       Assert.Equal(1000.0, axis(2))
                   End Using
               End Sub)
    End Sub

    ''' <summary>
    ''' A flat line still needs a scale to sit on: without the one-unit spread the line would
    ''' collapse onto the baseline and read as "nothing happened at all".
    ''' </summary>
    <Fact>
    Public Sub ValueAxis_FlatSeries_StillHasASpread()
        RunSta(Sub()
                   Using chart = NewSizedChart()
                       chart.ValueAxisMode = KBotChartValueAxisMode.FromMinimum
                       Dim s = chart.AddSeries("a", "Alfa")
                       s.AddPoint(New Date(2026, 1, 1), 500)
                       s.AddPoint(New Date(2026, 2, 1), 500)

                       Dim axis As Double() = chart.DebugValueAxis()
                       Assert.True(axis(1) > axis(0))
                   End Using
               End Sub)
    End Sub

    ' ── 3. Projection ─────────────────────────────────────────────────────────

    <Fact>
    Public Sub Points_AreProjectedInsideThePlot_AndInTimeOrder()
        RunSta(Sub()
                   Using chart = NewSizedChart()
                       Dim s = chart.AddSeries("a", "Alfa")
                       s.AddPoint(New Date(2026, 1, 1), 100)
                       s.AddPoint(New Date(2026, 3, 1), 400)

                       Dim plot As Rectangle = chart.DebugPlotRect()
                       Dim first As Point = chart.DebugPointLocation(0, 0)
                       Dim last As Point = chart.DebugPointLocation(0, 1)

                       Assert.Equal(plot.Left, first.X)
                       Assert.Equal(plot.Right, last.X)
                       ' A bigger value sits HIGHER, that is, at a smaller Y.
                       Assert.True(last.Y < first.Y)
                       Assert.True(first.Y <= plot.Bottom AndAlso last.Y >= plot.Top)
                   End Using
               End Sub)
    End Sub

    ''' <summary>
    ''' Everything at one instant gives a time span of zero. The point goes in the middle: a
    ''' measurement with nothing to be earlier or later than has no other honest place.
    ''' </summary>
    <Fact>
    Public Sub SinglePoint_SitsInTheMiddleOfThePlot()
        RunSta(Sub()
                   Using chart = NewSizedChart()
                       Dim s = chart.AddSeries("a", "Alfa")
                       s.AddPoint(New Date(2026, 1, 1), 100)

                       Dim plot As Rectangle = chart.DebugPlotRect()
                       Assert.Equal(plot.Left + plot.Width \ 2, chart.DebugPointLocation(0, 0).X)
                   End Using
               End Sub)
    End Sub

    ' ── 4. The band ───────────────────────────────────────────────────────────

    <Fact>
    Public Sub Tabs_ClickRaisesTabSelectedOnce_AndNotForTheCurrentOne()
        RunSta(Sub()
                   Using chart = NewSizedChart()
                       chart.AddTab("one", "One")
                       chart.AddTab("two", "Two")
                       chart.SelectedTabKey = "one"

                       Dim seen As New List(Of String)()
                       AddHandler chart.TabSelected, Sub(k As String) seen.Add(k)

                       chart.DebugClickAt(CentreOfTab(chart, 1))
                       Assert.Equal(New String() {"two"}, seen.ToArray())
                       Assert.Equal("two", chart.SelectedTabKey)

                       ' Pressing the button that is already current is not a choice.
                       chart.DebugClickAt(CentreOfTab(chart, 1))
                       Assert.Single(seen)
                   End Using
               End Sub)
    End Sub

    ''' <summary>
    ''' <see cref="KBotChartView.SelectedTabKey"/> is the host stating a fact, not the operator
    ''' choosing, so it stays quiet. <c>SelectTab</c> is the road that speaks.
    ''' </summary>
    <Fact>
    Public Sub SelectedTabKey_IsSilent_ButSelectTabRaises()
        RunSta(Sub()
                   Using chart = NewSizedChart()
                       chart.AddTab("one", "One")
                       chart.AddTab("two", "Two")

                       Dim seen As New List(Of String)()
                       AddHandler chart.TabSelected, Sub(k As String) seen.Add(k)

                       chart.SelectedTabKey = "one"
                       Assert.Empty(seen)

                       chart.SelectTab("two")
                       Assert.Equal(New String() {"two"}, seen.ToArray())
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Tabs_DisabledTabDoesNotSwitch()
        RunSta(Sub()
                   Using chart = NewSizedChart()
                       chart.AddTab("one", "One")
                       Dim two = chart.AddTab("two", "Two")
                       two.Enabled = False
                       chart.SelectedTabKey = "one"

                       chart.DebugClickAt(CentreOfTab(chart, 1))
                       Assert.Equal("one", chart.SelectedTabKey)
                   End Using
               End Sub)
    End Sub

    ' ── 5. Keys ───────────────────────────────────────────────────────────────

    <Fact>
    Public Sub Keys_DuplicateOrEmpty_AreRefused()
        RunSta(Sub()
                   Using chart = NewSizedChart()
                       chart.AddTab("one", "One")
                       Assert.Throws(Of ArgumentException)(Sub() chart.AddTab("one", "Again"))
                       Assert.Throws(Of ArgumentException)(Sub() chart.AddTab("  ", "Blank"))

                       chart.AddSeries("a", "Alfa")
                       Assert.Throws(Of ArgumentException)(Sub() chart.AddSeries("a", "Again"))
                       Assert.Throws(Of ArgumentException)(Sub() chart.SetSeriesVisible("nope", False))
                       Assert.Throws(Of ArgumentException)(Sub() chart.SelectTab("nope"))
                   End Using
               End Sub)
    End Sub

    ''' <summary>
    ''' The collection dialog inserts an empty item the moment Add is pressed, so the collection
    ''' cannot validate. <c>EndInit</c> is where the contract is enforced.
    ''' </summary>
    <Fact>
    Public Sub EndInit_RefusesADuplicateKeyTypedIntoTheCollection()
        RunSta(Sub()
                   Using chart = NewSizedChart()
                       Dim init As ISupportInitialize = chart
                       init.BeginInit()
                       chart.Tabs.Add(New KBotChartTab("same", "One"))
                       chart.Tabs.Add(New KBotChartTab("same", "Two"))
                       Assert.Throws(Of ArgumentException)(Sub() init.EndInit())
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Collections_RefuseNothing()
        RunSta(Sub()
                   Using chart = NewSizedChart()
                       Assert.Throws(Of ArgumentNullException)(Sub() chart.Tabs.Add(Nothing))
                       Assert.Throws(Of ArgumentNullException)(Sub() chart.Series.Add(Nothing))
                   End Using
               End Sub)
    End Sub

    ' ── 6. Hit testing ────────────────────────────────────────────────────────

    <Fact>
    Public Sub HitTest_FindsTheNearestMarker_AndNothingFarFromOne()
        RunSta(Sub()
                   Using chart = NewSizedChart()
                       Dim s = chart.AddSeries("a", "Alfa")
                       s.AddPoint(New Date(2026, 1, 1), 100)
                       s.AddPoint(New Date(2026, 3, 1), 400)

                       Dim at As Point = chart.DebugPointLocation(0, 1)
                       Dim hit As Integer() = chart.DebugHitTest(New Point(at.X - 2, at.Y + 2))
                       Assert.Equal(0, hit(0))
                       Assert.Equal(1, hit(1))

                       Dim plot As Rectangle = chart.DebugPlotRect()
                       Dim miss As Integer() = chart.DebugHitTest(New Point(plot.Left + plot.Width \ 2, plot.Top + 2))
                       Assert.Equal(-1, miss(0))
                       Assert.Equal(-1, miss(1))
                   End Using
               End Sub)
    End Sub

    ''' <summary>
    ''' Two markers in the same place: the emphasised series wins, because that is the line drawn
    ''' on top and therefore the one the operator reached for.
    ''' </summary>
    <Fact>
    Public Sub HitTest_EmphasisWinsATie()
        RunSta(Sub()
                   Using chart = NewSizedChart()
                       Dim moment As Date = New Date(2026, 1, 1)
                       Dim plain = chart.AddSeries("a", "Alfa")
                       plain.AddPoint(moment, 100)
                       Dim total = chart.AddSeries("total", "Total")
                       total.Emphasis = True
                       total.AddPoint(moment, 100)

                       Dim at As Point = chart.DebugPointLocation(0, 0)
                       Dim hit As Integer() = chart.DebugHitTest(at)
                       Assert.Equal(1, hit(0))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub HiddenSeries_IsNeitherProjectedNorHitTested()
        RunSta(Sub()
                   Using chart = NewSizedChart()
                       Dim s = chart.AddSeries("a", "Alfa")
                       s.AddPoint(New Date(2026, 1, 1), 100)
                       s.AddPoint(New Date(2026, 3, 1), 400)
                       Dim at As Point = chart.DebugPointLocation(0, 1)

                       chart.SetSeriesVisible("a", False)
                       Assert.Equal(Point.Empty, chart.DebugPointLocation(0, 1))
                       Assert.Equal(-1, chart.DebugHitTest(at)(0))
                   End Using
               End Sub)
    End Sub

    ' ── 7. It actually paints ─────────────────────────────────────────────────

    ''' <summary>
    ''' The chart is drawn into a bitmap on both roads — with data and empty — because a paint body
    ''' that throws is swallowed by design, so a broken one would otherwise fail silently.
    ''' </summary>
    <Fact>
    Public Sub Paints_WithDataAndWhenEmpty()
        RunSta(Sub()
                   Using chart = NewSizedChart()
                       chart.EmptyText = "Nimic de arătat."
                       chart.AddTab("one", "One")
                       chart.AddTab("two", "Two")
                       chart.SelectedTabKey = "one"
                       chart.ApplyTheme(BuiltInSchemes.Classic())

                       Using empty As New Bitmap(chart.Width, chart.Height)
                           chart.DrawToBitmap(empty, New Rectangle(0, 0, chart.Width, chart.Height))
                       End Using

                       Dim s = chart.AddSeries("a", "Alfa")
                       s.FillArea = True
                       s.AddPoint(New Date(2026, 1, 1), 100)
                       s.AddPoint(New Date(2026, 2, 1), 250)
                       s.AddPoint(New Date(2026, 3, 1), 180)
                       Dim total = chart.AddSeries("total", "Total")
                       total.Emphasis = True
                       total.AddPoint(New Date(2026, 1, 1), 300)
                       total.AddPoint(New Date(2026, 3, 1), 520)

                       Using drawn As New Bitmap(chart.Width, chart.Height)
                           chart.DrawToBitmap(drawn, New Rectangle(0, 0, chart.Width, chart.Height))
                       End Using
                   End Using
               End Sub)
    End Sub

End Class
