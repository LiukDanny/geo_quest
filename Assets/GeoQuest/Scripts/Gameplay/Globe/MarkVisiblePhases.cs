using System;

[Flags]
public enum MarkVisiblePhases
{
    Continents = 1 << 0,
    Countries = 1 << 1,
    Both = Continents | Countries
}
