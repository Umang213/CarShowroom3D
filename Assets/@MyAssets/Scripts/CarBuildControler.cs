using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using EasyButtons;
using UnityEngine;
using UnityEngine.Events;

public class CarBuildControler : MonoBehaviour
{
    public static CarBuildControler instance;
    public Transform carInstantiatePoint;
    public ParticleSystem smoke;
    public List<Car> allCars;
    public Transform[] processPoint;
    public MeshRenderer conveyorBelt;

    [Header("AddWheel")] public RoboHand roboHand;
    public RoboHand roboHand2;
    public List<WheelPoint> WheelPoints;
    public int fixTyreCount;

    [Header("AddEngine")] public RoboHand engineRoboHand;
    public WheelPoint enginePoint;
    public int fixEngineCount;

    [Header("AddColor")] public Animator colorRoboHand;
    public Transform colorSprayPoint;

    [Header("AddBody")] public RoboHand dBodyRoboHand;
    public Transform dBodyPoint;

    [Header("Collectable")] public Collectables wheel;
    public Collectables engine;
    public Collectables color;

    [Header("Add Trigger")] public GameObject buildCar;
    public GameObject addWheel;
    public GameObject addEngine;
    public GameObject addBody;
    public GameObject addColor;

    [Header("Show_Room_Point")] public List<CarPoint> showroomCarPoint;

    public Car currentCar;
    public CarPoint currentCarPoint;

