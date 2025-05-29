using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

public class FadeTransition : MonoBehaviour {
    private Animator animator;

    private void Awake() {
        animator = GetComponent<Animator>();
    }

    public async UniTask FadeIn() {
        animator.SetTrigger("fade_in");
        float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
        await UniTask.Delay(TimeSpan.FromSeconds(animationLength), DelayType.UnscaledDeltaTime, PlayerLoopTiming.Update, new CancellationToken());
    }

    public async UniTask FadeOut() {
        animator.SetTrigger("fade_out");
        float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
        await UniTask.Delay(TimeSpan.FromSeconds(animationLength), DelayType.UnscaledDeltaTime, PlayerLoopTiming.Update, new CancellationToken());
    }
}
