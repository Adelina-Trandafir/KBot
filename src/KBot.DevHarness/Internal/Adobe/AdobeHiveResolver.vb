Option Strict On
Imports System.IO

' Result of resolving which AVGeneral hive + policy product to use (slice 0023, plan §3.E/§3.F).
Public NotInheritable Class AdobeHiveResolution
    Public Property AvGeneralPath As String        ' full HKCU path chosen
    Public Property PolicyProduct As String        ' "Acrobat Reader" / "Adobe Acrobat"
    Public Property ProductFromExe As String       ' "" when the exe name is not recognised
    Public Property ReaderHiveExists As Boolean
    Public Property AcrobatHiveExists As Boolean
End Class

' Pure resolver: given which of the two AVGeneral hives exist and the resolved Adobe exe path,
' picks the AVGeneral hive to write and the policy <product> string. No registry I/O here — the
' existence booleans are supplied by the caller (which probed the registry).
Public NotInheritable Class AdobeHiveResolver

    Private Sub New()
    End Sub

    ' "AcroRd32.exe" -> classic Reader; "Acrobat.exe" -> Acrobat/newer; anything else -> "".
    Public Shared Function ProductFromExePath(exePath As String) As String
        If String.IsNullOrWhiteSpace(exePath) Then Return ""
        Dim f As String = Path.GetFileName(exePath).ToLowerInvariant()
        If f = "acrord32.exe" Then Return AdobeRegistryConstants.ProductReader
        If f = "acrobat.exe" Then Return AdobeRegistryConstants.ProductAcrobat
        Return ""
    End Function

    Public Shared Function Resolve(readerHiveExists As Boolean, acrobatHiveExists As Boolean,
                                   exePath As String) As AdobeHiveResolution
        Dim product As String = ProductFromExePath(exePath)
        Dim res As New AdobeHiveResolution With {
            .ReaderHiveExists = readerHiveExists,
            .AcrobatHiveExists = acrobatHiveExists,
            .ProductFromExe = product
        }

        ' Hive: exactly-one -> that one; both -> prefer the product from the exe, unknown -> Acrobat
        ' (the plan notes the Acrobat hive covers "Acrobat, and newer builds"); neither -> derive
        ' from the product so a write creates the right key, unknown -> Acrobat.
        If readerHiveExists AndAlso Not acrobatHiveExists Then
            res.AvGeneralPath = AdobeRegistryConstants.AvGeneralReader
        ElseIf acrobatHiveExists AndAlso Not readerHiveExists Then
            res.AvGeneralPath = AdobeRegistryConstants.AvGeneralAcrobat
        Else
            ' both-exist or neither-exist: same tie-break.
            res.AvGeneralPath = If(product = AdobeRegistryConstants.ProductReader,
                                   AdobeRegistryConstants.AvGeneralReader,
                                   AdobeRegistryConstants.AvGeneralAcrobat)
        End If

        ' Policy product: from the exe when known; otherwise inferred from the chosen hive.
        If product <> "" Then
            res.PolicyProduct = product
        ElseIf res.AvGeneralPath = AdobeRegistryConstants.AvGeneralReader Then
            res.PolicyProduct = AdobeRegistryConstants.ProductReader
        Else
            res.PolicyProduct = AdobeRegistryConstants.ProductAcrobat
        End If

        Return res
    End Function

End Class