    PlayerController _player;
    CustomerManager _customerManager;
    TutorialControler _tutorialControler;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }

        //LoadData();
    }

    private void Start()
    {
        _player = PlayerController.instance;
        _customerManager = CustomerManager.instance;
        _tutorialControler = TutorialControler.Instance;
        if (PlayerPrefs.GetInt(PlayerPrefsKey.TutorialCount, 0) == 0)
        {
            _tutorialControler.targetPoint = buildCar.transform;
        }
    }

    [Button]
    public void BuildCar()
    {
        var carPoint = showroomCarPoint.Find(x => x.car == null);
        currentCarPoint = carPoint;
        if (carPoint)
        {
            buildCar.Hide();
            if (PlayerPrefs.GetInt(PlayerPrefsKey.TutorialCount, 0).Equals(0))
            {
                //_tutorialControler.tutorialEvent ??= new UnityEvent();
                _tutorialControler.tutorialEvent.AddListener(_tutorialControler.CheckForAddWheel);
            }

            //var count = PlayerPrefs.GetInt(PlayerPrefsKey.CarBuildIndex, 0);
            currentCar = Instantiate(allCars[Helper.RandomInt(0, allCars.Count)], carInstantiatePoint.position,
                carInstantiatePoint.rotation);
            //carPoint.car = currentCar;
            buildCar.Hide();
            currentCar.Show();
            currentCar.transform.DOScale(Vector3.one, 0.5f).From(Vector3.zero).SetEase(Ease.OutBack).OnComplete(() =>
            {
                conveyorBelt.materials[1].DOOffset(new Vector2(0, 1f), 2).SetEase(Ease.Linear).From(Vector2.zero);
                conveyorBelt.materials[1].DOOffset(new Vector2(0, 1f), "_BumpMap", 2).SetEase(Ease.Linear)
                    .From(Vector2.zero);
                currentCar.transform.DOMove(processPoint[0].position, 2).SetEase(Ease.Linear)
                    .OnComplete(addWheel.Show);
            });
        }
        else
        {
            Debug.Log("No Space Available");
        }
    }

    public void AddWheel()
    {
        for (var i = 0; i < 4; i++)
        {
            var index = i;
            if (WheelPoints[index].wheel == null)
            {
                var pos = WheelPoints[index].stackPoint;
                var temp = _player.RemoveFromLast(wheel, pos);
                if (temp != null)
                {
                    WheelPoints[i].wheel = temp;
                }
            }
        }

        var count = WheelPoints.FindAll(x => x.wheel == null).Count;
        if (count.Equals(0)) _tutorialControler.RemoveTarget();

        CodeMonkey.Utils.FunctionTimer.Create(() =>
        {
            roboHand.AddTyre(this);
            roboHand2.AddTyre(this);
        }, 1);
    }

    public void CheckTyreIsFix()
    {
        if (fixTyreCount.Equals(4))
        {
            addWheel.Hide();
            fixTyreCount = 0;
            if (PlayerPrefs.GetInt(PlayerPrefsKey.TutorialCount, 0).Equals(0))
            {
                NextPoint(2, addEngine,
                    () => { _tutorialControler.tutorialEvent.AddListener(_tutorialControler.CheckForAddEngine); });
            }
            else
            {
                NextPoint(2, addEngine);
            }
        }
    }

    public void AddEngine()
    {
        if (enginePoint.wheel == null)
        {
            var pos = enginePoint.stackPoint;
            var temp = _player.RemoveFromLast(engine, pos);
            if (temp != null)
            {
                _tutorialControler.RemoveTarget();
                addEngine.Hide();
                enginePoint.wheel = temp;
            }
        }

        CodeMonkey.Utils.FunctionTimer.Create(() => { engineRoboHand.AddEngine(this); }, 1);
    }

    public void CheckEngineIsFix()
    {
        if (fixEngineCount.Equals(1))
        {
            fixEngineCount = 0;
            if (PlayerPrefs.GetInt(PlayerPrefsKey.TutorialCount, 0).Equals(0))
            {
                NextPoint(3, addBody, () => _tutorialControler.targetPoint = addBody.transform);
            }
            else
            {
                NextPoint(3, addBody);
            }
        }
    }

    public void AddBody()
    {
        _tutorialControler.RemoveTarget();
        //var count = PlayerPrefs.GetInt(PlayerPrefsKey.CarBuildIndex, 0);
        var car = currentCar.defaultBody;
        car.transform.position = dBodyPoint.position;
        car.transform.rotation = dBodyPoint.rotation;

        car.transform.DOScale(Vector3.one, 0.5f).From(Vector3.zero).SetEase(Ease.OutBack).OnComplete(() =>
        {
            addBody.Hide();
            dBodyRoboHand.AddBody(this);
        });
    }

    public void AfterAddingBody()
    {
        if (PlayerPrefs.GetInt(PlayerPrefsKey.TutorialCount, 0).Equals(0))
        {
            NextPoint(4, addColor,
                () => _tutorialControler.tutorialEvent.AddListener(_tutorialControler.CheckForAddColor));
        }
        else
        {
            NextPoint(4, addColor);
        }
    }

    public void AddColor()
    {
        StartCoroutine(StartAddColor());
    }

    IEnumerator StartAddColor()
    {
        var player = PlayerController.instance;
        //var count = PlayerPrefs.GetInt(PlayerPrefsKey.CarBuildIndex, 0);
        var body = currentCar.body;
        var temp = player.RemoveAll(color);
        if (temp.Any())
        {
            _tutorialControler.RemoveTarget();
            PlayerPrefs.SetInt(PlayerPrefsKey.TutorialCount, 1);
            smoke.transform.position = body.transform.position.With(y: body.transform.position.y - 0.5f);
            colorRoboHand.SetBool("Color", true);

            var transform1 = currentCar.carColor.transform;
            transform1.position = colorSprayPoint.position;
            transform1.rotation = colorSprayPoint.rotation;
            transform1.SetParent(colorSprayPoint);

            //currentCar.carColor.Show();
            currentCar.carColor.Play();
            for (var i = 0; i < temp.Count; i++)
            {
                var i1 = i;
                temp[i1].transform.SetParent(null);
                temp[i1].transform.DOJump(body.transform.position, 2, 1, 0.5f)
                    .OnComplete(() =>
                    {
                        smoke.Play();
                        temp[i1].transform.SetParent(body.transform);
                        temp[i1].gameObject.Hide();
                    });
                smoke.Play();
                yield return new WaitForSeconds(0.3f);
                smoke.Play();
                yield return new WaitForSeconds(0.3f);
                smoke.Play();
            }

            yield return new WaitForSeconds(1.5f);
            currentCar.defaultBody.Hide();
            body.Show();
            smoke.Stop();
            addColor.Hide();
            yield return new WaitForSeconds(2f);
            colorRoboHand.SetBool("Color", false);
            currentCar.carColor.Stop();
            currentCar.carColor.Hide();
            AddInShowRoom();
        }
    }

    private void NextPoint(int point, GameObject showGameObject, Action action = null)
    {
        CodeMonkey.Utils.FunctionTimer.Create(() =>
        {
            var temp = point * 1.5f;
            conveyorBelt.materials[1].DOOffset(new Vector2(0, temp), 3).SetEase(Ease.Linear);
            conveyorBelt.materials[1].DOOffset(new Vector2(0, temp), "_BumpMap", 2).SetEase(Ease.Linear);
            currentCar.transform.DOMove(processPoint[point - 1].position, 3f)
                .SetEase(Ease.Linear).OnComplete(() =>
                {
                    showGameObject.Show();
                    action?.Invoke();
                });
        }, 2);
    }

    private void AddInShowRoom()
    {
        CodeMonkey.Utils.FunctionTimer.Create(() =>
        {
            //var count = PlayerPrefs.GetInt(PlayerPrefsKey.CarBuildIndex, 0);
            var car = currentCar;
            car.transform.DOScale(Vector3.zero, 1.8f).SetDelay(1);
            car.transform.DOMove(processPoint[4].position, 2).SetEase(Ease.Linear).OnComplete(() =>
            {
                var transform1 = car.transform;
                transform1.position = currentCarPoint.transform.position;
                transform1.rotation = currentCarPoint.transform.rotation;
                car.transform.SetParent(currentCarPoint.transform);
                transform1.DOScale(Vector3.one, 1).SetEase(Ease.Linear);
                currentCarPoint.transform.DORotate(new Vector3(0, 180, 0), 10, RotateMode.FastBeyond360)
                    .SetLoops(-1, LoopType.Incremental).SetEase(Ease.Linear);
                currentCarPoint.car = car;
                _customerManager.instanceSpawing();
            });
            //PlayerPrefs.SetInt(PlayerPrefsKey.CarBuildIndex, count + 1);
            buildCar.Show();
        }, 2);
    }

    public void PurchaseCar(Customer customer)
    {
        customer.ShowHappyEmoji();
        _customerManager.allCustomer.Remove(customer);
        customer.purchaseCar = customer.carPoint.car;
        customer.purchaseCar.transform.SetParent(null);
        customer.carPoint.moneyStacker.GiveMoney(customer.transform, 10);
        var carPoint = customer.carPoint;
        var pos = carPoint.transform.position;
        carPoint.car.transform.rotation = carPoint.exitPoint.rotation;
        carPoint.transform.parent.DOMove(pos.With(y: -0.26f), 0.5f)
            .OnComplete((() =>
            {
                customer.Hide();
                carPoint.shutter.transform.DOScaleY(0, 0.5f).SetDelay(0.5f);
                customer.purchaseCar.transform.DOMove(carPoint.exitPoint.position, 5).OnComplete((() =>
                {
                    // customer.purchaseCar.transform.rotation = carPoint.exitPoint.transform.rotation;
                    customer.purchaseCar.navMeshAgent.enabled = true;
                    customer.purchaseCar.navMeshAgent.SetDestination(carPoint.pathEndPoint.position);
                    Destroy(customer.gameObject);
                    //carPoint.path.gameObject.Show();
                    //customer.purchaseCar.transform.SetParent(carPoint.path.transform);
                }));
                carPoint.shutter.transform.DOScaleY(1, 0.5f).SetDelay(6);
                carPoint.transform.parent.DOMove(carPoint.realPos, 0.5f).SetDelay(2f);
            }));
        carPoint.car = null;
        _customerManager.CheckForExit();
    }

    /*private void LoadData()
    {
        var count = PlayerPrefs.GetInt(PlayerPrefsKey.CarBuildIndex, 0);
        for (var i = 0; i < count; i++)
        {
            allCars[i].transform.localScale = Vector3.one;
            var temp = allCars[i].rightSideWheelPoint[0];
            var wheel1 = Instantiate(wheel, temp.position, temp.rotation);
            wheel1.transform.SetParent(temp);
            var temp2 = allCars[i].rightSideWheelPoint[1];
            var wheel2 = Instantiate(wheel, temp2.position, temp2.rotation);
            wheel2.transform.SetParent(temp);
            var temp3 = allCars[i].leftSideWheelPoint[0];
            var wheel3 = Instantiate(wheel, temp3.position, temp3.rotation);
            wheel3.transform.SetParent(temp);
            var temp4 = allCars[i].leftSideWheelPoint[1];
            var wheel4 = Instantiate(wheel, temp4.position, temp4.rotation);
            wheel4.transform.SetParent(temp);
            allCars[i].body.Show();
            allCars[i].transform.position = showroomCarPoint[i].transform.position;
            allCars[i].transform.rotation = showroomCarPoint[i].transform.rotation;
            allCars[i].transform.SetParent(showroomCarPoint[i].transform);
            showroomCarPoint[i].transform.DORotate(new Vector3(0, 180, 0), 10, RotateMode.FastBeyond360)
                .SetLoops(-1, LoopType.Incremental).SetEase(Ease.Linear);
        }
    }*/

    public void UnlockCar(Car car)
    {
        allCars.Add(car);
    }

    public void UnlockCarStand(CarPoint carPoint)
    {
        showroomCarPoint.Add(carPoint);
    }
}