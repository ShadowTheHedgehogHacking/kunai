///Taken from https://github.com/PTKay/ColoursXNCPGen/blob/master/ColoursXncpGen/FileManager.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColoursXncpGen
{
    class FileManager
    {
        private static byte[] signature = { 0x46, 0x41, 0x50, 0x43 };
        private static byte[] signature2 = { 0x43, 0x50, 0x41, 0x46};

        public static byte[] Combine(byte[] file, byte[] dxlFile)
        {
            byte[] output = new byte[signature.Length * 4 + file.Length + dxlFile.Length];
            int lastIndex = 0;
            Array.Copy(signature, 0, output, lastIndex += 0, signature.Length);
            Array.Copy(BitConverter.GetBytes((uint)file.Length), 0, output, lastIndex += signature.Length, 4);
            Array.Copy(file, 0, output, lastIndex += 4, file.Length);

            Array.Copy(BitConverter.GetBytes((uint)dxlFile.Length), 0, output, lastIndex += file.Length, 4);
            Array.Copy(dxlFile, 0, output, lastIndex += 4, dxlFile.Length);

            return output;
        }

        public static byte[][] Split(byte[] file)
        {
            uint xncpLength = BitConverter.ToUInt32(file.Range(4, 8), 0);
            byte[] xncp = new byte[xncpLength];
            Array.Copy(file, 8, xncp, 0, xncpLength);

            byte[] dxlLengthBytes = file.Range(8 + (int)xncpLength, 8 + (int)xncpLength + 4);
            uint dxlLength = BitConverter.ToUInt32(dxlLengthBytes, 0);
            byte[] dxl = new byte[dxlLength];
            Array.Copy(file, 8 + 4 + xncpLength, dxl, 0, dxlLength);
            
            return new byte[][] { xncp, dxl };
        }
    }
}
