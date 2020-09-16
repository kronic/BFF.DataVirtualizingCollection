using DryIoc;

namespace BFF.DataVirtualizingCollection.Sample.Persistence.Proxy
{
    public class DryIocModule
    {
        public static void Load(IRegistrator container)
        {
            BFF.DataVirtualizingCollection.Sample.Persistence.DryIocModule.Load(container);
        }
    }
}