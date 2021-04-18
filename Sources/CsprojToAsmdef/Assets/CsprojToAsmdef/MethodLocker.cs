using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

namespace CsprojToAsmdef
{
    public static class MethodLocker
    {
        private static readonly ConcurrentBag<string> RunningDictionary = new ConcurrentBag<string>();

        public static Task RunLockedMethod(Func<Task> methodBody, [CallerMemberName] string methodName = default)
        {
            if (methodName is null) throw new ArgumentNullException(nameof(methodName), "Must use method name");

            return RunLockedMethodInternal(methodBody, methodName);
        }

        private static async Task RunLockedMethodInternal(Func<Task> methodBody, string methodName)
        {
            try
            {
                var isRunning = RunningDictionary.TryPeek(out _);
                if (isRunning)
                {
                    Debug.Log($"Failed to run {methodName}, because it is already running");
                    return;
                }

                RunningDictionary.Add(methodName);

                await methodBody().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                RunningDictionary.TryTake(out _);
            }
        }
    }
}
