using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

/*
    IJob
        IJobを継承した構造体を定義しvoid Execute()を実装する。各WorkerThreadはこの構造体をコピー?しExecute()を実行する。
            https://youtu.be/rvBpUPFN5_I?list=PLtjAIRnny3h7KDDpkrsEEnILtEQLwOHiC&t=688
            コピーなのでIJob~構造体内の非NativeContainer型は各WorkerThreadで独立して共有していない。
        C#管理外で扱われ実行(Execute)される?ので管理された型(managed型)を使えない?
            Burstでなくても参照型はJobに含めない
                InvalidOperationException：Job6.clsは値型ではありません。 ジョブ構造体には、参照型を含めることはできません。
    IJobExtension
        JobHandle Schedule(int JobHandle): Jobが計画された型(JobHandle)を受け取りJobが計画された型を返す。
                                        (MainThreadも使われる事もある(MainThread暇になってしまうし、モバイルだとMainThreadの方が性能が良いコアの可能性が高いから?))
                                                                                    あと、JobHandleの静的フィールドのList<JobHandle>に登録される?
                                        同じインスタンスと引数で何回呼んでも別々にScheduleし別々のJobHandleとなる
                                        nextJobHandle = job.Schedule(prevJobHandle)と言う構造上、Scheduleが逐次実行的でJobHandleの依存関係がループすることがない
        void Run(int): IJob繋げず(JobHandleを使わず)に即時実行し完了を待つ(メインスレッドで実行される)。Burstの恩恵を受けれる
                    //●job3.Run(10); //IJobParallelFor(Job3)でもinnerloopBatchCountの指定ができず、全てMainThreadで実行される。
        JobHandle ScheduleParallel(int arrayLength, int innerloopBatchCount, JobHandle dependency) IJobParallelForと同じ?なのでIJobParallelForいらない?
            // innerloopBatchCountは、1スレッドあたりExecute(int index)のindexをinnerloopBatchCount連続で処理する
            // arrayLengthは、Execute(int index)のindexを0~arrayLengthまで処理する
                // arrayLength = 8, innerloopBatchCount = 3 の場合 [スレッド1]:0,1,2 [スレッド2]:3,4,5 [スレッド3]:6,7 (ScheduleParallel(8, 3, jobHandle))

    IJobParallelFor
        Execute(int index)のindex以外の要素にアクセスするとIndexOutOfRangeException~ReadWriteBuffer.がでる。
        [ReadOnly]を付けると出なくなるが読み込み専用となる。(書き込もうとすると実行時InvalidOperationException)
    [ReadOnly]は複数のJob(IJob~)から同時にアクセスする事ができる事を保証する。それと↑のことも踏まえると、
        [ReadOnly]は複数のWorkerThreadから同時に安全にアクセスする事ができる事を保証する。(追記: 並列に複数のJobのメンバに存在する時、Parallelでindex以外にアクセスする時は[ReadOnly]がいる)
        依存関係が結ばれた複数のJobは同じNativeContainerに対して複数のWorkerThreadから同時にアクセスする所は[ReadOnly]を付けて、そうでない所は付けない事ができる。
            job1 [ReadOnly] -> job3 Non[ReadOnly]
            job2 [ReadOnly] ->
    JobHandle(ソースを覗いた所IntPtr jobGroup int versionしかなくIntPtrとNativeMethodの闇に葬られる)
        スタティック
            JobHandle CombineDependencies(JobHandel,JobHandle,,): 複数のJobHandleをまとめて一つのJobHandleを返す(複数のJobHandleをList<JobHandle>に登録したJobHandleを返す?)
            void ScheduleBatchedJobs: 静的フィールドのList<JobHandle>を全て実行(IsCompleteをtrue)する?しかしComplete()は実行しない
                                      //●ScheduleBatchedJobsは多分全ての種類のIJob~ですぐにScheduleされたJobを実行する(Schedule前のJobは実行しない)
                                            これを呼ばなくてもBehaviourUpdateの後でJobが実行していた
                                      ScheduleBatchedJobsを何回呼んでもエラーは無い
            bool CheckFenceIsDependencyOrDidSyncFence(j0Handle, j1Handle): j1Handleはj0Handleに依存しているか?(j1Handle = job.Schedule(,j0Handle)) または
                                                                                j0Handleは完了しているか?(j0Handle.Complete())
                                                                           //●CheckFenceIsDependencyOrDidSyncFenceはIsCompleteに無関係で
                                                                                j0Handleとj1Handleの依存関係が直近でなく間が空いていても有効、
                                                                                自分自身はtrueになる(jobHandle0, jobHandle0)
                                                                           j1Handleの中のJobHandleフィールドメンバを再帰的に辿ってj0Handleに辿り着けるか?
                                                                           trueならばj1Handle.Complete()で両方を実行するまたはしていた
                                            //●defaultはどのJobHandleにも依存を持たないので↓(IsJobHandle_Complete)は
                                                jobHandleがJobHandle.Complete()を実行したかどうかだけ分かる(defaultは実行した扱いらしい?..)
                                            */
                                            public static class JobHandleExtention{
                                                public static bool IsJobHandle_Complete(this JobHandle jobHandle)
                                                    => JobHandle.CheckFenceIsDependencyOrDidSyncFence(jobHandle, default);
                                                public enum Ternary{True, False, Unknown}
                                                public static Ternary CheckFenceIsDependency(this JobHandle jobHandle, JobHandle dependsOn){
                                                    return 
                                                        IsJobHandle_Complete(dependsOn)?
                                                            dependsOn.Equals(jobHandle)?
                                                                Ternary.True
                                                                : Ternary.Unknown
                                                            : JobHandle.CheckFenceIsDependencyOrDidSyncFence(dependsOn, jobHandle)?
                                                                Ternary.True
                                                                : Ternary.False;
                                                }
                                            }
                                            /*

        インスタンス
            //●JobHandle jobHandle = default;でJobHandleの単位元を表現できる?ループの初期値に使える
            //●直列 -(引数渡し)-> 並列 -(Combine->引数渡し)-> 直列 でIJobForとIJobparallelForを混ぜてもJobが正しく実行される事を確認した
            //JobHandleはIJob系の構造体をコピーして持っている?
            //JobHandleはList<JobHandle>を持っている?(Disposeが実装されてないのでNativeArrayではない?)
            void Complete(): JobHandleのList<JobHandle>を底から深さ探索的に、IJobを各スレッドで実行しIsCompleteをtrueにしていく?
                             一度呼んだら(IsCompleteがtrue)また実行(Execute)されることはない
                             ScheduleしてからCompleteを呼ばなくてもエラーにならない、Completeを何回呼んでもエラーは無い
                             IsCompleteはComplete()の実行によってtrueになる訳では無い!!IsCompleteはただJobが実行されればtrueになります。
                             Complete()はJobの実行が終わったあとNativeContainerにMainThreadから安全にアクセスするために実行する。そして最後にMainThreadからDispose()で必ず開放する。
                             つまりComplete()は、 [Jobの実行(IsCompleteをtrue)] -> [MainThreadからNativeContainerへのアクセスを許可] をする。
                             Complete()をする前にNativeContainerにアスセスするとInvalidOperationExceptionになる。Dispose()も同じ
                             Dispose()をしないとA Native Collection has not been disposed,~になる
                             NativeContainerの使う流れは、 [Complete()] -> [NativeContainerへアスセス] -> [Dispose()] となる。
                             ([new IJob~()] -> [Schedule()] -> [ScheduleBatchedJobs()] -> [Complete()] -> [Result] -> [Dispose()])
            bool IsComplete{get;}: Scheduleされてから実行されたか?(追記:完了したか?)しかしComplete()は実行していない場合も含める(defaultまたはnew JobHandle()した場合はtrue)
                                //●IsCompleteはScheduleBatchedJobsと同じ挙動をしているので中で実行している?←の2つとCompleteを呼ばなくてもBehaviourUpdateの後でJobが実行していた

    プロファイラ操作
        フォーカスしているフレームの前後合わせて8フレーム内かつ画面表示内に処理の起点が入っていないと表示されない
        //あと、プロファイラーのタイムラインの横スクロールの操作性が悪すぎる。//追記 マウスのMMBで操作できる
        if(jobData.jobHandle2.IsCompleted!) return; 
        //jobData.jobHandle2.IsCompleted!ってなんだ？ｗ null許容演算子でコンパイラにWarning CS8602: ヌル参照の可能性のある間接参照。を抑制するもの(v8.0)

    Script Debugging https://qiita.com/sawasaka/items/47d480f55ffde0448876
        launch.jsonファイルを作成します。を押す (デバッカーを間違えた場合は.vscode/launch.jsonを消す)
        [▷ Unity Editor]の▷を押す -> Unityの▷を押す。　Unityと繋がりデバッガーがアタッチされます。
        並列処理されるメソッドにブレークを置くと調子が悪くなる
        続行ボタンでブレーク置きに実行が安定している。
*/

