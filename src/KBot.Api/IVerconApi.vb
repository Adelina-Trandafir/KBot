Option Strict On
Imports System.Collections.Generic
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Domain

''' <summary>
''' Slice 0123-02 -- the VERCON routes the Rapoarte page reads (PYTHON/routes/vercon/, the calls opened to the bearer token by kbot_bridge.py).
''' Kept out of <see cref="IApiClient"/> like the other side interfaces: <see cref="ApiClient"/> implements it and the shell asks for it with a
''' TryCast. The unit is the session's (the <c>X-Vercon-Unit</c> header); the subunit / gestiune / year go in the query of every call. Throws
''' <see cref="ApiException"/> on a non-2xx, with the server's Romanian «error» text and its «reason» code.
''' </summary>
Public Interface IVerconApi

    ''' <summary>GET /api/vercon/context -- the options of each level below the parts of <paramref name="k_selection"/> that are chosen (the server picks a single remaining option).</summary>
    Function GetVerconContextAsync(k_selection As VerconSelection, ct As CancellationToken) As Task(Of VerconContext)

    ''' <summary>GET /api/vercon/reports/catalog -- the reports of the gestiune's type and the periods it holds data in.</summary>
    Function GetVerconCatalogAsync(k_selection As VerconSelection, ct As CancellationToken) As Task(Of VerconCatalog)

    ''' <summary>GET /api/vercon/reports/{name}/selection -- one branch of the selection tree (<c>branch</c> in the parameters, <c>root</c> for the first).</summary>
    Function GetVerconSelectionAsync(k_selection As VerconSelection, k_report As String, k_parameters As IReadOnlyDictionary(Of String, String),
                                     ct As CancellationToken) As Task(Of VerconBranch)

    ''' <summary>GET /api/vercon/reports/{name}/generate -- the server builds the report and saves it for a few minutes.</summary>
    Function GenerateVerconReportAsync(k_selection As VerconSelection, k_report As String, k_parameters As IReadOnlyDictionary(Of String, String),
                                       ct As CancellationToken) As Task(Of VerconReportHandle)

    ''' <summary>GET /api/vercon/reports/export/{snapshot}/{pdf|xlsx} -- the saved report as a file.</summary>
    Function DownloadVerconReportAsync(k_selection As VerconSelection, k_snapshot As String, k_format As String, ct As CancellationToken) As Task(Of Byte())

    ''' <summary>DELETE /api/vercon/reports/export/{snapshot} -- the viewer was closed: the server removes the saved report.</summary>
    Function ReleaseVerconReportAsync(k_selection As VerconSelection, k_snapshot As String, ct As CancellationToken) As Task

End Interface
