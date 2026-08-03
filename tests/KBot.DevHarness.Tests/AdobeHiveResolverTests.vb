Option Strict On
Imports KBot.DevHarness
Imports Xunit

' Slice 0023, plan §6: the pure hive/product resolver — which AVGeneral hive and which
' FeatureLockDown <product> to use, from key existence + the resolved Adobe exe path.
Public Class AdobeHiveResolverTests

    Private Const ReaderExe As String = "C:\Program Files\Adobe\Acrobat Reader DC\Reader\AcroRd32.exe"
    Private Const AcrobatExe As String = "C:\Program Files\Adobe\Acrobat DC\Acrobat\Acrobat.exe"

    <Fact>
    Public Sub ProductFromExe_Recognized()
        Assert.Equal("Acrobat Reader", AdobeHiveResolver.ProductFromExePath(ReaderExe))
        Assert.Equal("Adobe Acrobat", AdobeHiveResolver.ProductFromExePath(AcrobatExe))
    End Sub

    <Fact>
    Public Sub ProductFromExe_UnknownOrEmpty_ReturnsEmpty()
        Assert.Equal("", AdobeHiveResolver.ProductFromExePath("C:\Tools\SumatraPDF.exe"))
        Assert.Equal("", AdobeHiveResolver.ProductFromExePath(""))
        Assert.Equal("", AdobeHiveResolver.ProductFromExePath(Nothing))
    End Sub

    <Fact>
    Public Sub OnlyReaderHiveExists_PicksReaderHive()
        Dim res = AdobeHiveResolver.Resolve(readerHiveExists:=True, acrobatHiveExists:=False, exePath:=AcrobatExe)
        ' The one existing hive wins regardless of the exe.
        Assert.Equal(AdobeRegistryConstants.AvGeneralReader, res.AvGeneralPath)
    End Sub

    <Fact>
    Public Sub OnlyAcrobatHiveExists_PicksAcrobatHive()
        Dim res = AdobeHiveResolver.Resolve(readerHiveExists:=False, acrobatHiveExists:=True, exePath:=ReaderExe)
        Assert.Equal(AdobeRegistryConstants.AvGeneralAcrobat, res.AvGeneralPath)
    End Sub

    <Fact>
    Public Sub BothExist_ExeBreaksTheTie()
        Dim reader = AdobeHiveResolver.Resolve(True, True, ReaderExe)
        Assert.Equal(AdobeRegistryConstants.AvGeneralReader, reader.AvGeneralPath)
        Assert.Equal("Acrobat Reader", reader.PolicyProduct)

        Dim acrobat = AdobeHiveResolver.Resolve(True, True, AcrobatExe)
        Assert.Equal(AdobeRegistryConstants.AvGeneralAcrobat, acrobat.AvGeneralPath)
        Assert.Equal("Adobe Acrobat", acrobat.PolicyProduct)
    End Sub

    <Fact>
    Public Sub BothExist_UnknownExe_DefaultsToAcrobat()
        ' Plan §2.1: the Acrobat hive covers "Acrobat, and newer builds" — the safer default.
        Dim res = AdobeHiveResolver.Resolve(True, True, "")
        Assert.Equal(AdobeRegistryConstants.AvGeneralAcrobat, res.AvGeneralPath)
        Assert.Equal("Adobe Acrobat", res.PolicyProduct)
    End Sub

    <Fact>
    Public Sub NeitherExists_DerivedFromExe_SoAWriteCreatesTheRightKey()
        Dim reader = AdobeHiveResolver.Resolve(False, False, ReaderExe)
        Assert.Equal(AdobeRegistryConstants.AvGeneralReader, reader.AvGeneralPath)
        Assert.Equal("Acrobat Reader", reader.PolicyProduct)

        Dim unknown = AdobeHiveResolver.Resolve(False, False, "C:\x\viewer.exe")
        Assert.Equal(AdobeRegistryConstants.AvGeneralAcrobat, unknown.AvGeneralPath)
        Assert.Equal("Adobe Acrobat", unknown.PolicyProduct)
    End Sub

    <Fact>
    Public Sub Resolution_ReportsHiveExistenceAndExeProduct()
        Dim res = AdobeHiveResolver.Resolve(True, False, ReaderExe)
        Assert.True(res.ReaderHiveExists)
        Assert.False(res.AcrobatHiveExists)
        Assert.Equal("Acrobat Reader", res.ProductFromExe)
    End Sub

End Class
