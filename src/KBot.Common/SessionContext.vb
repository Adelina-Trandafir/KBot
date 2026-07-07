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
    ' (db_name) și valoare DC în FX_Angajamente. Populat la login (felia login);
    ' până atunci se setează manual pentru rularea de probă.
    Public Property DbName As String = String.Empty

    ' --- Câmpuri adăugate de felia login (store-only pentru Role; enforcement ulterior). ---
    Public Property SessionId As Integer            ' IdLog din FX_LoginLog (> 0 după login)
    Public Property IdUnitate As Integer
    Public Property Role As String = String.Empty   ' Contabil / Administrator (neaplicat încă)
    ' Marcaj explicit de autentificare, setat de Populate / resetat de Clear.
    ' Distinct de IsLoaded (care rămâne derivat din CF pentru compatibilitatea seed-ului demo).
    Public Property IsAuthenticated As Boolean

    Public ReadOnly Property IsLoaded As Boolean
        Get
            Return Not String.IsNullOrEmpty(CF)
        End Get
    End Property

    ''' <summary>
    ''' Populează contextul dintr-un răspuns de login. Mapează DTO-ul de fir
    ''' (ANL -> An); username-ul e păstrat în OperatorName (numele istoric al câmpului).
    ''' Respinge un sessionId &lt;= 0 (garantează contractul AUTO_INCREMENT pe client).
    ''' </summary>
    Public Sub Populate(username As String, sessionId As Integer, ctx As SessionContextDto)
        If ctx Is Nothing Then Throw New ArgumentNullException(NameOf(ctx))
        If sessionId <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(sessionId))
        Me.OperatorName = username
        Me.SessionId = sessionId
        Me.Role = If(ctx.Role, String.Empty)
        Me.DbName = ctx.DbName
        Me.IdUnitate = ctx.IdUnitate
        Me.An = ctx.ANL
        Me.CodProgram = ctx.CodProgram
        Me.SectorSursa = ctx.SectorSursa
        Me.CF = ctx.CF
        Me.NumeUnitate = ctx.NumeUnitate
        Me.IsAuthenticated = True
    End Sub

    ''' <summary>Golește contextul (deconectare / închidere).</summary>
    Public Sub Clear()
        Me.IsAuthenticated = False
        Me.SessionId = 0
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
