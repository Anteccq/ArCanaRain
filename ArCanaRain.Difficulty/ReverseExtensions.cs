using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArCanaRain.Difficulty
{
    public static class ReverseExtensions
    {
        public static void ReverseIfLittleEndian(this byte[] data)
        {
            if (BitConverter.IsLittleEndian) Array.Reverse(data);
        }

        public static void ReverseIfLittleEndian(this Span<byte> data)
        {
            if (BitConverter.IsLittleEndian) data.Reverse();
        }
    }
}
