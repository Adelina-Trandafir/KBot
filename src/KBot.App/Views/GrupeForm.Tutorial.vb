Option Strict On
Imports System.Collections.Generic
Imports KBot.Theming

''' <summary>
''' Slice 000T-12: the groups window as a tutorial host. It is modal, so the tutorial builds its ring,
''' veil and bubble for THIS window after it is shown. Every control a step names is found by name;
''' the window raises two signals for the steps that wait on something a click does not show
''' (an angajament unticked in an existing group, an angajament dropped on a group).
''' </summary>
Partial Public Class GrupeForm
    Implements IKBotTutorialHost

    ''' <summary>Signal: an angajament was unticked in an existing group (it left the list).</summary>
    Friend Const SignalAngajamentScos As String = "angajament-scos"

    ''' <summary>Signal: an ungrouped angajament was dropped on a group and saved.</summary>
    Friend Const SignalAngajamentAdaugat As String = "angajament-adaugat"

    ''' <summary>The flows this window serves (their <c>host-key:</c>).</summary>
    Private Shared ReadOnly TutorialKeys As String() = {"grupe-meniu", "grupe-grupa-noua", "grupe-scoate", "grupe-adauga"}

    Public Event TutorialSignal(k_name As String) Implements IKBotTutorialHost.TutorialSignal

    Public Function TutorialSupports(k_key As String) As Boolean Implements IKBotTutorialHost.TutorialSupports
        Return Array.Exists(TutorialKeys, Function(k_k) String.Equals(k_k, k_key, StringComparison.OrdinalIgnoreCase))
    End Function

    Public Sub TutorialBegin(k_request As KBotTutorialRequest) Implements IKBotTutorialHost.TutorialBegin
        ' The window works as always while a tutorial points at it.
    End Sub

    Public Sub TutorialEnd() Implements IKBotTutorialHost.TutorialEnd
        ' Nothing to put back.
    End Sub

    Public Function TutorialAnchor(k_name As String) As IReadOnlyList(Of Rectangle) Implements IKBotTutorialHost.TutorialAnchor
        Return New List(Of Rectangle)()
    End Function

End Class
