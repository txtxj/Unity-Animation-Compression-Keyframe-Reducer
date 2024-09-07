# Keyframe Reducer In Unity

## Solution

Read the animation curve into several tracks, each track compressed independently.

First, all components of the same variable are combined and stored in the `IKeyframeBase` implementation.

Then, for each curve, determine whether each keyframe can be interpolated by the two keyframes before and after it, and if so, this frame can be deleted.

## Usage

The only interface is

```csharp
KeyframeReducer reducer = new KeyframeReducer();
reducer.ReduceKeyframes(clip, rotationError, positionError, scaleError, checkData);
```

You can use the scripts under inside the Editor:

1. As Context Menu of file (`RMB` -> `Citrine` -> `Fast Keyframe Reduction`):

<img src="./Images/context-item.gif"/>

2. As Menu Item for selected file (`Assets` -> `Citrine` -> `Fast Keyframe Reduction`):

<img src="./Images/menu-item.gif"/>

3. As Window (`Tools` -> `Citrine` -> `Keyframe Reducer`)

<img src="./Images/window-item.gif"/>

## Advanced

You can easily derive curve classes of other data types and compress them by implement `IKeyframeBase<T>` and `AnimationCurveBase<T>`, and then add an error function to KeyframeReducer.

## Result

### Single Curve:

<table><tr>
  <td>
    <img src="./Images/origin.png" border=0/>
    <p style="display: block; text-align: center; color: #969696;padding: 10px;">Origin</p>
  </td>
		<td>
    <img src="./Images/reduced.png" border=0/>
    <p style="display: block; text-align: center; color: #969696;padding: 10px;">Reduced</p>
  </td>
</tr></table>

### Model Animation:

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