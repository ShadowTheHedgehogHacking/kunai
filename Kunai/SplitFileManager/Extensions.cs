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
        public static byte[] Range(this byte[] array, int start, int finish) {
            return array.Skip(start).Take(finish - start).ToArray();
        }
    }
}
