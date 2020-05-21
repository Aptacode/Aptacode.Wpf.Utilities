using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Aptacode.CSharp.Utilities.Persistence;
using Aptacode.CSharp.Utilities.Persistence.Repository;
using Prism.Commands;
using Prism.Mvvm;

namespace Aptacode.Wpf.Utilities.Mvvm
{
    public abstract class BaseRepositoryViewModel<TEntity> : BindableBase where TEntity : EntityBase
    {
        protected readonly IRepository<TEntity> Repository;

        protected BaseRepositoryViewModel(IRepository<TEntity> repository)
        {
            Repository = repository;
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

        protected virtual async Task<IEnumerable<TEntity>> GetModels()
        {
            return await Repository.GetAll().ConfigureAwait(false);
        }

        public abstract BaseViewModel<TEntity> CreateViewModel(TEntity model);
        public abstract TEntity CreateNew();

        public async Task Update(BaseViewModel<TEntity> viewModel)
        {
            if (viewModel?.Model == null)
            {
                return;
            }

            await Repository.Update(viewModel.Model).ConfigureAwait(false);
        }

        public async Task Delete(BaseViewModel<TEntity> viewModel)
        {
            if (viewModel?.Model == null)
            {
                return;
            }

            await Repository.Delete(viewModel.Model.Id).ConfigureAwait(false);
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
            _createCommand ?? (_createCommand = new DelegateCommand(async () =>
            {
                await Repository.Create(CreateNew()).ConfigureAwait(false);
                await Load().ConfigureAwait(false);
            }));

        private DelegateCommand _loadCommand;

        public DelegateCommand LoadCommand =>
            _loadCommand ?? (_loadCommand = new DelegateCommand(async () => await Load().ConfigureAwait(false)));

        private DelegateCommand _clearCommand;

        public DelegateCommand ClearCommand =>
            _clearCommand ?? (_clearCommand = new DelegateCommand(Clear));


        private DelegateCommand<BaseViewModel<TEntity>> _updateCommand;

        public DelegateCommand<BaseViewModel<TEntity>> UpdateCommand =>
            _updateCommand ?? (_updateCommand =
                new DelegateCommand<BaseViewModel<TEntity>>(async viewModel =>
                    await Update(viewModel).ConfigureAwait(false)));


        private DelegateCommand<BaseViewModel<TEntity>> _deleteCommand;

        public DelegateCommand<BaseViewModel<TEntity>> DeleteCommand =>
            _deleteCommand ?? (_deleteCommand =
                new DelegateCommand<BaseViewModel<TEntity>>(async viewModel =>
                    await Delete(viewModel).ConfigureAwait(false)));

        #endregion
    }
}