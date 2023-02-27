using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;
    [SerializeField] bool _useSmooth;
    [SerializeField] Animator _anim;
    [SerializeField] Rigidbody _rb;

    public float m_MoveSpeed = 4f;
    public float m_TurnMultiplier = 4f;

    Action _rotation, _movement;

    protected void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }

        //_anim = GetComponentInChildren<Animator>();
        //_rb = GetComponent<Rigidbody>();
        enabled = false;

        if (_useSmooth)
        {
            _rotation = HandleSmoothRotation;
            _movement = HandleSmoothMovement;
        }
        else
        {
            _rotation = HandleRotation;
            _movement = HandleMovement;
        }

        GameController.OnGameStart += PlayerControllerOnGameStart;
    }

    void OnDestroy()
    {
        GameController.OnGameStart -= PlayerControllerOnGameStart;
    }

    void PlayerControllerOnGameStart()
    {
        GameController.OnGameStart -= PlayerControllerOnGameStart;
        enabled = true;
    }

    void Update()
    {
        _rotation();
        _anim.SetBool("Move", InputManager.IsOn);
    }

    void FixedUpdate()
    {
        if (InputManager.IsOn)
        {
            _movement();
        }
        else
        {
            _rb.velocity = Vector3.zero;
        }
    }

    #region Movement

    void HandleMovement()
    {
        _rb.MovePosition(Move(m_MoveSpeed * Time.deltaTime));
    }

    void HandleSmoothMovement()
    {
        _rb.MovePosition(MoveForward(m_MoveSpeed * Time.deltaTime));
    }

    Vector3 Move(float factor)
    {
        Vector3 forward = Forward();
        Vector3 right = Right();
        Vector3 movePosition = Vector3.zero.With(x: forward.x + right.x, z: forward.z + right.z);
        movePosition.Normalize();

        movePosition.x *= factor;
        movePosition.y *= factor;
        movePosition.z *= factor;

        movePosition.x += transform.position.x;
        movePosition.y += transform.position.y;
        movePosition.z += transform.position.z;
        return movePosition;
    }

    float InputY()
    {
        if (InputManager.IsOn)
        {
            if (InputManager.InputY != 0f || InputManager.InputX != 0f) return 1f;
            else return 0f;
        }
        else return 0f;
    }


    Vector3 MoveForward(float factor)
    {
        Vector3 forward = transform.forward;
        forward.x *= InputY() * factor;
        forward.y *= InputY() * factor;
        forward.z *= InputY() * factor;

        Vector3 movePosition = transform.position;
        movePosition.x += forward.x;
        movePosition.y += forward.y;
        movePosition.z += forward.z;

        return movePosition;
    }

    Vector3 Forward()
    {
        Vector3 forward = Vector3.forward;
        forward.z *= InputManager.InputY;
        return forward;
    }

    Vector3 Right()
    {
        var right = Vector3.right;
        right.x *= InputManager.InputX;
        return right;
    }

    void HandleRotation()
    {
        Vector3 lookDir = Vector3.zero.With(x: InputManager.InputX, z: InputManager.InputY);
        if (lookDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDir);
            transform.rotation = targetRotation;
        }
    }

    void HandleSmoothRotation()
    {
        Vector3 lookDir = Vector3.zero.With(x: InputManager.InputX, z: InputManager.InputY);
        if (lookDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDir);
            float factor = m_TurnMultiplier * Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, factor);
        }
    }

    #endregion

    #region Stack

    public int maxStackCount = 4;
    public Transform stackPoint;
    public List<Collectables> allStackItems;

    public bool IsStackEmpty()
    {
        return allStackItems.Count.Equals(0);
    }

    public bool IsStackFull()
    {
        return allStackItems.Count.Equals(maxStackCount);
    }

    public Collectables LastFromStack()
    {
        return allStackItems.Count.Equals(0) ? null : allStackItems[^1];

        /*var temp = allStackItems.LastOrDefault();
        return temp;*/
    }

    public Collectables FindLast(Collectables collectables)
    {
        return allStackItems.Find(x => x.CompareTag(collectables.tag));
    }

    public void AddToStack(Collectables collectable)
    {
        collectable.transform.DOJump(stackPoint.position, 2, 1, 0.5f).OnComplete(() =>
        {
            Transform transform1;
            (transform1 = collectable.transform).SetParent(stackPoint);
            transform1.rotation = stackPoint.rotation;
        });
        allStackItems.Add(collectable);
        _anim.SetLayerWeight(1, allStackItems.Count.Equals(0) ? 0 : 1);
    }

    public Collectables RemoveFromLast(Collectables collectables, Transform stackTransform)
    {
        var temp = allStackItems.Find(x => x.CompareTag(collectables.tag));
        temp.transform.SetParent(null);
        temp.transform.DOJump(stackTransform.position, 2, 1, 0.5f)
            .OnComplete(() =>
            {
                Transform transform1;
                (transform1 = temp.transform).SetParent(stackTransform);
                transform1.rotation = stackTransform.rotation;
            });
        allStackItems.Remove(temp);
        if (allStackItems.Count == 0)
        {
            _anim.SetLayerWeight(1, 0);
        }

        return temp;
    }

    public List<Collectables> RemoveAll(Collectables collectables)
    {
        var temp = allStackItems.FindAll(x => x.CompareTag(collectables.tag));
        foreach (var t in temp)
        {
            allStackItems.Remove(t);
        }

        if (allStackItems.Count == 0)
        {
            _anim.SetLayerWeight(1, 0);
        }

        return temp;
    }

    public void SetAnimationWeight(int id, int weight)
    {
        _anim.SetLayerWeight(id, weight);
    }

    #endregion

    public GameObject showRoomCloth;
    public GameObject garageCloth;

    public void PlayerInShowroom()
    {
        garageCloth.Hide();
        showRoomCloth.Show();
    }

    public void PlayerInGarage()
    {
        showRoomCloth.Hide();
        garageCloth.Show();
    }
}