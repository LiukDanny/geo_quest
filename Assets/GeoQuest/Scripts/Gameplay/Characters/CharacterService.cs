using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class CharacterService
{
    private readonly MovePositionHex _char1;
    private readonly MovePositionHex _char2;
    private readonly FollowConfig _followConfig;
    private readonly GridService _gridService;

    public MovePositionHex Active { get; private set; }

    public event Action<MovePositionHex> ActiveChanged;

    public CharacterService(MovePositionHex char1, MovePositionHex char2, FollowConfig followConfig, GridService gridService)
    {
        _char1 = char1;
        _char2 = char2;
        _followConfig = followConfig;
        _gridService = gridService;
    }

    public void SetActive(MovePositionHex character)
    {
        if (Active == character)
            return;

        Active = character;
        ActiveChanged?.Invoke(character);
    }

    public UniTask<ActionResult> MoveActive(Vector3 position)
    {
        MovePositionHex character = Active;
        return character.RunAction(ct => MoveWithFollow(character, position, ct));
    }

    private async UniTask<ActionResult> MoveWithFollow(MovePositionHex character, Vector3 position, CancellationToken ct)
    {
        using var followCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        if (_followConfig != null)
            FollowWhileMoving(followCts.Token).Forget();

        try
        {
            await character.MoveTo(position, ct);
        }
        finally
        {
            followCts.Cancel();
        }

        return ActionResult.Completed;
    }

    private MovePositionHex GetIdleChar()
    {
        return Active == _char1 ? _char2 : _char1;
    }

    private async UniTask FollowWhileMoving(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_followConfig.IntervalSeconds), cancellationToken: ct);

            if (!_followConfig.Enabled)
                continue;

            MovePositionHex idle = GetIdleChar();

            if (idle.State != CharacterState.Free)
                continue;

            MovePositionHex active = Active;
            int distance = _gridService.GetTileDistance(idle.transform.position, active.transform.position);

            if (distance <= _followConfig.MaxDistanceTiles)
                continue;

            Vector3 targetPoint = Utils.GetPointInRange(
                idle.transform.position,
                active.transform.position,
                _followConfig.MaxDistanceTiles - 1f);

            idle.RunAction(async idleCt =>
            {
                await idle.MoveTo(targetPoint, idleCt);
                return ActionResult.Completed;
            }).Forget();
        }
    }
}
