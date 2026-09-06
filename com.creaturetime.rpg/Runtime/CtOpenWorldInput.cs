
using UnityEngine;
using VRC.Udon.Common;

namespace CreatureTime
{
    public class CtOpenWorldInput : CtSingleton
    {
        [SerializeField] private CtDialogueManager dialogueManager;

        private CtPlayerEntity _localEntity;

        public CtPlayerEntity LocalEntity
        {
            set
            {
                _localEntity = value;
            }
        }

        public override void InputUse(bool value, UdonInputEventArgs args)
        {
            if (!gameObject.activeSelf || !_localEntity) return;

            if (args.handType != HandType.RIGHT || !args.boolValue) return;

            // NPC Interaction
            var position = _localEntity.RootTransform.position;
            var closestDistance = 2f;
            CtDialogueActor closestActor = null;
            var headLookDirection = _localEntity.HeadTransform.forward;
            foreach (var actor in dialogueManager.DialogueDatabase.Actors)
            {
                var actorPosition = actor.transform.position;
                var distance = Vector3.Distance(position, actorPosition);
#if DEBUG_LOGS
                LogDebug($"Checking actor distance (closestActor={closestActor}, distance={distance}).");
#endif

                if (distance >= closestDistance) continue;
                var angle = Vector3.SignedAngle(actorPosition - position, headLookDirection, Vector3.up);
#if DEBUG_LOGS
                LogDebug($"Checking actor angle (angle={angle}).");
#endif
                if (angle < -30 || angle > 30) continue;

                closestDistance = distance;
                closestActor = actor;
            }

#if DEBUG_LOGS
            LogDebug($"Closest actor check (closestDistance={closestDistance}, closestActor={closestActor}).");
#endif

            // var headTransform = _localEntity.HeadTransform;
            // if (!Physics.Raycast(headTransform.position, headTransform.forward, out var hit)) return;
            //
            // var closestActor = hit.collider.GetComponentInParent<CtDialogueActor>();

            if (!closestActor) return;
            if (dialogueManager.ConversationModel.Actor == closestActor) return;

            dialogueManager.StartConversationWithActor(closestActor);
        }
    }
}