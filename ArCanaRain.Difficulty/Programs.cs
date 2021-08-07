using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ArCanaRain.Difficulty
{
    class Program
    {
        static void Main(string[] args)
        {
            //difficulty 1 の difficulty bits = 0x1d00ffff
            const uint difficulty1Source = 0x1d00ffff;
            var difficulty1Bytes = BitConverter.GetBytes(difficulty1Source);

            //BTC の difficulty target は基本 float にパッケージされている。 これが difficulty Bits になっている。
            //float に詰める => byte array で再度取り出しを行っているので、意味はないが手順通りにするために一回格納する。
            var packagedDiff = BitConverter.ToSingle(difficulty1Bytes);
            var packagedDiffBytes = BitConverter.GetBytes(packagedDiff);
            packagedDiffBytes.ReverseIfLittleEndian();

            //この値は指数部/係数に分かれる。 指数:0x1d 係数:0x00ffff
            var exponent = packagedDiffBytes[0];
            var coefficient = packagedDiffBytes.AsSpan()[1..4];
            coefficient.ReverseIfLittleEndian();
            var exponentNum = Convert.ToUInt16(exponent);
            var coefficientNum = BitConverter.ToUInt16(coefficient);

            //difficulty target の導出
            //指数部分の計算：2^(8 * (exponent – 3))
            //ushort だと実際のコードでは例外吐くかもしれない。
            exponentNum -= 3;
            exponentNum *= 8;
            var bi = BigInteger.Pow(2, exponentNum);

            //32 byte の配列に整形しつつ結果を格納。
            var result = new byte[32];
            (coefficientNum * bi).TryWriteBytes(result.AsSpan(0), out _);
            result.ReverseIfLittleEndian();

            // 結果
            WriteString(result);

            //以下おまけ。
            //マイニングでは target より低いハッシュ値を導き出すことが求められる。

            var underHash = new byte[]
            {
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0xFF, 0xFF, 0xFF,
                0x00, 0xFF, 0xFF, 0xFF,
                0x00, 0xFF, 0xFF, 0xFF,
                0x00, 0xFF, 0xFF, 0xFF,
                0x00, 0xFF, 0xFF, 0xFF
            };
            WriteString(underHash);

            var overHash = new byte[]
            {
                0x7F, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0xFF, 0xFF, 0xFF,
                0x00, 0xFF, 0xFF, 0xFF,
                0x00, 0xFF, 0xFF, 0xFF,
                0x00, 0xFF, 0xFF, 0xFF,
                0x00, 0xFF, 0xFF, 0xFF
            };
            WriteString(overHash);

            Console.WriteLine(HashCheck(underHash, result));
            Console.WriteLine(HashCheck(overHash, result));
        }

        static void WriteString(IEnumerable<byte> data)
            => Console.WriteLine(string.Join("", data.Select(x => $"{x:X2}")));

        static bool HashCheck(IReadOnlyList<byte> data1, IReadOnlyList<byte> target)
        {
            if (data1.Count != 32 || target.Count != 32) return false;
            for (var i = 0; i < data1.Count; i++)
            {
                if (data1[i] < target[i]) return true;
                if (data1[i] > target[i]) return false;
            }
            return true;
        }
    }
}
