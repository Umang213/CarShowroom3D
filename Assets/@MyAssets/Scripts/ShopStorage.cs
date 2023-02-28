using System.Collections;
using DG.Tweening;
using UnityEngine;

public class ShopStorage : MonoBehaviour
{
    public int maxStackCount;
    public Transform[] points;
    public Transform blueColorCan;
    public Transform yellowColorCan;
    public Transform grayColorCan;
    public Collectables prefab;
    public Collectables[] colorCans;
    PlayerController _playerController;
    public bool isColorShop;
    bool _isPlayer;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            if (_isPlayer) return;
            _playerController = player;
            _isPlayer = true;
            StartCoroutine(AddToPlayerStack());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!_isPlayer) return;
            _isPlayer = false;
        }
    }

    IEnumerator AddToPlayerStack()
    {
        if (isColorShop)
        {
            yield return new WaitForSeconds(0.5f);
            if (_isPlayer)
            {
                if (_playerController.allStackItems.Count < maxStackCount)
                {
                    var carColorCode = CarBuildControler.instance.currentCar.carColorCode;
                    if (carColorCode.Equals(1))
                    {
                        blueColorCan.DOScale(Vector3.zero, 0.2f).OnComplete(() =>
                        {
                            var temp = Instantiate(colorCans[0], blueColorCan.position, blueColorCan.rotation);
                            _playerController.AddToStack(temp);
                            blueColorCan.DOScale(Vector3.one, 0.2f).SetDelay(0.2f);
                        });
                    }
                    else if (carColorCode.Equals(2))
                    {
                        yellowColorCan.DOScale(Vector3.zero, 0.2f).OnComplete(() =>
                        {
                            var temp = Instantiate(colorCans[1], yellowColorCan.position, yellowColorCan.rotation);
                            _playerController.AddToStack(temp);
                            yellowColorCan.DOScale(Vector3.one, 0.2f).SetDelay(0.2f);
                        });
                    }
                    else if (carColorCode.Equals(3))
                    {
                        grayColorCan.DOScale(Vector3.zero, 0.2f).OnComplete(() =>
                        {
                            var temp = Instantiate(colorCans[2], grayColorCan.position, grayColorCan.rotation);
                            _playerController.AddToStack(temp);
                            grayColorCan.DOScale(Vector3.one, 0.2f).SetDelay(0.2f);
                        });
                    }
                }
            }
        }
        else
        {
            for (var i = 0; i < 4; i++)
            {
                if (_isPlayer)
                {
                    if (_playerController.allStackItems.Count < maxStackCount)
                    {
                        //if (_playerController.allStackItems.Count.Equals(_playerController.maxStackCount)) yield break;
                        var pos = points[Helper.RandomInt(0, points.Length)];
                        pos.DOScale(Vector3.zero, 0.2f).OnComplete(() =>
                        {
                            var temp = Instantiate(prefab, pos.position, pos.rotation);
                            _playerController.AddToStack(temp);
                            pos.DOScale(Vector3.one, 0.2f).SetDelay(0.2f);
                        });
                        yield return new WaitForSeconds(0.5f);
                    }
                }
            }
        }
    }
}