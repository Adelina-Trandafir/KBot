Imports System.Linq
Imports Xunit

''' <summary>
''' Slice 0080-01, the VB migrator's half: dates typed as Access text going into a MariaDB
''' date column, and IdClsf = Clasificatii.IDClsf on the seven FX_ tables.
''' </summary>
Public Class ClsfPairAndDateTests

    Private Shared Function Col(dataType As String) As TargetColumn
        Return New TargetColumn("DataDoc", 1, True, Nothing, Nothing, dataType, dataType, Nothing)
    End Function

    ' ---- dates -------------------------------------------------------------------------

    <Theory>
    <InlineData("31.12.2025")>
    <InlineData("31/12/2025")>
    <InlineData("2025-12-31 00:00:00")>
    <InlineData("2025-12-31")>
    <InlineData(" 31.12.2025 ")>
    Public Sub Every_shape_in_DataDoc_becomes_the_same_date(text As String)
        Dim got = ValueConverter.ToParameter(text, Col("date"))
        Assert.Equal(New DateTime(2025, 12, 31), got)
    End Sub

    <Fact>
    Public Sub The_time_is_dropped_on_a_date_column()
        Dim got = ValueConverter.ToParameter("2025-12-31 14:05:09", Col("date"))
        Assert.Equal(New DateTime(2025, 12, 31), got)
    End Sub

    <Fact>
    Public Sub The_time_is_kept_on_a_datetime_column()
        Dim got = ValueConverter.ToParameter("31.12.2025 14:05:09", Col("datetime"))
        Assert.Equal(New DateTime(2025, 12, 31, 14, 5, 9), got)
    End Sub

    <Fact>
    Public Sub Ambiguous_dates_are_read_day_first_like_the_python_migrator()
        Assert.Equal(New DateTime(2025, 5, 4), ValueConverter.ToParameter("04.05.2025", Col("date")))
        Assert.Equal(New DateTime(2025, 5, 4), ValueConverter.ToParameter("04/05/2025", Col("date")))
    End Sub

    <Fact>
    Public Sub Slash_with_two_digit_year_is_month_first()
        Dim d As DateTime
        Assert.True(ValueConverter.TryReadDate("04/28/26 15:28:03", d))
        Assert.Equal(New DateTime(2026, 4, 28, 15, 28, 3), d)
    End Sub

    <Fact>
    Public Sub Empty_text_is_null()
        Assert.Equal(DBNull.Value, ValueConverter.ToParameter("", Col("date")))
        Assert.Equal(DBNull.Value, ValueConverter.ToParameter("   ", Col("date")))
    End Sub

    <Fact>
    Public Sub Unreadable_text_stops_the_run_with_the_value_named()
        Dim ex = Assert.Throws(Of TransferException)(
            Function() ValueConverter.ToParameter("31.13.2025", Col("date")))
        Assert.Contains("31.13.2025", ex.Message)
    End Sub

    <Fact>
    Public Sub Text_columns_are_left_alone()
        Assert.Equal("31.12.2025", ValueConverter.ToParameter("31.12.2025", Col("varchar")))
    End Sub

    ' ---- IdClsf = Clasificatii.IDClsf, resolved on the row's unit ----------------------

    Private Shared ReadOnly Seven As String() = {
        "FX_Extrase_H", "FX_Indicatori", "FX_Istoric", "FX_Plati", "FX_Receptii",
        "FX_Receptii_RHR", "FX_Rezervari"}

    <Fact>
    Public Sub The_seven_tables_are_marked_in_the_catalogue()
        Dim marked = TableMaps.All().Where(Function(m) m.HasClsfPair).
            Select(Function(m) m.TargetTable).OrderBy(Function(n) n).ToList()
        Assert.Equal(Seven.OrderBy(Function(n) n).ToList(), marked)
    End Sub

    <Fact>
    Public Sub IdClsf_is_resolved_on_the_row_unit_and_no_Access_copy_is_kept()
        ' FX_Extrase_H computes its IdClsf from the account instead - ExtrasHeaderRulesTests.
        For Each map In TableMaps.All().Where(Function(m) m.HasClsfPair AndAlso m.TargetTable <> "FX_Extrase_H")
            Dim idClsf = map.Derived.Single(Function(d) d.TargetColumn = "IdClsf")
            Assert.Equal(ColumnSourceKind.ClasificatieByRowUnit, idClsf.Kind)
            Assert.Equal("IdClsf", idClsf.AccessColumn)
            Assert.True(idClsf.BlockingOnMiss)
            ' Operator, 24.09.2026: no IdClsfAcc on these tables.
            Assert.False(map.Renames.ContainsKey("IdClsf"))
            Assert.DoesNotContain(map.Derived, Function(d) d.TargetColumn = "IdClsfAcc")
        Next
    End Sub

    <Fact>
    Public Sub Only_Clasificatii_writes_IdClsfAcc()
        ' Slice 0080-04 (operator, 25.09.2026): Clasificatii is the source of truth for the
        ' Access id; FX_DDF_REV_SA / _SB and Parteneri_Coduri no longer carry a copy.
        For Each map In TableMaps.All().Where(Function(m) m.TargetTable <> "Clasificatii")
            Assert.DoesNotContain(map.Derived, Function(d) d.TargetColumn = "IdClsfAcc")
            Assert.DoesNotContain(map.Renames.Values, Function(v) v = "IdClsfAcc")
        Next
    End Sub

End Class
