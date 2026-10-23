Imports System.Threading
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain

''' <summary>
''' The angajamente list in the middle column (slice 0086 split out of KbotForm.vb): loading
''' it from GET /api/forexe/tree, filling it, the node click that drives the views, and the
''' two header buttons that belong to it (hidden angajamente, internal info window). The
''' column / sort options live in KbotForm.TreeOptions.vb, the footer icons in
''' KbotForm.Extrase.vb and KbotForm.Download.vb.
''' </summary>
Partial Public Class KbotForm

    ' Slice 0101: the rows whose receptions do not close, kept so a theme switch can repaint them
    ' (the red is an explicit colour on the node, not «from the theme»).
    Private ReadOnly _nodLantNeinchis As New List(Of AdvancedTreeControl.TreeItem)()
    ' Slice 0114-26: the rows with reservations linked to a revision that does not cover them -- an error of a
    ' different kind, written in a different red (derived from the theme's error colour, see RosuAlternativ).
    Private ReadOnly _nodLegaturiGresite As New List(Of AdvancedTreeControl.TreeItem)()

    ''' <summary>
    ''' Loads the tree from GET /api/forexe/tree for the selected period (year + SS), through
    ''' WithReauth -- the same single re-login path on 401. The database is not sent: the
    ''' server takes it from the session (one database = one unit). Busy bar for the length of
    ''' the call; errors are shown to the operator in Romanian, never swallowed and never
    ''' masked by an empty tree.
    ''' </summary>
    ''' <param name="pastreazaSelectia">
    ''' True = the node selected now stays selected after the reload, and the open view gets
    ''' its context (slice 0060). Asked for AFTER a write on the current angajament: the figures
    ''' changed, but the operator is still there. Default False -- on a period change or on the
    ''' first load there is no selection to keep, and an old one would belong to another year.
    ''' </param>
    ''' <param name="codDeSelectat">Slice 0081-02: a code to select after the reload even if it was
    ''' not in the tree before (a K-BOT angajament just created, or one whose «!» code was replaced
    ''' by forexecab's). Wins over <paramref name="pastreazaSelectia"/>.</param>
    Private Async Function LoadTreeAsync(Optional pastreazaSelectia As Boolean = False,
                                         Optional codDeSelectat As String = Nothing) As Task
        ' No year/SS, no query to run (nothing chosen = periods not read).
        Dim anCheie As Integer? = AnulAles()
        Dim ssCheie As String = SsAles()
        If Not anCheie.HasValue OrElse String.IsNullOrEmpty(ssCheie) Then
            Return
        End If

        ' Slice 0008-02: a chosen group spans years and sources, so the tree then asks for every
        ' year (an = 0) and every source -- the group's angajamente would otherwise fall outside the
        ' year / SS of the title bar and the filtered tree would come out empty.
        Dim an As Integer = If(_grupaActiva Is Nothing, anCheie.Value, 0)
        capBar.SetSelectorShown(KBotCaptionBar.SelectorSector, SectorSeVede())
        ' Sorted by date the tree is a timeline of the whole year: every source, not only the
        ' SS in the bar's selector (operator, 23.09.2026 -- slice 0777).
        Dim ss As String = If(AppSettings.Current.TreeSortIsDate OrElse _grupaActiva IsNot Nothing,
                              ApiClient.TreeAllSources, ssCheie)
        busyBar.Running = True
        Try
            Dim ct As CancellationToken = CancellationToken.None
            Dim rows As IReadOnlyList(Of AngajamentTreeInfo) =
                Await WithReauth(Of IReadOnlyList(Of AngajamentTreeInfo))(
                    Function() _apiClient.GetTreeAsync(an, ss, _includeHidden, ct))
            ' The rows are kept: the tree options menu re-lays them without a fresh request.
            _treeRows = rows
            ' Read AFTER the request and BEFORE `PopulateTree` (which clears `_currentInfo`). Slice
            ' 0098: the reload after a queued refresh can take a while, and a node the operator
            ' clicked meanwhile is the one to keep -- read before, the reload put the old one back
            ' and undid the click.
            Dim codSelectat As String = If(pastreazaSelectia AndAlso _currentInfo IsNot Nothing,
                                           _currentInfo.CodAngajament, Nothing)
            If Not String.IsNullOrWhiteSpace(codDeSelectat) Then codSelectat = codDeSelectat
            PopulateTree(rows, codSelectat)
        Catch ex As Exception
            ' No silent net: an error (server down / 401 dead session / server defect after a
            ' re-login) is shown to the operator with the reason the server returned, not
            ' masked by an empty tree -- that would lie that the unit has no data.
            GlobalErrorLog.Write("MainForm.LoadTreeAsync", ex)
            KBotMessage.Show(Me,
                "Nu s-a putut încărca arborele de angajamente: " & ex.Message,
                "Angajamente", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            busyBar.Running = False
        End Try
    End Function

    ''' <summary>
    ''' Slice 0081-02: reloads the angajamente tree and selects <paramref name="cod"/> -- used when
    ''' the node did not exist before (a new K-BOT angajament) or changed its code (the send).
    ''' UI boundary (fire-and-forget from a Sub): LoadTreeAsync shows its own errors.
    ''' </summary>
    Private Async Sub ReincarcaArborelePe(cod As String)
        Try
            Await LoadTreeAsync(codDeSelectat:=cod).ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.ReincarcaArborelePe", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Clears and refills the list + the Cod -> info dictionary. Every row: caption =
    ''' Descriere (upper), CodAngajament cell, status icon (from Stare), refresh icon, bold when
    ''' it has sources (legacy behaviour), tooltip, Tag = Cod.
    ''' </summary>
    ''' <param name="codSelectat">
    ''' The angajament to re-select after the refill, or Nothing. A code that is no longer in
    ''' the tree is not an error: it behaves exactly like Nothing -- no view stays open on an
    ''' angajament that disappeared from the list.
    ''' </param>
    ' The tree rows by cod, rebuilt with _treeInfos.
    Private ReadOnly _treeNodes As New Dictionary(Of String, AdvancedTreeControl.TreeItem)(StringComparer.OrdinalIgnoreCase)

    Private Sub PopulateTree(rows As IReadOnlyList(Of AngajamentTreeInfo),
                             Optional codSelectat As String = Nothing)
        Try
            ArgumentNullException.ThrowIfNull(rows)
            tree.Clear()
            _treeInfos.Clear()
            _treeNodes.Clear()
            _treeOrder.Clear()
            _nodLantNeinchis.Clear()
            _nodLegaturiGresite.Clear()

            ' The view gate is applied ONCE, after the loop, when it is known whether the old
            ' node is still in the tree. Gating on Nothing up here, before the re-selection,
            ' used to hide every entry and push the nav onto «sumar» -- so a reload that KEPT
            ' the node still threw the operator off the view they were on (Rezervari,
            ' Receptii...) after every FOREXE refresh.

            Dim nodDeSelectat As AdvancedTreeControl.TreeItem = Nothing
            Dim infoDeSelectat As AngajamentTreeInfo = Nothing

            ' Slice 0008-02: a chosen group narrows the tree to its angajamente, captioned with the
            ' alias and written in the group's colour (KbotForm.Grupe.vb).
            Dim k_filtru As HashSet(Of String) = If(_grupaActiva Is Nothing, Nothing,
                New HashSet(Of String)(_grupaActiva.Coduri, StringComparer.OrdinalIgnoreCase))
            Dim k_culoareGrupa As Color = If(_grupaActiva Is Nothing, Color.Empty, GrupeUi.HexToColor(_grupaActiva.Culoare))

            For Each info As AngajamentTreeInfo In SortRows(rows)
                Dim cod As String = If(info.CodAngajament, String.Empty)
                If k_filtru IsNot Nothing AndAlso Not k_filtru.Contains(cod) Then Continue For
                Dim caption As String = If(k_filtru IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(info.AliasAng),
                                           info.AliasAng, If(info.Descriere, String.Empty)).Trim().ToUpperInvariant()

                Dim node As AdvancedTreeControl.TreeItem =
                    tree.AddItem("D_" & cod, caption,
                                 pLeftIconClosed:=FxIcons.StatusIcon(info.Stare),
                                 pRightIcon:=FxIcons.RefreshIcon(),
                                 pTag:=cod)

                ' Both cells are always written; which of them shows is the column set of the
                ' sort in force (slice 0777, ApplyTreeColumns) -- so a toggle needs no reload.
                node.Cells(COL_COD) = New AdvancedTreeControl.TreeItem.CellData With {.Value = cod}
                node.Cells(COL_SURSE) = New AdvancedTreeControl.TreeItem.CellData With {
                    .Value = FormatSurse(info.Surse)}
                node.Bold = info.AreIndicatori   ' legacy: bold = has sources (indicatori)
                node.Underline = _descarcateInSesiune.Contains(cod)   ' saved during this run
                node.Tooltip = TooltipTabel(info)   ' slice 0114-26: a table at 11 pt; TooltipFor is the old plain-text path
                If k_filtru IsNot Nothing Then node.NodeForeColor = k_culoareGrupa
                ' Slice 0101: a reception whose chain does not close paints the whole row red.
                ' Operator, 12.10.2026: so does a broken reservation chain (repaired from the Rezervari view).
                If info.LantNeinchis.Count > 0 OrElse info.RezervariGresite.Count > 0 Then
                    _nodLantNeinchis.Add(node)
                ElseIf info.LegaturiGresite.Count > 0 Then
                    ' Slice 0114-26 (operator, 23.10.2026): reservations linked to a revision that does not cover
                    ' them -- the same kind of warning, a different red. The first kind wins when a row has both.
                    _nodLegaturiGresite.Add(node)
                End If
                'node.ShowRightIconOnHover = True

                If Not _treeInfos.ContainsKey(cod) Then _treeOrder.Add(cod)
                _treeInfos(cod) = info
                _treeNodes(cod) = node   ' for SelectAngajament (KbotForm.Machete.vb)
                If codSelectat IsNot Nothing AndAlso
                   String.Equals(cod, codSelectat, StringComparison.OrdinalIgnoreCase) Then
                    nodDeSelectat = node
                    ' The info is taken FROM HERE, not through `_treeInfos(codSelectat)`: the
                    ' dictionary compares exactly, while the match above ignores case -- so a
                    ' key differing only in case would throw KeyNotFound.
                    infoDeSelectat = info
                End If
            Next

            ColoreazaLanturiNeinchise()

            ' The selection is put BACK at the end, with everything that hangs on it -- the
            ' view gate, the active view's context, the info window -- which is exactly what a
            ' click on the node does (`Tree_NodeMouseUp`). `SelectAndReveal`, not
            ' `SelectedNode`: written alone, the selection would be real and INVISIBLE, and the
            ' operator would see a list that jumped to the first row.
            If nodDeSelectat IsNot Nothing Then
                _currentInfo = infoDeSelectat
                tree.SelectAndReveal(nodDeSelectat)
                ' The nav key stays where it was: `ApplyViewGating` only falls back to «sumar»
                ' when the fresh flags no longer allow the open view.
                ApplyViewGating(infoDeSelectat)
                _activeView?.SetContext(infoDeSelectat)
            Else
                ' The old selection went away with the rows (or was not to be kept): no view
                ' stays open on an angajament that is no longer in the tree.
                _currentInfo = Nothing
                ApplyViewGating(Nothing)
            End If
            RefreshInfoForm()   ' the info window reflects the new selection (or its absence)

            tree.Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.PopulateTree", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Slice 0101: the error colour on every row collected in <c>_nodLantNeinchis</c>. Also run on a
    ''' theme switch, because the colour is set on the node and does not follow the theme by itself.
    ''' </summary>
    Private Sub ColoreazaLanturiNeinchise()
        Dim paleta As ThemePalette = ThemeManager.Current?.Palette
        If paleta Is Nothing Then Return
        For Each nod As AdvancedTreeControl.TreeItem In _nodLantNeinchis
            nod.NodeForeColor = paleta.ErrorColor
        Next
        Dim k_rosuAlt As Color = RosuAlternativ(paleta.ErrorColor)
        For Each nod As AdvancedTreeControl.TreeItem In _nodLegaturiGresite
            nod.NodeForeColor = k_rosuAlt
        Next
    End Sub

    ''' <summary>
    ''' Slice 0114-26: a second red for the second kind of error, taken from the theme's error colour by turning its
    ''' hue towards crimson (-22 degrees) and keeping its saturation and lightness -- so it follows every theme and
    ''' stays as readable as the first red, yet the two can be told apart at a glance.
    ''' </summary>
    Friend Shared Function RosuAlternativ(k_baza As Color) As Color
        Dim k_nuanta As Single = k_baza.GetHue() - 22.0F
        If k_nuanta < 0.0F Then k_nuanta += 360.0F
        Dim k_sat As Single = k_baza.GetSaturation()
        Dim k_luminozitate As Single = k_baza.GetBrightness()
        Dim k_chroma As Single = (1.0F - Math.Abs(2.0F * k_luminozitate - 1.0F)) * k_sat
        Dim k_sector As Single = k_nuanta / 60.0F
        Dim k_x As Single = k_chroma * (1.0F - Math.Abs(k_sector Mod 2.0F - 1.0F))
        Dim k_r As Single = 0.0F
        Dim k_g As Single = 0.0F
        Dim k_b As Single = 0.0F
        Select Case CInt(Math.Floor(k_sector)) Mod 6
            Case 0 : k_r = k_chroma : k_g = k_x
            Case 1 : k_r = k_x : k_g = k_chroma
            Case 2 : k_g = k_chroma : k_b = k_x
            Case 3 : k_g = k_x : k_b = k_chroma
            Case 4 : k_r = k_x : k_b = k_chroma
            Case Else : k_r = k_chroma : k_b = k_x
        End Select
        Dim k_m As Single = k_luminozitate - k_chroma / 2.0F
        Return Color.FromArgb(Canal(k_r + k_m), Canal(k_g + k_m), Canal(k_b + k_m))
    End Function

    Private Shared Function Canal(k_valoare As Single) As Integer
        Return Math.Max(0, Math.Min(255, CInt(Math.Round(k_valoare * 255.0F))))
    End Function

    ''' <summary>
    ''' The hover label of a tree row. Until 22.09.2026 it was the Descriere alone -- the very
    ''' text already written ON the row, in capitals -- so the operator had nothing to gain by
    ''' waiting for it, and it was reachable only over the 14 px status icon
    ''' (<c>TooltipShowOnlyOnLeftIcon</c>). Both halves are fixed: it now shows over the whole
    ''' row, and it says what the row cannot fit -- code, state, dates, sources.
    ''' </summary>
    ''' <remarks>
    ''' Plain lines, not the XML table the Receptii tree uses: there is one node per angajament
    ''' here, nothing to put in columns. Empty fields are left out rather than shown blank.
    ''' </remarks>
    Private Shared Function TooltipFor(info As AngajamentTreeInfo) As String
        If info Is Nothing Then Return String.Empty
        Try
            Dim linii As New List(Of String)()
            Dim descriere As String = If(info.Descriere, String.Empty).Trim()
            If descriere.Length > 0 Then linii.Add(descriere)

            Dim cod As String = If(info.CodAngajament, String.Empty).Trim()
            If cod.Length > 0 Then linii.Add("Cod: " & cod)

            Dim stare As String = If(info.Stare, String.Empty).Trim()
            If stare.Length > 0 Then linii.Add("Stare: " & stare)

            If info.DataCreare.HasValue Then
                linii.Add("Creat: " & info.DataCreare.Value.ToString("dd.MM.yyyy"))
            End If
            ' Slice 0777: the moment the date sort orders on, with its time part.
            If info.DataAngajamentNou.HasValue Then
                linii.Add("Angajament nou (FOREXE): " & info.DataAngajamentNou.Value.ToString("dd.MM.yyyy HH:mm:ss"))
            End If
            ' Slice 0100: the last saved download from FOREXE.
            If info.DataActualizare.HasValue Then
                linii.Add("Actualizat: " & info.DataActualizare.Value.ToString("dd.MM.yyyy HH:mm"))
            End If
            If info.DataDefinitivare.HasValue Then
                linii.Add("Definitivat: " & info.DataDefinitivare.Value.ToString("dd.MM.yyyy"))
            End If

            Dim surse As String = FormatSurse(info.Surse)
            If surse.Length > 0 Then linii.Add("Surse: " & surse)
            If info.Ascuns Then linii.Add("Ascuns")

            ' Slice 0101: what is wrong, one line per reception.
            If info.LantNeinchis.Count > 0 Then
                Dim ro As New Globalization.CultureInfo("ro-RO")
                linii.Add(String.Empty)
                linii.Add("⚠ Lanțul nu se închide: ultimul instantaneu nu are valoarea recepției.")
                For Each r As ReceptieNeinchisa In info.LantNeinchis
                    linii.Add($"Recepția din {r.DataReceptie:dd.MM.yyyy}: ultimul instantaneu " &
                              $"{r.TotalUltimInstantaneu.ToString("N2", ro)}, valoarea recepției " &
                              $"{r.ValoareReceptie.ToString("N2", ro)}")
                Next
            End If

            ' Operator, 12.10.2026: a reservation that does not start from the end value of the one
            ' before it. Both messages show when the angajament has both faults.
            If info.RezervariGresite.Count > 0 Then
                Const k_maxRanduri As Integer = 6
                Dim ro As New Globalization.CultureInfo("ro-RO")
                linii.Add(String.Empty)
                linii.Add("⚠ Rezervări greșite: valoarea anterioară nu e cea la care s-a oprit rezervarea dinainte. " &
                          "Repară din vederea «Rezervări».")
                For Each r As RezervareGresita In info.RezervariGresite.Take(k_maxRanduri)
                    linii.Add($"Rezervarea din {r.DataRezervare:dd.MM.yyyy}, {r.CodIndicator}: anterioară " &
                              $"{r.Anterioara.ToString("N2", ro)}, ar trebui {r.Asteptata.ToString("N2", ro)}")
                Next
                If info.RezervariGresite.Count > k_maxRanduri Then
                    linii.Add($"… și încă {info.RezervariGresite.Count - k_maxRanduri}")
                End If
            End If

            Return String.Join(vbLf, linii)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.TooltipFor", ex)
            Return If(info.Descriere, String.Empty)
        End Try
    End Function

    ' Slice 0114-26 (operator, 23.10.2026): the font of the table tooltip of this tree, in points. The plain-text tooltip
    ' above drew 8.5 pt in a pale grey on the popup's own back colour and could not be read; the table is the kind the
    ' Receptii tree uses (label | value rows under a header), at 11 pt.
    Private Const TOOLTIP_FONT_SIZE As String = "10"

    ''' <summary>
    ''' Slice 0114-26: the hover label of a tree row as a TABLE (the shape the Receptii tree uses, read by
    ''' <c>TooltipTableParser</c>): a header with the description, then label | value rows -- code, state, dates,
    ''' sources -- and, in the error colours of the theme, one block per warning. <see cref="TooltipFor"/> (plain
    ''' lines) is the way it was written before and is no longer called; it stays until the operator says it goes.
    ''' </summary>
    Private Shared Function TooltipTabel(k_info As AngajamentTreeInfo) As String
        If k_info Is Nothing Then Return String.Empty
        Try
            Dim k_paleta As ThemePalette = ThemeManager.Current?.Palette
            Dim k_rosu As String = If(k_paleta Is Nothing, Nothing, HexDin(k_paleta.ErrorColor))
            Dim k_rosuAlt As String = If(k_paleta Is Nothing, Nothing, HexDin(RosuAlternativ(k_paleta.ErrorColor)))
            Dim k_ro As New Globalization.CultureInfo("ro-RO")
            Dim k_sb As New System.Text.StringBuilder()

            k_sb.Append("<table FontSize=""").Append(TOOLTIP_FONT_SIZE).Append(""">")
            k_sb.Append("<header>")
            TtCelula(k_sb, "Angajament", Nothing, True)
            TtCelula(k_sb, If(k_info.Descriere, String.Empty).Trim(), Nothing, True)
            k_sb.Append("</header>")

            TtRand(k_sb, "Cod", If(k_info.CodAngajament, String.Empty).Trim(), Nothing)
            TtRand(k_sb, "Stare", If(k_info.Stare, String.Empty).Trim(), Nothing)
            If k_info.DataCreare.HasValue Then TtRand(k_sb, "Creat", k_info.DataCreare.Value.ToString("dd.MM.yyyy"), Nothing)
            ' Slice 0777: the moment the date sort orders on, with its time part.
            If k_info.DataAngajamentNou.HasValue Then
                TtRand(k_sb, "Angajament nou (FOREXE)", k_info.DataAngajamentNou.Value.ToString("dd.MM.yyyy HH:mm:ss"), Nothing)
            End If
            ' Slice 0100: the last saved download from FOREXE.
            If k_info.DataActualizare.HasValue Then TtRand(k_sb, "Actualizat", k_info.DataActualizare.Value.ToString("dd.MM.yyyy HH:mm"), Nothing)
            If k_info.DataDefinitivare.HasValue Then TtRand(k_sb, "Definitivat", k_info.DataDefinitivare.Value.ToString("dd.MM.yyyy"), Nothing)
            TtRand(k_sb, "Surse", FormatSurse(k_info.Surse), Nothing)
            If k_info.Ascuns Then TtRand(k_sb, "Ascuns", "da", Nothing)

            Const k_maxRanduri As Integer = 6

            ' Slice 0101: what is wrong with the receptions, one row per reception.
            If k_info.LantNeinchis.Count > 0 Then
                TtRand(k_sb, "⚠ Lanțul nu se închide", "ultimul instantaneu nu are valoarea recepției", k_rosu, True)
                For Each k_r As ReceptieNeinchisa In k_info.LantNeinchis
                    TtRand(k_sb, $"Recepția din {k_r.DataReceptie:dd.MM.yyyy}",
                           $"instantaneu {k_r.TotalUltimInstantaneu.ToString("N2", k_ro)}, recepție {k_r.ValoareReceptie.ToString("N2", k_ro)}",
                           k_rosu)
                Next
            End If

            ' Operator, 12.10.2026: a reservation that does not start from the end value of the one before it.
            If k_info.RezervariGresite.Count > 0 Then
                TtRand(k_sb, "⚠ Rezervări greșite", "valoarea anterioară nu e cea la care s-a oprit rezervarea dinainte", k_rosu, True)
                For Each k_r As RezervareGresita In k_info.RezervariGresite.Take(k_maxRanduri)
                    TtRand(k_sb, $"Rezervarea din {k_r.DataRezervare:dd.MM.yyyy}, {k_r.CodIndicator}",
                           $"anterioară {k_r.Anterioara.ToString("N2", k_ro)}, ar trebui {k_r.Asteptata.ToString("N2", k_ro)}", k_rosu)
                Next
                If k_info.RezervariGresite.Count > k_maxRanduri Then
                    TtRand(k_sb, $"… și încă {k_info.RezervariGresite.Count - k_maxRanduri}", String.Empty, k_rosu)
                End If
                TtRand(k_sb, "Se repară", "din vederea «Rezervări», butonul «Repară»", k_rosu)
            End If

            ' Slice 0114-26 (operator, 23.10.2026): reservations linked to a revision that does not cover them.
            If k_info.LegaturiGresite.Count > 0 Then
                TtRand(k_sb, "⚠ Rezervări legate greșit", "sunt legate de o revizie care nu le cuprinde, deci nu au «+»", k_rosuAlt, True)
                For Each k_r As RezervareLegataGresit In k_info.LegaturiGresite.Take(k_maxRanduri)
                    Dim k_zi As String = If(k_r.DataRevizie.HasValue, $" din {k_r.DataRevizie.Value:dd.MM.yyyy}", String.Empty)
                    TtRand(k_sb, $"Rezervarea din {k_r.DataRezervare:dd.MM.yyyy}, {k_r.CodIndicator}",
                           $"{k_r.Valoare.ToString("+#,##0.00;-#,##0.00;0.00", k_ro)}, legată de revizia {k_r.IdRevizie}{k_zi}", k_rosuAlt)
                Next
                If k_info.LegaturiGresite.Count > k_maxRanduri Then
                    TtRand(k_sb, $"… și încă {k_info.LegaturiGresite.Count - k_maxRanduri}", String.Empty, k_rosuAlt)
                End If
                TtRand(k_sb, "Se repară", "din vederea «Rezervări», butonul «Repară»", k_rosuAlt)
            End If

            k_sb.Append("</table>")
            Return k_sb.ToString()
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.TooltipTabel", ex)
            Return If(k_info.Descriere, String.Empty)
        End Try
    End Function

    ' One <row> of the table tooltip: label | value. A row whose value is empty is left out, unless it is a
    ' title of a block (k_titlu) or carries a colour of its own (the rest rows of a warning block).
    Private Shared Sub TtRand(k_sb As System.Text.StringBuilder, k_eticheta As String, k_valoare As String,
                              k_culoare As String, Optional k_titlu As Boolean = False)
        If String.IsNullOrWhiteSpace(k_valoare) AndAlso Not k_titlu AndAlso k_culoare Is Nothing Then Return
        k_sb.Append("<row>")
        TtCelula(k_sb, k_eticheta, k_culoare, k_titlu)
        TtCelula(k_sb, k_valoare, k_culoare, False)
        k_sb.Append("</row>")
    End Sub

    Private Shared Sub TtCelula(k_sb As System.Text.StringBuilder, k_text As String, k_culoare As String, k_gros As Boolean)
        k_sb.Append("<cell Align=""left""")
        If k_gros Then k_sb.Append(" Bold=""1""")
        If Not String.IsNullOrEmpty(k_culoare) Then k_sb.Append(" ForeColor=""").Append(k_culoare).Append(""""c)
        k_sb.Append(">"c).Append(System.Security.SecurityElement.Escape(If(k_text, String.Empty))).Append("</cell>")
    End Sub

    ' "#RRGGBB" for the table tooltip's ForeColor (the parser takes this form).
    Private Shared Function HexDin(k_culoare As Color) As String
        Return $"#{k_culoare.R:X2}{k_culoare.G:X2}{k_culoare.B:X2}"
    End Function

    ' The list selection pushes the context (AngajamentTreeInfo) to the active view.
    Private Sub Tree_NodeMouseUp(pNode As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles tree.NodeMouseUp
        Try
            Dim info As AngajamentTreeInfo = Nothing
            Dim cod As String = If(pNode Is Nothing, Nothing, TryCast(pNode.Tag, String))
            If cod IsNot Nothing Then
                _treeInfos.TryGetValue(cod, info)
            End If
            _currentInfo = info
            ' Slice 000T: a tutorial waiting for «an angajament that has reservations» hears it here.
            If info IsNot Nothing AndAlso info.AreRezervari Then RaiseEvent TutorialSignal(SignalAngajamentCuRezervari)
            ' The node's flags decide which views are reachable (the Are* gate).
            ApplyViewGating(info)
            _activeView?.SetContext(info)
            ' An operator CLICK with the «Browser FOREXE» view open sends the robot after the
            ' node (slice 0074) - only a click, never the reload-driven SetContext above.
            TryCast(_activeView, BrowserView)?.DeschideSelectia()
            RefreshInfoForm()   ' the «Informatii interne» window, when open
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.tree_NodeMouseUp", ex)
        End Try
    End Sub

    ' NOTE (slice 0034): collapsing the tree LEFT the shell -- the footer button, the
    ' tree_CollapsedChanged handler and ClampSplitter were deleted, and the left corner of the
    ' footer became the statements button. The trees in the VIEWS keep their collapse; this
    ' one was only the shell's.

    ''' <summary>
    ''' The tree options (Access bOpt). Toggles showing the hidden angajamente (ASCUNS) and
    ''' re-reads the tree -- ASCUNS is a server-side filter (include_hidden), not a local one.
    ''' </summary>
    Private Async Sub BtnOpt_Click(sender As Object, e As EventArgs) Handles btnOpt.Click
        Try
            _includeHidden = Not _includeHidden
            Await LoadTreeAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.btnOpt_Click", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Opens (or brings to the front) the modeless «Informatii interne» window, which shows
    ''' every field + the Are* flags of the selected angajament. Modeless: Show, not
    ''' ShowDialog -- the operator can work in the shell with it open. It refreshes itself on
    ''' every tree selection (see RefreshInfoForm); its own refresh button re-reads the
    ''' selection through the _currentInfo provider.
    ''' </summary>
    Private Sub BtnInfo_Click(sender As Object, e As EventArgs) Handles btnInfo.Click
        Try
            If _infoForm Is Nothing OrElse _infoForm.IsDisposed Then
                _infoForm = New InternalInfoForm(Function() _currentInfo)
                ' Placed next to the tree card, inside the shell.
                Dim anchor As Point = PointToScreen(New Point(pnlWork.Left + 8, pnlWork.Top + 8))
                _infoForm.StartPosition = FormStartPosition.Manual
                _infoForm.Location = anchor
                _infoForm.ShowInfo(_currentInfo)
                _infoForm.Show(Me)   ' modeless, owned by the shell (closes with it)
            Else
                _infoForm.ShowInfo(_currentInfo)
                If _infoForm.WindowState = FormWindowState.Minimized Then _infoForm.WindowState = FormWindowState.Normal
                _infoForm.BringToFront()
                _infoForm.Activate()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.btnInfo_Click", ex)
        End Try
    End Sub

    ' Pushes the current context to the info window, when it is open.
    Private Sub RefreshInfoForm()
        Try
            If _infoForm IsNot Nothing AndAlso Not _infoForm.IsDisposed Then
                _infoForm.ShowInfo(_currentInfo)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.RefreshInfoForm", ex)
        End Try
    End Sub
End Class
