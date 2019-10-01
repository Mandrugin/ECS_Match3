// This simple helper allows to work with 1-dimensional array as 2-dimensional
// by calculating 1-dimensional index from two coordinates and finding indexes of nearest elements

public struct ArrayHelper
{
    public readonly int Width;
    public readonly int Height;

    public ArrayHelper(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public int GetRight(int i) => (i % Width < Width - 1) ? i + 1 : -1;
    public int GetLeft(int i) => (i % Width > 0) ? i - 1 : -1;
    public int GetUp(int i) => ((i += Width) < Width * Height) ? i : -1;
    public int GetDown(int i) => ((i -= Width) >= 0) ? i : -1;
    public int GetX(int i) => i % Width;
    public int GetY(int i) => i / Width;
    public int GetI(int x, int y) => y * Width + x;
}
