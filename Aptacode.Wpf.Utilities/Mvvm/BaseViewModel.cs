using System;
using Aptacode.CSharp.Common.Utilities.Mvvm;

namespace Aptacode.Wpf.Utilities.Mvvm
{
    public abstract class BaseViewModel<TModel> : BindableBase, IEquatable<BaseViewModel<TModel>>
    {
        protected BaseViewModel() { }

        protected BaseViewModel(TModel model)
        {
            Model = model;
        }

        #region Methods

        public abstract void Present();

        #endregion

        #region Properties

        protected TModel _model;

        public TModel Model
        {
            get => _model;
            set
            {
                SetProperty(ref _model, value);
                Present();
            }
        }

        #endregion

        #region Equality

        public override int GetHashCode() => Model?.GetHashCode() ?? base.GetHashCode();

        public override bool Equals(object obj) => obj is BaseViewModel<TModel> other && Equals(other);

        public bool Equals(BaseViewModel<TModel> other) => other != null && GetHashCode() == other.GetHashCode();

        #endregion Equality
    }
}