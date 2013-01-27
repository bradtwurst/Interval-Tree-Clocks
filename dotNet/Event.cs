using System;
using System.Runtime.Serialization;

namespace IntervalTreeClocks
{
    [Serializable]
    [DataContract]
    public class Event : ICloneable
    {
        public Event()
        {
            SetAsLeaf(0);
        }

        public Event(int val)
        {
            SetAsLeaf(val);
        }

        public Event(Event e)
        {
            Value = e.Value;
            Left = Clone(e.Left);
            Right = Clone(e.Right);
        }

        [DataMember]
        public Event Left { get; private set; }

        [DataMember]
        public Event Right { get; private set; }

        [DataMember]
        public int Value { get; private set; }

        public bool IsLeaf
        {
            get { return (Left == null && Right == null); }
        }

        public object Clone()
        {
            var res = new Event(this);
            return res;
        }

        public static Event Clone(Event orig)
        {
            return (Event)(orig == null ? null : orig.Clone());
        }

        public static void Join(Event e1, Event e2)
        {
            if (!e1.IsLeaf
                && !e2.IsLeaf
                && e1.Value > e2.Value)
            {
                Join(e2, e1);
                e1.Copy(e2);
                e1.Normalize();
                return;
            }

            if (!e1.IsLeaf
                && !e2.IsLeaf
                && e1.Value <= e2.Value)
            {
                var d = e2.Value - e1.Value;
                e2.Left.Lift(d);
                e2.Right.Lift(d);
                Join(e1.Left, e2.Left);
                Join(e1.Right, e2.Right);
                e1.Normalize();
                return;
            }

            if (e1.IsLeaf
                && !e2.IsLeaf)
            {
                e1.SetAsNode();
                Join(e1, e2);
                e1.Normalize();
                return;
            }

            if (!e1.IsLeaf
                && e2.IsLeaf)
            {
                e2.SetAsNode();
                Join(e1, e2);
                e1.Normalize();
                return;
            }

            if (e1.IsLeaf
                && e2.IsLeaf)
            {
                e1.Value = GetMaxValue(e1, e2);
                e1.Normalize();
                return;
            }

            throw new EventOperationException("Event Join failed", e1, e2);
        }

        public static Event Lift(int val, Event e)
        {
            var res = (Event)e.Clone();
            res.Lift(val);
            return res;
        }

        protected static int GetMaxValue(Event e1, Event e2)
        {
            return Math.Max(e1.Value, e2.Value);
        }

        public void SetMaxValue(Event e1, Event e2)
        {
            Value = GetMaxValue(e1, e2);
        }

        public void SetValue(int value)
        {
            Value = value;
        }

        public void SetRight(Event right)
        {
            Right = right;
        }

        public void SetLeft(Event left)
        {
            Left = left;
        }
        public void Copy(Event e)
        {
            Value = e.Value;
            Left = e.Left;
            Right = e.Right;
        }

        public char[] DEncode()
        {
            return Encode(null).Unify();
        }

        public char DecN(BitArray bt)
        {
            var n = 0;
            var b = 2;
            while (bt.ReadBits(1) == 1)
            {
                n += (1 << b);
                b++;
            }
            var n2 = bt.ReadBits(b);
            n += n2;
            return (char)n;
        }

        public void Decode(BitArray bt)
        {
            var valCheck1 = bt.ReadBits(1);
            if (valCheck1 == 1)
            {
                SetAsLeaf(DecN(bt));
                return;
            }

            if (valCheck1 == 0)
            {
                DecodeCheck2(bt);
                return;
            }

            throw new EventOperationException("Decode Failed", this, null);
        }

        public void Drop(int val)
        {
            if (val <= Value)
            {
                Value -= val;
            }
        }

        public void EncN(BitArray bt, int val, int nb)
        {
            if (val < (1 << nb))
            {
                bt.AddBits(0, 1);
                bt.AddBits(val, nb);
            }
            else
            {
                bt.AddBits(1, 1);
                EncN(bt, val - (1 << nb), nb + 1);
            }
        }

