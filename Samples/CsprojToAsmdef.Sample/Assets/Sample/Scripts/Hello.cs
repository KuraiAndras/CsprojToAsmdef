﻿using ExternalDependency;
using SampleUnityDependency;
using TMPro;
using UnityEngine;

// ReSharper disable Unity.RedundantSerializeFieldAttribute
// ReSharper disable once FieldCanBeMadeReadOnly.Local

namespace Sample.Scripts
{
    public class Hello : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text = default;

        private void Start()
        {
            const string myDude = "My dude!";

            CustomLogger.Log(myDude);
            _text.text = "Hello, " + myDude + " " + CustomCalculator.Add(21, 21);
        }
    }
}
