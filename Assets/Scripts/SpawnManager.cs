using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Prefab Objects")]
    // object to hold the prefab object to allow local manipulation of localScale without changing prefab.
    [SerializeField] private GameObject _tempPrefab;
    // standard, stationary cube
    [SerializeField] private GameObject _cubePrefab;
    // standard cube except with a movement script and an OnCollisionEnter() method.
    [SerializeField] private GameObject _cubePrefabMoving;
    [SerializeField] private GameObject _spherePrefab;
    [SerializeField] private GameObject _spherePrefabMoving;

    [Header("Spawn Position Details")]
    // holds the information of the boundaries the prefabs can spawn within.
    [SerializeField] private Bounds     _boundariesForSpawning;
    // holds the randomly generated position in WorldSpace to spawn the prefab.
    [SerializeField] private Vector3    _spawnPos;
    // three variables holding the rng x,y,z positions in WorldSpace to plug into the spawn Vector.
    [SerializeField] private float _spawnOffsetX;
    [SerializeField] private float _spawnOffsetY;
    [SerializeField] private float _spawnOffsetZ;

    [Header("Size Variables")]
    // holds the rng number to multiply the local scale (size) of the spawned prefab.
    [SerializeField] private float _sizeMultiplier;
    // holds the minimum rng number that the localScale of the spawned prefab can be.
    [Tooltip("The minimum number that the size (localScale) of the spawned prefab can be.\nThis HAS to be smaller than the Max Size variable.")]
    [Range(0.25f, 99f)]
    [SerializeField] private float _minSize = 0.25f;
    // holds the maximum rng number that the localScale of the spawned prefab can be.
    [Tooltip("The maximum number that the size (localScale) of the spawned prefab can be.\nThis HAS to be larger than the Min Size variable.")]
    [Range(0.26f, 100f)]
    [SerializeField] private float _maxSize = 3f;

    [Header("Object Counters")]
    // holds the amount of cube prefabs that have spawned (moving or standard).
    [SerializeField] private ushort _numberOfCubesSpawned;
    [SerializeField] private ushort _numberOfSpheresSpawned;

    public ushort NumberOfCubesSpawned { get { return _numberOfCubesSpawned; } private set { _numberOfCubesSpawned = value; } }
    public ushort NumberOfSpheresSpawned { get { return _numberOfSpheresSpawned; } private set { _numberOfSpheresSpawned = value; } }

    void Start()
    {
        // reset the spawn count at the beginning of the level
        NumberOfCubesSpawned    = 0;
        NumberOfSpheresSpawned  = 0;
    }

    /// <summary>
    /// <para>Spawns a cube at a random position in the Game Scene within the <paramref name="spawnAreaBox_p"></paramref> bounds.</para>
    /// <br>When <paramref name="isMoving_p"></paramref> is passed as true, a moving cube will be spawned. A stationary one will be spawned if false.</br>
    /// </summary>
    /// <param name="spawnAreaBox_p"></param>
    /// <param name="isMoving_p"></param>
    public void SpawnCubeRandomPosition(BoxCollider spawnAreaBox_p, bool isMoving_p)
    {
        // check that our cube prefabs are not null, if they are send an error message and exit this method.
        if (_cubePrefab == null || _cubePrefabMoving == null)
        {
            Debug.LogError($"The <color=blue>cube prefab</color> or the <color=blue>cube prefab moving</color> is null, " +
                $"did you forget to assign them in the inspector?");
            return;
        }

        // setting the local variable that holds the bounds of the spawning area to
        // the passed BoxCollider's bound information
        _boundariesForSpawning = spawnAreaBox_p.bounds;

        // since the centre on the Y axis of the bounds is not 0, we have to manually calculate the 
        // furthest extents on the Y axis. extents.y will return the distance between the centre and the 
        // furthest Y point, NOT the world space position. So to get the actual world space position
        // we simply add or subtract the extent.y value from the centre's Y position (which is a float)
        // depending on if we want the top or bottom value. We have to make the returned extents.y value
        // always positive (Mathf.Abs()) because if the centre is negative we want to make sure the bottom
        // is going further down from the centre rather than up. ((-10) - 5 = -15. (-10) - (-5) = -5.)
        float _bottomYExtent = _boundariesForSpawning.center.y - Mathf.Abs(_boundariesForSpawning.extents.y);
        float _topYExtent = _boundariesForSpawning.center.y + Mathf.Abs(_boundariesForSpawning.extents.y);

        /*
        Debug.Log($"-Y Boundary for spawn: {_bottomYExtent}");
        Debug.Log($"Y Boundary for spawn: {_topYExtent}");
        Debug.Log($"Extents of Boundaries for spawn: {_boundariesForSpawning.extents}");
        Debug.Log($"Centre of Boundaries for spawn: {_boundariesForSpawning.center}");
        */

        // setting each axes position to a random value within the relative axes' farthest extent
        // (Note: since it is a BoxCollider, each axes' extent will be exactly the same all across that face)
        _spawnOffsetY = Random.Range(_bottomYExtent, _topYExtent);
        _spawnOffsetX = Random.Range(-_boundariesForSpawning.extents.x, _boundariesForSpawning.extents.x);
        _spawnOffsetZ = Random.Range(-_boundariesForSpawning.extents.z, _boundariesForSpawning.extents.z);

        // set the rng value for spawn position on each axis to our Vector3's relative axis
        _spawnPos.x = _spawnOffsetX;
        _spawnPos.y = _spawnOffsetY;
        _spawnPos.z = _spawnOffsetZ;

        // check if the maximum size for the localScale multiplication is greater than the minmum.
        // if it's not, decrease the minimum size by 0.01 units, else set to itself.
        _minSize = _maxSize > _minSize ? _minSize : _maxSize - 0.01f;

        // set the final size multiplier for the localScale adjustment of the spawned prefab to a random number
        // between the number resulting from the percentage of the global Scale based on min size,
        // and the number resulting from the percentage of the global Scale based on max size.
        // (if the lossyScale.x = 1, and minSize = 0.25f, the resulting number would be 0.25f)
        // PREFABS THAT ARE NOT INSTANTIATED DO NOT HAVE LOCAL SCALES
        _sizeMultiplier = Random.Range(_cubePrefab.transform.lossyScale.x * _minSize, _cubePrefab.transform.lossyScale.x * _maxSize);
        // check the boolean value of isMoving, relating to our prefab to spawn.
        switch (isMoving_p)
        {
            // if true, meaning we want to spawn the moving version of the prefab.
            case true:
                {                    
                    // create a second temporary GameObject that stores an VALUE TYPE version of the cube prefab
                    // so we don't edit the prefab constantly.
                    GameObject objectDelete = Instantiate(_cubePrefabMoving);
                    _tempPrefab = objectDelete;
                    //Debug.Log($"Name of the temp prefab: {_tempPrefab.name}");
                    // create the object in the scene at the random WorldSpace-spawn position, and a rotation of 0.
                    _tempPrefab = Instantiate(_tempPrefab, _spawnPos, Quaternion.identity);
                    // set the localScale (size) of the temp prefab based on our random size multiplier.
                    _tempPrefab.transform.localScale *= _sizeMultiplier;
                    //Debug.LogWarning($"Local Scale for new Cube prefab: {_tempPrefab.transform.localScale}");
                    //Debug.LogWarning($"Size Multiplier for new Cube prefab: {_sizeMultiplier}");
                    Debug.Log($"Number of Cubes spawned (before increment): {NumberOfCubesSpawned}");
                    // increment the counter for cubes spawned
                    ++NumberOfCubesSpawned;
                    Debug.Log($"Number of Cubes spawned (after increment): {NumberOfCubesSpawned}");

                    Destroy(objectDelete);
                    // an Instantiated (duplicated) GO will have "(Clone)" at the end of it, so we check if this is the case
                    if (_tempPrefab.name.Contains("(Clone)"))
                    {
                        // then create a temporary string to hold the first part of the Substring
                        // the part before the first parenthesis (which is the original name)
                        string newName = _tempPrefab.name.Split('(')[0];
                        // assign the name of the duplicated object to the original name
                        _tempPrefab.name = newName;
                    }
                    Debug.Log($"Name of the temp prefab: {_tempPrefab.name}");
                    break; 
                }
            // if false, meaning we want to spawn the stationary version of the prefab.
            case false: 
                {
                    GameObject objectDelete = Instantiate(_cubePrefab);
                    _tempPrefab = objectDelete;
                    _tempPrefab = Instantiate(_tempPrefab, _spawnPos, Quaternion.identity);
                    _tempPrefab.transform.localScale *= _sizeMultiplier;
                    //Debug.LogWarning($"Local Scale for new Cube prefab: {_tempPrefab.transform.localScale}");
                    //Debug.LogWarning($"Size Multiplier for new Cube prefab: {_sizeMultiplier}");
                    
                    Debug.Log($"Number of Cubes spawned (before increment): {NumberOfCubesSpawned}");
                    ++NumberOfCubesSpawned;
                    Debug.Log($"Number of Cubes spawned (before increment): {NumberOfCubesSpawned}");

                    Destroy(objectDelete);
                    if (_tempPrefab.name.Contains("(Clone)"))
                    {
                        string newName = _tempPrefab.name.Split('(')[0];
                        _tempPrefab.name = newName;
                    }
                    break; 
                }
        }
    }

    /// <summary>
    /// <para>Spawns a sphere at a random position in the Game Scene within the <paramref name="spawnAreaBox_p"></paramref> bounds.</para>
    /// <br>When <paramref name="isMoving_p"></paramref> is passed as true, a moving sphere will be spawned. A stationary one will be spawned if false.</br>
    /// </summary>
    /// <param name="spawnAreaBox_p"></param>
    /// <param name="isMoving_p"></param>
    public void SpawnSphereRandomPosition(BoxCollider spawnAreaBox_p, bool isMoving_p)
    {
        if (_spherePrefab == null || _spherePrefabMoving == null)
        {
            Debug.LogError($"The <color=blue>sphere prefab</color> or the <color=blue>sphere prefab moving</color> is null, " +
                $"did you forget to assign them in the inspector?");
            return;
        }

        _boundariesForSpawning = spawnAreaBox_p.bounds;

        float _bottomYExtent = _boundariesForSpawning.center.y - Mathf.Abs(_boundariesForSpawning.extents.y);
        float _topYExtent = _boundariesForSpawning.center.y + Mathf.Abs(_boundariesForSpawning.extents.y);
               
        _spawnOffsetY = Random.Range(_bottomYExtent, _topYExtent);
        _spawnOffsetX = Random.Range(-_boundariesForSpawning.extents.x, _boundariesForSpawning.extents.x);
        _spawnOffsetZ = Random.Range(-_boundariesForSpawning.extents.z, _boundariesForSpawning.extents.z);

        _spawnPos.x = _spawnOffsetX;
        _spawnPos.y = _spawnOffsetY;
        _spawnPos.z = _spawnOffsetZ;

        // check if the maximum size for the localScale multiplication is greater than the minmum.
        // if it's not, decrease the minimum size by 0.01 units, else set to itself.
        _minSize = _maxSize > _minSize ? _minSize : _maxSize - 0.01f;

        _sizeMultiplier = Random.Range(_spherePrefab.transform.lossyScale.x * _minSize, _spherePrefab.transform.lossyScale.x * _maxSize);

        Debug.Log($"Size Multiplier for new Sphere prefab: {_sizeMultiplier}");

        Debug.Log($"isMoving parameter passed to SpawnManager (Sphere): {isMoving_p}");

        switch (isMoving_p)
        {
            case true:
                {
                    GameObject objectDelete = Instantiate(_spherePrefabMoving);
                    _tempPrefab = objectDelete;
                    Debug.Log($"Name of the temp prefab: {_tempPrefab.name}");
                    _tempPrefab = Instantiate(_tempPrefab, _spawnPos, Quaternion.identity);
                    _tempPrefab.transform.localScale *= _sizeMultiplier;
                    Debug.LogWarning($"Local Scale for new Sphere prefab: {_tempPrefab.transform.localScale}");
                    Debug.LogWarning($"Size Multiplier for new Sphere prefab: {_sizeMultiplier}");
                    // increment the counter for cubes spawned

                    Debug.Log($"Number of Spheres spawned (before increment): {NumberOfSpheresSpawned}");
                    ++NumberOfSpheresSpawned;
                    Debug.Log($"Number of Spheres spawned (after increment): {NumberOfSpheresSpawned}");

                    Destroy(objectDelete);
                    if (_tempPrefab.name.Contains("(Clone)"))
                    {
                        string newName = _tempPrefab.name.Split('(')[0];
                        _tempPrefab.name = newName;
                    }
                    Debug.Log($"Name of the temp prefab: {_tempPrefab.name}");
                    break;
                }
            case false:
                {
                    GameObject objectDelete = Instantiate(_spherePrefab);
                    _tempPrefab = objectDelete;
                    _tempPrefab = Instantiate(_tempPrefab, _spawnPos, Quaternion.identity);
                    _tempPrefab.transform.localScale *= _sizeMultiplier;
                    Debug.LogWarning($"Local Scale for new Sphere prefab: {_tempPrefab.transform.localScale}");
                    Debug.LogWarning($"Size Multiplier for new Sphere prefab: {_sizeMultiplier}");

                    Debug.Log($"Number of Spheres spawned (before increment): {NumberOfSpheresSpawned}");
                    ++NumberOfSpheresSpawned;
                    Debug.Log($"Number of Spheres spawned (before increment): {NumberOfSpheresSpawned}");

                    Destroy(objectDelete);
                    if (_tempPrefab.name.Contains("(Clone)"))
                    {
                        string newName = _tempPrefab.name.Split('(')[0];
                        _tempPrefab.name = newName;
                    }
                    break;
                }
        }
    }

    // remember this is called when values in the inspector are changed.
    private void OnValidate()
    {
        // check if the maximum size for the localScale multiplication is not greater than the minmum.
        // if it's not greater, set min size to max size - 0.01 units, else set to itself.
        _minSize = _minSize > _maxSize ? _maxSize - 0.01f : _minSize;
    }
}
