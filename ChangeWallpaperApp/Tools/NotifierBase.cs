﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ChangeWallpaperApp.Properties;

namespace ChangeWallpaperApp.Tools
{
    public abstract class NotifierBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void RaiseEvent<TArgs>(EventHandler<TArgs> handler, TArgs args)
        {
            handler?.Invoke(this, args);
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
        {
            if (Equals(storage, value)) return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
