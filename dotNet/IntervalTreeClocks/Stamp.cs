using System;
using System.IO;

namespace IntervalTreeClocks
{
    public class Stamp
    {
        protected Event Event;
        protected Id Id;

        public Stamp()
        {
            Event = new Event();
            Id = new Id();
        }

        protected Stamp(Id i, Event e)
        {
            Id = i;
            Event = e;
        }

        protected Stamp(Stamp s)
        {
            Event = s.Event;
            Id = s.Id;
        }

        public static Stamp StampEvent(Stamp st)
        {
            // returns the new updated Stamp
            var res = st.Clone();
            StampFill(st.Id, st.Event);
            var notFilled = st.Event.Equals(res.Event);

            if (notFilled)
            {
                StampGrow(st.Id, st.Event);
            }

            return res;
        }

        public static Stamp[] StampFork(Stamp st)
        {
            var b = new Stamp
                        {
                            Event = (Event.Clone(st.Event))
                        };

            var par = st.Id.Split();
            st.Id = (par[0]);
            b.Id = (par[1]);

            return new[]{st, b};
        }

        public static Stamp StampJoin(Stamp s1, Stamp s2)
        {
            // joins two stamps, returning the resulting stamp
            Id.Sum(s1.Id, s2.Id);
            Event.Join(s1.Event, s2.Event);
            return s1;
        }

        public static Stamp StampPeek(Stamp st)
        {
            var i = new Id(0);
            var ev = Event.Clone(st.Event);
            return new Stamp(i, ev);
        }

        protected static void StampFill(Id i, Event e)
        {
            if (i.IsLeaf
                && i.Value == 0)
            {
                return;
            }
            
            if (i.IsLeaf
                && i.Value == 1)
            {
                e.Height();
                return;
            }
            
            if (e.IsLeaf)
            {
                return;
            }
            
            if (i.IsLeaf == false
                && i.Left.IsLeaf
                && i.Left.Value == 1)
            {
                StampFill(i.Right, e.Right);
                e.Left.Height();
                e.Left.SetMaxValue(e.Left, e.Right);
                e.Normalize();
                return;
            }
            
            if (i.IsLeaf == false
                && i.Right.IsLeaf
                && i.Right.Value == 1)
            {
                StampFill(i.Left, e.Left);
                e.Right.Height();
                e.Right.SetMaxValue(e.Right, e.Left);
                e.Normalize();
                return;
            }
            
            if (i.IsLeaf == false)
            {
                StampFill(i.Left, e.Left);
                StampFill(i.Right, e.Right);
                e.Normalize();
                return;
            }

            throw new StampOperationException(string.Format("Fill failed: id: {0}, event: {1}", i, e));
        }

        protected static int StampGrow(Id i, Event e)
        {
            int cost;

            if (i.IsLeaf
                && i.Value == 1
                && e.IsLeaf)
            {
                e.SetValue(e.Value + 1);
                return 0;
            }
            
            if (e.IsLeaf)
            {
                e.SetAsNode();
                cost = StampGrow(i, e);
                return cost + 1000;
            }
            
            if (i.IsLeaf == false
                && i.Left.IsLeaf
                && i.Left.Value == 0)
            {
                cost = StampGrow(i.Right, e.Right);
                return cost + 1;
            }
            
            if (i.IsLeaf == false
                && i.Right.IsLeaf
                && i.Right.Value == 0)
            {
                cost = StampGrow(i.Left, e.Left);
                return cost + 1;
            }
            
            if (i.IsLeaf == false)
            {
                var el = Event.Clone(e.Left);
                var er = Event.Clone(e.Right);
                var costr = StampGrow(i.Right, e.Right);
                var costl = StampGrow(i.Left, e.Left);
                if (costl < costr)
                {
                    e.SetRight(er);
                    return costl + 1;
                }

                e.SetLeft(el);
                return costr + 1;
            }
            
            throw new StampOperationException(string.Format("Grow failed: id: {0}, event: {1}", i, e));
        }

