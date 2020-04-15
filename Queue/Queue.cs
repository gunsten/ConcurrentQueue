using System;
using System.Threading;

namespace Queue
{
    public class Queue<T>
    {
        public Queue()
        {
            start = end = null;
        }

        public void enQueue(T content)
        {
            Node<T> temp = new Node<T>(content);

            if(isEmpty())
            {
                start = end = temp;
                return;
            }

            end.setNext(temp); // Add temp node last
            end = temp; // Change pointer
        }

        public T deQueue()
        {
            if (isEmpty())
            {
                throw new EmptyQueueException("Queue is empty");
            }
            Node<T> temp = start; // Store node to dequeue
            start = start.getNext(); // Move pointer

            // If start becomes null, queue is empty
            if(start == null) { end = null; }

            return temp.getContent();
        }

        public T peek()
        {
            if (isEmpty())
            {
                throw new EmptyQueueException("Queue is empty");
            }
            return start.getContent();
        }

        public bool isEmpty()
        {
            return end == null;
        }

        private Node<T> start, end;
    }

    public class ConcurrentQueue<T>
    {
        public ConcurrentQueue()
        {
            start = end = null;
        }

        public void enQueue(T content)
        {
            endPointerMutex.WaitOne();
            Node<T> temp = new Node<T>(content);

            if(end == null) // Queue is empty, dequeue is impossible and queue is effectively locked
            {               // Thus safe to modify start
                start = end = temp;
                endPointerMutex.ReleaseMutex();
                return;
            }

            end.setNext(temp); // Add temp node last
            end = temp; // Change pointer

            endPointerMutex.ReleaseMutex();
        }

        public T deQueue()
        {
            startPointerMutex.WaitOne();
            if (start == null)
            {
                startPointerMutex.ReleaseMutex();
                throw new EmptyQueueException("Queue is empty");
            }

            T temp = start.getContent(); // Store node to dequeue

            start = start.getNext(); // Move pointer

            // If start becomes null, queue is empty
            if(start == null) {
                endPointerMutex.WaitOne();
                end = null;
                endPointerMutex.ReleaseMutex();
            }

            startPointerMutex.ReleaseMutex();

            return temp;
        }

        public T peek()
        {
            startPointerMutex.WaitOne();
            if (start == null)
            {
                startPointerMutex.ReleaseMutex();
                throw new EmptyQueueException("Queue is empty");
            }
            T content = start.getContent();
            startPointerMutex.ReleaseMutex();
            return content;
        }

        // TODO: probably not needed if call-backs implemented
        public bool isEmpty()
        {
            endPointerMutex.WaitOne();
            bool isempty = end == null;
            endPointerMutex.ReleaseMutex();
            return isempty;
        }

        private Node<T> start, end;
        private Mutex startPointerMutex = new Mutex(), endPointerMutex = new Mutex();
    }

    public class Node<T>
    {
        public Node()
        {
            next = null;
            content = default(T);
        }

        public Node(T content)
        {
            next = null;
            this.content = content;
        }

        public Node(Node<T> next, T content)
        {
            this.next = next;
            this.content = content;
        }

        public Node<T> getNext()
        {
            return next;
        }

        public T getContent()
        {
            return content;
        }

        public void setContent(T content)
        {
            this.content = content;
        }

        public void setNext(Node<T> next)
        {
            this.next = next;
        }

        private Node<T> next;
        private T content;
    }

    public class LNode<T> : Node<T>
    {
        public LNode() : base() { }

        public LNode(T content) : base(content) { }

        public LNode(LNode<T> next, T content) : base(next, content) { }

        public void acquire()
        {
            mutex.WaitOne();
            owner = Thread.CurrentThread;
        }

        public void release()
        {
            owner = null;
            mutex.ReleaseMutex();
        }

        public new LNode<T> getNext()
        {
            if(!Thread.CurrentThread.Equals(owner))
            {
                throw new ConcurrentQueueException("getNext: Current thread is not owner");
            }
            return (LNode<T>) base.getNext();
        }

        public new T getContent()
        {
            if (!Thread.CurrentThread.Equals(owner))
            {
                throw new ConcurrentQueueException("getContent: Current thread is not owner");
            }
            return base.getContent();
        }

        public new void setContent(T content)
        {
            if (!Thread.CurrentThread.Equals(owner))
            {
                throw new ConcurrentQueueException("setContent: Current thread is not owner");
            }
            base.setContent(content);
        }

        public new void setNext(Node<T> next)
        {
            if (!Thread.CurrentThread.Equals(owner))
            {
                throw new ConcurrentQueueException("setNext: Current thread is not owner");
            }
            base.setNext(next);
        }

        ~LNode()
        {
            mutex.Dispose();
        }

        Mutex mutex = new Mutex();
        Thread owner = null;
    }

    public class EmptyQueueException : Exception
    {
        public EmptyQueueException(string message) : base(message) { }
    }

    public class ConcurrentQueueException : Exception
    {
        public ConcurrentQueueException(string message) : base(message) { }
    }
}
