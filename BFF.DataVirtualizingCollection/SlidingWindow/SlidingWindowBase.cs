﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;

namespace BFF.DataVirtualizingCollection.SlidingWindow
{
    internal abstract class SlidingWindowBase<T> : VirtualizationBase<T>, ISlidingWindow<T>
    {
        private readonly IScheduler _observeScheduler;

        protected int Size;

        protected int CountOfBackedDataSet;
        internal SlidingWindowBase(
            IScheduler observeScheduler)
        {
            Size = 0;
            Offset = 0;
            CountOfBackedDataSet = 0;
            _observeScheduler = observeScheduler;
        }
        
        public int Offset { get; protected set; }

        public int MaximumOffset => CountOfBackedDataSet - Size;
        
        public override int SelectedIndex { get; set; }

        public void SlideLeft()
        {
            if (Offset <= 0) return;
            var prev = this.Select(x => x).ToArray();
            Offset--;
            _observeScheduler.Schedule(Unit.Default, (_, __) =>
            {
                OnCollectionChangedReplace(this.Select(x => x).ToArray(), prev);
                OnPropertyChanged(nameof(Offset));
                OnIndexerChanged();
            });
        }

        public void SlideRight()
        {
            if (MaximumOffset <= Offset) return;
            var prev = this.Select(x => x).ToArray();
            Offset++;
            _observeScheduler.Schedule(Unit.Default, (_, __) =>
            {
                OnCollectionChangedReplace(this.Select(x => x).ToArray(), prev);
                OnPropertyChanged(nameof(Offset));
                OnIndexerChanged();
            });
        }

        public void JumpTo(int index)
        {
            var prev = this.Select(x => x).ToArray();
            Offset = Math.Max(0, Math.Min(CountOfBackedDataSet - Size, index));
            _observeScheduler.Schedule(Unit.Default, (_, __) =>
            {
                OnCollectionChangedReplace(this.Select(x => x).ToArray(), prev);
                OnPropertyChanged(nameof(Offset));
                OnIndexerChanged();
            });
        }

        public void IncreaseWindowSize()
        {
            IncreaseWindowSizeBy(1);
        }

        public void DecreaseWindowSize()
        {
            DecreaseWindowSizeBy(1);
        }

        public void IncreaseWindowSizeBy(int sizeIncrement)
        {
            var prev = this.Select(x => x).ToArray();
            sizeIncrement = Math.Max(0, sizeIncrement);
            Size = Math.Min(CountOfBackedDataSet, Size + sizeIncrement);
            if (Offset > MaximumOffset)
                Offset = MaximumOffset;
            _observeScheduler.Schedule(Unit.Default, (_, __) =>
            {
                OnCollectionChangedReplace(this.Select(x => x).ToArray(), prev);
                OnPropertyChanged(nameof(Offset));
                OnPropertyChanged(nameof(MaximumOffset));
                OnPropertyChanged(nameof(Count));
                OnIndexerChanged();
            });
        }

        public void DecreaseWindowSizeBy(int sizeIncrement)
        {
            var prev = this.Select(x => x).ToArray();
            sizeIncrement = Math.Max(0, sizeIncrement);
            Size = Math.Max(0, Size - sizeIncrement);
            _observeScheduler.Schedule(Unit.Default, (_, __) =>
            {
                OnCollectionChangedReplace(this.Select(x => x).ToArray(), prev);
                OnPropertyChanged(nameof(Offset));
                OnPropertyChanged(nameof(MaximumOffset));
                OnPropertyChanged(nameof(Count));
                OnIndexerChanged();
            });
        }

        public override IEnumerator<T> GetEnumerator()
        {
            return Enumeration().GetEnumerator();

            IEnumerable<T> Enumeration()
            {
                for (var i = 0; i < Count; i++)
                {
                    yield return this[i];
                }
            }
        }

        protected override T IndexerInnerGet(int index) =>
            index >= Size || Offset + index >= CountOfBackedDataSet || index < 0
                ? throw new IndexOutOfRangeException("Index was out of range. Must be non-negative and less than the size of the collection.")
                : GetItemInner(Offset + index);

        protected abstract T GetItemInner(int index);
        
        protected void OnCollectionChangedReplace(T[] newItems, T[] oldItems)
        {
            var commonBound = Math.Min(newItems.Length, oldItems.Length);
            var i = 0;
            for (; i < commonBound; i++)
            {
                OnCollectionChangedReplace(newItems[i], oldItems[0], i);
            }
            for (; i < newItems.Length; i++)
            {
                OnCollectionChangedAdd(newItems[i], i);
            }
            for (; i < oldItems.Length; i++)
            {
                OnCollectionChangedRemove(oldItems[i], i);
            }
        }
    }
}