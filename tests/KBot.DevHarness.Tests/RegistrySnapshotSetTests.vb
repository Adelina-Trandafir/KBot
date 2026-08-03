Option Strict On
Imports Microsoft.Win32
Imports KBot.DevHarness
Imports Xunit

' Slice 0023, plan §6: the snapshot/restore model. The one trap that matters most: an ABSENT
' original restores to a DELETION, never to 0 — writing 0 would leave the operator's Adobe in a
' state they never had.
Public Class RegistrySnapshotSetTests

    Private Const HivePath As String = "HKEY_CURRENT_USER\Software\Adobe\Acrobat Reader\DC\AVGeneral"
    Private Const ValName As String = "bExpandRHPInViewer"

    <Fact>
    Public Sub Absent_RestoresToDeletion_NotToZero()
        Dim reg As New FakeRegistryAccess()
        Dim snapSet As New RegistrySnapshotSet(reg)

        ' Value absent at capture time; the harness then writes 0 (the apply).
        snapSet.Capture(HivePath, ValName)
        reg.Write(HivePath, ValName, RegistryValueKind.DWord, 0)

        snapSet.RestoreAll()

        Dim after = reg.Read(HivePath, ValName)
        Assert.Equal(RegPresence.Absent, after.Presence)   ' deleted, NOT dword 0
    End Sub

    <Fact>
    Public Sub Present_RestoresExactOriginalValueAndKind()
        Dim reg As New FakeRegistryAccess()
        reg.Seed(HivePath, ValName, RegistryValueKind.DWord, 1)
        Dim snapSet As New RegistrySnapshotSet(reg)

        snapSet.Capture(HivePath, ValName)
        reg.Write(HivePath, ValName, RegistryValueKind.DWord, 0)

        snapSet.RestoreAll()

        Dim after = reg.Read(HivePath, ValName)
        Assert.Equal(RegPresence.Present, after.Presence)
        Assert.Equal(RegistryValueKind.DWord, after.Kind)
        Assert.Equal(1, CInt(after.Value))
    End Sub

    <Fact>
    Public Sub WrongTypeOriginal_IsPreservedAsWrongType()
        ' aDefaultRHPViewMode_L should be REG_SZ, but the original on this machine is a DWORD
        ' (the "wrong-type" case). Restore must write back the DWORD, not coerce to a string.
        Dim reg As New FakeRegistryAccess()
        reg.Seed(HivePath, "aDefaultRHPViewMode_L", RegistryValueKind.DWord, 5)
        Dim snapSet As New RegistrySnapshotSet(reg)

        snapSet.Capture(HivePath, "aDefaultRHPViewMode_L")
        reg.Write(HivePath, "aDefaultRHPViewMode_L", RegistryValueKind.String, "Collapsed")

        snapSet.RestoreAll()

        Dim after = reg.Read(HivePath, "aDefaultRHPViewMode_L")
        Assert.Equal(RegPresence.Present, after.Presence)
        Assert.Equal(RegistryValueKind.DWord, after.Kind)
        Assert.Equal(5, CInt(after.Value))
    End Sub

    <Fact>
    Public Sub Capture_IsOncePerSession_SecondApplyDoesNotOverwriteOriginal()
        Dim reg As New FakeRegistryAccess()
        reg.Seed(HivePath, ValName, RegistryValueKind.DWord, 1)
        Dim snapSet As New RegistrySnapshotSet(reg)

        ' First apply: capture original (1), write 0.
        snapSet.Capture(HivePath, ValName)
        reg.Write(HivePath, ValName, RegistryValueKind.DWord, 0)

        ' Second apply in the same session: capture again (must be a no-op), write 7.
        snapSet.Capture(HivePath, ValName)
        reg.Write(HivePath, ValName, RegistryValueKind.DWord, 7)

        snapSet.RestoreAll()

        ' Restores the TRUE original (1), not the harness's own intermediate write (0).
        Dim after = reg.Read(HivePath, ValName)
        Assert.Equal(1, CInt(after.Value))
    End Sub

    <Fact>
    Public Sub Capture_TracksCountAndIsCaptured()
        Dim reg As New FakeRegistryAccess()
        Dim snapSet As New RegistrySnapshotSet(reg)

        Assert.False(snapSet.IsCaptured(HivePath, ValName))
        snapSet.Capture(HivePath, ValName)
        snapSet.Capture(HivePath, ValName)   ' idempotent
        Assert.True(snapSet.IsCaptured(HivePath, ValName))
        Assert.Equal(1, snapSet.Count)
    End Sub

    <Fact>
    Public Sub RestoreAll_MixedSet_EachValueGetsItsOwnTreatment()
        Dim reg As New FakeRegistryAccess()
        reg.Seed(HivePath, "bRHPSticky", RegistryValueKind.DWord, 0)
        ' bExpandRHPInViewer left absent.
        Dim snapSet As New RegistrySnapshotSet(reg)

        snapSet.Capture(HivePath, "bRHPSticky")
        snapSet.Capture(HivePath, ValName)
        reg.Write(HivePath, "bRHPSticky", RegistryValueKind.DWord, 1)
        reg.Write(HivePath, ValName, RegistryValueKind.DWord, 0)

        snapSet.RestoreAll()

        Assert.Equal(0, CInt(reg.Read(HivePath, "bRHPSticky").Value))
        Assert.Equal(RegPresence.Absent, reg.Read(HivePath, ValName).Presence)
    End Sub

End Class
