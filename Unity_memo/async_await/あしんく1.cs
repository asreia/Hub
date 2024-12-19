class cls<T>
{
    Task<T> task;
    bool Trigger{get{return true;}}

    // 非同期メソッドの定義
    async Task<T> TaskFunc()
    {
        // 非同期処理のシミュレーション (例: 1秒待機)
        await Task.Delay(1000); 
        // 例として、仮の値を返す
        return default(T); // ここを適切な値に置き換える
    }

    // はい、そのような処理は可能です。Task<T>をキャッシュして、特定のタイミングでawaitし、
      // 非同期処理の結果を取得するというパターンは、C#のTaskおよびasync/awaitで自然に実現できます。
    async void Update_0()
    {
        // Task<T> をキャッシュ
        if (task == null) task = TaskFunc();

        // 特定のタイミング（Triggerが真の場合）でawait
        if (Trigger)
        {
            T result = await task; // 結果を取得
            // 結果を処理
            Console.WriteLine($"Task Result: {result}");
        }
    }
    void Update_1()
    {
        // Task<T> をキャッシュ（最初に1度だけ作成）
        if (task == null) task = TaskFunc();

        // タスクが完了していれば結果を取得
        if (task.IsCompleted) //タスクが、IsFaulted:失敗(例外), IsCanceled:キャンセル
        {
            // awaitを使わずに値を取得
            T result = task.Result; // Resultプロパティを使うとき、タスクがまだ完了していない場合には、同期的にブロックされます。
            Console.WriteLine($"Task Result: {result}");
        }
    }
}

//タスクとは-----------------------------------------------------------------------------------------------------------------------------------------------
//タスクは抽象化すると関数(射)(特にRun(()=>{..}))に対して、
  //タスクの生成, 非同期実行, コンテキスト指定(newタスクと継続タスクのみ(既存タスク(DelayとかRun)はできない)), 戻り値, キャンセル, タスクの状態 をサポートするもの
//☆Task-like(Awaitableパターン)---------------------------------------------------------------------------------------------------------------------------
public ⟪class¦struct⟫ MyTaskLike
{
    public MyAwaiter GetAwaiter() => new MyAwaiter();
}

public ⟪class¦struct⟫ MyAwaiter＠❰<T>❱ : System.Runtime.CompilerServices.INotifyCompletion
{
    private readonly Task＠❰<T>❱ task;

    public bool IsCompleted => task.IsCompleted; //非同期処理が完了したかのフラグ

    //☆awaitで、
    //IsCompleted == trueの場合は、"async関数"がそのまま同期的に処理を続け、 (OnCompleted(..)は呼び出されない)
    //IsCompleted == falseになる場合は、"async関数"がOnCompleted(..)で"Task＠❰-like❱"へ"continuation"(継続するためのコールバック(await以降の処理が入っている))を登録し、
    //Task＠❰-like❱の非同期処理の完了で、"continuation"を"Task＠❰-like❱"が登録時に決められた場所で、現在のawait以降の処理が、
      //⟪デフォルト:スレッドプール¦SynchronizationContext.Current:現在のスレッドの同期コンテキスト¦ExecuteSynchronously:Task＠❰-like❱の非同期処理の上⟫ で継続される。
    //つまり、OnCompletedで"continuation"を登録することによって、タスクとタスクをコールバックによってシームレスに繋いでいる。
    //なお、GetResult()はawaitの直後(Task＠❰-like❱の非同期処理の完了直後)に実行され後続の処理に使われる。(IsCompleted == falseの場合は、continuationの中の最初で実行される)
    public void OnCompleted(Action continuation){task.ContinueWith(_ => continuation());} //Task-likeで独自に実装している場合は、ContinueWith(..)を自前で作る必要がある
    // SynchronizationContext.Current は、現在のスレッドでアプリ側(UnityやWPFやASP.NETなど..)で設定されたコンテキストを指し、await後(非同期処理後)にどのスレッドで処理を再開するか制御します。
        // [ThreadStatic] SynchronizationContext Current <= new UnitySynchronizationContext() //のように設定されている。([ThreadStatic] は各スレッド独立の static 変数)
        // await task.ConfigureAwait(false) または Current == null の場合、SynchronizationContext を使用せず、スレッドプールで continuation を実行します。
          //(task <=> task.ConfigureAwait(true))
    public void OnCompleted(Action continuation)
    {
        var context = SynchronizationContext.Current;
        if (context != null)
        {
            task.ContinueWith(_ =>
                context.Post(_ => continuation(), null)
            );
        }
        else
        {
            task.ContinueWith(_ => continuation());
        }
    }

    public ⟪T¦void⟫ GetResult() {＠❰return task.Result;❱} //非同期処理の結果(continuationの中の最初で実行される)
}

MyTaskLike myTaskLike = new MyTaskLike();
＠❰T =❱ await myTaskLike; //IsCompleted で最初に完了をチェックし、

