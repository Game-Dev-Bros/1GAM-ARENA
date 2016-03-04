using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
	public bool debug;

	private Arena arena;

	[Tooltip("Units per second")]
	public float moveSpeed;
	[Tooltip("Degrees per second")]
	public float rotateSpeed;

	void Awake ()
	{
		arena = FindObjectOfType<Arena>();
	}
	
	void Update ()
	{
		if(Input.GetMouseButtonDown(0))
		{
			StartCoroutine(PickDirection());
		}
	}

	void OnDrawGizmos()
	{
		if(!Application.isPlaying || !debug) 
		{
			return;
		}

		DebugExtension.DrawCircle(arena.transform.position, transform.forward, Color.green, arena.radius);

		Vector3 mouseViewport = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		mouseViewport.z = 0;

		Vector3 direction = mouseViewport - transform.position;

		direction = direction.normalized * arena.radius / 4;
		DebugExtension.DrawArrow(transform.position, direction, picking ? Color.green : Color.red);

		Vector3 directionNorm = direction.normalized;
		direction = directionNorm * arena.radius * 2;

		float t;
		if(CircleLineIntersection(direction, transform.position, arena.radius, out t))
		{
			Vector3 endPosition = transform.position + direction * t;
			endPosition -= directionNorm;
			DebugExtension.DrawCircle(endPosition, transform.forward, Color.black);
		}
	}

	bool picking = false;
	private IEnumerator PickDirection()
	{
		if(picking || moving)
		{
			yield break;
		}

		picking = true;

		while(Input.GetMouseButton(0))
		{
			yield return new WaitForEndOfFrame();
		}

		StartCoroutine(Move());
		picking = false;
	}

	bool moving = false;
	private IEnumerator Move()
	{
		moving = true;

		Vector3 mouseViewport = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		mouseViewport.z = 0;

		Vector3 direction = mouseViewport - transform.position;
		Vector3 directionNorm = direction.normalized;
		direction = directionNorm * arena.radius * 2;

		float t;
		CircleLineIntersection(direction, transform.position, arena.radius, out t);

		Vector3 endPosition = transform.position + direction * t;
		endPosition -= directionNorm * 2;

		yield return StartCoroutine(RotateTo(endPosition));
		yield return StartCoroutine(MoveTo(endPosition));
		yield return StartCoroutine(RotateTo(arena.transform.position));

		moving = false;
	}

	private IEnumerator RotateTo(Vector3 position)
	{
		Vector3 direction = (position - transform.position).normalized;

		float angle = Vector3.Angle(transform.up, direction);
		Vector3 cross = Vector3.Cross(transform.up, direction);
		if (cross.z < 0)
		{
			angle = -angle;
		}

		float duration = Mathf.Abs(angle) / rotateSpeed;

		Quaternion startRotation = transform.rotation;
		Quaternion endRotation = transform.rotation * Quaternion.Euler(transform.forward * angle);

		float t = 0;
		while(t < duration)
		{
			t += Time.deltaTime;
			transform.rotation = Quaternion.Lerp(startRotation, endRotation, t / duration);
			yield return new WaitForFixedUpdate();
		}

		yield return null;
	}

	private IEnumerator MoveTo(Vector3 position)
	{
		float distance = Vector3.Distance(transform.position, position);
		float duration = distance / moveSpeed;

		Vector3 startPosition = transform.position;
		Vector3 endPosition = position;

		float t = 0;
		while(t < duration)
		{
			t += Time.deltaTime;
			transform.position = Vector3.Lerp(startPosition, endPosition, t / duration);
			yield return new WaitForFixedUpdate();
		}
	}

	private bool CircleLineIntersection(Vector3 d, Vector3 f, float r, out float t)
	{
		float a = Vector3.Dot(d, d);
		float b = 2*Vector3.Dot(f, d);
		float c = Vector3.Dot(f, f) - r*r ;

		t = 0;

		float discriminant = b*b-4*a*c;
		if( discriminant < 0 )
		{
			// no intersection
			return false;
		}
		else
		{
			// ray didn't totally miss sphere,
			// so there is a solution to
			// the equation.

			discriminant = Mathf.Sqrt(discriminant);

			// either solution may be on or off the ray so need to test both
			// t1 is always the smaller value, because BOTH discriminant and
			// a are nonnegative.
			float t1 = (-b - discriminant)/(2*a);
			float t2 = (-b + discriminant)/(2*a);

			// 3x HIT cases:
			//          -o->             --|-->  |            |  --|->
			// Impale(t1 hit,t2 hit), Poke(t1 hit,t2>1), ExitWound(t1<0, t2 hit), 

			// 3x MISS cases:
			//       ->  o                     o ->              | -> |
			// FallShort (t1>1,t2>1), Past (t1<0,t2<0), CompletelyInside(t1<0, t2>1)

			if( t1 >= 0 && t1 <= 1 )
			{
				// t1 is the intersection, and it's closer than t2
				// (since t1 uses -b - discriminant)
				// Impale, Poke
				t = t1;
				return true ;
			}

			// here t1 didn't intersect so we are either started
			// inside the sphere or completely past it
			if( t2 >= 0 && t2 <= 1 )
			{
				// ExitWound
				t = t2;
				return true ;
			}

			// no intn: FallShort, Past, CompletelyInside
			return false ;
		}
	}
}
