Option Strict On
Imports System.Globalization
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' «Sectiunea B» of the DDF editor (slice 0051) -- the port of <c>frmFX_DDF_REV_SECT_B</c>.
'''
''' <para><b>Never editable</b> (decision D8). Every value here is derived from section A:
''' <c>CA_Anterior</c> and <c>CB_Anterior</c> are the previous value, <c>Inf1</c> and
''' <c>Inf2</c> are the current one, <c>CA_Curent</c> and <c>CB_Curent</c> are the total. In
''' Access the grid could be edited in principle -- that is what
''' <c>Inf1_AfterUpdate</c> / <c>Inf2_BeforeUpdate</c> were for -- but in practice it was not,
''' and an override would let two numbers that must agree disagree. So those two handlers are
''' NOT ported, the list is rebuilt in full from section A on every change, and the server
''' writes what it receives.</para>
'''
''' <para>The page therefore holds no state of its own and raises
''' <c>DraftModificat</c> never: it only renders. The recomputation lives on
''' <c>DdfDraftRevizie.RecalculeazaSectiuneaB</c>, in the domain, where both this page and the
''' save path can reach the same one copy of it.</para>
'''
''' <para><c>CodSSI</c> is filled in by the SERVER (<c>CONCAT(SS, ClsfSal)</c> over
''' <c>Clasificatii</c>), so it is blank on a line the operator has only just added. That is
''' honest rather than a gap: the client cannot compute it, because <c>Clasificatii</c> has no
''' <c>CodSSI</c> column of its own.</para>
''' </summary>
Public Class DdfEditSectiuneaBPage
    Implements IDdfEditPage, IThemedControl

    ' The column keys. The columns themselves are declared in the .Designer.vb; these are only
    ' the names the cells are written through, and must stay identical to the designer's.
    Private Const COL_COD_ANGAJAMENT As String = "cod_angajament"
    Private Const COL_COD_INDICATOR As String = "cod_indicator"
    Private Const COL_COD_SSI As String = "cod_ssi"
    Private Const COL_CA_ANTERIOR As String = "ca_anterior"
    Private Const COL_INF1 As String = "inf1"
    Private Const COL_CA_CURENT As String = "ca_curent"
    Private Const COL_CB_ANTERIOR As String = "cb_anterior"
    Private Const COL_INF2 As String = "inf2"
    Private Const COL_CB_CURENT As String = "cb_curent"

    Private Shared ReadOnly _roCulture As New CultureInfo("ro-RO")

    Private _draft As DdfDraft

    ''' <summary>Declared because <see cref="IDdfEditPage"/> requires it. NEVER raised: this
    ''' page renders and does not edit, which is the whole point of decision D8.</summary>
    Public Event DraftModificat As EventHandler Implements IDdfEditPage.DraftModificat

    Public Sub New()
        InitializeComponent()
    End Sub

    Public ReadOnly Property PageKey As String Implements IDdfEditPage.PageKey
        Get
            Return "sectiunea-b"
        End Get
    End Property

    ''' <summary>
    ''' Renders the draft's section B.
    '''
    ''' <para>It is RECOMPUTED first. Reading whatever happens to be in the list would show a
    ''' stale twin of a section-A line that has since changed -- and the page is activated
    ''' precisely when the operator has come back from editing section A.</para>
    ''' </summary>
    Public Sub SetDraft(draft As DdfDraft) Implements IDdfEditPage.SetDraft
        Try
            _draft = draft
            grd.BeginUpdate()
            Try
                grd.ClearRows()
                If _draft Is Nothing Then
                    lblNota.Text = "Niciun document deschis."
                    Return
                End If

                _draft.Revizie.RecalculeazaSectiuneaB()

                Dim faraSsi As Integer = 0
                For Each b As DdfDraftLinieB In _draft.LiniiB
                    Dim r As KBotDataRow = grd.AddRow()
                    r.Tag = b
                    r(COL_COD_ANGAJAMENT) = b.CodAngajament
                    r(COL_COD_INDICATOR) = b.CodIndicator
                    r(COL_COD_SSI) = b.CodSsi
                    r(COL_CA_ANTERIOR) = b.CaAnterior
                    r(COL_INF1) = b.Inf1
                    r(COL_CA_CURENT) = b.CaCurent
                    r(COL_CB_ANTERIOR) = b.CbAnterior
                    r(COL_INF2) = b.Inf2
                    r(COL_CB_CURENT) = b.CbCurent
                    If String.IsNullOrWhiteSpace(b.CodSsi) Then faraSsi += 1
                Next
                grd.ClearDirty()

                lblNota.Text = MesajulDeStare(faraSsi)
            Finally
                grd.EndUpdate()
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditSectiuneaBPage.SetDraft", ex)
            Throw
        End Try
    End Sub

    ''' <summary>The line under the grid. It says why a blank Cod SSI is expected rather than
    ''' leaving the operator to wonder whether something failed.</summary>
    Private Shared Function MesajulDeStare(faraSsi As Integer) As String
        Dim baza As String = "Secțiunea B se calculează din secțiunea A și nu se editează."
        If faraSsi = 0 Then Return baza
        Return baza & $" {faraSsi} rânduri nu au încă Cod SSI: se completează pe server, la salvare."
    End Function

    ''' <summary>Required: this page owns child controls.</summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            BackColor = p.SurfaceAltColor
            tlyRoot.BackColor = p.SurfaceAltColor
            lblNota.ForeColor = p.TextDimColor
            lblNota.BackColor = Color.Transparent
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditSectiuneaBPage.ApplyTheme", ex)
        End Try
    End Sub
End Class
