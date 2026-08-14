Option Strict On
Imports System.Runtime.CompilerServices

' LogEntry își ține mutatoarele Friend: în afara ansamblului o intrare de jurnal e IMUABILĂ, iar
' singurul care are voie să o modifice după construcție e LogFileLoader (continuare, moștenirea
' marcajelor). Testele au totuși nevoie să compună intrări de probă, deci ansamblul de teste vede
' membrii Friend. Nimic altceva nu îi vede.
<Assembly: InternalsVisibleTo("KBot.Common.Tests")>
