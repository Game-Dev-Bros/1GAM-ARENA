using UnityEngine;

public class Arena : MonoBehaviour
{
	public float radius;

	void OnDrawGizmosSelected()
	{
		DebugExtension.DrawCircle(transform.position, transform.forward, Color.green, radius);
	}
}
