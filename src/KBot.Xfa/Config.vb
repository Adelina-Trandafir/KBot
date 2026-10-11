Option Strict On

' Configurația descărcării machetelor XFA (DDF/ORD).
' `mfp_bp` e înregistrat în ACEEAȘI aplicație Flask ca restul (auth/forexe/ddf/seed): nu
' există o instalare „legacy" separată. `adcredit.avatarsoft.ro:5008` și `kbot.avatarsoft.ro:443`
' sunt DOUĂ uși de intrare spre ACEEAȘI aplicație, iar `/api/mfp/templates/ddf` e servit în spatele
' amândurora — deci endpoint-ul NU trăiește pe „serverul vechi". K-BOT e client HTTPS, deci
' folosim ușa 443 (felia 0020-05, pasul 05-00).
'
' Machetele se descarca de pe rutele bearer /api/mfp/templates/* (plan securitate API, S2);
' clientul nu mai are nicio cheie X-Api-Key. Tokenul sesiunii vine din TemplateDownloader.TokenProvider.
Public Module Configs
    Public Const BASE_URL As String = "https://kbot.avatarsoft.ro/api/"
    Public Const DDF_URL As String = "mfp/templates/ddf"
    Public Const ORD_URL As String = "mfp/templates/ord"
    Public Const CACHE_DIR As String = "c:\avacont\cache"
End Module
