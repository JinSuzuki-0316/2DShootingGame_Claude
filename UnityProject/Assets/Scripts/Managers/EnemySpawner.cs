using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// ステージ進行にあわせて敵を出現させるスポナー。
/// スケジュール（spawnEntries）を最後まで実行したら、loopDelay秒待って
/// 最初から繰り返す。これにより敵が無限に出現し続ける。
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Serializable]
    public class SpawnEntry
    {
        public GameObject prefab;
        public Vector3 position;
        public float delay; // 1周の開始からの経過秒数
    }

    public SpawnEntry[] spawnEntries;

    [Header("ループ設定")]
    public bool loop = true;
    public float loopDelay = 3f;       // 1周終わってから次の周が始まるまでの待ち時間
    public float positionYJitter = 0.6f; // 周回ごとに出現位置を少しランダムにずらす（単調さの軽減）

    private void Start()
    {
        StartCoroutine(RunSchedule());
    }

    private IEnumerator RunSchedule()
    {
        do
        {
            float elapsed = 0f;
            foreach (var entry in spawnEntries)
            {
                float wait = entry.delay - elapsed;
                if (wait > 0f) yield return new WaitForSeconds(wait);
                elapsed = entry.delay;

                SpawnOne(entry);
            }

            if (loop)
            {
                yield return new WaitForSeconds(loopDelay);
            }
        }
        while (loop);
    }

    private void SpawnOne(SpawnEntry entry)
    {
        if (entry.prefab == null) return;

        Vector3 pos = entry.position;
        if (positionYJitter > 0f)
        {
            pos.y += UnityEngine.Random.Range(-positionYJitter, positionYJitter);
        }

        Instantiate(entry.prefab, pos, Quaternion.identity);
    }
}

