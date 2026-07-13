Imports KBot.Domain

' Înlocuiește globalele VBA (globANL, globCF, globNumeUnit, globCodProgram, globSectorSursa...).
' Identitatea se încarcă o singură dată la login (Populate); anul / SS / CodProgram
' devin valori de RUNTIME, setate de MainForm după login (din /api/auth/periods).
' Instanță singleton injectată (vezi Program.ConfigureServices).
Public Class SessionContext
    ' --- identitate, din login ---
    Public Property OperatorName As String = String.Empty   ' acum e-mailul operatorului
    Public Property Token As String = String.Empty
    Public Property CF As String = String.Empty
    Public Property NumeUnitate As String = String.Empty
    Public Property Role As String = String.Empty   ' Contabil / Administrator (neaplicat încă)
    ' Numele bazei de date a unității (ex. "018_GRRS"). Devine țintă a upsert-ului
    ' (db_name) și valoare DC în FX_Angajamente. Populat la login.
    Public Property DbName As String = String.Empty

    ' --- runtime, setate pe MainForm după login (SetPeriod) ---
    Public Property An As Integer
    Public Property SectorSursa As String = String.Empty
    Public Property CodProgram As String = String.Empty
    ' Hint venit de la login: ultimul SS ales de utilizator pe această bază (poate fi
    ' Nothing). MainForm îl folosește pentru a preselecta SS-ul; nu e o valoare de lucru.
    Public Property LastSS As String

    ' Token-ul bearer opac emis de server la login. Este SINGURA credențială pe
    ' care clientul o deține după fluxul de login; ApiClient îl trimite per-request
    ' (Authorization: Bearer). Golit de Clear / la deconectare.

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
    ''' Populează IDENTITATEA dintr-un răspuns de login. Anul / SS / CodProgram NU se
    ''' setează aici — sunt valori de runtime (vezi SetPeriod). Username-ul e păstrat în
    ''' OperatorName (numele istoric al câmpului). Respinge un token gol.
    ''' </summary>
    Public Sub Populate(username As String, token As String, ctx As SessionContextDto)
        If ctx Is Nothing Then Throw New ArgumentNullException(NameOf(ctx))
        If String.IsNullOrEmpty(token) Then Throw New ArgumentException("Token de sesiune gol.", NameOf(token))
        Me.OperatorName = username
        Me.Token = token
        Me.Role = If(ctx.Role, String.Empty)
        Me.DbName = ctx.DbName
        Me.CF = ctx.CF
        Me.NumeUnitate = ctx.NumeUnitate
    End Sub

    ''' <summary>
    ''' Fixează perioada de lucru (an + SS + CodProgram), aleasă pe MainForm din catalog.
    ''' </summary>
    Public Sub SetPeriod(an As Integer, ss As String, codProgram As String)
        Me.An = an
        Me.SectorSursa = ss
        Me.CodProgram = codProgram
    End Sub

    ''' <summary>Golește contextul (deconectare / închidere).</summary>
    Public Sub Clear()
        Me.Token = String.Empty
        Me.OperatorName = String.Empty
        Me.Role = String.Empty
        Me.DbName = String.Empty
        Me.CF = String.Empty
        Me.NumeUnitate = String.Empty
        Me.An = 0
        Me.SectorSursa = String.Empty
        Me.CodProgram = String.Empty
        Me.LastSS = Nothing
    End Sub
End Class
