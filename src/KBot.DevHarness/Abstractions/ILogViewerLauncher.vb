Option Strict On
Imports System.Windows.Forms

''' <summary>
''' Puntea prin care bancul de probă deschide vizualizatorul de jurnale (felia 0031-04).
'''
''' <para>Există fiindcă <c>LogViewerForm</c> trăiește în <c>KBot.App</c> — acolo îi e locul, ca să
''' rămână și în Release și ca legarea lui în shell să coste două linii — iar <c>KBot.DevHarness</c>
''' NU referă <c>KBot.App</c> (referința merge invers). Fără punte, ori mutam fereastra în banc (și
''' Release-ul o pierdea), ori bancul căpăta o referință pe care n-are voie s-o aibă.</para>
'''
''' <para>Implementarea stă în <c>KBot.App</c> și se înregistrează în DI (<c>Program.vb</c>). Un
''' test care n-o găsește NU trebuie să pice — înseamnă doar că rulează într-o gazdă fără shell.</para>
''' </summary>
Public Interface ILogViewerLauncher

    ''' <summary>
    ''' Construiește o instanță NOUĂ a vizualizatorului. Apelantul o deține: el o arată (modal sau
    ''' nu) și tot el o eliberează.
    ''' </summary>
    Function CreateLogViewer() As Form

End Interface
