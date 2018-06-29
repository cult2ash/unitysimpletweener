using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using UnityEditor.Graphs;
#if TextMeshPro
using TMPro;
#endif
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Tweener))]
public class TweenerInspector : Editor
{
	
	void OnEnable()
	{
		
	}

    public override void OnInspectorGUI()
    {

        Tweener tween = (Tweener)target;
        EditorUtility.SetDirty(tween);

        for(int i=0;i<tween.tween.Count;i++)
        {
            EditorGUILayout.LabelField(tween.tween[i].ToString());

        }

    }
}

[System.Serializable]
#endif
#region Tweener
public class Tweener : MonoBehaviour
{
	
	bool bPause = false;
	[SerializeField]
	public List<Tween> tween = new List<Tween> ();
	
	public void Flush (Tween _tw=null)
	{
		if(_tw==null) {
			foreach (Tween tw in tween) {
				tw._duration = 0.000000f;
				tw.update();
			}
		} else
		{
			_tw._duration = 0.000000f;
			_tw.update();
		}
		
	}

	public void Kill(Tween tw=null)
	{
		if(tw==null)
			tween.Clear();
		else tween.Remove(tw);
	}
	
	public void Pause ()
	{
		bPause = true;
		
	}
	
	public void Resume ()
	{
		bPause = false;
		
	}
	
	public void Reset ()
	{
		foreach (Tween tw in tween) {
			tw.Swap ();
			tw._duration = 0.000000f;
		}
	}
	
	public Tween AddTween (params Tween[] tw)
	{
		
		for (int i=0; i<tw.Length; i++) {
			tw [i].tweener = this;
			tw [i].Start ();
			tween.Add (tw [i]);
		}
		return tw[0];

	}
	
	public void Update ()
	{
		if (bPause)
			return;
		
		for (int i=0; i<tween.Count; i++) {
			if (tween [i].update ()) {
				tween.RemoveAt (i);
				i--;
				
				
			}
		}
		if (tween.Count == 0)
		{
			Destroy(gameObject.GetComponent("Tweener"));
			return;
		}

	}
	
}
#endregion

////////////////////////////////////////////////////////////////////////////////////////

public abstract class Tween<T> : Tween
{
	protected T _start, _end, _current;

	public Tween (float del, EasingFunction easeType) : base(del, easeType)
	{
	}
	
	public override void Swap ()
	{
		T temp;
		
		temp = _end;
		_end = _start;
		_start = temp;
		
	}
}

public abstract class Tween
{
	public static GameObject currentGameObject;

	public Tweener tweener;
	public delegate float EasingFunction (float start,float end,float Value);
	
	protected EasingFunction ease;
	protected float _progressTime;
	public float _duration;
	protected float delay0_1;
	Tween[] next;
	public System.Action endAction;

//	DeleEvent delegateEvent;
	int repeatCounter = 0;
	int yoyoCounter = 0;
	bool bSwap = false;
	bool bPause = false;
	
	
	#region EaseType	
    //public static float GetBezierEase(float p0, float p1, float p2 ,float p3, float value)   <--- Totally Fake Code!!
    //{

    //    value = Mathf.Clamp01(value);
    //    float OneMinusT = 1f - value;
    //    return 
    //        (OneMinusT * OneMinusT * OneMinusT * p0+
    //        3f * OneMinusT * OneMinusT * value * p1 +
    //        3f * OneMinusT * value * value * p2 +
    //        value * value * value*p3);
    //}     


	static public float linear (float start, float end, float value)
	{
		return Mathf.Lerp (start, end, value);
	}
	
	static public float clerp (float start, float end, float value)
	{
		float min = 0.0f;
		float max = 360.0f;
		float half = Mathf.Abs ((max - min) * 0.5f);
		float retval = 0.0f;
		float diff = 0.0f;
		if ((end - start) < -half) {
			diff = ((max - start) + end) * value;
			retval = start + diff;
		} else if ((end - start) > half) {
			diff = -((max - end) + start) * value;
			retval = start + diff;
		} else
			retval = start + (end - start) * value;
		return retval;
	}
	
