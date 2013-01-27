using System;
using System.Collections.Generic;

namespace IntervalTreeClocks
{
    public class BitArray
    {
        private const byte DTS = 16;

        private int _freebs;
        private int _startb;

        public BitArray()
        {
            Bits = new char[1];
            Bits[0] = (char)0;

            _startb = 0;
            SizeBits = 0;
            _freebs = DTS;
        }

        public BitArray(IList<char> array)
        {
            SizeBits = array[0];
            _freebs = array[1];
            _startb = array[2];
            var len = (SizeBits + _freebs) / DTS;
            Bits = new char[len];
            Array.Copy(Bits, 3, Bits, 0, len);
        }

        public char[] Bits { get; private set; }

        public int Length
        {
            get { return (_freebs + SizeBits) / DTS; }
        }

        public int SizeBits { get; private set; }

        public static char[] InvertEdianess(char[] inArray)
        {
            var inLength = inArray.Length;
            var inLengthMinus1 = inLength - 1;
            var outArray = new char[inLength];
            for (var j = 0;
                 j < inLength;
                 j++)
            {
                outArray[j] = inArray[inLengthMinus1 - j];
            }
            return outArray;
        }

        public void AddBits(int val, int nb)
        {
            if (nb > _freebs)
            {
                Expand(nb);
            }

            if ((SizeBits % DTS) > 0
                && (_freebs % DTS) < nb)
            {
                var bitsleft = (char)(_freebs % DTS);
                var mask = (char)(Math.Pow(2, bitsleft) - 1);

                var now = (char)(val & mask);

                var jump = (char)(SizeBits / DTS);
                var bjump = (char)(SizeBits % DTS);

                now = (char)(now << bjump);
                Bits[jump] = (char)(Bits[jump] | now);

                var then = (char)(val >> bitsleft);
                Bits[jump + 1] = (char)(Bits[jump + 1] | then);
            }
            else
            {
                var jump = (char)(SizeBits / DTS);
                var bjump = (char)(SizeBits % DTS);

                var now = (char)(val << bjump);
                Bits[jump] = (char)(Bits[jump] | now);
            }

            SizeBits += nb;
            _freebs -= nb;
        }

        public void Expand(int nb)
        {
            var len = (SizeBits + _freebs) / DTS;
            var ene = len;
            while (_freebs < nb)
            {
                ene *= 2;
                _freebs += ((ene / 2) * DTS);
            }

            var newArray = new char[ene];
            Array.Copy(Bits, 0, newArray, 0, len);
            Bits = new char[ene];
            Array.Copy(newArray, 0, Bits, 0, len);
        }

        public char GetChar(int ind)
        {
            return Bits[ind];
        }

        public int ReadBits(int nb)
        {
            var jump = (char)(_startb / DTS);
            var bjump = (char)(_startb % DTS);

            if (bjump > 0
                && (DTS - bjump) < nb)
            {
                var bitsleft = (char)(DTS - bjump);

                var mask = (char)(Math.Pow(2, bitsleft) - 1);
                mask = (char)(mask << bjump);

                var now = (char)(Bits[jump] & mask);
                now = (char)(now >> bjump);

                mask = (char)(Math.Pow(2, (nb - bitsleft)) - 1);

                var then = (char)(Bits[jump + 1] & mask);
                then = (char)(then << bitsleft);

                now = (char)(now | then);

                _startb += nb;
                return now;
            }
            else
            {
                var mask = (char)(Math.Pow(2, nb) - 1);
                mask = (char)(mask << bjump);

                var res = (char)(Bits[jump] & mask);
                res = (char)(res >> bjump);

                _startb += nb;
                return res;
            }
        }

        public void SetBits(char[] bits)
        {
            Bits = new char[bits.Length];
            Array.Copy(bits, 0, Bits, 0, bits.Length);
            SizeBits = bits.Length / DTS;
        }

        public char[] Unify()
        {
            var nfree = _freebs % DTS;
            var len = (SizeBits + nfree) / DTS;

            var res = new char[3 + len];
            res[0] = (char)SizeBits;
            res[1] = (char)nfree;
            res[2] = (char)_startb;
            Array.Copy(Bits, 0, res, 3, len);

            return res;
        }
    }
}