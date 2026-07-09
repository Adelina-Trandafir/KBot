Imports KBot.Domain

' Înlocuiește globalele VBA (globANL, globCF, globNumeUnit, globCodProgram, globSectorSursa...).
' Se încarcă o singură dată după login, din API (felia login populează prin Populate).
' Instanță singleton injectată (vezi Program.ConfigureServices).
Public Class SessionContext
    Public Property OperatorName As String = String.Empty
    Public Property CF As String = String.Empty
    Public Property NumeUnitate As String = String.Empty
    Public Property An As Integer
    Public Property CodProgram As String = String.Empty
    Public Property SectorSursa As String = String.Empty
    ' Numele bazei de date a unității (ex. "018_GRRS"). Devine țintă a upsert-ului
    ' (db_name) și valoare DC în FX_Angajamente. Populat la login.
    Public Property DbName As String = String.Empty

    ' Token-ul bearer opac emis de server la login. Este SINGURA credențială pe
    ' care clientul o deține după fluxul de login; ApiClient îl trimite per-request
    ' (Authorization: Bearer). Golit de Clear / la deconectare.
    Public Property Token As String = String.Empty
    Public Property IdUnitate As Integer
    Public Property Role As String = String.Empty   ' Contabil / Administrator (neaplicat încă)

    ' Autentificat == avem un token viu de la server. Derivat, nu setat manual.
    Public ReadOnly Property IsAuthenticated As Boolean
        Get
            Return Not String.IsNullOrEmpty(Token)
        End Get
    End Property

    Public ReadOnly Property IsLoaded As Boolean
        Get
            Return Not String.IsNullOrEmpty(CF)
        End Get
    End Property

    ''' <summary>
    ''' Populează contextul dintr-un răspuns de login. Mapează DTO-ul de fir
    ''' (ANL -> An); username-ul e păstrat în OperatorName (numele istoric al câmpului).
    ''' Respinge un token gol (garantează contractul de emitere pe client).
    ''' </summary>
    Public Sub Populate(username As String, token As String, ctx As SessionContextDto)
        If ctx Is Nothing Then Throw New ArgumentNullException(NameOf(ctx))
        If String.IsNullOrEmpty(token) Then Throw New ArgumentException("Token de sesiune gol.", NameOf(token))
        Me.OperatorName = username
        Me.Token = token
        Me.Role = If(ctx.Role, String.Empty)
        Me.DbName = ctx.DbName
        Me.IdUnitate = ctx.IdUnitate
        Me.An = ctx.ANL
        Me.CodProgram = ctx.CodProgram
        Me.SectorSursa = ctx.SectorSursa
        Me.CF = ctx.CF
        Me.NumeUnitate = ctx.NumeUnitate
    End Sub

    ''' <summary>Golește contextul (deconectare / închidere).</summary>
    Public Sub Clear()
        Me.Token = String.Empty
        Me.OperatorName = String.Empty
        Me.Role = String.Empty
        Me.DbName = String.Empty
        Me.IdUnitate = 0
        Me.An = 0
        Me.CodProgram = String.Empty
        Me.SectorSursa = String.Empty
        Me.CF = String.Empty
        Me.NumeUnitate = String.Empty
    End Sub
End Class
