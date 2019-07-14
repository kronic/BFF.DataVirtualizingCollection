﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Xunit;

namespace BFF.DataVirtualizingCollection.Test.PageRemoval
{
    public class AsyncHoardingTests
    {
        public static IEnumerable<object[]> Combinations =>
            new List<object[]>
            {
                new object[] { PageLoadingBehavior.NonPreloading, PageRemovalBehavior.Hoarding, FetchersKind.NonTaskBased, IndexAccessBehavior.Asynchronous },
                new object[] { PageLoadingBehavior.NonPreloading, PageRemovalBehavior.Hoarding, FetchersKind.TaskBased, IndexAccessBehavior.Asynchronous },
                new object[] { PageLoadingBehavior.Preloading, PageRemovalBehavior.Hoarding, FetchersKind.NonTaskBased, IndexAccessBehavior.Asynchronous },
                new object[] { PageLoadingBehavior.Preloading, PageRemovalBehavior.Hoarding, FetchersKind.TaskBased, IndexAccessBehavior.Asynchronous }
            };

        [Theory]
        [MemberData(nameof(Combinations))]
        public async Task With6969ElementsAndPageSize1000_GetAnElementFromEachPage_NoneDisposed(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            var set = new HashSet<int>();
            using var collection = Factory.CreateCollectionWithCustomPageFetchingLogic(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                6969,
                100,
                (offset, pSize) =>
                    Enumerable
                        .Range(offset, pSize)
                        .Select(i => Disposable.Create(() => set.Add(i)))
                        .ToArray(),
                Disposable.Empty);

            // Act
            for (var i = 0; i <= 6900; i += 100)
            {
                var _ = ((IList<IDisposable>)collection)[i];
            }
            await Task.Delay(50);

            // Assert
            Assert.Empty(set);
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public async Task With6969ElementsAndPageSize1000_GetAnElementFromEachPageDisposeCollection_AllDisposed(
            PageLoadingBehavior pageLoadingBehavior,
            PageRemovalBehavior pageRemovalBehavior,
            FetchersKind fetchersKind,
            IndexAccessBehavior indexAccessBehavior)
        {
            // Arrange
            const int expected = 69696;
            var set = new HashSet<int>();
            var collection = Factory.CreateCollectionWithCustomPageFetchingLogic(
                pageLoadingBehavior,
                pageRemovalBehavior,
                fetchersKind,
                indexAccessBehavior,
                expected,
                100,
                (offset, pSize) =>
                    Enumerable
                        .Range(offset, pSize)
                        .Select(i => Disposable.Create(() => set.Add(i)))
                        .ToArray(),
                Disposable.Empty);

            // Act
            for (var i = 0; i <= expected; i += 100)
            {
                var _ = ((IList<IDisposable>)collection)[i];
                await Task.Delay(50);
            }
            collection.Dispose();
            await Task.Delay(5000);

            // Assert
            Assert.Equal(expected, set.Count);
        }
    }
}