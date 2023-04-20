# Keyframe Reducer In Unity

## Solution

Read the animation curve into several tracks, each track compressed independently.

First, all components of the same variable are combined and stored in the IKeyframeBase implementation.

Then, for each curve, determine whether each keyframe can be interpolated by the two keyframes before and after it, and if so, this frame can be deleted.

## Usage

You can check the scripts under the Editor folder for some examples.

The only interface is 

```csharp
KeyframeReducer reducer = new KeyframeReducer();
reducer.ReduceKeyframes(clip, rotationError, positionError, scaleError, checkData);
```

## Advanced

You can easily derive curve classes of other data types and compress them by implement `IKeyframeBase<T>` and `AnimationCurveBase<T>`, and then add an error function to KeyframeReducer.

## Result

Model source: https://github.com/fish-ken/unity-animation-compressor

|   | before | after |
|:-:|:------:|:-----:|
| Attack | 176.3 | 168.1 |
| Dash | 97.7 | 87.9 |
| Skill | 446.3 | 415.0 |
| Walk | 78.1 | 74.8 |

Error Rate: 0.5%

## Reference

- https://www.bzetu.com/344/.html
- https://en.wikipedia.org/wiki/Cubic_Hermite_spline
- https://zhuanlan.zhihu.com/p/328712220
- https://blog.csdn.net/seizeF/article/details/96368503
- https://github.com/needle-mirror/com.unity.live-capture
- https://github.com/fish-ken/unity-animation-compressor
