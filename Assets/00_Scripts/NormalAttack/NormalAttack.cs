using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

public class NormalAttack : MonoBehaviour
{
    private IObjectPool<NormalAttack> pool;

    public void SetPool(IObjectPool<NormalAttack> pool)
    {
        this.pool = pool;
    }

    private void OnEnable()
    {
        TestDeActive().Forget();
    }

    private async UniTask TestDeActive()
    {
        await UniTask.Delay(
            TimeSpan.FromSeconds(2f), 
            cancellationToken: this.GetCancellationTokenOnDestroy()
        );

        pool.Release(this);
    }
}
