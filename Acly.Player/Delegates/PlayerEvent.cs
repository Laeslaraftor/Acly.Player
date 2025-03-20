namespace Acly.Player
{
    /// <summary>
    /// Событие плеера
    /// </summary>
    /// <param name="Player">Плеер, вызвавший событие</param>
    public delegate void PlayerEvent(IPlayer Player);
    /// <summary>
    /// Событие плеера
    /// </summary>
    /// <param name="Player">Плеер, вызвавший событие</param>
    /// <param name="Value">Время</param>
    public delegate void PlayerTimeEvent(IPlayer Player, TimeSpan Value);
}
