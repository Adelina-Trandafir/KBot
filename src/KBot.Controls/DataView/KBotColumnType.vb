Option Strict On

''' <summary>
''' Tipul unei coloane <see cref="KBotDataView"/>. Determină cum se pictează celula și
''' dacă (și cum) se editează.
''' </summary>
Public Enum KBotColumnType

    ''' <summary>Text simplu — editabil printr-un TextBox flotant.</summary>
    Text = 0

    ''' <summary>Listă derulantă — editabilă printr-un ComboBox flotant.</summary>
    Combo = 1

    ''' <summary>Bifă — comută la click/Space, fără editor flotant.</summary>
    CheckBox = 2

    ''' <summary>
    ''' Buton radio — mutual exclusiv între coloanele aceluiași rând marcate în același
    ''' OptionGroup; setarea uneia le stinge pe surori.
    ''' </summary>
    OptionButton = 3

    ''' <summary>Buton de acțiune — ridică ButtonClick, nu ține valoare.</summary>
    Button = 4

    ''' <summary>Bară de progres — doar afișare (min..max).</summary>
    ProgressBar = 5

End Enum
