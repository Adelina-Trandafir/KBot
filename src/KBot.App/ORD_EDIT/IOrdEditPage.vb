Option Strict On
Imports KBot.Domain

''' <summary>
''' Contractul unei pagini a EDITORULUI de ordonantare (felia 0049) — sora lui
''' <see cref="IOrdPage"/>, dar pentru scriere.
'''
''' <para>Cele trei ferestre pop-up ale Access-ului (<c>frmFX_ORD_DOC</c>,
''' <c>frmFX_ORD_PRTSCR</c> si subformularele lor) au devenit trei PAGINI ale aceluiasi
''' formular, in spatele unui <c>KBotNavList</c> orizontal — exact forma pe care o folosesc
''' deja <c>OrdView</c> si <c>DdfView</c>. Consecinta e ca <c>btnSav</c> de pe
''' <c>frmFX_ORD_DOC</c> dispare: exista O SINGURA salvare, a formularului, pentru tot graful.</para>
'''
''' <para>Ca la <see cref="IOrdPage"/>, pagina NU are client API si NU face nicio cerere de
''' retea — dar, spre deosebire de acolo, pagina MODIFICA obiectul primit. Draftul e tinut de
''' formular si dat paginilor prin referinta: toate trei scriu in acelasi graf, deci o
''' schimbare facuta pe o pagina e vizibila pe celelalte fara nicio sincronizare.</para>
'''
''' <para>Efect secundar deliberat: fara injectie de dependente, fiecare pagina are un
''' constructor FARA parametri, deci se instantiaza in designerul Visual Studio.</para>
''' </summary>
Public Interface IOrdEditPage

    ''' <summary>
    ''' Cheia paginii: «beneficiari», «documente» sau «atasamente». Trebuie sa fie IDENTICA
    ''' cu cheia intrarii din <c>navSub</c> (designerul le scrie ca literale).
    ''' </summary>
    ReadOnly Property PageKey As String

    ''' <summary>
    ''' Da paginii graful pe care il editeaza. <c>Nothing</c> = niciun document deschis -&gt;
    ''' pagina isi arata starea goala. Se apeleaza la fiecare activare a paginii, deci o
    ''' pagina creata tarziu vede tot ce s-a schimbat inainte de ea.
    ''' </summary>
    Sub SetDraft(draft As OrdDraft)

    ''' <summary>
    ''' Ceva din graf s-a schimbat pe pagina asta. Formularul reimprospateaza banda de antet
    ''' (totalul) si marcheaza documentul ca nesalvat.
    ''' </summary>
    Event DraftModificat As EventHandler

End Interface
