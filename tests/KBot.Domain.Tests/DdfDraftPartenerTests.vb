Option Strict On
Imports System.Collections.Generic
Imports Xunit
Imports KBot.Domain

' Pure-model tests for how the DDF draft carries its partner onto the lines. Two methods
' with deliberately different rules, and the difference is the whole point:
'   * ImpingePartenerulPeLinii  — the operator PICKED a partner: every line is overwritten;
'   * InheritHeaderPartner      — the form just OPENED: only the empty lines are filled.
'
' The second one exists because of a dead end in the editor: a NEW revision on a document
' already tied to a partner arrives with the partner on the header but with nothing on the
' generated lines, while the header's picker is locked for a new revision and the section-A
' Partener column is hidden. The save then refused a field the operator could not reach.
Public Class DdfDraftPartenerTests

    Private Shared Function Schita(codFiscal As String, partAng As Boolean,
                                   parteneriPeLinii As String()) As DdfDraft
        Dim d As New DdfDraft() With {
            .CodAngajament = "AN-1",
            .PartAng = partAng,
            .CodFiscal = codFiscal}

        For i As Integer = 0 To parteneriPeLinii.Length - 1
            d.LiniiA.Add(New DdfDraftLinieA() With {
                .TempId = -(i + 1),
                .CodAngajament = "AN-1",
                .CodIndicator = "IND" & (i + 1).ToString(Globalization.CultureInfo.InvariantCulture),
                .IdClsf = 100 + i,
                .IdUnitate = 7,
                .CodPartener = parteneriPeLinii(i),
                .ValCur = 10.0R})
        Next

        d.Revizie.RecalculeazaSectiuneaB()
        Return d
    End Function

    <Fact>
    Public Sub InheritHeaderPartner_FillsTheEmptyLines()
        Dim d As DdfDraft = Schita("RO123", partAng:=True, parteneriPeLinii:={"", ""})

        Assert.Equal(2, d.InheritHeaderPartner())
        Assert.All(d.LiniiA, Sub(a) Assert.Equal("RO123", a.CodPartener))
        ' The per-line flag follows the code, as the section-A cell handler derives it.
        Assert.All(d.LiniiA, Sub(a) Assert.True(a.PartInd))
    End Sub

    <Fact>
    Public Sub InheritHeaderPartner_LeavesALineThatAlreadyHasOne()
        Dim d As DdfDraft = Schita("RO123", partAng:=True, parteneriPeLinii:={"RO999", ""})

        Assert.Equal(1, d.InheritHeaderPartner())
        ' One CodFiscal can stand behind several units, so an existing choice is not touched.
        Assert.Equal("RO999", d.LiniiA(0).CodPartener)
        Assert.Equal("RO123", d.LiniiA(1).CodPartener)
    End Sub

    <Fact>
    Public Sub InheritHeaderPartner_DoesNothing_WhenTheDocumentHasNoPartner()
        Dim d As DdfDraft = Schita("RO123", partAng:=False, parteneriPeLinii:={"", ""})

        Assert.Equal(0, d.InheritHeaderPartner())
        Assert.All(d.LiniiA, Sub(a) Assert.Equal(String.Empty, a.CodPartener))
    End Sub

    <Fact>
    Public Sub InheritHeaderPartner_DoesNothing_WhenTheHeaderHasNoFiscalCode()
        Dim d As DdfDraft = Schita("   ", partAng:=True, parteneriPeLinii:={""})

        Assert.Equal(0, d.InheritHeaderPartner())
        Assert.Equal(String.Empty, d.LiniiA(0).CodPartener)
    End Sub

    <Fact>
    Public Sub InheritHeaderPartner_CarriesThroughToSectionB()
        Dim d As DdfDraft = Schita("RO123", partAng:=True, parteneriPeLinii:={""})

        d.InheritHeaderPartner()

        ' Section B mirrors section A, so the rebuild has to have run.
        Assert.Equal(1, d.LiniiB.Count)
        Assert.Equal("RO123", d.LiniiB(0).CodPartener)
    End Sub

    <Fact>
    Public Sub ImpingePartenerulPeLinii_OverwritesEveryLine()
        Dim d As DdfDraft = Schita("RO123", partAng:=True, parteneriPeLinii:={"RO999", ""})

        d.ImpingePartenerulPeLinii("RO123", 0)

        Assert.All(d.LiniiA, Sub(a) Assert.Equal("RO123", a.CodPartener))
        Assert.All(d.LiniiB, Sub(b) Assert.Equal("RO123", b.CodPartener))
    End Sub

End Class
