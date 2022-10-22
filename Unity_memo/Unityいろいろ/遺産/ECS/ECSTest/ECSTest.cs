using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;

// 追記:半分わけわからん↓

//_ = Dependency; //これはGetEntityQuery(archetypeの集合)やGetComponentTypeHandle(アクセスするcomp(特に書き込み))などの呼び出しから依存関係を推測し自動的に設定される。
//GetComponentDataFormEntityとかも? //とにかく複数のWorkerTheradから同じcompに書き込みえる状況を避ける様に設定される?
//基本Comp、ReadOnly,ReadWrite、EntityQueryDesc(With**<T1,...>()+ForEach(in,ref)(Anyが謎))、実行系メソッド、
//Get***TypeHandle、ComponentDataFromEntity、システムの実行順序
//Query系(Systemが起動する条件)、TypeHandle系(Jobがアクセスするデータ)、Job実行系、System順序系(== Dependencyの繋がり順)
//Job実行系 Run(): 依存Jobの完了を待ち(CompleteDependency()?)MainThreadで処理する(Jobで無い場合もある?)のでできる事が多い
//         Schedule: 1つのThreadを使用してJobを実行する。(Jobがアクセスするデータ(TypeHandle系)は自由に読み書きできる)
//         ScheduleParallel: 複数のThreadを使用してJobを実行する。(Jobがアクセスするデータ(TypeHandle系)のReadOnlyは自由だが
//                                                                  そうで無い場合はChunk内データのみ書き込み可能?)
//SystemとJobの処理単位はChunk。複数のThreadから同じChunkに書き込まないようにScheduleし依存関係(Dependency)を結ぶ?
//QueryはDescに合致するEntityArchetypeの集合のEntityの集合
//EntityManagerによってチャンク構造を変更する処理、 すなわちエンティティの生成と破棄、コンポーネントの追加と削除、共有コンポーネントの値の変更が行われた場合、 EntityManagerは実行中のすべてのジョブの完了を待ちます。 この完了待ちをするタイミングおよびイベントのことを同期点(sync point)と呼びます。
//順序不順でどちらも書き込む、Thread.Sleepを入れてプロファイラ(色も水色以外にできるか)、新しいプロジェクトで、ReadOnlyで書き込むとRunで動く? 
//ReadWrite,ReadOnlyはEntityQueryは区別されるが、どちらもReadWrite,ReadOnlyを取得できてしまう
//ReadWrite,ReadOnlyは区別なく何方も読み書きできるしクエリから取得できてしまう。意味はあるのか..
//System_0:in System_1:inは独立、それ以外は依存 //ある結果に対してinは並列にできるがrefはできない?
//G0  -> G1 -> G2
//ref -> in -> ref
//    -> in ->
//    -> in ->
//chunk utilization 1360 (Entity/chunk)
//基本Componentでも何方かがrefでもう片方がクエリに含めるだけで何もなしだと並列可能?(クエリに入ってるだけでアクセスしてないから)
//System内でScheduleしたものをDependencyに上書きしないとそれはJobの完了を待たなくなる
//Systemの実行順でSystemは順番に実行されて行く、その時に既に実行されたSystemのどのDependencyをセットしてくるか(しないか(default))


//タグComponentはrefでも並列に同チャンクにアクセス可能?(データが無いからrefとか意味がない)
//GetArchetypeChunkIterator()はチャンク毎にチャンク構造を変更する処理をしている?
//あるSystemが処理を開始する時、多分Update()の中で、
    //前回のSystemの終了時に設定されたDependencyを完了し、UnityECSがSystem実行順で
    //既にSystemが実行されScheduleが行われた時のTypeHandleと今からScheduleをするTypeHandleが両方ReadOnly(引数がTrue)なら依存関係が結ばれず並列に処理するが、
    //それ以外は基本的には依存関係が結ばれる(Component違い または Chunk違いであったとしても(両方違いは並列できる))(Component違い: ref -> in: o)
        //追記:NativeArray<ArchetypeChunk> Chunk_Arrayは、並列処理できない?(System間で共有できない)
