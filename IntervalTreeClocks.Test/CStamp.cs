using System.Collections.Generic;
using System.Linq;

namespace IntervalTreeClocks.Test
{
    public class CStamp
    {
        private readonly List<Atomo> _lista;

        public CStamp()
        {
            _lista = new List<Atomo>();
        }

        public CStamp(CStamp cs2)
        {
            _lista = new List<Atomo>(cs2.GetLista());
        }

        public void Event(Atomo a)
        {
            _lista.Add(a);
        }

        public override string ToString()
        {
            return string.Join(" ", _lista);
        }

        public void AddLista(List<Atomo> a)
        {
            _lista.AddRange(a);
        }

        public CStamp Copia()
        {
            var res = new CStamp();
            res.SetLista(_lista);

            return res;
        }

        // Métodos de instancia

        public CStamp Fork()
        {
            return Copia();
        }

        public List<Atomo> GetLista()
        {
            var res = new List<Atomo>();
            res.AddRange(_lista);

            return res;
        }

        public void Join(CStamp cs1, CStamp cs2)
        {
            _lista.AddRange(cs1.GetLista());

            foreach (var aa in cs2.GetLista())
            {
                if (!_lista.Contains(aa))
                    _lista.Add(aa);
            }
        }

        public bool Leq(CStamp s2)
        {
            return _lista.All(aa => s2.GetLista().Contains(aa));
        }

        public void Seed(Atomo a)
        {
            _lista.Clear();
            _lista.Add(a);
        }

        // Metodos complementares

        public void SetLista(List<Atomo> a)
        {
            _lista.Clear();
            _lista.AddRange(a);
        }
    }
}