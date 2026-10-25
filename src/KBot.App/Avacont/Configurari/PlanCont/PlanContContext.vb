Option Strict On
Imports System.Collections.Generic
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain

''' <summary>
''' What the «Plan de conturi» window and its two views share (slice AV01-04): the API, the working year, the nomenclator, the
''' subunits, the chart of the subunit on screen and its opening balances. The window loads it; the views read it and write the
''' balances they save back into <see cref="Solduri"/>, so the other view never shows a stale figure.
''' </summary>
Public NotInheritable Class PlanContContext

    Public ReadOnly Property Api As IConturiApi
    Public ReadOnly Property Gate As ReauthGate
    ''' <summary>The session: its sector/source (title bar) and the «Toate» flag decide the sector and source of an account.</summary>
    Public ReadOnly Property Session As SessionContext
    ''' <summary>The working year: the year of the opening balances shown and written (follows the title bar).</summary>
    Public ReadOnly Property An As Integer
        Get
            Return Session.An
        End Get
    End Property

    Public Property Nomenclator As NomenclatorConturi
    Public Property Subunitati As IReadOnlyList(Of SubunitateInfo) = Array.Empty(Of SubunitateInfo)()
    ''' <summary>The subunit on screen; Nothing = the unit has none yet.</summary>
    Public Property Subunitate As SubunitateInfo
    Public Property Plan As PlanConturiInfo = New PlanConturiInfo()
    ''' <summary>Show the accounts marked inactive too.</summary>
    Public Property ShowInactive As Boolean

    Public ReadOnly ContByCode As New Dictionary(Of Integer, ContNomenclator)()
    Public ReadOnly ClsfFNames As New Dictionary(Of String, String)(StringComparer.Ordinal)
    Public ReadOnly ClsfENames As New Dictionary(Of String, String)(StringComparer.Ordinal)
    ''' <summary>Balance rows of the accounts that have one (IdCont -> amounts); an account not in it is zero.</summary>
    Public ReadOnly Solduri As New Dictionary(Of Integer, SoldCont)()

    Public Sub New(k_api As IConturiApi, k_gate As ReauthGate, k_session As SessionContext)
        ArgumentNullException.ThrowIfNull(k_api)
        ArgumentNullException.ThrowIfNull(k_gate)
        ArgumentNullException.ThrowIfNull(k_session)
        _Api = k_api
        _Gate = k_gate
        _Session = k_session
    End Sub

    ''' <summary>
    ''' True = the title bar is on «Toate» (or its choice is not a known sector+source): the sector and source are typed on the
    ''' account itself. False = the account takes the title bar's choice and the fields only show it.
    ''' </summary>
    Public ReadOnly Property SsEditable As Boolean
        Get
            If Session.ToateSursele Then Return True
            Dim k_ss As String = Session.SectorSursa
            Return String.IsNullOrEmpty(k_ss) OrElse Nomenclator Is Nothing OrElse
                   Not Nomenclator.SurseSector.Any(Function(k_s) String.Equals(k_s.Cod, k_ss, StringComparison.Ordinal))
        End Get
    End Property

    ''' <summary>The sector+source a NEW account starts with: the title bar's, or the subunit's default on «Toate».</summary>
    Public Function DefaultSs() As String
        If Not SsEditable Then Return Session.SectorSursa
        Return If(Subunitate?.SursaSector, String.Empty)
    End Function

    ''' <summary>
    ''' The subunits that belong to the source chosen in the title bar: the ones whose sector+source is the session's. On «Toate»
    ''' (or with no source chosen) every subunit belongs.
    ''' </summary>
    Public Function SubunitatiForSource() As List(Of SubunitateInfo)
        If Session.ToateSursele OrElse String.IsNullOrEmpty(Session.SectorSursa) Then Return Subunitati.ToList()
        Return Subunitati.Where(Function(k_s) String.Equals(k_s.SursaSector, Session.SectorSursa, StringComparison.OrdinalIgnoreCase)).ToList()
    End Function

    ''' <summary>Keeps the nomenclator and the lookups the views use.</summary>
    Public Sub SetNomenclator(k_nomenclator As NomenclatorConturi)
        Nomenclator = k_nomenclator
        ContByCode.Clear()
        For Each k_c As ContNomenclator In k_nomenclator.Conturi
            ContByCode(k_c.Simbol) = k_c
        Next
        ClsfFNames.Clear()
        For Each k_c As ClsfCont In k_nomenclator.ClsfF
            ClsfFNames(k_c.Cod) = k_c.Denumire
        Next
        ClsfENames.Clear()
        For Each k_c As ClsfCont In k_nomenclator.ClsfE
            ClsfENames(k_c.Cod) = k_c.Denumire
        Next
    End Sub

    ''' <summary>Reads the chart and the balances of <see cref="Subunitate"/> (empty ones when there is none).</summary>
    Public Async Function ReloadPlanAsync() As Task
        Solduri.Clear()
        If Subunitate Is Nothing Then
            Plan = New PlanConturiInfo()
            Return
        End If
        Dim k_id As Integer = Subunitate.IdSubunitate
        Plan = Await Gate.RunAsync(Function() Api.GetPlanConturiAsync(k_id, CancellationToken.None)).ConfigureAwait(True)
        Dim k_rows As IReadOnlyList(Of SoldCont) = Await Gate.RunAsync(
            Function() Api.GetSolduriAsync(k_id, An, CancellationToken.None)).ConfigureAwait(True)
        For Each k_s As SoldCont In k_rows
            Solduri(k_s.IdCont) = k_s
        Next
    End Function

    ''' <summary>The accounts the views list: the active ones, plus the inactive ones when asked.</summary>
    Public Function VisibleAccounts() As List(Of ContPlan)
        Return Plan.Conturi.Where(Function(k_c) k_c.Activ OrElse ShowInactive).ToList()
    End Function

    ''' <summary>The balance row of an account, zero when it has none stored.</summary>
    Public Function SoldOf(k_idCont As Integer) As SoldCont
        Dim k_s As SoldCont = Nothing
        If Solduri.TryGetValue(k_idCont, k_s) Then Return k_s
        Return New SoldCont() With {.IdCont = k_idCont}
    End Function

End Class
