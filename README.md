#Unity Simple Tweener
Simple Tweener for Unity3D
referenced by ITween(https://www.assetstore.unity3d.com/kr/#!/content/84)

#Example

Move GameObject Repeat 2
```
gameObject.AddTween(new TweenMove(xpos,ypos,zpos,delay, easetype).Repeat(2));
```
Move and Scale GameObject Yoyo Eternity
```
gameObject.AddTween(new TweenMove(xpos,ypos,zpos,delay, easetype).Yoyo(-1),
    new TweenScale(xScale,yScale,zScale,delay,easetype).Yoyo(-1));
```
Move GameObject next Scale GameObject next Set Event
```
gameObject.AddTween(new TweenMove(xpos,ypos,zpos,delay, easetype).Next(
      new TweenScale(xScale,yScale,zScale,delay,easetype).EndEvent(()=> {
        //To Do
      }))
    );
```
Move GameObject next Hold 2sec next Scale and Move GameObject
```
gameObject.AddTween(new TweenMove(xpos,ypos,zpos,delay,easetype).Next(
      new TweenDelay(2).Next(
        new TweenScale(xScale,yScale,zScale,delay,easetype),new TweenMove(xpos,ypos,zpos,delay,easetype)
      )
    ));
```


