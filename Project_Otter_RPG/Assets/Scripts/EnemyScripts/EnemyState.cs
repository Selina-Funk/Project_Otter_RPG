using UnityEngine;

namespace EnemyAI
{
    public interface IEnemyState
    {
        void Enter(EnemyBase enemy);
        void Update();
        void Exit();
    }
}
