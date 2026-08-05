Option Strict On
Imports System.Drawing
Imports KBot.Controls
Imports Xunit

' Slice 0024 — the popup-candidate filter.
'
' This filter decides whether to call ShowWindow(SW_HIDE) on a window owned by ANOTHER PROCESS, so
' "probably the right one" is not good enough. Every rejection reason is pinned here, because the
' log line the operator will read after the next Adobe update is exactly this verdict.
Public Class AdobePopupFilterTests

    Private Shared ReadOnly Host As New Rectangle(100, 100, 800, 600)
    Private Shared ReadOnly Badge As New Rectangle(700, 150, 240, 80)
    Private Shared ReadOnly OurPids As Integer() = {4242}

    Private Shared Function Evaluate(Optional cls As String = AdobePopupFilter.PopupClass,
                                     Optional pid As Integer = 4242,
                                     Optional rect As Rectangle = Nothing,
                                     Optional visible As Boolean = True) As AdobePopupVerdict
        Dim r As Rectangle = If(rect = Nothing, Badge, rect)
        Return AdobePopupFilter.Evaluate(cls, pid, OurPids, r, Host, visible)
    End Function

    <Fact>
    Public Sub TheAdobeBadgeOverOurHost_IsAccepted()
        Assert.Equal(AdobePopupVerdict.Accepted, Evaluate())
    End Sub

    <Fact>
    Public Sub AnotherClass_IsRejectedFirst()
        ' The pre-filter: without it every window on the desktop would be a candidate.
        Assert.Equal(AdobePopupVerdict.WrongClass, Evaluate(cls:="AVL_AVView"))
        Assert.Equal(AdobePopupVerdict.WrongClass, Evaluate(cls:=""))
        Assert.Equal(AdobePopupVerdict.WrongClass, Evaluate(cls:=Nothing))
    End Sub

    <Fact>
    Public Sub TheClassComparison_IsCaseInsensitive()
        Assert.Equal(AdobePopupVerdict.Accepted, Evaluate(cls:="avl_avpopup"))
    End Sub

    <Fact>
    Public Sub AWindowOfAnotherProcess_IsRejected()
        ' The operator's OWN Adobe may be running with its own badges. Hiding one of those would be
        ' K-BOT reaching outside its window, which is the line this whole slice refuses to cross.
        Assert.Equal(AdobePopupVerdict.ForeignProcess, Evaluate(pid:=9999))
        Assert.Equal(AdobePopupVerdict.ForeignProcess, Evaluate(pid:=0))
    End Sub

    <Fact>
    Public Sub NoKnownAdobeProcess_RejectsEverything()
        Assert.Equal(AdobePopupVerdict.ForeignProcess,
                     AdobePopupFilter.Evaluate(AdobePopupFilter.PopupClass, 4242, Nothing, Badge, Host, True))
        Assert.Equal(AdobePopupVerdict.ForeignProcess,
                     AdobePopupFilter.Evaluate(AdobePopupFilter.PopupClass, 4242, New Integer() {}, Badge, Host, True))
    End Sub

    <Fact>
    Public Sub AnAlreadyInvisibleBadge_IsRejected()
        ' Same rule as HideOutcome: hiding what is already hidden proves nothing and must not be
        ' logged as a success.
        Assert.Equal(AdobePopupVerdict.NotVisible, Evaluate(visible:=False))
    End Sub

    <Fact>
    Public Sub ADegenerateRectangle_IsRejected()
        Assert.Equal(AdobePopupVerdict.TooSmall, Evaluate(rect:=New Rectangle(700, 150, 0, 0)))
        Assert.Equal(AdobePopupVerdict.TooSmall, Evaluate(rect:=New Rectangle(700, 150, 240, 2)))
    End Sub

    <Fact>
    Public Sub AWindowBiggerThanTheHost_IsRejected()
        ' A badge is small. Something the size of a window IS a window — very possibly the document
        ' we are hosting.
        Assert.Equal(AdobePopupVerdict.TooLarge, Evaluate(rect:=New Rectangle(100, 100, 900, 300)))
        Assert.Equal(AdobePopupVerdict.TooLarge, Evaluate(rect:=New Rectangle(100, 100, 300, 700)))
    End Sub

    <Fact>
    Public Sub ABadgeThatDoesNotOverlapTheHost_IsRejected()
        Assert.Equal(AdobePopupVerdict.NoIntersection,
                     Evaluate(rect:=New Rectangle(1500, 1500, 240, 80)))
    End Sub

    <Fact>
    Public Sub TouchingTheHostEdge_Counts()
        ' IntersectsWith is exclusive on the far edge; a badge that starts exactly where the host
        ' ends does NOT overlap, and one that ends exactly where the host starts does not either.
        Assert.Equal(AdobePopupVerdict.NoIntersection, Evaluate(rect:=New Rectangle(900, 150, 240, 80)))
        Assert.Equal(AdobePopupVerdict.Accepted, Evaluate(rect:=New Rectangle(899, 150, 240, 80)))
    End Sub

    <Fact>
    Public Sub AnUnknownHostRectangle_SkipsTheGeometryFilters()
        ' Before the host panel has a handle its rectangle is empty. Rejecting everything then would
        ' make the watcher silently useless; the class + process filters still apply.
        Assert.Equal(AdobePopupVerdict.Accepted,
                     AdobePopupFilter.Evaluate(AdobePopupFilter.PopupClass, 4242, OurPids,
                                               Badge, Rectangle.Empty, True))
    End Sub

    <Fact>
    Public Sub EveryVerdict_HasItsOwnRomanianLabel()
        Assert.Equal("ACCEPTAT", AdobePopupFilter.Label(AdobePopupVerdict.Accepted))
        Assert.Equal("RESPINS (altă clasă)", AdobePopupFilter.Label(AdobePopupVerdict.WrongClass))
        Assert.Equal("RESPINS (alt proces)", AdobePopupFilter.Label(AdobePopupVerdict.ForeignProcess))
        Assert.Equal("RESPINS (în afara gazdei)", AdobePopupFilter.Label(AdobePopupVerdict.NoIntersection))
        Assert.Equal("RESPINS (prea mic)", AdobePopupFilter.Label(AdobePopupVerdict.TooSmall))
        Assert.Equal("RESPINS (mai mare decât gazda)", AdobePopupFilter.Label(AdobePopupVerdict.TooLarge))
        Assert.Equal("RESPINS (deja invizibil)", AdobePopupFilter.Label(AdobePopupVerdict.NotVisible))
    End Sub

End Class
