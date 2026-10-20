Option Strict On
Imports System.Globalization
Imports System.Text

''' <summary>
''' Empties 000_DEMO and clones another database into it (slice ME01-04). The source is the database chosen in the
''' left tree. The server half (<c>routes/admin_cli/admin_demo.py</c>) takes a dump of 000_DEMO first, then does the
''' whole job in one transaction: DC becomes 000_DEMO everywhere and the tax code of the source becomes the tax code
''' of 000_DEMO. This page only asks, shows the plan and shows what was done.
''' </summary>
Public Class DemoCloneView
    Implements IAdminView

    Private Const TargetDb As String = "000_DEMO"

    Private _context As AdminContext

    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub Attach(k_context As AdminContext) Implements IAdminView.Attach
        _context = k_context
    End Sub

    Public Function ActivatedAsync() As Task Implements IAdminView.ActivatedAsync
        ShowTarget()
        Return Task.CompletedTask
    End Function

    Public Sub SelectionChanged() Implements IAdminView.SelectionChanged
        ShowTarget()
    End Sub

    Private Sub ShowTarget()
        If _context.IsAll Then
            lblDesc.Text = "Alegeți în arborele din stânga baza care se copiază în " & TargetDb & "."
        ElseIf String.Equals(_context.SelectedDb, TargetDb, StringComparison.OrdinalIgnoreCase) Then
            lblDesc.Text = "Baza aleasă este chiar " & TargetDb & ". Alegeți altă bază, cea care se copiază în " & TargetDb & "."
        Else
            lblDesc.Text = "Sursa: " & _context.SelectedDb & "   ►   destinația: " & TargetDb & " (se golește complet)." & vbLf &
                           "DC devine " & TargetDb & " peste tot, iar codul fiscal al sursei devine codul fiscal al lui " & TargetDb &
                           ". Se face întâi o copie de siguranță a lui " & TargetDb & "."
        End If
    End Sub

    ''' <summary>The database to copy from, or empty after telling the operator what is wrong with the choice.</summary>
    Private Function ChosenSource() As String
        Dim k_source As String = _context.RequireSingleDb("Clonarea în " & TargetDb)
        If k_source = String.Empty Then Return String.Empty
        If String.Equals(k_source, TargetDb, StringComparison.OrdinalIgnoreCase) Then
            KBotMessage.Show("Baza aleasă este chiar " & TargetDb & ". Alegeți altă bază, cea care se copiază în " & TargetDb & ".",
                             "Clonare DEMO", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return String.Empty
        End If
        Return k_source
    End Function

    Private Async Sub BtnPlan_Click(k_sender As Object, e As EventArgs) Handles btnPlan.Click
        Try
            Dim k_source As String = ChosenSource()
            If k_source = String.Empty Then Return
            Await UiRun.Guard("DemoCloneView.BtnPlan_Click", _context,
                Async Function()
                    _context.Say("Se pregătește previzualizarea clonării din " & k_source & "...")
                    Dim k_started As DateTime = DateTime.Now
                    Dim k_plan As DemoClonePlan = Await _context.Client.CallAsync(Of DemoClonePlan)("demo-clone-plan", "--from", k_source)
                    txtOutput.Text = RenderPlan(k_plan, False)
                    lblState.Text = "Previzualizare din " & k_source & ", " & Elapsed(k_started) & ". Nu s-a scris nimic."
                    _context.Say("Gata. Nu s-a scris nimic.")
                End Function)
        Catch ex As Exception
            GlobalErrorLog.Write("DemoCloneView.BtnPlan_Click", ex)
        End Try
    End Sub

    Private Async Sub BtnClone_Click(k_sender As Object, e As EventArgs) Handles btnClone.Click
        Try
            Dim k_source As String = ChosenSource()
            If k_source = String.Empty Then Return
            If Not String.Equals(txtConfirm.Text.Trim(), TargetDb, StringComparison.OrdinalIgnoreCase) Then
                KBotMessage.Show("Scrieți " & TargetDb & " în căsuța de confirmare, ca să știu că vreți să îl golesc.",
                                 "Clonare DEMO", MessageBoxButtons.OK, MessageBoxIcon.Information)
                txtConfirm.Focus()
                Return
            End If
            If Not UiRun.Confirm(TargetDb & " se golește complet și primește datele din " & k_source & "." & vbLf &
                                 "Se face întâi o copie de siguranță a lui " & TargetDb & ". Dacă ceva eșuează, " & TargetDb & " rămâne neschimbat." & vbLf & vbLf &
                                 "Continuați?", "Clonare DEMO") Then Return
            Await UiRun.Guard("DemoCloneView.BtnClone_Click", _context,
                Async Function()
                    _context.Say("Se golește " & TargetDb & " și se clonează " & k_source & " (poate dura câteva minute)...")
                    Dim k_started As DateTime = DateTime.Now
                    Dim k_plan As DemoClonePlan = Await _context.Client.CallAsync(Of DemoClonePlan)("demo-clone", "--from", k_source)
                    txtOutput.Text = RenderPlan(k_plan, True)
                    lblState.Text = "Gata: " & k_plan.CopiedTotal.ToString("N0", CultureInfo.InvariantCulture) & " rânduri copiate din " & k_source & ", " & Elapsed(k_started) & "."
                    _context.Say("Gata. " & TargetDb & " conține acum datele din " & k_source & ".")
                    txtConfirm.Text = String.Empty
                    _context.RequestRefresh()
                End Function)
        Catch ex As Exception
            GlobalErrorLog.Write("DemoCloneView.BtnClone_Click", ex)
        End Try
    End Sub

    Private Sub BtnCopy_Click(k_sender As Object, e As EventArgs) Handles btnCopy.Click
        Try
            If txtOutput.Text <> String.Empty Then Clipboard.SetText(txtOutput.Text)
            _context.Say("Rezultatul a fost copiat în clipboard.")
        Catch ex As Exception
            GlobalErrorLog.Write("DemoCloneView.BtnCopy_Click", ex)
        End Try
    End Sub

    Private Shared Function Elapsed(k_started As DateTime) As String
        Return (DateTime.Now - k_started).TotalSeconds.ToString("0.0", CultureInfo.InvariantCulture) & " s"
    End Function

    ' ------------------------------------------------------------------
    ' Formatting
    ' ------------------------------------------------------------------

    Private Shared Function RenderPlan(k_plan As DemoClonePlan, k_done As Boolean) As String
        Dim k_sb As New StringBuilder()
        k_sb.AppendLine("== " & k_plan.Target & " ◄ " & k_plan.Source & If(k_done, "  (executat)", "  (previzualizare, nu s-a scris nimic)") & " ==")
        k_sb.AppendLine("Cod fiscal " & k_plan.Target & ": " & k_plan.DemoCf & "   |   cod fiscal " & k_plan.Source & ": " &
                        If(k_plan.SourceCf = String.Empty, "(lipsește)", k_plan.SourceCf) &
                        If(k_plan.RewriteCf, "   ►   se înlocuiește cu cel al lui " & k_plan.Target, "   (nu se înlocuiește nimic)"))
        If k_done Then
            k_sb.AppendLine("Copie de siguranță a lui " & k_plan.Target & " (pe server): " & k_plan.Backup)
            k_sb.AppendLine("EF_Furnizor.CodFiscal păstrat: " & k_plan.FurnizorCf)
        End If
        k_sb.AppendLine("Rânduri șterse din " & k_plan.Target & ": " & k_plan.DemoTotal.ToString("N0", CultureInfo.InvariantCulture) &
                        "   |   rânduri de copiat: " & k_plan.SourceTotal.ToString("N0", CultureInfo.InvariantCulture))
        k_sb.AppendLine()
        If k_plan.Warnings.Count > 0 Then
            k_sb.AppendLine("AVERTISMENTE")
            For Each k_warning As String In k_plan.Warnings
                k_sb.AppendLine("  ! " & k_warning)
            Next
            k_sb.AppendLine()
        End If
        k_sb.AppendLine("COLOANE REȘCRISE")
        If k_plan.Rewrites.Count = 0 Then k_sb.AppendLine("  (niciuna)")
        For Each k_rewrite As DemoCloneRewrite In k_plan.Rewrites
            k_sb.AppendLine("  " & (k_rewrite.Table & "." & k_rewrite.Column).PadRight(40) & k_rewrite.Kind.PadRight(36) &
                            k_rewrite.Rows.ToString("N0", CultureInfo.InvariantCulture).PadLeft(10) & " rânduri")
        Next
        k_sb.AppendLine()
        k_sb.AppendLine("TABELE" & If(k_done, " (copiate / așteptate)", " (acum în " & k_plan.Target & " ► în sursă)"))
        Dim k_results As New Dictionary(Of String, DemoCloneResultRow)(StringComparer.OrdinalIgnoreCase)
        For Each k_row As DemoCloneResultRow In k_plan.Results
            k_results(k_row.Table) = k_row
        Next
        For Each k_table As DemoCloneTable In k_plan.Tables
            Dim k_counts As String
            If k_done AndAlso k_results.ContainsKey(k_table.Table) Then
                k_counts = k_results(k_table.Table).Rows.ToString("N0", CultureInfo.InvariantCulture) & " / " & k_results(k_table.Table).Expected.ToString("N0", CultureInfo.InvariantCulture)
            Else
                k_counts = k_table.DemoRows.ToString("N0", CultureInfo.InvariantCulture) & " ► " & If(k_table.Copy, k_table.SourceRows.ToString("N0", CultureInfo.InvariantCulture), "0")
            End If
            k_sb.AppendLine("  " & k_table.Table.PadRight(36) & k_counts.PadRight(24) & k_table.Note)
        Next
        Return k_sb.ToString()
    End Function

End Class
