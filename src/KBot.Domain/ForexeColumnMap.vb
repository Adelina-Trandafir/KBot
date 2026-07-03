Option Strict On

' Maps the volatile FOREXE scraped column keys to the stable domain fields for the
' ListaAngajamente result. The values are static today; this seam lets a future
' config- or API-backed implementation replace them WITHOUT touching the mapper.
Public Interface IAngajamenteColumnMap
    ReadOnly Property CodKey As String        ' scraped key -> Angajament.CodAngajament
    ReadOnly Property DescriereKey As String  ' scraped key -> Angajament.Descriere
    ReadOnly Property StareKey As String      ' scraped key -> Angajament.Stare
End Interface

' Static defaults — the current FOREXE column names (verified live, 2026).
' SINGLE SOURCE OF TRUTH: if the government FOREXE app renames a column, change it HERE
' (or register a config-/API-backed IAngajamenteColumnMap in DI instead).
Public NotInheritable Class DefaultAngajamenteColumnMap
    Implements IAngajamenteColumnMap

    Public ReadOnly Property CodKey As String Implements IAngajamenteColumnMap.CodKey
        Get
            Return "Cod"
        End Get
    End Property

    Public ReadOnly Property DescriereKey As String Implements IAngajamenteColumnMap.DescriereKey
        Get
            Return "Descriere"
        End Get
    End Property

    Public ReadOnly Property StareKey As String Implements IAngajamenteColumnMap.StareKey
        Get
            Return "Stare"
        End Get
    End Property
End Class
