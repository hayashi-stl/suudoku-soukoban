using System;
using Godot;

//
// Summary:
//     2D axis-aligned bounding box. Rect2I consists of a position, a size, and several
//     utility functions. It is typically used for fast overlap tests.
[Serializable]
public struct Rect2I : IEquatable<Rect2I>
{
    private Vector2I _position;

    private Vector2I _size;

    //
    // Summary:
    //     Beginning corner. Typically has values lower than Godot.Rect2I.End.
    //
    // Value:
    //     Directly uses a private field.
    public Vector2I Position
    {
        get
        {
            return _position;
        }
        set
        {
            _position = value;
        }
    }

    //
    // Summary:
    //     Size from Godot.Rect2I.Position to Godot.Rect2I.End. Typically all components are
    //     positive. If the size is negative, you can use Godot.Rect2I.Abs to fix it.
    //
    // Value:
    //     Directly uses a private field.
    public Vector2I Size
    {
        get
        {
            return _size;
        }
        set
        {
            _size = value;
        }
    }

    //
    // Summary:
    //     Ending corner. This is calculated as Godot.Rect2I.Position plus Godot.Rect2I.Size.
    //     Setting this value will change the size.
    //
    // Value:
    //     Getting is equivalent to value = Godot.Rect2I.Position + Godot.Rect2I.Size, setting
    //     is equivalent to Godot.Rect2I.Size = value - Godot.Rect2I.Position
    public Vector2I End
    {
        get
        {
            return _position + _size;
        }
        set
        {
            _size = value - _position;
        }
    }

    //
    // Summary:
    //     The area of this Godot.Rect2I.
    //
    // Value:
    //     Equivalent to Godot.Rect2I.GetArea.
    public float Area => GetArea();

    //
    // Summary:
    //     Returns a Godot.Rect2I with equivalent position and size, modified so that the
    //     top-left corner is the origin and width and height are positive.
    //
    // Returns:
    //     The modified Godot.Rect2I.
    public Rect2I Abs()
    {
        Vector2I end = End;
        Vector2I position = new Vector2I(Mathf.Min(_position.x, end.x), Mathf.Min(_position.y, end.y));
        return new Rect2I(position, _size.Abs());
    }

    //
    // Summary:
    //     Returns the intersection of this Godot.Rect2I and b. If the rectangles do not
    //     intersect, an empty Godot.Rect2I is returned.
    //
    // Parameters:
    //   b:
    //     The other Godot.Rect2I.
    //
    // Returns:
    //     The clipped Godot.Rect2I.
    public Rect2I Clip(Rect2I b)
    {
        Rect2I rect = b;
        if (!Intersects(rect))
        {
            return default(Rect2I);
        }

        rect._position.x = Mathf.Max(b._position.x, _position.x);
        rect._position.y = Mathf.Max(b._position.y, _position.y);
        Vector2I vector = b._position + b._size;
        Vector2I vector2 = _position + _size;
        rect._size.x = Mathf.Min(vector.x, vector2.x) - rect._position.x;
        rect._size.y = Mathf.Min(vector.y, vector2.y) - rect._position.y;
        return rect;
    }

    //
    // Summary:
    //     Returns true if this Godot.Rect2I completely encloses another one.
    //
    // Parameters:
    //   b:
    //     The other Godot.Rect2I that may be enclosed.
    //
    // Returns:
    //     A bool for whether or not this Godot.Rect2I encloses b.
    public bool Encloses(Rect2I b)
    {
        return b._position.x >= _position.x && b._position.y >= _position.y && b._position.x + b._size.x < _position.x + _size.x && b._position.y + b._size.y < _position.y + _size.y;
    }

    //
    // Summary:
    //     Returns this Godot.Rect2I expanded to include a given point.
    //
    // Parameters:
    //   to:
    //     The point to include.
    //
    // Returns:
    //     The expanded Godot.Rect2I.
    public Rect2I Expand(Vector2I to)
    {
        Rect2I result = this;
        Vector2I position = result._position;
        Vector2I vector = result._position + result._size;
        if (to.x < position.x)
        {
            position.x = to.x;
        }

        if (to.y < position.y)
        {
            position.y = to.y;
        }

        if (to.x > vector.x)
        {
            vector.x = to.x;
        }

        if (to.y > vector.y)
        {
            vector.y = to.y;
        }

        result._position = position;
        result._size = vector - position;
        return result;
    }

    //
    // Summary:
    //     Returns the area of the Godot.Rect2I.
    //
    // Returns:
    //     The area.
    public float GetArea()
    {
        return _size.x * _size.y;
    }

    //
    // Summary:
    //     Returns the center of the Godot.Rect2I, which is equal to Godot.Rect2I.Position
    //     + (Godot.Rect2I.Size / 2).
    //
    // Returns:
    //     The center.
    public Vector2I GetCenter()
    {
        return _position + _size / 2;
    }

