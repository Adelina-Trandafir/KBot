Option Strict On

''' <summary>
''' Pe ce axă se strânge <see cref="KBotDataView"/> la apăsarea butonului din subsol
''' (slice 0028).
''' </summary>
Public Enum KBotCollapseDirection

    ''' <summary>
    ''' Pe LĂȚIME, ca <c>AdvancedTreeControl</c> și <c>KBotNavList</c>: grila ajunge la
    ''' <see cref="KBotDataView.MinimumCollapsedWidth"/>, adică o fâșie îngustă lângă vederea de
    ''' alături. Contractul e identic cu al celor două surori, inclusiv regula gazdei care ține
    ''' lățimea (<see cref="KBotDataView.HostOwnsWidth"/>).
    ''' </summary>
    Horizontal = 0

    ''' <summary>
    ''' Pe ÎNĂLȚIME: corpul dispare cu totul și rămân cele două benzi — antetul și SUBSOLUL, deci
    ''' agregatele stau în continuare sub ochi. Asta e forma utilă pentru o grilă andocată
    ''' <c>Fill</c> într-o vedere, unde o fâșie verticală de 100px n-ar arăta nimic.
    ''' </summary>
    Vertical = 1

End Enum
