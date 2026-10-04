''' <summary>
''' Slice 000T: the DDF editor as a tutorial host. It is modal, so the tutorial builds its ring, veil
''' and bubble for THIS window after it is shown (a window made earlier would be disabled by the
''' modal loop). It serves the tutorials whose <c>host-key</c> it knows; every control a step names
''' is found by name, so it needs no anchors and raises no signals.
''' </summary>
Partial Public Class DdfEditForm
    Implements IKBotTutorialHost

    ''' <summary>The key of «Adauga o revizie pe baza unei rezervari existente».</summary>
    Friend Const TutorialKeyRezervarePlus As String = "rezervare-plus"

    ''' <summary>The editor raises no tutorial signal today; the contract asks every host for the event.</summary>
    Public Event TutorialSignal(k_name As String) Implements IKBotTutorialHost.TutorialSignal

    Public Function TutorialSupports(k_key As String) As Boolean Implements IKBotTutorialHost.TutorialSupports
        Return String.Equals(k_key, TutorialKeyRezervarePlus, StringComparison.OrdinalIgnoreCase)
    End Function

    Public Sub TutorialBegin(k_request As KBotTutorialRequest) Implements IKBotTutorialHost.TutorialBegin
        ' The editor works as always while a tutorial points at it.
    End Sub

    Public Sub TutorialEnd() Implements IKBotTutorialHost.TutorialEnd
        ' Nothing to put back.
    End Sub

    Public Function TutorialAnchor(k_name As String) As IReadOnlyList(Of Rectangle) Implements IKBotTutorialHost.TutorialAnchor
        Return New List(Of Rectangle)()
    End Function

End Class
