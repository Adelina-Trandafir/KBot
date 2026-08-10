Imports System.Drawing
Imports System.Globalization
Imports KBot.Theming
Imports Xunit

''' <summary>
''' Tests for the Access-style named <c>Format</c> property (slice 0028-02): the mapping of each
''' named format, the coercion it does on its way (a text column carrying numbers still formats),
''' how <c>DecimalPlaces</c> feeds it, the loud refusal to be used together with
''' <c>FormatString</c>, and the fact that a footer aggregate is written in the SAME format as
''' the column above it.
'''
''' Expectations are built with <c>CultureInfo.CurrentCulture</c> rather than hard-coded strings,
''' so the suite does not depend on the test host's decimal separator or currency symbol; what is
''' asserted is the MAPPING (Standard → N2, Percent → P2…), which is the part we wrote.
''' </summary>
Public Class KBotDataViewColumnFormatTests

    Private Shared ReadOnly Cc As CultureInfo = CultureInfo.CurrentCulture

    Private Shared Function Text(value As Object, format As KBotFormat,
                                 Optional decimale As Integer = -1) As String
        Dim t As String = Nothing
        Assert.True(KBotColumnFormat.TryFormat(value, format, decimale, t),
                    $"Formatul «{format}» n-a formatat valoarea «{value}».")
        Return t
    End Function

    ' ── Formatele numerice ───────────────────────────────────────────────────────

    <Fact>
    Public Sub Standard_And_Fixed_UseTwoDecimalsByDefault()
        Assert.Equal((1234.5).ToString("N2", Cc), Text(1234.5, KBotFormat.Standard))
        Assert.Equal((1234.5).ToString("F2", Cc), Text(1234.5, KBotFormat.Fixed))
        ' Diferența dintre ele e chiar separatorul de mii — altfel testul n-ar dovedi nimic.
        Assert.NotEqual(Text(1234.5, KBotFormat.Standard), Text(1234.5, KBotFormat.Fixed))
    End Sub

    <Fact>
    Public Sub Currency_Percent_Scientific_MapToTheirNetFormats()
        Assert.Equal((12.0).ToString("C2", Cc), Text(12.0, KBotFormat.Currency))
        Assert.Equal((0.25).ToString("P2", Cc), Text(0.25, KBotFormat.Percent))
        Assert.Equal((12345.0).ToString("0.00E+00", Cc), Text(12345.0, KBotFormat.Scientific))
    End Sub

    <Fact>
    Public Sub Euro_ForcesTheEuroSign()
        Assert.StartsWith("€", Text(12.0, KBotFormat.Euro))
    End Sub

    <Fact>
    Public Sub DecimalPlaces_DecidesHowManyDigitsANamedFormatWrites()
        Assert.Equal((1234.5).ToString("N0", Cc), Text(1234.5, KBotFormat.Standard, 0))
        Assert.Equal((1234.5).ToString("N3", Cc), Text(1234.5, KBotFormat.Standard, 3))
        Assert.Equal("N4", KBotColumnFormat.NetFormat(KBotFormat.Standard, 4))
    End Sub

    <Fact>
    Public Sub NamedFormats_ReadNumbersOutOfText()
        ' O coloană de TEXT care poartă numere (cazul DdfView/PlatiView) se formatează totuși.
        Dim caText As String = (1234.5).ToString(Cc)
        Assert.Equal((1234.5).ToString("N2", Cc), Text(caText, KBotFormat.Standard))
    End Sub

    <Fact>
    Public Sub NonNumericText_FallsThroughInsteadOfBlanking()
        Dim t As String = Nothing
        Assert.False(KBotColumnFormat.TryFormat("abc", KBotFormat.Standard, -1, t))
        ' Fals înseamnă „cade pe calea obișnuită”, adică valoarea tot se vede — nu se stinge.
        Assert.Null(t)
    End Sub

    ' ── Formatele calendaristice și logice ───────────────────────────────────────

    <Fact>
    Public Sub DateFormats_MapToTheCulturePatterns()
        Dim d As New Date(2026, 8, 10, 14, 30, 0)
        Assert.Equal(d.ToString("d", Cc), Text(d, KBotFormat.ShortDate))
        Assert.Equal(d.ToString("D", Cc), Text(d, KBotFormat.LongDate))
        Assert.Equal(d.ToString("HH:mm", Cc), Text(d, KBotFormat.ShortTime))
        Assert.Equal(d.ToString("dd-MMM-yy", Cc), Text(d, KBotFormat.MediumDate))
    End Sub

    <Fact>
    Public Sub GeneralDate_WritesTheTimeOnlyWhenThereIsOne()
        Dim doarData As New Date(2026, 8, 10)
        Dim cuOra As New Date(2026, 8, 10, 14, 30, 0)
        Assert.Equal(doarData.ToString("d", Cc), Text(doarData, KBotFormat.GeneralDate))
        Assert.Equal(cuOra.ToString("g", Cc), Text(cuOra, KBotFormat.GeneralDate))
    End Sub

    <Fact>
    Public Sub LogicalFormats_AreWrittenInRomanian()
        Assert.Equal("Da", Text(True, KBotFormat.YesNo))
        Assert.Equal("Nu", Text(False, KBotFormat.YesNo))
        Assert.Equal("Adevărat", Text(True, KBotFormat.TrueFalse))
        Assert.Equal("Oprit", Text(False, KBotFormat.OnOff))
        ' Aceeași coerciție ca la celula cu bifă: 0 = nebifat, orice altceva = bifat.
        Assert.Equal("Da", Text(1, KBotFormat.YesNo))
        Assert.Equal("Nu", Text(0, KBotFormat.YesNo))
    End Sub

    <Fact>
    Public Sub None_And_GeneralNumber_LeaveTheValueAlone()
        Dim t As String = Nothing
        Assert.False(KBotColumnFormat.TryFormat(12.0, KBotFormat.None, -1, t))
        Assert.False(KBotColumnFormat.TryFormat(12.0, KBotFormat.GeneralNumber, -1, t))
    End Sub

    ' ── Contractul de pe coloană ─────────────────────────────────────────────────

    Private Shared Function Grid() As KBotDataView
        Dim dv As New KBotDataView()
        dv.Size = New Size(600, 300)
        dv.ApplyTheme(BuiltInSchemes.Classic())
        Return dv
    End Function

    <Fact>
    Public Sub Format_And_FormatString_CannotBothBeSet()
        Using dv = Grid()
            Dim col = dv.AddColumn("a", "A", KBotColumnType.Text, 120)
            col.FormatString = "N2"
            Assert.Throws(Of ArgumentException)(Sub() col.Format = KBotFormat.Standard)

            ' Și invers — ordinea în care ajung nu schimbă răspunsul.
            Dim col2 = dv.AddColumn("b", "B", KBotColumnType.Text, 120)
            col2.Format = KBotFormat.Standard
            Assert.Throws(Of ArgumentException)(Sub() col2.FormatString = "N2")
        End Using
    End Sub

    <Fact>
    Public Sub TheConflictIsCaughtAtEndInit_WhateverOrderTheDesignerEmits()
        Using dv = Grid()
            Dim init As ComponentModel.ISupportInitialize = dv
            init.BeginInit()
            Dim col = dv.AddColumn("a", "A", KBotColumnType.Text, 120)
            ' În blocul de inițializare amândouă trec (designerul le emite în ordinea LUI)…
            col.Format = KBotFormat.Standard
            col.FormatString = "N2"
            ' …dar perechea așezată se verifică la EndInit.
            Assert.Throws(Of ArgumentException)(Sub() init.EndInit())
        End Using
    End Sub

    <Fact>
    Public Sub FooterAggregate_IsWrittenInTheColumnsOwnFormat()
        Using dv = Grid()
            Dim col = dv.AddColumn("suma", "Suma", KBotColumnType.Text, 120)
            col.ValueType = KBotValueType.Number
            col.Format = KBotFormat.Standard
            col.Aggregate = KBotAggregate.Sum
            dv.FooterVisible = True

            dv.AddRow() : dv("suma", 0) = 1000.0
            dv.AddRow() : dv("suma", 1) = 234.5

            ' Totalul se scrie exact ca valorile de deasupra lui, nu cu un ToString gol.
            Assert.Equal((1234.5).ToString("N2", Cc), dv.DebugFooterText("suma"))
        End Using
    End Sub

    <Fact>
    Public Sub AggregateFormatString_StillBeatsTheNamedFormat()
        Using dv = Grid()
            Dim col = dv.AddColumn("suma", "Suma", KBotColumnType.Text, 120)
            col.ValueType = KBotValueType.Number
            col.Format = KBotFormat.Standard
            col.AggregateFormatString = "F0"
            col.Aggregate = KBotAggregate.Sum
            dv.FooterVisible = True

            dv.AddRow() : dv("suma", 0) = 1234.5
            Assert.Equal((1234.5).ToString("F0", Cc), dv.DebugFooterText("suma"))
        End Using
    End Sub

    <Fact>
    Public Sub TheCellIsPaintedThroughTheNamedFormatToo()
        Using dv = Grid()
            Dim col = dv.AddColumn("suma", "Suma", KBotColumnType.Text, 200)
            col.Format = KBotFormat.Standard
            dv.AddRow() : dv("suma", 0) = 1234.5

            ' CellFormatting primește textul DEJA formatat — poarta prin care se vede ce s-ar picta.
            Dim vazut As String = Nothing
            AddHandler dv.CellFormatting, Sub(s, e) If e.Column.Key = "suma" Then vazut = e.Text
            Using bmp As New Bitmap(dv.Width, dv.Height)
                dv.DrawToBitmap(bmp, New Rectangle(0, 0, dv.Width, dv.Height))
            End Using

            Assert.Equal((1234.5).ToString("N2", Cc), vazut)
        End Using
    End Sub

End Class
