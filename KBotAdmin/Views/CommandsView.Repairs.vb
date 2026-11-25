Option Strict On
Imports System.Globalization
Imports System.Text

''' <summary>
''' FOREXE and checks: repair / explain / fix buttons that appear after a check (reception chains, FX_DDF, angajament deletion, generic repair).
''' </summary>
Partial Public Class CommandsView

    Private Sub HideLanturi()
        btnLanturi.Visible = False
        _lanturiDb = String.Empty
    End Sub

    Private Sub HideDelete()
        btnDeleteAng.Visible = False
        _delDb = String.Empty
        _delCode = String.Empty
        _delName = String.Empty
    End Sub

    ''' <summary>After the analysis of ONE code found rows to delete: «Șterge angajamentul» appears, tied to this code and this database. Typing another code hides it.</summary>
    Private Sub ConfigureDelete(k_result As CommandResult, k_check As String, k_db As String, k_code As String)
        HideDelete()
        Dim k_current As CommandInfo = CurrentCommand()
        If k_check <> AngAnalysisName OrElse _context.IsAll OrElse k_current Is Nothing OrElse k_current.Name <> k_check OrElse _context.SelectedDb <> k_db Then Return
        If k_result.Blocks.Any(Function(k_b) Not String.Equals(k_b.Db, k_db, StringComparison.OrdinalIgnoreCase)) Then Return
        If Not HasRepairableRows(k_result, AngAnalysisBlockTitle, AngAnalysisCountColumn) Then Return
        _delDb = k_db
        _delCode = k_code
        _delName = AnalysedName(k_result)
        btnDeleteAng.Visible = True
    End Sub

    ''' <summary>The name of the analysed angajament, read from the analysis summary («denumire» column), so the confirmation shows WHAT is deleted, not only the code.</summary>
    Private Shared Function AnalysedName(k_result As CommandResult) As String
        For Each k_block As CommandBlock In k_result.Blocks
            If Not k_block.Title.StartsWith(AngAnalysisBlockTitle, StringComparison.Ordinal) Then Continue For
            Dim k_column As Integer = k_block.Columns.IndexOf("denumire")
            If k_column >= 0 AndAlso k_block.Rows.Count > 0 AndAlso k_column < k_block.Rows(0).Count Then Return k_block.Rows(0)(k_column)
        Next
        Return String.Empty
    End Function

    ''' <summary>Deletes the analysed code (FX_ tables, EF_ untouched) after a confirmation that names it; ONE database, ONE code.</summary>
    Private Async Sub BtnDeleteAng_Click(k_sender As Object, e As EventArgs) Handles btnDeleteAng.Click
        Try
            Dim k_db As String = _context.RequireSingleDb("Ștergerea unui angajament")
            If k_db = String.Empty OrElse _delDb <> k_db OrElse _delCode = String.Empty Then Return
            Dim k_code As String = _delCode
            Dim k_label As String = k_code & If(_delName <> String.Empty, " – " & _delName, String.Empty)
            If Not UiRun.Confirm("Se șterge angajamentul " & k_label & " din baza " & k_db & ", exact ce a arătat analiza: tabelele FX_ ale codului (indicatori, istoric, documente de fundamentare, rezervări, recepții, plăți, ordonanțări, corecții CAB)." & vbLf &
                                 "Facturile primite (EF_) nu se ating, dar legăturile factură-document de fundamentare alese de mână pleacă odată cu documentul (rămân în Jurnal)." & vbLf &
                                 "Totul într-o singură tranzacție; dacă ceva nu merge, nu se șterge nimic. Operația NU se poate anula. Continuați?", "Ștergere angajament") Then Return
            HideDelete()
            Await UiRun.Guard("CommandsView.BtnDeleteAng_Click", _context,
                Async Function()
                    _context.Say("Se șterge «" & k_label & "» din baza " & k_db & "...")
                    Dim k_started As DateTime = DateTime.Now
                    Dim k_result As CommandResult = Await _context.Client.CallAsync(Of CommandResult)("command", "--name", DeleteAngName, "--db", k_db, "--param", k_code)
                    txtOutput.Text = RenderResult(k_result)
                    lblState.Text = "Șters " & k_label & ". " & (DateTime.Now - k_started).TotalSeconds.ToString("0.0", CultureInfo.InvariantCulture) & " s."
                    _context.Say("Gata.")
                End Function)
        Catch ex As Exception
            GlobalErrorLog.Write("CommandsView.BtnDeleteAng_Click", ex)
        End Try
    End Sub

    ''' <summary>After the reception-chain check found repairable cases: the code box and «Repară recepțiile» appear, tied to this one command and this database.</summary>
    Private Sub ConfigureLanturi(k_result As CommandResult, k_check As String, k_db As String)
        HideLanturi()
        Dim k_current As CommandInfo = CurrentCommand()
        If k_check <> LanturiCheckName OrElse _context.IsAll OrElse k_current Is Nothing OrElse k_current.Name <> k_check OrElse _context.SelectedDb <> k_db Then Return
        If k_result.Blocks.Any(Function(k_b) Not String.Equals(k_b.Db, k_db, StringComparison.OrdinalIgnoreCase)) Then Return
        If Not HasRepairableRows(k_result, LanturiBlockTitle, LanturiRepairableColumn) Then Return
        _lanturiDb = k_db
        txtParam.PlaceholderText = "Cod angajament"
        txtParam.Visible = True
        btnLanturi.Visible = True
    End Sub

    ''' <summary>Repairs the reception chains of ONE code the check listed (moves snapshots, puts a reception back, moves the ordonantare figures); ONE database, ONE code.</summary>
    Private Async Sub BtnLanturi_Click(k_sender As Object, e As EventArgs) Handles btnLanturi.Click
        Try
            Dim k_db As String = _context.RequireSingleDb("Repararea lanțurilor de recepții")
            If k_db = String.Empty OrElse _lanturiDb <> k_db Then Return
            Dim k_code As String = txtParam.Text.Trim()
            If k_code = String.Empty Then
                KBotMessage.Show("Scrieți codul angajamentului.", "Comenzi", MessageBoxButtons.OK, MessageBoxIcon.Information)
                txtParam.Focus()
                Return
            End If
            If Not UiRun.Confirm("În baza " & k_db & ", pentru angajamentul " & k_code & ": lanțurile de recepții găsite de verificare se repară." & vbLf &
                                 "Instantaneele așezate pe altă recepție se mută pe a lor; o recepție rescrisă de gemena ei din aceeași zi se pune la valorile din lanțul ei; se recalculează diferențele (DIF); totalul de recepții și rămasul din liniile ordonanțărilor afectate se mută cu diferența (ordonanțările se listează din nou, nu se generează din nou). Ce nu e clar nu se atinge." & vbLf &
                                 "Totul se face într-o singură tranzacție; dacă ceva nu e clar, nu se schimbă nimic. Se scrie în baza de date și rămâne în Jurnal, cu valorile de dinainte. Continuați?", "Repară recepțiile") Then Return
            HideLanturi()
            Await UiRun.Guard("CommandsView.BtnLanturi_Click", _context,
                Async Function()
                    _context.Say("Se repară lanțurile de recepții ale «" & k_code & "» în baza " & k_db & "...")
                    Dim k_started As DateTime = DateTime.Now
                    Dim k_result As CommandResult = Await _context.Client.CallAsync(Of CommandResult)("command", "--name", LanturiRepairName, "--db", k_db, "--param", k_code)
                    txtOutput.Text = RenderResult(k_result)
                    lblState.Text = "Reparat " & k_code & ". " & (DateTime.Now - k_started).TotalSeconds.ToString("0.0", CultureInfo.InvariantCulture) & " s. Rulați din nou verificarea ca să vedeți ce a mai rămas."
                    _context.Say("Gata.")
                End Function)
        Catch ex As Exception
            GlobalErrorLog.Write("CommandsView.BtnLanturi_Click", ex)
        End Try
    End Sub

    ''' <summary>After the FX_DDF check found cases: the code box and the «Explică» button appear, tied to this one command and this database.</summary>
    Private Sub ConfigureExplain(k_result As CommandResult, k_check As String, k_db As String)
        btnExplain.Visible = False
        _explainDb = String.Empty
        Dim k_current As CommandInfo = CurrentCommand()
        If k_check <> DdfCheckName OrElse _context.IsAll OrElse k_current Is Nothing OrElse k_current.Name <> k_check OrElse _context.SelectedDb <> k_db Then Return
        If Not HasRepairableRows(k_result, DdfBlockTitle, DdfCountColumn) Then Return
        _explainDb = k_db
        txtParam.PlaceholderText = "Cod angajament"
        txtParam.Visible = True
        btnExplain.Visible = True
    End Sub

    ''' <summary>Explains ONE code of the checked database in plain words; reads only.</summary>
    Private Async Sub BtnExplain_Click(k_sender As Object, e As EventArgs) Handles btnExplain.Click
        Try
            Dim k_db As String = _context.RequireSingleDb("Explicația pentru un cod de angajament")
            If k_db = String.Empty OrElse _explainDb <> k_db Then Return
            Dim k_code As String = txtParam.Text.Trim()
            If k_code = String.Empty Then
                KBotMessage.Show("Scrieți codul angajamentului.", "Comenzi", MessageBoxButtons.OK, MessageBoxIcon.Information)
                txtParam.Focus()
                Return
            End If
            Await UiRun.Guard("CommandsView.BtnExplain_Click", _context,
                Async Function()
                    _context.Say("Se explică «" & k_code & "» în baza " & k_db & "...")
                    Dim k_started As DateTime = DateTime.Now
                    Dim k_result As CommandResult = Await _context.Client.CallAsync(Of CommandResult)("command", "--name", DdfExplainName, "--db", k_db, "--param", k_code)
                    txtOutput.Text = RenderResult(k_result)
                    lblState.Text = "Explicație pentru " & k_code & ". " & (DateTime.Now - k_started).TotalSeconds.ToString("0.0", CultureInfo.InvariantCulture) & " s."
                    ConfigureFix(k_result, k_db, k_code)
                    _context.Say("Gata.")
                End Function)
        Catch ex As Exception
            GlobalErrorLog.Write("CommandsView.BtnExplain_Click", ex)
        End Try
    End Sub

    Private Sub HideFix()
        btnFix.Visible = False
        _fixDb = String.Empty
        _fixCode = String.Empty
    End Sub

    ''' <summary>The «Repară codul» button shows only when the explanation of THIS code in THIS database ends with REPARABIL.</summary>
    Private Sub ConfigureFix(k_result As CommandResult, k_db As String, k_code As String)
        HideFix()
        If _context.IsAll OrElse _context.SelectedDb <> k_db Then Return
        For Each k_block As CommandBlock In k_result.Blocks
            If Not k_block.Title.StartsWith(DdfConclusionTitle, StringComparison.Ordinal) Then Continue For
            For Each k_row As List(Of String) In k_block.Rows
                If k_row.Count > 0 AndAlso k_row(0).StartsWith(DdfRepairableMark, StringComparison.Ordinal) Then
                    _fixDb = k_db
                    _fixCode = k_code
                    btnFix.Visible = True
                    Return
                End If
            Next
        Next
    End Sub

    ''' <summary>A button that was earned for one code must not stay when another code is typed.</summary>
    Private Sub TxtParam2_TextChanged(k_sender As Object, e As EventArgs) Handles txtParam2.TextChanged
        If btnMuta.Visible OrElse tlyPlata.Visible Then HideMove()
    End Sub

    Private Sub TxtParam_TextChanged(k_sender As Object, e As EventArgs) Handles txtParam.TextChanged
        If btnMuta.Visible OrElse tlyPlata.Visible Then HideMove()
        If btnFix.Visible AndAlso Not String.Equals(txtParam.Text.Trim(), _fixCode, StringComparison.Ordinal) Then HideFix()
        If btnDeleteAng.Visible AndAlso Not String.Equals(txtParam.Text.Trim(), _delCode, StringComparison.Ordinal) Then HideDelete()
        If tlyBon.Visible AndAlso Not String.Equals(txtParam.Text.Trim(), _bonParam, StringComparison.Ordinal) Then HideBon()
        If _oiToken <> String.Empty AndAlso Not String.Equals(txtParam.Text.Trim(), _oiParam, StringComparison.Ordinal) Then HideOiWipe()
    End Sub

    ''' <summary>Moves the revisions of the extra headers of the explained code under the main header and deletes the emptied ones; ONE database, ONE code.</summary>
    Private Async Sub BtnFix_Click(k_sender As Object, e As EventArgs) Handles btnFix.Click
        Try
            Dim k_db As String = _context.RequireSingleDb("Repararea documentelor de fundamentare")
            If k_db = String.Empty OrElse _fixDb <> k_db OrElse _fixCode = String.Empty Then Return
            Dim k_code As String = _fixCode
            If Not UiRun.Confirm("În baza " & k_db & ", pentru angajamentul " & k_code & ": reviziile antetelor în plus care EXISTĂ DEJA în antetul principal (aceeași dată și aceeași sumă) se șterg, fără mutare (rezervările lor, care sunt unice, nu se șterg: trec pe revizia care rămâne)." & vbLf &
                                 "Doar reviziile care nu există deja în antetul principal se mută sub el. Antetele rămase goale se șterg. Detaliul, revizie cu revizie, e în «Concluzie»." & vbLf &
                                 "Totul se face într-o singură tranzacție; dacă ceva nu e clar, nu se schimbă nimic. Se scrie în baza de date și rămâne în Jurnal, cu conținutul reviziilor șterse și valorile antetelor șterse. Continuați?", "Repară codul") Then Return
            HideFix()
            Await UiRun.Guard("CommandsView.BtnFix_Click", _context,
                Async Function()
                    _context.Say("Se repară «" & k_code & "» în baza " & k_db & "...")
                    Dim k_started As DateTime = DateTime.Now
                    Dim k_result As CommandResult = Await _context.Client.CallAsync(Of CommandResult)("command", "--name", DdfRepairName, "--db", k_db, "--param", k_code)
                    txtOutput.Text = RenderResult(k_result)
                    lblState.Text = "Reparat " & k_code & ". " & (DateTime.Now - k_started).TotalSeconds.ToString("0.0", CultureInfo.InvariantCulture) & " s."
                    ConfigureFix(k_result, k_db, k_code)
                    _context.Say("Gata.")
                End Function)
        Catch ex As Exception
            GlobalErrorLog.Write("CommandsView.BtnFix_Click", ex)
        End Try
    End Sub

    Private Sub ConfigureRepair(k_result As CommandResult, k_check As String, k_db As String)
        btnRepair.Visible = False
        _repairCommand = String.Empty
        _repairDb = String.Empty
        Dim k_current As CommandInfo = CurrentCommand()
        If _context.IsAll OrElse k_current Is Nothing OrElse k_current.Name <> k_check OrElse _context.SelectedDb <> k_db Then Return
        If k_result.Blocks.Any(Function(k_b) Not String.Equals(k_b.Db, k_db, StringComparison.OrdinalIgnoreCase)) Then Return
        Dim k_title As String
        Dim k_column As String
        Dim k_repair As String
        Select Case k_check
            Case SignCheckName
                k_title = SignBlockTitle
                k_column = RepairableColumn
                k_repair = SignRepairName
            Case StornoCheckName
                k_title = StornoBlockTitle
                k_column = "reparabile"
                k_repair = StornoRepairName
            Case ClsfDupCheckName
                k_title = ClsfDupBlockTitle
                k_column = "reparabile"
                k_repair = ClsfDupRepairName
            Case ExtrCheckName
                k_title = ExtrBlockTitle
                k_column = "reparabile"
                k_repair = ExtrRepairName
            Case Else
                Return
        End Select
        If Not HasRepairableRows(k_result, k_title, k_column) Then Return
        _repairCommand = k_repair
        _repairDb = k_db
        btnRepair.Visible = True
    End Sub

    ''' <summary>Uses the summary count, so detail truncation cannot limit the repair.</summary>
    Private Shared Function HasRepairableRows(k_result As CommandResult, k_title As String, k_columnName As String) As Boolean
        For Each k_block As CommandBlock In k_result.Blocks
            If Not k_block.Title.StartsWith(k_title, StringComparison.Ordinal) Then Continue For
            Dim k_column As Integer = k_block.Columns.IndexOf(k_columnName)
            If k_column < 0 Then Continue For
            For Each k_row As List(Of String) In k_block.Rows
                Dim k_count As Long
                If k_column < k_row.Count AndAlso Long.TryParse(k_row(k_column), NumberStyles.Integer, CultureInfo.InvariantCulture, k_count) AndAlso k_count > 0 Then Return True
            Next
        Next
        Return False
    End Function

    ''' <summary>Repairs what the selected check found in ONE database, then shows the remaining differences.</summary>
    Private Async Sub BtnRepair_Click(k_sender As Object, e As EventArgs) Handles btnRepair.Click
        Try
            Dim k_db As String = _context.RequireSingleDb("Repararea datelor verificate")
            If k_db = String.Empty Then Return
            Dim k_current As CommandInfo = CurrentCommand()
            Dim k_repair As String = _repairCommand
            If k_repair = String.Empty OrElse _repairDb <> k_db OrElse k_current Is Nothing Then Return
            Dim k_check As String
            Dim k_explanation As String
            Select Case k_repair
                Case StornoRepairName
                    k_check = StornoCheckName
                    k_explanation = "Se repară toate prețurile storno și retur eligibile din baza " & k_db & ", în toate gestiunile și exercițiile: se recuperează zecimalele din prețul variantei aceleiași gestiuni. Cazurile de analizat rămân neschimbate."
                Case ClsfDupRepairName
                    k_check = ClsfDupCheckName
                    k_explanation = "În baza " & k_db & " se șterg clasificațiile dublate pe care nu le ține nicio altă tabelă în afară de Clasificatii_Rectificari și Clasificatii_Buget (un grup fără niciun rând legat păstrează unul). Rectificările și bugetele lor se șterg odată cu ele. Cele legate de FX_, EF_ sau altceva nu se ating; ce s-a șters sau mutat rămâne în Jurnal."
                Case ExtrRepairName
                    k_check = ExtrCheckName
                    k_explanation = "În baza " & k_db & " se scriu antetele de cont (FX_Extrase_H) și operațiunile (FX_Extrase) care lipsesc și pe care XML-ul din FX_Extrase_F le conține, sub fișierul din care vin. Ce există deja și FX_Extrase_F nu se ating."
                Case Else
                    k_check = SignCheckName
                    k_explanation = "Se repară registrul OI al bazei " & k_db & ": rândurile cu cantitate negativă și valoare pozitivă primesc valoarea negativă."
            End Select
            If k_current.Name <> k_check Then Return
            If Not UiRun.Confirm(k_explanation & vbLf &
                                 "Se scrie în baza de date și rămâne în Jurnal. Continuați?", "Repară") Then Return
            btnRepair.Visible = False
            _repairCommand = String.Empty
            _repairDb = String.Empty
            Await UiRun.Guard("CommandsView.BtnRepair_Click", _context,
                Async Function()
                    _context.Say("Se repară datele verificate din baza " & k_db & "...")
                    Dim k_started As DateTime = DateTime.Now
                    Dim k_result As CommandResult = Await _context.Client.CallAsync(Of CommandResult)("command", "--name", k_repair, "--db", k_db, "--param", String.Empty)
                    txtOutput.Text = RenderResult(k_result)
                    lblState.Text = "Reparat. " & (DateTime.Now - k_started).TotalSeconds.ToString("0.0", CultureInfo.InvariantCulture) & " s."
                    ConfigureRepair(k_result, k_check, k_db)
                    _context.Say("Gata.")
                End Function)
        Catch ex As Exception
            GlobalErrorLog.Write("CommandsView.BtnRepair_Click", ex)
        End Try
    End Sub

End Class
