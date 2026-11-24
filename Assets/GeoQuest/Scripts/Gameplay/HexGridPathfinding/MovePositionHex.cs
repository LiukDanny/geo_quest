using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MovePositionHex : MonoBehaviour
{
    [SerializeField] private float reachedPositionDistance = 1f;
    [SerializeField] private MoveTransformVelocity moveTransform;
    [SerializeField] private Animator _animator;

    public CharacterState State { get; private set; } = CharacterState.Free;

    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int ViewParam = Animator.StringToHash("View");
    private static readonly int TurnParam = Animator.StringToHash("Turn");
    private static readonly int IsActing = Animator.StringToHash("IsActing");
    private const float SideSlope = 0.577f;
    private int _view;
    private int _moveId;
    private int _actId;

    private CancellationTokenSource _actionCts;
    private GridService _gridService;

    private List<Vector3> pathVectorList;
    private int pathIndex = -1;
    private Action onReachedMovePosition;
    private Vector3 prevVelocity = Vector3.zero;

    private static readonly Vector2Int[] FootprintOffsets =
{
    new Vector2Int(0, 0),
    new Vector2Int(0, -1),
    new Vector2Int(1, -1),
};//test values


    public void Init(GridService gridService) => _gridService = gridService;

    public async UniTask<ActionResult> RunAction(Func<CancellationToken, UniTask<ActionResult>> body)
    {
        _actionCts?.Cancel();

        var cts = new CancellationTokenSource();
        _actionCts = cts;
        State = CharacterState.Busy;

        ActionResult result;
        try
        {
            result = await body(cts.Token);
        }
        catch (OperationCanceledException)
        {
            result = ActionResult.Interrupted;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            result = ActionResult.Failed;
        }
        finally
        {
            if (_actionCts == cts)
            {
                _actionCts = null;
                State = CharacterState.Free;
            }
            cts.Dispose();
        }

        return result;
    }

    public async UniTask MoveTo(Vector3 movePosition, CancellationToken cancellationToken)
    {
        pathVectorList = _gridService.FindPath(transform.position, movePosition, CanStandHere);
        if (pathVectorList == null)
            return;

            var ax = _gridService.GetGrid().GetGridObject(transform.position);
        foreach (var offset in FootprintOffsets)
        {
            var node = _gridService.GetGrid().GetGridObject(ax.x + offset.x, ax.y + offset.y);
        }

        for (int i = 0; i < pathVectorList.Count - 1; i++)
        {
            Debug.DrawLine(pathVectorList[i], pathVectorList[i + 1], Color.green, 3f);
        }

        if (pathVectorList.Count > 0)
        {
            // Remove first position so he doesn't go backwards
            pathVectorList.RemoveAt(0);
        }
        if (pathVectorList.Count > 0)
        {
            pathIndex = 0;
        }
        else
        {
            pathIndex = -1;
        }

        var moveId = ++_moveId;
        try
        {
            Face(movePosition - transform.position);
            SetMoving(true);
            foreach (var nextPathPosition in pathVectorList)
            {
                await MoveToPoint(nextPathPosition, cancellationToken);
            }
        }
        finally
        {
            if (moveId == _moveId)
            {
                moveTransform.SetVelocity(Vector3.zero);
                SetMoving(false);
            }
        }
    }

    // private void Update()
    // {
    //     if (pathIndex != -1)
    //     {
    //         // Move to next path position
    //         Vector3 nextPathPosition = pathVectorList[pathIndex];
    //         Vector3 moveVelocity = (nextPathPosition - transform.position).normalized;
    //         moveTransform.SetVelocity(moveVelocity);
    //         prevVelocity = moveVelocity;
    //
    //         if (Vector3.Distance(transform.position, nextPathPosition) < reachedPositionDistance)
    //         {
    //             pathIndex++;
    //             if (pathIndex >= pathVectorList.Count)
    //             {
    //                 // End of path
    //                 pathIndex = -1;
    //                 onReachedMovePosition();
    //             }
    //         }
    //     }
    //     else if(prevVelocity != Vector3.zero)
    //     {
    //         // Idle
    //         moveTransform.SetVelocity(Vector3.zero);
    //     }
    //      
    // }
    
    private async UniTask MoveToPoint(Vector3 targetPoint, CancellationToken cancellationToken)
    {
        while (Vector3.Distance(transform.position, targetPoint) > reachedPositionDistance)
        {
            Vector3 dir = (targetPoint - transform.position).normalized;
            moveTransform.SetVelocity(dir);

            await UniTask.Yield(cancellationToken);
        }
    }

    private void SetMoving(bool moving)
    {
        if (_animator != null)
            _animator.SetBool(IsMoving, moving);
    }

    public async UniTask Act(float seconds, CancellationToken cancellationToken)
    {
        var actId = ++_actId;
        Debug.Log($"{name}: action start ({seconds}s)");
        if (_animator != null)
            _animator.SetBool(IsActing, true);
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(seconds), cancellationToken: cancellationToken);
        }
        finally
        {
            Debug.Log($"{name}: action end");
            if (actId == _actId && _animator != null)
                _animator.SetBool(IsActing, false);
        }
    }

    private void Face(Vector3 direction)
    {
        if (_animator == null)
            return;

        var view = Mathf.Abs(direction.y) > Mathf.Abs(direction.x) * SideSlope
            ? (direction.y > 0 ? 2 : 1)
            : 0;
        if (view != _view)
        {
            _view = view;
            _animator.SetInteger(ViewParam, view);
            _animator.SetTrigger(TurnParam);
        }

        if (Mathf.Abs(direction.x) < 0.01f)
            return;

        var scale = _animator.transform.localScale;
        scale.x = Mathf.Abs(scale.x) * Mathf.Sign(direction.x);
        _animator.transform.localScale = scale;
    }

    private bool CanStandHere(PathNode anchorNode)
    {
        foreach (var offset in FootprintOffsets)
        {
            int x = anchorNode.x + offset.x;
            int y = anchorNode.y + offset.y;

            PathNode node = _gridService.GetGrid().GetGridObject(x, y);

            if (node == null)
                return false;

            if (!node.IsAvailable())
                return false;

        }

        return true;
    }
}

