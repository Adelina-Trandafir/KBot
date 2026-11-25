Option Strict On
Imports System.Globalization
Imports System.Text

''' <summary>
''' The entries of the choice boxes (payments, bons, partners).
''' </summary>
Partial Public Class CommandsView

    ' One payment of the source child, as the choice list shows it: Key = IDPL.
    Private NotInheritable Class PayChoice
        Public ReadOnly Key As String
        Public ReadOnly Caption As String

        Public Sub New(k_key As String, k_caption As String)
            Key = k_key
            Caption = k_caption
        End Sub

        Public Overrides Function ToString() As String
            Return Caption
        End Function
    End Class

    ' One partner of the choice box: Key = the CodFiscal (normalized by the server), the one value the command takes.
    Private NotInheritable Class PartnerChoice
        Public ReadOnly Key As String
        Public ReadOnly Caption As String

        Public Sub New(k_key As String, k_caption As String)
            Key = k_key
            Caption = k_caption
        End Sub

        Public Overrides Function ToString() As String
            Return Caption
        End Function
    End Class

    ' One bon the deletion search found: Key = SubunitId/GestiuneId/ExercitiuId/IdBonConsum, the same identifier the server accepts as «ID Key».
    Private NotInheritable Class BonChoice
        Public ReadOnly Key As String
        Public ReadOnly Caption As String

        Public Sub New(k_key As String, k_caption As String)
            Key = k_key
            Caption = k_caption
        End Sub

        Public ReadOnly Property Selector As String
            Get
                Return "ID " & Key
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return Caption
        End Function
    End Class

End Class
