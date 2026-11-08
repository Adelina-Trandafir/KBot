Option Strict On
Imports System.Globalization

''' <summary>SLICE-ME01-25. Column sorting and filtering for KBOTADMIN grids.</summary>
Friend Class AdminDataView
    Inherits KBotDataView

    Friend Shared ReadOnly RomanianText As StringComparer = StringComparer.Create(CultureInfo.GetCultureInfo("ro-RO"), True)

    Public Sub New()
        ShowDataColumnMenus = True
        TextSortComparer = RomanianText
    End Sub

    Friend Function FirstDisplayedRowIndex() As Integer
        Return ModelIndexAt(0)
    End Function

    ''' <summary>Converts server timestamps for sorting, preserving unrecognized display text.</summary>
    Friend Shared Function DateValue(k_text As String) As Object
        Dim k_date As Date
        If Date.TryParse(k_text, CultureInfo.InvariantCulture, DateTimeStyles.None, k_date) Then Return k_date
        Return If(k_text, String.Empty).Replace("T", " ")
    End Function
End Class
