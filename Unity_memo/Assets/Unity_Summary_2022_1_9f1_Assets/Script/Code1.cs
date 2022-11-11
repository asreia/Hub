using UnityEngine;

//https://youtu.be/mMW9JdbLskM?list=PLtjAIRnny3h6qGQLbe8Y-L4H3LxggyIa1&t=392
[ExecuteAlways]
public class Code1 : MonoBehaviour
{
    [SerializeField] int Count;
    void Awake()
    {
        Count += 1;
    }
}
