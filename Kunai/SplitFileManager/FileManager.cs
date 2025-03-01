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
        private static byte[] ms_Signature = { 0x46, 0x41, 0x50, 0x43 };
        private static byte[] ms_Signature2 = { 0x43, 0x50, 0x41, 0x46};

        public static byte[] Combine(byte[] in_File, byte[] in_DxlFile)
        {
            byte[] output = new byte[ms_Signature.Length * 4 + in_File.Length + in_DxlFile.Length];
            int lastIndex = 0;
            Array.Copy(ms_Signature, 0, output, lastIndex += 0, ms_Signature.Length);
            Array.Copy(BitConverter.GetBytes((uint)in_File.Length), 0, output, lastIndex += ms_Signature.Length, 4);
            Array.Copy(in_File, 0, output, lastIndex += 4, in_File.Length);

            Array.Copy(BitConverter.GetBytes((uint)in_DxlFile.Length), 0, output, lastIndex += in_File.Length, 4);
            Array.Copy(in_DxlFile, 0, output, lastIndex += 4, in_DxlFile.Length);

            return output;
        }

        public static byte[][] Split(byte[] in_File)
        {
            uint xncpLength = BitConverter.ToUInt32(in_File.Range(4, 8), 0);
            byte[] xncp = new byte[xncpLength];
            Array.Copy(in_File, 8, xncp, 0, xncpLength);

            byte[] dxlLengthBytes = in_File.Range(8 + (int)xncpLength, 8 + (int)xncpLength + 4);
            uint dxlLength = BitConverter.ToUInt32(dxlLengthBytes, 0);
            byte[] dxl = new byte[dxlLength];
            Array.Copy(in_File, 8 + 4 + xncpLength, dxl, 0, dxlLength);
            
            return new byte[][] { xncp, dxl };
        }
    }
}