    //
    // Summary:
    //     Returns a copy of the Godot.Rect2I grown a given amount of units towards all the
    //     sides.
    //
    // Parameters:
    //   by:
    //     The amount to grow by.
    //
    // Returns:
    //     The grown Godot.Rect2I.
    public Rect2I Grow(int by)
    {
        Rect2I result = this;
        result._position.x -= by;
        result._position.y -= by;
        result._size.x += by * 2;
        result._size.y += by * 2;
        return result;
    }

    //
    // Summary:
    //     Returns a copy of the Godot.Rect2I grown a given amount of units towards each
    //     direction individually.
    //
    // Parameters:
    //   left:
    //     The amount to grow by on the left.
    //
    //   top:
    //     The amount to grow by on the top.
    //
    //   right:
    //     The amount to grow by on the right.
    //
    //   bottom:
    //     The amount to grow by on the bottom.
    //
    // Returns:
    //     The grown Godot.Rect2I.
    public Rect2I GrowIndividual(int left, int top, int right, int bottom)
    {
        Rect2I result = this;
        result._position.x -= left;
        result._position.y -= top;
        result._size.x += left + right;
        result._size.y += top + bottom;
        return result;
    }

    //
    // Summary:
    //     Returns a copy of the Godot.Rect2I grown a given amount of units towards the Godot.Margin
    //     direction.
    //
    // Parameters:
    //   margin:
    //     The direction to grow in.
    //
    //   by:
    //     The amount to grow by.
    //
    // Returns:
    //     The grown Godot.Rect2I.
    public Rect2I GrowMargin(Margin margin, int by)
    {
        Rect2I rect = this;
        return rect.GrowIndividual((margin == Margin.Left) ? by : 0, (Margin.Top == margin) ? by : 0, (Margin.Right == margin) ? by : 0, (Margin.Bottom == margin) ? by : 0);
    }

    //
    // Summary:
    //     Returns true if the Godot.Rect2I is flat or empty, or false otherwise.
    //
    // Returns:
    //     A bool for whether or not the Godot.Rect2I has area.
    public bool HasNoArea()
    {
        return _size.x <= 0f || _size.y <= 0f;
    }

    //
    // Summary:
    //     Returns true if the Godot.Rect2I contains a point, or false otherwise.
    //
    // Parameters:
    //   point:
    //     The point to check.
    //
    // Returns:
    //     A bool for whether or not the Godot.Rect2I contains point.
    public bool HasPoint(Vector2I point)
    {
        if (point.x < _position.x)
        {
            return false;
        }

        if (point.y < _position.y)
        {
            return false;
        }

        if (point.x >= _position.x + _size.x)
        {
            return false;
        }

        if (point.y >= _position.y + _size.y)
        {
            return false;
        }

        return true;
    }

    //
    // Summary:
    //     Returns true if the Godot.Rect2I overlaps with b (i.e. they have at least one
    //     point in common). If includeBorders is true, they will also be considered overlapping
    //     if their borders touch, even without intersection.
    //
    // Parameters:
    //   b:
    //     The other Godot.Rect2I to check for intersections with.
    //
    //   includeBorders:
    //     Whether or not to consider borders.
    //
    // Returns:
    //     A bool for whether or not they are intersecting.
    public bool Intersects(Rect2I b, bool includeBorders = false)
    {
        if (includeBorders)
        {
            if (_position.x > b._position.x + b._size.x)
            {
                return false;
            }

            if (_position.x + _size.x < b._position.x)
            {
                return false;
            }

            if (_position.y > b._position.y + b._size.y)
            {
                return false;
            }

            if (_position.y + _size.y < b._position.y)
            {
                return false;
            }
        }
        else
        {
            if (_position.x >= b._position.x + b._size.x)
            {
                return false;
            }

            if (_position.x + _size.x <= b._position.x)
            {
                return false;
            }

            if (_position.y >= b._position.y + b._size.y)
            {
                return false;
            }

            if (_position.y + _size.y <= b._position.y)
            {
                return false;
            }
        }

        return true;
    }

    //
    // Summary:
    //     Returns a larger Godot.Rect2I that contains this Godot.Rect2I and b.
    //
    // Parameters:
    //   b:
    //     The other Godot.Rect2I.
    //
    // Returns:
    //     The merged Godot.Rect2I.
    public Rect2I Merge(Rect2I b)
    {
        Rect2I result = default(Rect2I);
        result._position.x = Mathf.Min(b._position.x, _position.x);
        result._position.y = Mathf.Min(b._position.y, _position.y);
        result._size.x = Mathf.Max(b._position.x + b._size.x, _position.x + _size.x);
        result._size.y = Mathf.Max(b._position.y + b._size.y, _position.y + _size.y);
        result._size -= result._position;
        return result;
    }

