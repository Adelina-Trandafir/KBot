Option Strict On
Imports KBot.Domain

''' <summary>
''' Contractul unei vederi de angajament găzduite în shell (MainForm): Sumar,
''' Rezervări, Recepții, Plăți, DDF, ORD. Fiecare vedere e un UserControl creat lazy
''' la prima activare; shell-ul îi împinge contextul selecției prin SetContext.
''' </summary>
Public Interface IAngajamentView

    ''' <summary>Cheia vederii ("sumar", "rezervari", "receptii", "plati", "ddf", "ord").</summary>
    ReadOnly Property ViewKey As String

    ''' <summary>
    ''' Selecția din arbore s-a schimbat. Nothing = niciun angajament selectat
    ''' (nod de capitol/nivel intermediar) — vederea își arată starea goală.
    ''' </summary>
    Sub SetContext(info As AngajamentTreeInfo)

End Interface
