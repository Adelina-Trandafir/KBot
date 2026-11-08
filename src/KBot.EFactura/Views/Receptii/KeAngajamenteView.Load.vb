Option Strict On
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain

' SLICE-KE21-01: reading the data of the angajamente (receptions, indicators) and the list the empty row offers.
Partial Public Class KeAngajamenteView

    ''' <summary>The angajamente the empty row can offer, read once. UI boundary (async Sub): logs and swallows; a failure becomes a notice.</summary>
    Public Async Sub BeginLoadAll()
        Try
            If _api Is Nothing Then Return
            Await LoadAllAsync().ConfigureAwait(True)
        Catch ex As OperationCanceledException
            ' The page was closed.
        Catch ex As ApiException
            _allAngLoaded = False
            GlobalErrorLog.Write("KeAngajamenteView.BeginLoadAll", ex)
            _host.ShowNotice(ex.Message, NoticeKind.Error)
        Catch ex As Exception
            _allAngLoaded = False
            GlobalErrorLog.Write("KeAngajamenteView.BeginLoadAll", ex)
            _host.ShowNotice("Angajamentele nu au putut fi citite. Detalii în jurnalul de erori.", NoticeKind.Error)
        End Try
    End Sub

    ''' <summary>The same, for a caller that waits and handles the failure itself.</summary>
    Public Async Function LoadAllAsync() As Task
        If _allAngLoaded Then Return
        _allAngLoaded = True
        _allAng = Await _gate.RunAsync(Function() _api.GetKeAngajamenteAsync(Nothing, _cts.Token)).ConfigureAwait(True)
        If IsDisposed Then Return
        FillChoices()
    End Function

    ' UI boundary (async Sub): logs and swallows; a failure becomes a notice.
    Private Async Sub BeginUseAngajament(k_cod As String)
        Try
            Await UseAngajamentAsync(k_cod).ConfigureAwait(True)
        Catch ex As OperationCanceledException
            ' The window was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("KeAngajamenteView.BeginUseAngajament", ex)
            _host.ShowNotice(ex.Message, NoticeKind.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("KeAngajamenteView.BeginUseAngajament", ex)
            _host.ShowNotice("Recepțiile angajamentului nu au putut fi citite. Detalii în jurnalul de erori.", NoticeKind.Error)
        End Try
    End Sub

    ' Reads the receptions and the indicators of the angajament (once; later they come from memory) and tells the host.
    Private Async Function UseAngajamentAsync(k_cod As String) As Task
        If _api Is Nothing OrElse String.IsNullOrWhiteSpace(k_cod) Then Return
        ' Already in use only when the data on screen ARE this angajament's (a code set by the empty row before its data were read must not count).
        Dim k_cached As EFacturaKeReceptiiAngajament = Nothing
        If String.Equals(k_cod, _angCod, StringComparison.OrdinalIgnoreCase) AndAlso _data IsNot Nothing AndAlso
           _angCache.TryGetValue(k_cod, k_cached) AndAlso ReferenceEquals(k_cached, _data) Then Return
        _angCod = k_cod
        _data = Nothing
        Dim k_mySeq As Integer = Interlocked.Increment(_angSeq)
        RaiseEvent AngajamentChanging(Me, EventArgs.Empty)
        Dim k_data As EFacturaKeReceptiiAngajament = Nothing
        If Not _angCache.TryGetValue(k_cod, k_data) Then
            Dim k_codNow As String = k_cod
            k_data = Await _gate.RunAsync(Function() _api.GetKeReceptiiAsync(k_codNow, _cts.Token)).ConfigureAwait(True)
            If IsDisposed OrElse k_mySeq <> _angSeq Then Return
            _angCache(k_cod) = k_data
        End If
        If IsDisposed OrElse k_mySeq <> _angSeq Then Return
        _data = k_data
        RaiseEvent AngajamentReady(Me, EventArgs.Empty)
        If k_data.Indicatori.Count = 0 Then
            _host.ShowNotice("Angajamentul nu are indicatori: o recepție nu poate fi făcută până nu are rezervări (documentul de fundamentare).", NoticeKind.Warning)
        End If
    End Function

    ' UI boundary (async Sub): reads the indicators of the other angajamente of the table, so their classifications can be chosen too.
    Private Async Sub BeginLoadOthers()
        Try
            If _api Is Nothing Then Return
            Dim k_mySeq As Integer = Interlocked.Increment(_othersSeq)
            Dim k_loaded As Boolean = False
            For Each k_a As AngItem In _items.ToList()
                If _angCache.ContainsKey(k_a.Cod) Then Continue For
                Dim k_cod As String = k_a.Cod
                Dim k_data As EFacturaKeReceptiiAngajament = Await _gate.RunAsync(Function() _api.GetKeReceptiiAsync(k_cod, _cts.Token)).ConfigureAwait(True)
                If IsDisposed OrElse k_mySeq <> _othersSeq Then Return
                _angCache(k_cod) = k_data
                k_loaded = True
            Next
            If k_loaded Then RaiseEvent OthersLoaded(Me, EventArgs.Empty)
        Catch ex As OperationCanceledException
            ' The page was closed.
        Catch ex As Exception
            GlobalErrorLog.Write("KeAngajamenteView.BeginLoadOthers", ex)
        End Try
    End Sub

End Class