//↑と内容が被るが、UnityECSはDependencyに依存関係をセットする時に２つのSystemが同じChunkの違うComponentへの書き込みであっても
    //２つのSystem間に依存関係が結ばれ並列に動作しない
    //しかし、１つのSystem内に↑のような２つのJobを手動で依存関係を結びScheduleすればエラーなく動作できる。  (↑改善される可能性もあるかも?)
//●SystemのDependencyは [互いに素なQuery] または [両方Readのみ] である場合は並列処理するが、それ以外は基本的に依存関係が結ばれ直列になる。
    //今実行しているSystemから既に実行されたSystemを遡り↑に違反するSystemのDependencyを代入し依存関係を結ぶ多分
    //(今が[ReadOnly]の場合過去直近のNot[ReadOnly]と繋ぐ。か、今がNot[ReadOnly]の場合過去直近のNot[ReadOnly]を繋ぐまたは、直近の全ての[ReadOnly]をCombineして繋ぐ?)
        //(なぜTypeHandle(Readか?とComponentType)が分かるのか..ForEachだから?)
            //システムが読み書きするコンポーネントは、 GetEntityQueryやGetArchetypeChunkComponentType(OnUpdateの中ですが..)などの呼び出しから判断されます。
    //[Chunk違いかつComponent違い]は間違い[互いに素なQuery]。
        //●Dependencyの自動設定はEntityの存在に無関係だった。
    //●本来は[TypeHandleの積集合は[ReadOnly]にする]で大丈夫なはず。
//●Run()はその前にDependencyComplete()が走るそしてMainThreadで処理しDependency = default(JobHandle)する多分
//●SystemのJobの依存関係は基本的に過去一周分以内でその繋がりはバネの様になるが一周するとComplete()するのでバネワッシャーのような形になる。
//●GatherChunksAndOffsetsJob(チャンクを収集し、ジョブをオフセットします)は
    //query.CreateArchetypeChunkArrayAsync(Allocator.TempJob, out JobHandle jobHandle)かな?GatherChunksJob (Burst)がでた
    //ForEachでSelfにならないのは↑これが挟まっている? //IJobForからつくる
//●SystemのDependencyサイクル: [Dependency.Complete()](Update?) ->
    //[Dependency = Combine(依存するDependency, query.CreateArchetypeChunkArrayAsync)](Update?) -> [Dependency = job.Schedule(Dependency)](OnUpdate)
//同じqueryであっても同時に実行する各Job毎にquery.CreateArchetypeChunkArrayしても別々のNativeArray<ArchetypeChunkArray>になりJobに[ReadOnly]は要らない
//●NativeContainerなどは、権限がJobかMainThreadかという状態の[Job:(jobHandle0,..)]?と、
    //アクセスは直列のみでReadWriteかまたは直列並列でReadのみなのかという状態の[ReadOnly] の２つの状態がある?
    //操作は、[Schedule(JobHandle)], [JobRead], [JobWrite], [Complete(JobHandle)], [MainReadWrite], [Dispose] のようなものがある?
//●OnUpdate()でForEachを[互いに素なQuery]になる様に2個書いて実行してプロファイラで見たところ確実に依存関係を繋いでるみたい。(Dependencyを書けないのでCheckDependencyを使えない)
    //この事よりForEachを複数書いた場合はどんなQueryであれそのForEachを書いた順序で実行されると思う。
//●ForEachで引数0個は無理なのでentityInQueryIndexやnativeThreadIndexを引数にして実行すると全てのEntityを集めるQueryが作られるみたい。
//JobChunkExtensionsのScheduleとScheduleParallelの定義は同じ
//警告DC0055：Entities.ForEachはCompDataを値で渡します。 加えられた変更は、基になるコンポーネントに保存されません。 
    //必要なアクセスを指定してください。 読み取り専用アクセスの場合は「in」を使用し、読み取り/書き込みアクセスの場合は「ref」を使用します。
    //●ForEachのrefもinも付けない値渡しはentityInQueryIndexやnativeThreadIndex用みたい

