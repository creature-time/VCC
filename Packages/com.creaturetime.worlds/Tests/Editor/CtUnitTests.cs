
using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CreatureTime
{
    public class CtUnitTests
    {
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)
        ]
        public class UnitTest : Attribute
        {
            public string Name { get; private set; }

            public UnitTest(string name)
            {
                Name = name;
            }
        }

        [AttributeUsage(AttributeTargets.Method)
        ]
        public class Test : Attribute
        {
            public string Name { get; private set; }

            public Test(string name)
            {
                Name = name;
            }
        }

        private Dictionary<Type, List<MethodInfo>> _unitTests = new Dictionary<Type, List<MethodInfo>>();

        public void GatherUnitTests()
        {
            _unitTests.Clear();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.GetCustomAttributes(typeof(UnitTest), true).Length == 0)
                        continue;

                    var methods = type.GetMethods();
                    if (methods.Length == 0)
                        continue;

                    var tests = new List<MethodInfo>();
                    foreach (var method in methods)
                    {
                        if (method.GetCustomAttributes(typeof(Test), true).Length > 0)
                        {
                            tests.Add(method);
                        }
                    }
                    _unitTests.Add(type, tests);
                }
            }
        }

        public void RunTests(out Dictionary<object, Dictionary<MethodInfo, List<string>>> results)
        {
            results = new Dictionary<object, Dictionary<MethodInfo, List<string>>>();
            foreach (var pair in _unitTests)
            {
                var unittest = Activator.CreateInstance(pair.Key);

                var setupMethod = pair.Key.GetMethod("Setup");
                var tearDownMethod = pair.Key.GetMethod("TearDown");

                var testResults = new Dictionary<MethodInfo, List<string>>();
                var instance = Activator.CreateInstance(pair.Key);
                foreach (var test in pair.Value)
                {
                    List<string> errors = new List<string>();
                    testResults.Add(test, errors);
                    setupMethod?.Invoke(unittest, null);

                    try
                    {
                        test.Invoke(instance, null);
                    }
                    catch (TargetInvocationException tie)
                    {
                        if (tie.InnerException != null)
                        {
                            errors.Add(tie.InnerException.Message);
                        }
                        else
                        {
                            errors.Add("Unknown unhandled exception.");
                        }
                    }
                    catch (Exception exc)
                    {
                        errors.Add(exc.Message);
                    }

                    tearDownMethod?.Invoke(unittest, null);
                }

                results.Add(unittest, testResults);
            }
        }

        [MenuItem("CreatureTime/Test Unit Tests")]
        static void TestUnitTests()
        {
            {
                var unittests = new CtUnitTests();
                unittests.GatherUnitTests();

                unittests.RunTests(out var results);

                foreach (var unittest in results)
                {
                    foreach (var test in unittest.Value)
                    {
                        foreach (var error in test.Value)
                        {
                            Debug.LogError(error);
                        }
                    }
                }
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    // [CtUnitTests.UnitTest("UnitTest UnitTest")]
    // public class UnitTestExample
    // {
    //     public void Setup()
    //     {
    //         Debug.Log("Setup");
    //     }
    //
    //     [CtUnitTests.Test("Test Pass Example")]
    //     public void TestPassExample()
    //     {
    //         Assert.AreSame(true, true);
    //     }
    //
    //     [CtUnitTests.Test("Test Fail Example")]
    //     public void TestFailExample()
    //     {
    //         Assert.AreSame(true, false);
    //     }
    //
    //     public void TearDown()
    //     {
    //         Debug.Log("TearDown");
    //     }
    // }
}