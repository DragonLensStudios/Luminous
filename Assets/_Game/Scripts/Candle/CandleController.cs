using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DragonLens.BrackeysGameJam2022_2.Candles;
/// <summary>
/// Script by Redstonetech125 & Mony Dragon
/// </summary>
public class CandleController : MonoBehaviour
{
    [SerializeField]
    private CandleData _candleData;
    [SerializeField]
    private bool candleStateFreeze;
    [SerializeField]
    private string refillSfx, candleOutSfx, foxBgm, levelBGM;

    private bool isCandleLit = true;
    private Animator anim;

    private Queue<CandleColor> candleChanges = new Queue<CandleColor>();
    private PlayerController pc;
    
    /// <summary>
    /// Seconds counting down
    /// </summary>
    private float timeSeconds;

    /// <summary>
    /// The current candle color.
    /// </summary>
    public CandleColor CurrentColor { get; private set; }

    /// <summary>
    /// The current state of candle. 12 = new, 0 = out.
    /// </summary>
    public int CurrentState { get; private set; }
    public bool CandleStateFreeze { get => candleStateFreeze; set => candleStateFreeze = value; }

    private void OnEnable()
    {
        EventManager.onCandleColorChanged += EventManager_onCandleColorChanged;
        EventManager.onCandleOut += EventManager_onCandleOut;
        EventManager.onCandleReset += EventManager_onCandleReset;
    }

    private void OnDisable()
    {
        EventManager.onCandleColorChanged -= EventManager_onCandleColorChanged;
        EventManager.onCandleOut -= EventManager_onCandleOut;
        EventManager.onCandleReset -= EventManager_onCandleReset;
    }

    private void Awake()
    {
        if(_candleData == null) {
            Debug.LogWarning("CandleData not assigned. Using default data from Resources...");
            _candleData = Resources.Load<CandleData>("ScriptableObjects/CandleYellow");
        }

        anim = GetComponent<Animator>();
        if(anim != null) anim.runtimeAnimatorController = _candleData.Anim;
        pc = FindObjectOfType<PlayerController>();
    }

    private void Start()
    {
        EventManager.CandleReset();
    }

    private void Update()
    {
        if (!candleStateFreeze && isCandleLit)
        {
            //Countdown Time
            if (timeSeconds < _candleData.DivisibleFactor && CurrentState > 0)
            {
                if (pc != null)
                {
                    if (pc.IsRunning) 
                    {
                        timeSeconds += Time.deltaTime * 2;
                    }
                    else
                    {
                        timeSeconds += Time.deltaTime;
                    }
                }

            }
            else if (timeSeconds >= _candleData.DivisibleFactor && CurrentState > 0)
            {
                timeSeconds = 0;
                CurrentState -= 1;
                //Debug.Log(CurrentState);
                anim.SetInteger("Candlestate", CurrentState);
            }
            else if (CurrentState <= 0)
            {
                EventManager.CandleOut();
                Debug.Log("Candle Out");
            }
        }
    }

    private void EventManager_onCandleColorChanged(CandleColor color)
    {
        CurrentColor = color;
        switch (color)
        {
            case CandleColor.Yellow:
                anim.SetBool("Yellow", true);
                anim.SetBool("Red", false);
                anim.SetBool("Purple", false);
                anim.SetBool("Blue", false);
                break;
            case CandleColor.Red:
                anim.SetBool("Yellow", false);
                anim.SetBool("Red", true);
                anim.SetBool("Purple", false);
                anim.SetBool("Blue", false);
                break;
            case CandleColor.Purple:
                anim.SetBool("Yellow", false);
                anim.SetBool("Red", false);
                anim.SetBool("Purple", true);
                anim.SetBool("Blue", false);
                break;
            case CandleColor.Blue:
                anim.SetBool("Yellow", false);
                anim.SetBool("Red", false);
                anim.SetBool("Purple", false);
                anim.SetBool("Blue", true);
                break;
        }
        if (anim != null)
        {
            anim.SetInteger("Candlestate", CurrentState);

        }
    }

    private void EventManager_onCandleOut()
    {
        isCandleLit = false;
        if (!string.IsNullOrWhiteSpace(candleOutSfx))
        {
            AudioManager.instance.PlaySound(candleOutSfx);
        }
        if (!string.IsNullOrWhiteSpace(foxBgm))
        {
            if (foxBgm != AudioManager.instance.CurrentlyPlayingMusic)
            {
                AudioManager.instance.StopMusic(AudioManager.instance.CurrentlyPlayingMusic);
                AudioManager.instance.PlayMusic(foxBgm);
            }
        }
    }

    private void EventManager_onCandleReset()
    {
        StopAllCoroutines();
        candleChanges.Clear();
        isCandleLit = true;
        CurrentState = _candleData.MaxStateIndex;
        EventManager.CandleColorChanged(_candleData.Color);
        anim.SetTrigger("Refill");
        anim.SetInteger("Candlestate", CurrentState);
        timeSeconds = 0;
        if (!string.IsNullOrWhiteSpace(refillSfx))
        {
            AudioManager.instance.PlaySound(refillSfx);
        }
        if (!string.IsNullOrWhiteSpace(levelBGM))
        {
            if (levelBGM != AudioManager.instance.CurrentlyPlayingMusic)
            {
                AudioManager.instance.StopMusic(AudioManager.instance.CurrentlyPlayingMusic);
                AudioManager.instance.PlayMusic(levelBGM);
            }
        }
    }
}