    //
    // Summary:
    //     Constructs a Godot.Rect2I from a position and size.
    //
    // Parameters:
    //   position:
    //     The position.
    //
    //   size:
    //     The size.
    public Rect2I(Vector2I position, Vector2I size)
    {
        _position = position;
        _size = size;
    }

    //
    // Summary:
    //     Constructs a Godot.Rect2I from a position, width, and height.
    //
    // Parameters:
    //   position:
    //     The position.
    //
    //   width:
    //     The width.
    //
    //   height:
    //     The height.
    public Rect2I(Vector2I position, int width, int height)
    {
        _position = position;
        _size = new Vector2I(width, height);
    }

    //
    // Summary:
    //     Constructs a Godot.Rect2I from x, y, and size.
    //
    // Parameters:
    //   x:
    //     The position's X coordinate.
    //
    //   y:
    //     The position's Y coordinate.
    //
    //   size:
    //     The size.
    public Rect2I(int x, int y, Vector2I size)
    {
        _position = new Vector2I(x, y);
        _size = size;
    }

    //
    // Summary:
    //     Constructs a Godot.Rect2I from x, y, width, and height.
    //
    // Parameters:
    //   x:
    //     The position's X coordinate.
    //
    //   y:
    //     The position's Y coordinate.
    //
    //   width:
    //     The width.
    //
    //   height:
    //     The height.
    public Rect2I(int x, int y, int width, int height)
    {
        _position = new Vector2I(x, y);
        _size = new Vector2I(width, height);
    }

    //
    // Summary:
    //     Returns true if the Godot.Rect2s are exactly equal. Note: Due to floating-point
    //     precision errors, consider using Godot.Rect2I.IsEqualApprox(Godot.Rect2I) instead,
    //     which is more reliable.
    //
    // Parameters:
    //   left:
    //     The left rect.
    //
    //   right:
    //     The right rect.
    //
    // Returns:
    //     Whether or not the rects are exactly equal.
    public static bool operator ==(Rect2I left, Rect2I right)
    {
        return left.Equals(right);
    }

    //
    // Summary:
    //     Returns true if the Godot.Rect2s are not equal. Note: Due to floating-point precision
    //     errors, consider using Godot.Rect2I.IsEqualApprox(Godot.Rect2I) instead, which
    //     is more reliable.
    //
    // Parameters:
    //   left:
    //     The left rect.
    //
    //   right:
    //     The right rect.
    //
    // Returns:
    //     Whether or not the rects are not equal.
    public static bool operator !=(Rect2I left, Rect2I right)
    {
        return !left.Equals(right);
    }

    //
    // Summary:
    //     Returns true if this rect and obj are equal.
    //
    // Parameters:
    //   obj:
    //     The other object to compare.
    //
    // Returns:
    //     Whether or not the rect and the other object are exactly equal.
    public override bool Equals(object obj)
    {
        if (obj is Rect2I)
        {
            return Equals((Rect2I)obj);
        }

        return false;
    }

    //
    // Summary:
    //     Returns true if this rect and other are equal.
    //
    // Parameters:
    //   other:
    //     The other rect to compare.
    //
    // Returns:
    //     Whether or not the rects are exactly equal.
    public bool Equals(Rect2I other)
    {
        return _position.Equals(other._position) && _size.Equals(other._size);
    }

    //
    // Summary:
    //     Returns true if this rect and other are approximately equal, by running Godot.Vector2I.IsEqualApprox(Godot.Vector2I)
    //     on each component.
    //
    // Parameters:
    //   other:
    //     The other rect to compare.
    //
    // Returns:
    //     Whether or not the rects are approximately equal.
    public bool IsEqualApprox(Rect2I other)
    {
        return _position.IsEqualApprox(other._position) && _size.IsEqualApprox(other.Size);
    }

    //
    // Summary:
    //     Serves as the hash function for Godot.Rect2I.
    //
    // Returns:
    //     A hash code for this rect.
    public override int GetHashCode()
    {
        return _position.GetHashCode() ^ _size.GetHashCode();
    }

    //
    // Summary:
    //     Converts this Godot.Rect2I to a string.
    //
    // Returns:
    //     A string representation of this rect.
    public override string ToString()
    {
        return string.Format("({0}, {1})", new object[2]
        {
            _position.ToString(),
            _size.ToString()
        });
    }

    //
    // Summary:
    //     Converts this Godot.Rect2I to a string with the given format.
    //
    // Returns:
    //     A string representation of this rect.
    public string ToString(string format)
    {
        return string.Format("({0}, {1})", new object[2]
        {
            _position.ToString(format),
            _size.ToString(format)
        });
    }
}