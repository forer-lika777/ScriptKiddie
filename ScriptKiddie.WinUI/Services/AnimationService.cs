using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Hosting;
using System;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI.Services;

public class AnimationService
{
    public static async Task AnimateOpacityEnterAsync(UIElement element, int milliseconds = 300)
    {
        var visual = ElementCompositionPreview.GetElementVisual(element);
        var compositor = visual.Compositor;

        element.Visibility = Visibility.Visible;

        var fadeAnimation = compositor.CreateScalarKeyFrameAnimation();
        fadeAnimation.InsertKeyFrame(1.0f, 1.0f);
        fadeAnimation.InsertKeyFrame(0.0f, 0.0f);
        fadeAnimation.Duration = TimeSpan.FromMilliseconds(milliseconds);

        visual.StartAnimation("Opacity", fadeAnimation);

        await Task.Delay(milliseconds);
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

        await Task.Delay(milliseconds);

        element.Visibility = Visibility.Collapsed;
    }

    public static async Task AnimateMotionEnterAsync(UIElement element, float fromX, float fromY, int milliseconds = 600)
    {
        var visual = ElementCompositionPreview.GetElementVisual(element);
        var compositor = visual.Compositor;

        ElementCompositionPreview.SetIsTranslationEnabled(element, true);

        element.Visibility = Visibility.Visible;

        var motionEasing = compositor.CreateCubicBezierEasingFunction(
            new System.Numerics.Vector2(0.1f, 0.7f),
            new System.Numerics.Vector2(0.15f, 1.0f)
        );
        var fadeEasing = compositor.CreateCubicBezierEasingFunction(
            new System.Numerics.Vector2(0.1f, 0.8f),
            new System.Numerics.Vector2(0.15f, 0.95f)
        );

        var motionXAnimation = compositor.CreateScalarKeyFrameAnimation();
        motionXAnimation.InsertKeyFrame(0.0f, fromX);
        motionXAnimation.InsertKeyFrame(1.0f, 0.0f, motionEasing);
        motionXAnimation.Duration = TimeSpan.FromMilliseconds(milliseconds);

        var motionYAnimation = compositor.CreateScalarKeyFrameAnimation();
        motionYAnimation.InsertKeyFrame(0.0f, fromY);
        motionYAnimation.InsertKeyFrame(1.0f, 0.0f, motionEasing);
        motionYAnimation.Duration = TimeSpan.FromMilliseconds(milliseconds);

        var fadeAnimation = compositor.CreateScalarKeyFrameAnimation();
        fadeAnimation.InsertKeyFrame(0.0f, 0.0f);
        fadeAnimation.InsertKeyFrame(1.0f, 1.0f, fadeEasing);
        fadeAnimation.Duration = TimeSpan.FromMilliseconds(milliseconds);

        visual.StartAnimation("Translation.X", motionXAnimation);
        visual.StartAnimation("Translation.Y", motionYAnimation);
        visual.StartAnimation("Opacity", fadeAnimation);

        await Task.Delay(milliseconds);
    }

    public static async Task AnimateMotionExitAsync(UIElement element, float toX, float toY, int milliseconds = 600)
    {
        var visual = ElementCompositionPreview.GetElementVisual(element);
        var compositor = visual.Compositor;

        ElementCompositionPreview.SetIsTranslationEnabled(element, true);

        var motionEasing = compositor.CreateCubicBezierEasingFunction(
            new System.Numerics.Vector2(0.1f, 0.7f),
            new System.Numerics.Vector2(0.15f, 1.0f)
        );
        var fadeEasing = compositor.CreateCubicBezierEasingFunction(
            new System.Numerics.Vector2(0.1f, 0.8f),
            new System.Numerics.Vector2(0.15f, 0.95f)
        );

        var motionXAnimation = compositor.CreateScalarKeyFrameAnimation();
        motionXAnimation.InsertKeyFrame(0.0f, toX);
        motionXAnimation.InsertKeyFrame(1.0f, 0.0f, motionEasing);
        motionXAnimation.Duration = TimeSpan.FromMilliseconds(milliseconds);

        var motionYAnimation = compositor.CreateScalarKeyFrameAnimation();
        motionYAnimation.InsertKeyFrame(0.0f, toY);
        motionYAnimation.InsertKeyFrame(1.0f, 0.0f, motionEasing);
        motionYAnimation.Duration = TimeSpan.FromMilliseconds(milliseconds);

        var fadeAnimation = compositor.CreateScalarKeyFrameAnimation();
        fadeAnimation.InsertKeyFrame(1.0f, 1.0f);
        fadeAnimation.InsertKeyFrame(0.0f, 0.0f, fadeEasing);
        fadeAnimation.Duration = TimeSpan.FromMilliseconds(milliseconds);

        visual.StartAnimation("Translation.X", motionXAnimation);
        visual.StartAnimation("Translation.Y", motionYAnimation);
        visual.StartAnimation("Opacity", fadeAnimation);

        await Task.Delay(milliseconds);

        element.Visibility = Visibility.Collapsed;
    }
}