//☆Task Task.ContinueWith(Task=>{..})-------------------------------------------------------------------------------------------------------------------
Task<TResult> ContinueWith<TResult>(Func<Task, TResult> continuationFunction); //Task<T>はTaskを継承
t0.ContinueWith(t0 => f1(t0.Result)).ContinueWith(t1_t0 => f2(t1_t0.Result)); //t1_t0 <=> t1(t0)
  //t0 -> t1(t0), t1(t0) -> t2(t1(t0)) //->:.ContinueWith(継続関数), 継続タスク:tn(t..(t1(t0)))
  //t0.Start() => t1(t0).Start(): t0が完了したらt1(t0).Start()し、t1(t0)が完了したらt2(t1(t0)).Start()する
  //t0 -> t1(t0):
    タスク0.ContinueWith(継続関数1) //継続関数1をタスク化(継続タスク1)してタスク0から間接的にcallされる
  //t0.Start() => t1(t0).Start():
    タスク0.Start() => 継続タスク1.Start(){継続関数1(); IsCompleted = true; AddTaskContextQueue(継続タスク2);} //AddTaskContextQueue:通常はスレッドプール

//☆Taskとasync/awaitについてふんわりと理解したが、確実に理解するにはTaskの詳細とasync/await関数の逆コンパイル解析が必要(追記:大体理解した)-----------------------
    //別スレッドに投げっぱなしなら、難しくないが、投げた後にスレッド間のやり取りや完了のチェックが発生すると複雑になる? (投げた後のスレッドの管理)

//以下最初に書いたから信用度低いかも===================

//並列にタスクを実行してからawaitでまとめて待機-------------------------------------------------------------------------------------------------------------
var task1 = Task1();
var task2 = Task2();
await Task.WhenAll(task1, task2);
//タスク(task) と async/await の基本的な原理---------------------------------------------------------------------------------------------------------------
//タスク(task)
    Task task = new Task(() => {～}); //Taskの作成
    task.Start(); //Taskの非同期での実行 (別仮想スレッドでTaskを実行)
//async/await (awaitは、同期的に非同期処理を続行しつつ、呼び出し元との関係を非同期にする)
    T = await task; //さらに別仮想スレッドを使い(追記:間違い)、そこでTaskの完了を待ち(await)、戻り値(T)を返す。(呼び出し元をブロックしない)
//new.Run(..) と new Task(..)----------------------------------------------------------------------------------------------------------------------------
//new.Run(..)
    // task0は、実行中の状態 (task0.Status == Running) (｡Taskの作成(new Task(..))と実行(Task.Start())を行い(new Task(..).Start())、そしてTaskを返す｡)
    Task<int> task0 = Task.Run(() => {return 0;});
//new Task(..)
    // task1は、まだ実行されていない状態 (task1.Status == Created)
    Task task1 = new Task(() => Console.WriteLine("Task is running"));
    task1.Start(); //(task1.Status == Running) (.Start()は戻り値がvoid)
// Taskの主な状態-----------------------------------------------------------------------------------------------------------------------------------------
    // Created: Taskが作成されたが、まだ開始されていない状態（Start()メソッドで開始されていない場合など）。
    // WaitingForActivation（未実行）: タスクが作成されたが、まだ実行が開始されていない状態。タスクの実行を待っています。
    // WaitingToRun: タスクがスケジュールされているが、まだ実行が開始されていない状態（例えば、スレッドプールでの実行を待っている状態）。
    // Running（実行中）: タスクが実行されている状態。タスクが現在処理中であることを示します。
    // RanToCompletion（実行完了）: タスクが正常に完了した状態。例外やキャンセルなしに終了しています。
// その他の状態
    // Canceled: タスクがキャンセルされた状態。
    // Faulted: タスクが例外によって失敗した状態。

//☆基本的なasync/awaitの動作。(待ち(await)が発生するコードで、普段通りコードを書け、外部では非同期になるようにできる)--------------------------------------------
async Task<bool> DoSomethingAsync() //async/awaitは、非同期(Task)の完了を待つ(await)ことによって、呼び出し元をブロックせず、同期的な処理を行う仕組み
{
    Console.WriteLine("immediate execution") //☆最初のawaitに到達するまでの処理は即時で同期的に実行される
    await Task.Delay(1000);  // >1秒待機 //☆非同期(Task)の完了を同期的に待つ(await)と同時に、呼び出し元(awaitが無ければ)とはここから非同期になる
    Console.WriteLine("Done"); //☆await後は、スレッドが変わりうる。(SynchronizationContextで挙動を設定?)
                                // >デフォルトでは、awaitの後に同じスレッドに戻ることが多い。
                                // >ConfigureAwait(false)を使うと、await後に異なるスレッドで処理が再開される可能性が高くなる。
    return true; //Task<T>の場合、戻り値(T)も返せる
}

//☆asyncの付いた関数を実行すると、非同期で実行
var task = DoSomethingAsync(); //即時にTaskが返される // >ここではまだ "Done" は表示されない

async Task ExecuteAsync() //awaitを含む関数にはasyncが必須
{
    //☆awaitで、asyncの付いた関数を実行すると、非同期の完了を待つ
    bool b = await DoSomethingAsync(); //☆戻り値がある場合(Task<T>)は、戻り値(T)も受け取れる // >ここで1秒待機してから "Done" が表示される
}
