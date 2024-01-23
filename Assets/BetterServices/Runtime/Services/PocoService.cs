using System;
using System.Threading;
using System.Threading.Tasks;
using Better.Services.Interfaces;
using Better.Validation.Runtime.Attributes;
using UnityEngine;

namespace Better.Services
{
    [Serializable]
    public abstract class PocoService : IService
    {
        public bool Initialized { get; private set; }

        async Task IService.InitializeAsync(CancellationToken cancellationToken)
        {
            Debug.Log($"[{GetType().Name}] {nameof(IService.InitializeAsync)}");

            if (Initialized)
            {
                Debug.LogError($"[{GetType().Name}] {nameof(IService.InitializeAsync)}: already initialized");
                return;
            }

            Initialized = true;
            await OnInitializeAsync(cancellationToken);
            Initialized = !cancellationToken.IsCancellationRequested;
        }

        Task IService.PostInitializeAsync(CancellationToken cancellationToken)
        {
            if (!Initialized)
            {
                Debug.LogError($"[{GetType().Name}] {nameof(IService.PostInitializeAsync)}: not initialized");
                return Task.CompletedTask;
            }

            Debug.Log($"[{GetType().Name}] {nameof(IService.PostInitializeAsync)}");
            return OnPostInitializeAsync(cancellationToken);
        }

        protected abstract Task OnInitializeAsync(CancellationToken cancellationToken);
        protected abstract Task OnPostInitializeAsync(CancellationToken cancellationToken);
    }

    [Serializable]
    public abstract class PocoService<TSettings> : PocoService where TSettings : ScriptableObject
    {
        [NotNull] [SerializeField] private TSettings _settings;

        protected TSettings Settings => _settings;

        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            if (Settings == null)
            {
                throw new NullReferenceException($"[{GetType().Name}] {nameof(OnInitializeAsync)}: {nameof(Settings)} cannot be null");
            }

            return Task.CompletedTask;
        }

        protected override Task OnPostInitializeAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}