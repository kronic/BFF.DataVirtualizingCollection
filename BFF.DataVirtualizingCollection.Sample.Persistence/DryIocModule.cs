using System.Linq;
using System.Reflection;
using DryIoc;
using MrMeeseeks.Extensions;

namespace BFF.DataVirtualizingCollection.Sample.Persistence
{
    public class DryIocModule
    {
        public static void Load(IRegistrator container)
        {
            container.RegisterMany(Assembly
                .GetExecutingAssembly()
                .ToEnumerable()
                .SelectMany(a => a.GetTypes())
                .Where(t =>
                    (t.Namespace?.StartsWith($"{nameof(BFF)}.{nameof(DataVirtualizingCollection)}") ?? false)
                    && t.IsAbstract.Not()
                    && t.IsStatic().Not()));
        }
    }
}