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

    //테스트 전용 함수
    //공격 이펙트 애니메이션을 받으면 마지막 프레임에 DeActive함수를 실행시키게 할 것
    private async UniTask TestDeActive()
    {
        await UniTask.Delay(
            TimeSpan.FromSeconds(1f), 
            cancellationToken: this.GetCancellationTokenOnDestroy()
        );

        pool.Release(this);
    }

    public void DeActive()
    {
        pool.Release(this);
    }
}
