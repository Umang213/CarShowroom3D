using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using System.Collections.Generic;

public class TutorialControler : MonoBehaviour
{
    public static TutorialControler Instance;
    public Transform targetPoint;
    public UnityEvent tutorialEvent = new UnityEvent();

    [Header("Shop Storage")] public ShopStorage wheelShop;
    public ShopStorage engineShop;
    public ShopStorage colorShop;

    [SerializeField] LineRenderer lineRenderer;

    PlayerController _playerController;
    CarBuildControler _carBuildController;
    public List<Unlockable> allUnlockables;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        _playerController = PlayerController.instance;
        _carBuildController = CarBuildControler.instance;
        lineRenderer.material.DOOffset(new Vector2(-1, 0), 1f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Incremental);
    }

    private void FixedUpdate()
    {
        if (tutorialEvent != null)
        {
            tutorialEvent.Invoke();
        }

        if (targetPoint != null)
        {
            lineRenderer.gameObject.SetActive(true);
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, _playerController.transform.position.With(y: 0.2f));
            lineRenderer.SetPosition(1, targetPoint.position.With(y: .2f));
        }
        else
        {
            lineRenderer.gameObject.SetActive(false);
        }

        if (PlayerPrefs.GetInt(PlayerPrefsKey.TutorialCount, 0).Equals(1))
        {
            for (var i = 0; i < allUnlockables.Count; i++)
            {
                var money = PlayerPrefs.GetInt(PlayerPrefsKey.Money, 0);
                if (allUnlockables[i].price > 0)
                {
                    if (money >= allUnlockables[i].price)
                    {
                        lineRenderer.gameObject.SetActive(true);
                        lineRenderer.positionCount = 2;
                        lineRenderer.SetPosition(0, _playerController.transform.position.With(y: 0.2f));
                        lineRenderer.SetPosition(1, allUnlockables[i].transform.position.With(y: 0.2f));
                    }
                    else
                    {
                        lineRenderer.gameObject.SetActive(false);
                    }

                    break;
                }
            }
        }
    }

    public void CheckForAddWheel()
    {
        var count = _playerController.allStackItems.FindAll(x => x.CompareTag(_carBuildController.wheel.tag)).Count;
        targetPoint = count >= 4 ? _carBuildController.addWheel.transform : wheelShop.transform;
    }

    public void CheckForAddEngine()
    {
        var count = _playerController.allStackItems.FindAll(x => x.CompareTag(_carBuildController.engine.tag)).Count;
        targetPoint = count >= 1 ? _carBuildController.addEngine.transform : engineShop.transform;
    }

    public void CheckForAddColor()
    {
        var count = _playerController.allStackItems.FindAll(x => x.CompareTag(_carBuildController.color.tag)).Count;
        targetPoint = count >= 1 ? _carBuildController.addColor.transform : colorShop.transform;
    }

    public void RemoveTarget()
    {
        targetPoint = null;
        tutorialEvent.RemoveAllListeners();
    }
}