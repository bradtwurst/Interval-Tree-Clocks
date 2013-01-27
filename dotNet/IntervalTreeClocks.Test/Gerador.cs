namespace IntervalTreeClocks.Test
{
    public class Gerador
    {
        private int _indice;

        public Gerador()
        {
            _indice = 1;
        }

        public Atomo Gera()
        {
            return new Atomo(_indice++);
        }

        public void Reset()
        {
            _indice = 1;
        }

        public Atomo Seed()
        {
            Reset();
            return Gera();
        }
    }
}