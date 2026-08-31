using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SFX : MonoBehaviour
{
    public static SFX Instance { get; private set; }
    public AudioSource sfxSource;
    public AudioClip pieceMoveClip;
    public AudioClip levelSelectClip;
    public AudioClip victoryClip;
    public AudioClip lossClip;
    public AudioClip dialogueClip;
    public AudioClip endingClip;
    public AudioClip buttonClickClip;
    public AudioClip startQuitClip;
    public Slider sfxSlider;
    public TextMeshProUGUI volumeText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (sfxSource == null)
            sfxSource = GetComponent<AudioSource>();

        if (sfxSource != null)
        {
            sfxSource.loop = false;
            sfxSource.volume = PlayerPrefs.GetFloat("SFXVolume", 1.0f);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        InitializeUI();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeUI();
    }

    public void InitializeUI()
    {
        float savedVolume = PlayerPrefs.GetFloat("SFXVolume", 1.0f);

        if (sfxSource != null)
        {
            sfxSource.volume = savedVolume;
        }

        if (sfxSlider == null)
        {
            sfxSlider = FindAnyObjectByType<Slider>(FindObjectsInactive.Include);
        }

        if (volumeText == null)
        {
            TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include);
            
            foreach (var t in allTexts)
            {
                if (t.gameObject.name == "VolumeText" || t.gameObject.name == "Volume")
                {
                    volumeText = t;
                    break;
                }
            }
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.SetValueWithoutNotify(savedVolume);
            sfxSlider.onValueChanged.AddListener(SetVolume);
        }

        UpdateVolumeUI(savedVolume);
    }
    public void SetVolume(float value)
    {
        if (sfxSource != null)
        {
            sfxSource.volume = value;
        }

        PlayerPrefs.SetFloat("SFXVolume", value);
        PlayerPrefs.Save();

        UpdateVolumeUI(value);
    }

    private void UpdateVolumeUI(float value)
    {
        if (volumeText != null)
        {
            int percentage = Mathf.RoundToInt(value * 100f);
            volumeText.text = percentage + "%";
            volumeText.color = (percentage == 0) ? Color.red : Color.white;
        }
    }

    public void PlayPieceMove() => PlaySound(pieceMoveClip);
    public void PlayLevelSelect() => PlaySound(levelSelectClip);
    public void PlayVictory() => PlaySound(victoryClip);
    public void PlayLoss() => PlaySound(lossClip);
    public void PlayEnding() => PlaySound(endingClip);
    public void PlayButtonClick() => PlaySound(buttonClickClip);
    public void PlayStartOrQuit() => PlaySound(startQuitClip);

    public void PlayDialogue()
    {
        if (sfxSource != null && dialogueClip != null)
        {
            if (!sfxSource.isPlaying)
            {
                sfxSource.PlayOneShot(dialogueClip);
            }
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}