using UnityEngine;

namespace Retro.ThirdPersonCharacter
{
    public class CombatSword : Combat
    {
        private int maxCombo = 3;
        private float comboResetTime = 1.2f;

        private int currentCombo = 0;
        private float lastAttackTime = 0f;
        protected override void Start()
        {
            base.Start(); 
        }

        private void Update()
        {
            if (Time.time - lastAttackTime > comboResetTime)
            {
                currentCombo = 0;
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (!AttackInProgress)
                {
                    currentCombo = (currentCombo % maxCombo) + 1;
                    Debug.Log(currentCombo);
                    lastAttackTime = Time.time;

                    _animator.SetTrigger("Attack" + currentCombo);
                }
            }
            else if (Input.GetMouseButtonDown(1))
            {
                SpecialAttack();
            }
        }
    }
}