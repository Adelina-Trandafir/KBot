Option Strict On

' Configurația descărcării machetelor XFA (DDF/ORD).
' ATENȚIE: acesta este API-ul LEGACY FOREXE/VBA (autentificare X-API-KEY), separat de
' API-ul K-BOT cu token bearer (https://kbot.avatarsoft.ro). Portat ca atare din
' XFA_WRITTER — endpoint-ul de machete trăiește pe serverul vechi.
Public Module Configs
    Public Const BASE_URL As String = "http://adcredit.avatarsoft.ro:5008/api/"
    Public Const DDF_URL As String = "mfp/template_ddf"
    Public Const ORD_URL As String = "mfp/template_ord"
    Public Const API_KEY As String = "Ad3l1na1i1ub3st3P310ana5iRazvan2026!@#"
    Public Const CACHE_DIR As String = "c:\avacont\cache"
End Module
