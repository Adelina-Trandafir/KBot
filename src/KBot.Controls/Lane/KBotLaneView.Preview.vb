Option Strict On
Imports System.ComponentModel

''' <summary>
''' WHAT THE CONTROL SHOWS INSIDE VISUAL STUDIO when it has no lanes yet.
'''
''' <para><b>The defect this fixes.</b> Every host fills <c>Lanes</c> at runtime, from data —
''' nothing is authored in the collection dialog. So on the design surface the control was an
''' empty rectangle with a band on top: a placeholder that told the person laying out the form
''' neither how tall a lane is, nor how much room the captions need, nor where the axis ends up,
''' nor whether the frame and the band read correctly against the form behind them. All of those
''' are decisions made ON the design surface, and none of them could be made there.</para>
'''
''' <para><b>The rule that keeps it honest: the preview is never DATA.</b> It lives in its own
''' collection, whose <c>Owner</c> is deliberately left Nothing, so nothing it does can reach the
''' real one, invalidate through it, or be picked up by the designer's serializer. The host's
''' <c>.Designer.vb</c> gains not one line from it. The moment the host adds a real lane — in the
''' collection dialog or in code — the preview steps aside and never comes back for that
''' instance.</para>
'''
''' <para><b>And it never runs.</b> <see cref="PreviewActive"/> is False unless
''' <c>KBotDesignTime.IsDesignTime</c> is True, so in the running application this file costs one
''' boolean per layout pass. Hit-testing, dragging and the labels all read the same accessor the
''' painter reads, so what the pointer finds is always what the eye is looking at — but at design
''' time the mouse handlers return before they get that far anyway.</para>
'''
''' <para>Switched off, for a host that would rather see the empty rectangle, with
''' <see cref="DesignPreviewVisible"/> = False. It is a design-time-only property and carries
''' <c>DesignerSerializationVisibility.Hidden</c>: a preference about Visual Studio has no
''' business being written into the form's generated code.</para>
''' </summary>
Partial Public NotInheritable Class KBotLaneView

    ''' <summary>How many days the sample spans. Six weeks, so the axis has something to say.</summary>
    Private Const PreviewSpanDays As Integer = 42

    ''' <summary>The caption the band borrows while the sample is up.</summary>
    Private Const PreviewCaptionText As String = "KBotLaneView"

    ' Owner is NEVER set on either of these. See the note on the class: an owner is exactly how a
    ' collection reaches back into the control, and the sample must not be able to.
    Private ReadOnly _previewLanes As New KBotLaneCollection()
    Private ReadOnly _previewGuides As New KBotChartGuideCollection()

    Private _designPreviewVisible As Boolean = True
    Private _previewActive As Boolean
    Private _previewBuilt As Boolean

    ''' <summary>
    ''' False =&gt; the control stays empty on the design surface, as it was before.
    ''' Has no effect at runtime, and is never serialized into the host form.
    ''' </summary>
    <Category("K-BOT Lane Appearance")>
    <Description("Design time only: show a sample of lanes and markers while Lanes is empty, so the surface can be laid out. Never affects the running application.")>
    <DefaultValue(True)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property DesignPreviewVisible As Boolean
        Get
            Return _designPreviewVisible
        End Get
        Set(value As Boolean)
            If _designPreviewVisible = value Then Return
            _designPreviewVisible = value
            InvalidateLaneLayout()
        End Set
    End Property

    ''' <summary>True while the surface on screen is the sample rather than the host's lanes.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property PreviewActive As Boolean
        Get
            Return _previewActive
        End Get
    End Property

    ''' <summary>
    ''' THE ONE ACCESSOR the painter, the layout, the hit-test and the drag all read. Never
    ''' <c>_lanes</c> directly — two readers disagreeing about which collection is on screen is
    ''' precisely how a pointer ends up naming a lane nobody can see.
    ''' </summary>
    Friend ReadOnly Property VisibleLanes As KBotLaneCollection
        Get
            If _previewActive Then Return _previewLanes
            Return _lanes
        End Get
    End Property

    ''' <summary>The guide twin of <see cref="VisibleLanes"/>.</summary>
    Friend ReadOnly Property VisibleGuides As KBotChartGuideCollection
        Get
            If _previewActive Then Return _previewGuides
            Return _guides
        End Get
    End Property

    ''' <summary>
    ''' Decides, once per layout pass, whether the sample is on. Cheap on purpose: at runtime it
    ''' is one call to <c>KBotDesignTime.IsDesignTime</c>, which answers from a cached field.
    ''' </summary>
    Friend Sub RefreshPreviewState()
        Dim wanted As Boolean = _designPreviewVisible AndAlso
                                _lanes.Count = 0 AndAlso
                                KBotDesignTime.IsDesignTime(Me)
        If wanted AndAlso Not _previewBuilt Then BuildPreview()
        _previewActive = wanted
    End Sub

    ''' <summary>
    ''' The caption the band shows while the sample is up — so a control dropped on a form says
    ''' what it is, instead of showing an unexplained empty strip.
    ''' </summary>
    Private Function PreviewCaption() As String
        If Not _previewActive Then Return String.Empty
        Return PreviewCaptionText
    End Function

    ''' <summary>
    ''' Builds the sample: five lanes over six weeks, exercising every shape the control can draw
    ''' so that all of them can be judged on the design surface at once.
    ''' </summary>
    ''' <remarks>
    ''' <para>Deliberately chosen, not random. There is one lane of each kind the reception editor
    ''' actually produces — a chain that closes, a chain that does not, a locked one, one that was
    ''' deleted — plus the "not placed yet" lane under a separator, which is the one arrangement
    ''' the whole surface exists for. A sample that showed five identical rows of discs would look
    ''' tidy and answer nothing.</para>
    ''' <para>Colours are left <c>Color.Empty</c> so the sample takes the theme's automatic set,
    ''' the same one a host gets when it says nothing (C1). Dates are counted BACK from today, so
    ''' the sample never goes stale and the axis always reads as a recent stretch of time.</para>
    ''' </remarks>
    Private Sub BuildPreview()
        _previewBuilt = True
        _previewLanes.Clear()
        _previewGuides.Clear()

        Dim first As Date = Date.Today.AddDays(-PreviewSpanDays)

        ' 1 — an ordinary chain that closes where it should.
        Dim a As New KBotLane("preview-1", "Recepția 101")
        a.EndMark = KBotLaneEndMark.Ok
        a.AddMarker(first.AddDays(2), "1.200,00")
        a.AddMarker(first.AddDays(11), "1.200,00")
        a.AddMarker(first.AddDays(19), "1.450,00")
        _previewLanes.Add(a)

        ' 2 — a chain that does NOT close: the warning end mark, and a save that recorded nothing.
        Dim b As New KBotLane("preview-2", "Recepția 102")
        b.EndMark = KBotLaneEndMark.Warning
        b.AddMarker(first.AddDays(5), "800,00")
        b.AddMarker(first.AddDays(14), "800,00").Style = KBotLaneMarkerStyle.NoChange
        b.AddMarker(first.AddDays(27), "950,00")
        _previewLanes.Add(b)

        ' 3 — one the server will refuse to move, and the record that closed it.
        Dim c As New KBotLane("preview-3", "Recepția 103")
        c.EndMark = KBotLaneEndMark.Ok
        c.AddMarker(first.AddDays(8), "2.310,00").Style = KBotLaneMarkerStyle.Locked
        c.AddMarker(first.AddDays(23), "2.310,00")
        c.AddMarker(first.AddDays(33), "anulată").Style = KBotLaneMarkerStyle.Deletion
        _previewLanes.Add(c)

        ' 4 — a heading, not a destination: drawn, never offered as a drop target.
        Dim d As New KBotLane("preview-4", "Recepția 104")
        d.IsTarget = False
        d.AddMarker(first.AddDays(16), "540,00")
        _previewLanes.Add(d)

        ' 5 — under the line: what has not been placed on anything yet. THE point of the surface.
        Dim e As New KBotLane("preview-5", "Neplasate")
        e.SeparatorAbove = True
        e.AddMarker(first.AddDays(30), "410,00").Style = KBotLaneMarkerStyle.Loose
        e.AddMarker(first.AddDays(38), "275,00").Style = KBotLaneMarkerStyle.Loose
        _previewLanes.Add(e)

        ' One dated line, so the shared axis with KBotChartView is visible here too.
        _previewGuides.Add(New KBotChartGuide(first.AddDays(21), "Plată"))
    End Sub

End Class
