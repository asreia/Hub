using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Entities;
using Unity.Burst;
using System.Threading;

public static class ScriptEnable3{
    public const bool Enable = false;
}

public class ECSTest2 : MonoBehaviour{
    World world;
    EntityManager entityManager;
    void Start(){
        enabled = ScriptEnable3.Enable;

        if(!ScriptEnable3.Enable) return;

        world = World.DefaultGameObjectInjectionWorld;
        entityManager = world.EntityManager;

        entityManager.CreateEntity(typeof(CompData));
    }
    void Update(){
        Debug.Log("[UpDate]");
        
        using(NativeArray<Entity> Entity_Array = entityManager.CreateEntityQuery(typeof(CompData)).ToEntityArray(Allocator.Temp)){
            Debug.Log($"CompData.IntValue: {entityManager.GetComponentData<CompData>(Entity_Array[0]).IntValue}");
        }
    }
}

public class System_6 : E_SystemBase2{
    EntityQuery query;
    protected override void OnCreate(){
        base.OnCreate();
        query = GetEntityQuery(typeof(CompData));
    }
    protected override void OnUpdate(){
        Debug.Log("[OnUpDate]");
                                                        //読みにくい。GetChunkArrayAsyncでよくない?
        NativeArray<ArchetypeChunk> Chunk_Array = query.CreateArchetypeChunkArrayAsync(Allocator.TempJob, out JobHandle createChunkArrayHandle);
        ComponentTypeHandle<CompData> CompData_TypeHandle = GetComponentTypeHandle<CompData>();
        
        ECS_IJobFor eCS_IJobFor = new ECS_IJobFor(){m_Chunk_Array = Chunk_Array, m_CompData_TypeHandle = CompData_TypeHandle};

        JobHandle Dependency_createChunkArrayHandle = JobHandle.CombineDependencies(Dependency, createChunkArrayHandle);

        Dependency = eCS_IJobFor.Schedule(Chunk_Array.Length, Dependency_createChunkArrayHandle);
    }
}

[BurstCompile]
public struct ECS_IJobFor : IJobFor{
    [DeallocateOnJobCompletion]
    public NativeArray<ArchetypeChunk> m_Chunk_Array;
    public ComponentTypeHandle<CompData> m_CompData_TypeHandle;
    public void Execute(int _index){
        Debug.Log("[ECS_IJobFor_Execute]"); //Debug.Logは[BurstCompile]でも動く!Burst用のコードが走るみたい?
        // Thread.Sleep(10); //[BurstCompile]を付けて実行すると 内部関数 `System.Threading.Thread :: SleepInternal`が見つかりません とでてBurstが掛からないだけで動く 
        ArchetypeChunk chunk = m_Chunk_Array[_index];
        NativeArray<CompData> CompData_Array = chunk.GetNativeArray(m_CompData_TypeHandle);
        for(int i = 0; i < chunk.Count; i++){
            CompData compData = CompData_Array[i];
            compData.IntValue++;
            CompData_Array[i] = compData;
        }
    }
}

public abstract class E_SystemBase2 : SystemBase{
    protected override void OnCreate(){
        base.OnCreate();
        Enabled = ScriptEnable3.Enable;
    }
}

//コメントアウトすれば多分動く
// public struct CompData : IComponentData{
//     public int IntValue;
// }