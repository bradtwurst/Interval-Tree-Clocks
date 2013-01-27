using System.Collections.Generic;

namespace IntervalTreeClocks.Test
{
    public class Bag<T>
    {
        private readonly List<T> _list;

        public Bag()
        {
            _list = new List<T>();
        }

        public T GetInd(int ind)
        {
            //if (i < this.list.size()){
            var item = _list[ind];
            return item;
            //}
        }

        public T Pop()
        {
            var d = new Dice();
            var ind = d.Iroll(0, _list.Count - 1);

            var item = _list[ind];

            _list.RemoveAt(ind);
            return item;
        }

        public T PopInd(int ind)
        {
            //if (i < this.list.size()){
            var item = _list[ind];

            _list.RemoveAt(ind);
            return item;
            //}
        }

        public void Push(T s)
        {
            _list.Add(s);
        }

        public T GetLast()
        {
            var ind = _list.Count - 1;
            // if( ind > 0){
            var item = _list[ind];
            return item;
            //}
        }

        public List<T> GetList()
        {
            return _list;
        }

        public int GetSize()
        {
            return _list.Count;
        }

        public int GetValidIndice()
        {
            var d = new Dice();
            return d.Iroll(0, _list.Count - 1);
        }

        // save bag with stamps
        //    public void saveBag(DataOutputStream out) {
        //        try {

        //            long len = this.getSize() & 0xffffffffl;
        //            byte[] n = ByteInt.intToByteArray((long) len);

        //            // writes the number of bytes to be saved
        //            for (int j = 0; j < 4; j++) {
        ////				System.out.println(n[j]);
        //                out.write(n[j]);
        //            }

        //            for (T stamp : this.list) {
        ////				System.out.println(((Stamp) stamp).toString());
        //                ((Stamp) stamp).saveToFile(out);
        //            }

        //        } catch (Exception ex) {
        //            ex.printStackTrace();
        //        }
        //    }

        //    public void loadBag(DataInputStream in) {
        //        try {
        //            byte[] ene = new byte[4];

        //            // reads the number of bytes to be saved
        //            for (int j = 0; j < 4; j++) {
        ////				System.out.println(ene[j]);
        //                ene[j] = (byte) in.read();
        //            }
        //            long len = (long) ByteInt.byteArrayToInt(ene);

        //            for (int j = 0; j < len; j++) {
        //                Stamp s = new Stamp();
        //                s.loadFromFile(in);
        ////				System.out.println("in" + s.toString());
        //                this.push((T) s);
        //            }

        //        } catch (Exception ex) {
        //            ex.printStackTrace();
        //        }
        //        }
    }
}