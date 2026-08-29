using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Hosting;
using System;
using System.Collections.Concurrent;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI.Services;

public static class AnimationService
{
    private static readonly ConcurrentDictionary<UIElement, CancellationTokenSource> runningAnimations = new ConcurrentDictionary<UIElement, CancellationTokenSource>();

    private static CancellationToken PrepareNewAnimation(UIElement element)
    {
        // 如果该元素已有正在运行的动画，触发取消
        if (runningAnimations.TryRemove(element, out var oldCts))
        {
            oldCts.Cancel();
            oldCts.Dispose();
        }

        var newCts = new CancellationTokenSource();
        runningAnimations[element] = newCts;
        return newCts.Token;
    }

    private static CancellationTokenSource? exitCts;
    private static CancellationTokenSource? enterCts;

    public static async Task AnimateOpacityEnterAsync(UIElement element, int milliseconds = 300)
    {
        var visual = ElementCompositionPreview.GetElementVisual(element);
        var compositor = visual.Compositor;

        element.Visibility = Visibility.Visible;

        var fadeAnimation = compositor.CreateScalarKeyFrameAnimation();
        fadeAnimation.InsertKeyFrame(0.0f, 0.0f);
        fadeAnimation.InsertKeyFrame(1.0f, 1.0f);
        fadeAnimation.Duration = TimeSpan.FromMilliseconds(milliseconds);

        visual.StartAnimation("Opacity", fadeAnimation);

        try
        {
            await EnterExitAnimationConfirmWait(milliseconds, enterCts, exitCts);
        }
        catch (OperationCanceledException)
        {
            return;
        }
    }

    public static async Task AnimateOpacityExitAsync(UIElement element, int milliseconds = 200)
    {
        var visual = ElementCompositionPreview.GetElementVisual(element);
        var compositor = visual.Compositor;

        var fadeAnimation = compositor.CreateScalarKeyFrameAnimation();
        fadeAnimation.InsertKeyFrame(0.0f, 1.0f);
        fadeAnimation.InsertKeyFrame(1.0f, 0.0f);
        fadeAnimation.Duration = TimeSpan.FromMilliseconds(milliseconds);

        visual.StartAnimation("Opacity", fadeAnimation);

        if (enterCts is not null)
        {
            enterCts.Cancel();
            enterCts.Dispose();
            enterCts = null;
        }

        try
        {
            await EnterExitAnimationConfirmWait(milliseconds, exitCts, enterCts);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        element.Visibility = Visibility.Collapsed;
    }

    public static async Task AnimateMotionEnterAsync(UIElement element, float fromX, float fromY, int milliseconds = 600)
    {
        var token = PrepareNewAnimation(element);

        var visual = ElementCompositionPreview.GetElementVisual(element);
        var compositor = visual.Compositor;

        ElementCompositionPreview.SetIsTranslationEnabled(element, true);

        // 立即还原状态，保证元素可见且位置/透明度复位
        element.Visibility = Visibility.Visible;

        var motionEasing = compositor.CreateCubicBezierEasingFunction(new Vector2(0.1f, 0.7f), new Vector2(0.15f, 1.0f));
        var fadeEasing = compositor.CreateCubicBezierEasingFunction(new Vector2(0.1f, 0.8f), new Vector2(0.15f, 0.95f));
        var duration = TimeSpan.FromMilliseconds(milliseconds);

        var translationAnimation = compositor.CreateVector3KeyFrameAnimation();
        translationAnimation.InsertKeyFrame(0.0f, new Vector3(fromX, fromY, 0f));
        translationAnimation.InsertKeyFrame(1.0f, Vector3.Zero, motionEasing);
        translationAnimation.Duration = duration;

        var fadeAnimation = compositor.CreateScalarKeyFrameAnimation();
        fadeAnimation.InsertKeyFrame(0.0f, 0.0f);
        fadeAnimation.InsertKeyFrame(1.0f, 1.0f, fadeEasing);
        fadeAnimation.Duration = duration;

        var tcs = new TaskCompletionSource<bool>();
        var batch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);

        visual.StartAnimation("Translation", translationAnimation);
        visual.StartAnimation("Opacity", fadeAnimation);

        batch.Completed += (s, e) => tcs.TrySetResult(true);
        batch.End();

        // 注册取消回调：如果在新动画启动前取消，提前结束等待
        using (token.Register(() => tcs.TrySetCanceled()))
        {
            try
            {
                await tcs.Task;
            }
            catch (TaskCanceledException)
            {
                // 动画被中途打断，无需处理
            }
        }
    }

    public static async Task AnimateMotionExitAsync(UIElement element, float toX, float toY, int milliseconds = 600)
    {
        var token = PrepareNewAnimation(element);

        var visual = ElementCompositionPreview.GetElementVisual(element);
        var compositor = visual.Compositor;

        ElementCompositionPreview.SetIsTranslationEnabled(element, true);

        var motionEasing = compositor.CreateCubicBezierEasingFunction(new Vector2(0.1f, 0.7f), new Vector2(0.15f, 1.0f));
        var fadeEasing = compositor.CreateCubicBezierEasingFunction(new Vector2(0.1f, 0.8f), new Vector2(0.15f, 0.95f));
        var duration = TimeSpan.FromMilliseconds(milliseconds);

        var translationAnimation = compositor.CreateVector3KeyFrameAnimation();
        translationAnimation.InsertKeyFrame(0.0f, Vector3.Zero);
        translationAnimation.InsertKeyFrame(1.0f, new Vector3(toX, toY, 0f), motionEasing);
        translationAnimation.Duration = duration;

        var fadeAnimation = compositor.CreateScalarKeyFrameAnimation();
        fadeAnimation.InsertKeyFrame(0.0f, 1.0f);
        fadeAnimation.InsertKeyFrame(0.5f, 0.0f, fadeEasing);
        fadeAnimation.Duration = duration;

        var tcs = new TaskCompletionSource<bool>();
        var batch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);

        visual.StartAnimation("Translation", translationAnimation);
        visual.StartAnimation("Opacity", fadeAnimation);

        batch.Completed += (s, e) => tcs.TrySetResult(true);
        batch.End();

        using (token.Register(() => tcs.TrySetCanceled()))
        {
            try
            {
                await tcs.Task;

                // 关键点：只有在未被取消的情况下，才将控件设置为 Collapsed 并复位状态
                element.Visibility = Visibility.Collapsed;
                visual.Opacity = 1.0f;
                visual.Properties.InsertVector3("Translation", Vector3.Zero);
            }
            catch (TaskCanceledException)
            {
                // 如果退出动画被打断（例如紧接着调用了 Enter 动画），
                // 直接跳过设为 Collapsed 的逻辑！
            }
        }
    }

    private static async Task EnterExitAnimationConfirmWait(int milliseconds, CancellationTokenSource? thisSideCts, CancellationTokenSource? otherSideCts)
    {
        if (otherSideCts is not null)
        {
            otherSideCts.Cancel();
            otherSideCts.Dispose();
            otherSideCts = null;
        }

        thisSideCts ??= new CancellationTokenSource();
        await Task.Delay(milliseconds, thisSideCts.Token);

        thisSideCts.Cancel();
        thisSideCts.Dispose();
        thisSideCts = null;
    }
}