	static public float spring (float start, float end, float value)
	{
		value = Mathf.Clamp01 (value);
		value = (Mathf.Sin (value * Mathf.PI * (0.15f + 3.3f * value * value * value)) * Mathf.Pow (1.4f - (1.4f * value), 2f) + value) * (1f + (1.1f * (1f - value)));
		return start + (end - start) * value;
	}


	
	static public float easeInQuad (float start, float end, float value)
	{
		end -= start;
		return end * value * value + start;
	}
	
	static public float easeOutQuad (float start, float end, float value)
	{
		end -= start;
		return -end * value * (value - 2) + start;
	}
	
	static public float easeInOutQuad (float start, float end, float value)
	{
		value /= .5f;
		end -= start;
		if (value < 1)
			return end * 0.5f * value * value + start;
		value--;
		return -end * 0.5f * (value * (value - 2) - 1) + start;
	}
	
	static public float easeInCubic (float start, float end, float value)
	{
		end -= start;
		return end * value * value * value + start;
	}
	
	static public float easeOutCubic (float start, float end, float value)
	{
		value--;
		end -= start;
		return end * (value * value * value + 1) + start;
	}
	
	static public float easeParabola (float start, float end, float value)
	{
		return end - (end - start) * ((2 * value - 1) * (2 * value - 1));
	}
	
	static public float easeInOutCubic (float start, float end, float value)
	{
		value /= .5f;
		end -= start;
		if (value < 1)
			return end * 0.5f * value * value * value + start;
		value -= 2;
		return end * 0.5f * (value * value * value + 2) + start;
	}
	
	static public float easeInQuart (float start, float end, float value)
	{
		end -= start;
		return end * value * value * value * value + start;
	}
	
	static public float easeOutQuart (float start, float end, float value)
	{
		value--;
		end -= start;
		return -end * (value * value * value * value - 1) + start;
	}
	
	static public float easeInOutQuart (float start, float end, float value)
	{
		value /= .5f;
		end -= start;
		if (value < 1)
			return end * 0.5f * value * value * value * value + start;
		value -= 2;
		return -end * 0.5f * (value * value * value * value - 2) + start;
	}
	
	static public float easeInQuint (float start, float end, float value)
	{
		end -= start;
		return end * value * value * value * value * value + start;
	}
	
	static public float easeOutQuint (float start, float end, float value)
	{
		value--;
		end -= start;
		return end * (value * value * value * value * value + 1) + start;
	}
	
	static public float easeInOutQuint (float start, float end, float value)
	{
		value /= .5f;
		end -= start;
		if (value < 1)
			return end * 0.5f * value * value * value * value * value + start;
		value -= 2;
		return end * 0.5f * (value * value * value * value * value + 2) + start;
	}
	
	static public float easeInSine (float start, float end, float value)
	{
		end -= start;
		return -end * Mathf.Cos (value * (Mathf.PI * 0.5f)) + end + start;
	}
	
	static public float easeOutSine (float start, float end, float value)
	{
		end -= start;
		return end * Mathf.Sin (value * (Mathf.PI * 0.5f)) + start;
	}
	
	static public float easeInOutSine (float start, float end, float value)
	{
		end -= start;
		return -end * 0.5f * (Mathf.Cos (Mathf.PI * value) - 1) + start;
	}
	
	static public float easeInExpo (float start, float end, float value)
	{
		end -= start;
		return end * Mathf.Pow (2, 10 * (value - 1)) + start;
	}
	
	static public float easeOutExpo (float start, float end, float value)
	{
		end -= start;
		return end * (-Mathf.Pow (2, -10 * value) + 1) + start;
	}
	
