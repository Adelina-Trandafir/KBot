Option Strict On

' Configurația descărcării machetelor XFA (DDF/ORD).
' `mfp_bp` e înregistrat în ACEEAȘI aplicație Flask ca restul (auth/forexe/ddf/seed): nu
' există o instalare „legacy" separată. `adcredit.avatarsoft.ro:5008` și `kbot.avatarsoft.ro:443`
' sunt DOUĂ uși de intrare spre ACEEAȘI aplicație, iar `/api/mfp/template_ddf` e servit în spatele
' amândurora — deci endpoint-ul NU trăiește pe „serverul vechi". K-BOT e client HTTPS, deci
' folosim ușa 443 (felia 0020-05, pasul 05-00).
'
' `API_KEY` rămâne (rutele `mfp` sunt încă `@require_api_key`); eliminarea lui — `@require_session`
' pe cele două rute de machetă + token bearer în `TemplateDownloader`, ceea ce atinge și ORD —
' e amânată DELIBERAT, consemnată ca fir deschis în `KBOT_STATUS.md`. Vezi felia 0020-05.
Public Module Configs
    Public Const BASE_URL As String = "https://kbot.avatarsoft.ro/api/"
    Public Const DDF_URL As String = "mfp/template_ddf"
    Public Const ORD_URL As String = "mfp/template_ord"
    Public Const API_KEY As String = "Ad3l1na1i1ub3st3P310ana5iRazvan2026!@#"
    Public Const CACHE_DIR As String = "c:\avacont\cache"
End Module