public class CSharpJobSystemSummary : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    JobHandle j1Handle;
    NativeArray<int> ni_;
    // bool b = true;
    void Update()
    {
        NativeArray<int> ni = new NativeArray<int>(new int[]{1,2,3}, Allocator.TempJob);
        JobHandle jobHandle0 = new Job1(){ni = ni}.Schedule(ni.Length, default);
        jobHandle0.Complete(); //おなじNativeArrayが同時にアクセスしないように完了させるか、依存を繋げる。
                                    //そうでないとInvalidOperationException: The previously scheduled job~ がでる
        JobHandle jobHandle1 = new Job1(){ni = ni}.Schedule(ni.Length, default);
        JobHandle.ScheduleBatchedJobs();
        jobHandle0.Complete(); jobHandle1.Complete();
        ni.Dispose();
        // if(!j1Handle.IsCompleted) j1Handle.Complete();
        // if(ni_.IsCreated) ni_.Dispose();
        // Job0 j0 = new Job0();
        // ni_ = new NativeArray<int>(new int[]{0,1,2,3,4}, Allocator.TempJob);
        // Job1 j1 = new Job1(){ni = ni_};
        // Job2 j2 = new Job2();
        // JobHandle j0Handle = j0.Schedule();//j0.Run();
        // j1Handle = j1.Schedule(4,default);
        // JobHandle j0j1Handle = JobHandle.CombineDependencies(j0Handle, j1Handle);
        // JobHandle j2Handle = j2.Schedule(j0j1Handle);
        // JobHandle j2Handle1 = j2.Schedule(j0j1Handle);
        // JobHandle j2Handle2 = j2.Schedule(j0j1Handle);
        // JobHandle j2Handle3 = j2.Schedule(j0j1Handle);
        // //j1.Schedule(4, j0.Schedule()).Complete();
        // using Job3 j3 = new Job3(new NativeArray<int>(new int[]{0,1,2,3,4}, Allocator.TempJob));
        // JobHandle j3Handle = j3.Schedule(4, 2, j2Handle);
        // //JobHandle.CheckFenceIsDependencyOrDidSyncFence(j0Handle, j1Handle) //j1Handleはj0Handleに依存しているか または j0Handleは完了しているか?
        //                                                                         //j1Handleの中のJobHandleフィールドメンバを再帰的に辿ってj0Handleに辿り着けるか?
        //                                                                         //trueならばj1Handle.Complete()で両方を実行するまたはしていた
        // Debug.Log($"depend-1 j0,j2: {JobHandle.CheckFenceIsDependencyOrDidSyncFence(j0j1Handle, j2Handle)}"); //true
        // Debug.Log($"depend-1 j2,j0: {JobHandle.CheckFenceIsDependencyOrDidSyncFence(j2Handle, j0j1Handle)}"); //false
        // j0Handle.Complete();
        // Debug.Log($"depend0 j0,j2: {JobHandle.CheckFenceIsDependencyOrDidSyncFence(j0j1Handle, j2Handle)}"); //true
        // Debug.Log($"depend0 j2,j0: {JobHandle.CheckFenceIsDependencyOrDidSyncFence(j2Handle, j0j1Handle)}"); //false
        // // JobHandle.CompleteAll(ref j0Handle, ref j1Handle);
        // j1Handle.Complete();
        // j1Handle.Complete();
        // Debug.Log($"depend1 j0,j2: {JobHandle.CheckFenceIsDependencyOrDidSyncFence(j0j1Handle, j2Handle)}"); //true
        // Debug.Log($"depend1 j2,j0: {JobHandle.CheckFenceIsDependencyOrDidSyncFence(j2Handle, j0j1Handle)}"); //false
        // j2Handle.Complete();
        // Debug.Log($"depend2 j0,j2: {JobHandle.CheckFenceIsDependencyOrDidSyncFence(j0j1Handle, j2Handle)}"); //true
        // Debug.Log($"depend2 j2,j0: {JobHandle.CheckFenceIsDependencyOrDidSyncFence(j2Handle, j0j1Handle)}"); //true
        // j2Handle1.Complete();
        // j2Handle2.Complete();
        // j2Handle2.Complete();
        // j2Handle3.Complete();
        // Debug.Log($"j0Handle.IsCompleted: {j0Handle.IsCompleted}");
        // j1Handle.Complete();
        // j3Handle.Complete();
        // Debug.Log($"j3Handle.IsCompleted: {j3Handle.IsCompleted}");

        // // Job0 jo0 = new Job0();
        // // Job0 jo1 = new Job0();
        // // Debug.Log($"jo0.Equals(jo1): {jo0.Equals(jo1)}"); //=>true
        // Job0 jo_= new Job0();
        // JobHandle jo0Handle = jo_.Schedule();
        // // jo0Handle.Complete();
        // JobHandle jo1Handle = jo_.Schedule(/*ここの引数以外に依存関係を構築することはない*/); //依存関係がある場合Schedule(JobHandle)は逐次実行的
        // Debug.Log($"_ jo0Handle.IsCompleted: {jo0Handle.IsCompleted}\njo1Handle.IsCompleted: {jo1Handle.IsCompleted}");
        // Debug.Log($"jo0Handle.Equals(jo1Handle): {jo0Handle.Equals(jo1Handle)}");//=>false //元のJobの構造体が同じ(jo_)でもSchedule()すれば別々の実行単位になる。
        // // jo1Handle.Complete(); //一度、Complete()したJobHandleはIsCompleteがtrueになり再びfalseになる事はない(はず) 
        //                          //Complete()をすると、このJobHandleに依存するJobHandleが未実行ならば依存順に実行されていくが、このJobHandleに依存されるJobHandleは実行されない
        //                          //j0H(Comp) -> j1H(NonComp) -> j2H(NonComp) -> j3H(NonComp)
        //                          //j2H.Complete();
        //                          //j0H(Comp) -> j1H(Comp) -> j2H(Comp) -> j3H(NonComp) //j1H(NonComp) -> j2H(NonComp)がその順で実行される
        // Debug.Log($"default(JobHandle).Equals(new JobHandle()): {default(JobHandle).Equals(new JobHandle())}");//=>true
        // Debug.Log($"default(JobHandle).IsCompleted: {default(JobHandle).IsCompleted}\nnew JobHandle().IsCompleted: {new JobHandle().IsCompleted}");//=>true
        // Debug.Log($"depend jo0,jo1: {JobHandle.CheckFenceIsDependencyOrDidSyncFence(jo0Handle, jo1Handle)}");//=>依存させてないのでfalse
        // jo0Handle.Complete();//←↓IsCompletedがtrueでもfalseでも同じJobHandleのCheckFenceIsDependencyOrDidSyncFenceはtrueになる。
        // Debug.Log($"jo0Handle.IsCompleted: {jo0Handle.IsCompleted}\ndepend j0,j0: {JobHandle.CheckFenceIsDependencyOrDidSyncFence(jo0Handle, jo0Handle)}");//=>true

        // Debug.Log($"_jo0Handle.IsCompleted: {jo0Handle.IsCompleted}\njo1Handle.IsCompleted: {jo1Handle.IsCompleted}");
        // JobHandle Combine = JobHandle.CombineDependencies(jo1Handle, jo0Handle); //Combine = CombineDependencies(j0Handle,j1Handle)で何方かがIsCompleted=>trueだとCombineもtrueになるが
        //                                                                          //CheckFenceIsDependencyOrDidSyncFence(Combine, new JobHandle())=>falseになる..
        // Debug.Log($"jo0Handle.IsCompleted: {jo0Handle.IsCompleted}\njo1Handle.IsCompleted: {jo1Handle.IsCompleted}");
        // Debug.Log($"Combine.IsCompleted: {Combine.IsCompleted}\ndepend Combine,new JobHandle(): {JobHandle.CheckFenceIsDependencyOrDidSyncFence(Combine, new JobHandle())}");//=>true,false
        // Debug.Log($"jo0Handle.IsCompleted: {jo0Handle.IsCompleted}\njo1Handle.IsCompleted: {jo1Handle.IsCompleted}");
        // Combine.Complete();
        // Debug.Log($"jo0Handle.IsCompleted: {jo0Handle.IsCompleted}\njo1Handle.IsCompleted: {jo1Handle.IsCompleted}");
        // Debug.Log($"Combine.IsCompleted: {Combine.IsCompleted}\ndepend Combine,new JobHandle(): {JobHandle.CheckFenceIsDependencyOrDidSyncFence(Combine, new JobHandle())}");//=>true,true

        // /*
        //     Job依存関係構築例
        //     JobHandle firest = firestJob.Schedule();
        //     j2.Schedule(JobHandle.CombineDependencies(j0.Schedule(firest), j1.Schedule(firest)))
        //     firest -> j0 -> j2
        //            -> j1 ->
        // */
        // /*
        //     //ECSで
        //     ECSJobHandle.IsCompleted //=>true
        //     JobHandle.CheckFenceIsDependencyOrDidSyncFence(ECSJobHandle, new JobHandle()) //=>false
        //     //であるようなECSJobHandleが存在する..
        // */



        //     // ni_.Dispose();
        // JobHandle.ScheduleBatchedJobs();
    }
    public struct Job0 : IJob{
        public void Execute(){
            Debug.Log("Job0");
        }
    }

    public struct Job1 : IJobFor{
        public NativeArray<int> ni;
        public void Execute(int index){
            Debug.Log($"Job1: {ni[index]}");
        }
    }
    public struct Job2 : IJob{
        public void Execute(){
            Debug.Log($"Job2");
        }
    }
    public struct Job3 : IJobParallelFor, IDisposable{
        NativeArray<int> ni;
        public Job3(NativeArray<int> _ni){ni = _ni;}
        public void Dispose(){ni.Dispose();}
        public void Execute(int index){
            Debug.Log($"Job3: {ni[index]}");
        }
    }
}

