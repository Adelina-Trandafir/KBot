Imports System.ComponentModel
Imports System.Drawing
Imports System.Globalization
Imports KBot.Theming
Imports Xunit

''' <summary>
''' Slice 0028: agregatele LĂRGITE ale benzii de subsol, poarta lor după
''' <see cref="KBotValueType"/>, și regula liniilor verticale.
'''
''' Trei lucruri se verifică aici și nicăieri altundeva:
''' <list type="number">
''' <item>o pereche tip × agregat nepermisă ARUNCĂ — nu se transformă tăcut într-o celulă goală
''' (regula casei), și aruncă pe toate cele trei drumuri: setterul agregatului, setterul tipului
''' și intrarea unei coloane libere în grilă;</item>
''' <item>fiecare agregat nou calculează ce spune că face, inclusiv pe rânduri fără valoare;</item>
''' <item>în subsol se despart DOAR coloanele agregate (<c>FooterDrawsRight/LeftSeparator</c> —
''' regula e o funcție pură tocmai ca să nu fie verificabilă doar cu ochiul).</item>
''' </list>
'''
''' Aserțiile numerice parsează textul formatat înapoi cu CurrentCulture, deci nu depind de
''' separatorul zecimal al mașinii; numărătorile compară direct întregul.
''' </summary>
Public Class KBotDataViewAggregateRulesTests

    Private Shared Function ParseNum(text As String) As Double
        Return Double.Parse(text, NumberStyles.Any, CultureInfo.CurrentCulture)
    End Function

    Private Shared Function Grid() As KBotDataView
        Dim dv As New KBotDataView()
        dv.Size = New Size(600, 400)
        dv.ApplyTheme(BuiltInSchemes.Classic())
        dv.FooterVisible = True
        Return dv
    End Function

    Private Shared Function ColumnOf(dv As KBotDataView, key As String, vt As KBotValueType,
                                     agg As KBotAggregate) As KBotDataColumn
        Dim c = dv.AddColumn(key, key, KBotColumnType.Text, 100)
        c.ValueType = vt
        c.Aggregate = agg
        Return c
    End Function

    ' ── Poarta: ce agregat are voie ce tip ───────────────────────────────────────

    <Fact>
    Public Sub Allowed_TextOffersCountsAndEdges_NeverSumOrAverage()
        Dim permise = KBotAggregateRules.Allowed(KBotValueType.Text)
        Assert.Contains(KBotAggregate.Count, permise)
        Assert.Contains(KBotAggregate.CountDistinct, permise)
        Assert.Contains(KBotAggregate.CountEmpty, permise)
        Assert.Contains(KBotAggregate.First, permise)
        Assert.Contains(KBotAggregate.Last, permise)
        Assert.DoesNotContain(KBotAggregate.Sum, permise)
        Assert.DoesNotContain(KBotAggregate.Average, permise)
        ' Min/Max stau deoparte deliberat la text: „cel mai mic alfabetic” s-ar confunda cu First.
        Assert.DoesNotContain(KBotAggregate.Min, permise)
        Assert.DoesNotContain(KBotAggregate.Max, permise)
    End Sub

    <Fact>
    Public Sub Allowed_NumberOffersSumAverageMinMax()
        Dim permise = KBotAggregateRules.Allowed(KBotValueType.Number)
        Assert.Contains(KBotAggregate.Sum, permise)
        Assert.Contains(KBotAggregate.Average, permise)
        Assert.Contains(KBotAggregate.Min, permise)
        Assert.Contains(KBotAggregate.Max, permise)
        Assert.DoesNotContain(KBotAggregate.CountTrue, permise)
    End Sub

    <Fact>
    Public Sub Allowed_DateTimeOffersMinMax_ButNotSum()
        Dim permise = KBotAggregateRules.Allowed(KBotValueType.DateTime)
        Assert.Contains(KBotAggregate.Min, permise)
        Assert.Contains(KBotAggregate.Max, permise)
        Assert.DoesNotContain(KBotAggregate.Sum, permise)
        Assert.DoesNotContain(KBotAggregate.Average, permise)
    End Sub

    <Fact>
    Public Sub Allowed_BooleanOffersTrueFalseCounts_ButNotSum()
        Dim permise = KBotAggregateRules.Allowed(KBotValueType.Boolean)
        Assert.Contains(KBotAggregate.CountTrue, permise)
        Assert.Contains(KBotAggregate.CountFalse, permise)
        Assert.DoesNotContain(KBotAggregate.Sum, permise)
        Assert.DoesNotContain(KBotAggregate.Min, permise)
    End Sub

    <Fact>
    Public Sub Aggregate_OnWrongValueType_Throws_NotSilentlyEmpty()
        Using dv = Grid()
            Dim c = dv.AddColumn("nume", "Nume", KBotColumnType.Text, 100)   ' ValueType = Text
            Assert.Throws(Of ArgumentException)(Sub() c.Aggregate = KBotAggregate.Sum)
            ' Respingerea nu lasă coloana pe jumătate schimbată.
            Assert.Equal(KBotAggregate.None, c.Aggregate)
        End Using
    End Sub

    <Fact>
    Public Sub ValueTypeChange_ThatWouldOrphanTheAggregate_Throws()
        Using dv = Grid()
            Dim c = ColumnOf(dv, "suma", KBotValueType.Number, KBotAggregate.Sum)
            ' Trecerea pe Text ar lăsa un Sum fără sens: se refuză, nu se stinge tăcut.
            Assert.Throws(Of ArgumentException)(Sub() c.ValueType = KBotValueType.Text)
            Assert.Equal(KBotValueType.Number, c.ValueType)
            ' Calea corectă: întâi cade agregatul, apoi tipul.
            c.Aggregate = KBotAggregate.None
            c.ValueType = KBotValueType.Text
            Assert.Equal(KBotValueType.Text, c.ValueType)
        End Using
    End Sub

    <Fact>
    Public Sub FreeFloatingColumn_WithBadPair_ThrowsWhenItJoinsTheGrid()
        ' O coloană construită liber n-are de la cine afla regula, deci setterul o lasă să treacă.
        Dim libera As New KBotDataColumn("x", "X", KBotColumnType.Text, 80) With {
            .Aggregate = KBotAggregate.Sum
        }
        Using dv = Grid()
            ' Intrarea în grilă e locul unde perechea devine a grilei — și unde se verifică.
            Assert.Throws(Of ArgumentException)(Sub() dv.Columns.Add(libera))
        End Using
    End Sub

    <Fact>
    Public Sub PropertyGrid_OffersOnlyTheAggregatesOfTheColumnsValueType()
        ' Convertorul e ce vede operatorul în grila de proprietăți: pentru o coloană de text nu
        ' trebuie să apară Sum, altfel greșeala se descoperă abia la rulare.
        Using dv = Grid()
            Dim c = dv.AddColumn("nume", "Nume", KBotColumnType.Text, 100)
            Dim prop As PropertyDescriptor = TypeDescriptor.GetProperties(c)("Aggregate")
            Dim valori = prop.Converter.GetStandardValues(New FakeContext(c))
            Assert.DoesNotContain(KBotAggregate.Sum, valori.Cast(Of KBotAggregate)())
            Assert.Contains(KBotAggregate.Count, valori.Cast(Of KBotAggregate)())

            c.ValueType = KBotValueType.Number
            valori = prop.Converter.GetStandardValues(New FakeContext(c))
            Assert.Contains(KBotAggregate.Sum, valori.Cast(Of KBotAggregate)())
        End Using
    End Sub

    ' Contextul minim de care are nevoie convertorul: doar Instance (coloana editată).
    Private NotInheritable Class FakeContext
        Implements ITypeDescriptorContext

        Private ReadOnly _instance As Object

        Public Sub New(instance As Object)
            _instance = instance
        End Sub

        Public ReadOnly Property Container As IContainer Implements ITypeDescriptorContext.Container
            Get
                Return Nothing
            End Get
        End Property

        Public ReadOnly Property Instance As Object Implements ITypeDescriptorContext.Instance
            Get
                Return _instance
            End Get
        End Property

        Public ReadOnly Property PropertyDescriptor As PropertyDescriptor Implements ITypeDescriptorContext.PropertyDescriptor
            Get
                Return Nothing
            End Get
        End Property

        Public Function GetService(serviceType As Type) As Object Implements IServiceProvider.GetService
            Return Nothing
        End Function

        Public Sub OnComponentChanged() Implements ITypeDescriptorContext.OnComponentChanged
        End Sub

        Public Function OnComponentChanging() As Boolean Implements ITypeDescriptorContext.OnComponentChanging
            Return True
        End Function
    End Class

    ' ── Agregatele noi ───────────────────────────────────────────────────────────

    <Fact>
    Public Sub MinMax_OnNumbers_SkipNonNumericCells()
        Using dv = Grid()
            ColumnOf(dv, "v", KBotValueType.Number, KBotAggregate.Min)
            dv.BeginUpdate()
            dv.AddRow()("v") = 30.0
            dv.AddRow()("v") = 10.0
            dv.AddRow()("v") = "nu-i număr"
            dv.AddRow()("v") = 20.0
            dv.EndUpdate()
            Assert.Equal(10.0, ParseNum(dv.DebugFooterText("v")), 2)

            dv.Column("v").Aggregate = KBotAggregate.Max
            Assert.Equal(30.0, ParseNum(dv.DebugFooterText("v")), 2)
        End Using
    End Sub

    <Fact>
    Public Sub MinMax_OnDates_CompareAsDates_NotAsText()
        Using dv = Grid()
            Dim c = ColumnOf(dv, "d", KBotValueType.DateTime, KBotAggregate.Min)
            c.AggregateFormatString = "yyyy-MM-dd"
            dv.BeginUpdate()
            dv.AddRow()("d") = New Date(2026, 3, 9)
            dv.AddRow()("d") = New Date(2026, 1, 31)     ' text-ul „31…” ar fi cel mai MARE alfabetic
            dv.AddRow()("d") = New Date(2026, 12, 1)
            dv.EndUpdate()
            Assert.Equal("2026-01-31", dv.DebugFooterText("d"))

            dv.Column("d").Aggregate = KBotAggregate.Max
            Assert.Equal("2026-12-01", dv.DebugFooterText("d"))
        End Using
    End Sub

    <Fact>
    Public Sub MinMax_WithNoUsableCells_IsEmpty_NotZero()
        Using dv = Grid()
            ColumnOf(dv, "v", KBotValueType.Number, KBotAggregate.Min)
            dv.AddRow()("v") = "abc"
            Assert.Equal(String.Empty, dv.DebugFooterText("v"))
        End Using
    End Sub

    <Fact>
    Public Sub CountDistinct_CountsWhatTheEyeSees_ThroughTheFormatString()
        Using dv = Grid()
            Dim c = ColumnOf(dv, "d", KBotValueType.DateTime, KBotAggregate.CountDistinct)
            c.FormatString = "dd.MM.yyyy"                ' orele nu se văd, deci nu se numără
            dv.BeginUpdate()
            dv.AddRow()("d") = New Date(2026, 5, 1, 8, 0, 0)
            dv.AddRow()("d") = New Date(2026, 5, 1, 17, 30, 0)
            dv.AddRow()("d") = New Date(2026, 5, 2, 9, 0, 0)
            dv.EndUpdate()
            Assert.Equal("2", dv.DebugFooterText("d"))
        End Using
    End Sub

    <Fact>
    Public Sub CountEmpty_CountsMissing_Nothing_AndBlankText()
        Using dv = Grid()
            ColumnOf(dv, "t", KBotValueType.Text, KBotAggregate.CountEmpty)
            dv.BeginUpdate()
            dv.AddRow()("t") = "plin"
            dv.AddRow()("t") = Nothing                   ' valoare stocată, dar goală
            dv.AddRow()("t") = "   "                     ' numai spații
            dv.AddRow()                                  ' celulă niciodată scrisă
            dv.EndUpdate()
            Assert.Equal("3", dv.DebugFooterText("t"))
        End Using
    End Sub

    <Fact>
    Public Sub CountTrueFalse_IgnoreRowsThatNeverStoredAValue()
        Using dv = Grid()
            Dim c = dv.AddColumn("b", "Bifat", KBotColumnType.CheckBox, 60)
            c.ValueType = KBotValueType.Boolean
            c.Aggregate = KBotAggregate.CountTrue
            dv.BeginUpdate()
            dv.AddRow()("b") = True
            dv.AddRow()("b") = True
            dv.AddRow()("b") = False
            dv.AddRow()                                  ' absentă: nici bifată, nici debifată
            dv.EndUpdate()
            Assert.Equal("2", dv.DebugFooterText("b"))

            dv.Column("b").Aggregate = KBotAggregate.CountFalse
            Assert.Equal("1", dv.DebugFooterText("b"))
        End Using
    End Sub

    <Fact>
    Public Sub FirstLast_TakeTheEdgesOfTheModel_SkippingEmptyCells()
        Using dv = Grid()
            ColumnOf(dv, "t", KBotValueType.Text, KBotAggregate.First)
            dv.BeginUpdate()
            dv.AddRow()                                  ' goală: se sare
            dv.AddRow()("t") = "alfa"
            dv.AddRow()("t") = "beta"
            dv.AddRow()("t") = "   "                     ' goală: se sare și la coadă
            dv.EndUpdate()
            Assert.Equal("alfa", dv.DebugFooterText("t"))

            dv.Column("t").Aggregate = KBotAggregate.Last
            Assert.Equal("beta", dv.DebugFooterText("t"))
        End Using
    End Sub

    <Fact>
    Public Sub ChangingTheAggregate_RefreshesTheBandImmediately()
        Using dv = Grid()
            Dim c = ColumnOf(dv, "v", KBotValueType.Number, KBotAggregate.Sum)
            dv.BeginUpdate()
            dv.AddRow()("v") = 10.0
            dv.AddRow()("v") = 30.0
            dv.EndUpdate()
            Assert.Equal(40.0, ParseNum(dv.DebugFooterText("v")), 2)

            c.Aggregate = KBotAggregate.Average          ' fără alt apel: banda se recalculează
            Assert.Equal(20.0, ParseNum(dv.DebugFooterText("v")), 2)
        End Using
    End Sub

    ' ── Regula liniilor verticale ────────────────────────────────────────────────

    <Fact>
    Public Sub FooterSeparators_OnlyAroundAggregatedColumns()
        Using dv = Grid()
            Dim gol1 = dv.AddColumn("a", "A", KBotColumnType.Text, 100)
            Dim suma = ColumnOf(dv, "b", KBotValueType.Number, KBotAggregate.Sum)
            Dim gol2 = dv.AddColumn("c", "C", KBotColumnType.Text, 100)

            ' Coloana neagregată nu poartă NICIO linie — nici la dreapta, nici la stânga.
            Assert.False(KBotDataView.FooterDrawsRightSeparator(gol1))
            Assert.False(KBotDataView.FooterDrawsLeftSeparator(gol1, Nothing))
            Assert.False(KBotDataView.FooterDrawsRightSeparator(gol2))
            Assert.False(KBotDataView.FooterDrawsLeftSeparator(gol2, suma))

            ' Cea agregată e închisă între două linii, fiindcă vecina din stânga n-a desenat una.
            Assert.True(KBotDataView.FooterDrawsRightSeparator(suma))
            Assert.True(KBotDataView.FooterDrawsLeftSeparator(suma, gol1))
        End Using
    End Sub

    <Fact>
    Public Sub FooterSeparators_TwoAggregatesInARow_ShareOneLine()
        Using dv = Grid()
            Dim s1 = ColumnOf(dv, "s1", KBotValueType.Number, KBotAggregate.Sum)
            Dim s2 = ColumnOf(dv, "s2", KBotValueType.Number, KBotAggregate.Sum)
            ' Vecina din stânga e agregată, deci muchia comună e deja desenată de ea: a doua
            ' coloană nu o mai desenează încă o dată (linie dublă pe același pixel).
            Assert.True(KBotDataView.FooterDrawsRightSeparator(s1))
            Assert.False(KBotDataView.FooterDrawsLeftSeparator(s2, s1))
            Assert.True(KBotDataView.FooterDrawsRightSeparator(s2))
        End Using
    End Sub

    <Fact>
    Public Sub FooterBand_PaintsWithoutThrowing_WhenNothingIsAggregated()
        Using dv = Grid()
            dv.AddColumn("a", "A", KBotColumnType.Text, 100)
            dv.AddColumn("b", "B", KBotColumnType.Text, 100)
            dv.AddRow()("a") = "x"
            Using bmp As New Bitmap(dv.Width, dv.Height)
                dv.DrawToBitmap(bmp, New Rectangle(0, 0, dv.Width, dv.Height))
            End Using
        End Using
    End Sub

End Class
