using Acr.XamForms;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hitcon2018.Wallet
{
    public abstract class ViewModel : ReactiveObject, IViewModelLifecycle
    {
        public virtual void OnActivated() { }
        public virtual void OnDeactivated() { }
        public virtual void OnOrientationChanged(bool isPortrait) { }
        public virtual bool OnBackRequested() => true;
        public virtual void OnDestroy() { }
    }
}
