Option Strict On
Imports System.Collections.Generic
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Domain

''' <summary>
''' Slice 0123-01 -- the ADECHIT routes the Rapoarte page reads (PYTHON/routes/adechit/). Kept out of <see cref="IApiClient"/> like the other
''' side interfaces: <see cref="ApiClient"/> implements it and the shell asks for it with a TryCast. The unit is the session's; the subunit
''' goes in the <c>X-Ade-Subunit</c> header of every call but the context one. Throws <see cref="ApiException"/> on a non-2xx, with the
''' server's Romanian «error» text and its «reason» code.
''' </summary>
Public Interface IAdechitApi

    ''' <summary>GET /api/adechit/context -- the subunits the user may work in and the operations the roles allow.</summary>
    Function GetAdechitContextAsync(ct As CancellationToken) As Task(Of AdechitContext)

    ''' <summary>GET /api/adechit/years -- the years that have months, newest first.</summary>
    Function GetAdechitYearsAsync(k_subunitId As Integer, ct As CancellationToken) As Task(Of List(Of Integer))

    ''' <summary>GET /api/adechit/rows/LunaD -- the months of a year.</summary>
    Function GetAdechitMonthsAsync(k_subunitId As Integer, k_year As Integer, ct As CancellationToken) As Task(Of List(Of AdechitMonth))

    ''' <summary>GET /api/adechit/catalog-data -- the groups that had attendance in a month.</summary>
    Function GetAdechitGroupsAsync(k_subunitId As Integer, k_monthId As Integer, ct As CancellationToken) As Task(Of List(Of AdechitGroup))

    ''' <summary>GET /api/adechit/child-search -- children of a month by name or CNP (three characters at least).</summary>
    Function SearchAdechitChildrenAsync(k_subunitId As Integer, k_monthId As Integer, k_query As String, ct As CancellationToken) As Task(Of AdechitChildSearch)

    ''' <summary>GET /api/adechit/reports/{kind}/generate -- the server builds the report and saves it for a few minutes.</summary>
    Function GenerateAdechitReportAsync(k_subunitId As Integer, k_kind As String, k_parameters As IReadOnlyDictionary(Of String, String),
                                        ct As CancellationToken) As Task(Of AdechitReportHandle)

    ''' <summary>GET /api/adechit/reports/export/{snapshot}/{pdf|xlsx} -- the saved report as a file (every column of the Excel).</summary>
    Function DownloadAdechitReportAsync(k_subunitId As Integer, k_snapshot As String, k_format As String, ct As CancellationToken) As Task(Of Byte())

    ''' <summary>DELETE /api/adechit/reports/export/{snapshot} -- the viewer was closed: the server removes the saved report.</summary>
    Function ReleaseAdechitReportAsync(k_subunitId As Integer, k_snapshot As String, ct As CancellationToken) As Task

End Interface
