using EnemyAI;
using UnityEngine;

namespace EnemyAI
{
    public class AttackState : IEnemyState
    {
        private EnemyBase enemyAI;

        public void Enter(EnemyBase enemy)
        {
            Debug.Log("ENEMY IS IN ATTACK STATE");
            enemyAI = enemy;
            enemyAI.SetCanMove(false);
            enemyAI.canAttack = true;
            enemyAI.attackDebug = true;
            enemyAI.ChoseMove();
            enemyAI.TilesToAttack();
            BattleManager.GetInstance().GetGridManager().ResetEnemyTileWeight();
            enemyAI.GetConnectionGraph().IncreaseEnemyTileWeights(enemyAI.GetCurrentTile(), enemyAI.GetEnemyData().weight, enemyAI.GetEnemyData().weightDecreaseValue);
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
            enemyAI.VisualizeAttack();
        }

        public void Exit()
        {
            enemyAI.Attack();
            enemyAI.UnvisualizeAttack();
            Debug.Log("ENEMY IS EXITING IN ATTACK STATE");
            enemyAI.SetAttackElapsed(0.0f);
            enemyAI.SetElapsedTime(0.0f);
        }
    }
}
