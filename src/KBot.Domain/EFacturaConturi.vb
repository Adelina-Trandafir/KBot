Option Strict On

' Slice 00EF-12 -- the bank accounts of the unit as the ISSUER of the invoices (PYTHON/routes/efactura/facturi.py,
' EF_FurnizorConturi). Plain data, no logic. These are the unit's own accounts, not the partners' ones.

''' <summary>One IBAN of the unit and the bank the server deduced from it.</summary>
Public NotInheritable Class EFacturaCont

    ''' <summary>0 = not saved yet.</summary>
    Public Property IdCont As Integer

    ''' <summary>The IBAN as the server keeps it: upper case, no spaces.</summary>
    Public Property Cont As String = String.Empty

    ''' <summary>The name of the bank (characters 5-8 of the IBAN looked up in the bank list); empty when the code is unknown. Read only: the server writes it.</summary>
    Public Property Banca As String = String.Empty

End Class
