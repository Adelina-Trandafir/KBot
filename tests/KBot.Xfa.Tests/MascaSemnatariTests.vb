Option Strict On
Imports Xunit
Imports KBot.Xfa

' Teste pentru AdobeUtils.MascaSemnatari / clasificarea semnatarilor — logica pură care
' mapează numele câmpurilor de semnătură pe masca AB/CD/Ordonator, offline (fără PDF/Adobe).
Public Class MascaSemnatariTests

    ' --- 1. Potrivire după subformular (acoperă atât ORD cât și DDF) ---

    <Theory>
    <InlineData("topmostSubform.SubformSemnaturaAB[0].SignatureField1", "ORD", AdobeUtils.SIGNER_AB)>
    <InlineData("topmostSubform.SubformSemnaturaCD[0].SignatureField3", "ORD", AdobeUtils.SIGNER_CD)>
    <InlineData("topmostSubform.SubformSemnaturaOrdonator[0].SignatureField5", "ORD", AdobeUtils.SIGNER_ORDONATOR)>
    <InlineData("form1.SubformSemnaturaA.SignatureField1", "DDF", AdobeUtils.SIGNER_AB)>
    <InlineData("form1.SubformSemnaturaB.SignatureField2", "DDF", AdobeUtils.SIGNER_CD)>
    Public Sub Subform_DaMascaCorecta(fieldName As String, docType As String, expected As Integer)
        Assert.Equal(expected, AdobeUtils.MascaSemnatari({fieldName}, docType))
    End Sub

    <Fact>
    Public Sub Subform_AB_ElucidatInainteaLui_A()
        ' „SubformSemnaturaAB” conține substringul „SubformSemnaturaA”; ordinea din ClassifySigner
        ' garantează că AB (ORD) câștigă înaintea lui A (DDF).
        Assert.Equal(AdobeUtils.SIGNER_AB, AdobeUtils.MascaSemnatari({"SubformSemnaturaAB.SignatureField1"}, "ORD"))
    End Sub

    ' --- 2. Fallback numeric după „SignatureField<n>”, în funcție de tip ---

    <Theory>
    <InlineData("form1.SignatureField1", "ORD", AdobeUtils.SIGNER_AB)>
    <InlineData("form1.SignatureField2", "ORD", AdobeUtils.SIGNER_AB)>
    <InlineData("form1.SignatureField3", "ORD", AdobeUtils.SIGNER_CD)>
    <InlineData("form1.SignatureField4", "ORD", AdobeUtils.SIGNER_CD)>
    <InlineData("form1.SignatureField5", "ORD", AdobeUtils.SIGNER_ORDONATOR)>
    <InlineData("form1.SignatureField1", "DDF", AdobeUtils.SIGNER_AB)>
    <InlineData("form1.SignatureField2", "DDF", AdobeUtils.SIGNER_CD)>
    <InlineData("form1.SignatureField3", "DDF", AdobeUtils.SIGNER_ORDONATOR)>
    <InlineData("form1.SignatureField11", "DDF", AdobeUtils.SIGNER_AB)>
    <InlineData("form1.SignatureField16", "DDF", AdobeUtils.SIGNER_AB)>
    <InlineData("form1.SignatureField21", "DDF", AdobeUtils.SIGNER_CD)>
    <InlineData("form1.SignatureField26", "DDF", AdobeUtils.SIGNER_CD)>
    Public Sub FallbackNumeric_DaMascaCorecta(fieldName As String, docType As String, expected As Integer)
        Assert.Equal(expected, AdobeUtils.MascaSemnatari({fieldName}, docType))
    End Sub

    ' --- 3. Compunerea măștilor + cazuri limită ---

    <Fact>
    Public Sub MaiMultiSemnatari_SeCombinaPeBiti()
        Dim nume = {
            "SubformSemnaturaAB.SignatureField1",
            "SubformSemnaturaCD.SignatureField3",
            "SubformSemnaturaOrdonator.SignatureField5"
        }
        Dim asteptat = AdobeUtils.SIGNER_AB Or AdobeUtils.SIGNER_CD Or AdobeUtils.SIGNER_ORDONATOR ' = 7
        Assert.Equal(asteptat, AdobeUtils.MascaSemnatari(nume, "ORD"))
    End Sub

    <Fact>
    Public Sub CampNeclasificat_NuAdaugaNimic()
        Assert.Equal(0, AdobeUtils.MascaSemnatari({"form1.NimicRelevant"}, "ORD"))
    End Sub

    <Fact>
    Public Sub ListaGoala_DaZero()
        Assert.Equal(0, AdobeUtils.MascaSemnatari(Array.Empty(Of String)(), "ORD"))
    End Sub

    <Fact>
    Public Sub ListaNothing_DaZero()
        Assert.Equal(0, AdobeUtils.MascaSemnatari(Nothing, "ORD"))
    End Sub

    <Fact>
    Public Sub NumeGolSauNothing_NuArunca_SiNuContribuie()
        Assert.Equal(0, AdobeUtils.MascaSemnatari({"", Nothing}, "DDF"))
    End Sub

    <Fact>
    Public Sub DuplicatIdempotent_AcelasiSemnatarNuSchimbaMasca()
        Dim nume = {"SubformSemnaturaAB.SignatureField1", "SubformSemnaturaAB.SignatureField2"}
        Assert.Equal(AdobeUtils.SIGNER_AB, AdobeUtils.MascaSemnatari(nume, "ORD"))
    End Sub

End Class
