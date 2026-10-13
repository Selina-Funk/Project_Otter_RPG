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
            enemyAI.ChoseMove();
            enemyAI.TilesToAttack();
            BattleManager.GetInstance().GetGridManager().ResetEnemyTileWeight();
            GraphBehavior.ChangeWeights(BattleManager.GetInstance().GetGridManager().FindTileKey(enemyAI.GetCurrentTile().gameObject, false), false, enemyAI.GetEnemyData().weight, enemyAI.GetEnemyData().weightDecreaseValue);
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
            //BattleManager.GetInstance().GetGridManager().ResetEnemyTileWeight();
            Debug.Log("ENEMY IS EXITING IN ATTACK STATE");
            enemyAI.SetElapsedTime(0.0f);
        }
    }
}
