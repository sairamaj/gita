using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Gita.Practice.App;

// Minimal base view model providing property change notification and a SetProperty helper.
public abstract class BaseViewModel : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler PropertyChanged;

	// Raises PropertyChanged for the given property name.
	protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	// Helper to set a backing field and raise PropertyChanged only when the value actually changes.
	protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
	{
		if (EqualityComparer<T>.Default.Equals(storage, value))
			return false;

		storage = value;
		OnPropertyChanged(propertyName);
		return true;
	}
}
