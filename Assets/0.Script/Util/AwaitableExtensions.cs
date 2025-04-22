using System.Runtime.CompilerServices;
using UnityEngine;

namespace Util
{
   public static class AwaitableExtensions
    {
        public static Awaiter GetAwaiter(this AsyncOperation operation)
        {
            return new Awaiter(operation);
        }

        public struct Awaiter : INotifyCompletion
        {
            private readonly AsyncOperation _operation;
            private System.Action _continuation;

            public Awaiter(AsyncOperation operation)
            {
                _operation = operation;
                _continuation = null;
                _operation.completed += OnRequestCompleted;
            }

            public bool IsCompleted => _operation.isDone;

            public void GetResult() { }

            public void OnCompleted(System.Action continuation)
            {
                _continuation = continuation;
            }

            private void OnRequestCompleted(AsyncOperation obj)
            {
                _continuation?.Invoke();
            }
        }
    }
}