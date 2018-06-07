﻿using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;
using Acr.Collections;
using Acr.UserDialogs;
using ReactiveUI;

namespace Hitcon2018.Wallet
{
    public class LogViewModel : ViewModel
    {
        readonly ILogService logs;
        IDisposable logWatch;


        public LogViewModel()
        {
            this.logs = LogService.Instance;
            this.Show = ReactiveCommand.Create<LogItem>(item => UserDialogs.Instance.Alert(item.Message));
            this.Clear = ReactiveCommand.Create(this.logs.Clear);
        }


        public override void OnActivated()
        {
            base.OnActivated();
            var l = this.logs.GetLogs();
            this.Logs.Clear();

            if (l.Any())
                this.Logs.AddRange(l);

            this.logWatch = this.logs
                .WhenUpdated()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x =>
                    this.Logs.Insert(0, x)
                );
        }


        public override void OnDeactivated()
        {
            base.OnDeactivated();
            this.logWatch?.Dispose();
        }


        public ObservableList<LogItem> Logs { get; } = new ObservableList<LogItem>();
        public ICommand Show { get; }
        public ICommand Clear { get; }
    }
}
