Option Strict On
Imports System.Drawing

''' <summary>
''' Modelul unei coloane <see cref="KBotDataView"/> (control NELEGAT de date). Controlul
''' deține colecția de coloane; caller-ul o construiește prin <c>AddColumn</c> și apoi
''' citește/scrie proprietățile de aici. Offset-ul X al coloanei e cache-uit de CONTROL
''' (nu se ține aici — depinde de scroll/freeze).
''' </summary>
Public NotInheritable Class KBotDataColumn

    ''' <summary>Identificator unic și stabil (folosit de API și de evenimente). Ne-modificabil.</summary>
    Public ReadOnly Property Key As String

    ''' <summary>Tipul coloanei — fixat la creare (determină pictarea/editarea).</summary>
    Public ReadOnly Property ColumnType As KBotColumnType

    ''' <summary>Textul din antet.</summary>
    Public Property HeaderText As String

    Private _minWidth As Integer = 40
    Private _width As Integer

    ''' <summary>Lățimea în pixeli. Nu coboară niciodată sub <see cref="MinWidth"/>.</summary>
    Public Property Width As Integer
        Get
            Return _width
        End Get
        Set(value As Integer)
            _width = Math.Max(value, _minWidth)
        End Set
    End Property

    ''' <summary>Lățimea minimă (px). Implicit 40. Ridicarea ei împinge și <see cref="Width"/>.</summary>
    Public Property MinWidth As Integer
        Get
            Return _minWidth
        End Get
        Set(value As Integer)
            _minWidth = Math.Max(0, value)
            If _width < _minWidth Then _width = _minWidth
        End Set
    End Property

    ''' <summary>Vizibilă. Implicit True. False => coloana nu se pictează și nu ocupă spațiu.</summary>
    Public Property Visible As Boolean = True

    ''' <summary>Coloană înghețată (non-scrolling): se randează la stânga, înaintea zonei derulate.</summary>
    Public Property Frozen As Boolean = False

    ''' <summary>Coloana nu intră niciodată în editare.</summary>
    Public Property [ReadOnly] As Boolean = False

    ''' <summary>Implicit True. False => întreaga coloană e ștearsă (gri) și inertă.</summary>
    Public Property Enabled As Boolean = True

    ''' <summary>Alinierea conținutului (se reutilizează enum-ul WinForms).</summary>
    Public Property TextAlign As ContentAlignment = ContentAlignment.MiddleLeft

    ''' <summary>Format .NET aplicat valorii la afișare (ex. „N2”, „dd.MM.yyyy”). Vid => ToString().</summary>
    Public Property FormatString As String

    ''' <summary>Sursa combo partajată pe coloană (override per-celulă prin evenimentul de formatare).</summary>
    Public Property ComboItems As IList(Of Object)

    ''' <summary>Minimul barei de progres (doar pentru <see cref="KBotColumnType.ProgressBar"/>).</summary>
    Public Property ProgressMin As Double = 0

    ''' <summary>Maximul barei de progres (doar pentru <see cref="KBotColumnType.ProgressBar"/>).</summary>
    Public Property ProgressMax As Double = 100

    ''' <summary>Redimensionabilă prin tragerea marginii din antet. Implicit True.</summary>
    Public Property Resizable As Boolean = True

    ''' <summary>Payload al caller-ului (nefolosit de control).</summary>
    Public Property Tag As Object

    ''' <summary>Cheia + tipul sunt fixate la creare; restul se lasă pe valorile implicite.</summary>
    Public Sub New(key As String, headerText As String, type As KBotColumnType, width As Integer)
        If String.IsNullOrWhiteSpace(key) Then Throw New ArgumentException("Cheie vidă.", NameOf(key))
        _Key = key
        _ColumnType = type
        HeaderText = If(headerText, String.Empty)
        _width = Math.Max(width, _minWidth)
    End Sub

End Class
