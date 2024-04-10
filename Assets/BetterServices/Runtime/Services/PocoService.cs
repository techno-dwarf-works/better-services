using System;
using System.Threading;
using System.Threading.Tasks;
using Better.Services.Runtime.Interfaces;
using UnityEngine;

#if BETTER_VALIDATION
using Better.Validation.Runtime.Attributes;
#endif

namespace Better.Services.Runtime
{
    [Serializable]
    public abstract class PocoService : IService
    {
        public bool Initialized { get; private set; }

        async Task IService.InitializeAsync(CancellationToken cancellationToken)
        {
            if (Initialized)
            {
                Debug.LogError("Service already initialized");
                return;
            }

            Initialized = true;
            await OnInitializeAsync(cancellationToken);
            Initialized = !cancellationToken.IsCancellationRequested;

            Debug.Log("Service initialized");
        }

        Task IService.PostInitializeAsync(CancellationToken cancellationToken)
        {
            if (!Initialized)
            {
                Debug.LogError("Service must be initialized");
                return Task.CompletedTask;
            }

            return OnPostInitializeAsync(cancellationToken);
        }

        protected abstract Task OnInitializeAsync(CancellationToken cancellationToken);
        protected abstract Task OnPostInitializeAsync(CancellationToken cancellationToken);
    }

    [Serializable]
    public abstract class PocoService<TSettings> : PocoService where TSettings : ScriptableObject
    {
#if BETTER_VALIDATION
        [NotNull]
#endif
        [SerializeField] private TSettings _settings;

        protected TSettings Settings => _settings;

        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            if (Settings == null)
            {
                var exception = new NullReferenceException(nameof(Settings));
                Debug.LogException(exception);
            }

            return Task.CompletedTask;
        }

        protected override Task OnPostInitializeAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}