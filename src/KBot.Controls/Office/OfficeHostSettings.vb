Option Strict On
Imports KBot.Common

''' <summary>
''' The Office host switch the operator sets in «Setări» (slice 0072): how Excel's ribbon is
''' taken down in the hosted preview. Text in <see cref="AppSettings"/>, enum here, because
''' <see cref="ExcelRibbonMode"/> lives in KBot.Controls.
'''
''' Fallback: an unrecognised value falls back to <see cref="ExcelRibbonMode.HideDockWindow"/>
''' (what the DDF file page has used since slice 0049) with a warning for the working log.
''' </summary>
Public NotInheritable Class OfficeHostSettings

    Private Sub New()
    End Sub

    ''' <summary>Stored text -&gt; ribbon mode. Unknown -&gt; HideDockWindow + warning.</summary>
    Public Shared Function ParseExcelRibbon(stored As String) As AdobeSettingRead(Of ExcelRibbonMode)
        If String.IsNullOrWhiteSpace(stored) Then
            Return New AdobeSettingRead(Of ExcelRibbonMode)(ExcelRibbonMode.HideDockWindow, "")
        End If
        Select Case stored.Trim().ToLowerInvariant()
            Case "excel4macro", "macro"
                Return New AdobeSettingRead(Of ExcelRibbonMode)(ExcelRibbonMode.Excel4Macro, "")
            Case "hidedockwindow", "window"
                Return New AdobeSettingRead(Of ExcelRibbonMode)(ExcelRibbonMode.HideDockWindow, "")
            Case Else
                Return New AdobeSettingRead(Of ExcelRibbonMode)(
                    ExcelRibbonMode.HideDockWindow,
                    $"Setarea «ExcelRibbon» are valoarea nerecunoscută «{stored}» — " &
                    "se folosește «HideDockWindow». Valori acceptate: Excel4Macro, HideDockWindow.")
        End Select
    End Function

    ''' <summary>The value to write for a ribbon mode.</summary>
    Public Shared Function ExcelRibbonToText(mode As ExcelRibbonMode) As String
        If mode = ExcelRibbonMode.Excel4Macro Then Return AppSettings.RibbonExcel4Macro
        Return AppSettings.RibbonHideDockWindow
    End Function

    ''' <summary>The Romanian combo label for a ribbon mode.</summary>
    Public Shared Function ExcelRibbonLabel(mode As ExcelRibbonMode) As String
        If mode = ExcelRibbonMode.Excel4Macro Then Return "Prin macro Excel 4 (SHOW.TOOLBAR)"
        Return "Ascunde fereastra panglicii (ca la Word)"
    End Function

    ''' <summary>The ribbon mode in force now.</summary>
    Public Shared Function CurrentExcelRibbon() As AdobeSettingRead(Of ExcelRibbonMode)
        Return ParseExcelRibbon(AppSettings.Current.ExcelRibbon)
    End Function

End Class
