Attribute VB_Name = "mdl_FX_CLS_SYNC_TO_KBOT"
Option Compare Database
Option Explicit

' Temporary replacement of the original module, not an additional module.
' Remove the callers and this stub in the final VBA cleanup.
' It preserves the public signature but never reports a successful sync.
Public Function FX_CLS_Sync_To_KBot(ByVal vIdUnitate As Long, ByVal vAn As Long, _
                                   Optional ByVal vIdClsf As Long = 0, _
                                   Optional ByVal vDataInceput As Variant) As Boolean
    FX_CLS_Sync_To_KBot = False
    Err.Raise vbObjectError + 2112, "FX_CLS_Sync_To_KBot", _
              "Sincronizarea VBA cu K-BOT a fost retrasa. Folositi Kbot.App."
End Function
