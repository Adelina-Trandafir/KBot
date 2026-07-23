Option Strict On
Imports Xunit
Imports KBot.Xfa

' Teste pentru XfaResult — obiectul care înlocuiește codurile de ieșire ale exe-ului
' (0 = generat fără semnare, 2 = nesemnat, 11..17 = semnat = 10 + mască).
Public Class XfaResultTests

    <Fact>
    Public Sub Generat_EsteReusitNesemnatCod0()
        Dim r = XfaResult.Generat("c:\out.pdf")

        Assert.True(r.Reusit)
        Assert.False(r.Semnat)
        Assert.Equal(0, r.Masca)
        Assert.Equal(0, r.CodIesireLegacy)
        Assert.Equal("c:\out.pdf", r.CaleOutputPdf)
        Assert.False(r.SemnatAB)
        Assert.False(r.SemnatCD)
        Assert.False(r.SemnatOrdonator)
    End Sub

    <Fact>
    Public Sub DupaSemnare_MascaZero_DaNesemnatCod2()
        Dim r = XfaResult.DupaSemnare(0, "c:\out.pdf")

        Assert.True(r.Reusit)
        Assert.False(r.Semnat)
        Assert.Equal(0, r.Masca)
        Assert.Equal(2, r.CodIesireLegacy)
    End Sub

    <Fact>
    Public Sub DupaSemnare_MascaNegativa_EsteTratataCaNesemnat()
        Dim r = XfaResult.DupaSemnare(-5, "c:\out.pdf")

        Assert.False(r.Semnat)
        Assert.Equal(0, r.Masca) ' clamp la 0
        Assert.Equal(2, r.CodIesireLegacy)
    End Sub

    <Theory>
    <InlineData(AdobeUtils.SIGNER_AB, 11)>
    <InlineData(AdobeUtils.SIGNER_CD, 12)>
    <InlineData(AdobeUtils.SIGNER_AB Or AdobeUtils.SIGNER_CD, 13)>
    <InlineData(AdobeUtils.SIGNER_ORDONATOR, 14)>
    <InlineData(AdobeUtils.SIGNER_AB Or AdobeUtils.SIGNER_ORDONATOR, 15)>
    <InlineData(AdobeUtils.SIGNER_CD Or AdobeUtils.SIGNER_ORDONATOR, 16)>
    <InlineData(AdobeUtils.SIGNER_AB Or AdobeUtils.SIGNER_CD Or AdobeUtils.SIGNER_ORDONATOR, 17)>
    Public Sub DupaSemnare_Cod_Este10PlusMasca(masca As Integer, codAsteptat As Integer)
        Dim r = XfaResult.DupaSemnare(masca, "c:\out.pdf")

        Assert.True(r.Semnat)
        Assert.Equal(masca, r.Masca)
        Assert.Equal(codAsteptat, r.CodIesireLegacy)
    End Sub

    <Fact>
    Public Sub BoolurileDeConveniente_ReflectaBitiiMastii()
        Dim r = XfaResult.DupaSemnare(AdobeUtils.SIGNER_AB Or AdobeUtils.SIGNER_ORDONATOR, "c:\out.pdf")

        Assert.True(r.SemnatAB)
        Assert.False(r.SemnatCD)
        Assert.True(r.SemnatOrdonator)
    End Sub

End Class
