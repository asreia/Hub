/*
    
*/


using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;

public class ECSSummary : MonoBehaviour{
    void Start(){
        //デフォルトで存在するWorldを取得
        World world = World.DefaultGameObjectInjectionWorld;
        //WorldからentityManagerを取得
        EntityManager entityManager = world.EntityManager;

        //ComponentTypeはSystem.Typeを拡張したComponentの型(通常、暗黙的キャストされる)
        //EntityArchetypeはComponentTypeの集合から作られるEntityの型
        System.Type CDA_Type = typeof(ComponentDataA);
        ComponentType CDA_ComponentType = ComponentType.ReadWrite(CDA_Type);
        ComponentType CDA_R_ComponentType = ComponentType.ReadOnly(CDA_Type); //ReadOnlyでもReadWriteと変わらず書き込めるしForEachのrefで合致したりして変わらない..
        ComponentType CDB_ComponentType = ComponentType.ReadWrite<ComponentDataB>();
        // ComponentType CDB_ComponentType = ComponentType.ReadOnly<ComponentDataB>();
        //Archetypeの作成にはentityManagerが必要?
        //ReadOnlyとReadWrite違いでも同じコンポーネント型とみなされArgumentException：同じエンティティに同じタイプの2つのコンポーネントを含めることはできません。と出る
        EntityArchetype CDA_CDB_entityArchetype = entityManager.CreateArchetype(CDA_ComponentType, /*CDA_R_ComponentType, */CDB_ComponentType); 

        Entity entity = entityManager.CreateEntity();
        entityManager.SetArchetype(entity, CDA_CDB_entityArchetype);
        //SetArchetypeはChunk移動をEntityArchetypeで一気に変えるもの
        //普通はentityManager.CreateEntity(CDA_CDB_entityArchetype)


        entityManager.AddBuffer<BufferElementData>(entity);
        DynamicBuffer<BufferElementData> buf = entityManager.GetBuffer<BufferElementData>(entity);
        buf.Add(new BufferElementData()); 
        buf.Add(new BufferElementData());

        entityManager.SetComponentData(entity, new ComponentDataB(){IntValue = 11});
        entityManager.AddSharedComponentData(entity, new SharedComponentDataA());
        entityManager.AddSharedComponentData(entity, new SharedComponentDataB());
        entityManager.SetSharedComponentData(entity, new SharedComponentDataA(){IntValue = 2});
        // entityManager.SetSharedComponentData(entity, new SharedComponentDataB(){IntValue = 6});
        entityManager.SetSharedComponentData(entity, new SharedComponentDataB(){IntValue = 6, cls = new ClsForShared(){ClsIntValue = 4}});

        Entity entity1 = entityManager.Instantiate(entity);
        
        // entityManager.SetSharedComponentData(entity1, new SharedComponentDataA(){IntValue = 4});
        entityManager.SetSharedComponentData(entity1, new SharedComponentDataB(){IntValue = 7});
        // entityManager.SetComponentData(entity1, new ComponentDataB(){IntValue = 22});
    }
    void Update(){
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        EntityQueryDesc entityQueryDesc = new EntityQueryDesc();
        entityQueryDesc.All = new ComponentType[]{ComponentType.ReadWrite<ComponentDataA>(), typeof(ComponentDataB), typeof(BufferElementData),/*DynamicBuffer<>は付けない*/
                                                    typeof(SharedComponentDataA)};
        //EntityQueryDescはComponentTypeの集合から作られるEntityArchetypeの集合のようなEntityQueryを作るための説明
        EntityQuery entityQuery =  entityManager.CreateEntityQuery(entityQueryDesc);
        //EntityQueryはentityManagerに関連付けることによって作られEntityArchetypeの集合と合致するEntityの集合を表す
        //中にEntityの配列がある訳ではなく取得時に取得される(多分、EntityArchetypeの要素と合致するchunk内のNativeArray<Entity>を全て繋げたもの?)

        EntitiesLog(entityQuery);

        void EntitiesLog(EntityQuery entityQuery){
            using (NativeArray<Entity> entities =  entityQuery.ToEntityArray(Allocator.Temp)){
                for(int i = 0; i < entities.Length; i++){
                    Debug.Log($"entities[{i}]: {entities[i]}");
                    if(entityManager.HasComponent<ComponentDataA>(entities[i])) 
                        Debug.Log($"{entities[i]}: ComponentDataA: {entityManager.GetComponentData<ComponentDataA>(entities[i]).IntValue}");
                    Debug.Log($"{entities[i]}: ComponentDataB: {entityManager.GetComponentData<ComponentDataB>(entities[i]).IntValue}");
                    Debug.Log($"{entities[i]}: SharedComponentDataA: {entityManager.GetSharedComponentData<SharedComponentDataA>(entities[i]).IntValue}");
                    Debug.Log($"{entities[i]}: SharedComponentDataB: {entityManager.GetSharedComponentData<SharedComponentDataB>(entities[i]).IntValue}");
                    Debug.Log($"{entities[i]}: SharedComponentDataBCls: {entityManager.GetSharedComponentData<SharedComponentDataB>(entities[i]).cls?.ClsIntValue}");
                    DynamicBuffer<BufferElementData> buf = entityManager.GetBuffer<BufferElementData>(entities[i]);
                    for(int j = 0; j < buf.Length; j++){
                        Debug.Log($"{entities[i]}: buf[{j}].IntValue: {buf[j].IntValue}");
                    }
                }
            }
        }
    }
}

