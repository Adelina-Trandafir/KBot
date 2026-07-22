Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports Xunit
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.App

' Headless behaviour tests for SumarView (slice 0011). They cover the three rules that
' no server test can reach: a null/blank context must NOT hit the network, a response
' must land in the grid, and a STALE response (operator clicked on to another node
' while the first was in flight) must be discarded instead of overwriting the newer one.
'
' Everything runs on a dedicated STA thread — creating a UserControl installs a
' WindowsFormsSynchronizationContext, so the Async Sub continuations are Post()-ed to
' the control's queue and need Application.DoEvents() to be pumped. Same pattern as
' HarnessTestsRunTest.
Public Class SumarViewTests

    ' Fake IApiClient: records the codes it was asked for and hands back a Task the
    ' TEST completes, so response ORDER is fully under the test's control.
    Private NotInheritable Class FakeApiClient
        Implements IApiClient

        Public ReadOnly RequestedCods As New List(Of String)()
        ' Cod -> sursa de completare a raspunsului (completata manual de test).
        Public ReadOnly Pending As New Dictionary(Of String, TaskCompletionSource(Of SumarInfo))(StringComparer.Ordinal)

        Public Function GetSumarAsync(cod As String, ct As CancellationToken) _
            As Task(Of SumarInfo) Implements IApiClient.GetSumarAsync
            RequestedCods.Add(cod)
            Dim tcs As New TaskCompletionSource(Of SumarInfo)()
            Pending(cod) = tcs
            Return tcs.Task
        End Function

        Public Sub Complete(cod As String, data As SumarInfo)
            Pending(cod).SetResult(data)
        End Sub

        ' --- restul contractului: nefolosit aici ---
        Public Function UpsertAngajamenteAsync(dbName As String, rows As IReadOnlyList(Of Angajament),
                                               ct As CancellationToken) As Task(Of String) _
            Implements IApiClient.UpsertAngajamenteAsync
            Throw New NotSupportedException()
        End Function

        Public Function GetAngajamenteAsync(dbName As String, idUnitate As Integer, doarAnulate As Boolean,
                                            ct As CancellationToken) As Task(Of IReadOnlyList(Of Angajament)) _
            Implements IApiClient.GetAngajamenteAsync
            Throw New NotSupportedException()
        End Function

        Public Function GetTreeAsync(an As Integer, ss As String, includeHidden As Boolean,
                                     ct As CancellationToken) As Task(Of IReadOnlyList(Of AngajamentTreeInfo)) _
            Implements IApiClient.GetTreeAsync
            Throw New NotSupportedException()
        End Function

        Public Function GetRezervariAsync(cod As String, ct As CancellationToken) As Task(Of RezervariInfo) _
            Implements IApiClient.GetRezervariAsync
            Throw New NotSupportedException()
        End Function

        Public Function GetReceptiiAsync(cod As String, ct As CancellationToken) As Task(Of ReceptiiInfo) _
            Implements IApiClient.GetReceptiiAsync
            Throw New NotSupportedException()
        End Function

        Public Function ProcessExcelAsync(job As ExcelJob, ct As CancellationToken) As Task(Of String) _
            Implements IApiClient.ProcessExcelAsync
            Throw New NotSupportedException()
        End Function

        Public Function GetAsync(Of T)(relativeUrl As String, ct As CancellationToken) As Task(Of T) _
            Implements IApiClient.GetAsync
            Throw New NotSupportedException()
        End Function

        Public Function PostAsync(Of TRequest, TResponse)(relativeUrl As String, payload As TRequest,
                                                          ct As CancellationToken) As Task(Of TResponse) _
            Implements IApiClient.PostAsync
            Throw New NotSupportedException()
        End Function
    End Class

    ' Plasa 401 din MainForm, redusa la identitate: testul nu verifica re-login-ul.
    Private Shared Function PassThrough() As Func(Of Func(Of Task(Of SumarInfo)), Task(Of SumarInfo))
        Return Function(op) op()
    End Function

    Private Shared Function Info(cod As String, ParamArray indicatori As String()) As SumarInfo
        Dim data As New SumarInfo() With {
            .Header = New SumarHeader() With {.CodAngajament = cod, .Descriere = "D-" & cod}
        }
        For Each ind As String In indicatori
            data.Rows.Add(New SumarRow() With {.CodIndicator = ind, .Clsf = "65.02"})
        Next
        Return data
    End Function

    Private Shared Function Context(cod As String) As AngajamentTreeInfo
        Return New AngajamentTreeInfo() With {.CodAngajament = cod, .NodeKey = cod}
    End Function

    ' Grila e Friend in KBot.App, deci se ajunge la ea prin colectia publica Controls.
    Private Shared Function GridOf(view As SumarView) As KBotDataView
        For Each c As Control In view.Controls
            Dim g = TryCast(c, KBotDataView)
            If g IsNot Nothing Then Return g
        Next
        Throw New InvalidOperationException("SumarView nu conține un KBotDataView.")
    End Function

    Private Shared Function IndicatorsIn(g As KBotDataView) As List(Of String)
        Dim result As New List(Of String)()
        For Each r As KBotDataRow In g.Rows
            result.Add(CStr(r("cod_indicator")))
        Next
        Return result
    End Function

    ' Ruleaza corpul testului pe un fir STA si propaga orice esec inapoi.
    Private Shared Sub RunSta(body As Action)
        Dim failure As Exception = Nothing
        Dim t As New Thread(Sub()
                                Try
                                    body()
                                Catch ex As Exception
                                    failure = ex
                                End Try
                            End Sub)
        t.SetApartmentState(ApartmentState.STA)
        t.Start()
        t.Join()
        If failure IsNot Nothing Then Throw failure
    End Sub

    <Fact>
    Public Sub SetContext_Nothing_MakesNoApiCall_AndClearsGrid()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New SumarView(api, PassThrough())
                       Dim g = GridOf(view)

                       ' Intai umplem grila printr-o selectie reala...
                       view.SetContext(Context("A100"))
                       api.Complete("A100", Info("A100", "IND-A", "IND-B"))
                       Application.DoEvents()
                       Assert.Equal(2, g.RowCount)

                       ' ...apoi deselectam: fara apel de retea, grila golita.
                       view.SetContext(Nothing)

                       Assert.Single(api.RequestedCods)      ' tot doar cererea initiala
                       Assert.Equal(0, g.RowCount)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub SetContext_BlankCod_MakesNoApiCall()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New SumarView(api, PassThrough())
                       view.SetContext(New AngajamentTreeInfo() With {.CodAngajament = "   "})
                       Assert.Empty(api.RequestedCods)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub SetContext_RequestsTheSelectedCod_AndFillsGrid()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New SumarView(api, PassThrough())
                       Dim g = GridOf(view)

                       view.SetContext(Context("A100"))
                       Assert.Equal("A100", Assert.Single(api.RequestedCods))

                       api.Complete("A100", Info("A100", "IND-A", "IND-B"))
                       Application.DoEvents()

                       Assert.Equal(2, g.RowCount)
                       Assert.Equal(New String() {"IND-A", "IND-B"}, IndicatorsIn(g))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub StaleResponse_ForSupersededCod_IsDiscarded()
        ' Operatorul trece rapid prin arbore: A100 e cerut, apoi B200 inainte ca A100 sa
        ' raspunda. Raspunsul lui A100 vine ULTIMUL si nu are voie sa suprascrie B200.
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New SumarView(api, PassThrough())
                       Dim g = GridOf(view)

                       view.SetContext(Context("A100"))
                       view.SetContext(Context("B200"))
                       Assert.Equal(New String() {"A100", "B200"}, api.RequestedCods.ToArray())

                       ' Raspunsul NOU ajunge primul si umple grila.
                       api.Complete("B200", Info("B200", "IND-B1"))
                       Application.DoEvents()
                       Assert.Equal(New String() {"IND-B1"}, IndicatorsIn(g))

                       ' Raspunsul VECHI ajunge dupa — trebuie ignorat complet.
                       api.Complete("A100", Info("A100", "IND-A1", "IND-A2"))
                       Application.DoEvents()

                       Assert.Equal(New String() {"IND-B1"}, IndicatorsIn(g))
                   End Using
               End Sub)
    End Sub

End Class
