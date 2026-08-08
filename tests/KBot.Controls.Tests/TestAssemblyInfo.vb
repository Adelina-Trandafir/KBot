' Testele de aici construiesc controale WinForms REALE, fiecare pe firul lui STA (vezi RunSta).
' WinForms ține stare statică per-proces care NU e thread-safe — în special dicționarul de
' handle-uri din NativeWindow: două fire care creează handle-uri în același timp îl corup, iar
' simptomul e un IndexOutOfRangeException din Dictionary.TryInsert, aparent fără legătură cu
' testul care pică. xUnit rulează implicit clasele de test în paralel, deci dezactivăm asta —
' exact motivul pentru care KBot.Theming.Tests o face deja (acolo, pentru starea din ThemeManager).
<Assembly: Xunit.CollectionBehavior(DisableTestParallelization:=True)>
