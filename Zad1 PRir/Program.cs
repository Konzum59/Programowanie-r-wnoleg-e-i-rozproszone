using System;
using System.Threading;
using System.Threading.Tasks;

namespace Semaphore
{
    class Program
    {
        static int coalInMine = 2000;
        static int coalPerMiner = 200;
        static int minedCoal= 0;
        static int msPerCoal= 10;
        static int cartRideTime = 2000;
        static SemaphoreSlim semaphore = new SemaphoreSlim(2, 2);
        static SemaphoreSlim depositSemaphore = new SemaphoreSlim(1, 1);

        static object lockObject = new object();

        static void Main(string[] args)
        {
            Task[] tasks = new Task[5];
            

                for (int i = 0; i < tasks.Length; i++)
                {
                    tasks[i%5] = Task.Run(() => AccessSharedResource());
                }
            

            Task.WaitAll(tasks);
            Console.WriteLine("Wartość współdzielonego zasobu na końcu: " + minedCoal);
        }

        static void AccessSharedResource()
        {
            do
            {

                int maxCapacity = 200;
                int coalInCart = 0;

                //śmiga do kopalni
                Console.WriteLine($"Górnik {Task.CurrentId} śmiga wagonikiem do kopalni");
                Thread.Sleep(cartRideTime);


                //kopanie
                semaphore.Wait();
                for (int i = 0; (i < maxCapacity && coalInMine > 0); i++)
                {
                    Thread.Sleep(msPerCoal);

                    lock (lockObject)
                    {
                        coalInMine--;
                        coalInCart++;
                    }

                }
                semaphore.Release();
                Console.WriteLine($"Górnik {Task.CurrentId} wydobył {coalInCart} jednostek węgla. W złożu pozostało: {coalInMine}");
                //Jazda wagonem

                Console.WriteLine($"Górnik {Task.CurrentId} śmiga do schowka");
                Thread.Sleep(cartRideTime);


                //wyładunek 
                depositSemaphore.Wait();
                for (int i = coalInCart; i > 0; i--)
                {
                    Thread.Sleep(msPerCoal);

                    lock (lockObject)
                    {
                        minedCoal++;

                    }

                }
                depositSemaphore.Release();

                Console.WriteLine($"Górnik {Task.CurrentId} schował swój wegiel. W schowdku jest: {minedCoal}");
            }
            while (coalInMine > 0);
        }
    }
}
