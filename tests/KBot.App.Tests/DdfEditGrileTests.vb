Option Strict On
Imports System
Imports System.Drawing
Imports System.Threading
Imports Xunit
Imports KBot.App
Imports KBot.Controls
Imports KBot.Domain

' Headless STA tests for the three DDF grids whose columns moved into the designer
' (DdfEditSectiuneaAPage, DdfEditSectiuneaBPage, DdfFileBrowser), plus the section-A lock.
'
' Two things are checked, and they are different things. First: the columns really do arrive
' from InitializeComponent -- a freshly built page has them before anything sets a draft, which
' is what "declared in the designer" has to mean. Second: section A is READ-ONLY for a document
' generated from FX_Rezervari, and editable otherwise.
Public Class DdfEditGrileTests

    Private Shared Sub RunSta(body As Action)
        Dim failure As Exception = Nothing
        Dim t As New Thread(Sub()
                                Try
                                    body()
                                Catch ex As Exception
                                    failure = ex
                                End Try
                            End Sub)
        t.SetApartmentState(ApartmentState.STA)
        t.Start()
        t.Join()
        If failure IsNot Nothing Then Throw failure
    End Sub

    Private Shared Function Chei(grd As KBotDataView) As String()
        Dim rezultat(grd.Columns.Count - 1) As String
        For i As Integer = 0 To grd.Columns.Count - 1
            rezultat(i) = grd.Columns(i).Key
        Next
        Return rezultat
    End Function

    ' ── Columns from the designer ────────────────────────────────────────────────

    <Fact>
    Public Sub SectiuneaA_AreColoaneleDinDesigner()
        RunSta(Sub()
                   Using p As New DdfEditSectiuneaAPage()
                       Assert.Equal({"clasificatie", "clsf", "element_fund", "parametrii_fund",
                                     "cod_partener", "buget", "val_rec", "val_prec",
                                     "val_cur", "val_tot"}, Chei(p.grd))

                       ' The one combo, and the only column the operator types a number into.
                       Assert.Equal(KBotColumnType.Combo, p.grd.Column("clasificatie").ColumnType)
                       Assert.False(p.grd.Column("val_cur").ReadOnly)
                       Assert.Equal(KBotAggregate.Sum, p.grd.Column("val_cur").Aggregate)

                       ' Everything derived is read-only, whatever the draft says.
                       For Each cheie As String In {"clsf", "buget", "val_rec", "val_prec", "val_tot"}
                           Assert.True(p.grd.Column(cheie).ReadOnly, cheie)
                       Next

                       ' The money columns are numbers with two decimals, right-aligned.
                       For Each cheie As String In {"buget", "val_rec", "val_prec", "val_cur", "val_tot"}
                           Dim col As KBotDataColumn = p.grd.Column(cheie)
                           Assert.Equal(KBotValueType.Number, col.ValueType)
                           Assert.Equal(KBotFormat.Standard, col.Format)
                           Assert.Equal(2, col.DecimalPlaces)
                           Assert.Equal(ContentAlignment.MiddleRight, col.TextAlign)
                       Next

                       Assert.True(p.grd.FooterVisible)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub SectiuneaB_AreColoaneleDinDesigner()
        RunSta(Sub()
                   Using p As New DdfEditSectiuneaBPage()
                       Assert.Equal({"cod_angajament", "cod_indicator", "cod_ssi",
                                     "ca_anterior", "inf1", "ca_curent",
                                     "cb_anterior", "inf2", "cb_curent"}, Chei(p.grd))

                       ' Never editable (decision D8), grid-wide and column by column.
                       Assert.True(p.grd.ReadOnlyGrid)
                       For Each cheie As String In {"ca_anterior", "inf1", "ca_curent",
                                                    "cb_anterior", "inf2", "cb_curent"}
                           Assert.True(p.grd.Column(cheie).ReadOnly, cheie)
                       Next

                       ' Only the two influence columns carry the totals row, as in Access.
                       Assert.Equal(KBotAggregate.Sum, p.grd.Column("inf1").Aggregate)
                       Assert.Equal(KBotAggregate.Sum, p.grd.Column("inf2").Aggregate)
                       Assert.Equal(KBotAggregate.None, p.grd.Column("ca_curent").Aggregate)
                       Assert.True(p.grd.FooterVisible)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub FileBrowser_AreColoaneleDinDesigner()
        RunSta(Sub()
                   Using b As New DdfFileBrowser()
                       Assert.Equal({"folder", "name", "cual", "rev", "size", "mod"}, Chei(b.grid))
                       Assert.Equal(ContentAlignment.MiddleRight, b.grid.Column("size").TextAlign)
                       Assert.True(b.grid.ReadOnlyGrid)
                   End Using
               End Sub)
    End Sub

    ' ── The section-A lock ───────────────────────────────────────────────────────

    Private Shared Function Schita(sursa As String, grpIdrz As String) As DdfDraft
        Dim d As New DdfDraft() With {.CodAngajament = "A100", .Sursa = sursa, .PartAng = True}
        d.LiniiA.Add(New DdfDraftLinieA() With {
            .TempId = -1, .CodAngajament = "A100", .CodIndicator = "!AB1",
            .IdClsf = 7, .Clsf = "65.03.01.20", .ElementFund = "Test",
            .ValPrec = 100.0R, .ValCur = 50.0R, .ValTot = 150.0R,
            .GrpIdrz = grpIdrz})
        Return d
    End Function

    <Fact>
    Public Sub SectiuneaA_DinRezervari_EsteBlocata()
        RunSta(Sub()
                   Using p As New DdfEditSectiuneaAPage()
                       p.SetDraft(Schita("rezervari", String.Empty))

                       Assert.True(p.grd.ReadOnlyGrid)
                       Assert.False(p.btnAdauga.Enabled)
                       Assert.False(p.btnSterge.Enabled)
                       ' The lock is above the partner gate: the document HAS a partner here.
                       Assert.True(p.grd.Column("cod_partener").ReadOnly)
                       Assert.Contains("rezervări", p.lblStare.Text, StringComparison.Ordinal)
                       ' No chevron on a list that cannot be opened.
                       Assert.Equal(KBotColumnType.Text, p.grd.Column("clasificatie").ColumnType)
                       ' And the rows survived the type swap.
                       Assert.Equal(1, p.grd.RowCount)
                   End Using
               End Sub)
    End Sub

    ' A revision read back answers «existent» whatever it was generated from; the reservation
    ' ids on the lines are what survives the round trip, so they lock the grid too.
    <Fact>
    Public Sub SectiuneaA_RevizieExistentaCuRezervari_EsteBlocata()
        RunSta(Sub()
                   Using p As New DdfEditSectiuneaAPage()
                       p.SetDraft(Schita("existent", "41;42"))

                       Assert.True(p.grd.ReadOnlyGrid)
                       Assert.False(p.btnAdauga.Enabled)
                       Assert.False(p.btnSterge.Enabled)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub SectiuneaA_DinIstoric_RamaneEditabila()
        RunSta(Sub()
                   Using p As New DdfEditSectiuneaAPage()
                       p.SetDraft(Schita("istoric", String.Empty))

                       Assert.False(p.grd.ReadOnlyGrid)
                       Assert.True(p.btnSterge.Enabled)
                       ' The document has a partner, so the cell stays open.
                       Assert.False(p.grd.Column("cod_partener").ReadOnly)
                       Assert.Equal(KBotColumnType.Combo, p.grd.Column("clasificatie").ColumnType)
                       Assert.Equal(1, p.grd.RowCount)
                   End Using
               End Sub)
    End Sub
End Class
