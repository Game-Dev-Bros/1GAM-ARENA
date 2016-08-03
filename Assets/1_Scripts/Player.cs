using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
	public bool debug;

	private Arena arena;
	
	private bool idling = true;
	private bool picking = false;
	private bool moving = false;
	private bool rotating = false;

	[Tooltip("Units per second")]
	public float moveSpeed;
	[Tooltip("Degrees per second")]
	public float rotateSpeed;

	public int health = 100;
	public int damage = 10;

	void Awake ()
	{
		arena = FindObjectOfType<Arena>();
	}
	
	void Update ()
	{
		if(Input.GetMouseButtonDown(0))
			StartCoroutine(PickDirection());
	}

	void OnDrawGizmos()
	{
		if(!Application.isPlaying || !debug) 
			return;

		DebugExtension.DrawCircle(arena.transform.position, transform.forward, Color.green, arena.radius);

		Vector3 mouseViewport = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		mouseViewport.z = 0;

		Vector3 direction = mouseViewport - transform.position;

		direction = direction.normalized * arena.radius / 4;
		DebugExtension.DrawArrow(transform.position, direction, picking ? Color.green : Color.red);

		Vector3 directionNorm = direction.normalized;
		direction = directionNorm * arena.radius * 2;

		float t;
		if(Utils.CircleLineIntersection(direction, transform.position, arena.radius, out t))
		{
			Vector3 endPosition = transform.position + direction * t;
			endPosition -= directionNorm;
			DebugExtension.DrawCircle(endPosition, transform.forward, Color.black);
		}
	}

	private IEnumerator PickDirection()
	{
		if(picking || !idling)
			yield break;

		picking = true;

		while(Input.GetMouseButton(0))
			yield return new WaitForEndOfFrame();

		StartCoroutine(Move());
		picking = false;
	}

	private IEnumerator Move()
	{
		idling = false;

		Vector3 mouseViewport = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		mouseViewport.z = 0;

		Vector3 direction = mouseViewport - transform.position;
		Vector3 directionNorm = direction.normalized;
		direction = directionNorm * arena.radius * 2;

		float t;
		Utils.CircleLineIntersection(direction, transform.position, arena.radius, out t);

		Vector3 endPosition = transform.position + direction * t;
		endPosition -= directionNorm * 2;

		yield return StartCoroutine(RotateTo(endPosition));
		yield return StartCoroutine(MoveTo(endPosition));
		yield return StartCoroutine(RotateTo(arena.transform.position));

		idling = true;
	}

	public bool IsMoving()
	{
		return moving;
	}

	private IEnumerator RotateTo(Vector3 position)
	{
		rotating = true;

		Vector3 direction = (position - transform.position).normalized;
		float angle = Utils.GetShortestRotationAngle(direction, transform.up);

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
		
		rotating = false;
	}

	private IEnumerator MoveTo(Vector3 position)
	{
		moving = true;

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

		moving = false;
	}

	public void ReceiveDamage(int damage)
	{
		health -= damage;
	}
}
