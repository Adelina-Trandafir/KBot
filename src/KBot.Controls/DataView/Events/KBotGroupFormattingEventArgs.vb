Option Strict On
Imports System.Drawing

''' <summary>
''' Argumentele evenimentului <c>GroupFormatting</c> (slice 0029) — ridicat O DATĂ pentru fiecare
''' bandă de grup pictată, antet sau subsol, ÎNAINTEA desenului ei.
'''
''' <para>Nivelul (<see cref="KBotGroupLevel"/>) spune cum arată TOATE grupurile lui; asta spune
''' cum arată UNUL. Aici se colorează grupul care iese din tipar — luna cu depășire pe roșu,
''' capitolul fără plăți pe gri — iar <see cref="Caption"/> se poate rescrie cu totul când șablonul
''' nivelului nu ajunge.</para>
'''
''' <para>ATENȚIE: instanța e REFOLOSITĂ de control pentru fiecare bandă pictată. Handler-ul NU
''' are voie să o rețină după ieșirea din event.</para>
''' </summary>
Public NotInheritable Class KBotGroupFormattingEventArgs
    Inherits EventArgs

    ''' <summary>Antet sau subsol de grup (niciodată <c>Data</c>).</summary>
    Public ReadOnly Property BandKind As KBotGroupBandKind
        Get
            Return _bandKind
        End Get
    End Property
    Private _bandKind As KBotGroupBandKind

    ''' <summary>Nivelul căruia îi aparține banda (0 = cel dinafară).</summary>
    Public ReadOnly Property Level As Integer
        Get
            Return _level
        End Get
    End Property
    Private _level As Integer

    ''' <summary>Definiția nivelului — pentru citirea proprietăților lui.</summary>
    Public ReadOnly Property GroupLevel As KBotGroupLevel
        Get
            Return _groupLevel
        End Get
    End Property
    Private _groupLevel As KBotGroupLevel

    ''' <summary>Valoarea BRUTĂ a grupului (cea din celulă, neformatată).</summary>
    Public ReadOnly Property Value As Object
        Get
            Return _value
        End Get
    End Property
    Private _value As Object

    ''' <summary>Valoarea grupului așa cum se AFIȘEAZĂ (formatată ca celulele coloanei).</summary>
    Public ReadOnly Property DisplayValue As String
        Get
            Return _displayValue
        End Get
    End Property
    Private _displayValue As String

    ''' <summary>Câte rânduri de date are grupul (inclusiv cele ale sub-grupurilor lui).</summary>
    Public ReadOnly Property RowCount As Integer
        Get
            Return _rowCount
        End Get
    End Property
    Private _rowCount As Integer

    ''' <summary>Grupul e strâns acum?</summary>
    Public ReadOnly Property Collapsed As Boolean
        Get
            Return _collapsed
        End Get
    End Property
    Private _collapsed As Boolean

    ''' <summary>Titlul benzii, deja compus din șablonul nivelului. Se poate rescrie.</summary>
    Public Property Caption As String

    ''' <summary>Fundalul benzii (implicit: al nivelului, sau al temei).</summary>
    Public Property BackColor As Color

    ''' <summary>Culoarea textului din bandă.</summary>
    Public Property ForeColor As Color

    ''' <summary>Fontul benzii.</summary>
    Public Property Font As Font

    ''' <summary>Re-inițializează instanța refolosită înaintea unei noi ridicări.</summary>
    Friend Sub Reset(bandKind As KBotGroupBandKind, level As Integer, groupLevel As KBotGroupLevel,
                     value As Object, displayValue As String, rowCount As Integer, collapsed As Boolean,
                     caption As String, back As Color, fore As Color, font As Font)
        _bandKind = bandKind
        _level = level
        _groupLevel = groupLevel
        _value = value
        _displayValue = displayValue
        _rowCount = rowCount
        _collapsed = collapsed
        Me.Caption = caption
        Me.BackColor = back
        Me.ForeColor = fore
        Me.Font = font
    End Sub

End Class
