Option Strict On
Imports KBot.Domain

''' <summary>
''' Cele patru comenzi de SCRIERE ale ordonantarii (felia 0049), pe care <c>OrdView</c> si
''' <c>PlatiView</c> le cer, iar <c>MainForm</c> le executa.
'''
''' <para>Doua vederi le cer, cu tot cu ce STIE fiecare. <c>OrdView</c> cere «Adauga» fara zi,
''' deci ziua se cere operatorului. <c>PlatiView</c> cere de pe «+»-ul arborelui de plati, unde
''' ziua (sau luna) e chiar nodul apasat — Access: <c>fxPlati_AdaugareOrdonantare</c> si
''' <c>fxPlati_AdaugareOrdonantari</c> din <c>frmFX_MAIN</c>.</para>
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

    ''' <summary>
    ''' Ziua ordonantarii, cand comanda vine de pe «+»-ul unei ZILE din arborele de plati.
    ''' <c>Nothing</c> = ziua nu e stiuta si se cere operatorului (<c>OrdZiuaForm</c>) — calea
    ''' prin care intra <c>OrdView</c>. Numai pentru <see cref="OrdActiune.Adauga"/>.
    ''' </summary>
    Public ReadOnly Property Ziua As Date?

    ''' <summary>
    ''' O SINGURA plata de acoperit (VBA: <c>vIdPlataFX</c>); <c>Nothing</c> = toate platile
    ''' neordonantate ale zilei (VBA: <c>vIdPlataFX = -1</c>, adica <c>sIdPlataFX = "*"</c>).
    ''' Numai pentru <see cref="OrdActiune.Adauga"/>.
    ''' </summary>
    Public ReadOnly Property IdPlataFx As Integer?

    ''' <summary>Luna careia i se face lotul (1-12); <c>Nothing</c> = toate lunile.
    ''' VBA: o jumatate din <c>vLunaAn</c>. Numai pentru <see cref="OrdActiune.Lot"/>.</summary>
    Public ReadOnly Property Luna As Integer?

    ''' <summary>Anul caruia i se face lotul; <c>Nothing</c> = toti anii. VBA: cealalta
    ''' jumatate din <c>vLunaAn</c>. Numai pentru <see cref="OrdActiune.Lot"/>.</summary>
    Public ReadOnly Property An As Integer?

    Public Sub New(actiune As OrdActiune, cod As String, Optional ordonantare As OrdHeaderRow = Nothing)
        Me.Actiune = actiune
        Me.Cod = If(cod, String.Empty)
        Me.Ordonantare = ordonantare
    End Sub

    Private Sub New(actiune As OrdActiune, cod As String, ziua As Date?, idPlataFx As Integer?,
                    luna As Integer?, an As Integer?)
        Me.Actiune = actiune
        Me.Cod = If(cod, String.Empty)
        Me.Ziua = ziua
        Me.IdPlataFx = idPlataFx
        Me.Luna = luna
        Me.An = an
    End Sub

    ''' <summary>
    ''' «+» pe o ZI din arborele de plati — portul lui <c>FX_Adaugare_ORD_Din_Plati</c>: ziua
    ''' e stiuta, deci nu se mai cere. <paramref name="idPlataFx"/> lasat <c>Nothing</c>
    ''' inseamna «toata ziua» (VBA: <c>-1</c>).
    ''' </summary>
    Public Shared Function DinPlati(cod As String, ziua As Date,
                                    Optional idPlataFx As Integer? = Nothing) As OrdComanda
        Return New OrdComanda(OrdActiune.Adauga, cod, ziua, idPlataFx, Nothing, Nothing)
    End Function

    ''' <summary>
    ''' «+» pe o LUNA din arborele de plati — portul lui
    ''' <c>FX_Adaugare_ORD_Din_Plati_Batch(CodAng, vLunaAn)</c>.
    ''' </summary>
    Public Shared Function LotPeLuna(cod As String, luna As Integer?, an As Integer?) As OrdComanda
        Return New OrdComanda(OrdActiune.Lot, cod, Nothing, Nothing, luna, an)
    End Function
End Class
