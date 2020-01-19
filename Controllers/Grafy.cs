﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace Controllers
{
    class Graf
    {
        public
        List<Wezel> wezly = new List<Wezel>();
        public
        List<Krawedz> krawedzie = new List<Krawedz>();

        public int ilosckrawedzi;
        public int iloscwezlow;

        public Graf(String fileName)
        {
            string[] wczytywanie = System.IO.File.ReadAllLines(fileName);
            iloscwezlow = Int32.Parse(wczytywanie[0]);
            int[] numerki = new int[4];
            ilosckrawedzi = Int32.Parse(wczytywanie[iloscwezlow + 1]);
            Console.WriteLine(ilosckrawedzi + " " + iloscwezlow);
            for (int i = 1; i <= iloscwezlow; i++)
            {

                string[] dane = wczytywanie[i].Split(new string[] { " " }, StringSplitOptions.None);
                for (int j = 0; j < dane.Length; j++)
                {
                    numerki[j] = Int32.Parse(dane[j]);
                }

                wezly.Add(new Wezel(numerki[0]));
            }

            for (int i = iloscwezlow + 2; i <= iloscwezlow + ilosckrawedzi + 1; i++)
            {
                string[] dane = wczytywanie[i].Split(new string[] { " " }, StringSplitOptions.None);
                for (int j = 0; j < dane.Length; j++)
                {
                    numerki[j] = Int32.Parse(dane[j]);
                }
                
                krawedzie.Add(new Krawedz(numerki[0], wezly[numerki[1] - 1], wezly[numerki[2] - 1], numerki[3]));
                krawedzie.Add(new Krawedz(numerki[0]+ilosckrawedzi, wezly[numerki[2] - 1], wezly[numerki[1] - 1], numerki[3]));
            }
            ilosckrawedzi *= 2;
        }

        public Graf(int o, int p)
        {
            ilosckrawedzi = o;
            iloscwezlow = p;
        }


        public int podajKrawedz(int a, int b)
        {
            for (int j = 0; j < krawedzie.Count; j++)
            {
                if (a == krawedzie[j].PodajPoczatek() && b == krawedzie[j].PodajKoniec())
                {
                    return krawedzie[j].Podajindeks();
                }
            }
            return -1;
        }



        public List<int> dijkstra(int a, int x)
        {
            Graf grafdijkstry = new Graf(0, 0);
            List<Krawedz> kolejka = new List<Krawedz>();
            List<int> droga = new List<int>();
            List<int> p = new List<int>();
            List<int> d = new List<int>();
            List<SortedSet<int>> holes = new List<SortedSet<int>>();

            for (int i = 0; i < wezly.Count; i++)
            {
                p.Add(-1);
                d.Add(777777);
                holes.Add(new SortedSet<int> { 0 });
            }
            d[0] = 0;
            holes[0] = new SortedSet<int> { 0, 1, 2, 3, 4, 5, 6, 7 };
            
            krawedzie[2].szczeliny.Add(3);
            krawedzie[4].szczeliny.Add(3);
            krawedzie[3].szczeliny.Add(3);
            krawedzie[0].szczeliny.Add(3);

            for (int i = 0; i < krawedzie.Count; i++)
            {
                if (krawedzie[i].PodajPoczatek() == a)
                {
                    kolejka.Add(krawedzie[i]);
                }
            }
            foreach (var krawedz in kolejka)
            {
                droga.Add(krawedz.PodajWage());
            }

            Console.WriteLine(droga[1]);
            grafdijkstry.wezly.Add(wezly[a - 1]);
            grafdijkstry.iloscwezlow++;

            int min = 12345677;
            int kraw = 0;
            bool war1 = false;
            Krawedz actualEdge;
            while (grafdijkstry.iloscwezlow != iloscwezlow)
            {
                min = 7777777;
                war1 = false;
                for (int i = 0; i < droga.Count; i++)
                {
                    if (droga[i] < min)
                    {
                        min = droga[i];     
                        kraw = i;
                    }
                }
                int kkk = 0;
                Console.WriteLine(kolejka[kraw].PodajPoczatek());
                Console.WriteLine(kolejka[kraw].PodajKoniec());

                actualEdge = kolejka[kraw];
                for (int j = 0; j < grafdijkstry.wezly.Count; j++)
                {                   
                    if (actualEdge.PodajKoniec() == grafdijkstry.wezly[j].PodajId())
                        war1 = true;
                }

                if (war1 == false)
                {
                    if(d[actualEdge.PodajKoniec() - 1] > d[actualEdge.PodajPoczatek() - 1] + actualEdge.PodajWage()) {

                        p[actualEdge.PodajKoniec() - 1] = actualEdge.PodajPoczatek();
                        d[actualEdge.PodajKoniec() - 1] = d[actualEdge.PodajPoczatek() - 1] + actualEdge.PodajWage();
                        SortedSet<int> hs = new SortedSet<int>();
                        foreach (int liczba in holes[actualEdge.PodajPoczatek() - 1])
                            hs.Add(liczba);
                        hs.IntersectWith(actualEdge.podajSzeliny());
                        holes[actualEdge.PodajKoniec() - 1].Clear();
                        foreach (int liczba in hs)
                            holes[actualEdge.PodajKoniec() - 1].Add(liczba);
                    }

                    grafdijkstry.krawedzie.Add(actualEdge);
                    grafdijkstry.ilosckrawedzi++;
                    grafdijkstry.wezly.Add(wezly[actualEdge.PodajKoniec() - 1]);
                    grafdijkstry.iloscwezlow++;

                    for (int i = 0; i < krawedzie.Count; i++)
                    {
                        if (krawedzie[i].PodajPoczatek() == actualEdge.PodajKoniec())
                        {
                            bool war2 = false;
                            for (int j = 0; j < kolejka.Count; j++)
                            {
                                if (krawedzie[i].PodajKoniec() == kolejka[j].PodajPoczatek() && krawedzie[i].PodajPoczatek() == kolejka[j].PodajKoniec())
                                    war2 = true;
                            }
                            if (war2 == false)
                            {
                                kolejka.Add(krawedzie[i]);
                                droga.Add(krawedzie[i].PodajWage() + droga[kraw]);
                            }
                        }

                    }

                }
                kolejka.RemoveAt(kraw);
                droga.RemoveAt(kraw);
            }
            Console.WriteLine( "--------------------------------------------------");
            for (int i = 0; i < wezly.Count; i++)
            {
                Console.WriteLine("Tablica d: " + d[i]);
            }
            Console.WriteLine("--------------------------------------------------");
            for (int i = 0; i < wezly.Count; i++)
            {
                Console.WriteLine("Tablica p: " + p[i]);
            }
            Console.WriteLine("--------------------------------------------------");
            
                Console.WriteLine("Tablica szczelinowa: ");
                foreach(SortedSet<int> bb in holes)
                {
                    foreach(int y in bb)
                        Console.Write(y + " ");
                    Console.WriteLine();
                }
            Console.WriteLine("--------------------------------------------------");
            

            Graf dijkstra2 = new Graf(0, 0);
            int lastestNode = x;
            int nextNodeToAdd = 0;
            while (a != lastestNode)
            {
                dijkstra2.wezly.Add(wezly[lastestNode - 1]);
                dijkstra2.iloscwezlow++;
                nextNodeToAdd = p[lastestNode - 1];
                if (podajKrawedz(nextNodeToAdd, lastestNode) == -1)
                {
                    dijkstra2.iloscwezlow = 0;
                    break;
                }
               
               dijkstra2.krawedzie.Add(krawedzie[podajKrawedz(nextNodeToAdd, lastestNode) - 1]);
               dijkstra2.ilosckrawedzi++;
               lastestNode = nextNodeToAdd;
            }

            if (dijkstra2.iloscwezlow > 0)
            {
                dijkstra2.wezly.Add(wezly[lastestNode - 1]);
                dijkstra2.iloscwezlow++;
            }

            List<int> returnValue = new List<int>();
            Console.WriteLine("TTTTTT: " + d[x - 1]);
            returnValue.Add(d[x-1]);
            returnValue.Add(-777);
            foreach(int number in returnContinuousSzczeliny(holes[x-1]))
            {
                returnValue.Add(x);
            }
            returnValue.Add(-777);
            foreach (Wezel wezel in dijkstra2.wezly)
            {
                returnValue.Add(wezel.PodajId());
            }
            return returnValue;
        }
        
        private int countContinuousSzczeliny(Krawedz krawedz)
        {
            // W TEJ WARTOSCI MOZE BYC BLAD, JEZELI INDEKS SZCZELINY BEDZIE ROWNY -18
            int y = -19;
            int countValue = 1, returnValue = 0;
            foreach(int x in krawedz.szczeliny)
            {
                if (x - 1 == y)
                    countValue++;
                else
                {
                    if(returnValue < countValue)
                        returnValue = countValue;
                    countValue = 1;
                }
                    
                y = x;
            }
            if (returnValue < countValue)
                returnValue = countValue;
            return returnValue;
        }

        private int countContinuousSzczeliny(SortedSet<int> set)
        {
            // W TEJ WARTOSCI MOZE BYC BLAD, JEZELI INDEKS SZCZELINY BEDZIE ROWNY -18
            int y = -19;
            int countValue = 1, returnValue = 0;
            foreach (int x in set)
            {
                if (x - 1 == y)
                    countValue++;
                else
                {
                    if (returnValue < countValue)
                        returnValue = countValue;
                    countValue = 1;
                }

                y = x;
            }
            if (returnValue < countValue)
                returnValue = countValue;
            return returnValue;
        }

        public SortedSet<int> returnContinuousSzczeliny(SortedSet<int> holes)
        {
            int y = -19;
            SortedSet<int> ss = new SortedSet<int>();
            SortedSet<int> returnSet = new SortedSet<int>();
            int countValue = 1, returnValue = 0;
            foreach (int x in holes)
            {
                
                if (x - 1 == y)
                {
                    countValue++;
                    ss.Add(x);
                }
                else
                {
                    if (returnValue < countValue)
                    {
                        returnValue = countValue;
                        returnSet.Clear();
                        foreach (int number in ss)
                            returnSet.Add(number);
                    }
                        ss.Clear();
                    countValue = 1;
                }

                y = x;
                if (countValue == 1)
                    ss.Add(x);
            }
            if (returnValue < countValue)
            {
                returnSet.Clear();
                foreach (int aa in ss)
                    returnSet.Add(aa);
            }
            return returnSet;
        }

        public Graf dijkstraSzczeliny(int a, int x)
        {
            Graf grafdijkstry = new Graf(0, 0);
            List<Krawedz> kolejka = new List<Krawedz>();
            List<double> droga = new List<double>();
            List<SortedSet<int>> holes = new List<SortedSet<int>>();
            List<int> pholes = new List<int>();
            for (int i = 0; i < wezly.Count; i++)
            {
                pholes.Add(-1);
                holes.Add(new SortedSet<int> { 0 });
            }
            holes[0] = new SortedSet<int> { 0, 1, 2, 3, 4, 5, 6, 7 };
            krawedzie[8].szczeliny.Add(3);
            krawedzie[9].szczeliny.Add(3);
            krawedzie[5].szczeliny.Add(3);
            krawedzie[4].szczeliny.Add(3);
            for (int i = 0; i < krawedzie.Count; i++)
            {
                if (krawedzie[i].PodajPoczatek() == a)
                {
                    kolejka.Add(krawedzie[i]);
                }
            }
            foreach (var krawedz in kolejka)
            {
                droga.Add(countContinuousSzczeliny(krawedz));
            }


            grafdijkstry.wezly.Add(wezly[a - 1]);
            grafdijkstry.iloscwezlow++;

            double max = -1;
            int kraw = 0;
            bool war1 = false;
            Krawedz actualEdge;
            while (grafdijkstry.iloscwezlow != iloscwezlow)
            {
                max = -1;
                war1 = false;
                for (int i = 0; i < droga.Count; i++)
                {
                    if (droga[i] > max)
                    {
                        max = droga[i];
                        kraw = i;

                    }
                }
                actualEdge = kolejka[kraw];
                for (int j = 0; j < grafdijkstry.wezly.Count; j++)
                {
                    if (actualEdge.PodajKoniec() == grafdijkstry.wezly[j].PodajId())
                    {
                        war1 = true;
                    }
                }

                if (war1 == false)
                {
                    Console.WriteLine(kolejka[kraw].PodajKoniec());

                    SortedSet<int> hs = new SortedSet<int>();
                    foreach (int liczba in holes[actualEdge.PodajPoczatek() - 1])
                        hs.Add(liczba);
                    hs.IntersectWith(actualEdge.podajSzeliny());
                    if (hs.Count >= holes[actualEdge.PodajKoniec() - 1].Count)
                    {
                        holes[actualEdge.PodajKoniec() - 1].Clear();
                        foreach (int liczba in hs)
                            holes[actualEdge.PodajKoniec() - 1].Add(liczba);
                        pholes[actualEdge.PodajKoniec() - 1] = actualEdge.PodajPoczatek();
                    }
                    

                    grafdijkstry.krawedzie.Add(actualEdge);
                    grafdijkstry.ilosckrawedzi++;
                    grafdijkstry.wezly.Add(wezly[actualEdge.PodajKoniec() - 1]);
                    grafdijkstry.iloscwezlow++;
                    for (int i = 0; i < krawedzie.Count; i++)
                    {
                        if (krawedzie[i].PodajPoczatek() == actualEdge.PodajKoniec())
                        {
                            bool war2 = false;
                            for (int j = 0; j < kolejka.Count; j++)
                            {
                                if (krawedzie[i].PodajKoniec() == kolejka[j].PodajPoczatek() && krawedzie[i].PodajPoczatek() == kolejka[j].PodajKoniec())
                                    war2 = true;
                            }
                            if (war2 == false)
                            {
                                kolejka.Add(krawedzie[i]);
                                //droga.Add(grafdijkstry.wezly.Find(krawedzie[i].PodajKoniec()));
                                //TUTAJ TRZEBA WSTAWIAC MAX SZCZELINY!!!!!
                                SortedSet<int> s1 = new SortedSet<int>() { 1, 2, 3, 4, 5, 6, 7 };
                                s1.IntersectWith(holes[krawedzie[i].PodajPoczatek() - 1]);
                                s1.IntersectWith(krawedzie[i].szczeliny);
                                droga.Add(countContinuousSzczeliny(s1));
                            }
                        }
                    }
                }
                kolejka.RemoveAt(kraw);
                droga.RemoveAt(kraw);
            }
            Console.WriteLine("--------------------------------------------------");
            for (int i = 0; i < wezly.Count; i++)
            {
                Console.WriteLine("Tablica holes: " + holes[i]);
                Console.WriteLine("Tablica pholes: " + pholes[i]);
            }

            Graf dijkstra2 = new Graf(0, 0);
            int lastestNode = x;
            int nextNodeToAdd = 0;
            while (a != lastestNode)
            {
                dijkstra2.wezly.Add(wezly[lastestNode - 1]);
                dijkstra2.iloscwezlow++;
                nextNodeToAdd = pholes[lastestNode - 1];
                if (podajKrawedz(nextNodeToAdd, lastestNode) == -1)
                {
                    dijkstra2.iloscwezlow = 0;
                    break;
                }

                dijkstra2.krawedzie.Add(krawedzie[podajKrawedz(nextNodeToAdd, lastestNode) - 1]);
                dijkstra2.ilosckrawedzi++;
                lastestNode = nextNodeToAdd;
            }

            if (dijkstra2.iloscwezlow > 0)
            {
                dijkstra2.wezly.Add(wezly[lastestNode - 1]);
                dijkstra2.iloscwezlow++;
            }

            return dijkstra2;
        }


    }
}