/*CSharpJobSystemProj*/
// /*
//     Script Debugging https://qiita.com/sawasaka/items/47d480f55ffde0448876
//         launch.jsonファイルを作成します。を押す (デバッカーを間違えた場合は.vscode/launch.jsonを消す)
//         [▷ Unity Editor]の▷を押す -> Unityの▷を押す。　Unityと繋がりデバッガーがアタッチされます。
//         並列処理されるメソッドにブレークを置くと調子が悪くなる
//         続行ボタンでブレーク置きに実行が安定している。

//     IJob
//         IJobを継承した構造体を定義しvoid Execute()を実装する。各スレッドはこの構造体をコピーしExecute()を実行する。
//         C#管理外で扱われ実行(Execute)される?ので管理された型(managed型)を使えない?
//     IJobExtension
//         JobHandle Schedule(JobHandle): Jobを実行できる型(JobHandle)を受け取りJobを実行できる型を返す。あと、JobHandleの静的フィールドのList<JobHandle>に登録される?
//                                         同じインスタンスと引数で何回呼んでも別々にScheduleし別々のJobHandleとなる
//         void Run(): IJob繋げず(JobHandleを使わず)に即時実行し完了を待つ(メインスレッドで実行される事もある)。Burstの恩恵を受けれる
//     JobHandle(ソースを覗いた所IntPtr jobGroup int versionしかなくIntPtrとNativeMethodの闇に葬られる)
//         スタティック
//             JobHandle CombineDependencies(JobHandel,JobHandle,,): 複数のJobHandleをまとめて一つのJobHandleを返す(複数のJobHandleをList<JobHandle>に登録したJobHandleを返す?)
//             void ScheduleBatchedJobs: 静的フィールドのList<JobHandle>を全て実行(Complete)する?しかしIsCompleteはtrueにしない?
//             bool CheckFenceIsDependencyOrDidSyncFence(j0Handle, j1Handle): j1Handleはj0Handleに依存しているか または j0Handleは完了しているか?
//                                                                            j1Handleの中のJobHandleフィールドメンバを再帰的に辿ってj0Handleに辿り着けるか?
//                                                                            trueならばj1Handle.Complete()で両方を実行するまたはしていた
//         インスタンス
//             //JobHandleはIJob系の構造体をコピーして持っている?
//             //JobHandleはList<JobHandle>を持っている?(Disposeが実装されてないのでNativeArrayではない?)
//             void Complete(): JobHandleのList<JobHandle>を底から深さ探索的に、IJobを各スレッドで実行しIsCompleteをtrueにしていく?
//                              一度呼んだら(IsCompleteがtrue)また実行(Execute)されることはない
//                              ScheduleしてからCompleteを呼ばなくてもエラーにならない
//             bool IsComplete{get;}: Scheduleされてから実行されたか?(defaultまたはnew JobHandle()した場合はtrue)
//     Burstでなくても参照型はJobに含めない
//         InvalidOperationException：Job6.clsは値型ではありません。 ジョブ構造体には、参照型を含めることはできません。
// */

