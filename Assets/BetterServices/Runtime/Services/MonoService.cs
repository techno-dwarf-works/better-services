using System.Threading;
using System.Threading.Tasks;
using Better.Services.Runtime.Interfaces;
using UnityEngine;

namespace Better.Services.Runtime
{
    public abstract class MonoService : MonoBehaviour, IService
    {
        // TODO: Add version dependency
        private CancellationTokenSource _destroyCancellationToken;

        public bool Initialized { get; private set; }
        protected CancellationToken DestroyCancellationToken => _destroyCancellationToken.Token;

        protected virtual void Awake()
        {
            _destroyCancellationToken = new();
        }

        async Task IService.InitializeAsync(CancellationToken cancellationToken)
        {
            if (Initialized)
            {
                Debug.LogError("Service already initialized");
                return;
            }

            var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(DestroyCancellationToken, cancellationToken);
            cancellationToken = linkedTokenSource.Token;

            await OnInitializeAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            Initialized = true;
            Debug.Log("Service initialized");
        }

        Task IService.PostInitializeAsync(CancellationToken cancellationToken)
        {
            if (!Initialized)
            {
                Debug.LogError("Service must be initialized");
                return Task.CompletedTask;
            }

            var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(DestroyCancellationToken, cancellationToken);
            cancellationToken = linkedTokenSource.Token;

            return OnPostInitializeAsync(cancellationToken);
        }

        protected abstract Task OnInitializeAsync(CancellationToken cancellationToken);
        protected abstract Task OnPostInitializeAsync(CancellationToken cancellationToken);

        protected virtual void OnDestroy()
        {
            _destroyCancellationToken?.Cancel();
        }
    }
}