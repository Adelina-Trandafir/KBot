' ThemeManager/ThemeStore au stare statică partajată (Module). Dezactivăm paralelismul
' ca testele care mută schema activă / rădăcina de persistență să nu se calce reciproc.
<Assembly: Xunit.CollectionBehavior(DisableTestParallelization:=True)>
