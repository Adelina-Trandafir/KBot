Imports KBot.Common

''' <summary>
''' Reads a DC's CodFiscal out of the legacy VBA application's settings store.
''' </summary>
''' <remarks>
''' <para>
''' Decision D9. The value lives at
''' <c>HKEY_CURRENT_USER\Software\VB and VBA Program Settings\AVACONT\&lt;DC&gt;</c> under
''' the name <c>CodFiscal</c>, one value per DC, where <c>&lt;DC&gt;</c> is the database
''' name (<c>000_DEMO</c>). That path is not a convention somebody chose here - it is
''' exactly where VB's own <c>SaveSetting</c>/<c>GetSetting</c> pair puts things, which is
''' why this reads it with <c>GetSetting</c> rather than opening the key by hand: the two
''' cannot drift apart, and the VBA application writes it with <c>SaveSetting</c>.
''' </para>
''' <para>
''' Verified on the operator's machine, 24.08: <c>000_DEMO</c> ▸ <c>2842919</c>, which is
''' the same seven digits every one of the 72 <c>FX_Extrase_F.NumeFisier</c> values
''' carries. Sixteen other DCs are present in the same store, each with its own value.
''' </para>
''' </remarks>
Public NotInheritable Class CodFiscalRegistry

    ''' <summary>The application name the VBA estate registers itself under.</summary>
    Public Const ApplicationName As String = "AVACONT"

    ''' <summary>The value name inside the DC's section.</summary>
    Public Const ValueName As String = "CodFiscal"

    Private Sub New()
    End Sub

    ''' <summary>
    ''' The CodFiscal recorded for one DC, or an empty string when there is none.
    ''' </summary>
    ''' <remarks>
    ''' Never throws: a missing key, a missing value and an unreadable hive are all "no
    ''' value recorded", and the caller turns that into the COD_FISCAL_LIPSA finding
    ''' rather than a crash. Returning empty is not the same as accepting empty - D15
    ''' makes the empty case blocking one level up.
    ''' </remarks>
    Public Shared Function ForDc(dc As String) As String
        If String.IsNullOrWhiteSpace(dc) Then Return String.Empty
        Try
            Dim value = Microsoft.VisualBasic.Interaction.GetSetting(
                ApplicationName, dc.Trim(), ValueName, String.Empty)
            Return If(value, String.Empty).Trim()
        Catch ex As Exception
            GlobalErrorLog.Write("CodFiscalRegistry.ForDc", ex)
            Return String.Empty
        End Try
    End Function

End Class