// using System;
// using System.Threading;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Profiling;
// using Unity.Jobs;
// using Unity.Collections;
// public static class JobHandleExtention{
//     public static bool IsJobHandle_Complete(this JobHandle jobHandle) => JobHandle.CheckFenceIsDependencyOrDidSyncFence(jobHandle, default);
//     public enum Ternary{True, False, Unknown}
//     public static Ternary CheckFenceIsDependency(this JobHandle jobHandle0, JobHandle jobHandle1){
//         return 
//             IsJobHandle_Complete(jobHandle0)?
//                 jobHandle0.Equals(jobHandle1)?
//                     Ternary.True
//                     : Ternary.Unknown
//                 : JobHandle.CheckFenceIsDependencyOrDidSyncFence(jobHandle0, jobHandle1)?
//                     Ternary.True
//                     : Ternary.False;
//     }
// }
// namespace doumei{public static class Cname{}}
// //namespace doumei{public static class Cname{}}

// namespace Unity.Jobs{
//     public static class IJobForExtensions{ //Unity.Jobs内に同名のクラスがあるのにエラーにならない..
//         public static void Run<T>(this T jobData, int arrayLength) where T : struct, IJobFor{}//←これも(同名クラスの同名メンバ)を定義してもエラーにならないが(アセンブリ(dll)が違うから?)
//                                                                                                 //呼び出す(job1.Run(4);)とエラーになる
//         //(CS0121)次のメソッドまたはプロパティ間で呼び出しが不適切です: 'Unity.Jobs.IJobForExtensions.Run<T>(T, int)' と 'Unity.Jobs.IJobForExtensions.Run<T>(T, int)' [Assembly-CSharp]
//         public static JobHandle Schedule<T>(this T jobData, int arrayLength, params JobHandle[] dependencies) where T : struct, IJobFor{
//             Debug.Log("||||PARAMS版SCHEDULE||||");
//             JobHandle combineJobHandle = default;
//             foreach(JobHandle jh in dependencies){
//                 combineJobHandle = JobHandle.CombineDependencies(jh, combineJobHandle);
//             }
//             return jobData.Schedule(arrayLength, combineJobHandle); //引数完全ー版とparams版で何故かparams版を呼びunity落とす..//名前空間か?
//                                                                         //名前空間(namespace Unity.Jobs)内に書くことでうまくいった。。
//             //return IJobForExtensions.Schedule(jobData, arrayLength, combineJobHandle);
//         }
//     }
// }
// public class CSharpJobSystemSummary : MonoBehaviour
// {
//     // Start is called before the first frame update
//     UnityEngine.Profiling.CustomSampler sampler;
//     void Start(){
//         // Debug.unityLogger.logEnabled = false;
//         Debug.Log("==Start Start==");
//         // jobData = JobData.JobSetup();
//         // sampler = UnityEngine.Profiling.CustomSampler.Create("ThreadSleep");
//         // Thread.Sleep(500);

