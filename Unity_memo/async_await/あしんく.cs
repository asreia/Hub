static async void AsyncTest()
{
    Console.WriteLine(DateTime.Now);  // 開始
    await Task.Delay(5000);
    Console.WriteLine(DateTime.Now);  // 終了
}

private static void AsyncTest()
{
    var stateMachine = new AsyncTestStateMachine();
    stateMachine.builder = AsyncVoidMethodBuilder.Create();
    stateMachine.state = -1;
    stateMachine.builder.Start(ref stateMachine);
}
private sealed struct AsyncTestStateMachine : IAsyncStateMachine {
    public int state;
    public AsyncVoidMethodBuilder builder;
    private TaskAwaiter taskAwaiter;
    private void MoveNext() {
        int num = state;
        try
        {
            TaskAwaiter awaiter;
            if (num != 0)
            {
                Console.WriteLine(DateTime.Now); //await 前
                awaiter = Task.Delay(5000).GetAwaiter(); //await
                if (!awaiter.IsCompleted) {
                    num = state = 0;
                    taskAwaiter = awaiter;
                    AsyncTestStateMachine stateMachine = this;
                    builder.AwaitUnsafeOnCompleted<TaskAwaiter, AsyncTestStateMachine>(ref awaiter, ref stateMachine);
                    return;
                }
            }
            else
            {
                awaiter = taskAwaiter;
                taskAwaiter = default(TaskAwaiter);
                num = state = -1;
            }
            awaiter.GetResult();
            Console.WriteLine(DateTime.Now); //await 後
        } 
        catch (Exception exception)
        {
            state = -2;
            builder.SetException(exception);
            return;
        }
        state = -2;
        builder.SetResult();
    }
}
