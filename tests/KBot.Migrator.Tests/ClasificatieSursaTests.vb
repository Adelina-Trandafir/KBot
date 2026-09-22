Imports System.Linq
Imports Xunit

''' <summary>
''' Slice 0075-00: <c>Clasificatii.Sursa</c> stopped being a GENERATED column and became a
''' written one, so that all fourteen <c>DefaSursaSector</c> values become reachable.
''' </summary>
''' <remarks>
''' These tests hold the migrator's half of that change: the normalisation rule, the
''' extended sector CASE, and the fact that the Access column now travels through a
''' DELIBERATE mapping rather than through the plain name match it would otherwise fall
''' into the moment the target column turned writable.
''' </remarks>
Public Class ClasificatieSursaTests

    ' ---- the rule --------------------------------------------------------------------

    <Fact>
    Public Sub Sursa_din_Access_ajunge_pe_rand()
        ' 65.01 is an ordinary capitol; F is a source letter the old generated column
        ' could never produce.
        Dim row = New ClasificatieDerived("65.01", "04.02", "20.01", "04", "F")
        Assert.Equal("F", row.Sursa)
        Assert.Equal("01F", row.SS)
    End Sub

    <Fact>
    Public Sub Sursa_lipsa_devine_A()
        ' What the old expression produced for every capitol but xx10, and the column default.
        Assert.Equal("A", ClasificatieDerived.NormalizeSursa(Nothing, "65.01"))
        Assert.Equal("A", ClasificatieDerived.NormalizeSursa("   ", "65.02"))
        Assert.Equal("01A", New ClasificatieDerived("65.01", "04.02", "20.01", "04").SS)
    End Sub

    <Fact>
    Public Sub Capitolul_xx10_este_E_indiferent_de_fisier()
        ' The capitol decides here: xx10 IS the E source, and a file saying otherwise is
        ' wrong about its own row.
        Assert.Equal("E", ClasificatieDerived.NormalizeSursa("A", "65.10"))
        Assert.Equal("E", ClasificatieDerived.NormalizeSursa(Nothing, "65.10"))
        Assert.Equal("02E", New ClasificatieDerived("65.10", "04.02", "20.01", "04", "A").SS)
    End Sub

    <Fact>
    Public Sub Sursa_se_normalizeaza_la_o_litera_mare()
        Assert.Equal("D", ClasificatieDerived.NormalizeSursa(" d ", "65.01"))
        Assert.Equal("G", ClasificatieDerived.NormalizeSursa("g", "65.01"))
        ' char(1) on the target: only the first character can survive anyway, so it is cut
        ' here rather than by a 1406 at INSERT.
        Assert.Equal("C", ClasificatieDerived.NormalizeSursa("CD", "65.01"))
    End Sub

    ' ---- the sector CASE -------------------------------------------------------------

    <Theory>
    <InlineData("65.00", "01")>
    <InlineData("65.01", "01")>
    <InlineData("65.02", "02")>
    <InlineData("65.10", "02")>
    Public Sub Sectoarele_istorice_isi_pastreaza_maparea(capitol As String, expected As String)
        Assert.Equal(expected, New ClasificatieDerived(capitol, "04.02", "20.01", "04").Sector)
    End Sub

    <Theory>
    <InlineData("65.03", "03")>
    <InlineData("65.04", "04")>
    <InlineData("65.05", "05")>
    <InlineData("65.08", "08")>
    Public Sub Sectoarele_noi_se_mapeaza_pe_ele_insele(capitol As String, expected As String)
        Assert.Equal(expected, New ClasificatieDerived(capitol, "04.02", "20.01", "04").Sector)
    End Sub

    <Fact>
    Public Sub Un_capitol_necunoscut_da_sector_gol()
        ' The failure mode is a near-blank SS, refused by the foreign key - never a wrong value.
        Dim row = New ClasificatieDerived("65.99", "04.02", "20.01", "04", "A")
        Assert.Equal(String.Empty, row.Sector)
        Assert.Equal("A", row.SS)
    End Sub

    <Theory>
    <InlineData("65.01", "D", "01D")>
    <InlineData("65.01", "F", "01F")>
    <InlineData("65.01", "G", "01G")>
    <InlineData("65.03", "A", "03A")>
    <InlineData("65.04", "A", "04A")>
    <InlineData("65.05", "A", "05A")>
    <InlineData("65.08", "A", "08A")>
    <InlineData("65.02", "C", "02C")>
    <InlineData("65.02", "D", "02D")>
    <InlineData("65.02", "G", "02G")>
    Public Sub Toate_valorile_din_DefaSursaSector_sunt_acum_accesibile(
            capitol As String, sursa As String, expected As String)
        ' The whole point of 0075-00: before it, only 01A, 02A and 02E could ever be produced.
        Assert.Equal(expected, New ClasificatieDerived(capitol, "04.02", "20.01", "04", sursa).SS)
    End Sub

    ' ---- the mapping is deliberate ---------------------------------------------------

    <Fact>
    Public Sub Harta_Clasificatii_revendica_explicit_coloana_Sursa()
        ' Without this entry the Access column would travel by plain NAME MATCH the moment
        ' the target column turned writable - right outcome, wrong reason, raw value.
        Dim map = TableMaps.Nomenclators().Single(Function(m) m.TargetTable = "Clasificatii")
        Dim sursa = map.Derived.SingleOrDefault(Function(d) d.TargetColumn = "Sursa")

        Assert.NotNull(sursa)
        Assert.Equal(ColumnSourceKind.ClasificatieSursa, sursa.Kind)
        ' The Access column is named so ColumnPlan counts it as consumed and the name match
        ' does not claim the target a second time.
        Assert.Equal("Sursa", sursa.AccessColumn)
    End Sub

    ' ---- the other generated columns are untouched ------------------------------------

    <Fact>
    Public Sub Celelalte_coloane_generate_raman_neschimbate()
        Dim row = New ClasificatieDerived("65.02", "04.02", "20.01", "04", "A")
        Assert.Equal("65.02.04.02.20.01.04", row.Clsf)
        Assert.Equal("20", row.Titlu)
        Assert.Equal("650402", row.ClsfF)
        Assert.Equal("200104", row.ClsfE)
    End Sub

End Class
