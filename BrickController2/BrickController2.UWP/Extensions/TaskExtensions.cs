using BrickController2.Helpers;
using System.Threading.Tasks;

namespace BrickController2.Windows.Extensions
{
    public static class TaskExtensions
    {
        public static void Forget(this Task task)
        {
            task.ContinueWith(
                        t => { Log.Error("The task has failed.", t.Exception); }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
