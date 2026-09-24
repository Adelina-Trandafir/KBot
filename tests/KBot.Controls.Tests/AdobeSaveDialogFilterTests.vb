Option Strict On
Imports Xunit
Imports KBot.Controls

' Slice 0078-02: the pure decisions of the Save As trap -- which dialog is filled, which prompt is
' answered «Yes», and when the path read back counts as the target.
Public Class AdobeSaveDialogFilterTests

    Private Shared ReadOnly Pids As Integer() = {100, 200}

    Private Shared Function SaveAs(Optional pid As Integer = 100) As AdobeDialogFacts
        Return New AdobeDialogFacts With {.ClassName = "#32770", .OwnerPid = pid,
                                          .HasFileNameEdit = True, .HasOkButton = True}
    End Function

    <Fact>
    Public Sub FileDialogOfHostedAdobe_IsSaveAs()
        Assert.Equal(AdobeDialogKind.SaveAs, AdobeSaveDialogFilter.Classify(SaveAs(), Pids, IntPtr.Zero))
    End Sub

    <Fact>
    Public Sub FileDialogOfAnotherProcess_IsNotOurs()
        Assert.Equal(AdobeDialogKind.NotOurs, AdobeSaveDialogFilter.Classify(SaveAs(pid:=999), Pids, IntPtr.Zero))
    End Sub

    <Fact>
    Public Sub NonDialogClass_IsNotOurs()
        Dim f As AdobeDialogFacts = SaveAs()
        f.ClassName = "AVL_AVView"
        Assert.Equal(AdobeDialogKind.NotOurs, AdobeSaveDialogFilter.Classify(f, Pids, IntPtr.Zero))
    End Sub

    <Fact>
    Public Sub FileDialogWithoutSaveButton_IsOther()
        Dim f As AdobeDialogFacts = SaveAs()
        f.HasOkButton = False
        Assert.Equal(AdobeDialogKind.Other, AdobeSaveDialogFilter.Classify(f, Pids, IntPtr.Zero))
    End Sub

    <Fact>
    Public Sub ConfirmOwnedByPressedSaveAs_IsConfirm()
        Dim pressed As New IntPtr(&H1234)
        Dim f As New AdobeDialogFacts With {.ClassName = "#32770", .OwnerPid = 200, .OwnerWindow = pressed, .IsTaskDialog = True}
        Assert.Equal(AdobeDialogKind.ConfirmOverwrite, AdobeSaveDialogFilter.Classify(f, Pids, pressed))
    End Sub

    <Fact>
    Public Sub YesNoPromptNotOwnedByOurSaveAs_IsNeverAnswered()
        Dim f As New AdobeDialogFacts With {.ClassName = "#32770", .OwnerPid = 100,
                                            .OwnerWindow = New IntPtr(&H9999), .HasYesButton = True}
        Assert.Equal(AdobeDialogKind.Other, AdobeSaveDialogFilter.Classify(f, Pids, New IntPtr(&H1234)))
    End Sub

    <Fact>
    Public Sub YesNoPromptWhileNothingPressed_IsOther()
        Dim f As New AdobeDialogFacts With {.ClassName = "#32770", .OwnerPid = 100, .HasYesButton = True}
        Assert.Equal(AdobeDialogKind.Other, AdobeSaveDialogFilter.Classify(f, Pids, IntPtr.Zero))
    End Sub

    <Theory>
    <InlineData("C:\PDF\DDF_NR_1_REV_0_X.PDF", "C:\PDF\DDF_NR_1_REV_0_X.PDF", True)>
    <InlineData("""c:\pdf\ddf_nr_1_rev_0_x.pdf""", "C:\PDF\DDF_NR_1_REV_0_X.PDF", True)>
    <InlineData("  C:\PDF\DDF_NR_1_REV_0_X.PDF  ", "C:\PDF\DDF_NR_1_REV_0_X.PDF", True)>
    <InlineData("DDF_NR_1_REV_0_X.PDF", "C:\PDF\DDF_NR_1_REV_0_X.PDF", False)>
    <InlineData("C:\PDF\DDF_NR_1_REV_0_X", "C:\PDF\DDF_NR_1_REV_0_X.PDF", False)>
    <InlineData("D:\Other\DDF_NR_1_REV_0_X.PDF", "C:\PDF\DDF_NR_1_REV_0_X.PDF", False)>
    <InlineData("", "C:\PDF\DDF_NR_1_REV_0_X.PDF", False)>
    Public Sub SamePath_OnlyTheExactTarget(readBack As String, target As String, expected As Boolean)
        Assert.Equal(expected, AdobeSaveDialogFilter.SamePath(readBack, target))
    End Sub

End Class