//     }

//     // Update is called once per frame
//     JobHandle j1Handle;
//     NativeArray<int> ni_;
//     private struct JobData : IDisposable{
//         public JobHandle jobHandle2; public NativeArray<int> nativeInt; public NativeArray<int> sum_; public Job3 job4;
//         public static JobData JobSetup(){
//             JobData jobData = new JobData();
//             jobData.nativeInt = new NativeArray<int>(new int[]{1,1,1,1, 1,1,1,1, 1,1,1,1, 1,1,1,1}, Allocator.TempJob);
//             jobData.sum_ = new NativeArray<int>(new int[]{4,4,4,4, 4,4,4,4,4}, Allocator.TempJob);
//             Job3 job3 = new Job3(){ni = jobData.nativeInt, sum = jobData.sum_};
//             jobData.job4 = new Job3(){ni = new NativeArray<int>(new int[]{6,6,6,6, 6,6,6,6}, Allocator.TempJob), sum = jobData.sum_};
//             Job5 job5 = new Job5(){ni = jobData.sum_};
//             JobHandle jobHandle = job3.Schedule(6, 3);
//             JobHandle jobHandle1 = jobData.job4.Schedule(4, 2);
//             jobData.jobHandle2 = job5.Schedule(4, JobHandle.CombineDependencies(jobHandle, jobHandle1));
//             return jobData;
//         }
//         public void Dispose(){nativeInt.Dispose(); sum_.Dispose(); job4.ni.Dispose();}
//     }
//     private JobData jobData;
//     static int thNum = 0;
//     static CustomSampler s = CustomSampler.Create("Sleep(2)");
//     static void ThreadFunc(){
//         // CustomSampler s = CustomSampler.Create("Sleep(2)");
//         // Profiler.BeginThreadProfiling("User_Thread", "ThreadFunc!");
//         s.Begin(); /*ほぼstatic的に動いている?CustomSamplerはどのThreadで呼ばれたか分かる?*/ Thread t = Thread.CurrentThread; //現在実行中のスレッドを取得します。
//         Debug.Log($"TreadFunc {thNum++}\nManagedThreadId: {t.ManagedThreadId}");Thread.Sleep(2);
//         s.End();
//         // Profiler.EndThreadProfiling();
//     }
//     float rtssu;
//     void Update()
//     {
//         Debug.Log("==Update Start==");
//         _ = typeof(global::doumei.Cname);

