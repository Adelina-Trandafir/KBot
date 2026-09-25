Option Strict On
Imports System.Collections.Generic

' What GET /api/forexe/extrase/lista answers (slice 0080-02 / 0080-03): the bank statements
' of one angajament (the Extrase view) or of the whole database (the «Extrase de cont»
' window). Two flat lists; the client builds the tree and both grids from them.
'
'   Antete     = FX_Extrase_H -- one account header of a statement: its classification and
'                the four pairs of balances (SID/SIC, RPD/RPC, TSD/TSC, SFD/SFC).
'   Operatiuni = FX_Extrase   -- one bank operation, linked to its header by IdFxh.
'
' Where a header sits in the tree is decided by its operations' DataBanca; a header with no
' operation sits on its statement's DataExtras (operator, 24.09.2026).

Public NotInheritable Class ExtrasAntet
    Public Property IdExh As Integer
    Public Property IdExf As Integer?
    ''' <summary>FX_Extrase_F.DataExtras -- the statement's date; shown in the «Data» column.</summary>
    Public Property DataExtras As Date?
    Public Property NumarExtras As String = String.Empty
    ''' <summary>Clasificatii.IDClsf (the MariaDB key since slice 0080-01).</summary>
    Public Property IdClsf As Integer?
    Public Property Clsf As String = String.Empty
    Public Property Denumire As String = String.Empty
    Public Property CodIban As String = String.Empty
    Public Property Cont As String = String.Empty
    Public Property Sid As Double
    Public Property Sic As Double
    Public Property Rpd As Double
    Public Property Rpc As Double
    Public Property Tsd As Double
    Public Property Tsc As Double
    Public Property Sfd As Double
    Public Property Sfc As Double
End Class

Public NotInheritable Class ExtrasOperatiune
    Public Property IdFxe As Integer
    ''' <summary>The owning FX_Extrase_H row (IDEXH). Nothing for an orphan operation.</summary>
    Public Property IdFxh As Integer?
    Public Property DataBanca As Date?
    Public Property DataDoc As Date?
    Public Property NrDoc As String = String.Empty
    Public Property Referinta As String = String.Empty
    Public Property ReferintaDest As String = String.Empty
    Public Property PlatitorNume As String = String.Empty
    Public Property PlatitorCui As String = String.Empty
    Public Property PlatitorIban As String = String.Empty
    Public Property SumaDebit As Double
    Public Property SumaCredit As Double
    Public Property Explicatii As String = String.Empty
    ''' <summary>FX_Extrase.CodContract = the CodAngajament. Empty on rows that are not an angajament's.</summary>
    Public Property CodContract As String = String.Empty
    ''' <summary>FX_Extrase.RandContract = the CodIndicator. Never present without CodContract.</summary>
    Public Property RandContract As String = String.Empty
    Public Property CodProgram As String = String.Empty
    Public Property CodAi As String = String.Empty
End Class

Public NotInheritable Class ExtraseInfo
    Public ReadOnly Property Antete As New List(Of ExtrasAntet)()
    Public ReadOnly Property Operatiuni As New List(Of ExtrasOperatiune)()
End Class
