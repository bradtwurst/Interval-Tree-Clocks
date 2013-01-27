using System.Diagnostics;
using NUnit.Framework;

namespace IntervalTreeClocks.Test
{
    internal class TestStamp
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestItvVsCh()
        {
            // HC mechanism
            var gen = new Gerador();
            var dado = new Dice();
            var saco = new Bag<CStamp>();

            var seed = new CStamp();
            seed.Seed(gen.Seed());
            saco.Push(seed);

            // ITC mechanism
            var bag = new Bag<Stamp>();

            var seedb = new Stamp();
            bag.Push(seedb);

            var forks = 0;
            var joins = 0;
            var events = 0;

            int i;
            var counter = 0;
            for (i = 0;
                 i < 1500;
                 i++)
            {
                Debug.WriteLine(i + 1 + ": bugs->" + counter);
                var tipo = dado.Iroll(1, 100);

                if (tipo <= 34
                    || saco.GetSize() == 1)
                {
                    // fork
                    Debug.WriteLine("Fork __________________________");
                    forks++;
                    var ind = saco.GetValidIndice();

                    // mecanismo hc
                    var outStamp = saco.PopInd(ind);
                    CStamp novo = outStamp.Fork();

                    saco.Push(outStamp);
                    saco.Push(novo);

                    // mecanismo itc in place
                    var sout = bag.PopInd(ind);
                    var sin = sout.Fork();

                    bag.Push(sout);
                    bag.Push(sin);

//				// mecanismo itc funcional
//				Stamp outb = (Stamp) bag.popInd(ind);
//				Stamp[] p = Stamp.fork(outb);
//				Stamp in1 = p[0];
//				Stamp in2 = p[1];
//
//				bag.push(in1);
//				bag.push(in2);
                }
                else if (tipo <= 66)
                {
                    // join
                    Debug.WriteLine("Join __________________________");
                    joins++;
                    var inda = saco.GetValidIndice();

                    var outa = saco.PopInd(inda);
                    var souta = bag.PopInd(inda);

                    var indb = saco.GetValidIndice();

                    var outb = saco.PopInd(indb);
                    var soutb = bag.PopInd(indb);

                    var novo = new CStamp();
                    novo.Join(outa, outb);
                    saco.Push(novo);

//				Stamp novob = Stamp.join(souta, soutb);
//				bag.push(novob);
                    souta.Join(soutb);
                    bag.Push(souta);
                }
                else
                {
                    // event
                    Debug.WriteLine("Event _________________________");
                    events++;
                    var ind = saco.GetValidIndice();

                    var outStamp = saco.PopInd(ind);
                    outStamp.Event(gen.Gera());
                    saco.Push(outStamp);

                    var outb = bag.PopInd(ind);
//				System.out.println("ANTES:"+outb.toString());
                    outb.MakeEvent();
//				System.out.println("DPS:"+outb.toString());
                    bag.Push(outb);
                }

                var tmp = saco.GetLast();
                var tmpb = bag.GetLast();
                var len = saco.GetSize();

                for (var n = 0;
                     n < len - 1;
                     n++)
                {
                    var a = tmp.Equals(saco.GetInd(n));

                    var decd = new Stamp();
                    var coise = bag.GetInd(n).EncodeCharArray();
                    decd.Decode(coise);
//				decd.dDecode(bag.getInd(n).dEncode());
                    var b = tmpb.Equals(decd);
//				boolean b = tmpb.equals((Stamp) bag.getInd(n));
                    if (!((a && b) || (!a && !b)))
                    {
                        Debug.WriteLine("Devia ser " + a + ", mas e " + b + "\n\t" + tmpb + "   E    " + decd);
                        counter++;
                    }
                }
            }

//		File f = new File("binaryfile");
//
//		try {
//			DataOutputStream out = new DataOutputStream(new FileOutputStream(f));
//
//			bag.saveBag(out);
//
//			out.close();
//
//			DataInputStream in = new DataInputStream(new FileInputStream(f));
//
//			Bag<Stamp> bbb = new Bag();
//			bbb.loadBag(in);
//
//
////			System.out.print(bbb.toString());
//		} catch (Exception ex) {
//			ex.printStackTrace();
//		}

            Debug.WriteLine(" Bugs : " + counter);
            Debug.WriteLine("=======================");
            Debug.WriteLine(" Forks  : " + forks);
            Debug.WriteLine(" Joins  : " + joins);
            Debug.WriteLine(" Events : " + events);
            Debug.WriteLine("");
            Debug.WriteLine(" Bag final size : " + bag.GetSize());
        }
    }
}