        public BitArray Encode(BitArray bt)
        {
            if (bt == null)
            {
                bt = new BitArray();
            }

            if (IsLeaf)
            {
                bt.AddBits(1, 1);
                EncN(bt, Value, 2);
                return bt;
            }

            if ((!IsLeaf && Value == 0)
                && (Left.IsLeaf && Left.Value == 0)
                && (!Right.IsLeaf || Right.Value != 0))
            {
                bt.AddBits(0, 1);
                bt.AddBits(0, 2);
                Right.Encode(bt);
                return bt;
            }

            if ((!IsLeaf && Value == 0)
                && (!Left.IsLeaf || Left.Value != 0)
                && (Right.IsLeaf && Right.Value == 0))
            {
                bt.AddBits(0, 1);
                bt.AddBits(1, 2);
                Left.Encode(bt);
                return bt;
            }

            if ((!IsLeaf && Value == 0)
                && (!Left.IsLeaf || Left.Value != 0)
                && (!Right.IsLeaf || Right.Value != 0))
            {
                bt.AddBits(0, 1);
                bt.AddBits(2, 2);
                Left.Encode(bt);
                Right.Encode(bt);
                return bt;
            }

            if ((!IsLeaf && Value != 0)
                && (Left.IsLeaf && Left.Value == 0)
                && (!Right.IsLeaf || Right.Value != 0))
            {
                bt.AddBits(0, 1);
                bt.AddBits(3, 2);
                bt.AddBits(0, 1);
                bt.AddBits(0, 1);
                EncN(bt, Value, 2);
                Right.Encode(bt);
                return bt;
            }

            if ((!IsLeaf && Value != 0)
                && (!IsLeaf || Left.Value != 0)
                && (Right.IsLeaf && Right.Value == 0))
            {
                bt.AddBits(0, 1);
                bt.AddBits(3, 2);
                bt.AddBits(0, 1);
                bt.AddBits(1, 1);
                EncN(bt, Value, 2);
                Left.Encode(bt);
                return bt;
            }

            if ((!IsLeaf && Value != 0)
                && (!Left.IsLeaf || Left.Value != 0)
                && (!Right.IsLeaf || Right.Value != 0))
            {
                bt.AddBits(0, 1);
                bt.AddBits(3, 2);
                bt.AddBits(1, 1);
                EncN(bt, Value, 2);
                Left.Encode(bt);
                Right.Encode(bt);
                return bt;
            }

            throw new EventOperationException(
                string.Format(
                    "Encoding failed with IsLeaf:{0} & Value:{1}{2}",
                    IsLeaf,
                    Value,
                    IsLeaf
                        ? ""
                        : string.Format(
                            " Left: IsLeaf:{0} & Value:{1}  Right: IsLeaf:{2} & Value:{3}",
                            Left.IsLeaf,
                            Left.Value,
                            Right.IsLeaf,
                            Right.Value)),
                this,
                null);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Event)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Left != null ? Left.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Right != null ? Right.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Value;
                return hashCode;
            }
        }

        public void Height()
        {
            if (!IsLeaf)
            {
                Left.Height();
                Right.Height();
                SetAsLeaf(GetMaxValue(Left, Right));
            }
        }

        public bool Leq(Event e)
        {
            if (!IsLeaf
                && !e.IsLeaf)
            {
                if (Value > e.Value)
                {
                    return false;
                }

                if (!Lift(Value, Left).Leq(Lift(e.Value, e.Left)))
                {
                    return false;
                }

                if (!Lift(Value, Right).Leq(Lift(e.Value, e.Right)))
                {
                    return false;
                }

                return true;
            }


            if (!IsLeaf
                && e.IsLeaf)
            {
                if (Value > e.Value)
                {
                    return false;
                }

                if (!Lift(Value, Left).Leq(e))
                {
                    return false;
                }

                if (!Lift(Value, Right).Leq(e))
                {
                    return false;
                }

                return true;
            }

            if (IsLeaf && e.IsLeaf)
            {
                return Value <= e.Value;
            }

            if (IsLeaf && !e.IsLeaf)
            {
                if (Value < e.Value)
                {
                    return true;
                }

                var ev = (Event)Clone();
                ev.SetAsNode();
                return ev.Leq(e);
            }

            throw new EventOperationException("Leq operation failed", this, e);
        }

        public void Lift(int val)
        {
            Value += val;
        }

        public void Normalize()
        {
            if (!IsLeaf
                && Left.IsLeaf
                && Right.IsLeaf
                && Left.Value == Right.Value)
            {
                SetAsLeaf(Left.Value);
                return;
            }

            if (!IsLeaf)
            {
                var mm = Math.Min(Left.Value, Right.Value);
                Lift(mm);
                Left.Drop(mm);
                Right.Drop(mm);
            }
        }


        public void SetAsLeaf(int value)
        {
            Left = null;
            Right = null;
            Value = value;
        }

        public void SetAsNode()
        {
            Left = new Event(0);
            Right = new Event(0);
        }

        public void SetAsNode(int value, int? leftValue, int? rightValue, BitArray bt)
        {
            Value = value;

            if (leftValue.HasValue)
            {
                Left = new Event(leftValue.Value);
            }
            else
            {
                Left = new Event();
                Left.Decode(bt);
            }

            if (rightValue.HasValue)
            {
                Right = new Event(rightValue.Value);
            }
            else
            {
                Right = new Event();
                Right.Decode(bt);
            }
        }

        public override string ToString()
        {
            if (IsLeaf)
            {
                return string.Format("Value: {0}", Value);
            }

            return string.Format("(Value: {0}, Left: {1}, Right: {2})", Value, Left, Right);
        }

        protected bool Equals(Event inEvent)
        {
            return Equals(Left, inEvent.Left) && Equals(Right, inEvent.Right) && Value == inEvent.Value;
        }

        private void DecodeCheck2(BitArray bt)
        {
            var valCheck2 = bt.ReadBits(2);

            if (valCheck2 == 0)
            {
                Value = 0;
                Left = new Event(0);
                Right = new Event();
                Right.Decode(bt);
                return;
            }

            if (valCheck2 == 1)
            {
                SetAsNode(0, null, 0, bt);
                return;
            }

            if (valCheck2 == 2)
            {
                SetAsNode(0, null, null, bt);
                return;
            }

            if (valCheck2 == 3)
            {
                DecodeCheck3(bt);
                return;
            }

            throw new EventOperationException("Decode Failed (Check2)", this, null);
        }

        private void DecodeCheck3(BitArray bt)
        {
            var valCheck3 = bt.ReadBits(1);

            if (valCheck3 == 0)
            {
                DecodeCheck4(bt);
                return;
            }

            if (valCheck3 == 1)
            {
                SetAsNode(DecN(bt), null, null, bt);
                return;
            }

            throw new EventOperationException("Decode Failed (Check3)", this, null);
        }

        private void DecodeCheck4(BitArray bt)
        {
            var valCheck4 = bt.ReadBits(1);

            if (valCheck4 == 0)
            {
                SetAsNode(DecN(bt), 0, null, bt);
                return;
            }

            if (valCheck4 == 1)
            {
                SetAsNode(DecN(bt), null, 0, bt);
                return;
            }

            throw new EventOperationException("Decode Failed (Check4)", this, null);
        }
    }
}