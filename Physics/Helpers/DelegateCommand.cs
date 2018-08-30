using System;
using System.Windows.Input;

namespace Physics.Helpers
{
    public class DelegateCommand : ICommand
    {
        #region Fields
        private dynamic execute;
        private dynamic canExecute;
        private bool hasParamater;

        public event EventHandler CanExecuteChanged;
        #endregion Fields

        #region Constructors
        public DelegateCommand(Action execute)
        {
            hasParamater = false;
            this.execute = execute;
            this.canExecute = new Func<bool>(() => true);
        }
        public DelegateCommand(Action execute, Func<bool> canExecute)
        {
            hasParamater = false;
            this.execute = execute;
            this.canExecute = canExecute;
        }
        public DelegateCommand(Action<object> execute)
        {
            hasParamater = true;
            this.execute = execute;
            this.canExecute = new Func<bool>(() => true);
        }
        public DelegateCommand(Action<object> execute, Func<bool, object> canExecute)
        {
            hasParamater = true;
            this.execute = execute;
            this.canExecute = canExecute;
        }
        #endregion Constructors

        #region ICommand Members
        public void Execute(object parameter = null)
        {
            if (hasParamater)
                execute(parameter);
            else
                execute();
        }
        public bool CanExecute(object parameter = null)
        {
            bool output = false;

            if (hasParamater)
                output = canExecute(parameter);
            else
                output = canExecute();

            return output;
        }
        #endregion ICommand Members

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class DelegateCommand<T> : ICommand
    {
        #region Fields
        private dynamic execute;
        private dynamic canExecute;

        public event EventHandler CanExecuteChanged;
        #endregion Fields

        #region Constructors
        public DelegateCommand(Action<T> execute)
        {
            this.execute = execute;
            this.canExecute = new Func<T, bool>((x) => true);
        }
        public DelegateCommand(Action<T> execute, Func<T, bool> canExecute)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }
        #endregion Constructors

        #region ICommand Members
        public void Execute(object parameter)
        {
            execute((T)parameter);
        }
        public bool CanExecute(object parameter)
        {
            return canExecute((T)parameter);
        }
        #endregion ICommand Members

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
