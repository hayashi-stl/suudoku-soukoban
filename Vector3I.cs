using System;
using Godot;

//
// Summary:
//     3-element integer structure that can be used to represent positions in 3D space or any
//     other pair of integer values.
[Serializable]
public struct Vector3I : IEquatable<Vector3I>
{
    //
    // Summary:
    //     Enumerated index values for the axes. Returned by Godot.Vector3I.MaxAxis and Godot.Vector3I.MinAxis.
    public enum Axis
    {
        //
        // Summary:
        //     The vector's X axis.
        X,
        //
        // Summary:
        //     The vector's Y axis.
        Y,
        //
        // Summary:
        //     The vector's Z axis.
        Z
    }

    //
    // Summary:
    //     The vector's X component. Also accessible by using the index position [0].
    public int x;

    //
    // Summary:
    //     The vector's Y component. Also accessible by using the index position [1].
    public int y;

    //
    // Summary:
    //     The vector's Z component. Also accessible by using the index position [2].
    public int z;

    private static readonly Vector3I _zero = new Vector3I(0, 0, 0);

    private static readonly Vector3I _one = new Vector3I(1, 1, 1);

    private static readonly Vector3I _negOne = new Vector3I(-1, -1, -1);

    private static readonly Vector3I _up = new Vector3I(0, 1, 0);

    private static readonly Vector3I _down = new Vector3I(0, -1, 0);

    private static readonly Vector3I _right = new Vector3I(1, 0, 0);

    private static readonly Vector3I _left = new Vector3I(-1, 0, 0);

    private static readonly Vector3I _forward = new Vector3I(0, 0, -1);

    private static readonly Vector3I _back = new Vector3I(0, 0, 1);

