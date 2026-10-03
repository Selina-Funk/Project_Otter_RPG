using UnityEngine;

namespace EnemyAI
{
    public interface IEnemyState
    {
        void Enter(TestEnemy enemy);
        void Update();
        void Exit();
    }
}
