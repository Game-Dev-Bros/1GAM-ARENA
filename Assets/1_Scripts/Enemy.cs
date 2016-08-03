using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
	public int health = 100;
	public int damage = 5;

	public float speed = 2f;
	public float stunSpeed = 20f;
	public float stunTime = 5f;
	public float attackTime = 3f;

	private new Rigidbody2D rigidbody;

	private Player _player;
	private Arena _arena;

	private float _attackTimer = float.MaxValue;

	public enum State
	{
		IDLE,
		ENTERING_ARENA,
		RUNNING,
		STUNNED,
		EXITING_ARENA,
	}

	public State state = State.ENTERING_ARENA;
	public State nextState = State.RUNNING;

	public float exitArenaDuration = 3; // seconds

	void Awake()
	{
		rigidbody = GetComponent<Rigidbody2D>();

		_player = FindObjectOfType<Player>();
		_arena = FindObjectOfType<Arena>();
	}

	void Start()
	{
		StartCoroutine(ApplyState());
	}

	IEnumerator ApplyState()
	{
		switch (state)
		{
			case State.ENTERING_ARENA:
				yield return StartCoroutine(EnterArena());
				break;
			case State.STUNNED:
				yield return StartCoroutine(Stun());
				break;
			case State.RUNNING:
				yield return StartCoroutine(Run());
				break;
			case State.EXITING_ARENA:
				yield return StartCoroutine(ExitArena());
				break;
		}

		state = nextState;

		yield return StartCoroutine(ApplyState());
	}

	IEnumerator EnterArena()
	{
		Vector3 direction = (-transform.position).normalized;
		float angle = Utils.GetShortestRotationAngle(direction, transform.up);

		transform.Rotate(Vector3.forward, angle);

		float maxDistance = 2;
		float distance = 0;
		float step = speed * Time.fixedDeltaTime;

		while (distance < maxDistance)
		{
			distance += step;

			rigidbody.MovePosition(transform.position + transform.up * step);
			yield return new WaitForFixedUpdate();
		}

		yield return null;
	}

	IEnumerator ExitArena()
	{
		Destroy(rigidbody);
		GetComponent<SpriteRenderer>().sortingOrder = 3;

		float duration = 0;

		Vector3 originalScale = transform.localScale;
		Vector3 finalScale = new Vector3(20, 20, 0);

		Vector3 originalPosition = transform.position;
		Vector3 direction = (transform.position - _player.transform.position).normalized;
		Vector3 finalPosition = originalPosition + direction * Random.Range(10f, 30f);
		finalPosition.z = -10;

		while (duration < exitArenaDuration)
		{
			duration += Time.deltaTime;

			transform.position = Vector3.Lerp(originalPosition, finalPosition, duration / exitArenaDuration);
			transform.localScale = Vector3.Lerp(originalScale, finalScale, duration / exitArenaDuration);

			yield return new WaitForEndOfFrame();
		}

		gameObject.SetActive(false);

		yield return null;
	}

	IEnumerator Run()
	{
		Vector3 direction = (_player.transform.position - transform.position).normalized;
		float angle = Utils.GetShortestRotationAngle(direction, transform.up);

		transform.Rotate(Vector3.forward, angle);
		transform.position += direction * speed * Time.deltaTime;

		yield return new WaitForEndOfFrame();
	}

	IEnumerator Stun()
	{
		nextState = State.RUNNING;

		Vector3 direction = (transform.position - _player.transform.position) * _arena.radius * 2;

		float t;
		Utils.CircleLineIntersection(direction, transform.position, _arena.radius, out t);

		Vector3 endPosition = transform.position + direction * t;
		endPosition -= direction.normalized;

		yield return StartCoroutine(StunTo(endPosition));
		yield return new WaitForSeconds(stunTime);
	}

	void ReceiveDamage(int damage)
	{
		health -= damage;
		if (health <= 0)
		{
			nextState = State.EXITING_ARENA;
			health = 0;
		}
		else
			nextState = State.STUNNED;
	}

	private IEnumerator StunTo(Vector3 position)
	{
		float distance = Vector3.Distance(transform.position, position);
		float duration = distance / stunSpeed;

		Vector3 startPosition = transform.position;
		Vector3 endPosition = position;

		float t = 0;
		while (t < duration)
		{
			t += Time.deltaTime;
			transform.position = Vector3.Lerp(startPosition, endPosition, t / duration);
			yield return new WaitForFixedUpdate();
		}
	}

	public void OnCollisionEnter2D(Collision2D collision)
	{
		OnCollisionStay2D(collision);
	}

	public void OnCollisionStay2D(Collision2D collision)
	{
		foreach(ContactPoint2D contact in collision.contacts)
		{
			if(contact.collider.tag == "Player/Head"  && _player.IsMoving() && state != State.STUNNED)
			{
				ReceiveDamage(_player.damage);
				return;
			}
		}

		_attackTimer += Time.deltaTime;

		if(_attackTimer >= attackTime)
		{
			_player.ReceiveDamage(damage);
			_attackTimer = 0;
		}
	}

	public void OnCollisionExit2D(Collision2D collision)
	{
		_attackTimer = 0;
	}
}
