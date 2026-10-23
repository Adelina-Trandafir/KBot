Option Strict On
Imports System.Collections.Generic

' Slice 0123-02 -- what the VERCON routes answer to the Rapoarte page (PYTHON/routes/vercon/, the calls opened to the K-BOT bearer token
' by kbot_bridge.py). Plain data, no logic.

''' <summary>A subunit of the open unit the user may work in.</summary>
Public NotInheritable Class VerconSubunit
    Public Property Id As Integer
    Public Property Name As String = String.Empty
End Class

''' <summary>A gestiune of a subunit; <see cref="GestiuneType"/> is MATERIALE, ALIMENTE, OI or MF.</summary>
Public NotInheritable Class VerconGestiune
    Public Property Id As Integer
    Public Property Name As String = String.Empty
    Public Property GestiuneType As String = String.Empty
End Class

''' <summary>The choice the server accepted: subunit, gestiune, year (each Nothing until chosen) and the gestiune's type.</summary>
Public NotInheritable Class VerconSelection
    Public Property Subunit As Integer?
    Public Property Gestiune As Integer?
    Public Property Year As Integer?
    Public Property GestiuneType As String = String.Empty

    Public Function Clone() As VerconSelection
        Return New VerconSelection() With {.Subunit = Subunit, .Gestiune = Gestiune, .Year = Year, .GestiuneType = GestiuneType}
    End Function
End Class

''' <summary>What <c>GET /api/vercon/context</c> says: the accepted choice and the options below each level.</summary>
Public NotInheritable Class VerconContext
    Public Property Selection As New VerconSelection()
    Public ReadOnly Property Subunits As New List(Of VerconSubunit)()
    Public ReadOnly Property Gestiuni As New List(Of VerconGestiune)()
    Public ReadOnly Property Years As New List(Of Integer)()

    ''' <summary>False when the unit has no gestiune of work yet.</summary>
    Public Property Ready As Boolean
End Class

''' <summary>One report of the catalog with the flags that decide its options and its selection tree.</summary>
Public NotInheritable Class VerconReportSpec
    Public Property Id As String = String.Empty
    Public Property Label As String = String.Empty
    Public Property Kind As String = String.Empty
    Public Property Group As String = String.Empty

    ''' <summary>When the server gives it (the inventory-object reports), the group of the report tree; then the tree is exactly these groups.</summary>
    Public Property TreeGroup As String = String.Empty
    Public Property Family As String = String.Empty
    Public Property Ledger As String = String.Empty
    Public ReadOnly Property Types As New List(Of String)()

    ''' <summary>The filter families the report accepts: variant, category, supplier, receiver, document.</summary>
    Public ReadOnly Property Filters As New List(Of String)()

    ''' <summary>Non-empty for the fixed-asset (MF) reports: the mode of their selection tree.</summary>
    Public Property MfSelection As String = String.Empty
    Public Property DocumentOptions As Boolean
    Public Property ReceiverOptions As Boolean
    Public Property OutgoingSelection As Boolean
    Public Property IncomingSelection As Boolean
    Public Property OutgoingFilters As Boolean
    Public Property VariantRequired As Boolean
    Public Property SupplierRequired As Boolean
    Public Property DocumentRequired As Boolean

    Public Function Required(k_key As String) As Boolean
        Select Case k_key
            Case "variant" : Return VariantRequired
            Case "supplier" : Return SupplierRequired
            Case "document" : Return DocumentRequired
            Case Else : Return False
        End Select
    End Function
End Class

''' <summary>The days a year holds documents on: the first and the last, and the last month with a movement.</summary>
Public NotInheritable Class VerconPeriod
    Public Property Year As Integer
    Public Property MinDate As String = String.Empty
    Public Property MaxDate As String = String.Empty
    Public Property LastMovementMonth As Integer
End Class

''' <summary>What <c>GET /api/vercon/reports/catalog</c> says: the reports of the gestiune's type and the periods it has data in.</summary>
Public NotInheritable Class VerconCatalog
    Public ReadOnly Property Reports As New List(Of VerconReportSpec)()
    Public ReadOnly Property Periods As New List(Of VerconPeriod)()
    Public ReadOnly Property Years As New List(Of Integer)()
    Public Property LatestYear As Integer

    ''' <summary>"Anul curent pana azi", when the server proposes it (ISO dates; empty otherwise).</summary>
    Public Property DefaultStart As String = String.Empty
    Public Property DefaultEnd As String = String.Empty

    ''' <summary>The span of all the years ("TOTI anii"), when the server proposes it (ISO dates; empty otherwise).</summary>
    Public Property AllYearsStart As String = String.Empty
    Public Property AllYearsEnd As String = String.Empty
End Class

''' <summary>A document of a selection (its label is what the report header shows when one is chosen).</summary>
Public NotInheritable Class VerconSelDocument
    Public Property Id As String = String.Empty
    Public Property Label As String = String.Empty
End Class

''' <summary>A node of the selection tree. <see cref="Lazy"/>: its children are asked for when it is opened.</summary>
Public NotInheritable Class VerconSelNode
    Public Property Id As String = String.Empty
    Public Property Label As String = String.Empty
    Public Property Lazy As Boolean
    Public ReadOnly Property Children As New List(Of VerconSelNode)()
End Class

''' <summary>A branch of the selection tree (the root, or the children of one node) and the documents it revealed.</summary>
Public NotInheritable Class VerconBranch
    Public ReadOnly Property Nodes As New List(Of VerconSelNode)()
    Public ReadOnly Property Documents As New List(Of VerconSelDocument)()
End Class

''' <summary>A report the server built and saved for a few minutes; PDF and Excel are asked from it by <see cref="Snapshot"/>.</summary>
Public NotInheritable Class VerconReportHandle
    Public Property Snapshot As String = String.Empty
    Public Property Title As String = String.Empty

    ''' <summary>The file name without extension.</summary>
    Public Property FileName As String = String.Empty
End Class
