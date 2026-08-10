Imports System.ComponentModel
Imports System.Drawing
Imports System.Globalization
Imports KBot.Theming
Imports Xunit

''' <summary>
''' Slice 0028: <see cref="KBotDataColumn.DecimalPlaces"/> — câte zecimale se AFIȘEAZĂ pe o coloană
''' numerică, cu rotunjire NORMALĂ (0,5 în sus).
'''
''' Testul care contează cel mai mult e cel de la mijloc de interval: <c>Math.Round</c> rotunjește
''' implicit „la par” (2,5 => 2), deci o implementare scrisă fără <c>MidpointRounding.AwayFromZero</c>
''' trece toate celelalte teste și cade doar în nota contabilă a operatorului.
'''
''' Textul se citește prin <c>DebugFooterText</c> (subsol) și prin evenimentul de formatare
''' (celulă) — adică exact șirurile care ajung pe ecran. Aserțiile care compară numere le parsează
''' înapoi cu CurrentCulture, ca să nu depindă de separatorul zecimal al mașinii; cele care verifică
''' NUMĂRUL de zecimale numără cifrele de după separatorul culturii curente.
''' </summary>
Public Class KBotDataViewDecimalPlacesTests

    Private Shared Function ParseNum(text As String) As Double
        Return Double.Parse(text, NumberStyles.Any, CultureInfo.CurrentCulture)
    End Function

    ' Câte zecimale are efectiv textul (0 dacă n-are separator zecimal).
    Private Shared Function Zecimale(text As String) As Integer
        Dim sep As String = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator
        Dim i As Integer = text.IndexOf(sep, StringComparison.Ordinal)
        If i < 0 Then Return 0
        Return text.Length - i - sep.Length
    End Function

    Private Shared Function Grid(places As Integer, Optional agg As KBotAggregate = KBotAggregate.Sum) As KBotDataView
        Dim dv As New KBotDataView()
        dv.Size = New Size(600, 400)
        dv.ApplyTheme(BuiltInSchemes.Classic())
        Dim c = dv.AddColumn("v", "Valoare", KBotColumnType.Text, 140)
        c.ValueType = KBotValueType.Number
        c.DecimalPlaces = places
        c.Aggregate = agg
        dv.FooterVisible = True
        Return dv
    End Function

    ' Textul pe care l-ar picta o celulă: aceeași trecere de formatare ca pictarea.
    Private Shared Function TextCelula(dv As KBotDataView, colKey As String, rowIndex As Integer) As String
        Dim vazut As String = Nothing
        Dim h As EventHandler(Of KBotCellFormattingEventArgs) =
            Sub(s As Object, e As KBotCellFormattingEventArgs)
                If e.RowIndex = rowIndex AndAlso String.Equals(e.ColumnKey, colKey, StringComparison.Ordinal) Then
                    vazut = e.Text
                End If
            End Sub
        AddHandler dv.CellFormatting, h
        Try
            Using bmp As New Bitmap(dv.Width, dv.Height)
                dv.DrawToBitmap(bmp, New Rectangle(0, 0, dv.Width, dv.Height))
            End Using
        Finally
            RemoveHandler dv.CellFormatting, h
        End Try
        Return vazut
    End Function

    ' ── Rotunjirea ───────────────────────────────────────────────────────────────

    <Theory>
    <InlineData(1.234, 2, 1.23)>
    <InlineData(1.236, 2, 1.24)>
    <InlineData(-1.236, 2, -1.24)>
    <InlineData(1.4999, 0, 1.0)>
    <InlineData(123.456789, 4, 123.4568)>
    Public Sub CellText_RoundsToTheColumnsPlaces(brut As Double, zecimale As Integer, asteptat As Double)
        Using dv = Grid(zecimale)
            dv.AddRow()("v") = brut
            Assert.Equal(asteptat, ParseNum(TextCelula(dv, "v", 0)), 6)
        End Using
    End Sub

    <Theory>
    <InlineData(2.5, 0, 3.0)>          ' .NET implicit ar da 2 (rotunjire „la par”)
    <InlineData(3.5, 0, 4.0)>
    <InlineData(-2.5, 0, -3.0)>        ' departe de zero și în minus
    <InlineData(0.125, 2, 0.13)>
    <InlineData(0.135, 2, 0.14)>
    Public Sub Midpoint_GoesUp_LikeSchoolRounding_NotBankers(brut As Double, zecimale As Integer, asteptat As Double)
        Using dv = Grid(zecimale)
            dv.AddRow()("v") = brut
            Assert.Equal(asteptat, ParseNum(TextCelula(dv, "v", 0)), 6)
        End Using
    End Sub

    <Fact>
    Public Sub Places_AreWritten_NotJustRounded()
        ' 2 zecimale înseamnă DOUĂ zecimale scrise: altfel «2,50» s-ar afișa «2,5» și rotunjirea
        ' ar fi invizibilă pe o coloană de bani.
        Using dv = Grid(2)
            dv.AddRow()("v") = 2.5
            Assert.Equal(2, Zecimale(TextCelula(dv, "v", 0)))
        End Using
    End Sub

    <Fact>
    Public Sub AnExplicitFormatString_StillWins_ButSeesTheRoundedValue()
        Using dv As New KBotDataView()
            dv.Size = New Size(600, 400)
            dv.ApplyTheme(BuiltInSchemes.Classic())
            Dim c = dv.AddColumn("v", "Valoare", KBotColumnType.Text, 140)
            c.ValueType = KBotValueType.Number
            c.DecimalPlaces = 2
            c.FormatString = "N3"          ' formatul cere 3 zecimale...
            dv.AddRow()("v") = 1.238

            ' ...dar valoarea a fost deja rotunjită la 2, deci a treia zecimală e zero.
            Assert.Equal(1.24, ParseNum(TextCelula(dv, "v", 0)), 6)
            Assert.Equal(3, Zecimale(TextCelula(dv, "v", 0)))
        End Using
    End Sub

    <Fact>
    Public Sub Unset_ChangesNothing()
        Using dv As New KBotDataView()
            dv.Size = New Size(600, 400)
            dv.ApplyTheme(BuiltInSchemes.Classic())
            Dim c = dv.AddColumn("v", "Valoare", KBotColumnType.Text, 140)
            Assert.Equal(KBotDataColumn.NoDecimalPlaces, c.DecimalPlaces)
            Assert.False(c.HasDecimalPlaces)
            dv.AddRow()("v") = 1.23456
            Assert.Equal(1.23456, ParseNum(TextCelula(dv, "v", 0)), 6)
        End Using
    End Sub

    <Fact>
    Public Sub NonNumericCells_AreLeftAlone()
        Using dv = Grid(2)
            dv.AddRow()("v") = "n/a"
            Assert.Equal("n/a", TextCelula(dv, "v", 0))
        End Using
    End Sub

    ' ── Subsolul adună ce se VEDE ────────────────────────────────────────────────

    <Fact>
    Public Sub TheTotal_AddsUpTheDISPLAYEDValues_NotTheStoredOnes()
        ' Trei valori care se afișează 0,34 / 0,34 / 0,34. Suma brută e 1,014 (=> 1,01 afișat),
        ' dar pe ecran scrie 0,34+0,34+0,34, deci totalul TREBUIE să fie 1,02: un total care nu
        ' iese la adunare pe pagină e o greșeală de calcul pentru cine o citește.
        Using dv = Grid(2)
            dv.BeginUpdate()
            dv.AddRow()("v") = 0.338
            dv.AddRow()("v") = 0.338
            dv.AddRow()("v") = 0.338
            dv.EndUpdate()

            Assert.Equal(0.34, ParseNum(TextCelula(dv, "v", 0)), 6)
            Assert.Equal(1.02, ParseNum(dv.DebugFooterText("v")), 6)
        End Using
    End Sub

    <Fact>
    Public Sub TheAverage_IsRoundedToo_NotLeftWithATail()
        Using dv = Grid(2, KBotAggregate.Average)
            dv.BeginUpdate()
            dv.AddRow()("v") = 1.0
            dv.AddRow()("v") = 1.0
            dv.AddRow()("v") = 2.0
            dv.EndUpdate()
            ' 4/3 = 1,3333… => 1,33, scris cu exact două zecimale.
            Assert.Equal(1.33, ParseNum(dv.DebugFooterText("v")), 6)
            Assert.Equal(2, Zecimale(dv.DebugFooterText("v")))
        End Using
    End Sub

    <Fact>
    Public Sub MinMax_ReportTheDisplayedValue()
        Using dv = Grid(1, KBotAggregate.Max)
            dv.BeginUpdate()
            dv.AddRow()("v") = 2.449
            dv.AddRow()("v") = 2.451
            dv.EndUpdate()
            Assert.Equal(2.5, ParseNum(dv.DebugFooterText("v")), 6)
        End Using
    End Sub

    <Fact>
    Public Sub CountDistinct_CountsWhatTheEyeSees()
        ' Două valori care se rotunjesc la același text sunt, pentru cine se uită la coloană,
        ' aceeași valoare.
        Using dv = Grid(1, KBotAggregate.CountDistinct)
            dv.BeginUpdate()
            dv.AddRow()("v") = 1.21
            dv.AddRow()("v") = 1.24
            dv.AddRow()("v") = 1.31
            dv.EndUpdate()
            Assert.Equal("2", dv.DebugFooterText("v"))
        End Using
    End Sub

    <Fact>
    Public Sub ChangingThePlaces_RefreshesTheFooterImmediately()
        Using dv = Grid(2)
            dv.BeginUpdate()
            dv.AddRow()("v") = 0.338
            dv.AddRow()("v") = 0.338
            dv.EndUpdate()
            Assert.Equal(0.68, ParseNum(dv.DebugFooterText("v")), 6)

            dv.Column("v").DecimalPlaces = 1          ' fără alt apel: subsolul se recalculează
            Assert.Equal(0.6, ParseNum(dv.DebugFooterText("v")), 6)
        End Using
    End Sub

    <Fact>
    Public Sub TheStoredValueIsNeverMutated()
        ' Rotunjirea e o regulă de afișare: rândul își păstrează valoarea întreagă, ca un commit
        ' spre server să nu trimită înapoi un număr ciuntit de grilă.
        Using dv = Grid(2)
            dv.AddRow()("v") = 1.23456
            Assert.Equal(1.23456, CDbl(dv.Rows(0)("v")), 6)
        End Using
    End Sub

    ' ── Contractul proprietății ──────────────────────────────────────────────────

    <Fact>
    Public Sub Places_OnANonNumericColumn_Throws()
        Using dv As New KBotDataView()
            Dim c = dv.AddColumn("nume", "Nume", KBotColumnType.Text, 100)   ' ValueType = Text
            Assert.Throws(Of ArgumentException)(Sub() c.DecimalPlaces = 2)
            Assert.False(c.HasDecimalPlaces)
        End Using
    End Sub

    <Fact>
    Public Sub ValueTypeChange_ThatWouldOrphanThePlaces_Throws()
        Using dv As New KBotDataView()
            Dim c = dv.AddColumn("v", "Valoare", KBotColumnType.Text, 100)
            c.ValueType = KBotValueType.Number
            c.DecimalPlaces = 2
            Assert.Throws(Of ArgumentException)(Sub() c.ValueType = KBotValueType.Text)
            ' Calea corectă: întâi cad zecimalele, apoi tipul.
            c.DecimalPlaces = KBotDataColumn.NoDecimalPlaces
            c.ValueType = KBotValueType.Text
            Assert.Equal(KBotValueType.Text, c.ValueType)
        End Using
    End Sub

    <Fact>
    Public Sub TooManyPlaces_Throws_AtTheMathRoundLimit()
        Using dv As New KBotDataView()
            Dim c = dv.AddColumn("v", "Valoare", KBotColumnType.Text, 100)
            c.ValueType = KBotValueType.Number
            Assert.Throws(Of ArgumentOutOfRangeException)(
                Sub() c.DecimalPlaces = KBotDataColumn.MaxDecimalPlaces + 1)
            c.DecimalPlaces = KBotDataColumn.MaxDecimalPlaces      ' limita însăși e validă
            Assert.Equal(KBotDataColumn.MaxDecimalPlaces, c.DecimalPlaces)
        End Using
    End Sub

    <Fact>
    Public Sub AnyNegative_MeansUnset_AndNormalisesToTheSameState()
        Using dv As New KBotDataView()
            Dim c = dv.AddColumn("v", "Valoare", KBotColumnType.Text, 100)
            c.ValueType = KBotValueType.Number
            c.DecimalPlaces = 3
            c.DecimalPlaces = -7                                   ' „gol”, oricum ar fi scris
            Assert.Equal(KBotDataColumn.NoDecimalPlaces, c.DecimalPlaces)
            Assert.False(c.HasDecimalPlaces)
        End Using
    End Sub

    <Fact>
    Public Sub UntouchedPlaces_AreNotSerialized()
        Using dv As New KBotDataView()
            Dim c = dv.AddColumn("v", "Valoare", KBotColumnType.Text, 100)
            Dim prop As PropertyDescriptor = TypeDescriptor.GetProperties(c)("DecimalPlaces")
            Assert.False(prop.ShouldSerializeValue(c))
            c.ValueType = KBotValueType.Number
            prop.SetValue(c, 2)
            Assert.True(prop.ShouldSerializeValue(c))
            prop.ResetValue(c)
            Assert.False(prop.ShouldSerializeValue(c))
        End Using
    End Sub

    <Fact>
    Public Sub AFreeFloatingColumn_WithPlacesOnAText_ThrowsWhenItJoinsTheGrid()
        ' Ca la pereche tip × agregat: coloana liberă n-are de la cine afla regula, dar intrarea
        ' în grilă e locul unde starea devine a grilei.
        Dim libera As New KBotDataColumn("x", "X", KBotColumnType.Text, 80) With {
            .DecimalPlaces = 2
        }
        Using dv As New KBotDataView()
            Assert.Throws(Of ArgumentException)(Sub() dv.Columns.Add(libera))
        End Using
    End Sub

End Class
