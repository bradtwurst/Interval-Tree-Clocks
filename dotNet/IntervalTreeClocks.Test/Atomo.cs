using System.Globalization;

namespace IntervalTreeClocks.Test
{
    public class Atomo
    {
        public int Id;

        public Atomo(int i)
        {
            Id = i;
        }

        public int GetId()
        {
            return Id;
        }

        public override string ToString()
        {
            return Id.ToString(CultureInfo.InvariantCulture);
        }
    }
}