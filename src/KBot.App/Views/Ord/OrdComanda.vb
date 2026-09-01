Option Strict On
Imports KBot.Domain

''' <summary>
''' Cele patru comenzi de SCRIERE ale ordonantarii (felia 0049), pe care <c>OrdView</c> le
''' cere si <c>MainForm</c> le executa.
'''
''' <para>De ce trece prin shell si nu se face in vedere: fiecare comanda are nevoie de plasa
''' de re-autentificare pe una sau mai multe forme de raspuns, iar <c>WithReauth</c> e privat
''' si generic in <c>MainForm</c>. Vederea primeste o singura actiune, deci politica de
''' re-login ramane, ca peste tot, intr-un singur loc — exact tiparul lui
''' <c>DeschideLegaturileReceptiilor</c> din felia 0048-04.</para>
''' </summary>
Public Enum OrdActiune
    ''' <summary>Genereaza o ordonantare noua pentru o zi si deschide editorul.</summary>
    Adauga = 0
    ''' <summary>Deschide editorul pe ordonantarea selectata.</summary>
    Modifica = 1
    ''' <summary>Sterge ordonantarea selectata, cu tot ce atarna de ea.</summary>
    Sterge = 2
    ''' <summary>Genereaza si salveaza, fara interactiune, cate o ordonantare pentru fiecare zi
    ''' cu plati neordonantate.</summary>
    Lot = 3
End Enum

''' <summary>
''' O comanda de scriere, cu tot ce-i trebuie ca sa fie executata in afara vederii.
''' POCO -&gt; fara Try/Catch.
''' </summary>
Public NotInheritable Class OrdComanda
    Public ReadOnly Property Actiune As OrdActiune
    Public ReadOnly Property Cod As String
    ''' <summary>Ordonantarea vizata; <c>Nothing</c> pentru <see cref="OrdActiune.Adauga"/> si
    ''' <see cref="OrdActiune.Lot"/>.</summary>
    Public ReadOnly Property Ordonantare As OrdHeaderRow

    Public Sub New(actiune As OrdActiune, cod As String, Optional ordonantare As OrdHeaderRow = Nothing)
        Me.Actiune = actiune
        Me.Cod = If(cod, String.Empty)
        Me.Ordonantare = ordonantare
    End Sub
End Class
