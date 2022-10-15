using System.Collections;
using UnityEditor;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private SpawnManager _spawnManager;
    [SerializeField] private BoxCollider  _spawnBoundsBox;

    [SerializeField] private float  _spawnDelay = 2f;
    [Range(0, 1000)]
    [SerializeField] private ushort _cubeSpawnLimit = 25;
    [Range(0, 1000)]
    [SerializeField] private ushort _sphereSpawnLimit = 25;

    [SerializeField] private bool   _hasReachedMaxSpawnLimit = false;
    [SerializeField] private bool   _isSpawning = false;

    public BoxCollider SpawnBoundsBox { get { return _spawnBoundsBox; } private set { _spawnBoundsBox = value; } }

    // Start is called before the first frame update
    void Start()
    {
        // check that our cube prefabs are not null, if they are send an error message and exit this method.
        if (SpawnBoundsBox == null)
        {
            Debug.LogError($"The <color=blue>Spawn Bounds Box</color> is null, did you forget to assign them in the inspector?");
    #if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
    #endif
        }
        
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!_isSpawning)
        { StartCoroutine(SpawnObject()); }
    }

    private IEnumerator SpawnObject()
    {
        _isSpawning = !_isSpawning;
        do
        {
            /*// if we have reached the spawn limit
            if (_hasReachedMaxSpawnLimit)
            {
                // break out of the SpawnObject() Coroutine
                yield break;
            }*/

            // whenever this Coroutine is called, generate a random number to decide which prefab
            // we are spawning (cube, or sphere?).
            ushort objectToSpawnIndex = (ushort)Random.Range(0, 2);
            // depending on the value will determine which base prefab type is spawned.
            switch (objectToSpawnIndex)
            {
                // cube type
                case 0:
                    {
                        // generate a random number to decide if the prefab is a moving or stationary one.
                        int cubePrefabIndex = Random.Range(0, 2);
                        // before we spawn the prefab, check that we haven't reached the limit of this type of object in the scene
                        // by asking our SpawnManager what the current number of spawned objects of this type is.
                        if (_spawnManager.NumberOfCubesSpawned < _cubeSpawnLimit)
                        {
                            // because C# uniqueness with ints not being able to be used as booleans, we 
                            // are checking to see if the int value for our index is 0 or not.
                            // if it's zero, we spawn a stationary cube, if not we spawn a moving cube.
                            bool isMovingCube = cubePrefabIndex == 0 ? false : true;
                            // calling our SpawnManager to spawn the prefab, passing it the BoxCollider for the boundaries
                            // and the boolean for if the prefab is a stationary or moving one.
                            _spawnManager.SpawnCubeRandomPosition(SpawnBoundsBox, isMovingCube);
                        }
                        break;
                    }
                // sphere type
                case 1:
                    {
                        int spherePrefabIndex = Random.Range(0, 2);
                        if (_spawnManager.NumberOfSpheresSpawned < _sphereSpawnLimit)
                        {
                            bool isMovingSphere = spherePrefabIndex == 0 ? false : true;
                            _spawnManager.SpawnCubeRandomPosition(SpawnBoundsBox, isMovingSphere);
                        }
                        break;
                    }
                // if, for some reason, the index of the prefab type we're spawning is out of the range of
                // the actual available ones we have, write an error to the Console.
                default: { Debug.LogError($"{objectToSpawnIndex} is greater than the number of objects available to spawn."); break; }
            }

            // keep running this spawn Coroutine every X seconds dependant on our spawn delay value.
            yield return new WaitForSeconds(_spawnDelay);

            // if we have reached the maximum spawn limit for all types of objects.
            if (_spawnManager.NumberOfCubesSpawned >= _cubeSpawnLimit
                && _spawnManager.NumberOfSpheresSpawned >= _sphereSpawnLimit)
            { _hasReachedMaxSpawnLimit = true; } // set our local boolean to true to stop spawning objects

        } while (!_hasReachedMaxSpawnLimit);
    }
}