	static public float easeInOutExpo (float start, float end, float value)
	{
		value /= .5f;
		end -= start;
		if (value < 1)
			return end * 0.5f * Mathf.Pow (2, 10 * (value - 1)) + start;
		value--;
		return end * 0.5f * (-Mathf.Pow (2, -10 * value) + 2) + start;
	}
	
	static public float easeInCirc (float start, float end, float value)
	{
		end -= start;
		return -end * (Mathf.Sqrt (1 - value * value) - 1) + start;
	}
	
	static public float easeOutCirc (float start, float end, float value)
	{
		value--;
		end -= start;
		return end * Mathf.Sqrt (1 - value * value) + start;
	}
	
	static public float easeInOutCirc (float start, float end, float value)
	{
		value /= .5f;
		end -= start;
		if (value < 1)
			return -end * 0.5f * (Mathf.Sqrt (1 - value * value) - 1) + start;
		value -= 2;
		return end * 0.5f * (Mathf.Sqrt (1 - value * value) + 1) + start;
	}
	
	/* GFX47 MOD START */
	static public float easeInBounce (float start, float end, float value)
	{
		end -= start;
		float d = 1f;
		return end - easeOutBounce (0, end, d - value) + start;
	}
	/* GFX47 MOD END */
	
	/* GFX47 MOD START */
	//private float bounce(float start, float end, float value){
	static public float easeOutBounce (float start, float end, float value)
	{
		value /= 1f;
		end -= start;
		if (value < (1 / 2.75f)) {
			return end * (7.5625f * value * value) + start;
		} else if (value < (2 / 2.75f)) {
			value -= (1.5f / 2.75f);
			return end * (7.5625f * (value) * value + .75f) + start;
		} else if (value < (2.5 / 2.75)) {
			value -= (2.25f / 2.75f);
			return end * (7.5625f * (value) * value + .9375f) + start;
		} else {
			value -= (2.625f / 2.75f);
			return end * (7.5625f * (value) * value + .984375f) + start;
		}
	}
	/* GFX47 MOD END */
	
	/* GFX47 MOD START */
	static public float easeInOutBounce (float start, float end, float value)
	{
		end -= start;
		float d = 1f;
		if (value < d * 0.5f)
			return easeInBounce (0, end, value * 2) * 0.5f + start;
		else
			return easeOutBounce (0, end, value * 2 - d) * 0.5f + end * 0.5f + start;
	}
	/* GFX47 MOD END */
	
	static public float easeInBack (float start, float end, float value)
	{
		end -= start;
		value /= 1;
		float s = 1.70158f;
		return end * (value) * value * ((s + 1) * value - s) + start;
	}
	
	static public float easeOutBack (float start, float end, float value)
	{
		float s = 1.70158f;
		end -= start;
		value = (value) - 1;
		return end * ((value) * value * ((s + 1) * value + s) + 1) + start;
	}
	
	static public float easeInOutBack (float start, float end, float value)
	{
		float s = 1.70158f;
		end -= start;
		value /= .5f;
		if ((value) < 1) {
			s *= (1.525f);
			return end * 0.5f * (value * value * (((s) + 1) * value - s)) + start;
		}
		value -= 2;
		s *= (1.525f);
		return end * 0.5f * ((value) * value * (((s) + 1) * value + s) + 2) + start;
	}
	
	static public float punch (float amplitude, float value)
	{
		float s = 9;
		if (value == 0) {
			return 0;
		} else if (value == 1) {
			return 0;
		}
		float period = 1 * 0.3f;
		s = period / (2 * Mathf.PI) * Mathf.Asin (0);
		return (amplitude * Mathf.Pow (2, -10 * value) * Mathf.Sin ((value * 1 - s) * (2 * Mathf.PI) / period));
	}
	
