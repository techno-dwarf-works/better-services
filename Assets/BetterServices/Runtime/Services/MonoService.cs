using System.Threading;
using System.Threading.Tasks;
using Better.Services.Runtime.Interfaces;
using UnityEngine;

namespace Better.Services.Runtime
{
    public abstract class MonoService : MonoBehaviour, IService
    {
        private CancellationTokenSource _aliveTokenSource;

        public bool Initialized { get; private set; }
        private CancellationToken AliveToken => _aliveTokenSource.Token;

        protected virtual void Awake()
        {
            _aliveTokenSource = new();
        }

        async Task IService.InitializeAsync(CancellationToken cancellationToken)
        {
            Debug.Log($"[{GetType().Name}] {nameof(IService.InitializeAsync)}");

            if (Initialized)
            {
                Debug.LogError($"[{GetType().Name}] {nameof(IService.InitializeAsync)}: already initialized");
                return;
            }

            var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(AliveToken, cancellationToken);
            cancellationToken = linkedTokenSource.Token;
            
            await OnInitializeAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            Initialized = true;
        }

        Task IService.PostInitializeAsync(CancellationToken cancellationToken)
        {
            if (!Initialized)
            {
                Debug.LogError($"[{GetType().Name}] {nameof(IService.PostInitializeAsync)}: not initialized");
                return Task.CompletedTask;
            }

            Debug.Log($"[{GetType().Name}] {nameof(IService.PostInitializeAsync)}");
            
            var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(AliveToken, cancellationToken);
            cancellationToken = linkedTokenSource.Token;
            
            return OnPostInitializeAsync(cancellationToken);
        }

        protected abstract Task OnInitializeAsync(CancellationToken cancellationToken);
        protected abstract Task OnPostInitializeAsync(CancellationToken cancellationToken);

        protected virtual void OnDestroy()
        {
            _aliveTokenSource?.Cancel();
        }
    }
}