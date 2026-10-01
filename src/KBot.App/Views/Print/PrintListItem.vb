Option Strict On
Imports System.Threading.Tasks
Imports KBot.Api

''' <summary>
''' One row of the print list (slice 0099): a revision of a DDF or an ordonantare, with what the list
''' shows about it and how to get its PDF. Built by the view that owns the data (<c>DdfView</c> /
''' <c>OrdView</c>); read by <see cref="PrintListPage"/>, which knows nothing about DDF or ORD.
''' POCO + two delegates -&gt; no Try/Catch.
''' </summary>
Friend NotInheritable Class PrintListItem

    ''' <summary>Which server document this is -- the print counter's key.</summary>
    Public ReadOnly Property Kind As PrintedDocumentKind
    ''' <summary>IDREV / IDORDP, by <see cref="Kind"/>.</summary>
    Public ReadOnly Property Id As Integer

    ''' <summary>What the first column says (Romanian, operator-visible): the revision or ordonantare.</summary>
    Public Property Label As String = String.Empty
    ''' <summary>The signer roles written by the signing upload ("A,B,Ordonator"); empty when unsigned.</summary>
    Public Property Signatures As String = String.Empty
    ''' <summary>When the signed PDF was last written on the server; Nothing when unsigned.</summary>
    Public Property SignedAt As Date?
    ''' <summary>How many times the document was printed (PDF row + document row). «Listat» is
    ''' <c>PrintCount &gt; 0</c>.</summary>
    Public Property PrintCount As Integer

    ''' <summary>
    ''' Puts the document's PDF on this computer and returns its path: the signed copy from the server
    ''' (through the local cache), or -- for an unsigned one -- the document generated into the work
    ''' area. Never opens anything on screen. Throws with a Romanian message the list shows.
    ''' </summary>
    Public Property ObtainPdfAsync As Func(Of Task(Of String))

    ''' <summary>Called with the new count after the server counted a print, so the view's own row
    ''' (<c>RevizieRow</c> / <c>OrdHeaderRow</c>) stays equal to what the list shows.</summary>
    Public Property CountChanged As Action(Of Integer)

    Public Sub New(kind As PrintedDocumentKind, id As Integer)
        Me.Kind = kind
        Me.Id = id
    End Sub

    Public ReadOnly Property IsListed As Boolean
        Get
            Return PrintCount > 0
        End Get
    End Property

    ''' <summary>Roles for the grid: «A, B, Ordonator».</summary>
    Public ReadOnly Property SignaturesText As String
        Get
            If String.IsNullOrWhiteSpace(Signatures) Then Return String.Empty
            Return String.Join(", ", Signatures.Split(","c).
                Select(Function(s) s.Trim()).
                Where(Function(s) s.Length > 0))
        End Get
    End Property

End Class
