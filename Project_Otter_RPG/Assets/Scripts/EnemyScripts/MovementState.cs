using EnemyAI;
using UnityEngine;

namespace EnemyAI
{
    public class MovementState : IEnemyState
    {
        private TestEnemy enemyAI;

        public void Enter(TestEnemy enemy)
        {
            Debug.Log("ENEMY IS IN MOVE STATE");
            enemyAI = enemy;
            enemyAI.SetCanMove(true);
            enemyAI.canAttack = false;
        }

        public void Update()
        {
            Debug.Log("ENEMY IS STILL IN MOVE STATE");
            if (enemyAI.GetElapsedTime() < enemyAI.GetTimeToMove())
            {
                enemyAI.AddToElapsedTime(Time.deltaTime);
            }
            else
            {
                enemyAI.SetCanMove(false);
            }
        }

        public void Exit()
        {
            Debug.Log("ENEMY IS EXITING IN MOVE STATE");
            enemyAI.SetElapsedTime(0.0f);
        }
    }
}
