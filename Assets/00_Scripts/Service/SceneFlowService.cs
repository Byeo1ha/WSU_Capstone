using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFlowService
{
    private readonly ScreenFader screenFader;

    private const string LoadingScene = "LoadingScene"; // 로딩 씬 이름

    public float Progress { get; private set; } // 로딩 진행도
    public bool IsTransitioning { get; private set; } // 씬 전환 중인지 여부

    // 생성자 함수
    public SceneFlowService(ScreenFader screenFader)
    {
        this.screenFader = screenFader;
    }

    // 로딩 씬 함수
    public async UniTask LoadSceneAsync(string targetScene)
    {
        if (IsTransitioning) return;

        IsTransitioning = true;
        Progress = 0f;

        try
        {
            // 1단계 : 페이드 아웃 진행
            await screenFader.FadeOutAsync();

            // 2단계 : 로딩씬으로 전환
            await SceneManager.LoadSceneAsync(LoadingScene);

            // 3단계 : 페이드 인 진행
            await screenFader.FadeInAsync();

            // 4단계 : 목표 씬으로 비동기 로드
            AsyncOperation operation =
                SceneManager.LoadSceneAsync(targetScene);

            // 5단계 : 로딩이 완료되어도 미리 씬이 전환되지 않도록 방지하기
            operation.allowSceneActivation = false;

            // 6단계 : 로드 진행도가 0.9가 될때까지 대기
            while (operation.progress < 0.9f)
            {
                Progress = Mathf.Clamp01(operation.progress / 0.9f);

                await UniTask.NextFrame();
            }

            // 7단계 : 로드 진행도를 1로 만들고 페이드 아웃 진행
            Progress = 1f;
            await UniTask.NextFrame();

            await screenFader.FadeOutAsync();

            // 8단계 : 씬 전환을 가능하게 바꾸고, 전환이 완료되면 페이드 인 진행
            operation.allowSceneActivation = true;
            await operation;

            await UniTask.NextFrame();

            await screenFader.FadeInAsync();
        }
        catch(Exception e)
        {
            Debug.LogException(e);
            await screenFader.FadeInAsync();
        }
        finally
        {
            IsTransitioning = false;
        }
    }
}
