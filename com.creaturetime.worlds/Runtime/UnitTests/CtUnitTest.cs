
using System;
using UdonSharp;
using UnityEngine;

namespace CreatureTime.UnitTest
{
    public abstract class CtUnitTest : CtLoggerUdonScript
    {
        [SerializeField] private bool runTest = true;

        public bool RunTest => runTest;

        public virtual void SetUp() { }
        public abstract void Run();
        public virtual void TearDown() { }
    }
}