//         using var job1 = new Job1(){ni = new NativeArray<int>(new int[]{0,1,2,3,4,5,6,7,8,9}, Allocator.TempJob)};
//         using var job3 = new Job3(){ni = new NativeArray<int>(new int[]{0,1,2,3,4,5,6,7,8,9}, Allocator.TempJob), sum = new NativeArray<int>(new int[10], Allocator.TempJob)};

//         JobHandle jobHandle_ = job3.Schedule(4, 2, default); 
        
//         JobHandle jobHandle, jobHandle0, jobHandle1; //defaultでJobHandleの単位元を表現できる?ループの初期値に使える
//         jobHandle0 = jobHandle1 = default;
//         jobHandle = jobHandle_; //jobHandle = default;
//         for(int i = 0; i < 7; i++){
//             jobHandle = job3.Schedule(10, 2, jobHandle);
//             if(i == 0) jobHandle0 = jobHandle;
//             if(i == 3) {jobHandle1 = jobHandle;JobHandle.ScheduleBatchedJobs();}
//         }
//         JobHandle jobHandleCb = job1.Schedule(6,jobHandle_);
//         JobHandle jobHandleE = job1.Schedule(4, jobHandle, jobHandleCb); //自作IJobForExtention
//         //  job1.Run(4);
//         JobHandle.ScheduleBatchedJobs();
//         // Thread.Sleep(200);
//         //●defaultはどのJobHandleにも依存を持たないので↓はjobHandleがJobHandle.Complete()を実行したかどうかだけ分かる(defaultは実行した扱いらしい?..)
//         bool IsJobHandle_Complete(JobHandle jobHandle) => JobHandle.CheckFenceIsDependencyOrDidSyncFence(jobHandle, default);
//         Debug.Log($"||jobHandleE.IsCompleted||: {jobHandleE.IsCompleted}");
//         Debug.Log($"||pre:default,default||false? true: {IsJobHandle_Complete(default)}");
//         Debug.Log($"||pre:jobHandleE, default||false: {IsJobHandle_Complete(jobHandleE)}");
//         Debug.Log($"||pre:default, jobHandleE||false? true: {JobHandle.CheckFenceIsDependencyOrDidSyncFence(default, jobHandleE)}");
//         Debug.Log($"||pre:jobHandleE, jobHandleE||true: {JobHandle.CheckFenceIsDependencyOrDidSyncFence(jobHandleE, jobHandleE)}");
//         // jobHandleE.Complete();
//         Debug.Log($"||jobHandleE, default||true: {IsJobHandle_Complete(jobHandleE)}");
//         Debug.Log($"||default, jobHandleE||false? true: {JobHandle.CheckFenceIsDependencyOrDidSyncFence(default, jobHandleE)}");
//         Debug.Log($"||jobHandleE, jobHandleE||true: {JobHandle.CheckFenceIsDependencyOrDidSyncFence(jobHandleE, jobHandleE)}");

//         JobHandle.ScheduleBatchedJobs();
//         JobHandle.ScheduleBatchedJobs();

