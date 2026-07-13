Option Strict On

''' <summary>
''' Controalele care își aplică singure culorile schemei active. ThemeManager.Traverse
''' le detectează ÎNAINTE de regula generică de Panel, cheamă <see cref="ApplyTheme"/>
''' și NU recurge în ele — altfel recursia ar repicta suprafața controlului și ar strica
''' pictura proprie a copiilor (ex. TextBox-ul intern din KBotTextField).
''' </summary>
Public Interface IThemedControl

    ''' <summary>Reaplică pe control culorile schemei date (și invalidează la nevoie).</summary>
    Sub ApplyTheme(scheme As ThemeScheme)

End Interface
