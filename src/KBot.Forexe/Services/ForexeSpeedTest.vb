Imports System.Globalization
Imports KBot.Common
Imports Microsoft.Playwright

''' <summary>
''' Slice 0091 (operator, 29.09.2026): measures the download speed through fast.com, for the
''' «Setări → FOREXE» page. Below <see cref="AppSettings.SlowInternetMbps"/> the page offers
''' the double read of tables.
'''
''' <para><b>Why a hidden browser and not HTTP.</b> fast.com has no public API: its page fetches
''' a token from its own script and then downloads from Netflix servers. Reading the figure the
''' page itself shows (<c>#speed-value</c>, marked <c>succeeded</c> when the test ends) keeps us
''' on their measurement, with the same Chromium the robot already ships. If fast.com changes
''' its page, the wait times out and the operator is told -- nothing is guessed.</para>
''' </summary>
Public NotInheritable Class ForexeSpeedTest

    Public Const Url As String = "https://fast.com"

    ''' <summary>The download test usually settles in 10-30 s; past this it is not coming.</summary>
    Private Const ResultTimeoutMs As Integer = 90000

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Opens fast.com in a headless Chromium, waits for its download figure and returns it in
    ''' Mb/s. Boundary (Playwright, network): logs and rethrows.
    ''' </summary>
    Public Shared Async Function MeasureMbpsAsync() As Task(Of Double)
        Try
            Using pw As IPlaywright = Await Playwright.CreateAsync()
                Dim browser As IBrowser = Await pw.Chromium.LaunchAsync(New BrowserTypeLaunchOptions() With {
                    .Headless = True,
                    .Channel = "chromium"})
                ' VB cannot Await in Finally: the failure is held, the browser closed, then rethrown.
                Dim valoare As String = Nothing
                Dim unitate As String = Nothing
                Dim esec As Exception = Nothing
                Try
                    Dim page As IPage = Await browser.NewPageAsync()
                    Await page.GotoAsync(Url, New PageGotoOptions() With {.Timeout = 30000})
                    Await page.WaitForSelectorAsync("#speed-value.succeeded",
                        New PageWaitForSelectorOptions() With {.Timeout = ResultTimeoutMs})
                    valoare = Await page.InnerTextAsync("#speed-value")
                    unitate = Await page.InnerTextAsync("#speed-units")
                Catch ex As Exception
                    esec = ex
                End Try
                Await browser.CloseAsync()
                If esec IsNot Nothing Then Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(esec).Throw()
                Return ToMbps(valoare, unitate)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("ForexeSpeedTest.MeasureMbpsAsync", ex)
            Throw
        End Try
    End Function

    ''' <summary>fast.com's figure and unit («Kbps» / «Mbps» / «Gbps») as Mb/s.</summary>
    Public Shared Function ToMbps(valoare As String, unitate As String) As Double
        Dim v As Double
        If Not Double.TryParse((If(valoare, String.Empty)).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, v) Then
            Throw New ArgumentException($"fast.com a afișat o valoare necunoscută: «{valoare}».", NameOf(valoare))
        End If
        Select Case (If(unitate, String.Empty)).Trim().ToLowerInvariant()
            Case "kbps" : Return v / 1000.0
            Case "mbps" : Return v
            Case "gbps" : Return v * 1000.0
            Case Else
                Throw New ArgumentException($"fast.com a afișat o unitate necunoscută: «{unitate}».", NameOf(unitate))
        End Select
    End Function

End Class