//         // JobHandle.ScheduleBatchedJobs(); //効果を発揮した IJobForでも これとCompleteを無効にしてもJobは次のフレームをまたいで実行される
//         // Debug.Log($"jobHandle.IsCompleted: {jobHandle.IsCompleted}");//IsCompleted: trueは↓に関係ない //ScheduleBatchedJobsの実行も含んでいる?
//             //これもCompleteも呼ばなくてもBehaviourUpdateの後でJobが実行している
//             //●ScheduleBatchedJobsは多分全ての種類のIJob~ですぐにScheduleされたJobを実行する(Schedule前のJobは実行しない)これを呼ばなくてもBehaviourUpdateの後でJobが実行している
//             //●IsCompleteはScheduleBatchedJobsと同じ挙動を含んでいるので中で実行している?←の2つとcompleteを呼ばない場合BehaviourUpdateの後でJobが実行している
//             //●CheckFenceIsDependencyOrDidSyncFenceはIsCompleteに無関係でjobHandle0とjobHandle1の依存関係が直近でなく間が空いていても有効、自分自身はtrueになる(jobHandle0, jobHandle0)
//             //●ScheduleBatchedJobsとCompleteはいくつ呼んでもエラーはない
//             //●defaultでJobHandleの単位元を表現できる?ループの初期値に使える
//             //●job3.Run(10); //IJobParallelForでもinnerloopBatchCountの指定ができず、全てMainThreadで実行される。
//             //●直列 -> 並列 -> 直列でIJobForとIJobparallelForを混ぜてもJobが正しく実行される事を確認した

//         // Debug.Log($"jobHandle.IsCompleted: {jobHandle.IsCompleted}");//IsCompleted: trueは↓に関係ない
//         // Debug.Log($"jobHandle0, jobHandle1: {JobHandle.CheckFenceIsDependencyOrDidSyncFence(jobHandle0, jobHandle1)}");
//         // Debug.Log($"jobHandle0, jobHandle: {JobHandle.CheckFenceIsDependencyOrDidSyncFence(jobHandle0, jobHandle)}");
//         // Debug.Log($"jobHandle1, jobHandle: {JobHandle.CheckFenceIsDependencyOrDidSyncFence(jobHandle1, jobHandle)}");
//         Debug.Log($"jobHandle0, jobHandle1: {jobHandle0.CheckFenceIsDependency(jobHandle1)}");
//         Debug.Log($"jobHandle0, jobHandle: {jobHandle0.CheckFenceIsDependency(jobHandle)}");
//         Debug.Log($"jobHandle1, jobHandle: {jobHandle1.CheckFenceIsDependency(jobHandle)}");
//         Debug.Log($"jobHandle1, jobHandle0: {jobHandle1.CheckFenceIsDependency(jobHandle0)}");
//         Debug.Log($"jobHandle, jobHandle0: {jobHandle.CheckFenceIsDependency(jobHandle0)}");
//         Debug.Log($"jobHandle, jobHandle1: {jobHandle.CheckFenceIsDependency(jobHandle1)}");

//         Debug.Log($"jobHandle, jobHandle: {jobHandle.CheckFenceIsDependency(jobHandle)}");

//         jobHandleE.Complete();//全true
//         // Debug.Log($"jobHandle1, jobHandle0: {JobHandle.CheckFenceIsDependencyOrDidSyncFence(jobHandle1, jobHandle0)}");
//         // Debug.Log($"jobHandle, jobHandle0: {JobHandle.CheckFenceIsDependencyOrDidSyncFence(jobHandle, jobHandle0)}");
//         // Debug.Log($"jobHandle, jobHandle1: {JobHandle.CheckFenceIsDependencyOrDidSyncFence(jobHandle, jobHandle1)}");
//         Debug.Log($"jobHandle0, jobHandle1: {jobHandle0.CheckFenceIsDependency(jobHandle1)}");
//         Debug.Log($"jobHandle0, jobHandle: {jobHandle0.CheckFenceIsDependency(jobHandle)}");
//         Debug.Log($"jobHandle1, jobHandle: {jobHandle1.CheckFenceIsDependency(jobHandle)}");
//         Debug.Log($"jobHandle1, jobHandle0: {jobHandle1.CheckFenceIsDependency(jobHandle0)}");
//         Debug.Log($"jobHandle, jobHandle0: {jobHandle.CheckFenceIsDependency(jobHandle0)}");
//         Debug.Log($"jobHandle, jobHandle1: {jobHandle.CheckFenceIsDependency(jobHandle1)}");

//         Debug.Log($"jobHandle, jobHandle: {jobHandle.CheckFenceIsDependency(jobHandle)}");

//         // Debug.Log($"jobHandle, jobHandle: {JobHandle.CheckFenceIsDependencyOrDidSyncFence(jobHandle, jobHandle)}");

//         // jobHandle.Complete();
//         //job3.Run(10); //IJobParallelForでもinnerloopBatchCountの指定ができず、全てMainThreadで実行される。
    
//     /*
//         Job6 job6 = new Job6(){intArr = new int[]{0,1,2,3,4,5,6,7,8,9}};//{cls = new Cls(){clsInt = 0}}
//         JobHandle j6Handle = job6.Schedule(8, default);
//         // j6Handle.Complete();
//         // Debug.Log($"job6.cls.clsInt: {job6.cls.clsInt}");
//     */

