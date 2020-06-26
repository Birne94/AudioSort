using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Audiosort
{
    public class TitleItemModel
    {
        public TitleItemModel()
        {
            EditCommand = new SimpleCommand
            {
                ExecuteDelegate = _ => MessageBox.Show("Hi"),
                CanExecuteDelegate = _ => true
            };
        }

        public ICommand EditCommand;
    }

    #region SimpleCommand
    public class SimpleCommand : ICommand
    {
        public Predicate<object> CanExecuteDelegate { get; set; }
        public Action<object> ExecuteDelegate { get; set; }

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            if (CanExecuteDelegate != null)
                return CanExecuteDelegate(parameter);
            return true;// if there is no can execute default to true
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            if (ExecuteDelegate != null)
                ExecuteDelegate(parameter);
        }

        #endregion
    }
    #endregion
}
