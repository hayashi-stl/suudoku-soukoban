using System;
using Godot;

//
// Summary:
//     2-element structure that can be used to represent positions in 2D space or any
//     other pair of numeric values.
[Serializable]
public struct Vector2I : IEquatable<Vector2I>
{
    //
    // Summary:
    //     Enumerated index values for the axes. Returned by Godot.Vector2I.MaxAxis and Godot.Vector2I.MinAxis.
    public enum Axis
    {
        //
        // Summary:
        //     The vector's X axis.
        X,
        //
        // Summary:
        //     The vector's Y axis.
        Y
    }

    //
    // Summary:
    //     The vector's X component. Also accessible by using the index position [0].
    public int x;

    //
    // Summary:
    //     The vector's Y component. Also accessible by using the index position [1].
    public int y;

    private static readonly Vector2I _zero = new Vector2I(0, 0);

    private static readonly Vector2I _one = new Vector2I(1, 1);

    private static readonly Vector2I _negOne = new Vector2I(-1, -1);

    private static readonly Vector2I _up = new Vector2I(0, -1);

    private static readonly Vector2I _down = new Vector2I(0, 1);

    private static readonly Vector2I _right = new Vector2I(1, 0);

    private static readonly Vector2I _left = new Vector2I(-1, 0);

    //
    // Summary:
    //     Access vector components using their index.
    //
    // Value:
    //     [0] is equivalent to Godot.Vector2I.x, [1] is equivalent to Godot.Vector2I.y.
    //
    // Exceptions:
    //   T:System.IndexOutOfRangeException:
    //     Thrown when the given the index is not 0 or 1.
    public int this[int index]
    {
        get
        {
            return index switch
            {
                0 => x,
                1 => y,
                _ => throw new IndexOutOfRangeException(),
            };
        }
        set
        {
            switch (index)
            {
                case 0:
                    x = value;
                    break;
                case 1:
                    y = value;
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }

    //
    // Summary:
    //     Zero vector, a vector with all components set to 0.
    //
    // Value:
    //     Equivalent to new Vector2I(0, 0).
    public static Vector2I Zero => _zero;

    //
    // Summary:
    //     Deprecated, please use a negative sign with Godot.Vector2I.One instead.
    //
    // Value:
    //     Equivalent to new Vector2I(-1, -1).
    [Obsolete("Use a negative sign with Vector2I.One instead.")]
    public static Vector2I NegOne => _negOne;

    //
    // Summary:
    //     One vector, a vector with all components set to 1.
    //
    // Value:
    //     Equivalent to new Vector2I(1, 1).
    public static Vector2I One => _one;

    //
    // Summary:
    //     Up unit vector. Y is down in 2D, so this vector points -Y.
    //
    // Value:
    //     Equivalent to new Vector2I(0, -1).
    public static Vector2I Up => _up;

    //
    // Summary:
    //     Down unit vector. Y is down in 2D, so this vector points +Y.
    //
    // Value:
    //     Equivalent to new Vector2I(0, 1).
    public static Vector2I Down => _down;

    //
    // Summary:
    //     Right unit vector. Represents the direction of right.
    //
    // Value:
    //     Equivalent to new Vector2I(1, 0).
    public static Vector2I Right => _right;

    //
    // Summary:
    //     Left unit vector. Represents the direction of left.
    //
    // Value:
    //     Equivalent to new Vector2I(-1, 0).
    public static Vector2I Left => _left;

    //
    // Summary:
    //     Returns a new vector with all components in absolute values (i.e. positive).
    //
    //
    // Returns:
    //     A vector with Godot.Mathf.Abs(System.Single) called on each component.
    public Vector2I Abs()
    {
        return new Vector2I(Math.Abs(x), Math.Abs(y));
    }

    //
    // Summary:
    //     Returns this vector's angle with respect to the X axis, or (1, 0) vector, in
    //     radians. Equivalent to the result of Godot.Mathf.Atan2(System.Single,System.Single)
    //     when called with the vector's Godot.Vector2I.y and Godot.Vector2I.x as parameters:
    //     Mathf.Atan2(v.y, v.x).
    //
    // Returns:
    //     The angle of this vector, in radians.
    public float Angle()
    {
        return Mathf.Atan2(y, x);
    }

    //
    // Summary:
    //     Returns the angle to the given vector, in radians.
    //
    // Parameters:
    //   to:
    //     The other vector to compare this vector to.
    //
    // Returns:
    //     The angle between the two vectors, in radians.
    public float AngleTo(Vector2I to)
    {
        return Mathf.Atan2(Cross(to), Dot(to));
    }

    //
    // Summary:
    //     Returns the angle between the line connecting the two points and the X axis,
    //     in radians.
    //
    // Parameters:
    //   to:
    //     The other vector to compare this vector to.
    //
    // Returns:
    //     The angle between the two vectors, in radians.
    public float AngleToPoint(Vector2I to)
    {
        return Mathf.Atan2(y - to.y, x - to.x);
    }

    //
    // Summary:
    //     Returns the aspect ratio of this vector, the ratio of Godot.Vector2I.x to Godot.Vector2I.y.
    //
    //
    // Returns:
    //     The Godot.Vector2I.x component divided by the Godot.Vector2I.y component.
    public float Aspect()
    {
        return x / y;
    }

    //
    // Summary:
    //     Returns the cross product of this vector and b.
    //
    // Parameters:
    //   b:
    //     The other vector.
    //
    // Returns:
    //     The cross product value.
    public float Cross(Vector2I b)
    {
        return x * b.y - y * b.x;
    }

    //
    // Summary:
    //     Returns the squared distance between this vector and to. This method runs faster
    //     than Godot.Vector2I.DistanceTo(Godot.Vector2I), so prefer it if you need to compare
    //     vectors or need the squared distance for some formula.
    //
    // Parameters:
    //   to:
    //     The other vector to use.
    //
    // Returns:
    //     The squared distance between the two vectors.
    public float DistanceSquaredTo(Vector2I to)
    {
        return (x - to.x) * (x - to.x) + (y - to.y) * (y - to.y);
    }

    //
    // Summary:
    //     Returns the distance between this vector and to.
    //
    // Parameters:
    //   to:
    //     The other vector to use.
    //
    // Returns:
    //     The distance between the two vectors.
    public float DistanceTo(Vector2I to)
    {
        return Mathf.Sqrt((x - to.x) * (x - to.x) + (y - to.y) * (y - to.y));
    }

    //
    // Summary:
    //     Returns the dot product of this vector and with.
    //
    // Parameters:
    //   with:
    //     The other vector to use.
    //
    // Returns:
    //     The dot product of the two vectors.
    public float Dot(Vector2I with)
    {
        return x * with.x + y * with.y;
    }

    //
    // Summary:
    //     Returns true if the vector is normalized, and false otherwise.
    //
    // Returns:
    //     A bool indicating whether or not the vector is normalized.
    public bool IsNormalized()
    {
        return Mathf.Abs(LengthSquared() - 1f) < 1E-06f;
    }

    //
    // Summary:
    //     Returns the length (magnitude) of this vector.
    //
    // Returns:
    //     The length of this vector.
    public float Length()
    {
        return Mathf.Sqrt(x * x + y * y);
    }

    //
    // Summary:
    //     Returns the squared length (squared magnitude) of this vector. This method runs
    //     faster than Godot.Vector2I.Length, so prefer it if you need to compare vectors
    //     or need the squared length for some formula.
    //
    // Returns:
    //     The squared length of this vector.
    public float LengthSquared()
    {
        return x * x + y * y;
    }

    //
    // Summary:
    //     Returns the axis of the vector's largest value. See Godot.Vector2I.Axis. If both
    //     components are equal, this method returns Godot.Vector2I.Axis.X.
    //
    // Returns:
    //     The index of the largest axis.
    public Axis MaxAxis()
    {
        return (x < y) ? Axis.Y : Axis.X;
    }

    //
    // Summary:
    //     Returns the axis of the vector's smallest value. See Godot.Vector2I.Axis. If both
    //     components are equal, this method returns Godot.Vector2I.Axis.Y.
    //
    // Returns:
    //     The index of the smallest axis.
    public Axis MinAxis()
    {
        return (!(x < y)) ? Axis.Y : Axis.X;
    }

    //
    // Summary:
    //     Returns a perpendicular vector rotated 90 degrees counter-clockwise compared
    //     to the original, with the same length.
    //
    // Returns:
    //     The perpendicular vector.
    public Vector2I Perpendicular()
    {
        return new Vector2I(y, -x);
    }

    //
    // Summary:
    //     Returns a vector composed of the Godot.Mathf.PosMod(System.Single,System.Single)
    //     of this vector's components and mod.
    //
    // Parameters:
    //   mod:
    //     A value representing the divisor of the operation.
    //
    // Returns:
    //     A vector with each component Godot.Mathf.PosMod(System.Single,System.Single)
    //     by mod.
    public Vector2I PosMod(int mod)
    {
        Vector2I result = default(Vector2I);
        result.x = Mathf.PosMod(x, mod);
        result.y = Mathf.PosMod(y, mod);
        return result;
    }

    //
    // Summary:
    //     Returns a vector composed of the Godot.Mathf.PosMod(System.Single,System.Single)
    //     of this vector's components and modv's components.
    //
    // Parameters:
    //   modv:
    //     A vector representing the divisors of the operation.
    //
    // Returns:
    //     A vector with each component Godot.Mathf.PosMod(System.Single,System.Single)
    //     by modv's components.
    public Vector2I PosMod(Vector2I modv)
    {
        Vector2I result = default(Vector2I);
        result.x = Mathf.PosMod(x, modv.x);
        result.y = Mathf.PosMod(y, modv.y);
        return result;
    }

    //
    // Summary:
    //     Returns a vector with each component set to one or negative one, depending on
    //     the signs of this vector's components, or zero if the component is zero, by calling
    //     Godot.Mathf.Sign(System.Single) on each component.
    //
    // Returns:
    //     A vector with all components as either 1, -1, or 0.
    public Vector2I Sign()
    {
        Vector2I result = default(Vector2I);
        result.x = Mathf.Sign(x);
        result.y = Mathf.Sign(y);
        return result;
    }

    //
    // Summary:
    //     Returns this vector with each component snapped to the nearest multiple of step.
    //     This can also be used to round to an arbitrary number of decimals.
    //
    // Parameters:
    //   step:
    //     A vector value representing the step size to snap to.
    //
    // Returns:
    //     The snapped vector.
    public Vector2I Snapped(Vector2I step)
    {
        return new Vector2I((int)Mathf.Stepify(x, step.x), (int)Mathf.Stepify(y, step.y));
    }

    //
    // Summary:
    //     Returns a perpendicular vector rotated 90 degrees counter-clockwise compared
    //     to the original, with the same length. Deprecated, will be replaced by Godot.Vector2I.Perpendicular
    //     in 4.0.
    //
    // Returns:
    //     The perpendicular vector.
    public Vector2I Tangent()
    {
        return new Vector2I(y, -x);
    }

    //
    // Summary:
    //     Constructs a new Godot.Vector2I with the given components.
    //
    // Parameters:
    //   x:
    //     The vector's X component.
    //
    //   y:
    //     The vector's Y component.
    public Vector2I(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    //
    // Summary:
    //     Constructs a new Godot.Vector2I from an existing Godot.Vector2I.
    //
    // Parameters:
    //   v:
    //     The existing Godot.Vector2I.
    public Vector2I(Vector2I v)
    {
        x = v.x;
        y = v.y;
    }

    //
    // Summary:
    //     Adds each component of the Godot.Vector2I with the components of the given Godot.Vector2I.
    //
    //
    // Parameters:
    //   left:
    //     The left vector.
    //
    //   right:
    //     The right vector.
    //
    // Returns:
    //     The added vector.
    public static Vector2I operator +(Vector2I left, Vector2I right)
    {
        left.x += right.x;
        left.y += right.y;
        return left;
    }

    //
    // Summary:
    //     Subtracts each component of the Godot.Vector2I by the components of the given
    //     Godot.Vector2I.
    //
    // Parameters:
    //   left:
    //     The left vector.
    //
    //   right:
    //     The right vector.
    //
    // Returns:
    //     The subtracted vector.
    public static Vector2I operator -(Vector2I left, Vector2I right)
    {
        left.x -= right.x;
        left.y -= right.y;
        return left;
    }

    //
    // Summary:
    //     Returns the negative value of the Godot.Vector2I. This is the same as writing
    //     new Vector2I(-v.x, -v.y). This operation flips the direction of the vector while
    //     keeping the same magnitude. With floats, the number zero can be either positive
    //     or negative.
    //
    // Parameters:
    //   vec:
    //     The vector to negate/flip.
    //
    // Returns:
    //     The negated/flipped vector.
    public static Vector2I operator -(Vector2I vec)
    {
        vec.x = -vec.x;
        vec.y = -vec.y;
        return vec;
    }

    //
    // Summary:
    //     Multiplies each component of the Godot.Vector2I by the given System.Single.
    //
    // Parameters:
    //   vec:
    //     The vector to multiply.
    //
    //   scale:
    //     The scale to multiply by.
    //
    // Returns:
    //     The multiplied vector.
    public static Vector2I operator *(Vector2I vec, int scale)
    {
        vec.x *= scale;
        vec.y *= scale;
        return vec;
    }

    //
    // Summary:
    //     Multiplies each component of the Godot.Vector2I by the given System.Single.
    //
    // Parameters:
    //   scale:
    //     The scale to multiply by.
    //
    //   vec:
    //     The vector to multiply.
    //
    // Returns:
    //     The multiplied vector.
    public static Vector2I operator *(int scale, Vector2I vec)
    {
        vec.x *= scale;
        vec.y *= scale;
        return vec;
    }

    //
    // Summary:
    //     Multiplies each component of the Godot.Vector2I by the components of the given
    //     Godot.Vector2I.
    //
    // Parameters:
    //   left:
    //     The left vector.
    //
    //   right:
    //     The right vector.
    //
    // Returns:
    //     The multiplied vector.
    public static Vector2I operator *(Vector2I left, Vector2I right)
    {
        left.x *= right.x;
        left.y *= right.y;
        return left;
    }

    //
    // Summary:
    //     Multiplies each component of the Godot.Vector2I by the given System.Single.
    //
    // Parameters:
    //   vec:
    //     The dividend vector.
    //
    //   divisor:
    //     The divisor value.
    //
    // Returns:
    //     The divided vector.
    public static Vector2I operator /(Vector2I vec, int divisor)
    {
        vec.x /= divisor;
        vec.y /= divisor;
        return vec;
    }

    //
    // Summary:
    //     Divides each component of the Godot.Vector2I by the components of the given Godot.Vector2I.
    //
    //
    // Parameters:
    //   vec:
    //     The dividend vector.
    //
    //   divisorv:
    //     The divisor vector.
    //
    // Returns:
    //     The divided vector.
    public static Vector2I operator /(Vector2I vec, Vector2I divisorv)
    {
        vec.x /= divisorv.x;
        vec.y /= divisorv.y;
        return vec;
    }

    //
    // Summary:
    //     Gets the remainder of each component of the Godot.Vector2I with the components
    //     of the given System.Single. This operation uses truncated division, which is
    //     often not desired as it does not work well with negative numbers. Consider using
    //     Godot.Vector2I.PosMod(System.Single) instead if you want to handle negative numbers.
    //
    //
    // Parameters:
    //   vec:
    //     The dividend vector.
    //
    //   divisor:
    //     The divisor value.
    //
    // Returns:
    //     The remainder vector.
    public static Vector2I operator %(Vector2I vec, int divisor)
    {
        vec.x %= divisor;
        vec.y %= divisor;
        return vec;
    }

    //
    // Summary:
    //     Gets the remainder of each component of the Godot.Vector2I with the components
    //     of the given Godot.Vector2I. This operation uses truncated division, which is
    //     often not desired as it does not work well with negative numbers. Consider using
    //     Godot.Vector2I.PosMod(Godot.Vector2I) instead if you want to handle negative numbers.
    //
    //
    // Parameters:
    //   vec:
    //     The dividend vector.
    //
    //   divisorv:
    //     The divisor vector.
    //
    // Returns:
    //     The remainder vector.
    public static Vector2I operator %(Vector2I vec, Vector2I divisorv)
    {
        vec.x %= divisorv.x;
        vec.y %= divisorv.y;
        return vec;
    }

    //
    // Summary:
    //     Returns true if the vectors are exactly equal. Note: Due to floating-point precision
    //     errors, consider using Godot.Vector2I.IsEqualApprox(Godot.Vector2I) instead, which
    //     is more reliable.
    //
    // Parameters:
    //   left:
    //     The left vector.
    //
    //   right:
    //     The right vector.
    //
    // Returns:
    //     Whether or not the vectors are exactly equal.
    public static bool operator ==(Vector2I left, Vector2I right)
    {
        return left.Equals(right);
    }

    //
    // Summary:
    //     Returns true if the vectors are not equal. Note: Due to floating-point precision
    //     errors, consider using Godot.Vector2I.IsEqualApprox(Godot.Vector2I) instead, which
    //     is more reliable.
    //
    // Parameters:
    //   left:
    //     The left vector.
    //
    //   right:
    //     The right vector.
    //
    // Returns:
    //     Whether or not the vectors are not equal.
    public static bool operator !=(Vector2I left, Vector2I right)
    {
        return !left.Equals(right);
    }

    //
    // Summary:
    //     Compares two Godot.Vector2I vectors by first checking if the X value of the left
    //     vector is less than the X value of the right vector. If the X values are exactly
    //     equal, then it repeats this check with the Y values of the two vectors. This
    //     operator is useful for sorting vectors.
    //
    // Parameters:
    //   left:
    //     The left vector.
    //
    //   right:
    //     The right vector.
    //
    // Returns:
    //     Whether or not the left is less than the right.
    public static bool operator <(Vector2I left, Vector2I right)
    {
        if (left.x == right.x)
        {
            return left.y < right.y;
        }

        return left.x < right.x;
    }

    //
    // Summary:
    //     Compares two Godot.Vector2I vectors by first checking if the X value of the left
    //     vector is greater than the X value of the right vector. If the X values are exactly
    //     equal, then it repeats this check with the Y values of the two vectors. This
    //     operator is useful for sorting vectors.
    //
    // Parameters:
    //   left:
    //     The left vector.
    //
    //   right:
    //     The right vector.
    //
    // Returns:
    //     Whether or not the left is greater than the right.
    public static bool operator >(Vector2I left, Vector2I right)
    {
        if (left.x == right.x)
        {
            return left.y > right.y;
        }

        return left.x > right.x;
    }

    //
    // Summary:
    //     Compares two Godot.Vector2I vectors by first checking if the X value of the left
    //     vector is less than or equal to the X value of the right vector. If the X values
    //     are exactly equal, then it repeats this check with the Y values of the two vectors.
    //     This operator is useful for sorting vectors.
    //
    // Parameters:
    //   left:
    //     The left vector.
    //
    //   right:
    //     The right vector.
    //
    // Returns:
    //     Whether or not the left is less than or equal to the right.
    public static bool operator <=(Vector2I left, Vector2I right)
    {
        if (left.x == right.x)
        {
            return left.y <= right.y;
        }

        return left.x < right.x;
    }

    //
    // Summary:
    //     Compares two Godot.Vector2I vectors by first checking if the X value of the left
    //     vector is greater than or equal to the X value of the right vector. If the X
    //     values are exactly equal, then it repeats this check with the Y values of the
    //     two vectors. This operator is useful for sorting vectors.
    //
    // Parameters:
    //   left:
    //     The left vector.
    //
    //   right:
    //     The right vector.
    //
    // Returns:
    //     Whether or not the left is greater than or equal to the right.
    public static bool operator >=(Vector2I left, Vector2I right)
    {
        if (left.x == right.x)
        {
            return left.y >= right.y;
        }

        return left.x > right.x;
    }

    //
    // Summary:
    //     Returns true if the vector is exactly equal to the given object (obj). Note:
    //     Due to floating-point precision errors, consider using Godot.Vector2I.IsEqualApprox(Godot.Vector2I)
    //     instead, which is more reliable.
    //
    // Parameters:
    //   obj:
    //     The object to compare with.
    //
    // Returns:
    //     Whether or not the vector and the object are equal.
    public override bool Equals(object obj)
    {
        if (obj is Vector2I)
        {
            return Equals((Vector2I)obj);
        }

        return false;
    }

    //
    // Summary:
    //     Returns true if the vectors are exactly equal. Note: Due to floating-point precision
    //     errors, consider using Godot.Vector2I.IsEqualApprox(Godot.Vector2I) instead, which
    //     is more reliable.
    //
    // Parameters:
    //   other:
    //     The other vector.
    //
    // Returns:
    //     Whether or not the vectors are exactly equal.
    public bool Equals(Vector2I other)
    {
        return x == other.x && y == other.y;
    }

    //
    // Summary:
    //     Returns true if this vector and other are approximately equal, by running Godot.Mathf.IsEqualApprox(System.Single,System.Single)
    //     on each component.
    //
    // Parameters:
    //   other:
    //     The other vector to compare.
    //
    // Returns:
    //     Whether or not the vectors are approximately equal.
    public bool IsEqualApprox(Vector2I other)
    {
        return Mathf.IsEqualApprox(x, other.x) && Mathf.IsEqualApprox(y, other.y);
    }

    //
    // Summary:
    //     Serves as the hash function for Godot.Vector2I.
    //
    // Returns:
    //     A hash code for this vector.
    public override int GetHashCode()
    {
        return y.GetHashCode() ^ x.GetHashCode();
    }

    //
    // Summary:
    //     Converts this Godot.Vector2I to a string.
    //
    // Returns:
    //     A string representation of this vector.
    public override string ToString()
    {
        return $"({x}, {y})";
    }

    //
    // Summary:
    //     Converts this Godot.Vector2I to a string with the given format.
    //
    // Returns:
    //     A string representation of this vector.
    public string ToString(string format)
    {
        return "(" + x.ToString(format) + ", " + y.ToString(format) + ")";
    }

    public static explicit operator Vector2(Vector2I v) => new Vector2(v.x, v.y);
    public static explicit operator Vector2I(Vector2 v) => new Vector2I((int)v.x, (int)v.y);
}