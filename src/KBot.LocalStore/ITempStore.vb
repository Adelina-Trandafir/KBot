Imports System

' Înlocuiește tabelele tmpFX_*. Implementare: SQLite in-memory.
' Suprafața de query (construirea setului de lucru) se adaugă per felie.
Public Interface ITempStore
    Inherits IDisposable

    ' Inițializează conexiunea SQLite in-memory și creează schema de lucru.
    Sub Open()

    ' Golește setul de lucru (echivalent ștergerii din tmpFX_*).
    Sub Reset()
End Interface
