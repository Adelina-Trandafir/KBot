Option Strict On

' Slice AV01-08 - the opening balances of a PARTNER (TZ_SoldInitial with IdPartener set; the Access ParteneriSI).

''' <summary>An account a partner's balance can sit on: any active account of any subunit of the unit.</summary>
Public NotInheritable Class ContDisponibil
    Public Property IdCont As Integer
    Public Property Simbol As String = String.Empty
    Public Property Denumire As String = String.Empty
    ''' <summary>The chapter of the account's subunit (65.01 ...), to tell apart equal symbols of two subunits.</summary>
    Public Property Capitol As String = String.Empty
End Class

''' <summary>One row of a partner's balances for one year: the account and its four amounts.</summary>
Public NotInheritable Class SoldPartener
    Public Property IdCont As Integer
    Public Property Simbol As String = String.Empty
    Public Property Denumire As String = String.Empty
    Public Property Capitol As String = String.Empty
    Public Property SoldDebitor As Decimal
    Public Property SoldCreditor As Decimal
    Public Property RulajDebitor As Decimal
    Public Property RulajCreditor As Decimal
End Class