	/* GFX47 MOD START */
	static public float easeInElastic (float start, float end, float value)
	{
		end -= start;
		
		float d = 1f;
		float p = d * .3f;
		float s = 0;
		float a = 0;
		
		if (value == 0)
			return start;
		
		if ((value /= d) == 1)
			return start + end;
		
		if (a == 0f || a < Mathf.Abs (end)) {
			a = end;
			s = p / 4;
		} else {
			s = p / (2 * Mathf.PI) * Mathf.Asin (end / a);
		}
		
		return -(a * Mathf.Pow (2, 10 * (value -= 1)) * Mathf.Sin ((value * d - s) * (2 * Mathf.PI) / p)) + start;
	}		
	/* GFX47 MOD END */
	
	/* GFX47 MOD START */
	//private float elastic(float start, float end, float value){
	static public float easeOutElastic (float start, float end, float value)
	{
		/* GFX47 MOD END */
		//Thank you to rafael.marteleto for fixing this as a port over from Pedro's UnityTween
		end -= start;
		
		float d = 1f;
		float p = d * .3f;
		float s = 0;
		float a = 0;
		
		if (value == 0)
			return start;
		
		if ((value /= d) == 1)
			return start + end;
		
		if (a == 0f || a < Mathf.Abs (end)) {
			a = end;
			s = p * 0.25f;
		} else {
			s = p / (2 * Mathf.PI) * Mathf.Asin (end / a);
		}
		
		return (a * Mathf.Pow (2, -10 * value) * Mathf.Sin ((value * d - s) * (2 * Mathf.PI) / p) + end + start);
	}		
	
	/* GFX47 MOD START */
	static public float easeInOutElastic (float start, float end, float value)
	{
		end -= start;
		
		float d = 1f;
		float p = d * .3f;
		float s = 0;
		float a = 0;
		
		if (value == 0)
			return start;
		
		if ((value /= d * 0.5f) == 2)
			return start + end;
		
		if (a == 0f || a < Mathf.Abs (end)) {
			a = end;
			s = p / 4;
		} else {
			s = p / (2 * Mathf.PI) * Mathf.Asin (end / a);
		}
		
		if (value < 1)
			return -0.5f * (a * Mathf.Pow (2, 10 * (value -= 1)) * Mathf.Sin ((value * d - s) * (2 * Mathf.PI) / p)) + start;
		return a * Mathf.Pow (2, -10 * (value -= 1)) * Mathf.Sin ((value * d - s) * (2 * Mathf.PI) / p) * 0.5f + end + start;
	}
	#endregion

	public void Start ()
	{
		Init ();
		_progressTime = 0;
		draw ();
		
	}
	
	public Tween (float _delay, EasingFunction easeType)
	{
		if (easeType == null)
			ease = linear;
		else
			ease = easeType;
		
		_duration = _delay;
		
	}
	
	public abstract void Swap ();
	
	public Tween Repeat (int _i)
	{
		
		repeatCounter = _i;
		return this;
	}
	
	public Tween Yoyo (int _i)
	{
		
		yoyoCounter = _i;
		return this;
	}
	

	public Tween EndEvent (Action _delegateEvent)
	{
		
		endAction = _delegateEvent;
		return this;
		
	}
	
	public Tween Next (params Tween[] tween)
	{
		next = tween;
		return this;
		
	}
	
	protected void GetDelay ()
	{
		if (_duration <= 0) {
			delay0_1 = 1;
		} else {
			_progressTime += Time.deltaTime;
			
			delay0_1 = (_progressTime) / _duration;
			if (delay0_1 < 0)
				delay0_1 = 0;
			else if (delay0_1 > 1)
				delay0_1 = 1;
		}
	}
	
	public bool update ()
	{
		if (bPause)
			return false;
		GetDelay ();
		draw ();
		return EndCheck ();
		
	}
	
	public abstract void draw ();
	
	public abstract void Init ();
	
