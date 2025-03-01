///Taken from https://github.com/PTKay/ColoursXNCPGen/blob/master/ColoursXncpGen/Extensions.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColoursXncpGen
{
    public static class Extensions
    {
        public static byte[] Range(this byte[] in_Array, int in_Start, int in_Finish) {
            return in_Array.Skip(in_Start).Take(in_Finish - in_Start).ToArray();
        }
    }
}
