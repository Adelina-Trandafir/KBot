Option Strict On

''' <summary>
''' «Am măsuri proprii, care nu se refac singure la o repictare.»
'''
''' <para>Constantele dintr-un <c>OnPaint</c> trec prin <see cref="ThemeShapes.ScaleDpi"/> la
''' FIECARE pictare, deci o schimbare de scară le prinde din mers — le ajunge o invalidare. Dar
''' un control care își ține înălțimea de rând, banda de antet sau lățimile de coloană într-un
''' câmp le-a calculat O DATĂ, la scara de atunci; ăla trebuie chemat pe nume. Interfața asta e
''' lista celor care trebuie chemați.</para>
'''
''' <para>O implementează <c>AdvancedTreeControl</c> și <c>KBotDataView</c> — exact cele două
''' controale cu perechi logic/scalat (vezi partialele lor <c>.Dpi.vb</c>). Trăiește în
''' <c>KBot.Theming</c> fiindcă de aici pleacă difuzarea (<see cref="AppScaling.Broadcast"/>), iar
''' motorul de teme nu poate referi <c>KBot.Controls</c> — sensul referinței e Controls → Theming
''' și numai așa.</para>
''' </summary>
Public Interface IDpiScaledControl

    ''' <summary>
    ''' Recitește scara și reface măsurile din perechile lor logice. Trebuie să fie IDEMPOTENTĂ
    ''' (recalculează din valoarea logică, nu compune peste cea scalată) — se cheamă și la
    ''' schimbarea de DPI, și la fiecare schimbare de setare.
    ''' </summary>
    Sub RefreshDpiMetrics()

End Interface
