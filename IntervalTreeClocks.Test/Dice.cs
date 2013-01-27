using System;

namespace IntervalTreeClocks.Test
{
    public class Dice
    {
        private readonly Random _rand;
        private readonly int _sub;
        private readonly int _top;

        public Dice()
        {
            _sub = 0;
            _top = 100;

            _rand = new Random();
        }

        public Dice(int top)
        {
            _top = top;

            _rand = new Random();
        }

        public Dice(int top, int sub)
        {
            _top = top;
            _sub = sub;

            _rand = new Random();
        }

        public int Iroll(int sub, int top)
        {
            var res = _rand.Next(1 + top - sub);

            return res + sub;
        }

        public int Roll()
        {
            var res = _rand.Next(_top - _sub + 1);

            return res + _sub;
        }

        public int Roll(int n)
        {
            var res = 0;

            for (var i = 0;
                 i < n;
                 i++)
            {
                res += Roll();
            }

            return res;
        }
    }
}