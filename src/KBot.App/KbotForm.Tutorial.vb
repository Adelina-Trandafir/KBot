Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain

''' <summary>
''' Slice 000T: the main window as a tutorial host. It serves every tutorial key (it is where they
''' start) and knows two places no generic control name can give: the rows of the angajamente that
''' have reservations, and, through the open view, the row that carries the «+».
''' </summary>
Partial Public Class KbotForm
    Implements IKBotTutorialHost

    ''' <summary>The key of the «Designer tutoriale» row of the menu (capture mode only).</summary>
    Private Const TutorialDesignerMenuKey As String = "tutoriale_designer"

    Private Sub DeschideDesignerulDeTutoriale()
        Try
            Dim k_help As HelpService = TryCast(KBotHelp.Provider, HelpService)
            If k_help Is Nothing Then Throw New InvalidOperationException("The help service is not installed.")
            TutorialDesignerForm.ShowFor(Me, k_help)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.DeschideDesignerulDeTutoriale", ex)
            KBotMessage.Show(Me, "Designerul de tutoriale nu a putut fi deschis. Detalii în jurnalul de erori.",
                             "Designer tutoriale", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>Anchor: the angajamente on screen that have reservations (the «Rezervari» view is open to them).</summary>
    Friend Const AnchorAngajamenteCuRezervari As String = "tree.angajamente-cu-rezervari"

    ''' <summary>Signal: the row just selected is an angajament that has reservations.</summary>
    Friend Const SignalAngajamentCuRezervari As String = "angajament-cu-rezervari"

    ''' <summary>Raised by <c>Tree_NodeMouseUp</c> (see <see cref="SignalAngajamentCuRezervari"/>).</summary>
    Public Event TutorialSignal(k_name As String) Implements IKBotTutorialHost.TutorialSignal

    Public Function TutorialSupports(k_key As String) As Boolean Implements IKBotTutorialHost.TutorialSupports
        Return True
    End Function

    Public Sub TutorialBegin(k_request As KBotTutorialRequest) Implements IKBotTutorialHost.TutorialBegin
        ' Nothing to switch on: the window behaves as always while a tutorial points at it.
    End Sub

    Public Sub TutorialEnd() Implements IKBotTutorialHost.TutorialEnd
        ' Nothing to put back.
    End Sub

    Public Function TutorialAnchor(k_name As String) As IReadOnlyList(Of Rectangle) Implements IKBotTutorialHost.TutorialAnchor
        Dim k_none As New List(Of Rectangle)()
        Try
            If String.Equals(k_name, AnchorAngajamenteCuRezervari, StringComparison.OrdinalIgnoreCase) Then
                Dim k_rows As List(Of Rectangle) = tree.RowRectsOnScreen(AddressOf HasReservations)
                Return k_rows.Select(Function(r) tree.RectangleToScreen(r)).ToList()
            End If
            ' «rezervari.*» belongs to the open Rezervari view.
            Dim k_view As RezervariView = TryCast(_activeView, RezervariView)
            If k_view IsNot Nothing Then Return k_view.TutorialAnchor(k_name)
            Return k_none
        Catch ex As Exception
            ' UI boundary (called by the tutorial's timer): an anchor that cannot be read is «not on screen».
            GlobalErrorLog.Write("KbotForm.TutorialAnchor", ex)
            Return k_none
        End Try
    End Function

    Private Function HasReservations(k_item As AdvancedTreeControl.TreeItem) As Boolean
        Dim k_cod As String = TryCast(k_item.Tag, String)
        Dim k_info As AngajamentTreeInfo = Nothing
        Return k_cod IsNot Nothing AndAlso _treeInfos.TryGetValue(k_cod, k_info) AndAlso k_info.AreRezervari
    End Function

End Class
