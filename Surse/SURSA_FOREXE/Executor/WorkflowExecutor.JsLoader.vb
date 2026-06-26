Imports System.Collections.Concurrent

' =============================================================================
'  JsLoader — cache centralizat pentru fișiere JS embedded.
'
'  Utilizare din orice fișier al clasei parțiale WorkflowExecutor:
'
'      Await _page.AddInitScriptAsync(GetEmbeddedJs("WicketMonitor.js"))
'      Await _page.EvaluateAsync(Of Object)(GetEmbeddedJs("ScrapeTableExtract.js"))
'
'  Fișierele .js trebuie să aibă Build Action = Embedded Resource în proiect.
'  Numele passat la GetEmbeddedJs trebuie să se termine exact cu numele fișierului
'  (fără path complet — match-ul acoperă orice structură de foldere).
'
'  ConcurrentDictionary.GetOrAdd: factory-ul poate fi apelat de mai multe ori
'  în caz de concurență pe același key, dar LoadEmbeddedJs e idempotentă
'  (citire pură din assembly) deci nu există efecte secundare.
' =============================================================================
Partial Public Class WorkflowExecutor
    Private Shared ReadOnly _jsCache As New ConcurrentDictionary(Of String, String)(
        StringComparer.OrdinalIgnoreCase)

    ' =========================================================================
    '  GetEmbeddedJs — punct unic de acces, cu cache automat
    ' =========================================================================
    Friend Shared Function GetEmbeddedJs(fileName As String) As String
        Return _jsCache.GetOrAdd(fileName, Function(f) LoadEmbeddedJs(f))
    End Function

    ' =========================================================================
    '  LoadEmbeddedJs — citire din assembly, apelat o singură dată per fișier
    '
    '  Match robust:
    '    - case-insensitive (numele resursei din manifest e comparat ordinal de
    '      String.EndsWith, deci diferențele de majusculă ar conta altfel);
    '    - pe graniță corectă: numele resursei e fie EXACT fileName (resurse fără
    '      prefix de namespace), fie se termină cu "." & fileName (resurse cu
    '      prefix), ca să nu prindă din greșeală un fișier cu sufix comun
    '      (ex. cererea "Table.js" să NU prindă "FindInTable.js").
    '
    '  Dacă nu găsește, aruncă o eroare care listează TOATE resursele embedate —
    '  ca nepotrivirea să fie vizibilă direct în log, fără ghicit.
    ' =========================================================================
    Private Shared Function LoadEmbeddedJs(fileName As String) As String
        Dim asm = System.Reflection.Assembly.GetExecutingAssembly()
        Dim allNames As String() = asm.GetManifestResourceNames()

        Dim suffix As String = "." & fileName
        Dim resourceName As String = allNames.FirstOrDefault(
            Function(n) n.Equals(fileName, StringComparison.OrdinalIgnoreCase) OrElse
                        n.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))

        If resourceName Is Nothing Then
            Dim disponibile As String = String.Join(", ", allNames)
            Throw New Exception(
                $"[JsLoader] Resursa '{fileName}' nu a fost găsită în assembly '{asm.GetName().Name}'. " &
                $"Resurse embedate disponibile: {disponibile}")
        End If

        Using stream = asm.GetManifestResourceStream(resourceName)
            If stream Is Nothing Then
                Throw New Exception($"[JsLoader] Stream null pentru resursa '{resourceName}'.")
            End If
            Using reader As New System.IO.StreamReader(stream)
                Return reader.ReadToEnd()
            End Using
        End Using
    End Function
End Class