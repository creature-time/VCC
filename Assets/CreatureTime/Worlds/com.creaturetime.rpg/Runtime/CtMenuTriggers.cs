
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtMenuTriggers : CtLoggerUdonScript
    {
        [SerializeField] private Transform target;

        public void _LoadMenu()
        {
            target.gameObject.SetActive(true);
        }

        public void _UnloadMenu()
        {
            target.gameObject.SetActive(false);
        }
    }
}
