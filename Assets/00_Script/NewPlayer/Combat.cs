using UnityEngine;
using NaughtyCharacter;

namespace Retro.ThirdPersonCharacter
{
    [RequireComponent(typeof(Animator))]
    public class Combat : MonoBehaviour
    {
        private const string attackTriggerName = "Attack";
        private const string specialAttackTriggerName = "Ability";

        protected Animator _animator; 

        public bool AttackInProgress { get; private set; } = false;

        protected virtual void Start()
        {
            _animator = GetComponent<Animator>();
        }

        private void Update()
        {
        }

        private void SetAttackStart()
        {
            AttackInProgress = true;
        }

        private void SetAttackEnd()
        {
            AttackInProgress = false;
        }

        public void Attack()
        {
            if (!AttackInProgress)
            {
                _animator.SetTrigger(attackTriggerName);
            }
        }

        public void SpecialAttack()
        {
            if (!AttackInProgress)
            {
                _animator.SetTrigger(specialAttackTriggerName);
            }
        }
    }
}