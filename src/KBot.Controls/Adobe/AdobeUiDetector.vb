Option Strict On
Imports System.Collections.Generic
Imports System.Linq

''' <summary>Which generation of Adobe's UI is actually on screen.</summary>
Public Enum AdobeUiGeneration
    ''' <summary>The window tree carries none of the known markers.</summary>
    Unknown = 0
    ''' <summary>Classic viewer — the one with the right-hand task pane (bEnableAv2 = 0).</summary>
    Classic = 1
    ''' <summary>Modern viewer — the tabbed AV2 shell (bEnableAv2 = 1).</summary>
    Modern = 2
End Enum

''' <summary>What the detector concluded and the evidence it concluded it from.</summary>
Public NotInheritable Class AdobeUiDetection

    Public ReadOnly Property Generation As AdobeUiGeneration
    ''' <summary>The marker that decided it, e.g. «AV2DockableTabStripView prezent».</summary>
    Public ReadOnly Property Evidence As String
    ''' <summary>True when markers of BOTH generations were present — reported, never hidden.</summary>
    Public ReadOnly Property Ambiguous As Boolean

    Public Sub New(generation As AdobeUiGeneration, evidence As String, ambiguous As Boolean)
        Me.Generation = generation
        Me.Evidence = If(evidence, "")
        Me.Ambiguous = ambiguous
    End Sub

    ''' <summary>The log line: «Mod detectat: Modern (AV2DockableTabStripView prezent)».</summary>
    Public Function Describe() As String
        Dim label As String
        Select Case Generation
            Case AdobeUiGeneration.Modern : label = "Modern"
            Case AdobeUiGeneration.Classic : label = "Clasic"
            Case Else : label = "NERECUNOSCUT"
        End Select
        Dim s As String = $"Mod detectat: {label} ({Evidence})"
        If Ambiguous Then s &= " — ATENȚIE: arborele conține marcaje din AMBELE generații."
        Return s
    End Function

End Class

''' <summary>
''' Reads the viewer generation off the window tree.
'''
''' DETECTION READS THE WINDOW TREE, NOT THE REGISTRY. It must not depend on <c>bEnableAv2</c>:
''' the shipping code never writes that value (see <see cref="AdobeReaderHost"/> and the worklog),
''' so the only honest question is "what is actually on screen right now".
'''
''' Markers are matched against a node's TEXT or its CLASS. In every probe the bench recorded they
''' arrive as window TEXT on <c>AVL_AVView</c> children — but a future Adobe could just as well make
''' them class names, and matching both costs nothing.
''' </summary>
Public NotInheritable Class AdobeUiDetector

    Private Sub New()
    End Sub

    ''' <summary>The classic viewer's right-hand task pane host.</summary>
    Public Const ClassicMarker As String = "AVTaskPaneHostView"
    ''' <summary>The modern viewer's document tab strip (either marker is enough).</summary>
    Public Const ModernMarkerTab As String = "AV2DocumentTabView"
    Public Const ModernMarkerStrip As String = "AV2DockableTabStripView"

    ''' <summary>
    ''' Pure: the same node list always produces the same verdict, so the whole rule is unit-tested
    ''' against recorded trees without Adobe being installed.
    '''
    ''' Rule order is the one the slice brief fixed: the classic marker decides first, the modern
    ''' markers second, and anything else is <see cref="AdobeUiGeneration.Unknown"/>. When markers
    ''' of both generations are present the classic rule still wins (first match), but the result is
    ''' flagged <see cref="AdobeUiDetection.Ambiguous"/> so the log says so out loud.
    ''' </summary>
    Public Shared Function Detect(nodes As IEnumerable(Of AdobeWindowNode)) As AdobeUiDetection
        Dim list As List(Of AdobeWindowNode) =
            If(nodes Is Nothing, New List(Of AdobeWindowNode)(), nodes.ToList())

        Dim classic As Boolean = list.Any(Function(n) n.Matches(ClassicMarker))
        Dim modernTab As Boolean = list.Any(Function(n) n.Matches(ModernMarkerTab))
        Dim modernStrip As Boolean = list.Any(Function(n) n.Matches(ModernMarkerStrip))
        Dim modern As Boolean = modernTab OrElse modernStrip
        Dim ambiguous As Boolean = classic AndAlso modern

        If classic Then
            Return New AdobeUiDetection(AdobeUiGeneration.Classic, ClassicMarker & " prezent", ambiguous)
        End If
        If modern Then
            Dim marker As String = If(modernStrip, ModernMarkerStrip, ModernMarkerTab)
            Return New AdobeUiDetection(AdobeUiGeneration.Modern, marker & " prezent", ambiguous)
        End If
        Return New AdobeUiDetection(AdobeUiGeneration.Unknown,
                                    $"niciun marcaj cunoscut în {list.Count} ferestre copil",
                                    ambiguous:=False)
    End Function

    ''' <summary>
    ''' The Romanian note shown DISCREETLY in the view when the Adobe version was not recognised.
    ''' Kept here so the wording lives next to the rule that triggers it.
    ''' </summary>
    Public Const UnrecognisedNote As String =
        "Versiunea Adobe nu a fost recunoscută — se folosește profilul clasic. Vezi jurnalul."

End Class
