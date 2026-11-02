Option Strict On
Imports System.Globalization
Imports System.Threading
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain

' Slice 01EF-02 (reworked 02.11.2026, operator) -- THE SUBUNIT of the application. K-BOT itself does not use it, but the accounting operations and the other Avacont
' operations (notes, orders of payment, the templates in use, ...) belong to a subunit, so the choice is the application's, like the year and the source: the selector
' of the title bar stands BEFORE «Sursa/Sector» and is drawn at ANY time, on any page, as soon as two or more subunits have the source in force. The pages read
' <see cref="CurrentSubunitId"/> when they work and are told by <see cref="SubunitPicked"/> when the operator picks another one.
'
' The subunits of the unit database are read once per database (and again when the database changes); the choices follow the source of the title bar
' (every subunit with «Toate»). The pick moves the selector at once; a page that cannot follow (an edit it could not save) moves it back with <see cref="SetSubunitKey"/>.
Partial Public Class KbotForm

    Public Event SubunitPicked As Action(Of String) Implements IPlanContHost.SubunitPicked, Global.KBot.EFactura.ISubunitHost.SubunitPicked

    Private _subunitati As IReadOnlyList(Of SubunitateInfo)
    Private _subunitatiDb As String = String.Empty
    Private _subunitCurent As Integer?
    Private _subunitLoading As Boolean

    ''' <summary>The subunit in force (the one shown in the title bar, or the only one of the source); Nothing = the unit has no subunit for this source.</summary>
    Public ReadOnly Property CurrentSubunitId As Integer? Implements IPlanContHost.CurrentSubunitId, Global.KBot.EFactura.ISubunitHost.CurrentSubunitId
        Get
            Return _subunitCurent
        End Get
    End Property

    ''' <summary>The sector+source of the subunit in force (empty when there is none): what the windows that follow the subunit show when the title bar says «Toate».</summary>
    Friend ReadOnly Property CurrentSubunitSs As String
        Get
            Dim k_sub As SubunitateInfo = If(_subunitati Is Nothing OrElse Not _subunitCurent.HasValue, Nothing,
                _subunitati.FirstOrDefault(Function(k_s) k_s.IdSubunitate = _subunitCurent.Value))
            Return If(k_sub Is Nothing, String.Empty, k_sub.SursaSector)
        End Get
    End Property

    ''' <summary>Moves the selector to a key without raising <see cref="SubunitPicked"/> (a page that could not follow a pick puts it back).</summary>
    Public Sub SetSubunitKey(k_key As String) Implements IPlanContHost.SetSubunitKey, Global.KBot.EFactura.ISubunitHost.SetSubunitKey
        Dim k_id As Integer
        _subunitCurent = If(Integer.TryParse(k_key, NumberStyles.Integer, CultureInfo.InvariantCulture, k_id), CType(k_id, Integer?), Nothing)
        capBar.SetSelectorKey(KBotCaptionBar.SelectorSubunit, k_key)
    End Sub

    ' The operator picked a subunit in the title bar (called from CapBar_PeriodChanged).
    Private Sub OnSubunitPicked(k_key As String)
        SetSubunitKey(k_key)
        RaiseEvent SubunitPicked(k_key)
    End Sub

    ' The year or the source changed (and the first time after login): the choices follow the source; the subunits are read when not read yet for this database.
    ' UI boundary (event handler): logs and swallows.
    Private Sub SubunitSessionPeriodChanged(k_sender As Object, k_e As PeriodChangedEventArgs)
        Try
            ApplySubunitSelector()
            If _subunitati Is Nothing OrElse Not String.Equals(_subunitatiDb, _session.DbName, StringComparison.Ordinal) Then LoadSubunitsAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("KbotForm.SubunitSessionPeriodChanged", ex)
        End Try
    End Sub

    ' Fills the selector with the subunits of the source in force and keeps the one in force when it is still among them (else the default one, else the first).
    Private Sub ApplySubunitSelector()
        Dim k_all As IReadOnlyList(Of SubunitateInfo) = If(_subunitati, New List(Of SubunitateInfo)())
        Dim k_choices As List(Of SubunitateInfo) = If(_session.ToateSursele OrElse String.IsNullOrEmpty(_session.SectorSursa), k_all.ToList(),
            k_all.Where(Function(k_s) String.Equals(k_s.SursaSector, _session.SectorSursa, StringComparison.OrdinalIgnoreCase)).ToList())
        Dim k_keep As Integer? = _subunitCurent
        Dim k_pick As SubunitateInfo = k_choices.FirstOrDefault(Function(k_s) k_keep.HasValue AndAlso k_s.IdSubunitate = k_keep.Value)
        If k_pick Is Nothing Then k_pick = k_choices.FirstOrDefault(Function(k_s) k_s.Implicita)
        If k_pick Is Nothing Then k_pick = k_choices.FirstOrDefault()
        _subunitCurent = If(k_pick Is Nothing, Nothing, CType(k_pick.IdSubunitate, Integer?))
        capBar.SetSelectorItems(KBotCaptionBar.SelectorSubunit,
            k_choices.Select(Function(k_s) New KeyValuePair(Of String, String)(k_s.IdSubunitate.ToString(CultureInfo.InvariantCulture), k_s.Denumire)),
            If(k_pick Is Nothing, Nothing, k_pick.IdSubunitate.ToString(CultureInfo.InvariantCulture)))
        capBar.SetSelectorShown(KBotCaptionBar.SelectorSubunit, k_choices.Count >= 2)
    End Sub

    ' UI boundary (async Sub): logs and swallows; without the list the selector simply stays hidden.
    Private Async Sub LoadSubunitsAsync()
        Try
            If _subunitLoading Then Return
            Dim k_api As IConturiApi = TryCast(_apiClient, IConturiApi)
            If k_api Is Nothing OrElse String.IsNullOrWhiteSpace(_session.DbName) OrElse String.IsNullOrWhiteSpace(_session.Token) Then Return
            _subunitLoading = True
            Dim k_db As String = _session.DbName
            Dim k_gate As New ReauthGate(Function(k_action) WithReauth(Of Object)(k_action))
            Dim k_list As IReadOnlyList(Of SubunitateInfo) = Await k_gate.RunAsync(Function() k_api.GetSubunitatiAsync(CancellationToken.None)).ConfigureAwait(True)
            If Not String.Equals(k_db, _session.DbName, StringComparison.Ordinal) Then Return   ' the unit changed meanwhile: the next period event reads again
            _subunitati = k_list
            _subunitatiDb = k_db
            ApplySubunitSelector()
        Catch ex As Exception
            GlobalErrorLog.Write("KbotForm.LoadSubunitsAsync", ex)
        Finally
            _subunitLoading = False
        End Try
    End Sub

End Class