	protected bool end ()
	{
		if (yoyoCounter != 0) {
			Swap ();
			_progressTime = _progressTime-_duration;
			
			if (!bSwap) {
				yoyoCounter--;
				bSwap = true;
			} else
				bSwap = false;
			if (endAction != null) {
				currentGameObject = tweener.gameObject;
				endAction ();
			}
			return false;
		} else if (repeatCounter != 0) {
			repeatCounter--;
			//InitRepeat ();
			_progressTime = _progressTime-_duration;
			if (endAction != null) {
				currentGameObject = tweener.gameObject;
				endAction ();
			}
			return false;
		}
		
		if (next != null) {
			for (int i=0; i<next.Length; i++) {
				
				tweener.AddTween (next [i]);
			}

		}
		
		if (endAction != null) {
			currentGameObject = tweener.gameObject;
			endAction ();
		}
		return true;
	}
	
	public bool EndCheck ()
	{
		if (delay0_1 >= 1) {
			return end ();
		}
		return false;
		
	}

}
/////////////////////////////////////////////////////////////////////////////////////////////////////
#region Variety
public class TweenDelay : Tween<float>
{
	public TweenDelay (float del) : base(del,null)
	{
		
		
	}
	
	public override void Init ()
	{
	}
	
	public override void draw ()
	{
	}
	
}

public abstract class TweenHierachy<T> : Tween<Dictionary<GameObject,T>> where T : struct {
	
	public TweenHierachy (float del, EasingFunction easeType) : base(del, easeType)
	{
	}
	
	protected Dictionary<GameObject,T> Values {
		get {
			var dic=new Dictionary<GameObject,T>();
			T? t;
			GameObject go;
			foreach(var tr in tweener.GetComponentsInChildren<Transform>())
			{
				go=tr.gameObject;
				t=GetObject(go);
				if(t!=null)
				{
					dic.Add(go,t.Value);
				}
			}
		
			return dic;
		}
		set
		{
			foreach(var v in value)
			{
				SetObject(v.Key,v.Value);
			}
		}
	}
	
	protected abstract T? GetObject(GameObject go);
	protected abstract void SetObject(GameObject go, T t);
	
	public override void draw ()
	{
	}
}

public abstract class TweenColorBase : TweenHierachy<Color>
{
	protected Color endColor;
	
	public TweenColorBase (float del, EasingFunction easeType) : base(del, easeType)
	{
	}
	
	protected override void SetObject(GameObject go, Color c)
	{
		if (go.GetComponent<Text>())
			go.GetComponent<Text>().color = c;
		else if (go.GetComponent<SpriteRenderer>())
		{
			go.GetComponent<SpriteRenderer>().color = c;
		}
		else if (go.GetComponent<Image>())
		{
			go.GetComponent<Image>().color = c;
		}
		else if (go.GetComponent<Renderer>() && tweener.GetComponent<Renderer>().material.HasProperty("_Color"))
		{
			go.GetComponent<Renderer>().material.color = c;
		}
		else if (go.GetComponent<Light>())
		{
			go.GetComponent<Light>().color = c;
		} 
	}
	
	protected override Color? GetObject(GameObject go)
	{
		if (go.GetComponent<Text>())
			return go.GetComponent<Text>().color;
		if (go.GetComponent<SpriteRenderer>())
		
			return go.GetComponent<SpriteRenderer>().color;
		
		if (go.GetComponent<Image>())
		
			return go.GetComponent<Image>().color;
		
		if (go.GetComponent<Renderer>() && tweener.GetComponent<Renderer>().material.HasProperty("_Color"))
		
			return go.GetComponent<Renderer>().material.color;
		
		if (go.GetComponent<Light>())
		
			return go.GetComponent<Light>().color;

		return null;
	}
	
	
}

public class TweenAlpha : TweenColorBase
{
	
	public TweenAlpha (float _a, float del, EasingFunction easeType=null) : base(del,easeType)
	{
       
		endColor.a = _a;
	
    }
	
	public override void Init ()
	{
		_current=Values;
		_start=Values;
	}
	
