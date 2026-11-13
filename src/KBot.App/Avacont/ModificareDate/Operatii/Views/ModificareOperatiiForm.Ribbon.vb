Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

' Slice AV02-05 - «Modificare operatii»: the buttons of the ribbon's view group while the page is on screen (the Access ribbon «MNC», what of it is
' ported: the info panel, the default order, clearing the filters, Excel of the list as shown or of the whole year, the two deletes, reload).
' The buttons are made in the designer of AvacontMain with these keys.
Partial Public Class ModificareOperatiiForm

    Friend Const KeyInfo As String = "modificare_operatii_info"
    Friend Const KeyOrdonare As String = "modificare_operatii_ordonare"
    Friend Const KeyCurata As String = "modificare_operatii_curata"
    Friend Const KeyExcel As String = "modificare_operatii_excel"
    Friend Const KeyExcelTot As String = "modificare_operatii_excel_tot"
    Friend Const KeyStergeOperatie As String = "modificare_operatii_sterge"
    Friend Const KeyStergeDocument As String = "modificare_operatii_sterge_doc"
    Friend Const KeyReincarca As String = "modificare_operatii_reincarca"

    ''' <summary>Every key of the page's view group (AvacontMain lists them among its view keys).</summary>
    Friend Shared ReadOnly Property AllRibbonKeys As String() = {
        KeyReincarca, KeyInfo, KeyOrdonare, KeyCurata, KeyExcel, KeyExcelTot, KeyStergeOperatie, KeyStergeDocument}

    Public ReadOnly Property RibbonCaption As String Implements IViewRibbon.RibbonCaption
        Get
            Return Caption
        End Get
    End Property

    Public ReadOnly Property RibbonKeys As IReadOnlyList(Of String) Implements IViewRibbon.RibbonKeys
        Get
            Return AllRibbonKeys
        End Get
    End Property

    Public Function RibbonButtonState(k_key As String) As ViewButtonState Implements IViewRibbon.RibbonButtonState
        If Not AllRibbonKeys.Contains(k_key) Then Return ViewButtonState.Hidden
        If _busy Then Return ViewButtonState.Dimmed
        Select Case k_key
            Case KeyReincarca, KeyInfo
                Return ViewButtonState.Enabled
            Case KeyStergeOperatie, KeyStergeDocument
                Return If(SelectedOperatie() IsNot Nothing, ViewButtonState.Enabled, ViewButtonState.Dimmed)
            Case KeyCurata
                Return If(gridOperatii.IsFiltered OrElse gridOperatii.SortDirection <> KBotSortDirection.None, ViewButtonState.Enabled, ViewButtonState.Dimmed)
            Case KeyOrdonare
                Return If(gridOperatii.SortDirection <> KBotSortDirection.None, ViewButtonState.Enabled, ViewButtonState.Dimmed)
            Case Else
                Return If(gridOperatii.RowCount > 0, ViewButtonState.Enabled, ViewButtonState.Dimmed)
        End Select
    End Function

    Public Function RibbonButtonText(k_key As String) As String Implements IViewRibbon.RibbonButtonText
        If Not String.Equals(k_key, KeyInfo, StringComparison.Ordinal) Then Return String.Empty
        Return If(InfoVisible, "Ascunde informațiile", "Arată informațiile")
    End Function

    Public Sub RunRibbonButton(k_key As String) Implements IViewRibbon.RunRibbonButton
        Try
            Select Case k_key
                Case KeyReincarca : LoadList()
                Case KeyInfo : ToggleInfo()
                Case KeyOrdonare : OrdonareImplicita()
                Case KeyCurata : CurataFiltre()
                Case KeyExcel : ExportExcel(True)
                Case KeyExcelTot : ExportExcel(False)
                Case KeyStergeOperatie : StergeOperatie()
                Case KeyStergeDocument : StergeDocument()
                Case Else : Throw New ArgumentException("Unknown ribbon key: " & k_key, NameOf(k_key))
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("ModificareOperatiiForm.RunRibbonButton", ex)
        End Try
    End Sub

    Private Sub GridOperatii_FilterChanged(sender As Object, e As EventArgs) Handles gridOperatii.FilterChanged
        RefreshButtons()
    End Sub

    ' «Exporta in Excel»: the rows as on screen (filters and sort applied), or the whole year in the default order.
    Private Sub ExportExcel(k_asShown As Boolean)
        Try
            If _busy OrElse gridOperatii.RowCount = 0 Then Return
            Dim k_rows As IEnumerable(Of KBotDataRow) = If(k_asShown, gridOperatii.VisibleRows(), gridOperatii.Rows)
            Dim k_ops As List(Of OperatieContabila) = k_rows.Select(Function(k_r) TryCast(k_r.Tag, OperatieContabila)).Where(Function(k_o) k_o IsNot Nothing).ToList()
            Using k_dialog As New SaveFileDialog()
                k_dialog.Title = "Salvează operațiile ca Excel"
                k_dialog.Filter = "Excel (*.xlsx)|*.xlsx"
                k_dialog.FileName = $"Operatii_contabile_{_an.ToString(CultureInfo.InvariantCulture)}{If(k_asShown AndAlso gridOperatii.IsFiltered, "_filtrat", String.Empty)}.xlsx"
                k_dialog.OverwritePrompt = True
                If k_dialog.ShowDialog(Me) <> DialogResult.OK Then Return
                Try
                    System.IO.File.WriteAllBytes(k_dialog.FileName, ModificareOperatiiExcel.Build(k_ops))
                Catch ex As System.IO.IOException
                    GlobalErrorLog.Write("ModificareOperatiiForm.ExportExcel", ex)
                    KBotMessage.Show(Me, "Fișierul nu a putut fi scris. Este deschis în alt program sau locul nu permite scrierea.",
                                     Caption, MessageBoxButtons.OK, MessageBoxIcon.Warning)
                End Try
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("ModificareOperatiiForm.ExportExcel", ex)
            If Not IsDisposed Then
                KBotMessage.Show(Me, "Excelul nu a putut fi salvat. Detalii în jurnalul de erori.", Caption, MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        End Try
    End Sub

End Class
