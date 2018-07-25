namespace VSOmniBox.API.Data
{
    using System;

    [Flags]
    public enum OmniBoxPivot
    {
        Code = 0b0001,
        IDE  = 0b0010,
        Help = 0b0100
    }
}
