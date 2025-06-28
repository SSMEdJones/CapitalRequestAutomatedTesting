using System.Runtime.Loader;

namespace CapitalRequestAutomatedTesting.UI.Helpers
{
    public class CustomAssemblyLoadContext : AssemblyLoadContext
    {
        public IntPtr LoadUnmanagedLibrary(string absolutePath) => LoadUnmanagedDll(absolutePath);

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
            => LoadUnmanagedDllFromPath(unmanagedDllName);
    }

}
