using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// ステージ進行にあわせて敵を出現させるスポナー。
/// インスペクタでウェーブ（出現タイミング・プレハブ・位置）を組んでステージ演出を作る。
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Serializable]
    public class SpawnEntry
    {
        public GameObject prefab;
        public Vector3 position;
        public float delay; // ステージ開始からの経過秒数
    }

    public SpawnEntry[] spawnEntries;

    private void Start()
    {
        StartCoroutine(RunSchedule());
    }

    private IEnumerator RunSchedule()
    {
        float elapsed = 0f;
        foreach (var entry in spawnEntries)
        {
            float wait = entry.delay - elapsed;
            if (wait > 0f) yield return new WaitForSeconds(wait);
            elapsed = entry.delay;

            if (entry.prefab != null)
            {
                Instantiate(entry.prefab, entry.position, Quaternion.identity);
            }
        }
    }
}