public class System_ : SystemBase{
    protected override void OnCreate(){
        base.OnCreate();
        Enabled = false;
    }
    protected override void OnUpdate(){
        Debug.Log("System_");
        Entities.
            WithoutBurst().
            ForEach((ref ComponentDataA compA, ref ComponentDataB compB, ref DynamicBuffer<BufferElementData> buf, in SharedComponentDataA SharedComp) => {
                compA.IntValue = 1234 * SharedComp.IntValue;
                // compB = new ComponentDataB(){IntValue = 5678};
                for(int i = 0; i < buf.Length; i++){
                    buf[i] = new BufferElementData(){IntValue = 1111 * (i + 1)};
                    //InternalBufferCapacity(n)を超えても使える。nを超えるとインデクサがヒープを回し始める?(List<>?)
                }
            }).Run();
    }
}

[UpdateInGroup(typeof(SimulationSystemGroup))] //デフォルト
[UpdateBefore(typeof(SystemGroupA))]
public class BeforeSystemGA : SystemBase{
    public JobHandle publicDependency;
    protected override void OnUpdate(){
        Debug.Log("BeforeSystemGA");
        Debug.Log($"Depandency is default?: {Dependency.Equals(default(JobHandle))}");//=>ForEach false, Non true
        Dependency = Entities.ForEach((ref ComponentDataA compA)=>{compA = new ComponentDataA(){IntValue = 456};}).Schedule(Dependency);
        Debug.Log($"Depandency is default?: {Dependency.Equals(default(JobHandle))}");//=>ForEach false, Non true
        publicDependency = Dependency;
        _ = 2 == 3;
        EntityQuery query = GetEntityQuery(typeof(ComponentDataA));
        query.GetArchetypeChunkIterator();
    }
}

