using SampleUnityDependency;
using SomeOtherDependency;
using TMPro;
using UnityEngine;

// ReSharper disable Unity.RedundantSerializeFieldAttribute

namespace Sample.Scripts
{
    public class Hello : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text = default;

        private void Start()
        {
            const string prefix = "My dude!";

            CustomLogger.LogWarning(prefix);
            _text.text = "Hello, " + Messenger.Message;
        }
    }
}
