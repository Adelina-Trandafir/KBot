Option Strict On
Imports Xunit
Imports KBot.Xfa

' Slice 0078: the value written to FX_DDF_REV.Semnatura / FX_ORD.Semnatura, and the key that
' decides whether a save brought a NEW signature. Pure -- no PDF, no Adobe.
Public Class PdfSignatureInfoTests

    <Fact>
    Public Sub Ddf_RolesInCanonicalOrder()
        Dim info As New PdfSignatureInfo({"x"}, AdobeUtils.SIGNER_ORDONATOR Or AdobeUtils.SIGNER_AB, "DDF", Nothing)
        Assert.Equal("A,Ordonator", info.ToSemnatura())
    End Sub

    <Fact>
    Public Sub Ord_UsesAbCdNames()
        Dim info As New PdfSignatureInfo({"x"}, AdobeUtils.SIGNER_AB Or AdobeUtils.SIGNER_CD Or AdobeUtils.SIGNER_ORDONATOR, "ord", Nothing)
        Assert.Equal("AB,CD,Ordonator", info.ToSemnatura())
    End Sub

    <Fact>
    Public Sub NoTrailingComma()
        Dim info As New PdfSignatureInfo({"x"}, AdobeUtils.SIGNER_CD, "DDF", Nothing)
        Assert.Equal("B", info.ToSemnatura())
    End Sub

    <Fact>
    Public Sub Unsigned_IsEmpty()
        Dim info As New PdfSignatureInfo(Nothing, 0, "DDF", Nothing)
        Assert.False(info.IsSigned)
        Assert.Equal("", info.ToSemnatura())
        Assert.Equal("", info.FieldKey())
    End Sub

    <Fact>
    Public Sub FieldKey_IgnoresOrder()
        Dim a As New PdfSignatureInfo({"f2", "f1"}, 1, "DDF", Nothing)
        Dim b As New PdfSignatureInfo({"f1", "f2"}, 1, "DDF", Nothing)
        Assert.Equal(a.FieldKey(), b.FieldKey())
    End Sub

    <Fact>
    Public Sub FieldKey_ChangesWithASecondFieldOfTheSameRole()
        ' DDF role A has SignatureField11..16: a second A-field changes the key, not the roles.
        Dim one As New PdfSignatureInfo({"form1.SignatureField11"}, AdobeUtils.SIGNER_AB, "DDF", Nothing)
        Dim two As New PdfSignatureInfo({"form1.SignatureField11", "form1.SignatureField12"}, AdobeUtils.SIGNER_AB, "DDF", Nothing)
        Assert.Equal(one.ToSemnatura(), two.ToSemnatura())
        Assert.NotEqual(one.FieldKey(), two.FieldKey())
    End Sub

End Class
