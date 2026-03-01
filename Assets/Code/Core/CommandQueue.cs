using System.Collections.Generic;

/// <summary>
/// Черга команд.
/// Поки що команди додає локальний інпут.
/// Потім заміниш джерело команд на мережу (з Java сервера),
/// але симуляція буде обробляти їх так само.
/// </summary>
public sealed class CommandQueue
{
    // Звичайна черга FIFO (першим прийшов — першим обробився).
    private readonly Queue<ICommand> queue = new();

    /// <summary>
    /// Додати команду в чергу.
    /// </summary>
    public void Enqueue(ICommand cmd) => queue.Enqueue(cmd);

    /// <summary>
    /// Спробувати взяти наступну команду.
    /// Якщо команд немає — повертаємо false.
    /// </summary>
    public bool TryDequeue(out ICommand cmd)
    {
        if (queue.Count > 0)
        {
            cmd = queue.Dequeue();
            return true;
        }

        cmd = null;
        return false;
    }
}