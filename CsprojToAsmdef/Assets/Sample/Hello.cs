using SampleUnityDependency;
using SomeOtherDependency;
using TMPro;
using UnityEngine;

namespace Sample
{
    // ReSharper disable once Unity.RedundantSerializeFieldAttribute
    public class Hello : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text = default;

        private void Start()
        {
            CustomLogger.LogWarning("My dude!");
            _text.text = Messenger.GetMessage();
        }
    }
}