    //
    // Summary:
    //     Access vector components using their index.
    //
    // Value:
    //     [0] is equivalent to Godot.Vector3I.x, [1] is equivalent to Godot.Vector3I.y, [2]
    //     is equivalent to Godot.Vector3I.z.
    //
    // Exceptions:
    //   T:System.IndexOutOfRangeException:
    //     Thrown when the given the index is not 0, 1 or 2.
    public int this[int index]
    {
        get
        {
            return index switch
            {
                0 => x,
                1 => y,
                2 => z,
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
                case 2:
                    z = value;
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }

    // Swizzles
    public Vector2I XY => new Vector2I(x, y);

    //
    // Summary:
    //     Zero vector, a vector with all components set to 0.
    //
    // Value:
    //     Equivalent to new Vector3I(0, 0, 0).
    public static Vector3I Zero => _zero;

    //
    // Summary:
    //     One vector, a vector with all components set to 1.
    //
    // Value:
    //     Equivalent to new Vector3I(1, 1, 1).
    public static Vector3I One => _one;

    //
    // Summary:
    //     Deprecated, please use a negative sign with Godot.Vector3I.One instead.
    //
    // Value:
    //     Equivalent to new Vector3I(-1, -1, -1).
    [Obsolete("Use a negative sign with Vector3I.One instead.")]
    public static Vector3I NegOne => _negOne;

    //
    // Summary:
    //     Up unit vector.
    //
    // Value:
    //     Equivalent to new Vector3I(0, 1, 0).
    public static Vector3I Up => _up;

    //
    // Summary:
    //     Down unit vector.
    //
    // Value:
    //     Equivalent to new Vector3I(0, -1, 0).
    public static Vector3I Down => _down;

    //
    // Summary:
    //     Right unit vector. Represents the local direction of right, and the global direction
    //     of east.
    //
    // Value:
    //     Equivalent to new Vector3I(1, 0, 0).
    public static Vector3I Right => _right;

    //
    // Summary:
    //     Left unit vector. Represents the local direction of left, and the global direction
    //     of west.
    //
    // Value:
    //     Equivalent to new Vector3I(-1, 0, 0).
    public static Vector3I Left => _left;

    //
    // Summary:
    //     Forward unit vector. Represents the local direction of forward, and the global
    //     direction of north.
    //
    // Value:
    //     Equivalent to new Vector3I(0, 0, -1).
    public static Vector3I Forward => _forward;

    //
    // Summary:
    //     Back unit vector. Represents the local direction of back, and the global direction
    //     of south.
    //
    // Value:
    //     Equivalent to new Vector3I(0, 0, 1).
    public static Vector3I Back => _back;


    //
    // Summary:
    //     Returns a new vector with all components in absolute values (i.e. positive).
    //
    //
    // Returns:
    //     A vector with Godot.Mathf.Abs(System.Single) called on each component.
    public Vector3I Abs()
    {
        return new Vector3I(Math.Abs(x), Math.Abs(y), Math.Abs(z));
    }

    //
    // Summary:
    //     Returns the unsigned minimum angle to the given vector, in radians.
    //
    // Parameters:
    //   to:
    //     The other vector to compare this vector to.
    //
    // Returns:
    //     The unsigned angle between the two vectors, in radians.
    public float AngleTo(Vector3I to)
    {
        return Mathf.Atan2(Cross(to).Length(), Dot(to));
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
    //     The cross product vector.
    public Vector3I Cross(Vector3I b)
    {
        return new Vector3I(y * b.z - z * b.y, z * b.x - x * b.z, x * b.y - y * b.x);
    }

    //
    // Summary:
    //     Returns the squared distance between this vector and b. This method runs faster
    //     than Godot.Vector3I.DistanceTo(Godot.Vector3I), so prefer it if you need to compare
    //     vectors or need the squared distance for some formula.
    //
    // Parameters:
    //   b:
    //     The other vector to use.
    //
    // Returns:
    //     The squared distance between the two vectors.
    public float DistanceSquaredTo(Vector3I b)
    {
        return (b - this).LengthSquared();
    }

    //
    // Summary:
    //     Returns the distance between this vector and b.
    //
    // Parameters:
    //   b:
    //     The other vector to use.
    //
    // Returns:
    //     The distance between the two vectors.
    public float DistanceTo(Vector3I b)
    {
        return (b - this).Length();
    }

    //
    // Summary:
    //     Returns the dot product of this vector and b.
    //
    // Parameters:
    //   b:
    //     The other vector to use.
    //
    // Returns:
    //     The dot product of the two vectors.
    public float Dot(Vector3I b)
    {
        return x * b.x + y * b.y + z * b.z;
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
        float num = x * x;
        float num2 = y * y;
        float num3 = z * z;
        return Mathf.Sqrt(num + num2 + num3);
    }

    //
    // Summary:
    //     Returns the squared length (squared magnitude) of this vector. This method runs
    //     faster than Godot.Vector3I.Length, so prefer it if you need to compare vectors
    //     or need the squared length for some formula.
    //
    // Returns:
    //     The squared length of this vector.
    public float LengthSquared()
    {
        float num = x * x;
        float num2 = y * y;
        float num3 = z * z;
        return num + num2 + num3;
    }

    //
    // Summary:
    //     Returns the axis of the vector's largest value. See Godot.Vector3I.Axis. If all
    //     components are equal, this method returns Godot.Vector3I.Axis.X.
    //
    // Returns:
    //     The index of the largest axis.
    public Axis MaxAxis()
    {
        return (!(x < y)) ? ((x < z) ? Axis.Z : Axis.X) : ((!(y < z)) ? Axis.Y : Axis.Z);
    }

    //
    // Summary:
    //     Returns the axis of the vector's smallest value. See Godot.Vector3I.Axis. If all
    //     components are equal, this method returns Godot.Vector3I.Axis.Z.
    //
    // Returns:
    //     The index of the smallest axis.
    public Axis MinAxis()
    {
        return (!(x < y)) ? ((y < z) ? Axis.Y : Axis.Z) : ((!(x < z)) ? Axis.Z : Axis.X);
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
    public Vector3I PosMod(int mod)
    {
        Vector3I result = default(Vector3I);
        result.x = Mathf.PosMod(x, mod);
        result.y = Mathf.PosMod(y, mod);
        result.z = Mathf.PosMod(z, mod);
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
    public Vector3I PosMod(Vector3I modv)
    {
        Vector3I result = default(Vector3I);
        result.x = Mathf.PosMod(x, modv.x);
        result.y = Mathf.PosMod(y, modv.y);
        result.z = Mathf.PosMod(z, modv.z);
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
    public Vector3I Sign()
    {
        Vector3I result = default(Vector3I);
        result.x = Mathf.Sign(x);
        result.y = Mathf.Sign(y);
        result.z = Mathf.Sign(z);
        return result;
    }

    //
    // Summary:
    //     Returns the signed angle to the given vector, in radians. The sign of the angle
    //     is positive in a counter-clockwise direction and negative in a clockwise direction
    //     when viewed from the side specified by the axis.
    //
    // Parameters:
    //   to:
    //     The other vector to compare this vector to.
    //
    //   axis:
    //     The reference axis to use for the angle sign.
    //
    // Returns:
    //     The signed angle between the two vectors, in radians.
    public float SignedAngleTo(Vector3I to, Vector3I axis)
    {
        Vector3I vector = Cross(to);
        float num = Mathf.Atan2(vector.Length(), Dot(to));
        float num2 = vector.Dot(axis);
        return (num2 < 0f) ? (0f - num) : num;
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
    public Vector3I Snapped(Vector3I step)
    {
        return new Vector3I((int)Mathf.Stepify(x, step.x), (int)Mathf.Stepify(y, step.y), (int)Mathf.Stepify(z, step.z));
    }

    //
    // Summary:
    //     Constructs a new Godot.Vector3I with the given components.
    //
    // Parameters:
    //   x:
    //     The vector's X component.
    //
    //   y:
    //     The vector's Y component.
    //
    //   z:
    //     The vector's Z component.
    public Vector3I(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    //
    // Summary:
    //     Constructs a new Godot.Vector3I from an existing Godot.Vector3I.
    //
    // Parameters:
    //   v:
    //     The existing Godot.Vector3I.
    public Vector3I(Vector3I v)
    {
        x = v.x;
        y = v.y;
        z = v.z;
    }

    //
    // Summary:
    //     Adds each component of the Godot.Vector3I with the components of the given Godot.Vector3I.
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
    public static Vector3I operator +(Vector3I left, Vector3I right)
    {
        left.x += right.x;
        left.y += right.y;
        left.z += right.z;
        return left;
    }

    //
    // Summary:
    //     Subtracts each component of the Godot.Vector3I by the components of the given
    //     Godot.Vector3I.
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
    public static Vector3I operator -(Vector3I left, Vector3I right)
    {
        left.x -= right.x;
        left.y -= right.y;
        left.z -= right.z;
        return left;
    }

    //
    // Summary:
    //     Returns the negative value of the Godot.Vector3I. This is the same as writing
    //     new Vector3I(-v.x, -v.y, -v.z). This operation flips the direction of the vector
    //     while keeping the same magnitude. With floats, the number zero can be either
    //     positive or negative.
    //
    // Parameters:
    //   vec:
    //     The vector to negate/flip.
    //
    // Returns:
    //     The negated/flipped vector.
    public static Vector3I operator -(Vector3I vec)
    {
        vec.x = 0 - vec.x;
        vec.y = 0 - vec.y;
        vec.z = 0 - vec.z;
        return vec;
    }

    //
    // Summary:
    //     Multiplies each component of the Godot.Vector3I by the given System.Single.
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
    public static Vector3I operator *(Vector3I vec, int scale)
    {
        vec.x *= scale;
        vec.y *= scale;
        vec.z *= scale;
        return vec;
    }

    //
    // Summary:
    //     Multiplies each component of the Godot.Vector3I by the given System.Single.
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
    public static Vector3I operator *(int scale, Vector3I vec)
    {
        vec.x *= scale;
        vec.y *= scale;
        vec.z *= scale;
        return vec;
    }

    //
    // Summary:
    //     Multiplies each component of the Godot.Vector3I by the components of the given
    //     Godot.Vector3I.
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
    public static Vector3I operator *(Vector3I left, Vector3I right)
    {
        left.x *= right.x;
        left.y *= right.y;
        left.z *= right.z;
        return left;
    }

    //
    // Summary:
    //     Divides each component of the Godot.Vector3I by the given System.Single.
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
    public static Vector3I operator /(Vector3I vec, int divisor)
    {
        vec.x /= divisor;
        vec.y /= divisor;
        vec.z /= divisor;
        return vec;
    }

    //
    // Summary:
    //     Divides each component of the Godot.Vector3I by the components of the given Godot.Vector3I.
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
    public static Vector3I operator /(Vector3I vec, Vector3I divisorv)
    {
        vec.x /= divisorv.x;
        vec.y /= divisorv.y;
        vec.z /= divisorv.z;
        return vec;
    }

    //
    // Summary:
    //     Gets the remainder of each component of the Godot.Vector3I with the components
    //     of the given System.Single. This operation uses truncated division, which is
    //     often not desired as it does not work well with negative numbers. Consider using
    //     Godot.Vector3I.PosMod(System.Single) instead if you want to handle negative numbers.
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
    public static Vector3I operator %(Vector3I vec, int divisor)
    {
        vec.x %= divisor;
        vec.y %= divisor;
        vec.z %= divisor;
        return vec;
    }

    //
    // Summary:
    //     Gets the remainder of each component of the Godot.Vector3I with the components
    //     of the given Godot.Vector3I. This operation uses truncated division, which is
    //     often not desired as it does not work well with negative numbers. Consider using
    //     Godot.Vector3I.PosMod(Godot.Vector3I) instead if you want to handle negative numbers.
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
    public static Vector3I operator %(Vector3I vec, Vector3I divisorv)
    {
        vec.x %= divisorv.x;
        vec.y %= divisorv.y;
        vec.z %= divisorv.z;
        return vec;
    }

    //
    // Summary:
    //     Returns true if the vectors are exactly equal. Note: Due to floating-point precision
    //     errors, consider using Godot.Vector3I.IsEqualApprox(Godot.Vector3I) instead, which
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
    public static bool operator ==(Vector3I left, Vector3I right)
    {
        return left.Equals(right);
    }

    //
    // Summary:
    //     Returns true if the vectors are not equal. Note: Due to floating-point precision
    //     errors, consider using Godot.Vector3I.IsEqualApprox(Godot.Vector3I) instead, which
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
    public static bool operator !=(Vector3I left, Vector3I right)
    {
        return !left.Equals(right);
    }

    //
    // Summary:
    //     Compares two Godot.Vector3I vectors by first checking if the X value of the left
    //     vector is less than the X value of the right vector. If the X values are exactly
    //     equal, then it repeats this check with the Y values of the two vectors, and then
    //     with the Z values. This operator is useful for sorting vectors.
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
    public static bool operator <(Vector3I left, Vector3I right)
    {
        if (left.x == right.x)
        {
            if (left.y == right.y)
            {
                return left.z < right.z;
            }

            return left.y < right.y;
        }

        return left.x < right.x;
    }

    //
    // Summary:
    //     Compares two Godot.Vector3I vectors by first checking if the X value of the left
    //     vector is greater than the X value of the right vector. If the X values are exactly
    //     equal, then it repeats this check with the Y values of the two vectors, and then
    //     with the Z values. This operator is useful for sorting vectors.
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
    public static bool operator >(Vector3I left, Vector3I right)
    {
        if (left.x == right.x)
        {
            if (left.y == right.y)
            {
                return left.z > right.z;
            }

            return left.y > right.y;
        }

        return left.x > right.x;
    }

    //
    // Summary:
    //     Compares two Godot.Vector3I vectors by first checking if the X value of the left
    //     vector is less than or equal to the X value of the right vector. If the X values
    //     are exactly equal, then it repeats this check with the Y values of the two vectors,
    //     and then with the Z values. This operator is useful for sorting vectors.
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
    public static bool operator <=(Vector3I left, Vector3I right)
    {
        if (left.x == right.x)
        {
            if (left.y == right.y)
            {
                return left.z <= right.z;
            }

            return left.y < right.y;
        }

        return left.x < right.x;
    }

    //
    // Summary:
    //     Compares two Godot.Vector3I vectors by first checking if the X value of the left
    //     vector is greater than or equal to the X value of the right vector. If the X
    //     values are exactly equal, then it repeats this check with the Y values of the
    //     two vectors, and then with the Z values. This operator is useful for sorting
    //     vectors.
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
    public static bool operator >=(Vector3I left, Vector3I right)
    {
        if (left.x == right.x)
        {
            if (left.y == right.y)
            {
                return left.z >= right.z;
            }

            return left.y > right.y;
        }

        return left.x > right.x;
    }

    //
    // Summary:
    //     Returns true if the vector is exactly equal to the given object (obj). Note:
    //     Due to floating-point precision errors, consider using Godot.Vector3I.IsEqualApprox(Godot.Vector3I)
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
        if (obj is Vector3I)
        {
            return Equals((Vector3I)obj);
        }

        return false;
    }

    //
    // Summary:
    //     Returns true if the vectors are exactly equal. Note: Due to floating-point precision
    //     errors, consider using Godot.Vector3I.IsEqualApprox(Godot.Vector3I) instead, which
    //     is more reliable.
    //
    // Parameters:
    //   other:
    //     The other vector.
    //
    // Returns:
    //     Whether or not the vectors are exactly equal.
    public bool Equals(Vector3I other)
    {
        return x == other.x && y == other.y && z == other.z;
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
    public bool IsEqualApprox(Vector3I other)
    {
        return Mathf.IsEqualApprox(x, other.x) && Mathf.IsEqualApprox(y, other.y) && Mathf.IsEqualApprox(z, other.z);
    }

    //
    // Summary:
    //     Serves as the hash function for Godot.Vector3I.
    //
    // Returns:
    //     A hash code for this vector.
    public override int GetHashCode()
    {
        return y.GetHashCode() ^ x.GetHashCode() ^ z.GetHashCode();
    }

    //
    // Summary:
    //     Converts this Godot.Vector3I to a string.
    //
    // Returns:
    //     A string representation of this vector.
    public override string ToString()
    {
        return $"({x}, {y}, {z})";
    }

    //
    // Summary:
    //     Converts this Godot.Vector3I to a string with the given format.
    //
    // Returns:
    //     A string representation of this vector.
    public string ToString(string format)
    {
        return "(" + x.ToString(format) + ", " + y.ToString(format) + ", " + z.ToString(format) + ")";
    }

    public static explicit operator Vector3(Vector3I v) => new Vector3(v.x, v.y, v.z);
    public static explicit operator Vector3I(Vector3 v) => new Vector3I((int)v.x, (int)v.y, (int)v.z);
}