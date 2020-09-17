using BFF.DataVirtualizingCollection.Sample.CompositionRoot;

namespace BFF.DataVirtualizingCollection.Sample.Root
{
    public partial class App
    {
        public App() => 
            AutofacModule.Start().Show();
    }
}