	public override void draw ()
	{
		foreach (var s in _start)
		{
			Color c=s.Value;
			c.a= ease (s.Value.a, endColor.a, delay0_1);
			_current[s.Key]=c;
		}
        
		Values=_current;
		
	}
}

public class TweenColor : TweenColorBase
{
	
	public TweenColor (Color _c, float del, EasingFunction easeType=null) : base(del,easeType)
	{
	 
		endColor = _c;
	
	}
	
	public override void Init ()
	{
		_current=Values;
		_start=Values;
	}
	
	public override void draw ()
	{
		foreach (var s in _start)
		{
			_current[s.Key]=new Color(
					ease (s.Value.r, endColor.r, delay0_1),
					ease (s.Value.g, endColor.g, delay0_1),
					ease (s.Value.b, endColor.b, delay0_1),
					ease (s.Value.a, endColor.a, delay0_1));
		}
        
		Values=_current;
		
	}
}

public class TweenScale : Tween<Vector3>
{
	
	public TweenScale (float px, float py, float pz, float del, EasingFunction easeType=null) : base(del,easeType)
	{
		_end = new Vector3 (px, py, pz);
		
	}

	public TweenScale (Vector3 scale, float del, EasingFunction easeType = null) : base(del, easeType)
	{
		_end = scale;
	}

	public override void Init ()
	{
		_start = tweener.transform.localScale;
		
	}
	
	public override void draw ()
	{
		
		_current.x = ease (_start.x, _end.x, delay0_1);
		_current.y = ease (_start.y, _end.y, delay0_1);
		_current.z = ease (_start.z, _end.z, delay0_1);
		
		tweener.transform.localScale = _current;
		
		
	}
}

public class TweenScaleTo : Tween<Vector3>
{

	public TweenScaleTo (float px, float py, float pz, float del, EasingFunction easeType=null) : base(del,easeType)
	{
		_end = new Vector3 (px, py, pz);

	}

	public override void Init ()
	{
		_start = tweener.transform.localScale;
		_end.x = _start.x*_end.x;
		_end.y = _start.y*_end.y;
		_end.z = _start.z*_end.z;
	}

	public override void draw ()
	{

		_current.x = ease (_start.x, _end.x, delay0_1);
		_current.y = ease (_start.y, _end.y, delay0_1);
		_current.z = ease (_start.z, _end.z, delay0_1);

		tweener.transform.localScale = _current;


	}
}

public class TweenBezier : Tween<Vector3>
{
	Vector3 _mid;
	public TweenBezier(Vector3 vec, Vector3 mid, float del, EasingFunction easeType = null) : base(del, easeType)
	{
		_end = vec;
		_mid = mid;
	}

	public override void Init()
	{
		_start = tweener.transform.position;
	}

	public override void draw()
	{
		float t = Mathf.Clamp01(ease(0, 1, delay0_1));
	
		float oneMinusT = 1f - t;
		_current=oneMinusT * oneMinusT * _start +
				2f * oneMinusT * t * _mid +
				t * t * _end;

		tweener.transform.position = _current;
	}


}

public class TweenMove : Tween<Vector3>
{
	public TweenMove(Vector3 vec, float del, EasingFunction easeType = null) : base(del, easeType)
	{
		_end = vec;

	}
	public TweenMove (float px, float py, float pz, float del, EasingFunction easeType=null) : base(del,easeType)
	{
		_end = new Vector3 (px, py, pz);
		
	}
	
	public override void Init ()
	{
		_start = tweener.transform.position;
	}
	
	public override void draw ()
	{
		
		_current.x = ease (_start [0], _end [0], delay0_1);
		_current.y = ease (_start [1], _end [1], delay0_1);
		_current.z = ease (_start [2], _end [2], delay0_1);
		
		tweener.transform.position = _current;
		
	}
	
}

public class TweenMoveTo : Tween<Vector3>
{
	
