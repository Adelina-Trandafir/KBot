Option Strict On
Imports System.Text.RegularExpressions

''' <summary>The kinds of data a window may keep in memory and must be told about when another place changes them (slice 0126-05).</summary>
<Flags>
Public Enum DataKinds As Integer
    None = 0
    ''' <summary>The budget versions or the corrections of a classification.</summary>
    Buget = 1
    ''' <summary>The angajamente and their indicators (what the robot downloads).</summary>
    Angajamente = 2
    Rezervari = 4
    Receptii = 8
    Plati = 16
End Enum

''' <summary>What changed: the kinds and, when the write names one, the classification.</summary>
Public NotInheritable Class DataChangedEventArgs
    Inherits EventArgs

    Public Sub New(k_kinds As DataKinds, k_idClsf As Integer?)
        Kinds = k_kinds
        IdClsf = k_idClsf
    End Sub

    Public ReadOnly Property Kinds As DataKinds
    ''' <summary>Set when the write is on ONE classification (a budget save); Nothing otherwise.</summary>
    Public ReadOnly Property IdClsf As Integer?
End Class

''' <summary>
''' Slice 0126-05: the one place that announces «the data changed». A window that keeps data in memory (the «Clasificatii» window keeps the
''' budgets, the corrections and the angajamente of every classification) listens to <see cref="Changed"/> and brings what it holds up to date.
''' It is raised by <c>ServerGateHandler</c> -- the one place every request of this program passes, the robot's ingest included -- after a
''' WRITE got a success answer; <see cref="Classify"/> decides from the route which kinds that write touched. A write that no rule knows
''' raises nothing, so a new route that changes one of these data must get a rule here.
''' Raised on whatever thread finished the request: a listener marshals to its own thread. Another PC's changes are NOT seen (this is
''' in-process).
''' </summary>
Public NotInheritable Class DataChangeBus

    Private Sub New()
    End Sub

    ''' <summary>Raised after a write that touched the data (any thread).</summary>
    Public Shared Event Changed As EventHandler(Of DataChangedEventArgs)

    ''' <summary>Tells every listener; one that throws does not stop the others.</summary>
    Public Shared Sub Raise(k_kinds As DataKinds, Optional k_idClsf As Integer? = Nothing)
        If k_kinds = DataKinds.None Then Return
        Dim k_args As New DataChangedEventArgs(k_kinds, k_idClsf)
        Dim k_handlers As EventHandler(Of DataChangedEventArgs) = ChangedEvent
        If k_handlers Is Nothing Then Return
        For Each k_d As [Delegate] In k_handlers.GetInvocationList()
            Try
                DirectCast(k_d, EventHandler(Of DataChangedEventArgs)).Invoke(Nothing, k_args)
            Catch ex As Exception
                GlobalErrorLog.Write("DataChangeBus.Raise", ex)
            End Try
        Next
    End Sub

    Private Shared ReadOnly BudgetRoute As New Regex("/clasificatii/(\d+)/buget$", RegexOptions.Compiled Or RegexOptions.IgnoreCase Or RegexOptions.CultureInvariant)
    Private Const Everything As DataKinds = DataKinds.Angajamente Or DataKinds.Rezervari Or DataKinds.Receptii Or DataKinds.Plati

    ''' <summary>
    ''' The kinds a successful write to this route touched (None = none the windows keep). Pure, so it can be read and tested without a server.
    ''' A POST that only READS is no problem: it costs a refresh, never a wrong screen; but the routes the memory itself reads with must
    ''' not match here, or a refresh would start the next one.
    ''' </summary>
    Public Shared Function Classify(k_path As String, ByRef k_idClsf As Integer?) As DataKinds
        k_idClsf = Nothing
        If String.IsNullOrEmpty(k_path) Then Return DataKinds.None
        Dim k_p As String = k_path.TrimEnd("/"c).ToLowerInvariant()
        Dim k_m As Match = BudgetRoute.Match(k_p)
        If k_m.Success Then
            k_idClsf = Integer.Parse(k_m.Groups(1).Value, Globalization.CultureInfo.InvariantCulture)
            Return DataKinds.Buget
        End If
        ' The robot's ingest and its processing: the whole picture of the angajamente.
        If k_p.EndsWith("/angajamente/upsert", StringComparison.Ordinal) OrElse k_p.EndsWith("/prelucrare", StringComparison.Ordinal) OrElse
           k_p.Contains("/prelucrare/") Then Return Everything
        If k_p.Contains("/asociere") Then Return DataKinds.Rezervari Or DataKinds.Receptii Or DataKinds.Plati
        If k_p.Contains("/rezervari/") OrElse k_p.Contains("/ddf/save") OrElse k_p.Contains("/ddf/rev/") OrElse
           Regex.IsMatch(k_p, "/ddf/\d+(/luna/\d+/\d+)?$") Then Return DataKinds.Rezervari
        If k_p.Contains("/receptii/") Then Return DataKinds.Receptii
        If k_p.Contains("/ord/save") OrElse Regex.IsMatch(k_p, "/ord/\d+$") OrElse k_p.Contains("/ord/genereaza-din-receptie") OrElse
           k_p.Contains("/ord/leaga-") OrElse k_p.Contains("/extrase/import") Then Return DataKinds.Plati
        Return DataKinds.None
    End Function

End Class
