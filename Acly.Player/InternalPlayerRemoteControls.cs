using System.ComponentModel;
using System.Windows.Input;
#if ANDROID
using Acly.Player.Android;
#endif

namespace Acly.Player
{
    internal partial class InternalPlayerRemoteControls : IPlayerRemoteControls
    {
        public InternalPlayerRemoteControls(IPlayer Player)
        {
            this.Player = Player;

#if ANDROID
            _IsEnabled = PlayerNotification.PlayersCount == 0
                || (PlayerNotification.PlayersCount == 1 && PlayerNotification.FirstAvailablePlayer == Player);
#else
            _IsEnabled = true;
#endif
        }
        ~InternalPlayerRemoteControls()
        {
            Dispose();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? SkippedToNext;
        public event EventHandler? SkippedToPrevious;
        public event EventHandler<bool>? CanSkipToNextChanged;
        public event EventHandler<bool>? CanSkipToPreviousChanged;

        public IPlayer Player { get; }
        public bool IsDisposed
        {
            get => _IsDisposed;
            private set
            {
                if (_IsDisposed != value)
                {
                    _IsDisposed = value;
                    InvokePropertyChanged(nameof(IsDisposed));
                }
            }
        }
        public bool IsEnabled
        {
            get => _IsEnabled;
            set
            {
                if (_IsEnabled != value)
                {
                    _IsEnabled = value;
                    InvokePropertyChanged(nameof(IsEnabled));
                }
            }
        }
        public ICommand? SkipToNextCommand
        {
            get => _SkipToNextCommand;
            set
            {
                if (_SkipToNextCommand != value)
                {
                    if (_SkipToNextCommand != null)
                    {
                        _SkipToNextCommand.CanExecuteChanged -= OnSkipToNextCommandCanExecuteChanged;
                    }
                    if (value != null)
                    {
                        value.CanExecuteChanged += OnSkipToNextCommandCanExecuteChanged;
                    }

                    _SkipToNextCommand = value;

                    InvokePropertyChanged(nameof(SkipToNextCommand));
                }
            }
        }
        public object? SkipToNextCommandParameter
        {
            get => _SkipToNextCommandParameter;
            set
            {
                if (_SkipToNextCommandParameter != value)
                {
                    _SkipToNextCommandParameter = value;
                    InvokePropertyChanged(nameof(SkipToNextCommandParameter));
                }
            }
        }
        public ICommand? SkipToPreviousCommand
        {
            get => _SkipToPreviousCommand;
            set
            {
                if (_SkipToPreviousCommand != value)
                {
                    if (_SkipToPreviousCommand != null)
                    {
                        _SkipToPreviousCommand.CanExecuteChanged -= OnSkipToPreviousCommandCanExecuteChanged;
                    }
                    if (value != null)
                    {
                        value.CanExecuteChanged += OnSkipToPreviousCommandCanExecuteChanged;
                    }

                    _SkipToPreviousCommand = value;

                    InvokePropertyChanged(nameof(SkipToPreviousCommand));
                }
            }
        }
        public object? SkipToPreviousCommandParameter
        {
            get => _SkipToPreviousCommandParameter;
            set
            {
                if (_SkipToPreviousCommandParameter != value)
                {
                    _SkipToPreviousCommandParameter = value;
                    InvokePropertyChanged(nameof(SkipToPreviousCommandParameter));
                }
            }
        }
        public bool CanSkipToNext
        {
            get => _CanSkipToNext;
            set
            {
                if (_CanSkipToNext != value)
                {
                    _CanSkipToNext = value;
                    UpdateSkipToNextStatus();
                    InvokePropertyChanged(nameof(CanSkipToNext));
                }
            }
        }
        public bool CanSkipToPrevious
        {
            get => _CanSkipToPrevious;
            set
            {
                if (_CanSkipToPrevious != value)
                {
                    _CanSkipToPrevious = value;
                    UpdateSkipToPreviousStatus();
                    InvokePropertyChanged(nameof(CanSkipToPrevious));
                }
            }
        }

        private bool _IsDisposed;
        private bool _IsEnabled;
        private ICommand? _SkipToNextCommand;
        private object? _SkipToNextCommandParameter;
        private ICommand? _SkipToPreviousCommand;
        private object? _SkipToPreviousCommandParameter;
        private bool _CanSkipToNext;
        private bool _CanSkipToPrevious;
        private bool _LastSkipToNextStatus;
        private bool _LastSkipToPreviousStatus;

        #region Управление

        public void SkipToNext()
        {
            if (!UpdateSkipToNextStatus())
            {
                return;
            }

            SkipToNextCommand?.Execute(SkipToNextCommandParameter);
            SkippedToNext?.Invoke(this, EventArgs.Empty);
        }
        public void SkipToPrevious()
        {
            if (!UpdateSkipToPreviousStatus())
            {
                return;
            }

            SkipToPreviousCommand?.Execute(SkipToPreviousCommandParameter);
            SkippedToPrevious?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            IsDisposed = true;
            SkipToNextCommand = null;
            SkipToPreviousCommand = null;
            SkipToNextCommandParameter = null;
            SkipToPreviousCommandParameter = null;

            GC.SuppressFinalize(this);
        }

        private bool UpdateSkipToNextStatus()
        {
            var Status = this.PeekCanSkipToNext();

            if (Status != _LastSkipToNextStatus)
            {
                CanSkipToNextChanged?.Invoke(this, Status);
            }

            _LastSkipToNextStatus = Status;

            return Status;
        }
        private bool UpdateSkipToPreviousStatus()
        {
            var Status = this.PeekCanSkipToPrevious();

            if (Status != _LastSkipToPreviousStatus)
            {
                CanSkipToPreviousChanged?.Invoke(this, Status);
            }

            _LastSkipToPreviousStatus = Status;

            return Status;
        }

        #endregion

        #region События

        private void InvokePropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new(PropertyName));
        }

        private void OnSkipToNextCommandCanExecuteChanged(object? sender, EventArgs e)
        {
            UpdateSkipToNextStatus();
        }
        private void OnSkipToPreviousCommandCanExecuteChanged(object? sender, EventArgs e)
        {
            UpdateSkipToPreviousStatus();
        }

        #endregion
    }
}
