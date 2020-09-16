using System.Linq;
using System.Reflection;
using BFF.DataVirtualizingCollection.Sample.ViewModel.Adapters;
using DryIoc;
using MrMeeseeks.Extensions;

namespace BFF.DataVirtualizingCollection.Sample.ViewModel
{
    public class DryIocModule
    {
        public static void Load(IContainer container)
        {
            var types = Assembly
                .GetExecutingAssembly()
                .ToEnumerable()
                .SelectMany(a => a.GetTypes())
                .Where(t =>
                    (t.Namespace?.StartsWith($"{nameof(BFF)}.{nameof(DataVirtualizingCollection)}") ?? false)
                    && t.IsAbstract.Not()
                    && t.IsStatic().Not()
                    && t.GetSingleConstructorOrNull().IsNotNull())
                .ToReadOnlyList();

            container.RegisterMany(types, nonPublicServiceTypes: true);
            
            Model.DryIocModule.Load(container);
        }
    }
}