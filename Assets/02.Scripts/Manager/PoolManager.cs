using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Pool;
// 심화: 유니티엔진의 Pool 써보기

public class PoolManager : MonoBehaviour
{
    // 풀링 매니저 싱글톤화
    public static PoolManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 1-1. 배열로 플레이어 폭탄 오브젝트 풀링
    [SerializeField] private GameObject _bombPrefab;
    [SerializeField] private int _poolSize = 10;
    private GameObject[] _bombPool;
    private int _currentIndex = 0;
    private int _poolIndex = 0;
    private int _poolCount = 0;

    // 1-2. 폭탄 오브젝트 풀링 초기화
    private void Start()
    {
        // 폭탄 오브젝트 풀 초기화
        _bombPool = new GameObject[_poolSize];
        for (int i = 0; i < _poolSize; i++)
        {
            _bombPool[i] = Instantiate(_bombPrefab);
            _bombPool[i].SetActive(false);
        }

        // 폭탄 이펙트 오브젝트 풀 초기화
        for (int i = 0; i < _poolSize; i++)
        {
            GameObject effect = Instantiate(_explosionEffectPrefab);
            effect.SetActive(false);
            _explosionEffectPool.Add(effect);
        }

        // 유니티 오브젝트 풀 초기화
        InitializeUnityBombPool();
    }

    // 1-3. 폭탄 오브젝트 풀에서 가져오기
    public GameObject GetBombFromPool()
    {
        for (int i = 0; i < _poolSize; i++)
        {
            _poolIndex = (_currentIndex + i) % _poolSize;
            if (!_bombPool[_poolIndex].activeInHierarchy)
            {
                _currentIndex = (_poolIndex + 1) % _poolSize;
                _poolCount++;
                return _bombPool[_poolIndex];
            }
        }
        Debug.LogWarning("No available bombs in the pool!");
        return null;
    }

    // 1-4. 폭탄 오브젝트 풀에 반환하기
    public void ReturnBombToPool(GameObject bomb)
    {
        bomb.SetActive(false);
        _poolCount--;
    }

    // 2-1. 리스트로 플레이어 폭탄 오브젝트 이펙트 풀링
    [SerializeField] private GameObject _explosionEffectPrefab;
    private List<GameObject> _explosionEffectPool = new List<GameObject>();

    // 2-2. 폭탄 이펙트 오브젝트 풀에서 가져오기 (초기화는 Start에서 처리)
    public GameObject GetExplosionEffectFromPool()
    {
        foreach (var effect in _explosionEffectPool)
        {
            if (!effect.activeInHierarchy)
            {
                return effect;
            }
        }
        Debug.LogWarning("No available explosion effects in the pool!");
        return null;
    }

    // 2-3. 폭탄 이펙트 오브젝트 풀에 반환하기
    public void ReturnExplosionEffectToPool(GameObject effect)
    {
        effect.SetActive(false);
    }

    // 3. 유니티 엔진의 Object Pool로 폭탄 오브젝트를 풀링해보자
    private ObjectPool<GameObject> _unityBombPool;

    private void InitializeUnityBombPool()
    {
        _unityBombPool = new ObjectPool<GameObject>(
            createFunc: () =>
            {
                GameObject bomb = Instantiate(_bombPrefab);
                bomb.SetActive(false);
                return bomb;
            },
            actionOnGet: (bomb) => bomb.SetActive(true),
            actionOnRelease: (bomb) => bomb.SetActive(false),
            actionOnDestroy: (bomb) => Destroy(bomb),
            collectionCheck: false,
            defaultCapacity: _poolSize,
            maxSize: 20
        );
    }

    public GameObject GetBombFromUnityPool()
    {
        return _unityBombPool.Get();
    }

    public void ReturnBombToUnityPool(GameObject bomb)
    {
        _unityBombPool.Release(bomb);
    }
}
