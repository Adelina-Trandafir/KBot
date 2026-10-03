Attribute VB_Name = "mdl_FX_CLS_SYNC_TO_KBOT"
Option Compare Database
Option Explicit

'======================================================================
' mdl_FX_CLS_SYNC_TO_KBOT                      (slice 0103-05)
' Access -> NEW MariaDB (the K-BOT server): the budget and the
' rectifications of the classifications changed in Access.
'
' It does NOT touch the database the rest of the VBA works with: the
' route /api/clasificatii/sync_acc_kbot writes on the K-BOT server only.
'
' Correlation (done on the server):
'     Access  IdUnitate + Clasificatii.IDClsf
'     MariaDB Clasificatii.IdUnitate + Clasificatii.IdClsfAcc
'
' What is sent
'   * Clasificatii.Trim1..4 of the classification  -> a budget VERSION
'     (Clasificatii_Buget) starting on vDataInceput; without a date the
'     server edits the latest version of the year (01.01 if none).
'   * Rectificari of the year of the classification -> Clasificatii_Rectificari
'     (upsert on IdClsf + Data + Document).
'
' Needs, already in the project (the DDF sync modules use them the same way):
'   API_BASE_URL  (http://adcredit.avatarsoft.ro:5008/api -- the part up to "/api" is used)
'   API_KEY, DC(), ConvertToJson(), Dictionary (Scripting.Dictionary)
'
' Call from the Clasificatii form, e.g. on a button:
'     FX_CLS_Sync_To_KBot Me!IdUnitate, 2026, Me!IDClsf
' or for all classifications of the unit:
'     FX_CLS_Sync_To_KBot 76, 2026
'======================================================================

Private Const KBOT_ROUTE As String = "/clasificatii/sync_acc_kbot"   ' after ".../api"
Private Const TIMEOUT_RCV_MS As Long = 300000   ' 5 minutes

Private Function DStr(ByVal V As Variant) As Variant
    If IsNull(V) Then DStr = Null Else DStr = Format(CDate(V), "yyyy-mm-dd")
End Function

Private Function OD(ByVal V As Variant) As Variant
    ' nullable Double
    If IsNull(V) Then OD = Null Else OD = CDbl(V)
End Function

Private Function KbotUrl() As String
    ' API_BASE_URL looks like "http://host:5008/api" (no trailing slash); a module-level constant
    ' may carry more, e.g. ".../api/ddf". Keep everything up to and including "/api", then the route.
    Dim p As Long
    p = InStr(1, API_BASE_URL, "/api", vbTextCompare)
    If p > 0 Then
        KbotUrl = Left$(API_BASE_URL, p + 3) & KBOT_ROUTE
    Else
        KbotUrl = API_BASE_URL & "/api" & KBOT_ROUTE
    End If
End Function

'======================================================================
' PUBLIC ENTRY POINT
'   vIdUnitate     the unit the Access file belongs to (Access has no IdUnitate column here)
'   vAn            the working year
'   vIdClsf        one classification (Access IDClsf); 0 = all of them
'   vDataInceput   the date the budget applies from; Empty = server default
'======================================================================
Public Function FX_CLS_Sync_To_KBot(ByVal vIdUnitate As Long, ByVal vAn As Long, _
                                    Optional ByVal vIdClsf As Long = 0, _
                                    Optional ByVal vDataInceput As Variant) As Boolean
On Error GoTo EROARE
    Dim CPN As String:           CPN = "mdl_FX_CLS_SYNC_TO_KBOT\FX_CLS_Sync_To_KBot"
    Dim rsC As DAO.Recordset
    Dim rsR As DAO.Recordset
    Dim root As Dictionary
    Dim dicC As Dictionary
    Dim dicR As Dictionary
    Dim colC As Collection
    Dim colR As Collection
    Dim httpReq As Object
    Dim sql As String
    Dim n As Long

    Set root = New Dictionary
    root("db_name") = DC()
    root("id_unitate") = vIdUnitate
    root("an") = vAn
    If Not IsMissing(vDataInceput) Then
        If Not IsNull(vDataInceput) And Not IsEmpty(vDataInceput) Then
            root("data_inceput") = DStr(vDataInceput)
        End If
    End If

    sql = "SELECT IDClsf, Trim1, Trim2, Trim3, Trim4 FROM Clasificatii"
    If vIdClsf > 0 Then sql = sql & " WHERE IDClsf = " & CLng(vIdClsf)
    Set rsC = CurrentDb.OpenRecordset(sql, dbOpenSnapshot)

    Set colC = New Collection
    Do While Not rsC.EOF
        Set dicC = New Dictionary
        dicC("IdClsfAcc") = CLng(rsC!IDClsf)
        dicC("Trim1") = OD(rsC!Trim1)
        dicC("Trim2") = OD(rsC!Trim2)
        dicC("Trim3") = OD(rsC!Trim3)
        dicC("Trim4") = OD(rsC!Trim4)

        Set colR = New Collection
        Set rsR = CurrentDb.OpenRecordset( _
            "SELECT Data, Document, Trim1, Trim2, Trim3, Trim4 FROM Rectificari" & _
            " WHERE IdClsf = " & CLng(rsC!IDClsf) & _
            " AND Year([Data]) = " & CLng(vAn) & " AND Document Is Not Null", dbOpenSnapshot)
        Do While Not rsR.EOF
            Set dicR = New Dictionary
            dicR("Data") = DStr(rsR!Data)
            dicR("Document") = CStr(rsR!Document)
            dicR("Trim1") = OD(rsR!Trim1)
            dicR("Trim2") = OD(rsR!Trim2)
            dicR("Trim3") = OD(rsR!Trim3)
            dicR("Trim4") = OD(rsR!Trim4)
            colR.Add dicR
            Set dicR = Nothing
            rsR.MoveNext
        Loop
        rsR.Close: Set rsR = Nothing
        dicC.Add "rectificari", colR
        Set colR = Nothing

        colC.Add dicC
        Set dicC = Nothing
        n = n + 1
        rsC.MoveNext
    Loop
    rsC.Close: Set rsC = Nothing

    If n = 0 Then
        MsgBox "Nu exista clasificatii de trimis.", vbInformation, CPN
        GoTo Iesire
    End If
    root.Add "clasificatii", colC

    Set httpReq = CreateObject("WinHttp.WinHttpRequest.5.1")
    With httpReq
        .Open "POST", KbotUrl(), False
        .SetRequestHeader "Content-Type", "application/json"
        .SetRequestHeader "X-API-Key", API_KEY
        .SetTimeouts 0, 60000, 60000, TIMEOUT_RCV_MS
        .Send ConvertToJson(root)
        FX_CLS_Sync_To_KBot = (.Status = 200)
        If .Status = 200 Then
            MsgBox "Trimis catre K-BOT: " & n & " clasificatii." & vbCrLf & Left$(.responseText, 400), _
                   vbInformation, CPN
        Else
            MsgBox "K-BOT a refuzat trimiterea (HTTP " & .Status & "):" & vbCrLf & Left$(.responseText, 400), _
                   vbExclamation, CPN
        End If
    End With

Iesire:
    Set httpReq = Nothing
    Set root = Nothing
    If Not rsR Is Nothing Then rsR.Close: Set rsR = Nothing
    If Not rsC Is Nothing Then rsC.Close: Set rsC = Nothing
    Exit Function

EROARE:
    MsgBox "Eroare " & CPN & ": " & Err.Description, vbCritical
    FX_CLS_Sync_To_KBot = False
    Resume Iesire
End Function
