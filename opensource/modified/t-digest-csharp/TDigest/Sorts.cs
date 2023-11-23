using System;

namespace TDigest;

public class Sorts
{
    private static readonly Random Prng = new(); // for choosing pivots during quicksort

    /**
     * Quick sort using an index array.  On return,
     * values[order[i]] is in order as i goes 0..values.Length
     *
     * @param order  Indexes into values
     * @param values The values to sort.
     */
    public static void Sort(int[] order, double[] values)
    {
        Sort(order, values, 0, values.Length);
    }

    /**
     * Quick sort using an index array.  On return,
     * values[order[i]] is in order as i goes 0..n
     *
     * @param order  Indexes into values
     * @param values The values to sort.
     * @param n      The number of values to sort
     */
    public static void Sort(int[] order, double[] values, int n)
    {
        Sort(order, values, 0, n);
    }

    /**
     * Quick sort using an index array.  On return,
     * values[order[i]] is in order as i goes start..n
     *
     * @param order  Indexes into values
     * @param values The values to sort.
     * @param start  The first element to sort
     * @param n      The number of values to sort
     */
    public static void Sort(int[] order, double[] values, int start, int n)
    {
        for (var i = start; i < start + n; i++)
        {
            order[i] = i;
        }

        QuickSort(order, values, start, start + n, 64);
        InsertionSort(order, values, start, start + n, 64);
    }

    /**
     * Standard quick sort except that sorting is done on an index array rather than the values themselves
     *
     * @param order  The pre-allocated index array
     * @param values The values to sort
     * @param start  The beginning of the values to sort
     * @param end    The value after the last value to sort
     * @param limit  The minimum size to recurse down to.
     */
    private static void QuickSort(int[] order, double[] values, int start, int end, int limit)
    {
        // the while loop implements tail-recursion to avoid excessive stack calls on nasty cases
        while (end - start > limit)
        {
            // pivot by a random element
            var pivotIndex = start + Prng.Next(end - start);
            var pivotValue = values[order[pivotIndex]];

            // move pivot to beginning of array
            Swap(order, start, pivotIndex);

            // we use a three way partition because many duplicate values is an important case

            var low = start + 1; // low points to first value not known to be equal to pivotValue
            var high = end; // high points to first value > pivotValue
            var i = low; // i scans the array
            while (i < high)
            {
                // invariant:  values[order[k]] == pivotValue for k in [0..low)
                // invariant:  values[order[k]] < pivotValue for k in [low..i)
                // invariant:  values[order[k]] > pivotValue for k in [high..end)
                // in-loop:  i < high
                // in-loop:  low < high
                // in-loop:  i >= low
                var vi = values[order[i]];
                if (vi == pivotValue)
                {
                    if (low != i)
                    {
                        Swap(order, low, i);
                    }
                    else
                    {
                        i++;
                    }

                    low++;
                }
                else if (vi > pivotValue)
                {
                    high--;
                    Swap(order, i, high);
                }
                else
                {
                    // vi < pivotValue
                    i++;
                }
            }
            // invariant:  values[order[k]] == pivotValue for k in [0..low)
            // invariant:  values[order[k]] < pivotValue for k in [low..i)
            // invariant:  values[order[k]] > pivotValue for k in [high..end)
            // assert i == high || low == high therefore, we are done with partition

            // at this point, i==high, from [start,low) are == pivot, [low,high) are < and [high,end) are >
            // we have to move the values equal to the pivot into the middle.  To do this, we swap pivot
            // values into the top end of the [low,high) range stopping when we run out of destinations
            // or when we run out of values to copy
            var from = start;
            var to = high - 1;
            for (i = 0; from < low && to >= low; i++)
            {
                Swap(order, from++, to--);
            }

            if (from == low)
            {
                // ran out of things to copy.  This means that the the last destination is the boundary
                low = to + 1;
            }
            else
            {
                // ran out of places to copy to.  This means that there are uncopied pivots and the
                // boundary is at the beginning of those
                low = from;
            }

//            checkPartition(order, values, pivotValue, start, low, high, end);

            // now recurse, but arrange it so we handle the longer limit by tail recursion
            if (low - start < end - high)
            {
                QuickSort(order, values, start, low, limit);

                // this is really a way to do
                //    quickSort(order, values, high, end, limit);
                start = high;
            }
            else
            {
                QuickSort(order, values, high, end, limit);
                // this is really a way to do
                //    quickSort(order, values, start, low, limit);
                end = low;
            }
        }
    }

    /**
     * Quick sort in place of several paired arrays.  On return,
     * keys[...] is in order and the values[] arrays will be
     * reordered as well in the same way.
     *
     * @param key    Values to sort on
     * @param values The auxilliary values to sort.
     */
    public static void Sort(double[] key, params double[][] values)
    {
        Sort(key, 0, key.Length, values);
    }

