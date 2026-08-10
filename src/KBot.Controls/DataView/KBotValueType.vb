Option Strict On

''' <summary>
''' Tipul VALORII dintr-o coloană <see cref="KBotDataView"/> — deliberat distinct de
''' <see cref="KBotColumnType"/>, care spune doar cum se PICTEAZĂ și cum se EDITEAZĂ celula.
''' O coloană «Text» (pictată ca text simplu, editată printr-un TextBox) poate foarte bine să
''' poarte numere: exact cazul coloanelor de sume din DdfView/PlatiView.
'''
''' English (slice 0028): the footer's aggregate offer is gated by THIS, not by the render
''' type — <see cref="KBotAggregate.Sum"/> on a column of names is a question without an answer,
''' and <see cref="KBotAggregate.CountTrue"/> on a column of amounts is another. See
''' <see cref="KBotAggregateRules"/> for the table and for what the property grid offers.
''' </summary>
Public Enum KBotValueType

    ''' <summary>Text (implicit). Se pot număra rânduri, nu se pot aduna.</summary>
    Text = 0

    ''' <summary>Numeric — singurul tip care admite <see cref="KBotAggregate.Sum"/>/<see cref="KBotAggregate.Average"/>.</summary>
    Number = 1

    ''' <summary>Dată/oră — admite <see cref="KBotAggregate.Min"/>/<see cref="KBotAggregate.Max"/>, nu și sume.</summary>
    DateTime = 2

    ''' <summary>Logic (bifă) — admite numărătorile <see cref="KBotAggregate.CountTrue"/>/<see cref="KBotAggregate.CountFalse"/>.</summary>
    [Boolean] = 3

End Enum
