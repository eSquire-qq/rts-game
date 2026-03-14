using UnityEngine;

/// <summary>
/// Базовий інтерфейс команди.
/// Команди — це ключ до мультиплеєра.
/// Клієнт надсилає команди (move/attack/build), сервер застосовує і розсилає результат.
/// </summary>
public interface ICommand
{
    // До якої сутності (юніта/будівлі) відноситься команда.
    int EntityId { get; }
}

/// <summary>
/// Команда "перемістити юніта в точку".
/// Це не "логіка руху" — це тільки дані (ID + Target).
/// </summary>
public readonly struct MoveCommand : ICommand
{
    public int EntityId { get; }
    public Vector2 Target { get; }

    public MoveCommand(int entityId, Vector2 target)
    {
        EntityId = entityId;
        Target = target;
    }
}

public readonly struct AttackCommand : ICommand
{
    public int EntityId { get; }
    public int TargetId { get; }

    public AttackCommand(int entityId, int targetId)
    {
        EntityId = entityId;
        TargetId = targetId;
    }
}