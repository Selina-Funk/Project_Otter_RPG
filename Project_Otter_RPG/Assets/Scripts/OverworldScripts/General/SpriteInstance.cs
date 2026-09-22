using System;
using System.Collections;
using UnityEngine;
using static BillboardManager;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteInstance : MonoBehaviour
{
    [Serializable]
    struct Renderer
    { 
        public SpriteRenderer spriteRenderer;
        public bool billboarded;
    }

    [Header("References")]
    public SpriteBundleData bundleData;
    [SerializeField] private bool usesBillboard = true;

    [Tooltip("For renderers that need to be synced with the main sprite")]
    [SerializeField] private Renderer[] additionalRenderers;

    private SpriteRenderer mainRenderer;
    private Direction _currentDirection;
    public Direction CurrentDirection
    {
        get { return _currentDirection; }
        set 
        { 
            Direction old = _currentDirection;
            if (old != value)
            {
                if (!string.IsNullOrEmpty(currentAnim.name)) Play(currentAnim.name);
                _currentDirection = value;
            }
        }
    }

    [Header("Animation")]
    public SpriteAnimation currentAnim;
    private int currentFrameIndex;
    public int CurrentFrameIndex { get { return Mathf.Min(currentFrameIndex, currentAnim.frameSets[(int)_currentDirection].frames.Length - 1); } }


    [Tooltip("Will be automatically played in Start().")]
    [SerializeField] private string defaultAnimation;
    
    private Coroutine animTimer;

    [Header("Misc")]
    [SerializeField] private bool hasSilhouette;

    private void Start()
    {
        mainRenderer = GetComponent<SpriteRenderer>();
        if(!string.IsNullOrEmpty(defaultAnimation)) Play(defaultAnimation);
        else Debug.Log("no default animation found");
    }

    private void LateUpdate()
    {
        if (currentAnim == null) return;
        int dir = (int)_currentDirection;
        if (dir >= currentAnim.frameSets.Length) return;
        var frames = currentAnim.frameSets[dir].frames;
        if (currentFrameIndex >= frames.Length)
        {
            currentAnim.hasEnded = true;
            return;
        }

        mainRenderer.sprite = frames[currentFrameIndex];
        foreach (var sr in additionalRenderers)
        {
            sr.spriteRenderer.sprite = frames[currentFrameIndex];
        }
    }

    private void OnEnable()
    {
        if(currentAnim != null) Play(currentAnim.name);

        if (BillboardManager.Instance != null)
        {
            BillboardManager.Instance.spriteInstances.Add(this);

            if (additionalRenderers.Length > 0)
            {
                foreach (var sr in additionalRenderers)
                {
                    if (sr.billboarded)
                    {
                        BillboardManager.Instance.additionalSprites.Add(sr.spriteRenderer.gameObject);
                    }
                }
            }
        }
    }

    private void OnDisable()
    {
        if(animTimer != null) StopCoroutine(animTimer);
        currentFrameIndex = 0;

        if (BillboardManager.Instance != null)
        {
            BillboardManager.Instance.spriteInstances.Remove(this);
            if (additionalRenderers.Length > 0)
            {
                foreach (var sr in additionalRenderers)
                {
                    if (sr.billboarded)
                    {
                        BillboardManager.Instance.additionalSprites.Remove(sr.spriteRenderer.gameObject);
                    }
                }
            }
        }
    }

    public void Play(string name)
    {
        if (animTimer != null) StopCoroutine(animTimer);
        var findAnim = bundleData.animations.Find((anim) => anim.name == name);

        if (findAnim != null)
        {
            currentAnim = findAnim;
            currentFrameIndex = 0;
            animTimer = StartCoroutine(AnimationTimer());
        }
        else Debug.Log($"anim '{name}' not found");
    }

    public void Stop(string fallback = null)
    {
        if (string.IsNullOrEmpty(fallback))
        {
            Play("idle");
        }
        else Play(fallback);
    }

    public void Stop(GameObject character, string fallback = null)
    {
        if (string.IsNullOrEmpty(fallback))
        {
            Play("idle");
        }
        else
        {
            foreach (var anim in bundleData.animations)
            {
                if (character.GetComponent<SpriteInstance>().currentAnim.name == anim.name)
                {
                    Play(anim.animOnEnd);
                    break;
                }
            }
        }
    }

    private IEnumerator AnimationTimer()
    {
        bool hasSpecificFrameDelays = currentAnim.frameDelays.Length > 0;

        do
        {
            for (currentFrameIndex = 0; currentFrameIndex < currentAnim.frameSets[(int)_currentDirection].frames.Length; currentFrameIndex++)
            {
                if (hasSpecificFrameDelays) yield return new WaitForSeconds(currentAnim.frameDelays[currentFrameIndex]);
                else yield return new WaitForSeconds(currentAnim.uniformFrameDelay);
            }
        }
        while (currentAnim.looping);

        if(!string.IsNullOrEmpty(currentAnim.animOnEnd))
        {
            Play(currentAnim.animOnEnd);
        }
    }

    public void SetAnimationOnEnd(string endAnimation)
    {
        currentAnim.animOnEnd = endAnimation;
    }

    public bool GetUsesBillboard()
    {
        return usesBillboard;
    }
}
