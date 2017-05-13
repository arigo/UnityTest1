using System.Collections.Generic;
using System.Threading;


public class PCQueue<T>
{
    /* Producer/consumer queue */

    EventWaitHandle wh = new AutoResetEvent(false);
    Queue<T> tasks = new Queue<T>();

    public void Push(T item)
    {
        lock (tasks)
            tasks.Enqueue(item);
        wh.Set();
    }

    public T Pop()
    {
        while (true)
        {
            lock (tasks)
            {
                if (tasks.Count > 0)
                    return tasks.Dequeue();
            }
            wh.WaitOne();
        }
    }
}
