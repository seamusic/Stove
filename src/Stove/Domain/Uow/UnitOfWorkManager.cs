﻿using System.Transactions;

using Autofac.Extras.IocManager;

namespace Stove.Domain.Uow
{
    /// <summary>
    ///     Unit of work manager.
    /// </summary>
    internal class UnitOfWorkManager : IUnitOfWorkManager, ITransientDependency
    {
        private IScopeResolver _childScope;
        private readonly ICurrentUnitOfWorkProvider _currentUnitOfWorkProvider;
        private readonly IUnitOfWorkDefaultOptions _defaultOptions;
        private readonly IScopeResolver _scopeResolver;

        public UnitOfWorkManager(
            IScopeResolver scopedResolver,
            ICurrentUnitOfWorkProvider currentUnitOfWorkProvider,
            IUnitOfWorkDefaultOptions defaultOptions)
        {
            _scopeResolver = scopedResolver;
            _currentUnitOfWorkProvider = currentUnitOfWorkProvider;
            _defaultOptions = defaultOptions;
        }

        public IActiveUnitOfWork Current
        {
            get { return _currentUnitOfWorkProvider.Current; }
        }

        public IUnitOfWorkCompleteHandle Begin()
        {
            return Begin(new UnitOfWorkOptions());
        }

        public IUnitOfWorkCompleteHandle Begin(TransactionScopeOption scope)
        {
            return Begin(new UnitOfWorkOptions { Scope = scope });
        }

        public IUnitOfWorkCompleteHandle Begin(UnitOfWorkOptions options)
        {
            _childScope = _scopeResolver.BeginScope();

            options.FillDefaultsForNonProvidedOptions(_defaultOptions);

            if (options.Scope == TransactionScopeOption.Required && _currentUnitOfWorkProvider.Current != null)
            {
                return new InnerUnitOfWorkCompleteHandle();
            }

            var uow = _childScope.Resolve<IUnitOfWork>();

            uow.Completed += (sender, args) => { _currentUnitOfWorkProvider.Current = null; };

            uow.Failed += (sender, args) => { _currentUnitOfWorkProvider.Current = null; };

            uow.Disposed += (sender, args) => { _childScope.Dispose(); };

            uow.Begin(options);

            _currentUnitOfWorkProvider.Current = uow;

            return uow;
        }
    }
}
