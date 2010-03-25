using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace ThreeByte.TaskModel
{
    public interface IAsyncTask : INotifyPropertyChanged
    {
        //Properties
        string Name { get; }
        string Status { get; }
        int PercentComplete { get; }

        bool HasError { get; }
        string Error { get; }

        //Events
        event EventHandler Completed;

        //Methods
        bool Setup();
        void Go();
        void Cancel();
    }
}
