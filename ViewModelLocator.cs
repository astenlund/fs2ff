// ReSharper disable MemberCanBeMadeStatic.Global

using Microsoft.Extensions.DependencyInjection;

namespace fs2ff
{
    public class ViewModelLocator
    {
        public MainViewModel? Main => App.Instance?.ServiceProvider?.GetRequiredService<MainViewModel>();
    }
}
