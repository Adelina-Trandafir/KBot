Option Strict On
Imports KBot.Common
Imports KBot.Controls

''' <summary>
''' What one search panel searches in (slice 0000-20): the «?» popup has one, the help window has
''' one. It turns <see cref="HelpHit"/>s into list rows, builds the popup's rows for an empty box
''' (the topic of the screen and the tours, given by <see cref="HelpService"/>) and carries out
''' what the operator does with a row.
''' </summary>
Friend NotInheritable Class HelpSearchSession
    Implements IKBotHelpSearchSource

    ''' <summary>Where the panel is: the «?» popup.</summary>
    Public Const WherePopup As String = "popup"

    ''' <summary>Where the panel is: the help window.</summary>
    Public Const WhereWindow As String = "fereastra"

    ''' <summary>
    ''' Slice 0000-21: a question the operator paused on this long counts as asked; editing it
    ''' afterwards starts a new one. Typing faster than this is still the same question.
    ''' </summary>
    Private Shared ReadOnly SettleTime As TimeSpan = TimeSpan.FromSeconds(3)

    Private ReadOnly _service As HelpService
    Private ReadOnly _home As Func(Of IList(Of KBotHelpRow))
    Private _query As String = String.Empty
    Private _hits As New List(Of HelpHit)()
    Private _question As HelpQuestion
    Private _lastSearchUtc As DateTime = DateTime.MinValue

    ''' <param name="where"><see cref="WherePopup"/> or <see cref="WhereWindow"/>.</param>
    ''' <param name="home">The rows for an empty box; Nothing = none (the help window shows its contents instead).</param>
    Public Sub New(service As HelpService, where As String, home As Func(Of IList(Of KBotHelpRow)))
        If service Is Nothing Then Throw New ArgumentNullException(NameOf(service))
        If where <> WherePopup AndAlso where <> WhereWindow Then Throw New ArgumentException("Unknown panel place '" & where & "'.", NameOf(where))
        _service = service
        Me.Where = where
        _home = home
    End Sub

    Public ReadOnly Property Where As String

    ''' <summary>The last question searched here (trimmed), and the hits it showed.</summary>
    Public ReadOnly Property Query As String
        Get
            Return _query
        End Get
    End Property

    Public ReadOnly Property Hits As IReadOnlyList(Of HelpHit)
        Get
            Return _hits
        End Get
    End Property

    Public Function HomeRows() As IList(Of KBotHelpRow) Implements IKBotHelpSearchSource.HomeRows
        Try
            Return If(_home Is Nothing, New List(Of KBotHelpRow)(), _home())
        Catch ex As Exception
            GlobalErrorLog.Write("HelpSearchSession.HomeRows", ex)
            Throw
        End Try
    End Function

    Public Function Search(query As String) As IList(Of KBotHelpRow) Implements IKBotHelpSearchSource.Search
        Try
            _query = String.Join(" ", If(query, String.Empty).Split(CType(Nothing, Char()), StringSplitOptions.RemoveEmptyEntries))
            _hits = _service.Library.Search(_query, _service.VisibleParts())
            TrackQuestion()
            Dim rows As New List(Of KBotHelpRow)()
            ' Slice 000T: a tutorial that answers the question comes first (it does the thing step by step).
            For Each flow As TutorialFlow In _service.Library.SearchTutorials(_query, _service.VisibleParts())
                rows.Add(HelpService.TutorialRow(flow))
            Next
            For Each h As HelpHit In _hits
                rows.Add(RowFor(h))
            Next
            If rows.Count = 0 Then
                rows.Add(New KBotHelpRow(KBotHelpRowKind.Note,
                    "N-am găsit nimic potrivit. Încearcă alte cuvinte (de exemplu numele butonului sau al ferestrei) sau deschide ajutorul complet."))
            End If
            Return rows
        Catch ex As Exception
            GlobalErrorLog.Write("HelpSearchSession.Search", ex)
            Throw
        End Try
    End Function

    ' ── The question (slice 0000-21) ──────────────────────────────────────────────

    ''' <summary>
    ''' Follows the text in the box: still typing = the same question; a changed text after a
    ''' click / rating, or after a pause of <see cref="SettleTime"/>, = a new question (the old one
    ''' is kept as asked, with no click if it had none).
    ''' </summary>
    Private Sub TrackQuestion()
        Dim now As DateTime = DateTime.UtcNow
        If _query.Length < 2 Then
            _lastSearchUtc = now
            Return
        End If
        If _question IsNot Nothing AndAlso Not String.Equals(_question.Text, _query, StringComparison.Ordinal) Then
            If _question.Saved Then
                _question = Nothing
            ElseIf now - _lastSearchUtc >= SettleTime Then
                _service.SaveQuestion(_question)
                _question = Nothing
            End If
        End If
        If _question Is Nothing Then _question = New HelpQuestion(Where)
        If Not _question.Saved Then
            _question.Text = _query
            _question.Hits = _hits.Take(HelpQuestion.MaxHits).Select(Function(h) h.Key).ToList()
        End If
        _lastSearchUtc = now
    End Sub

    ''' <summary>The question now in the box, or Nothing.</summary>
    Friend ReadOnly Property Question As HelpQuestion
        Get
            Return _question
        End Get
    End Property

    ''' <summary>
    ''' The help window carries on the popup's question: same id, so a rating given in the window
    ''' goes to the row the popup started.
    ''' </summary>
    Friend Sub Adopt(from As HelpSearchSession)
        If from Is Nothing OrElse from._question Is Nothing Then Return
        _question = from._question
        _lastSearchUtc = DateTime.UtcNow
    End Sub

    Private Function RowFor(h As HelpHit) As KBotHelpRow
        Dim row As New KBotHelpRow(KBotHelpRowKind.Hit, h.Title) With {
            .Subtitle = h.SectionTitle,
            .Snippet = h.Snippet,
            .Tag = h,
            .ToolTipText = "Deschide pagina de ajutor chiar la această secțiune."}
        If h.OpenTarget.Length > 0 Then row.OpenText = _service.OpenButtonText(h.OpenTarget)
        If h.TourId.Length > 0 Then row.TourText = "Tur ghidat"
        Return row
    End Function

    Public Function Invoke(row As KBotHelpRow, action As KBotHelpRowAction) As Boolean Implements IKBotHelpSearchSource.Invoke
        Try
            Dim hit As HelpHit = TryCast(row.Tag, HelpHit)
            If hit IsNot Nothing Then
                ' Slice 0000-21: which hit was used, and with which button. Saved before the action,
                ' so a screen or a tour that takes over cannot lose it.
                If _question IsNot Nothing Then
                    _question.Opened = hit.Key
                    If action = KBotHelpRowAction.Open Then _question.Action = "open"
                    If action = KBotHelpRowAction.Tour Then _question.Action = "tour"
                    _service.SaveQuestion(_question)
                End If
                Select Case action
                    Case KBotHelpRowAction.Open
                        _service.OpenScreen(hit.OpenTarget, hit.TopicId, _service.MainWindow())
                    Case KBotHelpRowAction.Tour
                        _service.StartTour(hit.TourId)
                    Case Else
                        _service.ShowHit(hit, Me)
                End Select
                Return True
            End If
            If TypeOf row.Tag Is HelpService.WhatsNewMarker Then
                UpdateOfferForm.ShowRecent(_service.MainWindow())
                Return True
            End If
            Dim topic As HelpTopic = TryCast(row.Tag, HelpTopic)
            If topic IsNot Nothing Then
                _service.ShowTopic(topic.Id)
                Return True
            End If
            Dim tour As HelpTour = TryCast(row.Tag, HelpTour)
            If tour IsNot Nothing Then
                _service.StartTour(tour.Id)
                Return True
            End If
            ' Slice 000T: an interactive tutorial.
            Dim flow As TutorialFlow = TryCast(row.Tag, TutorialFlow)
            If flow IsNot Nothing Then
                _service.StartTutorial(flow.Id)
                Return True
            End If
            Throw New ArgumentException("A help row without a known tag: " & row.Title, NameOf(row))
        Catch ex As Exception
            GlobalErrorLog.Write("HelpSearchSession.Invoke", ex)
            Throw
        End Try
    End Function

    ''' <summary>One rating per question; a new one replaces it (the same row on the server).</summary>
    Public Sub Rate(stars As Integer) Implements IKBotHelpSearchSource.Rate
        Try
            If _question Is Nothing Then Return
            _question.Rating = Math.Max(0, Math.Min(5, stars))
            _service.SaveQuestion(_question)
        Catch ex As Exception
            GlobalErrorLog.Write("HelpSearchSession.Rate", ex)
            Throw
        End Try
    End Sub

    Public Function CurrentRating() As Integer Implements IKBotHelpSearchSource.CurrentRating
        Return If(_question Is Nothing, 0, _question.Rating)
    End Function

    ''' <summary>The box was emptied or the panel closes: a question not yet saved is saved as asked.</summary>
    Public Sub EndQuestion() Implements IKBotHelpSearchSource.EndQuestion
        Try
            If _question IsNot Nothing AndAlso Not _question.Saved Then _service.SaveQuestion(_question)
            _question = Nothing
        Catch ex As Exception
            GlobalErrorLog.Write("HelpSearchSession.EndQuestion", ex)
            Throw
        End Try
    End Sub

End Class
