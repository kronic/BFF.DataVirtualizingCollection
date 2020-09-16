using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reflection;
using BFF.DataVirtualizingCollection.Sample.View.ViewModelInterfaceImplementations;
using BFF.DataVirtualizingCollection.Sample.View.Views;
using DryIoc;
using MrMeeseeks.Extensions;

namespace BFF.DataVirtualizingCollection.Sample.View
{
    public class DryIocModule
    {
        private static IResolverContext? _scope;
        
        public static MainWindowView Start()
        {
            var builder = new Container(Rules.Default.WithTrackingDisposableTransients());
            Load(builder);

            _scope = builder.OpenScope();

            return _scope.Resolve<MainWindowView>();
        }

        public static void Finish() => 
            _scope?.Dispose();

        private static void Load(IContainer container)
        {

            container.RegisterMany(Assembly
                .GetExecutingAssembly()
                .ToEnumerable()
                .SelectMany(a => a.GetTypes())
                .Where(t =>
                    (t.Namespace?.StartsWith($"{nameof(BFF)}.{nameof(DataVirtualizingCollection)}") ?? false)
                    && t.IsAbstract.Not()
                    && t.IsStatic().Not()));
            
            container.Register<GetSchedulers>(Reuse.Singleton, serviceKey: DefaultKey.Value);
            
            container.Register<CompositeDisposable>(Reuse.Scoped, Made.Of(() => new CompositeDisposable()));

            ViewModel.DryIocModule.Load(container);
            
            Persistence.Proxy.DryIocModule.Load(container);
        }
    }
}