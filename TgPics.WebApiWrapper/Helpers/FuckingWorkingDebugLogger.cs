namespace TgPics.WebApiWrapper.Helpers;

using HttpTracer.Logger;
using System.Diagnostics;

/*
 * Я, блять, не понимаю, почему ебаный стандартный логгер из библиотеки
 * нахуй не работает. Я убил на это 4 ебаных часа блять. Сука!!!
 * 
 * Пришлось.. делать так.
 */
public class FuckingWorkingDebugLogger : ILogger
{
    /// <summary>
    /// Выводит сраные сообщения в обоссанное окно дебага, ебись оно в рот.
    /// </summary>
    /// <param name="aFuckingMessage"><see cref="HttpTracer"/> Сообщение хуйни</param>
    public void Log(string aFuckingMessage) => Debug.WriteLine(aFuckingMessage);
}