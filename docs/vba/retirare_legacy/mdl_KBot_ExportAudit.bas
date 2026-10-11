Attribute VB_Name = "mdl_KBot_ExportAudit"
Option Compare Database
Option Explicit

' Export Access objects for a complete review. No HTTP, no stored credentials.
' This module does NOT rewrite code or disable calls in other modules.
Public Sub Kbot_ExportPentruCuratare(Optional ByVal folder As String = "")
    Dim logFile As Integer
    Dim failures As Long
    Dim errNumber As Long
    Dim errSource As String
    Dim errDescription As String
    Dim logOpened As Boolean

    On Error GoTo Fail
    If Len(folder) = 0 Then
        folder = CurrentProject.Path & "\KBOT_VBA_EXPORT_" & Format$(Now, "yyyymmdd_hhnnss")
    End If
    If Len(Dir$(folder, vbDirectory)) > 0 Then
        Err.Raise vbObjectError + 2110, "Kbot_ExportPentruCuratare", _
                  "Alegeti un folder nou pentru export."
    End If
    MkDir folder
    logFile = FreeFile
    Open folder & "\manifest.txt" For Output As #logFile
    logOpened = True
    Print #logFile, "ObjectType;ObjectName;Status;File"

    ExportGroup CurrentProject.AllModules, acModule, "module", folder, logFile, failures
    ExportGroup CurrentProject.AllForms, acForm, "form", folder, logFile, failures
    ExportGroup CurrentProject.AllReports, acReport, "report", folder, logFile, failures
    ExportGroup CurrentProject.AllMacros, acMacro, "macro", folder, logFile, failures
    ExportGroup CurrentData.AllQueries, acQuery, "query", folder, logFile, failures

    Close #logFile
    logOpened = False
    If failures > 0 Then
        Err.Raise vbObjectError + 2111, "Kbot_ExportPentruCuratare", _
                  "Export incomplet: " & CStr(failures) & " obiecte. Vezi manifest.txt."
    End If
    MsgBox "Export complet: " & folder, vbInformation, "Inventar VBA"
    Exit Sub

Fail:
    errNumber = Err.Number
    errSource = Err.Source
    errDescription = Err.Description
    If logOpened Then Close #logFile
    Err.Raise errNumber, errSource, errDescription
End Sub

Private Sub ExportGroup(ByVal objects As Object, ByVal objectType As Long, _
                        ByVal prefix As String, ByVal folder As String, _
                        ByVal logFile As Integer, ByRef failures As Long)
    Dim item As Object
    Dim counter As Long
    Dim destination As String

    For Each item In objects
        counter = counter + 1
        destination = folder & "\" & prefix & "_" & Format$(counter, "00000") & ".txt"
        If Not ExportOne(objectType, CStr(item.Name), destination, logFile) Then
            failures = failures + 1
        End If
    Next item
End Sub

Private Function ExportOne(ByVal objectType As Long, ByVal objectName As String, _
                           ByVal destination As String, ByVal logFile As Integer) As Boolean
    On Error GoTo Fail
    Application.SaveAsText objectType, objectName, destination
    Print #logFile, CStr(objectType) & ";" & objectName & ";OK;" & destination
    ExportOne = True
    Exit Function
Fail:
    Print #logFile, CStr(objectType) & ";" & objectName & ";ERROR " & CStr(Err.Number) & ";"
    ExportOne = False
End Function
