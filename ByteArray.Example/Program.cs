using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteArray.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            var nex = new Flash.Utils.ByteArray();
            nex.Endian = Flash.Utils.Endian.BIG_ENDIAN;
            nex.WriteInt(1);
            Console.WriteLine(BitConverter.ToString(nex.Bytes));
            Console.ReadLine();
        }
    }
}
