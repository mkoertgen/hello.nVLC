using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Hello.nVLC.Media
{
    public class PlayerError : IPlayerError
    {
        public static readonly PlayerError NoError = new PlayerError();
        private Exception _exception;

        public PlayerError(Exception exception = null)
        {
            Exception = exception;
        }

        public Exception Exception
        {
            get { return _exception; }
            set
            {
                if (_exception == value) return;
                _exception = value;
                OnPropertyChanged();
                // ReSharper disable ExplicitCallerInfoArgument
                OnPropertyChanged("HasException");
                OnPropertyChanged("Message");
                // ReSharper restore ExplicitCallerInfoArgument
            }
        }

        public bool HasException { get { return _exception != null; } }
        public string Message { get { return _exception != null ? _exception.Message : String.Empty; } }
        
        public event PropertyChangedEventHandler PropertyChanged = delegate {};

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}