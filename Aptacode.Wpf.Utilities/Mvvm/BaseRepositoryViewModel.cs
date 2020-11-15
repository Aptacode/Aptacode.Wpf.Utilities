using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Aptacode.CSharp.Common.Persistence;
using Aptacode.CSharp.Common.Persistence.Repository;
using Aptacode.CSharp.Common.Utilities.Extensions;
using Aptacode.CSharp.Common.Utilities.Mvvm;

namespace Aptacode.Wpf.Utilities.Mvvm
{
    public abstract class BaseRepositoryViewModel<TKey, TEntity> : BindableBase where TEntity : IEntity<TKey>
    {
        protected readonly IGenericAsyncRepository<TKey, TEntity> Repository;

        protected BaseRepositoryViewModel(IGenericAsyncRepository<TKey, TEntity> repository)
        {
            Repository = repository ?? throw new NullReferenceException("Repository was null");

            new TaskFactory().StartNew(async () => await Load().ConfigureAwait(false));
        }

        public event EventHandler<BaseViewModel<TEntity>> SelectedItemChanged;

        #region Methods

        public void Clear()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                SelectedItem = null;
                Items.Clear();
            });
        }

        public async Task Load()
        {
            var models = await GetModels().ConfigureAwait(false);

            Application.Current.Dispatcher.Invoke(() =>
            {
                Items.Clear();
                Items.AddRange(models.Select(CreateViewModel));
            });
        }

        protected virtual async Task<IEnumerable<TEntity>> GetModels() =>
            await Repository.GetAllAsync().ConfigureAwait(false);

        public abstract BaseViewModel<TEntity> CreateViewModel(TEntity model);
        public abstract TEntity CreateNew();

        public async Task Update(BaseViewModel<TEntity> viewModel)
        {
            if (viewModel.Model == null)
            {
                return;
            }

            await Repository.UpdateAsync(viewModel.Model).ConfigureAwait(false);
        }

        public async Task Delete(BaseViewModel<TEntity> viewModel)
        {
            if (viewModel.Model == null)
            {
                return;
            }

            await Repository.DeleteAsync(viewModel.Model.Id).ConfigureAwait(false);
        }

        #endregion

        #region Properties

        public ObservableCollection<BaseViewModel<TEntity>> Items { get; set; } =
            new ObservableCollection<BaseViewModel<TEntity>>();

        private BaseViewModel<TEntity> _selectedItem;

        public BaseViewModel<TEntity> SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                SelectedItemChanged?.Invoke(this, _selectedItem);
            }
        }

        #endregion

        #region Commands

        private DelegateCommand _createCommand;

        public DelegateCommand CreateCommand =>
            _createCommand ??= new DelegateCommand(async (_) =>
            {
                await Repository.CreateAsync(CreateNew()).ConfigureAwait(false);
                await Load().ConfigureAwait(false);
            });

        private DelegateCommand _loadCommand;

        public DelegateCommand LoadCommand =>
            _loadCommand ??= new DelegateCommand(async (_) => await Load().ConfigureAwait(false));

        private DelegateCommand _clearCommand;

        public DelegateCommand ClearCommand =>
            _clearCommand ??= new DelegateCommand((_) => Clear());

        private DelegateCommand<BaseViewModel<TEntity>> _updateCommand;

        public DelegateCommand<BaseViewModel<TEntity>> UpdateCommand =>
            _updateCommand ??= new DelegateCommand<BaseViewModel<TEntity>>(async viewModel =>
                await Update(viewModel).ConfigureAwait(false));

        private DelegateCommand<BaseViewModel<TEntity>> _deleteCommand;

        public DelegateCommand<BaseViewModel<TEntity>> DeleteCommand =>
            _deleteCommand ??= new DelegateCommand<BaseViewModel<TEntity>>(async viewModel =>
                await Delete(viewModel).ConfigureAwait(false));

        #endregion
    }
}