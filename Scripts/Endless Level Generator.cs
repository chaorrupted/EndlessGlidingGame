using System.Collections.Generic;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class EndlessLevelGenerator : MonoBehaviour
{
    // todo: some sort of fog effect so player cannot see the end of the world :-D
    
    [SerializeField] private GameObject rocketman;
    
    [SerializeField] private GameObject initialChunk;
    [SerializeField] private GameObject chunkPrefab;

    private const int CheckAfterFrames = 5;
    private const int ChunkLength = 30;

    private static readonly Vector3 ChunkCreationPosition = new (0, -5, 0);
    private readonly Queue<GameObject> _chunks = new Queue<GameObject>();
    
    private int _frameCounter = 0;
    private int _totalChunksGenerated = 0;
    

    private void Awake()
    {
        for (int i = 0; i < 2; i++)
        {
            var newChunk = Instantiate(chunkPrefab);
            newChunk.transform.position = ChunkCreationPosition;
            _chunks.Enqueue(newChunk);
        }
        _chunks.Enqueue(initialChunk);
    }

    private void Update()
    {
        if (_frameCounter < CheckAfterFrames)
        {
            _frameCounter++;
            return;
        }
        _frameCounter = 0;

        var willGenerateNewChunk = CheckPlayerPosition();
        
        if (willGenerateNewChunk)
        {
            var chunkCenter = SelectNextChunkCenter();
            PlaceChunk(chunkCenter);
        }
    }

    public void Restart()
    {
        foreach (var chunk in _chunks)
        {
            chunk.transform.position = ChunkCreationPosition;
        }
        
        PlaceChunk(Vector3.zero);
        
        _totalChunksGenerated = 0;
        _frameCounter = 0;
    }

    private void PlaceChunk(Vector3 chunkCenter)
    {
        var newChunk = _chunks.Dequeue();
        newChunk.transform.position = chunkCenter;
        // todo: something like newChunk.GetComponent<ChunkRandomizer>.Randomize(); here
        _totalChunksGenerated++;
        _chunks.Enqueue(newChunk);
    }

    private Vector3 SelectNextChunkCenter()
    {
        var centerZ = (_totalChunksGenerated + 1) * ChunkLength;
        return new Vector3(0, 0, centerZ);
    }

    private bool CheckPlayerPosition()
    {
        if (rocketman.transform.position.z > _totalChunksGenerated * ChunkLength - ChunkLength/5)
        {
            return true;
        }

        return false;
    }
}
