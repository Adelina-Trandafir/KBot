Option Strict On
Imports System.Diagnostics
Imports System.Globalization
Imports System.IO
Imports System.Text.RegularExpressions

''' <summary>
''' One PowerShell script of the repository that the «Scripturi» page can start. The rows of the switch list are the keys of
''' <see cref="Flags"/>: a script has a switch when it has the key, and a ticked switch adds that key's arguments to the call.
''' </summary>
Public NotInheritable Class ScriptDef

    Public Sub New(k_group As String, k_title As String, k_relPath As String, k_fixedArgs As String(), k_description As String,
                   k_flags As IDictionary(Of String, String()), k_takesNotes As Boolean, k_detached As Boolean)
        Group = k_group
        Title = k_title
        RelPath = k_relPath
        FixedArgs = k_fixedArgs
        Description = k_description
        Flags = k_flags
        TakesNotes = k_takesNotes
        Detached = k_detached
    End Sub

    ''' <summary>The band of the list the script is under: <see cref="ScriptCatalog.GroupPush"/> or <see cref="ScriptCatalog.GroupPublish"/>.</summary>
    Public ReadOnly Property Group As String

    ''' <summary>Stable id of the row, used to keep its ticked switches in the settings (the file plus its fixed arguments, so the publish-tray rows differ).</summary>
    Public ReadOnly Property Key As String
        Get
            Return RelPath & " " & String.Join(" ", FixedArgs)
        End Get
    End Property

    ''' <summary>What the page shows in the first column (the file name, plus the fixed argument when one file has several rows).</summary>
    Public ReadOnly Property Title As String

    ''' <summary>Path of the script from the repository root.</summary>
    Public ReadOnly Property RelPath As String

    ''' <summary>Arguments the script always gets (e.g. <c>-Action Stop</c> of publish-tray).</summary>
    Public ReadOnly Property FixedArgs As String()

    Public ReadOnly Property Description As String

    ''' <summary>Switch key -> arguments added when it is ticked. Order of the dictionary = order on the command line and in the switch list.</summary>
    Public ReadOnly Property Flags As IDictionary(Of String, String())

    ''' <summary>The script has a <c>-Notes</c> parameter (the text box under the switch list).</summary>
    Public ReadOnly Property TakesNotes As Boolean

    ''' <summary>
    ''' The script replaces the running KBotAdmin (publish-admin.ps1 deletes and rewrites <c>_Admin</c>, where this program runs from): it is
    ''' started detached, waits for this program to end, and only then runs.
    ''' </summary>
    Public ReadOnly Property Detached As Boolean

End Class

''' <summary>The scripts the «Scripturi» page offers, and how a call to one is built and started.</summary>
Public NotInheritable Class ScriptCatalog

    Public Const GroupPush As String = "Push - trimit pe server"
    Public Const GroupPublish As String = "Publish - construiesc local"

    Public Const FlagMandatory As String = "mandatory"
    Public Const FlagForce As String = "force"
    Public Const FlagSkipBuild As String = "skipbuild"
    Public Const FlagSkipUpload As String = "skipupload"
    Public Const FlagAccess As String = "access"
    Public Const FlagNoAccess As String = "noaccess"
    Public Const FlagSign As String = "sign"
    Public Const FlagNoSign As String = "nosign"
    Public Const FlagDryRun As String = "dryrun"
    Public Const FlagBaseline As String = "baseline"
    Public Const FlagRestart As String = "restart"
    Public Const FlagNoRestart As String = "norestart"
    Public Const FlagForget As String = "forget"
    Public Const FlagRelease As String = "release"
    Public Const FlagDebug As String = "debug"
    Public Const FlagClean As String = "clean"

    Private Shared ReadOnly FlagHelp As New Dictionary(Of String, String)(StringComparer.Ordinal) From {
        {FlagMandatory, "Toți clienții de acest fel sub versiunea trimisă sunt obligați să se actualizeze (fără «Mai târziu»)."},
        {FlagForce, "Scrie peste fără întrebare dacă serverul are deja versiunea aceasta sau una mai nouă."},
        {FlagSkipBuild, "Nu construiește nimic; trimite cel mai nou zip din artifacts."},
        {FlagSkipUpload, "Nu urcă pachetul din nou; copia lui de pe server trebuie să fie deja acolo."},
        {FlagAccess, "Pachetul pentru clienții care folosesc și aplicația Access. Dacă nu bifați nici linia aceasta, nici pe cea fără Access, scriptul întreabă."},
        {FlagNoAccess, "Pachetul pentru clienții fără aplicația Access. Dacă nu bifați nici linia aceasta, nici pe cea cu Access, scriptul întreabă."},
        {FlagSign, "Semnează executabilele. Dacă nu bifați nici linia aceasta, nici pe cea fără semnare, scriptul întreabă."},
        {FlagNoSign, "Nu semnează nimic; jetonul de semnare nu e cerut. Dacă nu bifați nici linia aceasta, nici pe cea cu semnare, scriptul întreabă."},
        {FlagDryRun, "Arată ce s-ar trimite, fără să trimită nimic."},
        {FlagBaseline, "Notează starea de acum ca fiind deja pe server, fără să trimită nimic. Se folosește o dată, înaintea primei trimiteri."},
        {FlagRestart, "Repornește serviciul chiar dacă nu s-a schimbat niciun .py."},
        {FlagNoRestart, "Nu repornește niciodată serviciul."},
        {FlagForget, "Șterge starea ținută minte; următoarea rulare consideră toate fișierele."},
        {FlagRelease, "Configurația Release (altfel scriptul folosește Debug)."},
        {FlagDebug, "Configurația Debug (altfel scriptul folosește Release)."},
        {FlagClean, "Golește folderul de ieșire înainte de publicare."}}

    ''' <summary>Pairs where ticking one clears the other (they are the two values of one parameter, or opposites).</summary>
    Public Shared ReadOnly Exclusive As String()() = {
        New String() {FlagAccess, FlagNoAccess}, New String() {FlagSign, FlagNoSign}, New String() {FlagRestart, FlagNoRestart}, New String() {FlagRelease, FlagDebug}}

    Private Shared ReadOnly ParameterName As New Regex("^-[A-Za-z]+$", RegexOptions.CultureInvariant)

    ''' <summary>What a switch does, in plain words (the last column of the switch list).</summary>
    Public Shared Function FlagDescription(k_key As String) As String
        Dim k_text As String = Nothing
        If Not FlagHelp.TryGetValue(k_key, k_text) Then Throw New ArgumentException("Switch necunoscut: " & k_key, NameOf(k_key))
        Return k_text
    End Function

    Private Shared Function Flags(ParamArray k_pairs As Object()) As IDictionary(Of String, String())
        Dim k_result As New Dictionary(Of String, String())(StringComparer.Ordinal)
        For k_i As Integer = 0 To k_pairs.Length - 1 Step 2
            k_result(CStr(k_pairs(k_i))) = DirectCast(k_pairs(k_i + 1), String())
        Next
        Return k_result
    End Function

    Private Shared Function Script(k_group As String, k_title As String, k_relPath As String, k_description As String,
                                   k_flags As IDictionary(Of String, String()), k_takesNotes As Boolean) As ScriptDef
        Return New ScriptDef(k_group, k_title, k_relPath, Array.Empty(Of String)(), k_description, k_flags, k_takesNotes, False)
    End Function

    ''' <summary>The scripts, pushes first, then the publishes.</summary>
    Public Shared Function All() As IReadOnlyList(Of ScriptDef)
        Dim k_audience As String() = {"-Audience", "Access"}
        Dim k_noAudience As String() = {"-Audience", "NonAccess"}
        Dim k_tray As String = "Construiește KBot.Tray și îl pornește / oprește pentru probe; nu face parte din livrare."
        Dim k_trayFlags As IDictionary(Of String, String()) = Flags(FlagRelease, {"-Configuration", "Release"})
        Return New List(Of ScriptDef) From {
            Script(GroupPush, "push-update.ps1", "push-update.ps1",
                   "Construiește pachetul Release (publicare, semnare, zip) și îl pune pe server ca actualizare K-BOT. Ce rămâne nebifat (pentru cine, ce parte din versiune crește, parola sftp), scriptul întreabă.",
                   Flags(FlagMandatory, {"-Mandatory"}, FlagForce, {"-Force"}, FlagSkipBuild, {"-SkipBuild"},
                         FlagAccess, k_audience, FlagNoAccess, k_noAudience, FlagSign, {"-Sign", "Yes"}, FlagNoSign, {"-Sign", "No"}), True),
            Script(GroupPush, "push-last-zip.ps1", "push-last-zip.ps1",
                   "Pune pe server cel mai nou zip din artifacts\, fără să construiască nimic. Versiunea e FileVersion-ul de acum al lui KBot.App.",
                   Flags(FlagMandatory, {"-Mandatory"}, FlagForce, {"-Force"}), True),
            Script(GroupPush, "rollback-update.ps1", "rollback-update.ps1",
                   "Readuce o versiune mai veche ca actualizare pe server (clienții care s-au actualizat deja rămân pe versiunea nouă).",
                   Flags(FlagForce, {"-Force"}, FlagSkipUpload, {"-SkipUpload"}, FlagAccess, k_audience, FlagNoAccess, k_noAudience), True),
            Script(GroupPush, "Push-Changed.ps1 (PYTHON)", "tools\Push-Changed.ps1",
                   "Trimite pe server doar fișierele din PYTHON schimbate de la ultima trimitere și repornește serviciul dacă s-a schimbat un .py. Prima dată: «-Baseline», ca să nu trimită tot.",
                   Flags(FlagDryRun, {"-DryRun"}, FlagBaseline, {"-Baseline"}, FlagRestart, {"-Restart"}, FlagNoRestart, {"-NoRestart"}, FlagForget, {"-Forget"}), False),
            Script(GroupPublish, "publish-release.ps1", "publish-release.ps1",
                   "Construiește pachetul Release (zip, setup, driver) în artifacts\, fără să-l trimită pe server. Ce rămâne nebifat, scriptul întreabă.",
                   Flags(FlagAccess, k_audience, FlagNoAccess, k_noAudience, FlagSign, {"-Sign", "Yes"}, FlagNoSign, {"-Sign", "No"}), False),
            Script(GroupPublish, "publish-debug.ps1", "publish-debug.ps1",
                   "Publică KBot.App în Debug și face un zip de copiat pe alt calculator.",
                   Flags(), False),
            New ScriptDef(GroupPublish, "publish-tray.ps1 (Run)", "publish-tray.ps1", {"-Action", "Run"}, "Oprește tray-ul, îl publică și îl pornește. " & k_tray, k_trayFlags, False, False),
            New ScriptDef(GroupPublish, "publish-tray.ps1 (Publish)", "publish-tray.ps1", {"-Action", "Publish"}, "Oprește tray-ul și îl publică, fără să-l pornească.", k_trayFlags, False, False),
            New ScriptDef(GroupPublish, "publish-tray.ps1 (Start)", "publish-tray.ps1", {"-Action", "Start"}, "Pornește tray-ul deja publicat, fără să publice.", k_trayFlags, False, False),
            New ScriptDef(GroupPublish, "publish-tray.ps1 (Stop)", "publish-tray.ps1", {"-Action", "Stop"}, "Oprește tray-ul care rulează.", Flags(), False, False),
            Script(GroupPublish, "publish-vrc-migrator.ps1", "publish-vrc-migrator.ps1",
                   "Publică VRC.Migrator și ADE.Migrator în folderul _VrcMigrator (Release; «Debug» schimbă configurația, «Clean» golește folderul întâi).",
                   Flags(FlagDebug, {"-Configuration", "Debug"}, FlagClean, {"-Clean"}), False),
            Script(GroupPublish, "publish-avc-migrator.ps1", "publish-avc-migrator.ps1",
                   "Publică DOAR AVC.Migrator în folderul _AvcMigrator (Release; «-Configuration Debug» schimbă configurația, «-Clean» golește întâi). Setările și jurnalele din folder se păstrează.",
                   Flags(FlagDebug, {"-Configuration", "Debug"}, FlagClean, {"-Clean"}), False),
            New ScriptDef(GroupPublish, "publish-admin.ps1", "publish-admin.ps1", Array.Empty(Of String)(),
                          "Publică KBotAdmin în _Admin. Fiind chiar acest program, el se închide, scriptul pornește după aceea; redeschideți programul din _Admin\KBotAdmin.exe.",
                          Flags(), False, True)}
    End Function

    ''' <summary>The switch as it is typed (<c>-Audience Access</c>): the arguments the switch adds to the call.</summary>
    Public Shared Function SwitchText(k_def As ScriptDef, k_key As String) As String
        Return String.Join(" ", k_def.Flags(k_key))
    End Function

    ''' <summary>The folder with KBot.sln above this program (the scripts live there). Empty when there is none.</summary>
    Public Shared Function FindRepoRoot() As String
        Dim k_dir As DirectoryInfo = New DirectoryInfo(AppContext.BaseDirectory)
        Do While k_dir IsNot Nothing
            If File.Exists(Path.Combine(k_dir.FullName, "KBot.sln")) Then Return k_dir.FullName
            k_dir = k_dir.Parent
        Loop
        Return String.Empty
    End Function

    ''' <summary>The arguments of the script: its fixed ones, then the ticked switches, then <c>-Notes</c> when text was written.</summary>
    Public Shared Function ScriptArguments(k_def As ScriptDef, k_ticked As ISet(Of String), k_notes As String) As List(Of String)
        Dim k_args As New List(Of String)(k_def.FixedArgs)
        For Each k_pair As KeyValuePair(Of String, String()) In k_def.Flags
            If k_ticked.Contains(k_pair.Key) Then k_args.AddRange(k_pair.Value)
        Next
        If k_def.TakesNotes AndAlso Not String.IsNullOrWhiteSpace(k_notes) Then
            k_args.Add("-Notes")
            k_args.Add(k_notes.Trim())
        End If
        Return k_args
    End Function

    ''' <summary>The call as the operator would type it in PowerShell: what the page shows and what the confirmation repeats.</summary>
    Public Shared Function DisplayCommand(k_def As ScriptDef, k_args As IEnumerable(Of String)) As String
        Dim k_parts As New List(Of String) From {".\" & k_def.RelPath}
        k_parts.AddRange(k_args.Select(Function(k_a) Literal(k_a)))
        Return String.Join(" ", k_parts)
    End Function

    ''' <summary>A PowerShell literal: a parameter name as it is, anything else between single quotes.</summary>
    Private Shared Function Literal(k_arg As String) As String
        If ParameterName.IsMatch(k_arg) Then Return k_arg
        Return "'" & k_arg.Replace("'", "''") & "'"
    End Function

    ''' <summary>
    ''' Starts the script in its own visible PowerShell window, so its questions and the sftp password prompt work as when typed by hand.
    ''' The window stays open until Enter is pressed; its exit code is that of the script. A detached script first waits for THIS program to end.
    ''' </summary>
    Public Shared Function Start(k_def As ScriptDef, k_root As String, k_args As IEnumerable(Of String)) As Process
        Try
            Dim k_script As String = Path.Combine(k_root, k_def.RelPath)
            If Not File.Exists(k_script) Then Throw New FileNotFoundException("Scriptul nu există: " & k_script)
            Dim k_call As String = "& " & Literal(k_script) & " " & String.Join(" ", k_args.Select(Function(k_a) Literal(k_a))) &
                                   "; $ok = $?; $code = $LASTEXITCODE; Read-Host 'Enter = inchide fereastra' | Out-Null; " &
                                   "if ($ok) { exit 0 } elseif ($code) { exit $code } else { exit 1 }"
            If k_def.Detached Then
                k_call = "Write-Host 'Astept sa se inchida KBotAdmin...'; Wait-Process -Id " &
                         Environment.ProcessId.ToString(CultureInfo.InvariantCulture) & " -ErrorAction SilentlyContinue; " & k_call
            End If
            Dim k_info As New ProcessStartInfo("powershell.exe") With {.UseShellExecute = True, .WorkingDirectory = k_root}
            k_info.ArgumentList.Add("-ExecutionPolicy")
            k_info.ArgumentList.Add("Bypass")
            k_info.ArgumentList.Add("-Command")
            k_info.ArgumentList.Add(k_call)
            Dim k_process As Process = Process.Start(k_info)
            If k_process Is Nothing Then Throw New InvalidOperationException("PowerShell nu a pornit.")
            Return k_process
        Catch ex As Exception
            GlobalErrorLog.Write("ScriptCatalog.Start", ex)
            Throw
        End Try
    End Function

End Class