	public TweenMoveTo (float px, float py, float pz, float del, EasingFunction easeType=null) : base(del,easeType)
	{
		_end = new Vector3(px, py, pz);
	}

	public TweenMoveTo(Vector3 pos, float del, EasingFunction easeType = null) : base(del, easeType)
	{
		_end = pos;
	}

	public override void Init ()
	{
		_start = tweener.transform.localPosition;
		_end += tweener.transform.localPosition;
	}
	
	public override void draw ()
	{
		_current.x = ease (_start [0], _end [0], delay0_1);
		_current.y = ease (_start [1], _end [1], delay0_1);
		_current.z = ease (_start [2], _end [2], delay0_1);
		
		tweener.transform.localPosition = _current;
		
	}
	
}

public class TweenMoveLocal : Tween<Vector3>
{
	
	public TweenMoveLocal (float px, float py, float pz, float del, EasingFunction easeType=null) : base(del,easeType)
	{
		_end = new Vector3 (px, py, pz);
		
	}
	
	public override void Init ()
	{
		_start = tweener.transform.localPosition;
	}
	
	public override void draw ()
	{
		
		_current.x = ease (_start.x, _end.x, delay0_1);
		_current.y = ease (_start.y, _end.y, delay0_1);
		_current.z = ease (_start.z, _end.z, delay0_1);
		
		tweener.transform.localPosition = _current;
		
	}
	
}

public class TweenRotateTo : Tween<Vector3> {

	public TweenRotateTo (float px,float py,float pz, float del, EasingFunction easeType=null) : base(del,easeType)
	{
		_end =new Vector3 (px, py, pz);
	}


	public override void Init ()
	{
		_start = tweener.transform.eulerAngles;
	}

	public override void draw ()
	{
		
		tweener.transform.eulerAngles= Vector3.LerpUnclamped(_start, _end,ease (0, 1, delay0_1));
	}

}

public class TweenRotate : Tween<Vector3> {

	public TweenRotate(float px,float py,float pz, float del, EasingFunction easeType=null) : base(del,easeType)
	{
		_end =new Vector3 (px, py, pz);
	}


	public override void Init ()
	{
		_start = tweener.transform.eulerAngles;
		_end += _start;
	}

	public override void draw ()
	{

		tweener.transform.eulerAngles= Vector3.LerpUnclamped(_start, _end,ease (0, 1, delay0_1));
	}

}

public class TweenSoundFade : Tween<float>
{

	public TweenSoundFade(float px, float del, EasingFunction easeType = null) : base(del, easeType)
	{
		_end = px;
	}

	public override void Init()
	{
		_start = Sound.BgVolume;

	}

	public override void draw()
	{
		Sound.BgVolume = ease(_start, _end, delay0_1);

	}
}

public class TweenPop : TweenScale
{			
	public TweenPop (float del) : base(1,1,1,del,spring)
	{
		
	}
	
	public override void Init ()
	{
		_start = new Vector3 (1e-4f, 1e-4f, 1e-4f);
	}
	
}

public class TweenParabola : Tween<Vector3>
{ 		
	
	public TweenParabola (float px, float py, float pz, float del) : base(del,null)
	{
		_end = new Vector3 (px, py, pz);
		
	}
	
	public override void Init ()
	{
		_start = tweener.transform.localPosition;
		_end += _start;
	}
	
	public override void draw ()
	{
		
		_current.x = linear (_start [0], _end [0], delay0_1);
		_current.y = easeParabola (_start [1], _end [1], delay0_1);
		_current.z = linear (_start [2], _end [2], delay0_1);
		
		tweener.transform.localPosition = _current;

	}

}

public class TweenText : Tween<long> {
	
	Func<long, string> stringCoverter =null;

	public TweenText(long start, long to, float del, Func<long,string> stringConvert=null,EasingFunction easeType=null) :base (del,easeType) {
		_start = start;
		_end = to;
		stringCoverter= stringConvert;
	}

	public override void Init ()
	{}

