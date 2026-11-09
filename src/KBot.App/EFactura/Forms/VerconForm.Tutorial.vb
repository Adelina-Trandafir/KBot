Option Strict On
Imports System.Collections.Generic
Imports System.Drawing
Imports KBot.Theming

''' <summary>
''' Slice 0000-118: the page «Trimite la Vercon» as a tutorial host (the tutorial «efactura-trimite-la-vercon»). The tutorial points at its controls by name
''' (the tree of invoices, the lines grid, the note, the buttons); the page works as always while it does.
''' </summary>
Partial Public Class VerconForm
    Implements IKBotTutorialHost

    ''' <summary>The flows this page serves (their <c>host-key:</c>).</summary>
    Private Shared ReadOnly TutorialKeys As String() = {"efactura-vercon"}

    Public Event TutorialSignal(k_name As String) Implements IKBotTutorialHost.TutorialSignal

    Public Function TutorialSupports(k_key As String) As Boolean Implements IKBotTutorialHost.TutorialSupports
        Return Array.Exists(TutorialKeys, Function(k_k) String.Equals(k_k, k_key, StringComparison.OrdinalIgnoreCase))
    End Function

    Public Sub TutorialBegin(k_request As KBotTutorialRequest) Implements IKBotTutorialHost.TutorialBegin
        ' The page works as always while a tutorial points at it.
    End Sub

    Public Sub TutorialEnd() Implements IKBotTutorialHost.TutorialEnd
        ' Nothing to put back.
    End Sub

    Public Function TutorialAnchor(k_name As String) As IReadOnlyList(Of Rectangle) Implements IKBotTutorialHost.TutorialAnchor
        Return New List(Of Rectangle)()
    End Function

End Class
