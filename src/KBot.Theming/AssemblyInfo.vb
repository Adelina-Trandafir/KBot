Imports System.Runtime.CompilerServices

' Testele văd membrii Friend (ex. ThemeStore.OverrideRootForTests) ca să poată
' rula round-trip-ul de persistență împotriva unui director temporar.
<Assembly: InternalsVisibleTo("KBot.Theming.Tests")>
' Slice 0062: the control tests need AppScaling.LoadFrom (set the scale WITHOUT persisting) so a
' scale test does not write the operator's real theme.json the way Configure would.
<Assembly: InternalsVisibleTo("KBot.Controls.Tests")>