    /**
     * Quick sort using an index array.  On return,
     * values[order[i]] is in order as i goes start..n
     * @param key    Values to sort on
     * @param start  The first element to sort
     * @param n      The number of values to sort
     * @param values The auxilliary values to sort.
     */
    public static void Sort(double[] key, int start, int n, params double[][] values)
    {
        QuickSort(key, values, start, start + n, 8);
        InsertionSort(key, values, start, start + n, 8);
    }

    /**
     * Standard quick sort except that sorting rearranges parallel arrays
     *
     * @param key    Values to sort on
     * @param values The auxilliary values to sort.
     * @param start  The beginning of the values to sort
     * @param end    The value after the last value to sort
     * @param limit  The minimum size to recurse down to.
     */
    private static void QuickSort(double[] key, double[][] values, int start, int end, int limit)
    {
        // the while loop implements tail-recursion to avoid excessive stack calls on nasty cases
        while (end - start > limit)
        {
            // median of three values for the pivot
            var a = start;
            var b = (start + end) / 2;
            var c = end - 1;

            int pivotIndex;
            double pivotValue;
            var va = key[a];
            var vb = key[b];
            var vc = key[c];
            //noinspection Duplicates
            if (va > vb)
            {
                if (vc > va)
                {
                    // vc > va > vb
                    pivotIndex = a;
                    pivotValue = va;
                }
                else
                {
                    // va > vb, va >= vc
                    if (vc < vb)
                    {
                        // va > vb > vc
                        pivotIndex = b;
                        pivotValue = vb;
                    }
                    else
                    {
                        // va >= vc >= vb
                        pivotIndex = c;
                        pivotValue = vc;
                    }
                }
            }
            else
            {
                // vb >= va
                if (vc > vb)
                {
                    // vc > vb >= va
                    pivotIndex = b;
                    pivotValue = vb;
                }
                else
                {
                    // vb >= va, vb >= vc
                    if (vc < va)
                    {
                        // vb >= va > vc
                        pivotIndex = a;
                        pivotValue = va;
                    }
                    else
                    {
                        // vb >= vc >= va
                        pivotIndex = c;
                        pivotValue = vc;
                    }
                }
            }

            // move pivot to beginning of array
            Swap(start, pivotIndex, key, values);

            // we use a three way partition because many duplicate values is an important case

            var low = start + 1; // low points to first value not known to be equal to pivotValue
            var high = end; // high points to first value > pivotValue
            var i = low; // i scans the array
            while (i < high)
            {
                // invariant:  values[order[k]] == pivotValue for k in [0..low)
                // invariant:  values[order[k]] < pivotValue for k in [low..i)
                // invariant:  values[order[k]] > pivotValue for k in [high..end)
                // in-loop:  i < high
                // in-loop:  low < high
                // in-loop:  i >= low
                var vi = key[i];
                if (vi == pivotValue)
                {
                    if (low != i)
                    {
                        Swap(low, i, key, values);
                    }
                    else
                    {
                        i++;
                    }

                    low++;
                }
                else if (vi > pivotValue)
                {
                    high--;
                    Swap(i, high, key, values);
                }
                else
                {
                    // vi < pivotValue
                    i++;
                }
            }
            // invariant:  values[order[k]] == pivotValue for k in [0..low)
            // invariant:  values[order[k]] < pivotValue for k in [low..i)
            // invariant:  values[order[k]] > pivotValue for k in [high..end)
            // assert i == high || low == high therefore, we are done with partition

            // at this point, i==high, from [start,low) are == pivot, [low,high) are < and [high,end) are >
            // we have to move the values equal to the pivot into the middle.  To do this, we swap pivot
            // values into the top end of the [low,high) range stopping when we run out of destinations
            // or when we run out of values to copy
            var from = start;
            var to = high - 1;
            for (i = 0; from < low && to >= low; i++)
            {
                Swap(from++, to--, key, values);
            }

            if (from == low)
            {
                // ran out of things to copy.  This means that the the last destination is the boundary
                low = to + 1;
            }
            else
            {
                // ran out of places to copy to.  This means that there are uncopied pivots and the
                // boundary is at the beginning of those
                low = from;
            }

//            checkPartition(order, values, pivotValue, start, low, high, end);

            // now recurse, but arrange it so we handle the longer limit by tail recursion
            if (low - start < end - high)
            {
                QuickSort(key, values, start, low, limit);

                // this is really a way to do
                //    quickSort(order, values, high, end, limit);
                start = high;
            }
            else
            {
                QuickSort(key, values, high, end, limit);
                // this is really a way to do
                //    quickSort(order, values, start, low, limit);
                end = low;
            }
        }
    }