	public override void draw () {
		double d =	ease (0f, 1f, delay0_1);
		_current = (long)(_start * (1f - d) + _end * d);
		if(stringCoverter == null)
			tweener.GetComponent<Text>().text = string.Format("{0:#,0}", _current);
		else tweener.GetComponent<Text>().text = stringCoverter(_current);
	}
}
#if TextMeshPro
public class TweenTextMeshPro : Tween<long>
{

    Func<long, string> stringCoverter = null;

    public TweenTextMeshPro(long start, long to, float del, Func<long, string> stringConvert = null, EasingFunction easeType = null) : base(del, easeType)
    {
        _start = start;
        _end = to;
        stringCoverter = stringConvert;
    }

    public override void Init()
    { }

    public override void draw()
    {
        double d = ease(0f, 1f, delay0_1);
        _current = (long)(_start * (1f - d) + _end * d);
        if (stringCoverter == null)
            tweener.GetComponent<TextMeshPro>().text = string.Format("{0:#,0}", _current);
        else tweener.GetComponent<TextMeshPro>().text = stringCoverter(_current);
    }
}
public class TweenTextMeshProUGUI : Tween<long>
{

    Func<long, string> stringCoverter = null;

    public TweenTextMeshProUGUI(long start, long to, float del, Func<long, string> stringConvert = null, EasingFunction easeType = null) : base(del, easeType)
    {
        _start = start;
        _end = to;
        stringCoverter = stringConvert;
    }

    public override void Init()
    { }

    public override void draw()
    {
        double d = ease(0f, 1f, delay0_1);
        _current = (long)(_start * (1f - d) + _end * d);
        if (stringCoverter == null)
            tweener.GetComponent<TextMeshProUGUI>().text = string.Format("{0:#,0}", _current);
        else tweener.GetComponent<TextMeshProUGUI>().text = stringCoverter(_current);
    }
}
#endif
public class TweenSlide : Tween<float>
{
    public TweenSlide(float to, float del, EasingFunction easeType = null) : base(del, easeType)
    {
        _end = to;
    }
    public override void Init()
    {
		_start = tweener.GetComponent<Image>().fillAmount;
       
    }
    public override void draw()
    {

        _current = ease(_start, _end, delay0_1);
		tweener.GetComponent<Image>().fillAmount = _current;
    }
}

#endregion

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#region ExtensionClass

public static class GameObjectExtension
{
	public static Tween AddTween (this GameObject obj, params Tween[] tw)
	{
		Tweener tweener;
		if ((tweener = obj.GetComponent<Tweener> ()) == null) {
			tweener = obj.AddComponent<Tweener> ();
			//			tweener.baseObj=this;
		}
		
		return tweener.AddTween (tw);
	}
	
	public static void KillTween (this GameObject obj,Tween tw=null)
	{
		Tweener tweener;

		if ((tweener = obj.GetComponent<Tweener>()) == null)
		{
			return;
		}
		tweener.Kill(tw);
		
	}

	public static void FlushTween (this GameObject obj)
	{
		Tweener tweener;
		
		if ((tweener = obj.GetComponent<Tweener> ()) == null) {
			return;
		}
		tweener.Flush();
		
	}
	
	public static void PauseTween (this GameObject obj)
	{
		Tweener tweener;
		
		if ((tweener = obj.GetComponent<Tweener> ()) == null) {
			return;
		}
		tweener.Pause ();
		
	}
	
	public static void ResumeTween (this GameObject obj)
	{
		Tweener tweener;
		
		if ((tweener = obj.GetComponent<Tweener> ()) == null) {
			return;
		}
		tweener.Resume ();
		
	}

	public static Tween CallDelay (this GameObject obj, float delay, Action action,bool bLoop=false)
	{
		return obj.AddTween(new TweenDelay(delay).EndEvent(action).Repeat(bLoop?-1:0));

	}
	
}

#endregion

