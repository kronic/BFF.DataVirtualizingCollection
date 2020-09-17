using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reflection;
using Autofac;
using BFF.DataVirtualizingCollection.Sample.View.ViewModelInterfaceImplementations;
using BFF.DataVirtualizingCollection.Sample.View.Views;
using Module = Autofac.Module;

namespace BFF.DataVirtualizingCollection.Sample.CompositionRoot
{
    public class AutofacModule : Module
    {
        public static MainWindowView Start()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new AutofacModule());
            return builder
                .Build()
                .BeginLifetimeScope()
                .Resolve<MainWindowView>();
        }
        
        private AutofacModule()
        {}
        
        protected override void Load(ContainerBuilder builder)
        {
            var assemblies = GetSolutionAssemblies().ToArray();
                    
            builder.RegisterAssemblyTypes(assemblies)
                .AsImplementedInterfaces()
                .AsSelf();

            builder.RegisterType<GetSchedulers>()
                .AsImplementedInterfaces()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<CompositeDisposable>()
                .AsSelf()
                .UsingConstructor(() => new CompositeDisposable())
                .InstancePerLifetimeScope();
            
            static IEnumerable<Assembly> GetSolutionAssemblies()
            {
                yield return Assembly.Load(typeof(View.Type).Assembly.FullName ?? "");
                yield return Assembly.Load(typeof(ViewModel.Type).Assembly.FullName ?? "");
                yield return Assembly.Load(typeof(Model.Type).Assembly.FullName ?? "");
                yield return Assembly.Load(typeof(Persistence.Type).Assembly.FullName ?? "");
            }
        }
    }
}