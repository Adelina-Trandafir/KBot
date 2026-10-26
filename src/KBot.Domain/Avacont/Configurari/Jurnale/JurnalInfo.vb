Option Strict On
Imports System.Collections.Generic

''' <summary>One accounting journal of the unit database (slice AV01-09, <c>TZ_Jurnale</c>): sector+source, code (capitals) and name.</summary>
Public NotInheritable Class JurnalInfo
    ''' <summary>0 = a journal not saved yet.</summary>
    Public Property IdJurnal As Integer
    ''' <summary>Sector + source (01A, 02E ...): the title bar's choice; typed on the journal only on «Toate».</summary>
    Public Property SursaSector As String = String.Empty
    Public Property Cod As String = String.Empty
    Public Property Denumire As String = String.Empty
End Class

''' <summary>The journals of the unit database and every sector+source the operator may pick on «Toate».</summary>
Public NotInheritable Class JurnaleCatalog
    Public Property Jurnale As IReadOnlyList(Of JurnalInfo) = New List(Of JurnalInfo)()
    Public Property Surse As IReadOnlyList(Of ClsfCont) = New List(Of ClsfCont)()
End Class