//     /*
//         Recorder recorder =Recorder.Get("BehaviourUpdate");
//         recorder.enabled = true;
//         Debug.Log($"BehaviourUpdate: {recorder.elapsedNanoseconds * 0.000001f}ms");
//         Recorder recorder1 = CustomSampler.Get("BehaviourUpdate").GetRecorder();
//         recorder1.enabled = true;
//         Debug.Log($"BehaviourUpdate1: {recorder1.elapsedNanoseconds * 0.000001f}ms");
//         string s = null;
//         Debug.Log(s);
//         Debug.Log(!!!true! != !!!false);
//         Thread.Sleep(1);
//         var sampler = UnityEngine.Profiling.CustomSampler.Create($"ThreadSleep: {rtssu * 1000f}ms");
//         sampler.Begin();
//         rtssu = Time.realtimeSinceStartup;
//         Thread.Sleep(2);
//         rtssu = Time.realtimeSinceStartup - rtssu; 
//         sampler.End();
//         Thread thread = new Thread(ThreadFunc);
//         thread.Start();
//         Thread thread1 = new Thread(ThreadFunc);
//         thread1.Start();
//     */

//     /*
//         using Job3 j_3 = new Job3(){ni = new NativeArray<int>(new int[]{2,4,6,8,10}, Allocator.TempJob), sum = new NativeArray<int>(new int[]{5,5,5,5,5,5}, Allocator.TempJob)};
//         j_3.Schedule(4,1).Complete();//j_3.Run(3);
//         foreach(int i in j_3.ni)Debug.Log($"i: {i}");
//         foreach(int s in j_3.sum)Debug.Log($"s: {s}");
//     */

//     /*
//         // Thread.Sleep(4);
//         //↓でスキップしてプロファイラーで見るとidleだけに見えるが、フォーカスしているフレームの前後合わせて8フレーム内かつ画面表示内に処理の起点が入っていないと表示されない
//         //あと、プロファイラーのタイムラインの横スクロールの操作性が悪すぎる。//追記 マウスのMMBで操作できる
//         if(!jobData.jobHandle2.IsCompleted) return; 
//         //jobData.jobHandle2.IsCompleted!ってなんだ？ｗ null許容演算子でコンパイラにWarning CS8602: ヌル参照の可能性のある間接参照。を抑制するもの(v8.0)
//         Debug.Log($"IsCompleted_: {jobData.jobHandle2.IsCompleted}");

//         jobData.jobHandle2.Complete();
//         foreach(int i in jobData.nativeInt)Debug.Log($"i: {i}");
//         foreach(int s in jobData.sum_)Debug.Log($"s: {s}");
//         jobData.Dispose();
//         // Thread.Sleep(500);
//         jobData = JobData.JobSetup();
//         // while(jobData.jobHandle2.IsCompleted!){Debug.Log("job executing");}
//         Debug.Log($"IsCompleted: {jobData.jobHandle2.IsCompleted}");
//         // Debug.Log("job end");
//         // JobHandle.ScheduleBatchedJobs();
//     */
//     }
//     public struct Job0 : IJob{
//         public void Execute(){
//             Debug.Log("Job0");
//         }
//     }

//     public struct Job1 : IJobFor, IDisposable{
//         public NativeArray<int> ni;
//         public void Execute(int index){
//             Thread.Sleep(1);
//             Debug.Log($"Job1: {ni[index]}");
//         }
//         public void Dispose(){
//             ni.Dispose();
//         }
//     }
//     public struct Job2 : IJob{
//         public void Execute(){
//             Debug.Log($"Job2");
//         }
//     }
//     public struct Job3 : IJobParallelFor, IDisposable{
//         public NativeArray<int> ni;
//         [ReadOnly] public NativeArray<int> sum;
//         //public Job3(NativeArray<int> _ni){ni = _ni;}
//         public void Dispose(){ni.Dispose(); sum.Dispose();}
//         public void Execute(int index){
//             Debug.Log("Thread.Sleep3()");
//             Thread.Sleep(5);
//             // Debug.Log($"Job3: {ni[index]}");
//             // sum[index] += ni[index];
//             Debug.Log($"Job3: {ni[index]}\nssuumm: {sum[index]}");
//             ni[index] = 9;
//         }
//     }
//     public struct Job5 : IJobFor{
//         public NativeArray<int> ni;
//         public void Execute(int index){
//             Debug.Log("Thread.Sleep5()");
//             Thread.Sleep(8);
//             Debug.Log($"ni: {ni[index]}");
//         }
//     }
//     public struct Job6 : IJobFor{
//         // public Cls cls; //InvalidOperationException：Job6.clsは値型ではありません。 ジョブ構造体には、参照型を含めることはできません。
//         /*static readonly*/ public int[] intArr/* = new int[]{0,1,2,3,4,5,6,7,8,9}*/; //static readonlyの配列ならいける。固定長配列のような扱いになる?
//         public void Execute(int index){
//             Debug.Log($"intArr[{index}]: {intArr[index]}");//\nClsFunc: {cls.ClsFunc()}
//         }
//     }
//     public class Cls{
//         public int clsInt;
//         public int ClsFunc(){
//             return this.clsInt++;
//         }
//     }
// }
