Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

' Playground AdvancedTreeControl: fiecare comutator din panoul stânga scrie într-o proprietate
' a arborelui și repictează live — perechea probei vizuale, la fel ca playground-ul KBotDataView.
' Arborele NU e IThemedControl (vezi CLAUDE.md): la comutarea temei îi împingem noi paleta,
' exact ca shell-ul. Toți handlerii sunt boundary UI: loghează și ÎNGHIT.
Public NotInheritable Class TreePlaygroundForm

    Private ReadOnly _log As Action(Of String)
    Private ReadOnly _originalScheme As ThemeScheme
    Private _loading As Boolean = True     ' cât e True, sincronizarea controale→arbore e suspendată

    ' Iconițe generate în cod (nu depindem de resurse) — eliberate în Dispose.
    Private _iconGrup As Bitmap
    Private _iconGrupOpen As Bitmap
    Private _iconFrunza As Bitmap
    Private _iconRight As Bitmap
    Private _iconSearch As Bitmap
    Private _iconHeaderRight As Bitmap
    Private _iconFooter As Bitmap
    Private _iconCollapseOpen As Bitmap
    Private _iconCollapseClosed As Bitmap

    ' Perechile (grupuri × frunze) din combo-ul de date.
    Private Shared ReadOnly _seturi As (Grupuri As Integer, Frunze As Integer)() = {
        (3, 4), (8, 12), (20, 30)
    }

    Public Sub New(log As Action(Of String))
        _log = log
        _originalScheme = ThemeManager.Current
        InitializeComponent()
        BuildIcons()
        FillImageList()
        PopulateCombos()
        SeedTree(0)
        SyncControls()
        _loading = False
        RefreshInfo()
    End Sub

    Private Sub OnClosedRestore(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        If _originalScheme IsNot Nothing AndAlso Not ReferenceEquals(ThemeManager.Current, _originalScheme) Then
            ThemeManager.SetScheme(_originalScheme)
        End If
    End Sub

    ' ── Iconițe + date ───────────────────────────────────────────────────────────
    Private Sub BuildIcons()
        _iconGrup = SolidIcon(16, Color.Goldenrod)
        _iconGrupOpen = SolidIcon(16, Color.Orange)
        _iconFrunza = SolidIcon(16, Color.SteelBlue)
        _iconRight = SolidIcon(16, Color.MediumSeaGreen)
        _iconSearch = SolidIcon(16, Color.DimGray)
        _iconHeaderRight = SolidIcon(16, Color.IndianRed)
        _iconFooter = SolidIcon(16, Color.MediumPurple)
        _iconCollapseOpen = SolidIcon(16, Color.SeaGreen)
        _iconCollapseClosed = SolidIcon(16, Color.Firebrick)
    End Sub

    Private Shared Function SolidIcon(latura As Integer, culoare As Color) As Bitmap
        Dim bmp As New Bitmap(latura, latura)
        Using g As Graphics = Graphics.FromImage(bmp)
            g.Clear(culoare)
        End Using
        Return bmp
    End Function

    Private Sub PopulateCombos()
        cboSearchIn.Items.AddRange(New Object() {"Caption", "Tag", "Ambele"})
        cboSearchType.Items.AddRange(New Object() {"Conține", "Începe cu"})
        cboScrollTheme.Items.AddRange(New Object() {"Default", "Explorer", "DarkMode"})
        cboSearchMode.Items.AddRange(New Object() {"Arbore", "Listă plată"})
        cboHeaderStyle.Items.AddRange(New Object() {"Solid", "Degrade vertical", "Degrade orizontal"})
        cboFooterStyle.Items.AddRange(New Object() {"Solid", "Degrade vertical", "Degrade orizontal"})
        cboCollapsePos.Items.AddRange(New Object() {"Dreapta", "Stânga"})
        For Each a As ContentAlignment In [Enum].GetValues(GetType(ContentAlignment))
            cboHeaderAlign.Items.Add(a.ToString())
            cboFooterAlign.Items.Add(a.ToString())
        Next
        For Each s In _seturi
            cboNodeCount.Items.Add($"{s.Grupuri} grupuri × {s.Frunze} frunze")
        Next
    End Sub

    ' Alimentăm ImageList-ul cu aceleași pătrate colorate, sub CHEI — așa se probează
    ' NodeImages + cheile din TreeNodeDefinition fără resurse în proiect.
    Private Sub FillImageList()
        imgNoduri.Images.Add("grup", SolidIcon(16, Color.Goldenrod))
        imgNoduri.Images.Add("grup-deschis", SolidIcon(16, Color.Orange))
        imgNoduri.Images.Add("frunza", SolidIcon(16, Color.SteelBlue))
        imgNoduri.Images.Add("dreapta", SolidIcon(16, Color.MediumSeaGreen))
        tree.NodeImages = imgNoduri
    End Sub

    ' Reconstruiește arborele. Caption-ul frunzelor conține și un text lung, ca tooltip-ul
    ' „textul nu încape" să fie testabil fără a seta Tooltip explicit pe fiecare nod.
    Private Sub SeedTree(setIndex As Integer)
        Try
            Dim cfg = _seturi(Math.Max(0, Math.Min(setIndex, _seturi.Length - 1)))
            tree.Clear()
            For gi As Integer = 1 To cfg.Grupuri
                Dim grup As AdvancedTreeControl.TreeItem =
                    tree.AddItem($"G{gi}", $"Grup {gi} — capitol bugetar",
                                 Nothing, _iconGrup, _iconGrupOpen, Nothing,
                                 $"grup-{gi}", pExpanded:=(gi = 1))
                grup.HasCheckBox = True
                grup.Tooltip = $"Grupul {gi}: {cfg.Frunze} poziții"
                For fi As Integer = 1 To cfg.Frunze
                    Dim frunza As AdvancedTreeControl.TreeItem =
                        tree.AddItem($"G{gi}F{fi}",
                                     $"Indicator {gi}.{fi} — denumire lungă de articol bugetar",
                                     grup, _iconFrunza, _iconFrunza, _iconRight,
                                     $"cod-{gi}-{fi}")
                    frunza.HasCheckBox = True
                    frunza.Tooltip = $"Nod {gi}.{fi} (tag: cod-{gi}-{fi})"
                Next
            Next
            tree.Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("TreePlaygroundForm.SeedTree", ex)
        End Try
    End Sub

    ' ── Temă ─────────────────────────────────────────────────────────────────────
    Private Sub btnClassic_Click(sender As Object, e As EventArgs) Handles btnClassic.Click
        SwitchScheme(BuiltInSchemes.Classic())
    End Sub
    Private Sub btnDark_Click(sender As Object, e As EventArgs) Handles btnDark.Click
        SwitchScheme(BuiltInSchemes.Dark())
    End Sub
    Private Sub btnModern_Click(sender As Object, e As EventArgs) Handles btnModern.Click
        SwitchScheme(BuiltInSchemes.Modern())
    End Sub
    Private Sub SwitchScheme(scheme As ThemeScheme)
        Try
            ThemeManager.SetScheme(scheme)
            _log("temă → " & scheme.Name)
            RefreshInfo()
        Catch ex As Exception
            GlobalErrorLog.Write("TreePlaygroundForm.SwitchScheme", ex)
        End Try
    End Sub

    ' Arborele e IThemedControl: ThemeManager îl cheamă singur și NU mai recurge în copiii lui.
    ' Playground-ul nu-i mai împinge nicio culoare — exact ca MainForm după corectură. Culorile
    ' alese aici din butoanele de culoare rămân peste comutarea de temă; cele nefixate o urmează.

    ' ── Antet ────────────────────────────────────────────────────────────────────
    Private Sub chkHeaderVisible_CheckedChanged(sender As Object, e As EventArgs) Handles chkHeaderVisible.CheckedChanged
        Apply(Sub() tree.HeaderVisible = chkHeaderVisible.Checked)
    End Sub
    Private Sub txtHeaderCaption_TextChanged(sender As Object, e As EventArgs) Handles txtHeaderCaption.TextChanged
        Apply(Sub() tree.HeaderCaption = txtHeaderCaption.Text)
    End Sub
    Private Sub numHeaderHeight_ValueChanged(sender As Object, e As EventArgs) Handles numHeaderHeight.ValueChanged
        Apply(Sub() tree.HeaderHeight = CInt(numHeaderHeight.Value))
    End Sub
    Private Sub btnHeaderFont_Click(sender As Object, e As EventArgs) Handles btnHeaderFont.Click
        PickFont(tree.HeaderFont, Sub(f) tree.HeaderFont = f, "HeaderFont")
    End Sub
    Private Sub cboHeaderAlign_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboHeaderAlign.SelectedIndexChanged
        Apply(Sub()
                  Dim valori As Array = [Enum].GetValues(GetType(ContentAlignment))
                  tree.HeaderTextAlign = CType(valori.GetValue(cboHeaderAlign.SelectedIndex), ContentAlignment)
              End Sub)
    End Sub
    Private Sub cboHeaderStyle_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboHeaderStyle.SelectedIndexChanged
        Apply(Sub() tree.HeaderBackStyle =
                  CType(cboHeaderStyle.SelectedIndex, AdvancedTreeControl.En_HeaderBackStyle))
    End Sub
    Private Sub btnHeaderBack_Click(sender As Object, e As EventArgs) Handles btnHeaderBack.Click
        PickColor(tree.HeaderBackColor, Sub(c) tree.HeaderBackColor = c, "HeaderBackColor")
    End Sub
    Private Sub btnHeaderFore_Click(sender As Object, e As EventArgs) Handles btnHeaderFore.Click
        PickColor(tree.HeaderForeColor, Sub(c) tree.HeaderForeColor = c, "HeaderForeColor")
    End Sub
    Private Sub btnHeaderGradEnd_Click(sender As Object, e As EventArgs) Handles btnHeaderGradEnd.Click
        PickColor(tree.HeaderGradientEndColor, Sub(c) tree.HeaderGradientEndColor = c, "HeaderGradientEndColor")
    End Sub
    Private Sub chkHeaderLeftIcon_CheckedChanged(sender As Object, e As EventArgs) Handles chkHeaderLeftIcon.CheckedChanged
        Apply(Sub() tree.HeaderLeftIcon = If(chkHeaderLeftIcon.Checked, _iconGrup, Nothing))
    End Sub
    ' Cu iconiță de căutare în antet, banda NU mai e permanentă: iconița o deschide/închide.
    Private Sub chkHeaderSearchIcon_CheckedChanged(sender As Object, e As EventArgs) Handles chkHeaderSearchIcon.CheckedChanged
        Apply(Sub() tree.HeaderSearchIcon = If(chkHeaderSearchIcon.Checked, _iconSearch, Nothing))
    End Sub
    Private Sub chkHeaderRightIcon_CheckedChanged(sender As Object, e As EventArgs) Handles chkHeaderRightIcon.CheckedChanged
        Apply(Sub() tree.HeaderRightIcon = If(chkHeaderRightIcon.Checked, _iconHeaderRight, Nothing))
    End Sub
    Private Sub numHeaderIconSize_ValueChanged(sender As Object, e As EventArgs) Handles numHeaderIconSize.ValueChanged
        Apply(Sub() tree.HeaderIconSize = Patrat(numHeaderIconSize))
    End Sub

    ' ── Subsol ───────────────────────────────────────────────────────────────────
    Private Sub chkFooterVisible_CheckedChanged(sender As Object, e As EventArgs) Handles chkFooterVisible.CheckedChanged
        Apply(Sub() tree.FooterVisible = chkFooterVisible.Checked)
    End Sub
    Private Sub txtFooterCaption_TextChanged(sender As Object, e As EventArgs) Handles txtFooterCaption.TextChanged
        Apply(Sub() tree.FooterCaption = txtFooterCaption.Text)
    End Sub
    Private Sub numFooterHeight_ValueChanged(sender As Object, e As EventArgs) Handles numFooterHeight.ValueChanged
        Apply(Sub() tree.FooterHeight = CInt(numFooterHeight.Value))
    End Sub
    Private Sub btnFooterFont_Click(sender As Object, e As EventArgs) Handles btnFooterFont.Click
        PickFont(tree.FooterCaptionFont, Sub(f) tree.FooterCaptionFont = f, "FooterCaptionFont")
    End Sub
    Private Sub cboFooterAlign_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboFooterAlign.SelectedIndexChanged
        Apply(Sub()
                  Dim valori As Array = [Enum].GetValues(GetType(ContentAlignment))
                  tree.FooterTextAlign = CType(valori.GetValue(cboFooterAlign.SelectedIndex), ContentAlignment)
              End Sub)
    End Sub
    Private Sub cboFooterStyle_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboFooterStyle.SelectedIndexChanged
        Apply(Sub() tree.FooterBackStyle =
                  CType(cboFooterStyle.SelectedIndex, AdvancedTreeControl.En_HeaderBackStyle))
    End Sub
    Private Sub btnFooterBack_Click(sender As Object, e As EventArgs) Handles btnFooterBack.Click
        PickColor(tree.FooterBackColor, Sub(c) tree.FooterBackColor = c, "FooterBackColor")
    End Sub
    Private Sub btnFooterFore_Click(sender As Object, e As EventArgs) Handles btnFooterFore.Click
        PickColor(tree.FooterForeColor, Sub(c) tree.FooterForeColor = c, "FooterForeColor")
    End Sub
    Private Sub btnFooterGradEnd_Click(sender As Object, e As EventArgs) Handles btnFooterGradEnd.Click
        PickColor(tree.FooterGradientEndColor, Sub(c) tree.FooterGradientEndColor = c, "FooterGradientEndColor")
    End Sub
    Private Sub btnFooterCapBack_Click(sender As Object, e As EventArgs) Handles btnFooterCapBack.Click
        PickColor(tree.FooterCaptionBackColor, Sub(c) tree.FooterCaptionBackColor = c, "FooterCaptionBackColor")
    End Sub
    Private Sub btnFooterCapFore_Click(sender As Object, e As EventArgs) Handles btnFooterCapFore.Click
        PickColor(tree.FooterCaptionForeColor, Sub(c) tree.FooterCaptionForeColor = c, "FooterCaptionForeColor")
    End Sub
    Private Sub chkFooterLeftIcon_CheckedChanged(sender As Object, e As EventArgs) Handles chkFooterLeftIcon.CheckedChanged
        Apply(Sub() tree.FooterLeftIcon = If(chkFooterLeftIcon.Checked, _iconFooter, Nothing))
    End Sub
    Private Sub numFooterIconSize_ValueChanged(sender As Object, e As EventArgs) Handles numFooterIconSize.ValueChanged
        Apply(Sub() tree.FooterIconSize = Patrat(numFooterIconSize))
    End Sub

    ' ── Subsol: strângere ────────────────────────────────────────────────────────
    Private Sub chkCollapseButton_CheckedChanged(sender As Object, e As EventArgs) Handles chkCollapseButton.CheckedChanged
        Apply(Sub() tree.FooterCollapseButton = chkCollapseButton.Checked)
    End Sub
    Private Sub numCollapseSize_ValueChanged(sender As Object, e As EventArgs) Handles numCollapseSize.ValueChanged
        Apply(Sub() tree.FooterCollapseButtonSize = CInt(numCollapseSize.Value))
    End Sub
    Private Sub cboCollapsePos_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboCollapsePos.SelectedIndexChanged
        Apply(Sub() tree.FooterCollapseButtonPosition =
                  CType(cboCollapsePos.SelectedIndex, AdvancedTreeControl.En_FooterButtonPosition))
    End Sub
    Private Sub chkCollapseImages_CheckedChanged(sender As Object, e As EventArgs) Handles chkCollapseImages.CheckedChanged
        Apply(Sub()
                  tree.FooterCollapseExpandedImage = If(chkCollapseImages.Checked, _iconCollapseOpen, Nothing)
                  tree.FooterCollapseCollapsedImage = If(chkCollapseImages.Checked, _iconCollapseClosed, Nothing)
              End Sub)
    End Sub
    Private Sub numMinCollapsed_ValueChanged(sender As Object, e As EventArgs) Handles numMinCollapsed.ValueChanged
        Apply(Sub() tree.MinimumCollapsedWidth = CInt(numMinCollapsed.Value))
    End Sub
    Private Sub chkCollapsedFlyout_CheckedChanged(sender As Object, e As EventArgs) Handles chkCollapsedFlyout.CheckedChanged
        Apply(Sub() tree.CollapsedFlyout = chkCollapsedFlyout.Checked)
    End Sub
    Private Sub chkFlyoutSelected_CheckedChanged(sender As Object, e As EventArgs) Handles chkFlyoutSelected.CheckedChanged
        Apply(Sub() tree.FlyoutSelectedNode = chkFlyoutSelected.Checked)
    End Sub
    Private Sub numFlyoutDelay_ValueChanged(sender As Object, e As EventArgs) Handles numFlyoutDelay.ValueChanged
        Apply(Sub() tree.FlyoutDelay = CInt(numFlyoutDelay.Value))
    End Sub
    Private Sub numFlyoutSlide_ValueChanged(sender As Object, e As EventArgs) Handles numFlyoutSlide.ValueChanged
        Apply(Sub() tree.FlyoutSlideDuration = CInt(numFlyoutSlide.Value))
    End Sub
    Private Sub btnToggleCollapse_Click(sender As Object, e As EventArgs) Handles btnToggleCollapse.Click
        Apply(Sub() tree.ToggleCollapse())
    End Sub

    ' Arborele anunță singur strângerea — exact cârligul pe care-l folosește o gazdă reală ca
    ' să-și mute splitter-ul. Aici doar îl scriem în jurnal, ca proba să fie vizibilă.
    Private Sub tree_CollapsedChanged(collapsed As Boolean) Handles tree.CollapsedChanged
        Try
            _log($"CollapsedChanged → {If(collapsed, "strâns", "desfășurat")} (lățime {tree.Width}px)")
            RefreshInfo()
        Catch ex As Exception
            GlobalErrorLog.Write("TreePlaygroundForm.tree_CollapsedChanged", ex)
        End Try
    End Sub

    ' ── Căutare ──────────────────────────────────────────────────────────────────
    Private Sub chkSearchShow_CheckedChanged(sender As Object, e As EventArgs) Handles chkSearchShow.CheckedChanged
        Apply(Sub() tree.SearchShow = chkSearchShow.Checked)
    End Sub
    Private Sub chkSearchClear_CheckedChanged(sender As Object, e As EventArgs) Handles chkSearchClear.CheckedChanged
        Apply(Sub() tree.SearchClearButton = chkSearchClear.Checked)
    End Sub
    Private Sub txtSearchLabel_TextChanged(sender As Object, e As EventArgs) Handles txtSearchLabel.TextChanged
        Apply(Sub() tree.SearchBarLabelText = txtSearchLabel.Text)
    End Sub
    Private Sub btnLabelFont_Click(sender As Object, e As EventArgs) Handles btnLabelFont.Click
        PickFont(tree.SearchBarLabelFont, Sub(f) tree.SearchBarLabelFont = f, "SearchBarLabelFont")
    End Sub
    Private Sub btnSearchFont_Click(sender As Object, e As EventArgs) Handles btnSearchFont.Click
        PickFont(tree.SearchBarFont, Sub(f) tree.SearchBarFont = f, "SearchBarFont")
    End Sub
    Private Sub txtPlaceholder_TextChanged(sender As Object, e As EventArgs) Handles txtPlaceholder.TextChanged
        Apply(Sub() tree.SearchDefaultText = txtPlaceholder.Text)
    End Sub
    Private Sub numClearPad_ValueChanged(sender As Object, e As EventArgs) Handles numClearPad.ValueChanged
        Apply(Sub() tree.SearchClearButtonPadding = New Padding(CInt(numClearPad.Value)))
    End Sub
    Private Sub chkClearImage_CheckedChanged(sender As Object, e As EventArgs) Handles chkClearImage.CheckedChanged
        Apply(Sub() tree.SearchClearButtonImage = If(chkClearImage.Checked, _iconRight, Nothing))
    End Sub
    Private Sub cboSearchIn_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboSearchIn.SelectedIndexChanged
        Apply(Sub() tree.SearchIn = CType(cboSearchIn.SelectedIndex, AdvancedTreeControl.En_Tree_SearchIn))
    End Sub
    Private Sub cboSearchType_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboSearchType.SelectedIndexChanged
        Apply(Sub() tree.SearchType = CType(cboSearchType.SelectedIndex, AdvancedTreeControl.En_Tree_SearchType))
    End Sub
    Private Sub btnSearchBack_Click(sender As Object, e As EventArgs) Handles btnSearchBack.Click
        PickColor(tree.SearchBackColor, Sub(c) tree.SearchBackColor = c, "SearchBackColor")
    End Sub
    Private Sub btnSearchBox_Click(sender As Object, e As EventArgs) Handles btnSearchBox.Click
        PickColor(tree.SearchBoxBackColor, Sub(c) tree.SearchBoxBackColor = c, "SearchBoxBackColor")
    End Sub
    Private Sub btnLabelFore_Click(sender As Object, e As EventArgs) Handles btnLabelFore.Click
        PickColor(tree.SearchBarLabelForeColor, Sub(c) tree.SearchBarLabelForeColor = c, "SearchBarLabelForeColor")
    End Sub
    Private Sub cboSearchMode_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboSearchMode.SelectedIndexChanged
        Apply(Sub() tree.SearchMode = CType(cboSearchMode.SelectedIndex, AdvancedTreeControl.En_Tree_SearchMode))
    End Sub

    ' Rezultatul căutării în banda de info — confirmă că filtrarea chiar rulează.
    Private Sub tree_SearchFinished(matchingItems As List(Of AdvancedTreeControl.TreeItem),
                                    searchText As String) Handles tree.SearchFinished
        Try
            lblInfo.Text = $"Căutare «{searchText}» → {matchingItems.Count} potriviri"
            _log($"căutare «{searchText}» → {matchingItems.Count} potriviri")
        Catch ex As Exception
            GlobalErrorLog.Write("TreePlaygroundForm.tree_SearchFinished", ex)
        End Try
    End Sub

    ' ── Arbore ───────────────────────────────────────────────────────────────────
    Private Sub numItemHeight_ValueChanged(sender As Object, e As EventArgs) Handles numItemHeight.ValueChanged
        Apply(Sub() tree.ItemHeight = CInt(numItemHeight.Value))
    End Sub
    Private Sub numIndent_ValueChanged(sender As Object, e As EventArgs) Handles numIndent.ValueChanged
        Apply(Sub() tree.Indent = CInt(numIndent.Value))
    End Sub
    Private Sub numExpander_ValueChanged(sender As Object, e As EventArgs) Handles numExpander.ValueChanged
        Apply(Sub() tree.ExpanderSize = CInt(numExpander.Value))
    End Sub
    Private Sub numCheckSize_ValueChanged(sender As Object, e As EventArgs) Handles numCheckSize.ValueChanged
        Apply(Sub() tree.CheckBoxSize = CInt(numCheckSize.Value))
    End Sub
    Private Sub chkCheckBoxes_CheckedChanged(sender As Object, e As EventArgs) Handles chkCheckBoxes.CheckedChanged
        Apply(Sub() tree.CheckBoxes = chkCheckBoxes.Checked)
    End Sub
    Private Sub chkRootExpander_CheckedChanged(sender As Object, e As EventArgs) Handles chkRootExpander.CheckedChanged
        Apply(Sub() tree.RootExpander = chkRootExpander.Checked)
    End Sub
    Private Sub chkNodeIcons_CheckedChanged(sender As Object, e As EventArgs) Handles chkNodeIcons.CheckedChanged
        Apply(Sub() tree.HasNodeIcons = chkNodeIcons.Checked)
    End Sub
    Private Sub numRadioLevel_ValueChanged(sender As Object, e As EventArgs) Handles numRadioLevel.ValueChanged
        Apply(Sub() tree.RadioButtonLevel = CInt(numRadioLevel.Value))
    End Sub
    Private Sub chkRightIconHover_CheckedChanged(sender As Object, e As EventArgs) Handles chkRightIconHover.CheckedChanged
        Apply(Sub() tree.ShowRightIconOnHover = chkRightIconHover.Checked)
    End Sub
    Private Sub chkReserveRight_CheckedChanged(sender As Object, e As EventArgs) Handles chkReserveRight.CheckedChanged
        Apply(Sub() tree.ReserveRightIconSpace = chkReserveRight.Checked)
    End Sub
    Private Sub numRightPad_ValueChanged(sender As Object, e As EventArgs) Handles numRightPad.ValueChanged
        Apply(Sub() tree.RightIconRightPadding = CInt(numRightPad.Value))
    End Sub
    Private Sub numLeftIconSize_ValueChanged(sender As Object, e As EventArgs) Handles numLeftIconSize.ValueChanged
        Apply(Sub() tree.LeftIconSize = Patrat(numLeftIconSize))
    End Sub
    Private Sub numRightIconSize_ValueChanged(sender As Object, e As EventArgs) Handles numRightIconSize.ValueChanged
        Apply(Sub() tree.RightIconSize = Patrat(numRightIconSize))
    End Sub
    ' Un singur font pentru arbore: Font. Îl desenează și nodurile, și tot el dă înălțimea rândului.
    Private Sub btnFont_Click(sender As Object, e As EventArgs) Handles btnFont.Click
        PickFont(tree.Font, Sub(f) tree.Font = f, "Font")
    End Sub
    Private Sub cboScrollTheme_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboScrollTheme.SelectedIndexChanged
        Apply(Sub() tree.ScrollBarTheme = CType(cboScrollTheme.SelectedIndex, AdvancedTreeControl.En_ScrollBarTheme))
    End Sub

    ' ── Culori ───────────────────────────────────────────────────────────────────
    Private Sub btnBackColor_Click(sender As Object, e As EventArgs) Handles btnBackColor.Click
        PickColor(tree.BackColor, Sub(c) tree.BackColor = c, "BackColor")
    End Sub
    Private Sub btnForeColor_Click(sender As Object, e As EventArgs) Handles btnForeColor.Click
        PickColor(tree.ForeColor, Sub(c) tree.ForeColor = c, "ForeColor")
    End Sub
    Private Sub btnHoverBack_Click(sender As Object, e As EventArgs) Handles btnHoverBack.Click
        PickColor(tree.HoverBackColor, Sub(c) tree.HoverBackColor = c, "HoverBackColor")
    End Sub
    Private Sub btnSelectedBack_Click(sender As Object, e As EventArgs) Handles btnSelectedBack.Click
        PickColor(tree.SelectedBackColor, Sub(c) tree.SelectedBackColor = c, "SelectedBackColor")
    End Sub
    Private Sub btnSelectedBorder_Click(sender As Object, e As EventArgs) Handles btnSelectedBorder.Click
        PickColor(tree.SelectedBorderColor, Sub(c) tree.SelectedBorderColor = c, "SelectedBorderColor")
    End Sub
    Private Sub btnLineColor_Click(sender As Object, e As EventArgs) Handles btnLineColor.Click
        PickColor(tree.LineColor, Sub(c) tree.LineColor = c, "LineColor")
    End Sub
    Private Sub btnBorderColor_Click(sender As Object, e As EventArgs) Handles btnBorderColor.Click
        PickColor(tree.BorderColor, Sub(c) tree.BorderColor = c, "BorderColor")
    End Sub

    ' ── Tooltip ──────────────────────────────────────────────────────────────────
    Private Sub chkTooltip_CheckedChanged(sender As Object, e As EventArgs) Handles chkTooltip.CheckedChanged
        Apply(Sub() tree.TooltipShow = chkTooltip.Checked)
    End Sub
    Private Sub chkTooltipIconOnly_CheckedChanged(sender As Object, e As EventArgs) Handles chkTooltipIconOnly.CheckedChanged
        Apply(Sub() tree.TooltipShowOnlyOnLeftIcon = chkTooltipIconOnly.Checked)
    End Sub
    Private Sub numTooltipDelay_ValueChanged(sender As Object, e As EventArgs) Handles numTooltipDelay.ValueChanged
        Apply(Sub() tree.TooltipDelayMs = CInt(numTooltipDelay.Value))
    End Sub
    Private Sub btnTooltipBack_Click(sender As Object, e As EventArgs) Handles btnTooltipBack.Click
        PickColor(tree.TooltipBackColor, Sub(c) tree.TooltipBackColor = c, "TooltipBackColor")
    End Sub
    Private Sub btnTooltipFore_Click(sender As Object, e As EventArgs) Handles btnTooltipFore.Click
        PickColor(tree.TooltipForeColor, Sub(c) tree.TooltipForeColor = c, "TooltipForeColor")
    End Sub

    ' ── Date ─────────────────────────────────────────────────────────────────────
    Private Sub cboNodeCount_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboNodeCount.SelectedIndexChanged
        Apply(Sub() SeedTree(cboNodeCount.SelectedIndex))
    End Sub
    Private Sub btnExpandAll_Click(sender As Object, e As EventArgs) Handles btnExpandAll.Click
        Apply(Sub() SetExpandedAll(True))
    End Sub
    Private Sub btnCollapseAll_Click(sender As Object, e As EventArgs) Handles btnCollapseAll.Click
        Apply(Sub() SetExpandedAll(False))
    End Sub

    ''' <summary>
    ''' Umple colecția de DESIGNER (tree.Nodes) și lasă arborele să se reconstruiască singur din
    ''' ea — aceeași cale pe care o ia InitializeComponent. Iconițele vin din NodeImages, prin chei.
    ''' </summary>
    Private Sub btnFromDefinitions_Click(sender As Object, e As EventArgs) Handles btnFromDefinitions.Click
        Apply(Sub()
                  tree.Nodes.Clear()
                  For gi As Integer = 1 To 3
                      tree.Nodes.Add(New TreeNodeDefinition($"D{gi}", $"Definiție {gi} — din designer") With {
                          .ImageKey = "grup",
                          .OpenImageKey = "grup-deschis",
                          .Expanded = True,
                          .HasCheckBox = True,
                          .Tooltip = $"Nod de designer {gi}"})
                      For fi As Integer = 1 To 3
                          tree.Nodes.Add(New TreeNodeDefinition($"D{gi}F{fi}", $"Frunză {gi}.{fi}") With {
                              .ParentKey = $"D{gi}",
                              .ImageKey = "frunza",
                              .RightImageKey = "dreapta",
                              .Tag = $"cod-{gi}-{fi}",
                              .HasCheckBox = True})
                      Next
                  Next
                  _log($"tree.Nodes = {tree.Nodes.Count} definiții → {CountNodes(tree.Items)} noduri vii")
              End Sub)
    End Sub

    ' ── Export ────────────────────────────────────────────────────────────────────
    ''' <summary>
    ''' Scoate combinația probată pe ecran ca LINII DE DESIGNER VB.NET: în clipboard (de lipit
    ''' direct în discuție) și într-un fișier de lângă executabil. Ce e omis și de ce — vezi
    ''' <see cref="TreeSettingsExporter"/>: culorile lăsate «din temă» NU pleacă în export.
    ''' </summary>
    Private Sub btnExport_Click(sender As Object, e As EventArgs) Handles btnExport.Click
        Try
            Dim continut As String = TreeSettingsExporter.Build(tree, ThemeManager.Current.Name)
            Dim cale As String = TreeSettingsExporter.Save(continut)

            ' Clipboard-ul poate fi ținut de alt proces: fișierul e deja scris, deci eșecul
            ' lui e o notă, nu o pierdere.
            Dim inClipboard As Boolean = True
            Try
                Clipboard.SetText(continut)
            Catch exClip As Exception
                inClipboard = False
                GlobalErrorLog.Write("TreePlaygroundForm.btnExport_Click/clipboard", exClip)
            End Try

            _log("export setări arbore → " & cale)
            lblInfo.Text = "Export scris în " & cale
            MessageBox.Show(Me,
                            If(inClipboard,
                               "Setările au fost copiate în clipboard și scrise în:",
                               "Clipboard-ul nu a putut fi scris. Setările sunt în fișier:") &
                            vbCrLf & vbCrLf & cale,
                            "Export setări arbore", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            GlobalErrorLog.Write("TreePlaygroundForm.btnExport_Click", ex)
            MessageBox.Show(Me, "Exportul a eșuat: " & ex.Message, "Export setări arbore",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub SetExpandedAll(expandat As Boolean)
        SetExpandedRecursive(tree.Items, expandat)
        tree.Invalidate()
    End Sub

    Private Shared Sub SetExpandedRecursive(noduri As List(Of AdvancedTreeControl.TreeItem),
                                            expandat As Boolean)
        For Each nod In noduri
            If nod.Children.Count > 0 Then
                nod.Expanded = expandat
                SetExpandedRecursive(nod.Children, expandat)
            End If
        Next
    End Sub

    ' ── Ajutoare ─────────────────────────────────────────────────────────────────
    ' Aplică o schimbare (suprimată în timpul încărcării), apoi reîmprospătează info-ul
    ' și activările dependente. Boundary UI: loghează și înghite.
    Private Sub Apply(action As Action)
        If _loading Then Return
        Try
            action()
            RefreshInfo()
            UpdateDependentControls()
        Catch ex As Exception
            GlobalErrorLog.Write("TreePlaygroundForm.Apply", ex)
        End Try
    End Sub

    Private Sub PickFont(curent As Font, seteaza As Action(Of Font), numeProp As String)
        Try
            Using dlg As New FontDialog() With {.Font = curent, .ShowEffects = True}
                If dlg.ShowDialog(Me) <> DialogResult.OK Then Return
                seteaza(dlg.Font)
                _log($"{numeProp} → {dlg.Font.Name} {dlg.Font.Size}pt {dlg.Font.Style}")
                RefreshInfo()
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("TreePlaygroundForm.PickFont", ex)
        End Try
    End Sub

    Private Sub PickColor(curenta As Color, seteaza As Action(Of Color), numeProp As String)
        Try
            Using dlg As New ColorDialog() With {.FullOpen = True,
                                                 .Color = If(curenta = Color.Empty, tree.BackColor, curenta)}
                If dlg.ShowDialog(Me) <> DialogResult.OK Then Return
                seteaza(dlg.Color)
                _log($"{numeProp} → {dlg.Color.Name}")
                RefreshInfo()
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("TreePlaygroundForm.PickColor", ex)
        End Try
    End Sub

    Private Sub SyncControls()
        chkHeaderVisible.Checked = tree.HeaderVisible
        txtHeaderCaption.Text = tree.HeaderCaption
        SetNum(numHeaderHeight, tree.HeaderHeight)
        cboHeaderAlign.SelectedIndex =
            Array.IndexOf([Enum].GetValues(GetType(ContentAlignment)), tree.HeaderTextAlign)
        cboHeaderStyle.SelectedIndex = CInt(tree.HeaderBackStyle)
        chkHeaderLeftIcon.Checked = tree.HeaderLeftIcon IsNot Nothing
        chkHeaderSearchIcon.Checked = tree.HeaderSearchIcon IsNot Nothing
        chkHeaderRightIcon.Checked = tree.HeaderRightIcon IsNot Nothing
        SetNum(numHeaderIconSize, tree.HeaderIconSize.Width)

        chkFooterVisible.Checked = tree.FooterVisible
        txtFooterCaption.Text = tree.FooterCaption
        SetNum(numFooterHeight, tree.FooterHeight)
        cboFooterAlign.SelectedIndex =
            Array.IndexOf([Enum].GetValues(GetType(ContentAlignment)), tree.FooterTextAlign)
        cboFooterStyle.SelectedIndex = CInt(tree.FooterBackStyle)
        chkFooterLeftIcon.Checked = tree.FooterLeftIcon IsNot Nothing
        SetNum(numFooterIconSize, tree.FooterIconSize.Width)

        chkCollapseButton.Checked = tree.FooterCollapseButton
        SetNum(numCollapseSize, tree.FooterCollapseButtonSize)
        cboCollapsePos.SelectedIndex = CInt(tree.FooterCollapseButtonPosition)
        chkCollapseImages.Checked = tree.FooterCollapseExpandedImage IsNot Nothing
        SetNum(numMinCollapsed, tree.MinimumCollapsedWidth)
        chkCollapsedFlyout.Checked = tree.CollapsedFlyout
        chkFlyoutSelected.Checked = tree.FlyoutSelectedNode
        SetNum(numFlyoutDelay, tree.FlyoutDelay)
        SetNum(numFlyoutSlide, tree.FlyoutSlideDuration)

        chkSearchShow.Checked = tree.SearchShow
        chkSearchClear.Checked = tree.SearchClearButton
        txtSearchLabel.Text = tree.SearchBarLabelText
        txtPlaceholder.Text = tree.SearchDefaultText
        SetNum(numClearPad, tree.SearchClearButtonPadding.All)
        chkClearImage.Checked = tree.SearchClearButtonImage IsNot Nothing
        cboSearchIn.SelectedIndex = CInt(tree.SearchIn)
        cboSearchType.SelectedIndex = CInt(tree.SearchType)
        cboSearchMode.SelectedIndex = CInt(tree.SearchMode)

        SetNum(numItemHeight, tree.ItemHeight)
        SetNum(numIndent, tree.Indent)
        SetNum(numExpander, tree.ExpanderSize)
        SetNum(numCheckSize, tree.CheckBoxSize)
        chkCheckBoxes.Checked = tree.CheckBoxes
        chkRootExpander.Checked = tree.RootExpander
        chkNodeIcons.Checked = tree.HasNodeIcons
        SetNum(numRadioLevel, tree.RadioButtonLevel)
        chkRightIconHover.Checked = tree.ShowRightIconOnHover
        chkReserveRight.Checked = tree.ReserveRightIconSpace
        SetNum(numRightPad, tree.RightIconRightPadding)
        SetNum(numLeftIconSize, tree.LeftIconSize.Width)
        SetNum(numRightIconSize, tree.RightIconSize.Width)
        cboScrollTheme.SelectedIndex = CInt(tree.ScrollBarTheme)

        chkTooltip.Checked = tree.TooltipShow
        chkTooltipIconOnly.Checked = tree.TooltipShowOnlyOnLeftIcon
        SetNum(numTooltipDelay, tree.TooltipDelayMs)

        cboNodeCount.SelectedIndex = 0
        UpdateDependentControls()
    End Sub

    ' Activează/dezactivează controalele care NU au efect în starea curentă, ca panoul să
    ' reflecte DOAR combinațiile valide.
    Private Sub UpdateDependentControls()
        ' Antetul ascuns înseamnă că nimic din el nu se vede (nici iconițele).
        Dim hdr As Boolean = tree.HeaderVisible
        lblHeaderCaption.Enabled = hdr
        txtHeaderCaption.Enabled = hdr
        lblHeaderHeight.Enabled = hdr
        numHeaderHeight.Enabled = hdr
        btnHeaderFont.Enabled = hdr
        lblHeaderAlign.Enabled = hdr
        cboHeaderAlign.Enabled = hdr
        lblHeaderStyle.Enabled = hdr
        cboHeaderStyle.Enabled = hdr
        btnHeaderBack.Enabled = hdr
        btnHeaderFore.Enabled = hdr
        chkHeaderLeftIcon.Enabled = hdr
        chkHeaderSearchIcon.Enabled = hdr
        chkHeaderRightIcon.Enabled = hdr
        lblHeaderIconSize.Enabled = hdr
        numHeaderIconSize.Enabled = hdr
        ' Capătul degradeului n-are ce colora pe fundal plin.
        btnHeaderGradEnd.Enabled = hdr AndAlso
            tree.HeaderBackStyle <> AdvancedTreeControl.En_HeaderBackStyle.Solid

        ' Subsolul ascuns înseamnă că nimic din el nu se vede — nici butonul de strângere.
        Dim ftr As Boolean = tree.FooterVisible
        For Each c As Control In New Control() {lblFooterCaption, txtFooterCaption, lblFooterHeight,
                                                numFooterHeight, btnFooterFont, lblFooterAlign,
                                                cboFooterAlign, lblFooterStyle, cboFooterStyle,
                                                btnFooterBack, btnFooterFore, btnFooterCapBack,
                                                btnFooterCapFore, chkCollapseButton}
            c.Enabled = ftr
        Next
        btnFooterGradEnd.Enabled = ftr AndAlso
            tree.FooterBackStyle <> AdvancedTreeControl.En_HeaderBackStyle.Solid
        ' Butonul pus în stânga îi ia locul iconiței — atunci nu mai are rost s-o alegi.
        Dim butonStanga As Boolean = tree.FooterCollapseButton AndAlso
            tree.FooterCollapseButtonPosition = AdvancedTreeControl.En_FooterButtonPosition.Left
        chkFooterLeftIcon.Enabled = ftr AndAlso Not butonStanga
        lblFooterIconSize.Enabled = chkFooterLeftIcon.Enabled AndAlso tree.FooterLeftIcon IsNot Nothing
        numFooterIconSize.Enabled = lblFooterIconSize.Enabled

        ' Restul secțiunii de strângere contează doar dacă butonul chiar există.
        Dim clps As Boolean = ftr AndAlso tree.FooterCollapseButton
        For Each c As Control In New Control() {lblCollapseSize, numCollapseSize, lblCollapsePos,
                                                cboCollapsePos, chkCollapseImages, lblMinCollapsed,
                                                numMinCollapsed, chkCollapsedFlyout, btnToggleCollapse}
            c.Enabled = clps
        Next
        ' Cronometrele și scutirea nodului selectat n-au ce guverna fără nodul plutitor.
        Dim fly As Boolean = clps AndAlso tree.CollapsedFlyout
        chkFlyoutSelected.Enabled = fly
        lblFlyoutDelay.Enabled = fly
        numFlyoutDelay.Enabled = fly
        lblFlyoutSlide.Enabled = fly
        numFlyoutSlide.Enabled = fly

        ' Restul benzii de căutare contează doar dacă banda poate apărea.
        Dim srch As Boolean = tree.SearchShow
        For Each c As Control In New Control() {chkSearchClear, lblSearchLabel, txtSearchLabel,
                                                btnLabelFont, btnSearchFont, lblPlaceholder,
                                                txtPlaceholder, lblSearchIn, cboSearchIn,
                                                lblSearchType, cboSearchType,
                                                btnSearchBack, btnSearchBox}
            c.Enabled = srch
        Next
        ' Padding-ul și imaginea contează doar dacă butonul ✕ chiar există.
        Dim clr As Boolean = srch AndAlso tree.SearchClearButton
        lblClearPad.Enabled = clr
        numClearPad.Enabled = clr
        chkClearImage.Enabled = clr

        ' RadioButtonLevel are prioritate față de CheckBoxes (vezi NodeHasCheckControl).
        chkCheckBoxes.Enabled = (tree.RadioButtonLevel < 0)
        ' Padding-ul iconiței din dreapta o POZIȚIONEAZĂ, deci contează oricând e desenată — nu
        ' doar când i se rezervă locul. (Era legat de ReserveRightIconSpace, ceea ce a devenit
        ' de-a dreptul greșit de când rezervarea nu mai e implicită.)
        ' Restrângerea tooltip-ului la icon e subordonată lui TooltipShow.
        chkTooltipIconOnly.Enabled = tree.TooltipShow
        lblTooltipDelay.Enabled = tree.TooltipShow
        numTooltipDelay.Enabled = tree.TooltipShow
    End Sub

    ' Iconițele arborelui sunt pătrate în toate probele: un singur numeric pe latură, nu două.
    Private Shared Function Patrat(n As NumericUpDown) As Size
        Dim latura As Integer = CInt(n.Value)
        Return New Size(latura, latura)
    End Function

    Private Shared Sub SetNum(n As NumericUpDown, value As Integer)
        Dim v As Decimal = value
        If v < n.Minimum Then v = n.Minimum
        If v > n.Maximum Then v = n.Maximum
        n.Value = v
    End Sub

    ' Rezumat live: câte noduri, starea benzii de căutare și modul ei (permanentă vs toggle).
    Private Sub RefreshInfo()
        Try
            Dim total As Integer = CountNodes(tree.Items)
            Dim modCautare As String
            If Not tree.SearchShow Then
                modCautare = "căutare OFF"
            ElseIf tree.HeaderSearchIcon IsNot Nothing Then
                modCautare = "căutare prin iconița din antet (toggle)"
            Else
                modCautare = "bandă de căutare permanentă"
            End If
            Dim stareArbore As String = If(tree.Collapsed,
                                           $"STRÂNS la {tree.Width}px", $"desfășurat ({tree.Width}px)")
            lblInfo.Text = $"{total} noduri • {modCautare} • {stareArbore} • temă {ThemeManager.Current.Name}"
        Catch ex As Exception
            GlobalErrorLog.Write("TreePlaygroundForm.RefreshInfo", ex)
        End Try
    End Sub

    Private Shared Function CountNodes(noduri As List(Of AdvancedTreeControl.TreeItem)) As Integer
        Dim n As Integer = 0
        For Each nod In noduri
            n += 1 + CountNodes(nod.Children)
        Next
        Return n
    End Function

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then
            _iconGrup?.Dispose() : _iconGrup = Nothing
            _iconGrupOpen?.Dispose() : _iconGrupOpen = Nothing
            _iconFrunza?.Dispose() : _iconFrunza = Nothing
            _iconRight?.Dispose() : _iconRight = Nothing
            _iconSearch?.Dispose() : _iconSearch = Nothing
            _iconHeaderRight?.Dispose() : _iconHeaderRight = Nothing
            _iconFooter?.Dispose() : _iconFooter = Nothing
            _iconCollapseOpen?.Dispose() : _iconCollapseOpen = Nothing
            _iconCollapseClosed?.Dispose() : _iconCollapseClosed = Nothing
        End If
        MyBase.Dispose(disposing)
    End Sub
End Class
