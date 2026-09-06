
using System;
using UdonSharp;
using UnityEngine;

namespace CreatureTime.UnitTest
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtUnitTests : CtLoggerUdonScript
    {
        private CtUnitTest[] _unitTests;

        private void Start()
        {
            _unitTests = gameObject.GetComponentsInChildren<CtUnitTest>(true);
        }

        public override void Interact()
        {
            foreach (var unitTest in _unitTests)
            {
                if (!unitTest.RunTest) continue;

                Log($"Running test (unitTest={unitTest.gameObject.name})...");

                unitTest.SetUp();
                unitTest.Run();
                unitTest.TearDown();
            }
        }
    }
}