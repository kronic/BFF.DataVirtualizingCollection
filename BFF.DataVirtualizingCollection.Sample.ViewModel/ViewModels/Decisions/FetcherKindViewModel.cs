using System.Threading;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.Sample.Model.BackendAccesses;

namespace BFF.DataVirtualizingCollection.Sample.ViewModel.ViewModels.Decisions
{
    public enum FetcherKind
    {
        NonTaskBased,
        TaskBased
    }

    public interface IFetcherKindViewModelInternal : IFetcherKindViewModel
    {
        IIndexAccessBehaviorViewModel IndexAccessBehaviorViewModel { get; }
    }

    public interface IFetcherKindViewModel
    {
        FetcherKind FetcherKind { get; set; }
        
        public int DelayPageFetcherInMilliseconds { get; set; }
        
        public int DelayCountFetcherInMilliseconds { get; set; }
        
        IAsyncOnlyIndexAccessBehaviorCollectionBuilder<T, TVirtualizationKind> Configure<T, TVirtualizationKind>(
            IFetchersKindCollectionBuilder<T, TVirtualizationKind> builder, 
            IBackendAccess<T> backendAccess)
        {
            return FetcherKind == FetcherKind.NonTaskBased
                ? builder.NonTaskBasedFetchers(
                    (offset, size) =>
                    {
                        Thread.Sleep(DelayPageFetcherInMilliseconds);
                        return backendAccess.PageFetch(offset, size);
                    },
                    () =>
                    {
                        Thread.Sleep(DelayCountFetcherInMilliseconds);
                        return backendAccess.CountFetch();
                    })
                : builder.TaskBasedFetchers(
                    async (offset, size) =>
                    {
                        await Task.Delay(DelayPageFetcherInMilliseconds);
                        return backendAccess.PageFetch(offset, size);
                    },
                    async () =>
                    {
                        await Task.Delay(DelayCountFetcherInMilliseconds);
                        return backendAccess.CountFetch();
                    });
        }
    }

    internal class FetcherKindViewModel : ObservableObject, IFetcherKindViewModelInternal
    {
        private readonly IIndexAccessBehaviorViewModelInternal _indexAccessBehaviorViewModelInternal;
        private FetcherKind _fetcherKind = FetcherKind.TaskBased;
        private int _delayPageFetcherInMilliseconds = 2000;
        private int _delayCountFetcherInMilliseconds = 2000;

        public FetcherKindViewModel(
            IIndexAccessBehaviorViewModelInternal indexAccessBehaviorViewModelInternal)
        {
            _indexAccessBehaviorViewModelInternal = indexAccessBehaviorViewModelInternal;
        }

        public FetcherKind FetcherKind
        {
            get => _fetcherKind;
            set
            {
                if (_fetcherKind == value) return;
                _fetcherKind = value;
                OnPropertyChanged();

                _indexAccessBehaviorViewModelInternal.SetIsSyncEnabled(_fetcherKind == FetcherKind.NonTaskBased);
            }
        }

        public int DelayPageFetcherInMilliseconds
        {
            get => _delayPageFetcherInMilliseconds;
            set
            {
                if (_delayPageFetcherInMilliseconds == value) return;
                _delayPageFetcherInMilliseconds = value;
                OnPropertyChanged();
            }
        }

        public int DelayCountFetcherInMilliseconds
        {
            get => _delayCountFetcherInMilliseconds;
            set
            {
                if (_delayCountFetcherInMilliseconds == value) return;
                _delayCountFetcherInMilliseconds = value;
                OnPropertyChanged();
            }
        }

        public IIndexAccessBehaviorViewModel IndexAccessBehaviorViewModel => _indexAccessBehaviorViewModelInternal;
    }
}