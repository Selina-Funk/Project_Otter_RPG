using System.Collections;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    //protected PlayerSystems playerManager;
    //protected PlayableCharacterData characterData;
    //[SerializeField] private static HealthBarUI healthBar;
    //protected PlayerActions playerActions;
    //private CombatState combatState;
    //private bool isDead = false;

    //protected static SpriteInstance spriteInstance;

    //public enum CombatState
    //{
    //    IDLE_COMBAT, THINKING_COMBAT, THINKING_COMBAT_END, ATTACK_COMBAT, HIT_COMBAT, DEATH_COMBAT
    //}

    //public enum InputKeyNames
    //{
    //    upArrow,
    //    downArrow,
    //    rightArrow,
    //    leftArrow
    //}


    //private void Awake()
    //{
    //    spriteInstance = gameObject.GetComponent<SpriteInstance>();
    //    characterData = Resources.Load<PlayableCharacterData>("ScriptableObjects/PlayableCharacterData/HarteData");
    //    characterData.characterCurrentHealth = characterData.characterMaxHealth;
    //    healthBar = GameObject.Find("Harte_Health_Bar").GetComponent<HealthBarUI>();
    //    healthBar.SetHaxHealth(characterData.characterMaxHealth);
    //}

    //public virtual void Init(PlayerSystems system)
    //{
    //    this.playerManager = system;
    //    playerActions = new PlayerActions();
    //    playerActions.Enable();
    //}

    //public virtual void Tick()
    //{
    //    if (characterData != null)
    //    {
    //        healthBar.SetHealth(characterData.characterCurrentHealth);
    //    }
    //}

    //public virtual void FixedTick()
    //{

    //}

    //public virtual void LateTick()
    //{

    //}

    //public void TakeDamage(int damage)
    //{
    //    spriteInstance.Play(CombatState.HIT_COMBAT.ToString().ToLower());
    //    characterData.characterCurrentHealth -= damage;
    //}
}