//●キャプチャまとめ
    //値型ローカル変数はReadだけなら多分Jobにコピーするだけなので制限なし
        //writeもすると、.Run()のみとなる(NativeArray<struct>にコピーしてJob実行し書き戻してるか、JobChunkExtensions.RunWithoutJobs()か分からん)
    //NativeContainerは普通にJobに渡すのと同じか、MainThreadでも使えるので制限はない。開放を忘れずに。
        //(.WithDisposeOnCompletion(NativeContainer)、.WithReadOnly(NativeContainer)などの設定ができる)
    //参照型はJobで動かないので全て .WithoutBurst().Run()(多分RunWithoutJobs())でしか動かない。フィールドメンバ変数も全て例外なくthisが参照型なため参照型として扱われる
        //フィールドメンバは値型ローカル変数にコピーする方法もある。
        //フィールドメンバメソッドはインスタンスメソッドとバーチャルメソッドはthisが参照型なため.WithoutBurst().Run()でしか動かない。
        //スタティックメソッドはインスタンス(this)が必要ないので単なる単純な静的メソッドで制限なしで呼べる

//●EntityManagerはNativeContainerの様にSafety機能?が付いていて複数のJobで同時(依存関係がない複数のJobまたはParallel)に
    //EntityManagerを保持またはアクセスしている場合は[ReadOnly]が要るが、Entityに関わるメソッド呼び出しはWriteした事になるのでEntityに関わらないメソッドしか呼べない。
    //複数のJobで同時でない(依存関係が繋がれていてParallelでない)場合はEntityに関わりChunkの構造を変えないメソッド(Get,Set,Has)は呼べるが、
    //GetComponentはin,[ReadOnly]と同等で依存関係を繋ぐ代わりにその依存関係の対象をComplete()する。なのでJobで未Complete()のJobHandleをComplete()すると↓↓のエラーがでる
    //SetComponentはref,Not[ReadOnly]相当、HasComponentは制限なしでComplete()なしで動く。(EntityMangerを使ったHasComponentはParallelで動かなかった(書き込み中と認識される))
    //Chunkの構造を変える(Add,Remove,Create)メソッドを呼ぶとUnityException：ScheduleBatchedJobsAndCompleteは、
    //メインスレッドからのみ呼び出すことができます。とでてMainThreadで呼ぶとCompleteAllJobs()を実行し同期点(SyncPoint)を作っていた。
    //EntityManagerを含んだJobをScheduleしてCompleteせずにMainThreadでEntityManagerの機能を使うとInvalidOperationException: ~You must call JobHandle.Complete()となる
    //なのでScheduleしたらNativeContainerの様にMainThreadでEntityManagerの機能を使う前にComplete()を実行しEntityManagerの権限をJobからMainThreadに返還しなければならない
    //Systemから抜ける前にもEntityManagerを含んだJobをComplete()しないとエラーとなる。
    //Chunkの構造を変えるメソッドを呼ぶJobはRunWithoutJobs(ref ArchetypeChunkIterator)(ForEachのWithStructuralChanges()?)で実行する事ができる(Jobでは無いかな)
    //●SystemBaseのGetComponentとHasComponentはScheduleParallelで並列に動く。

//~FromEntity<~>系(ComponentDataFromEntity<~>)はForEachの外で生成して変数に代入しそれをキャプチャしようとすると
    //書き込み可能な ~(存在する2つの対象が)~ ComponentDataFromEntity <CompData>であり、2つのコンテナーが同じではない可能性があります（エイリアシング）。
    //と、でて実行時エラーになる。しかし、ScheduleParallel()では[ReadOnly]にする必要がありそのためには~FromEntity<~>系をForEachの外に
    //出して、.WithReadOnly(キャプチャ変数)を付けないといけないが、その場合は実行時エラーがなく問題なく動く。(エラー文に"書き込み可能な"とあったので[ReadOnly]を付ける事で区別された?)



