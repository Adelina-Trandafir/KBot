Imports System.Runtime.CompilerServices

' Testele văd membrii Friend (ex. KBotDataView.DebugLastPaintedDataRows) ca să poată
' verifica virtualizarea headless — fără ecran, forțând o pictare cu DrawToBitmap.
<Assembly: InternalsVisibleTo("KBot.Controls.Tests")>
