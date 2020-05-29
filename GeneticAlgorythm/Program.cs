using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeneticAlgorythm
{
    public class Gen
    {
        public List<int> dna;
        public List<int> num_proc;
        public List<int> weigth;
        public int loading;
 
        public Gen(List<int> T)
        {
            Random ran = new Random();
            dna = new List<int>();
            num_proc = new List<int>();
            weigth = new List<int>();
            for (int i = 0; i < T.Count; i++)
            {
                dna.Add(ran.Next(1, 256));
                num_proc.Add(-1);
            }
            //weigth = (T.ToArray()
            //            .OrderBy(v => ran.Next()))
            //         .ToList();
            weigth = T;
            loading = 0;
        }
 
        public Gen(Gen copy)
        {
            dna = copy.dna;
            num_proc = copy.num_proc;
            weigth = copy.weigth;
            loading = copy.loading;
        }
 
    }
 
    class GA
    {
 
        private int n, m, t1, t2, population, repeat, crossoverRate, mutationRate;
        private List<int> tasks;
        public List<int> Tasks { get => this.tasks; set => this.tasks = value; }
        public int N { get => n; }
        public int M { get => m; }
        public int T1 { get => t1; }
        public int T2 { get => t2; }
        public int Population { get => population; }
        public int Repeat { get => repeat; }
        public int CrossoverRate { get => crossoverRate; }
        public int MutationRate { get => mutationRate; }
        public List<Gen> Individuals = new List<Gen>();

        /// <summary>
        /// <para>
        ///n – количество устройств,
        ///m – количество задач,
        ///t1,t2 – пределы времени выполнения задания,
        ///p – размер популяции,
        ///cr – вероятность кроссовера %,
        ///mr – вероятность мутации %,
        ///proc – количество повторов.
        ///</para>
        /// </summary>
        public GA(int n = 3, int m = 7, int t1 = 30, int t2 = 45, int ps = 5, int cr = 80, int mr = 30, int proc = 5)
        {
            this.n = n;
            this.m = m;
            this.t1 = t1;
            this.t2 = t2;
            this.population = ps;
            this.repeat = proc;
            this.crossoverRate = cr;
            this.mutationRate = mr;
            this.tasks = new List<int>();
        }
        
        public void GenerateRandomTasks()
        {
            var ran = new Random();
            tasks.Clear();
            for (int i = 0; i < M; i++)
            {
                tasks.Add(ran.Next(t1, t2 + 1));
            }
        }

        //номер процесса, куда будет распределяться задание в зависимости от приспособленности
        public void DistributeProcessors(ref Gen individual)
        {
            int proc = 256 / this.n;
            int rMin = 0;
            int rMax = proc;
            int _p = this.n;
            while (_p != 0)
            {
                for (int i = 0; i < this.m; i++)
                    if (individual.dna[i] > rMin && individual.dna[i] <= rMax)
                        individual.num_proc[i] = this.n - _p;
                rMin = rMax;
                rMax += proc;
                _p--;
            }
        }
 
        //максимальная нагрузка особи
        public int CountIndividualLoading(Gen individual)
        {
            int[] loading = new int[this.n];
            int max = 0;
 
            for (int count = 0; count < this.n + 1; count++)
                for (int i = 0; i < this.m; i++)
                    if (individual.num_proc[i] == count)
                        loading[count] += individual.weigth[i];
 
            foreach (int maxTimeOnProc in loading)
                if (maxTimeOnProc > max)
                    max = maxTimeOnProc;
 
            return max;
        }
 
        public static void ReducePopulation(Gen[] all_ind, Gen desc_1, Gen desc_2)
        {
            int ind_max_1 = GetMaxLoadedIndividual(all_ind, -1);
            all_ind[ind_max_1] = (all_ind[ind_max_1].loading >= desc_1.loading) ? desc_1 : all_ind[ind_max_1];
            string rez_1 = (all_ind[ind_max_1].loading == desc_1.loading) ? "произведена замена особи " + (ind_max_1 + 1) + " на потомок 1 " : "потомок 1 не включен в популяцию";
            Console.WriteLine(rez_1);
            int ind_max_2 = GetMaxLoadedIndividual(all_ind, ind_max_1);
            all_ind[ind_max_2] = (all_ind[ind_max_2].loading >= desc_2.loading) ? desc_2 : all_ind[ind_max_2];
            string rez_2 = (all_ind[ind_max_2].loading == desc_2.loading) ? "произведена замена особи " + (ind_max_2 + 1) + " на потомок 2 " : "потомок 2 не включен в популяцию";
            Console.WriteLine(rez_2);
            Console.WriteLine();
        }
 
        // худшая особь в популяции
        public static int GetMaxLoadedIndividual(Gen[] all_ind, int ind)
        {
            int[] pop = new int[all_ind.Length];
            for (int i = 0; i < all_ind.Length; i++)
                if (i != ind)
                    pop[i] = all_ind[i].loading;
                else 
                    pop[i] = 0;
            int max_nagr = pop.Max();
 
            return Array.IndexOf<int>(pop, max_nagr);
        }
 
        // лучшая особь в популяции
        public int GetMinLoadedIndividual(Gen[] iterations)
        {
            int[] population = new int[iterations.Length];
            for (int i = 0; i < iterations.Length; i++)
                population[i] = iterations[i].loading;
            int minimalLoading = population.Min();
 
            return Array.IndexOf<int>(population, minimalLoading);
        }
 
        // мутация хромосомы
        public static int ToBin(int val, int inv)
        {
            int[] arr = new int[8];
            for (int i = 7; i >= 0; i--)
            {
                arr[i] = val % 2;
                val /= 2;
            }
            if (arr[7 - inv] == 1)
            {
                arr[7 - inv] = 0;
            }
            else
            {
                arr[7 - inv] = 1;
            }
            int rez = 0;
            for (int i = 7; i >= 0; i--)
            {
                if (arr[i] == 1)
                {
                    rez += (int)Math.Pow(2, 7 - i);
                }
            }
            return rez;
        }
 
        //оператор мутации
        public void Mutate(Gen desc_1, Gen desc_2)
        {
            Random ran = new Random();
            int randomMutationRate = ran.Next(0, 100);
            Console.WriteLine("\nСлучайная вероятность мутации= " + randomMutationRate + "%" );
            if (randomMutationRate > mutationRate)
            {
                Console.WriteLine("\nСлучайная вероятность мутации = " + randomMutationRate + "%"
                    + " > минимальная вероятность мутации = " + mutationRate + "%");
                Console.WriteLine("\nНет мутации");
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("\nСлучайная вероятность мутации = " + randomMutationRate + "%"
                    + " < минимальная вероятность мутации = " + mutationRate + "%");
                
                Console.WriteLine("---\t---\t---\t---\t---\t---");
                Console.WriteLine("\nПервый потомок");
                int gen_mut_1 = ran.Next(this.m);
                Console.WriteLine("\nВыбранный ген для мутации {0}", gen_mut_1 + 1);
                int bit_mut_1 = ran.Next(8);
                int bit_mut_2 = ran.Next(8);
                while (bit_mut_1 == bit_mut_2 || Math.Abs(bit_mut_1 - bit_mut_2) == 1)
                {
                    bit_mut_1 = ran.Next(8);
                    bit_mut_2 = ran.Next(8);
                }
                Console.WriteLine("\nВыбранный биты для мутации {0} и {1}", bit_mut_1 + 1, bit_mut_2 + 1);
                Console.WriteLine("\nЗначение гена до мутации {0}", desc_1.dna[gen_mut_1]);
                Console.WriteLine(Convert.ToString(desc_1.dna[gen_mut_1], 2).PadLeft(8, '0'));
                int new_z_1 = ToBin(desc_1.dna[gen_mut_1], bit_mut_1);
                new_z_1 = ToBin(new_z_1, bit_mut_2);
                desc_1.dna[gen_mut_1] = new_z_1;
                Console.WriteLine(Convert.ToString(desc_1.dna[gen_mut_1], 2).PadLeft(8, '0'));
                Console.WriteLine("\nЗначение гена после мутации {0}", new_z_1);
                DistributeProcessors(ref desc_1);
                desc_1.loading = CountIndividualLoading(desc_1);
                Console.WriteLine("\nПотомок после мутации");
                PrintGenInfo(desc_1, ConsoleColor.DarkGreen, ConsoleColor.Red);
 
                Console.WriteLine("---\t---\t---\t---\t---\t---");
                Console.WriteLine("\nВторой потомок");
                int gen_mut_2 = ran.Next(this.m);
                while (gen_mut_2 == gen_mut_1)
                {
                    gen_mut_2 = ran.Next(this.m);
                }
                Console.WriteLine("\nВыбранный ген для мутации {0}", gen_mut_2 + 1);
                bit_mut_1 = ran.Next(8);
                bit_mut_2 = ran.Next(8);
                while (bit_mut_1 == bit_mut_2 || Math.Abs(bit_mut_1 - bit_mut_2) == 1)
                {
                    bit_mut_1 = ran.Next(8);
                    bit_mut_2 = ran.Next(8);
                }
                Console.WriteLine("\nВыбранный биты для мутации {0} и {1}", bit_mut_1 + 1, bit_mut_2 + 1);
                Console.WriteLine("\nЗначение гена до мутации {0}", desc_2.dna[gen_mut_2]);
                Console.WriteLine(Convert.ToString(desc_1.dna[gen_mut_1], 2).PadLeft(8, '0'));
                int new_z_2 = ToBin(desc_2.dna[gen_mut_2], bit_mut_1);
                new_z_2 = ToBin(new_z_2, bit_mut_2);
                desc_2.dna[gen_mut_2] = new_z_2;
                Console.WriteLine(Convert.ToString(desc_1.dna[gen_mut_1], 2).PadLeft(8, '0'));
                Console.WriteLine("\nЗначение гена после мутации {0}", new_z_2);
                DistributeProcessors(ref desc_2);
                desc_2.loading = CountIndividualLoading(desc_2);
                Console.WriteLine("\nПотомок после мутации");
                PrintGenInfo(desc_2, ConsoleColor.DarkGreen, ConsoleColor.Red);
 
 
            }
        }
 
        public void IndividFromPopulation(Gen[] iterations)
        {
            foreach (Gen i in iterations)
            {
                Individuals.Add(i);
            }
        }

        //модификация оператора кросовера
        public void Crossover(Gen[] iterations, int j)
        {
            Random ran = new Random();
            int r_cross = ran.Next(0, 100);
            //Кроссовер
            int point_cross = ran.Next(1, m/2);
            int point_cross_2 = ran.Next(m/2, m);
            while (Math.Abs(point_cross_2 - point_cross) <= 1)
            {
                point_cross = ran.Next(1, m / 2);
                point_cross_2 = ran.Next(m / 2, m);
            }
            Console.WriteLine("______________________________________");
            Console.WriteLine("Случайная вероятность скрещивания = " + r_cross + "%");
            if (r_cross < crossoverRate)
            {
                Console.WriteLine("Случайная вероятность скрещивания = " + r_cross + "%"
                    + " < минимальная вероятность скрещивания = " + crossoverRate + "%");
                //Выбор особей для кроссовера
                int l = ran.Next(0, iterations.Length);
                int k = ran.Next(0, iterations.Length);
                while (l == j || k == j || l == k || Math.Abs(l - k) <= 1)
                {
                    l = ran.Next(0, iterations.Length);
                    k = ran.Next(0, iterations.Length);
                }
                Console.WriteLine("\nВыбранны особи номер {0} и {1}", l + 1, k + 1);
                Console.WriteLine("\nОсобь  {0}", l + 1);
                PrintGenInfo(iterations[l], ConsoleColor.Black, ConsoleColor.Red);
                Console.WriteLine("\nОсобь  {0}", k + 1);
                PrintGenInfo(iterations[k], ConsoleColor.Black, ConsoleColor.Red);
                if (CountIndividualLoading(iterations[l]) < CountIndividualLoading(iterations[k]))
                {
                    Console.WriteLine("Особь " + (l + 1) + " лучше особи " + (k + 1));
                    Console.WriteLine("Родителями являются особи " + (j + 1) + " и " + (l + 1));
                    k = j;
                }
                else
                {
                    Console.WriteLine("Особь " + (k + 1) + " лучше особи " + (l + 1));
                    Console.WriteLine("Родителями являются особи " + (j + 1) + " и " + (k + 1));
                    l = j;
                }
                Console.WriteLine("\nВыбранная точка кроссовера: {0} и {1}", point_cross, point_cross_2);
                Console.WriteLine("\nРодитель 1 : Особь {0}", k + 1);
                PrintGenInfo(iterations[k], point_cross, 2);
                Console.WriteLine("\nРодитель 2 : Особь {0}", l + 1);
                PrintGenInfo(iterations[l], point_cross, 1);


                Gen desc_1 = new Gen(iterations[l]);
                Gen desc_2 = new Gen(iterations[k]);

                //for (int i = point_cross; i < m; i++)
                //{
                //    int tmp = desc_1.dna[i];
                //    desc_1.dna[i] = desc_2.dna[i];
                //    desc_2.dna[i] = tmp;
                //}

                for (int i = point_cross; i < point_cross_2 - 1; i++)
                {
                    int tmp = desc_1.dna[i];
                    desc_1.dna[i] = desc_2.dna[i];
                    desc_2.dna[i] = tmp;
                }


                DistributeProcessors(ref desc_1);
                desc_1.loading = CountIndividualLoading(desc_1);
                DistributeProcessors(ref desc_2);
                desc_2.loading = CountIndividualLoading(desc_2);
                Console.WriteLine("\nПотомки кроссовера");
                Console.WriteLine("\nПотомок 1");
                PrintGenInfo(desc_2, m, 2);
                Console.WriteLine("\nПотомок 2");
                PrintGenInfo(desc_1, m, 1);

                Mutate(desc_1, desc_2);
                ReducePopulation(iterations, desc_1, desc_2);
                Individuals.Add(desc_1);
                Individuals.Add(desc_2);
            }
            else
            {
                Console.WriteLine("Случайная вероятность скрещивания = " + r_cross + "%"
                    + " > минимальная вероятность скрещивания = " + crossoverRate + "%");
                Console.WriteLine("\nОсобь переходит в новое поколение без изменений");
                Console.WriteLine("Поколение без изменений");
            }
            
        }
 
        public bool Fit(Gen[] last, int repet_best)
        {
            int best_last = last[GetMinLoadedIndividual(last)].loading;
            return best_last == repet_best;
        }
 
        public void PrintGenInfo(Gen[] iterations)
        {
            Random r = new Random();
            int color = 0; ;
            while (color == 4 || color == 6 || color == 12 || color == 14 || color == 0 || color == 7)
                color = r.Next(0, 15);
            Console.ForegroundColor = (ConsoleColor)color;
            for (int i = 0; i < iterations.Length; i++)
            {
                Console.WriteLine("{0}-я особь", i + 1);
                Console.Write("{0, 13}", "Ген:");
                foreach (int tt in iterations[i].dna)
                    Console.Write("{0,4}", tt);
                Console.WriteLine();
                Console.Write("{0, 13}", "Вес:");
                foreach (int tt in iterations[i].weigth)
                    Console.Write("{0,4}", tt);
                Console.WriteLine();
                Console.Write("{0, 13}", "Процессор:");
                foreach (int tt in iterations[i].num_proc)
                    Console.Write("{0,4}", tt + 1);
                Console.WriteLine();
                Console.WriteLine("{0,15}: {1}", "Нагрузка", iterations[i].loading);
                Console.WriteLine();
            }
            Console.WriteLine(); ;
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public void PrintGenInfo(Gen indiv, int from, int order)
        {
            Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("{0, 13}", "Ген:");
            if (order == 1)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
            }
            else if (order == 2)
            {
                Console.BackgroundColor = ConsoleColor.Yellow;
                Console.ForegroundColor = ConsoleColor.DarkGray;
            }
            int c = -1;
            foreach (int tt in indiv.dna)
            {
                c++;
                if (c == from)
                    if (order == 1)
                    {
                        Console.BackgroundColor = ConsoleColor.Yellow;
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                    }
                    else if (order == 2)
                    {
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                Console.Write("{0,4}", tt);
            }
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine();
            Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("{0, 13}", "Вес:");
            if (order == 1)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
            }
            else if (order == 2)
            {
                Console.BackgroundColor = ConsoleColor.Yellow;
                Console.ForegroundColor = ConsoleColor.DarkGray;
            }
            c = -1;
            foreach (int tt in indiv.weigth)
            {
                c++;
                if (c == from)
                    if (order == 1)
                    {
                        Console.BackgroundColor = ConsoleColor.Yellow;
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                    }
                    else if (order == 2)
                    {
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                Console.Write("{0,4}", tt);
            }
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine();
            Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("{0, 13}", "Процессор:");
            if (order == 1)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
            }
            else if (order == 2)
            {
                Console.BackgroundColor = ConsoleColor.Yellow;
                Console.ForegroundColor = ConsoleColor.DarkGray;
            }
            c = -1;
            foreach (int tt in indiv.num_proc)
            {
                c++;
                if (c == from)
                    if (order == 1)
                    {
                        Console.BackgroundColor = ConsoleColor.Yellow;
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                    }
                    else if (order == 2)
                    {
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                Console.Write("{0,4}", tt + 1);
            }
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("{0,15}: {1}", "Нагрузка", indiv.loading);
            Console.WriteLine();
        }

        // TODO
        public void PrintGenInfo(Gen indiv, int from, int to, int order)
        {
            Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("{0, 13}", "Ген:");
            if (order == 1)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
            }
            else if (order == 2)
            {
                Console.BackgroundColor = ConsoleColor.Yellow;
                Console.ForegroundColor = ConsoleColor.DarkGray;
            }
            int c = -1;
            foreach (int tt in indiv.dna)
            {
                c++;
                if (c == from)
                    if (order == 1)
                    {
                        Console.BackgroundColor = ConsoleColor.Yellow;
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                    }
                    else if (order == 2)
                    {
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                Console.Write("{0,4}", tt);
            }
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine();
            Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("{0, 13}", "Вес:");
            if (order == 1)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
            }
            else if (order == 2)
            {
                Console.BackgroundColor = ConsoleColor.Yellow;
                Console.ForegroundColor = ConsoleColor.DarkGray;
            }
            c = -1;
            foreach (int tt in indiv.weigth)
            {
                c++;
                if (c == from)
                    if (order == 1)
                    {
                        Console.BackgroundColor = ConsoleColor.Yellow;
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                    }
                    else if (order == 2)
                    {
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                Console.Write("{0,4}", tt);
            }
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine();
            Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("{0, 13}", "Процессор:");
            if (order == 1)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
            }
            else if (order == 2)
            {
                Console.BackgroundColor = ConsoleColor.Yellow;
                Console.ForegroundColor = ConsoleColor.DarkGray;
            }
            c = -1;
            foreach (int tt in indiv.num_proc)
            {
                c++;
                if (c == from)
                    if (order == 1)
                    {
                        Console.BackgroundColor = ConsoleColor.Yellow;
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                    }
                    else if (order == 2)
                    {
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                Console.Write("{0,4}", tt + 1);
            }
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("{0,15}: {1}", "Нагрузка", indiv.loading);
            Console.WriteLine();
        }

        public void PrintGenInfo(Gen indiv, ConsoleColor back, ConsoleColor front )
        {
            Console.ForegroundColor = front;
            Console.BackgroundColor = back;
            Console.Write("{0, 13}", "Ген:");
            foreach (int tt in indiv.dna)
                Console.Write("{0,4}", tt);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine();
            Console.ForegroundColor = front;
            Console.BackgroundColor = back;
            Console.Write("{0, 13}", "Вес:");
            foreach (int tt in indiv.weigth)
                Console.Write("{0,4}", tt);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine();
            Console.ForegroundColor = front;
            Console.BackgroundColor = back;
            Console.Write("{0, 13}", "Процессор:");
            foreach (int tt in indiv.num_proc)
                Console.Write("{0,4}", tt + 1);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine();
            Console.ForegroundColor = front;
            Console.BackgroundColor = back;
            Console.Write("{0,15}: {1,3}{2,21}", "Нагрузка", indiv.loading, " ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine();
        }

        public Gen[] GetBest()
        {
            PrintGenInfo(Individuals.ToArray());
            Gen[] best_gens = new Gen[Population];
            for (int i = 0; i < Population; i++)
            {
                best_gens[i] = Individuals[GetMinLoadedIndividual(Individuals.ToArray())];
                Individuals.Remove(best_gens[i]);
            }

            Console.WriteLine("Лучшие {0} из всех получившихся индивидов", Population);
            Console.WriteLine("vvvvvvvvvvvvvvvvvvvvvvvvvvvvvv");
            PrintGenInfo(best_gens);
            return best_gens;
            
        }
    }
 
    class Program
    {
 
        // Начальные параметры по умолчанию
        static GA Default()
        {
            GA algo = new GA();

            algo.GenerateRandomTasks();
 
            return algo;
        }
 
        // Настраиваемые начальные параметры
        static GA Custom()
        {
            Console.Write("Количество устройств N:");
            int n = int.Parse(Console.ReadLine());
            Console.Write("Количество задач M:");
            int m = int.Parse(Console.ReadLine());
            Console.Write("Нижний порог времени выполнения t1:");
            int t1 = int.Parse(Console.ReadLine());
            Console.Write("Верхний порог времени выполнения t2:");
            int t2 = int.Parse(Console.ReadLine());
            Console.Write("Размер популяции P:");
            int population = int.Parse(Console.ReadLine());
            Console.Write("Вероятность скрещивания CR(%):");
            int crossoverRate = int.Parse(Console.ReadLine());
            Console.Write("Вероятность мутации MR(%):");
            int mutationRate = int.Parse(Console.ReadLine());
            Console.Write("Количество поколений R:");
            int repeat = int.Parse(Console.ReadLine());
            GA algo = new GA(n, m, t1, t2, population, crossoverRate, mutationRate, repeat);
 
            Random r_main = new Random();
            List<int> tasks = new List<int>();
            for (int i = 0; i < algo.M; i++)
            {
                tasks.Add(r_main.Next(algo.T1, algo.T2));
            }
            algo.Tasks = tasks;
 
            return algo;
        }
 
 
 
        static void Main(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Gray;
            bool error = true;
            GA algo = new GA();
            while (error)
            {
                Console.Clear();
                Console.WriteLine(
                    "1 - Программа с начальными параметрами по умолчаниюю\n" +
                    "2 - Программа с заданными начальными параметрами.\n" +
                    "\n\ne - выход");
                switch (Console.ReadLine())
                {
                    case "1":
                        Console.Clear();
                        algo = Default();
                        error = false;
                        break;
                    case "2":
                        Console.Clear();
                        algo = Custom();
                        error = false;
                        break;
                    case "e":
                        return;
                    default:
                        break;
                }
            }
 
            // начальная популяция
            Gen[] iterations = new Gen[algo.Population];
            iterations[0] = new Gen(algo.Tasks);
            Random ran = new Random();
            for (int i = 1; i < iterations.Length; i++)
            {
                List<int> weight = new List<int>();
                for (int w = 0; w < algo.M; w++)
                {
                    weight.Add(ran.Next(algo.T1, algo.T2+1));
                }
                iterations[i] = new Gen(weight);
                System.Threading.Thread.Sleep(50);
            }
 
            //T max для каждой особи
            for (int i = 0; i < algo.Population; i++)
            {
                algo.DistributeProcessors(ref iterations[i]);
                iterations[i].loading = algo.CountIndividualLoading(iterations[i]);
            }
 
            algo.PrintGenInfo(iterations);
 
            int num_pop = 1;
            int best = algo.Repeat;

            //while (best != 0)
            //{
            //    int j = 0;
            //    while (j != algo.Population)
            //    {
                    //int individ = algo.GetMinLoadedIndividual(iterations);
                    //int currentBest = iterations[individ].loading;
                    //algo.Crossover(iterations, j);
                    //Console.WriteLine("номер популяции {0}", num_pop);
                    //j++;
                    //num_pop++;
                    //if (algo.Fit(iterations, currentBest))
                    //{
                    //    best--;
                    //}
                    //else best = algo.Repeat;

                    //if (best == 0)
                    //{
                    //    Console.WriteLine("\nРезультат достигнут");
                    //    Console.WriteLine("\nРешением является:");
                    //    algo.PrintGenInfo(iterations[individ], ConsoleColor.Green, ConsoleColor.Black);
                    //    break;
                    //}
                    //else
                    //{
                    //    Console.Write("Лучшая особь в популяции ");
                    //    Console.WriteLine("{0}", individ + 1);
                    //}
            //    }
            //}

            //int j = 0;
            //int best_individ = 0;
            //while (j != algo.Population)
            //{
            //    int individ = algo.GetMinLoadedIndividual(iterations);
            //    int currentBest = iterations[individ].loading;
            //    algo.Crossover(iterations, j);
            //    Console.WriteLine("номер популяции {0}", num_pop);
            //    j++;
            //    best_individ = individ;


            //    num_pop++;
            //    if (algo.Fit(iterations, currentBest))
            //    {
            //        best--;
            //    }
            //    else best = algo.Repeat;

            //}

            num_pop = 0;
            int individ = 0;
            int current_best = 0;
            best = 5;
            while (best != 0)
            {
                Console.WriteLine("номер популяции {0}", num_pop);
                individ = algo.GetMinLoadedIndividual(iterations);
                current_best = iterations[individ].loading;
                algo.IndividFromPopulation(iterations);
                for (int i = 0; i < algo.Population; i++)
                {
                    algo.Crossover(iterations, i);
                }

                iterations = algo.GetBest();
                int ind = algo.GetMinLoadedIndividual(iterations);
                int l = iterations[ind].loading;

                num_pop++;
                if (algo.Fit(iterations, current_best))
                {
                    best--;
                }
                else
                {
                    best = algo.Repeat;
                }
                Console.WriteLine("--------------------------------------------------------------");
                Console.Write("Лучшая особь в популяции");
                Console.WriteLine("  {0}   |", ind + 1);
                Console.Write("Нагрузка особи в популяции");
                Console.WriteLine("  {0}   |", l);
                Console.WriteLine("--------------------------------------------------------------");

            }
            Console.WriteLine("\nРезультат достигнут");
            Console.WriteLine("\nРешением является:");
            algo.PrintGenInfo(iterations[individ], ConsoleColor.Green, ConsoleColor.Black);




            Console.Write("\nДля завершения работы нажмите на любую кпонку...");
            Console.ReadKey();
        }
    }
}