    /**
     * Limited range insertion sort.  We assume that no element has to move more than limit steps
     * because quick sort has done its thing. This version works on parallel arrays of keys and values.
     *
     * @param key    The array of keys
     * @param values The values we are sorting
     * @param start  The starting point of the sort
     * @param end    The ending point of the sort
     * @param limit  The largest amount of disorder
     */
    private static void InsertionSort(double[] key, double[][] values, int start, int end, int limit)
    {
        // loop invariant: all values start ... i-1 are ordered
        for (var i = start + 1; i < end; i++)
        {
            var v = key[i];
            var m = Math.Max(i - limit, start);
            for (var j = i; j >= m; j--)
            {
                if (j == m || key[j - 1] <= v)
                {
                    if (j < i)
                    {
                        Array.Copy(key, j, key, j + 1, i - j);
                        key[j] = v;
                        foreach (var value in values)
                        {
                            var tmp = value[i];
                            Array.Copy(value, j, value, j + 1, i - j);
                            value[j] = tmp;
                        }
                    }

                    break;
                }
            }
        }
    }

    private static void Swap(int[] order, int i, int j)
    {
        var t = order[i];
        order[i] = order[j];
        order[j] = t;
    }

    private static void Swap(int i, int j, double[] key, params double[][] values)
    {
        var t = key[i];
        key[i] = key[j];
        key[j] = t;

        for (var k = 0; k < values.Length; k++)
        {
            t = values[k][i];
            values[k][i] = values[k][j];
            values[k][j] = t;
        }
    }

    /**
     * Check that a partition step was done correctly.  For debugging and testing.
     *
     * @param order      The array of indexes representing a permutation of the keys.
     * @param values     The keys to sort.
     * @param pivotValue The value that splits the data
     * @param start      The beginning of the data of interest.
     * @param low        Values from start (inclusive) to low (exclusive) are &lt; pivotValue.
     * @param high       Values from low to high are equal to the pivot.
     * @param end        Values from high to end are above the pivot.
     */
    public static void CheckPartition(int[] order, double[] values, double pivotValue, int start, int low, int high,
        int end)
    {
        if (order.Length != values.Length)
        {
            throw new ArgumentException("Arguments must be same size");
        }

        if (!(start >= 0 && low >= start && high >= low && end >= high))
        {
            throw new ArgumentException($"Invalid indices {start}, {low}, {high}, {end}");
        }

        for (var i = 0; i < low; i++)
        {
            var v = values[order[i]];
            if (v >= pivotValue)
            {
                throw new ArgumentException($"Value greater than pivot at {i}");
            }
        }

        for (var i = low; i < high; i++)
        {
            if (values[order[i]] != pivotValue)
            {
                throw new ArgumentException($"Non-pivot at {i}");
            }
        }

        for (var i = high; i < end; i++)
        {
            var v = values[order[i]];
            if (v <= pivotValue)
            {
                throw new ArgumentException($"Value less than pivot at {i}");
            }
        }
    }

    /**
     * Limited range insertion sort.  We assume that no element has to move more than limit steps
     * because quick sort has done its thing.
     *
     * @param order  The permutation index
     * @param values The values we are sorting
     * @param start  Where to start the sort
     * @param n      How many elements to sort
     * @param limit  The largest amount of disorder
     */
    private static void InsertionSort(int[] order, double[] values, int start, int n, int limit)
    {
        for (var i = start + 1; i < n; i++)
        {
            var t = order[i];
            var v = values[order[i]];
            var m = Math.Max(i - limit, start);
            for (var j = i; j >= m; j--)
            {
                if (j == 0 || values[order[j - 1]] <= v)
                {
                    if (j < i)
                    {
                        Array.Copy(order, j, order, j + 1, i - j);
                        order[j] = t;
                    }

                    break;
                }
            }
        }
    }

    /**
     * Reverses an array in-place.
     *
     * @param order The array to reverse
     */
    public static void Reverse(int[] order)
    {
        Reverse(order, 0, order.Length);
    }

    /**
     * Reverses part of an array. See {@link #reverse(int[])}
     *
     * @param order  The array containing the data to reverse.
     * @param offset Where to start reversing.
     * @param length How many elements to reverse
     */
    public static void Reverse(int[] order, int offset, int length)
    {
        for (var i = 0; i < length / 2; i++)
        {
            var t = order[offset + i];
            order[offset + i] = order[offset + length - i - 1];
            order[offset + length - i - 1] = t;
        }
    }

    /**
     * Reverses part of an array. See {@link #reverse(int[])}
     *
     * @param order  The array containing the data to reverse.
     * @param offset Where to start reversing.
     * @param length How many elements to reverse
     */
    public static void Reverse(double[] order, int offset, int length)
    {
        for (var i = 0; i < length / 2; i++)
        {
            var t = order[offset + i];
            order[offset + i] = order[offset + length - i - 1];
            order[offset + length - i - 1] = t;
        }
    }
}