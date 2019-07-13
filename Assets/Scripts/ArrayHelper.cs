public struct ArrayHelper
{
    public int Width;
    public int Height;
        
    public int GetRight(int i)
    {
        if (i % Width < Width - 1) return i + 1;
        return -1;
    }
            
    public int GetLeft(int i)
    {
        if (i % Width > 0) return i - 1;
        return -1;
    }

    public int GetUp(int i)
    {
        if ((i += Width) >= Width * Height) return -1;
        return i;
    }

    public int GetDown(int i)
    {
        if ((i -= Width) < 0) return -1;
        return i;
    }

    public int GetX(int i)
    {
        return i % Width;
    }

    public int GetY(int i)
    {
        return i / Width;
    }

    public int GetI(int x, int y)
    {
        return y * Width + x;
    }
}