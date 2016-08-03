using UnityEngine;

public static class Utils
{
	public static float GetShortestRotationAngle(Vector3 direction, Vector3 up)
	{
		direction = direction.normalized;
		up = up.normalized;

		float angle = Vector3.Angle(up, direction);
		Vector3 cross = Vector3.Cross(up, direction);
		if (cross.z < 0)
			angle = -angle;

		return angle;
	}

	public static bool CircleLineIntersection(Vector3 direction, Vector3 position, float radius, out float t)
	{
		float a = Vector3.Dot(direction, direction);
		float b = 2*Vector3.Dot(position, direction);
		float c = Vector3.Dot(position, position) - radius*radius ;

		t = 0;

		float discriminant = b*b-4*a*c;
		if( discriminant < 0 )
			// no intersection
			return false;
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
