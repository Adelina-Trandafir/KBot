Imports System.Collections.Generic
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Domain

' Singurul loc care va ști BaseUrl / token bearer / retry / timeout / JSON.
Public Interface IApiClient
    ''' <summary>
    ''' Trimite lista de angajamente la /api/forexe/angajamente/upsert.
    ''' Întoarce corpul brut al răspunsului. Hard-fail (Throw) la eroare.
    ''' </summary>
    Function UpsertAngajamenteAsync(dbName As String,
                                    rows As IReadOnlyList(Of Angajament),
                                    ct As CancellationToken) As Task(Of String)
    ''' <summary>
    ''' Aduce lista de angajamente pentru vederea-listă din MainForm (oglindește
    ''' Angajamente_SQL). Filtrează după COALESCE(IdUnitate,0)=idUnitate; doarAnulate
    ''' comută pe filtrul anulate/suspendat/ascuns. Hard-fail (Throw) la non-2xx;
    ''' fără retry pe 401 (curge spre WithReauth).
    ''' </summary>
    Function GetAngajamenteAsync(dbName As String, idUnitate As Integer, doarAnulate As Boolean,
                                 ct As CancellationToken) As Task(Of IReadOnlyList(Of Angajament))

    ''' <summary>
    ''' Aduce arborele de angajamente pentru MainForm (GET /api/forexe/tree), filtrat
    ''' pe an + SS. includeHidden readuce angajamentele ASCUNS (opțiunea btnOpt).
    ''' Baza NU se trimite: serverul o ia din sesiune (o bază = o unitate). Hard-fail
    ''' (Throw) la non-2xx; fără retry pe 401 (curge spre WithReauth).
    ''' </summary>
    Function GetTreeAsync(an As Integer, ss As String, includeHidden As Boolean,
                          ct As CancellationToken) As Task(Of IReadOnlyList(Of AngajamentTreeInfo))

    ''' <summary>
    ''' Aduce sumarul unui angajament (GET /api/forexe/sumar): antetul + un rând per
    ''' indicator. FĂRĂ filtru SS — sumarul arată toți indicatorii. Baza NU se trimite:
    ''' serverul o ia din sesiune. Un cod necunoscut întoarce un SumarInfo cu Header
    ''' Nothing și zero rânduri (nu excepție). Hard-fail (Throw) la non-2xx; fără retry
    ''' pe 401 (curge spre WithReauth).
    ''' </summary>
    Function GetSumarAsync(cod As String, ct As CancellationToken) As Task(Of SumarInfo)

    ''' <summary>
    ''' Aduce rezervările unui angajament (GET /api/forexe/rezervari): un rând per
    ''' înregistrare FX_Rezervari. Baza NU se trimite: serverul o ia din sesiune. Un cod
    ''' necunoscut întoarce un RezervariInfo cu zero rânduri (nu excepție). Hard-fail
    ''' (Throw) la non-2xx; fără retry pe 401 (curge spre WithReauth).
    ''' </summary>
    Function GetRezervariAsync(cod As String, ct As CancellationToken) As Task(Of RezervariInfo)

    ''' <summary>
    ''' Aduce recepțiile unui angajament (GET /api/forexe/receptii): un rând per linie
    ''' FX_Receptii (cu antet + receptie părinte) plus lista de plăți a angajamentului.
    ''' Baza NU se trimite: serverul o ia din sesiune. Un cod necunoscut întoarce un
    ''' ReceptiiInfo cu zero rânduri (nu excepție). Hard-fail (Throw) la non-2xx; fără
    ''' retry pe 401 (curge spre WithReauth).
    ''' </summary>
    Function GetReceptiiAsync(cod As String, ct As CancellationToken) As Task(Of ReceptiiInfo)

    ''' <summary>
    ''' Trimite un Excel (base64) la server pentru conversie în JSON (/api/tools/process_excel).
    ''' Întoarce conținutul câmpului "data" din răspuns. Autorizare: bearer-ul sesiunii
    ''' curente (în ApiClient). Hard-fail (Throw ApiException) la non-2xx.
    ''' </summary>
    Function ProcessExcelAsync(job As ExcelJob, ct As CancellationToken) As Task(Of String)

    Function GetAsync(Of T)(relativeUrl As String, ct As CancellationToken) As Task(Of T)
    Function PostAsync(Of TRequest, TResponse)(relativeUrl As String, payload As TRequest, ct As CancellationToken) As Task(Of TResponse)
End Interface
