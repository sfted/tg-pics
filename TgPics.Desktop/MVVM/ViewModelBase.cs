using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TgPics.Desktop.MVVM
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected virtual bool SetProperty<T>(
            ref T storage,
            T value,
            [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;

            storage = value;
            NotifyPropertyChanged(propertyName);
            return true;
        }
    }
}