[UpdateInGroup(typeof(SystemGroupA), OrderFirst = true)]
public class SystemGAFirst1 : SystemBase{
    public struct MyJob : IJob{
        public void Execute(){Debug.Log("MyJob");}
    }
    protected override void OnUpdate(){
        _ = Dependency; //これはGetEntityQuery(archetypeの集合)やGetComponentTypeHandle(アクセスするcomp(特に書き込み))などの呼び出しから依存関係を推測し自動的に設定される。
                                //GetComponentDataFormEntityとかも? //とにかく複数のWorkerTheradから同じcompに書き込みえる状況を避ける様に設定される?
                                ValueType v = default;
                                ReferenceEquals(v, v);
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
                                //
                                //順序不順でどちらも書き込む、Thread.Sleepを入れてプロファイラ(色も水色以外にできるか)、新しいプロジェクトで、ReadOnlyで書き込むとRunで動く? 
                                //
        Debug.Log("SystemGAFirst1");
        Debug.Log($"Depandency is default?: {Dependency.Equals(default(JobHandle))}");//=>ForEach false, Non true
        Debug.Log($"Depandency is BeforeSystemGA?: {Dependency.Equals(World.GetExistingSystem<BeforeSystemGA>().publicDependency)}");//=>ForEach false, Non false
        Debug.Log($"Depend BeforeSystemGA?: {Dependency.CheckFenceIsDependency(World.GetExistingSystem<BeforeSystemGA>().publicDependency)}"); //ForEach true, Non false
        // var jobHandle = JobHandle.CombineDependencies(new MyJob().Schedule(), Dependency/*これ含めないとエラーがでる*/);
        // Debug.Log($"Dependency.IsJobHandle_Complete: {Dependency.IsJobHandle_Complete()}");
        // Action Complete = () => {Debug.Log("Complete");Dependency.Complete();};//＊
        Debug.Log("Complete");Dependency.Complete();
        JobHandle depend = Dependency;
        // Debug.Log($"Dependency.IsJobHandle_Complete: {Dependency.IsJobHandle_Complete()}");
        Dependency = Entities.ForEach((ref ComponentDataA compA)=>{}).Schedule(new MyJob().Schedule()); //後でも前でも同じ//＊
        Debug.Log($"Depend BeforeSystemGA?: {Dependency.CheckFenceIsDependency(World.GetExistingSystem<BeforeSystemGA>().publicDependency)}"); //ForEach true, Non false
        Debug.Log($"depend.Equals(Dependency): {depend.Equals(Dependency)}");

    }
}
[UpdateInGroup(typeof(SystemGroupA), OrderFirst = true)]
[UpdateAfter(typeof(SystemGAFirst1))]
public class SystemGAFirst2 : SystemBase{
    protected override void OnUpdate(){
        Debug.Log("SystemGAFirst2");
    }
}

[UpdateInGroup(typeof(SystemGroupA))]
// [UpdateBefore(typeof(SystemGA2))]
public class SystemGA1 : SystemBase{
    protected override void OnUpdate(){
        Debug.Log("SystemGA1");
    }
}

[UpdateInGroup(typeof(SystemGroupA))]
[UpdateAfter(typeof(SystemGA1))]
[UpdateBefore(typeof(SystemGroupAA))]
public class SystemGA2 : SystemBase{
    public JobHandle publicDependency;
    protected override void OnUpdate(){
        Debug.Log("SystemGA2");
        Entities.ForEach((in ComponentDataA compA) => {}).Schedule();
        // Debug.Log($"IsCompleted: {Dependency.IsCompleted}");
        publicDependency = Dependency;
    }
}

