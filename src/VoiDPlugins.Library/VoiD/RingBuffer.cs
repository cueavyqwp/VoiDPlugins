using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VoiDPlugins.Library
{
    public class RingBuffer<T> : IEnumerable<T>
    {
        public int Size { private set; get; }
        public bool IsFilled { private set; get; }

        private T[] dataStream;
        private int head;

        public RingBuffer(int size)
        {
            Size = size;
            dataStream = new T[size];
        }

        public void Insert(T item)
        {
            dataStream[head++] = item;
            if (head == Size)
            {
                head = 0;
                IsFilled = true;
            }
        }

        public void Clear()
        {
            dataStream = new T[Size];
            head = 0;
            IsFilled = false;
        }

        private int Wrap(int index)
        {
            return (index + Size) % Size;
        }

        IEnumerator<T> RingGetEnumerator()
        {
            if (head == 0 || !IsFilled)
            {
                foreach (var item in dataStream)
                {
                    yield return item;
                }
            }
            else
            {
                foreach (var item in dataStream[head..^0])
                {
                    yield return item;
                }
                foreach (var item in dataStream[0..head])
                {
                    yield return item;
                }
            }
        }

        public T this[int index]
        {
            get => dataStream[Wrap(index + head)];
            set => dataStream[Wrap(index + head)] = value;
        }

        public T this[Index index]
        {
            get => dataStream[Wrap(index.IsFromEnd ? Wrap(head - index.Value) : Wrap(index.Value + head))];
            set => dataStream[Wrap(index.IsFromEnd ? Wrap(head - index.Value) : Wrap(index.Value + head))] = value;
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (!IsFilled)
                return ((IEnumerable<T>)dataStream).GetEnumerator();
            else
                return RingGetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            string a = "";
            foreach (var item in this.SkipLast(1))
                a += $"{item}, ";

            a += this[^1];
            return a;
        }
    }
}