using System.Threading.Tasks;

namespace maliasmgr.Extensions
{
    /// <summary>
    /// Extension for handling async Tasks
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Wait for finishing a task and return the result
        /// </summary>
        /// <param name="task">A Task</param>
        /// <typeparam name="T">The resolt type of the Task</typeparam>
        /// <returns>The result</returns>
        public static T WaitForValue<T>(this Task<T> task)
        {
            task.Wait();
            return task.Result;
        }
    }
}
