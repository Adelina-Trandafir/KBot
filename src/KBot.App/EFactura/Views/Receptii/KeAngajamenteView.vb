Option Strict On
Imports System.Collections.Generic
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain

''' <summary>SLICE-KE21-01: what the table of angajamente asks of the window or the view that holds it (the invoices, the suppliers, the notices).</summary>
Public Interface IKeAngajamenteHost

    ''' <summary>The tax codes of the suppliers the table is about, one per supplier; an empty text for a supplier whose invoices carry none.</summary>
    Function SupplierTaxCodes() As List(Of String)

    ''' <summary>The states of the invoices the table is about are read again (the supplier's links changed).</summary>
    Function RefreshContextAsync() As Task

    ''' <summary>The manual links the invoices on screen have with a DDF of this angajament are removed; returns how many were removed.</summary>
    Function RemoveManualLinksAsync(k_cod As String) As Task(Of Integer)

    Sub ShowNotice(k_text As String, k_kind As NoticeKind)
End Interface

''' <summary>
''' SLICE-KE21-01: the table of angajamente of the reception work, in the left column under the tree of the invoices. ONE control for both places that do the work (the page «Recepții &lt;&gt; E-Factura»
''' and the view «Recepții» of an invoice): the table does NOT live in the view of the reception. It owns the choices of the table, the data of every angajament read so far and the angajament in
''' use; the host says which invoices it is about (<see cref="Fill"/> / <see cref="FillViewed"/>) and hears the angajament in use through <see cref="AngajamentReady"/>. The code is split by
''' responsibility: this file (state and surface), <c>.Fill</c> (the rows), <c>.Grid</c> (what the grid raises), <c>.Load</c> (the data of an angajament), <c>.Link</c> (adding and removing).
''' </summary>
Public Class KeAngajamenteView

    Friend Const ColAng As String = "ang"
    Friend Const ColOpt As String = "opt"

    ' A row of the table.
    Friend NotInheritable Class AngItem
        Public Property Cod As String = String.Empty
        Public Property Descriere As String = String.Empty

        ' «ddf», «legatura» or «furnizor» (as the server says), or «fix».
        Public Property Sursa As String = String.Empty

        ' Slice KE14: a reception is linked to an invoice on this angajament: it can no longer be taken off the list.
        Public Property Linked As Boolean

        Public ReadOnly Property Eticheta As String
            Get
                Return If(String.IsNullOrWhiteSpace(Descriere), Cod, $"{Cod} — {Descriere}")
            End Get
        End Property
    End Class

    Private _api As IEFacturaApi
    Private _gate As ReauthGate
    Private _cts As CancellationTokenSource
    Private _host As IKeAngajamenteHost
    Private _items As New List(Of AngItem)()
    Private _allAng As New List(Of EFacturaKeAngajamentAlegere)()
    Private _allAngLoaded As Boolean
    Private ReadOnly _angByLabel As New Dictionary(Of String, EFacturaKeAngajamentAlegere)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly _angCache As New Dictionary(Of String, EFacturaKeReceptiiAngajament)(StringComparer.OrdinalIgnoreCase)
    Private _angCod As String = String.Empty
    Private _data As EFacturaKeReceptiiAngajament
    Private _angSeq As Integer
    Private _othersSeq As Integer
    Private _angFilling As Boolean
    Private _fixedAng As String = String.Empty
    Private _shortLabels As Boolean
    Private _viewMode As Boolean

    ''' <summary>The angajament in use changes: what the host kept for the former one (budget, amounts above the lines, the message) is forgotten.</summary>
    Public Event AngajamentChanging As EventHandler

    ''' <summary>The data (receptions, indicators) of the angajament in use are on screen's side: the host puts them to work.</summary>
    Public Event AngajamentReady As EventHandler

    ''' <summary>No angajament is in use any more (its invoice was unticked, or another supplier was pressed).</summary>
    Public Event AngajamentCleared As EventHandler

    ''' <summary>The indicators of the other angajamente of the table were read: the classifications that can be chosen grew.</summary>
    Public Event OthersLoaded As EventHandler

    ''' <summary>The table has to be filled again (an angajament was added, or a link taken off): the host knows the invoices it is about.</summary>
    Public Event RefillRequested As EventHandler

    Public Sub New()
        InitializeComponent()
    End Sub

    ''' <summary>Once, before use: the server, the gate, the token of the host and the host itself.</summary>
    Public Sub Initialize(k_api As IEFacturaApi, k_gate As ReauthGate, k_cts As CancellationTokenSource, k_host As IKeAngajamenteHost)
        ArgumentNullException.ThrowIfNull(k_api)
        ArgumentNullException.ThrowIfNull(k_gate)
        ArgumentNullException.ThrowIfNull(k_cts)
        ArgumentNullException.ThrowIfNull(k_host)
        _api = k_api
        _gate = k_gate
        _cts = k_cts
        _host = k_host
    End Sub

    ' Built while its page is hidden: the first time it is shown the columns are worked out again.
    Protected Overrides Sub OnVisibleChanged(e As EventArgs)
        MyBase.OnVisibleChanged(e)
        Try
            If Visible AndAlso IsHandleCreated Then BeginInvoke(New Action(Sub() If Not IsDisposed Then gridAng.AutoSizeColumns()))
        Catch ex As Exception
            GlobalErrorLog.Write("KeAngajamenteView.OnVisibleChanged", ex)
        End Try
    End Sub

    ''' <summary>The grid itself (for the host's layout calls).</summary>
    Friend ReadOnly Property Grid As KBotDataView
        Get
            Return gridAng
        End Get
    End Property

    ''' <summary>The code of the angajament in use; empty when none.</summary>
    Public Property CurrentCod As String
        Get
            Return _angCod
        End Get
        Set(k_value As String)
            _angCod = If(k_value, String.Empty)
        End Set
    End Property

    ''' <summary>The data of the angajament in use; Nothing while they are being read.</summary>
    Public Property CurrentData As EFacturaKeReceptiiAngajament
        Get
            Return _data
        End Get
        Set(k_value As EFacturaKeReceptiiAngajament)
            _data = k_value
        End Set
    End Property

    ''' <summary>The view of ONE angajament (its code), or empty: that angajament and no other is offered and it cannot be taken off.</summary>
    Public Property FixedAngajament As String
        Get
            Return _fixedAng
        End Get
        Set(k_value As String)
            _fixedAng = If(k_value, String.Empty).Trim()
        End Set
    End Property

    ''' <summary>Inside a view of the invoices the rows show the description only (the code when it has none).</summary>
    Public Property ShortLabels As Boolean
        Get
            Return _shortLabels
        End Get
        Set(k_value As Boolean)
            _shortLabels = k_value
        End Set
    End Property

    ''' <summary>The list of angajamente the empty row offers was read.</summary>
    Public ReadOnly Property AllLoaded As Boolean
        Get
            Return _allAngLoaded
        End Get
    End Property

    ''' <summary>The data of the angajament in use first, then of every other angajament of the table read so far (KE12-04).</summary>
    Public Function KnownData() As List(Of EFacturaKeReceptiiAngajament)
        Dim k_list As New List(Of EFacturaKeReceptiiAngajament)()
        If _data IsNot Nothing Then k_list.Add(_data)
        For Each k_a As AngItem In _items
            Dim k_d As EFacturaKeReceptiiAngajament = Nothing
            If _angCache.TryGetValue(k_a.Cod, k_d) AndAlso Not k_list.Contains(k_d) Then k_list.Add(k_d)
        Next
        Return k_list
    End Function

    ''' <summary>Forgets what was kept about the former invoices: the data of the angajamente, the answers still on their way.</summary>
    Public Sub ResetCache()
        Interlocked.Increment(_angSeq)
        _angCache.Clear()
    End Sub

    ''' <summary>Only the answers still on their way are dropped (the data kept stay).</summary>
    Public Sub CancelPending()
        Interlocked.Increment(_angSeq)
        Interlocked.Increment(_othersSeq)
    End Sub

    ' Slice KE12-04: a classification that is not an indicator of the angajament in use moves the use to the first angajament that has it (the row of the table follows).
    Public Sub EnsureAngajamentFor(k_idClsf As Integer)
        If _data Is Nothing OrElse _data.Indicatori.Any(Function(k_i) k_i.IdClsf.HasValue AndAlso k_i.IdClsf.Value = k_idClsf) Then Return
        For k_index As Integer = 0 To _items.Count - 1
            Dim k_d As EFacturaKeReceptiiAngajament = Nothing
            If _angCache.TryGetValue(_items(k_index).Cod, k_d) AndAlso k_d.Indicatori.Any(Function(k_i) k_i.IdClsf.HasValue AndAlso k_i.IdClsf.Value = k_idClsf) Then
                gridAng.CurrentRowIndex = k_index
                Return
            End If
        Next
    End Sub

End Class
