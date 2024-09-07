namespace Baubit.Aggregation
{
    public static class TaskExtensions
    {
        public static void Wait(this Task task, bool ignoreTaskCancellationException = false)
        {
            try
            {
                task.Wait();
            }
            catch (AggregateException aExp)
            {
                if (aExp.InnerException is TaskCanceledException) 
                {
                    //ignore
                }
                else
                {
                    throw aExp;
                }
            }
        }
    }
}
