using System;

namespace Kunai;
public static class EnumsEx
{    
    public static T SetFlag<T>(this Enum in_Value, T in_Flag, bool in_Set)
    {
        //https://stackoverflow.com/questions/5850873/enum-hasflag-why-no-enum-setflag
        Type underlyingType = Enum.GetUnderlyingType(in_Value.GetType());

        // note: AsInt mean: math integer vs enum (not the c# int type)
        dynamic valueAsInt = Convert.ChangeType(in_Value, underlyingType);
        dynamic flagAsInt = Convert.ChangeType(in_Flag, underlyingType);
        if (in_Set)
        {
            valueAsInt |= flagAsInt;
        }
        else
        {
            valueAsInt &= ~flagAsInt;
        }

        return (T)valueAsInt;
    }

}