public static class ScriptEnable{
    public const bool Enable = false;
}
public class ECSTest : MonoBehaviour{
    World world;
    EntityManager entityManager;
    void Start(){
        enabled = ScriptEnable.Enable;
        world = World.DefaultGameObjectInjectionWorld;
        entityManager = world.EntityManager;

        if(!ScriptEnable.Enable) return;

        ComponentType CompDate_ComponentType = ComponentType.ReadWrite<CompData>();
        EntityArchetype CompDate_EntityArchetype = entityManager.CreateArchetype(CompDate_ComponentType);
        Entity CompDate_Entity = entityManager.CreateEntity(CompDate_EntityArchetype);
        entityManager.CreateEntity(typeof(CompData), typeof(TagA));
        entityManager.CreateEntity(typeof(CompData), typeof(TagB));
        entityManager.CreateEntity(typeof(CompData), typeof(CompData1), typeof(TagB), typeof(TagC));
        entityManager.CreateEntity(typeof(CompData), typeof(TagB), typeof(TagC));
        entityManager.CreateEntity(typeof(CompData), typeof(TagD));
        entityManager.CreateEntity(typeof(CompData2), typeof(TagE));
        entityManager.CreateEntity(typeof(CompData), typeof(CompData1), typeof(CompData2), typeof(TagB), typeof(TagE));
    }
    void Update(){
        Debug.Log("[Update]");
        Debug.Log($"System_0 depend System_1: {world.GetExistingSystem<System_0>().Dependency.CheckDependency(world.GetExistingSystem<System_1>().Dependency)}\nSystem_1 depend System_0: {world.GetExistingSystem<System_1>().Dependency.CheckDependency(world.GetExistingSystem<System_0>().Dependency)}");
        if(Time.frameCount % 4 == 0){
            // world.GetExistingSystem<System_1>().Enabled = true;
        }else{
            // world.GetExistingSystem<System_1>().Enabled = false;
        }
    }
}

// 実行順序を指定しない場合の実行順はクラス名でソートされるみたい
public  class System_0 : E_SystemBase{
    protected override void OnUpdate(){
        //Dependencyが自動設定される前に毎回SystemBase.Update()でif(!Dependency.IsCompleted)Dependency.Complete();されている?
       // Update();再帰になる気がする
        Debug.Log("[System_0]");
        Entities.ForEach((ref TagA tb)=>{ //System_0:in System_1:inは独立、それ以外は依存 //ある結果に対してinは並列にできるがrefはできない?
            Debug.Log("==S_0 Start==");
            Thread.Sleep(2);
            Debug.Log("==S_0 End==");
        }).WithoutBurst().Schedule();
        Debug.Log($"System_0 is default?: {World.GetExistingSystem<System_0>().Dependency.Equals(default(JobHandle))}\nSystem_1 is default?: {World.GetExistingSystem<System_1>().Dependency.Equals(default(JobHandle))}");
        Debug.Log($"System_0 Complete?: {World.GetExistingSystem<System_0>().Dependency.IsJobHandle_Complete()}\nSystem_1 Complete?: {World.GetExistingSystem<System_1>().Dependency.IsJobHandle_Complete()}");
        Debug.Log($"System_0 depend System_1: {World.GetExistingSystem<System_0>().Dependency.CheckDependency(World.GetExistingSystem<System_1>().Dependency)}\nSystem_1 depend System_0: {World.GetExistingSystem<System_1>().Dependency.CheckDependency(World.GetExistingSystem<System_0>().Dependency)}");
    }
}
// [AlwaysUpdateSystem] // GetEntityQueryに合致するEntityが一つも無くても起動する(でもGetEntityQuery自体がない場合はデフォルトで起動する) 
                            //正確にはGetEntityQueryを一回以上呼び出していて かつ EntityQueriesにEntityが１つ以上存在するか見ている?。EntityManager.CreateEntityQueryは関係ない
