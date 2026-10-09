Option Strict On
Imports System.Collections.Generic
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Domain

''' <summary>
''' The groups of angajamente (slice 0008-02): <c>/api/forexe/grupe...</c> in
''' <c>routes/forexe/grupe.py</c>. Kept out of <see cref="IApiClient"/>, like
''' <see cref="INomenclatoareApi"/>: only the groups window and the tree menu call it.
''' <see cref="ApiClient"/> implements it. Every method throws <see cref="ApiException"/> on a
''' non-2xx, with the server's Romanian message.
''' </summary>
Public Interface IGrupeApi

    ''' <summary>Every group with its members, and every visible angajament with its alias and indicators.</summary>
    Function GetGrupeAsync(ct As CancellationToken) As Task(Of GrupeCatalog)

    ''' <summary>
    ''' Saves a group whole (name, colour #RRGGBB, members) and returns its number.
    ''' <paramref name="idGr"/> Nothing = a new group.
    ''' </summary>
    Function SaveGrupaAsync(idGr As Integer?, denumire As String, culoare As String,
                            coduri As IReadOnlyList(Of String), ct As CancellationToken) As Task(Of Integer)

    ''' <summary>Adds one angajament to an existing group (the drag and drop).</summary>
    Function AdaugaInGrupaAsync(idGr As Integer, cod As String, ct As CancellationToken) As Task

    ''' <summary>Sets the alias of an angajament; an empty alias removes it.</summary>
    Function SaveAliasAsync(cod As String, aliasAng As String, ct As CancellationToken) As Task

End Interface
