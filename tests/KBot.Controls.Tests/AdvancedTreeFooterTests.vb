Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Threading
Imports System.Windows.Forms
Imports Xunit
Imports KBot.Controls

''' <summary>
''' SUBSOLUL arborelui: banda de jos (sora antetului) și butonul ei de strângere.
'''
''' Ce se ține fix aici: geometria butonului (o singură formulă, folosită și de desen și de
''' hit-test), contractul de strângere (lățimea la care se întoarce, evenimentul, refuzul de a
''' strânge fără buton), rezervarea de spațiu (subsolul nu e zonă de noduri) și calculul nodului
''' plutitor — totul FĂRĂ ecran, ca și la <c>KBotNavList</c>: fereastra e doar randare, decizia
''' «pentru cine, cât de desfășurat, unde» stă în funcții pure.
'''
''' Și, ca peste tot în felia asta: un arbore neatins nu are voie să scrie NICIO linie de subsol
''' în formularul gazdă.
''' </summary>
Public Class AdvancedTreeFooterTests

    Private Shared Sub RunSta(body As Action)
        Dim err As Exception = Nothing
        Dim t As New Thread(Sub()
                                Try
                                    body()
                                Catch ex As Exception
                                    err = ex
                                End Try
                            End Sub)
        t.SetApartmentState(ApartmentState.STA)
        t.Start()
        t.Join()
        If err IsNot Nothing Then Throw New Xunit.Sdk.XunitException(err.ToString())
    End Sub

    ' Un arbore cu subsol, buton de strângere și câteva noduri — punctul de plecare al
    ' majorității probelor de aici.
    Private Shared Function ArboreCuSubsol() As AdvancedTreeControl
        Dim tree As New AdvancedTreeControl() With {.Width = 300, .Height = 200}
        tree.FooterVisible = True
        tree.FooterHeight = 28
        tree.FooterCollapseButton = True
        Dim grup As AdvancedTreeControl.TreeItem =
            tree.AddItem("G1", "Grup unu — capitol bugetar", Nothing, pExpanded:=True)
        tree.AddItem("G1F1", "Indicator 1.1 — denumire lungă de articol bugetar", grup)
        tree.AddItem("G1F2", "Indicator 1.2", grup)
        Return tree
    End Function

    ''' <summary>
    ''' Punctul din mijlocul PRIMULUI rând. Arborele de probă n-are antet și n-are bandă de
    ''' căutare, deci zona de noduri începe chiar de sus, după marginea de vârf (5px).
    ''' </summary>
    Private Shared Function PePrimulNod(tree As AdvancedTreeControl) As Point
        Return New Point(20, 5 + tree.ItemHeight \ 2)
    End Function

    ' ── Serializare: subsolul nefolosit e invizibil pentru designer ──────────────
    <Fact>
    Public Sub Un_subsol_neatins_nu_scrie_nimic_in_designer()
        RunSta(Sub()
                   Using tree As New AdvancedTreeControl()
                       For Each nume As String In {"FooterIconSize", "FooterCaptionFont",
                                                   "FooterBackColor", "FooterForeColor",
                                                   "FooterCaptionBackColor", "FooterCaptionForeColor",
                                                   "FooterGradientEndColor"}
                           Dim pd As PropertyDescriptor = TypeDescriptor.GetProperties(tree)(nume)
                           Assert.NotNull(pd)
                           Assert.False(pd.ShouldSerializeValue(tree),
                                        $"«{nume}» nu ar trebui serializat pe un arbore neatins")
                       Next

                       ' Starea de strângere e stare de RULARE: serializată, ar îngheța
                       ' formularul strâns și s-ar bate cu Size-ul scris tot de designer.
                       Dim pdCollapsed As PropertyDescriptor = TypeDescriptor.GetProperties(tree)("Collapsed")
                       Assert.True(pdCollapsed Is Nothing OrElse Not pdCollapsed.IsBrowsable)
                   End Using
               End Sub)
    End Sub

    ' ── Geometria butonului ──────────────────────────────────────────────────────
    <Fact>
    Public Sub Butonul_de_strangere_exista_doar_cu_subsol_si_cu_comutatorul_pornit()
        RunSta(Sub()
                   Using tree As New AdvancedTreeControl() With {.Width = 300, .Height = 200}
                       Assert.True(tree.FooterCollapseButtonRect.IsEmpty)   ' fără subsol

                       tree.FooterVisible = True
                       Assert.True(tree.FooterCollapseButtonRect.IsEmpty)   ' subsol, dar fără buton

                       tree.FooterCollapseButton = True
                       Assert.False(tree.FooterCollapseButtonRect.IsEmpty)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Butonul_sta_in_banda_de_subsol_pe_latura_ceruta()
        RunSta(Sub()
                   Using tree As AdvancedTreeControl = ArboreCuSubsol()
                       Dim banda As New Rectangle(0, tree.Height - tree.FooterHeight,
                                                  tree.Width, tree.FooterHeight)

                       ' Implicit: dreapta.
                       Dim r As Rectangle = tree.FooterCollapseButtonRect
                       Assert.True(banda.Contains(r), "butonul trebuie să încapă în banda de subsol")
                       Assert.True(r.Right > tree.Width \ 2, "implicit butonul stă în dreapta")

                       tree.FooterCollapseButtonPosition = AdvancedTreeControl.En_FooterButtonPosition.Left
                       r = tree.FooterCollapseButtonRect
                       Assert.True(banda.Contains(r))
                       Assert.True(r.Left < tree.Width \ 2, "pe Left butonul stă în stânga")
                   End Using
               End Sub)
    End Sub

    ''' <summary>Un subsol scund nu poate găzdui un buton mai înalt decât el.</summary>
    <Fact>
    Public Sub Butonul_nu_depaseste_inaltimea_benzii()
        RunSta(Sub()
                   Using tree As AdvancedTreeControl = ArboreCuSubsol()
                       tree.FooterHeight = 18
                       tree.FooterCollapseButtonSize = 40
                       Dim r As Rectangle = tree.FooterCollapseButtonRect
                       Assert.True(r.Height <= tree.FooterHeight)
                       Assert.True(r.Top >= tree.Height - tree.FooterHeight)
                       Assert.True(r.Bottom <= tree.Height)
                   End Using
               End Sub)
    End Sub

    ' ── Strângerea ───────────────────────────────────────────────────────────────
    <Fact>
    Public Sub Strangerea_duce_la_MinimumCollapsedWidth_si_se_intoarce_la_latimea_desfasurata()
        RunSta(Sub()
                   Using tree As AdvancedTreeControl = ArboreCuSubsol()
                       Assert.Equal(100, tree.MinimumCollapsedWidth)   ' implicitul cerut
                       Assert.Equal(300, tree.Width)

                       tree.ToggleCollapse()
                       Assert.True(tree.Collapsed)
                       Assert.Equal(100, tree.Width)

                       tree.ToggleCollapse()
                       Assert.False(tree.Collapsed)
                       Assert.Equal(300, tree.Width)
                   End Using
               End Sub)
    End Sub

    ''' <summary>
    ''' Lățimea desfășurată e ULTIMA lățime avută desfășurat, nu cea din constructor: operatorul
    ''' poate lăți arborele înainte să-l strângă. Iar redimensionările făcute de strângerea însăși
    ''' nu contează — altfel prima strângere ar deveni noua «lățime desfășurată».
    ''' </summary>
    <Fact>
    Public Sub Latimea_de_intoarcere_e_ultima_latime_desfasurata()
        RunSta(Sub()
                   Using tree As AdvancedTreeControl = ArboreCuSubsol()
                       tree.Width = 420
                       tree.ToggleCollapse()
                       Assert.Equal(100, tree.Width)
                       tree.ToggleCollapse()
                       Assert.Equal(420, tree.Width)
                   End Using
               End Sub)
    End Sub

    ''' <summary>
    ''' Un arbore a cărui lățime o dă gazda (orice <c>Dock</c>, sau ancorare pe amândouă laturile)
    ''' NU-și scrie singur <c>Width</c>: layout-ul formularului i l-ar da înapoi la următoarea
    ''' trecere. Exact asta s-a văzut în `MainForm` (arbore `Dock=Fill` în `split.Panel1`) —
    ''' se strângea și se desfăcea instantaneu. Starea se schimbă și evenimentul pleacă; mutarea
    ''' e a gazdei.
    ''' </summary>
    <Fact>
    Public Sub Cand_gazda_tine_latimea_arborele_nu_se_bate_cu_layout_ul()
        RunSta(Sub()
                   Using gazda As New Form() With {.Width = 500, .Height = 400}
                       Using tree As AdvancedTreeControl = ArboreCuSubsol()
                           gazda.Controls.Add(tree)
                           tree.Dock = DockStyle.Fill
                           Assert.True(tree.HostOwnsWidth)

                           Dim latimeInainte As Integer = tree.Width
                           Dim anuntat As Boolean = False
                           AddHandler tree.CollapsedChanged, Sub(c As Boolean) anuntat = True

                           tree.ToggleCollapse()

                           Assert.True(tree.Collapsed)          ' starea s-a schimbat
                           Assert.True(anuntat)                 ' gazda a fost anunțată
                           Assert.Equal(latimeInainte, tree.Width)   ' dar lățimea NU s-a atins
                           ' …iar gazda știe unde să se întoarcă.
                           Assert.Equal(latimeInainte, tree.ExpandedWidth)
                       End Using
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub HostOwnsWidth_urmareste_Dock_ul_si_ancorarea_pe_ambele_laturi()
        RunSta(Sub()
                   Using tree As New AdvancedTreeControl() With {.Width = 300, .Height = 200}
                       Assert.False(tree.HostOwnsWidth)                      ' liber

                       tree.Anchor = AnchorStyles.Top Or AnchorStyles.Left
                       Assert.False(tree.HostOwnsWidth)                      ' ancorat într-un colț

                       tree.Anchor = AnchorStyles.Left Or AnchorStyles.Right
                       Assert.True(tree.HostOwnsWidth)                       ' întins între margini

                       tree.Anchor = AnchorStyles.Top Or AnchorStyles.Left
                       tree.Dock = DockStyle.Left
                       Assert.True(tree.HostOwnsWidth)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Strangerea_anunta_gazda()
        RunSta(Sub()
                   Using tree As AdvancedTreeControl = ArboreCuSubsol()
                       Dim anunturi As New List(Of Boolean)()
                       AddHandler tree.CollapsedChanged, Sub(c As Boolean) anunturi.Add(c)

                       tree.ToggleCollapse()
                       tree.ToggleCollapse()
                       Assert.Equal(New Boolean() {True, False}, anunturi.ToArray())

                       ' Aceeași stare cerută de două ori nu ridică nimic.
                       tree.Collapsed = False
                       Assert.Equal(2, anunturi.Count)
                   End Using
               End Sub)
    End Sub

    ''' <summary>
    ''' Fără buton nu există cale de întoarcere, deci nici drum de dus: setterul aruncă (regula
    ''' casei — niciun no-op tăcut), iar ToggleCollapse, care e apăsarea unui buton inexistent,
    ''' pur și simplu nu face nimic.
    ''' </summary>
    <Fact>
    Public Sub Fara_buton_arborele_nu_se_strange()
        RunSta(Sub()
                   Using tree As New AdvancedTreeControl() With {.Width = 300, .Height = 200}
                       tree.FooterVisible = True
                       Assert.Throws(Of InvalidOperationException)(Sub() tree.Collapsed = True)

                       tree.ToggleCollapse()
                       Assert.False(tree.Collapsed)
                       Assert.Equal(300, tree.Width)
                   End Using
               End Sub)
    End Sub

    ''' <summary>Stingerea butonului cât arborele e strâns îl desface — altfel ar rămâne îngust pe veci.</summary>
    <Fact>
    Public Sub Stingerea_butonului_desface_arborele_strans()
        RunSta(Sub()
                   Using tree As AdvancedTreeControl = ArboreCuSubsol()
                       tree.ToggleCollapse()
                       Assert.True(tree.Collapsed)

                       tree.FooterCollapseButton = False
                       Assert.False(tree.Collapsed)
                       Assert.Equal(300, tree.Width)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Clic_pe_buton_comuta_iar_clic_in_banda_nu_ajunge_la_noduri()
        RunSta(Sub()
                   Using tree As AdvancedTreeControl = ArboreCuSubsol()
                       Dim r As Rectangle = tree.FooterCollapseButtonRect
                       Assert.True(tree.HandleFooterMouseDown(New Point(r.Left + r.Width \ 2,
                                                                        r.Top + r.Height \ 2)))
                       Assert.True(tree.Collapsed)

                       ' Restul benzii e tot al subsolului: consumat, dar fără efect.
                       Assert.True(tree.HandleFooterMouseDown(New Point(2, tree.Height - 2)))
                       Assert.True(tree.Collapsed)

                       ' Deasupra benzii nu mai e treaba subsolului.
                       Assert.False(tree.HandleFooterMouseDown(New Point(2, 10)))
                   End Using
               End Sub)
    End Sub

    ' ── Rezervarea de spațiu ─────────────────────────────────────────────────────
    ''' <summary>
    ''' Subsolul ia din zona de noduri: bara de derulare se oprește deasupra lui (altfel săgeata
    ''' ei de jos ar cădea peste butonul de strângere).
    ''' </summary>
    <Fact>
    Public Sub Subsolul_scurteaza_zona_de_noduri()
        RunSta(Sub()
                   Using tree As New AdvancedTreeControl() With {.Width = 300, .Height = 200}
                       Assert.Equal(0, tree.FooterOffset)
                       tree.FooterVisible = True
                       tree.FooterHeight = 30
                       Assert.Equal(30, tree.FooterOffset)
                       tree.FooterVisible = False
                       Assert.Equal(0, tree.FooterOffset)
                   End Using
               End Sub)
    End Sub

    ' ── Nodul plutitor al arborelui strâns ───────────────────────────────────────
    <Fact>
    Public Sub Eticheta_iese_doar_cat_arborele_e_strans_si_doar_daca_e_activata()
        RunSta(Sub()
                   Using tree As AdvancedTreeControl = ArboreCuSubsol()
                       Dim peNod As Point = PePrimulNod(tree)

                       ' Desfășurat: nimic — rândurile se văd întregi.
                       Assert.Null(tree.CollapsedFlyoutTargetAt(peNod))

                       tree.ToggleCollapse()
                       Assert.NotNull(tree.CollapsedFlyoutTargetAt(peNod))

                       tree.CollapsedFlyout = False
                       Assert.Null(tree.CollapsedFlyoutTargetAt(peNod))
                   End Using
               End Sub)
    End Sub

    ''' <summary>
    ''' Desfășurarea pleacă EXACT din rândul strâns (lățimea arborelui) și crește doar spre
    ''' dreapta, până la textul întreg. Asta e ce o face să pară nodul care se desface, nu o
    ''' etichetă lipită alături.
    ''' </summary>
    <Fact>
    Public Sub Desfasurarea_pleaca_din_randul_strans_si_creste_spre_dreapta()
        RunSta(Sub()
                   Using tree As AdvancedTreeControl = ArboreCuSubsol()
                       tree.FlyoutDelay = 0        ' fără așteptare: ținta iese pe loc
                       tree.FlyoutSlideDuration = 0
                       tree.ToggleCollapse()

                       Dim peNod As Point = PePrimulNod(tree)
                       Dim nod As AdvancedTreeControl.TreeItem = tree.CollapsedFlyoutTargetAt(peNod)
                       Assert.NotNull(nod)

                       Assert.False(tree.HandleFooterMouseMove(peNod))   ' nu e în bandă
                       Assert.Same(nod, tree.DebugFlyoutItem())
                       Assert.True(tree.DebugFlyoutFullWidth() > tree.Width,
                                   "textul întreg nu încape în arborele strâns, deci eticheta e mai lată")

                       Dim laInceput As Rectangle = tree.FlyoutClientBounds(nod, 0.0)
                       Dim laFinal As Rectangle = tree.FlyoutClientBounds(nod, 1.0)
                       Assert.Equal(0, laInceput.Left)
                       Assert.Equal(tree.Width, laInceput.Width)
                       Assert.Equal(laInceput.Left, laFinal.Left)          ' crește DOAR spre dreapta
                       Assert.Equal(laInceput.Top, laFinal.Top)
                       Assert.Equal(tree.ItemHeight, laFinal.Height)
                       Assert.Equal(tree.DebugFlyoutFullWidth(), laFinal.Width)
                   End Using
               End Sub)
    End Sub

    ''' <summary>
    ''' Nodul SELECTAT nu primește etichetă (implicit): operatorul l-a ales el, deci eticheta
    ''' n-are ce-i spune, dar ar acoperi vederea de alături de fiecare dată când cursorul trece pe
    ''' deasupra. Suprimarea e DOAR pentru rândul selectat — vecinii lui ies în continuare.
    ''' </summary>
    <Fact>
    Public Sub Nodul_selectat_nu_scoate_eticheta_dar_vecinii_lui_da()
        RunSta(Sub()
                   Using tree As AdvancedTreeControl = ArboreCuSubsol()
                       tree.ToggleCollapse()
                       Assert.False(tree.FlyoutSelectedNode)          ' implicitul cerut

                       Dim peNod As Point = PePrimulNod(tree)
                       Dim primul As AdvancedTreeControl.TreeItem = tree.CollapsedFlyoutTargetAt(peNod)
                       Assert.NotNull(primul)                          ' neselectat: iese

                       tree.SelectedNode = primul
                       Assert.Null(tree.CollapsedFlyoutTargetAt(peNod))   ' selectat: nu mai iese

                       ' …dar rândul de dedesubt, neselectat, iese ca înainte.
                       Dim peAlDoilea As New Point(peNod.X, peNod.Y + tree.ItemHeight)
                       Dim alDoilea As AdvancedTreeControl.TreeItem = tree.CollapsedFlyoutTargetAt(peAlDoilea)
                       Assert.NotNull(alDoilea)
                       Assert.NotSame(primul, alDoilea)
                   End Using
               End Sub)
    End Sub

    ''' <summary>Cine chiar vrea eticheta și pe nodul selectat o cere explicit.</summary>
    <Fact>
    Public Sub FlyoutSelectedNode_readuce_eticheta_pe_nodul_selectat()
        RunSta(Sub()
                   Using tree As AdvancedTreeControl = ArboreCuSubsol()
                       tree.ToggleCollapse()
                       Dim peNod As Point = PePrimulNod(tree)
                       tree.SelectedNode = tree.CollapsedFlyoutTargetAt(peNod)
                       Assert.Null(tree.CollapsedFlyoutTargetAt(peNod))

                       tree.FlyoutSelectedNode = True
                       Assert.NotNull(tree.CollapsedFlyoutTargetAt(peNod))
                   End Using
               End Sub)
    End Sub

    ''' <summary>
    ''' Cazul obișnuit: survolez un rând, iese eticheta, dau clic pe el. Din clipa în care e
    ''' selectat, eticheta n-are ce mai arăta — se retrage singură.
    ''' </summary>
    <Fact>
    Public Sub Selectarea_nodului_survolat_retrage_eticheta_deja_iesita()
        RunSta(Sub()
                   Using tree As AdvancedTreeControl = ArboreCuSubsol()
                       tree.FlyoutDelay = 0
                       tree.FlyoutSlideDuration = 0
                       tree.ToggleCollapse()

                       Dim peNod As Point = PePrimulNod(tree)
                       tree.HandleFooterMouseMove(peNod)
                       Dim iesit As AdvancedTreeControl.TreeItem = tree.DebugFlyoutItem()
                       Assert.NotNull(iesit)

                       tree.SelectedNode = iesit
                       Assert.Null(tree.DebugFlyoutItem())
                   End Using
               End Sub)
    End Sub

    ''' <summary>Și invers: stingerea opțiunii cât eticheta e afară peste nodul selectat o retrage.</summary>
    <Fact>
    Public Sub Stingerea_optiunii_retrage_eticheta_de_pe_nodul_selectat()
        RunSta(Sub()
                   Using tree As AdvancedTreeControl = ArboreCuSubsol()
                       tree.FlyoutDelay = 0
                       tree.FlyoutSlideDuration = 0
                       tree.FlyoutSelectedNode = True
                       tree.ToggleCollapse()

                       Dim peNod As Point = PePrimulNod(tree)
                       tree.SelectedNode = tree.CollapsedFlyoutTargetAt(peNod)
                       tree.HandleFooterMouseMove(peNod)
                       Assert.NotNull(tree.DebugFlyoutItem())      ' opțiunea o lasă să iasă

                       tree.FlyoutSelectedNode = False
                       Assert.Null(tree.DebugFlyoutItem())
                   End Using
               End Sub)
    End Sub

    ''' <summary>Cursorul intrat în banda de subsol stinge eticheta: acolo nu e niciun nod.</summary>
    <Fact>
    Public Sub Cursorul_in_subsol_stinge_eticheta()
        RunSta(Sub()
                   Using tree As AdvancedTreeControl = ArboreCuSubsol()
                       tree.FlyoutDelay = 0
                       tree.FlyoutSlideDuration = 0
                       tree.ToggleCollapse()

                       tree.HandleFooterMouseMove(PePrimulNod(tree))
                       Assert.NotNull(tree.DebugFlyoutItem())

                       Assert.True(tree.HandleFooterMouseMove(New Point(20, tree.Height - 4)))
                       Assert.Null(tree.DebugFlyoutItem())
                   End Using
               End Sub)
    End Sub

    ''' <summary>Desfășurarea arborelui retrage eticheta — nu mai are ce dezvălui.</summary>
    <Fact>
    Public Sub Desfasurarea_arborelui_retrage_eticheta()
        RunSta(Sub()
                   Using tree As AdvancedTreeControl = ArboreCuSubsol()
                       tree.FlyoutDelay = 0
                       tree.FlyoutSlideDuration = 0
                       tree.ToggleCollapse()
                       tree.HandleFooterMouseMove(PePrimulNod(tree))
                       Assert.NotNull(tree.DebugFlyoutItem())

                       tree.ToggleCollapse()
                       Assert.Null(tree.DebugFlyoutItem())
                   End Using
               End Sub)
    End Sub
End Class
