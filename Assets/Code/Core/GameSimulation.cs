using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// "Симуляція гри" — це місце, де застосовуються команди.
/// У правильній архітектурі мультиплеєра саме сервер робить симуляцію авторитетно.
/// Але для прототипу ми робимо локальну симуляцію (імітація сервера).
/// </summary>
public sealed class GameSimulation : MonoBehaviour
{
    // Черга команд (сюди інпут або мережа додають команди).
    private readonly CommandQueue commandQueue = new();

    // Швидкий доступ до юнітів по ID (щоб за O(1) знаходити потрібного).
    private readonly Dictionary<int, UnitMotor2D> unitsById = new();

    // Даємо доступ іншим скриптам (наприклад інпуту) додавати команди.
    public CommandQueue Commands => commandQueue;

    private void Awake()
    {
        // Реєструємо всі UnitMotor2D на сцені.
        // Потім це можна замінити на “spawn систему” від сервера.
        foreach (var motor in FindObjectsOfType<UnitMotor2D>())
        {
            // Кожен юніт повинен мати EntityId
            var id = motor.GetComponent<EntityId>();
            if (id == null)
            {
                Debug.LogError($"Unit {motor.name} has no EntityId");
                continue;
            }

            // Якщо два юніти матимуть один і той самий id — буде проблема.
            unitsById[id.Id] = motor;
        }
    }

    private void Update()
    {
        // Обробляємо всі команди, які накопичились за кадр.
        while (commandQueue.TryDequeue(out var cmd))
        {
            Apply(cmd);
        }
    }

    /// <summary>
    /// Застосування команди до гри.
    /// Тут буде рости логіка: attack, build, cast spell, etc.
    /// </summary>
    private void Apply(ICommand cmd)
    {
        // Якщо це команда руху — застосовуємо її.
        if (cmd is MoveCommand move)
        {
            if (unitsById.TryGetValue(move.EntityId, out var motor))
            {
                // Передаємо ціль мотору.
                motor.SetMoveTarget(move.Target);
            }
        }
    }
}
