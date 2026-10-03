using EnemyAI;
using UnityEngine;

namespace EnemyAI
{
    public class AttackState : IEnemyState
    {
        private TestEnemy enemyAI;

        public void Enter(TestEnemy enemy)
        {
            Debug.Log("ENEMY IS IN ATTACK STATE");
            enemyAI = enemy;
            enemyAI.SetCanMove(false);
            enemyAI.canAttack = true;
            enemyAI.attackDebug = true;
        }

        public void Update()
        {
            Debug.Log("ENEMY IS STILL IN ATTACK STATE");
            if(enemyAI.GetElapsedTime() < enemyAI.GetTimeToAttack())
            {
                enemyAI.AddToElapsedTime(Time.deltaTime);
            }
            else
            {
                enemyAI.canAttack = false;
            }
        }

        public void Exit()
        {
            Debug.Log("ENEMY IS EXITING IN ATTACK STATE");
            enemyAI.SetElapsedTime(0.0f);
        }
    }
}
