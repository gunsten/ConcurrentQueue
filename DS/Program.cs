using System;
using System.Threading;
using Queue;

namespace DS
{
    class Consumer
    {
        public Consumer(ConcurrentQueue<int> queue)
        {
            this.queue = queue;
        }
        public void Sum()
        {
            while(run)
            {
                try
                {
                    sum += queue.deQueue();
                } catch(EmptyQueueException)
                {
                    Thread.Sleep(100);
                }
            }
        }

        public void Stop()
        {
            run = false;
        }

        public int GetResult()
        {
            return sum;
        }

        private int sum = 0;
        private bool run = true;
        private ConcurrentQueue<int> queue;
    }

    class Producer
    {
        public Producer(ConcurrentQueue<int> queue, int amount, int intSize)
        {
            this.queue = queue;
            this.amount = amount;
            this.intSize = intSize;
            list = new int[amount];
        }
        public void Produce()
        {
            var rand = new Random();
            for (int i = 0; i < amount; i++)
            {
                list[i] = rand.Next() % intSize;
                queue.enQueue(list[i]);
            }
        }
        public int Sum()
        {
            int sum = 0;
            for (int i = 0; i < amount; i++)
            {
                sum += list[i];
            }
            return sum;
        }
        private ConcurrentQueue<int> queue;
        private int[] list;
        private int amount, intSize;
    }

    class MainClass
    {
        public static void Main(string[] args)
        {
            //testQueue();
            //testLNode();

            ConcurrentQueue<int> queue = new ConcurrentQueue<int>();

            const int THREADS = 4;

            Thread[] consumerts = new Thread[THREADS];
            Consumer[] consumers = new Consumer[THREADS];
            Producer producer = new Producer(queue, 100000, 10);
            Thread producert = new Thread(new ThreadStart(producer.Produce));

            for(int i=0;i<THREADS;i++)
            {
                Consumer temp = new Consumer(queue);
                consumers[i] = temp;
                consumerts[i] = new Thread(new ThreadStart(temp.Sum));
            }

            producert.Start();

            for (int i = 0; i < THREADS; i++)
            {
                consumerts[i].Start();
            }

            producert.Join(); // wait for producer to finish

            //Busy-wait, TODO: solve with call-back
            while(!queue.isEmpty())
            {
                Thread.Sleep(100);
            }

            // queue empty, stop threads
            for (int i = 0; i < THREADS; i++)
            {
                consumers[i].Stop();
                consumerts[i].Join(); // Wait for consumer to finish
            }

            Console.WriteLine("Producer sum: " + producer.Sum());

            int sum = 0;
            for (int i = 0; i < THREADS; i++)
            {
                sum += consumers[i].GetResult();
            }
            Console.WriteLine("Consumers sum: " + sum);
        }

        public static void testLNode()
        {
            LNode<int> node = new LNode<int>(null, 4);
            node.acquire();
            Console.WriteLine("LNode, expect: 4, result: " + node.getContent());
            node.release();
            bool exc = false;
            try
            {
                Console.WriteLine("LNode, should crash: " + node.getContent());
            } catch(ConcurrentQueueException)
            {
                exc = true;
            }
            Console.WriteLine("LNode locked: " + exc);
        }

        public static void testQueue()
        {
            Queue<int> queue = new Queue<int>();
            Console.WriteLine("isEmpty, expects: True, result: " + queue.isEmpty());
            queue.enQueue(1);
            Console.WriteLine("Content, expects: 1, result: " + queue.peek());
            queue.deQueue();
            Console.WriteLine("deQueued, expects: True, result: " + queue.isEmpty());
            Console.WriteLine("Order, expects: True, result: " + testQueueRandom(50, 5));
            bool exc = false;
            try
            {
                queue.deQueue();
            }
            catch (EmptyQueueException)
            {
                exc = true;
            }
            Console.WriteLine("Exc on deQueue, expects: True, result: " + exc);
            exc = false;
            try
            {
                queue.peek();
            }
            catch (EmptyQueueException)
            {
                exc = true;
            }
            Console.WriteLine("Exc on peek, expects: True, result: " + exc);
        }

        public static bool testQueueRandom(int listSize, int intSize)
        {
            var rand = new Random();
            int[] list = new int[listSize];
            Queue<int> queue = new Queue<int>();
            for(int i=0;i<listSize;i++) 
            {
                list[i] = rand.Next() % intSize;
                queue.enQueue(list[i]);
            }
            // Test order
            for(int i=0;i<listSize;i++)
            {
                if(queue.deQueue() != list[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