[UpdateAfter(typeof(System_0))]
public class System_1 : E_SystemBase{
    EntityQuery query;
    EntityQuery query1;
    protected override void OnCreate(){
        base.OnCreate();
        // query = GetEntityQuery(ComponentType.ReadWrite<CompDate>()); //GetEntityQueryはEntityQueriesに記憶され既にあるQueryは記憶されない(集合的)、後ReadWriteとReadOnlyも区別できている。
    }
    protected override void OnUpdate(){
        Debug.Log("[System_1]");

        Debug.Log($"0:System_0 depend System_1: {World.GetExistingSystem<System_0>().Dependency.CheckDependency(World.GetExistingSystem<System_1>().Dependency)}\nSystem_1 depend System_0: {World.GetExistingSystem<System_1>().Dependency.CheckDependency(World.GetExistingSystem<System_0>().Dependency)}");//=>System_1 depend System_0: Depend

        JobHandle temp0 = Dependency;
        /*JobHandle j0 = */Entities
        // .WithAny<TagC, TagD>() //クエリの結果にしっかり反映される(クエリには含めずは間違い?)
        .WithAll<CompData1>()
        .WithStoreEntityQueryInField(ref query)
        .ForEach((ref TagB tb, ref CompData cd)=>{
            Debug.Log("==S_1 Start==");
            Thread.Sleep(10);
            Debug.Log("==S_1 End==");
        }).WithoutBurst().Schedule(/*Dependency*/);//引数を書いた場合は戻り値がDependencyに自動設定されない
        JobHandle temp = Dependency;
        // JobHandle j1 = Entities
        // .WithStoreEntityQueryInField(ref query1)
        // .ForEach((ref TagB tb, ref CompDate1 cd)=>{
        //     Debug.Log("==S_1 Start==");
        //     Thread.Sleep(10);
        //     Debug.Log("==S_1 End==");
        // }).WithoutBurst().ScheduleParallel(Dependency);
        // Dependency = JobHandle.CombineDependencies(j0, j1); //これをアウトするとScheduleしたもの(j0,j1)がJobの連なりに繋がれずScheduleを投げっぱなしになる。
        Debug.Log($"temp.CheckDependency(temp0): {temp.CheckDependency(temp0)}");
        Debug.Log($"Dependency depend temp?: {Dependency.CheckDependency(temp)}");//=>Depend
        Debug.Log($"Dependency depend temp0?: {Dependency.CheckDependency(temp0)}");//=>Depend
        Debug.Log($"temp depend Dependency?: {temp.CheckDependency(Dependency)}");//=>Not_Depend
        Debug.Log($"EntityQueries.Length: {EntityQueries.Length}");//=>2
        Debug.Log($"query is Queries[0]?: {query.Equals(EntityQueries[0])}");//=>True
        Debug.Log($"query.CalculateEntityCount(): {query.CalculateEntityCount()}");//=>2
        Debug.Log($"query.CalculateChunkCount(): {query.CalculateChunkCount()}");//=>1
        // Debug.Log($"query1 is Queries[1]?: {query1.Equals(EntityQueries[1])}");//=>True
        // Debug.Log($"query1.CalculateEntityCount(): {query1.CalculateEntityCount()}");//=>3
        // Debug.Log($"query1.CalculateChunkCount(): {query1.CalculateChunkCount()}");//=>2
        
        Debug.Log($"System_0 is default?: {World.GetExistingSystem<System_0>().Dependency.Equals(default(JobHandle))}\nSystem_1 is default?: {World.GetExistingSystem<System_1>().Dependency.Equals(default(JobHandle))}");
        Debug.Log($"System_0 Complete?: {World.GetExistingSystem<System_0>().Dependency.IsJobHandle_Complete()}\nSystem_1 Complete?: {World.GetExistingSystem<System_1>().Dependency.IsJobHandle_Complete()}");

        Debug.Log($"1:System_0 depend System_1: {World.GetExistingSystem<System_0>().Dependency.CheckDependency(World.GetExistingSystem<System_1>().Dependency)}\nSystem_1 depend System_0: {World.GetExistingSystem<System_1>().Dependency.CheckDependency(World.GetExistingSystem<System_0>().Dependency)}");//=>System_1 depend System_0: Depend

        Debug.Log($"System_1_1 depend System_1: {World.GetExistingSystem<System_2>().Dependency.CheckDependency(World.GetExistingSystem<System_1>().Dependency)}\nSystem_1 depend System_1_1: {World.GetExistingSystem<System_1>().Dependency.CheckDependency(World.GetExistingSystem<System_2>().Dependency)}");

        // Debug.Log($"System is World: {EntityManager.Equals(World.DefaultGameObjectInjectionWorld.EntityManager)}");//=>True SystemとWorldにあるEntityManagerは同じ
    }
}
[UpdateAfter(typeof(System_0))]
public class System_2 : E_SystemBase{
    protected override void OnUpdate(){
        Debug.Log("[System_1_1]");
        Debug.Log($"Dependency is default: {Dependency.Equals(default(JobHandle))}");                             //TypeHandle ⊂ Query //ChunkじゃなくQuery?
         Entities                                                          //ScheduleParallel -> ScheduleParallel //Schedule -> Scheduleも多分同じ  //ComponentじゃなくTypeHandle?
        //[.WithAll<CompData1>().ForEach((ref TagB tb, ref CompData cd)]   //RunはRunの実行前にDependencyを完了し全てMainに埋め込まれ処理されDependencyをdefaultにするかな
        // .WithAll<CompDate1>().ForEach((ref TagB tb, ref CompDate cd)=>{ //in -> in: o, ref -> in: x, in -> ref: x, ref -> ref: x //同じQuery, 同じTypeHandle
        // .ForEach((ref TagA ta, ref CompDate cd)=>{                      //in -> in: o, ref -> in: x, in -> ref: x, ref -> ref: x //共なQuery, 同じTypeHandle
        // .ForEach((ref TagB tb, ref CompData1 cd1)=>{                    //in -> in: o, ref -> in: o, in -> ref: x, ref -> ref: x //共なQuery, 違うTypeHandle
        .ForEach((ref TagE te, ref CompData2 cd2)=>{                       //in -> in: o, ref -> in: o, in -> ref: o, ref -> ref: o //素なQuery, 違うTypeHandle
            Debug.Log("==S_1 Start==");
            Thread.Sleep(10);
            Debug.Log("==S_1 End==");
        }).WithoutBurst().Schedule();
        Debug.Log($"Dependency is default: {Dependency.Equals(default(JobHandle))}");
        Debug.Log($"System_1 depend System_1_1: {World.GetExistingSystem<System_1>().Dependency.CheckDependency(World.GetExistingSystem<System_2>().Dependency)}\nSystem_1_1 depend System_1: {World.GetExistingSystem<System_2>().Dependency.CheckDependency(World.GetExistingSystem<System_1>().Dependency)}");

    }
}

public abstract class E_SystemBase : SystemBase{
    public new JobHandle Dependency {get{return base.Dependency;}set{base.Dependency = value;}}
    protected override void OnCreate(){
        base.OnCreate();
        Enabled = ScriptEnable.Enable;
    }
}
public struct CompData : IComponentData{
    public int IntValue;
}
public struct CompData1 : IComponentData{
    public int IntValue;
}
public struct CompData2 : IComponentData{
    public int IntValue;
}
public struct TagA :IComponentData{}
public struct TagB :IComponentData{}
public struct TagC :IComponentData{}
public struct TagD :IComponentData{}
public struct TagE :IComponentData{}
public class StaticValue{
    public static int IntValue = 0;
}
