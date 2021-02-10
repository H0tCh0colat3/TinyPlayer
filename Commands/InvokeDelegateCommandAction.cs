using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace TinyPlayer.Commands
{
    /// <summary>
    /// From https://weblogs.asp.net/alexeyzakharov/silverlight-commands-hacks-passing-eventargs-as-commandparameter-to-delegatecommand-triggered-by-eventtrigger
    /// </summary>
    public sealed class InvokeDelegateCommandAction : TriggerAction<DependencyObject>
    {
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(InvokeDelegateCommandAction), null);

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command", typeof(ICommand), typeof(InvokeDelegateCommandAction), null);

        public static readonly DependencyProperty InvokeParameterProperty = DependencyProperty.Register(
            "InvokeParameter", typeof(object), typeof(InvokeDelegateCommandAction), null);

        private string commandName;

        public object InvokeParameter
        {
            get
            {
                return GetValue(InvokeParameterProperty);
            }
            set
            {
                SetValue(InvokeParameterProperty, value);
            }
        }

        public ICommand Command
        {
            get
            {
                return (ICommand)GetValue(CommandProperty);
            }
            set
            {
                SetValue(CommandProperty, value);
            }
        }

        public string CommandName
        {
            get
            {
                return commandName;
            }
            set
            {
                if (CommandName != value)
                {
                    commandName = value;
                }
            }
        }

        public object CommandParameter
        {
            get
            {
                return GetValue(CommandParameterProperty);
            }
            set
            {
                SetValue(CommandParameterProperty, value);
            }
        }

        protected override void Invoke(object parameter)
        {
            InvokeParameter = parameter;

            if (AssociatedObject != null)
            {
                ICommand command = ResolveCommand();
                if ((command != null) && command.CanExecute(CommandParameter))
                {
                    command.Execute(CommandParameter);
                }
            }
        }

        private ICommand ResolveCommand()
        {
            ICommand command = null;
            if (Command != null)
            {
                return Command;
            }
            if (AssociatedObject is FrameworkElement frameworkElement)
            {
                object dataContext = frameworkElement.DataContext;
                if (dataContext != null)
                {
                    PropertyInfo commandPropertyInfo = dataContext
                        .GetType()
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .FirstOrDefault(
                            p =>
                            typeof(ICommand).IsAssignableFrom(p.PropertyType) &&
                            string.Equals(p.Name, CommandName, StringComparison.Ordinal)
                        );

                    if (commandPropertyInfo != null)
                    {
                        command = (ICommand)commandPropertyInfo.GetValue(dataContext, null);
                    }
                }
            }
            return command;
        }
    }
}