[UpdateInGroup(typeof(SystemGroupAA))]
// [UpdateBefore(typeof(SystemGAA2))]
public class SystemGAA1 : SystemBase{
    public JobHandle publicDependency;
    protected override void OnUpdate(){
        Debug.Log("SystemGAA1");
        Debug.Log($"Depend SystemGA2?: {Dependency.CheckFenceIsDependency(World.GetExistingSystem<SystemGA2>().publicDependency)}");
        Debug.Log($"Depend SystemGAA2?: {Dependency.CheckFenceIsDependency(World.GetExistingSystem<SystemGAA2>().publicDependency)}");
        Debug.Log($"IsJobHandle_Complete?: {World.GetExistingSystem<SystemGA2>().publicDependency.IsJobHandle_Complete()}");
        Debug.Log($"SystemGA2_IsCompleted: {World.GetExistingSystem<SystemGA2>().publicDependency.IsCompleted}"); //=>false? true

        Entities.ForEach((in ComponentDataA compA) => {}).Schedule();
        // Debug.Log($"IsCompleted: {Dependency.IsCompleted}");
        publicDependency = Dependency;
    }
}
[UpdateInGroup(typeof(SystemGroupAA))]
public class SystemGAA2 : SystemBase{
    public JobHandle publicDependency;
    protected override void OnUpdate(){
        Debug.Log("SystemGAA2");
        Debug.Log($"Depend SystemGA2?: {Dependency.CheckFenceIsDependency(World.GetExistingSystem<SystemGA2>().publicDependency)}");
        Debug.Log($"Depend SystemGAA1?: {Dependency.CheckFenceIsDependency(World.GetExistingSystem<SystemGAA1>().publicDependency)}");
        Entities.ForEach((in ComponentDataA compA) => {}).Schedule();
        // Debug.Log($"IsCompleted: {Dependency.IsCompleted}");
        publicDependency = Dependency;
    }
}
[UpdateInGroup(typeof(SystemGroupA))]
[UpdateAfter(typeof(SystemGroupAA))]
public class SystemGA3 : SystemBase{
    public JobHandle publicDependency;
    protected override void OnUpdate(){
        Debug.Log("SystemGA3");
        Debug.Log($"Depend SystemGA2?: {Dependency.CheckFenceIsDependency(World.GetExistingSystem<SystemGA2>().publicDependency)}");
        Debug.Log($"Depend SystemGAA1?: {Dependency.CheckFenceIsDependency(World.GetExistingSystem<SystemGAA1>().publicDependency)}");
        Debug.Log($"Depend SystemGAA2?: {Dependency.CheckFenceIsDependency(World.GetExistingSystem<SystemGAA2>().publicDependency)}");
        Entities.ForEach((in ComponentDataA compA) => {}).Schedule();
        // Debug.Log($"IsCompleted: {Dependency.IsCompleted}");
        publicDependency = Dependency;
    }
}
[UpdateInGroup(typeof(SystemGroupA), OrderLast = true)]
public class SystemGALast : SystemBase{
    protected override void OnUpdate(){
        Debug.Log("SystemGALast");
    }
}


[UpdateInGroup(typeof(SimulationSystemGroup))]
public class SystemGroupA : ComponentSystemGroup{}

[UpdateInGroup(typeof(SystemGroupA))]
public class SystemGroupAA : ComponentSystemGroup{}

public struct SharedComponentDataA : ISharedComponentData{
    public int IntValue;
}
public struct SharedComponentDataB : ISharedComponentData, IEquatable<SharedComponentDataB>{
    public int IntValue;
    public ClsForShared cls;
    public bool Equals(SharedComponentDataB shared) => IntValue.Equals(shared.IntValue) && (cls == null? false : cls.Equals(shared.cls));
    public override bool Equals(object obj) => (obj is SharedComponentDataB shared)? Equals(shared) : false ;
    public override int GetHashCode() => IntValue.GetHashCode() ^ (cls == null? 0 : cls.GetHashCode());
}
public class ClsForShared : IEquatable<ClsForShared>{
    public int ClsIntValue;
    public bool Equals(ClsForShared c) => (c == null)? false : ClsIntValue == c.ClsIntValue;
    public override bool Equals(object obj) => (obj is ClsForShared c)? Equals(c) : false ;
    public override int GetHashCode() => ClsIntValue.GetHashCode();
}

[InternalBufferCapacity(8)]
public struct BufferElementData : IBufferElementData{
    public int IntValue;
}

public struct ComponentDataA : IComponentData{
    public int IntValue;
}
public struct ComponentDataB : IComponentData{
    public int IntValue;
}