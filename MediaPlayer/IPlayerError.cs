using System;
using System.ComponentModel;

namespace MediaPlayer
{
    public interface IPlayerError : INotifyPropertyChanged
    {
        /// <summary>Gets the error exception.</summary>
        /// <returns>The error exception on error; null if no error.</returns>
        Exception Exception { get; set; }

        /// <summary>Gets a value that indicating whether this instance has an exception.</summary>
        /// <returns>true if this instance has an exception; otherwise, false.</returns>
        bool HasException { get; }

        /// <summary>Gets the current error message.</summary>
        /// <returns>The current error message or String.Empty if no error</returns>
        string Message { get; }
    }
}
