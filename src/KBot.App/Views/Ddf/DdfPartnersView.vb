Option Strict On
Imports System.Collections.Generic
Imports System.Linq
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>The partner a <see cref="DdfPartnersView"/> event is about.</summary>
Public NotInheritable Class DdfPartnerEventArgs
    Inherits EventArgs

    Public Sub New(partner As DdfPartenerAsociat)
        ArgumentNullException.ThrowIfNull(partner)
        Me.Partner = partner
    End Sub

    Public ReadOnly Property Partner As DdfPartenerAsociat
End Class

''' <summary>
''' The partners associated with a DDF (slice 0084-02): the list, a picker for one more, and a
''' way to take one out. ONE view for two hosts -- the «Parteneri» page of the DDF editor
''' (slice 0094-02) and the window the Sumar button opens (<c>SumarPartnersForm</c>).
'''
''' <para><b>The view decides nothing.</b> It shows what the host gives it
''' (<see cref="SetCandidates"/>, <see cref="SetAssociated"/>) and ASKS through
''' <see cref="AddRequested"/> / <see cref="RemoveRequested"/>; the host changes its own model
''' and hands the new list back. That is what lets the editor keep the list in its draft and the
''' Sumar window keep it until it is sent to the server, with nothing here knowing the
''' difference. The view makes no request and holds no document.</para>
'''
''' <para><b>The picker offers only partners not yet associated</b> (compared on the fiscal
''' code as digits, like the server), so the same partner cannot be added twice from here.</para>
''' </summary>
Public Class DdfPartnersView
    Implements IThemedControl

    Private Const COL_CF As String = "cod_fiscal"
    Private Const COL_NAME As String = "nume"
    Private Const COL_ROLE As String = "rol"

    Private ReadOnly _candidates As New List(Of DdfPartener)()
    ' The candidates the picker shows now, in the picker's own order, so a selected index can be
    ' turned back into a partner without parsing the caption.
    Private ReadOnly _offered As New List(Of DdfPartener)()
    Private ReadOnly _associated As New List(Of DdfPartenerAsociat)()

    ''' <summary>The operator asks for the chosen partner to be associated.</summary>
    Public Event AddRequested As EventHandler(Of DdfPartnerEventArgs)

    ''' <summary>The operator asks for the selected partner to be taken out.</summary>
    Public Event RemoveRequested As EventHandler(Of DdfPartnerEventArgs)

    ''' <summary>The word in the «Rol» column. Default: «Principal» for the header partner,
    ''' «Asociat» for the rest.</summary>
    Public Property RoleOf As Func(Of DdfPartenerAsociat, String) =
        Function(p) If(p.DinAntet, "Principal", "Asociat")

    ''' <summary>May this partner be taken out? Default: every one but the main (header)
    ''' partner. Decides whether «Scoate din asociere» is enabled for the selected row.</summary>
    Public Property CanRemove As Func(Of DdfPartenerAsociat, Boolean) = Function(p) Not p.DinAntet

    Public Sub New()
        InitializeComponent()
    End Sub

    ''' <summary>The partners the picker may offer (the editor's header combo source).</summary>
    Public Sub SetCandidates(candidates As IEnumerable(Of DdfPartener))
        Try
            _candidates.Clear()
            If candidates IsNot Nothing Then _candidates.AddRange(candidates.Where(Function(c) c IsNot Nothing))
            FillPicker()
        Catch ex As Exception
            GlobalErrorLog.Write("DdfPartnersView.SetCandidates", ex)
            Throw
        End Try
    End Sub

    ''' <summary>Shows the partners the document has. The rows stay the host's objects, so the
    ''' host can tell which one an event is about.</summary>
    Public Sub SetAssociated(associated As IEnumerable(Of DdfPartenerAsociat))
        Try
            _associated.Clear()
            If associated IsNot Nothing Then _associated.AddRange(associated.Where(Function(p) p IsNot Nothing))
            FillGrid()
            FillPicker()
        Catch ex As Exception
            GlobalErrorLog.Write("DdfPartnersView.SetAssociated", ex)
            Throw
        End Try
    End Sub

    ''' <summary>Re-reads the roles and the enabled state after the host changed what
    ''' <see cref="RoleOf"/> / <see cref="CanRemove"/> depend on.</summary>
    Public Sub RefreshRows()
        Try
            FillGrid()
        Catch ex As Exception
            GlobalErrorLog.Write("DdfPartnersView.RefreshRows", ex)
            Throw
        End Try
    End Sub

    Private Sub FillGrid()
        Dim selected As String = SelectedKey()
        grd.BeginUpdate()
        Try
            grd.ClearRows()
            For Each p As DdfPartenerAsociat In _associated
                Dim r As KBotDataRow = grd.AddRow()
                r.Tag = p
                r(COL_CF) = p.CodFiscal
                r(COL_NAME) = p.NumePartener
                r(COL_ROLE) = RoleOf(p)
            Next
            grd.ClearDirty()
        Finally
            grd.EndUpdate()
        End Try

        If selected.Length > 0 Then
            For i As Integer = 0 To _associated.Count - 1
                If String.Equals(DdfPartenerAsociat.Cheie(_associated(i).CodFiscal), selected, StringComparison.Ordinal) Then
                    grd.CurrentRowIndex = i
                    Exit For
                End If
            Next
        End If

        lblState.Text = If(_associated.Count = 0, "Niciun partener asociat.",
                           If(_associated.Count = 1, "1 partener asociat.", $"{_associated.Count} parteneri asociați."))
        UpdateRemoveEnabled()
    End Sub

    ' The picker lists the candidates that are NOT associated yet.
    Private Sub FillPicker()
        Dim taken As New HashSet(Of String)(_associated.Select(Function(p) DdfPartenerAsociat.Cheie(p.CodFiscal)),
                                            StringComparer.Ordinal)
        _offered.Clear()
        cmbPartner.BeginUpdate()
        Try
            cmbPartner.Items.Clear()
            For Each c As DdfPartener In _candidates
                If taken.Contains(DdfPartenerAsociat.Cheie(c.CodFiscal)) Then Continue For
                _offered.Add(c)
                cmbPartner.Items.Add($"{c.NumePartener} ({c.CodFiscal})")
            Next
            cmbPartner.SelectedIndex = -1
        Finally
            cmbPartner.EndUpdate()
        End Try
        btnAdd.Enabled = _offered.Count > 0
    End Sub

    Private Function SelectedKey() As String
        Dim p As DdfPartenerAsociat = SelectedPartner()
        Return If(p Is Nothing, String.Empty, DdfPartenerAsociat.Cheie(p.CodFiscal))
    End Function

    Private Function SelectedPartner() As DdfPartenerAsociat
        Dim i As Integer = grd.CurrentRowIndex
        If i < 0 OrElse i >= grd.RowCount Then Return Nothing
        Return TryCast(grd.Rows(i).Tag, DdfPartenerAsociat)
    End Function

    Private Sub UpdateRemoveEnabled()
        Dim p As DdfPartenerAsociat = SelectedPartner()
        btnRemove.Enabled = p IsNot Nothing AndAlso CanRemove(p)
    End Sub

    Private Sub Grd_SelectionChanged(sender As Object, e As EventArgs) Handles grd.SelectionChanged
        Try
            UpdateRemoveEnabled()
        Catch ex As Exception
            GlobalErrorLog.Write("DdfPartnersView.Grd_SelectionChanged", ex)
        End Try
    End Sub

    ' Boundary UI (button): the work is the host's, so a failure here can only be picking the row.
    Private Sub BtnAdd_Click(sender As Object, e As EventArgs) Handles btnAdd.Click
        Try
            cmbPartner.CommitText()
            Dim i As Integer = cmbPartner.SelectedIndex
            If i < 0 OrElse i >= _offered.Count Then
                KBotMessage.Show(Me, "Alege întâi un partener din listă.", "Parteneri asociați",
                                 MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            Dim c As DdfPartener = _offered(i)
            RaiseEvent AddRequested(Me, New DdfPartnerEventArgs(New DdfPartenerAsociat() With {
                .CodFiscal = c.CodFiscal, .NumePartener = c.NumePartener}))
        Catch ex As Exception
            GlobalErrorLog.Write("DdfPartnersView.BtnAdd_Click", ex)
        End Try
    End Sub

    Private Sub BtnRemove_Click(sender As Object, e As EventArgs) Handles btnRemove.Click
        Try
            Dim p As DdfPartenerAsociat = SelectedPartner()
            If p Is Nothing OrElse Not CanRemove(p) Then Return
            RaiseEvent RemoveRequested(Me, New DdfPartnerEventArgs(p))
        Catch ex As Exception
            GlobalErrorLog.Write("DdfPartnersView.BtnRemove_Click", ex)
        End Try
    End Sub

    ''' <summary>Required: this view owns child controls.</summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            BackColor = p.SurfaceAltColor
            tlyRoot.BackColor = p.SurfaceAltColor
            tlyPick.BackColor = p.SurfaceAltColor
            tlyBottom.BackColor = p.SurfaceAltColor
            lblPick.ForeColor = p.TextDimColor
            lblPick.BackColor = Color.Transparent
            lblState.ForeColor = p.TextDimColor
            lblState.BackColor = Color.Transparent
            ' The traversal does not carry the generic button rules into the children of an
            ' IThemedControl, so the house styles are applied by hand (as on every editor page).
            ButtonStyles.ApplyPrimary(btnAdd, scheme)
            ButtonStyles.ApplySecondary(btnRemove, scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfPartnersView.ApplyTheme", ex)
        End Try
    End Sub
End Class
