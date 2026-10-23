Option Strict On
Imports System.Collections.Generic

' Slice 0123-01 -- what the ADECHIT routes answer to the Rapoarte page (PYTHON/routes/adechit/, read calls opened to the
' K-BOT bearer token by kbot_bridge.py). Plain data, no logic.

''' <summary>A subunit (evidence) of the open unit, the one the reports are made for.</summary>
Public NotInheritable Class AdechitSubunit
    Public Property Id As Integer
    Public Property Name As String = String.Empty
End Class

''' <summary>What <c>GET /api/adechit/context</c> says about the signed-in user in the open unit.</summary>
Public NotInheritable Class AdechitContext
    Public ReadOnly Property Subunits As New List(Of AdechitSubunit)()

    ''' <summary>The operations the user's roles allow in ADECHIT (<c>read</c>, <c>cancel</c>, ...).</summary>
    Public ReadOnly Property Permissions As New List(Of String)()
End Class

''' <summary>A month of the evidence (<c>AD_LunaD</c>).</summary>
Public NotInheritable Class AdechitMonth
    Public Property Id As Integer
    Public Property Year As Integer
    Public Property MonthNumber As Integer

    ''' <summary>The month's name as the evidence keeps it.</summary>
    Public Property MonthName As String = String.Empty
    Public Property Closed As Boolean
End Class

''' <summary>A group that had attendance in a month.</summary>
Public NotInheritable Class AdechitGroup
    Public Property Id As Integer
    Public Property Name As String = String.Empty
    Public Property Educators As String = String.Empty
    Public Property Hidden As Boolean
End Class

''' <summary>A child found by name or CNP in a month.</summary>
Public NotInheritable Class AdechitChild
    ''' <summary>The child's id (<c>IDP</c>), what the account sheet is asked for.</summary>
    Public Property ChildId As Integer
    Public Property Name As String = String.Empty
    Public Property GroupName As String = String.Empty

    ''' <summary>The last digits of the CNP, to tell namesakes apart.</summary>
    Public Property CnpTail As String = String.Empty
End Class

''' <summary>The answer of a child search: at most 100 rows, <see cref="HasMore"/> when there were more.</summary>
Public NotInheritable Class AdechitChildSearch
    Public ReadOnly Property Rows As New List(Of AdechitChild)()
    Public Property HasMore As Boolean
End Class

''' <summary>A report the server built and saved for a few minutes; PDF and Excel are asked from it by <see cref="Snapshot"/>.</summary>
Public NotInheritable Class AdechitReportHandle
    Public Property Snapshot As String = String.Empty
    Public Property Title As String = String.Empty

    ''' <summary>The file name without extension.</summary>
    Public Property FileName As String = String.Empty
End Class

''' <summary>The kinds of report of the Rapoarte page (the <c>kind</c> of the routes).</summary>
Public NotInheritable Class AdechitReportKind

    Private Sub New()
    End Sub

    Public Const Situation As String = "situation"
    Public Const PaymentNotice As String = "payment-notice"
    Public Const Cash As String = "cash"
    Public Const Bank As String = "bank"
    Public Const Cancelled As String = "cancelled"
    Public Const Account As String = "account"
End Class