        public Stamp Clone()
        {
            return new Stamp
                       {
                           Event = Event.Clone(Event),
                           Id = Id.Clone(Id)
                       };
        }

        public void DDecode(char[] array)
        {
            var ilen = 3 + ((array[0] + array[1])/16);
            var elen = 3 + ((array[ilen] + array[ilen + 1])/16);
            var icode = new char[ilen];
            var ecode = new char[elen];

            Array.Copy(array, 0, icode, 0, ilen);
            Array.Copy(array, ilen, ecode, 0, elen);

            var bi = new BitArray(icode);
            var be = new BitArray(ecode);

            Id.Decode(bi);
            Event.Decode(be);
        }

        public char[] DEncode()
        {
            var idcode = Id.DEncode();
            var eventcode = Event.DEncode();

            const int dts = 16;
            var isize = 3 + ((idcode[0] + idcode[1])/dts);
            var esize = 3 + ((eventcode[0] + eventcode[1])/dts);

            var res = new char[isize + esize];
            Array.Copy(idcode, 0, res, 0, isize);
            Array.Copy(eventcode, 0, res, isize, esize);

            return res;
        }

        public void Decode(char[] bits)
        {
            var bt = new BitArray();
            bt.SetBits(bits);

            Id.Decode(bt);
            Event.Decode(bt);
        }

        public void Decode(BitArray bt)
        {
            Event.Decode(bt);
        }

        public BitArray EncodeBitArray()
        {
            return Event.Encode(null);
        }

        public char[] EncodeCharArray()
        {
            var bt = new BitArray();
            bt = Id.Encode(bt);
            bt = Event.Encode(bt);

            return bt.Bits;
        }

        public Stamp Fork()
        {
            var st = new Stamp
                         {
                             Event = (Event.Clone(Event))
                         };

            var par = Id.Split();
            Id = par[0];
            st.Id = (par[1]);

            return st;
        }

        public void FromStream(StreamReader inStream)
        {
            var ene = new byte[4];
            for (var j = 0;
                 j < 4;
                 j++)
            {
                ene[j] = (byte) inStream.Read();
            }
            var n = ByteInt.ByteArrayToInt(ene);

            n /= 2;

            var array = new char[(int) n];
            for (var j = (int) n - 1;
                 j >= 0;
                 j--)
            {
                array[j] = (char) inStream.Read();
            }

            Decode(array);
        }

        public void Join(Stamp s2)
        {
            // joins two stamps becoming itself the result stamp
            Id.Sum(Id, s2.Id);
            Event.Join(Event,s2.Event);
        }

        public bool Leq(Stamp s2)
        {
            return Event.Leq(s2.Event);
        }

        public void MakeEvent()
        {
            var old = Event.Clone(Event);
            StampFill(Id, Event);
            var notFilled = old.Equals(Event);

            if (notFilled)
            {
                StampGrow(Id, Event);
            }
        }

        public Stamp Peek()
        {
            var i = new Id(0);
            var ev = Event.Clone(Event);
            return new Stamp(i, ev);
        }

        //size of stamp in bits
        public int SizeInBits()
        {
            var ibits = Id.Encode(null).SizeBits;
            var ebits = Event.Encode(null).SizeBits;
            return ibits + ebits;
        }

        //size of stamp in byts
        public int SizeInBytes()
        {
            var ibits = Id.Encode(null).SizeBits;
            var ebits = Event.Encode(null).SizeBits;
            return ((ibits + ebits + 4)/8);
        }

        public void ToStream(StreamWriter outStream)
        {
            var array = EncodeCharArray();

            long len = (array.Length);
            var n = ByteInt.IntToByteArray((len*2) & 0xffffffffl);

            // writes the number of bytes to be saved
            for (var j = 0;
                 j < 4;
                 j++)
            {
                outStream.Write(n[j]);
            }

            // writes the bytes of the encoded stamp
            for (var j = (int) len - 1;
                 j >= 0;
                 j--)
            {
                outStream.Write(n[j]);
            }
        }

        public override String ToString()
        {
            return "( " + Id + ", " + Event + " )";
        }

    }
}