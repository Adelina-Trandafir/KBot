Option Strict On
Imports System.Collections.Generic
Imports KBot.Common
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' «Parteneri» of the DDF editor (slice 0094-02): every partner associated with the document.
'''
''' <para><b>Two places hold a partner, on purpose.</b> <c>FX_DDF</c> has one pair of columns
''' for it, so the header combo on the form still picks the document's MAIN partner -- the one
''' written on every section-A / section-B line and on the signed PDF. This page holds the whole
''' set, main partner included, in <see cref="DdfDraft.Parteneri"/>; the form keeps the two in
''' step (<see cref="DdfDraft.SyncHeaderPartner"/>). Nothing here changes the header.</para>
'''
''' <para>The main partner is shown but cannot be taken out from here: it changes from the
''' header combo. Anybody else can be added and removed freely; the list is stored by the save,
''' with the rest of the document.</para>
'''
''' <para>Like every page it makes no request: the candidate partners are the form's header
''' combo source, handed down by <see cref="SetCandidates"/>.</para>
''' </summary>
Public Class DdfEditPartnersPage
    Implements IDdfEditPage, IThemedControl

    Private _draft As DdfDraft

    Public Event DraftModificat As EventHandler Implements IDdfEditPage.DraftModificat

    Public Sub New()
        InitializeComponent()
    End Sub

    Public ReadOnly Property PageKey As String Implements IDdfEditPage.PageKey
        Get
            Return "parteneri"
        End Get
    End Property

    ''' <summary>The partners the picker may offer. Called by the form when its list arrives, and
    ''' again when the page is created after that.</summary>
    Public Sub SetCandidates(candidates As IEnumerable(Of DdfPartener))
        Try
            vwPartners.SetCandidates(candidates)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditPartnersPage.SetCandidates", ex)
            Throw
        End Try
    End Sub

    Public Sub SetDraft(draft As DdfDraft) Implements IDdfEditPage.SetDraft
        Try
            _draft = draft
            vwPartners.SetAssociated(If(_draft Is Nothing, New List(Of DdfPartenerAsociat)(), _draft.Parteneri))
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditPartnersPage.SetDraft", ex)
            Throw
        End Try
    End Sub

    Private Sub VwPartners_AddRequested(sender As Object, e As DdfPartnerEventArgs) Handles vwPartners.AddRequested
        Try
            If _draft Is Nothing Then Return
            If Not _draft.AddAssociatedPartner(e.Partner.CodFiscal, e.Partner.NumePartener) Then
                KBotMessage.Show(Me, "Partenerul este deja asociat documentului.", "Parteneri asociați",
                                 MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            vwPartners.SetAssociated(_draft.Parteneri)
            RaiseEvent DraftModificat(Me, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditPartnersPage.VwPartners_AddRequested", ex)
        End Try
    End Sub

    Private Sub VwPartners_RemoveRequested(sender As Object, e As DdfPartnerEventArgs) Handles vwPartners.RemoveRequested
        Try
            If _draft Is Nothing Then Return
            If Not _draft.RemoveAssociatedPartner(e.Partner.CodFiscal) Then Return
            vwPartners.SetAssociated(_draft.Parteneri)
            RaiseEvent DraftModificat(Me, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditPartnersPage.VwPartners_RemoveRequested", ex)
        End Try
    End Sub

    ''' <summary>Required: this page owns child controls.</summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            BackColor = p.SurfaceAltColor
            tlyRoot.BackColor = p.SurfaceAltColor
            lblInfo.ForeColor = p.TextDimColor
            lblInfo.BackColor = Color.Transparent
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditPartnersPage.ApplyTheme", ex)
        End Try
    End Sub
End Class
