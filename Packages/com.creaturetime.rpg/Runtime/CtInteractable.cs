
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtInteractable : CtAbstractSignal
    {
        [SerializeField] private Animator animator;
        [SerializeField] private CtSoundBuilder soundBuilder;
        [SerializeField] private AudioClip onInteractSound;
        [SerializeField] private AudioClip onInterruptSound;
        [SerializeField] private float closeDistance = 3f;
        [SerializeField] private GameObject tmpMenu;

        private bool isInteracting;

        public override void Interact()
        {
            isInteracting = !isInteracting;
            if (isInteracting)
            {
                Open();
            }
            else
            {
                Close();
            }
        }

        private void Open()
        {
            animator.SetBool("IsInteracting", true);
            soundBuilder
                .Setup(onInteractSound, false, true, 1f)
                .SetPosition(transform.position)
                .Play();
            isInteracting = true;
            tmpMenu.SetActive(true);
        }

        private void Close()
        {
            animator.SetBool("IsInteracting", false);
            soundBuilder
                .Setup(onInterruptSound, false, true, 1f)
                .SetPosition(transform.position)
                .Play();
            isInteracting = false;
            tmpMenu.SetActive(false);
        }

        private void Update()
        {
            if (!isInteracting) return;

            var localPlayer = Networking.LocalPlayer;
            if (localPlayer == null) return;
            if (Vector3.Distance(transform.position, localPlayer.GetPosition()) <= closeDistance) return;
            Close();
        }
    }
}