using System;
using System.Runtime.Serialization;

namespace IntervalTreeClocks
{
    [Serializable]
    [DataContract]
    public class Id : ICloneable
    {
        public Id()
        {
            SetAsLeaf(1);
        }

        public Id(int val)
        {
            SetAsLeaf(val);
        }

        public Id(Id i)
        {
            Value = i.Value;
            Left = Clone(i.Left);
            Right = Clone(i.Right);
        }

        [DataMember]
        public Id Left { get; private set; }

        [DataMember]
        public Id Right { get; private set; }

        [DataMember]
        public int Value { get; private set; }

        public bool IsLeaf
        {
            get { return (Left == null && Right == null); }
        }

        public object Clone()
        {
            var res = new Id(this);
            return res;
        }

        public static Id Clone(Id orig)
        {
            return (Id)(orig == null ? null : orig.Clone());
        }

        public static void Sum(Id i1, Id i2)
        {
            // this becomes the sum between i1 and i2

            //sum(0, X) -> X;
            //sum(X, 0) -> X;
            //sum({L1,R1}, {L2, R2}) -> norm_id({sum(L1, L2), sum(R1, R2)}).

            if (i1.IsLeaf
                && i1.Value == 0)
            {
                i1.Copy(i2);
                return;
            }

            if (i2.IsLeaf
                && i2.Value == 0)
            {
                return;
                //i1 is the result
            }

            if (!i1.IsLeaf
                && !i2.IsLeaf)
            {
                Sum(i1.Left, i2.Left);
                Sum(i1.Right, i2.Right);
                i1.Normalize();
                return;
            }

            throw new IdOperationException("Sum failed", i1, i2);
        }


        public void Copy(Id i)
        {
            Value = i.Value;
            Left = i.Left;
            Right = i.Right;
        }


        public char[] DEncode()
        {
            return Encode(null).Unify();
        }

        // code and decode dos ids

        public void Decode(BitArray bt)
        {
            var val = bt.ReadBits(2);

            if (val == 0)
            {
                SetAsLeaf(bt.ReadBits(1));
                return;
            }

            if (val == 1)
            {
                SetAsNode(null, 0, null, bt);
                return;
            }

            if (val == 2)
            {
                SetAsNode(null, null, 0, bt);
                return;
            }

            if (val == 3)
            {
                SetAsNode(null, null, null, bt);
                return;
            }

            throw new IdOperationException("Decode Failed", this, null);
        }

        public BitArray Encode(BitArray bt)
        {
            if (bt == null)
            {
                bt = new BitArray();
            }

            if (IsLeaf
                && Value == 0)
            {
                bt.AddBits(0, 3);
                return bt;
            }

            if (IsLeaf
                && Value == 1)
            {
                bt.AddBits(0, 2);
                bt.AddBits(1, 1);
                return bt;
            }

            if (!IsLeaf
                && (Left.IsLeaf && Left.Value == 0)
                && (!Right.IsLeaf || Right.Value == 1))
            {
                bt.AddBits(1, 2);
                Right.Encode(bt);
                return bt;
            }

            if (!IsLeaf
                && (Right.IsLeaf && Right.Value == 0)
                && (!Left.IsLeaf || Left.Value == 1))
            {
                bt.AddBits(2, 2);
                Left.Encode(bt);
                return bt;
            }

            if (!IsLeaf
                && (!Right.IsLeaf || Right.Value == 1)
                && (!Left.IsLeaf || Left.Value == 1))
            {
                //System.out.println("id enc d");
                bt.AddBits(3, 2);
                Left.Encode(bt);
                Right.Encode(bt);
                return bt;
            }

            throw new IdOperationException("Encode Failed", this, null);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Id)obj);
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

        public void Normalize()
        {
            if (!IsLeaf
                && Left.IsLeaf
                && Left.Value == 0
                && Right.IsLeaf
                && Right.Value == 0)
            {
                SetAsLeaf(0);
                return;
            }

            if (!IsLeaf
                && Left.IsLeaf
                && Left.Value == 1
                && Right.IsLeaf
                && Right.Value == 1)
            {
                SetAsLeaf(1);
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
            Value = -1;
            Left = new Id(1);
            Right = new Id(0);
        }

        public void SetAsNode(int? value, int? leftValue, int? rightValue, BitArray bt)
        {
            if (value.HasValue)
            {
                Value = value.Value;
            }
            else
            {
                Value = -1;
            }

            if (leftValue.HasValue)
            {
                Left = new Id(leftValue.Value);
            }
            else
            {
                Left = new Id();
                Left.Decode(bt);
            }

            if (rightValue.HasValue)
            {
                Right = new Id(rightValue.Value);
            }
            else
            {
                Right = new Id();
                Right.Decode(bt);
            }

        }

        public void SetAsNode(int value, Id left, Id right)
        {
            Value = value;
            Left = left;
            Right = right;

        }

        public Id[] Split()
        {
            var i1 = new Id();
            var i2 = new Id();

            if (IsLeaf
                && Value == 0)
            {
                i1.SetAsLeaf(0);

                i2.SetAsLeaf(0);

                return new[]
                           {
                               i1, i2
                           };
            }

            if (IsLeaf
                && Value == 1)
            {
                // id = 1
                i1.SetAsNode(0, 1, 0, null);

                i2.SetAsNode(0, 0, 1, null);

                return new[]
                           {
                               i1, i2
                           };
            }

            if (!IsLeaf
                && (Left.IsLeaf && Left.Value == 0)
                && (!Right.IsLeaf || Right.Value == 1))
            {
                // id = (0, i)
                var ip = Right.Split();

                i1.SetAsNode(0, new Id(0), ip[0]);

                i2.SetAsNode(0, new Id(0), ip[1]);

                return new[]
                           {
                               i1, i2
                           };
            }

            if (!IsLeaf
                && (!Left.IsLeaf || Left.Value == 1)
                && (Right.IsLeaf && Right.Value == 0))
            {
                // id = (i, 0)
                var ip = Left.Split();

                i1.SetAsNode(0, ip[0], new Id(0));

                i2.SetAsNode(0, ip[1], new Id(0));

                return new[]
                           {
                               i1, i2
                           };
            }

            if (!IsLeaf
                && (!Left.IsLeaf || Left.Value == 1)
                && (!Right.IsLeaf || Right.Value == 1))
            {
                // id = (i1, i2)
                i1.SetAsNode(0, Clone(Left), new Id(0));

                i2.SetAsNode(0, new Id(0), Clone(Right));

                return new[]
                           {
                               i1, i2
                           };
            }

            throw new IdOperationException("Split failed", this, null);
        }

        public override string ToString()
        {
            if (IsLeaf)
            {
                return string.Format("Value: {0}", Value);
            }

            return string.Format("(Value: {0}, Left: {1}, Right: {2})", Value, Left, Right);
        }

        protected bool Equals(Id inId)
        {
            return Equals(Left, inId.Left) && Equals(Right, inId.Right) && Value == inId.Value;
        }
    }
}