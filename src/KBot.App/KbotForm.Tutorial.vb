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
    Implements ITutorialRequirements

    ''' <summary>The key of the «Designer tutoriale» row of the menu (Debug build only, slice 000T-10).</summary>
    Private Const TutorialDesignerMenuKey As String = "tutoriale_designer"

    ''' <summary>Slice 000T-10: the tutorial designer / recorder is an operator tool of the Debug build only.</summary>
    Friend Shared ReadOnly Property TutorialDesignerAvailable As Boolean
        Get
#If DEBUG Then
            Return True
#Else
            Return False
#End If
        End Get
    End Property

    Private Sub DeschideDesignerulDeTutoriale()
        If Not TutorialDesignerAvailable Then Return
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

    ''' <summary>
    ''' Anchor prefix (slice 000T-09): <c>menu.&lt;key&gt;</c> = that row of the open MENIU menu (<c>menu.setari</c> =
    ''' «Configurare K-BOT»); not on screen while the menu is closed.
    ''' </summary>
    Friend Const AnchorMenuPrefix As String = "menu."

    ''' <summary>
    ''' Anchor prefix (slice 000T-10): <c>popup.&lt;key&gt;</c> = that row of the pop-up list that is open now (the main tree's
    ''' options list: <c>popup.sort-name</c>, <c>popup.sort-date</c>, <c>popup.update-many</c>...).
    ''' </summary>
    Friend Const AnchorPopupPrefix As String = "popup."

    ''' <summary>
    ''' Anchor prefix (slice 000T-12): <c>grupe.&lt;key&gt;</c> = that row of the groups submenu that is open now
    ''' (<c>grupe.grupe-editeaza</c> = the «Editeaza grupe...» row).
    ''' </summary>
    Friend Const AnchorGrupePrefix As String = "grupe."

    ''' <summary>Signal prefix (slice 000T-10): <c>tree-menu:&lt;key&gt;</c> = that row of the tree options list was chosen.</summary>
    Friend Const SignalTreeMenuPrefix As String = "tree-menu:"

    ''' <summary>Signal prefix (slice 000T-10): <c>selector-unit</c> / <c>-year</c> / <c>-ss</c> = another choice in that selector of the title bar.</summary>
    Friend Const SignalSelectorPrefix As String = "selector-"

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
            If k_name.StartsWith(AnchorMenuPrefix, StringComparison.OrdinalIgnoreCase) Then
                Dim k_row As Rectangle = menuNou.RowScreenBounds(k_name.Substring(AnchorMenuPrefix.Length))
                If Not k_row.IsEmpty Then k_none.Add(k_row)
                Return k_none
            End If
            If k_name.StartsWith(AnchorGrupePrefix, StringComparison.OrdinalIgnoreCase) Then
                Dim k_row As Rectangle = menuGrupe.RowScreenBounds(k_name.Substring(AnchorGrupePrefix.Length))
                If Not k_row.IsEmpty Then k_none.Add(k_row)
                Return k_none
            End If
            If k_name.StartsWith(AnchorPopupPrefix, StringComparison.OrdinalIgnoreCase) Then
                ' «popup.a+b» = the rows a and b together (one ring around both).
                Dim k_keys As String() = k_name.Substring(AnchorPopupPrefix.Length).Split("+"c)
                For Each k_popup As CustomPopup In Application.OpenForms.OfType(Of CustomPopup)().ToList()
                    Dim k_union As Rectangle = Rectangle.Empty
                    For Each k_key As String In k_keys
                        Dim k_row As Rectangle = k_popup.RowScreenBounds(k_key.Trim())
                        If k_row.IsEmpty Then Continue For
                        k_union = If(k_union.IsEmpty, k_row, Rectangle.Union(k_union, k_row))
                    Next
                    If k_union.IsEmpty Then Continue For
                    k_none.Add(k_union)
                    Exit For
                Next
                Return k_none
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

    ' Slice 000T-10: another choice in a selector of the title bar (unit, year, source/sector) is a signal.
    Private Sub Tutorial_SelectorChanged(sender As Object, e As CaptionSelectorChangedEventArgs) Handles capBar.SelectorChanged
        Try
            If e IsNot Nothing Then RaiseEvent TutorialSignal(SignalSelectorPrefix & e.Selector)
        Catch ex As Exception
            GlobalErrorLog.Write("KbotForm.Tutorial_SelectorChanged", ex)
        End Try
    End Sub

    ' ── what a tutorial may require (slice 000T-10) ─────────────────────────────────

    ''' <summary>The <c>requires:</c> value for a live FOREXE session.</summary>
    Friend Const RequiresForexe As String = "forexe"

    Public Function TutorialRequirementMet(k_name As String) As Boolean Implements ITutorialRequirements.TutorialRequirementMet
        If String.Equals(k_name, RequiresForexe, StringComparison.OrdinalIgnoreCase) Then
            ' A Debug build always meets it, so the tutorials can be written and tried without a certificate.
#If DEBUG Then
            Return True
#Else
            Return _controller IsNot Nothing AndAlso _controller.IsConnected
#End If
        End If
        ' Slice 000T-10: how the main tree is sorted (the steps of a tutorial that differ by it).
        If String.Equals(k_name, "sort-date", StringComparison.OrdinalIgnoreCase) Then Return AppSettings.Current.TreeSortIsDate
        If String.Equals(k_name, "sort-name", StringComparison.OrdinalIgnoreCase) Then Return Not AppSettings.Current.TreeSortIsDate
        GlobalErrorLog.Write("KbotForm.TutorialRequirementMet", New ArgumentException("Unknown tutorial requirement '" & k_name & "'."))
        Return False
    End Function

    Public Function TutorialRequirementText(k_name As String) As String Implements ITutorialRequirements.TutorialRequirementText
        If String.Equals(k_name, RequiresForexe, StringComparison.OrdinalIgnoreCase) Then
            Return "Acest tutorial se poate face doar cât ești conectat la FOREXE. Conectează-te întâi (butonul «Conectare» din banda de jos), apoi pornește-l din nou."
        End If
        Return "Acest tutorial nu poate porni acum."
    End Function

    Private Function HasReservations(k_item As AdvancedTreeControl.TreeItem) As Boolean
        Dim k_cod As String = TryCast(k_item.Tag, String)
        Dim k_info As AngajamentTreeInfo = Nothing
        Return k_cod IsNot Nothing AndAlso _treeInfos.TryGetValue(k_cod, k_info) AndAlso k_info.AreRezervari
    End Function

End Class
