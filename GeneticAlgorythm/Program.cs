using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GeneticAlgorythms
{
    public class Gen
    {
        public readonly List<int> Dna;
        public readonly List<int> NumProc;
        public readonly List<int> Weigth;
        public int Loading;

        // Конструктор создания гена
        public Gen(List<int> T)
        {
            var ran = new Random();
            
            Dna = new List<int>();
            NumProc = new List<int>();
            Weigth = new List<int>();
            for (var i = 0; i < T.Count; i++)
            {
                Dna.Add(ran.Next(1, 256));
                NumProc.Add(-1);
            }
            Weigth = (T.ToArray()
                        .OrderBy(v => ran.Next()))
                     .ToList();
            Loading = 0;
        }

        // Конструктор копирования гена
        public Gen(Gen copy)
        {
            Dna = copy.Dna;
            NumProc = copy.NumProc;
            Weigth = copy.Weigth;
            Loading = copy.Loading;
        }

    }

    internal class GeneticAlgorithm
    {
        public List<int> Tasks { get; set; }
        private int N { get; }
        public int M { get; }
        public int T1 { get; }
        public int T2 { get; }
        public int Population { get; }
        public int Repeat { get; }
        private int CrossoverRate { get; }
        private int MutationRate { get; }

        public GeneticAlgorithm(int n = 3, int m = 7, int t1 = 30, int t2 = 45, int ps = 5, int cr = 80, int mr = 30, int proc = 10)
        {
            N = n;
            M = m;
            T1 = t1;
            T2 = t2;
            Population = ps;
            Repeat = proc;
            CrossoverRate = cr;
            MutationRate = mr;
            Tasks = new List<int>();
        }

        // номер процесса, куда будет распределяться задание в зависимости от приспособленности
        // метод меняет процессоры индивида передаваемого в аргументах
        public void DistributeProcessors(ref Gen individual)
        {
            var proc = 256 / N;
            var rMin = 0;
            var rMax = proc;
            var p = N;
            while (p != 0)
            {
                for (var i = 0; i < M; i++)
                    if (individual.Dna[i] > rMin && individual.Dna[i] <= rMax)
                        individual.NumProc[i] = N - p;
                rMin = rMax;
                rMax += proc;
                p--;
            }
        }

        //максимальная нагрузка особи
        public int CountIndividualLoading(Gen individual)
        {
            var loading = new int[N];

            for (int count = 0; count < N + 1; count++)
                for (int i = 0; i < M; i++)
                    if (individual.NumProc[i] == count)
                        loading[count] += individual.Weigth[i];

            return loading.Concat(new[] {0}).Max();
        }

        private static void ReducePopulation(Gen[] allInd, Gen desc1, Gen desc2)
        {
            var ind_max_1 = GetMaxLoadedIndividual(allInd, -1);
            allInd[ind_max_1] = (allInd[ind_max_1].Loading >= desc1.Loading) ? desc1 : allInd[ind_max_1];
            var rez_1 = (allInd[ind_max_1].Loading == desc1.Loading) ? "произведена замена особи " + (ind_max_1 + 1) + " на потомок 1 " : "потомок 1 не включен в популяцию";
            Console.WriteLine(rez_1);
            var ind_max_2 = GetMaxLoadedIndividual(allInd, ind_max_1);
            allInd[ind_max_2] = (allInd[ind_max_2].Loading >= desc2.Loading) ? desc2 : allInd[ind_max_2];
            var rez_2 = (allInd[ind_max_2].Loading == desc2.Loading) ? "произведена замена особи " + (ind_max_2 + 1) + " на потомок 2 " : "потомок 2 не включен в популяцию";
            Console.WriteLine(rez_2);
            Console.WriteLine();
        }

        // худшая особь в данной популяции
        private static int GetMaxLoadedIndividual(IReadOnlyList<Gen> allInd, int ind)
        {
            var pop = new int[allInd.Count];
            for (var i = 0; i < allInd.Count; i++)
                if (i != ind)
                    pop[i] = allInd[i].Loading;
                else 
                    pop[i] = 0;
            var maxLoading = pop.Max();

            return Array.IndexOf(pop, maxLoading);
        }

        // лучшая особь в данной популяции
        public static int GetMinLoadedIndividual(Gen[] iterations)
        {
            var population = new int[iterations.Length];
            for (var i = 0; i < iterations.Length; i++)
                population[i] = iterations[i].Loading;
            var minimalLoading = population.Min();

            return Array.IndexOf(population, minimalLoading);
        }

        // изменение бита хромосомы
        private static int ToBin(int val, int inv)
        {
            var arr = new int[8];
            for (var i = 7; i >= 0; i--)
            {
                arr[i] = val % 2;
                val /= 2;
            }

            foreach (var t in arr)
            {
                Console.Write(t);
            }

            Console.WriteLine();
            if (arr[7 - inv] == 1)
            {
                arr[7 - inv] = 0;
            }
            else
            {
                arr[7 - inv] = 1;
            }

            foreach (var t in arr)
            {
                Console.Write(t);
            }

            Console.WriteLine();
            var rez = 0;
            for (var i = 7; i >= 0; i--)
            {
                if (arr[i] == 1)
                {
                    rez += (int)Math.Pow(2, 7 - i);
                }
            }
            return rez;
        }

        //оператор мутации
        private void Mutate(Gen desc1, Gen desc2)
        {
            var ran = new Random();
            var randomMutationRate = ran.Next(0, 100);
            Console.WriteLine("\nСлучайная вероятность мутации= " + randomMutationRate + "%" );
            if (randomMutationRate > MutationRate)
            {
                Console.WriteLine("\nСлучайная вероятность мутации = " + randomMutationRate + "%"
                    + " >= минимальная вероятность мутации = " + MutationRate + "%");
                Console.WriteLine("\nНет мутации");
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("\nСлучайная вероятность мутации = " + randomMutationRate + "%"
                    + " < минимальная вероятность мутации = " + MutationRate + "%");
                
                Console.WriteLine("***\n***\t***\t***\t***\t***");
                Console.WriteLine("\nПервый потомок");
                var gen_mut_1 = ran.Next(M);
                Console.WriteLine("\nВыбранный ген для мутации {0}", gen_mut_1 + 1);
                var bit_mut_1 = ran.Next(8);
                Console.WriteLine("\nВыбранный бит для мутации {0}", bit_mut_1 + 1);
                Console.WriteLine("\nВыбранное ген для мутации {0}", desc1.Dna[gen_mut_1]);
                var new_z_1 = ToBin(desc1.Dna[gen_mut_1], bit_mut_1);
                Console.WriteLine("\nЗначение гена после мутации {0}", new_z_1);
                desc1.Dna[gen_mut_1] = new_z_1;
                DistributeProcessors(ref desc1);
                desc1.Loading = CountIndividualLoading(desc1);
                Console.WriteLine("\nПотомок после мутации");
                
                PrintGenInfo(desc1);

                Console.WriteLine("***\n***\t***\t***\t***\t***");
                Console.WriteLine("\nВторой потомок");
                var gen_mut_2 = ran.Next(M);
                Console.WriteLine("\nВыбранный ген для мутации {0}", gen_mut_2 + 1);
                var bit_mut_2 = ran.Next(8);
                Console.WriteLine("\nВыбранный бит для мутации {0}", bit_mut_2 + 1);
                Console.WriteLine("\nВыбранное ген для мутации {0}", desc2.Dna[gen_mut_2]);
                var new_z_2 = ToBin(desc2.Dna[gen_mut_2], bit_mut_2);
                Console.WriteLine("\nЗначение гена после мутации {0}", new_z_2);
                desc2.Dna[gen_mut_2] = new_z_2;
                DistributeProcessors(ref desc2);
                desc2.Loading = CountIndividualLoading(desc2);
                Console.WriteLine("\nПотомок после мутации");

                PrintGenInfo(desc2);

            }
        }

        //модификация оператора кросовера
        public void Crossover(Gen[] iterations, int j)
        {
            var ran = new Random();
            var r_cross = ran.Next(0, 100);
            //Кроссовер
            var point_cross = ran.Next(1, M);
            Console.WriteLine("\n");
            Console.WriteLine("Случайная вероятность скрещивания = " + r_cross + "%");
            if (r_cross < CrossoverRate)
            {
                Console.WriteLine("Случайная вероятность скрещивания = " + r_cross + "%"
                    + " < минимальная вероятность скрещивания = " + CrossoverRate + "%");
                //Выбор особей для кроссовера
                var l = ran.Next(0, iterations.Length);
                var k = ran.Next(0, iterations.Length);
                while (l == j || k == j || l == k)
                {
                    l = ran.Next(0, iterations.Length);
                    k = ran.Next(0, iterations.Length);
                }
                Console.WriteLine("\nВыбранны особи номер {0} и {1}", l + 1, k + 1);
                Console.WriteLine("\nОсобь  {0}", l + 1);
                PrintGenInfo(iterations[l]);
                Console.WriteLine("\nОсобь  {0}", k + 1);
                PrintGenInfo(iterations[k]);
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
                Console.WriteLine("\nВыбранная точка кроссовера: {0}", point_cross);
                Console.WriteLine("\nРодитель 1 : Особь {0}", k + 1);
                PrintGenInfo(iterations[k], point_cross);
                Console.WriteLine("\nРодитель 2 : Особь {0}", l + 1);
                PrintGenInfo(iterations[l], point_cross);
                
                // индивид 1
                var desc_1 = new Gen(iterations[l]);
                // индивид 2
                var desc_2 = new Gen(iterations[k]);

                for (int i = point_cross; i < M; i++)
                {
                    int tmp = desc_1.Dna[i];
                    desc_1.Dna[i] = desc_2.Dna[i];
                    desc_2.Dna[i] = tmp;
                }

                DistributeProcessors(ref desc_1);
                desc_1.Loading = CountIndividualLoading(desc_1);
                DistributeProcessors(ref desc_2);
                desc_2.Loading = CountIndividualLoading(desc_2);
                Console.WriteLine("\nПотомки кроссовера");
                Console.WriteLine("\nПотомок 1");
                PrintGenInfo(desc_1, M);
                Console.WriteLine("\nПотомок 2");
                PrintGenInfo(desc_2, M);

                Mutate(desc_1, desc_2);
                ReducePopulation(iterations, desc_1, desc_2);

            }
            else
            {
                Console.WriteLine("Случайная вероятность скрещивания = " + r_cross + "%"
                    + " > минимальная вероятность скрещивания = " + CrossoverRate + "%");
                Console.WriteLine("\nОсобь переходит в новое поколение без изменений");
            }
            Console.WriteLine("Новая популяция");
            PrintGenInfo(iterations);
        }

        public static bool Fit(Gen[] last, int repetBest)
        {
            var best_last = last[GetMinLoadedIndividual(last)].Loading;
            return best_last == repetBest;
        }

        public void PrintGenInfo(Gen[] iterations)
        {
            var r = new Random();
            var color = 0; ;
            while (color == 4 || color == 6 || color == 12 || color == 14 || color == 0 || color == 7)
                color = r.Next(0, 15);
            for (var i = 0; i < iterations.Length; i++)
            {
                Console.WriteLine("{0}-я особь", i + 1);
                Console.Write("{0, 13}", "Ген:");
                foreach (var tt in iterations[i].Dna)
                    Console.Write("{0,4}", tt);
                Console.WriteLine();
                Console.Write("{0, 13}", "Вес:");
                foreach (var tt in iterations[i].Weigth)
                    Console.Write("{0,4}", tt);
                Console.WriteLine();
                Console.Write("{0, 13}", "Процессор:");
                foreach (var tt in iterations[i].NumProc)
                    Console.Write("{0,4}", tt + 1);
                Console.WriteLine();
                Console.WriteLine("{0,15}: {1}", "Нагрузка", iterations[i].Loading);
                Console.WriteLine();
            }
            Console.WriteLine(); ;
        }

        public void PrintGenInfo(Gen indiv, int border)
        {
            Console.Write("{0, 13}", "Ген:");

            foreach (int tt in indiv.Dna)
            {
                Console.Write("{0,4}", tt);
            }
            Console.WriteLine();
            Console.Write("{0, 13}", "Вес:");
            foreach (int tt in indiv.Weigth)
            {
                Console.Write("{0,4}", tt);
            }
            Console.WriteLine();
            Console.Write("{0, 13}", "Процессор:");
            foreach (int tt in indiv.NumProc)
            {
                Console.Write("{0,4}", tt + 1);
            }
            Console.WriteLine();
            Console.WriteLine("{0,15}: {1}", "Нагрузка", indiv.Loading);
            Console.WriteLine();
        }

        public void PrintGenInfo(Gen indiv )
        {
            Console.Write("{0, 13}", "Ген:");
            foreach (var tt in indiv.Dna)
            {
                Console.Write("{0,4}", tt);
            }
            Console.WriteLine();
            Console.Write("{0, 13}", "Вес:");
            foreach (var tt in indiv.Weigth)
            {
                Console.Write("{0,4}", tt);
            }

            Console.WriteLine();
            Console.Write("{0, 13}", "Процессор:");
            foreach (var tt in indiv.NumProc)
            {
                Console.Write("{0,4}", tt + 1);
            }
            Console.WriteLine();
            Console.Write("{0,15}: {1,3}{2,21}", "Нагрузка", indiv.Loading, " ");
            Console.WriteLine();
        }

    }

    internal static class Program
    {

        // Настраиваемые начальные параметры
        private static GeneticAlgorithm Custom()
        {
            Console.Write("Количество устройств N:");
            var n = int.Parse(Console.ReadLine());
            Console.Write("Количество задач M:");
            var m = int.Parse(Console.ReadLine());
            Console.Write("Нижний порог времени выполнения t1:");
            var t1 = int.Parse(Console.ReadLine());
            Console.Write("Верхний порог времени выполнения t2:");
            var t2 = int.Parse(Console.ReadLine());
            Console.Write("Размер популяции P:");
            var population = int.Parse(Console.ReadLine());
            Console.Write("Вероятность скрещивания CR(%):");
            var crossoverRate = int.Parse(Console.ReadLine());
            Console.Write("Вероятность мутации MR(%):");
            var mutationRate = int.Parse(Console.ReadLine());
            Console.Write("Количество поколений, в которых лучшая особь не меняется R:");
            var repeat = int.Parse(Console.ReadLine());
            var parameters = new GeneticAlgorithm(n, m, t1, t2, population, crossoverRate, mutationRate, repeat);

            var r_main = new Random();
            var tasks = new List<int>();
            for (var i = 0; i < parameters.M; i++)
            {
                tasks.Add(r_main.Next(parameters.T1, parameters.T2));
            }
            parameters.Tasks = tasks;

            return parameters;
        }


        private static void Main(string[] args)
        {
            var parameters = Custom();
            
            var iterations = new Gen[parameters.Population];
            iterations[0] = new Gen(parameters.Tasks);
            for (var i = 1; i < iterations.Length; i++)
            {
                iterations[i] = new Gen(iterations[i - 1].Weigth);
                Thread.Sleep(150);
            }

            for (var i = 0; i < parameters.Population; i++)
            {
                parameters.DistributeProcessors(ref iterations[i]);
                iterations[i].Loading = parameters.CountIndividualLoading(iterations[i]);
            }

            parameters.PrintGenInfo(iterations);

            var numPop = 1;
            var best = parameters.Repeat;
            while (best != 0)
            {
                var j = 0;
                while (j != parameters.Population)
                {
                    var individual = GeneticAlgorithm.GetMinLoadedIndividual(iterations);
                    var currentBest = iterations[individual].Loading;
                    parameters.Crossover(iterations, j);
                    
                    Console.WriteLine("номер популяции {0}", numPop);
                    j++;
                    numPop++;
                    
                    if (GeneticAlgorithm.Fit(iterations, currentBest))
                    {
                        best--;
                    }
                    else
                    {
                        best = parameters.Repeat;
                    }

                    if (best == 0)
                    {
                        Console.WriteLine("\nРезультат достигнут");
                        Console.WriteLine("\nРешением является:");
                        parameters.PrintGenInfo(iterations[individual]);
                        break;
                    }

                    Console.Write("Лучшая особь в популяции ");
                    Console.WriteLine("{0}", individual + 1);
                }
            }

            Console.Write("\nДля завершения работы нажмите на любую кпонку...");
            Console.ReadKey();
        }
    }
}
