// ReSharper disable ClassWithVirtualMembersNeverInherited.Global

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using fs2ff.Annotations;

namespace fs2ff
{
    public class MainViewModel : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public void Dispose()
        {
            // TODO: Dispose resources
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
