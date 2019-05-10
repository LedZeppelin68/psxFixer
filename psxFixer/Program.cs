using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace psxFixer
{
    class Program
    {
        internal static uint[] edc_lut = new uint[256];

        internal static byte[] ecc_f_lut = new byte[256];

        internal static byte[] ecc_b_lut = new byte[256];

        private static byte[] subHeader;

        private static byte[] extremePattern;

        private static void FillEDCECCLuts()
        {
            for (uint num = 0u; num < 256u; num += 1u)
            {
                uint num2 = (uint)((ulong)((ulong)num << 1) ^ (ulong)(((num & 128u) != 0u) ? 285L : 0L));
                Program.ecc_f_lut[(int)((UIntPtr)num)] = (byte)num2;
                Program.ecc_b_lut[(int)((UIntPtr)(num ^ num2))] = (byte)num;
                uint num3 = num;
                for (num2 = 0u; num2 < 8u; num2 += 1u)
                {
                    num3 = (num3 >> 1 ^ (((num3 & 1u) != 0u) ? 3623976961u : 0u));
                }
                Program.edc_lut[(int)((UIntPtr)num)] = num3;
            }
        }

        private static void CalculateEDC(ref byte[] buffer, int offset, int count)
        {
            uint num = 0u;
            int i = 0;
            while (i != count)
            {
                num = (num >> 8 ^ Program.edc_lut[(int)((UIntPtr)((num ^ (uint)buffer[offset + i++]) & 255u))]);
            }
            byte[] bytes = BitConverter.GetBytes(num);
            for (i = 0; i < 4; i++)
            {
                buffer[16 + count + i] = bytes[i];
            }
        }

        private static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                Program.FillEDCECCLuts();
                Console.WriteLine("PSX fixer v0.1\n\r");
                string file = args[0];
                Program.utility1(file);
                Program.utility2(file);
            }
        }

        private static void utility2(string file)
        {
            BinaryReader binaryReader = new BinaryReader(new FileStream(file, FileMode.Open));
            Console.WriteLine("2 utility: last sector fix in noEDC image");
            Console.Write("Fix last sector?: ");
            string text = Console.ReadLine();
            if (text != null)
            {
                if (text == "y")
                {
                    binaryReader.BaseStream.Position = binaryReader.BaseStream.Length - 2352L;
                    byte[] array = binaryReader.ReadBytes(2352);
                    BinaryReader binaryReader2 = new BinaryReader(new MemoryStream(array));
                    byte[] array2 = binaryReader2.ReadBytes(12);
                    binaryReader2.BaseStream.Position = 0L;
                    for (int i = 0; i < 12; i++)
                    {
                        binaryReader2.BaseStream.WriteByte(0);
                    }
                    Program.CalculateEDC(ref array, 16, 2056);
                    Program.CalculateECCP(ref array);
                    Program.CalculateECCQ(ref array);
                    binaryReader2.BaseStream.Position = 0L;
                    binaryReader2.BaseStream.Write(array2, 0, array2.Length);
                    binaryReader2.Close();
                    binaryReader.BaseStream.Position = binaryReader.BaseStream.Length - 2352L;
                    binaryReader.BaseStream.Write(array, 0, array.Length);
                }
            }
            binaryReader.Close();
        }

        private static void utility1(string file)
        {
            Console.WriteLine("1 utility: Extremegames hack");
            Console.WriteLine("Checking 16th sector");
            BinaryReader binaryReader = new BinaryReader(new FileStream(file, FileMode.Open));
            binaryReader.BaseStream.Position = 37632L;
            byte[] buffer = binaryReader.ReadBytes(2352);
            BinaryReader binaryReader2 = new BinaryReader(new MemoryStream(buffer));
            byte[] array = binaryReader2.ReadBytes(16);
            byte[] second = binaryReader2.ReadBytes(8);
            binaryReader2.BaseStream.Position = 685L;
            byte[] second2 = binaryReader2.ReadBytes(12);
            binaryReader2.BaseStream.Position = 853L;
            byte b = binaryReader2.ReadByte();
            if (!Program.subHeader.SequenceEqual(second))
            {
                Console.Write("fix subHeader? [y/n]: ");
                string text = Console.ReadLine();
                if (text != null)
                {
                    if (text == "y")
                    {
                        binaryReader2.BaseStream.Position = 16L;
                        binaryReader2.BaseStream.Write(Program.subHeader, 0, Program.subHeader.Length);
                    }
                }
            }
            if (Program.extremePattern.SequenceEqual(second2))
            {
                Console.Write("fix \"EXTREMEGAMES\"? [y/n]: ");
                string text = Console.ReadLine();
                if (text != null)
                {
                    if (text == "y")
                    {
                        binaryReader2.BaseStream.Position = 685L;
                        for (int i = 0; i < 12; i++)
                        {
                            binaryReader2.BaseStream.WriteByte(32);
                        }
                    }
                }
            }
            byte b2 = b;
            if (b2 != 0)
            {
                if (b2 == 36)
                {
                    Console.Write("Change 0x24 to 0x00?: ");
                    string text = Console.ReadLine();
                    if (text != null)
                    {
                        if (text == "y")
                        {
                            binaryReader2.BaseStream.Position = 853L;
                            binaryReader2.BaseStream.WriteByte(0);
                        }
                    }
                }
            }
            else
            {
                Console.Write("Change 0x00 to 0x24?: ");
                string text = Console.ReadLine();
                if (text != null)
                {
                    if (text == "y")
                    {
                        binaryReader2.BaseStream.Position = 853L;
                        binaryReader2.BaseStream.WriteByte(36);
                    }
                }
            }
            binaryReader2.BaseStream.Position = 0L;
            for (int i = 0; i < 16; i++)
            {
                binaryReader2.BaseStream.WriteByte(0);
            }
            Program.CalculateEDC(ref buffer, 16, 2056);
            Program.CalculateECCP(ref buffer);
            Program.CalculateECCQ(ref buffer);
            binaryReader2.BaseStream.Position = 0L;
            binaryReader2.BaseStream.Write(array, 0, array.Length);
            binaryReader2.Close();
            binaryReader.BaseStream.Position = 37632L;
            binaryReader.BaseStream.Write(buffer, 0, 2352);
            binaryReader.Close();
        }

        private static void CalculateECCQ(ref byte[] NullSector)
        {
            uint num = 52u;
            uint num2 = 43u;
            uint num3 = 86u;
            uint num4 = 88u;
            uint num5 = num * num2;
            for (uint num6 = 0u; num6 < num; num6 += 1u)
            {
                uint num7 = (num6 >> 1) * num3 + (num6 & 1u);
                byte b = 0;
                byte b2 = 0;
                for (uint num8 = 0u; num8 < num2; num8 += 1u)
                {
                    byte b3 = NullSector[(int)((UIntPtr)(12u + num7))];
                    num7 += num4;
                    if (num7 >= num5)
                    {
                        num7 -= num5;
                    }
                    b ^= b3;
                    b2 ^= b3;
                    b = Program.ecc_f_lut[(int)b];
                }
                b = Program.ecc_b_lut[(int)(Program.ecc_f_lut[(int)b] ^ b2)];
                NullSector[(int)((UIntPtr)(2248u + num6))] = b;
                NullSector[(int)((UIntPtr)(2248u + num6 + num))] = (b ^ b2);
            }
        }

        private static void CalculateECCP(ref byte[] NullSector)
        {
            uint num = 86u;
            uint num2 = 24u;
            uint num3 = 2u;
            uint num4 = 86u;
            uint num5 = num * num2;
            for (uint num6 = 0u; num6 < num; num6 += 1u)
            {
                uint num7 = (num6 >> 1) * num3 + (num6 & 1u);
                byte b = 0;
                byte b2 = 0;
                for (uint num8 = 0u; num8 < num2; num8 += 1u)
                {
                    byte b3 = NullSector[(int)((UIntPtr)(12u + num7))];
                    num7 += num4;
                    if (num7 >= num5)
                    {
                        num7 -= num5;
                    }
                    b ^= b3;
                    b2 ^= b3;
                    b = Program.ecc_f_lut[(int)b];
                }
                b = Program.ecc_b_lut[(int)(Program.ecc_f_lut[(int)b] ^ b2)];
                NullSector[(int)((UIntPtr)(2076u + num6))] = b;
                NullSector[(int)((UIntPtr)(2076u + num6 + num))] = (b ^ b2);
            }
        }

        static Program()
        {
            // Note: this type is marked as 'beforefieldinit'.
            byte[] array = new byte[8];
            array[2] = 9;
            array[6] = 9;
            Program.subHeader = array;
            Program.extremePattern = Encoding.ASCII.GetBytes("EXTREMEGAMES");
        }
    }
}
