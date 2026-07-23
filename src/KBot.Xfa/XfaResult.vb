Option Strict On

' Rezultatul unei operații XFA. Înlocuiește codurile de ieșire ale exe-ului XFA_WRITTER
' (citite de Access prin WScript.Shell.Run): acum apelantul din K-BOT primește direct un
' obiect, în proces, fără să mai lanseze un .exe separat.
Public NotInheritable Class XfaResult
    ''' <summary>Operația s-a încheiat fără eroare (PDF generat / verificat).</summary>
    Public ReadOnly Property Reusit As Boolean
    ''' <summary>Cel puțin un semnatar a semnat (masca <> 0).</summary>
    Public ReadOnly Property Semnat As Boolean
    ''' <summary>Masca semnatarilor (AB=1 | CD=2 | Ordonator=4). 0 = nesemnat.</summary>
    Public ReadOnly Property Masca As Integer
    ''' <summary>Calea PDF-ului rezultat / semnat.</summary>
    Public ReadOnly Property CaleOutputPdf As String

    ''' <summary>A semnat AB (ORD) / A (DDF)?</summary>
    Public ReadOnly Property SemnatAB As Boolean
        Get
            Return (Masca And AdobeUtils.SIGNER_AB) <> 0
        End Get
    End Property
    ''' <summary>A semnat CD (ORD) / B (DDF)?</summary>
    Public ReadOnly Property SemnatCD As Boolean
        Get
            Return (Masca And AdobeUtils.SIGNER_CD) <> 0
        End Get
    End Property
    ''' <summary>A semnat Ordonatorul?</summary>
    Public ReadOnly Property SemnatOrdonator As Boolean
        Get
            Return (Masca And AdobeUtils.SIGNER_ORDONATOR) <> 0
        End Get
    End Property

    ''' <summary>
    ''' Codul de ieșire echivalent cu vechiul exe (pentru compatibilitate/telemetrie):
    ''' 0 = OK generat fără semnare, 2 = nesemnat, 11..17 = semnat (10 + mască).
    ''' </summary>
    Public ReadOnly Property CodIesireLegacy As Integer

    Private Sub New(reusit As Boolean, masca As Integer, caleOutputPdf As String, codLegacy As Integer)
        Me.Reusit = reusit
        Me.Masca = masca
        Me.Semnat = masca > 0
        Me.CaleOutputPdf = caleOutputPdf
        Me.CodIesireLegacy = codLegacy
    End Sub

    ''' <summary>Rezultat pentru generare fără semnare (cod legacy 0).</summary>
    Public Shared Function Generat(caleOutputPdf As String) As XfaResult
        Return New XfaResult(True, 0, caleOutputPdf, 0)
    End Function

    ''' <summary>Rezultat pentru un mod de semnare: masca &lt;= 0 → 2 (nesemnat), altfel 10 + masca.</summary>
    Public Shared Function DupaSemnare(masca As Integer, caleOutputPdf As String) As XfaResult
        Dim cod As Integer = If(masca <= 0, 2, 10 + masca)
        Return New XfaResult(True, Math.Max(masca, 0), caleOutputPdf, cod)
    End Function
End Class
