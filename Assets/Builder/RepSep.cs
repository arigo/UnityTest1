using System.Collections;
using System.Collections.Generic;


public class RepSep<T>
{
    Dictionary<T, T> reps;

    public RepSep()
    {
        reps = new Dictionary<T, T>();
    }

    public void Merge(T item1, T item2)
    {
        item1 = GetRep(item1);
        reps[item1] = GetRep(item2);
    }

    public T GetRep(T item)
    {
        if (!reps.ContainsKey(item))
        {
            reps[item] = item;
            return item;
        }

        var cur = item;
        while (true)
        {
            var next = reps[cur];
            if (next.Equals(cur))
                break;
            cur = next;
        }
        return cur;
    }
}
